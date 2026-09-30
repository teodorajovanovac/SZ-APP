using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SzApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class MergeAgentBranches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RowHash",
                schema: "etl",
                table: "LegacyKeyMap",
                type: "varchar(64)",
                unicode: false,
                maxLength: 64,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowHash",
                schema: "etl",
                table: "LegacyKeyMap");
        }
    }
}
