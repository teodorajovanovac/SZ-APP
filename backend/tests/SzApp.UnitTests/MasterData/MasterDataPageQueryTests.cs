using SzApp.Api.Features.MasterData;

namespace SzApp.UnitTests.MasterData;

public sealed class MasterDataPageQueryTests
{
    [Fact]
    public void AllPropertiesAreOptional_SoAsParametersDoesNotRequireThem()
    {
        // Root cause of the live 400s: [AsParameters] treats a non-nullable value-type
        // property as a REQUIRED query parameter even when it has a C# field-initializer
        // default, because reflection can't see that default. Every query-string-bound
        // property here must be a nullable type (or string) so a request that omits it
        // (e.g. no "&descending=...") still binds successfully.
        // Only settable (init/set) properties are bindable query parameters - the
        // computed Normalized* properties are get-only and irrelevant to the binder.
        foreach (var property in typeof(MasterDataPageQuery).GetProperties().Where(p => p.CanWrite))
        {
            var type = property.PropertyType;
            var isOptional = !type.IsValueType || Nullable.GetUnderlyingType(type) is not null;
            Assert.True(isOptional, $"{property.Name} must be nullable (or a reference type) for [AsParameters] binding to treat it as optional.");
        }
    }

    [Fact]
    public void Normalize_AppliesDefaults_WhenEveryQueryParameterIsOmitted()
    {
        var query = new MasterDataPageQuery();

        Assert.Equal(1, query.NormalizedPage);
        Assert.Equal(25, query.NormalizedPageSize);
        Assert.False(query.NormalizedDescending);
        Assert.Equal(0, query.Skip);
        Assert.Equal(string.Empty, query.NormalizedSearch);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-5, 1)]
    [InlineData(3, 3)]
    public void NormalizedPage_ClampsToAtLeastOne(int page, int expected) =>
        Assert.Equal(expected, new MasterDataPageQuery { Page = page }.NormalizedPage);

    [Theory]
    [InlineData(0, 1)]
    [InlineData(500, 100)]
    [InlineData(40, 40)]
    public void NormalizedPageSize_ClampsBetweenOneAndMaximum(int pageSize, int expected) =>
        Assert.Equal(expected, new MasterDataPageQuery { PageSize = pageSize }.NormalizedPageSize);
}
