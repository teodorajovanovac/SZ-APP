using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SzApp.Api.Security;
using SzApp.Data;

namespace SzApp.Api.Features.Platform;

internal static class PlatformEndpointHelpers
{
    public static int StaffId(ClaimsPrincipal principal) =>
        int.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public static async Task<string> RoleAsync(ClaimsPrincipal principal, int companyId, SzAppDbContext db, CancellationToken cancellationToken)
    {
        if (principal.IsInRole(SecurityConstants.RootRole)) return SecurityConstants.RootRole;
        var staffId = StaffId(principal);
        return await db.StaffAccess.AsNoTracking().Where(x => x.CompanyId == companyId && x.StaffId == staffId)
            .Select(x => x.StaffRole.ToString()).SingleAsync(cancellationToken);
    }

    public static async Task<bool> CanWriteAsync(ClaimsPrincipal principal, int companyId, SzAppDbContext db, CancellationToken cancellationToken) =>
        (await RoleAsync(principal, companyId, db, cancellationToken)) != SecurityConstants.ReviewRole;

    public static IResult Unprocessable(string detail) => Results.Problem(statusCode: 422, title: "Poslovno pravilo nije zadovoljeno.", detail: detail);
    public static IResult Conflict() => Results.Conflict(new { message = "Podatak je u međuvremenu promenjen. Osvežite prikaz." });
}

