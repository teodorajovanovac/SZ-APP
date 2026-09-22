using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SzApp.Contracts.Reports;
using SzApp.Data;
using SzApp.Data.Entities.Reports;
using SzApp.Domain;
using SzApp.Domain.Reports;

namespace SzApp.Api.Features.Reports;

public sealed class ReportService(
    SzAppDbContext db,
    ISafeReportQueryRunner runner,
    IReportFunctionRegistry functions,
    TimeProvider timeProvider)
{
    public async Task<IReadOnlyList<AnalysisDefinitionResponse>> ListAnalysesAsync(int companyId, CancellationToken ct) =>
        await db.Set<AnalysisReportDefinition>().AsNoTracking()
            .Where(x => x.IsActive && (x.CompanyId == null || x.CompanyId == companyId))
            .OrderBy(x => x.DataGroup).ThenBy(x => x.SortIndex).ThenBy(x => x.Name)
            .Select(x => new AnalysisDefinitionResponse(x.Id, x.DataGroup, x.Name, x.IsCriticalGroup, x.QueryName,
                x.ReportName, x.FilterCaption, x.AutoRunAll, x.SortIndex, x.LastRunAt, x.LastRowCount,
                x.AdminAlert, x.Description, x.LegacyActionQuerySql != null || x.LegacyActionQueryName != null))
            .ToArrayAsync(ct);

    public async Task<IReadOnlyList<ReportDefinitionResponse>> ListReportsAsync(int companyId, CancellationToken ct)
    {
        var definitions = await db.Set<ReportDefinition>().AsNoTracking()
            .Where(x => x.IsActive && (x.CompanyId == null || x.CompanyId == companyId))
            .Include(x => x.Details).Include(x => x.Buttons)
            .OrderBy(x => x.SortIndex).ThenBy(x => x.Name).ToArrayAsync(ct);
        return definitions.Select(ToResponse).ToArray();
    }

    public async Task<AnalysisRunResponse> RunAnalysisAsync(
        int companyId,
        int staffId,
        int definitionId,
        RunReportRequest request,
        string correlationId,
        CancellationToken ct)
    {
        var definition = await db.Set<AnalysisReportDefinition>()
            .SingleOrDefaultAsync(x => x.Id == definitionId && x.IsActive && (x.CompanyId == null || x.CompanyId == companyId), ct)
            ?? throw new DomainRuleException("reports.analysis-not-found", "Definicija analize ne postoji.");
        var started = timeProvider.GetUtcNow();
        var timer = Stopwatch.StartNew();
        try
        {
            // LegacyActionQuerySql is intentionally never passed to the runner.
            var result = await runner.RunAsync(definition.QuerySql, companyId, request.Parameters, ct);
            timer.Stop();
            definition.LastRunAt = timeProvider.GetUtcNow();
            definition.LastRowCount = result.Rows.Count;
            db.Add(CreateAudit(companyId, staffId, "Analysis", definitionId, started, timer.Elapsed, request.Parameters,
                ReportExecutionStatus.Succeeded, result.Rows.Count, result.IsTruncated, correlationId, null));
            await db.SaveChangesAsync(ct);
            return new(definition.Id, definition.Name, ToTable(definition.QueryName, "Table", result), Duration(timer.Elapsed));
        }
        catch (Exception exception)
        {
            timer.Stop();
            db.ChangeTracker.Clear();
            db.Add(CreateAudit(companyId, staffId, "Analysis", definitionId, started, timer.Elapsed, request.Parameters,
                exception is DomainRuleException ? ReportExecutionStatus.Rejected : ReportExecutionStatus.Failed,
                null, false, correlationId, (exception as DomainRuleException)?.Code ?? "reports.execution-failed"));
            await db.SaveChangesAsync(CancellationToken.None);
            throw;
        }
    }

    public async Task<ReportRunResponse> RunReportAsync(
        int companyId,
        int staffId,
        int definitionId,
        RunReportRequest request,
        string correlationId,
        CancellationToken ct)
    {
        var definition = await db.Set<ReportDefinition>().AsNoTracking()
            .Include(x => x.Details).Include(x => x.Buttons)
            .SingleOrDefaultAsync(x => x.Id == definitionId && x.IsActive && (x.CompanyId == null || x.CompanyId == companyId), ct)
            ?? throw new DomainRuleException("reports.definition-not-found", "Definicija izveštaja ne postoji.");
        ValidateRegistry(definition);
        var started = timeProvider.GetUtcNow();
        var timer = Stopwatch.StartNew();
        try
        {
            var sections = new List<ReportTableResponse>();
            foreach (var detail in definition.Details.OrderBy(x => x.SortIndex))
            {
                var data = await runner.RunAsync(detail.QuerySql, companyId, request.Parameters, ct);
                sections.Add(ToTable(detail.QueryName, detail.Function, data));
            }
            if (sections.Count == 0) throw new DomainRuleException("reports.no-details", "Izveštaj nema nijednu aktivnu sekciju.");
            timer.Stop();
            var rowCount = sections.Sum(x => x.RowCount);
            db.Add(CreateAudit(companyId, staffId, "Report", definitionId, started, timer.Elapsed, request.Parameters,
                ReportExecutionStatus.Succeeded, rowCount, sections.Any(x => x.IsTruncated), correlationId, null));
            await db.SaveChangesAsync(ct);
            return new(definition.Id, definition.Title, sections, Duration(timer.Elapsed));
        }
        catch (Exception exception)
        {
            timer.Stop();
            db.ChangeTracker.Clear();
            db.Add(CreateAudit(companyId, staffId, "Report", definitionId, started, timer.Elapsed, request.Parameters,
                exception is DomainRuleException ? ReportExecutionStatus.Rejected : ReportExecutionStatus.Failed,
                null, false, correlationId, (exception as DomainRuleException)?.Code ?? "reports.execution-failed"));
            await db.SaveChangesAsync(CancellationToken.None);
            throw;
        }
    }

    public async Task<byte[]> ExportAnalysisCsvAsync(int companyId, int staffId, int definitionId, RunReportRequest request, string correlationId, CancellationToken ct)
    {
        var run = await RunAnalysisAsync(companyId, staffId, definitionId, request, correlationId, ct);
        return CsvReportExporter.Export(ToDomain(run.Result));
    }

    public async Task<byte[]> ExportReportCsvAsync(int companyId, int staffId, int definitionId, RunReportRequest request, string correlationId, CancellationToken ct)
    {
        var run = await RunReportAsync(companyId, staffId, definitionId, request, correlationId, ct);
        return CsvReportExporter.Export(ToDomain(run.Sections[0]));
    }

    public async Task<string> RenderReportHtmlAsync(int companyId, int staffId, int definitionId, RunReportRequest request, string correlationId, CancellationToken ct)
    {
        var run = await RunReportAsync(companyId, staffId, definitionId, request, correlationId, ct);
        if (run.Sections.Count == 1)
            return functions.RenderHtml(run.Sections[0].Function, run.Title, ToDomain(run.Sections[0]));

        var body = new StringBuilder("<!doctype html><html lang=\"sr-Latn\"><head><meta charset=\"utf-8\"><title>")
            .Append(System.Net.WebUtility.HtmlEncode(run.Title)).Append("</title></head><body><h1>")
            .Append(System.Net.WebUtility.HtmlEncode(run.Title)).Append("</h1>");
        foreach (var section in run.Sections)
        {
            var rendered = functions.RenderHtml(section.Function, section.Name, ToDomain(section));
            var start = rendered.IndexOf("<body>", StringComparison.Ordinal) + 6;
            var end = rendered.LastIndexOf("</body>", StringComparison.Ordinal);
            body.Append(rendered.AsSpan(start, end - start));
        }
        return body.Append("</body></html>").ToString();
    }

    private void ValidateRegistry(ReportDefinition definition)
    {
        var detail = definition.Details.FirstOrDefault(x => !functions.IsAllowed(x.Function));
        if (detail is not null) throw new DomainRuleException("reports.function-not-allowed", $"Funkcija '{detail.Function}' nije dozvoljena.");
        var button = definition.Buttons.FirstOrDefault(x => !functions.IsAllowedAction(x.Function));
        if (button is not null) throw new DomainRuleException("reports.action-not-allowed", $"Akcija '{button.Function}' nije dozvoljena.");
    }

    private static ReportDefinitionResponse ToResponse(ReportDefinition x) => new(
        x.Id, x.Name, x.Datasheet, x.Title, x.SortIndex, x.FilterCaption,
        x.Details.OrderBy(y => y.SortIndex).Select(y => new ReportDetailResponse(y.Id, y.QueryName, y.SortIndex, y.Function)).ToArray(),
        x.Buttons.OrderBy(y => y.SortIndex).Select(y => new ReportButtonResponse(y.Id, y.Caption, y.Function, y.FunctionTypeId, y.SortIndex)).ToArray());

    private static ReportTableResponse ToTable(string name, string function, TabularReportData data) =>
        new(name, function, data.Columns, data.Rows, data.Rows.Count, data.IsTruncated);
    private static TabularReportData ToDomain(ReportTableResponse table) => new(table.Columns, table.Rows, table.IsTruncated);

    private static ReportExecutionAudit CreateAudit(
        int companyId, int staffId, string type, int id, DateTimeOffset startedAt, TimeSpan duration,
        IReadOnlyDictionary<string, JsonElement>? parameters, ReportExecutionStatus status, int? rowCount,
        bool truncated, string correlationId, string? errorCode) => new()
        {
            CompanyId = companyId,
            StaffId = staffId,
            DefinitionType = type,
            DefinitionId = id,
            StartedAt = startedAt,
            DurationMs = Duration(duration),
            Status = status,
            RowCount = rowCount,
            WasTruncated = truncated,
            ParameterHash = HashParameters(companyId, parameters),
            CorrelationId = correlationId[..Math.Min(correlationId.Length, 64)],
            ErrorCode = errorCode
        };

    private static string HashParameters(int companyId, IReadOnlyDictionary<string, JsonElement>? parameters)
    {
        var normalized = new SortedDictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in parameters ?? new Dictionary<string, JsonElement>()) normalized[item.Key] = item.Value;
        var payload = JsonSerializer.Serialize(new { CompanyId = companyId, Parameters = normalized });
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(payload)));
    }

    private static int Duration(TimeSpan value) => (int)Math.Min(int.MaxValue, Math.Max(0, value.TotalMilliseconds));
}
