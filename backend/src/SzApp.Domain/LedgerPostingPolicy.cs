namespace SzApp.Domain;

public sealed record LedgerPostingLine(decimal DebitAmount, decimal CreditAmount);

public static class LedgerPostingPolicy
{
    public static decimal ValidateAndGetBalance(IEnumerable<LedgerPostingLine> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);
        var materialized = lines.ToArray();

        if (materialized.Length == 0)
        {
            throw new DomainRuleException("journal.empty", "Nalog mora sadržati najmanje jednu stavku.");
        }

        if (materialized.Any(x => x.DebitAmount < 0m || x.CreditAmount < 0m ||
                                  (x.DebitAmount > 0m && x.CreditAmount > 0m)))
        {
            throw new DomainRuleException("journal.invalid-line", "Stavka mora imati nenegativan iznos samo na jednoj strani.");
        }

        var debit = FinanceRounding.Calculation(materialized.Sum(x => x.DebitAmount));
        var credit = FinanceRounding.Calculation(materialized.Sum(x => x.CreditAmount));

        if (debit != credit)
        {
            throw new DomainRuleException("journal.unbalanced", $"Nalog nije uravnotežen: duguje {debit:0.0000}, potražuje {credit:0.0000}.");
        }

        return debit;
    }
}
