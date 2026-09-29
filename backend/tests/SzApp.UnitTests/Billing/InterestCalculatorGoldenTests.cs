using SzApp.Domain;
using SzApp.Domain.Billing;

namespace SzApp.UnitTests.Billing;

/// <summary>Legacy modKamata semantics (audit 9.4). Expected values are hand-computed from the legacy formula.</summary>
public sealed class InterestCalculatorGoldenTests
{
    private static readonly InterestRatePoint[] TenPercent = [new(new DateOnly(2020, 1, 1), 10m)];

    [Fact]
    public void Calculate_NoYearSplit_UsesDaysInYearOfSegmentStart_Legacy822()
    {
        var result = InterestCalculator.Calculate(10_000m, new DateOnly(2023, 12, 31), new DateOnly(2024, 1, 2), TenPercent);

        // Legacy: one row, 3 days inclusive, 365 (year of the segment start 2023), no split at 31.12.
        // Koef = Round(3/365 * 10/100, 8) = 0.00082192; 0.00082192 * 10000 = 8.2192 -> 8.22.
        var row = Assert.Single(result);
        Assert.Equal(3, row.Days);
        Assert.Equal(0.00082192m, row.Coefficient);
        Assert.Equal(8.22m, FinanceRounding.Money(result.Sum(x => x.Interest)));
    }

    [Fact]
    public void RateSegments_StrictlyBeforeStart_AndChangeStartsNewSegmentOnItsDate()
    {
        var rates = new[] { new InterestRatePoint(new DateOnly(2024, 1, 1), 10m), new InterestRatePoint(new DateOnly(2024, 9, 13), 13.75m) };

        var segments = InterestCalculator.RateSegments(new DateOnly(2024, 9, 1), new DateOnly(2024, 9, 30), rates);

        Assert.Collection(segments,
            s => { Assert.Equal(new DateOnly(2024, 9, 1), s.From); Assert.Equal(new DateOnly(2024, 9, 12), s.To); Assert.Equal(10m, s.AnnualRate); },
            s => { Assert.Equal(new DateOnly(2024, 9, 13), s.From); Assert.Equal(new DateOnly(2024, 9, 30), s.To); Assert.Equal(13.75m, s.AnnualRate); });
    }

    [Fact]
    public void RateSegments_RateDatedOnStartDay_IsNotTheOpeningRateButStartsImmediately()
    {
        var rates = new[] { new InterestRatePoint(new DateOnly(2024, 1, 1), 10m), new InterestRatePoint(new DateOnly(2024, 9, 1), 12m) };

        var segment = Assert.Single(InterestCalculator.RateSegments(new DateOnly(2024, 9, 1), new DateOnly(2024, 9, 30), rates));
        Assert.Equal(12m, segment.AnnualRate);
    }

    [Fact]
    public void RateSegments_NoRateBeforeStart_Throws()
    {
        var ex = Assert.Throws<DomainRuleException>(() =>
            InterestCalculator.RateSegments(new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 31), [new(new DateOnly(2024, 1, 1), 10m)]));
        Assert.Equal("interest.rate-not-found", ex.Code);
    }

    [Fact]
    public void CalculateRows_RunningBalance_DayOfChangeUsesNewBalance_OnlyPositive()
    {
        var from = new DateOnly(2025, 3, 1);
        var to = new DateOnly(2025, 3, 31);
        var movements = new[]
        {
            new InterestMovement(7, "10001", new DateOnly(2025, 2, 25), 1000m),  // opening
            new InterestMovement(7, "10001", from, 500m),                          // on start day -> opening
            new InterestMovement(7, "10001", new DateOnly(2025, 3, 11), -2000m),  // payment -> negative balance
            new InterestMovement(7, "10001", new DateOnly(2025, 3, 31), 3000m),   // on the last day -> 1 day
        };

        var rows = InterestCalculator.CalculateRows(from, to, TenPercent, movements);

        Assert.Collection(rows,
            r => { Assert.Equal((from, new DateOnly(2025, 3, 10), 10, 1500m), (r.From, r.To, r.Days, r.Balance)); Assert.Equal(0.00273973m, r.Coefficient); },
            r => { Assert.Equal((11, 20, -500m, 0m), (r.From.Day, r.Days, r.Balance, r.Interest)); },
            r => { Assert.Equal((31, 1, 2500m), (r.From.Day, r.Days, r.Balance)); Assert.Equal(0.00027397m, r.Coefficient); });
        Assert.Equal(31, rows.Sum(x => x.Days));
        // 0.00273973*1500 + 0.00027397*2500 = 4.109595 + 0.684925 = 4.79452 -> 4.79
        var total = Assert.Single(InterestCalculator.Totals(rows));
        Assert.Equal(4.79m, total.Interest);
    }

    [Fact]
    public void CalculateRows_RateChangeSplitsRows_AndLeapYearOfSegmentStart()
    {
        var rates = new[] { new InterestRatePoint(new DateOnly(2023, 1, 1), 10m), new InterestRatePoint(new DateOnly(2024, 1, 10), 20m) };
        var rows = InterestCalculator.CalculateRows(new DateOnly(2023, 12, 30), new DateOnly(2024, 1, 12), rates,
            [new InterestMovement(1, "10001", new DateOnly(2023, 1, 1), 36_500m)]);

        Assert.Collection(rows,
            r => { Assert.Equal((11, 10m), (r.Days, r.Rate)); Assert.Equal(decimal.Round(11m / 365m * 0.1m, 8, MidpointRounding.AwayFromZero), r.Coefficient); },
            r => { Assert.Equal((3, 20m), (r.Days, r.Rate)); Assert.Equal(decimal.Round(3m / 366m * 0.2m, 8, MidpointRounding.AwayFromZero), r.Coefficient); });
    }

    [Fact]
    public void Totals_RoundOnlyTheSum_AndDropNonPositive()
    {
        var rows = new[]
        {
            new InterestRow(1, "A", default, default, 1, 0m, 1m, 10m, 0m, 0.004m),
            new InterestRow(1, "A", default, default, 1, 0m, 1m, 10m, 0m, 0.004m),
            new InterestRow(2, "A", default, default, 1, 0m, -1m, 10m, 0m, 0m),
        };

        // per-row rounding would give 0.00 + 0.00; the sum 0.008 rounds to 0.01
        var total = Assert.Single(InterestCalculator.Totals(rows));
        Assert.Equal((1, 0.01m), (total.PartnerAccountId, total.Interest));
    }

    [Fact]
    public void Calculate_RejectsInvalidPeriod()
    {
        var exception = Assert.Throws<DomainRuleException>(() =>
            InterestCalculator.Calculate(100m, new DateOnly(2026, 2, 2), new DateOnly(2026, 2, 1), TenPercent));

        Assert.Equal("interest.invalid-period", exception.Code);
    }

    [Fact]
    public void Presets_FromLegacyInputs()
    {
        var presets = InterestPeriodPresets.Suggest(2609, null, null, new DateOnly(2026, 9, 25), new DateOnly(2026, 8, 25))
            .ToDictionary(x => x.Key);

        Assert.Equal((new DateOnly(2026, 8, 25), new DateOnly(2026, 8, 31)), (presets["fromPreviousDueDate"].Start, presets["fromPreviousDueDate"].End));
        Assert.Equal((new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 31)), (presets["wholeMonth"].Start, presets["wholeMonth"].End));
        Assert.Equal((new DateOnly(2026, 8, 25), new DateOnly(2026, 9, 24)), (presets["dueToDue"].Start, presets["dueToDue"].End));
        Assert.True(InterestPeriodPresets.Overlaps(new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 31), new DateOnly(2026, 8, 31), new DateOnly(2026, 9, 30)));
        Assert.False(InterestPeriodPresets.Overlaps(new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 30), new DateOnly(2026, 8, 31), new DateOnly(2026, 9, 30)));
    }
}
