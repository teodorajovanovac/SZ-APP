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
    string? BankAccountNumber,
    string? Info,
    int? Code,
    decimal Debit,
    decimal Credit,
    string? PaymentReference,
    string Status,
    string? MatchSource,
    bool IsConfidentMatch,
    string? MatchNote,
    string? BankRef,
    string RowVersion,
    IReadOnlyCollection<BankStatementAllocationResponse> Allocations);

public sealed record BankStatementAllocationResponse(
    int Id,
    string Account,
    int? PartnerAccountId,
    string? PartnerName,
    decimal Amount,
    string Kind,
    string? SubAccountId,
    string? Parameters,
    string? DocumentRef,
    int? InvoiceId,
    int? SupplierInvoiceId,
    int? CollectionPriority,
    int? ClosesDocumentType);

public sealed record BankStatementAllocationRequest(
    string Account,
    int? PartnerAccountId,
    decimal Amount,
    string? SubAccountId,
    string? Parameters,
    string? DocumentRef,
    int? InvoiceId,
    int? SupplierInvoiceId,
    int? CollectionPriority,
    int? ClosesDocumentType);

/// <summary>Accept the stored proposals as they are (Allocations null) or replace them with a manual split.</summary>
public sealed record AcceptBankStatementLineRequest(
    IReadOnlyCollection<BankStatementAllocationRequest>? Allocations,
    string RowVersion);

/// <summary>Re-run auto-matching for one line with a chosen partner (e.g. after "/" partner search).</summary>
public sealed record AssignPartnerRequest(int PartnerAccountId, string RowVersion);

public sealed record SavePayerAccountRequest(int PartnerAccountId);

public sealed record CreateTemplateFromLineRequest(int PartnerAccountId, string? Name, string? SubAccountId);

public sealed record BankTemplateConditionResponse(string Field, string Function, string Value);

public sealed record BankTemplateResponse(
    int Id,
    string Name,
    int? PartnerAccountId,
    string? SubAccountId,
    bool IsActive,
    IReadOnlyCollection<BankTemplateConditionResponse> Conditions);

public sealed record BankStatementFormatResponse(int BankCode, string Name, bool IsSupported);

public sealed record BankStatementImportResultResponse(
    BankStatementResponse Statement,
    bool AlreadyImported,
    int BankCode,
    string FormatName,
    IReadOnlyCollection<string> Warnings);

public sealed record MatchPartnerOptionResponse(int PartnerAccountId, string Account, int AccountNumber, string PartnerName);

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
