using SzApp.Domain.LedgerBanking;

namespace SzApp.Domain.Billing;

/// <summary>One notice's cost to post: the partner's 2040 account and the notice number/reference.</summary>
public sealed record NoticeCostPostingSource(
    int NoticeId,
    int PartnerAccountId,
    string CustomerAccount,
    string NoticeNumber,
    string PaymentReference,
    decimal Cost);

/// <summary>
/// Optional notice-cost posting (P11; whether costs are posted at all is still an open owner
/// question, so this is an explicit action): 2040 D per partner (line type 3, DocumentRef = notice
/// number, Parameters = notice payment reference so the payment closes it) and one 4900 P total.
/// Zero costs are skipped. Storno = <see cref="DocumentPostingRules.Negate"/> (P9 red storno).
/// </summary>
public static class NoticeCostPostingRules
{
    public static IReadOnlyList<PostingLine> Build(DateOnly postingDate, DateOnly dueDate, string batchRef, IEnumerable<NoticeCostPostingSource> sources)
    {
        var customer = sources
            .Where(x => FinanceRounding.Money(x.Cost) != 0m)
            .OrderBy(x => x.NoticeId)
            .Select(x => new PostingLine(
                x.CustomerAccount, FinanceRounding.Money(x.Cost), 0m, LedgerLineTypes.Invoice, postingDate,
                PartnerAccountId: x.PartnerAccountId,
                DueDate: dueDate,
                DocumentRef: x.NoticeNumber,
                Parameters: DocumentPostingRules.CleanPaymentReference(x.PaymentReference),
                Note: "Troškovi opomene"))
            .ToArray();
        if (customer.Length == 0)
            throw new DomainRuleException("notice.costs-zero", "Serija nema troškove opomena za knjiženje.");

        var revenue = new PostingLine(LedgerAccounts.Revenue, 0m, customer.Sum(x => x.Debit), LedgerLineTypes.Invoice, postingDate,
            DocumentRef: batchRef, Note: "Troškovi opomena");
        return [.. customer, revenue];
    }
}
