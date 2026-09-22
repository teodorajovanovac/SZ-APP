using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SzApp.Data;
using SzApp.Data.Entities;

namespace SzApp.IntegrationTests.MasterData;

public sealed class MasterDataExtendedModelTests
{
    private static SzAppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SzAppDbContext>()
            .UseSqlServer("Server=localhost;Database=SzAppModelOnly;User Id=sa;Password=ModelOnly!123;TrustServerCertificate=true")
            .Options;
        return new SzAppDbContext(options);
    }

    [Fact]
    public void ExtendedEntities_AreDiscoveredByCentralModel()
    {
        using var context = CreateContext();

        Assert.NotNull(context.Model.FindEntityType(typeof(BuildingEntrance)));
        Assert.NotNull(context.Model.FindEntityType(typeof(Unit)));
        Assert.NotNull(context.Model.FindEntityType(typeof(Contract)));
        Assert.NotNull(context.Model.FindEntityType(typeof(PartnerAccount)));
        Assert.NotNull(context.Model.FindEntityType(typeof(BankAccount)));
    }

    [Theory]
    [InlineData(typeof(BuildingEntrance))]
    [InlineData(typeof(Unit))]
    [InlineData(typeof(Contract))]
    [InlineData(typeof(PartnerAccount))]
    [InlineData(typeof(BankAccount))]
    public void MutableExtendedEntities_HaveConcurrencyToken(Type entityType)
    {
        using var context = CreateContext();

        var rowVersion = context.Model.FindEntityType(entityType)!.FindProperty("RowVersion");

        Assert.NotNull(rowVersion);
        Assert.True(rowVersion!.IsConcurrencyToken);
        Assert.Equal(ValueGenerated.OnAddOrUpdate, rowVersion.ValueGenerated);
    }

    [Fact]
    public void PartnerAccount_NumberIsUniqueInsideCompany()
    {
        using var context = CreateContext();
        var entity = context.Model.FindEntityType(typeof(PartnerAccount))!;

        var index = entity.GetIndexes().Single(value =>
            value.Properties.Select(property => property.Name).SequenceEqual(["CompanyId", "AccountNumber"]));

        Assert.True(index.IsUnique);
    }
}
