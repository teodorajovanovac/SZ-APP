using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SzApp.Data;
using SzApp.Data.Entities;
using SzApp.Domain.Platform;

namespace SzApp.Api.Features.Platform;

public sealed class PlatformStorageOptions
{
    public string RootPath { get; set; } = Path.Combine(AppContext.BaseDirectory, "storage", "documents");
}

public interface IPlatformDocumentStorage
{
    Task<(string RelativePath, string Sha256)> SaveAsync(int companyId, string fileName, string contentType, long length, Stream content, CancellationToken cancellationToken);
    Task<Stream> OpenReadAsync(string relativePath, CancellationToken cancellationToken);
    Task DeleteAsync(string relativePath, CancellationToken cancellationToken);
}

public sealed class LocalDocumentStorage(PlatformStorageOptions options, TimeProvider timeProvider) : IPlatformDocumentStorage
{
    public async Task<(string RelativePath, string Sha256)> SaveAsync(
        int companyId, string fileName, string contentType, long length, Stream content, CancellationToken cancellationToken)
    {
        var validated = DocumentStoragePolicy.Validate(companyId, fileName, contentType, length, timeProvider.GetUtcNow(), Guid.NewGuid());
        var fullPath = DocumentStoragePolicy.ResolveUnderRoot(options.RootPath, validated.RelativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        try
        {
            await using (var output = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, FileOptions.Asynchronous))
                await content.CopyToAsync(output, cancellationToken);
            await using var saved = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, FileOptions.Asynchronous);
            var checksum = await DocumentStoragePolicy.ComputeSha256Async(saved, cancellationToken);
            return (validated.RelativePath, checksum);
        }
        catch
        {
            if (File.Exists(fullPath)) File.Delete(fullPath);
            throw;
        }
    }

    public Task<Stream> OpenReadAsync(string relativePath, CancellationToken cancellationToken)
    {
        var fullPath = DocumentStoragePolicy.ResolveUnderRoot(options.RootPath, relativePath);
        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, FileOptions.Asynchronous);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string relativePath, CancellationToken cancellationToken)
    {
        var fullPath = DocumentStoragePolicy.ResolveUnderRoot(options.RootPath, relativePath);
        if (File.Exists(fullPath)) File.Delete(fullPath);
        return Task.CompletedTask;
    }
}

public sealed class DisabledEmailTransport : IEmailTransport
{
    public Task SendAsync(EmailEnvelope message, CancellationToken cancellationToken) =>
        throw new InvalidOperationException("Email transport nije konfigurisan. SMTP/Gmail adapter mora biti registrovan u deployment-u.");
}

public interface IPlatformOutboxHandler
{
    string MessageType { get; }
    Task HandleAsync(string payloadJson, CancellationToken cancellationToken);
}

public sealed record SendEmailOutboxPayload(long SentEmailId);

public sealed class SentEmailOutboxHandler(
    SzAppDbContext db,
    IPlatformDocumentStorage storage,
    IEmailTransport transport,
    TimeProvider timeProvider) : IPlatformOutboxHandler
{
    public const string Type = "platform.email.send";
    public string MessageType => Type;

    public async Task HandleAsync(string payloadJson, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Deserialize<SendEmailOutboxPayload>(payloadJson)
            ?? throw new InvalidOperationException("Neispravan email outbox payload.");
        var email = await db.Set<SentEmail>().Include(x => x.Attachments).ThenInclude(x => x.Document)
            .SingleAsync(x => x.Id == payload.SentEmailId, cancellationToken);
        if (email.Status == EmailSendStatus.Sent) return;
        email.Status = EmailSendStatus.Sending;
        await db.SaveChangesAsync(cancellationToken);

        var opened = new List<Stream>();
        try
        {
            var attachments = new List<EmailAttachmentContent>();
            foreach (var link in email.Attachments)
            {
                if (link.Document.CompanyId != email.CompanyId) throw new InvalidOperationException("Email prilog pripada drugoj kompaniji.");
                var stream = await storage.OpenReadAsync(link.Document.RelativePath, cancellationToken);
                opened.Add(stream);
                attachments.Add(new EmailAttachmentContent(link.Document.FileName, link.Document.ContentType, stream));
            }
            await transport.SendAsync(new EmailEnvelope(email.ToAddress, email.Cc, email.Bcc, email.Subject, email.BodyHtml, attachments), cancellationToken);
            email.Status = EmailSendStatus.Sent;
            email.SentAt = timeProvider.GetUtcNow();
            email.SendDescription = null;
            email.NextAttemptAt = null;
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            email.Status = EmailSendStatus.Failed;
            email.AttemptCount++;
            email.SendDescription = exception.Message[..Math.Min(exception.Message.Length, 2000)];
            var delayMinutes = Math.Min(60, Math.Pow(2, Math.Min(email.AttemptCount, 6)));
            email.NextAttemptAt = timeProvider.GetUtcNow().AddMinutes(delayMinutes);
            await db.SaveChangesAsync(cancellationToken);
            throw;
        }
        finally
        {
            foreach (var stream in opened) await stream.DisposeAsync();
        }
    }
}
