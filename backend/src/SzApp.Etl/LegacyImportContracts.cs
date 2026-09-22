namespace SzApp.Etl;

public sealed record LegacyImportRequest(string SourceSystem, string SourceFile, string ContentHash);

public sealed record ReconciliationResult(
    Guid RunId,
    int SourceRows,
    int ImportedRows,
    int QuarantinedRows,
    decimal SourceTotal,
    decimal ImportedTotal)
{
    public bool IsBalanced => SourceRows == ImportedRows + QuarantinedRows && SourceTotal == ImportedTotal;
}
