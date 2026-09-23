namespace SzApp.Contracts.Etl;

public sealed record EtlUploadResponse(
    Guid RunId,
    bool AlreadyExists,
    string ContentHash,
    string Stage);

public sealed record EtlValidateRequest(string EncodingName = "windows-1250", string Delimiter = ";");

public sealed record EtlRunCommandResponse(
    Guid RunId,
    string Stage,
    int SourceRows,
    int ImportedRows,
    int QuarantinedRows);

public sealed record ReconciliationItemResponse(
    string Metric,
    string Scope,
    decimal ExpectedValue,
    decimal ActualValue,
    bool? IsMatch, // null = not applicable (target table has no materialization yet)
    string? Details);

public sealed record ParityAssessmentResponse(
    string ArtifactKind,
    string Status,
    int ExpectedCount,
    int AssessedCount,
    string? Reason);

public sealed record EtlRunStatusResponse(
    Guid RunId,
    int CompanyId,
    string SourceSystem,
    string SourceTable,
    string SourceFile,
    string ContentHash,
    string Stage,
    string Status,
    int SourceRows,
    int ImportedRows,
    int QuarantinedRows,
    string? Error,
    IReadOnlyCollection<ReconciliationItemResponse> Reconciliation,
    IReadOnlyCollection<ParityAssessmentResponse> Parity);

public sealed record CsvPreviewResponse(
    IReadOnlyCollection<string> Headers,
    IReadOnlyCollection<IReadOnlyDictionary<string, string>> Rows,
    int TotalRows,
    IReadOnlyCollection<string> Errors);
