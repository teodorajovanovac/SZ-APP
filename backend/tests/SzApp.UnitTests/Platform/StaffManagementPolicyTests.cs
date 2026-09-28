using SzApp.Api.Security;

namespace SzApp.UnitTests.Platform;

// GAP-23 / SEC-05: who may administer which staff account.
public sealed class StaffManagementPolicyTests
{
    private static readonly HashSet<int> UpravnikOf1And2 = [1, 2];

    [Fact]
    public void Root_ManagesAnyoneElse_IncludingOtherRoots() =>
        Assert.True(StaffManagementPolicy.CanManage(true, 1, new HashSet<int>(), 2, targetIsRoot: true, []));

    [Fact]
    public void NobodyManagesTheirOwnAccount_EvenRoot() =>
        Assert.False(StaffManagementPolicy.CanManage(true, 7, new HashSet<int>(), 7, true, []));

    [Fact]
    public void Upravnik_ManagesStaffWhoseCompaniesAreAllHis() =>
        Assert.True(StaffManagementPolicy.CanManage(false, 1, UpravnikOf1And2, 9, false, [1, 2]));

    [Fact]
    public void Upravnik_CannotManageStaffWithAccessOutsideHisCompanies() =>
        Assert.False(StaffManagementPolicy.CanManage(false, 1, UpravnikOf1And2, 9, false, [1, 3]));

    [Fact]
    public void Upravnik_CannotManageRoot() =>
        Assert.False(StaffManagementPolicy.CanManage(false, 1, UpravnikOf1And2, 9, true, [1]));

    [Fact]
    public void Upravnik_CannotManageStaffWithoutAnyGrant() =>
        Assert.False(StaffManagementPolicy.CanManage(false, 1, UpravnikOf1And2, 9, false, []));

    [Fact]
    public void NonAdmin_ManagesNobody() =>
        Assert.False(StaffManagementPolicy.CanManage(false, 1, new HashSet<int>(), 9, false, [1]));

    [Theory]
    [InlineData(true, true, null, true)]      // Root may create Root, no grant needed
    [InlineData(true, false, null, true)]
    [InlineData(false, true, 1, false)]       // Upravnik can never create Root
    [InlineData(false, false, 1, true)]       // ...but can create staff in his company
    [InlineData(false, false, 3, false)]      // ...not in someone else's
    [InlineData(false, false, null, false)]   // ...and must attach a company
    public void CanCreate(bool callerIsRoot, bool newIsRoot, int? companyId, bool expected) =>
        Assert.Equal(expected, StaffManagementPolicy.CanCreate(callerIsRoot, UpravnikOf1And2, newIsRoot, companyId));

    [Theory]
    [InlineData(false, false, true, false)]   // Upravnik cannot elevate to Root
    [InlineData(false, true, false, false)]   // ...or demote a Root
    [InlineData(false, false, false, true)]   // unchanged flag is fine
    [InlineData(true, false, true, true)]     // Root can
    public void CanChangeRootFlag(bool callerIsRoot, bool current, bool requested, bool expected) =>
        Assert.Equal(expected, StaffManagementPolicy.CanChangeRootFlag(callerIsRoot, current, requested));

    [Theory]
    [InlineData("SmtpPassword", true)]
    [InlineData("GmailClientSecret", true)]
    [InlineData("RefreshToken", true)]
    [InlineData("ApiKey", true)]
    [InlineData("CountryCode", false)]
    [InlineData("SmtpHost", false)]
    public void IsSecretSettingKey(string key, bool expected) =>
        Assert.Equal(expected, StaffManagementPolicy.IsSecretSettingKey(key));
}
