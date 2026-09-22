using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SzApp.Contracts.Platform;
using SzApp.Data;
using SzApp.Data.Entities;
using SzApp.Domain.Platform;

namespace SzApp.Api.Features.Platform;

internal static class WorkflowEndpoints
{
    private static readonly HashSet<string> BasketTypes = new(StringComparer.OrdinalIgnoreCase) { "invoice", "unit", "partner", "document", "email" };

    public static RouteGroupBuilder MapWorkflowEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/events", ListEventsAsync); group.MapPost("/events", CreateEventAsync);
        group.MapPost("/events/{eventId:long}/approve", ApproveAsync); group.MapPost("/events/{eventId:long}/reject", RejectAsync);
        group.MapPost("/events/{eventId:long}/execute", ExecuteAsync);
        group.MapGet("/selection-basket", ListBasketAsync); group.MapPost("/selection-basket", AddBasketAsync);
        group.MapDelete("/selection-basket/{basketId:guid}", DeleteBasketAsync);
        return group;
    }

    private static async Task<IResult> ListEventsAsync(int companyId, SzAppDbContext db, CancellationToken ct)
    {
        var data = await db.Set<PlatformEvent>().AsNoTracking().Where(x => x.CompanyId == companyId)
            .OrderByDescending(x => x.RequestedAt).Take(200).ToArrayAsync(ct);
        return Results.Ok(data.Select(ToResponse));
    }

    private static async Task<IResult> CreateEventAsync(int companyId, CreatePlatformEventRequest request, ClaimsPrincipal principal, SzAppDbContext db, TimeProvider time, CancellationToken ct)
    {
        var role = await PlatformEndpointHelpers.RoleAsync(principal, companyId, db, ct);
        if (!EventWorkflowPolicy.CanSubmit(role)) return Results.Forbid();
        if (string.IsNullOrWhiteSpace(request.Description) || request.Description.Length > 255)
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["description"] = ["Opis je obavezan i može imati najviše 255 znakova."] });
        if (request.UnitId.HasValue && !await db.Set<Unit>().AnyAsync(x => x.Id == request.UnitId && x.CompanyId == companyId, ct))
            return PlatformEndpointHelpers.Unprocessable("Jedinica pripada drugoj kompaniji ili ne postoji.");
        if (request.ContractId.HasValue && !await db.Set<Contract>().AnyAsync(x => x.Id == request.ContractId && x.CompanyId == companyId, ct))
            return PlatformEndpointHelpers.Unprocessable("Ugovor pripada drugoj kompaniji ili ne postoji.");
        if (request.PartnerId.HasValue && !await db.Partners.AnyAsync(x => x.Id == request.PartnerId && x.CompanyId == companyId, ct))
            return PlatformEndpointHelpers.Unprocessable("Partner pripada drugoj kompaniji ili ne postoji.");
        var item = new PlatformEvent
        {
            CompanyId = companyId,
            RequestedAt = time.GetUtcNow(),
            RequestedByStaffId = PlatformEndpointHelpers.StaffId(principal),
            ContractId = request.ContractId,
            PartnerId = request.PartnerId,
            UnitId = request.UnitId,
            Description = request.Description.Trim(),
            PreviousValue = request.PreviousValue,
            NewValue = request.NewValue,
            FieldsRelated = request.FieldsRelated,
            RequestTypeId = request.RequestTypeId,
            RequestBy = request.RequestBy,
            RequestThrough = request.RequestThrough,
            Status = PlatformEventStatus.Requested
        };
        db.Add(item); await db.SaveChangesAsync(ct);
        return Results.Created($"/api/v1/companies/{companyId}/events/{item.Id}", ToResponse(item));
    }

    private static Task<IResult> ApproveAsync(int companyId, long eventId, DecidePlatformEventRequest request, ClaimsPrincipal principal, SzAppDbContext db, TimeProvider time, CancellationToken ct) =>
        DecideAsync(companyId, eventId, request, principal, db, time, true, ct);

    private static Task<IResult> RejectAsync(int companyId, long eventId, DecidePlatformEventRequest request, ClaimsPrincipal principal, SzAppDbContext db, TimeProvider time, CancellationToken ct) =>
        DecideAsync(companyId, eventId, request, principal, db, time, false, ct);

    private static async Task<IResult> DecideAsync(int companyId, long eventId, DecidePlatformEventRequest request, ClaimsPrincipal principal, SzAppDbContext db, TimeProvider time, bool approve, CancellationToken ct)
    {
        var item = await db.Set<PlatformEvent>().SingleOrDefaultAsync(x => x.Id == eventId && x.CompanyId == companyId, ct);
        if (item is null) return Results.NotFound();
        if (!TryRowVersion(request.RowVersion, out var rowVersion)) return Results.ValidationProblem(new Dictionary<string, string[]> { ["rowVersion"] = ["Neispravan rowversion."] });
        var role = await PlatformEndpointHelpers.RoleAsync(principal, companyId, db, ct); var staffId = PlatformEndpointHelpers.StaffId(principal);
        try
        {
            if (approve) EventWorkflowPolicy.EnsureCanApprove(item.Status, item.RequestedByStaffId, staffId, role, request.EmergencyOverride, request.Reason);
            else EventWorkflowPolicy.EnsureCanReject(item.Status, role);
        }
        catch (UnauthorizedAccessException) { return Results.Forbid(); }
        catch (InvalidOperationException ex) { return PlatformEndpointHelpers.Unprocessable(ex.Message); }
        db.Entry(item).Property(x => x.RowVersion).OriginalValue = rowVersion;
        item.Status = approve ? PlatformEventStatus.Approved : PlatformEventStatus.Rejected; item.DecidedAt = time.GetUtcNow();
        item.DecidedByStaffId = staffId; item.DecisionReason = request.Reason?.Trim(); item.EmergencyOverride = approve && request.EmergencyOverride;
        try { await db.SaveChangesAsync(ct); } catch (DbUpdateConcurrencyException) { return PlatformEndpointHelpers.Conflict(); }
        return Results.Ok(ToResponse(item));
    }

    private static async Task<IResult> ExecuteAsync(int companyId, long eventId, ExecutePlatformEventRequest request, ClaimsPrincipal principal, SzAppDbContext db, TimeProvider time, CancellationToken ct)
    {
        var item = await db.Set<PlatformEvent>().SingleOrDefaultAsync(x => x.Id == eventId && x.CompanyId == companyId, ct);
        if (item is null) return Results.NotFound();
        if (!TryRowVersion(request.RowVersion, out var rowVersion)) return Results.ValidationProblem(new Dictionary<string, string[]> { ["rowVersion"] = ["Neispravan rowversion."] });
        var role = await PlatformEndpointHelpers.RoleAsync(principal, companyId, db, ct);
        try { EventWorkflowPolicy.EnsureCanExecute(item.Status, role); }
        catch (UnauthorizedAccessException) { return Results.Forbid(); }
        catch (InvalidOperationException ex) { return PlatformEndpointHelpers.Unprocessable(ex.Message); }
        db.Entry(item).Property(x => x.RowVersion).OriginalValue = rowVersion;
        item.Status = PlatformEventStatus.Executed; item.ExecutedAt = time.GetUtcNow(); item.ExecutedByStaffId = PlatformEndpointHelpers.StaffId(principal);
        try { await db.SaveChangesAsync(ct); } catch (DbUpdateConcurrencyException) { return PlatformEndpointHelpers.Conflict(); }
        return Results.Ok(ToResponse(item));
    }

    private static async Task<IResult> ListBasketAsync(int companyId, ClaimsPrincipal principal, SzAppDbContext db, TimeProvider time, CancellationToken ct)
    {
        var staffId = PlatformEndpointHelpers.StaffId(principal); var now = time.GetUtcNow();
        var expired = await db.Set<SelectionBasket>().Where(x => x.OwnerStaffId == staffId && x.ExpiresAt <= now).ToArrayAsync(ct);
        if (expired.Length > 0) { db.RemoveRange(expired); await db.SaveChangesAsync(ct); }
        var items = await db.Set<SelectionBasket>().AsNoTracking().Where(x => x.CompanyId == companyId && x.OwnerStaffId == staffId && x.ExpiresAt > now)
            .OrderBy(x => x.CreatedAt).Select(x => new SelectionBasketResponse(x.Id, x.TargetType, x.TargetId, x.CreatedAt, x.ExpiresAt)).ToArrayAsync(ct);
        return Results.Ok(items);
    }

    private static async Task<IResult> AddBasketAsync(int companyId, AddSelectionBasketRequest request, ClaimsPrincipal principal, SzAppDbContext db, TimeProvider time, CancellationToken ct)
    {
        if (!BasketTypes.Contains(request.TargetType) || string.IsNullOrWhiteSpace(request.TargetId)) return Results.ValidationProblem(new Dictionary<string, string[]> { ["target"] = ["Tip ili identifikator cilja nije dozvoljen."] });
        var now = time.GetUtcNow(); var staffId = PlatformEndpointHelpers.StaffId(principal);
        var item = new SelectionBasket { Id = Guid.NewGuid(), CompanyId = companyId, OwnerStaffId = staffId, TargetType = request.TargetType.ToLowerInvariant(), TargetId = request.TargetId.Trim(), CreatedAt = now, ExpiresAt = now.AddMinutes(Math.Clamp(request.ExpiresInMinutes, 5, 1440)) };
        db.Add(item); try { await db.SaveChangesAsync(ct); } catch (DbUpdateException) { return Results.Conflict(new { message = "Cilj je već u korpi." }); }
        return Results.Created($"/api/v1/companies/{companyId}/selection-basket/{item.Id}", new SelectionBasketResponse(item.Id, item.TargetType, item.TargetId, item.CreatedAt, item.ExpiresAt));
    }

    private static async Task<IResult> DeleteBasketAsync(int companyId, Guid basketId, ClaimsPrincipal principal, SzAppDbContext db, CancellationToken ct)
    {
        var staffId = PlatformEndpointHelpers.StaffId(principal);
        var item = await db.Set<SelectionBasket>().SingleOrDefaultAsync(x => x.Id == basketId && x.CompanyId == companyId && x.OwnerStaffId == staffId, ct);
        if (item is null) return Results.NotFound(); db.Remove(item); await db.SaveChangesAsync(ct); return Results.NoContent();
    }

    private static PlatformEventResponse ToResponse(PlatformEvent x) => new(x.Id, x.CompanyId, x.RequestedAt, x.DecidedAt, x.ExecutedAt, x.RequestedByStaffId, x.DecidedByStaffId, x.ExecutedByStaffId, x.ContractId, x.PartnerId, x.UnitId, x.Description, x.PreviousValue, x.NewValue, x.FieldsRelated, x.RequestTypeId, x.RequestBy, x.RequestThrough, x.Status.ToString(), x.DecisionReason, x.EmergencyOverride, Convert.ToBase64String(x.RowVersion));
    private static bool TryRowVersion(string value, out byte[] bytes) { try { bytes = Convert.FromBase64String(value.Trim('"')); return bytes.Length > 0; } catch { bytes = []; return false; } }
}
