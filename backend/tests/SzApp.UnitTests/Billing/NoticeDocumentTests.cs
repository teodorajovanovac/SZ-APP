using System.Text;
using SzApp.Api.Features.Billing;
using SzApp.Domain.Billing;
using SzApp.Domain.Billing.IpsQr;

namespace SzApp.UnitTests.Billing;

public class NoticeDocumentTests
{
    private static NoticeDocumentData Sample() => new(
        NoticeNumber: "251-1001-P20260930",
        Date: new DateOnly(2026, 9, 30),
        PaymentCutoff: new DateOnly(2026, 9, 25),
        IssuerName: "SZ Bulevar 1",
        IssuerCity: "Beograd",
        IssuerTaxNumber: "123456789",
        IssuerRegistrationNumber: "98765432",
        IssuerAccountNumber: "265-0000000123456-89",
        DecisionDate: new DateOnly(2019, 3, 1),
        CustomerName: "Петар Петровић",
        CustomerAddress: "Bulevar 1/5",
        AccountNumber: 1001,
        Debt: 12345.6m,
        AdditionalCosts: 3000m,
        PaymentReference: "71-251-1001-P20260930",
        Subject: "Predmet: OPOMENA PRED UTUŽENJE",
        Body: "Stambena zajednica [NazivSS], PIB: [SZPIB], MB: [SZMB], odluka od [DatumUgovora]. Stanje na dan [DatumPI]: [Dug] dinara.",
        Closing: "Uplatite na račun [TR].",
        Signature: "U Beogradu [datum]. godine",
        SlipCaption: null,
        Lines: [new NoticeDocumentLine("R-2607", "Račun 2607", new DateOnly(2026, 8, 25), 4115.2m, 0m, 4115.2m)]);

    [Fact]
    public void Template_FillsLegacyPlaceholders()
    {
        var text = NoticeTemplateText.Apply(Sample().Body + " " + Sample().Closing + " " + Sample().Signature, Sample());

        Assert.Equal("Stambena zajednica SZ Bulevar 1, PIB: 123456789, MB: 98765432, odluka od 1.3.2019. Stanje na dan 25.9.2026: 12.345,60 dinara." +
                     " Uplatite na račun 265-0000000123456-89. U Beogradu 30.9.2026. godine", text);
    }

    [Fact]
    public void QrPayload_UsesDebtPlusCostAndNoticeReference()
    {
        var payload = IpsQrPayloadBuilder.Build(Sample().ToIpsQrInput());

        Assert.Contains("|R:265000000012345689|", payload);
        Assert.Contains("|I:RSD15345,60|", payload);
        Assert.Contains("|S:Uplata po opomeni 251-1001-P20260930|", payload);
        Assert.EndsWith("|RO:97712511001P20260930", payload);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Render_ProducesPdf(bool doubleSlip)
    {
        var bytes = NoticePdfRenderer.Render(Sample(), doubleSlip);

        Assert.Equal("%PDF-", Encoding.ASCII.GetString(bytes, 0, 5));
    }

    [Fact]
    public void Render_LongNoticeWithManyLines_Paginates()
    {
        var lines = Enumerable.Range(1, 60).Select(i => new NoticeDocumentLine($"R-{i}", $"Račun {i}", new DateOnly(2026, 1, 25), 100m, 0m, 100m)).ToArray();
        var bytes = NoticePdfRenderer.Render(Sample() with { Lines = lines, Body = string.Join("\n", Enumerable.Repeat(new string('x', 400), 5)) });

        Assert.Matches(@"/Count [2-9]", Encoding.ASCII.GetString(bytes));
    }
}
