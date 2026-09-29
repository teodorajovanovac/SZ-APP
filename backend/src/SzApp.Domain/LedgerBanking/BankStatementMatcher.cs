using System.Globalization;
using SzApp.Domain.LedgerBanking.StatementParsing;

namespace SzApp.Domain.LedgerBanking;

/// <summary>How the partner of a statement line was found (stored on the line).</summary>
public static class MatchSources
{
    public const string Template = "Template";
    public const string BankAccount = "BankAccount";
    public const string Reference = "Reference";
    public const string Manual = "Manual";
}

/// <summary>What one allocation (proposal part) of a statement line is.</summary>
public static class AllocationKinds
{
    public const string Reference = "Reference"; // 9.3 step 3: open items of the payment reference
    public const string Fifo = "Fifo";           // step 4: oldest open references of the partner
    public const string Advance = "Advance";     // step 5: surplus, no reference (avans)
    public const string Manual = "Manual";
}

public enum TemplateMatch
{
    Equals = 1,
    StartsWith = 2,
    Contains = 3
}

public sealed record TemplateCondition(string Field, TemplateMatch Function, string Value);

/// <summary>Legacy TemplateIzvodaKnjizenje: all conditions must hold; result is a PARTNER (GAP-05), not an account.</summary>
public sealed record MatchTemplate(int Id, string Name, int PartnerAccountId, string? SubAccountId, IReadOnlyList<TemplateCondition> Conditions);

public sealed record StatementLineFacts(
    string PayerName,
    string? PayerAccount,
    string? Info,
    int? Code,
    string? PaymentReference,
    decimal Debit,
    decimal Credit)
{
    public bool IsInflow => Credit > 0m;
    public decimal Amount => Credit > 0m ? Credit : Debit;
}

/// <summary>
/// One open group on a partner's account (legacy GROUP BY PRIORITET, SIFRAKN, PARAMETRI, DOK,
/// KontoTroska, RDOB, RACID). Balance = Σ(debit − credit); mutated as lines are allocated so later
/// lines of the same statement see what earlier ones took (legacy posted each line immediately).
/// </summary>
public sealed class OpenItem
{
    public required int PartnerAccountId { get; init; }
    public string? Parameters { get; init; }
    public string? DocumentRef { get; init; }
    public int? InvoiceId { get; init; }
    public int? SupplierInvoiceId { get; init; }
    public int? CollectionPriority { get; init; }
    public string? SubAccountId { get; init; }
    /// <summary>Legacy TIP_STAVKE of the item -> KNzaTIP of the payment (3 invoice, 4 supplier invoice).</summary>
    public int? LineTypeCode { get; init; }
    public DateOnly FirstDate { get; init; }
    public long FirstId { get; init; }
    public decimal Balance { get; set; }
}

public sealed record AllocationProposal(
    string Account,
    int? PartnerAccountId,
    decimal Amount,
    string Kind,
    string? SubAccountId = null,
    string? Parameters = null,
    string? DocumentRef = null,
    int? InvoiceId = null,
    int? SupplierInvoiceId = null,
    int? CollectionPriority = null,
    int? ClosesDocumentType = null);

/// <param name="IncludeOppositeDirectionGroups">
/// Legacy GK_KnjizenjePoParametrimaRacuna takes every group of the reference with balance ≠ 0, so an
/// overpaid group (opposite sign) is "taken" negatively and its surplus is re-spread onto the other
/// groups -- producing negative allocation parts. On = legacy (owner default); off = only open
/// groups in the payment's direction are touched.
/// </param>
public sealed record MatcherOptions(bool IncludeOppositeDirectionGroups = true);

public static class BankStatementMatcher
{
    /// <summary>Legacy GK_TemplateKnjizenja: first template whose every condition matches.</summary>
    public static MatchTemplate? FindTemplate(StatementLineFacts line, IEnumerable<MatchTemplate> templates) =>
        templates.FirstOrDefault(t => t.Conditions.Count > 0 && t.Conditions.All(c => Matches(line, c)));

    private static bool Matches(StatementLineFacts line, TemplateCondition condition)
    {
        var field = condition.Field.Trim().ToUpperInvariant();
        var isAccount = field is "BROJRACUNA" or "BANKACCOUNTNUMBER";
        var actual = field switch
        {
            "BROJRACUNA" or "BANKACCOUNTNUMBER" => StatementText.NormalizeAccount(line.PayerAccount),
            "DOZNAKA" or "INFO" => line.Info,
            "ODOBRENJE" or "CREDIT" => line.Credit.ToString(CultureInfo.InvariantCulture),
            "ZADUZENJE" or "DEBIT" => line.Debit.ToString(CultureInfo.InvariantCulture),
            "SIFRA" or "CODE" => (line.Code ?? 0).ToString(CultureInfo.InvariantCulture),
            "NAZIVPN" or "PAYERRECIPIENTNAME" => line.PayerName,
            "POZIVNABROJ" or "PAYMENTREFERENCE" => line.PaymentReference,
            _ => null
        };
        if (actual is null) return false;

        var expected = isAccount ? StatementText.NormalizeAccount(condition.Value) ?? string.Empty : condition.Value;
        actual = actual.Trim();
        expected = expected.Trim();
        if (condition.Function == TemplateMatch.Equals &&
            decimal.TryParse(actual, NumberStyles.Number, CultureInfo.InvariantCulture, out var a) &&
            decimal.TryParse(expected.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out var e))
        {
            return a == e;
        }

        // Access "Option Compare Database" = case-insensitive text comparison.
        return condition.Function switch
        {
            TemplateMatch.StartsWith => actual.StartsWith(expected, StringComparison.OrdinalIgnoreCase),
            TemplateMatch.Contains => actual.Contains(expected, StringComparison.OrdinalIgnoreCase),
            _ => string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase)
        };
    }

    /// <summary>
    /// 9.3 steps 3–5 (legacy GK_StavkaIzvoda) for one line whose partner is known:
    /// (3) the line's payment reference, its open groups ordered by (date, collection priority, id);
    /// (4) remainder FIFO over the partner's other open references (oldest first);
    /// (5) surplus as an advance without reference. Inflow closes debit balances (2040), outflow
    /// closes credit balances (4350). Amounts are in the line's direction and sum to the line amount.
    /// </summary>
    public static IReadOnlyList<AllocationProposal> Allocate(
        StatementLineFacts line,
        int partnerAccountId,
        string account,
        IReadOnlyCollection<OpenItem> openItems,
        MatcherOptions options,
        string? advanceSubAccountId = null)
    {
        var sign = line.IsInflow ? 1m : -1m;
        var remaining = FinanceRounding.Money(line.Amount);
        var result = new List<AllocationProposal>();
        var items = openItems.Where(x => x.PartnerAccountId == partnerAccountId).ToArray();

        decimal Open(OpenItem x) => FinanceRounding.Money(sign * x.Balance);

        void Spread(string reference, string kind)
        {
            var groups = items
                .Where(x => x.Parameters == reference)
                .OrderBy(x => x.FirstDate).ThenBy(x => x.CollectionPriority ?? int.MinValue).ThenBy(x => x.FirstId);
            foreach (var group in groups)
            {
                if (remaining == 0m) return;
                var open = Open(group);
                if (open == 0m || (open < 0m && !options.IncludeOppositeDirectionGroups)) continue;
                var take = remaining >= open ? open : remaining;
                group.Balance -= sign * take;
                remaining -= take;
                result.Add(new AllocationProposal(account, partnerAccountId, take, kind,
                    group.SubAccountId, group.Parameters, group.DocumentRef, group.InvoiceId,
                    group.SupplierInvoiceId, group.CollectionPriority, group.LineTypeCode));
            }
        }

        decimal NetOpen(string reference) => items.Where(x => x.Parameters == reference).Sum(Open);

        if (!string.IsNullOrEmpty(line.PaymentReference) && NetOpen(line.PaymentReference) > 0m)
        {
            Spread(line.PaymentReference, AllocationKinds.Reference);
        }

        if (remaining > 0m)
        {
            var references = items
                .Where(x => !string.IsNullOrEmpty(x.Parameters))
                .GroupBy(x => x.Parameters!)
                .OrderBy(g => g.Min(x => x.FirstDate)).ThenBy(g => g.Min(x => x.FirstId))
                .Select(g => g.Key)
                .ToArray();
            foreach (var reference in references)
            {
                if (remaining == 0m) break;
                if (NetOpen(reference) > 0m) Spread(reference, AllocationKinds.Fifo);
            }
        }

        if (remaining != 0m)
        {
            result.Add(new AllocationProposal(account, partnerAccountId, remaining, AllocationKinds.Advance, advanceSubAccountId));
        }

        return result;
    }

    /// <summary>"Sigurno" for Ctrl+Enter: template hit, or the whole amount closed by the line's own reference, no conflict.</summary>
    public static bool IsConfident(string? matchSource, bool referenceConflict, IReadOnlyCollection<AllocationProposal> proposals) =>
        !referenceConflict && proposals.Count > 0 &&
        (matchSource == MatchSources.Template || proposals.All(x => x.Kind == AllocationKinds.Reference));
}
