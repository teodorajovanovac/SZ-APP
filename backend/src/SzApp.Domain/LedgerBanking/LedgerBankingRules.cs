namespace SzApp.Domain.LedgerBanking;

public sealed record PostingAmounts(decimal Debit, decimal Credit);

public sealed record StatementTotals(
    decimal PreviousBalance,
    decimal Debit,
    decimal Credit,
    decimal NewBalance,
    int DebitCount,
    int CreditCount);

public static class LedgerBankingRules
{
    public static decimal ValidateJournal(IEnumerable<PostingAmounts> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);
        var entries = lines.ToArray();
        if (entries.Length == 0)
        {
            throw new DomainRuleException("journal.empty", "Nalog mora sadržati najmanje jednu stavku.");
        }

        if (entries.Any(x => x.Debit < 0m || x.Credit < 0m ||
                             (x.Debit == 0m && x.Credit == 0m) ||
                             (x.Debit > 0m && x.Credit > 0m)))
        {
            throw new DomainRuleException("journal.invalid-line", "Svaka stavka mora imati pozitivan iznos na tačno jednoj strani.");
        }

        var debit = FinanceRounding.Money(entries.Sum(x => x.Debit));
        var credit = FinanceRounding.Money(entries.Sum(x => x.Credit));
        if (debit != credit)
        {
            throw new DomainRuleException("journal.unbalanced", $"Nalog nije uravnotežen: duguje {debit:0.00}, potražuje {credit:0.00}.");
        }

        return debit;
    }

    public static StatementTotals ReconcileStatement(
        decimal previousBalance,
        decimal declaredDebit,
        decimal declaredCredit,
        decimal newBalance,
        IEnumerable<PostingAmounts> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);
        var entries = lines.ToArray();
        if (entries.Length == 0)
        {
            throw new DomainRuleException("statement.empty", "Izvod mora sadržati najmanje jednu stavku.");
        }

        if (entries.Any(x => x.Debit < 0m || x.Credit < 0m ||
                             (x.Debit == 0m && x.Credit == 0m) ||
                             (x.Debit > 0m && x.Credit > 0m)))
        {
            throw new DomainRuleException("statement.invalid-line", "Stavka izvoda mora imati pozitivan iznos na tačno jednoj strani.");
        }

        var calculatedDebit = FinanceRounding.Money(entries.Sum(x => x.Debit));
        var calculatedCredit = FinanceRounding.Money(entries.Sum(x => x.Credit));
        var roundedPrevious = FinanceRounding.Money(previousBalance);
        var roundedNew = FinanceRounding.Money(newBalance);

        if (calculatedDebit != FinanceRounding.Money(declaredDebit) ||
            calculatedCredit != FinanceRounding.Money(declaredCredit))
        {
            throw new DomainRuleException("statement.header-lines-mismatch", "Zbir stavki izvoda ne odgovara zaglavlju.");
        }

        if (FinanceRounding.Money(roundedPrevious + calculatedCredit - calculatedDebit) != roundedNew)
        {
            throw new DomainRuleException("statement.balance-mismatch", "Prethodno stanje i promet ne daju novo stanje izvoda.");
        }

        return new StatementTotals(
            roundedPrevious,
            calculatedDebit,
            calculatedCredit,
            roundedNew,
            entries.Count(x => x.Debit > 0m),
            entries.Count(x => x.Credit > 0m));
    }
}

public static class PostingSchemeRegistry
{
    private static readonly HashSet<string> AllowedSourceTypes = new(StringComparer.Ordinal)
    {
        "InvoiceBatch",
        "SupplierInvoice",
        "BankStatement",
        "InterestStatement",
        "Notice"
    };

    public static void EnsureAllowedSource(string sourceType)
    {
        if (!AllowedSourceTypes.Contains(sourceType))
        {
            throw new DomainRuleException("posting.source-not-allowed", $"Izvor knjiženja '{sourceType}' nije dozvoljen.");
        }
    }
}
