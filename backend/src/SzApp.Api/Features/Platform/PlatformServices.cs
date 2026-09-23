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

// Outbox message payload for "platform.email.send" (enqueued in EmailEndpoints.cs, handled by
// SzApp.Worker's EmailOutboxMessageHandler — the only implementation; this record is shared shape only).
public sealed record SendEmailOutboxPayload(long SentEmailId);
