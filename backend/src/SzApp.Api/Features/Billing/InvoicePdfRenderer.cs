using System.Globalization;
using System.Reflection;
using PdfSharpCore.Drawing;
using PdfSharpCore.Fonts;
using PdfSharpCore.Pdf;
using SzApp.Domain.Billing;
using SzApp.Domain.Billing.IpsQr;

namespace SzApp.Api.Features.Billing;

/// <summary>
/// Renders one <see cref="InvoiceDocumentData"/> as an A4 PDF: issuer/customer header, invoice
/// metadata, line items, previous debt, interest (shown as a total per audit 9.1 "kamata nije
/// stavka" -- never a line item), grand total, and the IPS QR image.
/// ponytail: single payment-slip area instead of the legacy double-slip layout -- a clean single
/// slip covers the requirement; add the second slip if the owner asks for the old dual layout.
/// </summary>
public static class InvoicePdfRenderer
{
    private const string FontFamily = "DejaVu Sans";

    static InvoicePdfRenderer()
    {
        GlobalFontSettings.FontResolver ??= new EmbeddedFontResolver();
    }

    /// <param name="includePreviousDebtLine">False renders the "_bez_duga" variant (legacy Racun_006_bez_duga): the previous-debt row is omitted.</param>
    public static byte[] Render(InvoiceDocumentData data, bool includePreviousDebtLine = true)
    {
        var qrPng = IpsQrCodeGenerator.GeneratePng(new IpsQrInput(
            data.IssuerAccountNumber, data.IssuerName, data.IssuerCity,
            data.CustomerName, data.CustomerAddress,
            data.PreviousDebt, data.Total, data.IsExtraordinary,
            PaymentPurpose: "Uplata po racunu", SeriesText: data.InvoiceNumber,
            PaymentReferenceDigits: data.PaymentReference));

        using var document = new PdfDocument();
        var page = document.AddPage();
        page.Size = PdfSharpCore.PageSize.A4;
        using var gfx = XGraphics.FromPdfPage(page);

        var regular = new XFont(FontFamily, 9);
        var small = new XFont(FontFamily, 8);
        var heading = new XFont(FontFamily, 14);
        var bold = new XFont(FontFamily, 10);

        double margin = 36, y = margin, width = page.Width - 2 * margin;

        gfx.DrawString($"Račun {data.InvoiceNumber}", heading, XBrushes.Black, new XPoint(margin, y));
        y += 24;

        // issuer / customer two-column header
        var colWidth = width / 2 - 10;
        DrawBlock(gfx, regular, bold, margin, y, colWidth, "Izdavalac", data.IssuerName, data.IssuerCity, data.IssuerAccountNumber);
        DrawBlock(gfx, regular, bold, margin + colWidth + 20, y, colWidth, "Kupac", data.CustomerName, data.CustomerAddress);
        y += 70;

        gfx.DrawString($"Datum izdavanja: {Fmt(data.IssueDate)}    Valuta: {Fmt(data.DueDate)}    Poziv na broj: {data.PaymentReference}",
            small, XBrushes.Black, new XPoint(margin, y));
        y += 18;

        // line items table
        double[] cols = [width * 0.42, width * 0.13, width * 0.15, width * 0.10, width * 0.20];
        string[] headers = ["Opis", "Količina", "Cena", "PDV %", "Iznos"];
        y = DrawTableRow(gfx, bold, margin, y, cols, headers);
        gfx.DrawLine(XPens.Black, margin, y, margin + width, y);
        y += 2;

        foreach (var line in data.Lines)
        {
            y = DrawTableRow(gfx, regular, margin, y, cols,
                [line.Description, Q(line.Quantity), Money(line.UnitPrice), $"{line.VatRate:0.##}", Money(line.Total)]);
        }
        gfx.DrawLine(XPens.Black, margin, y, margin + width, y);
        y += 8;

        y = DrawTotalRow(gfx, regular, margin, width, y, "Osnovica", Money(data.SubTotal));
        y = DrawTotalRow(gfx, regular, margin, width, y, "PDV", Money(data.VatAmount));
        if (data.Interest != 0m) y = DrawTotalRow(gfx, regular, margin, width, y, "Kamata", Money(data.Interest));
        if (includePreviousDebtLine) y = DrawTotalRow(gfx, regular, margin, width, y, "Prethodno dugovanje", Money(data.PreviousDebt));
        y = DrawTotalRow(gfx, bold, margin, width, y, "Za uplatu", Money(data.AmountDue));
        y += 12;

        // QR + payment slip area
        using (var qrImage = XImage.FromStream(() => new MemoryStream(qrPng)))
        {
            double qrSize = 100;
            gfx.DrawImage(qrImage, margin, y, qrSize, qrSize);
            gfx.DrawString("NBS IPS QR", small, XBrushes.Black, new XPoint(margin, y + qrSize + 12));

            var slipX = margin + qrSize + 20;
            gfx.DrawRectangle(XPens.Black, slipX, y, width - qrSize - 20, qrSize + 20);
            gfx.DrawString("UPLATNICA", bold, XBrushes.Black, new XPoint(slipX + 8, y + 16));
            gfx.DrawString($"Primalac: {data.IssuerName}", small, XBrushes.Black, new XPoint(slipX + 8, y + 34));
            gfx.DrawString($"Račun: {data.IssuerAccountNumber}", small, XBrushes.Black, new XPoint(slipX + 8, y + 48));
            gfx.DrawString($"Poziv na broj: {data.PaymentReference}", small, XBrushes.Black, new XPoint(slipX + 8, y + 62));
            gfx.DrawString($"Iznos: {Money(data.AmountDue)} RSD", small, XBrushes.Black, new XPoint(slipX + 8, y + 76));
        }

        using var output = new MemoryStream();
        document.Save(output, closeStream: false);
        return output.ToArray();
    }

    private static void DrawBlock(XGraphics gfx, XFont regular, XFont bold, double x, double y, double width, string title, string line1, string line2, string? line3 = null)
    {
        gfx.DrawString(title, bold, XBrushes.Black, new XPoint(x, y));
        gfx.DrawString(line1, regular, XBrushes.Black, new XPoint(x, y + 14));
        gfx.DrawString(line2, regular, XBrushes.Black, new XPoint(x, y + 28));
        if (line3 is not null) gfx.DrawString(line3, regular, XBrushes.Black, new XPoint(x, y + 42));
    }

    private static double DrawTableRow(XGraphics gfx, XFont font, double x, double y, double[] cols, string[] values)
    {
        var cx = x;
        for (var i = 0; i < values.Length; i++)
        {
            gfx.DrawString(values[i], font, XBrushes.Black, new XRect(cx, y, cols[i], 14),
                i is 1 or 2 or 3 or 4 ? XStringFormats.TopRight : XStringFormats.TopLeft);
            cx += cols[i];
        }
        return y + 14;
    }

    private static double DrawTotalRow(XGraphics gfx, XFont font, double x, double width, double y, string label, string value)
    {
        gfx.DrawString(label, font, XBrushes.Black, new XPoint(x + width - 180, y));
        gfx.DrawString(value, font, XBrushes.Black, new XRect(x + width - 80, y, 80, 14), XStringFormats.TopRight);
        return y + 16;
    }

    private static string Fmt(DateOnly date) => date.ToString("d.M.yyyy.", CultureInfo.InvariantCulture);
    private static string Money(decimal value) => value.ToString("N2", CultureInfo.InvariantCulture);
    private static string Q(decimal value) => value.ToString("0.####", CultureInfo.InvariantCulture);

    private sealed class EmbeddedFontResolver : IFontResolver
    {
        private static readonly byte[] DejaVuSansBytes = LoadEmbedded();

        public string DefaultFontName => FontFamily;

        public byte[] GetFont(string faceName) => DejaVuSansBytes;

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic) =>
            // Only one weight is embedded (see Assets/Fonts/DejaVuSans-LICENSE.txt) -- bold/italic
            // requests fall back to the regular face rather than a missing font.
            new(FontFamily);

        private static byte[] LoadEmbedded()
        {
            var assembly = typeof(EmbeddedFontResolver).Assembly;
            var resourceName = assembly.GetManifestResourceNames()
                .Single(n => n.EndsWith("DejaVuSans.ttf", StringComparison.Ordinal));
            using var stream = assembly.GetManifestResourceStream(resourceName)!;
            using var buffer = new MemoryStream();
            stream.CopyTo(buffer);
            return buffer.ToArray();
        }
    }
}
