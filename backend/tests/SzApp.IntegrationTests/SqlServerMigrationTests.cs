using Microsoft.EntityFrameworkCore;
using SzApp.Data;
using Testcontainers.MsSql;

namespace SzApp.IntegrationTests;

public sealed class SqlServerMigrationTests
{
    [Fact]
    public async Task InitialMigration_CreatesCleanSqlServerDatabase_WhenDockerGateIsEnabled()
    {
        if (!string.Equals(
                Environment.GetEnvironmentVariable("SZAPP_RUN_SQL_INTEGRATION"),
                "1",
                StringComparison.Ordinal))
        {
            return;
        }

        await using var sqlServer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2025-latest")
            .Build();
        await sqlServer.StartAsync();

        var options = new DbContextOptionsBuilder<SzAppDbContext>()
            .UseSqlServer(sqlServer.GetConnectionString())
            .Options;
        await using var context = new SzAppDbContext(options);

        await context.Database.MigrateAsync();

        Assert.True(await context.Database.CanConnectAsync());
        Assert.Contains("InitialFoundation", (await context.Database.GetAppliedMigrationsAsync()).Single());
    }
}
