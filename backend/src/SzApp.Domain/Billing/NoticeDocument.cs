using System.Globalization;
using SzApp.Domain.Billing.IpsQr;

namespace SzApp.Domain.Billing;

/// <summary>
/// Entity-agnostic shape of one opomena for the PDF/QR/email (9.5). Template texts still contain
/// the legacy placeholders; <see cref="NoticeTemplateText.Apply"/> fills them.
/// </summary>
public sealed record NoticeDocumentData(
    string NoticeNumber,
    DateOnly Date,
    DateOnly PaymentCutoff,
    string IssuerName,
    string IssuerCity,
    string? IssuerTaxNumber,
    string? IssuerRegistrationNumber,
    string IssuerAccountNumber,
    DateOnly? DecisionDate,
    string CustomerName,
    string CustomerAddress,
    int AccountNumber,
    decimal Debt,
    decimal AdditionalCosts,
    string PaymentReference,
    string? Subject,
    string Body,
    string? Closing,
    string? Signature,
    string? SlipCaption,
    IReadOnlyList<NoticeDocumentLine> Lines)
{
    /// <summary>Amount on the QR/slip: debt plus the notice cost (P11).</summary>
    public decimal Total => Debt + AdditionalCosts;

    /// <summary>
    /// IPS QR for the notice: amount = Dug + trošak (no previous-debt logic -- IsExtraordinary makes the
    /// builder use the total as-is), RO = the notice payment reference (KB97 of "{SZ}-{IDK}-P{yyyymmdd}").
    /// </summary>
    public IpsQrInput ToIpsQrInput() => new(
        IssuerAccountNumber, IssuerName, IssuerCity, CustomerName, CustomerAddress,
        PreviousDebt: 0m, InvoiceTotal: Total, IsExtraordinary: true,
        PaymentPurpose: "Uplata po opomeni", SeriesText: string.IsNullOrWhiteSpace(SlipCaption) ? NoticeNumber : SlipCaption,
        PaymentReferenceDigits: PaymentReference);
}

public sealed record NoticeDocumentLine(string DocumentRef, string Text, DateOnly DueDate, decimal Debit, decimal Credit, decimal Sum);

/// <summary>Legacy OpomeneSabloni placeholders: [NazivSS] [SZPIB] [SZMB] [DatumUgovora] [DatumPI] [Dug] [TR] [datum].</summary>
public static class NoticeTemplateText
{
    private static readonly NumberFormatInfo SerbianNumbers = new() { NumberDecimalSeparator = ",", NumberGroupSeparator = "." };

    public static string Money(decimal value) => value.ToString("N2", SerbianNumbers);

    public static string Date(DateOnly? value) => value?.ToString("d.M.yyyy", CultureInfo.InvariantCulture) ?? string.Empty;

    public static string Apply(string? template, NoticeDocumentData d) => (template ?? string.Empty)
        .Replace("[NazivSS]", d.IssuerName, StringComparison.OrdinalIgnoreCase)
        .Replace("[SZPIB]", d.IssuerTaxNumber ?? string.Empty, StringComparison.OrdinalIgnoreCase)
        .Replace("[SZMB]", d.IssuerRegistrationNumber ?? string.Empty, StringComparison.OrdinalIgnoreCase)
        .Replace("[DatumUgovora]", Date(d.DecisionDate), StringComparison.OrdinalIgnoreCase)
        .Replace("[DatumPI]", Date(d.PaymentCutoff), StringComparison.OrdinalIgnoreCase)
        .Replace("[Dug]", Money(d.Debt), StringComparison.OrdinalIgnoreCase)
        .Replace("[TR]", d.IssuerAccountNumber, StringComparison.OrdinalIgnoreCase)
        .Replace("[datum]", Date(d.Date), StringComparison.OrdinalIgnoreCase);
}
