using SzApp.Domain;
using SzApp.Domain.Billing;
using SzApp.Domain.LedgerBanking;

namespace SzApp.UnitTests.Billing;

public class NoticeCostPostingRulesTests
{
    private static readonly DateOnly Date = new(2026, 9, 30);

    private static NoticeCostPostingSource Source(int id, decimal cost) =>
        new(id, 100 + id, "2040", $"251-{1000 + id}-P20260930", $"71-251-{1000 + id}-P20260930", cost);

    [Fact]
    public void Build_PostsCustomerDebitPerPartnerAndOneRevenueCredit_Balanced()
    {
        var lines = NoticeCostPostingRules.Build(Date, Date, "OP-20260930", [Source(1, 3000m), Source(2, 0m), Source(3, 7500m)]);

        Assert.Equal(3, lines.Count);
        Assert.Equal(lines.Sum(x => x.Debit), lines.Sum(x => x.Credit));
        Assert.All(lines, x => Assert.Equal(LedgerLineTypes.Invoice, x.LineType));
        var customer = lines.Where(x => x.Account == LedgerAccounts.Customers).ToArray();
        Assert.Equal([101, 103], customer.Select(x => x.PartnerAccountId!.Value));
        Assert.Equal("251-1001-P20260930", customer[0].DocumentRef);
        Assert.Equal("712511001P20260930", customer[0].Parameters);
        var revenue = Assert.Single(lines, x => x.Account == LedgerAccounts.Revenue);
        Assert.Equal(10500m, revenue.Credit);
        Assert.Null(revenue.PartnerAccountId);
    }

    [Fact]
    public void Storno_IsRedSameSide_AndBalanced()
    {
        var storno = DocumentPostingRules.Negate(NoticeCostPostingRules.Build(Date, Date, "OP", [Source(1, 3000m)]), Date.AddDays(1));

        Assert.All(storno, x => Assert.Equal(LedgerLineTypes.Storno, x.LineType));
        Assert.Equal(-3000m, storno.Single(x => x.Account == LedgerAccounts.Customers).Debit);
        Assert.Equal(-3000m, storno.Single(x => x.Account == LedgerAccounts.Revenue).Credit);
        Assert.Equal(storno.Sum(x => x.Debit), storno.Sum(x => x.Credit));
    }

    [Fact]
    public void Build_AllZeroCosts_Throws()
    {
        Assert.Throws<DomainRuleException>(() => NoticeCostPostingRules.Build(Date, Date, "OP", [Source(1, 0m)]));
    }
}
