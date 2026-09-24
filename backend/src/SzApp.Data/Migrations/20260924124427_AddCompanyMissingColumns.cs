using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SzApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyMissingColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalAccount",
                schema: "core",
                table: "Company",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LocationCategoryId",
                schema: "core",
                table: "Company",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                schema: "core",
                table: "Company",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SortIndex",
                schema: "core",
                table: "Company",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Company_LocationCategoryId",
                schema: "core",
                table: "Company",
                column: "LocationCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Company_LocationCategory_LocationCategoryId",
                schema: "core",
                table: "Company",
                column: "LocationCategoryId",
                principalSchema: "core",
                principalTable: "LocationCategory",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Company_LocationCategory_LocationCategoryId",
                schema: "core",
                table: "Company");

            migrationBuilder.DropIndex(
                name: "IX_Company_LocationCategoryId",
                schema: "core",
                table: "Company");

            migrationBuilder.DropColumn(
                name: "ExternalAccount",
                schema: "core",
                table: "Company");

            migrationBuilder.DropColumn(
                name: "LocationCategoryId",
                schema: "core",
                table: "Company");

            migrationBuilder.DropColumn(
                name: "Note",
                schema: "core",
                table: "Company");

            migrationBuilder.DropColumn(
                name: "SortIndex",
                schema: "core",
                table: "Company");
        }
    }
}
