using Microsoft.EntityFrameworkCore;
using SzApp.Api.Features.Export;
using SzApp.Data;
using SzApp.Data.Entities;
using SzApp.Etl.Pipeline;

namespace SzApp.UnitTests.Etl;

public sealed class ExportCatalogTests
{
    [Fact]
    public void EveryTableQuery_TranslatesToSql()
    {
        var options = new DbContextOptionsBuilder<SzAppDbContext>()
            .UseSqlServer("Server=.;Database=translate-only;Trusted_Connection=True").Options;
        using var db = new SzAppDbContext(options);
        foreach (var table in ExportCatalog.Tables)
        {
            var sql = table.Query(db, [1, 2]).ToQueryString();
            Assert.False(string.IsNullOrWhiteSpace(sql), table.Name);
        }
    }

    [Fact]
    public void ReimportableTables_ExistInEtlTopology_AndCoverIt()
    {
        var importable = ExportCatalog.Tables.Where(x => x.ImportTable is not null).Select(x => x.ImportTable!).ToHashSet();
        Assert.All(importable, name => Assert.NotNull(LegacyImportTopology.Find(name)));
        Assert.All(LegacyImportTopology.OrderedPassOne(), node => Assert.Contains(node.Table, importable));
    }

    [Fact]
    public void NoSecretColumns_AndPersonalColumnsAreFlagged()
    {
        var columns = ExportCatalog.Tables.SelectMany(t => t.Columns.Select(c => (Table: t.Name, Column: c))).ToArray();
        Assert.DoesNotContain(columns, x => x.Column.Property.Name is "PasswordHash" or "SecurityStamp" or "ConcurrencyStamp" or "RowVersion");
        var partner = ExportCatalog.Find("Partner")!;
        Assert.True(partner.Columns.Single(c => c.Name == "Jmbg").IsPersonal);
        Assert.True(partner.Columns.Single(c => c.Name == "IdCardNumber").IsPersonal);
        Assert.Equal(["StaffId", "UserName", "PreferredLanguage", "LastIP", "LastLoginTimeStamp", "IsActive"],
            ExportCatalog.Find("Staff")!.Columns.Select(c => c.Name));
    }

    [Theory]
    [InlineData("Smtp.Password", true)]
    [InlineData("Api.ClientSecret", true)]
    [InlineData("Sef.ApiKey", true)]
    [InlineData("Bank.Token", true)]
    [InlineData("Invoice.DefaultPlace", false)]
    public void SecretSettings_AreDetected(string key, bool expected) =>
        Assert.Equal(expected, ExportCatalog.IsSecretSetting(new Setting { Key = key, Name = key }));
}
