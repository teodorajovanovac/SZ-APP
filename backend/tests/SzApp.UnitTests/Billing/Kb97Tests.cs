using SzApp.Domain.Billing;

namespace SzApp.UnitTests.Billing;

public sealed class Kb97Tests
{
    // 9.1 test vectors, verified against real legacy RBR payment references.
    [Theory]
    [InlineData("251-1001-2506", "08")]
    [InlineData("252-1299-2511", "46")]
    [InlineData("251-1379-2608", "69")]
    [InlineData("252-4102-2606-O1", "81")]
    public void Compute_MatchesLegacyVectors(string raw, string expected) =>
        Assert.Equal(expected, Kb97.Compute(raw));

    [Fact]
    public void WithControl_PrependsControlDigits()
    {
        Assert.Equal("08-251-1001-2506", Kb97.WithControl("251-1001-2506"));
    }
}
