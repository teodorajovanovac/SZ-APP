using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SzApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class PostingPerDocumentAndPeriodLock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_LedgerEntry_Amounts",
                schema: "finance",
                table: "LedgerEntry");

            migrationBuilder.AddColumn<int>(
                name: "ClosesDocumentType",
                schema: "finance",
                table: "LedgerEntry",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CollectionPriority",
                schema: "finance",
                table: "LedgerEntry",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InvoiceId",
                schema: "finance",
                table: "LedgerEntry",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SupplierInvoiceId",
                schema: "finance",
                table: "LedgerEntry",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PostingPeriodLock",
                schema: "finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    PeriodYYMM = table.Column<int>(type: "int", nullable: false),
                    LockedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LockedByStaffId = table.Column<int>(type: "int", nullable: false),
                    UnlockedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UnlockedByStaffId = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostingPeriodLock", x => x.Id);
                    table.CheckConstraint("CK_PostingPeriodLock_Period", "[PeriodYYMM] BETWEEN 1001 AND 9912 AND [PeriodYYMM] % 100 BETWEEN 1 AND 12");
                    table.ForeignKey(
                        name: "FK_PostingPeriodLock_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "core",
                        principalTable: "Company",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PostingPeriodLock_Staff_LockedByStaffId",
                        column: x => x.LockedByStaffId,
                        principalSchema: "auth",
                        principalTable: "Staff",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PostingPeriodLock_Staff_UnlockedByStaffId",
                        column: x => x.UnlockedByStaffId,
                        principalSchema: "auth",
                        principalTable: "Staff",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntry_InvoiceId",
                schema: "finance",
                table: "LedgerEntry",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntry_SupplierInvoiceId",
                schema: "finance",
                table: "LedgerEntry",
                column: "SupplierInvoiceId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_LedgerEntry_Amounts",
                schema: "finance",
                table: "LedgerEntry",
                sql: "NOT ([DebitAmount] <> 0 AND [CreditAmount] <> 0)");

            migrationBuilder.CreateIndex(
                name: "IX_PostingPeriodLock_CompanyId_PeriodYYMM",
                schema: "finance",
                table: "PostingPeriodLock",
                columns: new[] { "CompanyId", "PeriodYYMM" },
                unique: true,
                filter: "[UnlockedAt] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PostingPeriodLock_LockedByStaffId",
                schema: "finance",
                table: "PostingPeriodLock",
                column: "LockedByStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_PostingPeriodLock_UnlockedByStaffId",
                schema: "finance",
                table: "PostingPeriodLock",
                column: "UnlockedByStaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_LedgerEntry_Invoice_InvoiceId",
                schema: "finance",
                table: "LedgerEntry",
                column: "InvoiceId",
                principalSchema: "finance",
                principalTable: "Invoice",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LedgerEntry_SupplierInvoice_SupplierInvoiceId",
                schema: "finance",
                table: "LedgerEntry",
                column: "SupplierInvoiceId",
                principalSchema: "billing",
                principalTable: "SupplierInvoice",
                principalColumn: "Id");

            // Legacy TipStavke (9.2). LedgerEntry.LineTypeId is an FK to ShortList.Id, resolved at
            // runtime by (TableName='LedgerLineType', IndexValue=code). Insert only what's missing
            // so ETL-loaded or hand-edited rows are left alone.
            migrationBuilder.Sql("""
                INSERT INTO [core].[ShortList] ([TableName], [Caption], [IndexValue], [IndexSort])
                SELECT N'LedgerLineType', v.Caption, v.Code, v.Code
                FROM (VALUES
                    (1, N'Izvod'), (3, N'Izdat račun'), (4, N'Ulazni račun'), (7, N'Storno'),
                    (8, N'Preknjiženje'), (11, N'Provizija INO'), (91, N'Kompenzacija'),
                    (94, N'Preuzeto stanje'), (97, N'Pozajmica'), (98, N'Preuzeto dugovanje'),
                    (99, N'Početno stanje')) v(Code, Caption)
                WHERE NOT EXISTS (SELECT 1 FROM [core].[ShortList] s
                                  WHERE s.[TableName] = N'LedgerLineType' AND s.[IndexValue] = v.Code);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LedgerEntry_Invoice_InvoiceId",
                schema: "finance",
                table: "LedgerEntry");

            migrationBuilder.DropForeignKey(
                name: "FK_LedgerEntry_SupplierInvoice_SupplierInvoiceId",
                schema: "finance",
                table: "LedgerEntry");

            migrationBuilder.DropTable(
                name: "PostingPeriodLock",
                schema: "finance");

            migrationBuilder.DropIndex(
                name: "IX_LedgerEntry_InvoiceId",
                schema: "finance",
                table: "LedgerEntry");

            migrationBuilder.DropIndex(
                name: "IX_LedgerEntry_SupplierInvoiceId",
                schema: "finance",
                table: "LedgerEntry");

            migrationBuilder.DropCheckConstraint(
                name: "CK_LedgerEntry_Amounts",
                schema: "finance",
                table: "LedgerEntry");

            migrationBuilder.DropColumn(
                name: "ClosesDocumentType",
                schema: "finance",
                table: "LedgerEntry");

            migrationBuilder.DropColumn(
                name: "CollectionPriority",
                schema: "finance",
                table: "LedgerEntry");

            migrationBuilder.DropColumn(
                name: "InvoiceId",
                schema: "finance",
                table: "LedgerEntry");

            migrationBuilder.DropColumn(
                name: "SupplierInvoiceId",
                schema: "finance",
                table: "LedgerEntry");

            migrationBuilder.AddCheckConstraint(
                name: "CK_LedgerEntry_Amounts",
                schema: "finance",
                table: "LedgerEntry",
                sql: "[DebitAmount] >= 0 AND [CreditAmount] >= 0 AND NOT ([DebitAmount] > 0 AND [CreditAmount] > 0)");
        }
    }
}
