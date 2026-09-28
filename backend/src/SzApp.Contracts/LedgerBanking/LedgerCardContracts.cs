namespace SzApp.Contracts.LedgerBanking;

/// <summary>
/// One card row. With groupBy=paymentReference/document a row is a group: JournalEntryId/LedgerEntryId
/// are null, GroupKey is set and LineCount &gt; 1 is possible.
/// </summary>
public sealed record LedgerCardRowResponse(
    int? LedgerEntryId,
    int? JournalEntryId,
    DateOnly PostingDate,
    string Account,
    int? PartnerAccountId,
    string? SubAccountId,
    string? DocumentRef,
    string? Description,
    int? LineType,
    DateOnly? DueDate,
    string? PaymentReference,
    decimal Debit,
    decimal Credit,
    decimal Balance,
    string? GroupKey,
    int LineCount);

public sealed record LedgerCardResponse(
    decimal OpeningBalance,
    decimal TotalDebit,
    decimal TotalCredit,
    decimal ClosingBalance,
    IReadOnlyCollection<LedgerCardRowResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed record PartnerBalanceResponse(
    int PartnerAccountId,
    int PartnerId,
    string PartnerName,
    int AccountNumber,
    string Account,
    decimal Debit,
    decimal Credit,
    decimal Balance,
    decimal Overdue,
    DateOnly? LastPaymentDate);

/// <summary>Ctrl+K result. Type: partner | unit | payment.</summary>
public sealed record SearchResultResponse(
    string Type,
    string Title,
    string? Subtitle,
    int CompanyId,
    int? PartnerAccountId,
    string? Account,
    int? UnitId,
    string? PaymentReference,
    decimal? Balance);
