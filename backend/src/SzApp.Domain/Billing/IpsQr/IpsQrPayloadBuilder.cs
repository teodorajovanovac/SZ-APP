using System.Globalization;
using System.Text.RegularExpressions;

namespace SzApp.Domain.Billing.IpsQr;

/// <summary>
/// Builds the NBS IPS QR payload string for invoices, per audit-detaljni.md 9.1 "IPS QR".
/// Pure string construction — no image rendering here (see <see cref="IpsQrCodeGenerator"/>).
/// </summary>
public static class IpsQrPayloadBuilder
{
    private const int MaxNameBlockLength = 70;
    private const int MaxSeriesTextLength = 35;

    public static string Build(IpsQrInput input)
    {
        var issuerBlock = Truncate(NameCityLine(input.IssuerName, input.IssuerCity), MaxNameBlockLength);
        var customerBlock = Truncate(NameCityLine(input.CustomerName, input.CustomerAddress), MaxNameBlockLength);
        var seriesText = Truncate(Sanitize(input.SeriesText), MaxSeriesTextLength);
        var purpose = Sanitize(input.PaymentPurpose);
        var reference = StripReferenceDashes(input.PaymentReferenceDigits);
        var account = StripAccountFormatting(input.IssuerAccountNumber18);
        var amount = FormatAmount(input.Amount);

        return $"K:PR|V:01|C:1|R:{account}|N:{issuerBlock}|I:RSD{amount}|P:{customerBlock}|SF:221|S:{purpose} {seriesText}|RO:97{reference}";
    }

    private static string NameCityLine(string name, string cityOrAddress) =>
        $"{Sanitize(name)}\r\n{Sanitize(cityOrAddress)}";

    /// <summary>2 decimals, comma separator, no thousands grouping (e.g. "2966,04").</summary>
    private static string FormatAmount(decimal amount) =>
        amount.ToString("F2", CultureInfo.InvariantCulture).Replace('.', ',');

    private static string StripReferenceDashes(string reference) =>
        (reference ?? string.Empty).Replace("-", string.Empty);

    private static string StripAccountFormatting(string account) =>
        Regex.Replace(account ?? string.Empty, "[^0-9]", string.Empty);

    /// <summary>Sanitization per audit spec: &amp; -&gt; space, &lt;/&gt; -&gt; '-', quotes stripped.</summary>
    private static string Sanitize(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value
            .Replace("&", " ")
            .Replace("<", "-")
            .Replace(">", "-")
            .Replace("\"", string.Empty)
            .Replace("'", string.Empty);
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];
}
