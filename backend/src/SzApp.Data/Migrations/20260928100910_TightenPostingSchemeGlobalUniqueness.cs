using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SzApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class TightenPostingSchemeGlobalUniqueness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PostingScheme_CompanyId_SourceType_IsActive",
                schema: "finance",
                table: "PostingScheme");

            migrationBuilder.CreateIndex(
                name: "IX_PostingScheme_CompanyId_SourceType_IsActive",
                schema: "finance",
                table: "PostingScheme",
                columns: new[] { "CompanyId", "SourceType", "IsActive" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PostingScheme_CompanyId_SourceType_IsActive",
                schema: "finance",
                table: "PostingScheme");

            migrationBuilder.CreateIndex(
                name: "IX_PostingScheme_CompanyId_SourceType_IsActive",
                schema: "finance",
                table: "PostingScheme",
                columns: new[] { "CompanyId", "SourceType", "IsActive" },
                unique: true,
                filter: "[CompanyId] IS NOT NULL");
        }
    }
}
