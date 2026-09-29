using Microsoft.EntityFrameworkCore;
using SzApp.Api.Features.Reports;
using SzApp.Data;

namespace SzApp.IntegrationTests.Reports;

/// <summary>Every ERROR_* check must translate to SQL (no client evaluation), with and without a company filter. No DB needed.</summary>
public sealed class ConsistencyChecksTranslationTests
{
    public static TheoryData<string> CheckIds() => new(ConsistencyChecks.All.Select(x => x.Id));

    [Theory]
    [MemberData(nameof(CheckIds))]
    public void Check_TranslatesToSql(string id)
    {
        var options = new DbContextOptionsBuilder<SzAppDbContext>().UseSqlServer("Server=unused;Database=unused").Options;
        using var db = new SzAppDbContext(options);
        var check = ConsistencyChecks.All.Single(x => x.Id == id);

        Assert.Contains("SELECT", check.Query(db, 7).ToQueryString());
        Assert.Contains("SELECT", check.Query(db, null).ToQueryString());
    }

    [Fact]
    public void CheckIds_AreUnique() =>
        Assert.Equal(ConsistencyChecks.All.Count, ConsistencyChecks.All.Select(x => x.Id).Distinct().Count());
}
