using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SzApp.Data.Entities;

namespace SzApp.Data.Configurations;

public sealed class PartnerAddressConfiguration : IEntityTypeConfiguration<PartnerAddress>
{
    public void Configure(EntityTypeBuilder<PartnerAddress> e) { e.ToTable("PartnerAddress", "core"); e.HasKey(x => x.Id); e.Property(x => x.Id).UseIdentityColumn(); e.HasIndex(x => new { x.PartnerId, x.AddressTypeId, x.AddressId }).IsUnique(); e.HasOne(x => x.Partner).WithMany().HasForeignKey(x => x.PartnerId); e.HasOne(x => x.Address).WithMany().HasForeignKey(x => x.AddressId); e.HasOne(x => x.AddressType).WithMany().HasForeignKey(x => x.AddressTypeId); }
}
public sealed class PartnerCommunicationConfiguration : IEntityTypeConfiguration<PartnerCommunication>
{
    public void Configure(EntityTypeBuilder<PartnerCommunication> e) { e.ToTable("PartnerComms", "core"); e.HasKey(x => x.Id); e.Property(x => x.Id).UseIdentityColumn(); e.Property(x => x.ValueNormalized).HasMaxLength(255); e.Property(x => x.Note).HasMaxLength(255); e.HasIndex(x => new { x.PartnerId, x.ChannelId, x.ValueNormalized }).IsUnique(); e.HasOne(x => x.Partner).WithMany().HasForeignKey(x => x.PartnerId); e.HasOne(x => x.Channel).WithMany().HasForeignKey(x => x.ChannelId); }
}
public sealed class ExchangeRateConfiguration : IEntityTypeConfiguration<ExchangeRate>
{
    public void Configure(EntityTypeBuilder<ExchangeRate> e) { e.ToTable("ExchangeRate", "finance"); e.HasKey(x => x.Id); e.Property(x => x.Id).UseIdentityColumn(); e.Property(x => x.Rate).HasPrecision(19, 4); e.HasIndex(x => x.RateDateFrom).IsUnique(); }
}
public sealed class FiscalYearConfiguration : IEntityTypeConfiguration<FiscalYear>
{
    public void Configure(EntityTypeBuilder<FiscalYear> e) { e.ToTable("FiscalYear", "finance"); e.HasKey(x => x.Id); e.Property(x => x.Id).UseIdentityColumn(); e.Property(x => x.Display).HasMaxLength(255); e.Property(x => x.Folder).HasMaxLength(255); e.Property(x => x.FileName).HasMaxLength(255); e.HasIndex(x => new { x.CompanyId, x.Year }).IsUnique(); e.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId); }
}
public sealed class DocumentCategoryConfiguration : IEntityTypeConfiguration<DocumentCategory>
{
    public void Configure(EntityTypeBuilder<DocumentCategory> e) { e.ToTable("DocumentCategory", "platform"); e.HasKey(x => x.Id); e.Property(x => x.Id).UseIdentityColumn(); e.Property(x => x.GroupName).HasMaxLength(255); e.Property(x => x.Name).HasMaxLength(255); e.Property(x => x.Code).HasMaxLength(10).IsUnicode(false); e.Property(x => x.Description).HasMaxLength(255); e.HasIndex(x => x.Code).IsUnique(); }
}
public sealed class LocationCategoryConfiguration : IEntityTypeConfiguration<LocationCategory>
{
    public void Configure(EntityTypeBuilder<LocationCategory> e) { e.ToTable("LocationCategory", "core"); e.HasKey(x => x.Id); e.Property(x => x.Id).UseIdentityColumn(); e.Property(x => x.Name).HasMaxLength(255); e.HasIndex(x => new { x.ParentId, x.SortIndex }); e.HasOne(x => x.Parent).WithMany().HasForeignKey(x => x.ParentId); }
}
