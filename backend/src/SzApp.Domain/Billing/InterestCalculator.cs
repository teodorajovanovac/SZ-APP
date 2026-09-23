namespace SzApp.Domain.Billing;

public static class InterestCalculator
{
    public static IReadOnlyList<InterestPeriodResult> Calculate(
        decimal principal,
        IEnumerable<InterestPeriod> periods)
    {
        if (principal < 0m)
        {
            throw new DomainRuleException("interest.negative-principal", "Osnovica za kamatu ne može biti negativna.");
        }

        var results = new List<InterestPeriodResult>();
        foreach (var period in periods.OrderBy(x => x.From))
        {
            if (period.To < period.From || period.AnnualRate < 0m)
            {
                throw new DomainRuleException("interest.invalid-period", "Period i stopa kamate nisu ispravni.");
            }

            var cursor = period.From;
            while (cursor <= period.To)
            {
                var yearEnd = new DateOnly(cursor.Year, 12, 31);
                var segmentEnd = period.To < yearEnd ? period.To : yearEnd;
                var days = segmentEnd.DayNumber - cursor.DayNumber + 1;
                var daysInYear = DateTime.IsLeapYear(cursor.Year) ? 366m : 365m;
                var fraction = days / daysInYear * period.AnnualRate / 100m;
                // Interest is computed from the full-precision fraction, not the rounded
                // display coefficient below — rounding first before multiplying by principal
                // introduced material over/undercharges (see InterestCalculatorGoldenTests).
                var interest = FinanceRounding.Money(principal * fraction);
                var coefficient = FinanceRounding.Calculation(fraction);
                results.Add(new InterestPeriodResult(cursor, segmentEnd, days, period.AnnualRate, coefficient, interest));
                cursor = segmentEnd.AddDays(1);
            }
        }

        return results;
    }
}
