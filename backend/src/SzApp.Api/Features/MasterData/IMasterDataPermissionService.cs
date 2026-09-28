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

    // SEC-05: managing StaffAccess grants (who can access a company, at what role) is a more
    // sensitive action than ordinary master-data writes -- a Moderator (CanWriteAsync == true)
    // must not be able to create/edit/delete grants, only Upravnik or Root.
    Task<bool> CanManageStaffAccessAsync(ClaimsPrincipal principal, int companyId, CancellationToken cancellationToken);

    // Caller's own StaffRole in this company; null for Root (no per-company row) or when the
    // caller has no StaffAccess row here at all.
    Task<StaffRole?> GetOwnRoleAsync(ClaimsPrincipal principal, int companyId, CancellationToken cancellationToken);

    int? GetStaffId(ClaimsPrincipal principal);
}

public sealed class MasterDataPermissionService(SzAppDbContext dbContext) : IMasterDataPermissionService
{
    public bool IsRoot(ClaimsPrincipal principal) => principal.IsInRole(SecurityConstants.RootRole);

    public int? GetStaffId(ClaimsPrincipal principal) =>
        int.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var staffId) ? staffId : null;

    public async Task<bool> CanWriteAsync(
        ClaimsPrincipal principal,
        int companyId,
        CancellationToken cancellationToken)
    {
        if (IsRoot(principal))
        {
            return true;
        }

        var staffId = GetStaffId(principal);
        if (staffId is null)
        {
            return false;
        }

        return await dbContext.StaffAccess.AsNoTracking().AnyAsync(
            access => access.StaffId == staffId &&
                      access.CompanyId == companyId &&
                      access.StaffRole != StaffRole.Review,
            cancellationToken);
    }

    public async Task<bool> CanManageStaffAccessAsync(
        ClaimsPrincipal principal,
        int companyId,
        CancellationToken cancellationToken)
    {
        if (IsRoot(principal))
        {
            return true;
        }

        var role = await GetOwnRoleAsync(principal, companyId, cancellationToken);
        return role == StaffRole.Upravnik;
    }

    public async Task<StaffRole?> GetOwnRoleAsync(
        ClaimsPrincipal principal,
        int companyId,
        CancellationToken cancellationToken)
    {
        var staffId = GetStaffId(principal);
        if (staffId is null)
        {
            return null;
        }

        return await dbContext.StaffAccess.AsNoTracking()
            .Where(access => access.StaffId == staffId && access.CompanyId == companyId)
            .Select(access => (StaffRole?)access.StaffRole)
            .FirstOrDefaultAsync(cancellationToken);
    }
}

