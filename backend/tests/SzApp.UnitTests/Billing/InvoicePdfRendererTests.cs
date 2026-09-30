using System.Text;
using SzApp.Api.Features.Billing;
using SzApp.Domain.Billing;

namespace SzApp.UnitTests.Billing;

public class InvoicePdfRendererTests
{
    private static InvoiceDocumentData SampleInvoice() => new(
        InvoiceNumber: "251-1001-2609",
        PaymentReference: "08251-1001-2609",
        IssueDate: new DateOnly(2026, 9, 15),
        DueDate: new DateOnly(2026, 10, 25),
        IssuerName: "SZ Test",
        IssuerCity: "Beograd",
        IssuerAccountNumber: "265000000123456789",
        CustomerName: "Petar Petrović",
        CustomerAddress: "Bulevar 1",
        Lines: [new InvoiceDocumentLine("Održavanje", 1m, 2000m, 0m, 2000m)],
        PreviousDebt: 500m,
        Interest: 12.34m,
        SubTotal: 2000m,
        VatAmount: 0m,
        Total: 2012.34m,
        IsExtraordinary: false);

    [Fact]
    public void Render_ProducesNonEmptyValidPdf()
    {
        var bytes = InvoicePdfRenderer.Render(SampleInvoice());

        Assert.NotEmpty(bytes);
        var header = Encoding.ASCII.GetString(bytes, 0, 5);
        Assert.Equal("%PDF-", header);
    }

    [Fact]
    public void Render_BezDugaVariant_AlsoProducesValidPdf()
    {
        var bytes = InvoicePdfRenderer.Render(SampleInvoice(), includePreviousDebtLine: false);

        Assert.NotEmpty(bytes);
        Assert.Equal("%PDF-", Encoding.ASCII.GetString(bytes, 0, 5));
    }

    [Theory]
    [InlineData(InvoicePdfTemplate.Standard, false)]
    [InlineData(InvoicePdfTemplate.WithoutPreviousDebt, false)]
    [InlineData(InvoicePdfTemplate.Lease, true)]
    [InlineData(InvoicePdfTemplate.Benefit, true)]
    [InlineData(InvoicePdfTemplate.Group, false)]
    public void Render_AllTemplates_WithEurBenefitGroupAndCyrillic(InvoicePdfTemplate template, bool doubleSlip)
    {
        var data = SampleInvoice() with
        {
            CustomerName = "Петар Петровић",
            Lines =
            [
                new InvoiceDocumentLine("Održavanje", 1m, 2000m, 0m, 2000m),
                new InvoiceDocumentLine("Upravnik (EUR)", 1m, 1174.12m, 0m, 1174.12m, PriceEur: 10m, ExchangeRate: 117.412m),
                new InvoiceDocumentLine("Nulta stavka", 1m, 0m, 0m, 0m)
            ],
            BenefitLines = [new InvoiceDocumentBenefitLine("Upravnik", "Benefit FM 1/1 – 2609", 1174.12m)],
            GroupMembers = [new InvoiceDocumentGroupMember("251-1002-2609", "Član Grupe", "Ulica 2", 1500m)]
        };

        var bytes = InvoicePdfRenderer.Render(data, template, doubleSlip);

        Assert.Equal("%PDF-", Encoding.ASCII.GetString(bytes, 0, 5));
    }

    [Fact]
    public void Render_ManyLines_SpillsOntoSecondPage()
    {
        var lines = Enumerable.Range(1, 80).Select(i => new InvoiceDocumentLine($"Stavka {i}", 1m, 10m, 0m, 10m)).ToArray();
        var single = InvoicePdfRenderer.Render(SampleInvoice(), InvoicePdfTemplate.Standard);
        var multi = InvoicePdfRenderer.Render(SampleInvoice() with { Lines = lines }, InvoicePdfTemplate.Standard, doubleSlip: true);

        Assert.Contains("/Count 1", Encoding.ASCII.GetString(single));
        Assert.Matches(@"/Count [2-9]", Encoding.ASCII.GetString(multi));
    }
}
