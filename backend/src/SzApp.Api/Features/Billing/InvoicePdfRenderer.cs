using System.Globalization;
using PdfSharpCore.Drawing;
using PdfSharpCore.Fonts;
using PdfSharpCore.Pdf;
using SzApp.Domain.Billing;
using SzApp.Domain.Billing.IpsQr;

namespace SzApp.Api.Features.Billing;

/// <summary>Legacy report variants (audit 9.1 "Štampa").</summary>
public enum InvoicePdfTemplate
{
    /// <summary>Racun_006 (default).</summary>
    Standard,
    /// <summary>Racun_006_bez_duga: previous-debt row omitted.</summary>
    WithoutPreviousDebt,
    /// <summary>_013_Zakup: copy of 006 titled "RAČUN ZA ZAKUP".</summary>
    Lease,
    /// <summary>RACUN_012 BENEFIT: archived benefit lines listed.</summary>
    Benefit,
    /// <summary>Racun_007: group (master) invoice with its members listed.</summary>
    Group
}

/// <summary>
/// Renders one <see cref="InvoiceDocumentData"/> as an A4 PDF: issuer/customer header, invoice
/// metadata, line items (EUR/rate columns only when a line has a EUR price), benefit/group sections
/// when the data carries them, previous debt, interest (a total per audit 9.1 "kamata nije stavka"),
/// grand total, IPS QR, and either a single slip or the legacy double payment slip.
/// </summary>
public static class InvoicePdfRenderer
{
    internal const string FontFamily = "DejaVu Sans";

    static InvoicePdfRenderer()
    {
        EnsureFonts();
    }

    internal static void EnsureFonts() => GlobalFontSettings.FontResolver ??= new EmbeddedFontResolver();

    /// <param name="includePreviousDebtLine">False renders the "_bez_duga" variant (legacy Racun_006_bez_duga): the previous-debt row is omitted.</param>
    public static byte[] Render(InvoiceDocumentData data, bool includePreviousDebtLine = true) =>
        Render(data, includePreviousDebtLine ? InvoicePdfTemplate.Standard : InvoicePdfTemplate.WithoutPreviousDebt);

    public static byte[] Render(InvoiceDocumentData data, InvoicePdfTemplate template, bool doubleSlip = false)
    {
        var qrPng = IpsQrCodeGenerator.GeneratePng(new IpsQrInput(
            data.IssuerAccountNumber, data.IssuerName, data.IssuerCity,
            data.CustomerName, data.CustomerAddress,
            data.PreviousDebt, data.Total, data.IsExtraordinary,
            PaymentPurpose: "Uplata po racunu", SeriesText: data.InvoiceNumber,
            PaymentReferenceDigits: data.PaymentReference));

        using var document = new PdfDocument();
        var pages = new PageCursor(document, doubleSlip ? 190 : 130);
        var regular = new XFont(FontFamily, 9);
        var small = new XFont(FontFamily, 8);
        var heading = new XFont(FontFamily, 14);
        var bold = new XFont(FontFamily, 10);
        double margin = PageCursor.Margin, y = margin, width = pages.Width;

        var title = template switch
        {
            InvoicePdfTemplate.Lease => "RAČUN ZA ZAKUP",
            InvoicePdfTemplate.Group => "ZBIRNI RAČUN",
            _ when data.GroupMembers is { Count: > 0 } => "ZBIRNI RAČUN",
            _ => "RAČUN"
        };
        pages.Gfx.DrawString($"{title} {data.InvoiceNumber}", heading, XBrushes.Black, new XPoint(margin, y));
        y += 24;

        // issuer / customer two-column header
        var colWidth = width / 2 - 10;
        DrawBlock(pages.Gfx, regular, bold, margin, y, colWidth, "Izdavalac", data.IssuerName, data.IssuerCity, data.IssuerAccountNumber);
        DrawBlock(pages.Gfx, regular, bold, margin + colWidth + 20, y, colWidth, "Kupac", data.CustomerName, data.CustomerAddress);
        y += 70;

        pages.Gfx.DrawString($"Datum izdavanja: {Fmt(data.IssueDate)}    Valuta: {Fmt(data.DueDate)}    Poziv na broj: {data.PaymentReference}",
            small, XBrushes.Black, new XPoint(margin, y));
        y += 18;

        // Line items -- legacy shows the EUR/rate columns only when some line has CenaE != 0.
        var hasEur = data.Lines.Any(x => x.PriceEur != 0m);
        double[] cols = hasEur
            ? [width * 0.30, width * 0.10, width * 0.12, width * 0.10, width * 0.13, width * 0.08, width * 0.17]
            : [width * 0.42, width * 0.13, width * 0.15, width * 0.10, width * 0.20];
        string[] headers = hasEur
            ? ["Opis", "Količina", "Cena EUR", "Kurs", "Cena", "PDV %", "Iznos"]
            : ["Opis", "Količina", "Cena", "PDV %", "Iznos"];
        y = DrawTableRow(pages.Gfx, bold, margin, y, cols, headers);
        pages.Gfx.DrawLine(XPens.Black, margin, y, margin + width, y);
        y += 2;

        // Legacy: lines with UkupnoRSD = 0 stay on the invoice but are not printed.
        foreach (var line in data.Lines.Where(x => x.Total != 0m))
        {
            y = pages.Ensure(y, 14);
            string[] values = hasEur
                ? [line.Description, Q(line.Quantity), line.PriceEur == 0m ? "" : Q(line.PriceEur), line.PriceEur == 0m ? "" : Q(line.ExchangeRate),
                    Money(line.UnitPrice), $"{line.VatRate:0.##}", Money(line.Total)]
                : [line.Description, Q(line.Quantity), Money(line.UnitPrice), $"{line.VatRate:0.##}", Money(line.Total)];
            y = DrawTableRow(pages.Gfx, regular, margin, y, cols, values);
        }
        pages.Gfx.DrawLine(XPens.Black, margin, y, margin + width, y);
        y += 8;

        if (data.BenefitLines is { Count: > 0 } benefits)
        {
            y = pages.Ensure(y, 30);
            pages.Gfx.DrawString("Benefit (umanjenje naknade upravnika)", bold, XBrushes.Black, new XPoint(margin, y + 10));
            y += 16;
            double[] bcols = [width * 0.40, width * 0.40, width * 0.20];
            foreach (var b in benefits)
            {
                y = pages.Ensure(y, 14);
                y = DrawTableRow(pages.Gfx, small, margin, y, bcols, [b.Description, b.Note, Money(-b.OriginalAmount)], rightFrom: 2);
            }
            y += 6;
        }

        if (data.GroupMembers is { Count: > 0 } members)
        {
            y = pages.Ensure(y, 30);
            pages.Gfx.DrawString("Računi članova grupe", bold, XBrushes.Black, new XPoint(margin, y + 10));
            y += 16;
            double[] mcols = [width * 0.22, width * 0.33, width * 0.28, width * 0.17];
            foreach (var m in members)
            {
                y = pages.Ensure(y, 14);
                y = DrawTableRow(pages.Gfx, small, margin, y, mcols, [m.InvoiceNumber, m.CustomerName, m.Address, Money(m.Total)], rightFrom: 3);
            }
            y += 6;
        }

        y = pages.Ensure(y, 90);
        y = DrawTotalRow(pages.Gfx, regular, margin, width, y, "Osnovica", Money(data.SubTotal));
        y = DrawTotalRow(pages.Gfx, regular, margin, width, y, "PDV", Money(data.VatAmount));
        if (data.Interest != 0m) y = DrawTotalRow(pages.Gfx, regular, margin, width, y, "Kamata", Money(data.Interest));
        if (template != InvoicePdfTemplate.WithoutPreviousDebt) y = DrawTotalRow(pages.Gfx, regular, margin, width, y, "Prethodno dugovanje", Money(data.PreviousDebt));
        DrawTotalRow(pages.Gfx, bold, margin, width, y, "Za uplatu", Money(data.AmountDue));

        var slip = new SlipData(data.CustomerName, data.CustomerAddress, $"Uplata po računu {data.InvoiceNumber}",
            data.IssuerName, data.IssuerCity, data.IssuerAccountNumber, data.PaymentReference, data.AmountDue);
        pages.DrawSlip(slip, qrPng, doubleSlip, small, bold);
        return pages.Finish();
    }

    /// <summary>Tracks the current page; rows that would run into the reserved slip area continue on a new page.</summary>
    internal sealed class PageCursor
    {
        public const double Margin = 36;
        private readonly PdfDocument document;
        private readonly double reservedBottom;
        private PdfPage page;

        public PageCursor(PdfDocument document, double reservedBottom)
        {
            this.document = document;
            this.reservedBottom = reservedBottom;
            page = document.AddPage();
            page.Size = PdfSharpCore.PageSize.A4;
            Gfx = XGraphics.FromPdfPage(page);
        }

        public XGraphics Gfx { get; private set; }
        public double Width => page.Width - 2 * Margin;

        public double Ensure(double y, double needed)
        {
            if (y + needed <= page.Height - Margin - reservedBottom) return y;
            Gfx.Dispose();
            page = document.AddPage();
            page.Size = PdfSharpCore.PageSize.A4;
            Gfx = XGraphics.FromPdfPage(page);
            return Margin;
        }

        public void DrawSlip(SlipData slip, byte[] qrPng, bool doubleSlip, XFont small, XFont bold)
        {
            var y = page.Height - Margin - reservedBottom;
            if (doubleSlip) DrawDoubleSlip(Gfx, small, bold, qrPng, Margin, y, Width, slip);
            else DrawSingleSlip(Gfx, small, bold, qrPng, Margin, y, Width, slip);
        }

        public byte[] Finish()
        {
            Gfx.Dispose();
            using var output = new MemoryStream();
            document.Save(output, closeStream: false);
            return output.ToArray();
        }
    }

    internal sealed record SlipData(string PayerName, string PayerAddress, string Purpose, string RecipientName, string RecipientCity,
        string RecipientAccount, string PaymentReference, decimal Amount);

    private static void DrawSingleSlip(XGraphics gfx, XFont small, XFont bold, byte[] qrPng, double x, double y, double width, SlipData s)
    {
        using var qrImage = XImage.FromStream(() => new MemoryStream(qrPng));
        const double qrSize = 100;
        gfx.DrawImage(qrImage, x, y, qrSize, qrSize);
        gfx.DrawString("NBS IPS QR", small, XBrushes.Black, new XPoint(x, y + qrSize + 12));

        var slipX = x + qrSize + 20;
        gfx.DrawRectangle(XPens.Black, slipX, y, width - qrSize - 20, qrSize + 20);
        gfx.DrawString("UPLATNICA", bold, XBrushes.Black, new XPoint(slipX + 8, y + 16));
        gfx.DrawString($"Primalac: {s.RecipientName}", small, XBrushes.Black, new XPoint(slipX + 8, y + 34));
        gfx.DrawString($"Račun: {s.RecipientAccount}", small, XBrushes.Black, new XPoint(slipX + 8, y + 48));
        gfx.DrawString($"Poziv na broj: {s.PaymentReference}", small, XBrushes.Black, new XPoint(slipX + 8, y + 62));
        gfx.DrawString($"Iznos: {Money(s.Amount)} RSD", small, XBrushes.Black, new XPoint(slipX + 8, y + 76));
    }

    /// <summary>
    /// Legacy double payment slip (Racun_006 footer): two identical "nalog za uplatu" forms side by
    /// side -- one kept by the payer, one handed to the bank -- with the QR printed above them.
    /// </summary>
    private static void DrawDoubleSlip(XGraphics gfx, XFont small, XFont bold, byte[] qrPng, double x, double y, double width, SlipData s)
    {
        using (var qrImage = XImage.FromStream(() => new MemoryStream(qrPng)))
        {
            gfx.DrawImage(qrImage, x + width - 52, y, 52, 52);
        }
        gfx.DrawString("NBS IPS QR", small, XBrushes.Black, new XPoint(x + width - 110, y + 30));

        var slipY = y + 56;
        var slipWidth = (width - 10) / 2;
        (string Label, string Value)[] fields =
        [
            ("Uplatilac", $"{s.PayerName}, {s.PayerAddress}"),
            ("Svrha uplate", s.Purpose),
            ("Primalac", $"{s.RecipientName} {s.RecipientCity}".Trim()),
            ("Šifra plaćanja / valuta / iznos", $"221   RSD   {Money(s.Amount)}"),
            ("Račun primaoca", s.RecipientAccount),
            ("Model i poziv na broj", $"97   {s.PaymentReference}")
        ];
        for (var i = 0; i < 2; i++)
        {
            var sx = x + i * (slipWidth + 10);
            gfx.DrawRectangle(XPens.Black, sx, slipY, slipWidth, 130);
            gfx.DrawString("NALOG ZA UPLATU", bold, XBrushes.Black, new XPoint(sx + 6, slipY + 14));
            var ly = slipY + 26;
            foreach (var (label, value) in fields)
            {
                gfx.DrawString(label, small, XBrushes.Gray, new XPoint(sx + 6, ly));
                gfx.DrawString(value, small, XBrushes.Black, new XRect(sx + 6, ly + 1, slipWidth - 12, 10), XStringFormats.TopLeft);
                ly += 17;
            }
        }
    }

    internal static void DrawBlock(XGraphics gfx, XFont regular, XFont bold, double x, double y, double width, string title, string line1, string line2, string? line3 = null)
    {
        gfx.DrawString(title, bold, XBrushes.Black, new XPoint(x, y));
        gfx.DrawString(line1, regular, XBrushes.Black, new XPoint(x, y + 14));
        gfx.DrawString(line2, regular, XBrushes.Black, new XPoint(x, y + 28));
        if (line3 is not null) gfx.DrawString(line3, regular, XBrushes.Black, new XPoint(x, y + 42));
    }

    /// <summary>Columns from <paramref name="rightFrom"/> on are right-aligned (numbers).</summary>
    internal static double DrawTableRow(XGraphics gfx, XFont font, double x, double y, double[] cols, string[] values, int rightFrom = 1)
    {
        var cx = x;
        for (var i = 0; i < values.Length; i++)
        {
            gfx.DrawString(values[i], font, XBrushes.Black, new XRect(cx, y, cols[i], 14),
                i >= rightFrom ? XStringFormats.TopRight : XStringFormats.TopLeft);
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

    internal static string Fmt(DateOnly date) => date.ToString("d.M.yyyy.", CultureInfo.InvariantCulture);
    internal static string Money(decimal value) => value.ToString("N2", CultureInfo.InvariantCulture);
    private static string Q(decimal value) => value.ToString("0.####", CultureInfo.InvariantCulture);

    internal sealed class EmbeddedFontResolver : IFontResolver
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
