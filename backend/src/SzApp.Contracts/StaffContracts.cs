namespace SzApp.Contracts;

// GAP-23: staff (application users) administration. "Staff" in the legacy model is the Identity
// user (ApplicationUser); per-company rights are StaffAccess grants.
public sealed record StaffListItemResponse(
    int Id,
    string Email,
    bool IsActive,
    string PreferredLanguage,
    bool IsRoot,
    int CompanyCount,
    DateTimeOffset? LastLoginAt);

public sealed record StaffGrantResponse(int AccessId, int CompanyId, string CompanyName, string StaffRole);

public sealed record StaffDetailResponse(
    int Id,
    string Email,
    string? PhoneNumber,
    bool IsActive,
    string PreferredLanguage,
    bool IsRoot,
    bool MustChangePassword,
    bool IsLockedOut,
    DateTimeOffset? LastLoginAt,
    string? LastIp,
    bool CanManage,
    IReadOnlyCollection<StaffGrantResponse> Grants);

/// <summary>Non-Root callers must pass an initial grant (CompanyId + StaffRole) for a company they manage.</summary>
public sealed record CreateStaffRequest(
    string Email,
    string TemporaryPassword,
    string PreferredLanguage,
    bool IsRoot,
    int? CompanyId,
    string? StaffRole);

public sealed record UpdateStaffRequest(string Email, string? PhoneNumber, string PreferredLanguage, bool IsActive, bool IsRoot);

public sealed record ResetStaffPasswordRequest(string TemporaryPassword);

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);
