using Microsoft.EntityFrameworkCore;
using SzApp.Api.Features.LedgerBanking;
using SzApp.Contracts.LedgerBanking;
using SzApp.Data;
using SzApp.Data.Entities;
using SzApp.Domain.LedgerBanking;

namespace SzApp.Api.Features.Accounting;

/// <summary>
/// FIN-33 "Preknjiži avanse" (legacy <c>GK_AutoKnjizenjeRepova</c>, customer side, account 2040):
/// one type-8 journal per partner that has an unreferenced credit and open references.
/// </summary>
public sealed class AdvanceReclassificationService(SzAppDbContext db, IJournalPostingService journals, IBusinessClock clock)
{
    private const string Account = LedgerAccounts.Customers;

    public async Task<IReadOnlyList<AdvanceReclassificationPreviewResponse>> PreviewAsync(int companyId, CancellationToken ct)
    {
        var plans = await PlanAsync(companyId, clock.Today, null, ct);
        return plans.Select(x => x.Preview).ToArray();
    }

    public async Task<AdvanceReclassificationResponse> ExecuteAsync(int companyId, AdvanceReclassificationRequest request, int staffId, CancellationToken ct)
    {
        var date = request.PostingDate ?? clock.Today;
        var plans = await PlanAsync(companyId, date, request.PartnerAccountIds, ct);
        var ids = new List<int>();
        foreach (var plan in plans)
        {
            // Legacy: one "PREKNJIZAVANJE {partner}" journal per partner; each commits on its own.
            var journal = await journals.PostLinesAsync(companyId, date, $"PREKNJIŽAVANJE {plan.Preview.AccountNumber}",
                plan.Lines, staffId, null, ct);
            ids.Add(journal.Id);
        }

        return new AdvanceReclassificationResponse(ids.Count, plans.Sum(x => x.Preview.Amount), ids);
    }

    private sealed record Plan(AdvanceReclassificationPreviewResponse Preview, IReadOnlyList<PostingLine> Lines);

    private async Task<List<Plan>> PlanAsync(int companyId, DateOnly date, IReadOnlyCollection<int>? onlyPartners, CancellationToken ct)
    {
        var posted = db.LedgerEntries.AsNoTracking()
            .Where(x => x.CompanyId == companyId && x.JournalEntry.IsPosted && x.Account == Account && EF.Property<int?>(x, "PartnerAccountId") != null);

        // Cheap SQL pre-filter: partners whose unreferenced bucket nets to a credit AND that have
        // some referenced bucket netting to a debit. Only those partners' lines are loaded.
        var buckets = await posted
            .GroupBy(x => new { PartnerAccountId = EF.Property<int?>(x, "PartnerAccountId")!.Value, Blank = x.Parameters == null || x.Parameters == "" })
            .Select(g => new { g.Key.PartnerAccountId, g.Key.Blank, Net = g.Sum(x => x.DebitAmount - x.CreditAmount) })
            .ToListAsync(ct);
        var withAdvance = buckets.Where(x => x.Blank && x.Net <= -0.005m).Select(x => x.PartnerAccountId).ToHashSet();
        var candidates = buckets.Where(x => !x.Blank && withAdvance.Contains(x.PartnerAccountId))
            .Select(x => x.PartnerAccountId).Distinct()
            .Where(x => onlyPartners is not { Count: > 0 } || onlyPartners.Contains(x))
            .ToArray();
        if (candidates.Length == 0) return [];

        var lines = await posted.Where(x => candidates.Contains(EF.Property<int?>(x, "PartnerAccountId")!.Value))
            .Select(x => new
            {
                PartnerAccountId = EF.Property<int?>(x, "PartnerAccountId")!.Value,
                Line = new ReclassSourceLine(x.Id, x.Parameters, x.PostingDate, x.DebitAmount, x.CreditAmount, x.DueDate, x.DocumentRef,
                    x.InvoiceId, x.SupplierInvoiceId, EF.Property<string?>(x, "SubAccountId"), x.CollectionPriority)
            })
            .ToListAsync(ct);
        var partners = await db.Set<PartnerAccount>().AsNoTracking().Where(x => candidates.Contains(x.Id))
            .Select(x => new { x.Id, x.AccountNumber, x.Partner.Name })
            .ToDictionaryAsync(x => x.Id, ct);

        var result = new List<Plan>();
        foreach (var group in lines.GroupBy(x => x.PartnerAccountId).OrderBy(x => partners[x.Key].AccountNumber))
        {
            var source = group.Select(x => x.Line).ToArray();
            var built = AdvanceReclassificationRules.Build(Account, group.Key, date, source);
            if (built.Count == 0) continue;
            var advance = source.Where(x => string.IsNullOrWhiteSpace(x.Reference)).Sum(x => x.Credit - x.Debit);
            var open = source.Where(x => !string.IsNullOrWhiteSpace(x.Reference)).GroupBy(x => x.Reference!.Trim())
                .Select(g => g.Sum(x => x.Debit - x.Credit)).Where(x => x > 0m).ToArray();
            var moved = built.Where(x => x.Credit > 0m).ToArray();
            result.Add(new Plan(new AdvanceReclassificationPreviewResponse(
                group.Key, partners[group.Key].AccountNumber, partners[group.Key].Name,
                advance, open.Sum(), moved.Sum(x => x.Credit), moved.Select(x => x.Parameters).Distinct().Count()), built));
        }

        return result;
    }
}
