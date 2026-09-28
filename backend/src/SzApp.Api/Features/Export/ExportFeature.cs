using System.Globalization;
using System.IO.Compression;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SzApp.Api.Security;
using SzApp.Data;
using SzApp.Data.Entities;
using SzApp.Etl.Export;

namespace SzApp.Api.Features.Export;

public sealed record ExportTableResponse(string Name, string Group, bool Reimportable, string? ImportTable, bool HasPersonalData);

public static class ExportFeature
{
    public static IEndpointRouteBuilder MapExportEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var company = endpoints.MapGroup("/api/v1/companies/{companyId:int}/export")
            .WithTags("Export")
            .RequireAuthorization(SecurityConstants.CompanyAdminPolicy);

        company.MapGet("/tables", (int companyId) => Results.Ok(ExportCatalog.Tables.Select(x =>
            new ExportTableResponse(x.Name, x.Group.ToString(), x.ImportTable is not null, x.ImportTable, x.Columns.Any(c => c.IsPersonal)))));

        company.MapGet("", (int companyId, string? tables, string? encoding, bool? includePersonalData,
                HttpContext http, SzAppDbContext db, TimeProvider clock, CancellationToken ct) =>
            ExportAsync(http, db, clock, [companyId], tables, encoding, includePersonalData ?? false, ct));

        endpoints.MapGet("/api/v1/export", (string? companyIds, string? tables, string? encoding, bool? includePersonalData,
                HttpContext http, SzAppDbContext db, TimeProvider clock, CancellationToken ct) =>
            {
                int[]? ids = null;
                if (!string.IsNullOrWhiteSpace(companyIds) && !companyIds.Trim().Equals("all", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = companyIds.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                    ids = new int[parts.Length];
                    for (var i = 0; i < parts.Length; i++)
                        if (!int.TryParse(parts[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out ids[i]))
                            return Task.FromResult(Invalid("companyIds", "companyIds mora biti lista brojeva ili 'all'."));
                }
                return ExportAsync(http, db, clock, ids, tables, encoding, includePersonalData ?? false, ct);
            })
            .WithTags("Export")
            .RequireAuthorization(policy => policy.RequireRole(SecurityConstants.RootRole));

        return endpoints;
    }

    private static IResult Invalid(string field, string message) =>
        Results.ValidationProblem(new Dictionary<string, string[]> { [field] = [message] });

    /// <param name="companyIds">null = all companies (Root route only).</param>
    private static async Task<IResult> ExportAsync(
        HttpContext http, SzAppDbContext db, TimeProvider clock, int[]? companyIds,
        string? tables, string? encoding, bool includePersonalData, CancellationToken ct)
    {
        var names = string.IsNullOrWhiteSpace(tables)
            ? ExportCatalog.Tables.Select(x => x.Name).ToArray()
            : tables.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var selected = names.Select(ExportCatalog.Find).ToArray();
        if (selected.Length == 0 || selected.Any(x => x is null))
            return Invalid("tables", $"Nepoznate tabele: {string.Join(", ", names.Where(n => ExportCatalog.Find(n) is null))}");

        var fallback = new CountingReplacementFallback();
        Encoding textEncoding;
        try { textEncoding = LegacyCsvWriter.ResolveEncoding(encoding, fallback); }
        catch (ArgumentException exception) { return Invalid("encoding", exception.Message); }

        var companies = await db.Companies.AsNoTracking()
            .Where(x => companyIds == null || companyIds.Contains(x.Id))
            .OrderBy(x => x.Id)
            .Select(x => new { x.Id, x.ShortName })
            .ToArrayAsync(ct);
        if (companies.Length == 0 || (companyIds is not null && companies.Length != companyIds.Distinct().Count()))
            return Results.NotFound();
        var ids = companies.Select(x => x.Id).ToArray();

        var now = clock.GetUtcNow();
        var encodingLabel = textEncoding is UTF8Encoding ? "utf-8" : "windows-1250";
        db.AuditLogs.Add(new AuditLog
        {
            CompanyId = ids.Length == 1 ? ids[0] : null,
            StaffId = int.TryParse(http.User.FindFirstValue(ClaimTypes.NameIdentifier), out var staffId) ? staffId : null,
            Timestamp = now,
            CorrelationId = http.TraceIdentifier,
            EntityType = "DataExport",
            Action = "Export",
            EventSource = "api",
            DetailsJson = JsonSerializer.Serialize(new { companyIds = ids, tables = selected.Select(x => x!.Name), encoding = encodingLabel, includePersonalData })
        });
        await db.SaveChangesAsync(ct);

        var stamp = LegacyCsvWriter.Format(now);
        var scope = companyIds is null ? "all" : string.Join('-', ids);
        http.Response.ContentType = "application/zip";
        http.Response.Headers.ContentDisposition =
            $"attachment; filename=\"szapp-export-{scope}-{now.UtcDateTime:yyyyMMdd-HHmmss}.zip\"";

        var counts = new List<(string Name, int Rows, string? ImportTable)>();
        await using (var zip = await ZipArchive.CreateAsync(http.Response.Body, ZipArchiveMode.Create, leaveOpen: true, entryNameEncoding: null, ct))
        {
            foreach (var table in selected.Select(x => x!))
            {
                var columns = table.Columns.Where(c => includePersonalData || !c.IsPersonal).ToArray();
                var rows = 0;
                await using (var entry = await zip.CreateEntry($"{table.Name}.csv", CompressionLevel.Optimal).OpenAsync(ct))
                await using (var text = new StreamWriter(entry, textEncoding, 65536))
                {
                    var csv = new LegacyCsvWriter(text);
                    await csv.WriteRowAsync(columns.Select(c => c.Name), ct);
                    await foreach (var row in table.Rows(db, ids, ct))
                    {
                        if (table.RowFilter is not null && !table.RowFilter(row)) continue;
                        await csv.WriteRowAsync(columns.Select(c => c.Property.GetValue(row)), ct);
                        rows++;
                    }
                }
                counts.Add((table.Name, rows, table.ImportTable));
            }

            await using var manifestStream = await zip.CreateEntry("manifest.txt", CompressionLevel.Optimal).OpenAsync(ct);
            await using var manifest = new StreamWriter(manifestStream, new UTF8Encoding(true));
            await manifest.WriteAsync(BuildManifest(stamp, companies.Select(x => $"{x.Id} {x.ShortName}"), encodingLabel, includePersonalData, fallback.Count, counts));
        }

        return Results.Empty;
    }

    private static string BuildManifest(
        string stamp, IEnumerable<string> companies, string encoding, bool includePersonalData, int replacedCharacters,
        IEnumerable<(string Name, int Rows, string? ImportTable)> counts)
    {
        var text = new StringBuilder()
            .Append("SzApp izvoz podataka\r\n")
            .Append($"Vreme: {stamp} (Europe/Belgrade)\r\n")
            .Append($"Firme: {string.Join(", ", companies)}\r\n")
            .Append($"Kodna strana: {encoding}; razdvajač ';'; decimale zapeta; datumi d.M.yyyy; Da/Ne = -1/0\r\n")
            .Append($"Lični podaci (JMBG, broj lične karte): {(includePersonalData ? "uključeni" : "izostavljeni")}\r\n");
        if (replacedCharacters > 0)
            text.Append($"UPOZORENJE: {replacedCharacters} znakova nije moguće zapisati u windows-1250 i zamenjeno je sa '?'. Za verni izvoz izaberite UTF-8.\r\n");
        text.Append("\r\nTabela;Redova;Uvoz kao (prazno = samo izvoz)\r\n");
        foreach (var (name, rows, importTable) in counts)
            text.Append($"{name}.csv;{rows};{importTable}\r\n");
        return text.ToString();
    }
}
