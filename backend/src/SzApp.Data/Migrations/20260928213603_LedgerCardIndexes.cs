using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SzApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class LedgerCardIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntry_CompanyId_Account_PostingDate",
                schema: "finance",
                table: "LedgerEntry",
                columns: new[] { "CompanyId", "Account", "PostingDate" })
                .Annotation("SqlServer:Include", new[] { "DebitAmount", "CreditAmount" });

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntry_CompanyId_PartnerAccountId_PostingDate",
                schema: "finance",
                table: "LedgerEntry",
                columns: new[] { "CompanyId", "PartnerAccountId", "PostingDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LedgerEntry_CompanyId_Account_PostingDate",
                schema: "finance",
                table: "LedgerEntry");

            migrationBuilder.DropIndex(
                name: "IX_LedgerEntry_CompanyId_PartnerAccountId_PostingDate",
                schema: "finance",
                table: "LedgerEntry");
        }
    }
}
