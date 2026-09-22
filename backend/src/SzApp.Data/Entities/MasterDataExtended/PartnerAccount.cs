namespace SzApp.Data.Entities;

public sealed class PartnerAccount
{
    public int Id { get; set; }
    public int? CompanyId { get; set; }
    public string Account { get; set; } = string.Empty;
    public int PartnerId { get; set; }
    public int? ContractId { get; set; }
    public int AccountNumber { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public Company? Company { get; set; }
    public Partner Partner { get; set; } = null!;
    public Contract? Contract { get; set; }
}

