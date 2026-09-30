using SzApp.Domain.Billing;
using Xunit;

namespace SzApp.UnitTests.Billing;

public sealed class InvoiceGenerationEngineTests
{
    [Fact]
    public void Type9_PerCoefficient_SplitsPerCustomer_IgnoringInactiveInDenominatorOnly()
    {
        // Two customers, unit type 1: customer A has 1 active unit (K1=2), customer B has
        // 1 active + 1 INACTIVE unit (K1=3 active, K1=5 inactive). Type 9 uses K1 directly per
        // customer (no shared denominator), so the inactive unit must not count for anyone.
        var units = new[]
        {
            new R0UnitRow(CustomerId: 1, UnitTypeId: 1, IsActive: true, K1: 2m, K2: 1m, K3: 1m, K4: 1m, K5: 1m),
            new R0UnitRow(CustomerId: 2, UnitTypeId: 1, IsActive: true, K1: 3m, K2: 1m, K3: 1m, K4: 1m, K5: 1m),
            new R0UnitRow(CustomerId: 2, UnitTypeId: 1, IsActive: false, K1: 5m, K2: 1m, K3: 1m, K4: 1m, K5: 1m),
        };
        var invoices = new[]
        {
            new R0SupplierInput(SupplierInvoiceId: 100, SupplierPartnerAccountId: 9001, CalculationTypeId: 9,
                UnitTypeIds: [1], InvoiceAmountRsd: 0m, InvoiceAmountEur: 0m,
                PerCoefficientAmountRsd: 250m, PerCoefficientAmountEur: 0m, VatRatePercent: 0m),
        };

        var lines = InvoiceGenerationEngine.GenerateLines(units, invoices, nbsRate: 117.5m);

        Assert.Equal(2, lines.Count);
        var customerA = Assert.Single(lines, x => x.CustomerId == 1);
        Assert.Equal(500.00m, customerA.Result.NetAmount); // 250 * K1(2)
        var customerB = Assert.Single(lines, x => x.CustomerId == 2);
        Assert.Equal(750.00m, customerB.Result.NetAmount); // 250 * K1(3), inactive unit excluded
    }

    [Fact]
    public void Type1_DenominatorIncludesInactiveUnits_P3()
    {
        // SumK1K2 denominator must include the inactive unit even though it produces no line of its own.
        var units = new[]
        {
            new R0UnitRow(1, 1, true, K1: 4m, K2: 1m, K3: 1m, K4: 1m, K5: 1m),
            new R0UnitRow(2, 1, false, K1: 6m, K2: 1m, K3: 1m, K4: 1m, K5: 1m), // inactive, denominator only
        };
        var invoices = new[]
        {
            new R0SupplierInput(1, 9001, 1, [1], InvoiceAmountRsd: 1000m, InvoiceAmountEur: 0m,
                PerCoefficientAmountRsd: 0m, PerCoefficientAmountEur: 0m, VatRatePercent: 0m),
        };

        var lines = InvoiceGenerationEngine.GenerateLines(units, invoices, nbsRate: 100m);

        var only = Assert.Single(lines); // customer 2's only unit is inactive -> no line for it
        Assert.Equal(1, only.CustomerId);
        // SumK1K2 = 4*1 + 6*1 = 10; customer 1's K1xK2-quantity path (K1=4) -> amount = 1000/10 = 100; Suma = 100*4 = 400
        Assert.Equal(400.00m, only.Result.NetAmount);
    }

    [Fact]
    public void Type99_NeverProducesALine()
    {
        var units = new[] { new R0UnitRow(1, 1, true, 1m, 1m, 1m, 1m, 1m) };
        var invoices = new[] { new R0SupplierInput(1, 9001, 99, [1], 100m, 0m, 0m, 0m, 0m) };

        Assert.Empty(InvoiceGenerationEngine.GenerateLines(units, invoices, 100m));
    }

    [Fact]
    public void InvoiceCarriers_MembersGoToMaster_CancelledCarryNothing()
    {
        var carriers = InvoiceGenerationEngine.InvoiceCarriers(
        [
            new InvoiceCarrierRow(1, 100, false, null),  // plain invoice
            new InvoiceCarrierRow(2, 200, true, 9),      // group member of master 9
            new InvoiceCarrierRow(9, 900, false, null),  // group master
            new InvoiceCarrierRow(3, 300, true, null),   // user-cancelled
            new InvoiceCarrierRow(4, 400, true, 5),      // member of a cancelled master
            new InvoiceCarrierRow(5, 500, true, null),
        ]);

        Assert.Equal(1, carriers[100]);
        Assert.Equal(9, carriers[200]);
        Assert.Equal(9, carriers[900]);
        Assert.False(carriers.ContainsKey(300));
        Assert.False(carriers.ContainsKey(400));
    }

    [Theory]
    [InlineData(110, 100, 10)]
    [InlineData(90, 100, -10)]
    [InlineData(100, 300, -66.67)]
    public void ChangePercent_AgainstPreviousBatch(decimal current, decimal previous, decimal expected) =>
        Assert.Equal(expected, InvoiceGenerationEngine.ChangePercent(current, previous));

    [Fact]
    public void ChangePercent_NoPrevious_IsNull()
    {
        Assert.Null(InvoiceGenerationEngine.ChangePercent(100m, null));
        Assert.Null(InvoiceGenerationEngine.ChangePercent(100m, 0m));
    }
}
