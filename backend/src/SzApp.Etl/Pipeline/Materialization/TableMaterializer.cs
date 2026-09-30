using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SzApp.Data;
using SzApp.Data.Entities;
using SzApp.Data.Entities.Billing;
using SzApp.Data.Entities.EtlExtended;
using SzApp.Data.Entities.LedgerBanking;

namespace SzApp.Etl.Pipeline.Materialization;

public sealed class MaterializeResult
{
    public int Inserted { get; set; }
    public int Updated { get; set; }
    public int Unchanged { get; set; }
    public IReadOnlyList<string> DeletedInSource { get; set; } = [];
    public HashSet<int> CompanyIds { get; } = [];
    public List<QuarantineRecord> Quarantine { get; } = [];
}

/// <summary>
/// Writes one staged canonical table into its live table: delta plan (ETL-05), chunked
/// AddRange + one SaveChanges per chunk in a transaction together with the LegacyKeyMap rows
/// (ETL-09; a chunk is atomic, so a crashed run resumes cleanly). A chunk the DB rejects (unique
/// index, CHECK) is retried row by row and only the offending rows are quarantined.
/// </summary>
public sealed class TableMaterializer(ImportContext ctx, TableSpec spec, Guid runId, int runCompanyId, TimeProvider clock)
{
    // Must equal SzApp.Api.Security.StaffManagementPolicy.MustChangePasswordClaim (unit-tested).
    public const string MustChangePasswordClaim = "szapp:must_change_password";

    private readonly SzAppDbContext db = ctx.Db;
    private readonly MaterializeResult result = new();
    private readonly List<(string SourceKey, string TargetKey, List<(Microsoft.EntityFrameworkCore.Metadata.IProperty Property, ForeignKey Fk, string RawKey)> Refs)> selfRefs = [];
    private readonly List<PendingLegacyRelationship> pending = [];

    private sealed record Work(StagedRow Row, MappedKey? Map);

    public async Task<MaterializeResult> RunAsync(IReadOnlyList<StagedRow> rows, CancellationToken ct)
    {
        await ctx.PrepareAsync(spec, ct);
        var scopeRuns = await db.Set<EtlRunContext>().AsNoTracking()
            .Where(x => x.CompanyId == runCompanyId).Select(x => x.EtlRunId).ToArrayAsync(ct);
        var scope = scopeRuns.ToHashSet();
        var existing = (await db.LegacyKeyMaps.AsNoTracking()
                .Where(x => x.SourceTable == spec.Table && x.TargetTable == spec.Table)
                .Select(x => new { x.Id, x.SourceKey, x.TargetKey, x.RowHash, x.EtlRunId })
                .ToArrayAsync(ct))
            .ToDictionary(x => x.SourceKey, x => new MappedKey(x.Id, x.TargetKey, x.RowHash, scope.Contains(x.EtlRunId)), StringComparer.OrdinalIgnoreCase);

        var plan = DeltaPlanner.Plan(rows, existing);
        result.Unchanged = plan.Unchanged.Count;
        result.DeletedInSource = plan.DeletedInSource;
        foreach (var duplicate in plan.DuplicateKeys)
        {
            Quarantine(duplicate, "etl.duplicate-key", $"Ključ {duplicate.SourceKey} se ponavlja u fajlu.");
        }

        var work = plan.Inserts.Select(x => new Work(x, null))
            .Concat(plan.Updates.Select(x => new Work(x.Row, x.Map)))
            .ToArray();
        foreach (var chunk in work.Chunk(spec.ChunkSize))
        {
            await ProcessAsync(chunk, ct);
        }

        await ApplySelfReferencesAsync(ct);
        db.AddRange(pending);
        db.AddRange(result.Quarantine);
        await db.SaveChangesAsync(ct);
        db.ChangeTracker.Clear();
        return result;
    }

    private async Task ProcessAsync(IReadOnlyList<Work> chunk, CancellationToken ct)
    {
        db.ChangeTracker.Clear();
        var prepared = new List<(Work Work, EntityEntry Entry, BindResult Bind, bool IsInsert)>(chunk.Count);
        foreach (var item in chunk)
        {
            var targetKey = item.Map?.TargetKey ?? await FindExistingAsync(item.Row, ct);
            var entity = targetKey is null ? null : await FindAsync(targetKey, ct);
            var isInsert = entity is null;
            if (isInsert)
            {
                entity = Activator.CreateInstance(spec.Entity)!;
                if (spec.Key != KeyMode.Identity && !TrySetKey(entity, item.Row)) continue;
                db.Add(entity);
            }

            var entry = db.Entry(entity!);
            var bind = await RowBinder.BindAsync(ctx, spec, entry, item.Row.Values, item.Row.SourceKey, isInsert, ct);
            await ApplyRulesAsync(entry, item.Row, isInsert, bind, ct);
            if (bind.Errors.Count > 0)
            {
                foreach (var error in bind.Errors) Quarantine(item.Row, error.Code, error.Message);
                entry.State = isInsert ? EntityState.Detached : EntityState.Unchanged;
                if (!isInsert) await entry.ReloadAsync(ct);
                continue;
            }

            prepared.Add((item, entry, bind, isInsert));
        }

        if (prepared.Count == 0) return;
        try
        {
            await SaveAsync(prepared, ct);
        }
        catch (DbUpdateException exception)
        {
            db.ChangeTracker.Clear();
            if (chunk.Count == 1)
            {
                Quarantine(chunk[0].Row, "etl.db-rejected", (exception.InnerException ?? exception).Message);
                return;
            }

            // ponytail: row-by-row retry of a rejected chunk — fine for the handful of bad rows real
            // Access data has; a file where most rows violate a constraint degrades to per-row speed.
            foreach (var item in prepared.Select(x => x.Work)) await ProcessAsync([item], ct);
        }
    }

    private async Task SaveAsync(List<(Work Work, EntityEntry Entry, BindResult Bind, bool IsInsert)> prepared, CancellationToken ct)
    {
        var identityInsert = spec.Key == KeyMode.Explicit && prepared.Any(x => x.IsInsert);
        var entityType = db.Model.FindEntityType(spec.Entity)!;
        var table = $"[{entityType.GetSchema()}].[{entityType.GetTableName()}]";
        var strategy = db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(ct);
            if (identityInsert) await db.Database.ExecuteSqlRawAsync($"SET IDENTITY_INSERT {table} ON", ct);
            try
            {
                await db.SaveChangesAsync(ct);
            }
            finally
            {
                if (identityInsert) await db.Database.ExecuteSqlRawAsync($"SET IDENTITY_INSERT {table} OFF", ct);
            }

            var mapIds = prepared.Where(x => x.Work.Map is not null).Select(x => x.Work.Map!.MapId).ToArray();
            var maps = mapIds.Length == 0
                ? new Dictionary<long, LegacyKeyMap>()
                : await db.LegacyKeyMaps.Where(x => mapIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);
            foreach (var (work, entry, _, isInsert) in prepared)
            {
                if (work.Map is null)
                {
                    db.LegacyKeyMaps.Add(new LegacyKeyMap
                    {
                        EtlRunId = runId, SourceTable = spec.Table, SourceKey = work.Row.SourceKey,
                        TargetTable = spec.Table, TargetKey = TargetKeyOf(entry), RowHash = work.Row.RowHash
                    });
                }
                else if (maps.TryGetValue(work.Map.MapId, out var map))
                {
                    (map.EtlRunId, map.RowHash) = (runId, work.Row.RowHash);
                }

                if (spec.Table == "Staff") await GrantStaffAsync((ApplicationUser)entry.Entity, work.Row, isInsert, ct);
            }

            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        });

        foreach (var (work, entry, bind, isInsert) in prepared)
        {
            var targetKey = TargetKeyOf(entry);
            ctx.Register(spec.Table, work.Row.SourceKey, targetKey);
            if (entry.Entity is ShortList list) ctx.RegisterShortList(list.Id, list.TableName, list.IndexValue);
            if (entry.Metadata.FindProperty("CompanyId") is not null && entry.Property("CompanyId").CurrentValue is int companyId) result.CompanyIds.Add(companyId);
            if (isInsert) result.Inserted++; else result.Updated++;
            if (bind.SelfReferences.Count > 0) selfRefs.Add((work.Row.SourceKey, targetKey, bind.SelfReferences));
            pending.AddRange(bind.Deferred.Select(d => new PendingLegacyRelationship
            {
                EtlRunId = runId, SourceTable = spec.Table, SourceKey = work.Row.SourceKey, RelationshipName = d.Property,
                TargetSourceTable = d.TargetTable, TargetSourceKey = d.TargetKey
            }));
        }

        db.ChangeTracker.Clear();
    }

    private async Task ApplySelfReferencesAsync(CancellationToken ct)
    {
        foreach (var chunk in selfRefs.Chunk(500))
        {
            foreach (var (sourceKey, targetKey, refs) in chunk)
            {
                var entity = await FindAsync(targetKey, ct);
                if (entity is null) continue;
                var entry = db.Entry(entity);
                foreach (var (property, fk, rawKey) in refs)
                {
                    var target = ctx.Resolve(fk, rawKey);
                    entry.Property(property.Name).CurrentValue = target is null ? null : RowBinder.ConvertKey(property, target);
                }
            }

            await db.SaveChangesAsync(ct);
            db.ChangeTracker.Clear();
        }
    }

    /// <summary>First-time upsert against rows that exist without a key map (seeded code lists,
    /// a company created by hand before the import, a re-created user).</summary>
    private async Task<string?> FindExistingAsync(StagedRow row, CancellationToken ct)
    {
        var v = row.Values;
        switch (spec.Table)
        {
            case "ShortList":
                return v.TryGetValue("TableName", out var listName) && v.TryGetValue("IndexValue", out var rawValue) &&
                       int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) &&
                       ctx.FindShortList(listName, value) is { } listId
                    ? listId.ToString(CultureInfo.InvariantCulture)
                    : null;
            case "InterestRate":
                if (!v.TryGetValue("Date", out var rawDate) || string.IsNullOrWhiteSpace(rawDate)) return null;
                var date = Parsing.SerbianLegacyValueParser.ParseDate(rawDate);
                var timeCode = v.GetValueOrDefault("TimeCode") is { Length: > 0 } code ? code : "G";
                var rateId = await db.Set<InterestRate>().AsNoTracking()
                    .Where(x => x.Date == date && x.TimeCode == timeCode).Select(x => (int?)x.Id).FirstOrDefaultAsync(ct);
                return rateId?.ToString(CultureInfo.InvariantCulture);
            case "Staff":
                var userName = v.GetValueOrDefault("UserName")?.Trim().ToUpperInvariant();
                if (string.IsNullOrEmpty(userName)) return null;
                var userId = await db.Users.AsNoTracking().Where(x => x.NormalizedUserName == userName).Select(x => (int?)x.Id).FirstOrDefaultAsync(ct);
                return userId?.ToString(CultureInfo.InvariantCulture);
        }

        return spec.Key == KeyMode.Identity ? null : row.SourceKey;
    }

    private async Task<object?> FindAsync(string targetKey, CancellationToken ct)
    {
        var keyType = db.Model.FindEntityType(spec.Entity)!.FindPrimaryKey()!.Properties[0].ClrType;
        object key = keyType == typeof(string) ? targetKey
            : int.TryParse(targetKey, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) ? id : targetKey;
        if (key is string && keyType != typeof(string)) return null; // "staging:" row id etc.
        return await db.FindAsync(spec.Entity, [key], ct);
    }

    private bool TrySetKey(object entity, StagedRow row)
    {
        var keyProperty = db.Model.FindEntityType(spec.Entity)!.FindPrimaryKey()!.Properties[0];
        try
        {
            keyProperty.PropertyInfo!.SetValue(entity, RowBinder.Parse(keyProperty.ClrType, row.SourceKey));
            return true;
        }
        catch (FormatException exception)
        {
            Quarantine(row, "etl.invalid-key", exception.Message);
            return false;
        }
    }

    private static string TargetKeyOf(EntityEntry entry)
    {
        var key = entry.Metadata.FindPrimaryKey()!.Properties[0];
        return Convert.ToString(entry.Property(key.Name).CurrentValue, CultureInfo.InvariantCulture)!;
    }

    /// <summary>Table-specific rules the generic binder can't express.</summary>
    private async Task ApplyRulesAsync(EntityEntry entry, StagedRow row, bool isInsert, BindResult bind, CancellationToken ct)
    {
        var v = row.Values;
        switch (entry.Entity)
        {
            case InterestRate rate:
                // ETL-14: only rates from 2020 on, never the "?" placeholders.
                if (v.Values.Any(x => x.Contains('?')) || rate.Date < new DateOnly(2020, 1, 1))
                    bind.Errors.Add(new("etl.excluded", "Stopa pre 2020. ili sa '?' se ne uvozi (ETL-14)."));
                if (string.IsNullOrWhiteSpace(rate.TimeCode)) rate.TimeCode = "G";
                break;
            case Partner partner:
                if (string.IsNullOrWhiteSpace(partner.Language)) partner.Language = "sr-Latn";
                break;
            case ApplicationUser user:
                if (v.GetValueOrDefault("Level") is { } level && level.Trim() == "0")
                    bind.Errors.Add(new("etl.staff-no-access", "Nivo 0 (bez pristupa) se ne migrira (9.6)."));
                if (string.IsNullOrWhiteSpace(user.UserName)) bind.Errors.Add(new("etl.required-value", "UserName je obavezan."));
                user.Email ??= user.UserName?.Contains('@') == true ? user.UserName : null;
                user.NormalizedUserName = user.UserName?.ToUpperInvariant();
                user.NormalizedEmail = user.Email?.ToUpperInvariant();
                // ETL-10: no password is ever migrated (all legacy ones are compromised, SEC-16);
                // the user sets one via reset link and is forced to change it.
                user.EmailConfirmed = true;
                user.LockoutEnabled = true;
                if (isInsert)
                {
                    user.SecurityStamp = Guid.NewGuid().ToString("N");
                    user.ConcurrencyStamp = Guid.NewGuid().ToString();
                }
                if (string.IsNullOrWhiteSpace(user.PreferredLanguage)) user.PreferredLanguage = "sr-Latn";
                break;
            case JournalEntry journal:
                // ETL-06: Access GL has no drafts — every historical journal is posted.
                journal.IsPosted = true;
                journal.PostedAt ??= new DateTimeOffset(journal.PostingDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
                journal.PostedUserId ??= await ctx.FallbackStaffIdAsync(ct);
                if (journal.PostedUserId is null) journal.IsPosted = false;
                break;
            case InvoiceBatch batch:
                if (batch.StaffId == 0 && await ctx.FallbackStaffIdAsync(ct) is { } staffId)
                {
                    batch.StaffId = staffId;
                    bind.Errors.RemoveAll(x => x.Message.StartsWith("StaffId", StringComparison.Ordinal));
                }
                if (batch.EntryDate == default) batch.EntryDate = new DateTimeOffset(batch.IssueDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
                break;
            case LedgerEntry line:
                // 9.2: 73 GK lines carry >2 decimals — round (P1: away from zero); negative storno
                // amounts are legal (P9) and imported as-is.
                line.DebitAmount = Math.Round(line.DebitAmount, 2, MidpointRounding.AwayFromZero);
                line.CreditAmount = Math.Round(line.CreditAmount, 2, MidpointRounding.AwayFromZero);
                break;
            case BankStatement statement:
                if (string.IsNullOrWhiteSpace(statement.LedgerAccount)) statement.LedgerAccount = "2410";
                if (!v.ContainsKey("Status") && v.GetValueOrDefault("IsPosted") is { } posted &&
                    Parsing.SerbianLegacyValueParser.ParseAccessBoolean(posted))
                    statement.Status = BankStatementStatus.Posted;
                break;
            case NoticeBatch noticeBatch when noticeBatch.NoticeTemplateId == 0 && noticeBatch.CompanyId > 0:
                noticeBatch.NoticeTemplateId = await ctx.NoticeTemplateIdAsync(noticeBatch.CompanyId, ct);
                break;
        }
    }

    /// <summary>ETL-10 / 9.6: 9 -> Root; 8 -> Upravnik, 4 -> Moderator, 3 -> Review on every imported
    /// company (Access had no per-SZ restriction). Grants are only ever added, never removed.</summary>
    private async Task GrantStaffAsync(ApplicationUser user, StagedRow row, bool isInsert, CancellationToken ct)
    {
        if (isInsert)
        {
            db.Set<IdentityUserClaim<int>>().Add(new IdentityUserClaim<int>
            {
                UserId = user.Id, ClaimType = MustChangePasswordClaim, ClaimValue = "true"
            });
        }

        var level = row.Values.GetValueOrDefault("Level")?.Trim();
        if (level == "9")
        {
            var role = await db.Set<IdentityRole<int>>().SingleOrDefaultAsync(x => x.NormalizedName == "ROOT", ct);
            if (role is null)
            {
                role = new IdentityRole<int> { Name = ImportContext.RootRoleName, NormalizedName = "ROOT", ConcurrencyStamp = Guid.NewGuid().ToString() };
                db.Add(role);
                await db.SaveChangesAsync(ct);
            }

            if (!await db.Set<IdentityUserRole<int>>().AnyAsync(x => x.UserId == user.Id && x.RoleId == role.Id, ct))
                db.Add(new IdentityUserRole<int> { UserId = user.Id, RoleId = role.Id });
            return;
        }

        StaffRole? staffRole = level switch { "8" => StaffRole.Upravnik, "4" => StaffRole.Moderator, "3" => StaffRole.Review, _ => null };
        if (staffRole is null) return;
        var companies = await db.LegacyKeyMaps.AsNoTracking()
            .Where(x => x.SourceTable == "Company" && x.TargetTable == "Company").Select(x => x.TargetKey).ToListAsync(ct);
        var companyIds = companies.Select(int.Parse).Append(runCompanyId).Distinct().ToArray();
        var granted = await db.StaffAccess.AsNoTracking().Where(x => x.StaffId == user.Id).Select(x => x.CompanyId).ToListAsync(ct);
        foreach (var companyId in companyIds.Except(granted))
        {
            db.StaffAccess.Add(new StaffAccess { StaffId = user.Id, CompanyId = companyId, StaffRole = staffRole.Value });
        }
    }

    private void Quarantine(StagedRow row, string code, string message) =>
        result.Quarantine.Add(new QuarantineRecord
        {
            EtlRunId = runId,
            SourceTable = spec.Table,
            SourceKey = row.SourceKey,
            RawJson = System.Text.Json.JsonSerializer.Serialize(row.Values),
            ErrorCode = code,
            ErrorMessage = message.Length > 2000 ? message[..2000] : message,
            CreatedAt = clock.GetUtcNow()
        });
}
