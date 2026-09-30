using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SzApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class SupplierVirmanLinkAndPaymentOrderType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SupplierInvoiceId",
                schema: "billing",
                table: "PaymentOrder",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOrder_SupplierInvoiceId",
                schema: "billing",
                table: "PaymentOrder",
                column: "SupplierInvoiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentOrder_SupplierInvoice_SupplierInvoiceId",
                schema: "billing",
                table: "PaymentOrder",
                column: "SupplierInvoiceId",
                principalSchema: "billing",
                principalTable: "SupplierInvoice",
                principalColumn: "Id");

            // PaymentOrder.PaymentOrderTypeId is a required FK to ShortList(PaymentOrderType); seed the
            // one type supplier virmans use (legacy has no type table). Leave ETL/hand-edited rows alone.
            migrationBuilder.Sql("""
                INSERT INTO [core].[ShortList] ([TableName], [Caption], [IndexValue], [IndexSort])
                SELECT N'PaymentOrderType', N'Nalog za prenos', 1, 1
                WHERE NOT EXISTS (SELECT 1 FROM [core].[ShortList] s
                                  WHERE s.[TableName] = N'PaymentOrderType' AND s.[IndexValue] = 1);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentOrder_SupplierInvoice_SupplierInvoiceId",
                schema: "billing",
                table: "PaymentOrder");

            migrationBuilder.DropIndex(
                name: "IX_PaymentOrder_SupplierInvoiceId",
                schema: "billing",
                table: "PaymentOrder");

            migrationBuilder.DropColumn(
                name: "SupplierInvoiceId",
                schema: "billing",
                table: "PaymentOrder");
        }
    }
}
