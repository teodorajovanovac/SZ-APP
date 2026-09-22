using Microsoft.EntityFrameworkCore;
using SzApp.Data.Configurations.Billing;
using SzApp.Data.Entities;
using SzApp.Data.Entities.Billing;

namespace SzApp.IntegrationTests.Billing;

public sealed class BillingModelTests
{
    [Fact]
    public void ModelBuilderExtension_ConfiguresFinancialPrecisionAndTenantIndexes()
    {
        var options = new DbContextOptionsBuilder<BillingModelContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=SzAppBillingModelOnly;Trusted_Connection=True")
            .Options;
        using var context = new BillingModelContext(options);

        var notice = context.Model.FindEntityType(typeof(Notice))!;
        Assert.Equal(18, notice.FindProperty(nameof(Notice.Total))!.GetPrecision());
        Assert.Equal(2, notice.FindProperty(nameof(Notice.Total))!.GetScale());
        Assert.Contains(notice.GetIndexes(), index => index.IsUnique &&
            index.Properties.Select(x => x.Name).SequenceEqual([nameof(Notice.CompanyId), nameof(Notice.NoticeBatchId), nameof(Notice.PartnerAccountId)]));

        var supplier = context.Model.FindEntityType(typeof(SupplierInvoice))!;
        Assert.Equal(4, supplier.FindProperty(nameof(SupplierInvoice.InvoiceTotalCalculationAmountRsd))!.GetScale());

        var invoice = context.Model.FindEntityType(typeof(SzApp.Data.Entities.Invoice))!;
        Assert.NotNull(invoice.FindProperty("InvoiceBatchId"));
        Assert.NotNull(invoice.FindProperty("CancelReason"));
    }

    private sealed class BillingModelContext(DbContextOptions<BillingModelContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApplicationUser>().Ignore(x => x.CompanyAccess);
            modelBuilder.Entity<Company>().Ignore(x => x.StaffAccess);
            modelBuilder.Entity<Company>().HasOne(x => x.Partner).WithMany().HasForeignKey(x => x.PartnerId).OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Company>().HasOne(x => x.Manager).WithMany().HasForeignKey(x => x.ManagerId).OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Partner>().HasOne(x => x.OwningCompany).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<SzApp.Data.Entities.Invoice>().HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<SzApp.Data.Entities.Invoice>().HasOne(x => x.Partner).WithMany().HasForeignKey(x => x.PartnerId).OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<SzApp.Data.Entities.Invoice>().HasMany(x => x.Lines).WithOne(x => x.Invoice).HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<SzApp.Data.Entities.InvoiceLine>().HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<SzApp.Data.Entities.InvoiceLine>().HasOne(x => x.UnitOfMeasure).WithMany().HasForeignKey(x => x.UnitOfMeasureId).OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Contract>().Ignore(x => x.Company).Ignore(x => x.Unit).Ignore(x => x.OwnerPartner)
                .Ignore(x => x.InvoicePartner).Ignore(x => x.TenantPartner).Ignore(x => x.InvoiceDeliveryUnit);
            modelBuilder.Entity<PartnerAccount>().Ignore(x => x.Company).Ignore(x => x.Partner).Ignore(x => x.Contract);
            modelBuilder.ApplyBillingModel();
        }
    }
}
