using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SzApp.Api.Security;
using SzApp.Data;
using SzApp.Data.Entities;

namespace SzApp.Api.Features.MasterData;

public interface IMasterDataPermissionService
{
    Task<bool> CanWriteAsync(ClaimsPrincipal principal, int companyId, CancellationToken cancellationToken);
    bool IsRoot(ClaimsPrincipal principal);
}

public sealed class MasterDataPermissionService(SzAppDbContext dbContext) : IMasterDataPermissionService
{
    public bool IsRoot(ClaimsPrincipal principal) => principal.IsInRole(SecurityConstants.RootRole);

    public async Task<bool> CanWriteAsync(
        ClaimsPrincipal principal,
        int companyId,
        CancellationToken cancellationToken)
    {
        if (IsRoot(principal))
        {
            return true;
        }

        var staffIdValue = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(staffIdValue, out var staffId))
        {
            return false;
        }

        return await dbContext.StaffAccess.AsNoTracking().AnyAsync(
            access => access.StaffId == staffId &&
                      access.CompanyId == companyId &&
                      access.StaffRole != StaffRole.Review,
            cancellationToken);
    }
}

