using SzApp.Domain.Platform;

namespace SzApp.UnitTests.Platform;

public sealed class EventWorkflowPolicyTests
{
    [Fact]
    public void Approve_RejectsSelfApproval()
    {
        Assert.Throws<InvalidOperationException>(() => EventWorkflowPolicy.EnsureCanApprove(
            PlatformEventStatus.Requested, 7, 7, "Upravnik", false, null));
    }

    [Fact]
    public void Approve_AllowsAuditedRootEmergencyOverride()
    {
        EventWorkflowPolicy.EnsureCanApprove(
            PlatformEventStatus.Requested, 7, 7, "Root", true, "Hitna korekcija pre obračuna");
    }

    [Theory]
    [InlineData("Moderator")]
    [InlineData("Review")]
    public void Approve_RejectsUnauthorizedRole(string role)
    {
        Assert.Throws<UnauthorizedAccessException>(() => EventWorkflowPolicy.EnsureCanApprove(
            PlatformEventStatus.Requested, 7, 8, role, false, null));
    }

    [Fact]
    public void Execute_RequiresApprovedState()
    {
        Assert.Throws<InvalidOperationException>(() =>
            EventWorkflowPolicy.EnsureCanExecute(PlatformEventStatus.Requested, "Moderator"));
    }
}

