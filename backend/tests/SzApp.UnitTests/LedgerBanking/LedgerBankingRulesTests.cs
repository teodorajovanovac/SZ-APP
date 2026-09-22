using SzApp.Domain;
using SzApp.Domain.LedgerBanking;

namespace SzApp.UnitTests.LedgerBanking;

public sealed class LedgerBankingRulesTests
{
    [Fact]
    public void ValidateJournal_BalancesAtTwoDecimals()
    {
        var result = LedgerBankingRules.ValidateJournal([
            new PostingAmounts(50.004m, 0m),
            new PostingAmounts(49.996m, 0m),
            new PostingAmounts(0m, 100m)
        ]);

        Assert.Equal(100m, result);
    }

    [Fact]
    public void ValidateJournal_RejectsZeroAndTwoSidedLines()
    {
        Assert.Equal("journal.invalid-line", Assert.Throws<DomainRuleException>(() =>
            LedgerBankingRules.ValidateJournal([new PostingAmounts(0m, 0m)])).Code);
        Assert.Equal("journal.invalid-line", Assert.Throws<DomainRuleException>(() =>
            LedgerBankingRules.ValidateJournal([new PostingAmounts(1m, 1m)])).Code);
    }

    [Fact]
    public void ReconcileStatement_ReturnsVerifiedTotalsAndCounts()
    {
        var result = LedgerBankingRules.ReconcileStatement(
            1_000m,
            125.50m,
            300m,
            1_174.50m,
            [new PostingAmounts(125.50m, 0m), new PostingAmounts(0m, 200m), new PostingAmounts(0m, 100m)]);

        Assert.Equal(new StatementTotals(1_000m, 125.50m, 300m, 1_174.50m, 1, 2), result);
    }

    [Fact]
    public void ReconcileStatement_RejectsHeaderAndBalanceMismatch()
    {
        var header = Assert.Throws<DomainRuleException>(() => LedgerBankingRules.ReconcileStatement(
            100m, 1m, 0m, 99m, [new PostingAmounts(2m, 0m)]));
        Assert.Equal("statement.header-lines-mismatch", header.Code);

        var balance = Assert.Throws<DomainRuleException>(() => LedgerBankingRules.ReconcileStatement(
            100m, 1m, 0m, 100m, [new PostingAmounts(1m, 0m)]));
        Assert.Equal("statement.balance-mismatch", balance.Code);
    }

    [Theory]
    [InlineData("InvoiceBatch")]
    [InlineData("SupplierInvoice")]
    [InlineData("BankStatement")]
    public void PostingSchemeRegistry_AcceptsOnlyRegisteredSources(string sourceType)
    {
        PostingSchemeRegistry.EnsureAllowedSource(sourceType);
        Assert.Equal("posting.source-not-allowed", Assert.Throws<DomainRuleException>(() =>
            PostingSchemeRegistry.EnsureAllowedSource("System.Reflection.Evil")).Code);
    }
}
