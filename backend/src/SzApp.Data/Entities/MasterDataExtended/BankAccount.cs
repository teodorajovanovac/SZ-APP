namespace SzApp.Data.Entities;

public sealed class BankAccount
{
    public int Id { get; set; }
    public string? AccountNumber { get; set; }
    public bool IsActive { get; set; }
    public int? PartnerId { get; set; }
    public int? CompanyId { get; set; }
    public int? SortIndex { get; set; }
    public string Currency { get; set; } = "RSD";
    public byte[] RowVersion { get; set; } = [];
    public Partner? Partner { get; set; }
    public Company? Company { get; set; }
}

