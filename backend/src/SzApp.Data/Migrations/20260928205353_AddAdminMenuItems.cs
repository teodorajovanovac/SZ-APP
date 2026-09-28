using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SzApp.Data.Migrations
{
    /// <summary>
    /// Admin menu rows: "SZ ulazi" under Šifarnici (parent 4, after Posebni delovi) and
    /// Lokacije / Podešavanja / Šifarnici (liste) / Izvoz podataka under Sistem (parent 16, after
    /// the existing four). Ids are left to IDENTITY so parallel menu migrations can't collide;
    /// Down removes by ResourceKey.
    /// </summary>
    public partial class AddAdminMenuItems : Migration
    {
        private static readonly string[] Keys =
            ["menu.buildingEntrances", "menu.locationCategories", "menu.settings", "menu.shortLists", "menu.export"];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "core",
                table: "MenuItem",
                columns: new[] { "ParentId", "ResourceKey", "IconName", "Path", "SortIndex", "RequiredRoles" },
                values: new object[,]
                {
                    { 4, "menu.buildingEntrances", "DoorFront", "/building-entrances", 2, null },
                    { 16, "menu.locationCategories", "Place", "/location-categories", 4, "Root,Upravnik" },
                    { 16, "menu.settings", "Tune", "/settings", 5, "Root,Upravnik" },
                    { 16, "menu.shortLists", "ListAlt", "/short-lists", 6, "Root,Upravnik" },
                    { 16, "menu.export", "FileDownload", "/export", 7, "Root,Upravnik" },
                });

            migrationBuilder.InsertData(
                schema: "platform",
                table: "Translation",
                columns: new[] { "LanguageCode", "ResourceKey", "ResourceId", "CompanyId", "Value" },
                values: new object[,]
                {
                    { "sr-Latn", "menu.buildingEntrances", null, null, "SZ ulazi" },
                    { "sr-Latn", "menu.locationCategories", null, null, "Lokacije" },
                    { "sr-Latn", "menu.settings", null, null, "Podešavanja" },
                    { "sr-Latn", "menu.shortLists", null, null, "Šifarnici (liste)" },
                    { "sr-Latn", "menu.export", null, null, "Izvoz podataka" },

                    { "sr-Cyrl", "menu.buildingEntrances", null, null, "СЗ улази" },
                    { "sr-Cyrl", "menu.locationCategories", null, null, "Локације" },
                    { "sr-Cyrl", "menu.settings", null, null, "Подешавања" },
                    { "sr-Cyrl", "menu.shortLists", null, null, "Шифарници (листе)" },
                    { "sr-Cyrl", "menu.export", null, null, "Извоз података" },

                    { "en", "menu.buildingEntrances", null, null, "Building entrances" },
                    { "en", "menu.locationCategories", null, null, "Locations" },
                    { "en", "menu.settings", null, null, "Settings" },
                    { "en", "menu.shortLists", null, null, "Short lists" },
                    { "en", "menu.export", null, null, "Data export" },
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var keys = string.Join(", ", Keys.Select(key => $"'{key}'"));
            migrationBuilder.Sql($"DELETE FROM [platform].[Translation] WHERE [ResourceKey] IN ({keys});");
            migrationBuilder.Sql($"DELETE FROM [core].[MenuItem] WHERE [ResourceKey] IN ({keys});");
        }
    }
}
