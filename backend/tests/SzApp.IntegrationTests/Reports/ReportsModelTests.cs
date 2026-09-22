using Microsoft.EntityFrameworkCore;
using SzApp.Data.Configurations.Reports;
using SzApp.Data.Entities;
using SzApp.Data.Entities.Reports;

namespace SzApp.IntegrationTests.Reports;

public sealed class ReportsModelTests
{
    [Fact]
    public void ModelBuilderExtension_ConfiguresTenantDefinitionsAndExecutionAudit()
    {
        var options = new DbContextOptionsBuilder<ReportsModelContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=SzAppReportsModelOnly;Trusted_Connection=True")
            .Options;
        using var context = new ReportsModelContext(options);

        var analysis = context.Model.FindEntityType(typeof(AnalysisReportDefinition))!;
        Assert.Contains(analysis.GetIndexes(), x => x.IsUnique &&
            x.Properties.Select(y => y.Name).SequenceEqual([nameof(AnalysisReportDefinition.CompanyId), nameof(AnalysisReportDefinition.Name)]));
        var audit = context.Model.FindEntityType(typeof(ReportExecutionAudit))!;
        Assert.Equal(64, audit.FindProperty(nameof(ReportExecutionAudit.ParameterHash))!.GetMaxLength());
        Assert.NotNull(audit.FindProperty(nameof(ReportExecutionAudit.RowCount)));
    }

    private sealed class ReportsModelContext(DbContextOptions<ReportsModelContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApplicationUser>().Ignore(x => x.CompanyAccess);
            modelBuilder.Entity<Company>().Ignore(x => x.StaffAccess);
            modelBuilder.Entity<Company>().HasOne(x => x.Partner).WithMany().HasForeignKey(x => x.PartnerId).OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Company>().HasOne(x => x.Manager).WithMany().HasForeignKey(x => x.ManagerId).OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Partner>().HasOne(x => x.OwningCompany).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.NoAction);
            modelBuilder.ApplyReportsModel();
        }
    }
}
