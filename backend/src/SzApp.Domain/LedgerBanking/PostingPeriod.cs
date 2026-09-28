namespace SzApp.Domain.LedgerBanking;

/// <summary>P13 month lock: periods are YYMM, same encoding as InvoiceBatch.PeriodYYMM.</summary>
public static class PostingPeriod
{
    public static int ToYYMM(DateOnly date) => date.Year % 100 * 100 + date.Month;

    public static void EnsureValid(int periodYYMM)
    {
        if (periodYYMM is < 1001 or > 9912 || periodYYMM % 100 is < 1 or > 12)
        {
            throw new DomainRuleException("posting-period.invalid", "Period mora biti u formatu YYMM.");
        }
    }

    public static void EnsureOpen(IEnumerable<DateOnly> postingDates, IReadOnlySet<int> lockedPeriods)
    {
        var locked = postingDates.Select(ToYYMM).Where(lockedPeriods.Contains).Order().FirstOrDefault();
        if (locked != 0)
        {
            throw new PostingPeriodLockedException(locked);
        }
    }
}

/// <summary>Mapped to HTTP 409 by ApiExceptionHandler.</summary>
public sealed class PostingPeriodLockedException(int periodYYMM)
    : Exception($"Period {periodYYMM:0000} je zaključan za knjiženje.")
{
    public const string Code = "posting-period.locked";
    public int PeriodYYMM { get; } = periodYYMM;
}
