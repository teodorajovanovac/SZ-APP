using System.Data;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SzApp.Api.Features.MasterData;
using SzApp.Api.Security;
using SzApp.Contracts.Billing;
using SzApp.Data;
using SzApp.Data.Entities;
using SzApp.Data.Entities.Billing;
using SzApp.Domain;
using SzApp.Domain.Billing;
using SzApp.Domain.LedgerBanking;
using Invoice = SzApp.Data.Entities.Invoice;
using InvoiceLine = SzApp.Data.Entities.InvoiceLine;

namespace SzApp.Api.Features.Billing;

/// <summary>
/// Server-side invoice engine (audit 9.1, FIN-03): R0 aggregation -> R1 per-type formula ->
/// R2 header per customer (live 2040 balance) -> R3 numbering (RBR + KB97) -> benefit archive.
/// Invoices land in an InvoiceBatch with the same shadow columns as the legacy path, so the
/// existing batch posting (DocumentPostingRules) posts them unchanged.
/// </summary>
public sealed class InvoiceGenerationService(SzAppDbContext db, TimeProvider timeProvider)
{
    /// <summary>Legacy supplier "upravnik" account number whose lines a benefit zeroes (FIN-18).</summary>
    public const int ManagerSupplierAccountNumber = 9001;

    /// <summary>MasterAccountId = GAP-12 group master (Contract.InvoiceLegacyMasterId, a 2040 PartnerAccount.Id).</summary>
    private sealed record CustomerDraft(
        int CustomerId, Partner Partner, PartnerAccount Account, Address? Address,
        IReadOnlyList<R1CustomerLine> Lines, decimal PreviousDebt, decimal TakenOverDebt, int? MasterAccountId);

    /// <summary>
    /// Per-building preview. Interest is shown when the (company, period, marker) batch already has an
    /// interest run (the wizard runs it on the draft batch before previewing). Total excludes interest,
    /// so the ±% against the previous batch compares like with like.
    /// </summary>
    public async Task<InvoiceBatchPreviewV2Response> PreviewAsync(int companyId, int periodYYMM, string? marker, decimal nbsRate, CancellationToken ct)
    {
        marker = Normalize(marker);
        var drafts = await BuildAsync(companyId, periodYYMM, marker, nbsRate, ct);
        var batchId = await db.Set<InvoiceBatch>().AsNoTracking()
            .Where(x => x.CompanyId == companyId && x.PeriodYYMM == periodYYMM && x.ExtraordinaryInvoiceMarker == marker)
            .Select(x => (int?)x.Id).SingleOrDefaultAsync(ct);
        var interest = batchId is { } id
            ? (await LoadInterestTransferAsync(db, companyId, id, ct)).GroupBy(x => x.PartnerId).ToDictionary(g => g.Key, g => g.Sum(x => x.Total.Interest))
            : [];
        var customers = drafts.Select(d =>
        {
            var net = FinanceRounding.Money(d.Lines.Sum(x => x.Result.NetAmount));
            var vat = FinanceRounding.Money(d.Lines.Sum(x => x.Result.VatAmount));
            return new CustomerInvoicePreviewV2(d.CustomerId, d.Partner.Name, net, vat, interest.GetValueOrDefault(d.CustomerId), FinanceRounding.Money(net + vat));
        }).ToArray();
        var total = FinanceRounding.Money(customers.Sum(x => x.Total));
        var previous = await PreviousBatchTotalAsync(companyId, periodYYMM, ct);
        var name = await db.Companies.AsNoTracking().Where(x => x.Id == companyId).Select(x => x.PrintName).SingleAsync(ct);
        return new(companyId, periodYYMM, customers.Length,
            FinanceRounding.Money(customers.Sum(x => x.Net)), FinanceRounding.Money(customers.Sum(x => x.Vat)),
            FinanceRounding.Money(customers.Sum(x => x.Interest)), total, customers,
            name, previous, InvoiceGenerationEngine.ChangePercent(total, previous));
    }

    /// <summary>Σ Total (without interest) of the latest earlier regular, generated batch; group members are cancelled so not double counted.</summary>
    private async Task<decimal?> PreviousBatchTotalAsync(int companyId, int periodYYMM, CancellationToken ct)
    {
        var previousId = await db.Set<InvoiceBatch>().AsNoTracking()
            .Where(x => x.CompanyId == companyId && x.PeriodYYMM < periodYYMM && x.ExtraordinaryInvoiceMarker == null && x.Status != BillingBatchStatus.Draft)
            .OrderByDescending(x => x.PeriodYYMM).Select(x => (int?)x.Id).FirstOrDefaultAsync(ct);
        if (previousId is null) return null;
        return await db.Invoices.AsNoTracking()
            .Where(x => x.CompanyId == companyId && EF.Property<int?>(x, "InvoiceBatchId") == previousId && !x.IsCancelled)
            .SumAsync(x => x.Total, ct);
    }

    /// <summary>
    /// Scope -> company ids. "single" is the route company (the route policy already checked write access);
    /// "location"/"all" go through ContractsScopeResolver, intersected with companies where the caller has
    /// write access (Root: all) -- the same rule CompanyWritePolicy applies to one company.
    /// </summary>
    public async Task<IReadOnlyList<int>> ResolveScopeAsync(ClaimsPrincipal principal, int routeCompanyId, string? scope, int? locationCategoryId, CancellationToken ct)
    {
        var mode = string.IsNullOrWhiteSpace(scope) ? "single" : scope.Trim().ToLowerInvariant();
        if (mode == "single") return [routeCompanyId];
        if (mode is not ("all" or "location") || (mode == "location" && locationCategoryId is null))
            throw new DomainRuleException("billing.invalid-scope", "Obuhvat mora biti 'single', 'location' (uz lokaciju) ili 'all'.");

        IReadOnlyCollection<int> permitted;
        if (principal.IsInRole(SecurityConstants.RootRole))
        {
            permitted = await db.Companies.AsNoTracking().Select(x => x.Id).ToArrayAsync(ct);
        }
        else
        {
            var staffId = int.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var s) ? s : 0;
            permitted = (await db.StaffAccess.AsNoTracking().Where(x => x.StaffId == staffId)
                    .Select(x => new { x.CompanyId, x.StaffRole }).ToArrayAsync(ct))
                .Where(x => CompanyRoleRequirement.Satisfies(x.StaffRole, StaffRole.Moderator))
                .Select(x => x.CompanyId).ToArray();
        }
        var companies = await db.Companies.AsNoTracking().Select(x => new { x.Id, x.LocationCategoryId }).ToArrayAsync(ct);
        var categories = await db.Set<LocationCategory>().AsNoTracking().Select(x => new { x.Id, x.ParentId }).ToArrayAsync(ct);
        return ContractsScopeResolver.ResolveCompanyIds(mode, routeCompanyId, locationCategoryId, permitted,
                companies.Select(x => (x.Id, x.LocationCategoryId)).ToArray(), categories.Select(x => (x.Id, x.ParentId)).ToArray())
            .Order().ToArray();
    }

    /// <summary>Multi-company preview: one row per building; a building that can't be invoiced reports its error instead of failing the rest.</summary>
    public async Task<InvoiceScopePreviewResponse> PreviewScopeAsync(IReadOnlyList<int> companyIds, GenerateInvoicesV2Request request, CancellationToken ct)
    {
        var buildings = new List<InvoiceBatchPreviewV2Response>();
        foreach (var companyId in companyIds)
        {
            try
            {
                buildings.Add(await PreviewAsync(companyId, request.PeriodYYMM, request.ExtraordinaryMarker, request.ExchangeRateNbs, ct));
            }
            catch (DomainRuleException ex) when (companyIds.Count > 1)
            {
                var name = await db.Companies.AsNoTracking().Where(x => x.Id == companyId).Select(x => x.PrintName).SingleAsync(ct);
                buildings.Add(new(companyId, request.PeriodYYMM, 0, 0m, 0m, 0m, 0m, [], name, null, null, ex.Message));
            }
        }
        return new(buildings, buildings.Sum(x => x.CustomerCount),
            FinanceRounding.Money(buildings.Sum(x => x.NetTotal)), FinanceRounding.Money(buildings.Sum(x => x.VatTotal)),
            FinanceRounding.Money(buildings.Sum(x => x.InterestTotal)), FinanceRounding.Money(buildings.Sum(x => x.Total)));
    }

    /// <summary>
    /// Multi-company generate: one batch per company, each in its own transaction (a failing building
    /// doesn't roll back the others). With an interest period, the 9.4 run follows each new batch.
    /// </summary>
    public async Task<GenerateInvoicesScopeResponse> GenerateScopeAsync(
        IReadOnlyList<int> companyIds, int staffId, GenerateInvoicesV2Request request, BillingService billing, CancellationToken ct)
    {
        if (request.InterestPeriodStart is null != request.InterestPeriodEnd is null)
            throw new DomainRuleException("interest.invalid-period", "Period kamate mora imati i početak i kraj.");
        var names = await db.Companies.AsNoTracking().Where(x => companyIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.PrintName, ct);
        var results = new List<CompanyGenerateResultV2>();
        foreach (var companyId in companyIds)
        {
            try
            {
                var generated = await GenerateAsync(companyId, staffId, request, ct);
                decimal? interest = null;
                if (!generated.AlreadyGenerated && request.InterestPeriodStart is { } start && request.InterestPeriodEnd is { } end)
                {
                    interest = (await billing.RunInterestAsync(companyId, new RunInterestRequest(generated.InvoiceBatchId, start, end), ct)).TotalInterest;
                }
                results.Add(new(companyId, names.GetValueOrDefault(companyId, ""), generated.InvoiceBatchId, generated.AlreadyGenerated, generated.InvoiceIds.Count, interest, null));
            }
            catch (DomainRuleException ex) when (companyIds.Count > 1)
            {
                db.ChangeTracker.Clear();
                results.Add(new(companyId, names.GetValueOrDefault(companyId, ""), null, false, 0, null, ex.Message));
            }
        }
        return new(results);
    }

    public async Task<GenerateInvoicesV2Response> GenerateAsync(int companyId, int staffId, GenerateInvoicesV2Request request, CancellationToken ct)
    {
        Validate(request);
        var marker = Normalize(request.ExtraordinaryMarker);
        var strategy = db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);

            // Item 6 / FIN-09: one batch per (company, period, marker) is enforced by a unique index;
            // check it first so a repeat call is an idempotent no-op instead of a 500.
            var existing = await db.Set<InvoiceBatch>()
                .SingleOrDefaultAsync(x => x.CompanyId == companyId && x.PeriodYYMM == request.PeriodYYMM && x.ExtraordinaryInvoiceMarker == marker, ct);
            if (existing is not null && existing.Status != BillingBatchStatus.Draft)
            {
                var ids = await db.Invoices.AsNoTracking()
                    .Where(x => x.CompanyId == companyId && EF.Property<int?>(x, "InvoiceBatchId") == existing.Id)
                    .OrderBy(x => EF.Property<int>(x, "SortIndex")).Select(x => x.Id).ToArrayAsync(ct);
                return new GenerateInvoicesV2Response(existing.Id, true, ids);
            }

            var company = await db.Companies.AsNoTracking().SingleAsync(x => x.Id == companyId, ct);
            var drafts = await BuildAsync(companyId, request.PeriodYYMM, marker, request.ExchangeRateNbs, ct);
            if (drafts.Count == 0)
                throw new DomainRuleException("billing.nothing-to-invoice", "Za izabrani period nema stavki za fakturisanje.");

            var batch = existing ?? new InvoiceBatch { CompanyId = companyId };
            batch.StaffId = staffId;
            batch.PeriodYYMM = request.PeriodYYMM;
            batch.Year = 2000 + request.PeriodYYMM / 100;
            batch.Month = request.PeriodYYMM % 100;
            batch.Caption = $"R-{request.PeriodYYMM:0000}{(marker is null ? "" : "-" + marker)}";
            batch.Place = request.Place.Trim();
            batch.IssueDate = request.IssueDate;
            batch.DueDate = request.DueDate;
            batch.ServiceDateFrom = request.ServiceDateFrom;
            batch.ServiceDateTo = request.ServiceDateTo;
            batch.TransactionDate = request.TransactionDate;
            batch.ExchangeRateNbs = FinanceRounding.Calculation(request.ExchangeRateNbs);
            batch.ExtraordinaryInvoiceMarker = marker;
            batch.EntryDate = timeProvider.GetUtcNow();
            batch.Status = BillingBatchStatus.Draft;
            if (existing is null) db.Add(batch);
            await db.SaveChangesAsync(ct);

            var benefitContracts = await db.Set<Benefit>()
                .Where(x => x.CompanyId == companyId && x.PeriodYYMM == request.PeriodYYMM && x.InvoiceId == null)
                .Join(db.Set<Contract>(), b => b.ContractId, c => c.Id, (b, c) => new { Benefit = b, CustomerId = c.InvoicePartnerId ?? c.OwnerPartnerId ?? c.TenantPartnerId })
                .ToListAsync(ct);
            var managerSupplierAccountIds = await db.Set<PartnerAccount>().AsNoTracking()
                .Where(x => x.CompanyId == companyId && x.AccountNumber == ManagerSupplierAccountNumber)
                .Select(x => x.Id).ToArrayAsync(ct);
            var supplierNames = await db.Set<SupplierInvoice>().AsNoTracking()
                .Where(x => x.CompanyId == companyId && x.PeriodYYMM == request.PeriodYYMM)
                .ToDictionaryAsync(x => x.Id, x => x.Caption, ct);

            var invoiceIds = new List<int>();
            var invoiceByCustomer = new Dictionary<int, Invoice>();
            var sort = 0;
            foreach (var draft in drafts)
            {
                var rbr = InvoiceNumbering.Rbr(company.ShortName, draft.Account.AccountNumber, request.PeriodYYMM, marker);
                var invoice = new Invoice
                {
                    CompanyId = companyId,
                    PartnerId = draft.CustomerId,
                    SequenceNumber = rbr,
                    IssueDate = request.IssueDate,
                    DueDate = request.DueDate,
                    // R2: snapshot of customer identity at generation time.
                    PartnerName = Clip(draft.Partner.Name, 255),
                    Address = Clip(draft.Address?.StreetAddress ?? "-", 255),
                    PostalCode = draft.Address?.PostalCode,
                    City = Clip(draft.Address?.City ?? "-", 50),
                    TaxNumber = draft.Partner.TaxNumber,
                    RegistrationNumber = draft.Partner.RegistrationNumber,
                    Currency = "RSD",
                    VatRate = 0m, // see VAT note in BuildAsync
                };
                db.Invoices.Add(invoice);
                var entry = db.Entry(invoice);
                entry.Property("InvoiceBatchId").CurrentValue = batch.Id;
                entry.Property("PlaceOfIssue").CurrentValue = batch.Place;
                entry.Property("ServiceDateFrom").CurrentValue = batch.ServiceDateFrom;
                entry.Property("ServiceDateTo").CurrentValue = batch.ServiceDateTo;
                entry.Property("TransactionDate").CurrentValue = batch.TransactionDate;
                entry.Property("PreviousBalance").CurrentValue = draft.PreviousDebt;
                entry.Property("PaymentReference").CurrentValue = Kb97.PaymentReference(rbr);
                entry.Property("SortIndex").CurrentValue = sort++;

                var lineSort = 0;
                var managerLineList = new List<InvoiceLine>();
                foreach (var r in draft.Lines.OrderBy(x => x.SupplierInvoiceId).ThenBy(x => x.UnitTypeId))
                {
                    var line = new InvoiceLine
                    {
                        CompanyId = companyId,
                        Name = Clip(supplierNames.GetValueOrDefault(r.SupplierInvoiceId, $"#{r.SupplierInvoiceId}"), 255),
                        Quantity = r.Result.Quantity,
                        PricePcs = FinanceRounding.Calculation(r.Result.UnitAmountRsd),
                        ExchangeRateNbs = batch.ExchangeRateNbs,
                        PriceTotal = r.Result.NetAmount,
                        VatAmount = r.Result.VatAmount,
                        TotalAmount = r.Result.TotalAmount,
                        SortIndex = lineSort++,
                    };
                    invoice.Lines.Add(line);
                    if (managerSupplierAccountIds.Contains(r.SupplierPartnerAccountId) && line.TotalAmount != 0m) managerLineList.Add(line);
                    var le = db.Entry(line);
                    le.Property("InvoiceBatchId").CurrentValue = batch.Id;
                    le.Property("PartnerId").CurrentValue = draft.CustomerId;
                    le.Property("SupplierInvoiceId").CurrentValue = r.SupplierInvoiceId;
                    le.Property("Currency").CurrentValue = "RSD";
                }
                await db.SaveChangesAsync(ct);

                // Item 5 (FIN-18): benefit zeroes the manager's lines and archives the original -- no proportional reduction.
                var benefits = benefitContracts.Where(x => x.CustomerId == draft.CustomerId).Select(x => x.Benefit).ToArray();
                if (benefits.Length > 0)
                {
                    var managerLines = managerLineList;
                    for (var i = 0; i < managerLines.Count; i++)
                    {
                        var l = managerLines[i];
                        db.Add(new BenefitArchive
                        {
                            CompanyId = companyId, CustomerId = draft.CustomerId, PeriodYYMM = request.PeriodYYMM,
                            InvoiceId = invoice.Id, InvoiceLineId = l.Id, OriginalAmount = l.TotalAmount,
                            Note = BenefitNote(i + 1, managerLines.Count, request.PeriodYYMM), EntryDate = timeProvider.GetUtcNow()
                        });
                        l.PriceTotal = 0m; l.VatAmount = 0m; l.TotalAmount = 0m;
                    }
                    foreach (var b in benefits) b.InvoiceId = invoice.Id;
                }

                invoice.Amount = FinanceRounding.Money(invoice.Lines.Sum(x => x.PriceTotal));
                invoice.VatAmount = FinanceRounding.Money(invoice.Lines.Sum(x => x.VatAmount));
                invoice.Total = FinanceRounding.Money(invoice.Amount + invoice.VatAmount);
                invoice.InterestAmount = 0m; // R7/R8: set below from this batch's interest run (ApplyInterestAsync).
                invoice.InvoiceTotal = invoice.Total;
                invoiceIds.Add(invoice.Id);
                invoiceByCustomer[draft.CustomerId] = invoice;
            }

            await CreateGroupInvoicesAsync(company, batch, drafts, invoiceByCustomer, marker, sort, invoiceIds, ct);
            await db.SaveChangesAsync(ct);
            await ApplyInterestAsync(db, companyId, batch.Id, ct);

            batch.Status = BillingBatchStatus.Generated;
            batch.GeneratedAt = timeProvider.GetUtcNow();
            batch.GenerationFingerprint = BillingCalculator.CreateDeterministicKey(new { request.PeriodYYMM, marker, invoiceIds });
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return new GenerateInvoicesV2Response(batch.Id, false, invoiceIds);
        });
    }

    public static string BenefitNote(int n, int m, int periodYYMM) =>
        $"Benefit FM {n}/{m} – {periodYYMM:0000} / Бенефит ФМ {n}/{m} – {periodYYMM:0000}";

    private async Task<IReadOnlyList<CustomerDraft>> BuildAsync(int companyId, int periodYYMM, string? marker, decimal nbsRate, CancellationToken ct)
    {
        EnsurePeriod(periodYYMM);

        // R0 population: every unit with a type. Active = its current contract is active.
        var unitRows = await db.Set<Unit>().AsNoTracking()
            .Where(u => u.CompanyId == companyId && u.UnitTypeId != null)
            .Select(u => new
            {
                UnitTypeId = u.UnitTypeId!.Value,
                u.K1, u.K2, u.K3, u.K4, u.K5,
                IsActive = u.Contract != null && u.Contract.IsActive,
                CustomerId = u.Contract != null ? u.Contract.InvoicePartnerId ?? u.Contract.OwnerPartnerId ?? u.Contract.TenantPartnerId : null,
                MasterAccountId = u.Contract != null ? u.Contract.InvoiceLegacyMasterId : null,
                Address = u.BuildingEntrance != null ? u.BuildingEntrance.Address : null
            }).ToListAsync(ct);
        var units = unitRows.Select(u => new R0UnitRow(u.CustomerId ?? 0, u.UnitTypeId, u.IsActive && u.CustomerId != null,
            u.K1 ?? 0m, u.K2 ?? 0m, u.K3 ?? 0m, u.K4 ?? 0m, u.K5 ?? 0m)).ToArray();

        // Cat1=1 == SupplierDocumentType 1 "Predviđeni troškovi" (data-model.md; type 2 is never invoiced).
        var supplierInvoices = await db.Set<SupplierInvoice>().AsNoTracking().Include(x => x.UnitTypes)
            .Where(x => x.CompanyId == companyId && x.PeriodYYMM == periodYYMM
                && db.ShortLists.Any(t => t.Id == x.DocumentTypeId && t.TableName == "SupplierDocumentType" && t.IndexValue == SupplierDocumentTypes.Planned)
                && x.CalculationTypeId != 99
                && x.ExtraordinaryInvoiceMarker == marker)
            .ToListAsync(ct);
        // ponytail: VAT rate is 0 -- SupplierInvoice has no PDV column in data-model.md and no issuer is in VAT today (P4).
        var inputs = supplierInvoices.Select(x => new R0SupplierInput(x.Id, x.SupplierPartnerAccountId, x.CalculationTypeId,
            x.UnitTypes.Select(t => t.UnitTypeId).ToArray(), x.InvoiceTotalCalculationAmountRsd, x.InvoiceTotalCalculationAmountEur,
            x.CalculationAmountByCoefficientRsd, x.CalculationAmountByCoefficientEur, 0m)).ToArray();

        var lines = InvoiceGenerationEngine.GenerateLines(units, inputs, nbsRate);
        var customerIds = lines.Select(x => x.CustomerId).Distinct().ToArray();
        if (customerIds.Length == 0) return [];

        var partners = await db.Set<Partner>().AsNoTracking().Where(x => customerIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);
        var accounts = (await db.Set<PartnerAccount>().AsNoTracking()
                .Where(x => x.CompanyId == companyId && customerIds.Contains(x.PartnerId) && x.Account == LedgerAccounts.Customers)
                .ToListAsync(ct))
            .GroupBy(x => x.PartnerId).ToDictionary(g => g.Key, g => g.OrderBy(x => x.Id).First());
        var missing = customerIds.Where(id => !accounts.ContainsKey(id)).ToArray();
        if (missing.Length > 0)
            throw new DomainRuleException("billing.customer-account-missing",
                $"Kupci bez konta 2040: {string.Join(", ", missing.Take(10).Select(id => partners.TryGetValue(id, out var p) ? p.Name : id.ToString()))}.");

        // R2: PrethodniDug = full live 2040 balance (P5, no as-of cut); PD_iznos = balance of line type 98.
        var accountIds = accounts.Values.Select(x => x.Id).ToArray();
        var takenOverTypeId = await db.ShortLists.AsNoTracking()
            .Where(x => x.TableName == LedgerLineTypes.ShortListTable && x.IndexValue == LedgerLineTypes.TakenOverDebt)
            .Select(x => (int?)x.Id).FirstOrDefaultAsync(ct);
        var balances = await db.LedgerEntries.AsNoTracking()
            .Where(x => x.CompanyId == companyId && x.JournalEntry.IsPosted && x.Account == LedgerAccounts.Customers
                && accountIds.Contains(EF.Property<int?>(x, "PartnerAccountId") ?? 0))
            .GroupBy(x => EF.Property<int?>(x, "PartnerAccountId"))
            .Select(g => new
            {
                Id = g.Key!.Value,
                Total = g.Sum(x => x.DebitAmount - x.CreditAmount),
                TakenOver = g.Sum(x => x.LineTypeId == takenOverTypeId ? x.DebitAmount - x.CreditAmount : 0m)
            })
            .ToDictionaryAsync(x => x.Id, ct);

        var addressByCustomer = unitRows.Where(u => u.IsActive && u.CustomerId != null && u.Address != null)
            .GroupBy(u => u.CustomerId!.Value).ToDictionary(g => g.Key, g => g.First().Address);
        // ponytail: legacy keeps the master on the customer (Kupac.IDGrupniRacunMaster); here it's per contract, so the
        // first active contract with a master wins. Two different masters for one customer would need a per-contract split.
        var masterByCustomer = unitRows.Where(u => u.IsActive && u.CustomerId != null && u.MasterAccountId != null)
            .GroupBy(u => u.CustomerId!.Value).ToDictionary(g => g.Key, g => g.First().MasterAccountId);

        return lines.GroupBy(x => x.CustomerId)
            .Select(g =>
            {
                var account = accounts[g.Key];
                balances.TryGetValue(account.Id, out var b);
                return new CustomerDraft(g.Key, partners[g.Key], account, addressByCustomer.GetValueOrDefault(g.Key),
                    g.ToArray(), FinanceRounding.Money(b?.Total ?? 0m), FinanceRounding.Money(b?.TakenOver ?? 0m),
                    masterByCustomer.GetValueOrDefault(g.Key));
            })
            .OrderBy(x => x.Account.AccountNumber)
            .ToArray();
    }

    /// <summary>
    /// GAP-12 (9.1 R5/R9/R10): per group master (2040 PartnerAccount) one invoice = Σ of its members'
    /// Amount/VAT/Total (legacy RacunShema 'GR'); members become storno with InvoiceParentId = master
    /// (legacy Storno=True, SPC). Posting moves the members' 2040 lines onto the master (PostBatchAsync).
    /// Both carry InvoiceLegacyMasterId = master account; master = that set and no parent.
    /// </summary>
    private async Task CreateGroupInvoicesAsync(Company company, InvoiceBatch batch, IReadOnlyList<CustomerDraft> drafts,
        IReadOnlyDictionary<int, Invoice> invoiceByCustomer, string? marker, int sort, List<int> invoiceIds, CancellationToken ct)
    {
        var groups = drafts.Where(d => d.MasterAccountId is not null).GroupBy(d => d.MasterAccountId!.Value).ToArray();
        if (groups.Length == 0) return;

        var masterIds = groups.Select(g => g.Key).ToArray();
        var masters = await db.Set<PartnerAccount>().AsNoTracking().Include(x => x.Partner)
            .Where(x => masterIds.Contains(x.Id) && x.Account == LedgerAccounts.Customers && (x.CompanyId == company.Id || x.CompanyId == null))
            .ToDictionaryAsync(x => x.Id, ct);
        var missing = masterIds.Where(id => !masters.ContainsKey(id)).ToArray();
        if (missing.Length > 0)
            throw new DomainRuleException("billing.group-master-missing",
                $"Grupni račun: master konto (PartnerAccount {string.Join(", ", missing)}) ne postoji ili nije konto 2040 ove SZ.");
        var direct = drafts.Select(d => d.CustomerId).ToHashSet();
        if (masters.Values.FirstOrDefault(m => direct.Contains(m.PartnerId)) is { } clash)
            throw new DomainRuleException("billing.group-master-is-customer", $"Master grupnog računa '{clash.Partner.Name}' ima i sopstveni račun u seriji.");

        var masterPartnerIds = masters.Values.Select(x => x.PartnerId).ToArray();
        var addresses = (await db.Set<PartnerAddress>().AsNoTracking().Where(x => masterPartnerIds.Contains(x.PartnerId))
                .OrderByDescending(x => x.IsDefault).ThenBy(x => x.Id).Select(x => new { x.PartnerId, x.Address }).ToListAsync(ct))
            .GroupBy(x => x.PartnerId).ToDictionary(g => g.Key, g => g.First().Address);
        var balances = await db.LedgerEntries.AsNoTracking()
            .Where(x => x.CompanyId == company.Id && x.JournalEntry.IsPosted && x.Account == LedgerAccounts.Customers
                && masterIds.Contains(EF.Property<int?>(x, "PartnerAccountId") ?? 0))
            .GroupBy(x => EF.Property<int?>(x, "PartnerAccountId"))
            .Select(g => new { Id = g.Key!.Value, Total = g.Sum(x => x.DebitAmount - x.CreditAmount) })
            .ToDictionaryAsync(x => x.Id, x => x.Total, ct);

        foreach (var group in groups.OrderBy(g => masters[g.Key].AccountNumber))
        {
            var account = masters[group.Key];
            var members = group.Select(d => invoiceByCustomer[d.CustomerId]).ToArray();
            var address = addresses.GetValueOrDefault(account.PartnerId) ?? group.First().Address;
            var rbr = InvoiceNumbering.Rbr(company.ShortName, account.AccountNumber, batch.PeriodYYMM, marker);
            var master = new Invoice
            {
                CompanyId = company.Id,
                PartnerId = account.PartnerId,
                SequenceNumber = rbr,
                IssueDate = batch.IssueDate,
                DueDate = batch.DueDate,
                PartnerName = Clip(account.Partner.Name, 255),
                Address = Clip(address?.StreetAddress ?? "-", 255),
                PostalCode = address?.PostalCode,
                City = Clip(address?.City ?? "-", 50),
                TaxNumber = account.Partner.TaxNumber,
                RegistrationNumber = account.Partner.RegistrationNumber,
                Currency = "RSD",
                Amount = FinanceRounding.Money(members.Sum(x => x.Amount)),
                VatAmount = FinanceRounding.Money(members.Sum(x => x.VatAmount)),
                Total = FinanceRounding.Money(members.Sum(x => x.Total)),
            };
            master.InvoiceTotal = master.Total;
            db.Invoices.Add(master);
            var entry = db.Entry(master);
            entry.Property("InvoiceBatchId").CurrentValue = batch.Id;
            entry.Property("PlaceOfIssue").CurrentValue = batch.Place;
            entry.Property("ServiceDateFrom").CurrentValue = batch.ServiceDateFrom;
            entry.Property("ServiceDateTo").CurrentValue = batch.ServiceDateTo;
            entry.Property("TransactionDate").CurrentValue = batch.TransactionDate;
            entry.Property("PreviousBalance").CurrentValue = FinanceRounding.Money(balances.GetValueOrDefault(account.Id));
            entry.Property("PaymentReference").CurrentValue = Kb97.PaymentReference(rbr);
            entry.Property("InvoiceLegacyMasterId").CurrentValue = account.Id;
            entry.Property("SortIndex").CurrentValue = sort++;
            await db.SaveChangesAsync(ct);

            foreach (var member in members)
            {
                member.IsCancelled = true;
                member.CancelledAt = timeProvider.GetUtcNow();
                var m = db.Entry(member);
                m.Property("InvoiceParentId").CurrentValue = master.Id;
                m.Property("InvoiceLegacyMasterId").CurrentValue = account.Id;
                m.Property("CancelReason").CurrentValue = $"Grupni račun {rbr}";
            }
            invoiceIds.Add(master.Id);
        }
    }

    /// <summary>9.4 PrenesiZK over the batch's interest run: Round(Σ, 2) per (partner account, sub-account), only &gt; 0, with the partner behind the account.</summary>
    internal static async Task<IReadOnlyList<(int PartnerId, InterestTotal Total)>> LoadInterestTransferAsync(SzAppDbContext db, int companyId, int batchId, CancellationToken ct)
    {
        var sums = await db.Set<InterestStatement>().AsNoTracking()
            .Where(x => x.CompanyId == companyId && x.InvoiceBatchId == batchId)
            .GroupBy(x => new { x.PartnerAccountId, x.SubAccountId })
            .Select(g => new { g.Key.PartnerAccountId, g.Key.SubAccountId, Interest = g.Sum(x => x.Interest) })
            .ToArrayAsync(ct);
        var totals = InterestCalculator.Transfer(sums.Select(x => new InterestTotal(x.PartnerAccountId, x.SubAccountId ?? string.Empty, x.Interest)));
        if (totals.Count == 0) return [];
        var accountIds = totals.Select(x => x.PartnerAccountId).Distinct().ToArray();
        var partnerOf = await db.Set<PartnerAccount>().AsNoTracking().Where(x => accountIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.PartnerId, ct);
        return totals.Select(t => (partnerOf[t.PartnerAccountId], t)).ToArray();
    }

    /// <summary>
    /// R7/R8: KamataIznos = Σ PrenesiZK of the invoice's partner (group members' interest goes to their master);
    /// InvoiceTotal (legacy Ukupno) = Total + KamataIznos. Interest is not an invoice line. Idempotent; called
    /// after generation and after every interest run on a not-yet-posted batch.
    /// </summary>
    public static async Task ApplyInterestAsync(SzAppDbContext db, int companyId, int batchId, CancellationToken ct)
    {
        var invoices = await db.Invoices.Where(x => x.CompanyId == companyId && EF.Property<int?>(x, "InvoiceBatchId") == batchId).ToListAsync(ct);
        if (invoices.Count == 0) return;
        var carriers = InvoiceGenerationEngine.InvoiceCarriers(invoices
            .Select(x => new InvoiceCarrierRow(x.Id, x.PartnerId, x.IsCancelled, db.Entry(x).Property<int?>("InvoiceParentId").CurrentValue)));
        var byInvoice = (await LoadInterestTransferAsync(db, companyId, batchId, ct))
            .Where(x => carriers.ContainsKey(x.PartnerId))
            .GroupBy(x => carriers[x.PartnerId])
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Total.Interest));
        foreach (var invoice in invoices.Where(x => !x.IsCancelled))
        {
            invoice.InterestAmount = FinanceRounding.Money(byInvoice.GetValueOrDefault(invoice.Id));
            invoice.InvoiceTotal = FinanceRounding.Money(invoice.Total + invoice.InterestAmount);
        }
        await db.SaveChangesAsync(ct);
    }

    private static void Validate(GenerateInvoicesV2Request r)
    {
        EnsurePeriod(r.PeriodYYMM);
        if (r.ServiceDateFrom > r.ServiceDateTo || r.IssueDate > r.DueDate || r.ExchangeRateNbs <= 0m || string.IsNullOrWhiteSpace(r.Place) || r.Place.Trim().Length > 50)
            throw new DomainRuleException("billing.invalid-batch", "Datumi, mesto ili kurs fakturisanja nisu ispravni.");
    }

    private static void EnsurePeriod(int value)
    {
        if (value is < 1001 or > 9912 || value % 100 is < 1 or > 12)
            throw new DomainRuleException("billing.invalid-period", "Period mora biti u formatu YYMM.");
    }

    private static string? Normalize(string? marker) => string.IsNullOrWhiteSpace(marker) ? null : marker.Trim();
    private static string Clip(string value, int max) => value.Length > max ? value[..max] : value;
}

/// <summary>R3: RBR = "{SZ}-{customer account number}-{YYMM}[-{marker}]"; SZ = Company.ShortName (legacy SZ code, e.g. 251).</summary>
public static class InvoiceNumbering
{
    public static string Rbr(string companyCode, int customerAccountNumber, int periodYYMM, string? marker) =>
        $"{companyCode.Trim()}-{customerAccountNumber}-{periodYYMM:0000}{(string.IsNullOrWhiteSpace(marker) ? "" : "-" + marker.Trim())}";
}
