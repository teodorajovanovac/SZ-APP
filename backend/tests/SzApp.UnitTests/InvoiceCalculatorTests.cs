using SzApp.Domain;

namespace SzApp.UnitTests;

public sealed class InvoiceCalculatorTests
{
    [Fact]
    public void Calculate_ProducesGoldenInvoiceTotals()
    {
        var result = InvoiceCalculator.Calculate(
            grossBase: 10_000.005m,
            vatRate: 20m,
            benefitAmount: 500.005m,
            interestAmount: 125.555m);

        Assert.Equal(new InvoiceAmounts(
            GrossBase: 10_000.01m,
            BenefitAmount: 500.01m,
            NetBase: 9_500.00m,
            VatAmount: 1_900.00m,
            InterestAmount: 125.56m,
            Total: 11_525.56m), result);
    }

    [Theory]
    [InlineData(-1, 0, 0, 0, "invoice.negative-base")]
    [InlineData(100, 101, 0, 0, "invoice.invalid-vat-rate")]
    [InlineData(100, 20, 101, 0, "invoice.invalid-benefit")]
    [InlineData(100, 20, 0, -1, "invoice.negative-interest")]
    public void Calculate_RejectsInvalidInputs(
        double grossBase,
        double vatRate,
        double benefit,
        double interest,
        string expectedCode)
    {
        var exception = Assert.Throws<DomainRuleException>(() => InvoiceCalculator.Calculate(
            (decimal)grossBase,
            (decimal)vatRate,
            (decimal)benefit,
            (decimal)interest));

        Assert.Equal(expectedCode, exception.Code);
    }
}
