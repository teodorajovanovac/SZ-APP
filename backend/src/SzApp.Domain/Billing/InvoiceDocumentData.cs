namespace SzApp.Domain.Billing;

/// <summary>
/// Generic, entity-agnostic shape for rendering one invoice as PDF/QR. Callers map whatever
/// Invoice/InvoiceLine/InvoiceBatch entities exist at build time onto this record, so the
/// renderer keeps working once the calculation engine (FIN-03/04/16/17/18) lands or changes shape.
/// </summary>
public sealed record InvoiceDocumentData(
    string InvoiceNumber,
    string PaymentReference,
    DateOnly IssueDate,
    DateOnly DueDate,
    string IssuerName,
    string IssuerCity,
    string IssuerAccountNumber,
    string CustomerName,
    string CustomerAddress,
    IReadOnlyList<InvoiceDocumentLine> Lines,
    decimal PreviousDebt,
    decimal Interest,
    decimal SubTotal,
    decimal VatAmount,
    decimal Total,
    bool IsExtraordinary,
    IReadOnlyList<InvoiceDocumentBenefitLine>? BenefitLines = null,
    IReadOnlyList<InvoiceDocumentGroupMember>? GroupMembers = null)
{
    /// <summary>Amount actually due (audit 9.1 IPS QR rule): max(PreviousDebt + Total, 0) regular, Total for extraordinary.</summary>
    public decimal AmountDue => IsExtraordinary ? Total : Math.Max(PreviousDebt + Total, 0m);
}

public sealed record InvoiceDocumentLine(
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal VatRate,
    decimal Total,
    decimal PriceEur = 0m,
    decimal ExchangeRate = 0m);

/// <summary>Legacy RACUN_012 BENEFIT: a manager line zeroed by the benefit, with its archived original amount (BenefitArchive).</summary>
public sealed record InvoiceDocumentBenefitLine(string Description, string Note, decimal OriginalAmount);

/// <summary>Legacy Racun_007: one member invoice summed into a group (master) invoice.</summary>
public sealed record InvoiceDocumentGroupMember(string InvoiceNumber, string CustomerName, string Address, decimal Total);
