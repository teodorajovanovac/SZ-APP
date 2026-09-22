namespace SzApp.Contracts.LedgerBanking;

public sealed record PageResponse<T>(IReadOnlyCollection<T> Items, int Page, int PageSize, int TotalCount);

public sealed record LedgerLineRequest(
    string Account,
    decimal DebitAmount,
    decimal CreditAmount,
    DateOnly? DueDate,
    string? DocumentRef,
    string? SubAccountId,
    int? PartnerAccountId,
    string? Note);

public sealed record CreateJournalEntryRequest(
    DateOnly PostingDate,
    DateOnly? DueDate,
    string Description,
    string Currency,
    int? JournalEntryTypeId,
    IReadOnlyCollection<LedgerLineRequest> Lines);

public sealed record JournalEntrySummaryResponse(
    int Id,
    DateOnly PostingDate,
    string Description,
    string Currency,
    decimal Balance,
    bool IsPosted,
    DateTimeOffset? PostedAt,
    int? ReversalOfId,
    string RowVersion);

public sealed record JournalEntryResponse(
    JournalEntrySummaryResponse Header,
    IReadOnlyCollection<LedgerEntryResponse> Lines);

public sealed record LedgerEntryResponse(
    int Id,
    string Account,
    DateOnly PostingDate,
    DateOnly? DueDate,
    decimal DebitAmount,
    decimal CreditAmount,
    string? DocumentRef,
    string? SubAccountId,
    int? PartnerAccountId,
    string? Note);

public sealed record ConcurrencyCommandRequest(string RowVersion);

public sealed record PostingResultResponse(int JournalEntryId, bool AlreadyPosted, string RowVersion);

public sealed record BankStatementLineImportRequest(
    int LineNumber,
    string PayerRecipientName,
    string? BankAccountNumber,
    decimal Debit,
    decimal Credit,
    string? Info,
    int? Code,
    string? PaymentReference,
    string? PaymentReferenceOut,
    string? BankRef);

public sealed record BankStatementImportRequest(
    int BankAccountId,
    string LedgerAccount,
    int StatementNumber,
    string? StatementSuffix,
    DateOnly Date,
    decimal PreviousBalance,
    decimal NewBalance,
    decimal Debit,
    decimal Credit,
    IReadOnlyCollection<BankStatementLineImportRequest> Lines);

public sealed record BankStatementSummaryResponse(
    int Id,
    int BankAccountId,
    int StatementNumber,
    string? StatementSuffix,
    DateOnly Date,
    decimal PreviousBalance,
    decimal NewBalance,
    decimal Debit,
    decimal Credit,
    int LineCount,
    string Status,
    int? JournalEntryId,
    string RowVersion);

public sealed record BankStatementLineResponse(
    int Id,
    int LineNumber,
    string PayerRecipientName,
    decimal Debit,
    decimal Credit,
    string? PaymentReference,
    string Status,
    int? PartnerAccountId,
    string? SubAccountId,
    string? CounterAccount,
    string? BankRef,
    string RowVersion);

public sealed record MatchBankStatementLineRequest(
    int? PartnerAccountId,
    string? SubAccountId,
    string CounterAccount,
    string RowVersion);

public sealed record BankStatementResponse(
    BankStatementSummaryResponse Header,
    IReadOnlyCollection<BankStatementLineResponse> Lines);

public sealed record ChartAccountResponse(
    string Account,
    string? ShortName,
    string Name,
    string? ParentAccount,
    int Sign,
    bool IsActive,
    bool IsSynthetic);

public sealed record SaveChartAccountRequest(
    string? ShortName,
    string Name,
    string? ParentAccount,
    int Sign,
    bool IsActive,
    bool IsSynthetic);

public sealed record SubAccountResponse(string Id, string Name, string? ParentSubAccountId, bool IsActive);

public sealed record SaveSubAccountRequest(string Name, string? ParentSubAccountId, bool IsActive);
