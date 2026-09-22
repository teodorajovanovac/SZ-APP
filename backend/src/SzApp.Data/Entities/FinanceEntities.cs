namespace SzApp.Data.Entities;

public sealed class JournalEntry : ICompanyOwned
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public DateOnly PostingDate { get; set; }
    public DateOnly? DueDate { get; set; }
    public decimal Balance { get; set; }
    public string? Note { get; set; }
    public string Description { get; set; } = string.Empty;
    public int? JournalEntryTypeId { get; set; }
    public string Currency { get; set; } = "RSD";
    public bool IsPosted { get; set; }
    public DateTimeOffset? PostedAt { get; set; }
    public int? PostedUserId { get; set; }
    public int? ReversalOfId { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public Company Company { get; set; } = null!;
    public ApplicationUser? PostedUser { get; set; }
    public JournalEntry? ReversalOf { get; set; }
    public ShortList? JournalEntryType { get; set; }
    public ICollection<LedgerEntry> Lines { get; } = new List<LedgerEntry>();
}

public sealed class LedgerEntry : ICompanyOwned
{
    public int Id { get; set; }
    public int JournalEntryId { get; set; }
    public int CompanyId { get; set; }
    public string Account { get; set; } = string.Empty;
    public DateOnly PostingDate { get; set; }
    public DateOnly? DueDate { get; set; }
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public int? LineTypeId { get; set; }
    public string? DocumentRef { get; set; }
    public string? Note { get; set; }
    public string? Parameters { get; set; }
    public string? Description { get; set; }
    public int Priority { get; set; }
    public JournalEntry JournalEntry { get; set; } = null!;
    public Company Company { get; set; } = null!;
    public ShortList? LineType { get; set; }
}

public sealed class Invoice : ICompanyOwned
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int PartnerId { get; set; }
    public string SequenceNumber { get; set; } = string.Empty;
    public DateOnly IssueDate { get; set; }
    public DateOnly DueDate { get; set; }
    public string PartnerName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? PostalCode { get; set; }
    public string City { get; set; } = string.Empty;
    public string? TaxNumber { get; set; }
    public string? RegistrationNumber { get; set; }
    public string Currency { get; set; } = "RSD";
    public decimal Amount { get; set; }
    public decimal VatRate { get; set; }
    public decimal VatAmount { get; set; }
    public decimal Total { get; set; }
    public decimal InterestAmount { get; set; }
    public decimal InvoiceTotal { get; set; }
    public string? InvoiceDeliveryLocation { get; set; }
    public DateTimeOffset? CancelledAt { get; set; }
    public bool IsCancelled { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public Company Company { get; set; } = null!;
    public Partner Partner { get; set; } = null!;
    public ICollection<InvoiceLine> Lines { get; } = new List<InvoiceLine>();
}

public sealed class InvoiceLine : ICompanyOwned
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public int CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal K1 { get; set; }
    public decimal K2 { get; set; }
    public decimal K3 { get; set; }
    public decimal K4 { get; set; }
    public decimal K5 { get; set; }
    public decimal Quantity { get; set; }
    public int? UnitOfMeasureId { get; set; }
    public decimal PriceEur { get; set; }
    public decimal ExchangeRateNbs { get; set; }
    public decimal PricePcs { get; set; }
    public decimal PriceTotal { get; set; }
    public decimal VatRate { get; set; }
    public decimal VatAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public int SortIndex { get; set; }
    public Invoice Invoice { get; set; } = null!;
    public Company Company { get; set; } = null!;
    public ShortList? UnitOfMeasure { get; set; }
}
