using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SzApp.Api.Security;
using SzApp.Contracts.MasterData;
using SzApp.Data;
using SzApp.Data.Entities;

namespace SzApp.Api.Features.MasterData;

/// <summary>
/// GET /api/v1/contracts - cross-company "Ugovori" search/browse.
/// Unlike everything else under Features/MasterData this does NOT sit behind the
/// /api/v1/companies/{companyId}/... route group (that group's CompanyAccessPolicy
/// checks exactly one companyId route value; this endpoint intentionally spans
/// companies). Authorization is therefore hand-rolled here: resolve the caller's
/// permitted company-id set (Root => all companies, else StaffAccess rows - same
/// resolution as Program.cs's BuildCurrentUserAsync/"/api/v1/companies"), then
/// INTERSECT it with whatever the request's mode/companyId/locationCategoryId asks
/// for via ContractsScopeResolver. Every query below filters by that intersected
/// set, so a non-Root caller can never see a row from a company outside their
/// StaffAccess regardless of what scope they request.
/// </summary>
public static class ContractsOverviewEndpoints
{
    private const int MaximumPageSize = 100;

    public static IEndpointRouteBuilder MapContractsOverviewEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/v1/contracts", ListContractsOverviewAsync)
            .WithTags("Contracts")
            .RequireAuthorization();
        return endpoints;
    }

    private static async Task<IResult> ListContractsOverviewAsync(
        [AsParameters] ContractsOverviewQuery query,
        ClaimsPrincipal principal,
        SzAppDbContext db,
        CancellationToken cancellationToken)
    {
        var validationErrors = Validate(query);
        if (validationErrors is not null)
        {
            return Results.ValidationProblem(validationErrors);
        }

        var permittedCompanyIds = await ResolvePermittedCompanyIdsAsync(principal, db, cancellationToken);
        if (permittedCompanyIds.Count == 0)
        {
            return Results.Ok(EmptyPage(query));
        }

        // Reference data needed to resolve the request's scope. Both tables are
        // small (company/location-category counts, not contract counts), so pulling
        // them fully into memory for the closure walk is fine - see task guidance.
        var companies = await db.Companies.AsNoTracking()
            .Where(company => permittedCompanyIds.Contains(company.Id))
            .Select(company => new { company.Id, company.LocationCategoryId })
            .ToArrayAsync(cancellationToken);
        var locationCategories = await db.Set<LocationCategory>().AsNoTracking()
            .Select(category => new { category.Id, category.ParentId })
            .ToArrayAsync(cancellationToken);

        var allowedCompanyIds = ContractsScopeResolver.ResolveCompanyIds(
            query.NormalizedMode,
            query.CompanyId,
            query.LocationCategoryId,
            permittedCompanyIds,
            companies.Select(company => (company.Id, company.LocationCategoryId)).ToArray(),
            locationCategories.Select(category => (category.Id, category.ParentId)).ToArray());

        if (allowedCompanyIds.Count == 0)
        {
            return Results.Ok(EmptyPage(query));
        }

        var contracts = db.Set<Contract>().AsNoTracking()
            .Where(contract => allowedCompanyIds.Contains(contract.CompanyId) && contract.IsActive == query.NormalizedIsActive);

        var idSearch = query.NormalizedIdSearch;
        if (idSearch.Length > 0)
        {
            contracts = contracts.Where(contract =>
                contract.CompanyId.ToString().StartsWith(idSearch) ||
                db.Set<PartnerAccount>().Any(account =>
                    account.ContractId == contract.Id && account.AccountNumber.ToString().StartsWith(idSearch)));
        }

        var search = query.NormalizedSearch;
        if (search.Length > 0)
        {
            var like = $"%{EscapeLike(search)}%";
            contracts = contracts.Where(contract =>
                EF.Functions.Like(contract.Company.ShortName, like, "\\") ||
                (contract.Unit.BuildingEntrance != null &&
                    ((contract.Unit.BuildingEntrance.EntranceName != null && EF.Functions.Like(contract.Unit.BuildingEntrance.EntranceName, like, "\\")) ||
                     (contract.Unit.BuildingEntrance.BuildingName != null && EF.Functions.Like(contract.Unit.BuildingEntrance.BuildingName, like, "\\")))) ||
                (contract.OwnerPartner != null && EF.Functions.Like(contract.OwnerPartner.Name, like, "\\")) ||
                (contract.OwnerPartner != null && contract.OwnerPartner.TaxNumber != null && EF.Functions.Like(contract.OwnerPartner.TaxNumber, like, "\\")) ||
                (contract.Unit.Name != null && EF.Functions.Like(contract.Unit.Name, like, "\\")) ||
                (contract.OwnerPartnerId != null && db.Set<PartnerCommunication>().Any(comm =>
                    comm.PartnerId == contract.OwnerPartnerId && EF.Functions.Like(comm.ValueNormalized, like, "\\"))));
        }

        var total = await contracts.CountAsync(cancellationToken);
        var items = await contracts
            .OrderBy(contract => contract.CompanyId).ThenBy(contract => contract.Id)
            .Skip(query.Skip).Take(query.NormalizedPageSize)
            .Select(contract => new
            {
                Contract = contract,
                Account = db.Set<PartnerAccount>()
                    .Where(account => account.ContractId == contract.Id)
                    .OrderBy(account => account.Id)
                    .FirstOrDefault()
            })
            .Select(row => new ContractOverviewRowResponse(
                row.Contract.CompanyId,
                row.Contract.Company.ShortName,
                row.Account != null ? row.Account.Id : (int?)null,
                row.Account != null ? row.Account.AccountNumber : (int?)null,
                row.Contract.Unit.BuildingEntranceId,
                row.Contract.Unit.BuildingEntrance != null
                    ? (row.Contract.Unit.BuildingEntrance.EntranceName ?? row.Contract.Unit.BuildingEntrance.BuildingName)
                    : null,
                row.Contract.OwnerPartnerId,
                row.Contract.OwnerPartner != null ? row.Contract.OwnerPartner.Name : null,
                row.Contract.UnitId,
                row.Contract.Unit.Name,
                row.Contract.Unit.UnitType != null ? row.Contract.Unit.UnitType.ShortName : null,
                row.Contract.Id,
                row.Contract.IsActive))
            .ToArrayAsync(cancellationToken);

        return Results.Ok(new PagedResponse<ContractOverviewRowResponse>(items, query.NormalizedPage, query.NormalizedPageSize, total));
    }

    private static PagedResponse<ContractOverviewRowResponse> EmptyPage(ContractsOverviewQuery query) => new(
        [], query.NormalizedPage, query.NormalizedPageSize, 0);

    /// <summary>
    /// Root => every company. Otherwise exactly the companies the caller has a
    /// StaffAccess row for - identical resolution to Program.cs's
    /// BuildCurrentUserAsync and the "/api/v1/companies" endpoint, copied here
    /// because this endpoint cannot use the single-company CompanyAccessPolicy.
    /// </summary>
    private static async Task<IReadOnlySet<int>> ResolvePermittedCompanyIdsAsync(
        ClaimsPrincipal principal,
        SzAppDbContext db,
        CancellationToken cancellationToken)
    {
        if (principal.IsInRole(SecurityConstants.RootRole))
        {
            return (await db.Companies.AsNoTracking().Select(company => company.Id).ToArrayAsync(cancellationToken)).ToHashSet();
        }

        var staffIdValue = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(staffIdValue, out var staffId))
        {
            return new HashSet<int>();
        }

        return (await db.StaffAccess.AsNoTracking()
            .Where(access => access.StaffId == staffId)
            .Select(access => access.CompanyId)
            .Distinct()
            .ToArrayAsync(cancellationToken)).ToHashSet();
    }

    private static Dictionary<string, string[]>? Validate(ContractsOverviewQuery query)
    {
        var mode = query.NormalizedMode;
        if (mode is not ("single" or "all" or "location"))
        {
            return new Dictionary<string, string[]> { ["mode"] = ["Mode mora biti 'single', 'all' ili 'location'."] };
        }
        if (mode == "single" && query.CompanyId is null)
        {
            return new Dictionary<string, string[]> { ["companyId"] = ["companyId je obavezan kada je mode 'single'."] };
        }
        if (mode == "location" && query.LocationCategoryId is null)
        {
            return new Dictionary<string, string[]> { ["locationCategoryId"] = ["locationCategoryId je obavezan kada je mode 'location'."] };
        }
        return null;
    }

    private static string EscapeLike(string value) => value
        .Replace("\\", "\\\\", StringComparison.Ordinal)
        .Replace("%", "\\%", StringComparison.Ordinal)
        .Replace("_", "\\_", StringComparison.Ordinal)
        .Replace("[", "\\[", StringComparison.Ordinal);

    public sealed class ContractsOverviewQuery
    {
        public string? Mode { get; init; }
        public int? CompanyId { get; init; }
        public int? LocationCategoryId { get; init; }
        public string? IdSearch { get; init; }
        public string? Search { get; init; }
        public bool? IsActive { get; init; }
        public int? Page { get; init; }
        public int? PageSize { get; init; }

        public string NormalizedMode => Mode?.Trim().ToLowerInvariant() ?? string.Empty;
        public string NormalizedIdSearch => IdSearch?.Trim() ?? string.Empty;
        public string NormalizedSearch => Search?.Trim() ?? string.Empty;
        public bool NormalizedIsActive => IsActive ?? true;
        public int NormalizedPage => Math.Max(1, Page ?? 1);
        public int NormalizedPageSize => Math.Clamp(PageSize ?? 25, 1, MaximumPageSize);
        public int Skip => (NormalizedPage - 1) * NormalizedPageSize;
    }
}
