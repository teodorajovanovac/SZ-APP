using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SzApp.Data.Entities;

namespace SzApp.Data.Configurations.Platform;

public static class PlatformModelBuilderExtensions
{
    public static ModelBuilder ApplyPlatformModel(this ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(PlatformModelBuilderExtensions).Assembly,
            type => type.Namespace == typeof(PlatformModelBuilderExtensions).Namespace);
        return builder;
    }
}

public sealed class DocumentRecordConfiguration : IEntityTypeConfiguration<DocumentRecord>
{
    public void Configure(EntityTypeBuilder<DocumentRecord> e)
    {
        e.ToTable("Document", "platform"); e.HasKey(x => x.Id); e.Property(x => x.Id).UseIdentityColumn();
        e.Property(x => x.FileName).HasMaxLength(255); e.Property(x => x.RelativePath).HasMaxLength(500);
        e.Property(x => x.ContentType).HasMaxLength(127).IsUnicode(false); e.Property(x => x.Sha256).HasMaxLength(64).IsUnicode(false);
        e.Property(x => x.SourceTable).HasMaxLength(100).IsUnicode(false); e.Property(x => x.Description).HasMaxLength(255);
        e.Property(x => x.RowVersion).IsRowVersion(); e.HasIndex(x => new { x.CompanyId, x.CreatedAt }); e.HasIndex(x => new { x.CompanyId, x.Sha256 });
        e.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId); e.HasOne(x => x.OwnerStaff).WithMany().HasForeignKey(x => x.OwnerStaffId);
    }
}

public sealed class SentEmailConfiguration : IEntityTypeConfiguration<SentEmail>
{
    public void Configure(EntityTypeBuilder<SentEmail> e)
    {
        e.ToTable("SentEmail", "platform"); e.HasKey(x => x.Id); e.Property(x => x.Id).UseIdentityColumn();
        e.Property(x => x.Subject).HasMaxLength(255); e.Property(x => x.ToAddress).HasMaxLength(1000);
        e.Property(x => x.Cc).HasMaxLength(1000); e.Property(x => x.Bcc).HasMaxLength(1000); e.Property(x => x.Status).HasConversion<int>();
        e.Property(x => x.SendDescription).HasMaxLength(2000); e.Property(x => x.RowVersion).IsRowVersion();
        e.HasIndex(x => new { x.CompanyId, x.Status, x.NextAttemptAt });
        e.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId); e.HasOne(x => x.CreatedByStaff).WithMany().HasForeignKey(x => x.CreatedByStaffId);
    }
}

public sealed class SentEmailAttachmentConfiguration : IEntityTypeConfiguration<SentEmailAttachment>
{
    public void Configure(EntityTypeBuilder<SentEmailAttachment> e)
    {
        e.ToTable("SentEmailAttachment", "platform"); e.HasKey(x => x.Id); e.Property(x => x.Id).UseIdentityColumn();
        e.HasIndex(x => new { x.SentEmailId, x.DocumentId }).IsUnique();
        e.HasOne(x => x.SentEmail).WithMany(x => x.Attachments).HasForeignKey(x => x.SentEmailId);
        e.HasOne(x => x.Document).WithMany().HasForeignKey(x => x.DocumentId);
    }
}

public sealed class SettingConfiguration : IEntityTypeConfiguration<Setting>
{
    public void Configure(EntityTypeBuilder<Setting> e)
    {
        e.ToTable("Setting", "platform"); e.HasKey(x => x.Id); e.Property(x => x.Id).UseIdentityColumn();
        e.Property(x => x.Name).HasMaxLength(255); e.Property(x => x.Key).HasMaxLength(50).IsUnicode(false);
        e.Property(x => x.Value).HasMaxLength(255); e.Property(x => x.Description).HasMaxLength(255); e.Property(x => x.Category).HasMaxLength(50);
        e.Property(x => x.RowVersion).IsRowVersion(); e.HasIndex(x => new { x.CompanyId, x.Key }).IsUnique();
        e.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId);
    }
}

public sealed class PlatformEventConfiguration : IEntityTypeConfiguration<PlatformEvent>
{
    public void Configure(EntityTypeBuilder<PlatformEvent> e)
    {
        e.ToTable("Event", "platform"); e.HasKey(x => x.Id); e.Property(x => x.Id).UseIdentityColumn();
        e.Property(x => x.Description).HasMaxLength(255); e.Property(x => x.PreviousValue).HasMaxLength(255); e.Property(x => x.NewValue).HasMaxLength(255);
        e.Property(x => x.FieldsRelated).HasMaxLength(255); e.Property(x => x.RequestBy).HasMaxLength(255); e.Property(x => x.RequestThrough).HasMaxLength(255);
        e.Property(x => x.DecisionReason).HasMaxLength(1000); e.Property(x => x.Status).HasConversion<int>(); e.Property(x => x.RowVersion).IsRowVersion();
        e.HasIndex(x => new { x.CompanyId, x.Status, x.RequestedAt });
    }
}

public sealed class SelectionBasketConfiguration : IEntityTypeConfiguration<SelectionBasket>
{
    public void Configure(EntityTypeBuilder<SelectionBasket> e)
    {
        e.ToTable("SelectionBasket", "platform"); e.HasKey(x => x.Id);
        e.Property(x => x.TargetType).HasMaxLength(50).IsUnicode(false); e.Property(x => x.TargetId).HasMaxLength(100).IsUnicode(false);
        e.HasIndex(x => new { x.OwnerStaffId, x.CompanyId, x.ExpiresAt }); e.HasIndex(x => new { x.OwnerStaffId, x.CompanyId, x.TargetType, x.TargetId }).IsUnique();
        e.HasOne(x => x.OwnerStaff).WithMany().HasForeignKey(x => x.OwnerStaffId);
    }
}

public sealed class ImportDefinitionConfiguration : IEntityTypeConfiguration<ImportDefinition>
{
    public void Configure(EntityTypeBuilder<ImportDefinition> e)
    {
        e.ToTable("ImportDefinition", "platform"); e.HasKey(x => x.Id); e.Property(x => x.Id).UseIdentityColumn();
        e.Property(x => x.Name).HasMaxLength(255); e.Property(x => x.Code).HasMaxLength(50).IsUnicode(false); e.Property(x => x.FileMask).HasMaxLength(50);
        e.Property(x => x.FilePath).HasMaxLength(500); e.Property(x => x.TargetHeaderTable).HasMaxLength(100).IsUnicode(false); e.Property(x => x.TargetLineTable).HasMaxLength(100).IsUnicode(false);
        e.Property(x => x.RowVersion).IsRowVersion(); e.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique();
    }
}

public sealed class ImportMappingGroupConfiguration : IEntityTypeConfiguration<ImportMappingGroup>
{
    public void Configure(EntityTypeBuilder<ImportMappingGroup> e)
    {
        e.ToTable("ImportMappingGroup", "platform"); e.HasKey(x => x.Id); e.Property(x => x.Id).UseIdentityColumn();
        e.Property(x => x.Name).HasMaxLength(255); e.Property(x => x.TargetTable).HasMaxLength(100).IsUnicode(false); e.Property(x => x.SourcePath).HasMaxLength(500);
        e.HasOne(x => x.Definition).WithMany(x => x.Groups).HasForeignKey(x => x.ImportDefinitionId);
    }
}

public sealed class ImportMappingConfiguration : IEntityTypeConfiguration<ImportMapping>
{
    public void Configure(EntityTypeBuilder<ImportMapping> e)
    {
        e.ToTable("ImportMapping", "platform"); e.HasKey(x => x.Id); e.Property(x => x.Id).UseIdentityColumn();
        e.Property(x => x.TargetField).HasMaxLength(100).IsUnicode(false); e.Property(x => x.SourcePath).HasMaxLength(500); e.Property(x => x.SourceNode).HasMaxLength(255);
        e.Property(x => x.LookupTable).HasMaxLength(100).IsUnicode(false); e.Property(x => x.LookupField).HasMaxLength(100).IsUnicode(false); e.Property(x => x.LookupValueField).HasMaxLength(100).IsUnicode(false);
        e.Property(x => x.Format).HasMaxLength(50); e.Property(x => x.Description).HasMaxLength(255);
        e.HasOne(x => x.Group).WithMany(x => x.Mappings).HasForeignKey(x => x.ImportMappingGroupId);
    }
}

public sealed class LanguageConfiguration : IEntityTypeConfiguration<Language>
{
    public void Configure(EntityTypeBuilder<Language> e)
    {
        e.ToTable("Language", "platform"); e.HasKey(x => x.Code); e.Property(x => x.Code).HasMaxLength(10).IsUnicode(false); e.Property(x => x.Name).HasMaxLength(50);
        e.HasIndex(x => x.IsDefault).HasFilter("[IsDefault] = 1").IsUnique();
    }
}

public sealed class TranslationConfiguration : IEntityTypeConfiguration<Translation>
{
    public void Configure(EntityTypeBuilder<Translation> e)
    {
        e.ToTable("Translation", "platform"); e.HasKey(x => x.Id); e.Property(x => x.Id).UseIdentityColumn();
        e.Property(x => x.LanguageCode).HasMaxLength(10).IsUnicode(false); e.Property(x => x.ResourceKey).HasMaxLength(100).IsUnicode(false);
        e.HasIndex(x => new { x.LanguageCode, x.ResourceKey, x.ResourceId, x.CompanyId }).IsUnique();
        e.HasOne(x => x.Language).WithMany().HasForeignKey(x => x.LanguageCode); e.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId);
    }
}

public sealed class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
{
    public void Configure(EntityTypeBuilder<MenuItem> e)
    {
        e.ToTable("MenuItem", "core"); e.HasKey(x => x.Id); e.Property(x => x.Id).UseIdentityColumn();
        e.Property(x => x.ResourceKey).HasMaxLength(100); e.Property(x => x.IconName).HasMaxLength(100); e.Property(x => x.Path).HasMaxLength(100);
        e.Property(x => x.RequiredRoles).HasMaxLength(255);
        e.HasIndex(x => new { x.ParentId, x.SortIndex });
        e.HasOne(x => x.Parent).WithMany().HasForeignKey(x => x.ParentId);
    }
}
