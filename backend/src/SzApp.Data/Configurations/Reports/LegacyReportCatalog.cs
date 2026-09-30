namespace SzApp.Data.Configurations.Reports;

public sealed record LegacyReportSection(string QueryName, string Sql);

public sealed record LegacyReport(string Name, string Title, int SortIndex, string FilterCaption, IReadOnlyList<LegacyReportSection> Sections);

/// <summary>
/// GAP-18 / 9.8: legacy <c>Fin_Izv</c> (sub-reports <c>Fin_Izvestaj_SZ_00..11</c>) and the <c>tblIzvestaj</c>
/// datasheet reports translated to read-only T-SQL for the report engine. Seeded globally (CompanyId NULL)
/// by the LegacyReportsAndAuditTrail migration. Conventions: posted ledger only; legacy
/// <c>frmIzvestajFilterGK</c> = company + period + line type &lt;&gt; 98 (preuzeti dug); legacy "Partner"
/// (Kupac) = <c>core.PartnerAccount</c>; <c>TKONTO</c> = cost target of a 5-character sub-account.
/// Every section of one report must use the same parameters (the runner rejects unused ones).
/// </summary>
public static class LegacyReportCatalog
{
    public const string PeriodCaption = "Parametri: DateFrom, DateTo (yyyy-MM-dd)";

    private const string Ledger =
        "finance.LedgerEntry le JOIN finance.JournalEntry je ON je.Id = le.JournalEntryId AND je.IsPosted = 1";

    private const string Not98 =
        "NOT EXISTS (SELECT 1 FROM core.ShortList lt WHERE lt.Id = le.LineTypeId AND lt.IndexValue = 98)";

    private const string Tkonto =
        "tk AS (SELECT sa.Id AS SubAccountId, COALESCE(NULLIF(sa.CostToSubAccountId, ''), sa.Id) AS TKonto, " +
        "sa.CostToSubAccountId AS CostTarget FROM finance.SubAccount sa WHERE LEN(sa.Id) = 5)";

    private const string AccountPartner =
        "JOIN core.PartnerAccount pa ON pa.Id = le.PartnerAccountId JOIN core.Partner p ON p.Id = pa.PartnerId";

    private const string Period = "le.PostingDate BETWEEN @DateFrom AND @DateTo";

    // Fin_Izvestaj_SZ_00/_00_PS_Y/_00_TS/_01: 2410 by year before the period, then the period.
    public const string Fin00Sql =
        "SELECT CASE WHEN le.PostingDate < @DateFrom THEN CAST(YEAR(le.PostingDate) AS varchar(4)) ELSE 'PERIOD' END AS Period, " +
        "MIN(le.PostingDate) AS OdDatuma, MAX(le.PostingDate) AS DoDatuma, SUM(le.DebitAmount) AS Duguje, " +
        "SUM(le.CreditAmount) AS Potrazuje, SUM(le.DebitAmount - le.CreditAmount) AS Saldo " +
        $"FROM {Ledger} WHERE le.CompanyId = @CompanyId AND le.Account = '2410' AND le.PostingDate <= @DateTo " +
        "GROUP BY CASE WHEN le.PostingDate < @DateFrom THEN CAST(YEAR(le.PostingDate) AS varchar(4)) ELSE 'PERIOD' END " +
        "ORDER BY MIN(le.PostingDate)";

    // Fin_Izvestaj_SZ_02 (+_0, _2, sub0..sub4): per cost sub-account 1*, revenue 4900 and customers 2040 on the
    // sub-account itself, suppliers 4350/5532 via TKONTO; FondStanje as in _02_2.
    public const string Fin02Sql =
        $"WITH {Tkonto}, " +
        $"used AS (SELECT DISTINCT tk.TKonto FROM {Ledger} JOIN tk ON tk.SubAccountId = le.SubAccountId " +
        $"WHERE le.CompanyId = @CompanyId AND {Period} AND le.SubAccountId LIKE '1%' AND {Not98}), " +
        $"rev AS (SELECT le.SubAccountId, SUM(CASE WHEN le.Account = '4900' THEN le.CreditAmount ELSE 0 END) AS Fakturisano4900, " +
        "SUM(CASE WHEN le.Account = '2040' THEN le.DebitAmount ELSE 0 END) AS Fakturisano2040, " +
        "SUM(CASE WHEN le.Account = '2040' THEN le.CreditAmount ELSE 0 END) AS Uplaceno2040 " +
        $"FROM {Ledger} WHERE le.CompanyId = @CompanyId AND {Period} AND le.Account IN ('4900', '2040') GROUP BY le.SubAccountId), " +
        "dob AS (SELECT tk.TKonto, SUM(le.DebitAmount) AS PlacenoDob, SUM(le.CreditAmount) AS PotrazujuDob " +
        $"FROM {Ledger} JOIN tk ON tk.SubAccountId = le.SubAccountId WHERE le.CompanyId = @CompanyId AND {Period} " +
        "AND le.Account IN ('4350', '5532') GROUP BY tk.TKonto) " +
        "SELECT LEFT(u.TKonto, 2) AS Grupa, g1.Name AS NazivGrupe, LEFT(u.TKonto, 4) AS Grupa4, g4.Name AS NazivGrupe4, " +
        "u.TKonto AS Podkonto, sa.Name AS NazivPodkonta, ISNULL(rev.Fakturisano4900, 0) AS Fakturisano4900, " +
        "ISNULL(rev.Fakturisano2040, 0) AS Fakturisano2040, ISNULL(rev.Uplaceno2040, 0) AS Uplaceno2040, " +
        "ISNULL(dob.PlacenoDob, 0) AS PlacenoDob, ISNULL(dob.PotrazujuDob, 0) AS PotrazujuDob, " +
        "ISNULL(rev.Uplaceno2040, 0) - ISNULL(dob.PlacenoDob, 0) AS Saldo, " +
        "ISNULL(rev.Fakturisano2040, 0) - ISNULL(rev.Uplaceno2040, 0) AS Nenaplaceno2040, " +
        "CASE WHEN LEFT(u.TKonto, 2) = '10' OR ISNULL(rev.Uplaceno2040, 0) - ISNULL(dob.PlacenoDob, 0) < 0 " +
        "THEN ISNULL(rev.Uplaceno2040, 0) - ISNULL(dob.PlacenoDob, 0) ELSE 0 END AS FondStanje " +
        "FROM used u LEFT JOIN rev ON rev.SubAccountId = u.TKonto LEFT JOIN dob ON dob.TKonto = u.TKonto " +
        "LEFT JOIN finance.SubAccount sa ON sa.Id = u.TKonto LEFT JOIN finance.SubAccount g1 ON g1.Id = LEFT(u.TKonto, 2) " +
        "LEFT JOIN finance.SubAccount g4 ON g4.Id = LEFT(u.TKonto, 4) ORDER BY u.TKonto";

    // Fin_Izvestaj_SZ_03: supplier documents on sub-accounts whose cost goes to 10* (fund), up to DateTo.
    public const string Fin03Sql =
        $"WITH {Tkonto} " +
        "SELECT si.Id AS UlazniRacunId, si.Caption AS Racun, si.InvoiceDate AS DatumRacuna, p.Name AS Dobavljac, le.Account AS Konto, " +
        "LEFT(tk.TKonto, 2) AS Grupa, g1.Name AS NazivGrupe, tk.TKonto, t.Name AS NazivTKonta, le.SubAccountId AS Podkonto, " +
        "sa.Name AS NazivPodkonta, SUM(le.DebitAmount) AS Duguje, SUM(le.CreditAmount) AS Potrazuje, " +
        "SUM(le.DebitAmount - le.CreditAmount) AS Saldo, " +
        "CASE WHEN le.Account = '5532' THEN 0 WHEN SUM(le.CreditAmount) > 0 THEN SUM(le.CreditAmount - le.DebitAmount) ELSE 0 END AS Dug, " +
        "CASE WHEN le.Account IN ('5532', '5241') THEN SUM(le.DebitAmount - le.CreditAmount) ELSE SUM(le.CreditAmount) END AS Obracunato " +
        $"FROM {Ledger} JOIN tk ON tk.SubAccountId = le.SubAccountId {AccountPartner} " +
        "LEFT JOIN billing.SupplierInvoice si ON si.Id = le.SupplierInvoiceId LEFT JOIN core.ShortList dt ON dt.Id = si.DocumentTypeId " +
        "LEFT JOIN finance.SubAccount sa ON sa.Id = le.SubAccountId LEFT JOIN finance.SubAccount t ON t.Id = tk.TKonto " +
        "LEFT JOIN finance.SubAccount g1 ON g1.Id = LEFT(tk.TKonto, 2) " +
        "WHERE le.CompanyId = @CompanyId AND le.PostingDate <= @DateTo AND @DateFrom <= @DateTo AND tk.CostTarget LIKE '10%' " +
        "AND ((le.Account IN ('4350', '5532') AND si.Id IS NULL) OR (le.Account = '4350' AND dt.IndexValue = 2)) " +
        "GROUP BY si.Id, si.Caption, si.InvoiceDate, p.Name, le.Account, tk.TKonto, g1.Name, t.Name, le.SubAccountId, sa.Name " +
        "ORDER BY si.InvoiceDate, p.Name";

    // Fin_Izvestaj_SZ_04 (RR/VR, sub1/sub2): invoice batches with transactions in the period; charged vs paid on 2040.
    // Legacy summed every GK line of the invoice; in this system only 2040 lines carry InvoiceId, so it is explicit.
    private const string BatchCtes =
        "inv AS (SELECT i.InvoiceBatchId, COUNT(*) AS BrojRacuna, SUM(i.Total) AS Ukupno FROM finance.Invoice i " +
        "WHERE i.CompanyId = @CompanyId AND i.IsCancelled = 0 AND i.TransactionDate BETWEEN @DateFrom AND @DateTo " +
        "AND i.InvoiceBatchId IS NOT NULL GROUP BY i.InvoiceBatchId), " +
        $"gk AS (SELECT i.InvoiceBatchId, SUM(le.DebitAmount) AS Duguje, SUM(le.CreditAmount) AS Potrazuje FROM {Ledger} " +
        $"JOIN finance.Invoice i ON i.Id = le.InvoiceId WHERE le.CompanyId = @CompanyId AND i.IsCancelled = 0 AND {Period} " +
        "AND le.Account = '2040' GROUP BY i.InvoiceBatchId)";

    public const string Fin04Sql =
        $"WITH {BatchCtes} " +
        "SELECT CASE WHEN b.ExtraordinaryInvoiceMarker IS NULL OR b.ExtraordinaryInvoiceMarker = '' OR b.ExtraordinaryInvoiceMarker LIKE 'V%' " +
        "THEN 'Redovni' ELSE 'Vanredni' END AS Vrsta, CONCAT('R-', b.Id) AS Dokument, b.Caption AS Naziv, inv.BrojRacuna, " +
        "b.IssueDate AS DatumIzdavanja, b.DueDate AS Valuta, b.TransactionDate AS DatumPrometa, inv.Ukupno, gk.Duguje, gk.Potrazuje, " +
        "gk.Duguje - gk.Potrazuje AS Dug FROM inv JOIN billing.InvoiceBatch b ON b.Id = inv.InvoiceBatchId " +
        "JOIN gk ON gk.InvoiceBatchId = inv.InvoiceBatchId WHERE b.TransactionDate <= @DateTo ORDER BY Vrsta, b.IssueDate";

    // Fin_Izvestaj_SZ_05: 5532 per sub-account and partner in the period.
    public const string Fin05Sql =
        "SELECT le.SubAccountId AS Podkonto, sa.Name AS NazivPodkonta, p.Name AS Partner, SUM(le.DebitAmount) AS Placeno, " +
        $"SUM(le.CreditAmount) AS Uplaceno FROM {Ledger} {AccountPartner} LEFT JOIN finance.SubAccount sa ON sa.Id = le.SubAccountId " +
        $"WHERE le.CompanyId = @CompanyId AND le.Account = '5532' AND {Period} " +
        "GROUP BY le.SubAccountId, sa.Name, p.Name ORDER BY le.SubAccountId, p.Name";

    // Fin_Izvestaj_SZ_06_3 (_PS + _TS): funds 3* except 31912 -- earlier years summed, period line by line.
    public const string Fin06Sql =
        "SELECT le.SubAccountId AS Podkonto, sa.Name AS NazivPodkonta, CAST(YEAR(le.PostingDate) AS varchar(4)) AS Godina, " +
        "CAST(NULL AS date) AS Datum, CAST(NULL AS nvarchar(255)) AS Partner, CAST(NULL AS nvarchar(255)) AS Dokument, " +
        "SUM(le.DebitAmount) AS Duguje, SUM(le.CreditAmount) AS Potrazuje " +
        $"FROM {Ledger} JOIN finance.SubAccount sa ON sa.Id = le.SubAccountId {AccountPartner} " +
        "WHERE le.CompanyId = @CompanyId AND le.SubAccountId LIKE '3%' AND le.SubAccountId <> '31912' AND le.PostingDate < @DateFrom " +
        "GROUP BY le.SubAccountId, sa.Name, YEAR(le.PostingDate) " +
        "UNION ALL SELECT le.SubAccountId, sa.Name, CAST(YEAR(le.PostingDate) AS varchar(4)), le.PostingDate, p.Name, le.DocumentRef, " +
        $"le.DebitAmount, le.CreditAmount FROM {Ledger} JOIN finance.SubAccount sa ON sa.Id = le.SubAccountId {AccountPartner} " +
        $"WHERE le.CompanyId = @CompanyId AND le.SubAccountId LIKE '3%' AND le.SubAccountId <> '31912' AND {Period} " +
        "ORDER BY Podkonto, Godina, Datum";

    // Fin_Izvestaj_SZ_07_Stanari_NeRasporedjeno: 204* without a sub-account up to DateTo.
    public const string Fin07Sql =
        "SELECT SUM(le.DebitAmount) AS Duguje, SUM(le.CreditAmount) AS Potrazuje, SUM(le.DebitAmount - le.CreditAmount) AS Saldo " +
        $"FROM {Ledger} WHERE le.CompanyId = @CompanyId AND le.Account LIKE '204%' AND le.SubAccountId IS NULL " +
        $"AND le.PostingDate <= @DateTo AND @DateFrom <= @DateTo AND {Not98}";

    // Fin_Izvestaj_SZ_02_sub5 restricted to 11*: supplier obligations/payments per supplier account in the period.
    private const string SupplierCte =
        "dob AS (SELECT le.PartnerAccountId, SUM(le.CreditAmount) AS Potrazuje, SUM(le.DebitAmount) AS Placeno " +
        $"FROM {Ledger} WHERE le.CompanyId = @CompanyId AND le.Account IN ('4350', '5532') AND {Period} " +
        "AND LEN(le.SubAccountId) = 5 AND le.SubAccountId LIKE '11%' AND le.PartnerAccountId IS NOT NULL GROUP BY le.PartnerAccountId)";

    // Fin_Izvestaj_SZ_08 (+ sub1/sub2 procenat naplate): collection rate of the period's invoices vs. what was paid
    // to each 11* supplier; DoplatitiIznos = (rate - paid share) x supplier's claim.
    public const string Fin08Sql =
        $"WITH {BatchCtes}, {SupplierCte}, " +
        "rate AS (SELECT SUM(gk.Potrazuje) / NULLIF(SUM(inv.Ukupno), 0) AS ProcenatNaplate FROM inv JOIN gk ON gk.InvoiceBatchId = inv.InvoiceBatchId) " +
        "SELECT pa.AccountNumber AS Sifra, p.Name AS Dobavljac, dob.Potrazuje, dob.Placeno, dob.Placeno / NULLIF(dob.Potrazuje, 0) AS ProcenatPlaceno, " +
        "rate.ProcenatNaplate, rate.ProcenatNaplate - dob.Placeno / NULLIF(dob.Potrazuje, 0) AS DoplatitiProcenat, " +
        "(rate.ProcenatNaplate - dob.Placeno / NULLIF(dob.Potrazuje, 0)) * dob.Potrazuje AS DoplatitiIznos " +
        "FROM dob CROSS JOIN rate JOIN core.PartnerAccount pa ON pa.Id = dob.PartnerAccountId JOIN core.Partner p ON p.Id = pa.PartnerId " +
        "ORDER BY p.Name";

    // Fin_Izvestaj_SZ_09 (+ sub1_4350, sub2_2040) = tblIzvestaj "Isplata za dobavljače": what tenants paid (2040 P on
    // the supplier's invoices, 11*) vs. what was paid to the supplier in the period.
    public const string Fin09Sql =
        $"WITH {SupplierCte}, " +
        "nap AS (SELECT si.SupplierPartnerAccountId AS PartnerAccountId, SUM(le.CreditAmount) AS Naplaceno " +
        $"FROM {Ledger} JOIN billing.SupplierInvoice si ON si.Id = le.SupplierInvoiceId WHERE le.CompanyId = @CompanyId " +
        $"AND le.Account = '2040' AND le.SubAccountId LIKE '11%' AND {Period} GROUP BY si.SupplierPartnerAccountId) " +
        "SELECT pa.AccountNumber AS Sifra, p.Name AS Dobavljac, dob.Potrazuje, dob.Placeno, nap.Naplaceno, " +
        "nap.Naplaceno - dob.Placeno AS DoplatitiIznos FROM dob JOIN nap ON nap.PartnerAccountId = dob.PartnerAccountId " +
        "JOIN core.PartnerAccount pa ON pa.Id = dob.PartnerAccountId JOIN core.Partner p ON p.Id = pa.PartnerId ORDER BY p.Name";

    // Fin_Izvestaj_SZ_11: fund lines 3* except 31912 (legacy had no date filter -- limited to DateTo here).
    public const string Fin11Sql =
        "SELECT le.SubAccountId AS Podkonto, le.PostingDate AS Datum, le.Account AS Konto, pa.AccountNumber AS Sifra, p.Name AS Partner, " +
        $"le.DocumentRef AS Dokument, le.DebitAmount AS Duguje, le.CreditAmount AS Potrazuje FROM {Ledger} {AccountPartner} " +
        "WHERE le.CompanyId = @CompanyId AND le.SubAccountId LIKE '3%' AND le.SubAccountId <> '31912' " +
        "AND le.PostingDate <= @DateTo AND @DateFrom <= @DateTo ORDER BY le.DocumentRef, le.PostingDate";

    // frmIzvestaj_StanariStanje / _Duguju: customer accounts (2040) of the company with their period turnover.
    private const string TenantBalanceSelect =
        "SELECT pa.AccountNumber AS Sifra, p.Name AS Naziv, u.Name AS Prostor, ISNULL(SUM(le.DebitAmount), 0) AS Duguje, " +
        "ISNULL(SUM(le.CreditAmount), 0) AS Potrazuje, ROUND(ISNULL(SUM(le.DebitAmount - le.CreditAmount), 0), 2) AS Saldo, " +
        "CASE WHEN ROUND(ISNULL(SUM(le.DebitAmount - le.CreditAmount), 0), 2) > 0 THEN ROUND(SUM(le.DebitAmount - le.CreditAmount), 2) ELSE 0 END AS Dug, " +
        "CASE WHEN ROUND(ISNULL(SUM(le.DebitAmount - le.CreditAmount), 0), 2) < 0 THEN -ROUND(SUM(le.DebitAmount - le.CreditAmount), 2) ELSE 0 END AS Preplata, " +
        "CASE WHEN ROUND(ISNULL(SUM(le.DebitAmount - le.CreditAmount), 0), 2) < -0.001 THEN -1 " +
        "WHEN ABS(ROUND(ISNULL(SUM(le.DebitAmount - le.CreditAmount), 0), 2)) < 0.001 THEN 0 " +
        "WHEN ISNULL(SUM(le.CreditAmount), 0) = 0 THEN 2 ELSE 1 END AS Grupa " +
        "FROM core.PartnerAccount pa JOIN core.Partner p ON p.Id = pa.PartnerId " +
        "LEFT JOIN core.Contract c ON c.Id = pa.ContractId LEFT JOIN core.Unit u ON u.Id = c.UnitId " +
        "LEFT JOIN (finance.LedgerEntry le JOIN finance.JournalEntry je ON je.Id = le.JournalEntryId AND je.IsPosted = 1) " +
        $"ON le.PartnerAccountId = pa.Id AND le.CompanyId = @CompanyId AND {Period} AND {Not98} " +
        "WHERE pa.CompanyId = @CompanyId AND pa.Account = '2040' " +
        "GROUP BY pa.Id, pa.AccountNumber, p.Name, u.Name, u.SortingNumber ";

    public const string TenantBalanceSql = TenantBalanceSelect + "ORDER BY u.SortingNumber, u.Name, p.Name";

    public const string DebtorsSql = TenantBalanceSelect +
        "HAVING ROUND(SUM(le.DebitAmount - le.CreditAmount), 2) > 0 ORDER BY u.SortingNumber, u.Name, p.Name";

    // frmIzvestaj_Stanari: customer accounts with their unit (one row per account; legacy joined PDs into one cell).
    public const string TenantListSql =
        "SELECT pa.AccountNumber AS Sifra, p.Name AS Naziv, u.Name AS Prostor, ut.ShortName AS TipProstora, " +
        "be.EntranceName AS Ulaz, c.IsActive AS UgovorAktivan FROM core.PartnerAccount pa JOIN core.Partner p ON p.Id = pa.PartnerId " +
        "LEFT JOIN core.Contract c ON c.Id = pa.ContractId LEFT JOIN core.Unit u ON u.Id = c.UnitId " +
        "LEFT JOIN core.ShortList ut ON ut.Id = u.UnitTypeId LEFT JOIN core.BuildingEntrance be ON be.Id = u.BuildingEntranceId " +
        "WHERE pa.CompanyId = @CompanyId AND pa.Account = '2040' ORDER BY ut.IndexSort, u.SortingNumber, u.Name, p.Name";

    // frmIzvestaj_Izvod: statement lines of the period with their postings (bank 2410 side excluded).
    public const string StatementSql =
        "SELECT bs.Date AS Datum, bs.StatementNumber AS BrojIzvoda, bs.StatementSuffix AS Godina, bsl.LineNumber AS Stavka, " +
        "pa.AccountNumber AS SifraPartnera, p.Name AS Partner, COUNT(le.Id) AS BrojKnjizenja, SUM(le.CreditAmount) AS Potrazuje, " +
        "SUM(le.DebitAmount) AS Duguje, le.SubAccountId AS Podkonto, sa.Name AS NazivTroska, le.SupplierInvoiceId AS UlazniRacun " +
        "FROM finance.BankStatement bs JOIN finance.BankStatementLine bsl ON bsl.BankStatementId = bs.Id " +
        "LEFT JOIN (finance.LedgerEntry le JOIN finance.JournalEntry je ON je.Id = le.JournalEntryId AND je.IsPosted = 1) " +
        "ON le.BankStatementLineId = bsl.Id AND le.Account <> '2410' " +
        "LEFT JOIN core.PartnerAccount pa ON pa.Id = le.PartnerAccountId LEFT JOIN core.Partner p ON p.Id = pa.PartnerId " +
        "LEFT JOIN finance.SubAccount sa ON sa.Id = le.SubAccountId " +
        "WHERE bs.CompanyId = @CompanyId AND bs.Date BETWEEN @DateFrom AND @DateTo " +
        "GROUP BY bs.Date, bs.StatementNumber, bs.StatementSuffix, bsl.LineNumber, pa.AccountNumber, p.Name, le.SubAccountId, sa.Name, le.SupplierInvoiceId " +
        "ORDER BY bs.Date, bs.StatementNumber, bsl.LineNumber";

    // frmIzvestaj_Izvod_grp: one row per statement line (payer, purpose, amounts) with the posted partner.
    public const string StatementByPaymentSql =
        "SELECT bs.Date AS Datum, bs.StatementNumber AS BrojIzvoda, bs.StatementSuffix AS Godina, bsl.LineNumber AS Stavka, " +
        "bsl.PayerRecipientName AS Uplatilac, bsl.Info AS Svrha, bsl.Debit AS Isplata, bsl.Credit AS Uplata, " +
        "pa.AccountNumber AS SifraPartnera, p.Name AS Partner, COUNT(le.Id) AS BrojKnjizenja, SUM(le.CreditAmount) AS Potrazuje, " +
        "SUM(le.DebitAmount) AS Duguje FROM finance.BankStatement bs JOIN finance.BankStatementLine bsl ON bsl.BankStatementId = bs.Id " +
        "LEFT JOIN (finance.LedgerEntry le JOIN finance.JournalEntry je ON je.Id = le.JournalEntryId AND je.IsPosted = 1) " +
        "ON le.BankStatementLineId = bsl.Id AND le.Account <> '2410' " +
        "LEFT JOIN core.PartnerAccount pa ON pa.Id = le.PartnerAccountId LEFT JOIN core.Partner p ON p.Id = pa.PartnerId " +
        "WHERE bs.CompanyId = @CompanyId AND bs.Date BETWEEN @DateFrom AND @DateTo " +
        "GROUP BY bs.Date, bs.StatementNumber, bs.StatementSuffix, bsl.LineNumber, bsl.PayerRecipientName, bsl.Info, bsl.Debit, bsl.Credit, pa.AccountNumber, p.Name " +
        "ORDER BY bs.Date, bs.StatementNumber, bsl.LineNumber";

    // KONTO_TROSKA_SUME (+ _POTRAZIVANJA_STANAR/_DOB): 4-character cost sub-accounts; customers 2040 in the period,
    // suppliers 4350/5532 in total (legacy had no date filter on the supplier side).
    public const string CostAccountsSql =
        $"WITH st AS (SELECT le.SubAccountId, SUM(le.DebitAmount) AS Duguju, SUM(le.CreditAmount) AS Naplaceno FROM {Ledger} " +
        $"WHERE le.CompanyId = @CompanyId AND le.Account LIKE '2040%' AND {Period} GROUP BY le.SubAccountId), " +
        $"dob AS (SELECT le.SubAccountId, SUM(le.CreditAmount) AS Potrazuju, SUM(le.DebitAmount) AS Placeno FROM {Ledger} " +
        "WHERE le.CompanyId = @CompanyId AND (le.Account LIKE '4350%' OR le.Account LIKE '5532%') GROUP BY le.SubAccountId) " +
        "SELECT sa.Id AS Podkonto, sa.Name AS Naziv, dob.Potrazuju AS PotrazujuDobavljaci, dob.Placeno AS PlacenoDobavljacima, " +
        "st.Duguju AS DugujuKupci, st.Naplaceno AS NaplacenoOdKupaca FROM finance.SubAccount sa " +
        "JOIN st ON st.SubAccountId = sa.Id JOIN dob ON dob.SubAccountId = sa.Id WHERE LEN(sa.Id) = 4 ORDER BY sa.Id";

    // GK_GRP_DOK_COUNT (GK_Grp_Dok): customers that have both an open document (> 1) and an overpaid one (< -1).
    public const string RebookingSql =
        "WITH d AS (SELECT le.PartnerAccountId, le.DocumentRef, le.InvoiceId, SUM(le.DebitAmount - le.CreditAmount) AS Saldo " +
        $"FROM {Ledger} WHERE le.CompanyId = @CompanyId AND le.Account = '2040' AND le.PartnerAccountId IS NOT NULL " +
        "GROUP BY le.PartnerAccountId, le.DocumentRef, le.InvoiceId HAVING ABS(SUM(le.DebitAmount - le.CreditAmount)) > 1) " +
        "SELECT pa.AccountNumber AS Sifra, p.Name AS Partner, SUM(CASE WHEN d.Saldo > 0 THEN 1 ELSE 0 END) AS DokumentaSaDugom, " +
        "SUM(CASE WHEN d.Saldo < 0 THEN 1 ELSE 0 END) AS DokumentaSaPreplatom, SUM(d.Saldo) AS Saldo FROM d " +
        "JOIN core.PartnerAccount pa ON pa.Id = d.PartnerAccountId JOIN core.Partner p ON p.Id = pa.PartnerId " +
        "GROUP BY pa.AccountNumber, p.Name HAVING SUM(CASE WHEN d.Saldo > 0 THEN 1 ELSE 0 END) > 0 " +
        "AND SUM(CASE WHEN d.Saldo < 0 THEN 1 ELSE 0 END) > 0 ORDER BY p.Name";

    // frmIzvestaj_Objekti (+ _Objekti2/3): units with coefficients and the customer account's partner.
    public const string UnitListSql =
        "SELECT u.Id AS SifraPD, u.Name AS PosebniDeo, ut.Caption AS Tip, be.EntranceName AS Ulaz, u.FloorNumber AS Sprat, " +
        "u.K1, u.K2, u.K3, u.K4, u.K5, pa.AccountNumber AS SifraKorisnika, p.Name AS Korisnik, c.IsActive AS UgovorAktivan " +
        "FROM core.Unit u LEFT JOIN core.Contract c ON c.Id = u.ContractId " +
        "LEFT JOIN core.PartnerAccount pa ON pa.ContractId = c.Id AND pa.Account = '2040' LEFT JOIN core.Partner p ON p.Id = pa.PartnerId " +
        "LEFT JOIN core.ShortList ut ON ut.Id = u.UnitTypeId LEFT JOIN core.BuildingEntrance be ON be.Id = u.BuildingEntranceId " +
        "WHERE u.CompanyId = @CompanyId ORDER BY ut.IndexSort, u.SortingNumber, u.Name";

    // frmIzvestaj_Dobavljaci: supplier accounts (4350/5532) with period turnover in this company.
    public const string SupplierBalanceSql =
        "SELECT pa.AccountNumber AS Sifra, p.Name AS Dobavljac, pa.Account AS Konto, SUM(le.DebitAmount) AS Duguje, " +
        $"SUM(le.CreditAmount) AS Potrazuje, ROUND(SUM(le.DebitAmount - le.CreditAmount), 2) AS Saldo FROM {Ledger} {AccountPartner} " +
        $"WHERE le.CompanyId = @CompanyId AND pa.Account IN ('4350', '5532') AND {Period} AND {Not98} " +
        "GROUP BY pa.AccountNumber, p.Name, pa.Account ORDER BY p.Name";

    private static LegacyReport Single(string name, string title, int sort, string caption, string sql) =>
        new(name, title, sort, caption, [new LegacyReportSection(name, sql)]);

    public static IReadOnlyList<LegacyReport> Reports { get; } =
    [
        new("Fin_Izv - Finansijski izveštaj SZ", "Finansijski izveštaj SZ (Fin_Izv, sekcije 00-11)", 910, PeriodCaption,
        [
            new("Fin_Izvestaj_SZ_00 - Žiro račun 2410", Fin00Sql),
            new("Fin_Izvestaj_SZ_02 - Po podkontu troška", Fin02Sql),
            new("Fin_Izvestaj_SZ_03 - Računi dobavljača (fond 10*)", Fin03Sql),
            new("Fin_Izvestaj_SZ_04 - Serije računa", Fin04Sql),
            new("Fin_Izvestaj_SZ_05 - Konto 5532", Fin05Sql),
            new("Fin_Izvestaj_SZ_06 - Fondovi 3*", Fin06Sql),
            new("Fin_Izvestaj_SZ_07 - Neraspoređeno 2040", Fin07Sql),
            new("Fin_Izvestaj_SZ_08 - Procenat naplate", Fin08Sql),
            new("Fin_Izvestaj_SZ_09 - Isplata dobavljačima", Fin09Sql),
            new("Fin_Izvestaj_SZ_11 - Stavke fondova 3*", Fin11Sql),
        ]),
        Single("Izvod stanja stanara", "Izveštaj - stanje stanara za period", 911, PeriodCaption, TenantBalanceSql),
        Single("Spisak stanara", "Spisak stanara", 912, "Bez parametara", TenantListSql),
        Single("Izveštaj izvoda", "Izveštaj izvoda", 913, PeriodCaption, StatementSql),
        Single("Izveštaj izvoda po uplati", "Izveštaj izvoda po uplati", 914, PeriodCaption, StatementByPaymentSql),
        Single("Konta prihoda i troškova", "Izveštaj konta prihoda i troškova", 915, PeriodCaption, CostAccountsSql),
        Single("Potrebna preknjižavanja", "Potrebna preknjižavanja po dokumentima", 916, "Bez parametara", RebookingSql),
        Single("Izvod stanja korisnika duguju", "Izveštaj - duguju za period", 917, PeriodCaption, DebtorsSql),
        Single("Spisak posebnih delova", "Spisak posebnih delova (sa koeficijentima)", 918, "Bez parametara", UnitListSql),
        Single("Stanje dobavljača", "Dobavljači - presek stanja", 919, PeriodCaption, SupplierBalanceSql),
        Single("Isplata za dobavljače", "Isplata za dobavljače (Fin_Izvestaj_SZ_09)", 920, PeriodCaption, Fin09Sql),
    ];
}
