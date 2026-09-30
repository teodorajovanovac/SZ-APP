using PdfSharpCore.Drawing;
using PdfSharpCore.Drawing.Layout;
using PdfSharpCore.Pdf;
using SzApp.Domain.Billing;
using SzApp.Domain.Billing.IpsQr;

namespace SzApp.Api.Features.Billing;

/// <summary>
/// Opomena PDF (legacy report on OpomeneSabloni): recipient block, subject, template body with the
/// placeholders filled, the counted documents, closing text, signature, and the IPS QR + slip for
/// Dug + trošak. Reuses the invoice renderer's font, table and slip helpers.
/// </summary>
public static class NoticePdfRenderer
{
    public static byte[] Render(NoticeDocumentData data, bool doubleSlip = false)
    {
        InvoicePdfRenderer.EnsureFonts();
        var qrPng = IpsQrCodeGenerator.GeneratePng(data.ToIpsQrInput());

        using var document = new PdfDocument();
        var pages = new InvoicePdfRenderer.PageCursor(document, doubleSlip ? 190 : 130);
        var regular = new XFont(InvoicePdfRenderer.FontFamily, 9);
        var small = new XFont(InvoicePdfRenderer.FontFamily, 8);
        var heading = new XFont(InvoicePdfRenderer.FontFamily, 12);
        var bold = new XFont(InvoicePdfRenderer.FontFamily, 10);
        double margin = InvoicePdfRenderer.PageCursor.Margin, y = margin, width = pages.Width;

        InvoicePdfRenderer.DrawBlock(pages.Gfx, regular, bold, margin, y, width / 2 - 10, data.IssuerName, data.IssuerCity,
            $"PIB {data.IssuerTaxNumber}  MB {data.IssuerRegistrationNumber}".Trim(), data.IssuerAccountNumber);
        InvoicePdfRenderer.DrawBlock(pages.Gfx, regular, bold, margin + width / 2 + 10, y, width / 2 - 10, "Dužnik",
            data.CustomerName, data.CustomerAddress, $"Šifra: {data.AccountNumber}");
        y += 66;

        var subject = string.IsNullOrWhiteSpace(data.Subject) ? "OPOMENA" : NoticeTemplateText.Apply(data.Subject, data);
        pages.Gfx.DrawString(subject, heading, XBrushes.Black, new XPoint(margin, y));
        y += 20;

        y = DrawParagraph(pages, regular, NoticeTemplateText.Apply(data.Body, data), margin, y, width);

        double[] cols = [width * 0.20, width * 0.38, width * 0.14, width * 0.14, width * 0.14];
        y = pages.Ensure(y, 30);
        y = InvoicePdfRenderer.DrawTableRow(pages.Gfx, bold, margin, y, cols, ["Dokument", "Opis", "Valuta", "Iznos", "Dug"], rightFrom: 3);
        pages.Gfx.DrawLine(XPens.Black, margin, y, margin + width, y);
        y += 2;
        foreach (var line in data.Lines)
        {
            y = pages.Ensure(y, 14);
            y = InvoicePdfRenderer.DrawTableRow(pages.Gfx, small, margin, y, cols,
                [line.DocumentRef, line.Text, InvoicePdfRenderer.Fmt(line.DueDate), InvoicePdfRenderer.Money(line.Debit), InvoicePdfRenderer.Money(line.Sum)], rightFrom: 3);
        }
        pages.Gfx.DrawLine(XPens.Black, margin, y, margin + width, y);
        y += 6;
        y = pages.Ensure(y, 50);
        y = Total(pages.Gfx, regular, margin, width, y, "Dug", data.Debt);
        if (data.AdditionalCosts != 0m) y = Total(pages.Gfx, regular, margin, width, y, "Troškovi opomene", data.AdditionalCosts);
        y = Total(pages.Gfx, bold, margin, width, y, "Ukupno za uplatu", data.Total) + 8;

        if (!string.IsNullOrWhiteSpace(data.Closing)) y = DrawParagraph(pages, regular, NoticeTemplateText.Apply(data.Closing, data), margin, y, width);
        if (!string.IsNullOrWhiteSpace(data.Signature)) DrawParagraph(pages, regular, NoticeTemplateText.Apply(data.Signature, data), margin + width / 2, y, width / 2);

        pages.DrawSlip(new InvoicePdfRenderer.SlipData(data.CustomerName, data.CustomerAddress,
            string.IsNullOrWhiteSpace(data.SlipCaption) ? $"Uplata po opomeni {data.NoticeNumber}" : data.SlipCaption,
            data.IssuerName, data.IssuerCity, data.IssuerAccountNumber, data.PaymentReference, data.Total), qrPng, doubleSlip, small, bold);
        return pages.Finish();
    }

    /// <summary>Word-wrapped text, paragraph by paragraph so long bodies continue on the next page.</summary>
    private static double DrawParagraph(InvoicePdfRenderer.PageCursor pages, XFont font, string text, double x, double y, double width)
    {
        const double lineHeight = 12;
        // ponytail: height estimated from character count (~0.5em per char) instead of measuring each
        // wrapped line; good enough for page breaks, measure with XGraphics if text ever clips.
        var charsPerLine = Math.Max(20, (int)(width / (font.Size * 0.5)));
        foreach (var paragraph in text.Replace("\r\n", "\n").Split('\n'))
        {
            var lines = Math.Max(1, (int)Math.Ceiling(paragraph.Length / (double)charsPerLine) + 1);
            var height = lines * lineHeight;
            y = pages.Ensure(y, height);
            if (paragraph.Length > 0)
                new XTextFormatter(pages.Gfx).DrawString(paragraph, font, XBrushes.Black, new XRect(x, y, width, height));
            y += paragraph.Length == 0 ? lineHeight / 2 : height - lineHeight + 4;
        }
        return y + 4;
    }

    private static double Total(XGraphics gfx, XFont font, double x, double width, double y, string label, decimal value)
    {
        gfx.DrawString(label, font, XBrushes.Black, new XPoint(x + width - 200, y + 10));
        gfx.DrawString(InvoicePdfRenderer.Money(value), font, XBrushes.Black, new XRect(x + width - 80, y, 80, 14), XStringFormats.TopRight);
        return y + 16;
    }
}
