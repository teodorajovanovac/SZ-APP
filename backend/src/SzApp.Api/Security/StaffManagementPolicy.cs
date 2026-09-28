using System.Security.Claims;

namespace SzApp.Api.Security;

/// <summary>
/// GAP-23 / SEC-05: who may administer which staff account. Pure rules so they are unit-testable;
/// the endpoints load the facts (caller's Upravnik companies, target's grants/Root flag) and ask here.
/// </summary>
public static class StaffManagementPolicy
{
    /// <summary>Stored as an Identity user claim (no schema change); removed on successful password change.</summary>
    public const string MustChangePasswordClaim = "szapp:must_change_password";

    public static readonly string[] Languages = ["sr-Latn", "sr-Cyrl", "en"];

    public static bool MustChangePassword(ClaimsPrincipal principal) => principal.HasClaim(MustChangePasswordClaim, "true");

    /// <summary>
    /// Root manages everyone except their own account (self-service goes through change-password, so a
    /// Root can never deactivate/demote themselves). Upravnik manages a non-Root account only when
    /// every company that account can access is one where the caller is Upravnik -- otherwise editing it
    /// (e.g. resetting its password) would be a takeover of access in companies the caller doesn't run.
    /// </summary>
    public static bool CanManage(bool callerIsRoot, int callerId, IReadOnlySet<int> callerUpravnikCompanies,
        int targetId, bool targetIsRoot, IReadOnlyCollection<int> targetCompanies)
    {
        if (callerId == targetId) return false;
        if (callerIsRoot) return true;
        if (targetIsRoot) return false;
        return targetCompanies.Count > 0 && targetCompanies.All(callerUpravnikCompanies.Contains);
    }

    /// <summary>Only Root may create Root. Upravnik must attach the new user to a company they manage.</summary>
    public static bool CanCreate(bool callerIsRoot, IReadOnlySet<int> callerUpravnikCompanies, bool newIsRoot, int? companyId)
    {
        if (callerIsRoot) return true;
        return !newIsRoot && companyId is int id && callerUpravnikCompanies.Contains(id);
    }

    /// <summary>Only Root may change anyone's Root flag.</summary>
    public static bool CanChangeRootFlag(bool callerIsRoot, bool currentIsRoot, bool requestedIsRoot) =>
        callerIsRoot || currentIsRoot == requestedIsRoot;

    /// <summary>Anything whose name suggests a credential is never returned by the API.</summary>
    public static bool IsSecretSettingKey(string key) =>
        new[] { "password", "passwd", "pwd", "secret", "token", "key", "credential", "connectionstring" }
            .Any(x => key.Contains(x, StringComparison.OrdinalIgnoreCase));
}
