using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SzApp.Data.Migrations
{
    /// <summary>
    /// GAP-18: seeds two global Fin_Izvestaj_SZ report definitions (legacy Fin_Izv family) on the
    /// existing report engine. Posted ledger only, period = @DateFrom..@DateTo.
    /// </summary>
    public partial class FinancialReportSeed : Migration
    {
        private const string IncomeExpenseName = "Fin_Izvestaj_SZ - prihodi i rashodi";
        private const string CollectionName = "Fin_Izvestaj_SZ - naplata";

        public const string IncomeExpenseSql =
            "SELECT le.Account AS Konto, ca.Name AS NazivKonta, le.SubAccountId AS Podkonto, sa.Name AS NazivPodkonta, " +
            "SUM(le.DebitAmount) AS Duguje, SUM(le.CreditAmount) AS Potrazuje, " +
            "CASE WHEN le.Account LIKE ''4%'' THEN SUM(le.CreditAmount - le.DebitAmount) ELSE SUM(le.DebitAmount - le.CreditAmount) END AS Iznos " +
            "FROM finance.LedgerEntry le JOIN finance.JournalEntry je ON je.Id = le.JournalEntryId " +
            "LEFT JOIN finance.ChartOfAccounts ca ON ca.Account = le.Account " +
            "LEFT JOIN finance.SubAccount sa ON sa.Id = le.SubAccountId " +
            "WHERE le.CompanyId = @CompanyId AND je.IsPosted = 1 AND le.Account IN (''4900'', ''5590'') " +
            "AND le.PostingDate BETWEEN @DateFrom AND @DateTo " +
            "GROUP BY le.Account, ca.Name, le.SubAccountId, sa.Name ORDER BY le.Account, le.SubAccountId";

        public const string CollectionSql =
            "SELECT SUM(le.DebitAmount) AS Zaduzeno, SUM(le.CreditAmount) AS Naplaceno, " +
            "SUM(le.DebitAmount - le.CreditAmount) AS Razlika, " +
            "CAST(100.0 * SUM(le.CreditAmount) / NULLIF(SUM(le.DebitAmount), 0) AS decimal(9, 2)) AS ProcenatNaplate " +
            "FROM finance.LedgerEntry le JOIN finance.JournalEntry je ON je.Id = le.JournalEntryId " +
            "WHERE le.CompanyId = @CompanyId AND je.IsPosted = 1 AND le.Account = ''2040'' " +
            "AND le.PostingDate BETWEEN @DateFrom AND @DateTo";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            Seed(migrationBuilder, IncomeExpenseName, "Finansijski izveštaj SZ - prihodi i rashodi po kontu/podkontu", 900, IncomeExpenseSql);
            Seed(migrationBuilder, CollectionName, "Finansijski izveštaj SZ - zaduženo, naplaćeno, procenat naplate", 901, CollectionSql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"DELETE FROM [reports].[ReportDefinition] WHERE [CompanyId] IS NULL AND [Name] IN (N'{IncomeExpenseName}', N'{CollectionName}');");
        }

        private static void Seed(MigrationBuilder migrationBuilder, string name, string title, int sortIndex, string sql) =>
            migrationBuilder.Sql($"""
                IF NOT EXISTS (SELECT 1 FROM [reports].[ReportDefinition] WHERE [CompanyId] IS NULL AND [Name] = N'{name}')
                BEGIN
                    INSERT INTO [reports].[ReportDefinition] ([CompanyId], [Name], [Title], [SortIndex], [FilterCaption], [IsActive])
                    VALUES (NULL, N'{name}', N'{title}', {sortIndex}, N'Parametri: DateFrom, DateTo (yyyy-MM-dd)', 1);
                    INSERT INTO [reports].[ReportDefinitionDetail] ([ReportDefinitionId], [QuerySql], [QueryName], [SortIndex], [Function])
                    VALUES (SCOPE_IDENTITY(), N'{sql}', N'{name}', 1, 'Table');
                END
                """);
    }
}
