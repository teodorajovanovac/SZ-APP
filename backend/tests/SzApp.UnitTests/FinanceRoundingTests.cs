using SzApp.Domain;

namespace SzApp.UnitTests;

public sealed class FinanceRoundingTests
{
    [Theory]
    [InlineData(1.005, 1.01)]
    [InlineData(-1.005, -1.01)]
    [InlineData(12.344, 12.34)]
    [InlineData(12.345, 12.35)]
    public void Money_UsesTwoDecimalsAndAwayFromZero(double input, double expected)
    {
        Assert.Equal((decimal)expected, FinanceRounding.Money((decimal)input));
    }

    [Theory]
    [InlineData(1.23445, 1.2345)]
    [InlineData(-1.23445, -1.2345)]
    public void Calculation_UsesFourDecimalsAndAwayFromZero(double input, double expected)
    {
        Assert.Equal((decimal)expected, FinanceRounding.Calculation((decimal)input));
    }

    [Fact]
    public void ConvertCurrency_RoundsOnlyAtCalculationPrecision()
    {
        Assert.Equal(11725.3545m, InvoiceCalculator.ConvertCurrency(100.123m, 117.1095m));
    }
}
