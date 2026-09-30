namespace SzApp.Contracts.LedgerBanking;

/// <summary>GAP-33. Scope: 0 all, 1 regular (no marker), 2 extraordinary (legacy IndexZaRedovneVanderdneRacune).</summary>
public sealed record CopySupplierInvoicesRequest(int FromPeriodYYMM, int ToPeriodYYMM, int Scope);

public sealed record CopySupplierInvoicesResponse(int Copied, int Skipped, IReadOnlyCollection<int> NewSupplierInvoiceIds);

/// <summary>GAP-34. Either explicit supplier invoice ids or every invoice of a period.</summary>
public sealed record GeneratePaymentOrdersRequest(IReadOnlyCollection<int>? SupplierInvoiceIds, int? PeriodYYMM, DateOnly? Date);

public sealed record PaymentOrderSkipResponse(int SupplierInvoiceId, string Caption, string Reason);

public sealed record GeneratePaymentOrdersResponse(int Created, IReadOnlyCollection<PaymentOrderSkipResponse> Skipped);

public sealed record SupplierPaymentOrderResponse(
    int Id,
    int? SupplierInvoiceId,
    string PayerName,
    string PayerAccountNumber,
    string RecipientName,
    string RecipientAccountNumber,
    int PaymentCode,
    string Currency,
    decimal Amount,
    int? RecipientModelNumber,
    string? RecipientPaymentReference,
    string PaymentPurpose,
    string Place,
    DateOnly Date,
    DateOnly ValueDate,
    bool IsArchived,
    string RowVersion);

/// <summary>FIN-33 preview row: what "Preknjiži avanse" would move for one partner.</summary>
public sealed record AdvanceReclassificationPreviewResponse(
    int PartnerAccountId,
    int AccountNumber,
    string PartnerName,
    decimal Advance,
    decimal OpenAmount,
    decimal Amount,
    int ReferenceCount);

public sealed record AdvanceReclassificationRequest(DateOnly? PostingDate, IReadOnlyCollection<int>? PartnerAccountIds);

public sealed record AdvanceReclassificationResponse(int JournalCount, decimal TotalAmount, IReadOnlyCollection<int> JournalEntryIds);

/// <summary>GAP-19: balance per ledger account on a date (posted lines only).</summary>
public sealed record AccountBalanceResponse(string Account, string? Name, decimal Debit, decimal Credit, decimal Balance);

public sealed record PartnerAccountLookupResponse(int Id, int AccountNumber, string Account, string PartnerName);

/// <summary>GAP-13: re-save of a draft journal (optimistic concurrency via RowVersion).</summary>
public sealed record UpdateJournalDraftRequest(string RowVersion, CreateJournalEntryRequest Journal);
