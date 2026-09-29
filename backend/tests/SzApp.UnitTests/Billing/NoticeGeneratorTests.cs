using SzApp.Domain.Billing;

namespace SzApp.UnitTests.Billing;

public sealed class NoticeGeneratorTests
{
    private static readonly DateOnly PaymentCutoff = new(2026, 9, 30);
    private static readonly DateOnly DebtCutoff = new(2026, 9, 30);

    [Theory]
    // credit line on/before payment cutoff counts
    [InlineData(0, 100, "2026-09-30", true)]
    [InlineData(0, 100, "2026-10-01", false)]
    // debit <= 0 (storno/credit-note-style debit) on/before payment cutoff counts
    [InlineData(-50, 0, "2026-09-30", true)]
    [InlineData(-50, 0, "2026-10-01", false)]
    [InlineData(0, 0, "2026-09-30", true)]
    // positive debit uses the debt cutoff, not the payment cutoff
    [InlineData(200, 0, "2026-09-30", true)]
    [InlineData(200, 0, "2026-10-01", false)]
    public void PassesDateFilter_UsesExactLegacyPredicate(decimal debit, decimal credit, string date, bool expected) =>
        Assert.Equal(expected, NoticeGenerator.PassesDateFilter(debit, credit, DateOnly.Parse(date), PaymentCutoff, DebtCutoff));

    [Fact]
    public void Generate_GroupsByDocumentAndInvoice_ExcludesGroupsBelowMonthlyTolerance_RequiresMinLines()
    {
        var lines = new[]
        {
            // Partner 1: three invoice groups above the monthly tolerance -> qualifies (MinBNR=3).
            new NoticeGlLine(1, 1001, "R-2601", 10, 1000m, 0m, new(2026, 9, 1), new(2026, 9, 25), "Racun 1"),
            new NoticeGlLine(1, 1001, "R-2602", 11, 1000m, 0m, new(2026, 9, 1), new(2026, 9, 25), "Racun 2"),
            new NoticeGlLine(1, 1001, "R-2603", 12, 1000m, 0m, new(2026, 9, 1), new(2026, 9, 25), "Racun 3"),
            // a tiny group under the per-document tolerance (1) is excluded from both count and debt.
            new NoticeGlLine(1, 1001, "R-2604", 13, 0.5m, 0m, new(2026, 9, 1), new(2026, 9, 25), "Racun 4"),

            // Partner 2: only two qualifying groups -> below MinBNR, excluded entirely.
            new NoticeGlLine(2, 1002, "R-2601", 20, 1000m, 0m, new(2026, 9, 1), new(2026, 9, 25), "Racun 1"),
            new NoticeGlLine(2, 1002, "R-2602", 21, 1000m, 0m, new(2026, 9, 1), new(2026, 9, 25), "Racun 2"),
        };

        var result = NoticeGenerator.Generate(lines, PaymentCutoff, DebtCutoff, debtTolerance: 1m, debtToleranceByMonth: 1m, minLineCount: 3);

        var candidate = Assert.Single(result);
        Assert.Equal(1, candidate.PartnerAccountId);
        Assert.Equal(3, candidate.Lines.Count);
        Assert.Equal(3000m, candidate.Debt);
    }

    [Fact]
    public void Generate_PaymentClosesAgainstInvoiceDocumentRef_MergesIntoSameGroup()
    {
        // A payment line carries the invoice's DocumentRef (interest also merges this way) -- same
        // (DocumentRef, InvoiceId) key nets the payment against the charge in one group.
        var lines = new[]
        {
            new NoticeGlLine(1, 1001, "R-2601", 10, 1000m, 0m, new(2026, 9, 1), new(2026, 9, 25), "Racun"),
            new NoticeGlLine(1, 1001, "R-2601", 10, 0m, 300m, new(2026, 9, 15), null, "Uplata"),
            new NoticeGlLine(1, 1001, "R-2602", 11, 1000m, 0m, new(2026, 9, 1), new(2026, 9, 25), "Racun"),
            new NoticeGlLine(1, 1001, "R-2603", 12, 1000m, 0m, new(2026, 9, 1), new(2026, 9, 25), "Racun"),
        };

        var result = NoticeGenerator.Generate(lines, PaymentCutoff, DebtCutoff, debtTolerance: 1m, debtToleranceByMonth: 1m, minLineCount: 3);

        var candidate = Assert.Single(result);
        Assert.Equal(3, candidate.Lines.Count);
        Assert.Equal(2700m, candidate.Debt); // 700 + 1000 + 1000, overpayment on other documents not netted
    }

    [Fact]
    public void Generate_OverallBalanceAtOrBelowTolerance_ExcludesPartner()
    {
        var lines = new[]
        {
            new NoticeGlLine(1, 1001, "R-1", 1, 1m, 0m, new(2026, 9, 1), null, null),
            new NoticeGlLine(1, 1001, "R-2", 2, 0.5m, 0m, new(2026, 9, 1), null, null),
        };

        var result = NoticeGenerator.Generate(lines, PaymentCutoff, DebtCutoff, debtTolerance: 1.5m, debtToleranceByMonth: 0m, minLineCount: 1);

        Assert.Empty(result);
    }

    [Fact]
    public void Generate_DebtFeedsNoticeCostCalculator_ThresholdCrossing()
    {
        // FIN-12 + P11: the computed Dug is what NoticeCostCalculator prices, end to end.
        var thresholds = new NoticeCostThresholds(5_000m, 50_000m, 7_500m);
        var lines = new[]
        {
            new NoticeGlLine(1, 1001, "R-1", 1, 20_000m, 0m, new(2026, 9, 1), null, null),
            new NoticeGlLine(1, 1001, "R-2", 2, 20_000m, 0m, new(2026, 9, 1), null, null),
            new NoticeGlLine(1, 1001, "R-3", 3, 20_000m, 0m, new(2026, 9, 1), null, null),
        };

        var candidate = Assert.Single(NoticeGenerator.Generate(lines, PaymentCutoff, DebtCutoff, 1m, 1m, 3));
        Assert.Equal(60_000m, candidate.Debt);
        Assert.Equal(7_500m, NoticeCostCalculator.Cost(candidate.Debt, thresholds, carriesCost: true));
    }
}
