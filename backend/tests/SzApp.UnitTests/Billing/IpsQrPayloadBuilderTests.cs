using SzApp.Domain.Billing.IpsQr;

namespace SzApp.UnitTests.Billing;

public class IpsQrPayloadBuilderTests
{
    private static IpsQrInput Base(decimal previousDebt = 1000m, decimal invoiceTotal = 2000m, bool extraordinary = false) => new(
        IssuerAccountNumber18: "265-1234567890123-45",
        IssuerName: "SZ Test",
        IssuerCity: "Beograd",
        CustomerName: "Petar Petrović",
        CustomerAddress: "Ulica 1",
        PreviousDebt: previousDebt,
        InvoiceTotal: invoiceTotal,
        IsExtraordinary: extraordinary,
        PaymentPurpose: "Naknada za odrzavanje",
        SeriesText: "Serija 2609",
        PaymentReferenceDigits: "08-251-1001-2609");

    [Fact]
    public void Build_ProducesExpectedFieldsInOrder()
    {
        var payload = IpsQrPayloadBuilder.Build(Base());

        Assert.StartsWith("K:PR|V:01|C:1|R:", payload);
        Assert.Contains("|N:SZ Test\r\nBeograd|", payload);
        Assert.Contains("|I:RSD3000,00|", payload);
        Assert.Contains("|P:Petar Petrović\r\nUlica 1|", payload);
        Assert.Contains("|SF:221|", payload);
        Assert.Contains("|S:Naknada za odrzavanje Serija 2609|", payload);
        Assert.EndsWith("|RO:970825110012609", payload);
    }

    [Fact]
    public void Build_StripsNonDigitsFromAccountNumber()
    {
        var payload = IpsQrPayloadBuilder.Build(Base());
        Assert.Contains("|R:265123456789012345|", payload);
    }

    [Fact]
    public void Build_StripsDashesFromPaymentReference_KeepsRO97Prefix()
    {
        var payload = IpsQrPayloadBuilder.Build(Base());
        Assert.EndsWith("|RO:970825110012609", payload);
    }

    [Fact]
    public void Amount_RegularInvoice_IsMaxOfPreviousDebtPlusTotalAndZero()
    {
        var input = Base(previousDebt: 1000m, invoiceTotal: 2000m, extraordinary: false);
        Assert.Equal(3000m, input.Amount);
    }

    [Fact]
    public void Amount_RegularInvoice_NeverGoesNegative()
    {
        var input = Base(previousDebt: -5000m, invoiceTotal: 2000m, extraordinary: false);
        Assert.Equal(0m, input.Amount);
    }

    [Fact]
    public void Amount_ExtraordinaryInvoice_IgnoresPreviousDebt()
    {
        var input = Base(previousDebt: 1000m, invoiceTotal: 2000m, extraordinary: true);
        Assert.Equal(2000m, input.Amount);
    }

    [Fact]
    public void Build_FormatsAmountWithCommaDecimal()
    {
        var input = Base() with { PreviousDebt = 0m, InvoiceTotal = 2966.04m };
        var payload = IpsQrPayloadBuilder.Build(input);
        Assert.Contains("|I:RSD2966,04|", payload);
    }

    [Fact]
    public void Build_TruncatesIssuerNameCityBlockTo70Chars()
    {
        var longCity = new string('А', 80); // Cyrillic char, exercises non-ASCII truncation too
        var input = Base() with { IssuerCity = longCity };
        var payload = IpsQrPayloadBuilder.Build(input);
        var nField = ExtractField(payload, "N:");
        Assert.True(nField.Length <= 70, $"N field too long: {nField.Length}");
    }

    [Fact]
    public void Build_TruncatesCustomerAddressBlockTo70Chars()
    {
        var longAddress = new string('x', 80);
        var input = Base() with { CustomerAddress = longAddress };
        var payload = IpsQrPayloadBuilder.Build(input);
        var pField = ExtractField(payload, "P:");
        Assert.True(pField.Length <= 70, $"P field too long: {pField.Length}");
    }

    [Fact]
    public void Build_TruncatesSeriesTextTo35Chars()
    {
        var longSeries = new string('s', 50);
        var input = Base() with { SeriesText = longSeries };
        var payload = IpsQrPayloadBuilder.Build(input);
        var sField = ExtractField(payload, "S:");
        // "purpose " + truncated series(35)
        Assert.True(sField.Length <= "Naknada za odrzavanje ".Length + 35, $"S field too long: {sField.Length}");
    }

    [Theory]
    [InlineData("Naziv & partner", "Naziv   partner")]
    [InlineData("A<B>C", "A-B-C")]
    [InlineData("He said \"hi\" it's fine", "He said hi its fine")]
    public void Build_SanitizesSpecialCharactersInName(string raw, string expectedSanitized)
    {
        var input = Base() with { IssuerName = raw };
        var payload = IpsQrPayloadBuilder.Build(input);
        Assert.Contains($"|N:{expectedSanitized}\r\nBeograd|", payload);
    }

    private static string ExtractField(string payload, string fieldPrefix)
    {
        var start = payload.IndexOf('|' + fieldPrefix, StringComparison.Ordinal) + 1 + fieldPrefix.Length;
        var end = payload.IndexOf('|', start);
        if (end < 0) end = payload.Length;
        return payload[start..end];
    }
}
