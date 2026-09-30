using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SzApp.Data.Entities;

namespace SzApp.Data;

/// <summary>
/// GAP-24: writes ops.AuditLog for every insert/update/delete of a business entity (schemas core, finance,
/// billing, platform). Details = changed fields old→new; RowVersion and secret columns are skipped, JMBG /
/// ID card values are masked. Audit rows go out in one extra batched SaveChanges right after the business
/// save (needed for identity keys of inserted rows) -- no per-row queries.
/// </summary>
public sealed class AuditTrailInterceptor(Func<int?> currentStaffId, TimeProvider clock, Func<string?>? correlationId = null) : SaveChangesInterceptor
{
    private static readonly HashSet<string> AuditedSchemas = new(StringComparer.OrdinalIgnoreCase) { "core", "finance", "billing", "platform" };
    private static readonly string[] SecretFragments = ["Password", "Secret", "Token", "Hash", "SecurityStamp", "ConcurrencyStamp", "RowVersion"];
    private static readonly HashSet<string> MaskedColumns = new(StringComparer.OrdinalIgnoreCase) { "Jmbg", "IdCardNumber" };

    private sealed record Pending(EntityEntry Entry, string Action, int? CompanyId, Dictionary<string, object?[]> Changes, string? Key);

    private List<Pending>? pending;
    private bool writing;

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Capture(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Capture(eventData.Context);
        return ValueTask.FromResult(result);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        if (Flush(eventData.Context)) eventData.Context!.SaveChanges();
        writing = false;
        return result;
    }

    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        if (Flush(eventData.Context)) await eventData.Context!.SaveChangesAsync(cancellationToken);
        writing = false;
        return result;
    }

    public override void SaveChangesFailed(DbContextErrorEventData eventData) => (pending, writing) = (null, false);

    public override Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        (pending, writing) = (null, false);
        return Task.CompletedTask;
    }

    private void Capture(DbContext? context)
    {
        if (context is null || writing) return;
        pending = null;
        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted)) continue;
            if (entry.Entity is AuditLog || !AuditedSchemas.Contains(entry.Metadata.GetSchema() ?? string.Empty)) continue;

            var changes = new Dictionary<string, object?[]>();
            foreach (var property in entry.Properties)
            {
                var name = property.Metadata.Name;
                if (SecretFragments.Any(f => name.Contains(f, StringComparison.OrdinalIgnoreCase))) continue;
                object? oldValue = entry.State == EntityState.Added ? null : property.OriginalValue;
                object? newValue = entry.State == EntityState.Deleted ? null : property.CurrentValue;
                if (entry.State == EntityState.Modified && (!property.IsModified || Equals(oldValue, newValue))) continue;
                if (entry.State != EntityState.Modified && oldValue is null && newValue is null) continue;
                if (MaskedColumns.Contains(name)) (oldValue, newValue) = (Mask(oldValue), Mask(newValue));
                changes[name] = [oldValue, newValue];
            }
            if (entry.State == EntityState.Modified && changes.Count == 0) continue;

            var companyId = entry.Metadata.FindProperty("CompanyId") is { } cp ? entry.Property(cp.Name).CurrentValue as int? : null;
            (pending ??= []).Add(new Pending(entry, entry.State.ToString(), companyId, changes,
                entry.State == EntityState.Added ? null : Key(entry)));
        }
    }

    private bool Flush(DbContext? context)
    {
        if (context is null || pending is not { Count: > 0 } items) return false;
        pending = null;
        writing = true;
        var staffId = currentStaffId();
        var now = clock.GetUtcNow();
        var correlation = correlationId?.Invoke() ?? string.Empty;
        foreach (var item in items)
        {
            context.Add(new AuditLog
            {
                CompanyId = item.CompanyId,
                StaffId = staffId,
                Timestamp = now,
                CorrelationId = correlation.Length > 64 ? correlation[..64] : correlation,
                EntityType = item.Entry.Metadata.ClrType.Name,
                ItemId = item.Key ?? Key(item.Entry),
                Action = item.Action,
                EventSource = "ef",
                DetailsJson = JsonSerializer.Serialize(item.Changes),
            });
        }
        return true;
    }

    private static string? Key(EntityEntry entry)
    {
        var key = entry.Metadata.FindPrimaryKey();
        if (key is null) return null;
        var value = string.Join(",", key.Properties.Select(p => entry.Property(p.Name).CurrentValue));
        return value.Length > 100 ? value[..100] : value;
    }

    private static string? Mask(object? value) => value?.ToString() is { Length: > 0 } ? "***" : null;
}
