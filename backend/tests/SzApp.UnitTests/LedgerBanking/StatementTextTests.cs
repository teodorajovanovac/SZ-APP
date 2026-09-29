using SzApp.Domain.LedgerBanking.StatementParsing;

namespace SzApp.UnitTests.LedgerBanking;

public sealed class StatementTextTests
{
    // Legacy DodeliVrednostNovca bug: "12.5" was read as 12,05 because it treated the part after
    // '.' as cents without padding. The new parser must NOT reproduce that.
    [Theory]
    [InlineData("12.5", 12.5)]
    [InlineData("12,5", 12.5)]
    [InlineData("1.234,56", 1234.56)]
    [InlineData("1,234.56", 1234.56)]
    [InlineData("1234.56", 1234.56)]
    [InlineData("1234,56", 1234.56)]
    [InlineData("1000", 1000)]
    [InlineData("0.00", 0)]
    [InlineData("", 0)]
    public void ParseAmount_HandlesSerbianAndInternationalSeparators(string raw, decimal expected)
    {
        Assert.Equal(expected, StatementText.ParseAmount(raw));
    }

    [Fact]
    public void ParseAmount_NeverProducesTheLegacy12Point5To12Comma05Bug()
    {
        Assert.Equal(12.5m, StatementText.ParseAmount("12.5"));
        Assert.NotEqual(12.05m, StatementText.ParseAmount("12.5"));
    }

    [Theory]
    [InlineData("20220118", "2022-01-18")]
    [InlineData("18.01.2022", "2022-01-18")]
    [InlineData("18.1.2022", "2022-01-18")]
    [InlineData("2022-01-18", "2022-01-18")]
    public void ParseDate_HandlesLegacyAndIsoFormats(string raw, string expectedIso)
    {
        Assert.Equal(DateOnly.Parse(expectedIso), StatementText.ParseDate(raw));
    }

    [Theory]
    [InlineData("160-0000026-95", "160-26-95")]
    [InlineData("160-26-95", "160-26-95")]
    public void NormalizeAccount_StripsLeadingZerosOfMiddlePart(string raw, string expected)
    {
        Assert.Equal(expected, StatementText.NormalizeAccount(raw));
    }

    [Fact]
    public void NormalizeAccount_SplitsAnEighteenDigitAccountAsBankAccountControl()
    {
        // legacy RacunRemovePreviseNula: 3 (bank) + 13 (account, zero-stripped) + 2 (control).
        Assert.Equal("170-3000988400-08", StatementText.NormalizeAccount("170300098840008"));
    }

    [Fact]
    public void NormalizeAccount_KeepsIbanAndForeignAccountsAsIs()
    {
        Assert.Equal("RS35160005070000123456", StatementText.NormalizeAccount("RS35160005070000123456"));
    }

    // 9.3 / legacy OcistiPozivNaBroj: 275 strips PBO-97 / PBO- + leading zeros; 325 strips PBO- and
    // the "(97)" model prefix; every bank strips space/-/\//\.
    [Theory]
    [InlineData(275, "PBO-97001234", "1234")]
    [InlineData(275, "PBO-0001234", "1234")]
    [InlineData(325, "PBO-(97)00-12-34", "001234")]
    [InlineData(160, "97-123-45", "9712345")]
    [InlineData(200, "12 34-56/78", "12345678")]
    public void CleanPaymentReference_MatchesLegacyOcistiPozivNaBroj(int bank, string raw, string expected)
    {
        Assert.Equal(expected, StatementText.CleanPaymentReference(raw, bank));
    }

    [Fact]
    public void CleanPaymentReference_EmptyResultBecomesNull()
    {
        Assert.Null(StatementText.CleanPaymentReference("PBO-97000000", 275));
    }
}
