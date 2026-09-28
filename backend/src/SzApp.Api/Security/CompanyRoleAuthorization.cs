using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SzApp.Data;
using SzApp.Data.Entities;

namespace SzApp.Api.Security;

/// <summary>
/// Requires the caller's <see cref="StaffAccess.StaffRole"/> for the company resolved from the
/// route's "companyId" segment to be at least as privileged as <paramref name="MinRole"/>.
/// Root always passes. This is the single mechanism SEC-02 introduces to replace both the
/// "no role check at all" gaps in LedgerBanking and the dead global-Identity-role checks in
/// Billing (those global roles are only ever assigned to Root - see Program.cs bootstrap).
/// </summary>
public sealed record CompanyRoleRequirement(StaffRole MinRole) : IAuthorizationRequirement
{
    /// <summary>Lower StaffRole numeric value = more privileged (Upravnik=1 .. Review=3).</summary>
    public static bool Satisfies(StaffRole actual, StaffRole minRole) => actual <= minRole;
}

public sealed class CompanyRoleHandler(SzAppDbContext dbContext)
    : AuthorizationHandler<CompanyRoleRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CompanyRoleRequirement requirement)
    {
        if (context.User.IsInRole(SecurityConstants.RootRole))
        {
            context.Succeed(requirement);
            return;
        }

        if (context.Resource is not HttpContext httpContext ||
            !int.TryParse(httpContext.Request.RouteValues["companyId"]?.ToString(), out var companyId) ||
            !int.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var staffId))
        {
            return;
        }

        var role = await dbContext.StaffAccess.AsNoTracking()
            .Where(x => x.StaffId == staffId && x.CompanyId == companyId)
            .Select(x => (StaffRole?)x.StaffRole)
            .SingleOrDefaultAsync(httpContext.RequestAborted);

        if (role is not null && CompanyRoleRequirement.Satisfies(role.Value, requirement.MinRole))
        {
            context.Succeed(requirement);
        }
    }
}
