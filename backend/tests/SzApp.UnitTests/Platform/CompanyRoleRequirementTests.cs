using SzApp.Api.Security;
using SzApp.Data.Entities;

namespace SzApp.UnitTests.Platform;

// SEC-02 / P12: the one role-hierarchy rule every CompanyWrite/CompanyPost/CompanyAdmin policy
// check depends on. Review must never satisfy a Moderator-or-above requirement, and the
// enum's numeric ordering (Upravnik=1 most privileged .. Review=3 least) must keep meaning
// "at least as privileged as" the way CompanyRoleHandler assumes.
public sealed class CompanyRoleRequirementTests
{
    [Theory]
    [InlineData(StaffRole.Upravnik, StaffRole.Moderator, true)]
    [InlineData(StaffRole.Moderator, StaffRole.Moderator, true)]
    [InlineData(StaffRole.Review, StaffRole.Moderator, false)]
    [InlineData(StaffRole.Moderator, StaffRole.Upravnik, false)]
    [InlineData(StaffRole.Upravnik, StaffRole.Upravnik, true)]
    [InlineData(StaffRole.Review, StaffRole.Review, true)]
    public void Satisfies_ComparesRolePrivilegeCorrectly(StaffRole actual, StaffRole minRole, bool expected)
    {
        Assert.Equal(expected, CompanyRoleRequirement.Satisfies(actual, minRole));
    }
}
