namespace SzApp.Data.Entities.Reports;

public sealed class AnalysisReportDefinition
{
    public int Id { get; set; }
    public int? CompanyId { get; set; }
    public string DataGroup { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsCriticalGroup { get; set; }
    public string QueryName { get; set; } = string.Empty;
    public string QuerySql { get; set; } = string.Empty;
    public string? ReportName { get; set; }
    public string? FilterCaption { get; set; }
    public string? FilterQuerySql { get; set; }
    public string? LegacyActionQueryName { get; set; }
    public string? LegacyActionQuerySql { get; set; }
    public bool AutoRunAll { get; set; }
    public int SortIndex { get; set; }
    public string? LinkCreationTag { get; set; }
    public string? LinkFormName { get; set; }
    public string? LinkOpenArgs { get; set; }
    public DateTimeOffset? LastRunAt { get; set; }
    public int? LastRowCount { get; set; }
    public bool AdminAlert { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public byte[] RowVersion { get; set; } = [];
}

public sealed class ReportDefinition
{
    public int Id { get; set; }
    public int? CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Datasheet { get; set; }
    public string Title { get; set; } = string.Empty;
    public int SortIndex { get; set; }
    public string? FilterQuerySql { get; set; }
    public string? FilterCaption { get; set; }
    public bool IsActive { get; set; } = true;
    public byte[] RowVersion { get; set; } = [];
    public ICollection<ReportDefinitionDetail> Details { get; } = new List<ReportDefinitionDetail>();
    public ICollection<ReportDefinitionButton> Buttons { get; } = new List<ReportDefinitionButton>();
}

public sealed class ReportDefinitionDetail
{
    public int Id { get; set; }
    public int ReportDefinitionId { get; set; }
    public string QuerySql { get; set; } = string.Empty;
    public string QueryName { get; set; } = string.Empty;
    public int SortIndex { get; set; }
    public string Function { get; set; } = "Table";
    public ReportDefinition ReportDefinition { get; set; } = null!;
}

public sealed class ReportDefinitionButton
{
    public int Id { get; set; }
    public int ReportDefinitionId { get; set; }
    public string Caption { get; set; } = string.Empty;
    public string Function { get; set; } = string.Empty;
    public int FunctionTypeId { get; set; }
    public int SortIndex { get; set; }
    public ReportDefinition ReportDefinition { get; set; } = null!;
}

public enum ReportExecutionStatus
{
    Succeeded = 1,
    Rejected = 2,
    Failed = 3
}

public sealed class ReportExecutionAudit
{
    public long Id { get; set; }
    public int CompanyId { get; set; }
    public int StaffId { get; set; }
    public string DefinitionType { get; set; } = string.Empty;
    public int DefinitionId { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public int DurationMs { get; set; }
    public ReportExecutionStatus Status { get; set; }
    public int? RowCount { get; set; }
    public bool WasTruncated { get; set; }
    public string ParameterHash { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
}
