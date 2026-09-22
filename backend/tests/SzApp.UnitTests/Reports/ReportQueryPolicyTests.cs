using SzApp.Domain;
using SzApp.Domain.Reports;

namespace SzApp.UnitTests.Reports;

public sealed class ReportQueryPolicyTests
{
    [Fact]
    public void Select_WithTenantAndTypedParameters_IsAccepted()
    {
        var result = ReportQueryPolicy.Validate(
            "SELECT InvoiceId, Total FROM finance.Invoice WHERE CompanyId = @CompanyId AND PeriodYYMM = @Period");

        Assert.Equal(["CompanyId", "Period"], result.ParameterNames);
    }

    [Fact]
    public void Cte_WithTenantParameter_IsAccepted()
    {
        var result = ReportQueryPolicy.Validate(
            "WITH balances AS (SELECT CompanyId, SUM(DebitAmount-CreditAmount) Balance FROM finance.LedgerEntry WHERE CompanyId=@CompanyId GROUP BY CompanyId) SELECT * FROM balances");

        Assert.Contains("CompanyId", result.ParameterNames);
    }

    [Theory]
    [InlineData("SELECT * FROM core.Partner")]
    [InlineData("SELECT * FROM core.Partner WHERE CompanyId=@CompanyId; SELECT 1")]
    [InlineData("SELECT * FROM core.Partner -- WHERE CompanyId=@CompanyId")]
    [InlineData("SELECT * INTO #x FROM core.Partner WHERE CompanyId=@CompanyId")]
    [InlineData("UPDATE core.Partner SET Name='x' WHERE CompanyId=@CompanyId")]
    [InlineData("WITH x AS (DELETE FROM core.Partner OUTPUT deleted.Id) SELECT * FROM x WHERE @CompanyId=1")]
    [InlineData("EXEC reports.Run @CompanyId")]
    [InlineData("SELECT * FROM OPENROWSET('x','y','z') WHERE @CompanyId=1")]
    public void UnsafeSql_IsRejected(string sql)
    {
        var exception = Assert.Throws<DomainRuleException>(() => ReportQueryPolicy.Validate(sql));
        Assert.StartsWith("reports.", exception.Code);
    }
}
