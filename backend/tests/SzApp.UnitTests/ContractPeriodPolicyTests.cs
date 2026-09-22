using SzApp.Domain;

namespace SzApp.UnitTests;

public sealed class ContractPeriodPolicyTests
{
    [Theory]
    [InlineData("2026-01-01", "2026-01-31", "2026-01-31", "2026-02-28", true)]
    [InlineData("2026-01-01", "2026-01-30", "2026-01-31", "2026-02-28", false)]
    [InlineData("2026-01-01", null, "2030-01-01", null, true)]
    public void Overlaps_UsesInclusiveOpenEndedIntervals(
        string firstStart,
        string? firstEnd,
        string secondStart,
        string? secondEnd,
        bool expected)
    {
        Assert.Equal(expected, ContractPeriodPolicy.Overlaps(
            DateOnly.Parse(firstStart),
            firstEnd is null ? null : DateOnly.Parse(firstEnd),
            DateOnly.Parse(secondStart),
            secondEnd is null ? null : DateOnly.Parse(secondEnd)));
    }
}
