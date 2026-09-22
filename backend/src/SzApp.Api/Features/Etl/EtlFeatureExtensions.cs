using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using SzApp.Api.Infrastructure;
using SzApp.Api.Security;
using SzApp.Data;
using SzApp.Data.Entities;
using SzApp.Data.Entities.EtlExtended;
using SzApp.Etl.Csv;
using SzApp.Etl.Pipeline;

namespace SzApp.Api.Features.Etl;

public sealed class EtlStorageOptions
{
    public string RootPath { get; set; } = Path.Combine(AppContext.BaseDirectory, "storage", "imports");
    public long MaximumFileSize { get; set; } = 50 * 1024 * 1024;
}

public sealed record EtlRunResponse(
    Guid Id, string SourceSystem, string SourceFile, string SourceTable, string Stage,
    string Status, int SourceRows, int ImportedRows, int QuarantinedRows,
    DateTimeOffset StartedAt, DateTimeOffset? CompletedAt, string? Error);

public static class EtlFeatureExtensions
{
    public static IServiceCollection AddEtlFeature(this IServiceCollection services, IConfiguration configuration)
    {
        var options = new EtlStorageOptions();
        configuration.GetSection("Etl:Storage").Bind(options);
        services.AddSingleton(options);
        services.AddScoped<LegacyCsvParser>();
        services.AddScoped<IEtlPipelineService, EtlPipelineService>();
        return services;
    }

    public static IEndpointRouteBuilder MapEtlEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/companies/{companyId:int}/etl-runs")
            .WithTags("ETL")
            .RequireAuthorization(SecurityConstants.CompanyAccessPolicy);

        group.MapGet("/", ListAsync);
        group.MapGet("/{runId:guid}", GetAsync);
        group.MapPost("/import", ImportAsync)
            .DisableAntiforgery()
            .RequireAuthorization(policy => policy.RequireRole(SecurityConstants.RootRole, SecurityConstants.UpravnikRole));
        group.MapPost("/{runId:guid}/execute", ExecuteAsync)
            .AddEndpointFilter<AntiforgeryEndpointFilter>()
            .RequireAuthorization(policy => policy.RequireRole(SecurityConstants.RootRole, SecurityConstants.UpravnikRole));
        return endpoints;
    }

    private static async Task<IResult> ListAsync(int companyId, SzAppDbContext db, CancellationToken ct)
    {
        var rows = await Query(db, companyId).OrderByDescending(x => x.Run.StartedAt).Take(100).ToArrayAsync(ct);
        return Results.Ok(rows.Select(x => ToResponse(x.Run, x.Context)));
    }

    private static async Task<IResult> GetAsync(int companyId, Guid runId, SzAppDbContext db, CancellationToken ct)
    {
        var row = await Query(db, companyId).SingleOrDefaultAsync(x => x.Run.Id == runId, ct);
        return row is null ? Results.NotFound() : Results.Ok(ToResponse(row.Run, row.Context));
    }

    private static async Task<IResult> ImportAsync(
        int companyId, HttpRequest request, SzAppDbContext db, IEtlPipelineService pipeline,
        EtlStorageOptions options, TimeProvider timeProvider, CancellationToken ct)
    {
        if (!request.HasFormContentType) return Results.ValidationProblem(new Dictionary<string, string[]> { ["file"] = ["CSV fajl je obavezan."] });
        var form = await request.ReadFormAsync(ct);
        var file = form.Files.GetFile("file");
        var sourceTable = form["sourceTable"].ToString().Trim();
        var sourceSystem = form["sourceSystem"].ToString().Trim();
        var encoding = form["encoding"].ToString().Trim();
        var delimiterText = form["delimiter"].ToString();
        if (file is null || file.Length == 0 || file.Length > options.MaximumFileSize)
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["file"] = ["Fajl je prazan ili prelazi dozvoljenu veličinu."] });
        if (LegacyImportTopology.Find(sourceTable) is null)
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["sourceTable"] = ["Izvorna tabela nije na ETL allowlist-i."] });
        if (string.IsNullOrWhiteSpace(sourceSystem) || sourceSystem.Length > 100)
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["sourceSystem"] = ["Izvorni sistem je obavezan."] });
        encoding = string.IsNullOrWhiteSpace(encoding) ? "windows-1250" : encoding;
        _ = LegacyEncoding.Resolve(encoding);
        var delimiter = string.IsNullOrEmpty(delimiterText) ? ';' : delimiterText[0];

        var runId = Guid.NewGuid();
        var safeFileName = Path.GetFileName(file.FileName);
        var companyRoot = Path.GetFullPath(Path.Combine(options.RootPath, companyId.ToString(), runId.ToString("N")));
        Directory.CreateDirectory(companyRoot);
        var fullPath = Path.Combine(companyRoot, safeFileName);
        if (!Path.GetFullPath(fullPath).StartsWith(companyRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            return Results.BadRequest();
        await using (var output = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true))
            await file.CopyToAsync(output, ct);
        string hash;
        await using (var input = File.OpenRead(fullPath)) hash = Convert.ToHexStringLower(await SHA256.HashDataAsync(input, ct));

        var existing = await db.EtlRuns.AsNoTracking().SingleOrDefaultAsync(x => x.CompanyId == companyId && x.SourceSystem == sourceSystem && x.ContentHash == hash, ct);
        if (existing is not null)
        {
            File.Delete(fullPath);
            var context = await db.Set<EtlRunContext>().AsNoTracking().SingleAsync(x => x.EtlRunId == existing.Id, ct);
            return Results.Ok(ToResponse(existing, context));
        }

        var now = timeProvider.GetUtcNow();
        var run = new EtlRun { Id = runId, CompanyId = companyId, SourceSystem = sourceSystem, SourceFile = safeFileName, ContentHash = hash, Status = EtlRunStatus.Pending, StartedAt = now };
        var runContext = new EtlRunContext { EtlRunId = runId, CompanyId = companyId, SourceTable = sourceTable, StoredRelativePath = Path.GetRelativePath(options.RootPath, fullPath), EncodingName = encoding, Delimiter = delimiter.ToString(), Stage = EtlPipelineStage.Uploaded, PipelineVersion = EtlPipelineService.PipelineVersion, UpdatedAt = now };
        db.Add(run); db.Add(runContext); await db.SaveChangesAsync(ct);
        await pipeline.ValidateAndStageAsync(runId, fullPath, encoding, delimiter, ct);
        return Results.Created($"/api/v1/companies/{companyId}/etl-runs/{runId}", ToResponse(run, runContext));
    }

    private static async Task<IResult> ExecuteAsync(int companyId, Guid runId, SzAppDbContext db, IEtlPipelineService pipeline, CancellationToken ct)
    {
        var exists = await db.Set<EtlRunContext>().AnyAsync(x => x.EtlRunId == runId && x.CompanyId == companyId, ct);
        if (!exists) return Results.NotFound();
        await pipeline.ExecuteAsync(runId, ct);
        var row = await Query(db, companyId).SingleAsync(x => x.Run.Id == runId, ct);
        return Results.Ok(ToResponse(row.Run, row.Context));
    }

    private static IQueryable<EtlProjection> Query(SzAppDbContext db, int companyId) =>
        from run in db.EtlRuns.AsNoTracking()
        join context in db.Set<EtlRunContext>().AsNoTracking() on run.Id equals context.EtlRunId
        where context.CompanyId == companyId
        select new EtlProjection(run, context);

    private static EtlRunResponse ToResponse(EtlRun run, EtlRunContext context) => new(
        run.Id, run.SourceSystem, run.SourceFile, context.SourceTable, context.Stage.ToString(), run.Status.ToString(),
        run.SourceRowCount, run.ImportedRowCount, run.QuarantinedRowCount, run.StartedAt, run.CompletedAt, run.Error);

    private sealed record EtlProjection(EtlRun Run, EtlRunContext Context);
}
