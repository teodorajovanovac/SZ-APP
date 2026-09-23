using System.Data;
using Microsoft.EntityFrameworkCore;
using SzApp.Contracts.LedgerBanking;
using SzApp.Data;
using SzApp.Data.Entities;
using SzApp.Data.Entities.LedgerBanking;
using SzApp.Domain;
using SzApp.Domain.Billing;
using SzApp.Domain.LedgerBanking;

namespace SzApp.Api.Features.LedgerBanking;

public interface IJournalPostingService
{
    Task<JournalEntryResponse> CreateDraftAsync(int companyId, CreateJournalEntryRequest request, CancellationToken cancellationToken);
    Task<PostingResultResponse> PostAsync(int companyId, int journalEntryId, int staffId, byte[] expectedRowVersion, CancellationToken cancellationToken);
    Task<PostingResultResponse> ReverseAsync(int companyId, int journalEntryId, int staffId, byte[] expectedRowVersion, CancellationToken cancellationToken);
    Task<LedgerPostingResult> PostSourceAsync(LedgerPostingRequest request, int staffId, CancellationToken cancellationToken);
}

public sealed class JournalPostingService(
    SzAppDbContext dbContext,
    LedgerMutationScope mutationScope,
    IShortListValidator shortLists,
    TimeProvider timeProvider) : IJournalPostingService
{
    public async Task<JournalEntryResponse> CreateDraftAsync(
        int companyId,
        CreateJournalEntryRequest request,
        CancellationToken cancellationToken)
    {
        ValidateDraftRequest(request);
        await EnsurePostingAccountsAsync(request.Lines.Select(x => x.Account), cancellationToken);
        await shortLists.EnsureTypeAsync(request.JournalEntryTypeId, "LedgerLineType", cancellationToken);

        var journal = new JournalEntry
        {
            CompanyId = companyId,
            PostingDate = request.PostingDate,
            DueDate = request.DueDate,
            Description = request.Description.Trim(),
            Currency = request.Currency.Trim().ToUpperInvariant(),
            JournalEntryTypeId = request.JournalEntryTypeId,
            IsPosted = false
        };

        var lineSources = new List<(LedgerEntry Line, LedgerLineRequest Source)>();
        foreach (var sourceLine in request.Lines)
        {
            var line = CreateLedgerLine(journal, companyId, request.PostingDate, sourceLine);
            journal.Lines.Add(line);
            lineSources.Add((line, sourceLine));
        }

        dbContext.JournalEntries.Add(journal);
        foreach (var (line, source) in lineSources)
        {
            SetOptionalLedgerProperties(line, source.SubAccountId, source.PartnerAccountId, null);
        }
        await dbContext.SaveChangesAsync(cancellationToken);
        return MapJournal(journal);
    }

    public Task<PostingResultResponse> PostAsync(
        int companyId,
        int journalEntryId,
        int staffId,
        byte[] expectedRowVersion,
        CancellationToken cancellationToken) =>
        ExecuteSerializableAsync(async () =>
        {
            await LockJournalAsync(journalEntryId, cancellationToken);
            var journal = await dbContext.JournalEntries.Include(x => x.Lines)
                .SingleOrDefaultAsync(x => x.Id == journalEntryId && x.CompanyId == companyId, cancellationToken)
                ?? throw new KeyNotFoundException("Nalog nije pronađen.");

            EnsureExpectedVersion(expectedRowVersion, journal.RowVersion);
            if (journal.IsPosted)
            {
                return new PostingResultResponse(journal.Id, true, Convert.ToBase64String(journal.RowVersion));
            }

            await PostTrackedJournalAsync(journal, staffId, cancellationToken);
            return new PostingResultResponse(journal.Id, false, Convert.ToBase64String(journal.RowVersion));
        }, cancellationToken);

    public Task<PostingResultResponse> ReverseAsync(
        int companyId,
        int journalEntryId,
        int staffId,
        byte[] expectedRowVersion,
        CancellationToken cancellationToken) =>
        ExecuteSerializableAsync(async () =>
        {
            await LockJournalAsync(journalEntryId, cancellationToken);
            var original = await dbContext.JournalEntries.Include(x => x.Lines)
                .SingleOrDefaultAsync(x => x.Id == journalEntryId && x.CompanyId == companyId, cancellationToken)
                ?? throw new KeyNotFoundException("Nalog nije pronađen.");

            EnsureExpectedVersion(expectedRowVersion, original.RowVersion);
            if (!original.IsPosted)
            {
                throw new DomainRuleException("journal.not-posted", "Samo knjižen nalog može biti storniran.");
            }

            var existing = await dbContext.JournalEntries.AsNoTracking()
                .SingleOrDefaultAsync(x => x.ReversalOfId == original.Id, cancellationToken);
            if (existing is not null)
            {
                return new PostingResultResponse(existing.Id, true, Convert.ToBase64String(existing.RowVersion));
            }

            var reversal = new JournalEntry
            {
                CompanyId = companyId,
                PostingDate = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime),
                DueDate = original.DueDate,
                Description = $"STORNO {original.Id}: {original.Description}",
                Currency = original.Currency,
                JournalEntryTypeId = original.JournalEntryTypeId,
                ReversalOfId = original.Id,
                IsPosted = false
            };

            foreach (var line in original.Lines.OrderBy(x => x.Priority))
            {
                reversal.Lines.Add(new LedgerEntry
                {
                    CompanyId = companyId,
                    Account = line.Account,
                    PostingDate = reversal.PostingDate,
                    DueDate = line.DueDate,
                    DebitAmount = line.CreditAmount,
                    CreditAmount = line.DebitAmount,
                    LineTypeId = line.LineTypeId,
                    DocumentRef = line.DocumentRef,
                    Parameters = line.Parameters,
                    Description = $"STORNO: {line.Description}",
                    Note = line.Note,
                    Priority = line.Priority
                });
            }

            dbContext.JournalEntries.Add(reversal);
            await dbContext.SaveChangesAsync(cancellationToken);
            await PostTrackedJournalAsync(reversal, staffId, cancellationToken);
            return new PostingResultResponse(reversal.Id, false, Convert.ToBase64String(reversal.RowVersion));
        }, cancellationToken);

    public Task<LedgerPostingResult> PostSourceAsync(
        LedgerPostingRequest request,
        int staffId,
        CancellationToken cancellationToken) =>
        ExecuteSerializableAsync(async () =>
        {
            PostingSchemeRegistry.EnsureAllowedSource(request.SourceType);
            if (request.Amount <= 0m)
            {
                throw new DomainRuleException("posting.invalid-amount", "Iznos za knjiženje mora biti pozitivan.");
            }

            var prior = await dbContext.Set<LedgerSourcePosting>().AsNoTracking()
                .SingleOrDefaultAsync(x => x.CompanyId == request.CompanyId &&
                                           x.SourceType == request.SourceType &&
                                           x.SourceId == request.SourceId, cancellationToken);
            if (prior is not null)
            {
                return new LedgerPostingResult(prior.JournalEntryId, true);
            }

            var keyCollision = await dbContext.Set<LedgerSourcePosting>().AsNoTracking()
                .AnyAsync(x => x.CompanyId == request.CompanyId &&
                               x.IdempotencyKey == request.IdempotencyKey, cancellationToken);
            if (keyCollision)
            {
                throw new DomainRuleException("idempotency.key-reused", "Idempotency-Key je već iskorišćen za drugu komandu.");
            }

            var scheme = await dbContext.Set<PostingScheme>().AsNoTracking()
                .Where(x => x.IsActive && x.SourceType == request.SourceType &&
                            (x.CompanyId == request.CompanyId || x.CompanyId == null))
                .OrderByDescending(x => x.CompanyId == request.CompanyId)
                .SingleOrDefaultAsync(cancellationToken)
                ?? throw new DomainRuleException("posting.scheme-missing", "Nije definisana šema knjiženja za izvor.");

            await EnsurePostingAccountsAsync([scheme.DebitAccount, scheme.CreditAccount], cancellationToken);
            var amount = FinanceRounding.Money(request.Amount);
            var journal = new JournalEntry
            {
                CompanyId = request.CompanyId,
                PostingDate = request.PostingDate,
                Description = scheme.DescriptionTemplate ?? $"{request.SourceType} {request.DocumentReference}",
                Currency = request.Currency.ToUpperInvariant(),
                IsPosted = false
            };
            journal.Lines.Add(new LedgerEntry
            {
                CompanyId = request.CompanyId,
                Account = scheme.DebitAccount,
                PostingDate = request.PostingDate,
                DebitAmount = amount,
                DocumentRef = request.DocumentReference,
                Priority = 1
            });
            journal.Lines.Add(new LedgerEntry
            {
                CompanyId = request.CompanyId,
                Account = scheme.CreditAccount,
                PostingDate = request.PostingDate,
                CreditAmount = amount,
                DocumentRef = request.DocumentReference,
                Priority = 2
            });

            dbContext.JournalEntries.Add(journal);
            await dbContext.SaveChangesAsync(cancellationToken);
            await PostTrackedJournalAsync(journal, staffId, cancellationToken);
            dbContext.Set<LedgerSourcePosting>().Add(new LedgerSourcePosting
            {
                CompanyId = request.CompanyId,
                SourceType = request.SourceType,
                SourceId = request.SourceId,
                JournalEntryId = journal.Id,
                IdempotencyKey = request.IdempotencyKey,
                CreatedAt = timeProvider.GetUtcNow()
            });
            await dbContext.SaveChangesAsync(cancellationToken);
            return new LedgerPostingResult(journal.Id, false);
        }, cancellationToken);

    private async Task PostTrackedJournalAsync(JournalEntry journal, int staffId, CancellationToken cancellationToken)
    {
        journal.Balance = LedgerBankingRules.ValidateJournal(
            journal.Lines.Select(x => new PostingAmounts(x.DebitAmount, x.CreditAmount)));
        journal.IsPosted = true;
        journal.PostedUserId = staffId;
        journal.PostedAt = timeProvider.GetUtcNow();

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
    }

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

    private Task LockJournalAsync(int journalEntryId, CancellationToken cancellationToken) =>
        dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT 1 FROM [finance].[JournalEntry] WITH (UPDLOCK, HOLDLOCK) WHERE [Id] = {journalEntryId}",
            cancellationToken);

    private Task SetPostingSessionContextAsync(bool enabled, CancellationToken cancellationToken) =>
        dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"EXEC sys.sp_set_session_context @key=N'szapp_allow_posting', @value={(enabled ? 1 : (int?)null)}",
            cancellationToken);

    private async Task EnsurePostingAccountsAsync(IEnumerable<string> accountCodes, CancellationToken cancellationToken)
    {
        var codes = accountCodes.Select(x => x.Trim()).Distinct(StringComparer.Ordinal).ToArray();
        var validCount = await dbContext.Set<ChartAccount>().AsNoTracking()
            .CountAsync(x => codes.Contains(x.Account) && x.IsActive && !x.IsSynthetic, cancellationToken);
        if (validCount != codes.Length)
        {
            throw new DomainRuleException("journal.invalid-account", "Sve stavke moraju koristiti aktivna analitička konta.");
        }
    }

    private static void ValidateDraftRequest(CreateJournalEntryRequest request)
    {
        if (request.Lines.Count == 0)
        {
            throw new DomainRuleException("journal.empty", "Nalog mora sadržati najmanje jednu stavku.");
        }

        if (string.IsNullOrWhiteSpace(request.Description) || request.Currency.Trim().Length != 3)
        {
            throw new DomainRuleException("journal.invalid-header", "Opis i troslovna valuta su obavezni.");
        }

        LedgerBankingRules.ValidateJournal(request.Lines.Select(x => new PostingAmounts(x.DebitAmount, x.CreditAmount)));
    }

    private LedgerEntry CreateLedgerLine(
        JournalEntry journal,
        int companyId,
        DateOnly postingDate,
        LedgerLineRequest request) => new()
        {
            JournalEntry = journal,
            CompanyId = companyId,
            Account = request.Account.Trim(),
            PostingDate = postingDate,
            DueDate = request.DueDate,
            DebitAmount = FinanceRounding.Calculation(request.DebitAmount),
            CreditAmount = FinanceRounding.Calculation(request.CreditAmount),
            DocumentRef = request.DocumentRef?.Trim(),
            Note = request.Note?.Trim(),
            Priority = journal.Lines.Count + 1
        };

    private void SetOptionalLedgerProperties(
        LedgerEntry line,
        string? subAccountId,
        int? partnerAccountId,
        int? bankStatementLineId)
    {
        var entry = dbContext.Entry(line);
        entry.Property("SubAccountId").CurrentValue = subAccountId;
        entry.Property("PartnerAccountId").CurrentValue = partnerAccountId;
        entry.Property("BankStatementLineId").CurrentValue = bankStatementLineId;
    }

    private static void EnsureExpectedVersion(byte[] expected, byte[] current)
    {
        if (expected.Length == 0 || !expected.AsSpan().SequenceEqual(current))
        {
            throw new DbUpdateConcurrencyException("Podatak je u međuvremenu izmenjen.");
        }
    }

    private static JournalEntryResponse MapJournal(JournalEntry journal) => new(
        new JournalEntrySummaryResponse(
            journal.Id,
            journal.PostingDate,
            journal.Description,
            journal.Currency,
            journal.Balance,
            journal.IsPosted,
            journal.PostedAt,
            journal.ReversalOfId,
            Convert.ToBase64String(journal.RowVersion)),
        journal.Lines.OrderBy(x => x.Priority).Select(x => new LedgerEntryResponse(
            x.Id,
            x.Account,
            x.PostingDate,
            x.DueDate,
            x.DebitAmount,
            x.CreditAmount,
            x.DocumentRef,
            null,
            null,
            x.Note)).ToArray());
}
