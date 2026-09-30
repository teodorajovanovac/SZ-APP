namespace SzApp.Domain.Billing;

/// <summary>One 2040 movement for the interest base: signed D−P on its legacy GK.DPO date (invoice = due date, payment = posting date).</summary>
public sealed record InterestMovement(int PartnerAccountId, string SubAccountId, DateOnly Date, decimal Amount);

public sealed record InterestRatePoint(DateOnly Date, decimal Rate);

/// <summary>
/// One KamatniList row: [From, To] at a constant balance and rate. Coefficient is the legacy
/// 8-decimal value; Interest is NOT rounded (legacy rounds only the per-(partner, sub-account) sum).
/// Movement is the net change on From (for a segment's first row: the opening balance, like legacy DIPI).
/// </summary>
public sealed record InterestRow(
    int PartnerAccountId,
    string SubAccountId,
    DateOnly From,
    DateOnly To,
    int Days,
    decimal Movement,
    decimal Balance,
    decimal Rate,
    decimal Coefficient,
    decimal Interest);

public sealed record InterestTotal(int PartnerAccountId, string SubAccountId, decimal Interest);

/// <summary>
/// Legacy modKamata (audit 9.4, verified 475/475): simple interest on the running 2040 balance.
/// - Period [from, to] inclusive; the balance for a day includes that day's movements.
/// - Rate: last with Date &lt; from (strict); every later rate with Date ≤ to starts a new segment on its Date.
/// - Koef = Round(days / daysInYear(year of the rate segment's start) × rate / 100, 8); no split at 31.12.
/// - Interest = Koef × balance, only for balance &gt; 0, unrounded; totals are FinanceRounding.Money of the sum, only &gt; 0.
/// </summary>
public static class InterestCalculator
{
    public const int CoefficientScale = 8;

    public static IReadOnlyList<InterestPeriod> RateSegments(DateOnly from, DateOnly to, IEnumerable<InterestRatePoint> rates)
    {
        if (to < from) throw new DomainRuleException("interest.invalid-period", "Početak perioda kamate mora biti pre kraja.");
        var ordered = rates.OrderBy(x => x.Date).ToArray();
        var first = ordered.LastOrDefault(x => x.Date < from)
            ?? throw new DomainRuleException("interest.rate-not-found", "Nema stope kamate pre početka perioda.");
        var starts = new List<InterestRatePoint> { new(from, first.Rate) };
        starts.AddRange(ordered.Where(x => x.Date >= from && x.Date <= to));

        var segments = new List<InterestPeriod>();
        for (var i = 0; i < starts.Count; i++)
        {
            var end = i + 1 < starts.Count ? starts[i + 1].Date.AddDays(-1) : to;
            if (end < starts[i].Date) continue; // a rate dated exactly on `from` replaces the opening one
            if (starts[i].Rate < 0m) throw new DomainRuleException("interest.invalid-period", "Stopa kamate ne može biti negativna.");
            segments.Add(new InterestPeriod(starts[i].Date, end, starts[i].Rate));
        }
        return segments;
    }

    public static decimal Coefficient(int days, DateOnly segmentStart, decimal annualRate)
    {
        var daysInYear = DateTime.IsLeapYear(segmentStart.Year) ? 366m : 365m;
        return decimal.Round(days / daysInYear * annualRate / 100m, CoefficientScale, MidpointRounding.AwayFromZero);
    }

    public static IReadOnlyList<InterestRow> CalculateRows(
        DateOnly from,
        DateOnly to,
        IEnumerable<InterestRatePoint> rates,
        IEnumerable<InterestMovement> movements)
    {
        var segments = RateSegments(from, to, rates);
        var rows = new List<InterestRow>();
        foreach (var group in movements.Where(x => x.Date <= to)
                     .GroupBy(x => (x.PartnerAccountId, x.SubAccountId))
                     .OrderBy(x => x.Key.PartnerAccountId).ThenBy(x => x.Key.SubAccountId, StringComparer.Ordinal))
        {
            var groupRows = new List<InterestRow>();
            foreach (var segment in segments)
            {
                var balance = group.Where(x => x.Date <= segment.From).Sum(x => x.Amount);
                var movement = balance;
                var rowStart = segment.From;
                var changes = group.Where(x => x.Date > segment.From && x.Date <= segment.To)
                    .GroupBy(x => x.Date).Select(x => (Date: x.Key, Net: x.Sum(y => y.Amount)))
                    .Where(x => x.Net != 0m).OrderBy(x => x.Date);
                foreach (var change in changes)
                {
                    groupRows.Add(Row(group.Key, rowStart, change.Date.AddDays(-1), movement, balance, segment));
                    balance += change.Net;
                    movement = change.Net;
                    rowStart = change.Date;
                }
                groupRows.Add(Row(group.Key, rowStart, segment.To, movement, balance, segment));
            }
            if (groupRows.Any(x => x.Balance != 0m)) rows.AddRange(groupRows);
        }
        return rows;
    }

    public static IReadOnlyList<InterestTotal> Totals(IEnumerable<InterestRow> rows) =>
        Transfer(rows.Select(x => new InterestTotal(x.PartnerAccountId, x.SubAccountId, x.Interest)));

    /// <summary>Legacy PrenesiZK over unrounded interest amounts: Round(Σ, 2) per (partner, sub-account), only &gt; 0.</summary>
    public static IReadOnlyList<InterestTotal> Transfer(IEnumerable<InterestTotal> unrounded) =>
        unrounded.GroupBy(x => (x.PartnerAccountId, x.SubAccountId))
            .Select(g => new InterestTotal(g.Key.PartnerAccountId, g.Key.SubAccountId, FinanceRounding.Money(g.Sum(x => x.Interest))))
            .Where(x => x.Interest > 0m)
            .OrderBy(x => x.PartnerAccountId).ThenBy(x => x.SubAccountId, StringComparer.Ordinal)
            .ToArray();

    /// <summary>Calculator preview: a constant principal owed for the whole period.</summary>
    public static IReadOnlyList<InterestPeriodResult> Calculate(decimal principal, DateOnly from, DateOnly to, IEnumerable<InterestRatePoint> rates)
    {
        if (principal < 0m) throw new DomainRuleException("interest.negative-principal", "Osnovica za kamatu ne može biti negativna.");
        return CalculateRows(from, to, rates, [new InterestMovement(0, string.Empty, from, principal)])
            .Select(x => new InterestPeriodResult(x.From, x.To, x.Days, x.Rate, x.Coefficient, x.Interest))
            .ToArray();
    }

    private static InterestRow Row((int PartnerAccountId, string SubAccountId) key, DateOnly start, DateOnly end, decimal movement, decimal balance, InterestPeriod segment)
    {
        var days = end.DayNumber - start.DayNumber + 1;
        var coefficient = Coefficient(days, segment.From, segment.AnnualRate);
        var interest = balance > 0m ? coefficient * balance : 0m;
        return new InterestRow(key.PartnerAccountId, key.SubAccountId, start, end, days, movement, balance, segment.AnnualRate, coefficient, interest);
    }
}
