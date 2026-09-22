# Ledger/Banking integration request

Centralne fajlove treba povezati tek nakon spajanja svih feature modula:

1. U `Program.cs`, pre `builder.Build()`, pozvati:

   ```csharp
   builder.Services.AddLedgerBankingFeature(builder.Configuration);
   ```

2. Posle autentikacije/autorizacije i pre `app.Run()` pozvati:

   ```csharp
   app.MapLedgerBankingEndpoints();
   ```

3. `SzAppDbContext` već koristi `ApplyConfigurationsFromAssembly`, pa se Ledger/Banking
   konfiguracije otkrivaju automatski. Ne pozivati dodatno `ApplyLedgerBankingModel`, jer je
   extension namenjen drugim/test kontekstima.

4. Posle svih modelskih integracija generisati novu migraciju. U njen `Up` dodati, nakon
   kreiranja tabela:

   ```csharp
   migrationBuilder.Sql(LedgerBankingDbGuardSql.CreateJournalGuard);
   migrationBuilder.Sql(LedgerBankingDbGuardSql.CreateLedgerGuard);
   ```

   U `Down`, pre uklanjanja finance tabela, dodati:

   ```csharp
   migrationBuilder.Sql(LedgerBankingDbGuardSql.DropGuards);
   ```

5. Za worker/background knjiženje podesiti postojeći Identity staff ID kroz
   `LedgerBanking:SystemUserId` / `LedgerBanking__SystemUserId`. HTTP knjiženje koristi ID
   prijavljenog korisnika.

6. Globalni exception handler treba da mapira `KeyNotFoundException` na 404,
   `BadHttpRequestException` na 400 i `UnauthorizedAccessException` na 401/403.

7. U root routeru zameniti `ledger`/`banking` placeholder sa eksportovanim
   `LedgerBankingPage`; feature namerno ne menja router.
