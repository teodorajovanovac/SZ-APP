using SzApp.Domain.Platform;

namespace SzApp.UnitTests.Platform;

public sealed class ImportAllowlistRegistryTests
{
    private readonly ImportAllowlistRegistry _registry = new();

    [Fact]
    public void AllowsKnownTargetField() => Assert.True(_registry.IsAllowedTarget("Partner", "TaxNumber"));

    [Theory]
    [InlineData("Staff", "PasswordHash")]
    [InlineData("Partner", "DROP TABLE")]
    [InlineData("LedgerEntry", "DebitAmount")]
    public void RejectsUnknownTarget(string table, string field) => Assert.False(_registry.IsAllowedTarget(table, field));

    [Fact]
    public void LookupRequiresExactAllowlistedShape()
    {
        Assert.True(_registry.IsAllowedLookup("BankAccount", "AccountNumber", "Id"));
        Assert.False(_registry.IsAllowedLookup("BankAccount", "AccountNumber", "CompanyId"));
    }
}

