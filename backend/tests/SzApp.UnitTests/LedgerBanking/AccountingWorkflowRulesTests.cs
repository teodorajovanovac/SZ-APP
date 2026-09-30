using SzApp.Domain;
using SzApp.Domain.Billing;
using SzApp.Domain.LedgerBanking;

namespace SzApp.UnitTests.LedgerBanking;

/// <summary>GAP-33 copy, FIN-33 advance reclassification, GAP-13 manual journal validation.</summary>
public sealed class AccountingWorkflowRulesTests
{
    private static readonly DateOnly Today = new(2026, 9, 30);

    [Fact]
    public void Copy_SkipsType2_AlreadyCopied_AndHonoursScope()
    {
        SupplierCopyCandidate[] source =
        [
            new(1, 10, 1, null),
            new(2, 11, 2, null),   // izvršeni trošak: never copied
            new(3, 12, 1, "O1"),
            new(4, 13, 9, null),
            new(5, 14, 1, null)    // already in the target month
        ];
        var target = new HashSet<int> { 14 };

        Assert.Equal([1, 3, 4], SupplierInvoiceCopyRules.Select(source, target, SupplierCopyScope.All).Select(x => x.Id));
        Assert.Equal([1, 4], SupplierInvoiceCopyRules.Select(source, target, SupplierCopyScope.Regular).Select(x => x.Id));
        Assert.Equal([3], SupplierInvoiceCopyRules.Select(source, target, SupplierCopyScope.Extraordinary).Select(x => x.Id));
    }

    [Theory]
    [InlineData(1500.456, 1400, 1500.46)]
    [InlineData(0, 1400.004, 1400)]
    public void PaymentOrderAmount_PrefersPostedAmount(decimal posted, decimal rsd, decimal expected) =>
        Assert.Equal(expected, SupplierInvoiceCopyRules.PaymentOrderAmount(posted, rsd));

    [Fact]
    public void Reclassification_MovesAdvanceOntoOldestOpenReferences_Balanced()
    {
        ReclassSourceLine[] lines =
        [
            new(1, "9711", new(2026, 6, 30), 1000m, 0m, InvoiceId: 11, SupplierInvoiceId: 5, SubAccountId: "10001"),
            new(2, "9711", new(2026, 7, 10), 0m, 400m),                 // partly paid → 600 open
            new(3, "9712", new(2026, 7, 31), 800m, 0m, InvoiceId: 12),  // 800 open
            new(4, "9710", new(2026, 5, 31), 300m, 0m),
            new(5, "9710", new(2026, 6, 5), 0m, 300m),                  // closed
            new(6, null, new(2026, 8, 1), 0m, 500m),                    // advances: 500 + 400 = 900
            new(7, "", new(2026, 8, 20), 0m, 400m)
        ];

        var result = AdvanceReclassificationRules.Build("2040", 77, Today, lines);

        LedgerBankingRules.ValidateJournal(result.Select(x => new PostingAmounts(x.Debit, x.Credit)));
        Assert.All(result, x => Assert.Equal(LedgerLineTypes.Rebooking, x.LineType));
        Assert.All(result, x => Assert.Equal(77, x.PartnerAccountId));
        Assert.All(result, x => Assert.Equal(0m, x.Debit));
        // P9 red storno of the advances, newest first: 400 then 500.
        Assert.Equal([-400m, -500m], result.Where(x => x.Credit < 0m).Select(x => x.Credit));
        // Oldest open reference first: 600 on 9711, the rest (300) on 9712; closed 9710 untouched.
        var byRef = result.Where(x => x.Credit > 0m).GroupBy(x => x.Parameters).ToDictionary(g => g.Key!, g => g.Sum(x => x.Credit));
        Assert.Equal(600m, byRef["9711"]);
        Assert.Equal(300m, byRef["9712"]);
        Assert.False(byRef.ContainsKey("9710"));
        var first = result.First(x => x.Parameters == "9711");
        Assert.Equal((11, 5, "10001"), (first.InvoiceId, first.SupplierInvoiceId, first.SubAccountId));
    }

    [Fact]
    public void Reclassification_CapsAtOpenAmount_AndSkipsWithoutAdvance()
    {
        ReclassSourceLine[] lines = [new(1, "A", Today, 100m, 0m), new(2, null, Today, 0m, 250m)];
        var result = AdvanceReclassificationRules.Build("2040", 1, Today, lines);
        Assert.Equal(-100m, result.Where(x => x.Credit < 0m).Sum(x => x.Credit));
        Assert.Equal(100m, result.Where(x => x.Credit > 0m).Sum(x => x.Credit));

        Assert.Empty(AdvanceReclassificationRules.Build("2040", 1, Today, [new(1, "A", Today, 100m, 0m)]));
        Assert.Empty(AdvanceReclassificationRules.Build("2040", 1, Today, [new(1, null, Today, 0m, 50m), new(2, "A", Today, 0m, 10m)]));
        // Unreferenced debit (refund) larger than the credit: no advance left.
        Assert.Empty(AdvanceReclassificationRules.Build("2040", 1, Today,
            [new(1, null, Today, 0m, 50m), new(2, null, Today, 60m, 0m), new(3, "A", Today, 100m, 0m)]));
    }

    [Fact]
    public void ManualJournal_RequiresPartnerOnPartnerAccounts_AndMatchingAccount()
    {
        Assert.Equal(100m, ManualJournalRules.Validate(
        [
            new("2040", 100m, 0m, 5, "2040"),
            new("4900", 0m, 100m, null, null)
        ]));

        Assert.Equal("journal.partner-required", Assert.Throws<DomainRuleException>(() => ManualJournalRules.Validate(
        [
            new("2040", 100m, 0m, null, null),
            new("4900", 0m, 100m, null, null)
        ])).Code);

        Assert.Equal("journal.partner-account-mismatch", Assert.Throws<DomainRuleException>(() => ManualJournalRules.Validate(
        [
            new("4350", 100m, 0m, 5, "2040"),
            new("5590", 0m, 100m, null, null)
        ])).Code);

        Assert.Equal("journal.unbalanced", Assert.Throws<DomainRuleException>(() => ManualJournalRules.Validate(
        [
            new("2410", 100m, 0m, null, null),
            new("4900", 0m, 99.99m, null, null)
        ])).Code);

        Assert.Equal("journal.account-required", Assert.Throws<DomainRuleException>(() => ManualJournalRules.Validate(
        [
            new(" ", 100m, 0m, null, null)
        ])).Code);
    }
}
