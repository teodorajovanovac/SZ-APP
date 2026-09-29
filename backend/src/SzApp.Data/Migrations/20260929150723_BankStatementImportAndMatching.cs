using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SzApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class BankStatementImportAndMatching : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SetAccountCode",
                schema: "finance",
                table: "BankStatementPostingTemplate",
                type: "varchar(10)",
                unicode: false,
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldUnicode: false,
                oldMaxLength: 10);

            migrationBuilder.AddColumn<bool>(
                name: "IsConfidentMatch",
                schema: "finance",
                table: "BankStatementLine",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MatchNote",
                schema: "finance",
                table: "BankStatementLine",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MatchSource",
                schema: "finance",
                table: "BankStatementLine",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceFileName",
                schema: "finance",
                table: "BankStatement",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BankStatementLineAllocation",
                schema: "finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    BankStatementLineId = table.Column<int>(type: "int", nullable: false),
                    SortIndex = table.Column<int>(type: "int", nullable: false),
                    Account = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    PartnerAccountId = table.Column<int>(type: "int", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Kind = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    SubAccountId = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    Parameters = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    DocumentRef = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    InvoiceId = table.Column<int>(type: "int", nullable: true),
                    SupplierInvoiceId = table.Column<int>(type: "int", nullable: true),
                    CollectionPriority = table.Column<int>(type: "int", nullable: true),
                    ClosesDocumentType = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankStatementLineAllocation", x => x.Id);
                    table.CheckConstraint("CK_BankStatementLineAllocation_Amount", "[Amount] <> 0");
                    table.ForeignKey(
                        name: "FK_BankStatementLineAllocation_BankStatementLine_BankStatementLineId_CompanyId",
                        columns: x => new { x.BankStatementLineId, x.CompanyId },
                        principalSchema: "finance",
                        principalTable: "BankStatementLine",
                        principalColumns: new[] { "Id", "CompanyId" });
                    table.ForeignKey(
                        name: "FK_BankStatementLineAllocation_ChartOfAccounts_Account",
                        column: x => x.Account,
                        principalSchema: "finance",
                        principalTable: "ChartOfAccounts",
                        principalColumn: "Account");
                    table.ForeignKey(
                        name: "FK_BankStatementLineAllocation_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "core",
                        principalTable: "Company",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BankStatementLineAllocation_PartnerAccount_PartnerAccountId",
                        column: x => x.PartnerAccountId,
                        principalSchema: "core",
                        principalTable: "PartnerAccount",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BankStatementLineAllocation_SubAccount_SubAccountId",
                        column: x => x.SubAccountId,
                        principalSchema: "finance",
                        principalTable: "SubAccount",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BankStatementLineAllocation_Account",
                schema: "finance",
                table: "BankStatementLineAllocation",
                column: "Account");

            migrationBuilder.CreateIndex(
                name: "IX_BankStatementLineAllocation_BankStatementLineId_CompanyId",
                schema: "finance",
                table: "BankStatementLineAllocation",
                columns: new[] { "BankStatementLineId", "CompanyId" });

            migrationBuilder.CreateIndex(
                name: "IX_BankStatementLineAllocation_BankStatementLineId_SortIndex",
                schema: "finance",
                table: "BankStatementLineAllocation",
                columns: new[] { "BankStatementLineId", "SortIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_BankStatementLineAllocation_CompanyId",
                schema: "finance",
                table: "BankStatementLineAllocation",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_BankStatementLineAllocation_PartnerAccountId",
                schema: "finance",
                table: "BankStatementLineAllocation",
                column: "PartnerAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_BankStatementLineAllocation_SubAccountId",
                schema: "finance",
                table: "BankStatementLineAllocation",
                column: "SubAccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BankStatementLineAllocation",
                schema: "finance");

            migrationBuilder.DropColumn(
                name: "IsConfidentMatch",
                schema: "finance",
                table: "BankStatementLine");

            migrationBuilder.DropColumn(
                name: "MatchNote",
                schema: "finance",
                table: "BankStatementLine");

            migrationBuilder.DropColumn(
                name: "MatchSource",
                schema: "finance",
                table: "BankStatementLine");

            migrationBuilder.DropColumn(
                name: "SourceFileName",
                schema: "finance",
                table: "BankStatement");

            migrationBuilder.AlterColumn<string>(
                name: "SetAccountCode",
                schema: "finance",
                table: "BankStatementPostingTemplate",
                type: "varchar(10)",
                unicode: false,
                maxLength: 10,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldUnicode: false,
                oldMaxLength: 10,
                oldNullable: true);
        }
    }
}
