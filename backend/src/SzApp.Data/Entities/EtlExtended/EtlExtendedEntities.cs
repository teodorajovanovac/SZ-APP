using SzApp.Data.Entities;

namespace SzApp.Data.Entities.EtlExtended;

public enum EtlPipelineStage
{
    Uploaded = 0,
    Validated = 1,
    Staged = 2,
    Importing = 3,
    Reconciling = 4,
    Completed = 5,
    Failed = 6
}

public enum LegacyParityStatus
{
    Pending = 0,
    Verified = 1,
    Failed = 2,
    BlockedByMissingSource = 3
}

public sealed class EtlRunContext : ICompanyOwned
{
    public Guid EtlRunId { get; set; }
    public int CompanyId { get; set; }
    public string SourceTable { get; set; } = string.Empty;
    public string StoredRelativePath { get; set; } = string.Empty;
    public string EncodingName { get; set; } = "windows-1250";
    public string Delimiter { get; set; } = ";";
    public EtlPipelineStage Stage { get; set; }
    public string PipelineVersion { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public EtlRun EtlRun { get; set; } = null!;
    public Company Company { get; set; } = null!;
}

public sealed class RawStagingRow
{
    public long Id { get; set; }
    public Guid EtlRunId { get; set; }
    public string SourceSystem { get; set; } = string.Empty;
    public string SourceTable { get; set; } = string.Empty;
    public int SourceRowNumber { get; set; }
    public string? SourceKey { get; set; }
    public string RowHash { get; set; } = string.Empty;
    public string RawText { get; set; } = string.Empty;
    public string ValuesJson { get; set; } = string.Empty;
    public DateTimeOffset StagedAt { get; set; }
    public EtlRun EtlRun { get; set; } = null!;
}

public sealed class PendingLegacyRelationship
{
    public long Id { get; set; }
    public Guid EtlRunId { get; set; }
    public string SourceTable { get; set; } = string.Empty;
    public string SourceKey { get; set; } = string.Empty;
    public string RelationshipName { get; set; } = string.Empty;
    public string TargetSourceTable { get; set; } = string.Empty;
    public string TargetSourceKey { get; set; } = string.Empty;
    public bool IsResolved { get; set; }
    public string? ResolutionError { get; set; }
    public EtlRun EtlRun { get; set; } = null!;
}

public enum ReconciliationMetric
{
    RowCount = 1,
    OrphanCount = 2,
    InvoiceTotal = 3,
    TrialBalance = 4,
    BankDebit = 5,
    BankCredit = 6
}

public sealed class ReconciliationRecord
{
    public long Id { get; set; }
    public Guid EtlRunId { get; set; }
    public ReconciliationMetric Metric { get; set; }
    public string Scope { get; set; } = string.Empty;
    public decimal ExpectedValue { get; set; }
    public decimal ActualValue { get; set; }
    public bool IsMatch { get; set; }
    public string? Details { get; set; }
    public DateTimeOffset CheckedAt { get; set; }
    public EtlRun EtlRun { get; set; } = null!;
}

public sealed class EtlParityAssessment
{
    public long Id { get; set; }
    public Guid EtlRunId { get; set; }
    public string ArtifactKind { get; set; } = string.Empty;
    public LegacyParityStatus Status { get; set; }
    public int ExpectedCount { get; set; }
    public int AssessedCount { get; set; }
    public string? Reason { get; set; }
    public DateTimeOffset AssessedAt { get; set; }
    public EtlRun EtlRun { get; set; } = null!;
}
