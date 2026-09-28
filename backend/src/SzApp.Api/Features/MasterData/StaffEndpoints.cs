using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SzApp.Api.Security;
using SzApp.Contracts;
using SzApp.Contracts.MasterData;
using SzApp.Data;
using SzApp.Data.Entities;

namespace SzApp.Api.Features.MasterData;

// GAP-23: staff (Identity user) administration. Rules live in StaffManagementPolicy; this file only
// loads facts and applies them. Root manages everyone, Upravnik only accounts inside their companies.
public static class StaffEndpoints
{
    private sealed record Caller(int Id, bool IsRoot, HashSet<int> UpravnikCompanies)
    {
        public bool IsAdmin => IsRoot || UpravnikCompanies.Count > 0;
    }

    public static IEndpointRouteBuilder MapStaffEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/staff").WithTags("Staff").RequireAuthorization();
        group.MapGet("/", ListAsync);
        group.MapGet("/{staffId:int}", GetAsync);
        group.MapPost("/", CreateAsync);
        group.MapPut("/{staffId:int}", UpdateAsync);
        group.MapPost("/{staffId:int}/reset-password", ResetPasswordAsync);
        return endpoints;
    }

    private static async Task<Caller> CallerAsync(ClaimsPrincipal principal, SzAppDbContext db, CancellationToken ct)
    {
        var id = int.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isRoot = principal.IsInRole(SecurityConstants.RootRole);
        var companies = await db.StaffAccess.AsNoTracking()
            .Where(x => x.StaffId == id && x.StaffRole == StaffRole.Upravnik)
            .Select(x => x.CompanyId).ToListAsync(ct);
        return new Caller(id, isRoot, companies.ToHashSet());
    }

    private static IQueryable<int> RootUserIds(SzAppDbContext db) =>
        db.UserRoles.Where(ur => db.Roles.Any(r => r.Id == ur.RoleId && r.Name == SecurityConstants.RootRole)).Select(ur => ur.UserId);

    // What a caller may *see*: Root everything; Upravnik non-Root users with any grant in their companies.
    private static IQueryable<ApplicationUser> Visible(SzAppDbContext db, Caller caller)
    {
        var users = db.Users.AsNoTracking();
        if (caller.IsRoot) return users;
        var companies = caller.UpravnikCompanies.ToArray();
        var roots = RootUserIds(db);
        return users.Where(u => u.CompanyAccess.Any(a => companies.Contains(a.CompanyId)) && !roots.Contains(u.Id));
    }

    private static async Task<IResult> ListAsync([AsParameters] MasterDataPageQuery query, ClaimsPrincipal principal, SzAppDbContext db, CancellationToken ct)
    {
        var caller = await CallerAsync(principal, db, ct);
        if (!caller.IsAdmin) return Results.Forbid();
        var users = Visible(db, caller);
        if (query.NormalizedSearch.Length > 0) users = users.Where(u => u.Email != null && u.Email.Contains(query.NormalizedSearch));
        users = query.NormalizedDescending ? users.OrderByDescending(u => u.Email) : users.OrderBy(u => u.Email);
        var roots = RootUserIds(db);
        var total = await users.CountAsync(ct);
        var items = await users.Skip(query.Skip).Take(query.NormalizedPageSize)
            .Select(u => new StaffListItemResponse(u.Id, u.Email ?? string.Empty, u.IsActive, u.PreferredLanguage,
                roots.Contains(u.Id), u.CompanyAccess.Count, u.LastLoginAt))
            .ToArrayAsync(ct);
        return Results.Ok(new PagedResponse<StaffListItemResponse>(items, query.NormalizedPage, query.NormalizedPageSize, total));
    }

    private static async Task<IResult> GetAsync(int staffId, ClaimsPrincipal principal, SzAppDbContext db, UserManager<ApplicationUser> users, CancellationToken ct)
    {
        var caller = await CallerAsync(principal, db, ct);
        if (!caller.IsAdmin) return Results.Forbid();
        var user = await Visible(db, caller).SingleOrDefaultAsync(u => u.Id == staffId, ct);
        if (user is null) return Results.NotFound();
        return Results.Ok(await DetailAsync(user, caller, db, users, ct));
    }

    private static async Task<StaffDetailResponse> DetailAsync(ApplicationUser user, Caller caller, SzAppDbContext db, UserManager<ApplicationUser> users, CancellationToken ct)
    {
        var grants = await db.StaffAccess.AsNoTracking().Where(a => a.StaffId == user.Id)
            .OrderBy(a => a.Company.ShortName)
            .Select(a => new StaffGrantResponse(a.Id, a.CompanyId, a.Company.ShortName, a.StaffRole.ToString()))
            .ToArrayAsync(ct);
        var isRoot = await users.IsInRoleAsync(user, SecurityConstants.RootRole);
        var claims = await users.GetClaimsAsync(user);
        var canManage = StaffManagementPolicy.CanManage(caller.IsRoot, caller.Id, caller.UpravnikCompanies,
            user.Id, isRoot, grants.Select(g => g.CompanyId).ToArray());
        return new StaffDetailResponse(user.Id, user.Email ?? string.Empty, user.PhoneNumber, user.IsActive, user.PreferredLanguage,
            isRoot, claims.Any(c => c.Type == StaffManagementPolicy.MustChangePasswordClaim), await users.IsLockedOutAsync(user),
            user.LastLoginAt, user.LastIp, canManage, grants);
    }

    // Loads a tracked target and decides whether the caller may mutate it.
    private static async Task<(ApplicationUser? User, bool IsRoot, bool Allowed)> TargetAsync(int staffId, Caller caller, SzAppDbContext db, UserManager<ApplicationUser> users, CancellationToken ct)
    {
        var user = await users.FindByIdAsync(staffId.ToString());
        if (user is null) return (null, false, false);
        var isRoot = await users.IsInRoleAsync(user, SecurityConstants.RootRole);
        var companies = await db.StaffAccess.AsNoTracking().Where(a => a.StaffId == staffId).Select(a => a.CompanyId).ToArrayAsync(ct);
        return (user, isRoot, StaffManagementPolicy.CanManage(caller.IsRoot, caller.Id, caller.UpravnikCompanies, user.Id, isRoot, companies));
    }

    private static IResult IdentityProblem(IdentityResult result) =>
        Results.ValidationProblem(result.Errors.GroupBy(e => e.Code).ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray()));

    private static IResult Invalid(string field, string message) =>
        Results.ValidationProblem(new Dictionary<string, string[]> { [field] = [message] });
}
