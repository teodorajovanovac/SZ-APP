namespace SzApp.Data.Entities;

public sealed class BuildingEntrance : ICompanyOwned
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string? BuildingName { get; set; }
    public string? EntranceName { get; set; }
    public int? AddressId { get; set; }
    public string? BuildingLabel { get; set; }
    public string? Description { get; set; }
    public int? SortIndex { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public Company Company { get; set; } = null!;
    public Address? Address { get; set; }
}

