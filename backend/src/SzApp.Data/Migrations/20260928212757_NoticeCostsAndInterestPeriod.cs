using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SzApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class NoticeCostsAndInterestPeriod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AditionalCostsLowerAmount",
                schema: "billing",
                table: "NoticeBatch",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AditionalCostsLowerLimit",
                schema: "billing",
                table: "NoticeBatch",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AditionalCostsUpperAmount",
                schema: "billing",
                table: "NoticeBatch",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "InterestPeriodEnd",
                schema: "billing",
                table: "InvoiceBatch",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "InterestPeriodStart",
                schema: "billing",
                table: "InvoiceBatch",
                type: "date",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Interest",
                schema: "billing",
                table: "InterestStatement",
                type: "decimal(28,10)",
                precision: 28,
                scale: 10,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.CreateTable(
                name: "NoticeAditionalCosts",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DateStart = table.Column<DateOnly>(type: "date", nullable: false),
                    DateEnd = table.Column<DateOnly>(type: "date", nullable: true),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    AditionalCostsLowerAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AditionalCostsLowerLimit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AditionalCostsUpperAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoticeAditionalCosts", x => x.Id);
                    table.CheckConstraint("CK_NoticeAditionalCosts_Amounts", "[AditionalCostsLowerAmount] >= 0 AND [AditionalCostsLowerLimit] >= 0 AND [AditionalCostsUpperAmount] >= 0");
                    table.CheckConstraint("CK_NoticeAditionalCosts_Period", "[DateEnd] IS NULL OR [DateEnd] >= [DateStart]");
                    table.ForeignKey(
                        name: "FK_NoticeAditionalCosts_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "core",
                        principalTable: "Company",
                        principalColumn: "Id");
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_InvoiceBatch_InterestPeriod",
                schema: "billing",
                table: "InvoiceBatch",
                sql: "([InterestPeriodStart] IS NULL AND [InterestPeriodEnd] IS NULL) OR [InterestPeriodStart] <= [InterestPeriodEnd]");

            migrationBuilder.CreateIndex(
                name: "IX_NoticeAditionalCosts_CompanyId_DateStart",
                schema: "billing",
                table: "NoticeAditionalCosts",
                columns: new[] { "CompanyId", "DateStart" });

            // P11: NoticeType codes (SzApp.Domain.Billing.NoticeTypes), per data-model "Samostalna opomena,
            // Notifikacija na računu". IndexValue 0 = notification (never carries a cost). Insert only what's missing.
            migrationBuilder.Sql("""
                INSERT INTO [core].[ShortList] ([TableName], [Caption], [IndexValue], [IndexSort])
                SELECT N'NoticeType', v.Caption, v.Code, v.Code
                FROM (VALUES (0, N'Obaveštenje (tekst na računu)'), (1, N'Opomena')) v(Code, Caption)
                WHERE NOT EXISTS (SELECT 1 FROM [core].[ShortList] s
                                  WHERE s.[TableName] = N'NoticeType' AND s.[IndexValue] = v.Code);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NoticeAditionalCosts",
                schema: "billing");

            migrationBuilder.DropCheckConstraint(
                name: "CK_InvoiceBatch_InterestPeriod",
                schema: "billing",
                table: "InvoiceBatch");

            migrationBuilder.DropColumn(
                name: "AditionalCostsLowerAmount",
                schema: "billing",
                table: "NoticeBatch");

            migrationBuilder.DropColumn(
                name: "AditionalCostsLowerLimit",
                schema: "billing",
                table: "NoticeBatch");

            migrationBuilder.DropColumn(
                name: "AditionalCostsUpperAmount",
                schema: "billing",
                table: "NoticeBatch");

            migrationBuilder.DropColumn(
                name: "InterestPeriodEnd",
                schema: "billing",
                table: "InvoiceBatch");

            migrationBuilder.DropColumn(
                name: "InterestPeriodStart",
                schema: "billing",
                table: "InvoiceBatch");

            migrationBuilder.AlterColumn<decimal>(
                name: "Interest",
                schema: "billing",
                table: "InterestStatement",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,10)",
                oldPrecision: 28,
                oldScale: 10);
        }
    }
}
