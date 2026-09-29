using Microsoft.EntityFrameworkCore;
using SzApp.Api.Features.Billing;
using SzApp.Api.Features.Platform;
using SzApp.Data;
using SzApp.Data.Entities;
using SzApp.Data.Entities.Billing;

namespace SzApp.UnitTests.Billing;

/// <summary>
/// Outbox-enqueue smoke test for bulk invoice emailing: N invoices, M without a registered
/// email, verifies the right number of SentEmail/OutboxMessage rows land (and none for the
/// missing-email ones). Uses EF Core's InMemory provider (fast, no Docker) rather than the
/// repo's Testcontainers-gated SQL Server integration tests, since this only exercises
/// InvoicePdfService's own query/enqueue logic, not SQL Server-specific behavior.
/// </summary>
public class InvoicePdfServiceOutboxTests
{
    [Fact]
    public async Task SendBatchEmailsAsync_EnqueuesOneOutboxMessagePerInvoiceWithEmail_SkipsMissingEmails()
    {
        await using var db = CreateDb();
        var (companyId, batchId, staffId) = await SeedAsync(db, invoicesWithEmail: 2, invoicesWithoutEmail: 1);
        var service = new InvoicePdfService(db, new InvoiceDocumentMapper(db), new FakeDocumentStorage(), TimeProvider.System);

        var result = await service.SendBatchEmailsAsync(companyId, batchId, staffId, CancellationToken.None);

        Assert.Equal(2, result.Enqueued);
        Assert.Equal(1, result.Skipped);
        Assert.Equal(2, await db.Set<SentEmail>().CountAsync(x => x.CompanyId == companyId));
        Assert.Equal(2, await db.OutboxMessages.CountAsync(x => x.Type == "platform.email.send"));
        Assert.All(await db.Set<SentEmail>().Include(x => x.Attachments).ToListAsync(),
            email => Assert.Single(email.Attachments));
    }

    [Fact]
    public async Task PreviewBatchEmailsAsync_ReportsCountsWithoutSideEffects()
    {
        await using var db = CreateDb();
        var (companyId, batchId, _) = await SeedAsync(db, invoicesWithEmail: 3, invoicesWithoutEmail: 2);
        var service = new InvoicePdfService(db, new InvoiceDocumentMapper(db), new FakeDocumentStorage(), TimeProvider.System);

        var preview = await service.PreviewBatchEmailsAsync(companyId, batchId, CancellationToken.None);

        Assert.Equal(5, preview.TotalInvoices);
        Assert.Equal(3, preview.WithEmail);
        Assert.Equal(2, preview.MissingEmail);
        Assert.Equal(0, await db.OutboxMessages.CountAsync());
    }

    private static SzAppDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<SzAppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static async Task<(int CompanyId, int BatchId, int StaffId)> SeedAsync(SzAppDbContext db, int invoicesWithEmail, int invoicesWithoutEmail)
    {
        var staff = new ApplicationUser { UserName = "tester", Email = "tester@szapp.local" };
        db.Add(staff);

        var issuerPartner = new Partner { ShortName = "SZ", Name = "SZ Test" };
        db.Add(issuerPartner);
        await db.SaveChangesAsync();

        var company = new Company { PartnerId = issuerPartner.Id, ShortName = "SZ1", PrintName = "SZ Test" };
        db.Add(company);
        await db.SaveChangesAsync();

        var batch = new InvoiceBatch
        {
            CompanyId = company.Id, PeriodYYMM = 2609, Caption = "Septembar 2026", Month = 9, Year = 26,
            Place = "Beograd", IssueDate = new DateOnly(2026, 9, 15), ServiceDateFrom = new DateOnly(2026, 9, 1),
            ServiceDateTo = new DateOnly(2026, 9, 30), TransactionDate = new DateOnly(2026, 9, 30),
            DueDate = new DateOnly(2026, 10, 25), ExchangeRateNbs = 117.3m, EntryDate = DateTimeOffset.UtcNow,
            StaffId = staff.Id, Status = BillingBatchStatus.Generated
        };
        db.Add(batch);
        await db.SaveChangesAsync();

        var invoiceIndex = 0;
        for (var i = 0; i < invoicesWithEmail; i++) await AddInvoiceAsync(db, company.Id, batch.Id, ++invoiceIndex, withEmail: true);
        for (var i = 0; i < invoicesWithoutEmail; i++) await AddInvoiceAsync(db, company.Id, batch.Id, ++invoiceIndex, withEmail: false);

        return (company.Id, batch.Id, staff.Id);
    }

    private static async Task AddInvoiceAsync(SzAppDbContext db, int companyId, int batchId, int index, bool withEmail)
    {
        var customer = new Partner { ShortName = $"K{index}", Name = $"Kupac {index}" };
        db.Add(customer);
        await db.SaveChangesAsync();

        var invoice = new Invoice
        {
            CompanyId = companyId, PartnerId = customer.Id, SequenceNumber = $"251-{1000 + index}-2609",
            IssueDate = new DateOnly(2026, 9, 15), DueDate = new DateOnly(2026, 10, 25),
            PartnerName = customer.Name, Address = "Ulica 1", City = "Beograd", Amount = 2000m,
            VatAmount = 0m, Total = 2000m, InterestAmount = 0m, InvoiceTotal = 2000m
        };
        db.Add(invoice);
        db.Entry(invoice).Property("InvoiceBatchId").CurrentValue = batchId;
        db.Entry(invoice).Property("PaymentReference").CurrentValue = $"08-251-{1000 + index}-2609";
        db.Entry(invoice).Property("PreviousBalance").CurrentValue = 0m;
        await db.SaveChangesAsync();

        if (withEmail)
        {
            db.Add(new PartnerCommunication
            {
                PartnerId = customer.Id, ChannelId = 1, ValueNormalized = $"kupac{index}@example.com",
                IsActive = true, IsRegisteredForInvoiceReceipt = true, SortIndex = 0
            });
            await db.SaveChangesAsync();
        }
    }

    private sealed class FakeDocumentStorage : IPlatformDocumentStorage
    {
        public Task<(string RelativePath, string Sha256)> SaveAsync(int companyId, string fileName, string contentType, long length, Stream content, CancellationToken cancellationToken) =>
            Task.FromResult(($"fake/{companyId}/{fileName}", "sha"));

        public Task<Stream> OpenReadAsync(string relativePath, CancellationToken cancellationToken) =>
            Task.FromResult<Stream>(new MemoryStream());

        public Task DeleteAsync(string relativePath, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
