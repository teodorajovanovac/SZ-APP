namespace SzApp.Api.Features.Reports;

public sealed class ReportsOptions
{
    public const string SectionName = "Reports";
    public string ReadOnlyConnectionString { get; set; } = string.Empty;
    public int CommandTimeoutSeconds { get; set; } = 30;
    public int MaximumRows { get; set; } = 5_000;
    public int MaximumColumns { get; set; } = 100;
    public int MaximumCellCharacters { get; set; } = 10_000;
}
