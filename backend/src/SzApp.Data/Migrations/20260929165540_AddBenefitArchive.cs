using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SzApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBenefitArchive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BenefitArchive",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    PeriodYYMM = table.Column<int>(type: "int", nullable: false),
                    InvoiceId = table.Column<int>(type: "int", nullable: false),
                    InvoiceLineId = table.Column<int>(type: "int", nullable: false),
                    OriginalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    EntryDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BenefitArchive", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BenefitArchive_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "core",
                        principalTable: "Company",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BenefitArchive_InvoiceLine_InvoiceLineId",
                        column: x => x.InvoiceLineId,
                        principalSchema: "finance",
                        principalTable: "InvoiceLine",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BenefitArchive_Invoice_InvoiceId",
                        column: x => x.InvoiceId,
                        principalSchema: "finance",
                        principalTable: "Invoice",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BenefitArchive_Partner_CustomerId",
                        column: x => x.CustomerId,
                        principalSchema: "core",
                        principalTable: "Partner",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BenefitArchive_CompanyId_CustomerId_PeriodYYMM",
                schema: "billing",
                table: "BenefitArchive",
                columns: new[] { "CompanyId", "CustomerId", "PeriodYYMM" });

            migrationBuilder.CreateIndex(
                name: "IX_BenefitArchive_CustomerId",
                schema: "billing",
                table: "BenefitArchive",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_BenefitArchive_InvoiceId",
                schema: "billing",
                table: "BenefitArchive",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_BenefitArchive_InvoiceLineId",
                schema: "billing",
                table: "BenefitArchive",
                column: "InvoiceLineId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BenefitArchive",
                schema: "billing");
        }
    }
}
