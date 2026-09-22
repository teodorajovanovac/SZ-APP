using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SzApp.Data.Entities;
using SzApp.Data.Entities.EtlExtended;

namespace SzApp.Data.Configurations.EtlExtended;

public static class EtlExtendedModelBuilderExtensions
{
    public static ModelBuilder ApplyEtlExtendedModel(this ModelBuilder builder)
    {
        new EtlRunContextConfiguration().Configure(builder.Entity<EtlRunContext>());
        new RawStagingRowConfiguration().Configure(builder.Entity<RawStagingRow>());
        new PendingLegacyRelationshipConfiguration().Configure(builder.Entity<PendingLegacyRelationship>());
        new ReconciliationRecordConfiguration().Configure(builder.Entity<ReconciliationRecord>());
        new EtlParityAssessmentConfiguration().Configure(builder.Entity<EtlParityAssessment>());
        return builder;
    }
}

public sealed class EtlRunContextConfiguration : IEntityTypeConfiguration<EtlRunContext>
{
    public void Configure(EntityTypeBuilder<EtlRunContext> entity)
    {
        entity.ToTable("EtlRunContext", "etl");
        entity.HasKey(x => x.EtlRunId);
        entity.Property(x => x.SourceTable).HasMaxLength(128).IsUnicode(false);
        entity.Property(x => x.StoredRelativePath).HasMaxLength(500);
        entity.Property(x => x.EncodingName).HasMaxLength(50).IsUnicode(false);
        entity.Property(x => x.Delimiter).HasMaxLength(1);
        entity.Property(x => x.Stage).HasConversion<int>();
        entity.Property(x => x.PipelineVersion).HasMaxLength(32).IsUnicode(false);
        entity.Property(x => x.RowVersion).IsRowVersion();
        entity.HasIndex(x => new { x.CompanyId, x.SourceTable, x.UpdatedAt });
        entity.HasOne(x => x.EtlRun).WithOne().HasForeignKey<EtlRunContext>(x => x.EtlRunId).OnDelete(DeleteBehavior.NoAction);
        entity.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.NoAction);
    }
}

public sealed class RawStagingRowConfiguration : IEntityTypeConfiguration<RawStagingRow>
{
    public void Configure(EntityTypeBuilder<RawStagingRow> entity)
    {
        entity.ToTable("RawStagingRow", "etl");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).UseIdentityColumn();
        entity.Property(x => x.SourceSystem).HasMaxLength(100).IsUnicode(false);
        entity.Property(x => x.SourceTable).HasMaxLength(128).IsUnicode(false);
        entity.Property(x => x.SourceKey).HasMaxLength(255);
        entity.Property(x => x.RowHash).HasMaxLength(64).IsUnicode(false);
        entity.HasIndex(x => new { x.EtlRunId, x.SourceRowNumber }).IsUnique();
        entity.HasIndex(x => new { x.SourceTable, x.SourceKey });
        entity.HasIndex(x => x.RowHash);
        entity.HasOne(x => x.EtlRun).WithMany().HasForeignKey(x => x.EtlRunId).OnDelete(DeleteBehavior.NoAction);
    }
}

public sealed class PendingLegacyRelationshipConfiguration : IEntityTypeConfiguration<PendingLegacyRelationship>
{
    public void Configure(EntityTypeBuilder<PendingLegacyRelationship> entity)
    {
        entity.ToTable("PendingLegacyRelationship", "etl");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).UseIdentityColumn();
        entity.Property(x => x.SourceTable).HasMaxLength(128).IsUnicode(false);
        entity.Property(x => x.SourceKey).HasMaxLength(255);
        entity.Property(x => x.RelationshipName).HasMaxLength(128).IsUnicode(false);
        entity.Property(x => x.TargetSourceTable).HasMaxLength(128).IsUnicode(false);
        entity.Property(x => x.TargetSourceKey).HasMaxLength(255);
        entity.Property(x => x.ResolutionError).HasMaxLength(1000);
        entity.HasIndex(x => new { x.EtlRunId, x.SourceTable, x.SourceKey, x.RelationshipName }).IsUnique();
        entity.HasIndex(x => new { x.EtlRunId, x.IsResolved });
        entity.HasOne(x => x.EtlRun).WithMany().HasForeignKey(x => x.EtlRunId).OnDelete(DeleteBehavior.NoAction);
    }
}

public sealed class ReconciliationRecordConfiguration : IEntityTypeConfiguration<ReconciliationRecord>
{
    public void Configure(EntityTypeBuilder<ReconciliationRecord> entity)
    {
        entity.ToTable("ReconciliationRecord", "etl");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).UseIdentityColumn();
        entity.Property(x => x.Metric).HasConversion<int>();
        entity.Property(x => x.Scope).HasMaxLength(128).IsUnicode(false);
        entity.Property(x => x.ExpectedValue).HasPrecision(28, 4);
        entity.Property(x => x.ActualValue).HasPrecision(28, 4);
        entity.Property(x => x.Details).HasMaxLength(2000);
        entity.HasIndex(x => new { x.EtlRunId, x.Metric, x.Scope }).IsUnique();
        entity.HasOne(x => x.EtlRun).WithMany().HasForeignKey(x => x.EtlRunId).OnDelete(DeleteBehavior.NoAction);
    }
}

public sealed class EtlParityAssessmentConfiguration : IEntityTypeConfiguration<EtlParityAssessment>
{
    public void Configure(EntityTypeBuilder<EtlParityAssessment> entity)
    {
        entity.ToTable("EtlParityAssessment", "etl");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).UseIdentityColumn();
        entity.Property(x => x.ArtifactKind).HasMaxLength(50).IsUnicode(false);
        entity.Property(x => x.Status).HasConversion<int>();
        entity.Property(x => x.Reason).HasMaxLength(1000);
        entity.HasIndex(x => new { x.EtlRunId, x.ArtifactKind }).IsUnique();
        entity.HasOne(x => x.EtlRun).WithMany().HasForeignKey(x => x.EtlRunId).OnDelete(DeleteBehavior.NoAction);
    }
}
