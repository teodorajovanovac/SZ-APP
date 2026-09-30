using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SzApp.Contracts.LedgerBanking;
using SzApp.Data;
using SzApp.Data.Entities;
using SzApp.Data.Entities.LedgerBanking;
using SzApp.Domain;
using SzApp.Domain.LedgerBanking;
using SzApp.Domain.LedgerBanking.StatementParsing;

namespace SzApp.Api.Features.LedgerBanking;

public interface IBankStatementService
{
    Task<BankStatementResponse> ImportAsync(int companyId, BankStatementImportRequest request, CancellationToken cancellationToken);
    Task<BankStatementImportResultResponse> ImportFileAsync(int companyId, string fileName, byte[] content, int? bankCode, int? bankAccountId, CancellationToken cancellationToken);
    Task<BankStatementResponse?> GetAsync(int companyId, int statementId, CancellationToken cancellationToken);
    Task<BankStatementResponse> RematchAsync(int companyId, int statementId, CancellationToken cancellationToken);
    Task<BankStatementLineResponse> AcceptLineAsync(int companyId, int lineId, AcceptBankStatementLineRequest request, CancellationToken cancellationToken);
    Task<BankStatementLineResponse> ReopenLineAsync(int companyId, int lineId, byte[] expectedRowVersion, CancellationToken cancellationToken);
    Task<BankStatementLineResponse> AssignPartnerAsync(int companyId, int lineId, AssignPartnerRequest request, CancellationToken cancellationToken);
    Task<BankStatementResponse> AcceptConfidentAsync(int companyId, int statementId, CancellationToken cancellationToken);
    Task SavePayerAccountAsync(int companyId, int lineId, int partnerAccountId, CancellationToken cancellationToken);
    Task<BankTemplateResponse> CreateTemplateFromLineAsync(int companyId, int lineId, CreateTemplateFromLineRequest request, CancellationToken cancellationToken);
    Task<PostingResultResponse> PostAsync(int companyId, int statementId, int staffId, byte[] expectedRowVersion, CancellationToken cancellationToken);
    Task<PostingResultResponse> UnpostAsync(int companyId, int statementId, int staffId, byte[] expectedRowVersion, CancellationToken cancellationToken);
}

public sealed partial class BankStatementService(
    SzAppDbContext dbContext,
    BankStatementMatchingService matching,
    IJournalPostingService journals) : IBankStatementService
{
    /// <summary>Manual/JSON import (no file). Same checks as the file import, then auto-matching.</summary>
    public async Task<BankStatementResponse> ImportAsync(
        int companyId,
        BankStatementImportRequest request,
        CancellationToken cancellationToken)
    {
        var (statement, _) = await CreateAsync(companyId, request, null, cancellationToken);
        return await GetAsync(companyId, statement.Id, cancellationToken) ?? throw new KeyNotFoundException();
    }

    /// <summary>
    /// GAP-04: parse a bank file (format chosen or detected), resolve the company's bank account,
    /// check the sum (strict), duplicates (account, number, year) and continuity with the previous
    /// statement (FIN-14, warning only), create the statement (Imported) and propose matches.
    /// </summary>
    public async Task<BankStatementImportResultResponse> ImportFileAsync(
        int companyId,
        string fileName,
        byte[] content,
        int? bankCode,
        int? bankAccountId,
        CancellationToken cancellationToken)
    {
        var parser = BankStatementParsers.Resolve(fileName, bankCode, content);
        var parsed = BankStatementParsers.Parse(fileName, content, parser.BankCode);
        var warnings = new List<string>();
        var bankAccount = await ResolveCompanyBankAccountAsync(companyId, parsed.AccountNumber, bankAccountId, cancellationToken);

        if (parsed.DeclaredCount is { } count && count != parsed.Lines.Count)
        {
            throw new DomainRuleException("statement.count-mismatch", $"Izvod najavljuje {count} stavki, a sadrži {parsed.Lines.Count}.");
        }

        var existing = await dbContext.Set<BankStatement>().AsNoTracking()
            .Where(x => x.CompanyId == companyId && x.BankAccountId == bankAccount.Id &&
                        x.StatementNumber == parsed.StatementNumber && x.Date.Year == parsed.Date.Year)
            .Select(x => new { x.Id, x.Date })
            .FirstOrDefaultAsync(cancellationToken);
        if (existing is not null)
        {
            if (existing.Date != parsed.Date)
            {
                throw new DomainRuleException("statement.duplicate", $"Izvod br. {parsed.StatementNumber}/{parsed.Date.Year} za ovaj račun je već uvezen (datum {existing.Date:dd.MM.yyyy}).");
            }

            var already = await GetAsync(companyId, existing.Id, cancellationToken) ?? throw new KeyNotFoundException();
            return new BankStatementImportResultResponse(already, true, parser.BankCode, parser.Name,
                [$"Izvod br. {parsed.StatementNumber}/{parsed.Date.Year} je već uvezen."]);
        }

        var previous = await dbContext.Set<BankStatement>().AsNoTracking()
            .Where(x => x.CompanyId == companyId && x.BankAccountId == bankAccount.Id &&
                        (x.Date < parsed.Date || (x.Date == parsed.Date && x.StatementNumber < parsed.StatementNumber)))
            .OrderByDescending(x => x.Date).ThenByDescending(x => x.StatementNumber)
            .Select(x => new { x.StatementNumber, x.Date, x.NewBalance })
            .FirstOrDefaultAsync(cancellationToken);

        var debit = FinanceRounding.Money(parsed.Lines.Sum(x => x.Debit));
        var credit = FinanceRounding.Money(parsed.Lines.Sum(x => x.Credit));
        decimal previousBalance;
        decimal newBalance;
        if (parsed.PreviousBalance is { } fromFile)
        {
            previousBalance = fromFile;
            newBalance = parsed.NewBalance ?? fromFile + credit - debit;
            if (previous is not null && FinanceRounding.Money(previous.NewBalance) != FinanceRounding.Money(fromFile))
            {
                warnings.Add($"Prethodno stanje {fromFile:N2} ne odgovara novom stanju prethodnog izvoda br. {previous.StatementNumber} ({previous.NewBalance:N2}) — proverite da li nedostaje izvod.");
            }
        }
        else
        {
            // 170 TXT carries no balances (owner decision): continue from the previous statement.
            previousBalance = previous?.NewBalance ?? 0m;
            newBalance = previousBalance + credit - debit;
            warnings.Add(previous is null
                ? "Fajl nema stanja, a nema ni prethodnog izvoda — početno stanje je uzeto kao 0. Proverite."
                : $"Fajl nema stanja — početno stanje preuzeto sa izvoda br. {previous.StatementNumber} ({previousBalance:N2}).");
        }

        var request = new BankStatementImportRequest(
            bankAccount.Id,
            LedgerAccounts.Bank,
            parsed.StatementNumber,
            parsed.Date.Year.ToString(System.Globalization.CultureInfo.InvariantCulture),
            parsed.Date,
            previousBalance,
            newBalance,
            parsed.DeclaredDebit ?? debit,
            parsed.DeclaredCredit ?? credit,
            parsed.Lines.Select(x => new BankStatementLineImportRequest(
                x.LineNumber, x.PayerName, x.PayerAccount, x.Debit, x.Credit, x.Info, x.Code, x.PaymentReference, null, null)).ToArray());
        var (statement, alreadyImported) = await CreateAsync(companyId, request, StatementText.ClipOrNull(Path.GetFileName(fileName), 255), cancellationToken);
        var response = await GetAsync(companyId, statement.Id, cancellationToken) ?? throw new KeyNotFoundException();
        return new BankStatementImportResultResponse(response, alreadyImported, parser.BankCode, parser.Name, warnings);
    }

    private async Task<(BankStatement Statement, bool AlreadyImported)> CreateAsync(
        int companyId,
        BankStatementImportRequest request,
        string? sourceFileName,
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

        await EnsurePostingAccountsAsync([request.LedgerAccount.Trim()], cancellationToken);
        var suffix = request.StatementSuffix?.Trim();
        var existing = await dbContext.Set<BankStatement>().AsNoTracking().Include(x => x.Lines)
            .SingleOrDefaultAsync(x => x.CompanyId == companyId &&
                                       x.BankAccountId == request.BankAccountId &&
                                       x.StatementNumber == request.StatementNumber &&
                                       x.StatementSuffix == suffix &&
                                       x.Date == request.Date, cancellationToken);
        if (existing is not null)
        {
            if (existing.PreviousBalance != totals.PreviousBalance || existing.NewBalance != totals.NewBalance ||
                existing.Debit != totals.Debit || existing.Credit != totals.Credit ||
                existing.Lines.Count != request.Lines.Count)
            {
                throw new DomainRuleException("statement.duplicate-conflict", "Izvod sa istim identitetom već postoji sa drugačijim sadržajem.");
            }

            return (existing, true);
        }

        var statement = new BankStatement
        {
            CompanyId = companyId,
            BankAccountId = request.BankAccountId,
            LedgerAccount = request.LedgerAccount.Trim(),
            StatementNumber = request.StatementNumber,
            StatementSuffix = suffix,
            Date = request.Date,
            PreviousBalance = totals.PreviousBalance,
            NewBalance = totals.NewBalance,
            Debit = totals.Debit,
            Credit = totals.Credit,
            CountDebitEntry = totals.DebitCount,
            CountCreditEntry = totals.CreditCount,
            SourceFileName = sourceFileName,
            Status = BankStatementStatus.Imported
        };

        foreach (var line in request.Lines.OrderBy(x => x.LineNumber))
        {
            statement.Lines.Add(new BankStatementLine
            {
                CompanyId = companyId,
                LineNumber = line.LineNumber,
                PayerRecipientName = StatementText.Clip(line.PayerRecipientName, 255),
                BankAccountNumber = StatementText.ClipOrNull(StatementText.NormalizeAccount(line.BankAccountNumber), 50),
                Debit = FinanceRounding.Money(line.Debit),
                Credit = FinanceRounding.Money(line.Credit),
                Info = StatementText.ClipOrNull(line.Info, 255),
                Code = line.Code,
                PaymentReference = StatementText.ClipOrNull(line.PaymentReference, 50),
                PaymentReferenceOut = StatementText.ClipOrNull(line.PaymentReferenceOut, 50),
                BankRef = StatementText.ClipOrNull(line.BankRef, 50),
                Status = BankStatementLineStatus.Pending
            });
        }

        dbContext.Set<BankStatement>().Add(statement);
        await matching.MatchStatementAsync(statement, cancellationToken); // GAP-05: proposals right after import
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqlException { Number: 2601 or 2627 })
        {
            // The unique index is the real dedup guard for concurrent imports racing past the read above.
            throw new DomainRuleException("statement.duplicate", "Ovaj izvod je već uvezen.");
        }

        return (statement, false);
    }

    private async Task<BankAccount> ResolveCompanyBankAccountAsync(int companyId, string? fileAccount, int? bankAccountId, CancellationToken ct)
    {
        var accounts = await dbContext.Set<BankAccount>().AsNoTracking()
            .Where(x => x.CompanyId == companyId && x.PartnerId == null && x.IsActive)
            .ToArrayAsync(ct);
        var normalized = StatementText.NormalizeAccount(fileAccount);
        var byFile = normalized is null ? null : accounts.FirstOrDefault(x => StatementText.NormalizeAccount(x.AccountNumber) == normalized);
        if (bankAccountId is { } chosenId)
        {
            var chosen = accounts.FirstOrDefault(x => x.Id == chosenId)
                         ?? throw new DomainRuleException("statement.invalid-bank-account", "Bankovni račun ne pripada kompaniji ili nije aktivan.");
            if (normalized is not null && StatementText.NormalizeAccount(chosen.AccountNumber) != normalized)
            {
                throw new DomainRuleException("statement.account-mismatch", $"Račun iz fajla ({normalized}) ne odgovara izabranom računu ({chosen.AccountNumber}).");
            }

            return chosen;
        }

        return byFile ?? throw new DomainRuleException("statement.unknown-account",
            normalized is null
                ? "Račun nije naveden u fajlu — izaberite račun ručno."
                : $"Račun {normalized} nije aktivan tekući račun ove kompanije.");
    }

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

    private static void EnsureExpectedVersion(byte[] expected, byte[] current)
    {
        if (expected.Length == 0 || !expected.AsSpan().SequenceEqual(current))
        {
            throw new DbUpdateConcurrencyException("Podatak je u međuvremenu izmenjen.");
        }
    }

    internal static byte[] DecodeVersion(string value)
    {
        try
        {
            return Convert.FromBase64String(value);
        }
        catch (FormatException exception)
        {
            throw new Microsoft.AspNetCore.Http.BadHttpRequestException("RowVersion nije ispravan Base64.", exception);
        }
    }
}
