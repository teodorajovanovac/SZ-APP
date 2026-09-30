namespace SzApp.Domain.LedgerBanking;

/// <summary>One posted line of a partner's account (2040), as the reclassification sees it.</summary>
public sealed record ReclassSourceLine(
    int Id,
    string? Reference,
    DateOnly PostingDate,
    decimal Debit,
    decimal Credit,
    DateOnly? DueDate = null,
    string? DocumentRef = null,
    int? InvoiceId = null,
    int? SupplierInvoiceId = null,
    string? SubAccountId = null,
    int? CollectionPriority = null);

/// <summary>
/// FIN-33, legacy <c>GK_AutoKnjizenjeRepova</c> (customer side). A partner that has an advance
/// (unreferenced credit, the "" payment-reference bucket nets to a credit) and open references
/// (buckets that net to a debit) gets a type-8 journal on the same account:
/// <list type="bullet">
/// <item>red credit (negative, same side, P9) on the consumed advance lines, newest first
/// (legacy <c>ORDER BY Datum DESC</c>), copying the advance line's links;</item>
/// <item>positive credit on the open references, oldest first (<c>FirstOfDATUM</c>), copying the
/// reference's first debit line (invoice, supplier invoice, sub-account, priority); the line's due
/// date is the consumed advance line's date (legacy DPO = mDatum).</item>
/// </list>
/// Amount moved = min(advance, Σ open). Credits net to zero, debits are zero → always balanced.
/// </summary>
public static class AdvanceReclassificationRules
{
    public static IReadOnlyList<PostingLine> Build(
        string account,
        int partnerAccountId,
        DateOnly postingDate,
        IEnumerable<ReclassSourceLine> lines)
    {
        var all = lines.ToArray();
        var blank = all.Where(x => string.IsNullOrWhiteSpace(x.Reference)).ToArray();
        var advance = FinanceRounding.Money(blank.Sum(x => x.Credit - x.Debit));
        if (advance <= 0m) return [];

        var open = all.Where(x => !string.IsNullOrWhiteSpace(x.Reference))
            .GroupBy(x => x.Reference!.Trim(), StringComparer.Ordinal)
            .Select(g => new
            {
                Balance = FinanceRounding.Money(g.Sum(x => x.Debit - x.Credit)),
                First = g.Min(x => x.PostingDate),
                Key = g.Key,
                Template = g.Where(x => x.Debit > 0m).OrderBy(x => x.PostingDate).ThenBy(x => x.Id).FirstOrDefault()
                           ?? g.OrderBy(x => x.PostingDate).ThenBy(x => x.Id).First()
            })
            .Where(x => x.Balance > 0m)
            .OrderBy(x => x.First).ThenBy(x => x.Key, StringComparer.Ordinal)
            .ToArray();
        var amount = Math.Min(advance, open.Sum(x => x.Balance));
        if (amount <= 0m) return [];

        var result = new List<PostingLine>();
        var refIndex = 0;
        var refLeft = open.Length > 0 ? open[0].Balance : 0m;
        var remaining = amount;
        foreach (var source in blank.Where(x => x.Credit > 0m).OrderByDescending(x => x.PostingDate).ThenByDescending(x => x.Id))
        {
            if (remaining == 0m) break;
            var take = Math.Min(source.Credit, remaining);
            remaining -= take;
            result.Add(new PostingLine(account, 0m, -take, LedgerLineTypes.Rebooking, postingDate,
                PartnerAccountId: partnerAccountId, SubAccountId: source.SubAccountId, DueDate: source.DueDate,
                DocumentRef: source.DocumentRef, InvoiceId: source.InvoiceId, SupplierInvoiceId: source.SupplierInvoiceId,
                CollectionPriority: source.CollectionPriority, Description: "PREKNJIŽAVANJE AVANSA"));

            // Spread this advance line's share over the open references (oldest first).
            while (take > 0m)
            {
                var piece = Math.Min(take, refLeft);
                var target = open[refIndex];
                result.Add(new PostingLine(account, 0m, piece, LedgerLineTypes.Rebooking, postingDate,
                    PartnerAccountId: partnerAccountId, SubAccountId: target.Template.SubAccountId, DueDate: source.PostingDate,
                    DocumentRef: target.Template.DocumentRef, Parameters: target.Key, InvoiceId: target.Template.InvoiceId,
                    SupplierInvoiceId: target.Template.SupplierInvoiceId, CollectionPriority: target.Template.CollectionPriority,
                    Description: "PREKNJIŽAVANJE AVANSA"));
                take -= piece;
                refLeft -= piece;
                if (refLeft == 0m && ++refIndex < open.Length) refLeft = open[refIndex].Balance;
            }
        }

        return result;
    }
}
