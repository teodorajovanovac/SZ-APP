using SzApp.Domain;
using SzApp.Domain.Billing;

namespace SzApp.UnitTests.Billing;

public sealed class BillingCalculatorGoldenTests
{
    [Fact]
    public void CalculateLine_UsesAwayFromZeroAtMoneyBoundary()
    {
        var result = BillingCalculator.CalculateLine(new BillingLineInput(
            "Održavanje", 3m, 0.335m, 20m));

        Assert.Equal(1.01m, result.NetAmount);
        Assert.Equal(0.20m, result.VatAmount);
        Assert.Equal(1.21m, result.TotalAmount);
    }

    [Fact]
    public void CalculateInvoice_GoldenMixedVatBenefitAndInterest()
    {
        BillingLineInput[] lines =
        [
            new("Sitna stavka", 3m, 0.335m, 20m),
            new("Usluga", 2m, 10m, 10m, K1: 0.5m)
        ];

        var result = BillingCalculator.CalculateInvoice(lines, benefitAmount: 1.01m, interestAmount: 0.005m);

        Assert.Equal(11.01m, result.NetAmount);
        Assert.Equal(1.01m, result.BenefitAmount);
        Assert.Equal(10.00m, result.TaxableAmount);
        Assert.Equal(1.09m, result.VatAmount);
        Assert.Equal(0.01m, result.InterestAmount);
        Assert.Equal(11.10m, result.TotalAmount);
    }

    [Theory]
    [InlineData(1.005, 1.01)]
    [InlineData(2.345, 2.35)]
    [InlineData(-1.005, -1.01)]
    public void MoneyRounding_IsAlwaysAwayFromZero(decimal value, decimal expected) =>
        Assert.Equal(expected, FinanceRounding.Money(value));

    [Fact]
    public void NoticeTotal_IsRoundedAndAdditive() =>
        Assert.Equal(1250.01m, BillingCalculator.CalculateNoticeTotal(1000.005m, 250.004m));

    [Fact]
    public void DeterministicKey_IsStableAndSensitiveToInput()
    {
        var first = BillingCalculator.CreateDeterministicKey(new { Period = 2609, Total = 100.00m });
        var same = BillingCalculator.CreateDeterministicKey(new { Period = 2609, Total = 100.00m });
        var changed = BillingCalculator.CreateDeterministicKey(new { Period = 2609, Total = 100.01m });

        Assert.Equal(64, first.Length);
        Assert.Equal(first, same);
        Assert.NotEqual(first, changed);
    }

    [Fact]
    public void InvalidBenefit_IsRejected()
    {
        var error = Assert.Throws<DomainRuleException>(() =>
            BillingCalculator.CalculateInvoice([new("Stavka", 1m, 100m, 20m)], benefitAmount: 100.01m));

        Assert.Equal("billing.invalid-benefit", error.Code);
    }
}
