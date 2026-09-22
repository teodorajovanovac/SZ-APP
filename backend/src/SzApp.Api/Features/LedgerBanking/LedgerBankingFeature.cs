using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using SzApp.Api.Infrastructure;
using SzApp.Api.Security;
using SzApp.Contracts.LedgerBanking;
using SzApp.Data;
using SzApp.Data.Entities;
using SzApp.Data.Entities.LedgerBanking;
using SzApp.Domain.Billing;

namespace SzApp.Api.Features.LedgerBanking;

public sealed class LedgerBankingOptions
{
    public int? SystemUserId { get; set; }
}

public sealed class LedgerPostingGateway(
    IJournalPostingService journalPostingService,
    SzAppDbContext dbContext,
    IHttpContextAccessor httpContextAccessor,
    IOptions<LedgerBankingOptions> options) : ILedgerPostingGateway
{
    public Task<LedgerPostingResult> PostAsync(LedgerPostingRequest request, CancellationToken cancellationToken)
    {
        var claim = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var staffId = int.TryParse(claim, out var currentUserId)
            ? currentUserId
            : options.Value.SystemUserId ?? throw new InvalidOperationException("LedgerBanking:SystemUserId je obavezan za pozadinska knjiženja.");
        return journalPostingService.PostSourceAsync(request, staffId, cancellationToken);
    }

    public async Task<LedgerPostingResult> ReverseAsync(
        LedgerReversalRequest request,
        CancellationToken cancellationToken)
    {
        var claim = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var staffId = int.TryParse(claim, out var currentUserId)
            ? currentUserId
            : options.Value.SystemUserId ?? throw new InvalidOperationException("LedgerBanking:SystemUserId je obavezan za pozadinska knjiženja.");

        var sourcePosting = await dbContext.Set<LedgerSourcePosting>().AsNoTracking()
            .SingleOrDefaultAsync(x => x.CompanyId == request.CompanyId &&
                                       x.SourceType == request.SourceType &&
                                       x.SourceId == request.SourceId &&
                                       x.JournalEntryId == request.JournalEntryId, cancellationToken)
            ?? throw new InvalidOperationException("Izvorno knjiženje nije pronađeno.");
        var journal = await dbContext.JournalEntries.AsNoTracking()
            .SingleAsync(x => x.Id == sourcePosting.JournalEntryId, cancellationToken);
        var result = await journalPostingService.ReverseAsync(
            request.CompanyId,
            journal.Id,
            staffId,
            journal.RowVersion,
            cancellationToken);
        return new LedgerPostingResult(result.JournalEntryId, result.AlreadyPosted);
    }
}

public static class LedgerBankingFeature
{
    public static IServiceCollection AddLedgerBankingFeature(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<LedgerBankingOptions>(configuration.GetSection("LedgerBanking"));
        services.TryAddSingleton(TimeProvider.System);
        services.AddHttpContextAccessor();
        services.AddScoped<LedgerMutationScope>();
        services.AddScoped<LedgerMutationGuardInterceptor>();
        services.AddScoped<IJournalPostingService, JournalPostingService>();
        services.AddScoped<IBankStatementService, BankStatementService>();
        services.AddScoped<ILedgerPostingGateway, LedgerPostingGateway>();
        services.AddDbContext<SzAppDbContext>((serviceProvider, options) =>
            options.AddInterceptors(serviceProvider.GetRequiredService<LedgerMutationGuardInterceptor>()));
        return services;
    }

    public static IEndpointRouteBuilder MapLedgerBankingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/companies/{companyId:int}")
            .WithTags("Ledger & Banking")
            .RequireAuthorization(SecurityConstants.CompanyAccessPolicy);

        MapJournalEndpoints(group);
        MapBankingEndpoints(group);
        MapCatalogueEndpoints(group);
        return endpoints;
    }

    private static void MapJournalEndpoints(RouteGroupBuilder group)
    {
        group.MapGet("/journal-entries", async (
            int companyId,
            int? page,
            int? pageSize,
            bool? isPosted,
            SzAppDbContext db,
            CancellationToken cancellationToken) =>
        {
            var safePage = Math.Max(page ?? 1, 1);
            var safePageSize = Math.Clamp(pageSize ?? 25, 1, 100);
            var query = db.JournalEntries.AsNoTracking().Where(x => x.CompanyId == companyId);
            if (isPosted.HasValue)
            {
                query = query.Where(x => x.IsPosted == isPosted.Value);
            }

            var total = await query.CountAsync(cancellationToken);
            var items = await query.OrderByDescending(x => x.PostingDate).ThenByDescending(x => x.Id)
                .Skip((safePage - 1) * safePageSize).Take(safePageSize)
                .Select(x => new JournalEntrySummaryResponse(
                    x.Id, x.PostingDate, x.Description, x.Currency, x.Balance, x.IsPosted,
                    x.PostedAt, x.ReversalOfId, Convert.ToBase64String(x.RowVersion)))
                .ToArrayAsync(cancellationToken);
            return Results.Ok(new PageResponse<JournalEntrySummaryResponse>(items, safePage, safePageSize, total));
        });

        group.MapGet("/journal-entries/{id:int}", async (
            int companyId,
            int id,
            SzAppDbContext db,
            CancellationToken cancellationToken) =>
        {
            var journal = await db.JournalEntries.AsNoTracking().Include(x => x.Lines)
                .SingleOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId, cancellationToken);
            return journal is null ? Results.NotFound() : Results.Ok(MapJournal(journal, db));
        });

        group.MapPost("/journal-entries", async (
            int companyId,
            CreateJournalEntryRequest request,
            IJournalPostingService service,
            CancellationToken cancellationToken) =>
            Results.Created("", await service.CreateDraftAsync(companyId, request, cancellationToken)))
            .AddEndpointFilter<AntiforgeryEndpointFilter>();

        group.MapPost("/journal-entries/{id:int}/post", async (
            int companyId,
            int id,
            ConcurrencyCommandRequest request,
            ClaimsPrincipal principal,
            IJournalPostingService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.PostAsync(
                companyId, id, GetStaffId(principal), DecodeVersion(request.RowVersion), cancellationToken)))
            .AddEndpointFilter<IdempotencyKeyEndpointFilter>()
            .AddEndpointFilter<AntiforgeryEndpointFilter>();

        group.MapPost("/journal-entries/{id:int}/reverse", async (
            int companyId,
            int id,
            ConcurrencyCommandRequest request,
            ClaimsPrincipal principal,
            IJournalPostingService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.ReverseAsync(
                companyId, id, GetStaffId(principal), DecodeVersion(request.RowVersion), cancellationToken)))
            .AddEndpointFilter<IdempotencyKeyEndpointFilter>()
            .AddEndpointFilter<AntiforgeryEndpointFilter>();
    }

    private static void MapBankingEndpoints(RouteGroupBuilder group)
    {
        group.MapGet("/bank-statements", async (
            int companyId,
            int? page,
            int? pageSize,
            SzAppDbContext db,
            CancellationToken cancellationToken) =>
        {
            var safePage = Math.Max(page ?? 1, 1);
            var safePageSize = Math.Clamp(pageSize ?? 25, 1, 100);
            var query = db.Set<BankStatement>().AsNoTracking().Where(x => x.CompanyId == companyId);
            var total = await query.CountAsync(cancellationToken);
            var items = await query.OrderByDescending(x => x.Date).ThenByDescending(x => x.StatementNumber)
                .Skip((safePage - 1) * safePageSize).Take(safePageSize)
                .Select(x => new BankStatementSummaryResponse(
                    x.Id, x.BankAccountId, x.StatementNumber, x.StatementSuffix, x.Date,
                    x.PreviousBalance, x.NewBalance, x.Debit, x.Credit, x.Lines.Count,
                    x.Status.ToString(), x.JournalEntryId, Convert.ToBase64String(x.RowVersion)))
                .ToArrayAsync(cancellationToken);
            return Results.Ok(new PageResponse<BankStatementSummaryResponse>(items, safePage, safePageSize, total));
        });

        group.MapGet("/bank-statements/{id:int}", async (
            int companyId,
            int id,
            SzAppDbContext db,
            CancellationToken cancellationToken) =>
        {
            var statement = await db.Set<BankStatement>().AsNoTracking().Include(x => x.Lines)
                .SingleOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId, cancellationToken);
            return statement is null ? Results.NotFound() : Results.Ok(BankStatementService.MapStatement(statement));
        });

        group.MapPost("/bank-statements/import", async (
            int companyId,
            BankStatementImportRequest request,
            IBankStatementService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.ImportAsync(companyId, request, cancellationToken)))
            .AddEndpointFilter<IdempotencyKeyEndpointFilter>()
            .AddEndpointFilter<AntiforgeryEndpointFilter>();

        group.MapPost("/bank-statement-lines/{id:int}/match", async (
            int companyId,
            int id,
            MatchBankStatementLineRequest request,
            IBankStatementService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.MatchAsync(companyId, id, request, cancellationToken)))
            .AddEndpointFilter<AntiforgeryEndpointFilter>();

        group.MapPost("/bank-statement-lines/{id:int}/ignore", async (
            int companyId,
            int id,
            ConcurrencyCommandRequest request,
            IBankStatementService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.IgnoreAsync(companyId, id, DecodeVersion(request.RowVersion), cancellationToken)))
            .AddEndpointFilter<AntiforgeryEndpointFilter>();

        group.MapPost("/bank-statements/{id:int}/post", async (
            int companyId,
            int id,
            ConcurrencyCommandRequest request,
            ClaimsPrincipal principal,
            IBankStatementService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.PostAsync(
                companyId, id, GetStaffId(principal), DecodeVersion(request.RowVersion), cancellationToken)))
            .AddEndpointFilter<IdempotencyKeyEndpointFilter>()
            .AddEndpointFilter<AntiforgeryEndpointFilter>();
    }

    private static void MapCatalogueEndpoints(RouteGroupBuilder group)
    {
        group.MapGet("/chart-of-accounts", async (SzAppDbContext db, CancellationToken cancellationToken) =>
            Results.Ok(await db.Set<ChartAccount>().AsNoTracking().OrderBy(x => x.Account)
                .Select(x => new ChartAccountResponse(
                    x.Account, x.ShortName, x.Name, x.ParentAccount, x.Sign, x.IsActive, x.IsSynthetic))
                .ToArrayAsync(cancellationToken)));

        group.MapPut("/chart-of-accounts/{account}", async (
            string account,
            SaveChartAccountRequest request,
            SzAppDbContext db,
            CancellationToken cancellationToken) =>
        {
            var entity = await db.Set<ChartAccount>().SingleOrDefaultAsync(x => x.Account == account, cancellationToken);
            if (entity is null)
            {
                entity = new ChartAccount { Account = account };
                db.Add(entity);
            }

            entity.ShortName = request.ShortName;
            entity.Name = request.Name.Trim();
            entity.ParentAccount = request.ParentAccount;
            entity.Level = account.Length;
            entity.Sign = request.Sign;
            entity.IsActive = request.IsActive;
            entity.IsSynthetic = request.IsSynthetic;
            await db.SaveChangesAsync(cancellationToken);
            return Results.Ok(new ChartAccountResponse(
                entity.Account, entity.ShortName, entity.Name, entity.ParentAccount,
                entity.Sign, entity.IsActive, entity.IsSynthetic));
        }).AddEndpointFilter<AntiforgeryEndpointFilter>();

        group.MapGet("/sub-accounts", async (SzAppDbContext db, CancellationToken cancellationToken) =>
            Results.Ok(await db.Set<SubAccount>().AsNoTracking().OrderBy(x => x.Id)
                .Select(x => new SubAccountResponse(x.Id, x.Name, x.ParentSubAccountId, x.IsActive))
                .ToArrayAsync(cancellationToken)));

        group.MapPut("/sub-accounts/{id}", async (
            string id,
            SaveSubAccountRequest request,
            SzAppDbContext db,
            CancellationToken cancellationToken) =>
        {
            var entity = await db.Set<SubAccount>().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (entity is null)
            {
                entity = new SubAccount { Id = id };
                db.Add(entity);
            }

            entity.Name = request.Name.Trim();
            entity.ParentSubAccountId = request.ParentSubAccountId;
            entity.IsActive = request.IsActive;
            await db.SaveChangesAsync(cancellationToken);
            return Results.Ok(new SubAccountResponse(entity.Id, entity.Name, entity.ParentSubAccountId, entity.IsActive));
        }).AddEndpointFilter<AntiforgeryEndpointFilter>();
    }

    private static int GetStaffId(ClaimsPrincipal principal) =>
        int.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var staffId)
            ? staffId
            : throw new UnauthorizedAccessException();

    private static byte[] DecodeVersion(string value)
    {
        try
        {
            return Convert.FromBase64String(value);
        }
        catch (FormatException exception)
        {
            throw new BadHttpRequestException("RowVersion nije ispravan Base64.", exception);
        }
    }

    private static JournalEntryResponse MapJournal(JournalEntry journal, SzAppDbContext db) => new(
        new JournalEntrySummaryResponse(
            journal.Id, journal.PostingDate, journal.Description, journal.Currency, journal.Balance,
            journal.IsPosted, journal.PostedAt, journal.ReversalOfId, Convert.ToBase64String(journal.RowVersion)),
        journal.Lines.OrderBy(x => x.Priority).Select(x =>
        {
            var entry = db.Entry(x);
            return new LedgerEntryResponse(
                x.Id, x.Account, x.PostingDate, x.DueDate, x.DebitAmount, x.CreditAmount,
                x.DocumentRef, entry.Property<string?>("SubAccountId").CurrentValue,
                entry.Property<int?>("PartnerAccountId").CurrentValue, x.Note);
        }).ToArray());
}
