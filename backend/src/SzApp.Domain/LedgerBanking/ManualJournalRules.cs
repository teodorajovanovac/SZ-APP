namespace SzApp.Domain.LedgerBanking;

/// <summary>A manual journal line after the partner account was looked up (null = no partner).</summary>
public sealed record ManualJournalLine(string Account, decimal Debit, decimal Credit, int? PartnerAccountId, string? PartnerAccountAccount);

/// <summary>
/// GAP-13 manual journal (legacy U25). On top of <see cref="LedgerBankingRules.ValidateJournal"/>
/// (one side per line, balanced): partner-kept accounts (2040 customers, 4350 suppliers, 9.2) need a
/// partner, and a partner account may only be used on its own ledger account.
/// </summary>
public static class ManualJournalRules
{
    public static readonly IReadOnlySet<string> PartnerAccounts = new HashSet<string>(StringComparer.Ordinal)
    {
        LedgerAccounts.Customers,
        LedgerAccounts.Suppliers
    };

    public static decimal Validate(IReadOnlyList<ManualJournalLine> lines)
    {
        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            var row = i + 1;
            if (string.IsNullOrWhiteSpace(line.Account))
            {
                throw new DomainRuleException("journal.account-required", $"Stavka {row}: konto je obavezan.");
            }

            var account = line.Account.Trim();
            if (line.PartnerAccountId is null && PartnerAccounts.Contains(account))
            {
                throw new DomainRuleException("journal.partner-required", $"Stavka {row}: konto {account} se vodi po partneru — izaberite partnera.");
            }

            if (line.PartnerAccountId is not null && line.PartnerAccountAccount is { } own && !string.Equals(own.Trim(), account, StringComparison.Ordinal))
            {
                throw new DomainRuleException("journal.partner-account-mismatch", $"Stavka {row}: partner je otvoren na kontu {own}, a ne na {account}.");
            }
        }

        return LedgerBankingRules.ValidateJournal(lines.Select(x => new PostingAmounts(x.Debit, x.Credit)));
    }
}
