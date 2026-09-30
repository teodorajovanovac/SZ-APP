using Microsoft.EntityFrameworkCore.Migrations;
using SzApp.Data.Configurations.Reports;

#nullable disable

namespace SzApp.Data.Migrations
{
    /// <summary>
    /// GAP-18: seeds the legacy Fin_Izv / tblIzvestaj report definitions (<see cref="LegacyReportCatalog"/>) as
    /// global read-only reports. GAP-24: audit-log lookup indexes (entity history, per-user history).
    /// </summary>
    public partial class LegacyReportsAndAuditTrail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AuditLog_StaffId",
                schema: "ops",
                table: "AuditLog");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_EntityType_ItemId_Timestamp",
                schema: "ops",
                table: "AuditLog",
                columns: new[] { "EntityType", "ItemId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_StaffId_Timestamp",
                schema: "ops",
                table: "AuditLog",
                columns: new[] { "StaffId", "Timestamp" });

            foreach (var report in LegacyReportCatalog.Reports)
            {
                var sections = string.Join("\n", report.Sections.Select((s, i) => $"""
                    INSERT INTO [reports].[ReportDefinitionDetail] ([ReportDefinitionId], [QuerySql], [QueryName], [SortIndex], [Function])
                    VALUES (@id, N'{Escape(s.Sql)}', N'{Escape(s.QueryName)}', {i + 1}, 'Table');
                    """));
                migrationBuilder.Sql($"""
                    IF NOT EXISTS (SELECT 1 FROM [reports].[ReportDefinition] WHERE [CompanyId] IS NULL AND [Name] = N'{Escape(report.Name)}')
                    BEGIN
                        DECLARE @id int;
                        INSERT INTO [reports].[ReportDefinition] ([CompanyId], [Name], [Title], [SortIndex], [FilterCaption], [IsActive])
                        VALUES (NULL, N'{Escape(report.Name)}', N'{Escape(report.Title)}', {report.SortIndex}, N'{Escape(report.FilterCaption)}', 1);
                        SET @id = SCOPE_IDENTITY();
                        {sections}
                    END
                    """);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var report in LegacyReportCatalog.Reports)
            {
                migrationBuilder.Sql($"DELETE FROM [reports].[ReportDefinition] WHERE [CompanyId] IS NULL AND [Name] = N'{Escape(report.Name)}';");
            }

            migrationBuilder.DropIndex(
                name: "IX_AuditLog_EntityType_ItemId_Timestamp",
                schema: "ops",
                table: "AuditLog");

            migrationBuilder.DropIndex(
                name: "IX_AuditLog_StaffId_Timestamp",
                schema: "ops",
                table: "AuditLog");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_StaffId",
                schema: "ops",
                table: "AuditLog",
                column: "StaffId");
        }

        private static string Escape(string value) => value.Replace("'", "''");
    }
}
