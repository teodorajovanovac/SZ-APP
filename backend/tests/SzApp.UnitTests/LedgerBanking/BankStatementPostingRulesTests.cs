using SzApp.Domain;
using SzApp.Domain.LedgerBanking;

namespace SzApp.UnitTests.LedgerBanking;

public sealed class BankStatementPostingRulesTests
{
    private static readonly DateOnly Date = new(2026, 3, 15);

    private static StatementPostingPart Part(int lineId, bool inflow, string account, int? partnerAccountId, decimal amount, string? reference = null) =>
        new(lineId, inflow, "Petar Petrovic", new AllocationProposal(account, partnerAccountId, amount, AllocationKinds.Reference, Parameters: reference));

    [Fact]
    public void Build_ProducesTwoBankSummaryLines_DebitInflowsCreditOutflows()
    {
        var lines = new[] { (LineId: 1, Debit: 0m, Credit: 100m), (LineId: 2, Debit: 40m, Credit: 0m) };
        var parts = new[]
        {
            Part(1, true, "2040", 700, 100m),
            Part(2, false, "4350", 800, 40m),
        };

        var result = BankStatementPostingRules.Build("2410", 5, Date, lines, parts);

        var bankLines = result.Where(x => x.Account == "2410").ToArray();
        Assert.Equal(2, bankLines.Length);
        var debitLine = Assert.Single(bankLines, x => x.Debit > 0m);
        var creditLine = Assert.Single(bankLines, x => x.Credit > 0m);
        Assert.Equal(100m, debitLine.Debit);
        Assert.Equal(0m, debitLine.Credit);
        Assert.Equal(40m, creditLine.Credit);
        Assert.Equal(0m, creditLine.Debit);
        Assert.Null(debitLine.PartnerAccountId);
        Assert.Null(creditLine.PartnerAccountId);
        Assert.Equal("IZVOD 5/2026", debitLine.DocumentRef);
        Assert.Equal("IZVOD 5/2026", creditLine.DocumentRef);
    }

    [Fact]
    public void Build_InflowGoesToPartnerCredit_OutflowToPartnerDebit()
    {
        var lines = new[] { (LineId: 1, Debit: 0m, Credit: 100m), (LineId: 2, Debit: 40m, Credit: 0m) };
        var parts = new[]
        {
            Part(1, true, "2040", 700, 100m),
            Part(2, false, "4350", 800, 40m),
        };

        var result = BankStatementPostingRules.Build("2410", 5, Date, lines, parts);

        var customer = Assert.Single(result, x => x.Account == "2040");
        Assert.Equal(0m, customer.Debit);
        Assert.Equal(100m, customer.Credit);
        Assert.Equal(700, customer.PartnerAccountId);

        var supplier = Assert.Single(result, x => x.Account == "4350");
        Assert.Equal(40m, supplier.Debit);
        Assert.Equal(0m, supplier.Credit);
        Assert.Equal(800, supplier.PartnerAccountId);
    }

    [Fact]
    public void Build_ResultIsAlwaysBalanced()
    {
        var lines = new[] { (LineId: 1, Debit: 0m, Credit: 250m), (LineId: 2, Debit: 90m, Credit: 0m) };
        var parts = new[]
        {
            Part(1, true, "2040", 700, 150m, "97-1"),
            Part(1, true, "2040", 700, 100m), // advance part of the same line, no reference
            Part(2, false, "4350", 800, 90m),
        };

        var result = BankStatementPostingRules.Build("2410", 8, Date, lines, parts);

        LedgerBankingRules.ValidateJournal(result.Select(x => new PostingAmounts(x.Debit, x.Credit)));
    }

    [Fact]
    public void Build_RejectsALineThatIsNotFullyAllocated()
    {
        var lines = new[] { (LineId: 1, Debit: 0m, Credit: 100m) };
        var parts = new[] { Part(1, true, "2040", 700, 60m) }; // only 60 of 100

        var ex = Assert.Throws<DomainRuleException>(() => BankStatementPostingRules.Build("2410", 1, Date, lines, parts));
        Assert.Equal("statement.line-not-allocated", ex.Code);
    }

    [Fact]
    public void Negate_UnpostingNetsToZero()
    {
        var lines = new[] { (LineId: 1, Debit: 0m, Credit: 100m), (LineId: 2, Debit: 40m, Credit: 0m) };
        var parts = new[]
        {
            Part(1, true, "2040", 700, 100m),
            Part(2, false, "4350", 800, 40m),
        };
        var posted = BankStatementPostingRules.Build("2410", 5, Date, lines, parts);

        var storno = DocumentPostingRules.Negate(posted, Date);
        var combined = posted.Concat(storno).ToArray();

        // Every account's debit/credit sum nets to zero once the storno is added.
        foreach (var account in combined.Select(x => x.Account).Distinct())
        {
            var debit = combined.Where(x => x.Account == account).Sum(x => x.Debit);
            var credit = combined.Where(x => x.Account == account).Sum(x => x.Credit);
            Assert.Equal(debit, credit);
        }

        Assert.All(storno, x => Assert.Equal(LedgerLineTypes.Storno, x.LineType));
        LedgerBankingRules.ValidateJournal(storno.Select(x => new PostingAmounts(x.Debit, x.Credit)));
    }
}
