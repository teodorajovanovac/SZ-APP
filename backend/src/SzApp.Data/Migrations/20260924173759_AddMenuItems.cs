using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SzApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMenuItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MenuItem",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    ResourceKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IconName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Path = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SortIndex = table.Column<int>(type: "int", nullable: false),
                    RequiredRoles = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MenuItem_MenuItem_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "core",
                        principalTable: "MenuItem",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MenuItem_ParentId_SortIndex",
                schema: "core",
                table: "MenuItem",
                columns: new[] { "ParentId", "SortIndex" });

            // Languages must exist before Translation rows (FK) -- none were seeded by earlier migrations.
            migrationBuilder.InsertData(
                schema: "platform",
                table: "Language",
                columns: new[] { "Code", "Name", "IsActive", "IsDefault", "SortIndex" },
                values: new object[,]
                {
                    { "sr-Latn", "Srpski (latinica)", true, true, 1 },
                    { "sr-Cyrl", "Српски (ћирилица)", true, false, 2 },
                    { "en", "English", true, false, 3 },
                });

            migrationBuilder.InsertData(
                schema: "core",
                table: "MenuItem",
                columns: new[] { "Id", "ParentId", "ResourceKey", "IconName", "Path", "SortIndex", "RequiredRoles" },
                values: new object[,]
                {
                    { 1, null!, "menu.dashboard", "Dashboard", "/", 0, null! },
                    { 2, null!, "menu.groupSzData", null!, null!, 1, null! },
                    { 3, 2, "menu.contracts", "Gavel", "/contracts", 0, null! },
                    { 4, null!, "menu.groupCodeLists", null!, null!, 2, null! },
                    { 5, 4, "menu.partners", "Groups", "/partners", 0, null! },
                    { 6, 4, "menu.units", "Apartment", "/units", 1, null! },
                    { 7, null!, "menu.groupBilling", null!, null!, 3, null! },
                    { 8, 7, "menu.supplierInvoices", "LocalShipping", "/suppliers", 0, null! },
                    { 9, 7, "menu.invoices", "ReceiptLong", "/billing", 1, null! },
                    { 10, 7, "menu.notices", "Notifications", "/notices", 2, null! },
                    { 11, null!, "menu.groupFinance", null!, null!, 4, null! },
                    { 12, 11, "menu.statements", "Payments", "/banking", 0, null! },
                    { 13, 11, "menu.journalEntries", "AccountBalance", "/ledger", 1, null! },
                    { 14, 11, "menu.ledgerCards", "CreditCard", "/kartice", 2, null! },
                    { 15, 11, "menu.reports", "QueryStats", "/reports", 3, null! },
                    { 16, null!, "menu.groupSystem", null!, null!, 5, null! },
                    { 17, 16, "menu.imports", "ImportExport", "/imports", 0, "Root,Upravnik" },
                    { 18, 16, "menu.staff", "Badge", "/staff", 1, "Root,Upravnik" },
                    { 19, 16, "menu.myCompanies", "Business", "/companies", 2, "Root,Upravnik" },
                    { 20, 16, "menu.administration", "Settings", "/administration", 3, "Root,Upravnik" },
                });

            migrationBuilder.InsertData(
                schema: "platform",
                table: "Translation",
                columns: new[] { "LanguageCode", "ResourceKey", "ResourceId", "CompanyId", "Value" },
                values: new object[,]
                {
                    { "sr-Latn", "menu.dashboard", null!, null!, "Početna" },
                    { "sr-Latn", "menu.groupSzData", null!, null!, "Podaci SZ" },
                    { "sr-Latn", "menu.contracts", null!, null!, "Ugovori" },
                    { "sr-Latn", "menu.groupCodeLists", null!, null!, "Šifarnici" },
                    { "sr-Latn", "menu.partners", null!, null!, "Partneri" },
                    { "sr-Latn", "menu.units", null!, null!, "Posebni delovi" },
                    { "sr-Latn", "menu.groupBilling", null!, null!, "Fakturisanje" },
                    { "sr-Latn", "menu.supplierInvoices", null!, null!, "Ulazni računi" },
                    { "sr-Latn", "menu.invoices", null!, null!, "Izlazni računi" },
                    { "sr-Latn", "menu.notices", null!, null!, "Opomene" },
                    { "sr-Latn", "menu.groupFinance", null!, null!, "Finansije" },
                    { "sr-Latn", "menu.statements", null!, null!, "Izvodi" },
                    { "sr-Latn", "menu.journalEntries", null!, null!, "Nalozi" },
                    { "sr-Latn", "menu.ledgerCards", null!, null!, "Kartice" },
                    { "sr-Latn", "menu.reports", null!, null!, "Izveštaji" },
                    { "sr-Latn", "menu.groupSystem", null!, null!, "Sistem" },
                    { "sr-Latn", "menu.imports", null!, null!, "Uvoz podataka" },
                    { "sr-Latn", "menu.staff", null!, null!, "Korisnici" },
                    { "sr-Latn", "menu.myCompanies", null!, null!, "Moje kompanije" },
                    { "sr-Latn", "menu.administration", null!, null!, "Administracija" },

                    { "sr-Cyrl", "menu.dashboard", null!, null!, "Почетна" },
                    { "sr-Cyrl", "menu.groupSzData", null!, null!, "Подаци СЗ" },
                    { "sr-Cyrl", "menu.contracts", null!, null!, "Уговори" },
                    { "sr-Cyrl", "menu.groupCodeLists", null!, null!, "Шифарници" },
                    { "sr-Cyrl", "menu.partners", null!, null!, "Партнери" },
                    { "sr-Cyrl", "menu.units", null!, null!, "Посебни делови" },
                    { "sr-Cyrl", "menu.groupBilling", null!, null!, "Фактурисање" },
                    { "sr-Cyrl", "menu.supplierInvoices", null!, null!, "Улазни рачуни" },
                    { "sr-Cyrl", "menu.invoices", null!, null!, "Излазни рачуни" },
                    { "sr-Cyrl", "menu.notices", null!, null!, "Опомене" },
                    { "sr-Cyrl", "menu.groupFinance", null!, null!, "Финансије" },
                    { "sr-Cyrl", "menu.statements", null!, null!, "Изводи" },
                    { "sr-Cyrl", "menu.journalEntries", null!, null!, "Налози" },
                    { "sr-Cyrl", "menu.ledgerCards", null!, null!, "Картице" },
                    { "sr-Cyrl", "menu.reports", null!, null!, "Извештаји" },
                    { "sr-Cyrl", "menu.groupSystem", null!, null!, "Систем" },
                    { "sr-Cyrl", "menu.imports", null!, null!, "Увоз података" },
                    { "sr-Cyrl", "menu.staff", null!, null!, "Корисници" },
                    { "sr-Cyrl", "menu.myCompanies", null!, null!, "Моје компаније" },
                    { "sr-Cyrl", "menu.administration", null!, null!, "Администрација" },

                    { "en", "menu.dashboard", null!, null!, "Home" },
                    { "en", "menu.groupSzData", null!, null!, "Association data" },
                    { "en", "menu.contracts", null!, null!, "Contracts" },
                    { "en", "menu.groupCodeLists", null!, null!, "Code lists" },
                    { "en", "menu.partners", null!, null!, "Partners" },
                    { "en", "menu.units", null!, null!, "Units" },
                    { "en", "menu.groupBilling", null!, null!, "Billing" },
                    { "en", "menu.supplierInvoices", null!, null!, "Supplier invoices" },
                    { "en", "menu.invoices", null!, null!, "Invoices" },
                    { "en", "menu.notices", null!, null!, "Notices" },
                    { "en", "menu.groupFinance", null!, null!, "Finance" },
                    { "en", "menu.statements", null!, null!, "Statements" },
                    { "en", "menu.journalEntries", null!, null!, "Journal entries" },
                    { "en", "menu.ledgerCards", null!, null!, "Ledger cards" },
                    { "en", "menu.reports", null!, null!, "Reports" },
                    { "en", "menu.groupSystem", null!, null!, "System" },
                    { "en", "menu.imports", null!, null!, "Data import" },
                    { "en", "menu.staff", null!, null!, "Staff" },
                    { "en", "menu.myCompanies", null!, null!, "My companies" },
                    { "en", "menu.administration", null!, null!, "Administration" },
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM [platform].[Translation] WHERE [ResourceKey] LIKE 'menu.%';");
            migrationBuilder.Sql("DELETE FROM [platform].[Language] WHERE [Code] IN ('sr-Latn', 'sr-Cyrl', 'en');");

            migrationBuilder.DropTable(
                name: "MenuItem",
                schema: "core");
        }
    }
}
