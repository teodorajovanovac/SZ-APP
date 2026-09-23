using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SzApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class PostMergeFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BankStatement_CompanyId_BankAccountId_StatementNumber_StatementSuffix_Date",
                schema: "finance",
                table: "BankStatement");

            migrationBuilder.AlterColumn<bool>(
                name: "IsMatch",
                schema: "etl",
                table: "ReconciliationRecord",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.CreateIndex(
                name: "IX_BankStatement_CompanyId_BankAccountId_StatementNumber_StatementSuffix_Date",
                schema: "finance",
                table: "BankStatement",
                columns: new[] { "CompanyId", "BankAccountId", "StatementNumber", "StatementSuffix", "Date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BankStatement_CompanyId_BankAccountId_StatementNumber_StatementSuffix_Date",
                schema: "finance",
                table: "BankStatement");

            migrationBuilder.AlterColumn<bool>(
                name: "IsMatch",
                schema: "etl",
                table: "ReconciliationRecord",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BankStatement_CompanyId_BankAccountId_StatementNumber_StatementSuffix_Date",
                schema: "finance",
                table: "BankStatement",
                columns: new[] { "CompanyId", "BankAccountId", "StatementNumber", "StatementSuffix", "Date" },
                unique: true,
                filter: "[StatementSuffix] IS NOT NULL");
        }
    }
}
