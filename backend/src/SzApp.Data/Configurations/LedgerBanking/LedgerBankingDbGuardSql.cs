namespace SzApp.Data.Configurations.LedgerBanking;

public static class LedgerBankingDbGuardSql
{
    public const string CreateJournalGuard = """
        CREATE OR ALTER TRIGGER [finance].[TR_JournalEntry_PostingGuard]
        ON [finance].[JournalEntry]
        AFTER INSERT, UPDATE
        AS
        BEGIN
            SET NOCOUNT ON;
            IF EXISTS (SELECT 1 FROM deleted d JOIN inserted i ON i.Id = d.Id WHERE d.IsPosted = 1)
                THROW 51001, 'Posted journal entries are immutable.', 1;
            IF EXISTS (
                SELECT 1 FROM inserted i
                LEFT JOIN deleted d ON d.Id = i.Id
                WHERE i.IsPosted = 1 AND (d.Id IS NULL OR d.IsPosted = 0))
               AND ISNULL(TRY_CAST(SESSION_CONTEXT(N'szapp_allow_posting') AS int), 0) <> 1
                THROW 51002, 'Journal posting is allowed only through the posting service.', 1;
        END
        """;

    public const string CreateLedgerGuard = """
        CREATE OR ALTER TRIGGER [finance].[TR_LedgerEntry_PostedGuard]
        ON [finance].[LedgerEntry]
        AFTER INSERT, UPDATE, DELETE
        AS
        BEGIN
            SET NOCOUNT ON;
            IF EXISTS (
                SELECT 1
                FROM (SELECT JournalEntryId FROM inserted UNION SELECT JournalEntryId FROM deleted) x
                JOIN [finance].[JournalEntry] j ON j.Id = x.JournalEntryId
                WHERE j.IsPosted = 1)
                THROW 51003, 'Ledger lines belonging to a posted journal are immutable.', 1;
        END
        """;

    public const string DropGuards = """
        DROP TRIGGER IF EXISTS [finance].[TR_LedgerEntry_PostedGuard];
        DROP TRIGGER IF EXISTS [finance].[TR_JournalEntry_PostingGuard];
        """;
}
