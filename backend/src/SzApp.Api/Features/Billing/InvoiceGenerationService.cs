using System.Data;
using Microsoft.EntityFrameworkCore;
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

    private sealed record CustomerDraft(
        int CustomerId, Partner Partner, PartnerAccount Account, Address? Address,
        IReadOnlyList<R1CustomerLine> Lines, decimal PreviousDebt, decimal TakenOverDebt);

    public async Task<InvoiceBatchPreviewV2Response> PreviewAsync(int companyId, int periodYYMM, string? marker, decimal nbsRate, CancellationToken ct)
    {
        var drafts = await BuildAsync(companyId, periodYYMM, Normalize(marker), nbsRate, ct);
        var customers = drafts.Select(d =>
        {
            var net = FinanceRounding.Money(d.Lines.Sum(x => x.Result.NetAmount));
            var vat = FinanceRounding.Money(d.Lines.Sum(x => x.Result.VatAmount));
            return new CustomerInvoicePreviewV2(d.CustomerId, d.Partner.Name, net, vat, 0m, FinanceRounding.Money(net + vat));
        }).ToArray();
        return new(companyId, periodYYMM, customers.Length,
            FinanceRounding.Money(customers.Sum(x => x.Net)), FinanceRounding.Money(customers.Sum(x => x.Vat)), 0m,
            FinanceRounding.Money(customers.Sum(x => x.Total)), customers);
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
                invoice.InterestAmount = 0m; // R7/R8 interest is added by the interest run, not here.
                invoice.InvoiceTotal = invoice.Total;
                invoiceIds.Add(invoice.Id);
            }

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
                Address = u.BuildingEntrance != null ? u.BuildingEntrance.Address : null
            }).ToListAsync(ct);
        var units = unitRows.Select(u => new R0UnitRow(u.CustomerId ?? 0, u.UnitTypeId, u.IsActive && u.CustomerId != null,
            u.K1 ?? 0m, u.K2 ?? 0m, u.K3 ?? 0m, u.K4 ?? 0m, u.K5 ?? 0m)).ToArray();

        // Cat1=1 == SupplierDocumentType 1 "Predviđeni troškovi" (data-model.md; type 2 is never invoiced).
        var supplierInvoices = await db.Set<SupplierInvoice>().AsNoTracking().Include(x => x.UnitTypes)
            .Where(x => x.CompanyId == companyId && x.PeriodYYMM == periodYYMM
                && x.DocumentTypeId == SupplierDocumentTypes.Planned && x.CalculationTypeId != 99
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

        return lines.GroupBy(x => x.CustomerId)
            .Select(g =>
            {
                var account = accounts[g.Key];
                balances.TryGetValue(account.Id, out var b);
                return new CustomerDraft(g.Key, partners[g.Key], account, addressByCustomer.GetValueOrDefault(g.Key),
                    g.ToArray(), FinanceRounding.Money(b?.Total ?? 0m), FinanceRounding.Money(b?.TakenOver ?? 0m));
            })
            .OrderBy(x => x.Account.AccountNumber)
            .ToArray();
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
