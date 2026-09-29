using SzApp.Domain.LedgerBanking;

namespace SzApp.UnitTests.LedgerBanking;

public sealed class BankStatementMatcherTests
{
    private const int PartnerId = 700;
    private const string Account = "2040";

    private static OpenItem Item(string? reference, decimal balance, DateOnly date, long id, int priority = 0) => new()
    {
        PartnerAccountId = PartnerId,
        Parameters = reference,
        DocumentRef = $"R-{id}",
        InvoiceId = (int)id,
        CollectionPriority = priority,
        FirstDate = date,
        FirstId = id,
        Balance = balance
    };

    private static StatementLineFacts Inflow(decimal amount, string? reference) =>
        new("Petar Petrovic", "160-1-1", null, 221, reference, 0m, amount);

    [Fact]
    public void Allocate_SplitsByReference_OrderedByDateThenPriorityThenId()
    {
        // Two open groups share the payment reference; the older/lower-priority one should be closed first.
        var items = new[]
        {
            Item("97-1", 200m, new DateOnly(2026, 1, 10), 2, priority: 5),
            Item("97-1", 100m, new DateOnly(2026, 1, 5), 1, priority: 1),
        };
        var line = Inflow(250m, "97-1");

        var result = BankStatementMatcher.Allocate(line, PartnerId, Account, items, new MatcherOptions());

        Assert.Equal(2, result.Count);
        Assert.Equal(AllocationKinds.Reference, result[0].Kind);
        Assert.Equal(100m, result[0].Amount); // FirstId=1, earliest date -> closed first, fully
        Assert.Equal(150m, result[1].Amount); // remaining 150 of the 200 group
        Assert.All(result, x => Assert.Equal(AllocationKinds.Reference, x.Kind));
    }

    [Fact]
    public void Allocate_RemainderGoesFifoToOldestOtherReference()
    {
        var items = new[]
        {
            Item("97-1", 50m, new DateOnly(2026, 1, 1), 1),   // the line's own reference
            Item("97-2", 30m, new DateOnly(2026, 1, 2), 2),   // older other reference
            Item("97-3", 40m, new DateOnly(2026, 1, 3), 3),   // newer other reference
        };
        var line = Inflow(90m, "97-1"); // 50 closes its own reference, 40 remains for FIFO

        var result = BankStatementMatcher.Allocate(line, PartnerId, Account, items, new MatcherOptions());

        Assert.Equal(3, result.Count);
        Assert.Equal(50m, result[0].Amount);
        Assert.Equal(AllocationKinds.Reference, result[0].Kind);
        Assert.Equal(30m, result[1].Amount); // 97-2, older
        Assert.Equal(AllocationKinds.Fifo, result[1].Kind);
        Assert.Equal(10m, result[2].Amount); // 97-3 only partially closed
        Assert.Equal(AllocationKinds.Fifo, result[2].Kind);
    }

    [Fact]
    public void Allocate_SurplusBecomesAdvanceWithNoReference()
    {
        var items = new[] { Item("97-1", 50m, new DateOnly(2026, 1, 1), 1) };
        var line = Inflow(200m, "97-1");

        var result = BankStatementMatcher.Allocate(line, PartnerId, Account, items, new MatcherOptions());

        Assert.Equal(2, result.Count);
        Assert.Equal(50m, result[0].Amount);
        var advance = result[1];
        Assert.Equal(AllocationKinds.Advance, advance.Kind);
        Assert.Equal(150m, advance.Amount);
        Assert.Null(advance.Parameters);
    }

    [Fact]
    public void Allocate_NoOpenItems_WholeAmountIsAdvance()
    {
        var line = Inflow(75m, null);
        var result = BankStatementMatcher.Allocate(line, PartnerId, Account, [], new MatcherOptions());

        var single = Assert.Single(result);
        Assert.Equal(AllocationKinds.Advance, single.Kind);
        Assert.Equal(75m, single.Amount);
    }

    [Fact]
    public void Allocate_SumOfPartsEqualsLineAmount()
    {
        var items = new[]
        {
            Item("97-1", 20m, new DateOnly(2026, 1, 1), 1),
            Item("97-2", 10m, new DateOnly(2026, 1, 2), 2),
        };
        var line = Inflow(45m, "97-1");
        var result = BankStatementMatcher.Allocate(line, PartnerId, Account, items, new MatcherOptions());
        Assert.Equal(45m, result.Sum(x => x.Amount));
    }

    // Legacy GK_KnjizenjePoParametrimaRacuna touches every sub-group of the reference with
    // balance <> 0, including one whose balance already sits on the opposite side (an
    // overpayment) -- while the outer NetOpen check (mFilterDIPI) still requires the reference's
    // TOTAL balance to sit in the payment's direction. With the option off, the opposite-direction
    // group is skipped and the whole amount comes from the same-direction group instead.
    [Fact]
    public void Allocate_OppositeDirectionGroup_OnlyIncludedWhenOptionIsOn()
    {
        var groupA = () => Item("97-1", 40m, new DateOnly(2026, 1, 5), 2);   // owed, matches inflow direction
        var groupB = () => Item("97-1", -10m, new DateOnly(2026, 1, 1), 1); // already overpaid, opposite direction, but earlier
        var line = Inflow(30m, "97-1"); // NetOpen = 40 - 10 = 30 > 0, so the reference is still touched

        var withOption = BankStatementMatcher.Allocate(line, PartnerId, Account, [groupB(), groupA()], new MatcherOptions(IncludeOppositeDirectionGroups: true));
        var withoutOption = BankStatementMatcher.Allocate(line, PartnerId, Account, [groupB(), groupA()], new MatcherOptions(IncludeOppositeDirectionGroups: false));

        Assert.Equal(30m, withOption.Sum(x => x.Amount));
        Assert.Equal(30m, withoutOption.Sum(x => x.Amount));

        // On: touches the earlier opposite-direction group first (negative part), then the rest from A.
        Assert.Equal(2, withOption.Count);
        Assert.Contains(withOption, x => x.Amount == -10m && x.DocumentRef == "R-1");
        Assert.Contains(withOption, x => x.Amount == 40m && x.DocumentRef == "R-2");

        // Off: group B (negative) is skipped entirely, the whole amount comes from A alone.
        var single = Assert.Single(withoutOption);
        Assert.Equal(30m, single.Amount);
        Assert.Equal("R-2", single.DocumentRef);
    }

    [Fact]
    public void FindTemplate_AllConditionsMustMatch()
    {
        var templates = new[]
        {
            new MatchTemplate(1, "Intesa fee", 9108, null,
            [
                new TemplateCondition("BrojRacuna", TemplateMatch.Equals, "160-26-95"),
                new TemplateCondition("Odobrenje", TemplateMatch.Equals, "0"),
            ]),
        };
        var matchingLine = new StatementLineFacts("Banca Intesa", "160-26-95", null, 221, null, 50m, 0m);
        var nonMatchingLine = new StatementLineFacts("Banca Intesa", "160-26-95", null, 221, null, 0m, 50m); // Odobrenje != 0

        Assert.NotNull(BankStatementMatcher.FindTemplate(matchingLine, templates));
        Assert.Null(BankStatementMatcher.FindTemplate(nonMatchingLine, templates));
    }

    [Fact]
    public void FindTemplate_StartsWithFunction()
    {
        var templates = new[]
        {
            new MatchTemplate(1, "Any Intesa", 9108, null,
            [
                new TemplateCondition("BrojRacuna", TemplateMatch.StartsWith, "160-"),
            ]),
        };
        var line = new StatementLineFacts("X", "160-99-11", null, null, null, 10m, 0m);
        Assert.NotNull(BankStatementMatcher.FindTemplate(line, templates));
    }
}
