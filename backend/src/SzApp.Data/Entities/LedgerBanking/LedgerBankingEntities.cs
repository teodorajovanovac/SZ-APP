using SzApp.Data.Entities;

namespace SzApp.Data.Entities.LedgerBanking;

public sealed class ChartAccount
{
    public string Account { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ParentAccount { get; set; }
    public int Level { get; set; }
    public int Sign { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public bool IsSynthetic { get; set; }
    public ChartAccount? Parent { get; set; }
}

public sealed class SubAccount
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ParentSubAccountId { get; set; }
    public string? CostToSubAccountId { get; set; }
    public string? InterestSubAccountId { get; set; }
    public bool IsActive { get; set; } = true;
    public int? DefaultSupplierPartnerAccountId { get; set; }
    public SubAccount? Parent { get; set; }
    public SubAccount? CostToSubAccount { get; set; }
    public SubAccount? InterestSubAccount { get; set; }
}

public enum BankStatementStatus
{
    Imported = 0,
    PartiallyMatched = 1,
    Ready = 2,
    Posted = 3
}

public enum BankStatementLineStatus
{
    Pending = 0,
    Matched = 1,
    // FIN-15: never set any more -- every line must reach the ledger (9.3: legacy never ignored).
    Ignored = 2,
    Posted = 3
}

public sealed class BankStatement : ICompanyOwned
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int BankAccountId { get; set; }
    public string LedgerAccount { get; set; } = string.Empty;
    public int StatementNumber { get; set; }
    public string? StatementSuffix { get; set; }
    public DateOnly Date { get; set; }
    public decimal PreviousBalance { get; set; }
    public decimal NewBalance { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public int CountDebitEntry { get; set; }
    public int CountCreditEntry { get; set; }
    public string? Note { get; set; }
    public string? SourceFileName { get; set; }
    public int? JournalEntryId { get; set; }
    public BankStatementStatus Status { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public Company Company { get; set; } = null!;
    public JournalEntry? JournalEntry { get; set; }
    public ICollection<BankStatementLine> Lines { get; } = new List<BankStatementLine>();
}

public sealed class BankStatementLine : ICompanyOwned
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int BankStatementId { get; set; }
    public int LineNumber { get; set; }
    public string PayerRecipientName { get; set; } = string.Empty;
    public string? BankAccountNumber { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string? Info { get; set; }
    public int? Code { get; set; }
    public string? PaymentReference { get; set; }
    public string? PaymentReferenceOut { get; set; }
    public int? PartnerAccountId { get; set; }
    public string? SubAccountId { get; set; }
    public string? CounterAccount { get; set; }
    public BankStatementLineStatus Status { get; set; }
    public string? BankRef { get; set; }
    /// <summary>How the partner was found (MatchSources) and whether Ctrl+Enter may accept it.</summary>
    public string? MatchSource { get; set; }
    public bool IsConfidentMatch { get; set; }
    /// <summary>Shown to the user, e.g. the reference-vs-partner conflict (owner decision: never silent).</summary>
    public string? MatchNote { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public ICollection<BankStatementLineAllocation> Allocations { get; } = new List<BankStatementLineAllocation>();
    public BankStatement BankStatement { get; set; } = null!;
    public Company Company { get; set; } = null!;
    public SubAccount? SubAccount { get; set; }
}

/// <summary>
/// One part of a statement line: an auto-matching proposal (9.3 steps 1-6) or a manual split. The
/// line is Matched once the user accepts; posting turns every part into one ledger line.
/// Amount is in the line's direction (negative only for legacy "opposite direction" groups).
/// </summary>
public sealed class BankStatementLineAllocation : ICompanyOwned
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int BankStatementLineId { get; set; }
    public int SortIndex { get; set; }
    public string Account { get; set; } = string.Empty;
    public int? PartnerAccountId { get; set; }
    public decimal Amount { get; set; }
    public string Kind { get; set; } = string.Empty;
    public string? SubAccountId { get; set; }
    public string? Parameters { get; set; }
    public string? DocumentRef { get; set; }
    public int? InvoiceId { get; set; }
    public int? SupplierInvoiceId { get; set; }
    public int? CollectionPriority { get; set; }
    public int? ClosesDocumentType { get; set; }
    public BankStatementLine BankStatementLine { get; set; } = null!;
}

public sealed class BankInFlow : ICompanyOwned
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int BankAccountId { get; set; }
    public DateOnly DateInFlow { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Currency { get; set; } = "RSD";
    public decimal OriginalAmount { get; set; }
    public decimal AmountLocalCurrency { get; set; }
    public int? PartnerAccountId { get; set; }
    public string? InvoiceDescription { get; set; }
    public int BankStatementLineId { get; set; }
    public DateTimeOffset? SentToManagerAt { get; set; }
    public Company Company { get; set; } = null!;
    public BankStatementLine BankStatementLine { get; set; } = null!;
}

public enum BankTemplateFunction
{
    Equals = 1,
    StartsWith = 2,
    Contains = 3
}

public sealed class BankStatementPostingTemplate
{
    public int Id { get; set; }
    public int? CompanyId { get; set; }
    public int? ParentId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public string FieldValue { get; set; } = string.Empty;
    public BankTemplateFunction Function { get; set; }
    public int? SetPartnerAccountId { get; set; }
    public string? SetSubAccountId { get; set; }
    // GAP-05: a template yields a partner; the account comes from the partner account. Kept optional.
    public string? SetAccountCode { get; set; }
    public int SortIndex { get; set; }
    public bool IsActive { get; set; } = true;
    public Company? Company { get; set; }
    public BankStatementPostingTemplate? Parent { get; set; }
    public SubAccount? SetSubAccount { get; set; }
}

public sealed class PostingScheme
{
    public int Id { get; set; }
    public int? CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public string DebitAccount { get; set; } = string.Empty;
    public string CreditAccount { get; set; } = string.Empty;
    public string? DescriptionTemplate { get; set; }
    public int SortIndex { get; set; }
    public bool IsActive { get; set; } = true;
    public Company? Company { get; set; }
}

public sealed class LedgerSourcePosting : ICompanyOwned
{
    public long Id { get; set; }
    public int CompanyId { get; set; }
    public string SourceType { get; set; } = string.Empty;
    public int SourceId { get; set; }
    public int JournalEntryId { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public Company Company { get; set; } = null!;
    public JournalEntry JournalEntry { get; set; } = null!;
}
