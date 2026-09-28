using SzApp.Etl.Pipeline;

namespace SzApp.UnitTests.Etl;

// SEC-04: legacy Staff.Password must never reach etl.RawStagingRow / etl.QuarantineRecord.
public sealed class SensitiveColumnFilterTests
{
    [Theory]
    [InlineData("Password")]
    [InlineData("password")]
    [InlineData("StaffLogin")]
    [InlineData("Lozinka")]
    public void AnySensitive_DetectsKnownPasswordColumns(string header) =>
        Assert.True(SensitiveColumnFilter.AnySensitive([header]));

    [Fact]
    public void AnySensitive_IsFalse_ForOrdinaryHeaders() =>
        Assert.False(SensitiveColumnFilter.AnySensitive(["StaffId", "UserName", "IsActive"]));

    [Fact]
    public void Strip_RemovesPasswordColumn_KeepsRest()
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["StaffId"] = "1",
            ["UserName"] = "korisnik@example.com",
            ["Password"] = "plaintext-secret"
        };

        var safe = SensitiveColumnFilter.Strip(values);

        Assert.False(safe.ContainsKey("Password"));
        Assert.Equal("1", safe["StaffId"]);
        Assert.Equal("korisnik@example.com", safe["UserName"]);
        Assert.DoesNotContain("plaintext-secret", safe.Values);
    }

    [Fact]
    public void Strip_ReturnsSameValues_WhenNoSensitiveColumnPresent()
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["CompanyId"] = "251",
            ["ShortName"] = "Bulevar 24"
        };

        var safe = SensitiveColumnFilter.Strip(values);

        Assert.Equal(values.Count, safe.Count);
        Assert.Equal("251", safe["CompanyId"]);
    }
}
