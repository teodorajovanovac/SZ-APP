using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SzApp.Data;
using SzApp.Data.Configurations.Reports;
using Testcontainers.MsSql;

namespace SzApp.IntegrationTests.Reports;

/// <summary>GAP-18: every seeded legacy report compiles against the real migrated schema (docker gate).</summary>
public sealed class LegacyReportSqlTests
{
    [Fact]
    public async Task SeededReports_CompileAgainstMigratedSchema_WhenDockerGateIsEnabled()
    {
        if (Environment.GetEnvironmentVariable("SZAPP_RUN_SQL_INTEGRATION") != "1") return;

        await using var sqlServer = new MsSqlBuilder().WithImage("mcr.microsoft.com/mssql/server:2025-latest").Build();
        await sqlServer.StartAsync();
        var options = new DbContextOptionsBuilder<SzAppDbContext>().UseSqlServer(sqlServer.GetConnectionString()).Options;
        await using (var context = new SzAppDbContext(options)) await context.Database.MigrateAsync();

        await using var connection = new SqlConnection(sqlServer.GetConnectionString());
        await connection.OpenAsync();
        await using (var count = new SqlCommand("SELECT COUNT(*) FROM reports.ReportDefinitionDetail", connection))
            Assert.True((int)(await count.ExecuteScalarAsync())! >= LegacyReportCatalog.Reports.Sum(x => x.Sections.Count));

        var failures = new List<string>();
        foreach (var section in LegacyReportCatalog.Reports.SelectMany(x => x.Sections))
        {
            await using var command = new SqlCommand("sp_describe_first_result_set", connection) { CommandType = System.Data.CommandType.StoredProcedure };
            command.Parameters.AddWithValue("@tsql", section.Sql);
            command.Parameters.AddWithValue("@params", "@CompanyId int, @DateFrom nvarchar(10), @DateTo nvarchar(10)");
            try { await using var reader = await command.ExecuteReaderAsync(); }
            catch (SqlException exception) { failures.Add($"{section.QueryName}: {exception.Message}"); }
        }

        foreach (var export in SzApp.Api.Features.Export.LegacyFormatExports.All)
        {
            await using var command = new SqlCommand("sp_describe_first_result_set", connection) { CommandType = System.Data.CommandType.StoredProcedure };
            command.Parameters.AddWithValue("@tsql", export.Sql);
            command.Parameters.AddWithValue("@params", "@ids nvarchar(max), @yymm int");
            try { await using var reader = await command.ExecuteReaderAsync(); }
            catch (SqlException exception) { failures.Add($"{export.Name}: {exception.Message}"); }
        }

        Assert.Empty(failures);
    }
}
