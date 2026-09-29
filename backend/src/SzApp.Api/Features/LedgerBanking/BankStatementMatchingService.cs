using Microsoft.EntityFrameworkCore;
using SzApp.Data;
using SzApp.Data.Entities;
using SzApp.Data.Entities.LedgerBanking;
using SzApp.Domain;
using SzApp.Domain.LedgerBanking;
using SzApp.Domain.LedgerBanking.StatementParsing;

namespace SzApp.Api.Features.LedgerBanking;

/// <summary>
/// GAP-05 / 9.3 auto-matching: builds allocation PROPOSALS per statement line right after import.
/// Nothing is posted here; the user accepts (Enter / Ctrl+Enter) or edits the split.
/// </summary>
public sealed class BankStatementMatchingService(SzAppDbContext db)
{
    public static readonly MatcherOptions Options = new(IncludeOppositeDirectionGroups: true);

    private readonly Dictionary<(int PartnerAccountId, string Account), List<OpenItem>> _openItems = new();

    /// <summary>Proposes allocations for every line that is not yet accepted. Caller saves.</summary>
    public async Task MatchStatementAsync(BankStatement statement, CancellationToken ct)
    {
        var templates = await LoadTemplatesAsync(statement.CompanyId, ct);
        var payerAccounts = await LoadPayerAccountsAsync(statement.CompanyId, ct);
        foreach (var line in statement.Lines.OrderBy(x => x.LineNumber))
        {
            if (line.Status is BankStatementLineStatus.Matched or BankStatementLineStatus.Posted) continue;
            var facts = Facts(line);
            int? partnerAccountId = null;
            string? subAccountId = null;
            string? source = null;

            // 9.3 step 1: templates -> partner (GAP-05).
            if (BankStatementMatcher.FindTemplate(facts, templates) is { } template)
            {
                (partnerAccountId, subAccountId, source) = (template.PartnerAccountId, template.SubAccountId, MatchSources.Template);
            }
            // step 2: partner by the payer's bank account.
            else if (StatementText.NormalizeAccount(line.BankAccountNumber) is { } payer && payerAccounts.TryGetValue(payer, out var partnerId))
            {
                partnerAccountId = await PickPartnerAccountAsync(statement.CompanyId, partnerId, facts.IsInflow, ct);
                source = partnerAccountId is null ? null : MatchSources.BankAccount;
            }

            // Legacy GK_StavkaIzvoda: the payment reference's owner wins; owner decision: never
            // silently -- the conflict is flagged on the line and it is not "confident".
            string? note = null;
            var conflict = false;
            if (await FindReferenceOwnerAsync(statement.CompanyId, line.PaymentReference, facts.IsInflow, ct) is { } owner)
            {
                if (partnerAccountId is null)
                {
                    (partnerAccountId, source) = (owner, MatchSources.Reference);
                }
                else if (partnerAccountId != owner)
                {
                    note = $"Poziv na broj {line.PaymentReference} pripada partneru {await PartnerLabelAsync(owner, ct)}, a {SourceLabel(source)} daje {await PartnerLabelAsync(partnerAccountId.Value, ct)}. Predlog je po pozivu na broj — proverite.";
                    (partnerAccountId, source, conflict) = (owner, MatchSources.Reference, true);
                }
            }

            await ProposeAsync(line, partnerAccountId, subAccountId, source, note, conflict, ct);
        }
    }

    /// <summary>Re-proposes one line for a partner the user picked (legacy manual partner choice).</summary>
    public async Task ProposeForPartnerAsync(BankStatementLine line, int partnerAccountId, CancellationToken ct)
    {
        var template = BankStatementMatcher.FindTemplate(Facts(line), await LoadTemplatesAsync(line.CompanyId, ct));
        var subAccountId = template?.PartnerAccountId == partnerAccountId ? template.SubAccountId : null;
        await ProposeAsync(line, partnerAccountId, subAccountId, MatchSources.Manual, null, false, ct);
    }

    private async Task ProposeAsync(BankStatementLine line, int? partnerAccountId, string? subAccountId, string? source, string? note, bool conflict, CancellationToken ct)
    {
        foreach (var old in line.Allocations.ToArray())
        {
            line.Allocations.Remove(old);
            db.Remove(old);
        }

        line.Status = BankStatementLineStatus.Pending;
        line.MatchSource = source;
        line.MatchNote = note;
        line.IsConfidentMatch = false;
        if (partnerAccountId is not { } paId)
        {
            line.MatchNote = "Partner nije pronađen — izaberite partnera ili konto.";
            return;
        }

        var account = await db.Set<PartnerAccount>().AsNoTracking().Where(x => x.Id == paId).Select(x => x.Account).SingleAsync(ct);
        var facts = Facts(line);
        var proposals = BankStatementMatcher.Allocate(facts, paId, account, await OpenItemsAsync(line.CompanyId, paId, account, ct), Options, subAccountId);
        var index = 0;
        foreach (var p in proposals)
        {
            line.Allocations.Add(ToEntity(line, p, ++index));
        }

        line.IsConfidentMatch = BankStatementMatcher.IsConfident(source, conflict, proposals);
    }

    internal static BankStatementLineAllocation ToEntity(BankStatementLine line, AllocationProposal p, int sortIndex) => new()
    {
        CompanyId = line.CompanyId,
        SortIndex = sortIndex,
        Account = p.Account,
        PartnerAccountId = p.PartnerAccountId,
        Amount = FinanceRounding.Money(p.Amount),
        Kind = p.Kind,
        SubAccountId = p.SubAccountId,
        Parameters = p.Parameters is { Length: > DocumentPostingRules.ParametersMaxLength } ? p.Parameters[..DocumentPostingRules.ParametersMaxLength] : p.Parameters,
        DocumentRef = p.DocumentRef,
        InvoiceId = p.InvoiceId,
        SupplierInvoiceId = p.SupplierInvoiceId,
        CollectionPriority = p.CollectionPriority,
        ClosesDocumentType = p.ClosesDocumentType
    };

    internal static StatementLineFacts Facts(BankStatementLine line) =>
        new(line.PayerRecipientName, line.BankAccountNumber, line.Info, line.Code, line.PaymentReference, line.Debit, line.Credit);

    /// <summary>
    /// Open groups on the partner's account from POSTED ledger lines, cached per matching run so
    /// later lines of the same statement see what earlier lines took.
    /// ponytail: proposals of OTHER unposted statements are not subtracted; they are re-checked when
    /// those statements are re-matched.
    /// </summary>
    private async Task<List<OpenItem>> OpenItemsAsync(int companyId, int partnerAccountId, string account, CancellationToken ct)
    {
        if (_openItems.TryGetValue((partnerAccountId, account), out var cached)) return cached;
        var groups = await db.LedgerEntries.AsNoTracking()
            .Where(x => x.CompanyId == companyId && x.Account == account && x.JournalEntry.IsPosted &&
                        EF.Property<int?>(x, "PartnerAccountId") == partnerAccountId)
            .GroupBy(x => new
            {
                x.Parameters,
                x.DocumentRef,
                x.InvoiceId,
                x.SupplierInvoiceId,
                x.CollectionPriority,
                SubAccountId = EF.Property<string?>(x, "SubAccountId")
            })
            .Select(g => new
            {
                g.Key,
                Balance = g.Sum(x => x.DebitAmount - x.CreditAmount),
                FirstDate = g.Min(x => x.PostingDate),
                FirstId = g.Min(x => x.Id),
                LineTypeId = g.Where(x => x.ClosesDocumentType == null).Min(x => x.LineTypeId)
            })
            .ToArrayAsync(ct);

        var typeIds = groups.Where(x => x.LineTypeId != null).Select(x => x.LineTypeId!.Value).Distinct().ToArray();
        var typeCodes = await db.ShortLists.AsNoTracking()
            .Where(x => typeIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.IndexValue, ct);
        var items = groups
            .Select(g => new OpenItem
            {
                PartnerAccountId = partnerAccountId,
                Parameters = g.Key.Parameters,
                DocumentRef = g.Key.DocumentRef,
                InvoiceId = g.Key.InvoiceId,
                SupplierInvoiceId = g.Key.SupplierInvoiceId,
                CollectionPriority = g.Key.CollectionPriority,
                SubAccountId = g.Key.SubAccountId,
                LineTypeCode = g.LineTypeId is { } id && typeCodes.TryGetValue(id, out var code) ? code : null,
                FirstDate = g.FirstDate,
                FirstId = g.FirstId,
                Balance = FinanceRounding.Money(g.Balance)
            })
            .Where(x => x.Balance != 0m)
            .ToList();
        _openItems[(partnerAccountId, account)] = items;
        return items;
    }

    /// <summary>Partner account that owns the payment reference in this company (open in the line's direction first).</summary>
    private async Task<int?> FindReferenceOwnerAsync(int companyId, string? reference, bool inflow, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(reference)) return null;
        var owners = await db.LedgerEntries.AsNoTracking()
            .Where(x => x.CompanyId == companyId && x.Parameters == reference && x.JournalEntry.IsPosted &&
                        EF.Property<int?>(x, "PartnerAccountId") != null)
            .GroupBy(x => EF.Property<int?>(x, "PartnerAccountId"))
            .Select(g => new { PartnerAccountId = g.Key!.Value, Balance = g.Sum(x => x.DebitAmount - x.CreditAmount) })
            .ToArrayAsync(ct);
        var sign = inflow ? 1m : -1m;
        return owners.OrderByDescending(x => sign * x.Balance > 0m).ThenBy(x => x.PartnerAccountId)
            .Select(x => (int?)x.PartnerAccountId).FirstOrDefault();
    }

    /// <summary>Partner's account in this company: 2040 for inflows, 4350 for outflows, else any.</summary>
    private async Task<int?> PickPartnerAccountAsync(int companyId, int partnerId, bool inflow, CancellationToken ct)
    {
        var preferred = inflow ? LedgerAccounts.Customers : LedgerAccounts.Suppliers;
        return await db.Set<PartnerAccount>().AsNoTracking()
            .Where(x => x.PartnerId == partnerId && (x.CompanyId == companyId || x.CompanyId == null))
            .OrderByDescending(x => x.Account == preferred).ThenByDescending(x => x.CompanyId != null).ThenBy(x => x.Id)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync(ct);
    }

    private async Task<string> PartnerLabelAsync(int partnerAccountId, CancellationToken ct) =>
        await db.Set<PartnerAccount>().AsNoTracking().Where(x => x.Id == partnerAccountId)
            .Select(x => x.AccountNumber + " " + x.Partner.Name).SingleOrDefaultAsync(ct) ?? partnerAccountId.ToString();

    private static string SourceLabel(string? source) => source switch
    {
        MatchSources.Template => "šablon",
        MatchSources.BankAccount => "račun uplatioca",
        _ => "izbor"
    };

    private async Task<IReadOnlyList<MatchTemplate>> LoadTemplatesAsync(int companyId, CancellationToken ct)
    {
        var rows = await db.Set<BankStatementPostingTemplate>().AsNoTracking()
            .Where(x => x.IsActive && (x.CompanyId == companyId || x.CompanyId == null))
            .ToArrayAsync(ct);
        return rows
            .Where(x => x.ParentId == null && x.SetPartnerAccountId != null)
            .OrderBy(x => x.CompanyId == null).ThenBy(x => x.SortIndex).ThenBy(x => x.Id)
            .Select(root => new MatchTemplate(
                root.Id,
                root.TemplateName,
                root.SetPartnerAccountId!.Value,
                root.SetSubAccountId,
                rows.Where(x => x.Id == root.Id || x.ParentId == root.Id)
                    .Select(x => new TemplateCondition(x.FieldName, (TemplateMatch)x.Function, x.FieldValue))
                    .ToArray()))
            .ToArray();
    }

    /// <summary>Normalized payer account -> partner (BankAccount rows that belong to a partner).
    /// ponytail: loads all partner accounts of the company; add a normalized indexed column if it grows.</summary>
    private async Task<Dictionary<string, int>> LoadPayerAccountsAsync(int companyId, CancellationToken ct)
    {
        var rows = await db.Set<BankAccount>().AsNoTracking()
            .Where(x => x.PartnerId != null && x.IsActive && x.AccountNumber != null && (x.CompanyId == companyId || x.CompanyId == null))
            .OrderByDescending(x => x.CompanyId != null).ThenBy(x => x.Id)
            .Select(x => new { x.AccountNumber, PartnerId = x.PartnerId!.Value })
            .ToArrayAsync(ct);
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in rows)
        {
            if (StatementText.NormalizeAccount(row.AccountNumber) is { } key) map.TryAdd(key, row.PartnerId);
        }

        return map;
    }
}
