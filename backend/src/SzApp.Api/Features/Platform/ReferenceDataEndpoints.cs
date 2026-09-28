using Microsoft.EntityFrameworkCore;
using SzApp.Data;
using SzApp.Data.Entities;

namespace SzApp.Api.Features.Platform;

internal static class ReferenceDataEndpoints
{
    public sealed record ExchangeRateRequest(decimal Rate, DateOnly RateDateFrom);
    public sealed record FiscalYearRequest(int Year, DateOnly StartDate, DateOnly EndDate, bool IsArchived, string? Display, string? Folder, string? FileName, bool IsCurrent);
    public sealed record PartnerAddressRequest(int PartnerId, int AddressId, int AddressTypeId, bool IsDefault);
    public sealed record PartnerCommunicationRequest(int PartnerId, int ChannelId, string Value, string? Note, bool IsActive, bool IsPrimary, int SortIndex, bool IsRegisteredForInvoiceReceipt);

    public static RouteGroupBuilder MapReferenceDataEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/exchange-rates", async (SzAppDbContext db, CancellationToken ct) => Results.Ok(await db.Set<ExchangeRate>().AsNoTracking().OrderByDescending(x => x.RateDateFrom).Take(500).ToArrayAsync(ct)));
        group.MapGet("/location-categories", async (SzAppDbContext db, CancellationToken ct) => Results.Ok(await db.Set<LocationCategory>().AsNoTracking()
            .OrderBy(x => x.SortIndex).ThenBy(x => x.Name)
            .Select(x => new SzApp.Contracts.MasterData.LocationCategoryResponse(x.Id, x.Name, x.ParentId, x.SortIndex))
            .Take(500).ToArrayAsync(ct)));
        // LocationCategory is a global catalogue (no CompanyId); the company route is only the
        // access envelope, so mutations are Root-only.
        group.MapPost("/location-categories", SaveLocationCategoryAsync).RequireAuthorization(p => p.RequireRole(Security.SecurityConstants.RootRole));
        group.MapPut("/location-categories/{id:int}", SaveLocationCategoryAsync).RequireAuthorization(p => p.RequireRole(Security.SecurityConstants.RootRole));
        group.MapDelete("/location-categories/{id:int}", DeleteLocationCategoryAsync).RequireAuthorization(p => p.RequireRole(Security.SecurityConstants.RootRole));
        group.MapPost("/exchange-rates", async (ExchangeRateRequest r, SzAppDbContext db, TimeProvider clock, CancellationToken ct) => { if (r.Rate <= 0) return Results.ValidationProblem(new Dictionary<string, string[]> { ["rate"] = ["Kurs mora biti pozitivan."] }); var item = new ExchangeRate { Rate = r.Rate, RateDateFrom = r.RateDateFrom, UpdatedAt = clock.GetUtcNow() }; db.Add(item); await db.SaveChangesAsync(ct); return Results.Created($"exchange-rates/{item.Id}", item); }).RequireAuthorization(p => p.RequireRole(Security.SecurityConstants.RootRole, Security.SecurityConstants.UpravnikRole));
        group.MapGet("/fiscal-years", async (int companyId, SzAppDbContext db, CancellationToken ct) => Results.Ok(await db.Set<FiscalYear>().AsNoTracking().Where(x => x.CompanyId == companyId).OrderByDescending(x => x.Year).ToArrayAsync(ct)));
        group.MapPost("/fiscal-years", async (int companyId, FiscalYearRequest r, SzAppDbContext db, CancellationToken ct) => { if (r.EndDate < r.StartDate) return Results.ValidationProblem(new Dictionary<string, string[]> { ["endDate"] = ["Kraj perioda mora biti posle početka."] }); if (r.IsCurrent) foreach (var other in await db.Set<FiscalYear>().Where(x => x.CompanyId == companyId && x.IsCurrent).ToArrayAsync(ct)) other.IsCurrent = false; var item = new FiscalYear { CompanyId = companyId, Year = r.Year, StartDate = r.StartDate, EndDate = r.EndDate, IsArchived = r.IsArchived, Display = r.Display, Folder = r.Folder, FileName = r.FileName, IsCurrent = r.IsCurrent }; db.Add(item); await db.SaveChangesAsync(ct); return Results.Created($"fiscal-years/{item.Id}", item); }).RequireAuthorization(p => p.RequireRole(Security.SecurityConstants.RootRole, Security.SecurityConstants.UpravnikRole));
        group.MapGet("/partner-addresses", async (int companyId, int? partnerId, SzAppDbContext db, CancellationToken ct) => Results.Ok(await db.Set<PartnerAddress>().AsNoTracking().Where(x => x.Partner.CompanyId == companyId && (partnerId == null || x.PartnerId == partnerId)).Take(500).ToArrayAsync(ct)));
        group.MapPost("/partner-addresses", async (int companyId, PartnerAddressRequest r, SzAppDbContext db, IShortListValidator shortLists, TimeProvider clock, CancellationToken ct) => { if (!await db.Partners.AnyAsync(x => x.Id == r.PartnerId && x.CompanyId == companyId, ct)) return Results.NotFound(); await shortLists.EnsureTypeAsync(r.AddressTypeId, "AddressType", ct); if (r.IsDefault) foreach (var other in await db.Set<PartnerAddress>().Where(x => x.PartnerId == r.PartnerId && x.AddressTypeId == r.AddressTypeId && x.IsDefault).ToArrayAsync(ct)) other.IsDefault = false; var item = new PartnerAddress { PartnerId = r.PartnerId, AddressId = r.AddressId, AddressTypeId = r.AddressTypeId, IsDefault = r.IsDefault, UpdatedAt = clock.GetUtcNow() }; db.Add(item); await db.SaveChangesAsync(ct); return Results.Created($"partner-addresses/{item.Id}", item); }).RequireAuthorization(p => p.RequireRole(Security.SecurityConstants.RootRole, Security.SecurityConstants.UpravnikRole, Security.SecurityConstants.ModeratorRole));
        group.MapGet("/partner-comms", async (int companyId, int? partnerId, SzAppDbContext db, CancellationToken ct) => Results.Ok(await db.Set<PartnerCommunication>().AsNoTracking().Where(x => x.Partner.CompanyId == companyId && (partnerId == null || x.PartnerId == partnerId)).Take(500).ToArrayAsync(ct)));
        group.MapPost("/partner-comms", async (int companyId, PartnerCommunicationRequest r, SzAppDbContext db, IShortListValidator shortLists, CancellationToken ct) => { if (!await db.Partners.AnyAsync(x => x.Id == r.PartnerId && x.CompanyId == companyId, ct)) return Results.NotFound(); await shortLists.EnsureTypeAsync(r.ChannelId, "ChannelComms", ct); var normalized = r.Value.Trim().ToLowerInvariant(); if (r.IsPrimary) foreach (var other in await db.Set<PartnerCommunication>().Where(x => x.PartnerId == r.PartnerId && x.ChannelId == r.ChannelId && x.IsPrimary).ToArrayAsync(ct)) other.IsPrimary = false; var item = new PartnerCommunication { PartnerId = r.PartnerId, ChannelId = r.ChannelId, ValueNormalized = normalized, Note = r.Note, IsActive = r.IsActive, IsPrimary = r.IsPrimary, SortIndex = r.SortIndex, IsRegisteredForInvoiceReceipt = r.IsRegisteredForInvoiceReceipt }; db.Add(item); await db.SaveChangesAsync(ct); return Results.Created($"partner-comms/{item.Id}", item); }).RequireAuthorization(p => p.RequireRole(Security.SecurityConstants.RootRole, Security.SecurityConstants.UpravnikRole, Security.SecurityConstants.ModeratorRole));
        return group;
    }

    public sealed record LocationCategoryRequest(string Name, int? ParentId, int SortIndex);

    private static async Task<IResult> SaveLocationCategoryAsync(int? id, LocationCategoryRequest r, SzAppDbContext db, CancellationToken ct)
    {
        var name = r.Name?.Trim() ?? string.Empty;
        if (name.Length is 0 or > 255)
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["name"] = ["Naziv je obavezan (najviše 255 znakova)."] });
        var item = id is null ? new LocationCategory() : await db.Set<LocationCategory>().SingleOrDefaultAsync(x => x.Id == id, ct);
        if (item is null) return Results.NotFound();
        if (r.ParentId is not null)
        {
            // Walk up from the proposed parent: reaching this item means a cycle.
            var parentId = r.ParentId;
            for (var depth = 0; parentId is not null; depth++)
            {
                if (parentId == id || depth > 100)
                    return Results.ValidationProblem(new Dictionary<string, string[]> { ["parentId"] = ["Nadređena lokacija ne može biti sama lokacija ni njena podlokacija."] });
                var parent = await db.Set<LocationCategory>().AsNoTracking().Where(x => x.Id == parentId).Select(x => new { x.ParentId }).SingleOrDefaultAsync(ct);
                if (parent is null)
                    return Results.ValidationProblem(new Dictionary<string, string[]> { ["parentId"] = ["Nadređena lokacija ne postoji."] });
                parentId = parent.ParentId;
            }
        }
        item.Name = name;
        item.ParentId = r.ParentId;
        item.SortIndex = r.SortIndex;
        if (id is null) db.Add(item);
        await db.SaveChangesAsync(ct);
        var response = new SzApp.Contracts.MasterData.LocationCategoryResponse(item.Id, item.Name, item.ParentId, item.SortIndex);
        return id is null ? Results.Created($"location-categories/{item.Id}", response) : Results.Ok(response);
    }

    private static async Task<IResult> DeleteLocationCategoryAsync(int id, SzAppDbContext db, CancellationToken ct)
    {
        var item = await db.Set<LocationCategory>().SingleOrDefaultAsync(x => x.Id == id, ct);
        if (item is null) return Results.NotFound();
        if (await db.Set<LocationCategory>().AnyAsync(x => x.ParentId == id, ct) || await db.Companies.AnyAsync(x => x.LocationCategoryId == id, ct))
            return Results.Conflict(new { message = "Lokacija se koristi (podlokacije ili kompanije) i ne može se obrisati." });
        db.Remove(item);
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }
}
