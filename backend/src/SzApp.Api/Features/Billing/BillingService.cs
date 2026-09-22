using Microsoft.EntityFrameworkCore;
using SzApp.Contracts.Billing;
using SzApp.Data;
using SzApp.Data.Entities;
using SzApp.Data.Entities.Billing;
using SzApp.Domain;
using SzApp.Domain.Billing;
using Invoice = SzApp.Data.Entities.Invoice;
using InvoiceLine = SzApp.Data.Entities.InvoiceLine;

namespace SzApp.Api.Features.Billing;

public sealed class BillingService(
    SzAppDbContext db,
    IShortListValidator shortLists,
    ILedgerPostingGateway ledger,
    INoticeWorkflowGateway noticeWorkflow,
    TimeProvider timeProvider)
{
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

    public async Task<BillingPage<SupplierInvoiceResponse>> ListSupplierInvoicesAsync(int companyId, int page, int pageSize, CancellationToken ct)
    {
        (page, pageSize) = NormalizePage(page, pageSize);
        var query = db.Set<SupplierInvoice>().AsNoTracking().Where(x => x.CompanyId == companyId);
        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(x => x.PeriodYYMM).ThenBy(x => x.InvoiceNo)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new SupplierInvoiceResponse(
                x.Id, x.InvoiceNo, x.CodeName, x.Caption, x.CalculationTypeId, x.PeriodYYMM,
                x.InvoiceTotalCalculationAmountEur, x.InvoiceTotalCalculationAmountRsd, x.PostedInvoiceAmount,
                x.InvoiceDate, x.TransactionDate, x.UnitTypes.OrderBy(y => y.UnitTypeId).Select(y => y.UnitTypeId).ToArray(),
                Convert.ToBase64String(x.RowVersion)))
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
            entity.InvoiceDate, entity.TransactionDate, entity.UnitTypes.Select(x => x.UnitTypeId).Order().ToArray(), Convert.ToBase64String(entity.RowVersion));
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

    public async Task<InvoiceBatchGenerationResponse> GenerateBatchAsync(int companyId, int batchId, InvoiceGenerationRequest request, CancellationToken ct)
    {
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

        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var invoiceIds = new List<int>();
        foreach (var seed in request.Invoices.OrderBy(x => x.SortIndex).ThenBy(x => x.SequenceNumber, StringComparer.Ordinal))
        {
            var contractPartners = await db.Set<Contract>().AsNoTracking()
                .Where(x => seed.ContractIds.Contains(x.Id) && x.CompanyId == companyId)
                .Select(x => new { x.Id, x.InvoicePartnerId, x.OwnerPartnerId, x.TenantPartnerId }).ToArrayAsync(ct);
            if (contractPartners.Length != seed.ContractIds.Distinct().Count() ||
                contractPartners.Any(x => seed.PartnerId != (x.InvoicePartnerId ?? x.OwnerPartnerId ?? x.TenantPartnerId)))
            {
                throw new DomainRuleException("billing.contract-tenant-mismatch", "Ugovor ili primalac računa ne pripada aktivnoj kompaniji.");
            }
            var amounts = BillingCalculator.CalculateInvoice(seed.Lines.Select(ToDomainLine), seed.BenefitAmount, seed.InterestAmount);
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

            if (seed.BenefitAmount > 0m)
            {
                var benefits = await db.Set<Benefit>()
                    .Where(x => x.CompanyId == companyId && x.PeriodYYMM == batch.PeriodYYMM && x.InvoiceId == null && seed.ContractIds.Contains(x.ContractId))
                    .ToArrayAsync(ct);
                if (FinanceRounding.Money(benefits.Sum(x => x.Amount)) != FinanceRounding.Money(seed.BenefitAmount))
                {
                    throw new DomainRuleException("billing.benefit-mismatch", "Iznos raspoloživih benefita ne odgovara ulazu za račun.");
                }
                foreach (var benefit in benefits) benefit.InvoiceId = invoice.Id;
            }
        }

        batch.GenerationFingerprint = preview.Fingerprint;
        batch.GeneratedAt = timeProvider.GetUtcNow();
        batch.Status = BillingBatchStatus.Generated;
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return new(batchId, false, preview.Fingerprint, invoiceIds);
    }

    public async Task<InvoiceBatchResponse> PostBatchAsync(int companyId, int batchId, string idempotencyKey, CancellationToken ct)
    {
        var batch = await GetBatchAsync(companyId, batchId, true, ct);
        if (batch.Status == BillingBatchStatus.Posted) return ToBatchResponse(batch);
        if (batch.Status != BillingBatchStatus.Generated)
            throw new DomainRuleException("billing.batch-not-generated", "Samo generisana serija može biti knjižena.");

        var invoices = db.Invoices.Where(x => x.CompanyId == companyId && EF.Property<int?>(x, "InvoiceBatchId") == batchId && !x.IsCancelled);
        var count = await invoices.CountAsync(ct);
        if (count == 0) throw new DomainRuleException("billing.batch-empty", "Serija nema aktivne račune.");
        var amount = FinanceRounding.Money(await invoices.SumAsync(x => x.InvoiceTotal, ct));
        var result = await ledger.PostAsync(new LedgerPostingRequest(
            companyId, "InvoiceBatch", batchId, batch.TransactionDate,
            $"INV-{batch.PeriodYYMM}-{batch.Id}", amount, "RSD", idempotencyKey), ct);
        batch.JournalEntryId = result.JournalEntryId;
        batch.Status = BillingBatchStatus.Posted;
        batch.PostedAt = timeProvider.GetUtcNow();
        await db.SaveChangesAsync(ct);
        return ToBatchResponse(batch);
    }

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

    public async Task<InvoiceResponse> CancelInvoiceAsync(int companyId, int invoiceId, CancelInvoiceRequest request, string idempotencyKey, CancellationToken ct)
    {
        var invoice = await db.Invoices.SingleOrDefaultAsync(x => x.CompanyId == companyId && x.Id == invoiceId, ct)
            ?? throw new DomainRuleException("billing.invoice-not-found", "Račun ne postoji.");
        if (invoice.IsCancelled) return ToInvoiceResponse(invoice, db.Entry(invoice).Property<int?>("InvoiceBatchId").CurrentValue, null);
        db.Entry(invoice).Property(x => x.RowVersion).OriginalValue = DecodeRowVersion(request.RowVersion);

        var batchId = db.Entry(invoice).Property<int?>("InvoiceBatchId").CurrentValue;
        if (batchId is not null)
        {
            var batch = await db.Set<InvoiceBatch>().SingleAsync(x => x.CompanyId == companyId && x.Id == batchId, ct);
            if (batch.Status == BillingBatchStatus.Posted && batch.JournalEntryId is not null)
            {
                await ledger.ReverseAsync(new LedgerReversalRequest(companyId, "Invoice", invoiceId, batch.JournalEntryId.Value,
                    DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime), invoice.InvoiceTotal, invoice.Currency,
                    Required(request.Reason, 500, "Razlog storna"), idempotencyKey), ct);
            }
        }
        invoice.IsCancelled = true;
        invoice.CancelledAt = timeProvider.GetUtcNow();
        db.Entry(invoice).Property("CancelReason").CurrentValue = Required(request.Reason, 500, "Razlog storna");
        await db.SaveChangesAsync(ct);
        return ToInvoiceResponse(invoice, batchId, null);
    }

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
        if (request.To < request.From) throw new DomainRuleException("interest.invalid-period", "Period kamate nije ispravan.");
        var rates = await db.Set<InterestRate>().AsNoTracking().Where(x => x.TimeCode == "G" && x.Date <= request.To).OrderBy(x => x.Date).ToArrayAsync(ct);
        var effective = rates.LastOrDefault(x => x.Date <= request.From)
            ?? throw new DomainRuleException("interest.rate-not-found", "Nema stope kamate za početak perioda.");
        var changes = rates.Where(x => x.Date > request.From).ToArray();
        var periods = new List<InterestPeriod>();
        var from = request.From;
        var rate = effective.Rate;
        foreach (var change in changes)
        {
            periods.Add(new(from, change.Date.AddDays(-1), rate));
            from = change.Date;
            rate = change.Rate;
        }
        periods.Add(new(from, request.To, rate));
        var calculated = InterestCalculator.Calculate(request.Principal, periods);
        var lines = calculated.Select(x => new InterestCalculationLineResponse(x.From, x.To, x.Days, x.AnnualRate, x.Coefficient, x.Interest)).ToArray();
        return new(FinanceRounding.Money(request.Principal), FinanceRounding.Money(lines.Sum(x => x.Interest)), lines);
    }

    public async Task<IReadOnlyList<InterestStatementResponse>> ListInterestStatementsAsync(int companyId, int invoiceBatchId, CancellationToken ct) =>
        await db.Set<InterestStatement>().AsNoTracking()
            .Where(x => x.CompanyId == companyId && x.InvoiceBatchId == invoiceBatchId)
            .OrderBy(x => x.PartnerAccountId).ThenBy(x => x.Date)
            .Select(x => new InterestStatementResponse(x.Id, x.Account, x.Date, x.Amount, x.Balance, x.Days,
                x.Rate, x.Coefficient, x.Interest, x.PartnerAccountId, x.InvoiceBatchId))
            .ToArrayAsync(ct);

    public async Task<IReadOnlyList<InterestStatementResponse>> CreateInterestStatementsAsync(int companyId, CreateInterestStatementRequest request, CancellationToken ct)
    {
        if (!await db.Set<InvoiceBatch>().AnyAsync(x => x.Id == request.InvoiceBatchId && x.CompanyId == companyId, ct))
            throw new DomainRuleException("billing.batch-not-found", "Serija računa ne postoji.");
        var calculation = await CalculateInterestAsync(new CalculateInterestRequest(request.Principal, request.From, request.To), ct);
        var entities = calculation.Lines.Select(line => new InterestStatement
        {
            CompanyId = companyId,
            Account = Required(request.Account, 50, "Konto"),
            Date = line.From,
            Amount = FinanceRounding.Money(request.Principal),
            Balance = FinanceRounding.Money(request.Balance),
            Days = line.Days,
            Rate = line.Rate,
            Coefficient = line.Coefficient,
            Interest = line.Interest,
            PartnerAccountId = request.PartnerAccountId,
            SubAccountId = Trim(request.SubAccountId, 10),
            InvoiceBatchId = request.InvoiceBatchId
        }).ToArray();
        db.AddRange(entities);
        await db.SaveChangesAsync(ct);
        return entities.Select(x => new InterestStatementResponse(x.Id, x.Account, x.Date, x.Amount, x.Balance,
            x.Days, x.Rate, x.Coefficient, x.Interest, x.PartnerAccountId, x.InvoiceBatchId)).ToArray();
    }

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
        await shortLists.EnsureTypeAsync(request.NoticeTypeId, "NoticeType", ct);
        if (!await db.Set<NoticeTemplate>().AnyAsync(x => x.Id == request.NoticeTemplateId && x.CompanyId == companyId && x.IsActive, ct))
            throw new DomainRuleException("notice.template-not-found", "Aktivan šablon opomene ne postoji.");
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
        db.Add(entity);
        await db.SaveChangesAsync(ct);
        return new(entity.Id, entity.Title, entity.Date, entity.NoticeTemplateId, entity.NoticeTypeId, Convert.ToBase64String(entity.RowVersion));
    }

    public async Task<NoticeGenerationResponse> GenerateNoticesAsync(int companyId, int batchId, GenerateNoticesRequest request, CancellationToken ct)
    {
        var batch = await db.Set<NoticeBatch>().Include(x => x.Notices).SingleOrDefaultAsync(x => x.Id == batchId && x.CompanyId == companyId, ct)
            ?? throw new DomainRuleException("notice.batch-not-found", "Serija opomena ne postoji.");
        var normalized = request.Notices.OrderBy(x => x.PartnerAccountId).ToArray();
        var fingerprint = BillingCalculator.CreateDeterministicKey(normalized);
        if (batch.GenerationFingerprint is not null)
        {
            if (batch.GenerationFingerprint != fingerprint) throw new DomainRuleException("notice.batch-input-changed", "Opomene su već generisane sa drugačijim ulazom.");
            return new(batchId, true, batch.Notices.OrderBy(x => x.Id).Select(x => x.Id).ToArray());
        }

        foreach (var seed in normalized.Where(x => x.UnpaidInvoiceCount >= batch.MinUnpaidInvoiceCount && x.Debt > batch.DebtTolerance))
        {
            if (!await db.Set<PartnerAccount>().AnyAsync(x => x.Id == seed.PartnerAccountId && x.CompanyId == companyId, ct))
                throw new DomainRuleException("notice.partner-account-not-found", "Konto partnera ne pripada aktivnoj kompaniji.");
            var notice = new Notice
            {
                CompanyId = companyId,
                PartnerAccountId = seed.PartnerAccountId,
                UnpaidInvoiceCount = seed.UnpaidInvoiceCount,
                Debt = FinanceRounding.Money(seed.Debt),
                InvoiceText = Trim(seed.InvoiceText, 255),
                PaymentReference = Required(seed.PaymentReference, 50, "Poziv na broj"),
                AdditionalCosts = FinanceRounding.Money(seed.AdditionalCosts),
                Total = BillingCalculator.CalculateNoticeTotal(seed.Debt, seed.AdditionalCosts)
            };
            foreach (var line in seed.Lines)
            {
                notice.Lines.Add(new NoticeLine
                {
                    DocumentRef = Required(line.DocumentRef, 255, "Dokument"),
                    Debit = FinanceRounding.Money(line.Debit),
                    Credit = FinanceRounding.Money(line.Credit),
                    Sum = FinanceRounding.Money(line.Debit - line.Credit),
                    Text = Required(line.Text, 255, "Opis"),
                    DueDate = line.DueDate,
                    InvoiceId = line.InvoiceId,
                    InvoiceDate = line.InvoiceDate,
                    UnitAddress = Trim(line.UnitAddress, 255)
                });
            }
            batch.Notices.Add(notice);
        }
        batch.GenerationFingerprint = fingerprint;
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
            var invoice = BillingCalculator.CalculateInvoice(seed.Lines.Select(ToDomainLine), seed.BenefitAmount, seed.InterestAmount);
            return new InvoicePreviewResponse(seed.PartnerId, seed.SequenceNumber, invoice.NetAmount, invoice.BenefitAmount, invoice.VatAmount, invoice.InterestAmount, invoice.TotalAmount, lineAmounts);
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
