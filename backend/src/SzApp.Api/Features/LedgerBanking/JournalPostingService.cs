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
    TimeProvider timeProvider,
    PostingPeriodGuard periodGuard,
    IBusinessClock clock) : IJournalPostingService
{
    public async Task<JournalEntryResponse> CreateDraftAsync(
        int companyId,
        CreateJournalEntryRequest request,
        CancellationToken cancellationToken)
    {
        ValidateDraftRequest(request);
        await EnsurePostingAccountsAsync(request.Lines.Select(x => x.Account), cancellationToken);
        await shortLists.EnsureTypeAsync(request.JournalEntryTypeId, "LedgerLineType", cancellationToken);
        await EnsurePartnerAccountsAsync(companyId, request.Lines.Select(x => x.PartnerAccountId), cancellationToken);
        await EnsureSubAccountsAsync(request.Lines.Select(x => x.SubAccountId), cancellationToken);

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

            // FIN-07: a journal that belongs to a source document (invoice batch, supplier invoice,
            // bank statement) must be reversed through that document so its status is reset too.
            var hasSource = await dbContext.Set<LedgerSourcePosting>().AnyAsync(x => x.JournalEntryId == original.Id, cancellationToken) ||
                            await dbContext.Set<BankStatement>().AnyAsync(x => x.JournalEntryId == original.Id, cancellationToken);
            if (hasSource)
            {
                throw new DomainRuleException("journal.reverse-via-source", "Nalog dokumenta se stornira preko samog dokumenta (npr. storno računa).");
            }

            // P9 / FIN-06: red storno -- same lines, negative amounts on the same side, type 7,
            // every document link (partner, sub-account, statement line, invoice) copied.
            var stornoDate = clock.Today;
            var reversal = new JournalEntry
            {
                CompanyId = companyId,
                PostingDate = stornoDate,
                DueDate = original.DueDate,
                Description = Clip($"STORNO {original.Id}: {original.Description}", 255),
                Currency = original.Currency,
                JournalEntryTypeId = original.JournalEntryTypeId,
                ReversalOfId = original.Id,
                IsPosted = false
            };
            var lines = DocumentPostingRules.Negate(original.Lines.OrderBy(x => x.Priority).Select(ToPostingLine), stornoDate);
            await AddLinesAsync(reversal, lines, cancellationToken);
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
            if (request.Lines.Count == 0)
            {
                throw new DomainRuleException("posting.empty", "Dokument nema stavki za knjiženje.");
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

            // Lines are built by DocumentPostingRules (9.2) in the calling module; this only
            // validates references and posts them as one journal.
            await EnsurePostingAccountsAsync(request.Lines.Select(x => x.Account), cancellationToken);
            await EnsurePartnerAccountsAsync(request.CompanyId, request.Lines.Select(x => x.PartnerAccountId), cancellationToken);
            await EnsureSubAccountsAsync(request.Lines.Select(x => x.SubAccountId), cancellationToken);
            var journal = new JournalEntry
            {
                CompanyId = request.CompanyId,
                PostingDate = request.PostingDate,
                Description = Clip(request.Description, 255),
                Currency = request.Currency.ToUpperInvariant(),
                IsPosted = false
            };
            await AddLinesAsync(journal, request.Lines, cancellationToken);
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
        await periodGuard.EnsureOpenAsync(
            journal.CompanyId, journal.Lines.Select(x => x.PostingDate).Append(journal.PostingDate).ToArray(), cancellationToken);
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
        // Document posting (invoice batch, supplier invoice, storno) runs inside the caller's
        // transaction so the journal and the source status change commit or roll back together.
        if (dbContext.Database.CurrentTransaction is not null)
        {
            return await operation();
        }

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

    // SEC-06: PartnerAccountId comes from the client per line and was written straight into the
    // shadow property with no ownership check -- a manual journal entry could reference another
    // company's partner account. PartnerAccount.CompanyId can be null (shared partner accounts,
    // same convention as Partner), so those remain allowed for any company.
    private async Task EnsurePartnerAccountsAsync(int companyId, IEnumerable<int?> partnerAccountIds, CancellationToken cancellationToken)
    {
        var ids = partnerAccountIds.Where(x => x is not null).Select(x => x!.Value).Distinct().ToArray();
        if (ids.Length == 0) return;
        var validCount = await dbContext.Set<PartnerAccount>().AsNoTracking()
            .CountAsync(x => ids.Contains(x.Id) && (x.CompanyId == companyId || x.CompanyId == null), cancellationToken);
        if (validCount != ids.Length)
        {
            throw new DomainRuleException("journal.invalid-partner-account", "Konto partnera ne pripada aktivnoj kompaniji.");
        }
    }

    // SubAccount is a global chart of sub-accounts (no CompanyId) -- not an IDOR risk, but it was
    // never validated to even exist before being written onto the ledger line.
    private async Task EnsureSubAccountsAsync(IEnumerable<string?> subAccountIds, CancellationToken cancellationToken)
    {
        var ids = subAccountIds.Where(x => x is not null).Select(x => x!).Distinct(StringComparer.Ordinal).ToArray();
        if (ids.Length == 0) return;
        var validCount = await dbContext.Set<SubAccount>().AsNoTracking()
            .CountAsync(x => ids.Contains(x.Id), cancellationToken);
        if (validCount != ids.Length)
        {
            throw new DomainRuleException("journal.invalid-sub-account", "Podkonto ne postoji.");
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

    private async Task AddLinesAsync(JournalEntry journal, IEnumerable<PostingLine> lines, CancellationToken cancellationToken)
    {
        var source = lines.ToArray();
        var lineTypeIds = await ResolveLineTypeIdsAsync(source.Select(x => x.LineType), cancellationToken);
        var created = new List<(LedgerEntry Entry, PostingLine Source)>();
        foreach (var line in source)
        {
            var entry = new LedgerEntry
            {
                JournalEntry = journal,
                CompanyId = journal.CompanyId,
                Account = line.Account,
                PostingDate = line.PostingDate,
                DueDate = line.DueDate,
                DebitAmount = FinanceRounding.Calculation(line.Debit),
                CreditAmount = FinanceRounding.Calculation(line.Credit),
                LineTypeId = lineTypeIds[line.LineType],
                DocumentRef = line.DocumentRef,
                Parameters = line.Parameters,
                Note = line.Note,
                Description = line.Description,
                InvoiceId = line.InvoiceId,
                SupplierInvoiceId = line.SupplierInvoiceId,
                CollectionPriority = line.CollectionPriority,
                ClosesDocumentType = line.ClosesDocumentType,
                Priority = journal.Lines.Count + 1
            };
            journal.Lines.Add(entry);
            created.Add((entry, line));
        }

        dbContext.JournalEntries.Add(journal);
        foreach (var (entry, line) in created)
        {
            SetOptionalLedgerProperties(entry, line.SubAccountId, line.PartnerAccountId, line.BankStatementLineId);
        }
    }

    private async Task<Dictionary<int, int>> ResolveLineTypeIdsAsync(IEnumerable<int> codes, CancellationToken cancellationToken)
    {
        var wanted = codes.Distinct().ToArray();
        var map = await dbContext.ShortLists.AsNoTracking()
            .Where(x => x.TableName == LedgerLineTypes.ShortListTable && wanted.Contains(x.IndexValue))
            .ToDictionaryAsync(x => x.IndexValue, x => x.Id, cancellationToken);
        var missing = wanted.Where(x => !map.ContainsKey(x)).ToArray();
        if (missing.Length > 0)
        {
            throw new DomainRuleException("posting.line-type-missing", $"Tip stavke GK nije definisan u šifarniku: {string.Join(", ", missing)}.");
        }

        return map;
    }

    /// <summary>Tracked ledger line → PostingLine (shadow links read through the change tracker).</summary>
    private PostingLine ToPostingLine(LedgerEntry line)
    {
        var entry = dbContext.Entry(line);
        return new PostingLine(
            line.Account, line.DebitAmount, line.CreditAmount, 0, line.PostingDate,
            PartnerAccountId: entry.Property<int?>("PartnerAccountId").CurrentValue,
            SubAccountId: entry.Property<string?>("SubAccountId").CurrentValue,
            DueDate: line.DueDate,
            DocumentRef: line.DocumentRef,
            Parameters: line.Parameters,
            InvoiceId: line.InvoiceId,
            SupplierInvoiceId: line.SupplierInvoiceId,
            CollectionPriority: line.CollectionPriority,
            Note: line.Note,
            BankStatementLineId: entry.Property<int?>("BankStatementLineId").CurrentValue,
            ClosesDocumentType: line.ClosesDocumentType,
            Description: line.Description is null ? null : Clip($"STORNO: {line.Description}", 255));
    }

    private static string Clip(string value, int maxLength) => value.Length <= maxLength ? value : value[..maxLength];

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
