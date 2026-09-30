using Microsoft.EntityFrameworkCore;
using SzApp.Data;
using SzApp.Data.Entities;
using SzApp.Data.Entities.Billing;
using SzApp.Domain.Billing;

namespace SzApp.Api.Features.Billing;

/// <summary>
/// Maps DB entities onto the generic <see cref="InvoiceDocumentData"/> the PDF/QR renderer reads.
/// Kept separate from BillingService.cs (owned concurrently by the calculation-engine and
/// notices/reports agents) -- this only reads, never touches calculation methods.
/// </summary>
public sealed class InvoiceDocumentMapper(SzAppDbContext db)
{
    public async Task<InvoiceDocumentData?> BuildAsync(int companyId, int invoiceId, CancellationToken ct)
    {
        var invoice = await db.Invoices.AsNoTracking().Include(x => x.Lines)
            .SingleOrDefaultAsync(x => x.CompanyId == companyId && x.Id == invoiceId, ct);
        if (invoice is null) return null;

        var entry = db.Entry(invoice);
        var batchId = entry.Property<int?>("InvoiceBatchId").CurrentValue;
        var paymentReference = entry.Property<string?>("PaymentReference").CurrentValue ?? string.Empty;
        var previousDebt = entry.Property<decimal>("PreviousBalance").CurrentValue;

        var isExtraordinary = false;
        if (batchId is { } id)
        {
            var marker = await db.Set<InvoiceBatch>().Where(x => x.Id == id)
                .Select(x => x.ExtraordinaryInvoiceMarker).SingleOrDefaultAsync(ct);
            isExtraordinary = !string.IsNullOrWhiteSpace(marker);
        }

        var company = await db.Set<Company>().AsNoTracking().SingleAsync(x => x.Id == companyId, ct);
        var issuerAccount = await db.Set<BankAccount>().AsNoTracking()
            .Where(x => x.CompanyId == companyId && x.IsActive)
            .OrderBy(x => x.SortIndex ?? int.MaxValue)
            .Select(x => x.AccountNumber)
            .FirstOrDefaultAsync(ct) ?? string.Empty;
        // ponytail: issuer city sourced from the invoice batch's Place (set at generation time),
        // not a Partner/Address join -- InvoiceBatch already carries it, no extra lookup needed.
        var issuerCity = batchId is { } bId
            ? await db.Set<InvoiceBatch>().Where(x => x.Id == bId).Select(x => x.Place).SingleOrDefaultAsync(ct) ?? string.Empty
            : string.Empty;

        var lines = invoice.Lines.OrderBy(x => x.SortIndex)
            .Select(x => new InvoiceDocumentLine(x.Name, x.Quantity, x.PricePcs, x.VatRate, x.TotalAmount, x.PriceEur, x.ExchangeRateNbs))
            .ToArray();

        // RACUN_012 BENEFIT: the zeroed manager lines with their archived original amounts.
        var lineNames = invoice.Lines.ToDictionary(x => x.Id, x => x.Name);
        var benefitLines = (await db.Set<BenefitArchive>().AsNoTracking()
                .Where(x => x.CompanyId == companyId && x.InvoiceId == invoiceId).OrderBy(x => x.Id)
                .Select(x => new { x.InvoiceLineId, x.Note, x.OriginalAmount }).ToArrayAsync(ct))
            .Select(x => new InvoiceDocumentBenefitLine(lineNames.GetValueOrDefault(x.InvoiceLineId, string.Empty), x.Note, x.OriginalAmount))
            .ToArray();
        // Racun_007: members point at their master through InvoiceParentId (legacy SPC).
        var groupMembers = await db.Invoices.AsNoTracking()
            .Where(x => x.CompanyId == companyId && EF.Property<int?>(x, "InvoiceParentId") == invoiceId)
            .OrderBy(x => x.SequenceNumber)
            .Select(x => new InvoiceDocumentGroupMember(x.SequenceNumber, x.PartnerName, x.Address, x.InvoiceTotal))
            .ToArrayAsync(ct);

        return new InvoiceDocumentData(
            InvoiceNumber: invoice.SequenceNumber,
            PaymentReference: paymentReference,
            IssueDate: invoice.IssueDate,
            DueDate: invoice.DueDate,
            IssuerName: company.PrintName,
            IssuerCity: issuerCity,
            IssuerAccountNumber: issuerAccount,
            CustomerName: invoice.PartnerName,
            CustomerAddress: $"{invoice.Address}, {invoice.City}".Trim(' ', ','),
            Lines: lines,
            PreviousDebt: previousDebt,
            Interest: invoice.InterestAmount,
            SubTotal: invoice.Amount,
            VatAmount: invoice.VatAmount,
            // InvoiceTotal = Total + InterestAmount (audit 9.1 R7 "Ukupno = UkupnoRacun + KamataIznos") --
            // this is the figure the IPS QR amount rule (max(PreviousDebt + Total, 0)) is defined against.
            Total: invoice.InvoiceTotal,
            IsExtraordinary: isExtraordinary,
            BenefitLines: benefitLines,
            GroupMembers: groupMembers);
    }
}
