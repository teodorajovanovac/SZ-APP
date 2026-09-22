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

        Assert.Collection(result,
            first =>
            {
                Assert.Equal(new DateOnly(2023, 12, 31), first.From);
                Assert.Equal(1, first.Days);
                Assert.Equal(0.0003m, first.Coefficient);
                Assert.Equal(3.00m, first.Interest);
            },
            second =>
            {
                Assert.Equal(new DateOnly(2024, 1, 1), second.From);
                Assert.Equal(2, second.Days);
                Assert.Equal(0.0005m, second.Coefficient);
                Assert.Equal(5.00m, second.Interest);
            });
        Assert.Equal(8.00m, result.Sum(x => x.Interest));
    }

    [Fact]
    public void Calculate_RejectsInvalidPeriod()
    {
        var exception = Assert.Throws<DomainRuleException>(() => InterestCalculator.Calculate(100m,
            [new InterestPeriod(new DateOnly(2026, 2, 2), new DateOnly(2026, 2, 1), 10m)]));

        Assert.Equal("interest.invalid-period", exception.Code);
    }
}
