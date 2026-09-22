using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SzApp.Api.Security;
using SzApp.Contracts.MasterData;
using SzApp.Data;
using SzApp.Data.Entities;

namespace SzApp.Api.Features.MasterData;

public static class MasterDataFeatureExtensions
{
    public static IServiceCollection AddMasterDataFeature(this IServiceCollection services)
    {
        services.AddScoped<IMasterDataPermissionService, MasterDataPermissionService>();
        return services;
    }

    public static IEndpointRouteBuilder MapMasterDataEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/v1/master-data/companies", CreateCompanyAsync)
            .WithTags("Master Data - Companies")
            .RequireAuthorization(policy => policy.RequireRole(SecurityConstants.RootRole));

        var companies = endpoints.MapGroup("/api/v1/companies/{companyId:int}")
            .WithTags("Master Data")
            .RequireAuthorization(SecurityConstants.CompanyAccessPolicy);

        companies.MapGet("/", GetCompanyAsync);
        companies.MapPut("/", UpdateCompanyAsync);
        companies.MapDelete("/", DeleteCompanyAsync);

        companies.MapGet("/partners", ListPartnersAsync);
        companies.MapGet("/partners/{partnerId:int}", GetPartnerAsync);
        companies.MapPost("/partners", CreatePartnerAsync);
        companies.MapPut("/partners/{partnerId:int}", UpdatePartnerAsync);
        companies.MapDelete("/partners/{partnerId:int}", DeletePartnerAsync);

        // Address currently has no tenant owner in the canonical model. These
        // endpoints are Root-only until PartnerAddress/BuildingEntrance provide
        // a tenant-scoped ownership path.
        companies.MapGet("/addresses", ListAddressesAsync);
        companies.MapGet("/addresses/{addressId:int}", GetAddressAsync);
        companies.MapPost("/addresses", CreateAddressAsync);
        companies.MapPut("/addresses/{addressId:int}", UpdateAddressAsync);
        companies.MapDelete("/addresses/{addressId:int}", DeleteAddressAsync);

        companies.MapGet("/staff-access", ListStaffAccessAsync);
        companies.MapGet("/staff-access/{accessId:int}", GetStaffAccessAsync);
        companies.MapPost("/staff-access", CreateStaffAccessAsync);
        companies.MapPut("/staff-access/{accessId:int}", UpdateStaffAccessAsync);
        companies.MapDelete("/staff-access/{accessId:int}", DeleteStaffAccessAsync);

        companies.MapMasterDataExtendedEndpoints();

        return endpoints;
    }

    private static async Task<IResult> CreateCompanyAsync(
        CreateCompanyRequest request,
        SzAppDbContext dbContext,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var errors = MasterDataValidation.Validate(request);
        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }

        if (!await dbContext.Partners.AnyAsync(
                partner => partner.Id == request.PartnerId && partner.CompanyId == null,
                cancellationToken))
        {
            return Unprocessable("Partner kompanije mora biti postojeći globalni partner.");
        }
        if (request.ManagerId.HasValue && !await dbContext.Partners.AnyAsync(
                partner => partner.Id == request.ManagerId && partner.CompanyId == null,
                cancellationToken))
        {
            return Unprocessable("Upravnik mora biti postojeći globalni partner.");
        }

        var company = new Company();
        Apply(company, request);
        dbContext.Companies.Add(company);
        await dbContext.SaveChangesAsync(cancellationToken);
        SetETag(httpContext, company.RowVersion);
        return Results.Created($"/api/v1/companies/{company.Id}", ToResponse(company));
    }

    private static async Task<IResult> GetCompanyAsync(
        int companyId,
        SzAppDbContext dbContext,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var company = await dbContext.Companies.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == companyId, cancellationToken);
        if (company is null)
        {
            return Results.NotFound();
        }

        SetETag(httpContext, company.RowVersion);
        return Results.Ok(ToResponse(company));
    }

    private static async Task<IResult> UpdateCompanyAsync(
        int companyId,
        UpdateCompanyRequest request,
        ClaimsPrincipal principal,
        IMasterDataPermissionService permission,
        SzAppDbContext dbContext,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!await permission.CanWriteAsync(principal, companyId, cancellationToken))
        {
            return Results.Forbid();
        }
        var errors = MasterDataValidation.Validate(request);
        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }

        var company = await dbContext.Companies.SingleOrDefaultAsync(
            item => item.Id == companyId,
            cancellationToken);
        if (company is null)
        {
            return Results.NotFound();
        }
        if (!await PartnerCanBelongToCompanyAsync(dbContext, request.PartnerId, companyId, cancellationToken) ||
            request.ManagerId.HasValue && !await PartnerCanBelongToCompanyAsync(
                dbContext,
                request.ManagerId.Value,
                companyId,
                cancellationToken))
        {
            return Unprocessable("Partner i upravnik moraju biti globalni ili pripadati istoj kompaniji.");
        }

        ETagCodec.TryDecode(request.RowVersion, out var rowVersion);
        dbContext.Entry(company).Property(item => item.RowVersion).OriginalValue = rowVersion;
        Apply(company, request);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Results.Conflict(new { message = "Kompaniju je u međuvremenu izmenio drugi korisnik." });
        }

        SetETag(httpContext, company.RowVersion);
        return Results.Ok(ToResponse(company));
    }

    private static async Task<IResult> DeleteCompanyAsync(
        int companyId,
        ClaimsPrincipal principal,
        IMasterDataPermissionService permission,
        SzAppDbContext dbContext,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!permission.IsRoot(principal))
        {
            return Results.Forbid();
        }
        if (!TryGetIfMatch(httpContext, out var rowVersion))
        {
            return Results.Problem(
                statusCode: StatusCodes.Status428PreconditionRequired,
                title: "If-Match zaglavlje je obavezno.");
        }

        var company = await dbContext.Companies.SingleOrDefaultAsync(
            item => item.Id == companyId,
            cancellationToken);
        if (company is null)
        {
            return Results.NotFound();
        }
        dbContext.Entry(company).Property(item => item.RowVersion).OriginalValue = rowVersion;
        dbContext.Companies.Remove(company);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Results.Conflict(new { message = "Kompaniju je u međuvremenu izmenio drugi korisnik." });
        }
        catch (DbUpdateException)
        {
            return Results.Conflict(new { message = "Kompanija se ne može obrisati dok ima povezane podatke." });
        }
        return Results.NoContent();
    }

    private static async Task<IResult> ListPartnersAsync(
        int companyId,
        [AsParameters] MasterDataPageQuery query,
        SzAppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var partners = dbContext.Partners.AsNoTracking().Where(item => item.CompanyId == companyId);
        if (query.NormalizedSearch.Length > 0)
        {
            var search = $"%{EscapeLike(query.NormalizedSearch)}%";
            partners = partners.Where(item =>
                EF.Functions.Like(item.ShortName, search, "\\") ||
                EF.Functions.Like(item.Name, search, "\\") ||
                item.TaxNumber != null && EF.Functions.Like(item.TaxNumber, search, "\\"));
        }

        partners = (query.SortBy?.ToLowerInvariant(), query.Descending) switch
        {
            ("name", true) => partners.OrderByDescending(item => item.Name).ThenByDescending(item => item.Id),
            ("name", false) => partners.OrderBy(item => item.Name).ThenBy(item => item.Id),
            ("taxnumber", true) => partners.OrderByDescending(item => item.TaxNumber).ThenByDescending(item => item.Id),
            ("taxnumber", false) => partners.OrderBy(item => item.TaxNumber).ThenBy(item => item.Id),
            ("shortname", true) => partners.OrderByDescending(item => item.ShortName).ThenByDescending(item => item.Id),
            _ => partners.OrderBy(item => item.ShortName).ThenBy(item => item.Id)
        };

        var total = await partners.CountAsync(cancellationToken);
        var items = await partners.Skip(query.Skip).Take(query.NormalizedPageSize)
            .Select(item => ToResponse(item))
            .ToArrayAsync(cancellationToken);
        return Results.Ok(new PagedResponse<PartnerResponse>(
            items,
            query.NormalizedPage,
            query.NormalizedPageSize,
            total));
    }

    private static async Task<IResult> GetPartnerAsync(
        int companyId,
        int partnerId,
        SzAppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var partner = await dbContext.Partners.AsNoTracking().SingleOrDefaultAsync(
            item => item.Id == partnerId && item.CompanyId == companyId,
            cancellationToken);
        return partner is null ? Results.NotFound() : Results.Ok(ToResponse(partner));
    }

    private static async Task<IResult> CreatePartnerAsync(
        int companyId,
        SavePartnerRequest request,
        ClaimsPrincipal principal,
        IMasterDataPermissionService permission,
        SzAppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        if (!await permission.CanWriteAsync(principal, companyId, cancellationToken))
        {
            return Results.Forbid();
        }
        var errors = MasterDataValidation.Validate(request);
        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }

        var partner = new Partner { CompanyId = companyId };
        Apply(partner, request);
        dbContext.Partners.Add(partner);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Results.Created($"/api/v1/companies/{companyId}/partners/{partner.Id}", ToResponse(partner));
    }

    private static async Task<IResult> UpdatePartnerAsync(
        int companyId,
        int partnerId,
        SavePartnerRequest request,
        ClaimsPrincipal principal,
        IMasterDataPermissionService permission,
        SzAppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        if (!await permission.CanWriteAsync(principal, companyId, cancellationToken))
        {
            return Results.Forbid();
        }
        var errors = MasterDataValidation.Validate(request);
        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }

        var partner = await dbContext.Partners.SingleOrDefaultAsync(
            item => item.Id == partnerId && item.CompanyId == companyId,
            cancellationToken);
        if (partner is null)
        {
            return Results.NotFound();
        }
        Apply(partner, request);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Results.Ok(ToResponse(partner));
    }

    private static async Task<IResult> DeletePartnerAsync(
        int companyId,
        int partnerId,
        ClaimsPrincipal principal,
        IMasterDataPermissionService permission,
        SzAppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        if (!await permission.CanWriteAsync(principal, companyId, cancellationToken))
        {
            return Results.Forbid();
        }
        var partner = await dbContext.Partners.SingleOrDefaultAsync(
            item => item.Id == partnerId && item.CompanyId == companyId,
            cancellationToken);
        if (partner is null)
        {
            return Results.NotFound();
        }
        dbContext.Partners.Remove(partner);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return Results.Conflict(new { message = "Partner se ne može obrisati dok ima povezane podatke." });
        }
        return Results.NoContent();
    }

    private static async Task<IResult> ListAddressesAsync(
        int companyId,
        [AsParameters] MasterDataPageQuery query,
        ClaimsPrincipal principal,
        IMasterDataPermissionService permission,
        SzAppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        if (!permission.IsRoot(principal))
        {
            return Results.Forbid();
        }
        var addresses = dbContext.Addresses.AsNoTracking();
        if (query.NormalizedSearch.Length > 0)
        {
            var search = $"%{EscapeLike(query.NormalizedSearch)}%";
            addresses = addresses.Where(item =>
                EF.Functions.Like(item.StreetAddress, search, "\\") ||
                EF.Functions.Like(item.City, search, "\\") ||
                item.PostalCode != null && EF.Functions.Like(item.PostalCode, search, "\\"));
        }
        addresses = query.Descending
            ? addresses.OrderByDescending(item => item.City).ThenByDescending(item => item.StreetAddress)
            : addresses.OrderBy(item => item.City).ThenBy(item => item.StreetAddress);

        var total = await addresses.CountAsync(cancellationToken);
        var items = await addresses.Skip(query.Skip).Take(query.NormalizedPageSize)
            .Select(item => ToResponse(item))
            .ToArrayAsync(cancellationToken);
        return Results.Ok(new PagedResponse<AddressResponse>(
            items,
            query.NormalizedPage,
            query.NormalizedPageSize,
            total));
    }

    private static async Task<IResult> GetAddressAsync(
        int companyId,
        int addressId,
        ClaimsPrincipal principal,
        IMasterDataPermissionService permission,
        SzAppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        if (!permission.IsRoot(principal))
        {
            return Results.Forbid();
        }
        var address = await dbContext.Addresses.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == addressId, cancellationToken);
        return address is null ? Results.NotFound() : Results.Ok(ToResponse(address));
    }

    private static async Task<IResult> CreateAddressAsync(
        int companyId,
        SaveAddressRequest request,
        ClaimsPrincipal principal,
        IMasterDataPermissionService permission,
        SzAppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        if (!permission.IsRoot(principal))
        {
            return Results.Forbid();
        }
        var errors = MasterDataValidation.Validate(request);
        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }
        var address = new Address();
        Apply(address, request);
        dbContext.Addresses.Add(address);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Results.Created($"/api/v1/companies/{companyId}/addresses/{address.Id}", ToResponse(address));
    }

    private static async Task<IResult> UpdateAddressAsync(
        int companyId,
        int addressId,
        SaveAddressRequest request,
        ClaimsPrincipal principal,
        IMasterDataPermissionService permission,
        SzAppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        if (!permission.IsRoot(principal))
        {
            return Results.Forbid();
        }
        var errors = MasterDataValidation.Validate(request);
        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }
        var address = await dbContext.Addresses.SingleOrDefaultAsync(
            item => item.Id == addressId,
            cancellationToken);
        if (address is null)
        {
            return Results.NotFound();
        }
        Apply(address, request);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Results.Ok(ToResponse(address));
    }

    private static async Task<IResult> DeleteAddressAsync(
        int companyId,
        int addressId,
        ClaimsPrincipal principal,
        IMasterDataPermissionService permission,
        SzAppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        if (!permission.IsRoot(principal))
        {
            return Results.Forbid();
        }
        var address = await dbContext.Addresses.SingleOrDefaultAsync(
            item => item.Id == addressId,
            cancellationToken);
        if (address is null)
        {
            return Results.NotFound();
        }
        dbContext.Addresses.Remove(address);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return Results.Conflict(new { message = "Adresa se ne može obrisati dok je u upotrebi." });
        }
        return Results.NoContent();
    }

    private static async Task<IResult> ListStaffAccessAsync(
        int companyId,
        [AsParameters] MasterDataPageQuery query,
        SzAppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var access = dbContext.StaffAccess.AsNoTracking().Where(item => item.CompanyId == companyId);
        if (query.NormalizedSearch.Length > 0)
        {
            var search = $"%{EscapeLike(query.NormalizedSearch)}%";
            access = access.Where(item => item.Staff.Email != null && EF.Functions.Like(item.Staff.Email, search, "\\"));
        }
        access = query.Descending
            ? access.OrderByDescending(item => item.Staff.Email).ThenByDescending(item => item.Id)
            : access.OrderBy(item => item.Staff.Email).ThenBy(item => item.Id);

        var total = await access.CountAsync(cancellationToken);
        var items = await access.Skip(query.Skip).Take(query.NormalizedPageSize)
            .Select(item => new StaffAccessResponse(
                item.Id,
                item.StaffId,
                item.Staff.Email ?? string.Empty,
                item.CompanyId,
                item.StaffRole.ToString()))
            .ToArrayAsync(cancellationToken);
        return Results.Ok(new PagedResponse<StaffAccessResponse>(
            items,
            query.NormalizedPage,
            query.NormalizedPageSize,
            total));
    }

    private static async Task<IResult> GetStaffAccessAsync(
        int companyId,
        int accessId,
        SzAppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var access = await dbContext.StaffAccess.AsNoTracking()
            .Where(item => item.Id == accessId && item.CompanyId == companyId)
            .Select(item => new StaffAccessResponse(
                item.Id,
                item.StaffId,
                item.Staff.Email ?? string.Empty,
                item.CompanyId,
                item.StaffRole.ToString()))
            .SingleOrDefaultAsync(cancellationToken);
        return access is null ? Results.NotFound() : Results.Ok(access);
    }

    private static async Task<IResult> CreateStaffAccessAsync(
        int companyId,
        SaveStaffAccessRequest request,
        ClaimsPrincipal principal,
        IMasterDataPermissionService permission,
        SzAppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        if (!await permission.CanWriteAsync(principal, companyId, cancellationToken))
        {
            return Results.Forbid();
        }
        var errors = MasterDataValidation.Validate(request);
        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }
        if (!await dbContext.Users.AnyAsync(item => item.Id == request.StaffId, cancellationToken))
        {
            return Unprocessable("Korisnik ne postoji.");
        }
        if (await dbContext.StaffAccess.AnyAsync(
                item => item.CompanyId == companyId && item.StaffId == request.StaffId,
                cancellationToken))
        {
            return Results.Conflict(new { message = "Korisnik već ima pristup ovoj kompaniji." });
        }

        Enum.TryParse<StaffRole>(request.StaffRole, true, out var role);
        var access = new StaffAccess
        {
            CompanyId = companyId,
            StaffId = request.StaffId,
            StaffRole = role
        };
        dbContext.StaffAccess.Add(access);
        await dbContext.SaveChangesAsync(cancellationToken);
        var email = await dbContext.Users.Where(item => item.Id == access.StaffId)
            .Select(item => item.Email)
            .SingleAsync(cancellationToken);
        return Results.Created(
            $"/api/v1/companies/{companyId}/staff-access/{access.Id}",
            new StaffAccessResponse(access.Id, access.StaffId, email ?? string.Empty, companyId, role.ToString()));
    }

    private static async Task<IResult> UpdateStaffAccessAsync(
        int companyId,
        int accessId,
        SaveStaffAccessRequest request,
        ClaimsPrincipal principal,
        IMasterDataPermissionService permission,
        SzAppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        if (!await permission.CanWriteAsync(principal, companyId, cancellationToken))
        {
            return Results.Forbid();
        }
        var errors = MasterDataValidation.Validate(request);
        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }
        var access = await dbContext.StaffAccess.Include(item => item.Staff).SingleOrDefaultAsync(
            item => item.Id == accessId && item.CompanyId == companyId,
            cancellationToken);
        if (access is null)
        {
            return Results.NotFound();
        }
        if (access.StaffId != request.StaffId)
        {
            return Unprocessable("StaffId postojećeg pristupa se ne može promeniti; obrišite ga i kreirajte novi.");
        }
        Enum.TryParse<StaffRole>(request.StaffRole, true, out var role);
        access.StaffRole = role;
        await dbContext.SaveChangesAsync(cancellationToken);
        return Results.Ok(new StaffAccessResponse(
            access.Id,
            access.StaffId,
            access.Staff.Email ?? string.Empty,
            access.CompanyId,
            access.StaffRole.ToString()));
    }

    private static async Task<IResult> DeleteStaffAccessAsync(
        int companyId,
        int accessId,
        ClaimsPrincipal principal,
        IMasterDataPermissionService permission,
        SzAppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        if (!await permission.CanWriteAsync(principal, companyId, cancellationToken))
        {
            return Results.Forbid();
        }
        var access = await dbContext.StaffAccess.SingleOrDefaultAsync(
            item => item.Id == accessId && item.CompanyId == companyId,
            cancellationToken);
        if (access is null)
        {
            return Results.NotFound();
        }
        dbContext.StaffAccess.Remove(access);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }

    private static CompanyDetailResponse ToResponse(Company company) => new(
        company.Id,
        company.PartnerId,
        company.ManagerId,
        company.ShortName,
        company.PrintName,
        company.RelativeFolderName,
        company.CompanyTypeId,
        company.VatTypeId,
        company.LedgerEntryDate,
        Convert.ToBase64String(company.RowVersion));

    private static PartnerResponse ToResponse(Partner partner) => new(
        partner.Id,
        partner.CompanyId!.Value,
        partner.ShortName,
        partner.Name,
        partner.RegistrationNumber,
        partner.TaxNumber,
        partner.Jbkjs,
        Mask(partner.IdCardNumber),
        Mask(partner.Jmbg),
        partner.PartnerTypeId,
        partner.Language,
        partner.Note);

    private static AddressResponse ToResponse(Address address) => new(
        address.Id,
        address.StreetAddress,
        address.PostalCode,
        address.City,
        address.CountryCode);

    private static void Apply(Company company, CreateCompanyRequest request)
    {
        company.PartnerId = request.PartnerId;
        company.ManagerId = request.ManagerId;
        company.ShortName = request.ShortName.Trim();
        company.PrintName = request.PrintName.Trim();
        company.RelativeFolderName = TrimToNull(request.RelativeFolderName);
        company.CompanyTypeId = request.CompanyTypeId;
        company.VatTypeId = request.VatTypeId;
        company.LedgerEntryDate = request.LedgerEntryDate;
    }

    private static void Apply(Company company, UpdateCompanyRequest request) => Apply(
        company,
        new CreateCompanyRequest(
            request.PartnerId,
            request.ManagerId,
            request.ShortName,
            request.PrintName,
            request.RelativeFolderName,
            request.CompanyTypeId,
            request.VatTypeId,
            request.LedgerEntryDate));

    private static void Apply(Partner partner, SavePartnerRequest request)
    {
        partner.ShortName = request.ShortName.Trim();
        partner.Name = request.Name.Trim();
        partner.RegistrationNumber = TrimToNull(request.RegistrationNumber);
        partner.TaxNumber = TrimToNull(request.TaxNumber);
        partner.Jbkjs = TrimToNull(request.Jbkjs);
        partner.IdCardNumber = TrimToNull(request.IdCardNumber);
        partner.Jmbg = TrimToNull(request.Jmbg);
        partner.PartnerTypeId = request.PartnerTypeId;
        partner.Language = request.Language.Trim();
        partner.Note = TrimToNull(request.Note);
    }

    private static void Apply(Address address, SaveAddressRequest request)
    {
        address.StreetAddress = request.StreetAddress.Trim();
        address.PostalCode = TrimToNull(request.PostalCode);
        address.City = request.City.Trim();
        address.CountryCode = request.CountryCode.Trim().ToUpperInvariant();
    }

    private static Task<bool> PartnerCanBelongToCompanyAsync(
        SzAppDbContext dbContext,
        int partnerId,
        int companyId,
        CancellationToken cancellationToken) => dbContext.Partners.AnyAsync(
        partner => partner.Id == partnerId && (partner.CompanyId == null || partner.CompanyId == companyId),
        cancellationToken);

    private static string EscapeLike(string value) => value
        .Replace("\\", "\\\\", StringComparison.Ordinal)
        .Replace("%", "\\%", StringComparison.Ordinal)
        .Replace("_", "\\_", StringComparison.Ordinal)
        .Replace("[", "\\[", StringComparison.Ordinal);

    private static string? Mask(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }
        var visible = Math.Min(4, value.Length);
        return new string('*', value.Length - visible) + value[^visible..];
    }

    private static string? TrimToNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static IResult Unprocessable(string detail) => Results.Problem(
        statusCode: StatusCodes.Status422UnprocessableEntity,
        title: "Poslovno pravilo nije zadovoljeno.",
        detail: detail);

    private static void SetETag(HttpContext httpContext, byte[] rowVersion) =>
        httpContext.Response.Headers.ETag = ETagCodec.Encode(rowVersion);

    private static bool TryGetIfMatch(HttpContext httpContext, out byte[] rowVersion) =>
        ETagCodec.TryDecode(httpContext.Request.Headers.IfMatch.FirstOrDefault(), out rowVersion);
}
