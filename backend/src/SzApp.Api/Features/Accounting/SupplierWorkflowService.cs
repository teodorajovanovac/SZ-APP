using Microsoft.EntityFrameworkCore;
using SzApp.Contracts.LedgerBanking;
using SzApp.Data;
using SzApp.Data.Entities;
using SzApp.Data.Entities.Billing;
using SzApp.Domain;
using SzApp.Domain.Billing;
using SzApp.Api.Features.LedgerBanking;

namespace SzApp.Api.Features.Accounting;

/// <summary>
/// Monthly supplier work: GAP-33 copy of supplier invoices to the next month
/// (<c>DuplirajRacuneDobavljaca</c>) and GAP-34 payment orders (<c>modVirman.Virman_AddNew</c>).
/// Virmans are NOT posted (legacy doesn't): 4350 is closed when the bank statement pays it
/// (the outgoing statement line carries SupplierInvoiceId + ClosesDocumentType 4, see 9.3 step 6).
/// </summary>
public sealed class SupplierWorkflowService(SzAppDbContext db, TimeProvider timeProvider, IBusinessClock clock)
{
    private const string PaymentOrderTypeTable = "PaymentOrderType";
    private const string DefaultPlace = "Beograd"; // legacy hard-codes "Beograd"; used when the company has no address

    public async Task<CopySupplierInvoicesResponse> CopyToPeriodAsync(int companyId, CopySupplierInvoicesRequest request, CancellationToken ct)
    {
        EnsurePeriod(request.FromPeriodYYMM);
        EnsurePeriod(request.ToPeriodYYMM);
        if (request.FromPeriodYYMM == request.ToPeriodYYMM)
            throw new DomainRuleException("supplier-copy.same-period", "Izvorni i ciljni mesec moraju biti različiti.");
        if (!Enum.IsDefined(typeof(SupplierCopyScope), request.Scope))
            throw new DomainRuleException("supplier-copy.scope", "Obim kopiranja mora biti 0 (svi), 1 (redovni) ili 2 (vanredni).");

        var sources = await db.Set<SupplierInvoice>().Include(x => x.UnitTypes)
            .Where(x => x.CompanyId == companyId && x.PeriodYYMM == request.FromPeriodYYMM)
            .ToArrayAsync(ct);
        var typeCodes = await db.ShortLists.AsNoTracking()
            .Where(x => x.TableName == "SupplierDocumentType")
            .ToDictionaryAsync(x => x.Id, x => x.IndexValue, ct);
        var inTarget = (await db.Set<SupplierInvoice>().AsNoTracking()
                .Where(x => x.CompanyId == companyId && x.PeriodYYMM == request.ToPeriodYYMM)
                .Select(x => x.InvoiceNo).ToArrayAsync(ct))
            .ToHashSet();

        var candidates = sources.Select(x => new SupplierCopyCandidate(
            x.Id, x.InvoiceNo, typeCodes.GetValueOrDefault(x.DocumentTypeId), x.ExtraordinaryInvoiceMarker)).ToArray();
        var scope = (SupplierCopyScope)request.Scope;
        var eligible = SupplierInvoiceCopyRules.Select(candidates, new HashSet<int>(), scope).Count;
        var selected = SupplierInvoiceCopyRules.Select(candidates, inTarget, scope).Select(x => x.Id).ToHashSet();

        // Legacy copies every column except the posting results (IznosRacunaKN, nalog); dates are kept as-is.
        var copies = sources.Where(x => selected.Contains(x.Id)).Select(x =>
        {
            var copy = new SupplierInvoice
            {
                CompanyId = companyId,
                InvoiceNo = x.InvoiceNo,
                CodeName = x.CodeName,
                Caption = x.Caption,
                SupplierPartnerAccountId = x.SupplierPartnerAccountId,
                CalculationTypeId = x.CalculationTypeId,
                PeriodYYMM = request.ToPeriodYYMM,
                InvoiceTotalCalculationAmountEur = x.InvoiceTotalCalculationAmountEur,
                InvoiceTotalCalculationAmountRsd = x.InvoiceTotalCalculationAmountRsd,
                CalculationAmountByCoefficientEur = x.CalculationAmountByCoefficientEur,
                CalculationAmountByCoefficientRsd = x.CalculationAmountByCoefficientRsd,
                PaymentPriority = x.PaymentPriority,
                SubAccountId = x.SubAccountId,
                DocumentTypeId = x.DocumentTypeId,
                ExtraordinaryInvoiceMarker = x.ExtraordinaryInvoiceMarker,
                InvoiceNameFunction = x.InvoiceNameFunction,
                InvoiceDate = x.InvoiceDate,
                TransactionDate = x.TransactionDate,
                PaymentDate = x.PaymentDate,
                InvoiceDescription = x.InvoiceDescription,
                PaymentReference = x.PaymentReference,
                ClosesAccount = x.ClosesAccount
            };
            foreach (var unitType in x.UnitTypes)
                copy.UnitTypes.Add(new SupplierInvoiceUnitType { UnitTypeId = unitType.UnitTypeId });
            return copy;
        }).ToArray();

        db.AddRange(copies);
        await db.SaveChangesAsync(ct);
        return new CopySupplierInvoicesResponse(copies.Length, eligible - copies.Length, copies.Select(x => x.Id).ToArray());
    }

    public async Task<GeneratePaymentOrdersResponse> GeneratePaymentOrdersAsync(int companyId, GeneratePaymentOrdersRequest request, CancellationToken ct)
    {
        if (request.SupplierInvoiceIds is not { Count: > 0 } && request.PeriodYYMM is null)
            throw new DomainRuleException("payment-order.scope", "Izaberite ulazne račune ili mesec.");
        if (request.PeriodYYMM is { } p) EnsurePeriod(p);

        var query = db.Set<SupplierInvoice>().AsNoTracking().Where(x => x.CompanyId == companyId);
        if (request.SupplierInvoiceIds is { Count: > 0 } ids) query = query.Where(x => ids.Contains(x.Id));
        if (request.PeriodYYMM is { } period) query = query.Where(x => x.PeriodYYMM == period);
        var invoices = await query.OrderBy(x => x.PeriodYYMM).ThenBy(x => x.InvoiceNo)
            .Select(x => new
            {
                x.Id, x.Caption, x.PostedInvoiceAmount, x.InvoiceTotalCalculationAmountRsd, x.PaymentReference,
                SupplierPartnerId = db.Set<PartnerAccount>().Where(pa => pa.Id == x.SupplierPartnerAccountId).Select(pa => pa.PartnerId).First()
            })
            .ToArrayAsync(ct);

        var invoiceIds = invoices.Select(x => x.Id).ToArray();
        var alreadyOrdered = (await db.Set<PaymentOrder>().AsNoTracking()
                .Where(x => x.CompanyId == companyId && x.SupplierInvoiceId != null && invoiceIds.Contains(x.SupplierInvoiceId.Value))
                .Select(x => x.SupplierInvoiceId!.Value).ToArrayAsync(ct))
            .ToHashSet();

        var company = await db.Companies.AsNoTracking().Where(x => x.Id == companyId)
            .Select(x => new { x.Id, x.PrintName, x.PartnerId }).SingleAsync(ct);
        var payerAccount = await db.Set<BankAccount>().AsNoTracking()
            .Where(x => x.IsActive && x.AccountNumber != null && (x.CompanyId == companyId || x.PartnerId == company.PartnerId))
            .OrderBy(x => x.SortIndex ?? int.MaxValue).ThenBy(x => x.Id)
            .Select(x => x.AccountNumber).FirstOrDefaultAsync(ct)
            ?? throw new DomainRuleException("payment-order.payer-account-missing", "Stambena zajednica nema aktivan tekući račun.");

        var partnerIds = invoices.Select(x => x.SupplierPartnerId).Append(company.PartnerId).Distinct().ToArray();
        var names = await db.Partners.AsNoTracking().Where(x => partnerIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var addresses = (await db.Set<PartnerAddress>().AsNoTracking()
                .Where(x => partnerIds.Contains(x.PartnerId))
                .OrderByDescending(x => x.IsDefault).ThenBy(x => x.Id)
                .Select(x => new { x.PartnerId, x.Address.StreetAddress, x.Address.PostalCode, x.Address.City })
                .ToArrayAsync(ct))
            .GroupBy(x => x.PartnerId).ToDictionary(g => g.Key, g => g.First());
        var recipientAccounts = (await db.Set<BankAccount>().AsNoTracking()
                .Where(x => x.IsActive && x.AccountNumber != null && x.PartnerId != null && partnerIds.Contains(x.PartnerId.Value))
                .OrderBy(x => x.SortIndex ?? int.MaxValue).ThenBy(x => x.Id)
                .Select(x => new { PartnerId = x.PartnerId!.Value, x.AccountNumber })
                .ToArrayAsync(ct))
            .GroupBy(x => x.PartnerId).ToDictionary(g => g.Key, g => g.First().AccountNumber!);
        var typeId = await db.ShortLists.AsNoTracking()
            .Where(x => x.TableName == PaymentOrderTypeTable)
            .OrderBy(x => x.IndexValue == 1 ? 0 : 1).ThenBy(x => x.Id)
            .Select(x => (int?)x.Id).FirstOrDefaultAsync(ct)
            ?? throw new DomainRuleException("payment-order.type-missing", "Šifarnik PaymentOrderType nema nijednu stavku.");

        string Block(int partnerId, string fallbackName)
        {
            var name = names.GetValueOrDefault(partnerId) ?? fallbackName;
            if (!addresses.TryGetValue(partnerId, out var a)) return name;
            return string.Join("\n", new[] { name, a.StreetAddress, $"{a.PostalCode} {a.City}".Trim() }.Where(s => !string.IsNullOrWhiteSpace(s)));
        }

        var date = request.Date ?? clock.Today;
        var place = addresses.TryGetValue(company.PartnerId, out var own) && !string.IsNullOrWhiteSpace(own.City) ? own.City : DefaultPlace;
        var skipped = new List<PaymentOrderSkipResponse>();
        var created = new List<PaymentOrder>();
        foreach (var invoice in invoices)
        {
            var amount = SupplierInvoiceCopyRules.PaymentOrderAmount(invoice.PostedInvoiceAmount, invoice.InvoiceTotalCalculationAmountRsd);
            string? reason = alreadyOrdered.Contains(invoice.Id) ? "Virman za ovaj račun već postoji."
                : amount <= 0m ? "Račun nema iznos."
                : !recipientAccounts.ContainsKey(invoice.SupplierPartnerId) ? "Dobavljač nema aktivan tekući račun."
                : null;
            if (reason is not null)
            {
                skipped.Add(new PaymentOrderSkipResponse(invoice.Id, invoice.Caption, reason));
                continue;
            }

            created.Add(new PaymentOrder
            {
                CompanyId = companyId,
                SupplierInvoiceId = invoice.Id,
                TemplateTitle = Clip(invoice.Caption, 50),
                PayerName = Clip(Block(company.PartnerId, company.PrintName), 255),
                RecipientName = Clip(Block(invoice.SupplierPartnerId, string.Empty), 255),
                PaymentPurpose = Clip(invoice.Caption, 255),
                PaymentCode = SupplierInvoiceCopyRules.SupplierPaymentCode,
                Currency = "RSD",
                Amount = amount,
                PayerAccountNumber = Clip(payerAccount, 50),
                RecipientAccountNumber = Clip(recipientAccounts[invoice.SupplierPartnerId], 50),
                RecipientPaymentReference = string.IsNullOrWhiteSpace(invoice.PaymentReference) ? null : Clip(invoice.PaymentReference.Trim(), 50),
                Place = Clip(place, 50),
                Date = date,
                ValueDate = date,
                PaymentOrderTypeId = typeId,
                CreatedTimestamp = timeProvider.GetUtcNow()
            });
        }

        db.AddRange(created);
        await db.SaveChangesAsync(ct);
        return new GeneratePaymentOrdersResponse(created.Count, skipped);
    }

    public async Task<IReadOnlyList<SupplierPaymentOrderResponse>> ListPaymentOrdersAsync(int companyId, bool includeArchived, IReadOnlyCollection<int>? ids, CancellationToken ct)
    {
        var query = db.Set<PaymentOrder>().AsNoTracking().Where(x => x.CompanyId == companyId);
        if (ids is { Count: > 0 }) query = query.Where(x => ids.Contains(x.Id));
        else if (!includeArchived) query = query.Where(x => !x.IsArchived);
        // ponytail: unpaged, capped at 500 -- open virmans are a handful per month; page it if archives are browsed.
        return await query.OrderByDescending(x => x.Date).ThenByDescending(x => x.Id).Take(500)
            .Select(x => new SupplierPaymentOrderResponse(
                x.Id, x.SupplierInvoiceId, x.PayerName, x.PayerAccountNumber, x.RecipientName, x.RecipientAccountNumber,
                x.PaymentCode, x.Currency, x.Amount, x.RecipientModelNumber, x.RecipientPaymentReference, x.PaymentPurpose,
                x.Place, x.Date, x.ValueDate, x.IsArchived, Convert.ToBase64String(x.RowVersion)))
            .ToArrayAsync(ct);
    }

    /// <summary>Legacy VirmanArhive / DeleteVirman.</summary>
    public async Task<bool> ArchiveOrDeleteAsync(int companyId, int id, bool delete, CancellationToken ct)
    {
        var entity = await db.Set<PaymentOrder>().SingleOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId, ct);
        if (entity is null) return false;
        if (delete) db.Remove(entity);
        else entity.IsArchived = true;
        await db.SaveChangesAsync(ct);
        return true;
    }

    private static void EnsurePeriod(int value)
    {
        if (value is < 1001 or > 9912 || value % 100 is < 1 or > 12)
            throw new DomainRuleException("billing.invalid-period", "Period mora biti u formatu YYMM.");
    }

    private static string Clip(string value, int max) => value.Length <= max ? value : value[..max];
}
