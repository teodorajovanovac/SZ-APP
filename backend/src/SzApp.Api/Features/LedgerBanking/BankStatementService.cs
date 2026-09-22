using System.Data;
using Microsoft.EntityFrameworkCore;
using SzApp.Contracts.LedgerBanking;
using SzApp.Data;
using SzApp.Data.Entities;
using SzApp.Data.Entities.LedgerBanking;
using SzApp.Domain;
using SzApp.Domain.LedgerBanking;

namespace SzApp.Api.Features.LedgerBanking;

public interface IBankStatementService
{
    Task<BankStatementResponse> ImportAsync(int companyId, BankStatementImportRequest request, CancellationToken cancellationToken);
    Task<BankStatementLineResponse> MatchAsync(int companyId, int lineId, MatchBankStatementLineRequest request, CancellationToken cancellationToken);
    Task<BankStatementLineResponse> IgnoreAsync(int companyId, int lineId, byte[] expectedRowVersion, CancellationToken cancellationToken);
    Task<PostingResultResponse> PostAsync(int companyId, int statementId, int staffId, byte[] expectedRowVersion, CancellationToken cancellationToken);
}

public sealed class BankStatementService(
    SzAppDbContext dbContext,
    LedgerMutationScope mutationScope,
    TimeProvider timeProvider) : IBankStatementService
{
    public async Task<BankStatementResponse> ImportAsync(
        int companyId,
        BankStatementImportRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Lines.Select(x => x.LineNumber).Distinct().Count() != request.Lines.Count)
        {
            throw new DomainRuleException("statement.duplicate-line-number", "Brojevi stavki izvoda moraju biti jedinstveni.");
        }

        var totals = LedgerBankingRules.ReconcileStatement(
            request.PreviousBalance,
            request.Debit,
            request.Credit,
            request.NewBalance,
            request.Lines.Select(x => new PostingAmounts(x.Debit, x.Credit)));
        var validBankAccount = await dbContext.Set<BankAccount>().AsNoTracking()
            .AnyAsync(x => x.Id == request.BankAccountId && x.CompanyId == companyId && x.IsActive, cancellationToken);
        if (!validBankAccount)
        {
            throw new DomainRuleException("statement.invalid-bank-account", "Bankovni račun ne pripada kompaniji ili nije aktivan.");
        }
        await EnsurePostingAccountAsync(request.LedgerAccount, cancellationToken);

        var existing = await dbContext.Set<BankStatement>().AsNoTracking().Include(x => x.Lines)
            .SingleOrDefaultAsync(x => x.CompanyId == companyId &&
                                       x.BankAccountId == request.BankAccountId &&
                                       x.StatementNumber == request.StatementNumber &&
                                       x.StatementSuffix == request.StatementSuffix &&
                                       x.Date == request.Date, cancellationToken);
        if (existing is not null)
        {
            if (existing.PreviousBalance != totals.PreviousBalance || existing.NewBalance != totals.NewBalance ||
                existing.Debit != totals.Debit || existing.Credit != totals.Credit ||
                existing.Lines.Count != request.Lines.Count)
            {
                throw new DomainRuleException("statement.duplicate-conflict", "Izvod sa istim identitetom već postoji sa drugačijim sadržajem.");
            }

            return MapStatement(existing);
        }

        var statement = new BankStatement
        {
            CompanyId = companyId,
            BankAccountId = request.BankAccountId,
            LedgerAccount = request.LedgerAccount.Trim(),
            StatementNumber = request.StatementNumber,
            StatementSuffix = request.StatementSuffix?.Trim(),
            Date = request.Date,
            PreviousBalance = totals.PreviousBalance,
            NewBalance = totals.NewBalance,
            Debit = totals.Debit,
            Credit = totals.Credit,
            CountDebitEntry = totals.DebitCount,
            CountCreditEntry = totals.CreditCount,
            Status = BankStatementStatus.Imported
        };

        foreach (var line in request.Lines.OrderBy(x => x.LineNumber))
        {
            statement.Lines.Add(new BankStatementLine
            {
                CompanyId = companyId,
                LineNumber = line.LineNumber,
                PayerRecipientName = line.PayerRecipientName.Trim(),
                BankAccountNumber = line.BankAccountNumber?.Trim(),
                Debit = FinanceRounding.Money(line.Debit),
                Credit = FinanceRounding.Money(line.Credit),
                Info = line.Info?.Trim(),
                Code = line.Code,
                PaymentReference = line.PaymentReference?.Trim(),
                PaymentReferenceOut = line.PaymentReferenceOut?.Trim(),
                BankRef = line.BankRef?.Trim(),
                Status = BankStatementLineStatus.Pending
            });
        }

        dbContext.Set<BankStatement>().Add(statement);
        await dbContext.SaveChangesAsync(cancellationToken);
        return MapStatement(statement);
    }

    public async Task<BankStatementLineResponse> MatchAsync(
        int companyId,
        int lineId,
        MatchBankStatementLineRequest request,
        CancellationToken cancellationToken)
    {
        var line = await dbContext.Set<BankStatementLine>().Include(x => x.BankStatement)
            .SingleOrDefaultAsync(x => x.Id == lineId && x.CompanyId == companyId, cancellationToken)
            ?? throw new KeyNotFoundException("Stavka izvoda nije pronađena.");
        EnsureStatementMutable(line.BankStatement);
        EnsureExpectedVersion(Convert.FromBase64String(request.RowVersion), line.RowVersion);
        await EnsurePostingAccountAsync(request.CounterAccount, cancellationToken);

        line.PartnerAccountId = request.PartnerAccountId;
        line.SubAccountId = NullIfWhiteSpace(request.SubAccountId);
        line.CounterAccount = request.CounterAccount.Trim();
        line.Status = BankStatementLineStatus.Matched;
        UpdateStatementStatus(line.BankStatement);
        await dbContext.SaveChangesAsync(cancellationToken);
        return MapLine(line);
    }

    public async Task<BankStatementLineResponse> IgnoreAsync(
        int companyId,
        int lineId,
        byte[] expectedRowVersion,
        CancellationToken cancellationToken)
    {
        var line = await dbContext.Set<BankStatementLine>().Include(x => x.BankStatement)
            .SingleOrDefaultAsync(x => x.Id == lineId && x.CompanyId == companyId, cancellationToken)
            ?? throw new KeyNotFoundException("Stavka izvoda nije pronađena.");
        EnsureStatementMutable(line.BankStatement);
        EnsureExpectedVersion(expectedRowVersion, line.RowVersion);

        line.PartnerAccountId = null;
        line.SubAccountId = null;
        line.CounterAccount = null;
        line.Status = BankStatementLineStatus.Ignored;
        UpdateStatementStatus(line.BankStatement);
        await dbContext.SaveChangesAsync(cancellationToken);
        return MapLine(line);
    }

    public Task<PostingResultResponse> PostAsync(
        int companyId,
        int statementId,
        int staffId,
        byte[] expectedRowVersion,
        CancellationToken cancellationToken) =>
        ExecuteSerializableAsync(async () =>
        {
            await dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT 1 FROM [finance].[BankStatement] WITH (UPDLOCK, HOLDLOCK) WHERE [Id] = {statementId}",
                cancellationToken);
            var statement = await dbContext.Set<BankStatement>().Include(x => x.Lines)
                .SingleOrDefaultAsync(x => x.Id == statementId && x.CompanyId == companyId, cancellationToken)
                ?? throw new KeyNotFoundException("Izvod nije pronađen.");
            EnsureExpectedVersion(expectedRowVersion, statement.RowVersion);

            if (statement.Status == BankStatementStatus.Posted && statement.JournalEntryId.HasValue)
            {
                var postedJournal = await dbContext.JournalEntries.AsNoTracking()
                    .SingleAsync(x => x.Id == statement.JournalEntryId.Value, cancellationToken);
                return new PostingResultResponse(postedJournal.Id, true, Convert.ToBase64String(postedJournal.RowVersion));
            }

            if (statement.Status != BankStatementStatus.Ready || statement.Lines.Any(x => x.Status == BankStatementLineStatus.Pending))
            {
                throw new DomainRuleException("statement.not-ready", "Sve stavke izvoda moraju biti uparene ili ignorisane.");
            }

            var matched = statement.Lines.Where(x => x.Status == BankStatementLineStatus.Matched).ToArray();
            if (matched.Length == 0)
            {
                throw new DomainRuleException("statement.no-matched-lines", "Izvod mora imati najmanje jednu uparenu stavku za knjiženje.");
            }

            var accounts = matched.Select(x => x.CounterAccount!).Append(statement.LedgerAccount);
            await EnsurePostingAccountsAsync(accounts, cancellationToken);

            var journal = new JournalEntry
            {
                CompanyId = companyId,
                PostingDate = statement.Date,
                Description = $"Izvod {statement.StatementNumber}{statement.StatementSuffix}",
                Currency = "RSD",
                IsPosted = false
            };
            var priority = 0;
            var lineSources = new List<(LedgerEntry LedgerLine, BankStatementLine BankLine)>();
            foreach (var bankLine in matched.OrderBy(x => x.LineNumber))
            {
                var amount = FinanceRounding.Money(bankLine.Debit + bankLine.Credit);
                var bankLedgerLine = new LedgerEntry
                {
                    CompanyId = companyId,
                    Account = statement.LedgerAccount,
                    PostingDate = statement.Date,
                    DebitAmount = bankLine.Credit > 0m ? amount : 0m,
                    CreditAmount = bankLine.Debit > 0m ? amount : 0m,
                    DocumentRef = bankLine.BankRef,
                    Parameters = bankLine.PaymentReference,
                    Description = bankLine.PayerRecipientName,
                    Priority = ++priority
                };
                var counterLine = new LedgerEntry
                {
                    CompanyId = companyId,
                    Account = bankLine.CounterAccount!,
                    PostingDate = statement.Date,
                    DebitAmount = bankLine.Debit > 0m ? amount : 0m,
                    CreditAmount = bankLine.Credit > 0m ? amount : 0m,
                    DocumentRef = bankLine.BankRef,
                    Parameters = bankLine.PaymentReference,
                    Description = bankLine.PayerRecipientName,
                    Priority = ++priority
                };
                journal.Lines.Add(bankLedgerLine);
                journal.Lines.Add(counterLine);
                lineSources.Add((bankLedgerLine, bankLine));
                lineSources.Add((counterLine, bankLine));
            }

            journal.Balance = LedgerBankingRules.ValidateJournal(
                journal.Lines.Select(x => new PostingAmounts(x.DebitAmount, x.CreditAmount)));
            dbContext.JournalEntries.Add(journal);
            foreach (var (ledgerLine, bankLine) in lineSources)
            {
                SetLedgerSource(ledgerLine, bankLine);
            }
            await dbContext.SaveChangesAsync(cancellationToken);

            journal.IsPosted = true;
            journal.PostedUserId = staffId;
            journal.PostedAt = timeProvider.GetUtcNow();
            statement.JournalEntryId = journal.Id;
            statement.Status = BankStatementStatus.Posted;
            foreach (var line in matched)
            {
                line.Status = BankStatementLineStatus.Posted;
                if (line.Credit > 0m)
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
                        PartnerAccountId = line.PartnerAccountId,
                        InvoiceDescription = line.Info,
                        BankStatementLineId = line.Id
                    });
                }
            }

            using var scope = mutationScope.AllowPosting();
            await SetPostingSessionContextAsync(true, cancellationToken);
            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            finally
            {
                await SetPostingSessionContextAsync(false, cancellationToken);
            }

            return new PostingResultResponse(journal.Id, false, Convert.ToBase64String(journal.RowVersion));
        }, cancellationToken);

    private async Task<T> ExecuteSerializableAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            var result = await operation();
            await transaction.CommitAsync(cancellationToken);
            return result;
        });
    }

    private Task SetPostingSessionContextAsync(bool enabled, CancellationToken cancellationToken) =>
        dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"EXEC sys.sp_set_session_context @key=N'szapp_allow_posting', @value={(enabled ? 1 : (int?)null)}",
            cancellationToken);

    private async Task EnsurePostingAccountsAsync(IEnumerable<string> accounts, CancellationToken cancellationToken)
    {
        var codes = accounts.Distinct(StringComparer.Ordinal).ToArray();
        var count = await dbContext.Set<ChartAccount>().AsNoTracking()
            .CountAsync(x => codes.Contains(x.Account) && x.IsActive && !x.IsSynthetic, cancellationToken);
        if (count != codes.Length)
        {
            throw new DomainRuleException("journal.invalid-account", "Konto ne postoji, nije aktivan ili je sintetički.");
        }
    }

    private Task EnsurePostingAccountAsync(string account, CancellationToken cancellationToken) =>
        EnsurePostingAccountsAsync([account.Trim()], cancellationToken);

    private void SetLedgerSource(LedgerEntry ledgerLine, BankStatementLine bankLine)
    {
        var entry = dbContext.Entry(ledgerLine);
        entry.Property("BankStatementLineId").CurrentValue = bankLine.Id;
        entry.Property("PartnerAccountId").CurrentValue = bankLine.PartnerAccountId;
        entry.Property("SubAccountId").CurrentValue = bankLine.SubAccountId;
    }

    private static void UpdateStatementStatus(BankStatement statement)
    {
        statement.Status = statement.Lines.All(x => x.Status is BankStatementLineStatus.Matched or BankStatementLineStatus.Ignored)
            ? BankStatementStatus.Ready
            : statement.Lines.Any(x => x.Status is BankStatementLineStatus.Matched or BankStatementLineStatus.Ignored)
                ? BankStatementStatus.PartiallyMatched
                : BankStatementStatus.Imported;
    }

    private static void EnsureStatementMutable(BankStatement statement)
    {
        if (statement.Status == BankStatementStatus.Posted)
        {
            throw new DomainRuleException("statement.posted-immutable", "Knjižen izvod se ne može menjati.");
        }
    }

    private static void EnsureExpectedVersion(byte[] expected, byte[] current)
    {
        if (expected.Length == 0 || !expected.AsSpan().SequenceEqual(current))
        {
            throw new DbUpdateConcurrencyException("Podatak je u međuvremenu izmenjen.");
        }
    }

    private static string? NullIfWhiteSpace(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    internal static BankStatementResponse MapStatement(BankStatement statement) => new(
        new BankStatementSummaryResponse(
            statement.Id,
            statement.BankAccountId,
            statement.StatementNumber,
            statement.StatementSuffix,
            statement.Date,
            statement.PreviousBalance,
            statement.NewBalance,
            statement.Debit,
            statement.Credit,
            statement.Lines.Count,
            statement.Status.ToString(),
            statement.JournalEntryId,
            Convert.ToBase64String(statement.RowVersion)),
        statement.Lines.OrderBy(x => x.LineNumber).Select(MapLine).ToArray());

    internal static BankStatementLineResponse MapLine(BankStatementLine line) => new(
        line.Id,
        line.LineNumber,
        line.PayerRecipientName,
        line.Debit,
        line.Credit,
        line.PaymentReference,
        line.Status.ToString(),
        line.PartnerAccountId,
        line.SubAccountId,
        line.CounterAccount,
        line.BankRef,
        Convert.ToBase64String(line.RowVersion));
}
