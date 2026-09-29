using Microsoft.EntityFrameworkCore;
using SzApp.Contracts.LedgerBanking;
using SzApp.Data.Entities;
using SzApp.Data.Entities.LedgerBanking;
using SzApp.Domain;
using SzApp.Domain.LedgerBanking;
using SzApp.Domain.LedgerBanking.StatementParsing;

namespace SzApp.Api.Features.LedgerBanking;

/// <summary>Flow B (5.9): review proposals, accept / split / re-assign, remember payer account, create template.</summary>
public sealed partial class BankStatementService
{
    public async Task<BankStatementResponse?> GetAsync(int companyId, int statementId, CancellationToken cancellationToken)
    {
        var statement = await dbContext.Set<BankStatement>().AsNoTracking()
            .Include(x => x.Lines).ThenInclude(x => x.Allocations)
            .SingleOrDefaultAsync(x => x.Id == statementId && x.CompanyId == companyId, cancellationToken);
        if (statement is null) return null;
        var names = await PartnerNamesAsync(statement.Lines.SelectMany(x => x.Allocations).Select(x => x.PartnerAccountId), cancellationToken);
        return MapStatement(statement, names);
    }

    public async Task<BankStatementResponse> RematchAsync(int companyId, int statementId, CancellationToken cancellationToken)
    {
        var statement = await LoadStatementAsync(companyId, statementId, cancellationToken);
        EnsureStatementMutable(statement);
        await matching.MatchStatementAsync(statement, cancellationToken);
        UpdateStatementStatus(statement);
        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetAsync(companyId, statementId, cancellationToken) ?? throw new KeyNotFoundException();
    }

    /// <summary>
    /// Enter: accept the stored proposals, or replace them with a manual split (across partners,
    /// references, or a plain counter-account). Every line must be fully allocated (FIN-15: no Ignored).
    /// </summary>
    public async Task<BankStatementLineResponse> AcceptLineAsync(int companyId, int lineId, AcceptBankStatementLineRequest request, CancellationToken cancellationToken)
    {
        var line = await LoadLineAsync(companyId, lineId, cancellationToken);
        EnsureStatementMutable(line.BankStatement);
        EnsureExpectedVersion(DecodeVersion(request.RowVersion), line.RowVersion);

        if (request.Allocations is { } manual)
        {
            var parts = await ValidateManualAllocationsAsync(companyId, manual, cancellationToken);
            foreach (var old in line.Allocations.ToArray())
            {
                line.Allocations.Remove(old);
                dbContext.Remove(old);
            }

            var index = 0;
            foreach (var part in parts)
            {
                line.Allocations.Add(BankStatementMatchingService.ToEntity(line, part, ++index));
            }

            line.MatchSource ??= MatchSources.Manual;
            line.MatchNote = null;
        }

        EnsureFullyAllocated(line);
        line.Status = BankStatementLineStatus.Matched;
        UpdateStatementStatus(line.BankStatement);
        await dbContext.SaveChangesAsync(cancellationToken);
        return await MapLineAsync(line, cancellationToken);
    }

    /// <summary>Esc: back to proposal (unaccepted), allocations kept.</summary>
    public async Task<BankStatementLineResponse> ReopenLineAsync(int companyId, int lineId, byte[] expectedRowVersion, CancellationToken cancellationToken)
    {
        var line = await LoadLineAsync(companyId, lineId, cancellationToken);
        EnsureStatementMutable(line.BankStatement);
        EnsureExpectedVersion(expectedRowVersion, line.RowVersion);
        line.Status = BankStatementLineStatus.Pending;
        UpdateStatementStatus(line.BankStatement);
        await dbContext.SaveChangesAsync(cancellationToken);
        return await MapLineAsync(line, cancellationToken);
    }

    /// <summary>"/" partner search result: re-run the split (reference, FIFO, advance) for that partner.</summary>
    public async Task<BankStatementLineResponse> AssignPartnerAsync(int companyId, int lineId, AssignPartnerRequest request, CancellationToken cancellationToken)
    {
        var line = await LoadLineAsync(companyId, lineId, cancellationToken);
        EnsureStatementMutable(line.BankStatement);
        EnsureExpectedVersion(DecodeVersion(request.RowVersion), line.RowVersion);
        await EnsurePartnerAccountAsync(companyId, request.PartnerAccountId, cancellationToken);
        await matching.ProposeForPartnerAsync(line, request.PartnerAccountId, cancellationToken);
        UpdateStatementStatus(line.BankStatement);
        await dbContext.SaveChangesAsync(cancellationToken);
        return await MapLineAsync(line, cancellationToken);
    }

    /// <summary>Ctrl+Enter: accept every "confident" proposal (template or own reference, no conflict).</summary>
    public async Task<BankStatementResponse> AcceptConfidentAsync(int companyId, int statementId, CancellationToken cancellationToken)
    {
        var statement = await LoadStatementAsync(companyId, statementId, cancellationToken);
        EnsureStatementMutable(statement);
        foreach (var line in statement.Lines.Where(x => x.Status == BankStatementLineStatus.Pending && x.IsConfidentMatch))
        {
            if (IsFullyAllocated(line)) line.Status = BankStatementLineStatus.Matched;
        }

        UpdateStatementStatus(statement);
        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetAsync(companyId, statementId, cancellationToken) ?? throw new KeyNotFoundException();
    }

    /// <summary>Legacy "Snimi TR kod partnera": the payer's account becomes the partner's bank account (step 2 next time).</summary>
    public async Task SavePayerAccountAsync(int companyId, int lineId, int partnerAccountId, CancellationToken cancellationToken)
    {
        var line = await LoadLineAsync(companyId, lineId, cancellationToken);
        var account = StatementText.NormalizeAccount(line.BankAccountNumber)
                      ?? throw new DomainRuleException("statement.no-payer-account", "Stavka nema račun uplatioca.");
        var partnerId = await EnsurePartnerAccountAsync(companyId, partnerAccountId, cancellationToken);
        var existing = await dbContext.Set<BankAccount>()
            .Where(x => x.PartnerId == partnerId && (x.CompanyId == companyId || x.CompanyId == null))
            .Select(x => x.AccountNumber)
            .ToArrayAsync(cancellationToken);
        if (existing.Any(x => StatementText.NormalizeAccount(x) == account)) return;
        dbContext.Set<BankAccount>().Add(new BankAccount
        {
            AccountNumber = account,
            PartnerId = partnerId,
            CompanyId = companyId,
            IsActive = true,
            Currency = "RSD"
        });
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Legacy "Kreiraj šablon": payer account (or name) equals + direction (Odobrenje=0 for outflows,
    /// Zaduzenje=0 for inflows) -> partner (GAP-05), optional sub-account (bank fees / AutoKontoTroska).
    /// </summary>
    public async Task<BankTemplateResponse> CreateTemplateFromLineAsync(int companyId, int lineId, CreateTemplateFromLineRequest request, CancellationToken cancellationToken)
    {
        var line = await LoadLineAsync(companyId, lineId, cancellationToken);
        await EnsurePartnerAccountAsync(companyId, request.PartnerAccountId, cancellationToken);
        var subAccountId = string.IsNullOrWhiteSpace(request.SubAccountId) ? null : request.SubAccountId.Trim();
        if (subAccountId is not null && !await dbContext.Set<SubAccount>().AnyAsync(x => x.Id == subAccountId, cancellationToken))
        {
            throw new DomainRuleException("journal.invalid-sub-account", "Podkonto ne postoji.");
        }

        var name = StatementText.Clip(string.IsNullOrWhiteSpace(request.Name) ? line.PayerRecipientName : request.Name, 255);
        var account = StatementText.NormalizeAccount(line.BankAccountNumber);
        var root = new BankStatementPostingTemplate
        {
            CompanyId = companyId,
            TemplateName = name,
            FieldName = account is null ? "NazivPN" : "BrojRacuna",
            FieldValue = account ?? StatementText.Clip(line.PayerRecipientName, 255),
            Function = BankTemplateFunction.Equals,
            SetPartnerAccountId = request.PartnerAccountId,
            SetSubAccountId = subAccountId,
            IsActive = true
        };
        var direction = new BankStatementPostingTemplate
        {
            CompanyId = companyId,
            Parent = root,
            TemplateName = name,
            FieldName = line.Credit > 0m ? "Zaduzenje" : "Odobrenje",
            FieldValue = "0",
            Function = BankTemplateFunction.Equals,
            SetPartnerAccountId = request.PartnerAccountId,
            SetSubAccountId = subAccountId,
            IsActive = true
        };
        dbContext.AddRange(root, direction);
        await dbContext.SaveChangesAsync(cancellationToken);
        return MapTemplate(root, [root, direction]);
    }

    internal static BankTemplateResponse MapTemplate(BankStatementPostingTemplate root, IEnumerable<BankStatementPostingTemplate> rows) => new(
        root.Id,
        root.TemplateName,
        root.SetPartnerAccountId,
        root.SetSubAccountId,
        root.IsActive,
        rows.Select(x => new BankTemplateConditionResponse(x.FieldName, x.Function.ToString(), x.FieldValue)).ToArray());

    private async Task<IReadOnlyList<AllocationProposal>> ValidateManualAllocationsAsync(
        int companyId, IReadOnlyCollection<BankStatementAllocationRequest> allocations, CancellationToken ct)
    {
        if (allocations.Count == 0)
        {
            throw new DomainRuleException("statement.no-allocation", "Stavka mora biti raspoređena na partnera ili konto.");
        }

        var partnerIds = allocations.Where(x => x.PartnerAccountId != null).Select(x => x.PartnerAccountId!.Value).Distinct().ToArray();
        var partnerAccounts = await dbContext.Set<PartnerAccount>().AsNoTracking()
            .Where(x => partnerIds.Contains(x.Id) && (x.CompanyId == companyId || x.CompanyId == null))
            .ToDictionaryAsync(x => x.Id, x => x.Account, ct);
        if (partnerAccounts.Count != partnerIds.Length)
        {
            throw new DomainRuleException("journal.invalid-partner-account", "Konto partnera ne pripada aktivnoj kompaniji.");
        }

        var result = allocations.Select(x =>
        {
            if (FinanceRounding.Money(x.Amount) == 0m)
            {
                throw new DomainRuleException("statement.zero-allocation", "Iznos raspodele ne može biti 0.");
            }

            // A partner's allocation always goes to that partner's own account (2040 / 4350).
            var account = x.PartnerAccountId is { } id ? partnerAccounts[id] : x.Account.Trim();
            return new AllocationProposal(account, x.PartnerAccountId, FinanceRounding.Money(x.Amount), AllocationKinds.Manual,
                string.IsNullOrWhiteSpace(x.SubAccountId) ? null : x.SubAccountId.Trim(),
                StatementText.ClipOrNull(x.Parameters, DocumentPostingRules.ParametersMaxLength),
                StatementText.ClipOrNull(x.DocumentRef, 50),
                x.InvoiceId, x.SupplierInvoiceId, x.CollectionPriority, x.ClosesDocumentType);
        }).ToArray();
        await EnsurePostingAccountsAsync(result.Select(x => x.Account), ct);
        return result;
    }

    private async Task<int> EnsurePartnerAccountAsync(int companyId, int partnerAccountId, CancellationToken ct) =>
        await dbContext.Set<PartnerAccount>().AsNoTracking()
            .Where(x => x.Id == partnerAccountId && (x.CompanyId == companyId || x.CompanyId == null))
            .Select(x => (int?)x.PartnerId)
            .SingleOrDefaultAsync(ct)
        ?? throw new DomainRuleException("journal.invalid-partner-account", "Konto partnera ne pripada aktivnoj kompaniji.");

    private static bool IsFullyAllocated(BankStatementLine line) =>
        line.Allocations.Count > 0 &&
        FinanceRounding.Money(line.Allocations.Sum(x => x.Amount)) == FinanceRounding.Money(line.Debit + line.Credit);

    private static void EnsureFullyAllocated(BankStatementLine line)
    {
        if (!IsFullyAllocated(line))
        {
            throw new DomainRuleException("statement.line-not-allocated",
                $"Stavka {line.LineNumber}: zbir raspodele ({line.Allocations.Sum(x => x.Amount):N2}) mora biti jednak iznosu ({line.Debit + line.Credit:N2}).");
        }
    }

    private async Task<BankStatement> LoadStatementAsync(int companyId, int statementId, CancellationToken ct) =>
        await dbContext.Set<BankStatement>()
            .Include(x => x.Lines).ThenInclude(x => x.Allocations)
            .SingleOrDefaultAsync(x => x.Id == statementId && x.CompanyId == companyId, ct)
        ?? throw new KeyNotFoundException("Izvod nije pronađen.");

    private async Task<BankStatementLine> LoadLineAsync(int companyId, int lineId, CancellationToken ct)
    {
        var line = await dbContext.Set<BankStatementLine>()
                       .Include(x => x.Allocations)
                       .SingleOrDefaultAsync(x => x.Id == lineId && x.CompanyId == companyId, ct)
                   ?? throw new KeyNotFoundException("Stavka izvoda nije pronađena.");
        // The statement status is derived from all its lines.
        await dbContext.Set<BankStatement>().Include(x => x.Lines).SingleAsync(x => x.Id == line.BankStatementId, ct);
        return line;
    }

    private static void UpdateStatementStatus(BankStatement statement)
    {
        if (statement.Status == BankStatementStatus.Posted) return;
        statement.Status = statement.Lines.All(x => x.Status == BankStatementLineStatus.Matched)
            ? BankStatementStatus.Ready
            : statement.Lines.Any(x => x.Status == BankStatementLineStatus.Matched)
                ? BankStatementStatus.PartiallyMatched
                : BankStatementStatus.Imported;
    }

    private static void EnsureStatementMutable(BankStatement statement)
    {
        if (statement.Status == BankStatementStatus.Posted)
        {
            throw new DomainRuleException("statement.posted-immutable", "Knjižen izvod se ne može menjati — prvo ga rasknjižite.");
        }
    }

    private async Task<Dictionary<int, string>> PartnerNamesAsync(IEnumerable<int?> ids, CancellationToken ct)
    {
        var wanted = ids.Where(x => x != null).Select(x => x!.Value).Distinct().ToArray();
        return wanted.Length == 0
            ? []
            : await dbContext.Set<PartnerAccount>().AsNoTracking()
                .Where(x => wanted.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.AccountNumber + " " + x.Partner.Name, ct);
    }

    private async Task<BankStatementLineResponse> MapLineAsync(BankStatementLine line, CancellationToken ct) =>
        MapLine(line, await PartnerNamesAsync(line.Allocations.Select(x => x.PartnerAccountId), ct));

    internal static BankStatementResponse MapStatement(BankStatement statement, IReadOnlyDictionary<int, string> names) => new(
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
        statement.Lines.OrderBy(x => x.LineNumber).Select(x => MapLine(x, names)).ToArray());

    internal static BankStatementLineResponse MapLine(BankStatementLine line, IReadOnlyDictionary<int, string> names) => new(
        line.Id,
        line.LineNumber,
        line.PayerRecipientName,
        line.BankAccountNumber,
        line.Info,
        line.Code,
        line.Debit,
        line.Credit,
        line.PaymentReference,
        line.Status.ToString(),
        line.MatchSource,
        line.IsConfidentMatch,
        line.MatchNote,
        line.BankRef,
        Convert.ToBase64String(line.RowVersion),
        line.Allocations.OrderBy(x => x.SortIndex).Select(x => new BankStatementAllocationResponse(
            x.Id, x.Account, x.PartnerAccountId,
            x.PartnerAccountId is { } id && names.TryGetValue(id, out var name) ? name : null,
            x.Amount, x.Kind, x.SubAccountId, x.Parameters, x.DocumentRef, x.InvoiceId,
            x.SupplierInvoiceId, x.CollectionPriority, x.ClosesDocumentType)).ToArray());
}
