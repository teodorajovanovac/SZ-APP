using System.Net;
using System.Net.Mail;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SzApp.Data;
using SzApp.Data.Entities;

namespace SzApp.Worker;

public sealed class EmailOutboxMessageHandler(
    IDbContextFactory<SzAppDbContext> dbFactory,
    IConfiguration configuration,
    TimeProvider clock) : IOutboxMessageHandler
{
    public string MessageType => "platform.email.send";

    public async Task HandleAsync(string payloadJson, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Deserialize<Payload>(payloadJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidOperationException("Neispravan email payload.");
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        var email = await db.Set<SentEmail>().Include(x => x.Attachments).ThenInclude(x => x.Document)
            .SingleAsync(x => x.Id == payload.SentEmailId, cancellationToken);
        if (email.Status == EmailSendStatus.Sent) return;

        var host = configuration["Email:Smtp:Host"];
        var from = configuration["Email:From"];
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(from))
            throw new InvalidOperationException("SMTP host i Email:From moraju biti podešeni kroz tajne okruženja.");

        using var message = new MailMessage { From = new MailAddress(from), Subject = email.Subject, Body = email.BodyHtml, IsBodyHtml = true };
        message.To.Add(email.ToAddress);
        AddAddresses(message.CC, email.Cc); AddAddresses(message.Bcc, email.Bcc);
        var root = Path.GetFullPath(configuration["Platform:Storage:RootPath"] ?? Path.Combine(AppContext.BaseDirectory, "storage", "documents"));
        foreach (var link in email.Attachments)
        {
            if (link.Document.CompanyId != email.CompanyId) throw new InvalidOperationException("Prilog pripada drugoj kompaniji.");
            var path = Path.GetFullPath(Path.Combine(root, link.Document.RelativePath));
            if (!path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Putanja priloga nije dozvoljena.");
            message.Attachments.Add(new Attachment(path, link.Document.ContentType));
        }

        using var smtp = new SmtpClient(host, configuration.GetValue("Email:Smtp:Port", 587))
        {
            EnableSsl = configuration.GetValue("Email:Smtp:UseTls", true)
        };
        var user = configuration["Email:Smtp:Username"]; var password = configuration["Email:Smtp:Password"];
        if (!string.IsNullOrWhiteSpace(user)) smtp.Credentials = new NetworkCredential(user, password);
        email.Status = EmailSendStatus.Sending; await db.SaveChangesAsync(cancellationToken);
        await smtp.SendMailAsync(message, cancellationToken);
        email.Status = EmailSendStatus.Sent; email.SentAt = clock.GetUtcNow(); email.SendDescription = null; email.NextAttemptAt = null;
        await db.SaveChangesAsync(cancellationToken);
    }

    private static void AddAddresses(MailAddressCollection target, string? values)
    {
        foreach (var value in (values ?? string.Empty).Split([';', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)) target.Add(value);
    }

    private sealed record Payload(long SentEmailId);
}
