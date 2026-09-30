using System.Globalization;
using System.Net;
using System.Text;
using SzApp.Contracts.LedgerBanking;

namespace SzApp.Api.Features.Accounting;

/// <summary>GAP-34 output: printable "Nalog za prenos" HTML and a flat CSV export (UTF-8 with BOM, ';').</summary>
public static class PaymentOrderDocuments
{
    private static readonly CultureInfo Sr = CultureInfo.GetCultureInfo("sr-Latn-RS");

    public static string Html(IReadOnlyList<SupplierPaymentOrderResponse> orders)
    {
        var sb = new StringBuilder();
        sb.Append("""
            <!doctype html><html lang="sr"><head><meta charset="utf-8"><title>Nalozi za prenos</title>
            <style>
            body{font-family:Arial,sans-serif;font-size:12px;margin:16px}
            .po{border:1px solid #000;padding:10px;margin-bottom:16px;display:grid;grid-template-columns:1fr 1fr;gap:6px 24px;page-break-inside:avoid;max-width:900px}
            .po h2{grid-column:1/3;margin:0 0 6px;font-size:14px}
            .f span{display:block;font-size:10px;color:#444}.f div{border:1px solid #999;min-height:18px;padding:3px;white-space:pre-line}
            .row{display:grid;grid-template-columns:70px 60px 1fr;gap:6px}
            @media print{.po:nth-of-type(3n){page-break-after:always}}
            </style></head><body>
            """);
        foreach (var o in orders)
        {
            sb.Append("<section class=\"po\"><h2>NALOG ZA PRENOS</h2><div>");
            Field(sb, "Platilac", o.PayerName);
            Field(sb, "Svrha plaćanja", o.PaymentPurpose);
            Field(sb, "Primalac", o.RecipientName);
            sb.Append("</div><div><div class=\"row\">");
            Field(sb, "Šifra plaćanja", o.PaymentCode.ToString(CultureInfo.InvariantCulture));
            Field(sb, "Valuta", o.Currency);
            Field(sb, "Iznos", "=" + o.Amount.ToString("N2", Sr));
            sb.Append("</div>");
            Field(sb, "Račun platioca", o.PayerAccountNumber);
            Field(sb, "Račun primaoca", o.RecipientAccountNumber);
            Field(sb, "Model i poziv na broj (odobrenje)", $"{o.RecipientModelNumber} {o.RecipientPaymentReference}".Trim());
            Field(sb, "Mesto i datum prijema", $"{o.Place}, {o.Date.ToString("d.M.yyyy", Sr)}");
            Field(sb, "Datum valute", o.ValueDate.ToString("d.M.yyyy", Sr));
            sb.Append("</div></section>");
        }

        sb.Append("<script>window.addEventListener('load',()=>window.print())</script></body></html>");
        return sb.ToString();
    }

    public static byte[] Csv(IReadOnlyList<SupplierPaymentOrderResponse> orders)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Platilac;RacunPlatioca;Primalac;RacunPrimaoca;SifraPlacanja;Valuta;Iznos;Model;PozivNaBroj;SvrhaPlacanja;Mesto;Datum;DatumValute");
        foreach (var o in orders)
        {
            sb.AppendJoin(';',
                Cell(o.PayerName), Cell(o.PayerAccountNumber), Cell(o.RecipientName), Cell(o.RecipientAccountNumber),
                o.PaymentCode.ToString(CultureInfo.InvariantCulture), o.Currency, o.Amount.ToString("0.00", Sr),
                o.RecipientModelNumber?.ToString(CultureInfo.InvariantCulture) ?? string.Empty, Cell(o.RecipientPaymentReference),
                Cell(o.PaymentPurpose), Cell(o.Place), o.Date.ToString("d.M.yyyy", Sr), o.ValueDate.ToString("d.M.yyyy", Sr));
            sb.AppendLine();
        }

        return [.. Encoding.UTF8.GetPreamble(), .. Encoding.UTF8.GetBytes(sb.ToString())];
    }

    private static void Field(StringBuilder sb, string label, string? value) =>
        sb.Append("<div class=\"f\"><span>").Append(WebUtility.HtmlEncode(label)).Append("</span><div>")
            .Append(WebUtility.HtmlEncode(value ?? string.Empty)).Append("</div></div>");

    // Multi-line payer/recipient blocks are flattened; quotes doubled; formula-leading chars neutralised.
    private static string Cell(string? value)
    {
        var text = (value ?? string.Empty).Replace("\r", string.Empty).Replace('\n', ' ');
        if (text.Length > 0 && "=+-@".Contains(text[0])) text = "'" + text;
        return "\"" + text.Replace("\"", "\"\"") + "\"";
    }
}
