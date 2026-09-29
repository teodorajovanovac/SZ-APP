using Microsoft.EntityFrameworkCore;
using SzApp.Api.Features.LedgerBanking;
using SzApp.Data;
using SzApp.Data.Entities;
using SzApp.Data.Entities.LedgerBanking;
using SzApp.Domain.LedgerBanking;
using Testcontainers.MsSql;

namespace SzApp.IntegrationTests.LedgerBanking;

/// <summary>
/// Kartica / stanja / Ctrl+K queries against real SQL Server (gated by SZAPP_RUN_SQL_INTEGRATION=1,
/// needs Docker): verifies the EF queries translate and the balance math holds across pages.
/// </summary>
public sealed class LedgerCardSqlTests
{
    [Fact]
    public async Task Card_Balances_Search_EndToEnd()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("SZAPP_RUN_SQL_INTEGRATION"), "1", StringComparison.Ordinal))
        {
            return;
        }

        await using var sqlServer = new MsSqlBuilder().WithImage("mcr.microsoft.com/mssql/server:2025-latest").Build();
        await sqlServer.StartAsync();
        var options = new DbContextOptionsBuilder<SzAppDbContext>().UseSqlServer(sqlServer.GetConnectionString()).Options;
        await using var db = new SzAppDbContext(options);
        await db.Database.MigrateAsync();

        var companyPartner = new Partner { ShortName = "SZ", Name = "SZ Test" };
        var petar = new Partner { ShortName = "Petar", Name = "Petar Petrović", TaxNumber = "100200300" };
        var company = new Company { Partner = companyPartner, ShortName = "SZ", PrintName = "SZ Test" };
        // Line types (and possibly accounts) are seeded by migrations.
        var bankLine = await db.ShortLists.SingleAsync(x => x.TableName == LedgerLineTypes.ShortListTable && x.IndexValue == LedgerLineTypes.BankStatement);
        var invoiceLine = await db.ShortLists.SingleAsync(x => x.TableName == LedgerLineTypes.ShortListTable && x.IndexValue == LedgerLineTypes.Invoice);
        var staff = new ApplicationUser { UserName = "poster", NormalizedUserName = "POSTER", Email = "p@x.rs" };
        db.AddRange(companyPartner, petar, company, staff);
        foreach (var account in new[] { "2040", "2410", "4900" }.Where(a => !db.Set<ChartAccount>().Any(x => x.Account == a)))
        {
            db.Add(new ChartAccount { Account = account, Name = account, Level = 4 });
        }
        await db.SaveChangesAsync();
        var pa = new PartnerAccount { CompanyId = company.Id, Partner = petar, Account = "2040", AccountNumber = 17 };
        db.Add(pa);
        await db.SaveChangesAsync();

        // 2040 for Petar: July invoice 1000 (paid 1000 in Aug), Aug invoice 800 (paid 300), storno −50 on Aug.
        var toPost = new List<JournalEntry>();
        void Post(DateOnly date, string reference, decimal debit, decimal credit, ShortList type, DateOnly? due = null, bool posted = true)
        {
            var journal = new JournalEntry { CompanyId = company.Id, PostingDate = date, Description = $"N {reference}" };
            if (posted) toPost.Add(journal);
            var line = new LedgerEntry
            {
                CompanyId = company.Id, Account = "2040", PostingDate = date, DueDate = due, DebitAmount = debit, CreditAmount = credit,
                Parameters = reference, DocumentRef = $"R-{date:yyMM}", LineTypeId = type.Id, Priority = 1
            };
            var counter = new LedgerEntry
            {
                CompanyId = company.Id, Account = debit != 0 ? "4900" : "2410", PostingDate = date,
                DebitAmount = credit, CreditAmount = debit, LineTypeId = type.Id, Priority = 2
            };
            journal.Lines.Add(line);
            journal.Lines.Add(counter);
            db.Add(journal);
            db.Entry(line).Property("PartnerAccountId").CurrentValue = pa.Id;
        }

        // Seed posted journals directly: open the posting gate the posting service uses (DB guard trigger).
        await db.Database.OpenConnectionAsync();
        await db.Database.ExecuteSqlRawAsync("EXEC sys.sp_set_session_context @key=N'szapp_allow_posting', @value=1");
        Post(new(2026, 7, 1), "9717072607", 1000m, 0m, invoiceLine, new(2026, 7, 15));
        Post(new(2026, 8, 1), "9717072608", 800m, 0m, invoiceLine, new(2026, 8, 15));
        Post(new(2026, 8, 5), "9717072607", 0m, 1000m, bankLine);
        Post(new(2026, 8, 20), "9717072608", 0m, 300m, bankLine);
        Post(new(2026, 8, 25), "9717072608", -50m, 0m, invoiceLine);
        Post(new(2026, 9, 1), "9717072609", 999m, 0m, invoiceLine, posted: false); // draft: never on the card
        await db.SaveChangesAsync();
        foreach (var journal in toPost)
        {
            (journal.IsPosted, journal.PostedAt, journal.PostedUserId) = (true, DateTimeOffset.UtcNow, staff.Id);
        }
        await db.SaveChangesAsync();

        var filter = new LedgerCardFilter(Account: "2040", PartnerAccountId: pa.Id, From: new(2026, 8, 1));
        var page1 = await LedgerCardQueries.GetCardAsync(db, company.Id, filter, 1, 2, CancellationToken.None);
        var page2 = await LedgerCardQueries.GetCardAsync(db, company.Id, filter, 2, 2, CancellationToken.None);
        Assert.Equal(1000m, page1.OpeningBalance);
        Assert.Equal((750m, 1300m, 450m, 4), (page1.TotalDebit, page1.TotalCredit, page1.ClosingBalance, page1.TotalCount));
        Assert.Equal([1800m, 800m], page1.Items.Select(x => x.Balance));
        Assert.Equal([500m, 450m], page2.Items.Select(x => x.Balance)); // running continues across pages
        Assert.Equal(LedgerLineTypes.BankStatement, page1.Items.Last().LineType);

        var byPartner = await LedgerCardQueries.GetCardAsync(db, company.Id, new LedgerCardFilter(PartnerId: petar.Id), 1, 50, CancellationToken.None);
        Assert.Equal((5, 450m), (byPartner.TotalCount, byPartner.ClosingBalance));

        var open = await LedgerCardQueries.GetCardAsync(db, company.Id, filter with { GroupBy = LedgerCardGrouping.OpenItems }, 1, 50, CancellationToken.None);
        Assert.Equal(0m, open.OpeningBalance); // July reference is closed
        Assert.All(open.Items, x => Assert.Equal("9717072608", x.PaymentReference));
        Assert.Equal(450m, open.ClosingBalance);

        var byRef = await LedgerCardQueries.GetCardAsync(db, company.Id, filter with { From = null, GroupBy = LedgerCardGrouping.PaymentReference }, 1, 50, CancellationToken.None);
        Assert.Equal([("9717072607", 0m), ("9717072608", 450m)], byRef.Items.Select(x => (x.GroupKey!, x.Balance)));

        var balances = await LedgerCardQueries.GetPartnerBalancesAsync(db, company.Id, "2040", new(2026, 9, 1), true, null,
            "balance", true, 1, 25, CancellationToken.None);
        var row = Assert.Single(balances.Items);
        Assert.Equal((450m, 450m, new DateOnly(2026, 8, 20), "Petar Petrović"), (row.Balance, row.Overdue, row.LastPaymentDate, row.PartnerName));

        var hits = await LedgerCardQueries.SearchAsync(db, company.Id, "100200300", CancellationToken.None);
        Assert.Equal(450m, Assert.Single(hits, x => x.Type == "partner").Balance);
        hits = await LedgerCardQueries.SearchAsync(db, company.Id, "97-17-07-2608", CancellationToken.None);
        Assert.Equal(450m, Assert.Single(hits, x => x.Type == "payment").Balance);
    }
}
