namespace SzApp.Domain.Billing;

/// <summary>One posted 2040 (customer) ledger line, as fetched for FIN-12's notice computation.</summary>
public sealed record NoticeGlLine(
    int PartnerAccountId,
    int AccountNumber,
    string? DocumentRef,
    int? InvoiceId,
    decimal Debit,
    decimal Credit,
    DateOnly PostingDate,
    DateOnly? DueDate,
    string? Description);

/// <summary>One (DocumentRef, InvoiceId) group that counted toward a notice's debt (9.5 §3-5).</summary>
public sealed record NoticeLineGroup(
    string? DocumentRef,
    int? InvoiceId,
    decimal Debit,
    decimal Credit,
    decimal Sum,
    DateOnly? DueDate,
    string? Description);

/// <summary>One partner that qualifies for a notice, with its computed debt and counted line groups.</summary>
public sealed record NoticeCandidate(
    int PartnerAccountId,
    int AccountNumber,
    decimal Debt,
    IReadOnlyList<NoticeLineGroup> Lines);

/// <summary>
/// FIN-12: opomene computed straight from the general ledger (9.5 GrupaOpomena_Add), replacing the
/// legacy client-supplied debt/lines. Pure function over 2040 lines already fetched for one company --
/// the caller (BillingService) does the EF query and PartnerAccount/company scoping.
/// </summary>
public static class NoticeGenerator
{
    /// <summary>
    /// 9.5 §1: GL filter by DATUM (posting date), not due date -- the legacy DatumPI (payment cutoff)
    /// and DatumDI (debt cutoff). Document types are not excluded.
    /// </summary>
    public static bool PassesDateFilter(decimal debit, decimal credit, DateOnly postingDate, DateOnly paymentCutoff, DateOnly debtCutoff) =>
        (credit != 0m && postingDate <= paymentCutoff)
        || (debit <= 0m && postingDate <= paymentCutoff)
        || (debit != 0m && postingDate <= debtCutoff);

    /// <summary>
    /// 9.5 §2-5 per partner: an overall balance above <paramref name="debtTolerance"/>, grouped by
    /// (DocumentRef, InvoiceId) with only groups above <paramref name="debtToleranceByMonth"/> counted,
    /// at least <paramref name="minLineCount"/> such groups, debt = sum of the counted groups (legacy
    /// does not net out overpayments on other documents).
    /// </summary>
    public static IReadOnlyList<NoticeCandidate> Generate(
        IEnumerable<NoticeGlLine> lines,
        DateOnly paymentCutoff,
        DateOnly debtCutoff,
        decimal debtTolerance,
        decimal debtToleranceByMonth,
        int minLineCount)
    {
        var filtered = lines.Where(x => PassesDateFilter(x.Debit, x.Credit, x.PostingDate, paymentCutoff, debtCutoff));

        var result = new List<NoticeCandidate>();
        foreach (var byPartner in filtered.GroupBy(x => x.PartnerAccountId))
        {
            var partnerLines = byPartner.ToArray();

            // §2: overall 2040 balance must exceed the debtor tolerance.
            var balance = partnerLines.Sum(x => FinanceRounding.Money(x.Debit - x.Credit));
            if (balance <= debtTolerance) continue;

            // §3: group by (DocumentRef, InvoiceId); only groups above the per-document tolerance count.
            var groups = partnerLines
                .GroupBy(x => (x.DocumentRef, x.InvoiceId))
                .Select(g => new NoticeLineGroup(
                    g.Key.DocumentRef,
                    g.Key.InvoiceId,
                    FinanceRounding.Money(g.Sum(x => x.Debit)),
                    FinanceRounding.Money(g.Sum(x => x.Credit)),
                    FinanceRounding.Money(g.Sum(x => x.Debit - x.Credit)),
                    g.Max(x => x.DueDate),
                    g.Select(x => x.Description).FirstOrDefault(d => !string.IsNullOrWhiteSpace(d))))
                .Where(g => g.Sum > debtToleranceByMonth)
                .ToArray();

            // §4: minimum number of counted groups.
            if (groups.Length < minLineCount) continue;

            // §5: Dug = sum of the counted groups only.
            var debt = FinanceRounding.Money(groups.Sum(x => x.Sum));
            var first = partnerLines[0];
            result.Add(new NoticeCandidate(first.PartnerAccountId, first.AccountNumber, debt, groups));
        }
        return result;
    }
}
