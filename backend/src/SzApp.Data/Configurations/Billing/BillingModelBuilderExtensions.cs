using Microsoft.EntityFrameworkCore;
using SzApp.Data.Entities;
using SzApp.Data.Entities.Billing;
using BillingInvoice = SzApp.Data.Entities.Invoice;
using BillingInvoiceLine = SzApp.Data.Entities.InvoiceLine;

namespace SzApp.Data.Configurations.Billing;

public static class BillingModelBuilderExtensions
{
    public static ModelBuilder ApplyBillingModel(this ModelBuilder builder)
    {
        ConfigureCalculationTypes(builder);
        ConfigureSupplierInvoices(builder);
        ConfigureInvoiceBatches(builder);
        ExtendInvoices(builder);
        ConfigureBenefitsAndInterest(builder);
        ConfigureNotices(builder);
        ConfigurePaymentOrders(builder);
        return builder;
    }

    private static void ConfigureCalculationTypes(ModelBuilder builder)
    {
        builder.Entity<CalculationType>(entity =>
        {
            entity.ToTable("CalculationType", "billing");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).UseIdentityColumn();
            entity.Property(x => x.Name).HasMaxLength(100);
            entity.Property(x => x.SupplierAmountRule).HasMaxLength(100).IsUnicode(false);
            entity.Property(x => x.AllocationRule).HasMaxLength(100).IsUnicode(false);
            entity.Property(x => x.QuantityRule).HasMaxLength(100).IsUnicode(false);
            entity.Property(x => x.Note).HasMaxLength(255);
            entity.HasIndex(x => x.Name).IsUnique();
            entity.HasOne<ShortList>().WithMany().HasForeignKey(x => x.UnitOfMeasureId).OnDelete(DeleteBehavior.NoAction);
        });
    }

    private static void ConfigureSupplierInvoices(ModelBuilder builder)
    {
        builder.Entity<SupplierInvoice>(entity =>
        {
            entity.ToTable("SupplierInvoice", "billing", table =>
            {
                table.HasCheckConstraint("CK_SupplierInvoice_Period", "[PeriodYYMM] BETWEEN 1001 AND 9912 AND [PeriodYYMM] % 100 BETWEEN 1 AND 12");
                table.HasCheckConstraint("CK_SupplierInvoice_Amounts", "[InvoiceTotalCalculationAmountEur] >= 0 AND [InvoiceTotalCalculationAmountRsd] >= 0 AND [PostedInvoiceAmount] >= 0");
            });
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).UseIdentityColumn();
            entity.Property(x => x.CodeName).HasMaxLength(255);
            entity.Property(x => x.Caption).HasMaxLength(255);
            entity.Property(x => x.InvoiceTotalCalculationAmountEur).HasPrecision(18, 4);
            entity.Property(x => x.InvoiceTotalCalculationAmountRsd).HasPrecision(18, 4);
            entity.Property(x => x.CalculationAmountByCoefficientEur).HasPrecision(18, 4);
            entity.Property(x => x.CalculationAmountByCoefficientRsd).HasPrecision(18, 4);
            entity.Property(x => x.PostedInvoiceAmount).HasPrecision(18, 2);
            entity.Property(x => x.SubAccountId).HasMaxLength(10).IsUnicode(false);
            entity.Property(x => x.ExtraordinaryInvoiceMarker).HasMaxLength(10);
            entity.Property(x => x.InvoiceNameFunction).HasMaxLength(100).IsUnicode(false);
            entity.Property(x => x.InvoiceDescription).HasMaxLength(255);
            entity.Property(x => x.PaymentReference).HasMaxLength(255);
            entity.Property(x => x.ClosesAccount).HasMaxLength(10);
            entity.Property(x => x.RowVersion).IsRowVersion();
            entity.HasIndex(x => new { x.CompanyId, x.PeriodYYMM, x.InvoiceNo }).IsUnique();
            entity.HasIndex(x => x.CalculationTypeId);
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne<CalculationType>().WithMany().HasForeignKey(x => x.CalculationTypeId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne<PartnerAccount>().WithMany().HasForeignKey(x => x.SupplierPartnerAccountId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne<ShortList>().WithMany().HasForeignKey(x => x.DocumentTypeId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne<SupplierInvoice>().WithMany().HasForeignKey(x => x.PreviousSupplierInvoiceId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne<SupplierInvoice>().WithMany().HasForeignKey(x => x.NewSupplierInvoiceId).OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<SupplierInvoiceUnitType>(entity =>
        {
            entity.ToTable("SupplierInvoiceUnitType", "billing");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).UseIdentityColumn();
            entity.HasIndex(x => new { x.SupplierInvoiceId, x.UnitTypeId }).IsUnique();
            entity.HasOne(x => x.SupplierInvoice).WithMany(x => x.UnitTypes).HasForeignKey(x => x.SupplierInvoiceId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<ShortList>().WithMany().HasForeignKey(x => x.UnitTypeId).OnDelete(DeleteBehavior.NoAction);
        });
    }

    private static void ConfigureInvoiceBatches(ModelBuilder builder)
    {
        builder.Entity<InvoiceBatch>(entity =>
        {
            entity.ToTable("InvoiceBatch", "billing", table =>
            {
                table.HasCheckConstraint("CK_InvoiceBatch_Period", "[Month] BETWEEN 1 AND 12 AND [PeriodYYMM] = ([Year] % 100) * 100 + [Month]");
                table.HasCheckConstraint("CK_InvoiceBatch_Dates", "[ServiceDateFrom] <= [ServiceDateTo] AND [IssueDate] <= [DueDate]");
            });
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).UseIdentityColumn();
            entity.Property(x => x.Caption).HasMaxLength(50);
            entity.Property(x => x.Place).HasMaxLength(50);
            entity.Property(x => x.ExchangeRateNbs).HasPrecision(18, 4);
            entity.Property(x => x.ExtraordinaryInvoiceMarker).HasMaxLength(255);
            entity.Property(x => x.PaymentPurpose).HasMaxLength(255);
            entity.Property(x => x.Status).HasConversion<int>();
            entity.Property(x => x.GenerationFingerprint).HasMaxLength(64).IsUnicode(false);
            entity.Property(x => x.RowVersion).IsRowVersion();
            entity.HasIndex(x => new { x.CompanyId, x.PeriodYYMM, x.ExtraordinaryInvoiceMarker }).IsUnique();
            entity.HasIndex(x => new { x.CompanyId, x.Status });
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.StaffId).OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<InvoiceUnit>(entity =>
        {
            entity.ToTable("InvoiceUnit", "billing");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).UseIdentityColumn();
            entity.HasIndex(x => new { x.InvoiceId, x.ContractId }).IsUnique();
            entity.HasIndex(x => new { x.CompanyId, x.ContractId });
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne<BillingInvoice>().WithMany().HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<Contract>().WithMany().HasForeignKey(x => x.ContractId).OnDelete(DeleteBehavior.NoAction);
        });
    }

    private static void ExtendInvoices(ModelBuilder builder)
    {
        builder.Entity<BillingInvoice>(entity =>
        {
            entity.Property<int?>("InvoiceBatchId");
            entity.Property<string?>("PlaceOfIssue").HasMaxLength(50);
            entity.Property<DateOnly?>("ServiceDateFrom");
            entity.Property<DateOnly?>("ServiceDateTo");
            entity.Property<DateOnly?>("TransactionDate");
            entity.Property<string?>("Pak").HasMaxLength(50).IsUnicode(false);
            entity.Property<DateOnly?>("BalanceAsOfDate");
            entity.Property<decimal>("PreviousBalance").HasPrecision(18, 2);
            entity.Property<int?>("NoticeId");
            entity.Property<string?>("PrintNote");
            entity.Property<int?>("InvoiceDeliveryUnitId");
            entity.Property<string?>("DeliveryLocation").HasMaxLength(255);
            entity.Property<int?>("InvoiceLayoutId");
            entity.Property<int?>("InvoiceLegacyMasterId");
            entity.Property<int?>("InvoiceParentId");
            entity.Property<string?>("PaymentReference").HasMaxLength(50);
            entity.Property<string?>("Note").HasMaxLength(255);
            entity.Property<string?>("CancelReason").HasMaxLength(500);
            entity.Property<int>("PageCount");
            entity.Property<int>("SortIndex");
            entity.HasIndex("CompanyId", "InvoiceBatchId", "PartnerId", "SequenceNumber").IsUnique();
            entity.HasOne<InvoiceBatch>().WithMany().HasForeignKey("InvoiceBatchId").OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<BillingInvoiceLine>(entity =>
        {
            entity.Property<int?>("InvoiceBatchId");
            entity.Property<int?>("PartnerId");
            entity.Property<int?>("SupplierInvoiceId");
            entity.Property<string>("Currency").HasMaxLength(3).IsUnicode(false).HasDefaultValue("RSD");
            entity.HasIndex("InvoiceBatchId");
            entity.HasIndex("SupplierInvoiceId");
            entity.HasOne<InvoiceBatch>().WithMany().HasForeignKey("InvoiceBatchId").OnDelete(DeleteBehavior.NoAction);
            entity.HasOne<SupplierInvoice>().WithMany().HasForeignKey("SupplierInvoiceId").OnDelete(DeleteBehavior.NoAction);
        });
    }

    private static void ConfigureBenefitsAndInterest(ModelBuilder builder)
    {
        builder.Entity<Benefit>(entity =>
        {
            entity.ToTable("Benefit", "billing", table =>
                table.HasCheckConstraint("CK_Benefit_Amount", "[Amount] >= 0"));
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).UseIdentityColumn();
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.RowVersion).IsRowVersion();
            entity.HasIndex(x => new { x.CompanyId, x.ContractId, x.PeriodYYMM }).IsUnique();
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne<Contract>().WithMany().HasForeignKey(x => x.ContractId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne<BillingInvoice>().WithMany().HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<InterestRate>(entity =>
        {
            entity.ToTable("InterestRate", "billing", table =>
                table.HasCheckConstraint("CK_InterestRate_Rate", "[Rate] >= 0 AND [TimeCode] IN ('M', 'G')"));
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).UseIdentityColumn();
            entity.Property(x => x.Rate).HasPrecision(18, 4);
            entity.Property(x => x.TimeCode).HasMaxLength(1).IsUnicode(false);
            entity.HasIndex(x => new { x.Date, x.TimeCode }).IsUnique();
        });

        builder.Entity<InterestStatement>(entity =>
        {
            entity.ToTable("InterestStatement", "billing");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).UseIdentityColumn();
            entity.Property(x => x.Account).HasMaxLength(50).IsUnicode(false);
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.Balance).HasPrecision(18, 2);
            entity.Property(x => x.Rate).HasPrecision(18, 4);
            entity.Property(x => x.Coefficient).HasPrecision(18, 4);
            entity.Property(x => x.Interest).HasPrecision(18, 2);
            entity.Property(x => x.SubAccountId).HasMaxLength(10).IsUnicode(false);
            entity.HasIndex(x => new { x.CompanyId, x.InvoiceBatchId, x.PartnerAccountId });
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne<InvoiceBatch>().WithMany().HasForeignKey(x => x.InvoiceBatchId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne<PartnerAccount>().WithMany().HasForeignKey(x => x.PartnerAccountId).OnDelete(DeleteBehavior.NoAction);
        });
    }

    private static void ConfigureNotices(ModelBuilder builder)
    {
        builder.Entity<NoticeTemplate>(entity =>
        {
            entity.ToTable("NoticeTemplate", "billing");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).UseIdentityColumn();
            entity.Property(x => x.Name).HasMaxLength(100);
            entity.Property(x => x.RowVersion).IsRowVersion();
            entity.HasIndex(x => new { x.CompanyId, x.Name }).IsUnique();
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<NoticeBatch>(entity =>
        {
            entity.ToTable("NoticeBatch", "billing");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).UseIdentityColumn();
            entity.Property(x => x.Title).HasMaxLength(50);
            entity.Property(x => x.DebtTolerance).HasPrecision(18, 2);
            entity.Property(x => x.DebtToleranceByMonth).HasPrecision(18, 2);
            entity.Property(x => x.CustomCaptionOnSlip).HasMaxLength(255);
            entity.Property(x => x.GenerationFingerprint).HasMaxLength(64).IsUnicode(false);
            entity.Property(x => x.RowVersion).IsRowVersion();
            entity.HasIndex(x => new { x.CompanyId, x.Date, x.Title });
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne<NoticeTemplate>().WithMany().HasForeignKey(x => x.NoticeTemplateId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne<ShortList>().WithMany().HasForeignKey(x => x.NoticeTypeId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne<InvoiceBatch>().WithMany().HasForeignKey(x => x.InvoiceBatchId).OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<Notice>(entity =>
        {
            entity.ToTable("Notice", "billing", table =>
            {
                table.HasCheckConstraint("CK_Notice_Amounts", "[Debt] >= 0 AND [AdditionalCosts] >= 0 AND [Total] = [Debt] + [AdditionalCosts]");
            });
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).UseIdentityColumn();
            entity.Property(x => x.Debt).HasPrecision(18, 2);
            entity.Property(x => x.InvoiceText).HasMaxLength(255);
            entity.Property(x => x.PaymentReference).HasMaxLength(50);
            entity.Property(x => x.AdditionalCosts).HasPrecision(18, 2);
            entity.Property(x => x.Total).HasPrecision(18, 2);
            entity.Property(x => x.DeliveryStatus).HasConversion<int>();
            entity.Property(x => x.RenderedDocumentPath).HasMaxLength(500);
            entity.Property(x => x.RowVersion).IsRowVersion();
            entity.HasIndex(x => new { x.CompanyId, x.NoticeBatchId, x.PartnerAccountId }).IsUnique();
            entity.HasOne(x => x.NoticeBatch).WithMany(x => x.Notices).HasForeignKey(x => x.NoticeBatchId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne<PartnerAccount>().WithMany().HasForeignKey(x => x.PartnerAccountId).OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<NoticeLine>(entity =>
        {
            entity.ToTable("NoticeLine", "billing");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).UseIdentityColumn();
            entity.Property(x => x.Parameters).HasMaxLength(25);
            entity.Property(x => x.DocumentRef).HasMaxLength(255);
            entity.Property(x => x.Debit).HasPrecision(18, 2);
            entity.Property(x => x.Credit).HasPrecision(18, 2);
            entity.Property(x => x.Sum).HasPrecision(18, 2);
            entity.Property(x => x.Text).HasMaxLength(255);
            entity.Property(x => x.UnitAddress).HasMaxLength(255);
            entity.HasOne(x => x.Notice).WithMany(x => x.Lines).HasForeignKey(x => x.NoticeId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<BillingInvoice>().WithMany().HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.NoAction);
        });
    }

    private static void ConfigurePaymentOrders(ModelBuilder builder)
    {
        builder.Entity<PaymentOrder>(entity =>
        {
            entity.ToTable("PaymentOrder", "billing", table =>
                table.HasCheckConstraint("CK_PaymentOrder_Amount", "[Amount] > 0"));
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).UseIdentityColumn();
            entity.Property(x => x.TemplateTitle).HasMaxLength(50);
            entity.Property(x => x.PayerName).HasMaxLength(255);
            entity.Property(x => x.PaymentPurpose).HasMaxLength(255);
            entity.Property(x => x.RecipientName).HasMaxLength(255);
            entity.Property(x => x.Currency).HasMaxLength(3).IsUnicode(false);
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.PayerAccountNumber).HasMaxLength(50).IsUnicode(false);
            entity.Property(x => x.PayerPaymentReference).HasMaxLength(50);
            entity.Property(x => x.RecipientAccountNumber).HasMaxLength(50).IsUnicode(false);
            entity.Property(x => x.RecipientPaymentReference).HasMaxLength(50);
            entity.Property(x => x.Place).HasMaxLength(50);
            entity.Property(x => x.RowVersion).IsRowVersion();
            entity.HasIndex(x => new { x.CompanyId, x.IsArchived, x.Date });
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne<ShortList>().WithMany().HasForeignKey(x => x.PaymentOrderTypeId).OnDelete(DeleteBehavior.NoAction);
        });
    }
}
