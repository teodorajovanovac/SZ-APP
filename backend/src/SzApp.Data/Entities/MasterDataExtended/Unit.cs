namespace SzApp.Data.Entities;

public sealed class Unit : ICompanyOwned
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string? Name { get; set; }
    public int? ContractId { get; set; }
    public int? UnitTypeId { get; set; }
    public int? BuildingEntranceId { get; set; }
    public string? Note { get; set; }
    public int? SortingNumber { get; set; }
    public decimal? K1 { get; set; }
    public decimal? K2 { get; set; }
    public decimal? K3 { get; set; }
    public decimal? K4 { get; set; }
    public decimal? K5 { get; set; }
    public int? FloorNumber { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public Company Company { get; set; } = null!;
    public Contract? Contract { get; set; }
    public ShortList? UnitType { get; set; }
    public BuildingEntrance? BuildingEntrance { get; set; }
    public ICollection<Contract> ContractHistory { get; } = new List<Contract>();
}

