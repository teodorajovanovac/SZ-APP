namespace SzApp.Domain.LedgerBanking;

/// <summary>Legacy Kartica F7 grouping modes (9.7).</summary>
public enum LedgerCardGrouping
{
    None = 0,
    PaymentReference = 1,
    Document = 2,
    OpenItems = 3
}

/// <summary>
/// Pure ledger-card arithmetic (GAP-07/08). Balance is always D − P; storno lines are negative
/// amounts on the same side (P9), so plain sums are correct.
/// </summary>
public static class LedgerCardMath
{
    /// <summary>|Σ(D−P)| below half a para counts as closed (amounts are stored with 4 decimals).</summary>
    public const decimal ClosedTolerance = 0.005m;

    public static bool IsOpen(decimal debit, decimal credit) => Math.Abs(debit - credit) >= ClosedTolerance;

    /// <summary>Running balance after each line, starting from <paramref name="start"/>.</summary>
    public static decimal[] Running(decimal start, IEnumerable<(decimal Debit, decimal Credit)> lines)
    {
        var balance = start;
        return lines.Select(x => balance += x.Debit - x.Credit).ToArray();
    }

    /// <summary>
    /// "Samo otvoreni": payment references whose Σ(D−P) is not zero. A null/empty reference is its
    /// own bucket (key ""), so unreferenced lines only show up while they don't net to zero.
    /// </summary>
    public static HashSet<string> OpenKeys(IEnumerable<(string? Key, decimal Debit, decimal Credit)> sums) =>
        sums.GroupBy(x => x.Key ?? string.Empty)
            .Where(g => IsOpen(g.Sum(x => x.Debit), g.Sum(x => x.Credit)))
            .Select(g => g.Key)
            .ToHashSet(StringComparer.Ordinal);

    /// <summary>
    /// Overdue part of an open balance, FIFO: payments (the reducing side) settle the oldest
    /// charges first, so overdue = charges already due − all payments, clamped to [0, outstanding].
    /// For a credit-normal account (4350 suppliers) pass the sides swapped.
    /// </summary>
    // ponytail: FIFO approximation, not per-invoice matching via ClosesDocumentType; confirm with the accountant.
    public static decimal Overdue(decimal chargesDue, decimal payments, decimal outstanding) =>
        Math.Clamp(chargesDue - payments, 0m, Math.Max(outstanding, 0m));
}
