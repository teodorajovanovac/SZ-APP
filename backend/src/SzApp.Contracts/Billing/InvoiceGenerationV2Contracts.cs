namespace SzApp.Contracts.Billing;

/// <summary>
/// Item 8 wizard (flow A, 5.9): server-computed R0..R3 invoice generation, as opposed to the
/// legacy client-seeded CreateInvoiceBatchRequest/InvoiceGenerationRequest pair.
/// Scope (9.1 "više SZ odjednom"): "single" (default, route company), "location" (LocationCategoryId
/// and its descendants) or "all" -- always intersected with the companies the caller may write to.
/// InterestPeriodStart/End: when set, the interest run (9.4) is executed for every newly generated
/// batch and its PrenesiZK totals land on the invoices (R7/R8).
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
    decimal ExchangeRateNbs,
    string? Scope = null,
    int? LocationCategoryId = null,
    DateOnly? InterestPeriodStart = null,
    DateOnly? InterestPeriodEnd = null);

public sealed record CustomerInvoicePreviewV2(int CustomerId, string CustomerName, decimal Net, decimal Vat, decimal Interest, decimal Total);

/// <summary>
/// Per-building (company) preview. PreviousTotal = Σ Total of the latest earlier regular batch,
/// ChangePercent = ±% of this preview's Total against it (null when there is nothing to compare).
/// Error is set (and amounts are zero) when this building can't be invoiced in a multi-company scope.
/// </summary>
public sealed record InvoiceBatchPreviewV2Response(
    int CompanyId,
    int PeriodYYMM,
    int CustomerCount,
    decimal NetTotal,
    decimal VatTotal,
    decimal InterestTotal,
    decimal Total,
    IReadOnlyList<CustomerInvoicePreviewV2> Customers,
    string CompanyName = "",
    decimal? PreviousTotal = null,
    decimal? ChangePercent = null,
    string? Error = null);

public sealed record InvoiceScopePreviewResponse(
    IReadOnlyList<InvoiceBatchPreviewV2Response> Buildings,
    int CustomerCount,
    decimal NetTotal,
    decimal VatTotal,
    decimal InterestTotal,
    decimal Total);

public sealed record GenerateInvoicesV2Response(int InvoiceBatchId, bool AlreadyGenerated, IReadOnlyList<int> InvoiceIds);

public sealed record CompanyGenerateResultV2(
    int CompanyId,
    string CompanyName,
    int? InvoiceBatchId,
    bool AlreadyGenerated,
    int InvoiceCount,
    decimal? InterestTotal,
    string? Error);

public sealed record GenerateInvoicesScopeResponse(IReadOnlyList<CompanyGenerateResultV2> Companies);
