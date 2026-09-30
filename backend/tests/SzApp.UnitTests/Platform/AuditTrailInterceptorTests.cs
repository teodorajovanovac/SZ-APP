using Microsoft.EntityFrameworkCore;
using SzApp.Data;
using SzApp.Data.Entities;

namespace SzApp.UnitTests.Platform;

public sealed class AuditTrailInterceptorTests
{
    [Fact]
    public async Task RecordsInsertAndUpdate_WithMaskedJmbg()
    {
        await using var db = new SzAppDbContext(new DbContextOptionsBuilder<SzAppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(new AuditTrailInterceptor(() => 7, TimeProvider.System))
            .Options);

        var partner = new Partner { Name = "Pera", Jmbg = "0101990710001" };
        db.Partners.Add(partner);
        await db.SaveChangesAsync();
        partner.Name = "Pera Perić";
        await db.SaveChangesAsync();

        var logs = await db.AuditLogs.OrderBy(x => x.Id).ToListAsync();
        Assert.Equal(["Added", "Modified"], logs.Select(x => x.Action));
        Assert.All(logs, x => Assert.Equal(7, x.StaffId));
        Assert.Equal(partner.Id.ToString(), logs[0].ItemId);
        Assert.DoesNotContain("0101990710001", logs[0].DetailsJson);
        Assert.Contains("Pera Peri", logs[1].DetailsJson);
        Assert.DoesNotContain("Jmbg", logs[1].DetailsJson);
    }
}
