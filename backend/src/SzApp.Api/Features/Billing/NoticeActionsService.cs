using System.Data;
using Microsoft.EntityFrameworkCore;
using SzApp.Api.Features.LedgerBanking;
using SzApp.Contracts.Billing;
using SzApp.Data;
using SzApp.Data.Entities;
using SzApp.Data.Entities.Billing;
using SzApp.Domain;
using SzApp.Domain.Billing;
using SzApp.Domain.LedgerBanking;
using SzApp.Domain.Reports;

namespace SzApp.Api.Features.Billing;

/// <summary>
/// Notice batch actions: listing, the optional notice-cost posting + P9 storno, and GAP-20
/// "za utuženje" marking/export. Kept out of BillingService (shared with other agents).
/// </summary>
public sealed class NoticeActionsService(
    SzAppDbContext db,
    IJournalPostingService journals,
    IBusinessClock clock)
{
    public async Task<IReadOnlyList<NoticeBatchListItem>> ListBatchesAsync(int companyId, CancellationToken ct) =>
        await db.Set<NoticeBatch>().AsNoTracking().Where(x => x.CompanyId == companyId)
            .OrderByDescending(x => x.Date).ThenByDescending(x => x.Id)
            .Select(x => new NoticeBatchListItem(x.Id, x.Title, x.Date, x.Notices.Count(n => n.IsActive),
                x.Notices.Where(n => n.IsActive).Sum(n => n.AdditionalCosts), x.CostsJournalEntryId))
            .ToArrayAsync(ct);

    /// <summary>
    /// Posts the batch's notice costs (2040 D per partner / 4900 P, line type 3) as one journal,
    /// through the journal service (period guard + balance check). Idempotent: an already-posted
    /// batch returns its journal.
    /// </summary>
    public Task<NoticeCostPostingResult> PostCostsAsync(int companyId, int batchId, int staffId, CancellationToken ct) =>
        ExecuteSerializableAsync(async () =>
        {
            var batch = await LockBatchAsync(companyId, batchId, ct);
            if (batch.CostsJournalEntryId is { } existing)
                return new NoticeCostPostingResult(batchId, existing, existing, await TotalCostsAsync(batchId, ct));

            var sources = await (from n in db.Set<Notice>().AsNoTracking()
                                 join pa in db.Set<PartnerAccount>().AsNoTracking() on n.PartnerAccountId equals pa.Id
                                 where n.CompanyId == companyId && n.NoticeBatchId == batchId && n.IsActive && n.AdditionalCosts != 0m
                                 select new { n.Id, PartnerAccountId = pa.Id, pa.Account, n.PaymentReference, n.AdditionalCosts })
                .ToArrayAsync(ct);
            var lines = NoticeCostPostingRules.Build(batch.Date, batch.Date, $"OP-{batch.Date:yyyyMMdd}",
                sources.Select(x => new NoticeCostPostingSource(x.Id, x.PartnerAccountId, x.Account,
                    NoticeDocumentService.NoticeNumber(x.PaymentReference), x.PaymentReference, x.AdditionalCosts)));
            var journal = await journals.PostLinesAsync(companyId, batch.Date, $"TROŠKOVI OPOMENA {batch.Title}", lines, staffId, null, ct);

            batch.CostsJournalEntryId = journal.Id;
            await db.SaveChangesAsync(ct);
            return new NoticeCostPostingResult(batchId, journal.Id, journal.Id, lines.Where(x => x.Account == LedgerAccounts.Revenue).Sum(x => x.Credit));
        }, ct);

    /// <summary>P9 red storno of the posted notice costs (same lines negated, same side, type 7, dated today).</summary>
    public Task<NoticeCostPostingResult> CancelCostsAsync(int companyId, int batchId, int staffId, CancellationToken ct) =>
        ExecuteSerializableAsync(async () =>
        {
            var batch = await LockBatchAsync(companyId, batchId, ct);
            if (batch.CostsJournalEntryId is not { } journalId)
                throw new DomainRuleException("notice.costs-not-posted", "Troškovi opomena ove serije nisu proknjiženi.");

            var posted = (await db.LedgerEntries.AsNoTracking()
                    .Where(x => x.CompanyId == companyId && x.JournalEntryId == journalId)
                    .OrderBy(x => x.Priority)
                    .Select(x => new
                    {
                        x.Account, x.DebitAmount, x.CreditAmount, x.PostingDate, x.DueDate, x.DocumentRef, x.Parameters, x.Note,
                        PartnerAccountId = EF.Property<int?>(x, "PartnerAccountId")
                    })
                    .ToArrayAsync(ct))
                .Select(x => new PostingLine(x.Account, x.DebitAmount, x.CreditAmount, LedgerLineTypes.Invoice, x.PostingDate,
                    x.PartnerAccountId, DueDate: x.DueDate, DocumentRef: x.DocumentRef, Parameters: x.Parameters, Note: x.Note))
                .ToArray();
            var storno = DocumentPostingRules.Negate(posted, clock.Today);
            var journal = await journals.PostLinesAsync(companyId, clock.Today, $"STORNO TROŠKOVI OPOMENA {batch.Title}", storno, staffId, journalId, ct);

            batch.CostsJournalEntryId = null;
            await db.SaveChangesAsync(ct);
            return new NoticeCostPostingResult(batchId, null, journal.Id, -storno.Where(x => x.Account == LedgerAccounts.Revenue).Sum(x => x.Credit));
        }, ct);

    /// <summary>GAP-20: mark/unmark for lawsuit (legacy basket type 11) with an optional manual lawyer cost.</summary>
    public async Task<NoticeResponse> SetLawsuitAsync(int companyId, int noticeId, SetNoticeLawsuitRequest request, CancellationToken ct)
    {
        if (request.LawyerCost is < 0m)
            throw new DomainRuleException("notice.lawyer-cost-negative", "Trošak advokata ne može biti negativan.");
        var notice = await db.Set<Notice>().SingleOrDefaultAsync(x => x.CompanyId == companyId && x.Id == noticeId, ct)
            ?? throw new DomainRuleException("notice.not-found", "Opomena ne postoji.");
        notice.IsForLawsuit = request.IsForLawsuit;
        notice.LawyerCost = request.IsForLawsuit && request.LawyerCost is { } cost ? FinanceRounding.Money(cost) : null;
        await db.SaveChangesAsync(ct);
        return new NoticeResponse(notice.Id, notice.NoticeBatchId, notice.PartnerAccountId, notice.UnpaidInvoiceCount, notice.Debt,
            notice.AdditionalCosts, notice.Total, notice.PaymentReference, notice.DeliveryStatus.ToString(), notice.RenderedDocumentPath,
            Convert.ToBase64String(notice.RowVersion), notice.IsForLawsuit, notice.LawyerCost);
    }

    /// <summary>CSV of partners marked "za utuženje" (optionally one batch) for the lawyer.</summary>
    public async Task<byte[]> ExportLawsuitCsvAsync(int companyId, int? batchId, CancellationToken ct)
    {
        var rows = await (from n in db.Set<Notice>().AsNoTracking()
                          join pa in db.Set<PartnerAccount>().AsNoTracking() on n.PartnerAccountId equals pa.Id
                          where n.CompanyId == companyId && n.IsForLawsuit && (batchId == null || n.NoticeBatchId == batchId)
                          orderby n.NoticeBatch.Date descending, pa.AccountNumber
                          select new object?[]
                          {
                              pa.AccountNumber, pa.Partner.Name, pa.Partner.Jmbg ?? pa.Partner.TaxNumber, n.NoticeBatch.Title, n.NoticeBatch.Date,
                              n.UnpaidInvoiceCount, n.Debt, n.AdditionalCosts, n.LawyerCost, n.Total + (n.LawyerCost ?? 0m), n.PaymentReference
                          }).ToArrayAsync(ct);
        return CsvReportExporter.Export(new TabularReportData(
            ["Sifra", "Partner", "JMBG/PIB", "Serija", "Datum opomene", "Broj racuna", "Dug", "Troskovi opomene", "Trosak advokata", "Ukupno", "Poziv na broj"],
            rows));
    }

    private async Task<decimal> TotalCostsAsync(int batchId, CancellationToken ct) =>
        await db.Set<Notice>().Where(x => x.NoticeBatchId == batchId && x.IsActive).SumAsync(x => x.AdditionalCosts, ct);

    private async Task<NoticeBatch> LockBatchAsync(int companyId, int batchId, CancellationToken ct)
    {
        await db.Database.ExecuteSqlInterpolatedAsync($"SELECT 1 FROM [billing].[NoticeBatch] WITH (UPDLOCK, HOLDLOCK) WHERE [Id] = {batchId}", ct);
        return await db.Set<NoticeBatch>().SingleOrDefaultAsync(x => x.CompanyId == companyId && x.Id == batchId, ct)
            ?? throw new DomainRuleException("notice.batch-not-found", "Serija opomena ne postoji.");
    }

    private async Task<T> ExecuteSerializableAsync<T>(Func<Task<T>> operation, CancellationToken ct)
    {
        var strategy = db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
            var result = await operation();
            await transaction.CommitAsync(ct);
            return result;
        });
    }
}
