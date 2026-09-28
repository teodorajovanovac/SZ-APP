namespace SzApp.Domain.Billing;

public sealed record BillingLineInput(
    string Name,
    decimal Quantity,
    decimal UnitPrice,
    decimal VatRate,
    decimal K1 = 1m,
    decimal K2 = 1m,
    decimal K3 = 1m,
    decimal K4 = 1m,
    decimal K5 = 1m);

public sealed record BillingLineAmounts(
    decimal Quantity,
    decimal UnitPrice,
    decimal NetAmount,
    decimal VatAmount,
    decimal TotalAmount);

public sealed record BillingInvoiceAmounts(
    decimal NetAmount,
    decimal BenefitAmount,
    decimal TaxableAmount,
    decimal VatAmount,
    decimal InterestAmount,
    decimal TotalAmount);

public sealed record InterestPeriod(
    DateOnly From,
    DateOnly To,
    decimal AnnualRate);

public sealed record InterestPeriodResult(
    DateOnly From,
    DateOnly To,
    int Days,
    decimal AnnualRate,
    decimal Coefficient,
    decimal Interest);

/// <summary>
/// A fully built, balanced document posting (lines from <see cref="LedgerBanking.DocumentPostingRules"/>).
/// Posted as one journal, recorded once per (SourceType, SourceId).
/// </summary>
public sealed record LedgerPostingRequest(
    int CompanyId,
    string SourceType,
    int SourceId,
    DateOnly PostingDate,
    string Description,
    string Currency,
    string IdempotencyKey,
    IReadOnlyList<LedgerBanking.PostingLine> Lines);

public sealed record LedgerPostingResult(int JournalEntryId, bool AlreadyPosted);

public interface ILedgerPostingGateway
{
    Task<LedgerPostingResult> PostAsync(LedgerPostingRequest request, CancellationToken cancellationToken);
}

public interface INoticeWorkflowGateway
{
    Task<string> RenderAsync(int companyId, int noticeId, CancellationToken cancellationToken);
    Task SendAsync(int companyId, int noticeId, string renderedDocumentPath, CancellationToken cancellationToken);
}
