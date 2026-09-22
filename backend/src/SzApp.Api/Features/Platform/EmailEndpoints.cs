using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SzApp.Contracts.Platform;
using SzApp.Data;
using SzApp.Data.Entities;

namespace SzApp.Api.Features.Platform;

internal static class EmailEndpoints
{
    public static RouteGroupBuilder MapEmailEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/emails", ListAsync);
        group.MapGet("/emails/{emailId:long}", GetAsync);
        group.MapPost("/emails", CreateAsync);
        group.MapPost("/emails/{emailId:long}/send", QueueAsync);
        return group;
    }

    private static async Task<IResult> ListAsync(int companyId, SzAppDbContext db, CancellationToken ct)
    {
        var items = await db.Set<SentEmail>().AsNoTracking().Where(x => x.CompanyId == companyId)
            .OrderByDescending(x => x.CreatedAt).Take(200).Select(x => ToResponse(x)).ToArrayAsync(ct);
        return Results.Ok(items);
    }

    private static async Task<IResult> GetAsync(int companyId, long emailId, SzAppDbContext db, CancellationToken ct)
    {
        var item = await db.Set<SentEmail>().AsNoTracking().Include(x => x.Attachments)
            .SingleOrDefaultAsync(x => x.Id == emailId && x.CompanyId == companyId, ct);
        return item is null ? Results.NotFound() : Results.Ok(ToResponse(item));
    }

    private static async Task<IResult> CreateAsync(int companyId, CreateSentEmailRequest request, ClaimsPrincipal principal, SzAppDbContext db, TimeProvider timeProvider, CancellationToken ct)
    {
        if (!await PlatformEndpointHelpers.CanWriteAsync(principal, companyId, db, ct)) return Results.Forbid();
        if (string.IsNullOrWhiteSpace(request.ToAddress) || string.IsNullOrWhiteSpace(request.Subject))
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["email"] = ["Primalac i naslov su obavezni."] });
        var documentIds = request.DocumentIds?.Distinct().ToArray() ?? [];
        var validDocuments = await db.Set<DocumentRecord>().CountAsync(x => x.CompanyId == companyId && documentIds.Contains(x.Id), ct);
        if (validDocuments != documentIds.Length) return PlatformEndpointHelpers.Unprocessable("Svi prilozi moraju pripadati istoj kompaniji.");
        var item = new SentEmail
        {
            CompanyId = companyId,
            CreatedByStaffId = PlatformEndpointHelpers.StaffId(principal),
            Subject = request.Subject.Trim(),
            ToAddress = request.ToAddress.Trim(),
            Cc = request.Cc?.Trim(),
            Bcc = request.Bcc?.Trim(),
            BodyHtml = request.BodyHtml,
            CreatedAt = timeProvider.GetUtcNow(),
            Status = EmailSendStatus.Draft
        };
        foreach (var documentId in documentIds) item.Attachments.Add(new SentEmailAttachment { DocumentId = documentId });
        db.Add(item); await db.SaveChangesAsync(ct);
        return Results.Created($"/api/v1/companies/{companyId}/emails/{item.Id}", ToResponse(item));
    }

    private static async Task<IResult> QueueAsync(int companyId, long emailId, ClaimsPrincipal principal, SzAppDbContext db, TimeProvider timeProvider, CancellationToken ct)
    {
        if (!await PlatformEndpointHelpers.CanWriteAsync(principal, companyId, db, ct)) return Results.Forbid();
        var item = await db.Set<SentEmail>().SingleOrDefaultAsync(x => x.Id == emailId && x.CompanyId == companyId, ct);
        if (item is null) return Results.NotFound();
        if (item.Status is EmailSendStatus.Sent or EmailSendStatus.Sending) return Results.Conflict(new { message = "Email je već poslat ili se trenutno šalje." });
        var outboxExists = await db.OutboxMessages.AnyAsync(x => x.DedupeKey == $"email:{emailId}" && x.ProcessedAt == null, ct);
        if (!outboxExists)
        {
            db.OutboxMessages.Add(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                OccurredAt = timeProvider.GetUtcNow(),
                Type = SentEmailOutboxHandler.Type,
                DedupeKey = $"email:{emailId}",
                PayloadJson = JsonSerializer.Serialize(new SendEmailOutboxPayload(emailId)),
                CorrelationId = Guid.NewGuid().ToString("N")
            });
        }
        item.Status = EmailSendStatus.Queued; item.SendDescription = null; item.NextAttemptAt = timeProvider.GetUtcNow();
        await db.SaveChangesAsync(ct);
        return Results.Accepted($"/api/v1/companies/{companyId}/emails/{emailId}", ToResponse(item));
    }

    private static SentEmailResponse ToResponse(SentEmail x) => new(
        x.Id, x.Subject, x.ToAddress, x.Cc, x.Bcc, x.BodyHtml, x.CreatedAt, x.SentAt, x.Status.ToString(),
        x.SendDescription, x.AttemptCount, x.Attachments.Select(a => a.DocumentId).ToArray(), Convert.ToBase64String(x.RowVersion));
}
