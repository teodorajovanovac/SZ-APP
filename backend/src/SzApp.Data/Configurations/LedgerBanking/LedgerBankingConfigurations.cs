using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SzApp.Data.Entities;
using SzApp.Data.Entities.LedgerBanking;

namespace SzApp.Data.Configurations.LedgerBanking;

public static class LedgerBankingModelBuilderExtensions
{
    public static ModelBuilder ApplyLedgerBankingModel(this ModelBuilder builder)
    {
        new ChartAccountConfiguration().Configure(builder.Entity<ChartAccount>());
        new SubAccountConfiguration().Configure(builder.Entity<SubAccount>());
        new BankStatementConfiguration().Configure(builder.Entity<BankStatement>());
        new BankStatementLineConfiguration().Configure(builder.Entity<BankStatementLine>());
        new BankInFlowConfiguration().Configure(builder.Entity<BankInFlow>());
        new BankStatementPostingTemplateConfiguration().Configure(builder.Entity<BankStatementPostingTemplate>());
        new PostingSchemeConfiguration().Configure(builder.Entity<PostingScheme>());
        new LedgerSourcePostingConfiguration().Configure(builder.Entity<LedgerSourcePosting>());
        new JournalEntryLedgerGuardConfiguration().Configure(builder.Entity<JournalEntry>());
        new LedgerEntryBankingExtensionConfiguration().Configure(builder.Entity<LedgerEntry>());
        new PartnerAccountFinanceConfiguration().Configure(builder.Entity<PartnerAccount>());
        return builder;
    }
}

public sealed class ChartAccountConfiguration : IEntityTypeConfiguration<ChartAccount>
{
    public void Configure(EntityTypeBuilder<ChartAccount> entity)
    {
        entity.ToTable("ChartOfAccounts", "finance", table =>
            table.HasCheckConstraint("CK_ChartOfAccounts_Sign", "[Sign] IN (-1, 1)"));
        entity.HasKey(x => x.Account);
        entity.Property(x => x.Account).HasMaxLength(10).IsUnicode(false).ValueGeneratedNever();
        entity.Property(x => x.ShortName).HasMaxLength(50);
        entity.Property(x => x.Name).HasMaxLength(255);
        entity.Property(x => x.ParentAccount).HasMaxLength(10).IsUnicode(false);
        entity.HasOne(x => x.Parent).WithMany().HasForeignKey(x => x.ParentAccount).OnDelete(DeleteBehavior.NoAction);
        entity.HasIndex(x => new { x.IsActive, x.Account });
    }
}

public sealed class SubAccountConfiguration : IEntityTypeConfiguration<SubAccount>
{
    public void Configure(EntityTypeBuilder<SubAccount> entity)
    {
        entity.ToTable("SubAccount", "finance");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasMaxLength(10).IsUnicode(false).ValueGeneratedNever();
        entity.Property(x => x.Name).HasMaxLength(50);
        entity.Property(x => x.ParentSubAccountId).HasMaxLength(10).IsUnicode(false);
        entity.Property(x => x.CostToSubAccountId).HasMaxLength(10).IsUnicode(false);
        entity.Property(x => x.InterestSubAccountId).HasMaxLength(10).IsUnicode(false);
        entity.HasOne(x => x.Parent).WithMany().HasForeignKey(x => x.ParentSubAccountId).OnDelete(DeleteBehavior.NoAction);
        entity.HasOne(x => x.CostToSubAccount).WithMany().HasForeignKey(x => x.CostToSubAccountId).OnDelete(DeleteBehavior.NoAction);
        entity.HasOne(x => x.InterestSubAccount).WithMany().HasForeignKey(x => x.InterestSubAccountId).OnDelete(DeleteBehavior.NoAction);
        entity.HasOne<PartnerAccount>().WithMany().HasForeignKey(x => x.DefaultSupplierPartnerAccountId).OnDelete(DeleteBehavior.NoAction);
        entity.HasIndex(x => new { x.IsActive, x.Id });
    }
}

public sealed class BankStatementConfiguration : IEntityTypeConfiguration<BankStatement>
{
    public void Configure(EntityTypeBuilder<BankStatement> entity)
    {
        entity.ToTable("BankStatement", "finance", table =>
        {
            table.HasCheckConstraint("CK_BankStatement_Amounts", "[PreviousBalance] + [Credit] - [Debit] = [NewBalance] AND [Debit] >= 0 AND [Credit] >= 0");
            table.HasCheckConstraint("CK_BankStatement_Counts", "[CountDebitEntry] >= 0 AND [CountCreditEntry] >= 0");
        });
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).UseIdentityColumn();
        entity.Property(x => x.LedgerAccount).HasMaxLength(10).IsUnicode(false);
        entity.Property(x => x.StatementSuffix).HasMaxLength(50);
        entity.Property(x => x.PreviousBalance).HasPrecision(18, 2);
        entity.Property(x => x.NewBalance).HasPrecision(18, 2);
        entity.Property(x => x.Debit).HasPrecision(18, 2);
        entity.Property(x => x.Credit).HasPrecision(18, 2);
        entity.Property(x => x.Note).HasMaxLength(255);
        entity.Property(x => x.Status).HasConversion<int>();
        entity.Property(x => x.RowVersion).IsRowVersion();
        entity.HasAlternateKey(x => new { x.Id, x.CompanyId });
        // Explicit HasFilter(null) overrides the SQL Server provider's default convention of
        // auto-filtering out NULLs on a unique index that includes a nullable column (StatementSuffix) --
        // without this, SQL Server would silently allow duplicate statements when StatementSuffix is null,
        // which is the common case (see docs/predlog.md finding on bank-statement dedup).
        entity.HasIndex(x => new { x.CompanyId, x.BankAccountId, x.StatementNumber, x.StatementSuffix, x.Date }).IsUnique().HasFilter(null);
        entity.HasIndex(x => new { x.CompanyId, x.Status, x.Date });
        entity.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.NoAction);
        entity.HasOne<BankAccount>().WithMany().HasForeignKey(x => x.BankAccountId).OnDelete(DeleteBehavior.NoAction);
        entity.HasOne(x => x.JournalEntry).WithMany()
            .HasForeignKey(x => new { x.JournalEntryId, x.CompanyId })
            .HasPrincipalKey(x => new { x.Id, x.CompanyId })
            .OnDelete(DeleteBehavior.NoAction);
        entity.HasOne<ChartAccount>().WithMany().HasForeignKey(x => x.LedgerAccount).OnDelete(DeleteBehavior.NoAction);
    }
}

public sealed class BankStatementLineConfiguration : IEntityTypeConfiguration<BankStatementLine>
{
    public void Configure(EntityTypeBuilder<BankStatementLine> entity)
    {
        entity.ToTable("BankStatementLine", "finance", table =>
            table.HasCheckConstraint("CK_BankStatementLine_Amounts", "[Debit] >= 0 AND [Credit] >= 0 AND (([Debit] > 0 AND [Credit] = 0) OR ([Debit] = 0 AND [Credit] > 0))"));
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).UseIdentityColumn();
        entity.Property(x => x.PayerRecipientName).HasMaxLength(255);
        entity.Property(x => x.BankAccountNumber).HasMaxLength(50).IsUnicode(false);
        entity.Property(x => x.Debit).HasPrecision(18, 2);
        entity.Property(x => x.Credit).HasPrecision(18, 2);
        entity.Property(x => x.Info).HasMaxLength(255);
        entity.Property(x => x.PaymentReference).HasMaxLength(50);
        entity.Property(x => x.PaymentReferenceOut).HasMaxLength(50);
        entity.Property(x => x.SubAccountId).HasMaxLength(10).IsUnicode(false);
        entity.Property(x => x.CounterAccount).HasMaxLength(10).IsUnicode(false);
        entity.Property(x => x.Status).HasConversion<int>();
        entity.Property(x => x.BankRef).HasMaxLength(50);
        entity.Property(x => x.RowVersion).IsRowVersion();
        entity.HasAlternateKey(x => new { x.Id, x.CompanyId });
        entity.HasIndex(x => new { x.BankStatementId, x.LineNumber }).IsUnique();
        entity.HasIndex(x => new { x.CompanyId, x.Status });
        entity.HasIndex(x => x.PaymentReference);
        entity.HasOne(x => x.BankStatement).WithMany(x => x.Lines)
            .HasForeignKey(x => new { x.BankStatementId, x.CompanyId })
            .HasPrincipalKey(x => new { x.Id, x.CompanyId })
            .OnDelete(DeleteBehavior.NoAction);
        entity.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.NoAction);
        entity.HasOne(x => x.SubAccount).WithMany().HasForeignKey(x => x.SubAccountId).OnDelete(DeleteBehavior.NoAction);
        entity.HasOne<PartnerAccount>().WithMany().HasForeignKey(x => x.PartnerAccountId).OnDelete(DeleteBehavior.NoAction);
        entity.HasOne<ChartAccount>().WithMany().HasForeignKey(x => x.CounterAccount).OnDelete(DeleteBehavior.NoAction);
    }
}

public sealed class BankInFlowConfiguration : IEntityTypeConfiguration<BankInFlow>
{
    public void Configure(EntityTypeBuilder<BankInFlow> entity)
    {
        entity.ToTable("BankInFlow", "finance");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).UseIdentityColumn();
        entity.Property(x => x.ReferenceNumber).HasMaxLength(255);
        entity.Property(x => x.Currency).HasMaxLength(3).IsUnicode(false);
        entity.Property(x => x.OriginalAmount).HasPrecision(18, 2);
        entity.Property(x => x.AmountLocalCurrency).HasPrecision(18, 2);
        entity.Property(x => x.InvoiceDescription).HasMaxLength(50);
        entity.HasIndex(x => x.BankStatementLineId).IsUnique();
        entity.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.NoAction);
        entity.HasOne<BankAccount>().WithMany().HasForeignKey(x => x.BankAccountId).OnDelete(DeleteBehavior.NoAction);
        entity.HasOne<PartnerAccount>().WithMany().HasForeignKey(x => x.PartnerAccountId).OnDelete(DeleteBehavior.NoAction);
        entity.HasOne(x => x.BankStatementLine).WithMany()
            .HasForeignKey(x => new { x.BankStatementLineId, x.CompanyId })
            .HasPrincipalKey(x => new { x.Id, x.CompanyId })
            .OnDelete(DeleteBehavior.NoAction);
    }
}

public sealed class BankStatementPostingTemplateConfiguration : IEntityTypeConfiguration<BankStatementPostingTemplate>
{
    public void Configure(EntityTypeBuilder<BankStatementPostingTemplate> entity)
    {
        entity.ToTable("BankStatementPostingTemplate", "finance");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).UseIdentityColumn();
        entity.Property(x => x.TemplateName).HasMaxLength(255);
        entity.Property(x => x.FieldName).HasMaxLength(50).IsUnicode(false);
        entity.Property(x => x.FieldValue).HasMaxLength(255);
        entity.Property(x => x.Function).HasConversion<int>();
        entity.Property(x => x.SetSubAccountId).HasMaxLength(10).IsUnicode(false);
        entity.Property(x => x.SetAccountCode).HasMaxLength(10).IsUnicode(false);
        entity.HasIndex(x => new { x.CompanyId, x.IsActive, x.SortIndex });
        entity.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.NoAction);
        entity.HasOne(x => x.Parent).WithMany().HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.NoAction);
        entity.HasOne(x => x.SetSubAccount).WithMany().HasForeignKey(x => x.SetSubAccountId).OnDelete(DeleteBehavior.NoAction);
        entity.HasOne<PartnerAccount>().WithMany().HasForeignKey(x => x.SetPartnerAccountId).OnDelete(DeleteBehavior.NoAction);
        entity.HasOne<ChartAccount>().WithMany().HasForeignKey(x => x.SetAccountCode).OnDelete(DeleteBehavior.NoAction);
    }
}

public sealed class PostingSchemeConfiguration : IEntityTypeConfiguration<PostingScheme>
{
    public void Configure(EntityTypeBuilder<PostingScheme> entity)
    {
        entity.ToTable("PostingScheme", "finance");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).UseIdentityColumn();
        entity.Property(x => x.Name).HasMaxLength(255);
        entity.Property(x => x.SourceType).HasMaxLength(50).IsUnicode(false);
        entity.Property(x => x.DebitAccount).HasMaxLength(10).IsUnicode(false);
        entity.Property(x => x.CreditAccount).HasMaxLength(10).IsUnicode(false);
        entity.Property(x => x.DescriptionTemplate).HasMaxLength(255);
        entity.HasIndex(x => new { x.CompanyId, x.SourceType, x.IsActive }).IsUnique();
        entity.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.NoAction);
        entity.HasOne<ChartAccount>().WithMany().HasForeignKey(x => x.DebitAccount).OnDelete(DeleteBehavior.NoAction);
        entity.HasOne<ChartAccount>().WithMany().HasForeignKey(x => x.CreditAccount).OnDelete(DeleteBehavior.NoAction);
    }
}

public sealed class LedgerSourcePostingConfiguration : IEntityTypeConfiguration<LedgerSourcePosting>
{
    public void Configure(EntityTypeBuilder<LedgerSourcePosting> entity)
    {
        entity.ToTable("LedgerSourcePosting", "finance");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).UseIdentityColumn();
        entity.Property(x => x.SourceType).HasMaxLength(50).IsUnicode(false);
        entity.Property(x => x.IdempotencyKey).HasMaxLength(128).IsUnicode(false);
        entity.HasIndex(x => new { x.CompanyId, x.SourceType, x.SourceId }).IsUnique();
        entity.HasIndex(x => new { x.CompanyId, x.IdempotencyKey }).IsUnique();
        entity.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.NoAction);
        entity.HasOne(x => x.JournalEntry).WithMany()
            .HasForeignKey(x => new { x.JournalEntryId, x.CompanyId })
            .HasPrincipalKey(x => new { x.Id, x.CompanyId })
            .OnDelete(DeleteBehavior.NoAction);
    }
}

public sealed class JournalEntryLedgerGuardConfiguration : IEntityTypeConfiguration<JournalEntry>
{
    public void Configure(EntityTypeBuilder<JournalEntry> entity)
    {
        entity.ToTable("JournalEntry", "finance", table => table.HasTrigger("TR_JournalEntry_PostingGuard"));
        entity.HasIndex(x => x.ReversalOfId).IsUnique().HasFilter("[ReversalOfId] IS NOT NULL");

    }
}

public sealed class LedgerEntryBankingExtensionConfiguration : IEntityTypeConfiguration<LedgerEntry>
{
    public void Configure(EntityTypeBuilder<LedgerEntry> entity)
    {
        entity.ToTable("LedgerEntry", "finance", table => table.HasTrigger("TR_LedgerEntry_PostedGuard"));
        entity.Property<int?>("PartnerAccountId");
        entity.Property<int?>("BankStatementLineId");
        entity.Property<string?>("SubAccountId").HasMaxLength(10).IsUnicode(false);
        entity.HasIndex("BankStatementLineId");
        entity.HasOne<BankStatementLine>().WithMany().HasForeignKey("BankStatementLineId").OnDelete(DeleteBehavior.NoAction);
        entity.HasOne<SubAccount>().WithMany().HasForeignKey("SubAccountId").OnDelete(DeleteBehavior.NoAction);
        entity.HasOne<PartnerAccount>().WithMany().HasForeignKey("PartnerAccountId").OnDelete(DeleteBehavior.NoAction);
    }
}

public sealed class PartnerAccountFinanceConfiguration : IEntityTypeConfiguration<PartnerAccount>
{
    public void Configure(EntityTypeBuilder<PartnerAccount> entity)
    {
        entity.HasOne<ChartAccount>().WithMany().HasForeignKey(x => x.Account).OnDelete(DeleteBehavior.NoAction);
    }
}
