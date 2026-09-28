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

    private static async Task<IResult> CreateAsync(CreateStaffRequest request, ClaimsPrincipal principal, SzAppDbContext db, UserManager<ApplicationUser> users, CancellationToken ct)
    {
        var caller = await CallerAsync(principal, db, ct);
        if (!caller.IsAdmin || !StaffManagementPolicy.CanCreate(caller.IsRoot, caller.UpravnikCompanies, request.IsRoot, request.CompanyId))
            return Results.Forbid();
        if (!StaffManagementPolicy.Languages.Contains(request.PreferredLanguage)) return Invalid("preferredLanguage", "Nepoznat jezik.");
        StaffRole role = default;
        if (request.CompanyId is int companyId)
        {
            // SEC-05: defined enum value only, and never above the caller's own role (Upravnik is the ceiling).
            if (!Enum.TryParse(request.StaffRole, true, out role) || !Enum.IsDefined(role) || int.TryParse(request.StaffRole, out _))
                return Invalid("staffRole", "Nepoznata uloga.");
            if (!await db.Companies.AnyAsync(c => c.Id == companyId, ct)) return Invalid("companyId", "Stambena zajednica ne postoji.");
        }

        if (string.IsNullOrEmpty(request.TemporaryPassword)) return Invalid("temporaryPassword", "Privremena lozinka je obavezna.");
        var email = (request.Email ?? string.Empty).Trim();
        // One transaction: Identity user + must-change claim + Root role + first grant, all or nothing.
        // Wrapped in the execution strategy because the context uses EnableRetryOnFailure (FIN-10).
        return await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            db.ChangeTracker.Clear();
            await using var tx = await db.Database.BeginTransactionAsync(ct);
            var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true, IsActive = true, PreferredLanguage = request.PreferredLanguage };
            var result = await users.CreateAsync(user, request.TemporaryPassword);
            if (result.Succeeded) result = await users.AddClaimAsync(user, new Claim(StaffManagementPolicy.MustChangePasswordClaim, "true"));
            if (result.Succeeded && request.IsRoot) result = await users.AddToRoleAsync(user, SecurityConstants.RootRole);
            if (!result.Succeeded) return IdentityProblem(result); // tx disposed uncommitted -> rollback
            if (request.CompanyId is int cid)
            {
                db.StaffAccess.Add(new StaffAccess { StaffId = user.Id, CompanyId = cid, StaffRole = role });
                await db.SaveChangesAsync(ct);
            }
            await tx.CommitAsync(ct);
            return Results.Created($"/api/v1/staff/{user.Id}", await DetailAsync(user, caller, db, users, ct));
        });
    }

    private static async Task<IResult> UpdateAsync(int staffId, UpdateStaffRequest request, ClaimsPrincipal principal, SzAppDbContext db, UserManager<ApplicationUser> users, CancellationToken ct)
    {
        var caller = await CallerAsync(principal, db, ct);
        if (!caller.IsAdmin) return Results.Forbid();
        var (user, isRoot, allowed) = await TargetAsync(staffId, caller, db, users, ct);
        if (user is null) return Results.NotFound();
        if (!allowed || !StaffManagementPolicy.CanChangeRootFlag(caller.IsRoot, isRoot, request.IsRoot)) return Results.Forbid();
        if (!StaffManagementPolicy.Languages.Contains(request.PreferredLanguage)) return Invalid("preferredLanguage", "Nepoznat jezik.");

        var email = (request.Email ?? string.Empty).Trim();
        var deactivated = user.IsActive && !request.IsActive;
        var emailChanged = !string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase);
        user.Email = email; user.UserName = email;
        user.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();
        user.PreferredLanguage = request.PreferredLanguage;
        user.IsActive = request.IsActive;
        var result = await users.UpdateAsync(user); // validates unique email/username
        if (result.Succeeded && request.IsRoot != isRoot)
            result = request.IsRoot ? await users.AddToRoleAsync(user, SecurityConstants.RootRole) : await users.RemoveFromRoleAsync(user, SecurityConstants.RootRole);
        // SEC-10: invalidate existing cookies (checked every ValidationInterval) on deactivation,
        // role change or login-identifier change.
        if (result.Succeeded && (deactivated || emailChanged || request.IsRoot != isRoot)) result = await users.UpdateSecurityStampAsync(user);
        if (!result.Succeeded) return IdentityProblem(result);
        return Results.Ok(await DetailAsync(user, caller, db, users, ct));
    }

    private static async Task<IResult> ResetPasswordAsync(int staffId, ResetStaffPasswordRequest request, ClaimsPrincipal principal, SzAppDbContext db, UserManager<ApplicationUser> users, CancellationToken ct)
    {
        var caller = await CallerAsync(principal, db, ct);
        if (!caller.IsAdmin) return Results.Forbid();
        var (user, _, allowed) = await TargetAsync(staffId, caller, db, users, ct);
        if (user is null) return Results.NotFound();
        if (!allowed) return Results.Forbid();
        if (string.IsNullOrEmpty(request.TemporaryPassword)) return Invalid("temporaryPassword", "Privremena lozinka je obavezna.");
        var token = await users.GeneratePasswordResetTokenAsync(user);
        var result = await users.ResetPasswordAsync(user, token, request.TemporaryPassword); // also rotates the security stamp
        if (!result.Succeeded) return IdentityProblem(result);
        if (!(await users.GetClaimsAsync(user)).Any(c => c.Type == StaffManagementPolicy.MustChangePasswordClaim))
            await users.AddClaimAsync(user, new Claim(StaffManagementPolicy.MustChangePasswordClaim, "true"));
        await users.SetLockoutEndDateAsync(user, null);
        await users.ResetAccessFailedCountAsync(user);
        return Results.Ok(await DetailAsync(user, caller, db, users, ct));
    }

    private static IResult IdentityProblem(IdentityResult result) =>
        Results.ValidationProblem(result.Errors.GroupBy(e => e.Code).ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray()));

    private static IResult Invalid(string field, string message) =>
        Results.ValidationProblem(new Dictionary<string, string[]> { [field] = [message] });
}
