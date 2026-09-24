using SzApp.Api.Features.MasterData;

namespace SzApp.UnitTests.MasterData;

public sealed class ContractsScopeResolverTests
{
    // Company A (id 1) and Company B (id 2) both sit under the same location
    // category (10). A non-Root caller only has StaffAccess to Company A.
    private static readonly (int CompanyId, int? LocationCategoryId)[] Companies =
    [
        (1, 10),
        (2, 10),
    ];

    private static readonly (int Id, int? ParentId)[] FlatCategories = [(10, null)];

    [Fact]
    public void Mode_All_NeverReturnsACompanyOutsideThePermittedSet()
    {
        var permitted = new[] { 1 };

        var result = ContractsScopeResolver.ResolveCompanyIds(
            "all", companyId: null, locationCategoryId: null,
            permittedCompanyIds: permitted, companies: Companies, locationCategories: FlatCategories);

        Assert.Equal(new HashSet<int> { 1 }, result);
        Assert.DoesNotContain(2, result);
    }

    [Fact]
    public void Mode_Location_NeverReturnsACompanyOutsideThePermittedSet_EvenWhenBothCompaniesMatchTheCategory()
    {
        var permitted = new[] { 1 };

        var result = ContractsScopeResolver.ResolveCompanyIds(
            "location", companyId: null, locationCategoryId: 10,
            permittedCompanyIds: permitted, companies: Companies, locationCategories: FlatCategories);

        Assert.Equal(new HashSet<int> { 1 }, result);
    }

    [Fact]
    public void Mode_Single_RequestingAForeignCompany_ReturnsEmpty()
    {
        var permitted = new[] { 1 };

        var result = ContractsScopeResolver.ResolveCompanyIds(
            "single", companyId: 2, locationCategoryId: null,
            permittedCompanyIds: permitted, companies: Companies, locationCategories: FlatCategories);

        Assert.Empty(result);
    }

    [Fact]
    public void Mode_Single_RequestingAPermittedCompany_ReturnsIt()
    {
        var permitted = new[] { 1, 2 };

        var result = ContractsScopeResolver.ResolveCompanyIds(
            "single", companyId: 2, locationCategoryId: null,
            permittedCompanyIds: permitted, companies: Companies, locationCategories: FlatCategories);

        Assert.Equal(new HashSet<int> { 2 }, result);
    }

    [Fact]
    public void Mode_Location_WalksDescendantCategories()
    {
        // 20 is a child of the root category 10; company 2 sits directly on 20.
        (int CompanyId, int? LocationCategoryId)[] companies = [(1, 10), (2, 20)];
        (int Id, int? ParentId)[] categories = [(10, null), (20, 10)];
        var permitted = new[] { 1, 2 };

        var result = ContractsScopeResolver.ResolveCompanyIds(
            "location", companyId: null, locationCategoryId: 10,
            permittedCompanyIds: permitted, companies: companies, locationCategories: categories);

        Assert.Equal(new HashSet<int> { 1, 2 }, result);
    }

    [Fact]
    public void Mode_Location_ExcludesASiblingBranch()
    {
        // Company 2 is under category 30, a sibling of 20 - not a descendant of 10's
        // other branch, so it must not be pulled in when scoping to category 20.
        (int CompanyId, int? LocationCategoryId)[] companies = [(1, 20), (2, 30)];
        (int Id, int? ParentId)[] categories = [(10, null), (20, 10), (30, 10)];
        var permitted = new[] { 1, 2 };

        var result = ContractsScopeResolver.ResolveCompanyIds(
            "location", companyId: null, locationCategoryId: 20,
            permittedCompanyIds: permitted, companies: companies, locationCategories: categories);

        Assert.Equal(new HashSet<int> { 1 }, result);
    }

    [Fact]
    public void UnknownMode_ReturnsEmpty()
    {
        var result = ContractsScopeResolver.ResolveCompanyIds(
            "bogus", companyId: 1, locationCategoryId: 10,
            permittedCompanyIds: [1], companies: Companies, locationCategories: FlatCategories);

        Assert.Empty(result);
    }
}
