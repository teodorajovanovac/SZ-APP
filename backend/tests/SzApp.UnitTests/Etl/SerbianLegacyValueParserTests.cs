using SzApp.Etl.Parsing;

namespace SzApp.UnitTests.Etl;

public sealed class SerbianLegacyValueParserTests
{
    [Theory]
    [InlineData("2966,04", 2966.04)]
    [InlineData("-15,5", -15.5)]
    [InlineData("0", 0)]
    [InlineData("  100,00  ", 100.00)]
    public void ParseDecimal_ParsesSerbianCommaDecimals(string raw, double expected) =>
        Assert.Equal((decimal)expected, SerbianLegacyValueParser.ParseDecimal(raw));

    [Theory]
    [InlineData("(123,45)", -123.45)]
    [InlineData("(10)", -10)]
    public void ParseDecimal_ParsesNegativeInParentheses(string raw, double expected) =>
        Assert.Equal((decimal)expected, SerbianLegacyValueParser.ParseDecimal(raw));

    [Fact]
    public void ParseDecimal_ThrowsFormatException_ForGarbage() =>
        Assert.Throws<FormatException>(() => SerbianLegacyValueParser.ParseDecimal("nije broj"));

    [Theory]
    [InlineData("5.3.2024", 2024, 3, 5)]
    [InlineData("05.03.2024.", 2024, 3, 5)]
    [InlineData("31.12.2025", 2025, 12, 31)]
    public void ParseDate_ParsesSerbianDayMonthYear(string raw, int year, int month, int day) =>
        Assert.Equal(new DateOnly(year, month, day), SerbianLegacyValueParser.ParseDate(raw));

    [Fact]
    public void ParseDate_ThrowsFormatException_ForIsoFormat() =>
        // legacy exports are d.M.yyyy, not ISO — an ISO-shaped value should not silently parse
        Assert.Throws<FormatException>(() => SerbianLegacyValueParser.ParseDate("2024-03-05"));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("0")]
    public void ZeroToNull_TreatsBlankAndZeroAsNull(string? raw) =>
        Assert.Null(SerbianLegacyValueParser.ZeroToNull(raw));

    [Fact]
    public void ZeroToNull_KeepsNonZeroValue() =>
        Assert.Equal("42", SerbianLegacyValueParser.ZeroToNull(" 42 "));

    [Theory]
    [InlineData("-1", true)]
    [InlineData("true", true)]
    [InlineData("da", true)]
    [InlineData("0", false)]
    [InlineData("false", false)]
    [InlineData("ne", false)]
    public void ParseAccessBoolean_ParsesKnownRepresentations(string raw, bool expected) =>
        Assert.Equal(expected, SerbianLegacyValueParser.ParseAccessBoolean(raw));

    [Fact]
    public void ParseNullableForeignKey_TreatsZeroAsNull() =>
        Assert.Null(SerbianLegacyValueParser.ParseNullableForeignKey("0"));

    [Fact]
    public void ParseNullableForeignKey_ParsesPositiveInteger() =>
        Assert.Equal(17, SerbianLegacyValueParser.ParseNullableForeignKey("17"));
}
