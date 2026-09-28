using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SzApp.Data;
using SzApp.Data.Entities;
using SzApp.Data.Entities.Billing;

namespace SzApp.IntegrationTests;

public sealed class ModelInvariantTests
{
    private static SzAppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SzAppDbContext>()
            .UseSqlServer("Server=localhost;Database=ModelOnly;User Id=sa;Password=Not_used_123!;TrustServerCertificate=True")
            .Options;
        return new SzAppDbContext(options);
    }

    [Fact]
    public void AllForeignKeys_DefaultToNoAction()
    {
        using var context = CreateContext();

        var foreignKeys = context.Model.GetEntityTypes().SelectMany(x => x.GetForeignKeys()).ToArray();

        Assert.NotEmpty(foreignKeys);
        Assert.All(foreignKeys, foreignKey => Assert.Equal(DeleteBehavior.NoAction, foreignKey.DeleteBehavior));
    }

    [Fact]
    public void StaffAccess_And_ShortList_HaveRequiredUniqueIndexes()
    {
        using var context = CreateContext();

        AssertUniqueIndex<StaffAccess>(context, nameof(StaffAccess.StaffId), nameof(StaffAccess.CompanyId));
        AssertUniqueIndex<ShortList>(context, nameof(ShortList.TableName), nameof(ShortList.IndexValue));
    }

    [Fact]
    public void FinancialPrecisionAndConcurrencyTokens_AreConfigured()
    {
        using var context = CreateContext();

        var invoiceTotal = context.Model.FindEntityType(typeof(Invoice))!
            .FindProperty(nameof(Invoice.InvoiceTotal))!;
        var ledgerDebit = context.Model.FindEntityType(typeof(LedgerEntry))!
            .FindProperty(nameof(LedgerEntry.DebitAmount))!;
        var companyVersion = context.Model.FindEntityType(typeof(Company))!
            .FindProperty(nameof(Company.RowVersion))!;

        Assert.Equal(18, invoiceTotal.GetPrecision());
        Assert.Equal(2, invoiceTotal.GetScale());
        Assert.Equal(18, ledgerDebit.GetPrecision());
        Assert.Equal(4, ledgerDebit.GetScale());
        Assert.True(companyVersion.IsConcurrencyToken);
        Assert.Equal(ValueGenerated.OnAddOrUpdate, companyVersion.ValueGenerated);

        var ledgerJournalFk = context.Model.FindEntityType(typeof(LedgerEntry))!.GetForeignKeys()
            .Single(x => x.PrincipalEntityType.ClrType == typeof(JournalEntry));
        Assert.Equal(
            [nameof(LedgerEntry.JournalEntryId), nameof(LedgerEntry.CompanyId)],
            ledgerJournalFk.Properties.Select(x => x.Name));
    }

    // FIN-09: without an explicit .HasFilter(null), EF Core's SqlServer provider auto-adds a
    // "WHERE [col] IS NOT NULL" filter to a unique index over a nullable column -- which let
    // duplicate regular batches (marker always NULL) through. Asserting GetFilter() is null
    // here catches a regression at the model level, without needing a live SQL Server.
    [Fact]
    public void InvoiceBatch_UniqueIndex_HasNoFilter()
    {
        using var context = CreateContext();

        var index = context.Model.FindEntityType(typeof(InvoiceBatch))!.GetIndexes()
            .Single(x => x.Properties.Select(p => p.Name)
                .SequenceEqual([nameof(InvoiceBatch.CompanyId), nameof(InvoiceBatch.PeriodYYMM), nameof(InvoiceBatch.ExtraordinaryInvoiceMarker)]));

        Assert.True(index.IsUnique);
        Assert.Null(index.GetFilter());
    }

    private static void AssertUniqueIndex<TEntity>(SzAppDbContext context, params string[] propertyNames)
    {
        var index = context.Model.FindEntityType(typeof(TEntity))!.GetIndexes()
            .SingleOrDefault(x => x.Properties.Select(p => p.Name).SequenceEqual(propertyNames));
        Assert.NotNull(index);
        Assert.True(index.IsUnique);
    }
}
