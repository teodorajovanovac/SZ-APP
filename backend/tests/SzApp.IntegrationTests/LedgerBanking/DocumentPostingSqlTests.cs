using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SzApp.Api.Features.Billing;
using SzApp.Api.Features.LedgerBanking;
using SzApp.Contracts.Billing;
using SzApp.Data;
using SzApp.Data.Entities;
using SzApp.Data.Entities.Billing;
using SzApp.Data.Entities.LedgerBanking;
using SzApp.Domain;
using SzApp.Domain.LedgerBanking;
using Testcontainers.MsSql;

namespace SzApp.IntegrationTests.LedgerBanking;

/// <summary>
/// End-to-end 9.2 posting against a real SQL Server (gated by SZAPP_RUN_SQL_INTEGRATION=1, needs
/// Docker): batch posting, locked period, per-invoice red storno, source-journal reverse block,
/// supplier invoice post/storno. Verifies the EF queries translate and the DB constraints accept
/// negative (storno) amounts.
/// </summary>
public sealed class DocumentPostingSqlTests
{
    [Fact]
    public async Task InvoiceBatch_Storno_Lock_SupplierInvoice_EndToEnd()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("SZAPP_RUN_SQL_INTEGRATION"), "1", StringComparison.Ordinal))
        {
            return;
        }

        await using var sqlServer = new MsSqlBuilder().WithImage("mcr.microsoft.com/mssql/server:2025-latest").Build();
        await sqlServer.StartAsync();
        var options = new DbContextOptionsBuilder<SzAppDbContext>().UseSqlServer(sqlServer.GetConnectionString()).Options;
        await using (var migrate = new SzAppDbContext(options))
        {
            await migrate.Database.MigrateAsync();
        }

        await using var db = new SzAppDbContext(options);
        var seed = await SeedAsync(db);
        var clock = new BelgradeBusinessClock(TimeProvider.System);
        var guard = new PostingPeriodGuard(db, TimeProvider.System);
        var journals = new JournalPostingService(db, new LedgerMutationScope(), new ShortListValidator(db), TimeProvider.System, guard, clock);
        var gateway = new LedgerPostingGateway(journals, new HttpContextAccessor(),
            Options.Create(new LedgerBankingOptions { SystemUserId = seed.StaffId }));
        var billing = new BillingService(db, new ShortListValidator(db), gateway, null!, TimeProvider.System, clock);

        // Locked month: 409-type exception, nothing posted, batch stays Generated (atomic).
        await guard.LockAsync(seed.CompanyId, 2608, seed.StaffId, CancellationToken.None);
        await Assert.ThrowsAsync<PostingPeriodLockedException>(() => billing.PostBatchAsync(seed.CompanyId, seed.BatchId, "k-locked", CancellationToken.None));
        db.ChangeTracker.Clear();
        Assert.Equal(0, await db.JournalEntries.CountAsync());
        Assert.Equal(BillingBatchStatus.Generated, (await db.Set<InvoiceBatch>().SingleAsync()).Status);
        await guard.UnlockAsync(seed.CompanyId, 2608, seed.StaffId, CancellationToken.None);

        var batch = await billing.PostBatchAsync(seed.CompanyId, seed.BatchId, "k-post", CancellationToken.None);
        Assert.Equal("Posted", batch.Status);
        var lines = await db.LedgerEntries.AsNoTracking().Where(x => x.JournalEntryId == batch.JournalEntryId).ToArrayAsync();
        Assert.Equal(lines.Sum(x => x.DebitAmount), lines.Sum(x => x.CreditAmount));
        Assert.Equal(3, lines.Count(x => x.Account == "2040")); // inv1×A, inv1×B, inv2×A
        Assert.Equal(1, lines.Count(x => x.Account == "4350")); // only type-1 supplier invoice A
        Assert.All(lines.Where(x => x.Account == "2040"), x => Assert.NotNull(x.InvoiceId));
        db.ChangeTracker.Clear();
        Assert.Equal(1300m, (await db.Set<SupplierInvoice>().SingleAsync(x => x.Id == seed.SupplierA)).PostedInvoiceAmount);

        // Generic reverse of a document journal is blocked (FIN-07).
        var journal = await db.JournalEntries.AsNoTracking().SingleAsync(x => x.Id == batch.JournalEntryId);
        Assert.Equal("journal.reverse-via-source", (await Assert.ThrowsAsync<DomainRuleException>(() =>
            journals.ReverseAsync(seed.CompanyId, journal.Id, seed.StaffId, journal.RowVersion, CancellationToken.None))).Code);

        // Per-invoice red storno nets the invoice (and its 4900/4350/5590 share) to zero.
        var invoice = await billing.GetInvoiceAsync(seed.CompanyId, seed.Invoice1, CancellationToken.None);
        await billing.CancelInvoiceAsync(seed.CompanyId, seed.Invoice1, new CancelInvoiceRequest("greška", invoice!.RowVersion), "k-cancel", CancellationToken.None);
        db.ChangeTracker.Clear();
        var invoiceLines = await db.LedgerEntries.AsNoTracking().Where(x => x.InvoiceId == seed.Invoice1).ToArrayAsync();
        Assert.Equal(0m, invoiceLines.Sum(x => x.DebitAmount));
        Assert.Contains(invoiceLines, x => x.DebitAmount < 0m);
        var all = await db.LedgerEntries.AsNoTracking().ToArrayAsync();
        Assert.Equal(700m, all.Where(x => x.Account == "2040").Sum(x => x.DebitAmount)); // only invoice 2 remains
        Assert.Equal(700m, all.Where(x => x.Account == "4350").Sum(x => x.CreditAmount));
        Assert.Equal(700m, (await db.Set<SupplierInvoice>().SingleAsync(x => x.Id == seed.SupplierA)).PostedInvoiceAmount);

        // Supplier invoice type 2: 4350 credit on the supplier, storno nets it out.
        var posted = await billing.PostSupplierInvoiceAsync(seed.CompanyId, seed.SupplierB, "k-sup", CancellationToken.None);
        var supplierLines = await db.LedgerEntries.AsNoTracking().Where(x => x.JournalEntryId == posted.JournalEntryId).ToArrayAsync();
        Assert.Equal(450m, supplierLines.Single(x => x.Account == "4350").CreditAmount);
        Assert.Equal(450m, supplierLines.Single(x => x.Account == "5590").DebitAmount);
        var cancelled = await billing.CancelSupplierInvoicePostingAsync(seed.CompanyId, seed.SupplierB, "k-sup-cancel", CancellationToken.None);
        Assert.True(cancelled.IsPostingCancelled);
        var net = await db.LedgerEntries.AsNoTracking().Where(x => x.SupplierInvoiceId == seed.SupplierB && x.Account == "4350" && x.InvoiceId == null)
            .SumAsync(x => x.CreditAmount);
        Assert.Equal(0m, net);
    }

    private sealed record Seed(int CompanyId, int StaffId, int BatchId, int Invoice1, int SupplierA, int SupplierB);

    private static async Task<Seed> SeedAsync(SzAppDbContext db)
    {
        var staff = new ApplicationUser { UserName = "poster", NormalizedUserName = "POSTER", Email = "p@x.rs" };
        db.Users.Add(staff);
        Partner P(string name) => new() { ShortName = name, Name = name };
        var companyPartner = P("SZ Test");
        var customer1 = P("Kupac 1");
        var customer2 = P("Kupac 2");
        var supplier = P("Dobavljač");
        var company = new Company { Partner = companyPartner, ShortName = "SZ", PrintName = "SZ Test" };
        db.AddRange(companyPartner, customer1, customer2, supplier, company);
        foreach (var account in new[] { "2040", "4900", "4350", "5590" })
        {
            db.Add(new ChartAccount { Account = account, Name = account, Level = 4 });
        }
        var planned = new ShortList { TableName = "SupplierDocumentType", Caption = "Predviđeni", IndexValue = 1 };
        var actual = new ShortList { TableName = "SupplierDocumentType", Caption = "Izvršeni", IndexValue = 2 };
        var unit = new ShortList { TableName = "UnitOfMeasure", Caption = "kom", IndexValue = 1 };
        db.AddRange(planned, actual, unit);
        await db.SaveChangesAsync();

        var calc = new CalculationType { Name = "Fiksno", SupplierAmountRule = "FixedRsd", AllocationRule = "Equal", QuantityRule = "One", UnitOfMeasureId = unit.Id };
        PartnerAccount Pa(Partner partner, string account) => new() { CompanyId = company.Id, Partner = partner, Account = account, AccountNumber = partner.Id };
        var pa1 = Pa(customer1, "2040");
        var pa2 = Pa(customer2, "2040");
        var paSupplier = Pa(supplier, "4350");
        db.AddRange(calc, pa1, pa2, paSupplier);
        await db.SaveChangesAsync();

        SupplierInvoice Si(int no, int type, decimal amount) => new()
        {
            CompanyId = company.Id, InvoiceNo = no, CodeName = $"RT-{no}", Caption = $"Račun {no}", SupplierPartnerAccountId = paSupplier.Id,
            CalculationTypeId = calc.Id, PeriodYYMM = 2608, InvoiceTotalCalculationAmountRsd = amount, DocumentTypeId = type,
            PaymentPriority = 3, InvoiceDate = new DateOnly(2026, 8, 5), TransactionDate = new DateOnly(2026, 8, 5),
            PaymentDate = new DateOnly(2026, 9, 5), PaymentReference = "97-11-22"
        };
        var supplierA = Si(1, planned.Id, 1300m);
        var supplierB = Si(2, actual.Id, 450m);
        var batch = new InvoiceBatch
        {
            CompanyId = company.Id, PeriodYYMM = 2608, Caption = "Avgust", Month = 8, Year = 2026, Place = "Beograd",
            IssueDate = new DateOnly(2026, 9, 1), ServiceDateFrom = new DateOnly(2026, 8, 1), ServiceDateTo = new DateOnly(2026, 8, 31),
            TransactionDate = new DateOnly(2026, 8, 31), DueDate = new DateOnly(2026, 9, 15), ExchangeRateNbs = 117m,
            EntryDate = DateTimeOffset.UtcNow, StaffId = staff.Id, Status = BillingBatchStatus.Generated
        };
        db.AddRange(supplierA, supplierB, batch);
        await db.SaveChangesAsync();

        Invoice Inv(Partner partner, string seq, params (int SupplierId, decimal Amount)[] items)
        {
            var total = items.Sum(x => x.Amount);
            var invoice = new Invoice
            {
                CompanyId = company.Id, PartnerId = partner.Id, SequenceNumber = seq, IssueDate = batch.IssueDate, DueDate = batch.DueDate,
                PartnerName = partner.Name, Address = "Ulica 1", City = "Beograd", Amount = total, Total = total, InvoiceTotal = total
            };
            db.Add(invoice);
            db.Entry(invoice).Property("InvoiceBatchId").CurrentValue = batch.Id;
            db.Entry(invoice).Property("PaymentReference").CurrentValue = $"97-{seq}-2608";
            foreach (var (supplierId, amount) in items)
            {
                var line = new InvoiceLine { CompanyId = company.Id, Name = "Stavka", Quantity = 1, PricePcs = amount, PriceTotal = amount, TotalAmount = amount };
                invoice.Lines.Add(line);
                db.Entry(line).Property("InvoiceBatchId").CurrentValue = batch.Id;
                db.Entry(line).Property("SupplierInvoiceId").CurrentValue = supplierId;
            }
            return invoice;
        }

        var invoice1 = Inv(customer1, "1", (supplierA.Id, 600m), (supplierB.Id, 300m));
        Inv(customer2, "2", (supplierA.Id, 700m));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return new Seed(company.Id, staff.Id, batch.Id, invoice1.Id, supplierA.Id, supplierB.Id);
    }
}
