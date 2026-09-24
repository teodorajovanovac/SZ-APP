namespace SzApp.Api.Features.MasterData;

/// <summary>
/// Pure company-id-set resolution for the cross-company Contracts overview endpoint.
/// Kept free of EF/DB so the security-critical intersection logic (a non-Root caller
/// must never see a company outside their StaffAccess, no matter what scope they ask
/// for) is unit-testable without a database.
/// </summary>
public static class ContractsScopeResolver
{
    /// <summary>
    /// Resolves the final set of company ids a request may see: whatever the
    /// mode/companyId/locationCategoryId asks for, INTERSECTED with the caller's
    /// own permitted set. The intersection is the security boundary - it is applied
    /// last, after the mode-specific set is computed, so no mode can escape it.
    /// </summary>
    public static IReadOnlySet<int> ResolveCompanyIds(
        string mode,
        int? companyId,
        int? locationCategoryId,
        IReadOnlyCollection<int> permittedCompanyIds,
        IReadOnlyCollection<(int CompanyId, int? LocationCategoryId)> companies,
        IReadOnlyCollection<(int Id, int? ParentId)> locationCategories)
    {
        var permitted = permittedCompanyIds as IReadOnlySet<int> ?? permittedCompanyIds.ToHashSet();

        IEnumerable<int> requested = mode switch
        {
            "single" => companyId.HasValue ? [companyId.Value] : [],
            "all" => permitted,
            "location" => locationCategoryId.HasValue
                ? companies
                    .Where(company => company.LocationCategoryId.HasValue &&
                        IsSelfOrDescendant(company.LocationCategoryId.Value, locationCategoryId.Value, locationCategories))
                    .Select(company => company.CompanyId)
                : [],
            _ => []
        };

        return requested.Where(permitted.Contains).ToHashSet();
    }

    /// <summary>
    /// True if categoryId == rootId, or rootId is an ancestor of categoryId, walking
    /// LocationCategory.ParentId. Guards against cycles with a bounded step count.
    /// </summary>
    private static bool IsSelfOrDescendant(
        int categoryId,
        int rootId,
        IReadOnlyCollection<(int Id, int? ParentId)> categories)
    {
        var current = categoryId;
        for (var step = 0; step <= categories.Count; step++)
        {
            if (current == rootId)
            {
                return true;
            }

            var parentId = categories.FirstOrDefault(category => category.Id == current).ParentId;
            if (parentId is null)
            {
                return false;
            }

            current = parentId.Value;
        }

        return false;
    }
}
