using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SzApp.Contracts.MasterData;
using SzApp.Data;
using SzApp.Data.Entities;
using SzApp.Domain;

namespace SzApp.Api.Features.MasterData;

public static class MasterDataExtendedFeatureExtensions
{
    public static RouteGroupBuilder MapMasterDataExtendedEndpoints(this RouteGroupBuilder companies)
    {
        companies.MapGet("/building-entrances", ListBuildingEntrancesAsync);
        companies.MapGet("/building-entrances/{entranceId:int}", GetBuildingEntranceAsync);
        companies.MapPost("/building-entrances", CreateBuildingEntranceAsync);
        companies.MapPut("/building-entrances/{entranceId:int}", UpdateBuildingEntranceAsync);
        companies.MapDelete("/building-entrances/{entranceId:int}", DeleteBuildingEntranceAsync);

        companies.MapGet("/units", ListUnitsAsync);
        companies.MapGet("/units/{unitId:int}", GetUnitAsync);
        companies.MapPost("/units", CreateUnitAsync);
        companies.MapPut("/units/{unitId:int}", UpdateUnitAsync);
        companies.MapDelete("/units/{unitId:int}", DeleteUnitAsync);
        companies.MapGet("/units/{unitId:int}/contracts", ListContractsAsync);
        companies.MapGet("/contracts/{contractId:int}", GetContractAsync);
        companies.MapPost("/units/{unitId:int}/contracts/replace", ReplaceContractAsync);

        companies.MapGet("/partner-accounts", ListPartnerAccountsAsync);
        companies.MapGet("/partner-accounts/{accountId:int}", GetPartnerAccountAsync);
        companies.MapPost("/partner-accounts", CreatePartnerAccountAsync);
        companies.MapPut("/partner-accounts/{accountId:int}", UpdatePartnerAccountAsync);
        companies.MapDelete("/partner-accounts/{accountId:int}", DeletePartnerAccountAsync);

        companies.MapGet("/bank-accounts", ListBankAccountsAsync);
        companies.MapGet("/bank-accounts/{accountId:int}", GetBankAccountAsync);
        companies.MapPost("/bank-accounts", CreateBankAccountAsync);
        companies.MapPut("/bank-accounts/{accountId:int}", UpdateBankAccountAsync);
        companies.MapDelete("/bank-accounts/{accountId:int}", DeleteBankAccountAsync);
        return companies;
    }

    private static async Task<IResult> ListBuildingEntrancesAsync(
        int companyId,
        [AsParameters] MasterDataPageQuery query,
        SzAppDbContext db,
        CancellationToken cancellationToken)
    {
        var source = db.Set<BuildingEntrance>().AsNoTracking().Where(item => item.CompanyId == companyId);
        if (query.NormalizedSearch.Length > 0)
        {
            source = source.Where(item =>
                (item.BuildingName != null && item.BuildingName.Contains(query.NormalizedSearch)) ||
                (item.EntranceName != null && item.EntranceName.Contains(query.NormalizedSearch)) ||
                (item.BuildingLabel != null && item.BuildingLabel.Contains(query.NormalizedSearch)));
        }
        source = query.NormalizedDescending
            ? source.OrderByDescending(item => item.SortIndex).ThenByDescending(item => item.Id)
            : source.OrderBy(item => item.SortIndex).ThenBy(item => item.Id);
        var total = await source.CountAsync(cancellationToken);
        var items = await source.Skip(query.Skip).Take(query.NormalizedPageSize)
            .Select(item => ToResponse(item)).ToArrayAsync(cancellationToken);
        return Results.Ok(new PagedResponse<BuildingEntranceResponse>(items, query.NormalizedPage, query.NormalizedPageSize, total));
    }

    private static async Task<IResult> GetBuildingEntranceAsync(
        int companyId,
        int entranceId,
        SzAppDbContext db,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var item = await db.Set<BuildingEntrance>().AsNoTracking().SingleOrDefaultAsync(
            value => value.Id == entranceId && value.CompanyId == companyId,
            cancellationToken);
        if (item is null) return Results.NotFound();
        SetETag(context, item.RowVersion);
        return Results.Ok(ToResponse(item));
    }

    private static async Task<IResult> CreateBuildingEntranceAsync(
        int companyId,
        SaveBuildingEntranceRequest request,
        ClaimsPrincipal principal,
        IMasterDataPermissionService permission,
        SzAppDbContext db,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (!await permission.CanWriteAsync(principal, companyId, cancellationToken)) return Results.Forbid();
        var error = ValidateEntrance(request);
        if (error is not null) return Unprocessable(error);
        if (request.AddressId.HasValue && !await db.Addresses.AnyAsync(item => item.Id == request.AddressId, cancellationToken))
            return Unprocessable("Adresa ne postoji.");
        var item = new BuildingEntrance { CompanyId = companyId };
        Apply(item, request);
        db.Set<BuildingEntrance>().Add(item);
        await db.SaveChangesAsync(cancellationToken);
        SetETag(context, item.RowVersion);
        return Results.Created($"/api/v1/companies/{companyId}/building-entrances/{item.Id}", ToResponse(item));
    }

    private static async Task<IResult> UpdateBuildingEntranceAsync(
        int companyId,
        int entranceId,
        SaveBuildingEntranceRequest request,
        ClaimsPrincipal principal,
        IMasterDataPermissionService permission,
        SzAppDbContext db,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (!await permission.CanWriteAsync(principal, companyId, cancellationToken)) return Results.Forbid();
        var error = ValidateEntrance(request);
        if (error is not null) return Unprocessable(error);
        if (!ETagCodec.TryDecode(request.RowVersion, out var rowVersion)) return InvalidRowVersion();
        var item = await db.Set<BuildingEntrance>().SingleOrDefaultAsync(
            value => value.Id == entranceId && value.CompanyId == companyId,
            cancellationToken);
        if (item is null) return Results.NotFound();
        if (request.AddressId.HasValue && !await db.Addresses.AnyAsync(value => value.Id == request.AddressId, cancellationToken))
            return Unprocessable("Adresa ne postoji.");
        db.Entry(item).Property(value => value.RowVersion).OriginalValue = rowVersion;
        Apply(item, request);
        var save = await SaveConcurrentAsync(db, cancellationToken);
        if (save is not null) return save;
        SetETag(context, item.RowVersion);
        return Results.Ok(ToResponse(item));
    }

    private static async Task<IResult> DeleteBuildingEntranceAsync(
        int companyId,
        int entranceId,
        ClaimsPrincipal principal,
        IMasterDataPermissionService permission,
        SzAppDbContext db,
        HttpContext context,
        CancellationToken cancellationToken) => await DeleteCompanyOwnedAsync<BuildingEntrance>(
            companyId, entranceId, principal, permission, db, context, cancellationToken);

    private static async Task<IResult> ListUnitsAsync(
        int companyId,
        [AsParameters] MasterDataPageQuery query,
        SzAppDbContext db,
        CancellationToken cancellationToken)
    {
        var source = db.Set<Unit>().AsNoTracking().Where(item => item.CompanyId == companyId);
        if (query.NormalizedSearch.Length > 0)
            source = source.Where(item => item.Name != null && item.Name.Contains(query.NormalizedSearch));
        source = query.NormalizedDescending
            ? source.OrderByDescending(item => item.SortingNumber).ThenByDescending(item => item.Id)
            : source.OrderBy(item => item.SortingNumber).ThenBy(item => item.Id);
        var total = await source.CountAsync(cancellationToken);
        var items = await source.Skip(query.Skip).Take(query.NormalizedPageSize)
            .Select(item => ToResponse(item)).ToArrayAsync(cancellationToken);
        return Results.Ok(new PagedResponse<UnitResponse>(items, query.NormalizedPage, query.NormalizedPageSize, total));
    }

    private static async Task<IResult> GetUnitAsync(
        int companyId,
        int unitId,
        SzAppDbContext db,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var item = await db.Set<Unit>().AsNoTracking().SingleOrDefaultAsync(
            value => value.Id == unitId && value.CompanyId == companyId,
            cancellationToken);
        if (item is null) return Results.NotFound();
        SetETag(context, item.RowVersion);
        return Results.Ok(ToResponse(item));
    }

    private static async Task<IResult> CreateUnitAsync(
        int companyId,
        SaveUnitRequest request,
        ClaimsPrincipal principal,
        IMasterDataPermissionService permission,
        SzAppDbContext db,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (!await permission.CanWriteAsync(principal, companyId, cancellationToken)) return Results.Forbid();
        var error = await ValidateUnitAsync(companyId, request, db, cancellationToken);
        if (error is not null) return Unprocessable(error);
        var item = new Unit { CompanyId = companyId };
        Apply(item, request);
        db.Set<Unit>().Add(item);
        await db.SaveChangesAsync(cancellationToken);
        SetETag(context, item.RowVersion);
        return Results.Created($"/api/v1/companies/{companyId}/units/{item.Id}", ToResponse(item));
    }

    private static async Task<IResult> UpdateUnitAsync(
        int companyId,
        int unitId,
        SaveUnitRequest request,
        ClaimsPrincipal principal,
        IMasterDataPermissionService permission,
        SzAppDbContext db,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (!await permission.CanWriteAsync(principal, companyId, cancellationToken)) return Results.Forbid();
        if (!ETagCodec.TryDecode(request.RowVersion, out var rowVersion)) return InvalidRowVersion();
        var error = await ValidateUnitAsync(companyId, request, db, cancellationToken);
        if (error is not null) return Unprocessable(error);
        var item = await db.Set<Unit>().SingleOrDefaultAsync(
            value => value.Id == unitId && value.CompanyId == companyId,
            cancellationToken);
        if (item is null) return Results.NotFound();
        db.Entry(item).Property(value => value.RowVersion).OriginalValue = rowVersion;
        Apply(item, request);
        var save = await SaveConcurrentAsync(db, cancellationToken);
        if (save is not null) return save;
        SetETag(context, item.RowVersion);
        return Results.Ok(ToResponse(item));
    }

    private static async Task<IResult> DeleteUnitAsync(
        int companyId,
        int unitId,
        ClaimsPrincipal principal,
        IMasterDataPermissionService permission,
        SzAppDbContext db,
        HttpContext context,
        CancellationToken cancellationToken) => await DeleteCompanyOwnedAsync<Unit>(
            companyId, unitId, principal, permission, db, context, cancellationToken);

    private static async Task<IResult> ListContractsAsync(
        int companyId,
        int unitId,
        SzAppDbContext db,
        CancellationToken cancellationToken)
    {
        var exists = await db.Set<Unit>().AnyAsync(item => item.Id == unitId && item.CompanyId == companyId, cancellationToken);
        if (!exists) return Results.NotFound();
        var items = await db.Set<Contract>().AsNoTracking()
            .Where(item => item.CompanyId == companyId && item.UnitId == unitId)
            .OrderByDescending(item => item.ContractDate)
            .Select(item => ToResponse(item))
            .ToArrayAsync(cancellationToken);
        return Results.Ok(items);
    }

    private static async Task<IResult> GetContractAsync(
        int companyId,
        int contractId,
        SzAppDbContext db,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var item = await db.Set<Contract>().AsNoTracking().SingleOrDefaultAsync(
            value => value.Id == contractId && value.CompanyId == companyId,
            cancellationToken);
        if (item is null) return Results.NotFound();
        SetETag(context, item.RowVersion);
        return Results.Ok(ToResponse(item));
    }

    private static async Task<IResult> ReplaceContractAsync(
        int companyId,
        int unitId,
        ReplaceContractRequest request,
        ClaimsPrincipal principal,
        IMasterDataPermissionService permission,
        SzAppDbContext db,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (!await permission.CanWriteAsync(principal, companyId, cancellationToken)) return Results.Forbid();
        var validation = MasterDataValidation.ValidateContractPeriod(
            request.EffectiveFrom, null, request.InvoiceStartDate, request.InvoiceEndDate);
        if (validation.Count > 0) return Results.ValidationProblem(validation);
        if (request.InvoiceDeliveryLocation?.Length > 50) return Unprocessable("Mesto dostave računa može imati najviše 50 znakova.");

        var unit = await db.Set<Unit>().SingleOrDefaultAsync(
            item => item.Id == unitId && item.CompanyId == companyId,
            cancellationToken);
        if (unit is null) return Results.NotFound();
        var partnerIds = new[] { request.OwnerPartnerId, request.InvoicePartnerId, request.TenantPartnerId }
            .Where(value => value.HasValue).Select(value => value!.Value).Distinct().ToArray();
        var validPartnerCount = await db.Partners.CountAsync(
            item => partnerIds.Contains(item.Id) && item.CompanyId == companyId,
            cancellationToken);
        if (validPartnerCount != partnerIds.Length) return Unprocessable("Svi partneri ugovora moraju pripadati istoj kompaniji.");
        if (request.InvoiceDeliveryUnitId.HasValue && !await db.Set<Unit>().AnyAsync(
                item => item.Id == request.InvoiceDeliveryUnitId && item.CompanyId == companyId,
                cancellationToken))
            return Unprocessable("Jedinica za dostavu računa mora pripadati istoj kompaniji.");

        var current = unit.ContractId.HasValue
            ? await db.Set<Contract>().SingleOrDefaultAsync(
                item => item.Id == unit.ContractId && item.CompanyId == companyId,
                cancellationToken)
            : await db.Set<Contract>().SingleOrDefaultAsync(
                item => item.UnitId == unitId && item.CompanyId == companyId && item.IsActive,
                cancellationToken);
        if (current is not null && request.EffectiveFrom <= current.ContractDate)
            return Unprocessable("Novi ugovor mora početi posle početka trenutnog ugovora.");
        if (current is not null)
        {
            if (!ETagCodec.TryDecode(request.CurrentContractRowVersion, out var rowVersion)) return InvalidRowVersion();
            db.Entry(current).Property(item => item.RowVersion).OriginalValue = rowVersion;
        }
        var activeContracts = await db.Set<Contract>().AsNoTracking()
            .Where(item => item.CompanyId == companyId && item.UnitId == unitId && item.IsActive &&
                           (current == null || item.Id != current.Id))
            .Select(item => new { item.ContractDate, item.ContractEndDate })
            .ToArrayAsync(cancellationToken);
        var overlapsFuture = activeContracts.Any(item =>
            ContractPeriodPolicy.Overlaps(request.EffectiveFrom, null, item.ContractDate, item.ContractEndDate));
        if (overlapsFuture) return Unprocessable("Period novog ugovora preklapa se sa postojećom istorijom.");

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            if (current is not null)
            {
                current.ContractEndDate = request.EffectiveFrom.AddDays(-1);
                current.IsActive = false;
            }
            var next = new Contract
            {
                CompanyId = companyId,
                UnitId = unitId,
                AccountNumber = request.AccountNumber,
                OwnerPartnerId = request.OwnerPartnerId,
                InvoicePartnerId = request.InvoicePartnerId ?? request.OwnerPartnerId,
                TenantPartnerId = request.TenantPartnerId,
                ContractDate = request.EffectiveFrom,
                InvoiceStartDate = request.InvoiceStartDate,
                InvoiceEndDate = request.InvoiceEndDate,
                IsActive = true,
                Note = request.Note,
                InvoiceDeliveryLocation = request.InvoiceDeliveryLocation,
                InvoiceDeliveryUnitId = request.InvoiceDeliveryUnitId,
                IsPrintInvoiceMandatory = request.IsPrintInvoiceMandatory,
                IsPrintInvoiceToPostOffice = request.IsPrintInvoiceToPostOffice,
                IsPrintInvoiceSkipped = request.IsPrintInvoiceSkipped,
                ExportExternalAccount = request.ExportExternalAccount
            };
            db.Set<Contract>().Add(next);
            await db.SaveChangesAsync(cancellationToken);
            unit.ContractId = next.Id;
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            SetETag(context, next.RowVersion);
            return Results.Created($"/api/v1/companies/{companyId}/contracts/{next.Id}", ToResponse(next));
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            return ConcurrencyConflict();
        }
    }

    private static async Task<IResult> ListPartnerAccountsAsync(
        int companyId,
        [AsParameters] MasterDataPageQuery query,
        SzAppDbContext db,
        CancellationToken cancellationToken)
    {
        var source = db.Set<PartnerAccount>().AsNoTracking().Where(item => item.CompanyId == companyId);
        if (query.NormalizedSearch.Length > 0)
            source = source.Where(item => item.Account.Contains(query.NormalizedSearch) || item.AccountNumber.ToString().Contains(query.NormalizedSearch));
        source = query.NormalizedDescending
            ? source.OrderByDescending(item => item.AccountNumber).ThenByDescending(item => item.Id)
            : source.OrderBy(item => item.AccountNumber).ThenBy(item => item.Id);
        var total = await source.CountAsync(cancellationToken);
        var items = await source.Skip(query.Skip).Take(query.NormalizedPageSize)
            .Select(item => ToResponse(item)).ToArrayAsync(cancellationToken);
        return Results.Ok(new PagedResponse<PartnerAccountResponse>(items, query.NormalizedPage, query.NormalizedPageSize, total));
    }

    private static async Task<IResult> GetPartnerAccountAsync(
        int companyId,
        int accountId,
        SzAppDbContext db,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var item = await db.Set<PartnerAccount>().AsNoTracking().SingleOrDefaultAsync(
            value => value.Id == accountId && value.CompanyId == companyId,
            cancellationToken);
        if (item is null) return Results.NotFound();
        SetETag(context, item.RowVersion);
        return Results.Ok(ToResponse(item));
    }

    private static async Task<IResult> CreatePartnerAccountAsync(
        int companyId,
        SavePartnerAccountRequest request,
        ClaimsPrincipal principal,
        IMasterDataPermissionService permission,
        SzAppDbContext db,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (!await permission.CanWriteAsync(principal, companyId, cancellationToken)) return Results.Forbid();
        var error = await ValidatePartnerAccountAsync(companyId, request, db, cancellationToken);
        if (error is not null) return Unprocessable(error);
        var item = new PartnerAccount { CompanyId = companyId };
        Apply(item, request);
        db.Set<PartnerAccount>().Add(item);
        try { await db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException) { return Results.Conflict(new { message = "Broj partnerskog konta već postoji u kompaniji." }); }
        SetETag(context, item.RowVersion);
        return Results.Created($"/api/v1/companies/{companyId}/partner-accounts/{item.Id}", ToResponse(item));
    }

    private static async Task<IResult> UpdatePartnerAccountAsync(
        int companyId,
        int accountId,
        SavePartnerAccountRequest request,
        ClaimsPrincipal principal,
        IMasterDataPermissionService permission,
        SzAppDbContext db,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (!await permission.CanWriteAsync(principal, companyId, cancellationToken)) return Results.Forbid();
        if (!ETagCodec.TryDecode(request.RowVersion, out var rowVersion)) return InvalidRowVersion();
        var error = await ValidatePartnerAccountAsync(companyId, request, db, cancellationToken);
        if (error is not null) return Unprocessable(error);
        var item = await db.Set<PartnerAccount>().SingleOrDefaultAsync(
            value => value.Id == accountId && value.CompanyId == companyId,
            cancellationToken);
        if (item is null) return Results.NotFound();
        db.Entry(item).Property(value => value.RowVersion).OriginalValue = rowVersion;
        Apply(item, request);
        try
        {
            var save = await SaveConcurrentAsync(db, cancellationToken);
            if (save is not null) return save;
        }
        catch (DbUpdateException)
        {
            return Results.Conflict(new { message = "Broj partnerskog konta već postoji u kompaniji." });
        }
        SetETag(context, item.RowVersion);
        return Results.Ok(ToResponse(item));
    }

    private static async Task<IResult> DeletePartnerAccountAsync(
        int companyId,
        int accountId,
        ClaimsPrincipal principal,
        IMasterDataPermissionService permission,
        SzAppDbContext db,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (!await permission.CanWriteAsync(principal, companyId, cancellationToken)) return Results.Forbid();
        if (!TryIfMatch(context, out var rowVersion)) return InvalidRowVersion();
        var item = await db.Set<PartnerAccount>().SingleOrDefaultAsync(
            value => value.Id == accountId && value.CompanyId == companyId,
            cancellationToken);
        if (item is null) return Results.NotFound();
        db.Entry(item).Property(value => value.RowVersion).OriginalValue = rowVersion;
        db.Remove(item);
        return await SaveDeleteAsync(db, cancellationToken);
    }

    private static async Task<IResult> ListBankAccountsAsync(
        int companyId,
        [AsParameters] MasterDataPageQuery query,
        SzAppDbContext db,
        CancellationToken cancellationToken)
    {
        var source = db.Set<BankAccount>().AsNoTracking().Where(item => item.CompanyId == companyId);
        if (query.NormalizedSearch.Length > 0)
            source = source.Where(item => item.AccountNumber != null && item.AccountNumber.Contains(query.NormalizedSearch));
        source = query.NormalizedDescending
            ? source.OrderByDescending(item => item.SortIndex).ThenByDescending(item => item.Id)
            : source.OrderBy(item => item.SortIndex).ThenBy(item => item.Id);
        var total = await source.CountAsync(cancellationToken);
        var items = await source.Skip(query.Skip).Take(query.NormalizedPageSize)
            .Select(item => ToResponse(item)).ToArrayAsync(cancellationToken);
        return Results.Ok(new PagedResponse<BankAccountResponse>(items, query.NormalizedPage, query.NormalizedPageSize, total));
    }

    private static async Task<IResult> GetBankAccountAsync(
        int companyId,
        int accountId,
        SzAppDbContext db,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var item = await db.Set<BankAccount>().AsNoTracking().SingleOrDefaultAsync(
            value => value.Id == accountId && value.CompanyId == companyId,
            cancellationToken);
        if (item is null) return Results.NotFound();
        SetETag(context, item.RowVersion);
        return Results.Ok(ToResponse(item));
    }

    private static async Task<IResult> CreateBankAccountAsync(
        int companyId,
        SaveBankAccountRequest request,
        ClaimsPrincipal principal,
        IMasterDataPermissionService permission,
        SzAppDbContext db,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (!await permission.CanWriteAsync(principal, companyId, cancellationToken)) return Results.Forbid();
        var error = await ValidateBankAccountAsync(companyId, request, db, cancellationToken);
        if (error is not null) return Unprocessable(error);
        var item = new BankAccount { CompanyId = companyId };
        Apply(item, request);
        db.Set<BankAccount>().Add(item);
        await db.SaveChangesAsync(cancellationToken);
        SetETag(context, item.RowVersion);
        return Results.Created($"/api/v1/companies/{companyId}/bank-accounts/{item.Id}", ToResponse(item));
    }

    private static async Task<IResult> UpdateBankAccountAsync(
        int companyId,
        int accountId,
        SaveBankAccountRequest request,
        ClaimsPrincipal principal,
        IMasterDataPermissionService permission,
        SzAppDbContext db,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (!await permission.CanWriteAsync(principal, companyId, cancellationToken)) return Results.Forbid();
        if (!ETagCodec.TryDecode(request.RowVersion, out var rowVersion)) return InvalidRowVersion();
        var error = await ValidateBankAccountAsync(companyId, request, db, cancellationToken);
        if (error is not null) return Unprocessable(error);
        var item = await db.Set<BankAccount>().SingleOrDefaultAsync(
            value => value.Id == accountId && value.CompanyId == companyId,
            cancellationToken);
        if (item is null) return Results.NotFound();
        db.Entry(item).Property(value => value.RowVersion).OriginalValue = rowVersion;
        Apply(item, request);
        var save = await SaveConcurrentAsync(db, cancellationToken);
        if (save is not null) return save;
        SetETag(context, item.RowVersion);
        return Results.Ok(ToResponse(item));
    }

    private static async Task<IResult> DeleteBankAccountAsync(
        int companyId,
        int accountId,
        ClaimsPrincipal principal,
        IMasterDataPermissionService permission,
        SzAppDbContext db,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (!await permission.CanWriteAsync(principal, companyId, cancellationToken)) return Results.Forbid();
        if (!TryIfMatch(context, out var rowVersion)) return InvalidRowVersion();
        var item = await db.Set<BankAccount>().SingleOrDefaultAsync(
            value => value.Id == accountId && value.CompanyId == companyId,
            cancellationToken);
        if (item is null) return Results.NotFound();
        db.Entry(item).Property(value => value.RowVersion).OriginalValue = rowVersion;
        db.Remove(item);
        return await SaveDeleteAsync(db, cancellationToken);
    }

    private static BuildingEntranceResponse ToResponse(BuildingEntrance item) => new(
        item.Id, item.CompanyId, item.BuildingName, item.EntranceName, item.AddressId,
        item.BuildingLabel, item.Description, item.SortIndex, Convert.ToBase64String(item.RowVersion));

    private static UnitResponse ToResponse(Unit item) => new(
        item.Id, item.CompanyId, item.Name, item.ContractId, item.UnitTypeId, item.BuildingEntranceId,
        item.Note, item.SortingNumber, item.K1, item.K2, item.K3, item.K4, item.K5, item.FloorNumber,
        Convert.ToBase64String(item.RowVersion));

    private static ContractResponse ToResponse(Contract item) => new(
        item.Id, item.UnitId, item.AccountNumber, item.OwnerPartnerId, item.InvoicePartnerId,
        item.TenantPartnerId, item.ContractDate, item.ContractEndDate, item.InvoiceStartDate,
        item.InvoiceEndDate, item.IsActive, item.Note, item.InvoiceDeliveryLocation,
        item.InvoiceDeliveryUnitId, item.IsPrintInvoiceMandatory, item.IsPrintInvoiceToPostOffice,
        item.IsPrintInvoiceSkipped, item.ExportExternalAccount, Convert.ToBase64String(item.RowVersion));

    private static PartnerAccountResponse ToResponse(PartnerAccount item) => new(
        item.Id, item.CompanyId, item.Account, item.PartnerId, item.ContractId, item.AccountNumber,
        Convert.ToBase64String(item.RowVersion));

    private static BankAccountResponse ToResponse(BankAccount item) => new(
        item.Id, item.CompanyId, item.AccountNumber, item.IsActive, item.PartnerId, item.SortIndex,
        item.Currency, Convert.ToBase64String(item.RowVersion));

    private static void Apply(BuildingEntrance item, SaveBuildingEntranceRequest request)
    {
        item.BuildingName = Trim(request.BuildingName);
        item.EntranceName = Trim(request.EntranceName);
        item.AddressId = request.AddressId;
        item.BuildingLabel = Trim(request.BuildingLabel);
        item.Description = Trim(request.Description);
        item.SortIndex = request.SortIndex;
    }

    private static void Apply(Unit item, SaveUnitRequest request)
    {
        item.Name = Trim(request.Name);
        item.UnitTypeId = request.UnitTypeId;
        item.BuildingEntranceId = request.BuildingEntranceId;
        item.Note = Trim(request.Note);
        item.SortingNumber = request.SortingNumber;
        item.K1 = request.K1;
        item.K2 = request.K2;
        item.K3 = request.K3;
        item.K4 = request.K4;
        item.K5 = request.K5;
        item.FloorNumber = request.FloorNumber;
    }

    private static void Apply(PartnerAccount item, SavePartnerAccountRequest request)
    {
        item.Account = request.Account.Trim();
        item.PartnerId = request.PartnerId;
        item.ContractId = request.ContractId;
        item.AccountNumber = request.AccountNumber;
    }

    private static void Apply(BankAccount item, SaveBankAccountRequest request)
    {
        item.AccountNumber = Trim(request.AccountNumber);
        item.IsActive = request.IsActive;
        item.PartnerId = request.PartnerId;
        item.SortIndex = request.SortIndex;
        item.Currency = request.Currency.Trim().ToUpperInvariant();
    }

    private static string? ValidateEntrance(SaveBuildingEntranceRequest request)
    {
        if (request.BuildingName?.Length > 255 || request.EntranceName?.Length > 255 ||
            request.BuildingLabel?.Length > 255 || request.Description?.Length > 255)
            return "Tekstualna polja ulaza mogu imati najviše 255 znakova.";
        return null;
    }

    private static async Task<string?> ValidateUnitAsync(
        int companyId,
        SaveUnitRequest request,
        SzAppDbContext db,
        CancellationToken cancellationToken)
    {
        if (request.Name?.Length > 255 || request.Note?.Length > 255) return "Naziv i napomena mogu imati najviše 255 znakova.";
        if (new[] { request.K1, request.K2, request.K3, request.K4, request.K5 }.Any(value => value < 0))
            return "Koeficijenti jedinice ne mogu biti negativni.";
        if (request.BuildingEntranceId.HasValue && !await db.Set<BuildingEntrance>().AnyAsync(
                item => item.Id == request.BuildingEntranceId && item.CompanyId == companyId,
                cancellationToken))
            return "Ulaz mora pripadati istoj kompaniji.";
        return null;
    }

    private static async Task<string?> ValidatePartnerAccountAsync(
        int companyId,
        SavePartnerAccountRequest request,
        SzAppDbContext db,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Account) || request.Account.Trim().Length > 10) return "Konto je obavezan i može imati najviše 10 znakova.";
        if (request.AccountNumber <= 0) return "Broj partnerskog konta mora biti pozitivan.";
        if (!await db.Partners.AnyAsync(item => item.Id == request.PartnerId && (item.CompanyId == companyId || item.CompanyId == null), cancellationToken))
            return "Partner mora pripadati istoj kompaniji ili biti globalni dobavljač.";
        if (request.ContractId.HasValue && !await db.Set<Contract>().AnyAsync(
                item => item.Id == request.ContractId && item.CompanyId == companyId,
                cancellationToken))
            return "Ugovor mora pripadati istoj kompaniji.";
        return null;
    }

    private static async Task<string?> ValidateBankAccountAsync(
        int companyId,
        SaveBankAccountRequest request,
        SzAppDbContext db,
        CancellationToken cancellationToken)
    {
        if (request.AccountNumber?.Length > 50) return "Broj računa može imati najviše 50 znakova.";
        if (string.IsNullOrWhiteSpace(request.Currency) || request.Currency.Trim().Length != 3) return "Valuta mora imati tačno tri znaka.";
        if (request.PartnerId.HasValue && !await db.Partners.AnyAsync(
                item => item.Id == request.PartnerId && (item.CompanyId == companyId || item.CompanyId == null),
                cancellationToken))
            return "Partner mora pripadati istoj kompaniji ili biti globalni partner.";
        return null;
    }

    private static async Task<IResult> DeleteCompanyOwnedAsync<T>(
        int companyId,
        int id,
        ClaimsPrincipal principal,
        IMasterDataPermissionService permission,
        SzAppDbContext db,
        HttpContext context,
        CancellationToken cancellationToken) where T : class, ICompanyOwned
    {
        if (!await permission.CanWriteAsync(principal, companyId, cancellationToken)) return Results.Forbid();
        if (!TryIfMatch(context, out var rowVersion)) return InvalidRowVersion();
        var item = await db.Set<T>().SingleOrDefaultAsync(
            value => EF.Property<int>(value, "Id") == id && value.CompanyId == companyId,
            cancellationToken);
        if (item is null) return Results.NotFound();
        db.Entry(item).Property<byte[]>("RowVersion").OriginalValue = rowVersion;
        db.Remove(item);
        return await SaveDeleteAsync(db, cancellationToken);
    }

    private static async Task<IResult?> SaveConcurrentAsync(SzAppDbContext db, CancellationToken cancellationToken)
    {
        try { await db.SaveChangesAsync(cancellationToken); return null; }
        catch (DbUpdateConcurrencyException) { return ConcurrencyConflict(); }
    }

    private static async Task<IResult> SaveDeleteAsync(SzAppDbContext db, CancellationToken cancellationToken)
    {
        try { await db.SaveChangesAsync(cancellationToken); return Results.NoContent(); }
        catch (DbUpdateConcurrencyException) { return ConcurrencyConflict(); }
        catch (DbUpdateException) { return Results.Conflict(new { message = "Zapis se ne može obrisati dok ima povezane podatke." }); }
    }

    private static IResult InvalidRowVersion() => Results.Problem(
        statusCode: StatusCodes.Status428PreconditionRequired,
        title: "Važeći rowversion/If-Match je obavezan.");

    private static IResult ConcurrencyConflict() => Results.Conflict(new
    {
        message = "Podatak je u međuvremenu izmenio drugi korisnik. Osvežite prikaz i pokušajte ponovo."
    });

    private static IResult Unprocessable(string detail) => Results.Problem(
        statusCode: StatusCodes.Status422UnprocessableEntity,
        title: "Poslovno pravilo nije zadovoljeno.",
        detail: detail);

    private static bool TryIfMatch(HttpContext context, out byte[] rowVersion) =>
        ETagCodec.TryDecode(context.Request.Headers.IfMatch.FirstOrDefault(), out rowVersion);

    private static void SetETag(HttpContext context, byte[] rowVersion) =>
        context.Response.Headers.ETag = ETagCodec.Encode(rowVersion);

    private static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
