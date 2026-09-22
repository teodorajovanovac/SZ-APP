namespace SzApp.Data.Entities;

public sealed class AuditLog
{
    public long Id { get; set; }
    public int? CompanyId { get; set; }
    public int? StaffId { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? ItemId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EventSource { get; set; } = string.Empty;
    public string? DetailsJson { get; set; }
    public Company? Company { get; set; }
    public ApplicationUser? Staff { get; set; }
}

public sealed class OutboxMessage
{
    public Guid Id { get; set; }
    public int? CompanyId { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public string Type { get; set; } = string.Empty;
    public string DedupeKey { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public DateTimeOffset? ProcessedAt { get; set; }
    public int AttemptCount { get; set; }
    public string? LastError { get; set; }
    public Company? Company { get; set; }
}

public sealed class IdempotencyRequest
{
    public long Id { get; set; }
    public int StaffId { get; set; }
    public int? CompanyId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string RequestHash { get; set; } = string.Empty;
    public int? ResponseStatusCode { get; set; }
    public string? ResponseJson { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public ApplicationUser Staff { get; set; } = null!;
    public Company? Company { get; set; }
}

public enum EtlRunStatus
{
    Pending = 0,
    Running = 1,
    Completed = 2,
    Failed = 3
}

public sealed class EtlRun
{
    public Guid Id { get; set; }
    public int CompanyId { get; set; }
    public string SourceSystem { get; set; } = string.Empty;
    public string SourceFile { get; set; } = string.Empty;
    public string ContentHash { get; set; } = string.Empty;
    public EtlRunStatus Status { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public int SourceRowCount { get; set; }
    public int ImportedRowCount { get; set; }
    public int QuarantinedRowCount { get; set; }
    public string? Error { get; set; }
    public Company Company { get; set; } = null!;
}

public sealed class LegacyKeyMap
{
    public long Id { get; set; }
    public Guid EtlRunId { get; set; }
    public string SourceTable { get; set; } = string.Empty;
    public string SourceKey { get; set; } = string.Empty;
    public string TargetTable { get; set; } = string.Empty;
    public string TargetKey { get; set; } = string.Empty;
    public EtlRun EtlRun { get; set; } = null!;
}

public sealed class QuarantineRecord
{
    public long Id { get; set; }
    public Guid EtlRunId { get; set; }
    public string SourceTable { get; set; } = string.Empty;
    public string? SourceKey { get; set; }
    public string RawJson { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public EtlRun EtlRun { get; set; } = null!;
}
