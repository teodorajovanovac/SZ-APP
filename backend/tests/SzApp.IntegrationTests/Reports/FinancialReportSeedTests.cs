using SzApp.Data.Migrations;
using SzApp.Domain.Reports;

namespace SzApp.IntegrationTests.Reports;

/// <summary>GAP-18: the seeded Fin_Izvestaj_SZ queries must pass the report engine's read-only policy.</summary>
public sealed class FinancialReportSeedTests
{
    [Theory]
    [InlineData(FinancialReportSeed.IncomeExpenseSql)]
    [InlineData(FinancialReportSeed.CollectionSql)]
    public void SeededQuery_PassesReportPolicy(string escapedSql)
    {
        var validated = ReportQueryPolicy.Validate(escapedSql.Replace("''", "'"));
        Assert.Equal(["CompanyId", "DateFrom", "DateTo"], validated.ParameterNames);
    }
}
