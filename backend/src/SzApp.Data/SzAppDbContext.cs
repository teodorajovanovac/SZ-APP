using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SzApp.Data.Configurations.Billing;
using SzApp.Data.Configurations.EtlExtended;
using SzApp.Data.Configurations.Reports;
using SzApp.Data.Entities;

namespace SzApp.Data;

public sealed class SzAppDbContext(DbContextOptions<SzAppDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>(options)
{
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Partner> Partners => Set<Partner>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<ShortList> ShortLists => Set<ShortList>();
    public DbSet<StaffAccess> StaffAccess => Set<StaffAccess>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<IdempotencyRequest> IdempotencyRequests => Set<IdempotencyRequest>();
    public DbSet<EtlRun> EtlRuns => Set<EtlRun>();
    public DbSet<LegacyKeyMap> LegacyKeyMaps => Set<LegacyKeyMap>();
    public DbSet<QuarantineRecord> QuarantineRecords => Set<QuarantineRecord>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        ConfigureIdentity(builder);
        ConfigureMasterData(builder);
        ConfigureFinance(builder);
        ConfigureOperations(builder);
        builder.ApplyBillingModel();
        builder.ApplyReportsModel();
        builder.ApplyEtlExtendedModel();
        builder.ApplyConfigurationsFromAssembly(typeof(SzAppDbContext).Assembly);

        foreach (var foreignKey in builder.Model.GetEntityTypes().SelectMany(x => x.GetForeignKeys()))
        {
            foreignKey.DeleteBehavior = DeleteBehavior.NoAction;
        }
    }

    private static void ConfigureIdentity(ModelBuilder builder)
    {
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("Staff", "auth");
            entity.Property(x => x.PreferredLanguage).HasMaxLength(10).IsUnicode(false);
            entity.Property(x => x.LastIp).HasMaxLength(45).IsUnicode(false);
        });
        builder.Entity<IdentityRole<int>>().ToTable("Role", "auth");
        builder.Entity<IdentityUserRole<int>>().ToTable("StaffRole", "auth");
        builder.Entity<IdentityUserClaim<int>>().ToTable("StaffClaim", "auth");
        builder.Entity<IdentityUserLogin<int>>().ToTable("StaffLogin", "auth");
        builder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaim", "auth");
        builder.Entity<IdentityUserToken<int>>().ToTable("StaffToken", "auth");

        builder.Entity<StaffAccess>(entity =>
        {
            entity.ToTable("StaffAccess", "auth");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).UseIdentityColumn();
            entity.Property(x => x.StaffRole).HasConversion<int>();
            entity.HasIndex(x => new { x.StaffId, x.CompanyId }).IsUnique();
            entity.HasIndex(x => new { x.CompanyId, x.StaffRole });
            entity.HasOne(x => x.Staff).WithMany(x => x.CompanyAccess).HasForeignKey(x => x.StaffId);
            entity.HasOne(x => x.Company).WithMany(x => x.StaffAccess).HasForeignKey(x => x.CompanyId);
        });
    }

    private static void ConfigureMasterData(ModelBuilder builder)
    {
        builder.Entity<Company>(entity =>
        {
            entity.ToTable("Company", "core");
            entity.HasKey(x => x.Id);
            // Stays an IDENTITY column at the DB/FK level -- rebuilding it as a plain column would
            // require dropping and recreating every FK across the schema that references
            // Company.Id (Unit, StaffAccess, Invoice, LedgerEntry, JournalEntry, ...), which SQL
            // Server cannot do via ALTER COLUMN and is too risky to hand-write blind. Instead,
            // CreateCompanyAsync assigns Id explicitly (max+1, or Root's chosen free Id) via
            // SET IDENTITY_INSERT -- same effective behavior the boss asked for, zero schema risk.
            entity.Property(x => x.Id).UseIdentityColumn();
            entity.Property(x => x.ShortName).HasMaxLength(50);
            entity.Property(x => x.PrintName).HasMaxLength(50);
            entity.Property(x => x.RelativeFolderName).HasMaxLength(255);
            entity.Property(x => x.Note).HasMaxLength(255);
            entity.Property(x => x.ExternalAccount).HasMaxLength(255);
            entity.Property(x => x.RowVersion).IsRowVersion();
            entity.HasIndex(x => x.ShortName);
            entity.HasOne(x => x.Partner).WithMany().HasForeignKey(x => x.PartnerId);
            entity.HasOne(x => x.Manager).WithMany().HasForeignKey(x => x.ManagerId);
            entity.HasOne(x => x.CompanyType).WithMany().HasForeignKey(x => x.CompanyTypeId);
            entity.HasOne(x => x.VatType).WithMany().HasForeignKey(x => x.VatTypeId);
            entity.HasOne(x => x.LocationCategory).WithMany().HasForeignKey(x => x.LocationCategoryId);
        });

        builder.Entity<Partner>(entity =>
        {
            entity.ToTable("Partner", "core");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).UseIdentityColumn();
            entity.Property(x => x.ShortName).HasMaxLength(100);
            entity.Property(x => x.Name).HasMaxLength(255);
            entity.Property(x => x.RegistrationNumber).HasMaxLength(10).IsUnicode(false);
            entity.Property(x => x.TaxNumber).HasMaxLength(10).IsUnicode(false);
            entity.Property(x => x.Jbkjs).HasMaxLength(10).IsUnicode(false);
            entity.Property(x => x.IdCardNumber).HasMaxLength(20).IsUnicode(false);
            entity.Property(x => x.Jmbg).HasMaxLength(15).IsUnicode(false);
            entity.Property(x => x.Language).HasMaxLength(10).IsUnicode(false);
            entity.HasIndex(x => x.CompanyId);
            entity.HasIndex(x => x.TaxNumber);
            entity.HasOne(x => x.OwningCompany).WithMany().HasForeignKey(x => x.CompanyId);
            entity.HasOne(x => x.PartnerType).WithMany().HasForeignKey(x => x.PartnerTypeId);
        });

        builder.Entity<Address>(entity =>
        {
            entity.ToTable("Address", "core");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).UseIdentityColumn();
            entity.Property(x => x.StreetAddress).HasColumnName("Address").HasMaxLength(255);
            entity.Property(x => x.PostalCode).HasMaxLength(20).IsUnicode(false);
            entity.Property(x => x.City).HasMaxLength(255);
            entity.Property(x => x.CountryCode).HasMaxLength(2).IsUnicode(false);
            entity.HasIndex(x => new { x.PostalCode, x.City });
        });

        builder.Entity<ShortList>(entity =>
        {
            entity.ToTable("ShortList", "core");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).UseIdentityColumn();
            entity.Property(x => x.TableName).HasMaxLength(100).IsUnicode(false);
            entity.Property(x => x.Caption).HasMaxLength(255);
            entity.Property(x => x.ShortName).HasMaxLength(50);
            entity.Property(x => x.Description).HasMaxLength(255);
            entity.Property(x => x.IndexKey).HasMaxLength(50).IsUnicode(false);
            entity.HasIndex(x => new { x.TableName, x.IndexValue }).IsUnique();
            entity.HasIndex(x => new { x.TableName, x.IndexKey });
        });
    }

    private static void ConfigureFinance(ModelBuilder builder)
    {
        builder.Entity<JournalEntry>(entity =>
        {
            entity.ToTable("JournalEntry", "finance", table =>
            {
                table.HasCheckConstraint("CK_JournalEntry_Posted", "[IsPosted] = 0 OR ([PostedAt] IS NOT NULL AND [PostedUserId] IS NOT NULL)");
            });
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).UseIdentityColumn();
            entity.Property(x => x.Balance).HasPrecision(18, 4);
            entity.Property(x => x.Note).HasMaxLength(255);
            entity.Property(x => x.Description).HasMaxLength(255);
            entity.Property(x => x.Currency).HasMaxLength(3).IsUnicode(false);
            entity.Property(x => x.RowVersion).IsRowVersion();
            entity.HasAlternateKey(x => new { x.Id, x.CompanyId });
            entity.HasIndex(x => new { x.CompanyId, x.PostingDate });
            entity.HasIndex(x => new { x.CompanyId, x.IsPosted });
            entity.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId);
            entity.HasOne(x => x.PostedUser).WithMany().HasForeignKey(x => x.PostedUserId);
            entity.HasOne(x => x.ReversalOf).WithMany().HasForeignKey(x => x.ReversalOfId);
            entity.HasOne(x => x.JournalEntryType).WithMany().HasForeignKey(x => x.JournalEntryTypeId);
        });

        builder.Entity<LedgerEntry>(entity =>
        {
            entity.ToTable("LedgerEntry", "finance", table =>
            {
                table.HasCheckConstraint("CK_LedgerEntry_Amounts", "[DebitAmount] >= 0 AND [CreditAmount] >= 0 AND NOT ([DebitAmount] > 0 AND [CreditAmount] > 0)");
            });
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).UseIdentityColumn();
            entity.Property(x => x.Account).HasMaxLength(10).IsUnicode(false);
            entity.Property(x => x.DebitAmount).HasPrecision(18, 4);
            entity.Property(x => x.CreditAmount).HasPrecision(18, 4);
            entity.Property(x => x.DocumentRef).HasMaxLength(50);
            entity.Property(x => x.Note).HasMaxLength(255);
            entity.Property(x => x.Parameters).HasMaxLength(25);
            entity.Property(x => x.Description).HasMaxLength(255);
            entity.HasIndex(x => new { x.CompanyId, x.PostingDate });
            entity.HasIndex(x => x.JournalEntryId);
            entity.HasOne(x => x.JournalEntry).WithMany(x => x.Lines)
                .HasForeignKey(x => new { x.JournalEntryId, x.CompanyId })
                .HasPrincipalKey(x => new { x.Id, x.CompanyId });
            entity.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId);
            entity.HasOne(x => x.LineType).WithMany().HasForeignKey(x => x.LineTypeId);
        });

        builder.Entity<Invoice>(entity =>
        {
            entity.ToTable("Invoice", "finance", table =>
            {
                table.HasCheckConstraint("CK_Invoice_Totals", "[Amount] >= 0 AND [VatAmount] >= 0 AND [InterestAmount] >= 0 AND [InvoiceTotal] = [Total] + [InterestAmount]");
            });
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).UseIdentityColumn();
            entity.Property(x => x.SequenceNumber).HasMaxLength(20).IsUnicode(false);
            entity.Property(x => x.PartnerName).HasMaxLength(255);
            entity.Property(x => x.Address).HasMaxLength(255);
            entity.Property(x => x.PostalCode).HasMaxLength(50).IsUnicode(false);
            entity.Property(x => x.City).HasMaxLength(50);
            entity.Property(x => x.TaxNumber).HasMaxLength(50).IsUnicode(false);
            entity.Property(x => x.RegistrationNumber).HasMaxLength(50).IsUnicode(false);
            entity.Property(x => x.Currency).HasMaxLength(3).IsUnicode(false);
            entity.Property(x => x.InvoiceDeliveryLocation).HasMaxLength(50);
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.VatRate).HasPrecision(18, 2);
            entity.Property(x => x.VatAmount).HasPrecision(18, 2);
            entity.Property(x => x.Total).HasPrecision(18, 2);
            entity.Property(x => x.InterestAmount).HasPrecision(18, 2);
            entity.Property(x => x.InvoiceTotal).HasPrecision(18, 2);
            entity.Property(x => x.RowVersion).IsRowVersion();
            entity.HasAlternateKey(x => new { x.Id, x.CompanyId });
            entity.HasIndex(x => new { x.CompanyId, x.SequenceNumber }).IsUnique();
            entity.HasIndex(x => new { x.CompanyId, x.IssueDate });
            entity.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId);
            entity.HasOne(x => x.Partner).WithMany().HasForeignKey(x => x.PartnerId);
        });

        builder.Entity<InvoiceLine>(entity =>
        {
            entity.ToTable("InvoiceLine", "finance");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).UseIdentityColumn();
            entity.Property(x => x.Name).HasMaxLength(255);
            entity.Property(x => x.K1).HasPrecision(18, 4);
            entity.Property(x => x.K2).HasPrecision(18, 4);
            entity.Property(x => x.K3).HasPrecision(18, 4);
            entity.Property(x => x.K4).HasPrecision(18, 4);
            entity.Property(x => x.K5).HasPrecision(18, 4);
            entity.Property(x => x.Quantity).HasPrecision(18, 2);
            entity.Property(x => x.PriceEur).HasPrecision(18, 4);
            entity.Property(x => x.ExchangeRateNbs).HasPrecision(18, 4);
            entity.Property(x => x.PricePcs).HasPrecision(18, 4);
            entity.Property(x => x.PriceTotal).HasPrecision(18, 2);
            entity.Property(x => x.VatRate).HasPrecision(18, 2);
            entity.Property(x => x.VatAmount).HasPrecision(18, 2);
            entity.Property(x => x.TotalAmount).HasPrecision(18, 2);
            entity.HasIndex(x => new { x.InvoiceId, x.SortIndex });
            entity.HasOne(x => x.Invoice).WithMany(x => x.Lines)
                .HasForeignKey(x => new { x.InvoiceId, x.CompanyId })
                .HasPrincipalKey(x => new { x.Id, x.CompanyId });
            entity.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId);
            entity.HasOne(x => x.UnitOfMeasure).WithMany().HasForeignKey(x => x.UnitOfMeasureId);
        });
    }

    private static void ConfigureOperations(ModelBuilder builder)
    {
        builder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLog", "ops");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).UseIdentityColumn();
            entity.Property(x => x.CorrelationId).HasMaxLength(64).IsUnicode(false);
            entity.Property(x => x.EntityType).HasMaxLength(100).IsUnicode(false);
            entity.Property(x => x.ItemId).HasMaxLength(100).IsUnicode(false);
            entity.Property(x => x.Action).HasMaxLength(50).IsUnicode(false);
            entity.Property(x => x.EventSource).HasMaxLength(50).IsUnicode(false);
            entity.HasIndex(x => new { x.CompanyId, x.Timestamp });
            entity.HasIndex(x => x.CorrelationId);
            entity.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId);
            entity.HasOne(x => x.Staff).WithMany().HasForeignKey(x => x.StaffId);
        });

        builder.Entity<OutboxMessage>(entity =>
        {
            entity.ToTable("OutboxMessage", "ops");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Type).HasMaxLength(255).IsUnicode(false);
            entity.Property(x => x.DedupeKey).HasMaxLength(128).IsUnicode(false);
            entity.Property(x => x.CorrelationId).HasMaxLength(64).IsUnicode(false);
            entity.HasIndex(x => x.DedupeKey).IsUnique();
            entity.HasIndex(x => new { x.ProcessedAt, x.OccurredAt });
            entity.HasIndex(x => new { x.CompanyId, x.OccurredAt });
            entity.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId);
        });

        builder.Entity<IdempotencyRequest>(entity =>
        {
            entity.ToTable("IdempotencyRequest", "ops");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).UseIdentityColumn();
            entity.Property(x => x.Key).HasMaxLength(128).IsUnicode(false);
            entity.Property(x => x.RequestHash).HasMaxLength(64).IsUnicode(false);
            entity.HasIndex(x => new { x.StaffId, x.CompanyId, x.Key }).IsUnique();
            entity.HasIndex(x => x.ExpiresAt);
            entity.HasOne(x => x.Staff).WithMany().HasForeignKey(x => x.StaffId);
            entity.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId);
        });

        builder.Entity<EtlRun>(entity =>
        {
            entity.ToTable("EtlRun", "etl");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SourceSystem).HasMaxLength(100).IsUnicode(false);
            entity.Property(x => x.SourceFile).HasMaxLength(500);
            entity.Property(x => x.ContentHash).HasMaxLength(64).IsUnicode(false);
            entity.Property(x => x.Status).HasConversion<int>();
            entity.HasIndex(x => new { x.CompanyId, x.SourceSystem, x.ContentHash }).IsUnique();
            entity.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId);
        });

        builder.Entity<LegacyKeyMap>(entity =>
        {
            entity.ToTable("LegacyKeyMap", "etl");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).UseIdentityColumn();
            entity.Property(x => x.SourceTable).HasMaxLength(128).IsUnicode(false);
            entity.Property(x => x.SourceKey).HasMaxLength(255);
            entity.Property(x => x.TargetTable).HasMaxLength(128).IsUnicode(false);
            entity.Property(x => x.TargetKey).HasMaxLength(255);
            entity.HasIndex(x => new { x.SourceTable, x.SourceKey, x.TargetTable }).IsUnique();
            entity.HasOne(x => x.EtlRun).WithMany().HasForeignKey(x => x.EtlRunId);
        });

        builder.Entity<QuarantineRecord>(entity =>
        {
            entity.ToTable("QuarantineRecord", "etl");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).UseIdentityColumn();
            entity.Property(x => x.SourceTable).HasMaxLength(128).IsUnicode(false);
            entity.Property(x => x.SourceKey).HasMaxLength(255);
            entity.Property(x => x.ErrorCode).HasMaxLength(100).IsUnicode(false);
            entity.Property(x => x.ErrorMessage).HasMaxLength(2000);
            entity.HasIndex(x => new { x.EtlRunId, x.SourceTable });
            entity.HasOne(x => x.EtlRun).WithMany().HasForeignKey(x => x.EtlRunId);
        });
    }
}
