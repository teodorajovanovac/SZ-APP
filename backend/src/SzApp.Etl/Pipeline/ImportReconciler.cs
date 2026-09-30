using Microsoft.EntityFrameworkCore;
using SzApp.Data;
using SzApp.Data.Entities;
using SzApp.Data.Entities.Billing;
using SzApp.Data.Entities.EtlExtended;
using SzApp.Data.Entities.LedgerBanking;

namespace SzApp.Etl.Pipeline;

/// <summary>
/// ETL-08: after a financial table lands, per company: trial balance (total and by account),
/// journals with D ≠ P, the 2040 customer balance against the source file, GL 2410 against the
/// latest bank statement closing balance, and invoice total vs 2040 debit per invoice batch.
/// </summary>
public static class ImportReconciler
{
    public static readonly HashSet<string> FinancialTables = new(StringComparer.OrdinalIgnoreCase)
    {
        "JournalEntry", "LedgerEntry", "BankStatement", "BankStatementLine", "Invoice", "InvoiceBatch"
    };

    private const int DetailLimit = 200;

    public static async Task<List<ReconciliationRecord>> ReconcileAsync(
        SzAppDbContext db, Guid runId, IEnumerable<int> companyIds, IReadOnlyDictionary<int, decimal>? source2040,
        DateTimeOffset now, CancellationToken ct)
    {
        var records = new List<ReconciliationRecord>();
        void Add(ReconciliationMetric metric, string scope, decimal expected, decimal actual, bool? match, string? details = null) =>
            records.Add(new ReconciliationRecord
            {
                EtlRunId = runId, Metric = metric, Scope = scope.Length > 128 ? scope[..128] : scope,
                ExpectedValue = expected, ActualValue = actual, IsMatch = match, Details = details, CheckedAt = now
            });

        foreach (var companyId in companyIds.Distinct().Order())
        {
            var c = $"C{companyId}";
            var lines = db.LedgerEntries.AsNoTracking().Where(x => x.CompanyId == companyId);
            var byAccount = await lines.GroupBy(x => x.Account)
                .Select(g => new { Account = g.Key, D = g.Sum(x => x.DebitAmount), P = g.Sum(x => x.CreditAmount) })
                .OrderBy(x => x.Account).ToArrayAsync(ct);
            var totalD = byAccount.Sum(x => x.D);
            var totalP = byAccount.Sum(x => x.P);
            Add(ReconciliationMetric.TrialBalance, c, totalD, totalP, totalD == totalP, $"Bruto bilans: D={totalD:N2} P={totalP:N2}");
            foreach (var account in byAccount.Take(DetailLimit))
            {
                Add(ReconciliationMetric.AccountBalance, $"{c}:{account.Account}", account.D, account.P, null,
                    $"Saldo {account.D - account.P:N2}");
            }

            var unbalanced = await lines.GroupBy(x => x.JournalEntryId)
                .Select(g => new { Id = g.Key, D = g.Sum(x => x.DebitAmount), P = g.Sum(x => x.CreditAmount) })
                .Where(x => x.D != x.P).OrderBy(x => x.Id).ToArrayAsync(ct);
            Add(ReconciliationMetric.JournalImbalance, c, 0, unbalanced.Length, unbalanced.Length == 0,
                unbalanced.Length == 0 ? "Svi nalozi D = P." : $"Neuravnoteženih naloga: {unbalanced.Length}");
            foreach (var journal in unbalanced.Take(DetailLimit))
            {
                Add(ReconciliationMetric.JournalImbalance, $"{c}:N{journal.Id}", journal.D, journal.P, false,
                    $"Nalog {journal.Id}: razlika {journal.D - journal.P:N2}");
            }

            var customers = await lines.Where(x => x.Account == "2040")
                .GroupBy(x => EF.Property<int?>(x, "PartnerAccountId"))
                .Select(g => new { g.Key, Balance = g.Sum(x => x.DebitAmount - x.CreditAmount) })
                .ToArrayAsync(ct);
            var customerBalance = customers.Sum(x => x.Balance);
            var hasSource = source2040?.ContainsKey(companyId) == true;
            var expected2040 = hasSource ? source2040![companyId] : customerBalance;
            Add(ReconciliationMetric.PartnerBalance, $"{c}:2040", expected2040, customerBalance,
                hasSource ? expected2040 == customerBalance : null,
                $"Kupaca sa saldom ≠ 0: {customers.Count(x => x.Balance != 0)}, bez partnera: {customers.Where(x => x.Key == null).Sum(x => x.Balance):N2}");

            var gl2410 = await lines.Where(x => x.Account == "2410").SumAsync(x => (decimal?)(x.DebitAmount - x.CreditAmount), ct) ?? 0m;
            var closing = await db.Set<BankStatement>().AsNoTracking().Where(x => x.CompanyId == companyId)
                .GroupBy(x => x.BankAccountId)
                .Select(g => g.OrderByDescending(x => x.Date).ThenByDescending(x => x.StatementNumber).Select(x => x.NewBalance).First())
                .ToArrayAsync(ct);
            Add(ReconciliationMetric.BankLedgerBalance, $"{c}:2410", closing.Sum(), gl2410,
                closing.Length == 0 ? null : closing.Sum() == gl2410,
                $"Poslednje stanje izvoda ({closing.Length} rač.) vs saldo 2410 u GK");

            var batches = await db.Set<InvoiceBatch>().AsNoTracking().Where(x => x.CompanyId == companyId)
                .OrderBy(x => x.PeriodYYMM).Take(DetailLimit)
                .Select(b => new
                {
                    b.Id, b.PeriodYYMM, b.JournalEntryId,
                    Invoiced = db.Invoices.Where(i => i.CompanyId == companyId && EF.Property<int?>(i, "InvoiceBatchId") == b.Id)
                        .Sum(i => (decimal?)i.InvoiceTotal) ?? 0m,
                    Posted = db.LedgerEntries.Where(l => l.CompanyId == companyId && l.JournalEntryId == b.JournalEntryId && l.Account == "2040")
                        .Sum(l => (decimal?)l.DebitAmount) ?? 0m
                })
                .ToArrayAsync(ct);
            foreach (var batch in batches)
            {
                Add(ReconciliationMetric.InvoiceTotal, $"{c}:B{batch.Id}", batch.Invoiced, batch.Posted,
                    batch.JournalEntryId is null ? null : batch.Invoiced == batch.Posted,
                    $"Serija {batch.PeriodYYMM}: računi {batch.Invoiced:N2}, 2040 D {batch.Posted:N2}");
            }
        }

        return records;
    }
}
