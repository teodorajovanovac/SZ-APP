using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SzApp.Api.Security;
using SzApp.Data;

namespace SzApp.Api.Features.Audit;

public sealed record AuditLogEntryResponse(long Id, DateTimeOffset Timestamp, int? StaffId, string? StaffName, string Action,
    string EntityType, string? ItemId, string? DetailsJson);

/// <summary>GAP-24: EF audit interceptor registration + company-scoped change history (Root/Upravnik = CompanyAdmin).</summary>
public static class AuditFeature
{
    public static IServiceCollection AddAuditFeature(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped(sp =>
        {
            var http = sp.GetRequiredService<IHttpContextAccessor>();
            return new AuditTrailInterceptor(
                () => int.TryParse(http.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null,
                sp.GetService<TimeProvider>() ?? TimeProvider.System,
                () => http.HttpContext?.TraceIdentifier);
        });
        services.AddDbContext<SzAppDbContext>((sp, options) => options.AddInterceptors(sp.GetRequiredService<AuditTrailInterceptor>()));
        return services;
    }

    public static IEndpointRouteBuilder MapAuditEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/v1/companies/{companyId:int}/audit",
                async (int companyId, string? entity, string? itemId, int? staffId, DateOnly? from, DateOnly? to, long? before, int? take,
                    SzAppDbContext db, CancellationToken ct) =>
                {
                    var query = db.AuditLogs.AsNoTracking().Where(x => x.CompanyId == companyId && x.EventSource == "ef");
                    if (!string.IsNullOrWhiteSpace(entity)) query = query.Where(x => x.EntityType == entity);
                    if (!string.IsNullOrWhiteSpace(itemId)) query = query.Where(x => x.ItemId == itemId);
                    if (staffId is { } s) query = query.Where(x => x.StaffId == s);
                    if (from is { } f) { var start = new DateTimeOffset(f.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero); query = query.Where(x => x.Timestamp >= start); }
                    if (to is { } t) { var end = new DateTimeOffset(t.AddDays(1).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero); query = query.Where(x => x.Timestamp < end); }
                    if (before is { } b) query = query.Where(x => x.Id < b);
                    var rows = await query.OrderByDescending(x => x.Id).Take(Math.Clamp(take ?? 100, 1, 500))
                        .Select(x => new AuditLogEntryResponse(x.Id, x.Timestamp, x.StaffId, x.Staff != null ? x.Staff.UserName : null,
                            x.Action, x.EntityType, x.ItemId, x.DetailsJson))
                        .ToArrayAsync(ct);
                    return Results.Ok(rows);
                })
            .WithTags("Audit")
            .RequireAuthorization(SecurityConstants.CompanyAdminPolicy);
        return endpoints;
    }
}
