using System.Security.Claims;
using System.Text;
using SzApp.Api.Infrastructure;
using SzApp.Api.Security;
using SzApp.Contracts.Reports;
using SzApp.Domain;
using SzApp.Domain.Reports;

namespace SzApp.Api.Features.Reports;

public static class ReportsFeatureExtensions
{
    public static IServiceCollection AddReportsFeature(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ReportsOptions>().Bind(configuration.GetSection(ReportsOptions.SectionName));
        services.AddSingleton<IReportFunctionRegistry, DefaultReportFunctionRegistry>();
        services.AddScoped<ISafeReportQueryRunner, SafeReportQueryRunner>();
        services.AddScoped<ReportService>();
        return services;
    }

    public static IEndpointRouteBuilder MapReportsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var root = endpoints.MapGroup("/api/v1/companies/{companyId:int}")
            .RequireAuthorization(SecurityConstants.CompanyAccessPolicy);

        var analyses = root.MapGroup("/analyses").WithTags("Analyses");
        analyses.MapGet("/", (int companyId, ReportService service, CancellationToken ct) =>
            service.ListAnalysesAsync(companyId, ct));
        analyses.MapPost("/{definitionId:int}/run", (int companyId, int definitionId, RunReportRequest request,
                ClaimsPrincipal principal, HttpContext context, ReportService service, CancellationToken ct) =>
            service.RunAnalysisAsync(companyId, StaffId(principal), definitionId, request, context.TraceIdentifier, ct))
            .AddEndpointFilter<AntiforgeryEndpointFilter>();
        analyses.MapPost("/{definitionId:int}/export.csv", async (int companyId, int definitionId, RunReportRequest request,
            ClaimsPrincipal principal, HttpContext context, ReportService service, CancellationToken ct) =>
        {
            var bytes = await service.ExportAnalysisCsvAsync(companyId, StaffId(principal), definitionId, request, context.TraceIdentifier, ct);
            return Results.File(bytes, "text/csv; charset=utf-8", $"analysis-{definitionId}.csv");
        }).AddEndpointFilter<AntiforgeryEndpointFilter>();

        var reports = root.MapGroup("/reports").WithTags("Reports");
        reports.MapGet("/", (int companyId, ReportService service, CancellationToken ct) =>
            service.ListReportsAsync(companyId, ct));
        reports.MapPost("/{definitionId:int}/run", (int companyId, int definitionId, RunReportRequest request,
                ClaimsPrincipal principal, HttpContext context, ReportService service, CancellationToken ct) =>
            service.RunReportAsync(companyId, StaffId(principal), definitionId, request, context.TraceIdentifier, ct))
            .AddEndpointFilter<AntiforgeryEndpointFilter>();
        reports.MapPost("/{definitionId:int}/export.csv", async (int companyId, int definitionId, RunReportRequest request,
            ClaimsPrincipal principal, HttpContext context, ReportService service, CancellationToken ct) =>
        {
            var bytes = await service.ExportReportCsvAsync(companyId, StaffId(principal), definitionId, request, context.TraceIdentifier, ct);
            return Results.File(bytes, "text/csv; charset=utf-8", $"report-{definitionId}.csv");
        }).AddEndpointFilter<AntiforgeryEndpointFilter>();
        reports.MapPost("/{definitionId:int}/print", async (int companyId, int definitionId, RunReportRequest request,
            ClaimsPrincipal principal, HttpContext context, ReportService service, CancellationToken ct) =>
        {
            var html = await service.RenderReportHtmlAsync(companyId, StaffId(principal), definitionId, request, context.TraceIdentifier, ct);
            return Results.Content(html, "text/html; charset=utf-8", Encoding.UTF8);
        }).AddEndpointFilter<AntiforgeryEndpointFilter>();

        return endpoints;
    }

    private static int StaffId(ClaimsPrincipal principal) =>
        int.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : throw new DomainRuleException("security.staff-id-missing", "Identitet zaposlenog nije dostupan.");
}
