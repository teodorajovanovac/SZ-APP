namespace SzApp.Contracts.Billing;

/// <summary>
/// Item 8 wizard (flow A, 5.9): server-computed R0..R3 invoice generation, as opposed to the
/// legacy client-seeded CreateInvoiceBatchRequest/InvoiceGenerationRequest pair.
/// </summary>
public sealed record GenerateInvoicesV2Request(
    int PeriodYYMM,
    string? ExtraordinaryMarker,
    string Place,
    DateOnly IssueDate,
    DateOnly DueDate,
    DateOnly ServiceDateFrom,
    DateOnly ServiceDateTo,
    DateOnly TransactionDate,
    decimal ExchangeRateNbs);

public sealed record CustomerInvoicePreviewV2(int CustomerId, string CustomerName, decimal Net, decimal Vat, decimal Interest, decimal Total);

/// <summary>Per-building (company) preview. "Skipped" percent-vs-last-month per item 8 (not cheap to compute here).</summary>
public sealed record InvoiceBatchPreviewV2Response(
    int CompanyId,
    int PeriodYYMM,
    int CustomerCount,
    decimal NetTotal,
    decimal VatTotal,
    decimal InterestTotal,
    decimal Total,
    IReadOnlyList<CustomerInvoicePreviewV2> Customers);

public sealed record GenerateInvoicesV2Response(int InvoiceBatchId, bool AlreadyGenerated, IReadOnlyList<int> InvoiceIds);
