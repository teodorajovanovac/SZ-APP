using SzApp.Data.Configurations.Reports;
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

/// <summary>GAP-18: every seeded legacy report section is read-only, company-scoped and shares one parameter set per report.</summary>
public sealed class LegacyReportCatalogTests
{
    public static TheoryData<string> Names => new(LegacyReportCatalog.Reports.Select(x => x.Name));

    [Theory]
    [MemberData(nameof(Names))]
    public void Report_PassesPolicy_WithUniformParameters(string name)
    {
        var report = LegacyReportCatalog.Reports.Single(x => x.Name == name);
        var parameterSets = report.Sections.Select(s => string.Join(",", ReportQueryPolicy.Validate(s.Sql).ParameterNames)).Distinct().ToArray();
        var parameters = Assert.Single(parameterSets);
        Assert.StartsWith("CompanyId", parameters);
        Assert.True(report.Name.Length <= 255 && report.Title.Length <= 255);
    }
}
