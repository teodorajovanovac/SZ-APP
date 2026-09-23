using SzApp.Worker;

namespace SzApp.UnitTests.Worker;

public sealed class EmailOutboxBackoffTests
{
    [Theory]
    [InlineData(1, 2)]
    [InlineData(2, 4)]
    [InlineData(3, 8)]
    [InlineData(6, 60)] // 2^6 = 64, capped at 60
    [InlineData(10, 60)] // stays capped for further attempts
    public void ComputeBackoffDelay_DoublesUntilCappedAtSixtyMinutes(int attemptCount, double expectedMinutes)
    {
        var delay = EmailOutboxMessageHandler.ComputeBackoffDelay(attemptCount);

        Assert.Equal(expectedMinutes, delay.TotalMinutes);
    }
}
