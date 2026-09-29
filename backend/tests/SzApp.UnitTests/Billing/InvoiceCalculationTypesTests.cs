using SzApp.Domain.Billing;
using Xunit;

namespace SzApp.UnitTests.Billing;

public sealed class InvoiceCalculationTypesTests
{
    private static R0Aggregate Row(
        decimal invoiceAmountRsd = 0m, decimal invoiceAmountEur = 0m,
        decimal perCoefRsd = 0m, decimal perCoefEur = 0m,
        decimal sumK1K2 = 0m, decimal sumK2 = 0m,
        decimal k1 = 0m, decimal k2 = 0m, decimal k3 = 0m,
        decimal k2xk3 = 0m, decimal k2xk4 = 0m, decimal k2xk5 = 0m,
        decimal sumK1 = 0m, decimal sumK4 = 0m) =>
        new(invoiceAmountRsd, invoiceAmountEur, perCoefRsd, perCoefEur, sumK1K2, sumK2,
            k1, k2, k3, k2xk3, k2xk4, k2xk5, sumK1, sumK4);

    [Fact]
    public void Type9_RsdPerCoefficient_UsesK1()
    {
        var result = InvoiceCalculationTypes.Calculate(9, Row(perCoefRsd: 250.00m, k1: 4m), vatRatePercent: 20m, nbsRate: 117.5m);

        Assert.NotNull(result);
        Assert.Equal(1000.00m, result!.NetAmount);
        Assert.Equal(200.00m, result.VatAmount);
        Assert.Equal(1200.00m, result.TotalAmount);
    }

    [Fact]
    public void Type60_EurPerCoefficient_UsesK2()
    {
        var result = InvoiceCalculationTypes.Calculate(60, Row(perCoefEur: 2.00m, k2: 3m), vatRatePercent: 0m, nbsRate: 117.50m);

        Assert.Equal(705.00m, result!.NetAmount);
        Assert.Equal(0m, result.VatAmount);
        Assert.Equal(705.00m, result.TotalAmount);
    }

    [Fact]
    public void Type65_EurPerCoefficient_UsesK3()
    {
        var result = InvoiceCalculationTypes.Calculate(65, Row(perCoefEur: 1.50m, k3: 2m), vatRatePercent: 0m, nbsRate: 100.00m);

        Assert.Equal(300.00m, result!.NetAmount);
    }

    [Fact]
    public void Type62_EurPerCoefficient_UsesK2xK4()
    {
        var result = InvoiceCalculationTypes.Calculate(62, Row(perCoefEur: 4.00m, k2xk4: 1.25m), vatRatePercent: 0m, nbsRate: 100.00m);

        Assert.Equal(500.00m, result!.NetAmount);
    }

    [Fact]
    public void Type59_EurPerCoefficient_UsesK1_WithVat()
    {
        var result = InvoiceCalculationTypes.Calculate(59, Row(perCoefEur: 3.00m, k1: 2m), vatRatePercent: 20m, nbsRate: 100.00m);

        Assert.Equal(600.00m, result!.NetAmount);
        Assert.Equal(120.00m, result.VatAmount);
        Assert.Equal(720.00m, result.TotalAmount);
    }

    [Fact]
    public void Type1_RsdDistributedOverSumK1K2()
    {
        // 1000 RSD invoice spread over SUM(K1*K2)=10 across all (incl. inactive) units; this row's K1=4.
        var result = InvoiceCalculationTypes.Calculate(1, Row(invoiceAmountRsd: 1000m, sumK1K2: 10m, k1: 4m), vatRatePercent: 0m, nbsRate: 100m);

        Assert.Equal(400.00m, result!.NetAmount);
    }

    [Fact]
    public void Type99_IsNeverInvoiced()
    {
        var result = InvoiceCalculationTypes.Calculate(99, Row(), vatRatePercent: 20m, nbsRate: 100m);

        Assert.Null(result);
    }

    [Fact]
    public void EurConversion_HasNoIntermediateRounding()
    {
        // CenaE=1, NBS=117.145 -> Iznos is NOT pre-rounded to 117.15 before multiplying by Kol=3.
        // Correct (unrounded intermediate): Round(3 * 117.145, 2) = Round(351.435, 2) = 351.44.
        // Wrong (pre-rounded intermediate): Round(3 * 117.15, 2) = 351.45.
        var result = InvoiceCalculationTypes.Calculate(59, Row(perCoefEur: 1m, k1: 3m), vatRatePercent: 0m, nbsRate: 117.145m);

        Assert.Equal(351.44m, result!.NetAmount);
    }

    [Fact]
    public void UnknownCalculationType_Throws()
    {
        Assert.Throws<SzApp.Domain.DomainRuleException>(() =>
            InvoiceCalculationTypes.Calculate(999, Row(), 20m, 100m));
    }
}
