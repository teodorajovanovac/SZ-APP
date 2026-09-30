using SzApp.Domain.LedgerBanking;

namespace SzApp.Domain.Billing;

/// <summary>Legacy <c>IndexZaRedovneVanderdneRacune</c>: 0 all, 1 regular (no marker), 2 extraordinary.</summary>
public enum SupplierCopyScope
{
    All = 0,
    Regular = 1,
    Extraordinary = 2
}

public sealed record SupplierCopyCandidate(int Id, int InvoiceNo, int DocumentTypeCode, string? ExtraordinaryMarker);

/// <summary>
/// GAP-33, legacy <c>modRacun.DuplirajRacuneDobavljaca</c>: every supplier invoice of the source
/// month except document type 2 (izvršeni trošak) is copied to the target month with its unit types.
/// New: an invoice whose number already exists in the target month is skipped (legacy had no guard
/// and duplicated; InvoiceNo is unique per company and month).
/// </summary>
public static class SupplierInvoiceCopyRules
{
    public static IReadOnlyList<SupplierCopyCandidate> Select(
        IEnumerable<SupplierCopyCandidate> source,
        IReadOnlySet<int> invoiceNosInTarget,
        SupplierCopyScope scope) =>
        source.Where(x => x.DocumentTypeCode != SupplierDocumentTypes.Actual)
            .Where(x => scope switch
            {
                SupplierCopyScope.Regular => string.IsNullOrWhiteSpace(x.ExtraordinaryMarker),
                SupplierCopyScope.Extraordinary => !string.IsNullOrWhiteSpace(x.ExtraordinaryMarker),
                _ => true
            })
            .Where(x => !invoiceNosInTarget.Contains(x.InvoiceNo))
            .OrderBy(x => x.InvoiceNo)
            .ToArray();

    /// <summary>Legacy Virman_AddNew: IznosRacunaKN (posted/billed amount) if &gt; 0, else IznosRacunaRSD.</summary>
    public static decimal PaymentOrderAmount(decimal postedInvoiceAmount, decimal invoiceAmountRsd) =>
        FinanceRounding.Money(postedInvoiceAmount > 0m ? postedInvoiceAmount : invoiceAmountRsd);

    /// <summary>Payment code for supplier payments (legacy constant "221").</summary>
    public const int SupplierPaymentCode = 221;
}
