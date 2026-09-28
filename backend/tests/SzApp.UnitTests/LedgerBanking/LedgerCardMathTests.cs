using SzApp.Domain.LedgerBanking;

namespace SzApp.UnitTests.LedgerBanking;

public sealed class LedgerCardMathTests
{
    [Fact]
    public void Running_StartsFromOpening_AndEndsAtClosing()
    {
        (decimal, decimal)[] lines = [(1000m, 0m), (0m, 400m), (-100m, 0m), (0m, 500m)]; // -100 = red storno
        var running = LedgerCardMath.Running(250m, lines);

        Assert.Equal([1250m, 850m, 750m, 250m], running);
        var closing = 250m + lines.Sum(x => x.Item1) - lines.Sum(x => x.Item2);
        Assert.Equal(closing, running[^1]);
    }

    [Fact]
    public void Running_EmptyCard_IsEmpty() => Assert.Empty(LedgerCardMath.Running(10m, []));

    [Fact]
    public void OpenKeys_KeepsOnlyReferencesThatDoNotNetToZero()
    {
        var keys = LedgerCardMath.OpenKeys([
            ("97-1", 1000m, 0m), ("97-1", 0m, 1000m),        // paid
            ("97-2", 500m, 0m), ("97-2", 0m, 200m),          // 300 open
            ("97-3", 0m, 50m),                               // advance (credit) is open too
            ("97-4", 100m, 0m), ("97-4", -100m, 0m),         // storno nets out
            ("97-5", 10.0001m, 0m), ("97-5", 0m, 10m),       // sub-para residue = closed
            (null, 70m, 0m), ("", 0m, 20m)                   // null and "" share a bucket
        ]);

        Assert.Equal(["", "97-2", "97-3"], keys.Order().ToArray());
    }

    [Theory]
    [InlineData(1000, 400, 900, 600)]  // 1000 due, 400 paid, 900 outstanding → 600 overdue
    [InlineData(300, 500, 200, 0)]     // payments exceed what's due → nothing overdue
    [InlineData(1000, 0, 700, 700)]    // capped at outstanding
    [InlineData(100, 0, -50, 0)]       // overpaid partner
    public void Overdue_IsFifoAndClamped(decimal due, decimal paid, decimal outstanding, decimal expected) =>
        Assert.Equal(expected, LedgerCardMath.Overdue(due, paid, outstanding));
}
