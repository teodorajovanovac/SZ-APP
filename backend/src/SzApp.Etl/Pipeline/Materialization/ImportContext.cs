using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SzApp.Data;
using SzApp.Data.Entities;
using SzApp.Data.Entities.Billing;
using SzApp.Data.Entities.LedgerBanking;
using SzApp.Etl.Parsing;

namespace SzApp.Etl.Pipeline.Materialization;

/// <summary>
/// Per-run lookup state: legacy key -> target key maps (ETL-04), ShortList codes, natural-key code
/// lists, and a few lazily loaded fallbacks. Everything is preloaded once per table so binding a
/// 40k-row GL file doesn't do one query per FK (ETL-09).
/// </summary>
public sealed class ImportContext(SzAppDbContext db)
{
    public const string RootRoleName = "Root";

    private readonly Dictionary<string, Dictionary<string, string>> maps = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, HashSet<string>> naturalKeys = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Dictionary<int, int>> companyOf = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<int, int> noticeTemplates = [];
    private Dictionary<int, (string Table, int Value)> shortListById = [];
    private Dictionary<(string Table, int Value), int> shortListByValue = [];
    private bool shortListsLoaded;
    private int? fallbackStaffId;
    private bool fallbackStaffLoaded;

    public SzAppDbContext Db => db;

    public async Task PrepareAsync(TableSpec spec, CancellationToken ct)
    {
        foreach (var target in spec.ForeignKeys.Select(x => x.TargetTable).Append(spec.Table).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            await LoadMapAsync(target, ct);
            if (target.Equals("ShortList", StringComparison.OrdinalIgnoreCase))
            {
                await LoadShortListsAsync(ct);
            }
            else if (target is "ChartAccount" or "SubAccount")
            {
                naturalKeys[target] = target == "ChartAccount"
                    ? await db.Set<ChartAccount>().AsNoTracking().Select(x => x.Account).ToHashSetAsync(StringComparer.OrdinalIgnoreCase, ct)
                    : await db.Set<SubAccount>().AsNoTracking().Select(x => x.Id).ToHashSetAsync(StringComparer.OrdinalIgnoreCase, ct);
            }
        }
    }

    private async Task LoadMapAsync(string table, CancellationToken ct)
    {
        var rows = await db.LegacyKeyMaps.AsNoTracking()
            .Where(x => x.SourceTable == table && x.TargetTable == table)
            .Select(x => new { x.SourceKey, x.TargetKey })
            .ToArrayAsync(ct);
        var map = new Dictionary<string, string>(rows.Length, StringComparer.OrdinalIgnoreCase);
        foreach (var row in rows) map[row.SourceKey] = row.TargetKey;
        maps[table] = map;
    }

    private async Task LoadShortListsAsync(CancellationToken ct)
    {
        var rows = await db.ShortLists.AsNoTracking().Select(x => new { x.Id, x.TableName, x.IndexValue }).ToArrayAsync(ct);
        shortListById = rows.ToDictionary(x => x.Id, x => (x.TableName, x.IndexValue));
        shortListByValue = rows.GroupBy(x => (x.TableName, x.IndexValue)).ToDictionary(x => x.Key, x => x.First().Id);
        shortListsLoaded = true;
    }

    /// <summary>Records a freshly imported row so later rows (same table or later chunks) resolve it.</summary>
    public void Register(string table, string sourceKey, string targetKey)
    {
        if (!maps.TryGetValue(table, out var map)) maps[table] = map = new(StringComparer.OrdinalIgnoreCase);
        map[sourceKey] = targetKey;
        if (naturalKeys.TryGetValue(table, out var keys)) keys.Add(targetKey);
    }

    public void RegisterShortList(int id, string table, int value)
    {
        shortListById[id] = (table, value);
        shortListByValue[(table, value)] = id;
    }

    /// <summary>Legacy key -> target key, or null when it isn't imported (yet).</summary>
    public string? Resolve(ForeignKey fk, string raw)
    {
        if (fk.ShortListTable is { } listTable)
        {
            if (!shortListsLoaded || !int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var code)) return null;
            // Mapped ShortList id of the right list (export round-trip) wins; otherwise the value is
            // the legacy code itself (IndexValue), which is what the Access tables store.
            if (maps.GetValueOrDefault("ShortList")?.GetValueOrDefault(raw) is { } mapped &&
                int.TryParse(mapped, out var mappedId) &&
                shortListById.TryGetValue(mappedId, out var hit) && hit.Table == listTable)
            {
                return mapped;
            }

            return shortListByValue.TryGetValue((listTable, code), out var byValue)
                ? byValue.ToString(CultureInfo.InvariantCulture)
                : null;
        }

        if (maps.GetValueOrDefault(fk.TargetTable)?.GetValueOrDefault(raw) is { } target) return target;
        if (naturalKeys.TryGetValue(fk.TargetTable, out var keys) && keys.Contains(raw)) return raw;
        return null;
    }

    public bool HasMap(string table, string sourceKey) =>
        maps.GetValueOrDefault(table)?.ContainsKey(sourceKey) == true;

    public int? FindShortList(string table, int value) =>
        shortListByValue.TryGetValue((table, value), out var id) ? id : null;

    /// <summary>Used when a required StaffId/PostedUserId has no imported Staff row: the lowest-id
    /// Root user (the person running the import is Root), else any user.</summary>
    public async Task<int?> FallbackStaffIdAsync(CancellationToken ct)
    {
        if (fallbackStaffLoaded) return fallbackStaffId;
        fallbackStaffLoaded = true;
        var rootRoleId = await db.Set<IdentityRole<int>>().AsNoTracking()
            .Where(x => x.NormalizedName == "ROOT").Select(x => (int?)x.Id).FirstOrDefaultAsync(ct);
        fallbackStaffId = rootRoleId is null
            ? null
            : await db.Set<IdentityUserRole<int>>().AsNoTracking()
                .Where(x => x.RoleId == rootRoleId).OrderBy(x => x.UserId).Select(x => (int?)x.UserId).FirstOrDefaultAsync(ct);
        fallbackStaffId ??= await db.Users.AsNoTracking().OrderBy(x => x.Id).Select(x => (int?)x.Id).FirstOrDefaultAsync(ct);
        return fallbackStaffId;
    }

    /// <summary>CompanyId of an already-imported parent row (for CSVs that omit CompanyId).</summary>
    public async Task<int?> CompanyOfAsync(Type parentEntity, int parentId, CancellationToken ct)
    {
        var entityType = db.Model.FindEntityType(parentEntity)!;
        var name = entityType.Name;
        if (!companyOf.TryGetValue(name, out var cache))
        {
            var table = $"[{entityType.GetSchema()}].[{entityType.GetTableName()}]";
            var rows = await db.Database
                .SqlQueryRaw<IdCompanyRow>($"SELECT [Id] AS [Id], CAST([CompanyId] AS int) AS [CompanyId] FROM {table} WHERE [CompanyId] IS NOT NULL")
                .ToArrayAsync(ct);
            companyOf[name] = cache = rows.ToDictionary(x => x.Id, x => x.CompanyId);
        }

        return cache.TryGetValue(parentId, out var companyId) ? companyId : null;
    }

    public void ForgetCompanyCache() => companyOf.Clear();

    /// <summary>NoticeBatch needs a NoticeTemplate; Access has none, so one placeholder per company.</summary>
    public async Task<int> NoticeTemplateIdAsync(int companyId, CancellationToken ct)
    {
        if (noticeTemplates.TryGetValue(companyId, out var cached)) return cached;
        const string name = "Legacy uvoz";
        var id = await db.Set<NoticeTemplate>().AsNoTracking()
            .Where(x => x.CompanyId == companyId && x.Name == name).Select(x => (int?)x.Id).FirstOrDefaultAsync(ct);
        if (id is null)
        {
            var template = new NoticeTemplate { CompanyId = companyId, Name = name, Body = "Opomena preuzeta iz starog sistema (Access).", IsActive = false };
            db.Add(template);
            await db.SaveChangesAsync(ct);
            db.Entry(template).State = EntityState.Detached;
            id = template.Id;
        }

        noticeTemplates[companyId] = id.Value;
        return id.Value;
    }

    public static string? Normalize(string? raw) => string.IsNullOrWhiteSpace(raw) ? null : raw.Trim();

    public static string? NormalizeFk(string? raw, bool keepZero) =>
        keepZero ? Normalize(raw) : SerbianLegacyValueParser.ZeroToNull(raw);

}

public sealed class IdCompanyRow
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
}
