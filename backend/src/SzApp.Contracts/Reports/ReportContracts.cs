using System.Text.Json;

namespace SzApp.Contracts.Reports;

public sealed record AnalysisDefinitionResponse(
    int Id,
    string DataGroup,
    string Name,
    bool IsCriticalGroup,
    string QueryName,
    string? ReportName,
    string? FilterCaption,
    bool AutoRunAll,
    int SortIndex,
    DateTimeOffset? LastRunAt,
    int? LastRowCount,
    bool AdminAlert,
    string? Description,
    bool HasBlockedLegacyAction);

public sealed record ReportDetailResponse(int Id, string QueryName, int SortIndex, string Function);
public sealed record ReportButtonResponse(int Id, string Caption, string Function, int FunctionTypeId, int SortIndex);
public sealed record ReportDefinitionResponse(
    int Id,
    string Name,
    string? Datasheet,
    string Title,
    int SortIndex,
    string? FilterCaption,
    IReadOnlyList<ReportDetailResponse> Details,
    IReadOnlyList<ReportButtonResponse> Buttons);

public sealed record RunReportRequest(IReadOnlyDictionary<string, JsonElement>? Parameters);

public sealed record ReportTableResponse(
    string Name,
    string Function,
    IReadOnlyList<string> Columns,
    IReadOnlyList<IReadOnlyList<object?>> Rows,
    int RowCount,
    bool IsTruncated);

public sealed record AnalysisRunResponse(
    int DefinitionId,
    string Name,
    ReportTableResponse Result,
    int DurationMs);

public sealed record ReportRunResponse(
    int DefinitionId,
    string Title,
    IReadOnlyList<ReportTableResponse> Sections,
    int DurationMs);
