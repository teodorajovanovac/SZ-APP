using System.IO.Compression;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SzApp.Api.Features.Platform;
using SzApp.Contracts.Billing;
using SzApp.Data;
using SzApp.Data.Entities;
using SzApp.Data.Entities.Billing;
using SzApp.Domain;
using SzApp.Domain.Billing;

namespace SzApp.Api.Features.Billing;

/// <summary>
/// GAP-09: opomena PDF + IPS QR, batch ZIP (legacy OPOMENE/{YYYY}/{YYYY-MM-DD}/{FolderSZ}/{IDK}-{date}.pdf)
/// and bulk email through the outbox. Also the real <see cref="INoticeWorkflowGateway"/> behind the
/// per-notice render/send commands in BillingService.
/// </summary>
public sealed class NoticeDocumentService(
    SzAppDbContext db,
    IPlatformDocumentStorage storage,
    TimeProvider timeProvider,
    IHttpContextAccessor httpContextAccessor) : INoticeWorkflowGateway
{
    private const string DefaultSubject = "Opomena {NoticeNumber}";
    private const string DefaultBody = "Poštovani {CustomerName},<br/>u prilogu je opomena za dug od {Debt} RSD (ukupno za uplatu {Total} RSD).";

    public async Task<NoticeDocumentData?> BuildAsync(int companyId, int noticeId, CancellationToken ct)
    {
        var notice = await db.Set<Notice>().AsNoTracking().Include(x => x.Lines).Include(x => x.NoticeBatch)
            .SingleOrDefaultAsync(x => x.CompanyId == companyId && x.Id == noticeId, ct);
        if (notice is null) return null;
        var batch = notice.NoticeBatch;
        var template = await db.Set<NoticeTemplate>().AsNoTracking().SingleAsync(x => x.Id == batch.NoticeTemplateId, ct);
        var company = await db.Set<Company>().AsNoTracking().Include(x => x.Partner).SingleAsync(x => x.Id == companyId, ct);
        var account = await db.Set<PartnerAccount>().AsNoTracking().Include(x => x.Partner).SingleAsync(x => x.Id == notice.PartnerAccountId, ct);
        var issuerAccount = await db.Set<BankAccount>().AsNoTracking()
            .Where(x => x.CompanyId == companyId && x.IsActive)
            .OrderBy(x => x.SortIndex ?? int.MaxValue)
            .Select(x => x.AccountNumber)
            .FirstOrDefaultAsync(ct) ?? string.Empty;
        var issuerAddress = await DefaultAddressAsync(company.PartnerId, ct);
        var customerAddress = await DefaultAddressAsync(account.PartnerId, ct);

        return new NoticeDocumentData(
            NoticeNumber: NoticeNumber(notice.PaymentReference),
            Date: batch.Date,
            PaymentCutoff: batch.UpToPaymentDate,
            IssuerName: company.PrintName,
            IssuerCity: issuerAddress?.City ?? string.Empty,
            IssuerTaxNumber: company.Partner.TaxNumber,
            IssuerRegistrationNumber: company.Partner.RegistrationNumber,
            IssuerAccountNumber: issuerAccount,
            DecisionDate: template.DecisionDate,
            CustomerName: account.Partner.Name,
            CustomerAddress: customerAddress is null ? string.Empty : $"{customerAddress.StreetAddress}, {customerAddress.City}".Trim(' ', ','),
            AccountNumber: account.AccountNumber,
            Debt: notice.Debt,
            AdditionalCosts: notice.AdditionalCosts,
            PaymentReference: notice.PaymentReference,
            Subject: template.Subject,
            Body: template.Body,
            Closing: template.Closing,
            Signature: template.Signature,
            SlipCaption: batch.CustomCaptionOnSlip,
            Lines: notice.Lines.OrderBy(x => x.DueDate).ThenBy(x => x.Id)
                .Select(x => new NoticeDocumentLine(x.DocumentRef, x.Text, x.DueDate, x.Debit, x.Credit, x.Sum)).ToArray());
    }

    /// <summary>Notice number = the payment reference without its KB97 check digits ("{SZ}-{IDK}-P{yyyymmdd}").</summary>
    public static string NoticeNumber(string paymentReference)
    {
        var dash = paymentReference.IndexOf('-');
        return dash < 0 ? paymentReference : paymentReference[(dash + 1)..];
    }

    public async Task<InvoicePdfResult?> RenderSingleAsync(int companyId, int noticeId, bool doubleSlip, CancellationToken ct)
    {
        var data = await BuildAsync(companyId, noticeId, ct);
        return data is null ? null : new InvoicePdfResult(NoticePdfRenderer.Render(data, doubleSlip), FileName(data));
    }

    public async Task<InvoicePdfResult?> RenderBatchZipAsync(int companyId, int batchId, bool doubleSlip, CancellationToken ct)
    {
        var batch = await db.Set<NoticeBatch>().AsNoTracking().SingleOrDefaultAsync(x => x.CompanyId == companyId && x.Id == batchId, ct);
        if (batch is null) return null;
        var company = await db.Set<Company>().AsNoTracking().SingleAsync(x => x.Id == companyId, ct);
        var folderSz = InvoicePdfService.SanitizeFileNamePart(string.IsNullOrWhiteSpace(company.RelativeFolderName) ? company.ShortName : company.RelativeFolderName);

        using var zipStream = new MemoryStream();
        using (var zip = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var noticeId in await NoticeIdsAsync(companyId, batchId, ct))
            {
                var data = await BuildAsync(companyId, noticeId, ct);
                if (data is null) continue;
                var entry = zip.CreateEntry($"OPOMENE/{batch.Date:yyyy}/{batch.Date:yyyy-MM-dd}/{folderSz}/{FileName(data)}", CompressionLevel.Fastest);
                await using var entryStream = entry.Open();
                await entryStream.WriteAsync(NoticePdfRenderer.Render(data, doubleSlip), ct);
            }
        }
        return new InvoicePdfResult(zipStream.ToArray(), $"opomene-{batch.Date:yyyy-MM-dd}.zip");
    }

    public async Task<NoticeEmailPreviewResult> PreviewBatchEmailsAsync(int companyId, int batchId, CancellationToken ct)
    {
        var recipients = await RecipientsAsync(companyId, batchId, ct);
        var missing = recipients.Where(x => x.Email is null).Select(x => x.Name).ToArray();
        return new NoticeEmailPreviewResult(recipients.Count, recipients.Count - missing.Length, missing.Length, missing);
    }

    public async Task<NoticeEmailSendResult> SendBatchEmailsAsync(int companyId, int batchId, int staffId, CancellationToken ct)
    {
        var enqueued = 0;
        var skipped = 0;
        foreach (var recipient in await RecipientsAsync(companyId, batchId, ct))
        {
            if (recipient.Email is null) { skipped++; continue; }
            var notice = await db.Set<Notice>().SingleAsync(x => x.Id == recipient.NoticeId, ct);
            await SendOneAsync(companyId, notice, recipient.Email, staffId, ct);
            enqueued++;
        }
        return new NoticeEmailSendResult(enqueued, skipped);
    }

    // INoticeWorkflowGateway: per-notice render/send used by BillingService.RenderNoticeAsync/SendNoticeAsync.
    public async Task<string> RenderAsync(int companyId, int noticeId, CancellationToken cancellationToken)
    {
        var data = await BuildAsync(companyId, noticeId, cancellationToken)
            ?? throw new DomainRuleException("notice.not-found", "Opomena ne postoji.");
        var document = await InvoicePdfService.StoreDocumentAsync(db, storage, timeProvider, companyId, CurrentStaffId(),
            FileName(data), NoticePdfRenderer.Render(data), "Notice", noticeId, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return document.RelativePath;
    }

    public async Task SendAsync(int companyId, int noticeId, string renderedDocumentPath, CancellationToken cancellationToken)
    {
        var notice = await db.Set<Notice>().AsNoTracking().SingleAsync(x => x.CompanyId == companyId && x.Id == noticeId, cancellationToken);
        var partnerId = await db.Set<PartnerAccount>().Where(x => x.Id == notice.PartnerAccountId).Select(x => x.PartnerId).SingleAsync(cancellationToken);
        var email = await InvoicePdfService.FindRecipientEmailAsync(db, partnerId, cancellationToken)
            ?? throw new DomainRuleException("notice.email-missing", "Partner nema e-mail adresu za prijem dokumenata.");
        var document = await db.Set<DocumentRecord>()
            .Where(x => x.CompanyId == companyId && x.SourceTable == "Notice" && x.ReferenceId == noticeId && x.RelativePath == renderedDocumentPath)
            .OrderByDescending(x => x.Id).FirstOrDefaultAsync(cancellationToken)
            ?? throw new DomainRuleException("notice.document-missing", "Renderovani dokument opomene nije pronađen.");
        var data = (await BuildAsync(companyId, noticeId, cancellationToken))!;
        var (subject, body) = await LoadTemplateAsync(companyId, cancellationToken);
        await InvoicePdfService.EnqueueEmailAsync(db, timeProvider, companyId, CurrentStaffId(), email,
            ApplyTokens(subject, data), ApplyTokens(body, data), document, cancellationToken);
    }

    private async Task SendOneAsync(int companyId, Notice notice, string email, int staffId, CancellationToken ct)
    {
        var data = (await BuildAsync(companyId, notice.Id, ct))!;
        var document = await InvoicePdfService.StoreDocumentAsync(db, storage, timeProvider, companyId, staffId,
            FileName(data), NoticePdfRenderer.Render(data), "Notice", notice.Id, ct);
        var (subject, body) = await LoadTemplateAsync(companyId, ct);
        await InvoicePdfService.EnqueueEmailAsync(db, timeProvider, companyId, staffId, email,
            ApplyTokens(subject, data), ApplyTokens(body, data), document, ct);
        notice.RenderedDocumentPath = document.RelativePath;
        notice.DeliveryStatus = NoticeDeliveryStatus.Queued;
        await db.SaveChangesAsync(ct);
    }

    private sealed record Recipient(int NoticeId, string Name, string? Email);

    private async Task<List<Recipient>> RecipientsAsync(int companyId, int batchId, CancellationToken ct)
    {
        var rows = await (from n in db.Set<Notice>().AsNoTracking()
                          join pa in db.Set<PartnerAccount>().AsNoTracking() on n.PartnerAccountId equals pa.Id
                          where n.CompanyId == companyId && n.NoticeBatchId == batchId && n.IsActive
                          orderby pa.AccountNumber
                          select new { n.Id, pa.PartnerId, pa.Partner.Name }).ToArrayAsync(ct);
        var result = new List<Recipient>(rows.Length);
        foreach (var row in rows)
            result.Add(new Recipient(row.Id, row.Name, await InvoicePdfService.FindRecipientEmailAsync(db, row.PartnerId, ct)));
        return result;
    }

    private async Task<List<int>> NoticeIdsAsync(int companyId, int batchId, CancellationToken ct) =>
        await db.Set<Notice>().AsNoTracking()
            .Where(x => x.CompanyId == companyId && x.NoticeBatchId == batchId && x.IsActive)
            .OrderBy(x => x.Id).Select(x => x.Id).ToListAsync(ct);

    private async Task<Address?> DefaultAddressAsync(int partnerId, CancellationToken ct) =>
        await db.Set<PartnerAddress>().AsNoTracking()
            .Where(x => x.PartnerId == partnerId)
            .OrderByDescending(x => x.IsDefault).ThenBy(x => x.Id)
            .Select(x => x.Address).FirstOrDefaultAsync(ct);

    // Legacy template OpomenaObavestenjeEmail001* does not exist in the data (9.5) -- per-company
    // Settings keys with a neutral default, same pattern as invoice emails.
    private async Task<(string Subject, string Body)> LoadTemplateAsync(int companyId, CancellationToken ct)
    {
        var settings = await db.Set<Setting>().AsNoTracking()
            .Where(x => x.CompanyId == companyId && (x.Key == "NoticeEmailSubject" || x.Key == "NoticeEmailBody"))
            .ToDictionaryAsync(x => x.Key, x => x.Value, ct);
        var subject = settings.GetValueOrDefault("NoticeEmailSubject");
        var body = settings.GetValueOrDefault("NoticeEmailBody");
        return (string.IsNullOrWhiteSpace(subject) ? DefaultSubject : subject, string.IsNullOrWhiteSpace(body) ? DefaultBody : body);
    }

    private static string ApplyTokens(string template, NoticeDocumentData data) => NoticeTemplateText.Apply(template, data)
        .Replace("{NoticeNumber}", data.NoticeNumber)
        .Replace("{CustomerName}", data.CustomerName)
        .Replace("{Debt}", NoticeTemplateText.Money(data.Debt))
        .Replace("{Total}", NoticeTemplateText.Money(data.Total));

    private static string FileName(NoticeDocumentData data) => $"{data.AccountNumber}-{data.Date:yyyy-MM-dd}.pdf";

    private int CurrentStaffId() =>
        int.TryParse(httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : throw new DomainRuleException("security.staff-id-missing", "Identitet zaposlenog nije dostupan.");
}
