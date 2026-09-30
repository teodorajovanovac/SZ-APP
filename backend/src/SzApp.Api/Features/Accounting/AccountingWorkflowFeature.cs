using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using SzApp.Api.Features.LedgerBanking;
using SzApp.Api.Infrastructure;
using SzApp.Api.Security;
using SzApp.Contracts.LedgerBanking;
using SzApp.Data;
using SzApp.Data.Entities;
using SzApp.Data.Entities.LedgerBanking;

namespace SzApp.Api.Features.Accounting;

/// <summary>
/// Suppliers / journals / year-end workflows: GAP-33 copy, GAP-34 virmans, FIN-33 advance
/// reclassification, GAP-13 manual journal drafts (edit/delete), GAP-19 balances per account.
/// </summary>
public static class AccountingWorkflowFeature
{
    public static IServiceCollection AddAccountingWorkflowFeature(this IServiceCollection services)
    {
        services.AddScoped<SupplierWorkflowService>();
        services.AddScoped<AdvanceReclassificationService>();
        return services;
    }

    public static IEndpointRouteBuilder MapAccountingWorkflowEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/companies/{companyId:int}")
            .WithTags("Accounting workflows")
            .RequireAuthorization(SecurityConstants.CompanyAccessPolicy);

        group.MapPost("/supplier-invoices/copy-to-period", (int companyId, CopySupplierInvoicesRequest request, SupplierWorkflowService service, CancellationToken ct) =>
                service.CopyToPeriodAsync(companyId, request, ct))
            .RequireAuthorization(SecurityConstants.CompanyWritePolicy).AddEndpointFilter<AntiforgeryEndpointFilter>();

        group.MapPost("/supplier-invoices/payment-orders", (int companyId, GeneratePaymentOrdersRequest request, SupplierWorkflowService service, CancellationToken ct) =>
                service.GeneratePaymentOrdersAsync(companyId, request, ct))
            .RequireAuthorization(SecurityConstants.CompanyWritePolicy).AddEndpointFilter<AntiforgeryEndpointFilter>();

        group.MapGet("/payment-orders/supplier", (int companyId, bool? includeArchived, SupplierWorkflowService service, CancellationToken ct) =>
            service.ListPaymentOrdersAsync(companyId, includeArchived ?? false, null, ct));

        group.MapGet("/payment-orders/print", async (int companyId, string? ids, SupplierWorkflowService service, CancellationToken ct) =>
            Results.Content(PaymentOrderDocuments.Html(await service.ListPaymentOrdersAsync(companyId, false, ParseIds(ids), ct)),
                "text/html; charset=utf-8", Encoding.UTF8));

        group.MapGet("/payment-orders/export", async (int companyId, string? ids, SupplierWorkflowService service, CancellationToken ct) =>
            Results.File(PaymentOrderDocuments.Csv(await service.ListPaymentOrdersAsync(companyId, false, ParseIds(ids), ct)),
                "text/csv; charset=utf-8", "virmani.csv"));

        group.MapPost("/payment-orders/{id:int}/archive", async Task<Results<NoContent, NotFound>> (int companyId, int id, SupplierWorkflowService service, CancellationToken ct) =>
                await service.ArchiveOrDeleteAsync(companyId, id, false, ct) ? TypedResults.NoContent() : TypedResults.NotFound())
            .RequireAuthorization(SecurityConstants.CompanyWritePolicy).AddEndpointFilter<AntiforgeryEndpointFilter>();

        group.MapDelete("/payment-orders/{id:int}", async Task<Results<NoContent, NotFound>> (int companyId, int id, SupplierWorkflowService service, CancellationToken ct) =>
                await service.ArchiveOrDeleteAsync(companyId, id, true, ct) ? TypedResults.NoContent() : TypedResults.NotFound())
            .RequireAuthorization(SecurityConstants.CompanyWritePolicy).AddEndpointFilter<AntiforgeryEndpointFilter>();

        group.MapGet("/advance-reclassification/preview", (int companyId, AdvanceReclassificationService service, CancellationToken ct) =>
            service.PreviewAsync(companyId, ct));

        group.MapPost("/advance-reclassification", (int companyId, AdvanceReclassificationRequest request, ClaimsPrincipal principal, AdvanceReclassificationService service, CancellationToken ct) =>
                service.ExecuteAsync(companyId, request, StaffId(principal), ct))
            .RequireAuthorization(SecurityConstants.CompanyPostPolicy)
            .AddEndpointFilter<IdempotencyKeyEndpointFilter>()
            .AddEndpointFilter<AntiforgeryEndpointFilter>();

        // GAP-13: a draft (unposted) journal can be re-saved or dropped; posting stays /post (CompanyPost).
        group.MapPut("/journal-entries/{id:int}", (int companyId, int id, UpdateJournalDraftRequest request, IJournalPostingService service, CancellationToken ct) =>
                service.UpdateDraftAsync(companyId, id, request.Journal, DecodeVersion(request.RowVersion), ct))
            .RequireAuthorization(SecurityConstants.CompanyWritePolicy).AddEndpointFilter<AntiforgeryEndpointFilter>();

        group.MapDelete("/journal-entries/{id:int}", async (int companyId, int id, string rowVersion, IJournalPostingService service, CancellationToken ct) =>
            {
                await service.DeleteDraftAsync(companyId, id, DecodeVersion(rowVersion), ct);
                return Results.NoContent();
            })
            .RequireAuthorization(SecurityConstants.CompanyWritePolicy).AddEndpointFilter<AntiforgeryEndpointFilter>();

        // GAP-19: balance per account on a date (posted lines, PostingDate <= asOf).
        group.MapGet("/account-balances", async (int companyId, DateOnly? asOf, SzAppDbContext db, IBusinessClock clock, CancellationToken ct) =>
        {
            var date = asOf ?? clock.Today;
            var rows = await db.LedgerEntries.AsNoTracking()
                .Where(x => x.CompanyId == companyId && x.JournalEntry.IsPosted && x.PostingDate <= date)
                .GroupBy(x => x.Account)
                .Select(g => new { Account = g.Key, Debit = g.Sum(x => x.DebitAmount), Credit = g.Sum(x => x.CreditAmount) })
                .ToListAsync(ct);
            var names = await db.Set<ChartAccount>().AsNoTracking().ToDictionaryAsync(x => x.Account, x => x.Name, ct);
            return rows.OrderBy(x => x.Account, StringComparer.Ordinal)
                .Select(x => new AccountBalanceResponse(x.Account, names.GetValueOrDefault(x.Account), x.Debit, x.Credit, x.Debit - x.Credit))
                .ToArray();
        });

        // Journal editor / opening-balance import: resolve legacy partner codes (ID_K) to partner accounts.
        group.MapGet("/partner-accounts/lookup", async Task<PartnerAccountLookupResponse[]> (int companyId, string? numbers, string? account, SzAppDbContext db, CancellationToken ct) =>
        {
            var wanted = ParseIds(numbers) ?? [];
            if (wanted.Count == 0) return [];
            var query = db.Set<PartnerAccount>().AsNoTracking()
                .Where(x => (x.CompanyId == companyId || x.CompanyId == null) && wanted.Contains(x.AccountNumber));
            if (!string.IsNullOrWhiteSpace(account)) query = query.Where(x => x.Account == account);
            return await query.OrderBy(x => x.AccountNumber).Take(500)
                .Select(x => new PartnerAccountLookupResponse(x.Id, x.AccountNumber, x.Account, x.Partner.Name))
                .ToArrayAsync(ct);
        });

        return endpoints;
    }

    private static IReadOnlyCollection<int>? ParseIds(string? csv) =>
        string.IsNullOrWhiteSpace(csv)
            ? null
            : csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(x => int.TryParse(x, out var n) ? n : throw new BadHttpRequestException("Lista brojeva nije ispravna."))
                .Distinct().Take(1000).ToArray();

    private static int StaffId(ClaimsPrincipal principal) =>
        int.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : throw new UnauthorizedAccessException();

    private static byte[] DecodeVersion(string value)
    {
        try
        {
            return Convert.FromBase64String(value);
        }
        catch (FormatException exception)
        {
            throw new BadHttpRequestException("RowVersion nije ispravan Base64.", exception);
        }
    }
}
