using Microsoft.EntityFrameworkCore;
using SzApp.Data.Entities;
using SzApp.Data.Entities.Reports;

namespace SzApp.Data.Configurations.Reports;

public static class ReportsModelBuilderExtensions
{
    public static ModelBuilder ApplyReportsModel(this ModelBuilder builder)
    {
        builder.Entity<AnalysisReportDefinition>(entity =>
        {
            entity.ToTable("AnalysisReportDefinition", "reports");
            entity.HasKey(x => x.Id); entity.Property(x => x.Id).UseIdentityColumn();
            entity.Property(x => x.DataGroup).HasMaxLength(255);
            entity.Property(x => x.Name).HasMaxLength(255);
            entity.Property(x => x.QueryName).HasMaxLength(255);
            entity.Property(x => x.ReportName).HasMaxLength(50);
            entity.Property(x => x.FilterCaption).HasMaxLength(50);
            entity.Property(x => x.LegacyActionQueryName).HasMaxLength(255);
            entity.Property(x => x.LinkCreationTag).HasMaxLength(255);
            entity.Property(x => x.LinkFormName).HasMaxLength(255);
            entity.Property(x => x.LinkOpenArgs).HasMaxLength(255);
            entity.Property(x => x.RowVersion).IsRowVersion();
            entity.HasIndex(x => new { x.CompanyId, x.IsActive, x.DataGroup, x.SortIndex });
            entity.HasIndex(x => new { x.CompanyId, x.Name }).IsUnique();
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<ReportDefinition>(entity =>
        {
            entity.ToTable("ReportDefinition", "reports");
            entity.HasKey(x => x.Id); entity.Property(x => x.Id).UseIdentityColumn();
            entity.Property(x => x.Name).HasMaxLength(255);
            entity.Property(x => x.Datasheet).HasMaxLength(255);
            entity.Property(x => x.Title).HasMaxLength(255);
            entity.Property(x => x.FilterCaption).HasMaxLength(255);
            entity.Property(x => x.RowVersion).IsRowVersion();
            entity.HasIndex(x => new { x.CompanyId, x.IsActive, x.SortIndex });
            entity.HasIndex(x => new { x.CompanyId, x.Name }).IsUnique();
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<ReportDefinitionDetail>(entity =>
        {
            entity.ToTable("ReportDefinitionDetail", "reports");
            entity.HasKey(x => x.Id); entity.Property(x => x.Id).UseIdentityColumn();
            entity.Property(x => x.QueryName).HasMaxLength(255);
            entity.Property(x => x.Function).HasMaxLength(100).IsUnicode(false);
            entity.HasIndex(x => new { x.ReportDefinitionId, x.SortIndex }).IsUnique();
            entity.HasOne(x => x.ReportDefinition).WithMany(x => x.Details).HasForeignKey(x => x.ReportDefinitionId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ReportDefinitionButton>(entity =>
        {
            entity.ToTable("ReportDefinitionButton", "reports");
            entity.HasKey(x => x.Id); entity.Property(x => x.Id).UseIdentityColumn();
            entity.Property(x => x.Caption).HasMaxLength(255);
            entity.Property(x => x.Function).HasMaxLength(100).IsUnicode(false);
            entity.HasIndex(x => new { x.ReportDefinitionId, x.SortIndex }).IsUnique();
            entity.HasOne(x => x.ReportDefinition).WithMany(x => x.Buttons).HasForeignKey(x => x.ReportDefinitionId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<ShortList>().WithMany().HasForeignKey(x => x.FunctionTypeId).OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<ReportExecutionAudit>(entity =>
        {
            entity.ToTable("ReportExecutionAudit", "reports");
            entity.HasKey(x => x.Id); entity.Property(x => x.Id).UseIdentityColumn();
            entity.Property(x => x.DefinitionType).HasMaxLength(20).IsUnicode(false);
            entity.Property(x => x.Status).HasConversion<int>();
            entity.Property(x => x.ParameterHash).HasMaxLength(64).IsUnicode(false);
            entity.Property(x => x.CorrelationId).HasMaxLength(64).IsUnicode(false);
            entity.Property(x => x.ErrorCode).HasMaxLength(100).IsUnicode(false);
            entity.HasIndex(x => new { x.CompanyId, x.StartedAt });
            entity.HasIndex(x => new { x.DefinitionType, x.DefinitionId, x.StartedAt });
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.StaffId).OnDelete(DeleteBehavior.NoAction);
        });
        return builder;
    }
}
