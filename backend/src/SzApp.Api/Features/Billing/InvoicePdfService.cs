using System.IO.Compression;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SzApp.Api.Features.Platform;
using SzApp.Data;
using SzApp.Data.Entities;
using SzApp.Domain.Platform;

namespace SzApp.Api.Features.Billing;

public sealed record InvoicePdfResult(byte[] Bytes, string FileName);
public sealed record EmailPreviewResult(int TotalInvoices, int WithEmail, int MissingEmail, IReadOnlyList<string> MissingCustomerNames);
public sealed record EmailSendResult(int Enqueued, int Skipped);

/// <summary>
/// PDF rendering + bulk email sending for invoices/invoice batches (GAP-02, GAP-03, GAP-26).
/// Reads only -- never touches BillingService.cs's calculation methods (owned by other agents).
/// </summary>
public sealed class InvoicePdfService(
    SzAppDbContext db,
    InvoiceDocumentMapper mapper,
    IPlatformDocumentStorage storage,
    TimeProvider timeProvider)
{
    private const string EmailOutboxMessageType = "platform.email.send";

    public async Task<InvoicePdfResult?> RenderSingleAsync(int companyId, int invoiceId, CancellationToken ct)
    {
        var data = await mapper.BuildAsync(companyId, invoiceId, ct);
        if (data is null) return null;
        var bytes = InvoicePdfRenderer.Render(data);
        return new InvoicePdfResult(bytes, $"{SanitizeFileNamePart(data.InvoiceNumber)}.pdf");
    }

    /// <summary>Zips one PDF per invoice in the batch, path RACUNI/{YYYY}/{MM}/{FolderSZ}/{RBR}.pdf (mirrors legacy layout).</summary>
    public async Task<InvoicePdfResult?> RenderBatchZipAsync(int companyId, int batchId, CancellationToken ct)
    {
        var batch = await db.Set<SzApp.Data.Entities.Billing.InvoiceBatch>().AsNoTracking()
            .SingleOrDefaultAsync(x => x.CompanyId == companyId && x.Id == batchId, ct);
        if (batch is null) return null;
        var company = await db.Set<Company>().AsNoTracking().SingleAsync(x => x.Id == companyId, ct);
        var folderSz = string.IsNullOrWhiteSpace(company.RelativeFolderName) ? company.ShortName : company.RelativeFolderName;

        var invoiceIds = await InvoiceIdsForBatchAsync(companyId, batchId, ct);

        using var zipStream = new MemoryStream();
        using (var zip = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var invoiceId in invoiceIds)
            {
                var data = await mapper.BuildAsync(companyId, invoiceId, ct);
                if (data is null) continue;
                var bytes = InvoicePdfRenderer.Render(data);
                var entryPath = $"RACUNI/{batch.Year:D4}/{batch.Month:D2}/{folderSz}/{SanitizeFileNamePart(data.InvoiceNumber)}.pdf";
                var entry = zip.CreateEntry(entryPath, CompressionLevel.Fastest);
                await using var entryStream = entry.Open();
                await entryStream.WriteAsync(bytes, ct);
            }
        }
        return new InvoicePdfResult(zipStream.ToArray(), $"racuni-{batch.PeriodYYMM}.zip");
    }

    public async Task<EmailPreviewResult> PreviewBatchEmailsAsync(int companyId, int batchId, CancellationToken ct)
    {
        var invoiceIds = await InvoiceIdsForBatchAsync(companyId, batchId, ct);
        var withEmail = 0;
        var missingNames = new List<string>();
        foreach (var invoiceId in invoiceIds)
        {
            var invoice = await db.Invoices.AsNoTracking().SingleAsync(x => x.Id == invoiceId, ct);
            var email = await FindRecipientEmailAsync(invoice.PartnerId, ct);
            if (email is null) missingNames.Add(invoice.PartnerName); else withEmail++;
        }
        return new EmailPreviewResult(invoiceIds.Count, withEmail, missingNames.Count, missingNames);
    }

    /// <summary>Renders each invoice's PDF, stores it as a Document, and enqueues one outbox email per recipient (reusing the existing EmailOutboxMessageHandler/SentEmail/SentEmailAttachment pipeline).</summary>
    public async Task<EmailSendResult> SendBatchEmailsAsync(int companyId, int batchId, int staffId, CancellationToken ct)
    {
        var (subject, bodyTemplate) = await LoadTemplateAsync(companyId, ct);
        var invoiceIds = await InvoiceIdsForBatchAsync(companyId, batchId, ct);
        var enqueued = 0;
        var skipped = 0;

        foreach (var invoiceId in invoiceIds)
        {
            var invoice = await db.Invoices.AsNoTracking().SingleAsync(x => x.Id == invoiceId, ct);
            var email = await FindRecipientEmailAsync(invoice.PartnerId, ct);
            if (email is null) { skipped++; continue; }

            var data = await mapper.BuildAsync(companyId, invoiceId, ct);
            if (data is null) { skipped++; continue; }
            var pdfBytes = InvoicePdfRenderer.Render(data);
            var fileName = $"{SanitizeFileNamePart(data.InvoiceNumber)}.pdf";

            await using var pdfStream = new MemoryStream(pdfBytes);
            var saved = await storage.SaveAsync(companyId, fileName, "application/pdf", pdfBytes.Length, pdfStream, ct);
            var document = new DocumentRecord
            {
                CompanyId = companyId,
                OwnerStaffId = staffId,
                FileName = fileName,
                RelativePath = saved.RelativePath,
                ContentType = "application/pdf",
                Size = pdfBytes.Length,
                Sha256 = saved.Sha256,
                SourceTable = "Invoice",
                ReferenceId = invoiceId,
                CreatedAt = timeProvider.GetUtcNow()
            };
            db.Add(document);

            var sentEmail = new SentEmail
            {
                CompanyId = companyId,
                CreatedByStaffId = staffId,
                Subject = ApplyTokens(subject, data),
                ToAddress = email,
                BodyHtml = ApplyTokens(bodyTemplate, data),
                CreatedAt = timeProvider.GetUtcNow(),
                Status = EmailSendStatus.Queued,
                NextAttemptAt = timeProvider.GetUtcNow()
            };
            sentEmail.Attachments.Add(new SentEmailAttachment { Document = document });
            db.Add(sentEmail);
            await db.SaveChangesAsync(ct); // need sentEmail.Id for the outbox payload/dedupe key

            db.OutboxMessages.Add(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                OccurredAt = timeProvider.GetUtcNow(),
                Type = EmailOutboxMessageType,
                DedupeKey = $"email:{sentEmail.Id}",
                PayloadJson = JsonSerializer.Serialize(new SendEmailOutboxPayload(sentEmail.Id)),
                CorrelationId = Guid.NewGuid().ToString("N")
            });
            await db.SaveChangesAsync(ct);
            enqueued++;
        }

        return new EmailSendResult(enqueued, skipped);
    }

    private async Task<List<int>> InvoiceIdsForBatchAsync(int companyId, int batchId, CancellationToken ct) =>
        await db.Invoices.AsNoTracking()
            .Where(x => x.CompanyId == companyId && !x.IsCancelled && EF.Property<int?>(x, "InvoiceBatchId") == batchId)
            .OrderBy(x => x.SequenceNumber)
            .Select(x => x.Id)
            .ToListAsync(ct);

    // ponytail: no seeded "Email" channel id in ShortList yet, so the email channel is identified
    // heuristically (contains '@') rather than by ChannelId. Upgrade: filter by Channel.Caption
    // once a stable seed value exists for the communication-channel ShortList.
    private async Task<string?> FindRecipientEmailAsync(int partnerId, CancellationToken ct) =>
        await db.Set<PartnerCommunication>().AsNoTracking()
            .Where(x => x.PartnerId == partnerId && x.IsActive && x.IsRegisteredForInvoiceReceipt && x.ValueNormalized.Contains('@'))
            .OrderBy(x => x.SortIndex)
            .Select(x => x.ValueNormalized)
            .FirstOrDefaultAsync(ct);

    private const string DefaultSubject = "Račun {InvoiceNumber}";
    private const string DefaultBody = "Poštovani {CustomerName},<br/>u prilogu je račun {InvoiceNumber} sa rokom plaćanja {DueDate}. Iznos za uplatu: {AmountDue} RSD.";

    private async Task<(string Subject, string Body)> LoadTemplateAsync(int companyId, CancellationToken ct)
    {
        var settings = await db.Set<Setting>().AsNoTracking()
            .Where(x => x.CompanyId == companyId && (x.Key == "InvoiceEmailSubject" || x.Key == "InvoiceEmailBody"))
            .ToDictionaryAsync(x => x.Key, x => x.Value, ct);
        var subject = settings.GetValueOrDefault("InvoiceEmailSubject");
        var body = settings.GetValueOrDefault("InvoiceEmailBody");
        return (string.IsNullOrWhiteSpace(subject) ? DefaultSubject : subject, string.IsNullOrWhiteSpace(body) ? DefaultBody : body);
    }

    private static string ApplyTokens(string template, Domain.Billing.InvoiceDocumentData data) => template
        .Replace("{InvoiceNumber}", data.InvoiceNumber)
        .Replace("{CustomerName}", data.CustomerName)
        .Replace("{DueDate}", data.DueDate.ToString("d.M.yyyy."))
        .Replace("{AmountDue}", data.AmountDue.ToString("N2"));

    private static string SanitizeFileNamePart(string value) =>
        string.Concat(value.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
}
