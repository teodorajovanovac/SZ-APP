using System.Data;
using Microsoft.EntityFrameworkCore;
using SzApp.Contracts.Billing;
using SzApp.Data;
using SzApp.Data.Entities;
using SzApp.Data.Entities.Billing;
using SzApp.Domain;
using SzApp.Domain.Billing;
using SzApp.Domain.LedgerBanking;
using SzApp.Api.Features.LedgerBanking;
using SzApp.Data.Entities.LedgerBanking;
using Invoice = SzApp.Data.Entities.Invoice;
using InvoiceLine = SzApp.Data.Entities.InvoiceLine;

namespace SzApp.Api.Features.Billing;

public sealed class BillingService(
    SzAppDbContext db,
    IShortListValidator shortLists,
    ILedgerPostingGateway ledger,
    INoticeWorkflowGateway noticeWorkflow,
    TimeProvider timeProvider,
    IBusinessClock clock)
{
    private const string SupplierInvoiceSource = "SupplierInvoice";
    private const string SupplierInvoiceCancelSource = "SupplierInvoiceCancel";

    private static readonly HashSet<string> SupplierAmountRules = ["FixedRsd", "FixedEur", "SupplierTotal"];
    private static readonly HashSet<string> AllocationRules = ["Equal", "ByArea", "ByUnit", "ByCoefficient"];
    private static readonly HashSet<string> QuantityRules = ["One", "UnitArea", "UnitCount", "Coefficient"];

    public async Task<IReadOnlyList<CalculationTypeResponse>> ListCalculationTypesAsync(CancellationToken ct) =>
        await db.Set<CalculationType>().AsNoTracking().OrderBy(x => x.Name)
            .Select(x => new CalculationTypeResponse(x.Id, x.Name, x.SupplierAmountRule, x.AllocationRule, x.QuantityRule, x.UnitOfMeasureId, x.Note))
            .ToArrayAsync(ct);

    public async Task<CalculationTypeResponse> CreateCalculationTypeAsync(CreateCalculationTypeRequest request, CancellationToken ct)
    {
        EnsureRule(SupplierAmountRules, request.SupplierAmountRule, "supplier amount");
        EnsureRule(AllocationRules, request.AllocationRule, "allocation");
        EnsureRule(QuantityRules, request.QuantityRule, "quantity");
        await shortLists.EnsureTypeAsync(request.UnitOfMeasureId, "UnitOfMeasure", ct);

        var entity = new CalculationType
        {
            Name = Required(request.Name, 100, "Naziv"),
            SupplierAmountRule = request.SupplierAmountRule,
            AllocationRule = request.AllocationRule,
            QuantityRule = request.QuantityRule,
            UnitOfMeasureId = request.UnitOfMeasureId,
            Note = Trim(request.Note, 255)
        };
        db.Add(entity);
        await db.SaveChangesAsync(ct);
        return new(entity.Id, entity.Name, entity.SupplierAmountRule, entity.AllocationRule, entity.QuantityRule, entity.UnitOfMeasureId, entity.Note);
    }

    public async Task<BillingPage<SupplierInvoiceResponse>> ListSupplierInvoicesAsync(int companyId, SupplierInvoiceListQuery filter, CancellationToken ct)
    {
        var (page, pageSize) = NormalizePage(filter.Page ?? 1, filter.PageSize ?? 25);
        var query = db.Set<SupplierInvoice>().AsNoTracking().Where(x => x.CompanyId == companyId);
        if (filter.PeriodYYMM is { } periodYYMM) query = query.Where(x => x.PeriodYYMM == periodYYMM);
        if (filter.SupplierPartnerAccountId is { } supplierPartnerAccountId) query = query.Where(x => x.SupplierPartnerAccountId == supplierPartnerAccountId);
        if (filter.DocumentTypeId is { } documentTypeId) query = query.Where(x => x.DocumentTypeId == documentTypeId);
        if (filter.HasExtraordinaryMarker is { } hasExtraordinaryMarker)
            query = hasExtraordinaryMarker
                ? query.Where(x => !string.IsNullOrWhiteSpace(x.ExtraordinaryInvoiceMarker))
                : query.Where(x => string.IsNullOrWhiteSpace(x.ExtraordinaryInvoiceMarker));
        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(x => x.PeriodYYMM).ThenBy(x => x.InvoiceNo)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new SupplierInvoiceResponse(
                x.Id, x.InvoiceNo, x.CodeName, x.Caption, x.CalculationTypeId, x.PeriodYYMM,
                x.InvoiceTotalCalculationAmountEur, x.InvoiceTotalCalculationAmountRsd, x.PostedInvoiceAmount,
                x.InvoiceDate, x.TransactionDate, x.SupplierPartnerAccountId, x.DocumentTypeId, x.ExtraordinaryInvoiceMarker,
                x.UnitTypes.OrderBy(y => y.UnitTypeId).Select(y => y.UnitTypeId).ToArray(),
                Convert.ToBase64String(x.RowVersion), x.JournalEntryId,
                db.Set<LedgerSourcePosting>().Any(p => p.CompanyId == x.CompanyId && p.SourceType == SupplierInvoiceCancelSource && p.SourceId == x.Id)))
            .ToArrayAsync(ct);
        return new(items, page, pageSize, total);
    }

    public async Task<SupplierInvoiceResponse> CreateSupplierInvoiceAsync(int companyId, CreateSupplierInvoiceRequest request, CancellationToken ct)
    {
        EnsurePeriod(request.PeriodYYMM);
        if (request.InvoiceTotalCalculationAmountEur < 0m || request.InvoiceTotalCalculationAmountRsd < 0m)
        {
            throw new DomainRuleException("billing.negative-supplier-total", "Iznos dobavljačkog računa ne može biti negativan.");
        }
        await shortLists.EnsureTypeAsync(request.DocumentTypeId, "SupplierDocumentType", ct);
        foreach (var unitTypeId in request.UnitTypeIds.Distinct())
        {
            await shortLists.EnsureTypeAsync(unitTypeId, "UnitType", ct);
        }

        if (!await db.Set<CalculationType>().AnyAsync(x => x.Id == request.CalculationTypeId, ct))
        {
            throw new DomainRuleException("billing.calculation-type-not-found", "Tip obračuna ne postoji.");
        }
        if (!await db.Set<PartnerAccount>().AnyAsync(x => x.Id == request.SupplierPartnerAccountId && x.CompanyId == companyId, ct))
        {
            throw new DomainRuleException("billing.supplier-account-not-found", "Konto dobavljača ne pripada aktivnoj kompaniji.");
        }

        var entity = new SupplierInvoice
        {
            CompanyId = companyId,
            InvoiceNo = request.InvoiceNo,
            CodeName = Required(request.CodeName, 255, "Šifra"),
            Caption = Required(request.Caption, 255, "Naziv"),
            SupplierPartnerAccountId = request.SupplierPartnerAccountId,
            CalculationTypeId = request.CalculationTypeId,
            PeriodYYMM = request.PeriodYYMM,
            InvoiceTotalCalculationAmountEur = FinanceRounding.Calculation(request.InvoiceTotalCalculationAmountEur),
            InvoiceTotalCalculationAmountRsd = FinanceRounding.Calculation(request.InvoiceTotalCalculationAmountRsd),
            CalculationAmountByCoefficientEur = FinanceRounding.Calculation(request.CalculationAmountByCoefficientEur),
            CalculationAmountByCoefficientRsd = FinanceRounding.Calculation(request.CalculationAmountByCoefficientRsd),
            PaymentPriority = request.PaymentPriority,
            SubAccountId = Trim(request.SubAccountId, 10),
            DocumentTypeId = request.DocumentTypeId,
            ExtraordinaryInvoiceMarker = Trim(request.ExtraordinaryInvoiceMarker, 10),
            InvoiceNameFunction = Trim(request.InvoiceNameRule, 100),
            InvoiceDate = request.InvoiceDate,
            TransactionDate = request.TransactionDate,
            PaymentDate = request.PaymentDate,
            InvoiceDescription = Trim(request.InvoiceDescription, 255),
            PaymentReference = Trim(request.PaymentReference, 255)
        };
        foreach (var unitTypeId in request.UnitTypeIds.Distinct())
        {
            entity.UnitTypes.Add(new SupplierInvoiceUnitType { UnitTypeId = unitTypeId });
        }
        db.Add(entity);
        await db.SaveChangesAsync(ct);
        return new(entity.Id, entity.InvoiceNo, entity.CodeName, entity.Caption, entity.CalculationTypeId, entity.PeriodYYMM,
            entity.InvoiceTotalCalculationAmountEur, entity.InvoiceTotalCalculationAmountRsd, entity.PostedInvoiceAmount,
            entity.InvoiceDate, entity.TransactionDate, entity.SupplierPartnerAccountId, entity.DocumentTypeId, entity.ExtraordinaryInvoiceMarker,
            entity.UnitTypes.Select(x => x.UnitTypeId).Order().ToArray(), Convert.ToBase64String(entity.RowVersion));
    }

    public async Task<BillingPage<InvoiceBatchResponse>> ListInvoiceBatchesAsync(int companyId, int page, int pageSize, CancellationToken ct)
    {
        (page, pageSize) = NormalizePage(page, pageSize);
        var query = db.Set<InvoiceBatch>().AsNoTracking().Where(x => x.CompanyId == companyId);
        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(x => x.PeriodYYMM).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => ToBatchResponse(x)).ToArrayAsync(ct);
        return new(items, page, pageSize, total);
    }

    public async Task<InvoiceBatchResponse> CreateInvoiceBatchAsync(int companyId, int staffId, CreateInvoiceBatchRequest request, CancellationToken ct)
    {
        EnsurePeriod(request.PeriodYYMM);
        if (request.Month is < 1 or > 12 || request.PeriodYYMM != request.Year % 100 * 100 + request.Month)
        {
            throw new DomainRuleException("billing.batch-period-mismatch", "Period, godina i mesec nisu usklađeni.");
        }
        if (request.ServiceDateFrom > request.ServiceDateTo || request.IssueDate > request.DueDate || request.ExchangeRateNbs <= 0m)
        {
            throw new DomainRuleException("billing.invalid-batch", "Datumi ili kurs fakturisanja nisu ispravni.");
        }

        var entity = new InvoiceBatch
        {
            CompanyId = companyId,
            StaffId = staffId,
            PeriodYYMM = request.PeriodYYMM,
            Caption = Required(request.Caption, 50, "Naziv serije"),
            Month = request.Month,
            Year = request.Year,
            Place = Required(request.Place, 50, "Mesto izdavanja"),
            IssueDate = request.IssueDate,
            ServiceDateFrom = request.ServiceDateFrom,
            ServiceDateTo = request.ServiceDateTo,
            TransactionDate = request.TransactionDate,
            DueDate = request.DueDate,
            ExchangeRateNbs = FinanceRounding.Calculation(request.ExchangeRateNbs),
            ExtraordinaryInvoiceMarker = Trim(request.ExtraordinaryInvoiceMarker, 255),
            BalanceAsOfDate = request.BalanceAsOfDate,
            PreviousValueDate = request.PreviousValueDate,
            IsInterestCalculated = request.IsInterestCalculated,
            PaymentPurpose = Trim(request.PaymentPurpose, 255),
            EntryDate = timeProvider.GetUtcNow(),
            Status = BillingBatchStatus.Draft
        };
        db.Add(entity);
        await db.SaveChangesAsync(ct);
        return ToBatchResponse(entity);
    }

    public async Task<InvoiceBatchPreviewResponse> PreviewBatchAsync(int companyId, int batchId, InvoiceGenerationRequest request, CancellationToken ct)
    {
        _ = await GetBatchAsync(companyId, batchId, false, ct);
        return CreatePreview(batchId, request);
    }

    public Task<InvoiceBatchGenerationResponse> GenerateBatchAsync(int companyId, int batchId, InvoiceGenerationRequest request, CancellationToken ct) =>
        ExecuteSerializableAsync(() => GenerateBatchCoreAsync(companyId, batchId, request, ct), ct);

    private async Task<InvoiceBatchGenerationResponse> GenerateBatchCoreAsync(int companyId, int batchId, InvoiceGenerationRequest request, CancellationToken ct)
    {
        await LockInvoiceBatchAsync(batchId, ct);
        var batch = await GetBatchAsync(companyId, batchId, true, ct);
        var preview = CreatePreview(batchId, request);
        if (batch.Status != BillingBatchStatus.Draft)
        {
            if (!string.Equals(batch.GenerationFingerprint, preview.Fingerprint, StringComparison.Ordinal))
            {
                throw new DomainRuleException("billing.batch-already-generated-different-input", "Serija je već generisana sa drugačijim ulaznim podacima.");
            }
            var existingIds = await db.Invoices.AsNoTracking()
                .Where(x => x.CompanyId == companyId && EF.Property<int?>(x, "InvoiceBatchId") == batchId)
                .OrderBy(x => EF.Property<int>(x, "SortIndex")).Select(x => x.Id).ToArrayAsync(ct);
            return new(batchId, true, preview.Fingerprint, existingIds);
        }

        // SEC-06: SupplierInvoiceId comes from the client per line (SetLineShadows below) -- validate
        // up front, in one query, that every referenced supplier invoice belongs to this company.
        var supplierInvoiceIds = request.Invoices.SelectMany(x => x.Lines)
            .Select(x => x.SupplierInvoiceId).Where(id => id is not null).Select(id => id!.Value).Distinct().ToArray();
        if (supplierInvoiceIds.Length > 0)
        {
            var validSupplierInvoiceCount = await db.Set<SupplierInvoice>().AsNoTracking()
                .CountAsync(x => supplierInvoiceIds.Contains(x.Id) && x.CompanyId == companyId, ct);
            if (validSupplierInvoiceCount != supplierInvoiceIds.Length)
            {
                throw new DomainRuleException("billing.supplier-invoice-not-found", "Ulazni račun u stavci ne pripada aktivnoj kompaniji.");
            }
        }

        var invoiceIds = new List<int>();
        foreach (var seed in request.Invoices.OrderBy(x => x.SortIndex).ThenBy(x => x.SequenceNumber, StringComparer.Ordinal))
        {
            var contractPartners = await db.Set<Contract>().AsNoTracking()
                .Where(x => seed.ContractIds.Contains(x.Id) && x.CompanyId == companyId)
                .Select(x => new { x.Id, x.InvoicePartnerId, x.OwnerPartnerId, x.TenantPartnerId }).ToArrayAsync(ct);
            // SEC-06: an empty ContractIds list makes both checks below vacuously true (0 == 0,
            // Any() on empty is false), which used to let seed.PartnerId through unchecked -- i.e.
            // an invoice could be generated for another company's partner. Partner can be shared
            // across companies (Partner.CompanyId == null), so that's still allowed.
            if (contractPartners.Length != seed.ContractIds.Distinct().Count() ||
                contractPartners.Any(x => seed.PartnerId != (x.InvoicePartnerId ?? x.OwnerPartnerId ?? x.TenantPartnerId)) ||
                !await db.Set<Partner>().AsNoTracking()
                    .AnyAsync(x => x.Id == seed.PartnerId && (x.CompanyId == companyId || x.CompanyId == null), ct))
            {
                throw new DomainRuleException("billing.contract-tenant-mismatch", "Ugovor ili primalac računa ne pripada aktivnoj kompaniji.");
            }
            var amounts = BillingCalculator.CalculateInvoice(seed.Lines.Select(ToDomainLine), seed.InterestAmount);
            var calculatedLines = seed.Lines.Select(line => (Seed: line, Amounts: BillingCalculator.CalculateLine(ToDomainLine(line)))).ToArray();
            var invoice = new Invoice
            {
                CompanyId = companyId,
                PartnerId = seed.PartnerId,
                SequenceNumber = Required(seed.SequenceNumber, 20, "Redni broj"),
                IssueDate = batch.IssueDate,
                DueDate = batch.DueDate,
                PartnerName = Required(seed.PartnerName, 255, "Naziv partnera"),
                Address = Required(seed.Address, 255, "Adresa"),
                PostalCode = Trim(seed.PostalCode, 50),
                City = Required(seed.City, 50, "Grad"),
                TaxNumber = Trim(seed.TaxNumber, 50),
                RegistrationNumber = Trim(seed.RegistrationNumber, 255),
                Currency = Required(seed.Currency, 3, "Valuta").ToUpperInvariant(),
                Amount = amounts.TaxableAmount,
                VatRate = DistinctVatRate(seed.Lines),
                VatAmount = amounts.VatAmount,
                Total = FinanceRounding.Money(amounts.TaxableAmount + amounts.VatAmount),
                InterestAmount = amounts.InterestAmount,
                InvoiceTotal = amounts.TotalAmount,
                InvoiceDeliveryLocation = Trim(seed.InvoiceDeliveryLocation, 50)
            };
            db.Invoices.Add(invoice);
            SetInvoiceShadows(invoice, batch, seed);

            foreach (var item in calculatedLines)
            {
                var line = new InvoiceLine
                {
                    CompanyId = companyId,
                    Name = Required(item.Seed.Name, 255, "Naziv stavke"),
                    K1 = FinanceRounding.Calculation(item.Seed.K1),
                    K2 = FinanceRounding.Calculation(item.Seed.K2),
                    K3 = FinanceRounding.Calculation(item.Seed.K3),
                    K4 = FinanceRounding.Calculation(item.Seed.K4),
                    K5 = FinanceRounding.Calculation(item.Seed.K5),
                    Quantity = item.Amounts.Quantity,
                    UnitOfMeasureId = item.Seed.UnitOfMeasureId,
                    PriceEur = 0m,
                    ExchangeRateNbs = batch.ExchangeRateNbs,
                    PricePcs = item.Amounts.UnitPrice,
                    PriceTotal = item.Amounts.NetAmount,
                    VatRate = item.Seed.VatRate,
                    VatAmount = item.Amounts.VatAmount,
                    TotalAmount = item.Amounts.TotalAmount,
                    SortIndex = item.Seed.SortIndex
                };
                invoice.Lines.Add(line);
                SetLineShadows(line, batchId, seed.PartnerId, item.Seed.SupplierInvoiceId, seed.Currency);
            }

            await db.SaveChangesAsync(ct);
            invoiceIds.Add(invoice.Id);

            foreach (var contractId in seed.ContractIds.Distinct())
            {
                db.Add(new InvoiceUnit { CompanyId = companyId, InvoiceId = invoice.Id, ContractId = contractId });
            }

        }

        batch.GenerationFingerprint = preview.Fingerprint;
        batch.GeneratedAt = timeProvider.GetUtcNow();
        batch.Status = BillingBatchStatus.Generated;
        await db.SaveChangesAsync(ct);
        return new(batchId, false, preview.Fingerprint, invoiceIds);
    }

    /// <summary>
    /// FIN-05/FIN-11: 9.2 batch posting -- one journal, 2040 per invoice × supplier invoice,
    /// 4900 (+4350/5590 for type 1) per supplier invoice, atomic with the batch status change
    /// and the PostedInvoiceAmount write-back (FIN-31).
    /// </summary>
    public Task<InvoiceBatchResponse> PostBatchAsync(int companyId, int batchId, string idempotencyKey, CancellationToken ct) =>
        ExecuteSerializableAsync(async () =>
        {
            await LockInvoiceBatchAsync(batchId, ct);
            var batch = await GetBatchAsync(companyId, batchId, true, ct);
            if (batch.Status == BillingBatchStatus.Posted) return ToBatchResponse(batch);
            if (batch.Status != BillingBatchStatus.Generated)
                throw new DomainRuleException("billing.batch-not-generated", "Samo generisana serija može biti knjižena.");

            var invoices = await db.Invoices.AsNoTracking()
                .Where(x => x.CompanyId == companyId && EF.Property<int?>(x, "InvoiceBatchId") == batchId && !x.IsCancelled)
                .Select(x => new InvoiceHead(x.Id, x.PartnerId, x.PartnerName, x.DueDate, x.InvoiceTotal, EF.Property<string?>(x, "PaymentReference")))
                .ToArrayAsync(ct);
            if (invoices.Length == 0) throw new DomainRuleException("billing.batch-empty", "Serija nema aktivne račune.");

            var lineSums = await db.InvoiceLines.AsNoTracking()
                .Where(x => x.CompanyId == companyId && EF.Property<int?>(x, "InvoiceBatchId") == batchId && !x.Invoice.IsCancelled)
                .GroupBy(x => new { x.InvoiceId, SupplierInvoiceId = EF.Property<int?>(x, "SupplierInvoiceId") })
                .Select(g => new { g.Key.InvoiceId, g.Key.SupplierInvoiceId, Amount = g.Sum(x => x.TotalAmount) })
                .ToArrayAsync(ct);

            var customerAccounts = await ResolveCustomerAccountsAsync(companyId, invoices, ct);
            var sources = new List<InvoicePostingSource>();
            foreach (var invoice in invoices)
            {
                var account = customerAccounts[invoice.Id];
                var own = lineSums.Where(x => x.InvoiceId == invoice.Id).ToArray();
                sources.AddRange(own.Select(x => new InvoicePostingSource(
                    invoice.Id, account.Id, account.Account, invoice.DueDate, invoice.PaymentReference, x.SupplierInvoiceId, x.Amount)));
                // Invoice-level amounts that aren't on a line (benefit reduction, interest,
                // rounding) still belong to the customer's debt: posted without a supplier invoice.
                var residual = FinanceRounding.Money(invoice.InvoiceTotal - own.Sum(x => x.Amount));
                if (residual != 0m)
                {
                    sources.Add(new InvoicePostingSource(
                        invoice.Id, account.Id, account.Account, invoice.DueDate, invoice.PaymentReference, null, residual));
                }
            }

            var supplierIds = sources.Where(x => x.SupplierInvoiceId is not null).Select(x => x.SupplierInvoiceId!.Value).Distinct().ToArray();
            var suppliers = await LoadSupplierPostingInfoAsync(companyId, supplierIds, ct);
            var documentRef = DocumentPostingRules.InvoiceDocumentRef(batch.PeriodYYMM);
            var lines = DocumentPostingRules.BuildInvoiceBatch(batch.TransactionDate, documentRef, sources, suppliers);
            var result = await ledger.PostAsync(new LedgerPostingRequest(
                companyId, "InvoiceBatch", batchId, batch.TransactionDate, $"SZ RACUNI {documentRef}", "RSD", idempotencyKey, lines), ct);

            await AdjustPostedSupplierAmountsAsync(companyId, lines, ct);
            batch.JournalEntryId = result.JournalEntryId;
            batch.Status = BillingBatchStatus.Posted;
            batch.PostedAt = timeProvider.GetUtcNow();
            await db.SaveChangesAsync(ct);
            return ToBatchResponse(batch);
        }, ct);

    public async Task<BillingPage<InvoiceResponse>> ListInvoicesAsync(int companyId, int page, int pageSize, CancellationToken ct)
    {
        (page, pageSize) = NormalizePage(page, pageSize);
        var query = db.Invoices.AsNoTracking().Where(x => x.CompanyId == companyId);
        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(x => x.IssueDate).ThenBy(x => x.SequenceNumber)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new InvoiceResponse(x.Id, x.PartnerId, EF.Property<int?>(x, "InvoiceBatchId"), x.SequenceNumber,
                x.IssueDate, x.DueDate, x.PartnerName, x.Address, x.PostalCode, x.City, x.Currency, x.Amount,
                x.VatAmount, x.Total, x.InterestAmount, x.InvoiceTotal, x.IsCancelled, Convert.ToBase64String(x.RowVersion), null))
            .ToArrayAsync(ct);
        return new(items, page, pageSize, total);
    }

    public async Task<InvoiceResponse?> GetInvoiceAsync(int companyId, int invoiceId, CancellationToken ct)
    {
        var invoice = await db.Invoices.Include(x => x.Lines)
            .SingleOrDefaultAsync(x => x.CompanyId == companyId && x.Id == invoiceId, ct);
        if (invoice is null) return null;
        var lines = invoice.Lines.OrderBy(x => x.SortIndex).Select(x => new InvoiceLineResponse(
            x.Id, x.Name, x.Quantity, x.PricePcs, x.PriceTotal, x.VatRate, x.VatAmount, x.TotalAmount, x.SortIndex)).ToArray();
        return ToInvoiceResponse(invoice, db.Entry(invoice).Property<int?>("InvoiceBatchId").CurrentValue, lines);
    }

    /// <summary>
    /// FIN-02 / P9: storno of ONE invoice -- a new journal with this invoice's 2040 lines negated on
    /// the same side plus the matching negative 4900 (+4350/5590), type 7, dated today (Belgrade).
    /// </summary>
    public Task<InvoiceResponse> CancelInvoiceAsync(int companyId, int invoiceId, CancelInvoiceRequest request, string idempotencyKey, CancellationToken ct) =>
        ExecuteSerializableAsync(async () =>
        {
            var invoice = await db.Invoices.SingleOrDefaultAsync(x => x.CompanyId == companyId && x.Id == invoiceId, ct)
                ?? throw new DomainRuleException("billing.invoice-not-found", "Račun ne postoji.");
            if (invoice.IsCancelled) return ToInvoiceResponse(invoice, db.Entry(invoice).Property<int?>("InvoiceBatchId").CurrentValue, null);
            db.Entry(invoice).Property(x => x.RowVersion).OriginalValue = DecodeRowVersion(request.RowVersion);
            var reason = Required(request.Reason, 500, "Razlog storna");

            var batchId = db.Entry(invoice).Property<int?>("InvoiceBatchId").CurrentValue;
            var batch = batchId is null ? null : await db.Set<InvoiceBatch>().SingleAsync(x => x.CompanyId == companyId && x.Id == batchId, ct);
            if (batch is { Status: BillingBatchStatus.Posted, JournalEntryId: { } journalId })
            {
                var posted = (await db.LedgerEntries.AsNoTracking()
                        .Where(x => x.CompanyId == companyId && x.JournalEntryId == journalId && x.InvoiceId == invoiceId)
                        .OrderBy(x => x.Priority)
                        .Select(x => new
                        {
                            x.Account, x.DebitAmount, x.CreditAmount, x.PostingDate, x.DueDate, x.DocumentRef, x.Parameters,
                            x.SupplierInvoiceId, x.CollectionPriority, x.Note,
                            PartnerAccountId = EF.Property<int?>(x, "PartnerAccountId"),
                            SubAccountId = EF.Property<string?>(x, "SubAccountId")
                        })
                        .ToArrayAsync(ct))
                    .Select(x => new PostingLine(x.Account, x.DebitAmount, x.CreditAmount, LedgerLineTypes.Invoice, x.PostingDate,
                        x.PartnerAccountId, x.SubAccountId, x.DueDate, x.DocumentRef, x.Parameters, invoiceId,
                        x.SupplierInvoiceId, x.CollectionPriority, x.Note))
                    .ToArray();
                if (posted.Length == 0)
                {
                    // Batches posted before per-document posting existed carry no InvoiceId on their lines.
                    throw new DomainRuleException("billing.invoice-not-posted-per-document",
                        "Serija je proknjižena zbirno (stari način); storno pojedinačnog računa nije moguć automatski.");
                }

                var supplierIds = posted.Where(x => x.SupplierInvoiceId is not null).Select(x => x.SupplierInvoiceId!.Value).Distinct().ToArray();
                var suppliers = await LoadSupplierPostingInfoAsync(companyId, supplierIds, ct);
                var stornoDate = clock.Today;
                var lines = DocumentPostingRules.BuildInvoiceStorno(stornoDate, posted, suppliers);
                await ledger.PostAsync(new LedgerPostingRequest(
                    companyId, "InvoiceCancel", invoiceId, stornoDate, $"STORNO računa {invoice.SequenceNumber}", invoice.Currency, idempotencyKey, lines), ct);
                await AdjustPostedSupplierAmountsAsync(companyId, lines, ct);
            }

            invoice.IsCancelled = true;
            invoice.CancelledAt = timeProvider.GetUtcNow();
            db.Entry(invoice).Property("CancelReason").CurrentValue = reason;
            await db.SaveChangesAsync(ct);
            return ToInvoiceResponse(invoice, batchId, null);
        }, ct);

    /// <summary>FIN-34: standalone posting of a supplier invoice (types 2 and 3), legacy GK_KnjizenjeRacunaTroska.</summary>
    public Task<SupplierInvoiceResponse> PostSupplierInvoiceAsync(int companyId, int supplierInvoiceId, string idempotencyKey, CancellationToken ct) =>
        ExecuteSerializableAsync(async () =>
        {
            await LockSupplierInvoiceAsync(supplierInvoiceId, ct);
            var entity = await GetSupplierInvoiceAsync(companyId, supplierInvoiceId, ct);
            if (entity.JournalEntryId is null)
            {
                var info = (await LoadSupplierPostingInfoAsync(companyId, [supplierInvoiceId], ct))[supplierInvoiceId];
                var lines = DocumentPostingRules.BuildSupplierInvoice(info);
                var result = await ledger.PostAsync(new LedgerPostingRequest(
                    companyId, SupplierInvoiceSource, supplierInvoiceId, info.InvoiceDate,
                    $"Račun RT {entity.CodeName}", "RSD", idempotencyKey, lines), ct);
                entity.JournalEntryId = result.JournalEntryId;
                await db.SaveChangesAsync(ct);
            }
            return await GetSupplierInvoiceResponseAsync(companyId, supplierInvoiceId, ct);
        }, ct);

    /// <summary>
    /// Red storno of a posted supplier invoice. Terminal: a corrected document is entered as a new
    /// supplier invoice (legacy Previous/NewSupplierInvoiceId), it can't be re-posted.
    /// </summary>
    public Task<SupplierInvoiceResponse> CancelSupplierInvoicePostingAsync(int companyId, int supplierInvoiceId, string idempotencyKey, CancellationToken ct) =>
        ExecuteSerializableAsync(async () =>
        {
            await LockSupplierInvoiceAsync(supplierInvoiceId, ct);
            var entity = await GetSupplierInvoiceAsync(companyId, supplierInvoiceId, ct);
            if (entity.JournalEntryId is not { } journalId)
                throw new DomainRuleException("billing.supplier-invoice-not-posted", "Ulazni račun nije proknjižen.");

            var posted = (await db.LedgerEntries.AsNoTracking()
                    .Where(x => x.CompanyId == companyId && x.JournalEntryId == journalId)
                    .OrderBy(x => x.Priority)
                    .Select(x => new
                    {
                        x.Account, x.DebitAmount, x.CreditAmount, x.PostingDate, x.DueDate, x.DocumentRef, x.Parameters,
                        x.SupplierInvoiceId, x.CollectionPriority, x.Note,
                        PartnerAccountId = EF.Property<int?>(x, "PartnerAccountId"),
                        SubAccountId = EF.Property<string?>(x, "SubAccountId")
                    })
                    .ToArrayAsync(ct))
                .Select(x => new PostingLine(x.Account, x.DebitAmount, x.CreditAmount, LedgerLineTypes.SupplierInvoice, x.PostingDate,
                    x.PartnerAccountId, x.SubAccountId, x.DueDate, x.DocumentRef, x.Parameters, null,
                    x.SupplierInvoiceId, x.CollectionPriority, x.Note))
                .ToArray();
            var stornoDate = clock.Today;
            await ledger.PostAsync(new LedgerPostingRequest(
                companyId, SupplierInvoiceCancelSource, supplierInvoiceId, stornoDate,
                $"STORNO Račun RT {entity.CodeName}", "RSD", idempotencyKey, DocumentPostingRules.Negate(posted, stornoDate)), ct);
            return await GetSupplierInvoiceResponseAsync(companyId, supplierInvoiceId, ct);
        }, ct);

    private sealed record InvoiceHead(int Id, int PartnerId, string PartnerName, DateOnly DueDate, decimal InvoiceTotal, string? PaymentReference);

    /// <summary>
    /// Customer's 2040 partner account per invoice: the partner's 2040 account in this company,
    /// preferring one tied to a contract billed on the invoice, then a company-specific one.
    /// </summary>
    private async Task<Dictionary<int, (int Id, string Account)>> ResolveCustomerAccountsAsync(int companyId, IReadOnlyCollection<InvoiceHead> invoices, CancellationToken ct)
    {
        var partnerIds = invoices.Select(x => x.PartnerId).Distinct().ToArray();
        var invoiceIds = invoices.Select(x => x.Id).ToArray();
        var accounts = await db.Set<PartnerAccount>().AsNoTracking()
            .Where(x => partnerIds.Contains(x.PartnerId) && x.Account == LedgerAccounts.Customers && (x.CompanyId == companyId || x.CompanyId == null))
            .Select(x => new { x.Id, x.PartnerId, x.ContractId, x.CompanyId, x.Account })
            .ToArrayAsync(ct);
        var contracts = (await db.Set<InvoiceUnit>().AsNoTracking()
                .Where(x => x.CompanyId == companyId && invoiceIds.Contains(x.InvoiceId))
                .Select(x => new { x.InvoiceId, x.ContractId })
                .ToArrayAsync(ct))
            .ToLookup(x => x.InvoiceId, x => x.ContractId);

        var result = new Dictionary<int, (int, string)>();
        foreach (var invoice in invoices)
        {
            var billed = contracts[invoice.Id].ToHashSet();
            var account = accounts.Where(x => x.PartnerId == invoice.PartnerId)
                .OrderByDescending(x => x.ContractId is { } c && billed.Contains(c))
                .ThenByDescending(x => x.CompanyId == companyId)
                .ThenBy(x => x.Id)
                .FirstOrDefault()
                ?? throw new DomainRuleException("billing.customer-account-missing", $"Kupac '{invoice.PartnerName}' nema konto {LedgerAccounts.Customers}.");
            result[invoice.Id] = (account.Id, account.Account);
        }
        return result;
    }

    private async Task<Dictionary<int, SupplierPostingInfo>> LoadSupplierPostingInfoAsync(int companyId, IReadOnlyCollection<int> ids, CancellationToken ct)
    {
        if (ids.Count == 0) return [];
        var rows = await (
                from s in db.Set<SupplierInvoice>().AsNoTracking()
                join type in db.ShortLists.AsNoTracking() on s.DocumentTypeId equals type.Id
                join supplier in db.Set<PartnerAccount>().AsNoTracking() on s.SupplierPartnerAccountId equals supplier.Id
                where s.CompanyId == companyId && ids.Contains(s.Id)
                select new
                {
                    s.Id, DocumentType = type.IndexValue, s.SupplierPartnerAccountId, SupplierAccount = supplier.Account, s.SubAccountId,
                    s.PaymentPriority, s.PaymentReference, s.Caption, s.CodeName, s.InvoiceTotalCalculationAmountRsd,
                    s.InvoiceDate, s.PaymentDate, s.ClosesAccount
                })
            .ToArrayAsync(ct);
        if (rows.Length != ids.Count)
            throw new DomainRuleException("billing.supplier-invoice-not-found", "Ulazni račun ne pripada aktivnoj kompaniji ili nema tip dokumenta / konto dobavljača.");
        return rows.ToDictionary(x => x.Id, x => new SupplierPostingInfo(
            x.Id, x.DocumentType, x.SupplierPartnerAccountId, x.SupplierAccount, x.SubAccountId, x.PaymentPriority,
            x.PaymentReference, x.Caption, x.CodeName, x.InvoiceTotalCalculationAmountRsd, x.InvoiceDate, x.PaymentDate, x.ClosesAccount));
    }

    /// <summary>FIN-31 (legacy DobavljacInfoKnjizenogIznosa): PostedInvoiceAmount follows what 4900 received per supplier invoice.</summary>
    private async Task AdjustPostedSupplierAmountsAsync(int companyId, IEnumerable<PostingLine> lines, CancellationToken ct)
    {
        var deltas = lines.Where(x => x.Account == LedgerAccounts.Revenue && x.SupplierInvoiceId is not null)
            .GroupBy(x => x.SupplierInvoiceId!.Value)
            .ToDictionary(g => g.Key, g => FinanceRounding.Money(g.Sum(x => x.Credit)));
        if (deltas.Count == 0) return;
        var ids = deltas.Keys.ToArray();
        foreach (var supplier in await db.Set<SupplierInvoice>().Where(x => x.CompanyId == companyId && ids.Contains(x.Id)).ToArrayAsync(ct))
        {
            supplier.PostedInvoiceAmount = FinanceRounding.Money(supplier.PostedInvoiceAmount + deltas[supplier.Id]);
        }
    }

    private async Task<SupplierInvoice> GetSupplierInvoiceAsync(int companyId, int supplierInvoiceId, CancellationToken ct)
    {
        var entity = await db.Set<SupplierInvoice>().SingleOrDefaultAsync(x => x.CompanyId == companyId && x.Id == supplierInvoiceId, ct)
            ?? throw new DomainRuleException("billing.supplier-invoice-not-found", "Ulazni račun ne postoji.");
        if (await db.Set<LedgerSourcePosting>().AnyAsync(x => x.CompanyId == companyId && x.SourceType == SupplierInvoiceCancelSource && x.SourceId == supplierInvoiceId, ct))
            throw new DomainRuleException("billing.supplier-invoice-cancelled", "Knjiženje ulaznog računa je već stornirano; ispravku unesite kao novi ulazni račun.");
        return entity;
    }

    private async Task<SupplierInvoiceResponse> GetSupplierInvoiceResponseAsync(int companyId, int supplierInvoiceId, CancellationToken ct) =>
        (await ListSupplierInvoicesByIdAsync(companyId, supplierInvoiceId, ct)).Single();

    private Task<SupplierInvoiceResponse[]> ListSupplierInvoicesByIdAsync(int companyId, int supplierInvoiceId, CancellationToken ct) =>
        db.Set<SupplierInvoice>().AsNoTracking().Where(x => x.CompanyId == companyId && x.Id == supplierInvoiceId)
            .Select(x => new SupplierInvoiceResponse(
                x.Id, x.InvoiceNo, x.CodeName, x.Caption, x.CalculationTypeId, x.PeriodYYMM,
                x.InvoiceTotalCalculationAmountEur, x.InvoiceTotalCalculationAmountRsd, x.PostedInvoiceAmount,
                x.InvoiceDate, x.TransactionDate, x.SupplierPartnerAccountId, x.DocumentTypeId, x.ExtraordinaryInvoiceMarker,
                x.UnitTypes.OrderBy(y => y.UnitTypeId).Select(y => y.UnitTypeId).ToArray(),
                Convert.ToBase64String(x.RowVersion), x.JournalEntryId,
                db.Set<LedgerSourcePosting>().Any(p => p.CompanyId == x.CompanyId && p.SourceType == SupplierInvoiceCancelSource && p.SourceId == x.Id)))
            .ToArrayAsync(ct);

    private Task LockSupplierInvoiceAsync(int supplierInvoiceId, CancellationToken ct) =>
        db.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT 1 FROM [billing].[SupplierInvoice] WITH (UPDLOCK, HOLDLOCK) WHERE [Id] = {supplierInvoiceId}",
            ct);

    public async Task<IReadOnlyList<BenefitResponse>> ListBenefitsAsync(int companyId, int period, CancellationToken ct) =>
        await db.Set<Benefit>().AsNoTracking().Where(x => x.CompanyId == companyId && x.PeriodYYMM == period)
            .OrderBy(x => x.ContractId).Select(x => new BenefitResponse(x.Id, x.ContractId, x.PeriodYYMM, x.Amount, x.InvoiceId, Convert.ToBase64String(x.RowVersion))).ToArrayAsync(ct);

    public async Task<BenefitResponse> CreateBenefitAsync(int companyId, CreateBenefitRequest request, CancellationToken ct)
    {
        EnsurePeriod(request.PeriodYYMM);
        if (request.Amount <= 0m) throw new DomainRuleException("billing.invalid-benefit", "Benefit mora biti veći od nule.");
        if (!await db.Set<Contract>().AnyAsync(x => x.Id == request.ContractId && x.CompanyId == companyId, ct))
            throw new DomainRuleException("billing.contract-not-found", "Ugovor ne pripada aktivnoj kompaniji.");
        var entity = new Benefit { CompanyId = companyId, ContractId = request.ContractId, PeriodYYMM = request.PeriodYYMM, Amount = FinanceRounding.Money(request.Amount), EntryDate = timeProvider.GetUtcNow() };
        db.Add(entity);
        await db.SaveChangesAsync(ct);
        return new(entity.Id, entity.ContractId, entity.PeriodYYMM, entity.Amount, null, Convert.ToBase64String(entity.RowVersion));
    }

    public async Task<IReadOnlyList<InterestRateResponse>> ListInterestRatesAsync(CancellationToken ct) =>
        await db.Set<InterestRate>().AsNoTracking().OrderByDescending(x => x.Date)
            .Select(x => new InterestRateResponse(x.Id, x.Date, x.Rate, x.TimeCode)).ToArrayAsync(ct);

    public async Task<InterestRateResponse> CreateInterestRateAsync(CreateInterestRateRequest request, CancellationToken ct)
    {
        var timeCode = request.TimeCode.Trim().ToUpperInvariant();
        if (request.Rate < 0m || timeCode is not ("M" or "G")) throw new DomainRuleException("interest.invalid-rate", "Stopa ili vremenska oznaka nisu ispravni.");
        var entity = new InterestRate { Date = request.Date, Rate = FinanceRounding.Calculation(request.Rate), TimeCode = timeCode };
        db.Add(entity);
        await db.SaveChangesAsync(ct);
        return new(entity.Id, entity.Date, entity.Rate, entity.TimeCode);
    }

    public async Task<InterestCalculationResponse> CalculateInterestAsync(CalculateInterestRequest request, CancellationToken ct)
    {
        var rates = await LoadInterestRatesAsync(request.To, ct);
        var calculated = InterestCalculator.Calculate(request.Principal, request.From, request.To, rates);
        var lines = calculated.Select(x => new InterestCalculationLineResponse(x.From, x.To, x.Days, x.AnnualRate, x.Coefficient, x.Interest)).ToArray();
        return new(FinanceRounding.Money(request.Principal), FinanceRounding.Money(calculated.Sum(x => x.Interest)), lines);
    }

    public async Task<IReadOnlyList<InterestStatementResponse>> ListInterestStatementsAsync(int companyId, int invoiceBatchId, CancellationToken ct) =>
        await db.Set<InterestStatement>().AsNoTracking()
            .Where(x => x.CompanyId == companyId && x.InvoiceBatchId == invoiceBatchId)
            .OrderBy(x => x.PartnerAccountId).ThenBy(x => x.SubAccountId).ThenBy(x => x.Date)
            .Select(x => new InterestStatementResponse(x.Id, x.Account, x.Date, x.Amount, x.Balance, x.Days,
                x.Rate, x.Coefficient, x.Interest, x.PartnerAccountId, x.SubAccountId, x.InvoiceBatchId))
            .ToArrayAsync(ct);

    /// <summary>P10 suggestions for the run period; the run itself always takes explicit dates.</summary>
    public async Task<InterestPeriodPresetsResponse> GetInterestPeriodPresetsAsync(
        int companyId, int periodYYMM, DateOnly? previousValueDate, DateOnly? balanceAsOfDate, DateOnly? dueDate, CancellationToken ct)
    {
        var previousBatchDue = await db.Set<InvoiceBatch>().AsNoTracking()
            .Where(x => x.CompanyId == companyId && x.PeriodYYMM < periodYYMM && x.ExtraordinaryInvoiceMarker == null)
            .OrderByDescending(x => x.PeriodYYMM).Select(x => (DateOnly?)x.DueDate).FirstOrDefaultAsync(ct);
        var lastRunEnd = await db.Set<InvoiceBatch>().AsNoTracking()
            .Where(x => x.CompanyId == companyId && x.InterestPeriodEnd != null)
            .MaxAsync(x => x.InterestPeriodEnd, ct);
        var setting = await db.Set<Setting>().AsNoTracking()
            .Where(x => (x.CompanyId == companyId || x.CompanyId == null) && x.Key == InterestPeriodPresets.DefaultPresetSettingKey)
            .OrderByDescending(x => x.CompanyId).Select(x => x.Value).FirstOrDefaultAsync(ct);
        var presets = InterestPeriodPresets.Suggest(periodYYMM, previousValueDate, balanceAsOfDate, dueDate, previousBatchDue);
        return new(InterestPeriodPresets.NormalizeKey(setting), lastRunEnd,
            presets.Select(x => new InterestPeriodPresetResponse(x.Key, x.Start, x.End)).ToArray());
    }

    /// <summary>
    /// Legacy ObracunajKamatu: ClearKamatniList (delete the batch's rows) + recreate from the 2040 ledger.
    /// Idempotent per batch; periods of different batches of one company may not overlap (runs cover every partner).
    /// </summary>
    public Task<InterestRunResponse> RunInterestAsync(int companyId, RunInterestRequest request, CancellationToken ct) =>
        ExecuteSerializableAsync(async () =>
        {
            if (request.PeriodEnd < request.PeriodStart)
                throw new DomainRuleException("interest.invalid-period", "Početak perioda kamate mora biti pre kraja.");
            await LockInvoiceBatchAsync(request.InvoiceBatchId, ct);
            var batch = await db.Set<InvoiceBatch>().SingleOrDefaultAsync(x => x.Id == request.InvoiceBatchId && x.CompanyId == companyId, ct)
                ?? throw new DomainRuleException("billing.batch-not-found", "Serija računa ne postoji.");
            if (batch.Status == BillingBatchStatus.Posted)
                throw new DomainRuleException("interest.batch-posted", "Serija je proknjižena; kamata se ne može ponovo obračunati.");
            var overlapping = await db.Set<InvoiceBatch>().AsNoTracking()
                .Where(x => x.CompanyId == companyId && x.Id != batch.Id && x.InterestPeriodStart != null
                    && x.InterestPeriodStart <= request.PeriodEnd && request.PeriodStart <= x.InterestPeriodEnd)
                .Select(x => new { x.Caption, x.InterestPeriodStart, x.InterestPeriodEnd })
                .FirstOrDefaultAsync(ct);
            if (overlapping is not null)
                throw new DomainRuleException("interest.period-overlap",
                    $"Period se preklapa sa obračunom kamate serije '{overlapping.Caption}' ({overlapping.InterestPeriodStart:dd.MM.yyyy}–{overlapping.InterestPeriodEnd:dd.MM.yyyy}).");

            var rates = await LoadInterestRatesAsync(request.PeriodEnd, ct);
            var movements = await LoadInterestBaseAsync(companyId, request.PeriodEnd, ct);
            var rows = InterestCalculator.CalculateRows(request.PeriodStart, request.PeriodEnd, rates, movements);

            await db.Set<InterestStatement>().Where(x => x.CompanyId == companyId && x.InvoiceBatchId == batch.Id).ExecuteDeleteAsync(ct);
            db.AddRange(rows.Select(x => new InterestStatement
            {
                CompanyId = companyId,
                Account = LedgerAccounts.Customers,
                Date = x.From,
                Amount = x.Movement,
                Balance = x.Balance,
                Days = x.Days,
                Rate = x.Rate,
                Coefficient = FinanceRounding.Calculation(x.Coefficient),
                Interest = x.Interest,
                PartnerAccountId = x.PartnerAccountId,
                SubAccountId = x.SubAccountId,
                InvoiceBatchId = batch.Id
            }));
            batch.IsInterestCalculated = true;
            batch.InterestPeriodStart = request.PeriodStart;
            batch.InterestPeriodEnd = request.PeriodEnd;
            await db.SaveChangesAsync(ct);

            var totals = InterestCalculator.Totals(rows);
            return new InterestRunResponse(batch.Id, request.PeriodStart, request.PeriodEnd, rows.Count,
                FinanceRounding.Money(totals.Sum(x => x.Interest)),
                totals.Select(x => new InterestTotalResponse(x.PartnerAccountId, x.SubAccountId, x.Interest)).ToArray());
        }, ct);

    private async Task<InterestRatePoint[]> LoadInterestRatesAsync(DateOnly to, CancellationToken ct) =>
        await db.Set<InterestRate>().AsNoTracking().Where(x => x.TimeCode == "G" && x.Date <= to)
            .OrderBy(x => x.Date).Select(x => new InterestRatePoint(x.Date, x.Rate)).ToArrayAsync(ct);

    /// <summary>
    /// The single query behind the interest base (audit 9.4): posted 2040 lines with a partner account and a
    /// sub-account whose SubAccount.InterestSubAccountId is set (legacy Troskovi_PodKonta.Kamata not -1, so interest
    /// itself, booked on the mapped sub-account, never earns interest). Date = legacy DPO (due date, else posting date).
    /// Sub-accounts 9xxxx are merged into 3xxxx like legacy.
    /// </summary>
    private async Task<IReadOnlyList<InterestMovement>> LoadInterestBaseAsync(int companyId, DateOnly to, CancellationToken ct)
    {
        var rows = await (
                from e in db.LedgerEntries.AsNoTracking()
                join s in db.Set<SubAccount>().AsNoTracking() on EF.Property<string?>(e, "SubAccountId") equals s.Id
                where e.CompanyId == companyId && e.Account == LedgerAccounts.Customers
                    && EF.Property<int?>(e, "PartnerAccountId") != null
                    && s.InterestSubAccountId != null && s.InterestSubAccountId != "-1"
                    && (e.DueDate ?? e.PostingDate) <= to
                select new
                {
                    PartnerAccountId = EF.Property<int?>(e, "PartnerAccountId")!.Value,
                    SubAccountId = s.Id,
                    Date = e.DueDate ?? e.PostingDate,
                    Amount = e.DebitAmount - e.CreditAmount
                })
            .ToArrayAsync(ct);
        return rows.Select(x => new InterestMovement(x.PartnerAccountId,
                x.SubAccountId.StartsWith('9') ? "3" + x.SubAccountId[1..] : x.SubAccountId,
                x.Date, x.Amount))
            .ToArray();
    }

    public async Task<IReadOnlyList<NoticeAditionalCostResponse>> ListNoticeCostsAsync(int companyId, CancellationToken ct) =>
        await db.Set<NoticeAditionalCost>().AsNoTracking()
            .Where(x => x.CompanyId == null || x.CompanyId == companyId)
            .OrderBy(x => x.CompanyId == null).ThenByDescending(x => x.DateStart)
            .Select(x => ToNoticeCostResponse(x))
            .ToArrayAsync(ct);

    /// <summary>P11 rule create (id null) / edit. Global rows are Root-only; the scope of an existing row never changes.</summary>
    public Task<NoticeAditionalCostResponse> SaveNoticeCostAsync(int companyId, int? id, SaveNoticeAditionalCostRequest request, bool isRoot, CancellationToken ct) =>
        ExecuteSerializableAsync(async () =>
        {
            NoticeAditionalCost entity;
            if (id is null)
            {
                entity = new NoticeAditionalCost { CompanyId = request.IsGlobal ? null : companyId };
                db.Add(entity);
            }
            else
            {
                entity = await FindNoticeCostAsync(companyId, id.Value, ct);
                db.Entry(entity).Property(x => x.RowVersion).OriginalValue =
                    DecodeRowVersion(request.RowVersion ?? throw new DomainRuleException("concurrency.row-version-required", "RowVersion je obavezan za izmenu."));
            }
            if (entity.CompanyId is null && !isRoot)
                throw new UnauthorizedAccessException("Globalne troškove opomena menja samo Root.");

            var rule = new NoticeCostRule(entity.Id, request.DateStart, request.DateEnd, entity.CompanyId,
                FinanceRounding.Money(request.AditionalCostsLowerAmount), FinanceRounding.Money(request.AditionalCostsLowerLimit),
                FinanceRounding.Money(request.AditionalCostsUpperAmount));
            NoticeCostCalculator.EnsureValid(rule, await LoadNoticeCostRulesAsync(entity.CompanyId, ct));
            entity.DateStart = rule.DateStart;
            entity.DateEnd = rule.DateEnd;
            entity.AditionalCostsLowerAmount = rule.LowerAmount;
            entity.AditionalCostsLowerLimit = rule.LowerLimit;
            entity.AditionalCostsUpperAmount = rule.UpperAmount;
            await db.SaveChangesAsync(ct);
            return ToNoticeCostResponse(entity);
        }, ct);

    public async Task DeleteNoticeCostAsync(int companyId, int id, bool isRoot, CancellationToken ct)
    {
        var entity = await FindNoticeCostAsync(companyId, id, ct);
        if (entity.CompanyId is null && !isRoot)
            throw new UnauthorizedAccessException("Globalne troškove opomena briše samo Root.");
        db.Remove(entity);
        await db.SaveChangesAsync(ct);
    }

    private async Task<NoticeAditionalCost> FindNoticeCostAsync(int companyId, int id, CancellationToken ct) =>
        await db.Set<NoticeAditionalCost>().SingleOrDefaultAsync(x => x.Id == id && (x.CompanyId == null || x.CompanyId == companyId), ct)
            ?? throw new DomainRuleException("notice-costs.not-found", "Red troškova opomena ne postoji.");

    private static NoticeAditionalCostResponse ToNoticeCostResponse(NoticeAditionalCost x) =>
        new(x.Id, x.DateStart, x.DateEnd, x.CompanyId, x.AditionalCostsLowerAmount, x.AditionalCostsLowerLimit,
            x.AditionalCostsUpperAmount, Convert.ToBase64String(x.RowVersion));

    public async Task<IReadOnlyList<NoticeTemplateResponse>> ListNoticeTemplatesAsync(int companyId, CancellationToken ct) =>
        await db.Set<NoticeTemplate>().AsNoTracking().Where(x => x.CompanyId == companyId)
            .OrderBy(x => x.Name).Select(x => new NoticeTemplateResponse(x.Id, x.Name, x.Body, x.IsActive, Convert.ToBase64String(x.RowVersion)))
            .ToArrayAsync(ct);

    public async Task<NoticeTemplateResponse> CreateNoticeTemplateAsync(int companyId, CreateNoticeTemplateRequest request, CancellationToken ct)
    {
        var entity = new NoticeTemplate { CompanyId = companyId, Name = Required(request.Name, 100, "Naziv šablona"), Body = Required(request.Body, int.MaxValue, "Sadržaj šablona") };
        db.Add(entity);
        await db.SaveChangesAsync(ct);
        return new(entity.Id, entity.Name, entity.Body, entity.IsActive, Convert.ToBase64String(entity.RowVersion));
    }

    public async Task<NoticeBatchResponse> CreateNoticeBatchAsync(int companyId, CreateNoticeBatchRequest request, CancellationToken ct)
    {
        await shortLists.EnsureTypeAsync(request.NoticeTypeId, NoticeTypes.ShortListTable, ct);
        if (!await db.Set<NoticeTemplate>().AnyAsync(x => x.Id == request.NoticeTemplateId && x.CompanyId == companyId && x.IsActive, ct))
            throw new DomainRuleException("notice.template-not-found", "Aktivan šablon opomene ne postoji.");
        if (request.InvoiceBatchId is not null &&
            !await db.Set<InvoiceBatch>().AnyAsync(x => x.Id == request.InvoiceBatchId && x.CompanyId == companyId, ct))
            throw new DomainRuleException("notice.invoice-batch-not-found", "Serija računa ne pripada aktivnoj kompaniji.");
        var entity = new NoticeBatch
        {
            CompanyId = companyId,
            Title = Required(request.Title, 50, "Naziv serije opomena"),
            Date = request.Date,
            MinUnpaidInvoiceCount = request.MinUnpaidInvoiceCount,
            DebtTolerance = FinanceRounding.Money(request.DebtTolerance),
            DebtToleranceByMonth = FinanceRounding.Money(request.DebtToleranceByMonth),
            NoticeTemplateId = request.NoticeTemplateId,
            NoticeTypeId = request.NoticeTypeId,
            UpToClaimDate = request.UpToClaimDate,
            UpToPaymentDate = request.UpToPaymentDate,
            InvoiceBatchId = request.InvoiceBatchId,
            CustomCaptionOnSlip = Trim(request.CustomCaptionOnSlip, 255)
        };
        // P11: snapshot the cost thresholds in force on the batch date (company row beats global).
        var thresholds = NoticeCostCalculator.Snapshot(NoticeCostCalculator.Applicable(await LoadNoticeCostRulesAsync(companyId, ct), companyId, request.Date));
        entity.AditionalCostsLowerAmount = thresholds?.LowerAmount;
        entity.AditionalCostsLowerLimit = thresholds?.LowerLimit;
        entity.AditionalCostsUpperAmount = thresholds?.UpperAmount;
        db.Add(entity);
        await db.SaveChangesAsync(ct);
        return new(entity.Id, entity.Title, entity.Date, entity.NoticeTemplateId, entity.NoticeTypeId,
            entity.AditionalCostsLowerAmount, entity.AditionalCostsLowerLimit, entity.AditionalCostsUpperAmount, Convert.ToBase64String(entity.RowVersion));
    }

    private async Task<NoticeCostRule[]> LoadNoticeCostRulesAsync(int? companyId, CancellationToken ct) =>
        await db.Set<NoticeAditionalCost>().AsNoTracking()
            .Where(x => x.CompanyId == null || x.CompanyId == companyId)
            .Select(x => new NoticeCostRule(x.Id, x.DateStart, x.DateEnd, x.CompanyId,
                x.AditionalCostsLowerAmount, x.AditionalCostsLowerLimit, x.AditionalCostsUpperAmount))
            .ToArrayAsync(ct);

    public Task<NoticeGenerationResponse> GenerateNoticesAsync(int companyId, int batchId, GenerateNoticesRequest request, CancellationToken ct) =>
        ExecuteSerializableAsync(() => GenerateNoticesCoreAsync(companyId, batchId, request, ct), ct);

    // FIN-12: debt/lines/count are computed here from the general ledger (9.5), never taken from
    // the client. Regenerating an already-generated batch requires Confirm (legacy: confirm, then
    // delete-and-recreate).
    private async Task<NoticeGenerationResponse> GenerateNoticesCoreAsync(int companyId, int batchId, GenerateNoticesRequest request, CancellationToken ct)
    {
        await LockNoticeBatchAsync(batchId, ct);
        var batch = await db.Set<NoticeBatch>().Include(x => x.Notices).ThenInclude(x => x.Lines)
                .SingleOrDefaultAsync(x => x.Id == batchId && x.CompanyId == companyId, ct)
            ?? throw new DomainRuleException("notice.batch-not-found", "Serija opomena ne postoji.");

        if (batch.Notices.Count > 0)
        {
            if (!request.Confirm)
                throw new DomainRuleException("notice.regenerate-confirm-required", "Opomene za ovu seriju već postoje. Potvrdite ponovno generisanje.");
            db.RemoveRange(batch.Notices.SelectMany(x => x.Lines));
            db.RemoveRange(batch.Notices);
            batch.Notices.Clear();
        }

        var noticeTypeCode = await db.ShortLists.AsNoTracking().Where(x => x.Id == batch.NoticeTypeId).Select(x => x.IndexValue).SingleAsync(ct);
        var carriesCost = NoticeTypes.CarriesCost(noticeTypeCode);
        NoticeCostThresholds? thresholds = batch.AditionalCostsLowerAmount is { } lowerAmount
            && batch.AditionalCostsLowerLimit is { } lowerLimit && batch.AditionalCostsUpperAmount is { } upperAmount
            ? new(lowerAmount, lowerLimit, upperAmount)
            : null;

        var glLines = await (
                from e in db.LedgerEntries.AsNoTracking()
                join pa in db.Set<PartnerAccount>().AsNoTracking() on EF.Property<int?>(e, "PartnerAccountId") equals (int?)pa.Id
                where e.CompanyId == companyId && e.Account == LedgerAccounts.Customers && e.JournalEntry.IsPosted
                select new NoticeGlLine(pa.Id, pa.AccountNumber, e.DocumentRef, e.InvoiceId, e.DebitAmount, e.CreditAmount,
                    e.PostingDate, e.DueDate, e.Description ?? e.Note))
            .ToArrayAsync(ct);

        var candidates = NoticeGenerator.Generate(glLines, batch.UpToPaymentDate, batch.UpToClaimDate,
            batch.DebtTolerance, batch.DebtToleranceByMonth, batch.MinUnpaidInvoiceCount);

        foreach (var candidate in candidates)
        {
            // P11: the cost is computed here from the batch snapshot, never taken from the client.
            var cost = NoticeCostCalculator.Cost(candidate.Debt, thresholds, carriesCost);
            var raw = $"{companyId}-{candidate.AccountNumber}-P{batch.Date:yyyyMMdd}";
            var notice = new Notice
            {
                CompanyId = companyId,
                PartnerAccountId = candidate.PartnerAccountId,
                UnpaidInvoiceCount = candidate.Lines.Count,
                Debt = candidate.Debt,
                PaymentReference = Kb97.PaymentReference(raw),
                AdditionalCosts = cost,
                Total = BillingCalculator.CalculateNoticeTotal(candidate.Debt, cost)
            };
            foreach (var line in candidate.Lines)
            {
                notice.Lines.Add(new NoticeLine
                {
                    DocumentRef = Trim(line.DocumentRef, 255) ?? string.Empty,
                    Debit = line.Debit,
                    Credit = line.Credit,
                    Sum = line.Sum,
                    Text = Trim(line.Description, 255) ?? Trim(line.DocumentRef, 255) ?? string.Empty,
                    DueDate = line.DueDate ?? batch.Date,
                    InvoiceId = line.InvoiceId
                });
            }
            batch.Notices.Add(notice);
        }
        await db.SaveChangesAsync(ct);
        return new(batchId, false, batch.Notices.OrderBy(x => x.Id).Select(x => x.Id).ToArray());
    }

    public async Task<BillingPage<NoticeResponse>> ListNoticesAsync(int companyId, int page, int pageSize, CancellationToken ct)
    {
        (page, pageSize) = NormalizePage(page, pageSize);
        var query = db.Set<Notice>().AsNoTracking().Where(x => x.CompanyId == companyId);
        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => ToNoticeResponse(x)).ToArrayAsync(ct);
        return new(items, page, pageSize, total);
    }

    public async Task<NoticeResponse> RenderNoticeAsync(int companyId, int noticeId, CancellationToken ct)
    {
        var notice = await GetNoticeAsync(companyId, noticeId, ct);
        var path = await noticeWorkflow.RenderAsync(companyId, noticeId, ct);
        notice.RenderedDocumentPath = path;
        notice.DeliveryStatus = NoticeDeliveryStatus.Rendered;
        await db.SaveChangesAsync(ct);
        return ToNoticeResponse(notice);
    }

    public async Task<NoticeResponse> SendNoticeAsync(int companyId, int noticeId, CancellationToken ct)
    {
        var notice = await GetNoticeAsync(companyId, noticeId, ct);
        if (notice.DeliveryStatus is not (NoticeDeliveryStatus.Rendered or NoticeDeliveryStatus.Failed) || string.IsNullOrWhiteSpace(notice.RenderedDocumentPath))
            throw new DomainRuleException("notice.not-rendered", "Opomena mora biti renderovana pre slanja.");
        notice.DeliveryStatus = NoticeDeliveryStatus.Queued;
        await db.SaveChangesAsync(ct);
        await noticeWorkflow.SendAsync(companyId, noticeId, notice.RenderedDocumentPath, ct);
        notice.DeliveryStatus = NoticeDeliveryStatus.Sent;
        notice.SentAt = timeProvider.GetUtcNow();
        await db.SaveChangesAsync(ct);
        return ToNoticeResponse(notice);
    }

    public async Task<BillingPage<PaymentOrderResponse>> ListPaymentOrdersAsync(int companyId, int page, int pageSize, CancellationToken ct)
    {
        (page, pageSize) = NormalizePage(page, pageSize);
        var query = db.Set<PaymentOrder>().AsNoTracking().Where(x => x.CompanyId == companyId);
        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(x => x.Date).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => ToPaymentOrderResponse(x)).ToArrayAsync(ct);
        return new(items, page, pageSize, total);
    }

    public async Task<PaymentOrderResponse> CreatePaymentOrderAsync(int companyId, CreatePaymentOrderRequest request, CancellationToken ct)
    {
        await shortLists.EnsureTypeAsync(request.PaymentOrderTypeId, "PaymentOrderType", ct);
        if (request.Amount <= 0m || request.ValueDate < request.Date) throw new DomainRuleException("payment-order.invalid", "Iznos ili datum platnog naloga nije ispravan.");
        var entity = new PaymentOrder
        {
            CompanyId = companyId,
            TemplateTitle = Required(request.TemplateTitle, 50, "Naziv"),
            PayerName = Required(request.PayerName, 255, "Platilac"),
            PaymentPurpose = Required(request.PaymentPurpose, 255, "Svrha plaćanja"),
            RecipientName = Required(request.RecipientName, 255, "Primalac"),
            PaymentCode = request.PaymentCode,
            Currency = Required(request.Currency, 3, "Valuta").ToUpperInvariant(),
            Amount = FinanceRounding.Money(request.Amount),
            PayerAccountNumber = Required(request.PayerAccountNumber, 50, "Račun platioca"),
            PayerModelNumber = request.PayerModelNumber,
            PayerPaymentReference = Trim(request.PayerPaymentReference, 50),
            RecipientAccountNumber = Required(request.RecipientAccountNumber, 50, "Račun primaoca"),
            RecipientModelNumber = request.RecipientModelNumber,
            RecipientPaymentReference = Trim(request.RecipientPaymentReference, 50),
            Place = Required(request.Place, 50, "Mesto"),
            Date = request.Date,
            ValueDate = request.ValueDate,
            IsUrgent = request.IsUrgent,
            PaymentOrderTypeId = request.PaymentOrderTypeId,
            CreatedTimestamp = timeProvider.GetUtcNow(),
            IsFavorite = request.IsFavorite
        };
        db.Add(entity);
        await db.SaveChangesAsync(ct);
        return ToPaymentOrderResponse(entity);
    }

    private InvoiceBatchPreviewResponse CreatePreview(int batchId, InvoiceGenerationRequest request)
    {
        if (request.Invoices.Count == 0) throw new DomainRuleException("billing.batch-empty", "Serija mora sadržati najmanje jedan račun.");
        // FIN-18: benefits are applied only by the server-side engine (InvoiceGenerationService).
        if (request.Invoices.Any(x => x.BenefitAmount != 0m))
            throw new DomainRuleException("billing.benefit-server-side", "Benefit se obračunava isključivo pri serverskom generisanju računa.");
        if (request.Invoices.GroupBy(x => x.SequenceNumber, StringComparer.OrdinalIgnoreCase).Any(x => x.Count() > 1))
            throw new DomainRuleException("billing.duplicate-sequence", "Redni brojevi računa moraju biti jedinstveni u seriji.");
        var normalized = request.Invoices.OrderBy(x => x.SortIndex).ThenBy(x => x.SequenceNumber, StringComparer.Ordinal)
            .Select(x => new { Invoice = x, Contracts = x.ContractIds.Order().ToArray(), Lines = x.Lines.OrderBy(y => y.SortIndex).ToArray() }).ToArray();
        var fingerprint = BillingCalculator.CreateDeterministicKey(normalized);
        var previews = request.Invoices.OrderBy(x => x.SortIndex).ThenBy(x => x.SequenceNumber, StringComparer.Ordinal).Select(seed =>
        {
            var lineAmounts = seed.Lines.OrderBy(x => x.SortIndex).Select(line =>
            {
                var amounts = BillingCalculator.CalculateLine(ToDomainLine(line));
                return new InvoicePreviewLineResponse(line.Name, amounts.Quantity, amounts.UnitPrice, amounts.NetAmount, amounts.VatAmount, amounts.TotalAmount);
            }).ToArray();
            var invoice = BillingCalculator.CalculateInvoice(seed.Lines.Select(ToDomainLine), seed.InterestAmount);
            return new InvoicePreviewResponse(seed.PartnerId, seed.SequenceNumber, invoice.NetAmount, 0m, invoice.VatAmount, invoice.InterestAmount, invoice.TotalAmount, lineAmounts);
        }).ToArray();
        return new(batchId, fingerprint, previews.Length, FinanceRounding.Money(previews.Sum(x => x.NetAmount - x.BenefitAmount)),
            FinanceRounding.Money(previews.Sum(x => x.VatAmount)), FinanceRounding.Money(previews.Sum(x => x.InterestAmount)),
            FinanceRounding.Money(previews.Sum(x => x.TotalAmount)), previews);
    }

    private async Task<InvoiceBatch> GetBatchAsync(int companyId, int batchId, bool tracked, CancellationToken ct)
    {
        var query = db.Set<InvoiceBatch>().Where(x => x.Id == batchId && x.CompanyId == companyId);
        if (!tracked) query = query.AsNoTracking();
        return await query.SingleOrDefaultAsync(ct) ?? throw new DomainRuleException("billing.batch-not-found", "Serija računa ne postoji.");
    }

    // Same pattern as JournalPostingService/BankStatementService: a Serializable
    // transaction plus an explicit UPDLOCK/HOLDLOCK read makes the status/fingerprint
    // check-then-generate sequence atomic, so two concurrent calls can't both observe
    // Draft/no-fingerprint and both generate invoices or notices.
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

    private Task LockInvoiceBatchAsync(int batchId, CancellationToken ct) =>
        db.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT 1 FROM [billing].[InvoiceBatch] WITH (UPDLOCK, HOLDLOCK) WHERE [Id] = {batchId}",
            ct);

    private Task LockNoticeBatchAsync(int batchId, CancellationToken ct) =>
        db.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT 1 FROM [billing].[NoticeBatch] WITH (UPDLOCK, HOLDLOCK) WHERE [Id] = {batchId}",
            ct);

    private async Task<Notice> GetNoticeAsync(int companyId, int noticeId, CancellationToken ct) =>
        await db.Set<Notice>().SingleOrDefaultAsync(x => x.Id == noticeId && x.CompanyId == companyId, ct)
        ?? throw new DomainRuleException("notice.not-found", "Opomena ne postoji.");

    private void SetInvoiceShadows(Invoice invoice, InvoiceBatch batch, InvoiceSeedRequest seed)
    {
        var entry = db.Entry(invoice);
        entry.Property("InvoiceBatchId").CurrentValue = batch.Id;
        entry.Property("PlaceOfIssue").CurrentValue = batch.Place;
        entry.Property("ServiceDateFrom").CurrentValue = batch.ServiceDateFrom;
        entry.Property("ServiceDateTo").CurrentValue = batch.ServiceDateTo;
        entry.Property("TransactionDate").CurrentValue = batch.TransactionDate;
        entry.Property("BalanceAsOfDate").CurrentValue = batch.BalanceAsOfDate;
        entry.Property("PreviousBalance").CurrentValue = FinanceRounding.Money(seed.PreviousBalance);
        entry.Property("DeliveryLocation").CurrentValue = Trim(seed.DeliveryLocation, 255);
        entry.Property("PaymentReference").CurrentValue = Trim(seed.PaymentReference, 50);
        entry.Property("SortIndex").CurrentValue = seed.SortIndex;
    }

    private void SetLineShadows(InvoiceLine line, int batchId, int partnerId, int? supplierInvoiceId, string currency)
    {
        var entry = db.Entry(line);
        entry.Property("InvoiceBatchId").CurrentValue = batchId;
        entry.Property("PartnerId").CurrentValue = partnerId;
        entry.Property("SupplierInvoiceId").CurrentValue = supplierInvoiceId;
        entry.Property("Currency").CurrentValue = currency.ToUpperInvariant();
    }

    private static BillingLineInput ToDomainLine(InvoiceLineSeedRequest x) =>
        new(x.Name, x.Quantity, x.UnitPrice, x.VatRate, x.K1, x.K2, x.K3, x.K4, x.K5);

    private static decimal DistinctVatRate(IEnumerable<InvoiceLineSeedRequest> lines)
    {
        var rates = lines.Select(x => x.VatRate).Distinct().Take(2).ToArray();
        return rates.Length == 1 ? rates[0] : 0m;
    }

    private static InvoiceBatchResponse ToBatchResponse(InvoiceBatch x) =>
        new(x.Id, x.PeriodYYMM, x.Caption, x.Month, x.Year, x.IssueDate, x.DueDate, x.ExchangeRateNbs, x.Status.ToString(), x.JournalEntryId, Convert.ToBase64String(x.RowVersion));

    private static InvoiceResponse ToInvoiceResponse(Invoice x, int? batchId, IReadOnlyList<InvoiceLineResponse>? lines) =>
        new(x.Id, x.PartnerId, batchId, x.SequenceNumber, x.IssueDate, x.DueDate,
            x.PartnerName, x.Address, x.PostalCode, x.City, x.Currency, x.Amount, x.VatAmount, x.Total, x.InterestAmount,
            x.InvoiceTotal, x.IsCancelled, Convert.ToBase64String(x.RowVersion), lines);

    private static NoticeResponse ToNoticeResponse(Notice x) =>
        new(x.Id, x.NoticeBatchId, x.PartnerAccountId, x.UnpaidInvoiceCount, x.Debt, x.AdditionalCosts, x.Total,
            x.PaymentReference, x.DeliveryStatus.ToString(), x.RenderedDocumentPath, Convert.ToBase64String(x.RowVersion));

    private static PaymentOrderResponse ToPaymentOrderResponse(PaymentOrder x) =>
        new(x.Id, x.TemplateTitle, x.PayerName, x.RecipientName, x.PaymentPurpose, x.Amount, x.Currency,
            x.Date, x.ValueDate, x.IsUrgent, x.IsFavorite, x.IsArchived, Convert.ToBase64String(x.RowVersion));

    private static (int Page, int PageSize) NormalizePage(int page, int pageSize) => (Math.Max(1, page), Math.Clamp(pageSize, 1, 100));
    private static void EnsurePeriod(int value)
    {
        if (value is < 1001 or > 9912 || value % 100 is < 1 or > 12)
            throw new DomainRuleException("billing.invalid-period", "Period mora biti u formatu YYMM.");
    }
    private static void EnsureRule(HashSet<string> registry, string value, string category)
    {
        if (!registry.Contains(value)) throw new DomainRuleException("billing.rule-not-allowed", $"Pravilo za {category} nije dozvoljeno.");
    }
    private static string Required(string value, int maxLength, string field)
    {
        var trimmed = value.Trim();
        if (trimmed.Length == 0 || trimmed.Length > maxLength) throw new DomainRuleException("validation.invalid-text", $"Polje '{field}' je obavezno i mora imati najviše {maxLength} znakova.");
        return trimmed;
    }
    private static string? Trim(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var trimmed = value.Trim();
        if (trimmed.Length > maxLength) throw new DomainRuleException("validation.text-too-long", $"Tekst mora imati najviše {maxLength} znakova.");
        return trimmed;
    }
    private static byte[] DecodeRowVersion(string value)
    {
        try { return Convert.FromBase64String(value); }
        catch (FormatException) { throw new DomainRuleException("concurrency.invalid-row-version", "RowVersion nije ispravan."); }
    }
}
