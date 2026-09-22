using Microsoft.AspNetCore.Identity;

namespace SzApp.Data.Entities;

public sealed class ApplicationUser : IdentityUser<int>
{
    public string PreferredLanguage { get; set; } = "sr-Latn";
    public string? LastIp { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<StaffAccess> CompanyAccess { get; } = new List<StaffAccess>();
}

public enum StaffRole
{
    Upravnik = 1,
    Moderator = 2,
    Review = 3
}

public sealed class StaffAccess
{
    public int Id { get; set; }
    public int StaffId { get; set; }
    public int CompanyId { get; set; }
    public StaffRole StaffRole { get; set; }
    public ApplicationUser Staff { get; set; } = null!;
    public Company Company { get; set; } = null!;
}
