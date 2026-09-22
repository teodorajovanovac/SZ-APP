using Microsoft.EntityFrameworkCore;
using SzApp.Data;
using SzApp.Data.Configurations.LedgerBanking;
using SzApp.Data.Entities;
using SzApp.Data.Entities.LedgerBanking;

namespace SzApp.IntegrationTests.LedgerBanking;

public sealed class LedgerBankingModelTests
{
    private static SzAppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SzAppDbContext>()
            .UseSqlServer("Server=localhost;Database=ModelOnly;User Id=sa;Password=Not_used_123!;TrustServerCertificate=True")
            .Options;
        return new SzAppDbContext(options);
    }

    [Fact]
    public void BankingModel_HasTenantCompositeKeysAndConcurrencyTokens()
    {
        using var context = CreateContext();
        var statement = context.Model.FindEntityType(typeof(BankStatement))!;
        var line = context.Model.FindEntityType(typeof(BankStatementLine))!;

        Assert.True(statement.FindProperty(nameof(BankStatement.RowVersion))!.IsConcurrencyToken);
        Assert.True(line.FindProperty(nameof(BankStatementLine.RowVersion))!.IsConcurrencyToken);

        var statementFk = line.GetForeignKeys().Single(x => x.PrincipalEntityType.ClrType == typeof(BankStatement));
        Assert.Equal(
            [nameof(BankStatementLine.BankStatementId), nameof(BankStatementLine.CompanyId)],
            statementFk.Properties.Select(x => x.Name));
    }

    [Fact]
    public void JournalAndLines_DeclareDatabaseGuards()
    {
        using var context = CreateContext();
        var journalTriggers = context.Model.FindEntityType(typeof(JournalEntry))!.GetDeclaredTriggers();
        var lineTriggers = context.Model.FindEntityType(typeof(LedgerEntry))!.GetDeclaredTriggers();

        Assert.Contains(journalTriggers, x => x.ModelName == "TR_JournalEntry_PostingGuard");
        Assert.Contains(lineTriggers, x => x.ModelName == "TR_LedgerEntry_PostedGuard");
        Assert.Contains("SESSION_CONTEXT", LedgerBankingDbGuardSql.CreateJournalGuard, StringComparison.Ordinal);
        Assert.Contains("AFTER INSERT, UPDATE, DELETE", LedgerBankingDbGuardSql.CreateLedgerGuard, StringComparison.Ordinal);
    }

    [Fact]
    public void SourcePosting_EnforcesSourceAndIdempotencyUniqueness()
    {
        using var context = CreateContext();
        var indexes = context.Model.FindEntityType(typeof(LedgerSourcePosting))!.GetIndexes().ToArray();

        Assert.Contains(indexes, x => x.IsUnique &&
            x.Properties.Select(p => p.Name).SequenceEqual([
                nameof(LedgerSourcePosting.CompanyId),
                nameof(LedgerSourcePosting.SourceType),
                nameof(LedgerSourcePosting.SourceId)]));
        Assert.Contains(indexes, x => x.IsUnique &&
            x.Properties.Select(p => p.Name).SequenceEqual([
                nameof(LedgerSourcePosting.CompanyId),
                nameof(LedgerSourcePosting.IdempotencyKey)]));
    }
}
