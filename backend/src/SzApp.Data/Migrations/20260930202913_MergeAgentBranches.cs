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

            // GAP-24 audit log page under Sistem (parent 16), same pattern as AddAdminMenuItems.
            migrationBuilder.InsertData(schema: "core", table: "MenuItem",
                columns: new[] { "ParentId", "ResourceKey", "IconName", "Path", "SortIndex", "RequiredRoles" },
                values: new object[] { 16, "menu.audit", "History", "/audit", 8, "Root,Upravnik" });
            migrationBuilder.InsertData(schema: "platform", table: "Translation",
                columns: new[] { "LanguageCode", "ResourceKey", "ResourceId", "CompanyId", "Value" },
                values: new object[,]
                {
                    { "sr-Latn", "menu.audit", null, null, "Istorija izmena" },
                    { "sr-Cyrl", "menu.audit", null, null, "Историја измена" },
                    { "en", "menu.audit", null, null, "Audit log" },
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM [platform].[Translation] WHERE [ResourceKey] = 'menu.audit'; DELETE FROM [core].[MenuItem] WHERE [ResourceKey] = 'menu.audit';");
            migrationBuilder.DropColumn(
                name: "RowHash",
                schema: "etl",
                table: "LegacyKeyMap");
        }
    }
}
