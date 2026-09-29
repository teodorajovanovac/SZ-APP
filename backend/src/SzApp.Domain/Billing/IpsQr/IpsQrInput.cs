namespace SzApp.Domain.Billing.IpsQr;

/// <summary>
/// Inputs for the NBS IPS QR payload (audit 9.1 "IPS QR"). Kept generic (plain strings/decimal,
/// no entity references) so it works against any invoice-shaped source once the calculation
/// engine's Invoice/InvoiceLine/InvoiceBatch entities are wired up on the caller side.
/// </summary>
/// <param name="IssuerAccountNumber18">Issuer's transaction account, 18 digits (dashes/spaces allowed, stripped internally).</param>
/// <param name="IssuerName">Issuer (SZ) display name.</param>
/// <param name="IssuerCity">Issuer city/place.</param>
/// <param name="CustomerName">Customer (kupac) display name.</param>
/// <param name="CustomerAddress">Customer address.</param>
/// <param name="PreviousDebt">Snapshot of the customer's ledger balance at generation time (R2 `PrethodniDug`). Ignored when <paramref name="IsExtraordinary"/> is true.</param>
/// <param name="InvoiceTotal">Invoice total before previous debt (R1/R7 `Ukupno`, includes interest but not previous debt).</param>
/// <param name="IsExtraordinary">True when the invoice batch carries an extraordinary marker (vanredni račun) — amount becomes just <paramref name="InvoiceTotal"/>.</param>
/// <param name="PaymentPurpose">Doznaka / payment purpose text.</param>
/// <param name="SeriesText">Free text describing the series (e.g. caption), truncated to 35 chars together with the purpose.</param>
/// <param name="PaymentReferenceDigits">Poziv na broj (RBR + check digits), dashes stripped internally.</param>
public sealed record IpsQrInput(
    string IssuerAccountNumber18,
    string IssuerName,
    string IssuerCity,
    string CustomerName,
    string CustomerAddress,
    decimal PreviousDebt,
    decimal InvoiceTotal,
    bool IsExtraordinary,
    string PaymentPurpose,
    string SeriesText,
    string PaymentReferenceDigits)
{
    /// <summary>Iznos = max(PrethodniDug + Ukupno, 0) za redovne račune; Ukupno za vanredne (audit 9.1).</summary>
    public decimal Amount => IsExtraordinary ? InvoiceTotal : Math.Max(PreviousDebt + InvoiceTotal, 0m);
}
