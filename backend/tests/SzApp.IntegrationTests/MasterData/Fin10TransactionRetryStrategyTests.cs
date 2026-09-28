using Microsoft.EntityFrameworkCore;
using SzApp.Data;
using SzApp.Data.Entities;
using Testcontainers.MsSql;

namespace SzApp.IntegrationTests.MasterData;

// FIN-10: CreateCompanyAsync and ReplaceContractAsync used to open a manual
// Database.BeginTransactionAsync() outside of Database.CreateExecutionStrategy(), while the
// DbContext is registered with EnableRetryOnFailure (matches Program.cs). This reproduces that
// exact combination against a real SQL Server (gated like SqlServerMigrationTests, since it needs
// Docker) to confirm the bug really throws, and confirms the fixed pattern -- the operation
// wrapped in strategy.ExecuteAsync(...), the same way BillingService.ExecuteSerializableAsync
// already does it -- does not.
public sealed class Fin10TransactionRetryStrategyTests
{
    [Fact]
    public async Task ManualTransaction_WithRetryOnFailure_ThrowsOnSaveChanges_ReproducesFin10()
    {
        if (!IsSqlIntegrationEnabled())
        {
            return;
        }

        await using var sqlServer = await StartMigratedSqlServerAsync();
        await using var context = CreateRetryingContext(sqlServer.GetConnectionString());

        // The exact buggy shape: BeginTransactionAsync + SaveChangesAsync, NOT wrapped in
        // CreateExecutionStrategy().ExecuteAsync(...).
        await using var transaction = await context.Database.BeginTransactionAsync();
        context.Addresses.Add(new Address { StreetAddress = "Test 1", City = "Beograd", CountryCode = "RS" });

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => context.SaveChangesAsync());
        Assert.Contains("does not support user-initiated transactions", exception.Message);
    }

    [Fact]
    public async Task ExecutionStrategyWrappedTransaction_WithRetryOnFailure_Succeeds()
    {
        if (!IsSqlIntegrationEnabled())
        {
            return;
        }

        await using var sqlServer = await StartMigratedSqlServerAsync();
        await using var context = CreateRetryingContext(sqlServer.GetConnectionString());

        // The fixed shape (matches BillingService.cs:661-671 / the FIN-10 fix in
        // MasterDataFeatureExtensions.CreateCompanyAsync and
        // MasterDataExtendedFeatureExtensions.ReplaceContractAsync).
        var strategy = context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync();
            context.Addresses.Add(new Address { StreetAddress = "Test 2", City = "Beograd", CountryCode = "RS" });
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        });

        Assert.Equal(1, await context.Addresses.CountAsync(x => x.StreetAddress == "Test 2"));
    }

    private static bool IsSqlIntegrationEnabled() => string.Equals(
        Environment.GetEnvironmentVariable("SZAPP_RUN_SQL_INTEGRATION"), "1", StringComparison.Ordinal);

    private static async Task<MsSqlContainer> StartMigratedSqlServerAsync()
    {
        var sqlServer = new MsSqlBuilder().WithImage("mcr.microsoft.com/mssql/server:2025-latest").Build();
        await sqlServer.StartAsync();
        var migrationOptions = new DbContextOptionsBuilder<SzAppDbContext>()
            .UseSqlServer(sqlServer.GetConnectionString())
            .Options;
        await using var migrationContext = new SzAppDbContext(migrationOptions);
        await migrationContext.Database.MigrateAsync();
        return sqlServer;
    }

    private static SzAppDbContext CreateRetryingContext(string connectionString)
    {
        var options = new DbContextOptionsBuilder<SzAppDbContext>()
            .UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure())
            .Options;
        return new SzAppDbContext(options);
    }
}
