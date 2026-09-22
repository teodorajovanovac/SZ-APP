using SzApp.Data.Entities;

namespace SzApp.Data.Entities.Billing;

public enum BillingBatchStatus
{
    Draft = 0,
    Generated = 1,
    Posted = 2
}

public enum NoticeDeliveryStatus
{
    Draft = 0,
    Rendered = 1,
    Queued = 2,
    Sent = 3,
    Failed = 4
}

public sealed class CalculationType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SupplierAmountRule { get; set; } = string.Empty;
    public string AllocationRule { get; set; } = string.Empty;
    public string QuantityRule { get; set; } = string.Empty;
    public int UnitOfMeasureId { get; set; }
    public string? Note { get; set; }
}

public sealed class SupplierInvoice : ICompanyOwned
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int InvoiceNo { get; set; }
    public string CodeName { get; set; } = string.Empty;
    public string Caption { get; set; } = string.Empty;
    public int SupplierPartnerAccountId { get; set; }
    public int CalculationTypeId { get; set; }
    public int PeriodYYMM { get; set; }
    public decimal InvoiceTotalCalculationAmountEur { get; set; }
    public decimal InvoiceTotalCalculationAmountRsd { get; set; }
    public decimal CalculationAmountByCoefficientEur { get; set; }
    public decimal CalculationAmountByCoefficientRsd { get; set; }
    public int PaymentPriority { get; set; }
    public string? SubAccountId { get; set; }
    public int DocumentTypeId { get; set; }
    public string? ExtraordinaryInvoiceMarker { get; set; }
    public string? InvoiceNameFunction { get; set; }
    public decimal PostedInvoiceAmount { get; set; }
    public DateOnly InvoiceDate { get; set; }
    public DateOnly TransactionDate { get; set; }
    public DateOnly? PaymentDate { get; set; }
    public string? InvoiceDescription { get; set; }
    public string? PaymentReference { get; set; }
    public int? PreviousSupplierInvoiceId { get; set; }
    public int? NewSupplierInvoiceId { get; set; }
    public int? JournalEntryId { get; set; }
    public string? ClosesAccount { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public ICollection<SupplierInvoiceUnitType> UnitTypes { get; } = new List<SupplierInvoiceUnitType>();
}

public sealed class SupplierInvoiceUnitType
{
    public int Id { get; set; }
    public int SupplierInvoiceId { get; set; }
    public int UnitTypeId { get; set; }
    public SupplierInvoice SupplierInvoice { get; set; } = null!;
}

public sealed class InvoiceBatch : ICompanyOwned
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int PeriodYYMM { get; set; }
    public string Caption { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
    public string Place { get; set; } = string.Empty;
    public DateOnly IssueDate { get; set; }
    public DateOnly ServiceDateFrom { get; set; }
    public DateOnly ServiceDateTo { get; set; }
    public DateOnly TransactionDate { get; set; }
    public DateOnly DueDate { get; set; }
    public decimal ExchangeRateNbs { get; set; }
    public int? JournalEntryId { get; set; }
    public DateTimeOffset EntryDate { get; set; }
    public int StaffId { get; set; }
    public string? ExtraordinaryInvoiceMarker { get; set; }
    public DateOnly? BalanceAsOfDate { get; set; }
    public DateOnly? PreviousValueDate { get; set; }
    public bool IsInterestCalculated { get; set; }
    public string? PaymentPurpose { get; set; }
    public BillingBatchStatus Status { get; set; }
    public string? GenerationFingerprint { get; set; }
    public DateTimeOffset? GeneratedAt { get; set; }
    public DateTimeOffset? PostedAt { get; set; }
    public byte[] RowVersion { get; set; } = [];
}

public sealed class InvoiceUnit : ICompanyOwned
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int InvoiceId { get; set; }
    public int ContractId { get; set; }
}

public sealed class Benefit : ICompanyOwned
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int ContractId { get; set; }
    public int PeriodYYMM { get; set; }
    public DateTimeOffset EntryDate { get; set; }
    public decimal Amount { get; set; }
    public int? InvoiceId { get; set; }
    public byte[] RowVersion { get; set; } = [];
}

public sealed class InterestRate
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public decimal Rate { get; set; }
    public string TimeCode { get; set; } = "G";
}

public sealed class InterestStatement : ICompanyOwned
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string Account { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public decimal Amount { get; set; }
    public decimal Balance { get; set; }
    public int Days { get; set; }
    public decimal Rate { get; set; }
    public decimal Coefficient { get; set; }
    public decimal Interest { get; set; }
    public int PartnerAccountId { get; set; }
    public string? SubAccountId { get; set; }
    public int InvoiceBatchId { get; set; }
}

public sealed class NoticeTemplate : ICompanyOwned
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public byte[] RowVersion { get; set; } = [];
}

public sealed class NoticeBatch : ICompanyOwned
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public int MinUnpaidInvoiceCount { get; set; }
    public decimal DebtTolerance { get; set; }
    public decimal DebtToleranceByMonth { get; set; }
    public int NoticeTemplateId { get; set; }
    public int NoticeTypeId { get; set; }
    public DateOnly UpToClaimDate { get; set; }
    public DateOnly UpToPaymentDate { get; set; }
    public int? InvoiceBatchId { get; set; }
    public string? CustomCaptionOnSlip { get; set; }
    public string? GenerationFingerprint { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public ICollection<Notice> Notices { get; } = new List<Notice>();
}

public sealed class Notice : ICompanyOwned
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int NoticeBatchId { get; set; }
    public int PartnerAccountId { get; set; }
    public int UnpaidInvoiceCount { get; set; }
    public decimal Debt { get; set; }
    public string? InvoiceText { get; set; }
    public bool IsActive { get; set; } = true;
    public string PaymentReference { get; set; } = string.Empty;
    public decimal AdditionalCosts { get; set; }
    public decimal Total { get; set; }
    public NoticeDeliveryStatus DeliveryStatus { get; set; }
    public string? RenderedDocumentPath { get; set; }
    public DateTimeOffset? SentAt { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public NoticeBatch NoticeBatch { get; set; } = null!;
    public ICollection<NoticeLine> Lines { get; } = new List<NoticeLine>();
}

public sealed class NoticeLine
{
    public int Id { get; set; }
    public int NoticeId { get; set; }
    public string? Parameters { get; set; }
    public string DocumentRef { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal Sum { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateOnly DueDate { get; set; }
    public int? InvoiceId { get; set; }
    public DateOnly? InvoiceDate { get; set; }
    public string? UnitAddress { get; set; }
    public Notice Notice { get; set; } = null!;
}

public sealed class PaymentOrder : ICompanyOwned
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string TemplateTitle { get; set; } = string.Empty;
    public string PayerName { get; set; } = string.Empty;
    public string PaymentPurpose { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public int PaymentCode { get; set; }
    public string Currency { get; set; } = "RSD";
    public decimal Amount { get; set; }
    public string PayerAccountNumber { get; set; } = string.Empty;
    public int? PayerModelNumber { get; set; }
    public string? PayerPaymentReference { get; set; }
    public string RecipientAccountNumber { get; set; } = string.Empty;
    public int? RecipientModelNumber { get; set; }
    public string? RecipientPaymentReference { get; set; }
    public string Place { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public DateOnly ValueDate { get; set; }
    public bool IsUrgent { get; set; }
    public int PaymentOrderTypeId { get; set; }
    public DateTimeOffset CreatedTimestamp { get; set; }
    public bool IsFavorite { get; set; }
    public bool IsArchived { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
