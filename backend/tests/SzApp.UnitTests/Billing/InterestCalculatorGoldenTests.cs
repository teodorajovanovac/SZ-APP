using SzApp.Domain;
using SzApp.Domain.Billing;

namespace SzApp.UnitTests.Billing;

public sealed class InterestCalculatorGoldenTests
{
    [Fact]
    public void Calculate_SplitsCalendarYearsAndUsesLeapYearDenominator()
    {
        var result = InterestCalculator.Calculate(10_000m,
            [new InterestPeriod(new DateOnly(2023, 12, 31), new DateOnly(2024, 1, 2), 10m)]);

        // Interest is computed from the full-precision fraction (days/daysInYear*rate/100),
        // not from the 4-decimal-rounded display Coefficient. Expected values below are
        // computed by hand from that exact formula:
        //   segment 1: 1/365 * 10/100 * 10000 = 2.739726... -> 2.74
        //   segment 2: 2/366 * 10/100 * 10000 = 5.464480... -> 5.46
        Assert.Collection(result,
            first =>
            {
                Assert.Equal(new DateOnly(2023, 12, 31), first.From);
                Assert.Equal(1, first.Days);
                Assert.Equal(0.0003m, first.Coefficient);
                Assert.Equal(2.74m, first.Interest);
            },
            second =>
            {
                Assert.Equal(new DateOnly(2024, 1, 1), second.From);
                Assert.Equal(2, second.Days);
                Assert.Equal(0.0005m, second.Coefficient);
                Assert.Equal(5.46m, second.Interest);
            });
        Assert.Equal(8.20m, result.Sum(x => x.Interest));
    }

    [Fact]
    public void Calculate_RejectsInvalidPeriod()
    {
        var exception = Assert.Throws<DomainRuleException>(() => InterestCalculator.Calculate(100m,
            [new InterestPeriod(new DateOnly(2026, 2, 2), new DateOnly(2026, 2, 1), 10m)]));

        Assert.Equal("interest.invalid-period", exception.Code);
    }
}
