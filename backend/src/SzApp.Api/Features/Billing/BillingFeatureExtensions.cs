using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SzApp.Api.Infrastructure;
using SzApp.Api.Security;
using SzApp.Contracts.Billing;
using SzApp.Domain;
using SzApp.Domain.Billing;

namespace SzApp.Api.Features.Billing;

public static class BillingFeatureExtensions
{
    public static IServiceCollection AddBillingFeature(this IServiceCollection services)
    {
        services.AddScoped<BillingService>();
        services.AddScoped<InvoiceDocumentMapper>();
        services.AddScoped<InvoicePdfService>();
        services.AddScoped<InvoiceGenerationService>();
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddScoped<ILedgerPostingGateway, UnavailableLedgerPostingGateway>();
        services.TryAddScoped<INoticeWorkflowGateway, UnavailableNoticeWorkflowGateway>();
        return services;
    }

    public static IEndpointRouteBuilder MapBillingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var root = endpoints.MapGroup("/api/v1/companies/{companyId:int}")
            .RequireAuthorization(SecurityConstants.CompanyAccessPolicy);

        var calculationTypes = root.MapGroup("/calculation-types").WithTags("Billing - Calculation types");
        calculationTypes.MapGet("/", (BillingService service, CancellationToken ct) => service.ListCalculationTypesAsync(ct));
        calculationTypes.MapPost("/", (CreateCalculationTypeRequest request, BillingService service, CancellationToken ct) =>
                service.CreateCalculationTypeAsync(request, ct))
            .RequireAuthorization(SecurityConstants.CompanyWritePolicy).AddEndpointFilter<AntiforgeryEndpointFilter>();

        var suppliers = root.MapGroup("/supplier-invoices").WithTags("Billing - Supplier invoices");
        suppliers.MapGet("/", (int companyId, [AsParameters] SupplierInvoiceListQuery query, BillingService service, CancellationToken ct) =>
            service.ListSupplierInvoicesAsync(companyId, query, ct));
        suppliers.MapPost("/", (int companyId, CreateSupplierInvoiceRequest request, BillingService service, CancellationToken ct) =>
                service.CreateSupplierInvoiceAsync(companyId, request, ct))
            .RequireAuthorization(SecurityConstants.CompanyWritePolicy).AddEndpointFilter<AntiforgeryEndpointFilter>();
        suppliers.MapPost("/{supplierInvoiceId:int}/post", (int companyId, int supplierInvoiceId, HttpContext context, BillingService service, CancellationToken ct) =>
                service.PostSupplierInvoiceAsync(companyId, supplierInvoiceId, IdempotencyKey(context), ct))
            .RequireAuthorization(SecurityConstants.CompanyPostPolicy)
            .AddEndpointFilter<AntiforgeryEndpointFilter>().AddEndpointFilter<IdempotencyKeyEndpointFilter>();
        suppliers.MapPost("/{supplierInvoiceId:int}/cancel", (int companyId, int supplierInvoiceId, HttpContext context, BillingService service, CancellationToken ct) =>
                service.CancelSupplierInvoicePostingAsync(companyId, supplierInvoiceId, IdempotencyKey(context), ct))
            .RequireAuthorization(SecurityConstants.CompanyPostPolicy)
            .AddEndpointFilter<AntiforgeryEndpointFilter>().AddEndpointFilter<IdempotencyKeyEndpointFilter>();

        var batches = root.MapGroup("/invoice-batches").WithTags("Billing - Invoice batches");
        batches.MapGet("/", (int companyId, int page, int pageSize, BillingService service, CancellationToken ct) =>
            service.ListInvoiceBatchesAsync(companyId, page, pageSize, ct));
        batches.MapPost("/", (int companyId, CreateInvoiceBatchRequest request, ClaimsPrincipal principal, BillingService service, CancellationToken ct) =>
                service.CreateInvoiceBatchAsync(companyId, StaffId(principal), request, ct))
            .RequireAuthorization(SecurityConstants.CompanyWritePolicy).AddEndpointFilter<AntiforgeryEndpointFilter>();
        // Item 8 wizard: server-computed invoices (R0..R3). Scope = route company, a location category or all
        // companies the caller may write to (request.Scope); one batch per company.
        batches.MapPost("/preview", async (int companyId, GenerateInvoicesV2Request request, ClaimsPrincipal principal, InvoiceGenerationService service, CancellationToken ct) =>
                await service.PreviewScopeAsync(await service.ResolveScopeAsync(principal, companyId, request.Scope, request.LocationCategoryId, ct), request, ct))
            .RequireAuthorization(SecurityConstants.CompanyWritePolicy).AddEndpointFilter<AntiforgeryEndpointFilter>();
        batches.MapPost("/generate", async (int companyId, GenerateInvoicesV2Request request, ClaimsPrincipal principal, InvoiceGenerationService service,
                BillingService billing, CancellationToken ct) =>
                await service.GenerateScopeAsync(await service.ResolveScopeAsync(principal, companyId, request.Scope, request.LocationCategoryId, ct),
                    StaffId(principal), request, billing, ct))
            .RequireAuthorization(SecurityConstants.CompanyWritePolicy)
            .AddEndpointFilter<AntiforgeryEndpointFilter>().AddEndpointFilter<IdempotencyKeyEndpointFilter>();
        batches.MapPost("/{batchId:int}/preview", (int companyId, int batchId, InvoiceGenerationRequest request, BillingService service, CancellationToken ct) =>
                service.PreviewBatchAsync(companyId, batchId, request, ct))
            .RequireAuthorization(SecurityConstants.CompanyWritePolicy).AddEndpointFilter<AntiforgeryEndpointFilter>();
        batches.MapPost("/{batchId:int}/generate", (int companyId, int batchId, InvoiceGenerationRequest request, BillingService service, CancellationToken ct) =>
                service.GenerateBatchAsync(companyId, batchId, request, ct))
            .RequireAuthorization(SecurityConstants.CompanyWritePolicy)
            .AddEndpointFilter<AntiforgeryEndpointFilter>().AddEndpointFilter<IdempotencyKeyEndpointFilter>();
        batches.MapPost("/{batchId:int}/post", (int companyId, int batchId, HttpContext context, BillingService service, CancellationToken ct) =>
                service.PostBatchAsync(companyId, batchId, IdempotencyKey(context), ct))
            .RequireAuthorization(SecurityConstants.CompanyPostPolicy)
            .AddEndpointFilter<AntiforgeryEndpointFilter>().AddEndpointFilter<IdempotencyKeyEndpointFilter>();

        // GAP-02/03/26: PDF + IPS QR rendering and bulk email sending. Read access is enough to
        // view/download (mirrors other read endpoints in this file); only /emails/send mutates.
        batches.MapPost("/{batchId:int}/pdf", async (int companyId, int batchId, InvoicePdfService pdfService, CancellationToken ct) =>
        {
            var result = await pdfService.RenderBatchZipAsync(companyId, batchId, ct);
            return result is null ? Results.NotFound() : Results.File(result.Bytes, "application/zip", result.FileName);
        }).AddEndpointFilter<AntiforgeryEndpointFilter>();
        batches.MapPost("/{batchId:int}/emails/preview", (int companyId, int batchId, InvoicePdfService pdfService, CancellationToken ct) =>
                pdfService.PreviewBatchEmailsAsync(companyId, batchId, ct))
            .AddEndpointFilter<AntiforgeryEndpointFilter>();
        batches.MapPost("/{batchId:int}/emails/send", (int companyId, int batchId, ClaimsPrincipal principal, InvoicePdfService pdfService, CancellationToken ct) =>
                pdfService.SendBatchEmailsAsync(companyId, batchId, StaffId(principal), ct))
            .RequireAuthorization(SecurityConstants.CompanyWritePolicy)
            .AddEndpointFilter<AntiforgeryEndpointFilter>().AddEndpointFilter<IdempotencyKeyEndpointFilter>();

        var invoices = root.MapGroup("/invoices").WithTags("Billing - Invoices");
        invoices.MapGet("/", (int companyId, int page, int pageSize, BillingService service, CancellationToken ct) =>
            service.ListInvoicesAsync(companyId, page, pageSize, ct));
        invoices.MapGet("/{invoiceId:int}", async (int companyId, int invoiceId, BillingService service, CancellationToken ct) =>
        {
            var invoice = await service.GetInvoiceAsync(companyId, invoiceId, ct);
            return invoice is null ? Results.NotFound() : Results.Ok(invoice);
        });
        invoices.MapGet("/{invoiceId:int}/pdf", async (int companyId, int invoiceId, InvoicePdfService pdfService, CancellationToken ct) =>
        {
            var result = await pdfService.RenderSingleAsync(companyId, invoiceId, ct);
            return result is null ? Results.NotFound() : Results.File(result.Bytes, "application/pdf", result.FileName);
        });
        invoices.MapPost("/{invoiceId:int}/cancel", (int companyId, int invoiceId, CancelInvoiceRequest request, HttpContext context, BillingService service, CancellationToken ct) =>
                service.CancelInvoiceAsync(companyId, invoiceId, request, IdempotencyKey(context), ct))
            .RequireAuthorization(SecurityConstants.CompanyPostPolicy)
            .AddEndpointFilter<AntiforgeryEndpointFilter>().AddEndpointFilter<IdempotencyKeyEndpointFilter>();

        var benefits = root.MapGroup("/benefits").WithTags("Billing - Benefits");
        benefits.MapGet("/", (int companyId, int period, BillingService service, CancellationToken ct) =>
            service.ListBenefitsAsync(companyId, period, ct));
        benefits.MapPost("/", (int companyId, CreateBenefitRequest request, BillingService service, CancellationToken ct) =>
                service.CreateBenefitAsync(companyId, request, ct))
            .RequireAuthorization(SecurityConstants.CompanyWritePolicy).AddEndpointFilter<AntiforgeryEndpointFilter>();

        var interest = root.MapGroup("/interest").WithTags("Billing - Interest");
        interest.MapGet("/rates", (BillingService service, CancellationToken ct) => service.ListInterestRatesAsync(ct));
        interest.MapPost("/rates", (CreateInterestRateRequest request, BillingService service, CancellationToken ct) => service.CreateInterestRateAsync(request, ct))
            .RequireAuthorization(SecurityConstants.CompanyAdminPolicy)
            .AddEndpointFilter<AntiforgeryEndpointFilter>();
        interest.MapPost("/calculate", (CalculateInterestRequest request, BillingService service, CancellationToken ct) => service.CalculateInterestAsync(request, ct))
            .RequireAuthorization(SecurityConstants.CompanyWritePolicy).AddEndpointFilter<AntiforgeryEndpointFilter>();
        interest.MapGet("/statements", (int companyId, int invoiceBatchId, BillingService service, CancellationToken ct) =>
            service.ListInterestStatementsAsync(companyId, invoiceBatchId, ct));
        interest.MapGet("/period-presets", (int companyId, int periodYYMM, DateOnly? previousValueDate, DateOnly? balanceAsOfDate, DateOnly? dueDate,
                BillingService service, CancellationToken ct) =>
            service.GetInterestPeriodPresetsAsync(companyId, periodYYMM, previousValueDate, balanceAsOfDate, dueDate, ct));
        interest.MapPost("/runs", (int companyId, RunInterestRequest request, BillingService service, CancellationToken ct) =>
                service.RunInterestAsync(companyId, request, ct))
            .RequireAuthorization(SecurityConstants.CompanyWritePolicy).AddEndpointFilter<AntiforgeryEndpointFilter>();

        var notices = root.MapGroup("/notices").WithTags("Billing - Notices");
        notices.MapGet("/", (int companyId, int page, int pageSize, BillingService service, CancellationToken ct) =>
            service.ListNoticesAsync(companyId, page, pageSize, ct));
        notices.MapPost("/{noticeId:int}/render", (int companyId, int noticeId, BillingService service, CancellationToken ct) =>
                service.RenderNoticeAsync(companyId, noticeId, ct))
            .RequireAuthorization(SecurityConstants.CompanyWritePolicy).AddEndpointFilter<AntiforgeryEndpointFilter>();
        notices.MapPost("/{noticeId:int}/send", (int companyId, int noticeId, BillingService service, CancellationToken ct) =>
                service.SendNoticeAsync(companyId, noticeId, ct))
            .RequireAuthorization(SecurityConstants.CompanyWritePolicy).AddEndpointFilter<AntiforgeryEndpointFilter>();

        var noticeTemplates = root.MapGroup("/notice-templates").WithTags("Billing - Notice templates");
        noticeTemplates.MapGet("/", (int companyId, BillingService service, CancellationToken ct) =>
            service.ListNoticeTemplatesAsync(companyId, ct));
        noticeTemplates.MapPost("/", (int companyId, CreateNoticeTemplateRequest request, BillingService service, CancellationToken ct) =>
                service.CreateNoticeTemplateAsync(companyId, request, ct))
            .RequireAuthorization(SecurityConstants.CompanyAdminPolicy)
            .AddEndpointFilter<AntiforgeryEndpointFilter>();

        // P11: company rows need CompanyAdmin; global rows (isGlobal / CompanyId null) additionally Root (checked in the service).
        var noticeCosts = root.MapGroup("/notice-additional-costs").WithTags("Billing - Notice additional costs");
        noticeCosts.MapGet("/", (int companyId, BillingService service, CancellationToken ct) => service.ListNoticeCostsAsync(companyId, ct));
        noticeCosts.MapPost("/", (int companyId, SaveNoticeAditionalCostRequest request, ClaimsPrincipal principal, BillingService service, CancellationToken ct) =>
                service.SaveNoticeCostAsync(companyId, null, request, principal.IsInRole(SecurityConstants.RootRole), ct))
            .RequireAuthorization(SecurityConstants.CompanyAdminPolicy).AddEndpointFilter<AntiforgeryEndpointFilter>();
        noticeCosts.MapPut("/{id:int}", (int companyId, int id, SaveNoticeAditionalCostRequest request, ClaimsPrincipal principal, BillingService service, CancellationToken ct) =>
                service.SaveNoticeCostAsync(companyId, id, request, principal.IsInRole(SecurityConstants.RootRole), ct))
            .RequireAuthorization(SecurityConstants.CompanyAdminPolicy).AddEndpointFilter<AntiforgeryEndpointFilter>();
        noticeCosts.MapDelete("/{id:int}", async (int companyId, int id, ClaimsPrincipal principal, BillingService service, CancellationToken ct) =>
            {
                await service.DeleteNoticeCostAsync(companyId, id, principal.IsInRole(SecurityConstants.RootRole), ct);
                return Results.NoContent();
            })
            .RequireAuthorization(SecurityConstants.CompanyAdminPolicy).AddEndpointFilter<AntiforgeryEndpointFilter>();

        var noticeBatches = root.MapGroup("/notice-batches").WithTags("Billing - Notice batches");
        noticeBatches.MapPost("/", (int companyId, CreateNoticeBatchRequest request, BillingService service, CancellationToken ct) =>
                service.CreateNoticeBatchAsync(companyId, request, ct))
            .RequireAuthorization(SecurityConstants.CompanyWritePolicy).AddEndpointFilter<AntiforgeryEndpointFilter>();
        noticeBatches.MapPost("/{batchId:int}/generate", (int companyId, int batchId, GenerateNoticesRequest request, BillingService service, CancellationToken ct) =>
                service.GenerateNoticesAsync(companyId, batchId, request, ct))
            .RequireAuthorization(SecurityConstants.CompanyWritePolicy)
            .AddEndpointFilter<AntiforgeryEndpointFilter>().AddEndpointFilter<IdempotencyKeyEndpointFilter>();

        var paymentOrders = root.MapGroup("/payment-orders").WithTags("Billing - Payment orders");
        paymentOrders.MapGet("/", (int companyId, int page, int pageSize, BillingService service, CancellationToken ct) =>
            service.ListPaymentOrdersAsync(companyId, page, pageSize, ct));
        paymentOrders.MapPost("/", (int companyId, CreatePaymentOrderRequest request, BillingService service, CancellationToken ct) =>
                service.CreatePaymentOrderAsync(companyId, request, ct))
            .RequireAuthorization(SecurityConstants.CompanyWritePolicy).AddEndpointFilter<AntiforgeryEndpointFilter>();

        return endpoints;
    }

    private static int StaffId(ClaimsPrincipal principal) =>
        int.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : throw new DomainRuleException("security.staff-id-missing", "Identitet zaposlenog nije dostupan.");

    private static string IdempotencyKey(HttpContext context) =>
        context.Request.Headers[IdempotencyKeyEndpointFilter.HeaderName].ToString().Trim();

    private sealed class UnavailableLedgerPostingGateway : ILedgerPostingGateway
    {
        public Task<LedgerPostingResult> PostAsync(LedgerPostingRequest request, CancellationToken cancellationToken) =>
            throw new DomainRuleException("billing.ledger-unavailable", "Servis glavne knjige nije povezan.");
    }

    private sealed class UnavailableNoticeWorkflowGateway : INoticeWorkflowGateway
    {
        public Task<string> RenderAsync(int companyId, int noticeId, CancellationToken cancellationToken) =>
            throw new DomainRuleException("notice.renderer-unavailable", "Servis za dokumente nije povezan.");

        public Task SendAsync(int companyId, int noticeId, string renderedDocumentPath, CancellationToken cancellationToken) =>
            throw new DomainRuleException("notice.sender-unavailable", "Servis za slanje nije povezan.");
    }
}
