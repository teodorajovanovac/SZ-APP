using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SzApp.Contracts.Platform;
using SzApp.Data;
using SzApp.Data.Entities;

namespace SzApp.Api.Features.Platform;

internal static class DocumentEndpoints
{
    public static RouteGroupBuilder MapDocumentEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/documents", ListAsync);
        group.MapGet("/documents/{documentId:long}", DownloadAsync);
        group.MapPost("/documents", UploadAsync).DisableAntiforgery();
        group.MapDelete("/documents/{documentId:long}", DeleteAsync);
        return group;
    }

    private static async Task<IResult> ListAsync(int companyId, SzAppDbContext db, CancellationToken ct)
    {
        var items = await db.Set<DocumentRecord>().AsNoTracking().Where(x => x.CompanyId == companyId)
            .OrderByDescending(x => x.CreatedAt).Take(200)
            .Select(x => new DocumentResponse(x.Id, x.CompanyId, x.FileName, x.ContentType, x.Size, x.Sha256, x.CreatedAt, x.Description, Convert.ToBase64String(x.RowVersion)))
            .ToArrayAsync(ct);
        return Results.Ok(items);
    }

    private static async Task<IResult> DownloadAsync(int companyId, long documentId, SzAppDbContext db, IPlatformDocumentStorage storage, CancellationToken ct)
    {
        var item = await db.Set<DocumentRecord>().AsNoTracking().SingleOrDefaultAsync(x => x.Id == documentId && x.CompanyId == companyId, ct);
        if (item is null) return Results.NotFound();
        try { return Results.File(await storage.OpenReadAsync(item.RelativePath, ct), item.ContentType, item.FileName, enableRangeProcessing: true); }
        catch (FileNotFoundException) { return Results.NotFound(); }
    }

    private static async Task<IResult> UploadAsync(
        int companyId,
        HttpContext context,
        ClaimsPrincipal principal,
        SzAppDbContext db,
        IPlatformDocumentStorage storage,
        TimeProvider timeProvider,
        CancellationToken ct)
    {
        if (!await PlatformEndpointHelpers.CanWriteAsync(principal, companyId, db, ct)) return Results.Forbid();
        var form = await context.Request.ReadFormAsync(ct);
        var file = form.Files.GetFile("file");
        if (file is null) return Results.ValidationProblem(new Dictionary<string, string[]> { ["file"] = ["Fajl je obavezan."] });
        (string RelativePath, string Sha256) saved;
        try
        {
            await using var input = file.OpenReadStream();
            saved = await storage.SaveAsync(companyId, file.FileName, file.ContentType, file.Length, input, ct);
        }
        catch (InvalidOperationException exception) { return Results.ValidationProblem(new Dictionary<string, string[]> { ["file"] = [exception.Message] }); }

        var item = new DocumentRecord
        {
            CompanyId = companyId,
            OwnerStaffId = PlatformEndpointHelpers.StaffId(principal),
            FileName = Path.GetFileName(file.FileName),
            RelativePath = saved.RelativePath,
            ContentType = file.ContentType,
            Size = file.Length,
            Sha256 = saved.Sha256,
            CreatedAt = timeProvider.GetUtcNow(),
            Description = form["description"].FirstOrDefault()?.Trim(),
            SourceTable = form["sourceTable"].FirstOrDefault()?.Trim(),
            ReferenceId = int.TryParse(form["referenceId"], out var referenceId) ? referenceId : null
        };
        db.Set<DocumentRecord>().Add(item);
        try { await db.SaveChangesAsync(ct); }
        catch { await storage.DeleteAsync(saved.RelativePath, ct); throw; }
        context.Response.Headers.ETag = $"\"{Convert.ToBase64String(item.RowVersion)}\"";
        return Results.Created($"/api/v1/companies/{companyId}/documents/{item.Id}", new DocumentResponse(item.Id, companyId, item.FileName, item.ContentType, item.Size, item.Sha256, item.CreatedAt, item.Description, Convert.ToBase64String(item.RowVersion)));
    }

    private static async Task<IResult> DeleteAsync(int companyId, long documentId, HttpContext context, ClaimsPrincipal principal, SzAppDbContext db, IPlatformDocumentStorage storage, CancellationToken ct)
    {
        if (!await PlatformEndpointHelpers.CanWriteAsync(principal, companyId, db, ct)) return Results.Forbid();
        var item = await db.Set<DocumentRecord>().SingleOrDefaultAsync(x => x.Id == documentId && x.CompanyId == companyId, ct);
        if (item is null) return Results.NotFound();
        if (await db.Set<SentEmailAttachment>().AnyAsync(x => x.DocumentId == documentId, ct)) return Results.Conflict(new { message = "Dokument je email prilog i ne može se obrisati." });
        db.Remove(item); await db.SaveChangesAsync(ct); await storage.DeleteAsync(item.RelativePath, ct);
        return Results.NoContent();
    }
}
