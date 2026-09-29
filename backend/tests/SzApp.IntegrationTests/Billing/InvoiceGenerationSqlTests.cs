using Microsoft.EntityFrameworkCore;
using SzApp.Api.Features.Billing;
using SzApp.Contracts.Billing;
using SzApp.Data;
using SzApp.Data.Entities;
using SzApp.Data.Entities.Billing;
using SzApp.Domain.Billing;
using Testcontainers.MsSql;

namespace SzApp.IntegrationTests.Billing;

/// <summary>
/// End-to-end server-side invoice generation (audit 9.1) against real SQL Server
/// (gated by SZAPP_RUN_SQL_INTEGRATION=1, needs Docker): R0 filter + aggregation, R2 live
/// PrethodniDug, R3 numbering/KB97, benefit zero+archive, idempotent repeat.
/// </summary>
public sealed class InvoiceGenerationSqlTests
{
    [Fact]
    public async Task Generate_SmallBatch_EndToEnd()
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
        var s = await SeedAsync(db);
        var service = new InvoiceGenerationService(db, TimeProvider.System);
        var request = new GenerateInvoicesV2Request(2608, null, "Beograd", new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 15),
            new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 31), new DateOnly(2026, 8, 31), 117m);

        var preview = await service.PreviewAsync(s.CompanyId, 2608, null, 117m, CancellationToken.None);
        Assert.Equal(2, preview.CustomerCount);
        Assert.Equal(625m, preview.NetTotal); // type 1: 200 + 300 (inactive unit not billed), manager type 9: 50 + 75

        var result = await service.GenerateAsync(s.CompanyId, s.StaffId, request, CancellationToken.None);
        Assert.False(result.AlreadyGenerated);
        Assert.Equal(2, result.InvoiceIds.Count);

        db.ChangeTracker.Clear();
        var invoices = await db.Invoices.AsNoTracking().Include(x => x.Lines).OrderBy(x => x.PartnerId).ToArrayAsync();
        var inv1 = invoices.Single(x => x.PartnerId == s.Customer1);
        Assert.Equal($"SZ-{s.Account1No}-2608", inv1.SequenceNumber);
        Assert.Equal(Kb97.PaymentReference(inv1.SequenceNumber), db.Entry(inv1).Property<string?>("PaymentReference").CurrentValue);
        Assert.Equal(123.45m, db.Entry(inv1).Property<decimal>("PreviousBalance").CurrentValue); // live 2040 balance
        // Customer 1 has a benefit: manager (9001) line zeroed and archived, maintenance line kept.
        Assert.Equal(200m, inv1.Amount);
        var archive = await db.Set<BenefitArchive>().AsNoTracking().SingleAsync();
        Assert.Equal(50m, archive.OriginalAmount);
        Assert.Equal(s.Customer1, archive.CustomerId);
        Assert.Contains("2608", archive.Note);
        Assert.Contains(inv1.Lines, l => l.TotalAmount == 0m); // zeroed row is kept

        var inv2 = invoices.Single(x => x.PartnerId == s.Customer2);
        Assert.Equal(300m + 75m, inv2.Amount);

        // Item 6: repeat is a no-op returning the same invoices.
        var again = await service.GenerateAsync(s.CompanyId, s.StaffId, request, CancellationToken.None);
        Assert.True(again.AlreadyGenerated);
        Assert.Equal(result.InvoiceIds, again.InvoiceIds);
    }

    private sealed record Seed(int CompanyId, int StaffId, int Customer1, int Customer2, int Account1No);

    private static async Task<Seed> SeedAsync(SzAppDbContext db)
    {
        var staff = new ApplicationUser { UserName = "gen", NormalizedUserName = "GEN", Email = "g@x.rs" };
        db.Users.Add(staff);
        Partner P(string name) => new() { ShortName = name, Name = name };
        var companyPartner = P("SZ Test");
        var c1 = P("Kupac 1");
        var c2 = P("Kupac 2");
        var c3 = P("Kupac neaktivan");
        var supplier = P("Dobavljač");
        var manager = P("Upravnik");
        var company = new Company { Partner = companyPartner, ShortName = "SZ", PrintName = "SZ Test" };
        db.AddRange(companyPartner, c1, c2, c3, supplier, manager, company);
        var planned = new ShortList { TableName = "SupplierDocumentType", Caption = "Predviđeni", IndexValue = 1 };
        var actual = new ShortList { TableName = "SupplierDocumentType", Caption = "Izvršeni", IndexValue = 2 };
        var uom = new ShortList { TableName = "UnitOfMeasure", Caption = "kom", IndexValue = 1 };
        var flat = new ShortList { TableName = "UnitType", Caption = "Stan", IndexValue = 1 };
        var takenOver = new ShortList { TableName = "LedgerLineType", Caption = "Preuzeti dug", IndexValue = 98 };
        db.AddRange(planned, actual, uom, flat, takenOver);
        db.Add(new ChartAccount { Account = "2040", Name = "Kupci", Level = 4 });
        await db.SaveChangesAsync();

        // Calculation type ids follow the legacy table (InvoiceCalculationTypes): 1 = amount over SUM(K1*K2).
        var calc1 = new CalculationType { Name = "Raspodela", SupplierAmountRule = "SupplierTotal", AllocationRule = "ByCoefficient", QuantityRule = "Coefficient", UnitOfMeasureId = uom.Id };
        db.Add(calc1);
        await db.SaveChangesAsync();
        Assert.Equal(1, calc1.Id);
        await db.Database.ExecuteSqlRawAsync(
            $"SET IDENTITY_INSERT [billing].[CalculationType] ON; INSERT INTO [billing].[CalculationType] ([Id],[Name],[SupplierAmountRule],[AllocationRule],[QuantityRule],[UnitOfMeasureId]) VALUES (9,'Po koef','FixedRsd','ByCoefficient','Coefficient',{uom.Id}); SET IDENTITY_INSERT [billing].[CalculationType] OFF;");

        PartnerAccount Pa(Partner p, string account, int no) => new() { CompanyId = company.Id, Partner = p, Account = account, AccountNumber = no };
        var pa1 = Pa(c1, "2040", 101);
        var pa2 = Pa(c2, "2040", 102);
        var pa3 = Pa(c3, "2040", 103);
        var paSupplier = Pa(supplier, "4350", 5001);
        var paManager = Pa(manager, "4350", InvoiceGenerationService.ManagerSupplierAccountNumber);
        db.AddRange(pa1, pa2, pa3, paSupplier, paManager);
        await db.SaveChangesAsync();

        async Task<Contract> UnitWith(Partner owner, decimal k1, bool active)
        {
            var unit = new Unit { CompanyId = company.Id, Name = owner.Name, UnitTypeId = flat.Id, K1 = k1, K2 = 1m, K3 = 1m, K4 = 1m, K5 = 1m };
            db.Add(unit);
            await db.SaveChangesAsync();
            var contract = new Contract { CompanyId = company.Id, UnitId = unit.Id, OwnerPartnerId = owner.Id, ContractDate = new DateOnly(2020, 1, 1), IsActive = active };
            db.Add(contract);
            await db.SaveChangesAsync();
            unit.ContractId = contract.Id;
            await db.SaveChangesAsync();
            return contract;
        }
        var contract1 = await UnitWith(c1, 2m, true);
        await UnitWith(c2, 3m, true);
        await UnitWith(c3, 5m, false);

        SupplierInvoice Si(int no, int partnerAccountId, int calcType, int docType, decimal total, decimal perCoef) => new()
        {
            CompanyId = company.Id, InvoiceNo = no, CodeName = $"RT-{no}", Caption = $"Račun {no}", SupplierPartnerAccountId = partnerAccountId,
            CalculationTypeId = calcType, PeriodYYMM = 2608, InvoiceTotalCalculationAmountRsd = total, CalculationAmountByCoefficientRsd = perCoef,
            DocumentTypeId = docType, InvoiceDate = new DateOnly(2026, 8, 5), TransactionDate = new DateOnly(2026, 8, 5)
        };
        var maintenance = Si(1, paSupplier.Id, 1, planned.Id, 1000m, 0m);
        var managerFee = Si(2, paManager.Id, 9, planned.Id, 0m, 25m);
        var executed = Si(3, paSupplier.Id, 1, actual.Id, 9999m, 0m); // type 2: never invoiced
        foreach (var si in new[] { maintenance, managerFee, executed }) si.UnitTypes.Add(new SupplierInvoiceUnitType { UnitTypeId = flat.Id });
        db.AddRange(maintenance, managerFee, executed);
        db.Add(new Benefit { CompanyId = company.Id, ContractId = contract1.Id, PeriodYYMM = 2608, Amount = 50m, EntryDate = DateTimeOffset.UtcNow });

        // Live 2040 balance for customer 1: 100 regular + 23.45 taken-over debt (line type 98).
        var journal = new JournalEntry { CompanyId = company.Id, PostingDate = new DateOnly(2026, 7, 1), Description = "PS", IsPosted = true };
        journal.Lines.Add(new LedgerEntry { CompanyId = company.Id, Account = "2040", PostingDate = journal.PostingDate, DebitAmount = 100m });
        journal.Lines.Add(new LedgerEntry { CompanyId = company.Id, Account = "2040", PostingDate = journal.PostingDate, DebitAmount = 23.45m, LineTypeId = takenOver.Id });
        db.Add(journal);
        foreach (var line in journal.Lines) db.Entry(line).Property("PartnerAccountId").CurrentValue = pa1.Id;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return new Seed(company.Id, staff.Id, c1.Id, c2.Id, pa1.AccountNumber);
    }
}
