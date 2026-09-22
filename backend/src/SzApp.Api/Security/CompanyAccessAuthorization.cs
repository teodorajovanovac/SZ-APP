using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SzApp.Data;

namespace SzApp.Api.Security;

public sealed class CompanyAccessRequirement : IAuthorizationRequirement;

public sealed class CompanyAccessHandler(SzAppDbContext dbContext)
    : AuthorizationHandler<CompanyAccessRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CompanyAccessRequirement requirement)
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

        if (await dbContext.StaffAccess.AsNoTracking()
                .AnyAsync(x => x.StaffId == staffId && x.CompanyId == companyId, httpContext.RequestAborted))
        {
            context.Succeed(requirement);
        }
    }
}
