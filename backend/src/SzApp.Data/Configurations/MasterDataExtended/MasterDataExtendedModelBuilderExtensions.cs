using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SzApp.Data.Entities;

namespace SzApp.Data.Configurations.MasterDataExtended;

public static class MasterDataExtendedModelBuilderExtensions
{
    public static ModelBuilder ApplyMasterDataExtendedModel(this ModelBuilder builder)
    {
        builder.ApplyConfiguration(new BuildingEntranceConfiguration());
        builder.ApplyConfiguration(new UnitConfiguration());
        builder.ApplyConfiguration(new ContractConfiguration());
        builder.ApplyConfiguration(new PartnerAccountConfiguration());
        builder.ApplyConfiguration(new BankAccountConfiguration());
        return builder;
    }
}

public sealed class BuildingEntranceConfiguration : IEntityTypeConfiguration<BuildingEntrance>
{
    public void Configure(EntityTypeBuilder<BuildingEntrance> entity)
    {
        entity.ToTable("BuildingEntrance", "core");
        entity.HasKey(item => item.Id);
        entity.Property(item => item.Id).UseIdentityColumn();
        entity.Property(item => item.BuildingName).HasMaxLength(255);
        entity.Property(item => item.EntranceName).HasMaxLength(255);
        entity.Property(item => item.BuildingLabel).HasMaxLength(255);
        entity.Property(item => item.Description).HasMaxLength(255);
        entity.Property(item => item.RowVersion).IsRowVersion();
        entity.HasIndex(item => new { item.CompanyId, item.SortIndex });
        entity.HasOne(item => item.Company).WithMany().HasForeignKey(item => item.CompanyId);
        entity.HasOne(item => item.Address).WithMany().HasForeignKey(item => item.AddressId);
    }
}

public sealed class UnitConfiguration : IEntityTypeConfiguration<Unit>
{
    public void Configure(EntityTypeBuilder<Unit> entity)
    {
        entity.ToTable("Unit", "core");
        entity.HasKey(item => item.Id);
        entity.Property(item => item.Id).UseIdentityColumn();
        entity.Property(item => item.Name).HasMaxLength(255);
        entity.Property(item => item.Note).HasMaxLength(255);
        entity.Property(item => item.K1).HasPrecision(9, 4);
        entity.Property(item => item.K2).HasPrecision(9, 4);
        entity.Property(item => item.K3).HasPrecision(9, 4);
        entity.Property(item => item.K4).HasPrecision(9, 4);
        entity.Property(item => item.K5).HasPrecision(9, 4);
        entity.Property(item => item.RowVersion).IsRowVersion();
        entity.HasIndex(item => new { item.CompanyId, item.SortingNumber });
        entity.HasIndex(item => item.BuildingEntranceId);
        entity.HasOne(item => item.Company).WithMany().HasForeignKey(item => item.CompanyId);
        entity.HasOne(item => item.Contract).WithMany().HasForeignKey(item => item.ContractId);
        entity.HasOne(item => item.UnitType).WithMany().HasForeignKey(item => item.UnitTypeId);
        entity.HasOne(item => item.BuildingEntrance).WithMany().HasForeignKey(item => item.BuildingEntranceId);
    }
}

public sealed class ContractConfiguration : IEntityTypeConfiguration<Contract>
{
    public void Configure(EntityTypeBuilder<Contract> entity)
    {
        entity.ToTable("Contract", "core", table =>
        {
            table.HasCheckConstraint("CK_Contract_Period", "[ContractEndDate] IS NULL OR [ContractEndDate] >= [ContractDate]");
            table.HasCheckConstraint("CK_Contract_InvoicePeriod", "[InvoiceEndDate] IS NULL OR ([InvoiceStartDate] IS NOT NULL AND [InvoiceEndDate] >= [InvoiceStartDate])");
        });
        entity.HasKey(item => item.Id);
        entity.Property(item => item.Id).UseIdentityColumn();
        entity.Property(item => item.Note).HasMaxLength(4000);
        entity.Property(item => item.InvoiceDeliveryLocation).HasMaxLength(50);
        entity.Property(item => item.ExportExternalAccount).HasMaxLength(255);
        entity.Property(item => item.RowVersion).IsRowVersion();
        entity.HasIndex(item => new { item.CompanyId, item.UnitId, item.ContractDate });
        entity.HasIndex(item => new { item.CompanyId, item.UnitId, item.IsActive });
        entity.HasOne(item => item.Company).WithMany().HasForeignKey(item => item.CompanyId);
        entity.HasOne(item => item.Unit).WithMany(item => item.ContractHistory).HasForeignKey(item => item.UnitId);
        entity.HasOne(item => item.OwnerPartner).WithMany().HasForeignKey(item => item.OwnerPartnerId);
        entity.HasOne(item => item.InvoicePartner).WithMany().HasForeignKey(item => item.InvoicePartnerId);
        entity.HasOne(item => item.TenantPartner).WithMany().HasForeignKey(item => item.TenantPartnerId);
        entity.HasOne(item => item.InvoiceDeliveryUnit).WithMany().HasForeignKey(item => item.InvoiceDeliveryUnitId);
    }
}

public sealed class PartnerAccountConfiguration : IEntityTypeConfiguration<PartnerAccount>
{
    public void Configure(EntityTypeBuilder<PartnerAccount> entity)
    {
        entity.ToTable("PartnerAccount", "core");
        entity.HasKey(item => item.Id);
        entity.Property(item => item.Id).UseIdentityColumn();
        entity.Property(item => item.Account).HasMaxLength(10).IsUnicode(false);
        entity.Property(item => item.RowVersion).IsRowVersion();
        entity.HasIndex(item => new { item.CompanyId, item.AccountNumber }).IsUnique();
        entity.HasIndex(item => new { item.CompanyId, item.PartnerId });
        entity.HasOne(item => item.Company).WithMany().HasForeignKey(item => item.CompanyId);
        entity.HasOne(item => item.Partner).WithMany().HasForeignKey(item => item.PartnerId);
        entity.HasOne(item => item.Contract).WithMany().HasForeignKey(item => item.ContractId);
    }
}

public sealed class BankAccountConfiguration : IEntityTypeConfiguration<BankAccount>
{
    public void Configure(EntityTypeBuilder<BankAccount> entity)
    {
        entity.ToTable("BankAccount", "core");
        entity.HasKey(item => item.Id);
        entity.Property(item => item.Id).UseIdentityColumn();
        entity.Property(item => item.AccountNumber).HasMaxLength(50).IsUnicode(false);
        entity.Property(item => item.Currency).HasMaxLength(3).IsUnicode(false);
        entity.Property(item => item.RowVersion).IsRowVersion();
        entity.HasIndex(item => new { item.CompanyId, item.IsActive, item.SortIndex });
        entity.HasIndex(item => item.PartnerId);
        entity.HasOne(item => item.Company).WithMany().HasForeignKey(item => item.CompanyId);
        entity.HasOne(item => item.Partner).WithMany().HasForeignKey(item => item.PartnerId);
    }
}
