namespace SzApp.Api.Security;

public static class SecurityConstants
{
    public const string RootRole = "Root";
    public const string UpravnikRole = "Upravnik";
    public const string ModeratorRole = "Moderator";
    public const string ReviewRole = "Review";
    public const string CompanyAccessPolicy = "CompanyAccess";

    public static readonly string[] AllRoles = [RootRole, UpravnikRole, ModeratorRole, ReviewRole];
}
