using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SzApp.Api.Security;
using SzApp.Contracts.Platform;
using SzApp.Data;
using SzApp.Data.Entities;
using SzApp.Domain.Platform;

namespace SzApp.Api.Features.Platform;

internal static class AdministrationEndpoints
{
    private static readonly string[] SecretFragments = ["password", "secret", "token", "credential", "clientsecret", "apikey", "connectionstring"];

    public static RouteGroupBuilder MapAdministrationEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/settings", ListSettingsAsync); group.MapPut("/settings/{key}", PutSettingAsync);
        group.MapGet("/import-definitions", ListImportsAsync); group.MapPost("/import-definitions", CreateImportAsync);
        group.MapPut("/import-definitions/{definitionId:int}", UpdateImportAsync); group.MapDelete("/import-definitions/{definitionId:int}", DeleteImportAsync);
        group.MapPost("/import-definitions/{definitionId:int}/groups", AddGroupAsync);
        group.MapPost("/import-groups/{groupId:int}/mappings", AddMappingAsync);
        group.MapGet("/languages", ListLanguagesAsync); group.MapPut("/languages/{code}", PutLanguageAsync);
        group.MapGet("/translations", ListTranslationsAsync); group.MapPost("/translations", SaveTranslationAsync);
        group.MapGet("/short-lists/{tableName}", ListShortListAsync); group.MapPost("/short-lists", SaveShortListAsync);
        return group;
    }

    private static async Task<IResult> ListSettingsAsync(int companyId, SzAppDbContext db, CancellationToken ct)
    {
        var items = await db.Set<Setting>().AsNoTracking().Where(x => x.CompanyId == companyId || x.CompanyId == null)
            .OrderBy(x => x.Category).ThenBy(x => x.Name)
            .Select(x => new SettingResponse(x.Id, x.CompanyId, x.Name, x.Key, x.Value, x.Description, x.Category, x.ValueMax, Convert.ToBase64String(x.RowVersion))).ToArrayAsync(ct);
        return Results.Ok(items);
    }

    private static async Task<IResult> PutSettingAsync(int companyId, string key, SaveSettingRequest request, ClaimsPrincipal principal, SzAppDbContext db, CancellationToken ct)
    {
        if (!await PlatformEndpointHelpers.CanWriteAsync(principal, companyId, db, ct)) return Results.Forbid();
        var normalized = key.Trim();
        if (normalized.Length is 0 or > 50 || SecretFragments.Any(x => normalized.Contains(x, StringComparison.OrdinalIgnoreCase)))
            return PlatformEndpointHelpers.Unprocessable("Tajni ili neispravan ključ podešavanja nije dozvoljen u bazi.");
        var item = await db.Set<Setting>().SingleOrDefaultAsync(x => x.CompanyId == companyId && x.Key == normalized, ct);
        if (item is null) { item = new Setting { CompanyId = companyId, Key = normalized }; db.Add(item); }
        else if (!TryVersion(request.RowVersion, out var version)) return Results.Problem(statusCode: 428, title: "RowVersion je obavezan za izmenu.");
        else db.Entry(item).Property(x => x.RowVersion).OriginalValue = version;
        item.Name = request.Name.Trim(); item.Value = request.Value?.Trim(); item.Description = request.Description?.Trim(); item.Category = request.Category?.Trim(); item.ValueMax = request.ValueMax;
        try { await db.SaveChangesAsync(ct); } catch (DbUpdateConcurrencyException) { return PlatformEndpointHelpers.Conflict(); }
        return Results.Ok(new SettingResponse(item.Id, item.CompanyId, item.Name, item.Key, item.Value, item.Description, item.Category, item.ValueMax, Convert.ToBase64String(item.RowVersion)));
    }

    private static async Task<IResult> ListImportsAsync(int companyId, SzAppDbContext db, CancellationToken ct)
    {
        var items = await db.Set<ImportDefinition>().AsNoTracking().Where(x => x.CompanyId == companyId).OrderBy(x => x.SortIndex)
            .Select(x => ToResponse(x)).ToArrayAsync(ct); return Results.Ok(items);
    }

    private static async Task<IResult> CreateImportAsync(int companyId, SaveImportDefinitionRequest request, ClaimsPrincipal principal, SzAppDbContext db, IImportAllowlistRegistry allowlist, CancellationToken ct)
    {
        if (!await PlatformEndpointHelpers.CanWriteAsync(principal, companyId, db, ct)) return Results.Forbid();
        var error = ValidateImport(request, allowlist); if (error is not null) return PlatformEndpointHelpers.Unprocessable(error);
        var item = new ImportDefinition { CompanyId = companyId }; Apply(item, request); db.Add(item); await db.SaveChangesAsync(ct);
        return Results.Created($"/api/v1/companies/{companyId}/import-definitions/{item.Id}", ToResponse(item));
    }

    private static async Task<IResult> UpdateImportAsync(int companyId, int definitionId, SaveImportDefinitionRequest request, ClaimsPrincipal principal, SzAppDbContext db, IImportAllowlistRegistry allowlist, CancellationToken ct)
    {
        if (!await PlatformEndpointHelpers.CanWriteAsync(principal, companyId, db, ct)) return Results.Forbid();
        var error = ValidateImport(request, allowlist); if (error is not null) return PlatformEndpointHelpers.Unprocessable(error);
        if (!TryVersion(request.RowVersion, out var version)) return Results.Problem(statusCode: 428, title: "RowVersion je obavezan.");
        var item = await db.Set<ImportDefinition>().SingleOrDefaultAsync(x => x.Id == definitionId && x.CompanyId == companyId, ct); if (item is null) return Results.NotFound();
        db.Entry(item).Property(x => x.RowVersion).OriginalValue = version; Apply(item, request);
        try { await db.SaveChangesAsync(ct); } catch (DbUpdateConcurrencyException) { return PlatformEndpointHelpers.Conflict(); }
        return Results.Ok(ToResponse(item));
    }

    private static async Task<IResult> DeleteImportAsync(int companyId, int definitionId, ClaimsPrincipal principal, SzAppDbContext db, CancellationToken ct)
    {
        if (!await PlatformEndpointHelpers.CanWriteAsync(principal, companyId, db, ct)) return Results.Forbid();
        var item = await db.Set<ImportDefinition>().Include(x => x.Groups).ThenInclude(x => x.Mappings).SingleOrDefaultAsync(x => x.Id == definitionId && x.CompanyId == companyId, ct);
        if (item is null) return Results.NotFound(); db.RemoveRange(item.Groups.SelectMany(x => x.Mappings)); db.RemoveRange(item.Groups); db.Remove(item); await db.SaveChangesAsync(ct); return Results.NoContent();
    }

    private static async Task<IResult> AddGroupAsync(int companyId, int definitionId, SaveImportMappingGroupRequest request, ClaimsPrincipal principal, SzAppDbContext db, IImportAllowlistRegistry allowlist, CancellationToken ct)
    {
        if (!await PlatformEndpointHelpers.CanWriteAsync(principal, companyId, db, ct)) return Results.Forbid();
        var definition = await db.Set<ImportDefinition>().SingleOrDefaultAsync(x => x.Id == definitionId && x.CompanyId == companyId, ct); if (definition is null) return Results.NotFound();
        if (!allowlist.IsAllowedTarget(request.TargetTable, request.TargetTable == "Partner" ? "Id" : request.TargetTable == "Address" ? "Address" : "StatementNumber"))
            return PlatformEndpointHelpers.Unprocessable("Ciljna tabela nije na import allowlist-i.");
        var item = new ImportMappingGroup { ImportDefinitionId = definitionId, Name = request.Name.Trim(), TargetTable = request.TargetTable.Trim(), SourcePath = request.SourcePath.Trim(), IsRepeating = request.IsRepeating };
        db.Add(item); await db.SaveChangesAsync(ct); return Results.Created($"/api/v1/companies/{companyId}/import-groups/{item.Id}", new ImportMappingGroupResponse(item.Id, definitionId, item.Name, item.TargetTable, item.SourcePath, item.IsRepeating));
    }

    private static async Task<IResult> AddMappingAsync(int companyId, int groupId, SaveImportMappingRequest request, ClaimsPrincipal principal, SzAppDbContext db, IImportAllowlistRegistry allowlist, CancellationToken ct)
    {
        if (!await PlatformEndpointHelpers.CanWriteAsync(principal, companyId, db, ct)) return Results.Forbid();
        var group = await db.Set<ImportMappingGroup>().Include(x => x.Definition).SingleOrDefaultAsync(x => x.Id == groupId && x.Definition.CompanyId == companyId, ct); if (group is null) return Results.NotFound();
        if (!allowlist.IsAllowedTarget(group.TargetTable, request.TargetField)) return PlatformEndpointHelpers.Unprocessable("Ciljno polje nije na import allowlist-i.");
        if (request.IsLookup && (request.LookupTable is null || request.LookupField is null || request.LookupValueField is null || !allowlist.IsAllowedLookup(request.LookupTable, request.LookupField, request.LookupValueField)))
            return PlatformEndpointHelpers.Unprocessable("Lookup nije na import allowlist-i.");
        var item = new ImportMapping { ImportMappingGroupId = groupId, TargetField = request.TargetField.Trim(), SourcePath = request.SourcePath.Trim(), SourceNode = request.SourceNode, MappingTypeId = request.MappingTypeId, DataTypeId = request.DataTypeId, DefaultValue = request.DefaultValue, IsRequired = request.IsRequired, IsKey = request.IsKey, IsLookup = request.IsLookup, LookupTable = request.LookupTable, LookupField = request.LookupField, LookupValueField = request.LookupValueField, Format = request.Format, SortIndex = request.SortIndex, Description = request.Description };
        db.Add(item); await db.SaveChangesAsync(ct); return Results.Created($"/api/v1/companies/{companyId}/import-mappings/{item.Id}", new ImportMappingResponse(item.Id, groupId, item.TargetField, item.SourcePath, item.SourceNode, item.MappingTypeId, item.DataTypeId, item.DefaultValue, item.IsRequired, item.IsKey, item.IsLookup, item.LookupTable, item.LookupField, item.LookupValueField, item.Format, item.SortIndex, item.Description));
    }

    private static async Task<IResult> ListLanguagesAsync(int companyId, SzAppDbContext db, CancellationToken ct) => Results.Ok(await db.Set<Language>().AsNoTracking().OrderBy(x => x.SortIndex).Select(x => new LanguageResponse(x.Code, x.Name, x.IsActive, x.IsDefault, x.SortIndex)).ToArrayAsync(ct));
    private static async Task<IResult> PutLanguageAsync(int companyId, string code, SaveLanguageRequest request, ClaimsPrincipal principal, SzAppDbContext db, CancellationToken ct)
    {
        if (!principal.IsInRole(SecurityConstants.RootRole)) return Results.Forbid(); if (code.Length is < 2 or > 10) return Results.ValidationProblem(new Dictionary<string, string[]> { ["code"] = ["Kod jezika nije ispravan."] });
        var item = await db.Set<Language>().SingleOrDefaultAsync(x => x.Code == code, ct) ?? new Language { Code = code };
        if (db.Entry(item).State == EntityState.Detached) db.Add(item); if (request.IsDefault) { var defaults = await db.Set<Language>().Where(x => x.IsDefault && x.Code != code).ToArrayAsync(ct); foreach (var other in defaults) other.IsDefault = false; }
        item.Name = request.Name; item.IsActive = request.IsActive; item.IsDefault = request.IsDefault; item.SortIndex = request.SortIndex; await db.SaveChangesAsync(ct); return Results.Ok(new LanguageResponse(item.Code, item.Name, item.IsActive, item.IsDefault, item.SortIndex));
    }

    private static async Task<IResult> ListTranslationsAsync(int companyId, SzAppDbContext db, CancellationToken ct) => Results.Ok(await db.Set<Translation>().AsNoTracking().Where(x => x.CompanyId == companyId || x.CompanyId == null).OrderBy(x => x.ResourceKey).Select(x => new TranslationResponse(x.Id, x.LanguageCode, x.ResourceKey, x.ResourceId, x.CompanyId, x.Value)).ToArrayAsync(ct));
    private static async Task<IResult> SaveTranslationAsync(int companyId, SaveTranslationRequest request, ClaimsPrincipal principal, SzAppDbContext db, CancellationToken ct)
    {
        if (!await PlatformEndpointHelpers.CanWriteAsync(principal, companyId, db, ct)) return Results.Forbid(); if (!await db.Set<Language>().AnyAsync(x => x.Code == request.LanguageCode && x.IsActive, ct)) return PlatformEndpointHelpers.Unprocessable("Jezik nije aktivan.");
        var item = await db.Set<Translation>().SingleOrDefaultAsync(x => x.CompanyId == companyId && x.LanguageCode == request.LanguageCode && x.ResourceKey == request.ResourceKey && x.ResourceId == request.ResourceId, ct) ?? new Translation { CompanyId = companyId, LanguageCode = request.LanguageCode, ResourceKey = request.ResourceKey, ResourceId = request.ResourceId };
        if (db.Entry(item).State == EntityState.Detached) db.Add(item); item.Value = request.Value; await db.SaveChangesAsync(ct); return Results.Ok(new TranslationResponse(item.Id, item.LanguageCode, item.ResourceKey, item.ResourceId, item.CompanyId, item.Value));
    }

    private static async Task<IResult> ListShortListAsync(int companyId, string tableName, SzAppDbContext db, CancellationToken ct) => Results.Ok(await db.ShortLists.AsNoTracking().Where(x => x.TableName == tableName).OrderBy(x => x.IndexSort).Select(x => new ShortListResponse(x.Id, x.TableName, x.Caption, x.ShortName, x.Description, x.IndexValue, x.IndexSort, x.IndexKey)).ToArrayAsync(ct));
    private static async Task<IResult> SaveShortListAsync(int companyId, SaveShortListRequest request, ClaimsPrincipal principal, SzAppDbContext db, CancellationToken ct)
    {
        if (!principal.IsInRole(SecurityConstants.RootRole)) return Results.Forbid(); if (string.IsNullOrWhiteSpace(request.TableName) || string.IsNullOrWhiteSpace(request.Caption)) return Results.ValidationProblem(new Dictionary<string, string[]> { ["shortList"] = ["TableName i Caption su obavezni."] });
        var item = new ShortList { TableName = request.TableName.Trim(), Caption = request.Caption.Trim(), ShortName = request.ShortName, Description = request.Description, IndexValue = request.IndexValue, IndexSort = request.IndexSort, IndexKey = request.IndexKey }; db.Add(item); await db.SaveChangesAsync(ct); return Results.Created($"/api/v1/companies/{companyId}/short-lists/{item.TableName}", new ShortListResponse(item.Id, item.TableName, item.Caption, item.ShortName, item.Description, item.IndexValue, item.IndexSort, item.IndexKey));
    }

    private static ImportDefinitionResponse ToResponse(ImportDefinition x) => new(x.Id, x.Name, x.Code, x.FileMask, x.ImportSourceId, x.FilePath, x.TargetHeaderTable, x.TargetLineTable, x.IsActive, x.SortIndex, Convert.ToBase64String(x.RowVersion));
    private static void Apply(ImportDefinition x, SaveImportDefinitionRequest r) { x.Name = r.Name.Trim(); x.Code = r.Code.Trim(); x.FileMask = r.FileMask.Trim(); x.ImportSourceId = r.ImportSourceId; x.FilePath = r.FilePath?.Trim(); x.TargetHeaderTable = r.TargetHeaderTable.Trim(); x.TargetLineTable = r.TargetLineTable?.Trim(); x.IsActive = r.IsActive; x.SortIndex = r.SortIndex; }
    private static string? ValidateImport(SaveImportDefinitionRequest r, IImportAllowlistRegistry allowlist) { if (string.IsNullOrWhiteSpace(r.Name) || string.IsNullOrWhiteSpace(r.Code) || string.IsNullOrWhiteSpace(r.FileMask)) return "Naziv, kod i maska fajla su obavezni."; if (!allowlist.IsAllowedTarget(r.TargetHeaderTable, r.TargetHeaderTable == "Partner" ? "Name" : r.TargetHeaderTable == "Address" ? "Address" : "StatementNumber")) return "Ciljna tabela nije na allowlist-i."; if (r.FilePath?.Contains("..", StringComparison.Ordinal) == true) return "Import putanja nije dozvoljena."; return null; }
    private static bool TryVersion(string? value, out byte[] version) { try { version = Convert.FromBase64String(value?.Trim('"') ?? ""); return version.Length > 0; } catch { version = []; return false; } }
}
