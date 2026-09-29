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
}
