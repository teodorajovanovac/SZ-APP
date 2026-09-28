using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SzApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class Fin09InvoiceBatchUniqueIndexNoFilter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InvoiceBatch_CompanyId_PeriodYYMM_ExtraordinaryInvoiceMarker",
                schema: "billing",
                table: "InvoiceBatch");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceBatch_CompanyId_PeriodYYMM_ExtraordinaryInvoiceMarker",
                schema: "billing",
                table: "InvoiceBatch",
                columns: new[] { "CompanyId", "PeriodYYMM", "ExtraordinaryInvoiceMarker" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InvoiceBatch_CompanyId_PeriodYYMM_ExtraordinaryInvoiceMarker",
                schema: "billing",
                table: "InvoiceBatch");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceBatch_CompanyId_PeriodYYMM_ExtraordinaryInvoiceMarker",
                schema: "billing",
                table: "InvoiceBatch",
                columns: new[] { "CompanyId", "PeriodYYMM", "ExtraordinaryInvoiceMarker" },
                unique: true,
                filter: "[ExtraordinaryInvoiceMarker] IS NOT NULL");
        }
    }
}
