namespace SzApp.Api.Security;

public static class SecurityConstants
{
    public const string RootRole = "Root";
    public const string UpravnikRole = "Upravnik";
    public const string ModeratorRole = "Moderator";
    public const string ReviewRole = "Review";
    public const string CompanyAccessPolicy = "CompanyAccess";

    /// <summary>Any mutation that isn't ledger posting/reversal. Minimum role: Moderator.</summary>
    public const string CompanyWritePolicy = "CompanyWrite";

    /// <summary>
    /// Posting/reversing ledger entries and bank statements (P12: a distinct permission from
    /// plain write, even though today's role population is the same set — Moderator or Upravnik,
    /// per the owner's P13 decision). Kept as its own named policy/requirement so tightening posting
    /// rights later (e.g. a dedicated StaffAccess flag) doesn't touch general write endpoints.
    /// </summary>
    public const string CompanyPostPolicy = "CompanyPost";

    /// <summary>Company-scoped admin actions (global-ish catalogues edited via a company route). Minimum role: Upravnik.</summary>
    public const string CompanyAdminPolicy = "CompanyAdmin";

    public static readonly string[] AllRoles = [RootRole, UpravnikRole, ModeratorRole, ReviewRole];
}
