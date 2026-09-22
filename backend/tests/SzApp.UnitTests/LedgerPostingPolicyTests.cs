using SzApp.Domain;

namespace SzApp.UnitTests;

public sealed class LedgerPostingPolicyTests
{
    [Fact]
    public void EmptyJournal_IsRejected()
    {
        var exception = Assert.Throws<DomainRuleException>(() =>
            LedgerPostingPolicy.ValidateAndGetBalance([]));

        Assert.Equal("journal.empty", exception.Code);
    }

    [Fact]
    public void UnbalancedJournal_IsRejected()
    {
        var lines = new[]
        {
            new LedgerPostingLine(100m, 0m),
            new LedgerPostingLine(0m, 99.9999m)
        };

        var exception = Assert.Throws<DomainRuleException>(() =>
            LedgerPostingPolicy.ValidateAndGetBalance(lines));

        Assert.Equal("journal.unbalanced", exception.Code);
    }

    [Fact]
    public void BalancedJournal_ReturnsFourDecimalTurnover()
    {
        var lines = new[]
        {
            new LedgerPostingLine(50.55555m, 0m),
            new LedgerPostingLine(49.44445m, 0m),
            new LedgerPostingLine(0m, 100m)
        };

        Assert.Equal(100m, LedgerPostingPolicy.ValidateAndGetBalance(lines));
    }

    [Fact]
    public void LineWithBothSides_IsRejected()
    {
        var exception = Assert.Throws<DomainRuleException>(() =>
            LedgerPostingPolicy.ValidateAndGetBalance([new LedgerPostingLine(1m, 1m)]));

        Assert.Equal("journal.invalid-line", exception.Code);
    }
}
