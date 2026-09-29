using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using SzApp.Contracts.LedgerBanking;
using SzApp.Data;
using SzApp.Data.Entities;
using SzApp.Domain.LedgerBanking;

namespace SzApp.Api.Features.LedgerBanking;

public sealed record LedgerCardFilter(
    string? Account = null,
    int? PartnerAccountId = null,
    string? SubAccountId = null,
    DateOnly? From = null,
    DateOnly? To = null,
    string? PaymentReference = null,
    string? Document = null,
    LedgerCardGrouping GroupBy = LedgerCardGrouping.None,
    int? PartnerId = null);

/// <summary>Read side of the general ledger: Kartica (GAP-07), partner balances (GAP-08), Ctrl+K search (DES-08).</summary>
public static class LedgerCardQueries
{
    /// <summary>Company-scoped read endpoints; the group already requires company access.</summary>
    public static void MapLedgerCardEndpoints(RouteGroupBuilder group)
    {
        group.MapGet("/ledger-cards", async Task<IResult> (
            int companyId, string? account, int? partnerAccountId, int? partnerId, string? subAccountId, DateOnly? from, DateOnly? to,
            string? paymentReference, string? document, string? groupBy, int? page, int? pageSize,
            SzAppDbContext db, CancellationToken ct) =>
        {
            if (!Enum.TryParse<LedgerCardGrouping>(groupBy ?? "none", true, out var grouping) || !Enum.IsDefined(grouping))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["groupBy"] = ["Dozvoljeno: none, paymentReference, document, openItems."]
                });
            }

            return Results.Ok(await GetCardAsync(db, companyId,
                new LedgerCardFilter(account, partnerAccountId, subAccountId, from, to, paymentReference, document, grouping, partnerId),
                Math.Max(page ?? 1, 1), Math.Clamp(pageSize ?? 100, 1, 500), ct));
        });

        group.MapGet("/partner-balances", async (
            int companyId, string? account, DateOnly? asOf, bool? onlyDebtors, decimal? minDebt, string? sort, bool? desc,
            int? page, int? pageSize, SzAppDbContext db, IBusinessClock clock, CancellationToken ct) =>
            Results.Ok(await GetPartnerBalancesAsync(db, companyId, string.IsNullOrWhiteSpace(account) ? LedgerAccounts.Customers : account,
                asOf ?? clock.Today, onlyDebtors ?? false, minDebt, sort ?? "balance", desc ?? true,
                Math.Max(page ?? 1, 1), Math.Clamp(pageSize ?? 25, 1, 200), ct)));

        group.MapGet("/search", async (int companyId, string? q, SzAppDbContext db, CancellationToken ct) =>
            Results.Ok(await SearchAsync(db, companyId, q ?? string.Empty, ct)));
    }

    /// <summary>Legacy OcistiPozivNaBroj: GK.PARAMETRI stores the reference without separators.</summary>
    public static string NormalizeReference(string value) =>
        new(value.Where(c => c is not (' ' or '-' or '/' or '\\')).ToArray());

    private static IQueryable<LedgerEntry> Posted(SzAppDbContext db, int companyId) =>
        db.LedgerEntries.AsNoTracking().Where(x => x.CompanyId == companyId && x.JournalEntry.IsPosted);

    private static IQueryable<LedgerEntry> Filtered(SzAppDbContext db, int companyId, LedgerCardFilter f)
    {
        var q = Posted(db, companyId);
        if (!string.IsNullOrWhiteSpace(f.Account)) q = q.Where(x => x.Account == f.Account);
        if (f.PartnerAccountId is { } partnerAccountId) q = q.Where(x => EF.Property<int?>(x, "PartnerAccountId") == partnerAccountId);
        if (f.PartnerId is { } partnerId)
        {
            q = q.Where(x => db.Set<PartnerAccount>().Any(pa => pa.PartnerId == partnerId && (int?)pa.Id == EF.Property<int?>(x, "PartnerAccountId")));
        }
        if (!string.IsNullOrWhiteSpace(f.SubAccountId)) q = q.Where(x => EF.Property<string?>(x, "SubAccountId") == f.SubAccountId);
        if (f.To is { } to) q = q.Where(x => x.PostingDate <= to);
        if (!string.IsNullOrWhiteSpace(f.PaymentReference))
        {
            var reference = NormalizeReference(f.PaymentReference);
            q = q.Where(x => x.Parameters!.StartsWith(reference));
        }
        if (!string.IsNullOrWhiteSpace(f.Document)) q = q.Where(x => x.DocumentRef!.StartsWith(f.Document.Trim()));
        return q;
    }

    public static async Task<LedgerCardResponse> GetCardAsync(
        SzAppDbContext db, int companyId, LedgerCardFilter f, int page, int pageSize, CancellationToken ct)
    {
        var all = Filtered(db, companyId, f);
        if (f.GroupBy == LedgerCardGrouping.OpenItems)
        {
            // Open/closed is decided over everything up to `to` (not just the visible period),
            // so opening + rows == closing == Σ open items.
            var sums = await all.GroupBy(x => x.Parameters)
                .Select(g => new { g.Key, D = g.Sum(x => x.DebitAmount), C = g.Sum(x => x.CreditAmount) })
                .ToListAsync(ct);
            var open = LedgerCardMath.OpenKeys(sums.Select(x => (x.Key, x.D, x.C)));
            var keys = open.Where(x => x.Length > 0).ToList();
            var includeBlank = open.Contains(string.Empty);
            all = all.Where(x => (x.Parameters != null && x.Parameters != "" && keys.Contains(x.Parameters))
                                 || (includeBlank && (x.Parameters == null || x.Parameters == "")));
        }

        var opening = f.From is { } from
            ? await all.Where(x => x.PostingDate < from).SumAsync(x => x.DebitAmount - x.CreditAmount, ct)
            : 0m;
        var range = f.From is { } start ? all.Where(x => x.PostingDate >= start) : all;
        var totals = await range.GroupBy(_ => 1)
            .Select(g => new { D = g.Sum(x => x.DebitAmount), C = g.Sum(x => x.CreditAmount), N = g.Count() })
            .SingleOrDefaultAsync(ct);
        var (debit, credit, count) = totals is null ? (0m, 0m, 0) : (totals.D, totals.C, totals.N);
        var skip = (page - 1) * pageSize;

        IReadOnlyCollection<LedgerCardRowResponse> items;
        if (f.GroupBy is LedgerCardGrouping.PaymentReference or LedgerCardGrouping.Document)
        {
            (items, count) = await GroupedPageAsync(range, f, opening, skip, pageSize, ct);
        }
        else
        {
            items = await LinePageAsync(range, opening, skip, pageSize, ct);
        }

        return new LedgerCardResponse(opening, debit, credit, opening + debit - credit, items, page, pageSize, count);
    }

    private static async Task<LedgerCardRowResponse[]> LinePageAsync(
        IQueryable<LedgerEntry> range, decimal opening, int skip, int take, CancellationToken ct)
    {
        var ordered = range.OrderBy(x => x.PostingDate).ThenBy(x => x.JournalEntryId).ThenBy(x => x.Priority).ThenBy(x => x.Id);
        var before = skip > 0 ? await ordered.Take(skip).SumAsync(x => x.DebitAmount - x.CreditAmount, ct) : 0m;
        var rows = await ordered.Skip(skip).Take(take).Select(x => new
        {
            x.Id, x.JournalEntryId, x.PostingDate, x.Account,
            PartnerAccountId = EF.Property<int?>(x, "PartnerAccountId"),
            SubAccountId = EF.Property<string?>(x, "SubAccountId"),
            x.DocumentRef, Description = x.Description ?? x.JournalEntry.Description,
            LineType = x.LineType != null ? (int?)x.LineType.IndexValue : null,
            x.DueDate, x.Parameters, x.DebitAmount, x.CreditAmount
        }).ToListAsync(ct);
        var running = LedgerCardMath.Running(opening + before, rows.Select(x => (x.DebitAmount, x.CreditAmount)));
        return rows.Select((x, i) => new LedgerCardRowResponse(
            x.Id, x.JournalEntryId, x.PostingDate, x.Account, x.PartnerAccountId, x.SubAccountId, x.DocumentRef,
            x.Description, x.LineType, x.DueDate, x.Parameters, x.DebitAmount, x.CreditAmount, running[i], null, 1)).ToArray();
    }

    private static async Task<(LedgerCardRowResponse[] Rows, int Count)> GroupedPageAsync(
        IQueryable<LedgerEntry> range, LedgerCardFilter f, decimal opening, int skip, int take, CancellationToken ct)
    {
        var byDocument = f.GroupBy == LedgerCardGrouping.Document;
        var grouped = byDocument ? range.GroupBy(x => x.DocumentRef) : range.GroupBy(x => x.Parameters);
        // ponytail: groups are paged in memory (one row per reference/document); push to SQL if a card ever has 100k+ groups.
        var groups = await grouped.Select(g => new
        {
            g.Key, Date = g.Min(x => x.PostingDate), Due = g.Min(x => x.DueDate),
            D = g.Sum(x => x.DebitAmount), C = g.Sum(x => x.CreditAmount), N = g.Count()
        }).ToListAsync(ct);
        var ordered = groups.OrderBy(x => x.Date).ThenBy(x => x.Key, StringComparer.Ordinal).ToList();
        var running = LedgerCardMath.Running(opening, ordered.Select(x => (x.D, x.C)));
        var rows = ordered.Select((x, i) => new LedgerCardRowResponse(
                null, null, x.Date, f.Account ?? string.Empty, f.PartnerAccountId, f.SubAccountId,
                byDocument ? x.Key : null, null, null, x.Due, byDocument ? null : x.Key,
                x.D, x.C, running[i], x.Key ?? string.Empty, x.N))
            .Skip(skip).Take(take).ToArray();
        return (rows, ordered.Count);
    }

    public static async Task<PageResponse<PartnerBalanceResponse>> GetPartnerBalancesAsync(
        SzAppDbContext db, int companyId, string account, DateOnly asOf, bool onlyDebtors, decimal? minDebt,
        string sort, bool descending, int page, int pageSize, CancellationToken ct)
    {
        var paymentTypeId = await db.ShortLists.AsNoTracking()
            .Where(x => x.TableName == LedgerLineTypes.ShortListTable && x.IndexValue == LedgerLineTypes.BankStatement)
            .Select(x => (int?)x.Id).FirstOrDefaultAsync(ct);
        // Class 4 (4350 dobavljači) is credit-normal: what we owe is P − D.
        var creditNormal = account.StartsWith('4');
        var sums = Posted(db, companyId)
            .Where(x => x.Account == account && x.PostingDate <= asOf && EF.Property<int?>(x, "PartnerAccountId") != null)
            .GroupBy(x => EF.Property<int?>(x, "PartnerAccountId"))
            .Select(g => new
            {
                Id = g.Key,
                D = g.Sum(x => x.DebitAmount),
                C = g.Sum(x => x.CreditAmount),
                DebitDue = g.Sum(x => (x.DueDate ?? x.PostingDate) < asOf ? x.DebitAmount : 0m),
                CreditDue = g.Sum(x => (x.DueDate ?? x.PostingDate) < asOf ? x.CreditAmount : 0m),
                LastPayment = g.Max(x => x.LineTypeId == paymentTypeId ? (DateOnly?)x.PostingDate : null)
            });
        var q = from s in sums
                join pa in db.Set<PartnerAccount>() on s.Id equals (int?)pa.Id
                select new
                {
                    s.Id, s.D, s.C, s.DebitDue, s.CreditDue, s.LastPayment, pa.PartnerId, pa.AccountNumber,
                    pa.Partner.Name, Outstanding = creditNormal ? s.C - s.D : s.D - s.C
                };
        var threshold = onlyDebtors ? Math.Max(minDebt ?? 0m, LedgerCardMath.ClosedTolerance) : minDebt;
        if (threshold is { } min) q = q.Where(x => x.Outstanding >= min);

        var total = await q.CountAsync(ct);
        q = (sort, descending) switch
        {
            ("name", false) => q.OrderBy(x => x.Name).ThenBy(x => x.Id),
            ("name", true) => q.OrderByDescending(x => x.Name).ThenBy(x => x.Id),
            ("accountNumber", false) => q.OrderBy(x => x.AccountNumber).ThenBy(x => x.Id),
            ("accountNumber", true) => q.OrderByDescending(x => x.AccountNumber).ThenBy(x => x.Id),
            (_, false) => q.OrderBy(x => x.Outstanding).ThenBy(x => x.Id),
            _ => q.OrderByDescending(x => x.Outstanding).ThenBy(x => x.Id),
        };
        var rows = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        var items = rows.Select(x => new PartnerBalanceResponse(
            x.Id!.Value, x.PartnerId, x.Name, x.AccountNumber, account, x.D, x.C, x.D - x.C,
            creditNormal ? LedgerCardMath.Overdue(x.CreditDue, x.D, x.Outstanding) : LedgerCardMath.Overdue(x.DebitDue, x.C, x.Outstanding),
            x.LastPayment)).ToArray();
        return new PageResponse<PartnerBalanceResponse>(items, page, pageSize, total);
    }

    public static async Task<SearchResultResponse[]> SearchAsync(SzAppDbContext db, int companyId, string term, CancellationToken ct)
    {
        term = term.Trim();
        if (term.Length < 2) return [];
        var number = int.TryParse(term, out var n) ? n : (int?)null;

        var accounts = await db.Set<PartnerAccount>().AsNoTracking()
            .Where(pa => pa.CompanyId == companyId && (pa.Partner.Name.Contains(term) || pa.Partner.ShortName.Contains(term)
                || pa.Partner.TaxNumber == term || pa.Partner.RegistrationNumber == term || pa.AccountNumber == number))
            .OrderBy(pa => pa.Partner.Name).Take(15)
            .Select(pa => new { pa.Id, pa.Account, pa.AccountNumber, pa.Partner.Name, pa.Partner.TaxNumber })
            .ToListAsync(ct);

        var reference = NormalizeReference(term);
        var payments = reference.Length < 3 ? [] : await (
                from x in Posted(db, companyId)
                where x.Parameters!.StartsWith(reference)
                join pa in db.Set<PartnerAccount>() on EF.Property<int?>(x, "PartnerAccountId") equals (int?)pa.Id
                group x by new { x.Parameters, PartnerAccountId = pa.Id, x.Account, pa.Partner.Name } into g
                select new { g.Key.Parameters, g.Key.PartnerAccountId, g.Key.Account, g.Key.Name, Balance = g.Sum(x => x.DebitAmount - x.CreditAmount) })
            .OrderBy(x => x.Parameters).Take(10).ToListAsync(ct);

        var ids = accounts.Select(x => x.Id).ToList();
        var balances = await Posted(db, companyId)
            .Where(x => ids.Contains(EF.Property<int?>(x, "PartnerAccountId") ?? 0))
            .GroupBy(x => EF.Property<int?>(x, "PartnerAccountId"))
            .Select(g => new { g.Key, Balance = g.Sum(x => x.DebitAmount - x.CreditAmount) })
            .ToDictionaryAsync(x => x.Key!.Value, x => x.Balance, ct);

        var units = await db.Set<Unit>().AsNoTracking()
            .Where(u => u.CompanyId == companyId && u.Name!.Contains(term))
            .OrderBy(u => u.Name).Take(10)
            .Select(u => new { u.Id, u.Name, EntranceName = u.BuildingEntrance != null ? u.BuildingEntrance.EntranceName : null })
            .ToListAsync(ct);

        return accounts.Select(x => new SearchResultResponse("partner", x.Name,
                $"{x.Account} / {x.AccountNumber}" + (x.TaxNumber is null ? "" : $" · PIB {x.TaxNumber}"),
                companyId, x.Id, x.Account, null, null, balances.GetValueOrDefault(x.Id)))
            .Concat(payments.Select(x => new SearchResultResponse("payment", x.Parameters!, $"{x.Name} · {x.Account}",
                companyId, x.PartnerAccountId, x.Account, null, x.Parameters, x.Balance)))
            .Concat(units.Select(x => new SearchResultResponse("unit", x.Name ?? $"#{x.Id}", x.EntranceName,
                companyId, null, null, x.Id, null, null)))
            .ToArray();
    }
}
