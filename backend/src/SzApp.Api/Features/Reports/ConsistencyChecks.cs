using Microsoft.EntityFrameworkCore;
using SzApp.Data;
using SzApp.Data.Entities;
using SzApp.Data.Entities.Billing;
using SzApp.Data.Entities.LedgerBanking;
using SzApp.Domain.LedgerBanking;

namespace SzApp.Api.Features.Reports;

/// <summary>One violating row: company, a drill-down key (journal/partner account/document) and a short info text.</summary>
public sealed record ConsistencyRow(int CompanyId, string Key, string Info, decimal? Amount);
public sealed record ConsistencyCheckResult(string Id, string Legacy, string Name, bool Critical, int Count, IReadOnlyList<ConsistencyRow> Rows);
public sealed record ConsistencyNotTranslated(string Id, string Reason);
public sealed record ConsistencyReport(int? CompanyId, IReadOnlyList<ConsistencyCheckResult> Checks, IReadOnlyList<ConsistencyNotTranslated> NotTranslated);

/// <summary>
/// ERR-01/03: a legacy ERROR_* consistency check, translated to a read-only query on the new schema.
/// companyId null = all companies (Root only, enforced by the endpoint). Never mutates anything --
/// the legacy auto-executed DELETE/rasknjižavanje (070/071/007) is deliberately not ported.
/// </summary>
public sealed record ConsistencyCheck(
    string Id,
    string Legacy,
    string Name,
    bool Critical,
    Func<SzAppDbContext, int?, IQueryable<ConsistencyRow>> Query);

public static class ConsistencyChecks
{
    private static IQueryable<LedgerEntry> Ledger(SzAppDbContext db, int? companyId) =>
        db.LedgerEntries.AsNoTracking().Where(x => companyId == null || x.CompanyId == companyId);

    public static readonly IReadOnlyList<ConsistencyCheck> All =
    [
        new("GK-015", "ERROR_015_GK_POVEZIVANJE_SUBKONTO_RAZLIKA",
            "2040: dokument sa uplatom i zaduženjem je u preplati (Σ D−P < 0)", true,
            (db, c) => Ledger(db, c).Where(x => x.Account == LedgerAccounts.Customers)
                .GroupBy(x => new { x.CompanyId, PartnerAccountId = EF.Property<int?>(x, "PartnerAccountId"), x.DocumentRef, x.SupplierInvoiceId, SubAccountId = EF.Property<string?>(x, "SubAccountId") })
                .Where(g => g.Sum(x => x.CreditAmount) != 0 && g.Sum(x => x.DebitAmount) != 0 && g.Sum(x => x.DebitAmount - x.CreditAmount) <= -0.005m)
                .Select(g => new ConsistencyRow(g.Key.CompanyId, "PA " + g.Key.PartnerAccountId, (g.Key.DocumentRef ?? "") + " / RDOB " + g.Key.SupplierInvoiceId, g.Sum(x => x.DebitAmount - x.CreditAmount)))),

        new("GK-016", "ERROR_016_GK_204X_DPO_DATUM_NEMA",
            "2040: stavka bez datuma dospeća (DPO)", true,
            (db, c) => Ledger(db, c).Where(x => x.Account.StartsWith("204") && x.DueDate == null)
                .Select(x => new ConsistencyRow(x.CompanyId, "Nalog " + x.JournalEntryId, "PA " + EF.Property<int?>(x, "PartnerAccountId") + " / " + (x.DocumentRef ?? ""), x.DebitAmount - x.CreditAmount))),

        new("GK-017", "ERROR_017_GK_204X_POVEZIVANJE_SUBKONTA",
            "2040: partner ima istovremeno otvorene dugove i preplate po dokumentima", true,
            (db, c) => Ledger(db, c).Where(x => x.Account.StartsWith("204"))
                .GroupBy(x => new { x.CompanyId, PartnerAccountId = EF.Property<int?>(x, "PartnerAccountId"), x.DocumentRef, x.SupplierInvoiceId })
                .Select(g => new { g.Key.CompanyId, g.Key.PartnerAccountId, Balance = g.Sum(x => x.DebitAmount - x.CreditAmount), Debit = g.Sum(x => x.DebitAmount) })
                .Where(x => (x.Balance >= 0.005m || x.Balance <= -0.005m) && x.Debit > -0.01m)
                .GroupBy(x => new { x.CompanyId, x.PartnerAccountId })
                .Where(g => g.Any(x => x.Balance > 0) && g.Any(x => x.Balance < 0))
                .Select(g => new ConsistencyRow(g.Key.CompanyId, "PA " + g.Key.PartnerAccountId, "dokumenata: " + g.Count(), g.Sum(x => x.Balance)))),

        new("GK-021", "ERROR_021_RACUNI-DATUMPROMETA-DATUMVALUTE-RACUNA",
            "Knjiženje serije računa: datum stavke ≠ datum prometa serije", true,
            (db, c) => from x in Ledger(db, c)
                       join b in db.Set<InvoiceBatch>() on (int?)x.JournalEntryId equals b.JournalEntryId
                       where x.PostingDate != b.TransactionDate
                       select new ConsistencyRow(x.CompanyId, "Nalog " + x.JournalEntryId, "serija " + b.Id + ": " + x.PostingDate + " ≠ " + b.TransactionDate, null)),

        new("GK-023", "ERROR_023_GK4350_KONTOTROSKANULL",
            "4350: stavka dobavljača bez podkonta (konto troška)", true,
            (db, c) => Ledger(db, c).Where(x => x.Account == LedgerAccounts.Suppliers && EF.Property<string?>(x, "SubAccountId") == null)
                .Select(x => new ConsistencyRow(x.CompanyId, "Nalog " + x.JournalEntryId, "PA " + EF.Property<int?>(x, "PartnerAccountId"), x.DebitAmount - x.CreditAmount))),

        new("GK-024", "ERROR_024_RACUN_STORO_PROKNJIZEN_PLACEN",
            "Storniran račun čija knjiženja na 2040 nisu neutralisana", true,
            (db, c) => from x in Ledger(db, c).Where(x => x.Account == LedgerAccounts.Customers && x.InvoiceId != null)
                       join i in db.Invoices on x.InvoiceId equals (int?)i.Id
                       where i.IsCancelled
                       group x by new { x.CompanyId, i.Id, i.SequenceNumber } into g
                       where g.Sum(x => x.DebitAmount) >= 0.005m || g.Sum(x => x.DebitAmount) <= -0.005m
                             || g.Sum(x => x.CreditAmount) >= 0.005m || g.Sum(x => x.CreditAmount) <= -0.005m
                       select new ConsistencyRow(g.Key.CompanyId, "Račun " + g.Key.Id, g.Key.SequenceNumber, g.Sum(x => x.DebitAmount - x.CreditAmount))),

        new("GK-027", "ERROR_027_GK_GRP_BY_NALOG",
            "Nalog čije stavke pripadaju različitim kompanijama", true,
            (db, c) => db.LedgerEntries.AsNoTracking()
                .Where(x => c == null || db.LedgerEntries.Any(y => y.JournalEntryId == x.JournalEntryId && y.CompanyId == c))
                .GroupBy(x => x.JournalEntryId)
                .Where(g => g.Select(x => x.CompanyId).Distinct().Count() > 1)
                .Select(g => new ConsistencyRow(g.Min(x => x.CompanyId), "Nalog " + g.Key, "kompanija: " + g.Select(x => x.CompanyId).Distinct().Count(), null))),

        new("IZV-014", "ERROR_014-IZVOD-NALOG-SUMA-NIJE-NULA",
            "Nalog izvoda nije u ravnoteži (Σ D−P ≠ 0)", true,
            (db, c) => from s in db.Set<BankStatement>().AsNoTracking()
                       where (c == null || s.CompanyId == c) && s.JournalEntryId != null
                       join x in db.LedgerEntries on s.JournalEntryId equals (int?)x.JournalEntryId
                       group x by new { s.CompanyId, s.Id, s.StatementNumber, s.Date } into g
                       where g.Sum(x => x.DebitAmount - x.CreditAmount) >= 0.005m || g.Sum(x => x.DebitAmount - x.CreditAmount) <= -0.005m
                       select new ConsistencyRow(g.Key.CompanyId, "Izvod " + g.Key.Id, "br. " + g.Key.StatementNumber + " / " + g.Key.Date, g.Sum(x => x.DebitAmount - x.CreditAmount))),

        new("IZV-018", "ERROR_018_GK_PretplateIzvodi",
            "Stavka izvoda u GK sa negativnim potraživanjem (povraćaj/pretplata)", true,
            (db, c) => Ledger(db, c).Where(x => x.CreditAmount < 0 && x.LineType != null && x.LineType.IndexValue == LedgerLineTypes.BankStatement
                    && EF.Property<int?>(x, "BankStatementLineId") != null)
                .Select(x => new ConsistencyRow(x.CompanyId, "Nalog " + x.JournalEntryId, "PA " + EF.Property<int?>(x, "PartnerAccountId") + " / " + x.PostingDate, x.CreditAmount))),

        new("MAIL-022", "ERROR_022_MAILSEND_DONTHAVE_SENDDATE",
            "Mejl nije poslat, a nema ni grešku", true,
            (db, c) => db.Set<SentEmail>().AsNoTracking()
                .Where(x => (c == null || x.CompanyId == c) && x.SentAt == null && x.Status != EmailSendStatus.Failed && x.Status != EmailSendStatus.Draft && !x.IsArchived)
                .Select(x => new ConsistencyRow(x.CompanyId, "Mejl " + x.Id, x.ToAddress + " / " + x.Subject, null))),

        new("GK-103", "ERROR_103_2040_IZVOD_NEMASTAVKUIZVODA",
            "2040: stavka tipa izvod bez veze na stavku izvoda", true,
            (db, c) => Ledger(db, c).Where(x => x.Account == LedgerAccounts.Customers && x.LineType != null
                    && x.LineType.IndexValue == LedgerLineTypes.BankStatement && EF.Property<int?>(x, "BankStatementLineId") == null)
                .Select(x => new ConsistencyRow(x.CompanyId, "Nalog " + x.JournalEntryId, "PA " + EF.Property<int?>(x, "PartnerAccountId"), x.DebitAmount - x.CreditAmount))),

        new("GK-901", "ERROR_901_A_NALOG_VISE_IZVODA",
            "Jedan nalog je vezan za više izvoda", false,
            (db, c) => db.Set<BankStatement>().AsNoTracking()
                .Where(x => x.JournalEntryId != null && (c == null || x.CompanyId == c))
                .GroupBy(x => new { x.CompanyId, x.JournalEntryId })
                .Where(g => g.Count() > 1)
                .Select(g => new ConsistencyRow(g.Key.CompanyId, "Nalog " + g.Key.JournalEntryId, "izvoda: " + g.Count(), null))),

        new("GK-104", "ERROR_104_NALOG_RAVNOTEZA",
            "Nalog nije u ravnoteži (Σ D ≠ Σ P)", true,
            (db, c) => Ledger(db, c)
                .GroupBy(x => new { x.CompanyId, x.JournalEntryId })
                .Where(g => g.Sum(x => x.DebitAmount - x.CreditAmount) >= 0.005m || g.Sum(x => x.DebitAmount - x.CreditAmount) <= -0.005m)
                .Select(g => new ConsistencyRow(g.Key.CompanyId, "Nalog " + g.Key.JournalEntryId, "neravnoteža", g.Sum(x => x.DebitAmount - x.CreditAmount)))),
    ];

    public const int MaxRows = 200;

    /// <summary>Runs every check (read-only); rows are capped at <see cref="MaxRows"/>, Count is the full total.</summary>
    public static async Task<ConsistencyReport> RunAsync(SzAppDbContext db, int? companyId, string? checkId, CancellationToken ct)
    {
        var results = new List<ConsistencyCheckResult>();
        foreach (var check in All.Where(x => checkId is null || x.Id == checkId))
        {
            var query = check.Query(db, companyId);
            var count = await query.CountAsync(ct);
            var rows = count == 0 ? [] : await query.Take(MaxRows).ToArrayAsync(ct);
            results.Add(new(check.Id, check.Legacy, check.Name, check.Critical, count, rows));
        }
        return new(companyId, results, NotTranslated.Select(x => new ConsistencyNotTranslated(x.Key, x.Value)).ToArray());
    }

    /// <summary>
    /// Critical legacy checks not translated, with the reason (reported by the endpoint, not faked).
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string> NotTranslated = new Dictionary<string, string>
    {
        ["GK-902"] = "Stavka GK bez datuma: PostingDate je NOT NULL u novoj šemi (strukturno pokriveno).",
        ["GK-101/102"] = "Nepostojeći podkonto / roditelj podkonta: FK LedgerEntry.SubAccountId i SubAccount.ParentSubAccountId (strukturno pokriveno).",
        ["IZV-001"] = "Poređenje Σ stavki izvoda sa GK zahteva znak strane (odobrenje/zaduženje) po kontu izvoda — potvrditi sa vlasnicom.",
        ["IZV-009/011"] = "Pogrešna SZ na stavci izvoda: u novoj šemi CompanyId je obavezan i nasleđen od izvoda (strukturno pokriveno).",
        ["IZV-019"] = "Zavisi od legacy pomoćnih upita IzvodSumaZO/GK_2410_Sum; delimično pokriveno kroz IZV-014.",
    };
}
