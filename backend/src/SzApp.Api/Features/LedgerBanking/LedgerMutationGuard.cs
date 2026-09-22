using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SzApp.Data;
using SzApp.Data.Entities;
using SzApp.Domain;

namespace SzApp.Api.Features.LedgerBanking;

public sealed class LedgerMutationScope
{
    private static readonly AsyncLocal<int> Depth = new();

    public bool IsPostingAllowed => Depth.Value > 0;

    public IDisposable AllowPosting()
    {
        Depth.Value++;
        return new ScopeLease();
    }

    private sealed class ScopeLease : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            Depth.Value--;
            _disposed = true;
        }
    }
}

public sealed class LedgerMutationGuardInterceptor(LedgerMutationScope mutationScope) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        GuardAsync(eventData.Context, CancellationToken.None).GetAwaiter().GetResult();
        return result;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        await GuardAsync(eventData.Context, cancellationToken);
        return result;
    }

    private async Task GuardAsync(DbContext? context, CancellationToken cancellationToken)
    {
        if (context is not SzAppDbContext dbContext)
        {
            return;
        }

        var changedJournals = dbContext.ChangeTracker.Entries<JournalEntry>()
            .Where(x => x.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToArray();

        if (!mutationScope.IsPostingAllowed && changedJournals.Any(IsPostingStatusMutation))
        {
            throw new DomainRuleException("journal.posting-bypass", "Status knjiženja može menjati samo servis za knjiženje.");
        }

        if (changedJournals.Any(x => x.State != EntityState.Added && x.OriginalValues.GetValue<bool>(nameof(JournalEntry.IsPosted))))
        {
            throw new DomainRuleException("journal.posted-immutable", "Knjiženi nalog se ne može menjati.");
        }

        var lineEntries = dbContext.ChangeTracker.Entries<LedgerEntry>()
            .Where(x => x.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToArray();
        if (lineEntries.Length == 0)
        {
            return;
        }

        var journalIds = lineEntries.Select(GetJournalId).Where(x => x > 0).Distinct().ToArray();
        var trackedPostedIds = changedJournals.Where(x => x.Entity.IsPosted).Select(x => x.Entity.Id).ToHashSet();
        var databaseHasPostedJournal = journalIds.Length > 0 && await dbContext.JournalEntries
            .AsNoTracking()
            .AnyAsync(x => journalIds.Contains(x.Id) && x.IsPosted, cancellationToken);

        if (databaseHasPostedJournal || journalIds.Any(trackedPostedIds.Contains))
        {
            throw new DomainRuleException("journal.lines-posted-immutable", "Stavke knjiženog naloga se ne mogu menjati.");
        }
    }

    private static bool IsPostingStatusMutation(EntityEntry<JournalEntry> entry) =>
        entry.State == EntityState.Added
            ? entry.Entity.IsPosted
            : entry.State == EntityState.Modified && entry.Property(x => x.IsPosted).IsModified;

    private static int GetJournalId(EntityEntry<LedgerEntry> entry) =>
        entry.State == EntityState.Deleted
            ? entry.OriginalValues.GetValue<int>(nameof(LedgerEntry.JournalEntryId))
            : entry.Entity.JournalEntryId;
}
