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
    bool IsExtraordinary)
{
    /// <summary>Amount actually due (audit 9.1 IPS QR rule): max(PreviousDebt + Total, 0) regular, Total for extraordinary.</summary>
    public decimal AmountDue => IsExtraordinary ? Total : Math.Max(PreviousDebt + Total, 0m);
}

public sealed record InvoiceDocumentLine(
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal VatRate,
    decimal Total);
