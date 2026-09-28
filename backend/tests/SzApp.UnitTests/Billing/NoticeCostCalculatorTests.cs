using SzApp.Domain;
using SzApp.Domain.Billing;

namespace SzApp.UnitTests.Billing;

public sealed class NoticeCostCalculatorTests
{
    private static readonly NoticeCostThresholds Today = new(5_000m, 50_000m, 7_500m);

    [Theory]
    [InlineData(49_999.99, 5_000)]
    [InlineData(50_000, 7_500)]
    [InlineData(120_000, 7_500)]
    [InlineData(0, 5_000)]
    public void Cost_BelowLimitLower_AtOrAboveLimitUpper(decimal debt, decimal expected) =>
        Assert.Equal(expected, NoticeCostCalculator.Cost(debt, Today, carriesCost: true));

    [Fact]
    public void Cost_NotificationOrNoRule_IsZero()
    {
        Assert.Equal(0m, NoticeCostCalculator.Cost(100_000m, Today, NoticeTypes.CarriesCost(NoticeTypes.Notification)));
        Assert.Equal(0m, NoticeCostCalculator.Cost(100_000m, null, carriesCost: true));
        Assert.True(NoticeTypes.CarriesCost(NoticeTypes.Standalone));
    }

    [Fact]
    public void Applicable_CompanyBeatsGlobal_AndRespectsDateRange()
    {
        var rules = new[]
        {
            new NoticeCostRule(1, new DateOnly(2020, 1, 1), new DateOnly(2025, 12, 31), null, 3_000m, 35_000m, 4_500m),
            new NoticeCostRule(2, new DateOnly(2026, 1, 1), null, null, 5_000m, 50_000m, 7_500m),
            new NoticeCostRule(3, new DateOnly(2026, 6, 1), new DateOnly(2026, 6, 30), 251, 1_000m, 10_000m, 2_000m),
        };

        Assert.Equal(1, NoticeCostCalculator.Applicable(rules, 251, new DateOnly(2025, 12, 31))!.Id);
        Assert.Equal(2, NoticeCostCalculator.Applicable(rules, 251, new DateOnly(2026, 1, 1))!.Id);
        Assert.Equal(3, NoticeCostCalculator.Applicable(rules, 251, new DateOnly(2026, 6, 15))!.Id);
        Assert.Equal(2, NoticeCostCalculator.Applicable(rules, 252, new DateOnly(2026, 6, 15))!.Id);
        Assert.Equal(2, NoticeCostCalculator.Applicable(rules, 251, new DateOnly(2026, 7, 1))!.Id);
        Assert.Null(NoticeCostCalculator.Applicable(rules, 251, new DateOnly(2019, 12, 31)));
    }

    [Fact]
    public void EnsureValid_RejectsOverlapInSameScopeOnly()
    {
        var existing = new[]
        {
            new NoticeCostRule(1, new DateOnly(2026, 1, 1), null, null, 5_000m, 50_000m, 7_500m),
            new NoticeCostRule(2, new DateOnly(2026, 1, 1), new DateOnly(2026, 3, 31), 251, 1m, 2m, 3m),
        };

        var global = Assert.Throws<DomainRuleException>(() =>
            NoticeCostCalculator.EnsureValid(new NoticeCostRule(0, new DateOnly(2027, 1, 1), null, null, 1m, 2m, 3m), existing));
        Assert.Equal("notice-costs.overlap", global.Code);
        Assert.Throws<DomainRuleException>(() =>
            NoticeCostCalculator.EnsureValid(new NoticeCostRule(0, new DateOnly(2026, 3, 31), null, 251, 1m, 2m, 3m), existing));

        // different company, adjacent period, and editing itself are fine
        NoticeCostCalculator.EnsureValid(new NoticeCostRule(0, new DateOnly(2026, 1, 1), null, 252, 1m, 2m, 3m), existing);
        NoticeCostCalculator.EnsureValid(new NoticeCostRule(0, new DateOnly(2026, 4, 1), null, 251, 1m, 2m, 3m), existing);
        NoticeCostCalculator.EnsureValid(existing[0] with { UpperAmount = 8_000m }, existing);
    }

    [Fact]
    public void EnsureValid_RejectsEndBeforeStartAndNegativeAmounts()
    {
        Assert.Equal("notice-costs.invalid-period", Assert.Throws<DomainRuleException>(() =>
            NoticeCostCalculator.EnsureValid(new NoticeCostRule(0, new DateOnly(2026, 2, 1), new DateOnly(2026, 1, 31), null, 1m, 2m, 3m), [])).Code);
        Assert.Equal("notice-costs.negative-amount", Assert.Throws<DomainRuleException>(() =>
            NoticeCostCalculator.EnsureValid(new NoticeCostRule(0, new DateOnly(2026, 2, 1), null, null, -1m, 2m, 3m), [])).Code);
    }
}
