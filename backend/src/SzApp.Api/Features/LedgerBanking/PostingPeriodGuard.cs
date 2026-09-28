using Microsoft.EntityFrameworkCore;
using SzApp.Data;
using SzApp.Data.Entities;
using SzApp.Domain;
using SzApp.Domain.LedgerBanking;

namespace SzApp.Api.Features.LedgerBanking;

/// <summary>FIN-20: "today" for business dates (storno date etc.) is Belgrade time, not container UTC.</summary>
public interface IBusinessClock
{
    DateOnly Today { get; }
}

public sealed class BelgradeBusinessClock(TimeProvider timeProvider) : IBusinessClock
{
    private static readonly TimeZoneInfo Zone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Belgrade");

    public DateOnly Today => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), Zone).DateTime);
}

public sealed record PostingPeriodLockResponse(
    int Id,
    int PeriodYYMM,
    DateTimeOffset LockedAt,
    int LockedByStaffId,
    DateTimeOffset? UnlockedAt,
    int? UnlockedByStaffId);

/// <summary>
/// FIN-01 / P13. <see cref="EnsureOpenAsync"/> is THE single check every posting and storno path
/// calls before a journal becomes posted; a hit throws <see cref="PostingPeriodLockedException"/> (409).
/// </summary>
public sealed class PostingPeriodGuard(SzAppDbContext db, TimeProvider timeProvider)
{
    public async Task EnsureOpenAsync(int companyId, IReadOnlyCollection<DateOnly> postingDates, CancellationToken ct)
    {
        var periods = postingDates.Select(PostingPeriod.ToYYMM).Distinct().ToArray();
        var locked = await db.PostingPeriodLocks.AsNoTracking()
            .Where(x => x.CompanyId == companyId && x.UnlockedAt == null && periods.Contains(x.PeriodYYMM))
            .Select(x => x.PeriodYYMM)
            .ToListAsync(ct);
        PostingPeriod.EnsureOpen(postingDates, locked.ToHashSet());
    }

    public async Task<IReadOnlyList<PostingPeriodLockResponse>> ListAsync(int companyId, CancellationToken ct) =>
        await db.PostingPeriodLocks.AsNoTracking()
            .Where(x => x.CompanyId == companyId)
            .OrderByDescending(x => x.PeriodYYMM).ThenByDescending(x => x.Id)
            .Select(x => new PostingPeriodLockResponse(x.Id, x.PeriodYYMM, x.LockedAt, x.LockedByStaffId, x.UnlockedAt, x.UnlockedByStaffId))
            .ToArrayAsync(ct);

    public async Task<PostingPeriodLockResponse> LockAsync(int companyId, int periodYYMM, int staffId, CancellationToken ct)
    {
        PostingPeriod.EnsureValid(periodYYMM);
        var active = await ActiveAsync(companyId, periodYYMM, ct);
        if (active is not null) return Map(active); // idempotent

        var entity = new PostingPeriodLock
        {
            CompanyId = companyId,
            PeriodYYMM = periodYYMM,
            LockedAt = timeProvider.GetUtcNow(),
            LockedByStaffId = staffId
        };
        db.PostingPeriodLocks.Add(entity);
        await db.SaveChangesAsync(ct); // filtered unique index turns a racing double-lock into 409
        return Map(entity);
    }

    public async Task<PostingPeriodLockResponse> UnlockAsync(int companyId, int periodYYMM, int staffId, CancellationToken ct)
    {
        PostingPeriod.EnsureValid(periodYYMM);
        var active = await ActiveAsync(companyId, periodYYMM, ct)
            ?? throw new DomainRuleException("posting-period.not-locked", "Period nije zaključan.");
        active.UnlockedAt = timeProvider.GetUtcNow();
        active.UnlockedByStaffId = staffId;
        await db.SaveChangesAsync(ct);
        return Map(active);
    }

    private Task<PostingPeriodLock?> ActiveAsync(int companyId, int periodYYMM, CancellationToken ct) =>
        db.PostingPeriodLocks.SingleOrDefaultAsync(x => x.CompanyId == companyId && x.PeriodYYMM == periodYYMM && x.UnlockedAt == null, ct);

    private static PostingPeriodLockResponse Map(PostingPeriodLock x) =>
        new(x.Id, x.PeriodYYMM, x.LockedAt, x.LockedByStaffId, x.UnlockedAt, x.UnlockedByStaffId);
}
