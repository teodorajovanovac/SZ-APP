using Microsoft.EntityFrameworkCore;
using SzApp.Contracts.LedgerBanking;
using SzApp.Data.Entities;
using SzApp.Data.Entities.LedgerBanking;
using SzApp.Domain;
using SzApp.Domain.LedgerBanking;

namespace SzApp.Api.Features.LedgerBanking;

/// <summary>9.3 posting / GAP-06 unposting.</summary>
public sealed partial class BankStatementService
{
    /// <summary>
    /// 9.3: two 2410 summary lines (debit inflows, credit outflows) + one partner line per accepted
    /// allocation. Every statement line must be Matched (FIN-15: nothing is left unposted). Runs
    /// through JournalPostingService.PostLinesAsync, so accounts/partners/sub-accounts, the period
    /// guard and the balance check are the same ones every other posting path uses.
    /// </summary>
    public Task<PostingResultResponse> PostAsync(
        int companyId,
        int statementId,
        int staffId,
        byte[] expectedRowVersion,
        CancellationToken cancellationToken) =>
        ExecuteSerializableAsync(async () =>
        {
            await LockStatementAsync(statementId, cancellationToken);
            var statement = await LoadStatementAsync(companyId, statementId, cancellationToken);
            EnsureExpectedVersion(expectedRowVersion, statement.RowVersion);

            if (statement.Status == BankStatementStatus.Posted && statement.JournalEntryId.HasValue)
            {
                var posted = await dbContext.JournalEntries.AsNoTracking()
                    .SingleAsync(x => x.Id == statement.JournalEntryId.Value, cancellationToken);
                return new PostingResultResponse(posted.Id, true, Convert.ToBase64String(posted.RowVersion));
            }

            if (statement.Lines.Any(x => x.Status != BankStatementLineStatus.Matched))
            {
                throw new DomainRuleException("statement.not-ready", "Sve stavke izvoda moraju biti prihvaćene pre knjiženja.");
            }

            var parts = statement.Lines
                .SelectMany(line => line.Allocations.Select(a => new StatementPostingPart(line.Id, line.Credit > 0m, line.PayerRecipientName, ToAllocationProposal(a))))
                .ToArray();
            var statementLines = statement.Lines.Select(x => (x.Id, x.Debit, x.Credit)).ToArray();
            var lines = BankStatementPostingRules.Build(statement.LedgerAccount, statement.StatementNumber, statement.Date, statementLines, parts);

            var journal = await journals.PostLinesAsync(
                companyId, statement.Date, $"Izvod {statement.StatementNumber}/{statement.Date.Year}", lines, staffId, null, cancellationToken);

            statement.JournalEntryId = journal.Id;
            statement.Status = BankStatementStatus.Posted;
            foreach (var line in statement.Lines)
            {
                line.Status = BankStatementLineStatus.Posted;
                if (line.Credit > 0m && !await dbContext.Set<BankInFlow>().AnyAsync(x => x.BankStatementLineId == line.Id, cancellationToken))
                {
                    dbContext.Set<BankInFlow>().Add(new BankInFlow
                    {
                        CompanyId = companyId,
                        BankAccountId = statement.BankAccountId,
                        DateInFlow = statement.Date,
                        ReferenceNumber = line.PaymentReference ?? line.BankRef ?? $"{statement.StatementNumber}/{line.LineNumber}",
                        Currency = "RSD",
                        OriginalAmount = line.Credit,
                        AmountLocalCurrency = line.Credit,
                        PartnerAccountId = line.Allocations.Select(a => a.PartnerAccountId).FirstOrDefault(),
                        InvoiceDescription = line.Info,
                        BankStatementLineId = line.Id
                    });
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            return new PostingResultResponse(journal.Id, false, Convert.ToBase64String(journal.RowVersion));
        }, cancellationToken);

    /// <summary>
    /// GAP-06: red storno of the statement's journal (negative same side, line type 7, P9), dated on
    /// the statement's own date so a locked month still blocks it. Every line goes back to Pending
    /// so it can be re-matched and re-posted; unsent BankInFlow rows for this statement are removed.
    /// </summary>
    public Task<PostingResultResponse> UnpostAsync(
        int companyId,
        int statementId,
        int staffId,
        byte[] expectedRowVersion,
        CancellationToken cancellationToken) =>
        ExecuteSerializableAsync(async () =>
        {
            await LockStatementAsync(statementId, cancellationToken);
            var statement = await LoadStatementAsync(companyId, statementId, cancellationToken);
            EnsureExpectedVersion(expectedRowVersion, statement.RowVersion);

            if (statement.Status != BankStatementStatus.Posted || statement.JournalEntryId is not { } journalId)
            {
                throw new DomainRuleException("statement.not-posted", "Izvod nije proknjižen.");
            }

            var existingReversal = await dbContext.JournalEntries.AsNoTracking()
                .SingleOrDefaultAsync(x => x.ReversalOfId == journalId, cancellationToken);
            if (existingReversal is null)
            {
                var posted = (await dbContext.LedgerEntries.AsNoTracking()
                        .Where(x => x.CompanyId == companyId && x.JournalEntryId == journalId)
                        .OrderBy(x => x.Priority)
                        .Select(x => new
                        {
                            x.Account, x.DebitAmount, x.CreditAmount, x.PostingDate, x.DueDate, x.DocumentRef, x.Parameters,
                            x.InvoiceId, x.SupplierInvoiceId, x.CollectionPriority, x.ClosesDocumentType, x.Description,
                            PartnerAccountId = EF.Property<int?>(x, "PartnerAccountId"),
                            SubAccountId = EF.Property<string?>(x, "SubAccountId"),
                            BankStatementLineId = EF.Property<int?>(x, "BankStatementLineId")
                        })
                        .ToArrayAsync(cancellationToken))
                    .Select(x => new PostingLine(x.Account, x.DebitAmount, x.CreditAmount, LedgerLineTypes.BankStatement, x.PostingDate,
                        x.PartnerAccountId, x.SubAccountId, x.DueDate, x.DocumentRef, x.Parameters, x.InvoiceId,
                        x.SupplierInvoiceId, x.CollectionPriority, x.Description, x.BankStatementLineId, x.ClosesDocumentType, x.Description))
                    .ToArray();

                var stornoLines = DocumentPostingRules.Negate(posted, statement.Date);
                await journals.PostLinesAsync(
                    companyId, statement.Date, $"STORNO Izvod {statement.StatementNumber}/{statement.Date.Year}", stornoLines, staffId, journalId, cancellationToken);
            }

            var lineIds = statement.Lines.Select(x => x.Id).ToArray();
            var unsent = await dbContext.Set<BankInFlow>()
                .Where(x => x.CompanyId == companyId && lineIds.Contains(x.BankStatementLineId) && x.SentToManagerAt == null)
                .ToArrayAsync(cancellationToken);
            dbContext.RemoveRange(unsent);

            statement.JournalEntryId = null;
            statement.Status = BankStatementStatus.Imported;
            foreach (var line in statement.Lines)
            {
                line.Status = BankStatementLineStatus.Pending;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            var reversalId = existingReversal?.Id ?? (await dbContext.JournalEntries.AsNoTracking()
                .SingleAsync(x => x.ReversalOfId == journalId, cancellationToken)).Id;
            var reversal = await dbContext.JournalEntries.AsNoTracking().SingleAsync(x => x.Id == reversalId, cancellationToken);
            return new PostingResultResponse(reversal.Id, existingReversal is not null, Convert.ToBase64String(reversal.RowVersion));
        }, cancellationToken);

    private Task LockStatementAsync(int statementId, CancellationToken cancellationToken) =>
        dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT 1 FROM [finance].[BankStatement] WITH (UPDLOCK, HOLDLOCK) WHERE [Id] = {statementId}",
            cancellationToken);

    private static AllocationProposal ToAllocationProposal(BankStatementLineAllocation a) => new(
        a.Account, a.PartnerAccountId, a.Amount, a.Kind, a.SubAccountId, a.Parameters, a.DocumentRef,
        a.InvoiceId, a.SupplierInvoiceId, a.CollectionPriority, a.ClosesDocumentType);
}
