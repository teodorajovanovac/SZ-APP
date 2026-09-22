namespace SzApp.Data.Entities;

public sealed class Contract : ICompanyOwned
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int UnitId { get; set; }
    public int? AccountNumber { get; set; }
    public int? OwnerPartnerId { get; set; }
    public int? InvoicePartnerId { get; set; }
    public int? TenantPartnerId { get; set; }
    public DateOnly ContractDate { get; set; }
    public DateOnly? ContractEndDate { get; set; }
    public DateOnly? InvoiceStartDate { get; set; }
    public DateOnly? InvoiceEndDate { get; set; }
    public bool IsActive { get; set; }
    public string? Note { get; set; }
    public string? InvoiceDeliveryLocation { get; set; }
    public int? InvoiceDeliveryUnitId { get; set; }
    public int? InvoiceLegacyMasterId { get; set; }
    public bool IsPrintInvoiceMandatory { get; set; }
    public bool IsPrintInvoiceToPostOffice { get; set; }
    public bool IsPrintInvoiceSkipped { get; set; }
    public string? ExportExternalAccount { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public Company Company { get; set; } = null!;
    public Unit Unit { get; set; } = null!;
    public Partner? OwnerPartner { get; set; }
    public Partner? InvoicePartner { get; set; }
    public Partner? TenantPartner { get; set; }
    public Unit? InvoiceDeliveryUnit { get; set; }
}

