using SzApp.Domain;
using SzApp.Domain.LedgerBanking;

namespace SzApp.UnitTests.LedgerBanking;

public sealed class DocumentPostingRulesTests
{
    private static readonly DateOnly Turnover = new(2026, 8, 31);
    private static readonly DateOnly Due = new(2026, 9, 15);

    // 10 = type 1 "predviđeni" (fond, gets 4350/5590), 20 = type 2 (4900 only in the batch).
    private static readonly Dictionary<int, SupplierPostingInfo> Suppliers = new()
    {
        [10] = new SupplierPostingInfo(10, SupplierDocumentTypes.Planned, 900, "4350", "10001", 5, "97-123-45", "Održavanje"),
        [20] = new SupplierPostingInfo(20, SupplierDocumentTypes.Actual, 901, "4350", "10002", 2, null, "Čišćenje")
    };

    private static InvoicePostingSource Line(int invoiceId, int? supplierId, decimal amount) =>
        new(invoiceId, 500 + invoiceId, "2040", Due, $"97-{invoiceId}-26", supplierId, amount);

    private static IReadOnlyList<PostingLine> Batch() => DocumentPostingRules.BuildInvoiceBatch(Turnover, "R-2608",
    [
        Line(1, 10, 1000m), Line(1, 10, 200.50m), Line(1, 20, 300m),
        Line(2, 10, 800m), Line(2, 20, 0m),
        Line(3, 20, 0m) // zero-only invoice: skipped entirely
    ], Suppliers);

    private static void AssertBalanced(IEnumerable<PostingLine> lines) =>
        LedgerBankingRules.ValidateJournal(lines.Select(x => new PostingAmounts(x.Debit, x.Credit)));

    [Fact]
    public void InvoiceBatch_Posts2040PerInvoiceAndSupplierInvoice_AndBalances()
    {
        var lines = Batch();
        AssertBalanced(lines);

        var customer = lines.Where(x => x.Account == "2040").ToArray();
        Assert.Equal(3, customer.Length); // (1,10) (1,20) (2,10); zero groups skipped
        Assert.All(customer, x =>
        {
            Assert.Equal(LedgerLineTypes.Invoice, x.LineType);
            Assert.Equal(Turnover, x.PostingDate);
            Assert.Equal(Due, x.DueDate);
            Assert.Equal("R-2608", x.DocumentRef);
            Assert.True(x.Debit > 0m && x.Credit == 0m);
            Assert.NotNull(x.InvoiceId);
            Assert.DoesNotContain("-", x.Parameters);
        });
        var first = customer.Single(x => x.InvoiceId == 1 && x.SupplierInvoiceId == 10);
        Assert.Equal(1200.50m, first.Debit);
        Assert.Equal(501, first.PartnerAccountId);
        Assert.Equal("10001", first.SubAccountId);
        Assert.Equal(5, first.CollectionPriority);
        Assert.Equal("97126", first.Parameters);

        var revenue = lines.Where(x => x.Account == "4900").ToDictionary(x => x.SupplierInvoiceId!.Value);
        Assert.Equal(2000.50m, revenue[10].Credit);
        Assert.Equal(300m, revenue[20].Credit);

        // Only the type-1 supplier invoice gets the fond pair 4350 C (partner = supplier) / 5590 D, type 4.
        var fond = lines.Single(x => x.Account == "4350");
        Assert.Equal((10, 900, 2000.50m, LedgerLineTypes.SupplierInvoice), (fond.SupplierInvoiceId!.Value, fond.PartnerAccountId!.Value, fond.Credit, fond.LineType));
        Assert.Equal(2000.50m, lines.Single(x => x.Account == "5590").Debit);
    }

    [Fact]
    public void InvoiceStorno_IsRedStornoOnSameSide_AndNetsInvoiceToZero()
    {
        var single = DocumentPostingRules.BuildInvoiceBatch(Turnover, "R-2608", [Line(1, 10, 1200.50m), Line(1, 20, 300m)], Suppliers);
        var stornoDate = new DateOnly(2026, 9, 28);
        var storno = DocumentPostingRules.BuildInvoiceStorno(stornoDate, single.Where(x => x.Account == "2040"), Suppliers);

        AssertBalanced(storno);
        Assert.All(storno, x =>
        {
            Assert.Equal(LedgerLineTypes.Storno, x.LineType);
            Assert.Equal(stornoDate, x.PostingDate);
            Assert.True(x.Debit <= 0m && x.Credit <= 0m); // negative, never flipped to the other side
        });
        foreach (var account in single.Concat(storno).GroupBy(x => (x.Account, x.SupplierInvoiceId, x.InvoiceId)))
        {
            Assert.Equal(0m, account.Sum(x => x.Debit));
            Assert.Equal(0m, account.Sum(x => x.Credit));
        }
    }

    [Fact]
    public void SupplierInvoice_Type2CreditsSupplier_Type3SwapsSidesAsStorno()
    {
        var actual = new SupplierPostingInfo(30, SupplierDocumentTypes.Actual, 902, "4350", "10003", 1, "12-34", "Lift",
            "RT-7", 1500m, new DateOnly(2026, 8, 10), new DateOnly(2026, 9, 10));

        var posted = DocumentPostingRules.BuildSupplierInvoice(actual);
        AssertBalanced(posted);
        var supplier = posted.Single(x => x.Account == "4350");
        Assert.Equal((0m, 1500m, 902, LedgerLineTypes.SupplierInvoice), (supplier.Debit, supplier.Credit, supplier.PartnerAccountId!.Value, supplier.LineType));
        Assert.Equal(1500m, posted.Single(x => x.Account == "5590").Debit);

        var credit = DocumentPostingRules.BuildSupplierInvoice(actual with { DocumentType = SupplierDocumentTypes.CreditNote });
        AssertBalanced(credit);
        Assert.Equal(1500m, credit.Single(x => x.Account == "4350").Debit);
        Assert.Equal(1500m, credit.Single(x => x.Account == "5590").Credit);
        Assert.All(credit, x => Assert.Equal(LedgerLineTypes.Storno, x.LineType));

        Assert.Equal("5800", DocumentPostingRules.BuildSupplierInvoice(actual with { ClosesAccount = "5800" })[1].Account);
        Assert.Equal("posting.supplier-document-type", Assert.Throws<DomainRuleException>(() =>
            DocumentPostingRules.BuildSupplierInvoice(actual with { DocumentType = SupplierDocumentTypes.Planned })).Code);
    }

    [Fact]
    public void ValidateJournal_AcceptsRedStornoNegatives()
    {
        Assert.Equal(-50m, LedgerBankingRules.ValidateJournal([new PostingAmounts(-50m, 0m), new PostingAmounts(0m, -50m)]));
    }

    [Fact]
    public void PostingPeriod_LockedMonthRejects_OpenMonthPasses()
    {
        var locked = new HashSet<int> { 2608 };
        var ex = Assert.Throws<PostingPeriodLockedException>(() =>
            PostingPeriod.EnsureOpen([new DateOnly(2026, 9, 1), new DateOnly(2026, 8, 31)], locked));
        Assert.Equal(2608, ex.PeriodYYMM);

        PostingPeriod.EnsureOpen([new DateOnly(2026, 9, 1), new DateOnly(2025, 8, 31)], locked);
        Assert.Throws<DomainRuleException>(() => PostingPeriod.EnsureValid(2613));
    }

    [Fact]
    public void InvoiceBatch_Interest_2040OnInterestSubAccount_WithAndWithoutType9Supplier()
    {
        var suppliers = new Dictionary<int, SupplierPostingInfo>(Suppliers)
        {
            [90] = new SupplierPostingInfo(90, SupplierDocumentTypes.Interest, 950, "4350", "11309", 1, null, "Kamata upravnik")
        };
        var lines = DocumentPostingRules.BuildInvoiceBatch(Turnover, "R-2608", [Line(1, 10, 100m)], suppliers,
        [
            new InterestPostingSource(1, 501, "2040", Due, "97-1-26", "11309", 90, 8.22m),
            new InterestPostingSource(1, 501, "2040", Due, "97-1-26", "10009", null, 1.50m),
            new InterestPostingSource(1, 501, "2040", Due, "97-1-26", "10009", null, 0m) // not > 0: skipped
        ]);
        AssertBalanced(lines);

        var interest = lines.Where(x => x.Account == "2040" && x.Note == DocumentPostingRules.InterestNote).ToArray();
        Assert.Equal(2, interest.Length);
        Assert.All(interest, x => { Assert.Equal(LedgerLineTypes.Invoice, x.LineType); Assert.Equal(501, x.PartnerAccountId); Assert.Equal(1, x.InvoiceId); });
        Assert.Equal(8.22m, interest.Single(x => x.SubAccountId == "11309").Debit);

        // Type-9 supplier: 4900 C + 4350 C on the supplier + 5590 D; without one: 4900 C on the interest sub-account only.
        Assert.Equal(8.22m, lines.Single(x => x.Account == "4900" && x.SupplierInvoiceId == 90).Credit);
        Assert.Equal(8.22m, lines.Single(x => x.Account == "4350" && x.SupplierInvoiceId == 90 && x.PartnerAccountId == 950).Credit);
        Assert.Equal(8.22m, lines.Single(x => x.Account == "5590" && x.SupplierInvoiceId == 90).Debit);
        Assert.Equal(1.50m, lines.Single(x => x.Account == "4900" && x.SubAccountId == "10009" && x.SupplierInvoiceId == null).Credit);
    }

    [Fact]
    public void InvoiceBatch_GroupMember_StornoedAndPostedOnMaster()
    {
        var master = new GroupInvoiceTarget(9, 777, "2040", Due.AddDays(1), "97-9-26");
        var lines = DocumentPostingRules.BuildInvoiceBatch(Turnover, "R-2608",
            [Line(1, 10, 100m), Line(1, 20, 50m), Line(2, 10, 30m)], Suppliers,
            groupTargets: new Dictionary<int, GroupInvoiceTarget> { [1] = master });
        AssertBalanced(lines);

        var member = lines.Where(x => x.Account == "2040" && x.InvoiceId == 1).ToArray();
        Assert.Equal(4, member.Length); // original + red storno per line
        Assert.Equal(0m, member.Sum(x => x.Debit));
        Assert.All(member.Where(x => x.Debit < 0m), x => Assert.Equal(LedgerLineTypes.Invoice, x.LineType));

        var onMaster = lines.Where(x => x.InvoiceId == 9).ToArray();
        Assert.Equal(150m, onMaster.Sum(x => x.Debit));
        Assert.All(onMaster, x =>
        {
            Assert.Equal(777, x.PartnerAccountId);
            Assert.Equal(master.DueDate, x.DueDate);
            Assert.Equal("97926", x.Parameters);
        });
        Assert.Equal("10001", onMaster.Single(x => x.SupplierInvoiceId == 10).SubAccountId); // sub-account kept

        // Supplier side unchanged by the move: revenue = everything invoiced once.
        Assert.Equal(180m, lines.Where(x => x.Account == "4900").Sum(x => x.Credit));
        // Non-member untouched.
        Assert.Single(lines, x => x.Account == "2040" && x.InvoiceId == 2);
    }
}
