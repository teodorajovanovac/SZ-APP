namespace SzApp.Domain.Platform;

public sealed record EmailEnvelope(
    string To,
    string? Cc,
    string? Bcc,
    string Subject,
    string BodyHtml,
    IReadOnlyCollection<EmailAttachmentContent> Attachments);

public sealed record EmailAttachmentContent(string FileName, string ContentType, Stream Content);

public interface IEmailTransport
{
    Task SendAsync(EmailEnvelope message, CancellationToken cancellationToken);
}

public sealed class TestEmailTransport : IEmailTransport
{
    private readonly List<EmailEnvelope> _messages = [];
    public IReadOnlyList<EmailEnvelope> Messages => _messages;

    public Task SendAsync(EmailEnvelope message, CancellationToken cancellationToken)
    {
        _messages.Add(message);
        return Task.CompletedTask;
    }
}
