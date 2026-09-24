using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SzApp.Contracts.Platform;
using SzApp.Data;
using SzApp.Data.Entities;

namespace SzApp.Api.Features.Platform;

internal static class MenuEndpoints
{
    private const string FallbackLanguage = "sr-Latn";

    public static RouteGroupBuilder MapMenuEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/menu", GetMenuAsync);
        return group;
    }

    // companyId is only here for CompanyAccessPolicy consistency with sibling endpoints -- the menu itself is global, not company-scoped data.
    private static async Task<IResult> GetMenuAsync(int companyId, ClaimsPrincipal principal, SzAppDbContext db, CancellationToken ct)
    {
        var staffId = PlatformEndpointHelpers.StaffId(principal);
        var preferredLanguage = await db.Users.AsNoTracking().Where(x => x.Id == staffId)
            .Select(x => x.PreferredLanguage).SingleOrDefaultAsync(ct) ?? FallbackLanguage;

        var items = await db.Set<MenuItem>().AsNoTracking()
            .OrderBy(x => x.ParentId).ThenBy(x => x.SortIndex)
            .ToArrayAsync(ct);

        var resourceKeys = items.Select(x => x.ResourceKey).Distinct().ToArray();
        var translations = await db.Set<Translation>().AsNoTracking()
            .Where(x => x.CompanyId == null && resourceKeys.Contains(x.ResourceKey)
                && (x.LanguageCode == preferredLanguage || x.LanguageCode == FallbackLanguage))
            .ToArrayAsync(ct);
        var captionsByKey = translations.GroupBy(x => x.ResourceKey).ToDictionary(
            g => g.Key,
            g => g.FirstOrDefault(x => x.LanguageCode == preferredLanguage)?.Value
                ?? g.FirstOrDefault(x => x.LanguageCode == FallbackLanguage)?.Value
                ?? g.First().Value);

        var response = items
            .Where(x => x.RequiredRoles is null || x.RequiredRoles
                .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Any(principal.IsInRole))
            .Select(x => new MenuItemResponse(
                x.Id, x.ParentId, x.ResourceKey,
                captionsByKey.GetValueOrDefault(x.ResourceKey, x.ResourceKey),
                x.IconName, x.Path, x.SortIndex))
            .ToArray();

        return Results.Ok(response);
    }
}
