using Microsoft.EntityFrameworkCore;
using SzApp.Data;
using SzApp.Data.Entities;

namespace SzApp.IntegrationTests.Platform;

public sealed class PlatformModelTests
{
    [Fact]
    public void PlatformEntitiesAreDiscoveredWithTenantIndexes()
    {
        var options = new DbContextOptionsBuilder<SzAppDbContext>()
            .UseSqlServer("Server=localhost;Database=ModelOnly;User Id=sa;Password=ModelOnly!123;TrustServerCertificate=true").Options;
        using var context = new SzAppDbContext(options);

        Assert.NotNull(context.Model.FindEntityType(typeof(DocumentRecord)));
        Assert.NotNull(context.Model.FindEntityType(typeof(SentEmail)));
        Assert.NotNull(context.Model.FindEntityType(typeof(PlatformEvent)));
        Assert.NotNull(context.Model.FindEntityType(typeof(SelectionBasket)));
        Assert.NotNull(context.Model.FindEntityType(typeof(ImportDefinition)));
        Assert.Contains(context.Model.FindEntityType(typeof(Setting))!.GetIndexes(), index => index.IsUnique);
    }
}
