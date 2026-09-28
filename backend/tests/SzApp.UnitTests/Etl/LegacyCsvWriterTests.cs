using System.Text;
using SzApp.Etl.Csv;
using SzApp.Etl.Export;
using SzApp.Etl.Parsing;
using SzApp.Etl.Pipeline;

namespace SzApp.UnitTests.Etl;

public sealed class LegacyCsvWriterTests
{
    private static async Task<byte[]> WriteAsync(string encodingName, params object?[][] rows)
    {
        using var buffer = new MemoryStream();
        await using (var text = new StreamWriter(buffer, LegacyCsvWriter.ResolveEncoding(encodingName), leaveOpen: true))
        {
            var writer = new LegacyCsvWriter(text);
            foreach (var row in rows) await writer.WriteRowAsync(row);
        }
        return buffer.ToArray();
    }

    [Fact]
    public async Task Cp1250_WritesSerbianLettersAsSingleBytes()
    {
        var bytes = await WriteAsync("cp1250", ["šđčćž ŠĐČĆŽ"]);
        Assert.Equal(new byte[] { 0x9A, 0xF0, 0xE8, 0xE6, 0x9E, 0x20, 0x8A, 0xD0, 0xC8, 0xC6, 0x8E, 0x0D, 0x0A }, bytes);
    }

    [Fact]
    public async Task Utf8_StartsWithBom()
    {
        var bytes = await WriteAsync("utf8", ["š"]);
        Assert.Equal(new byte[] { 0xEF, 0xBB, 0xBF, 0xC5, 0xA1, 0x0D, 0x0A }, bytes);
    }

    [Fact]
    public void Cp1250_CountsUnrepresentableCharacters()
    {
        var fallback = new CountingReplacementFallback();
        using var buffer = new MemoryStream();
        using (var text = new StreamWriter(buffer, LegacyCsvWriter.ResolveEncoding("cp1250", fallback), leaveOpen: true))
            text.Write("Жš");
        Assert.Equal(new byte[] { (byte)'?', 0x9A }, buffer.ToArray());
        Assert.Equal(1, fallback.Count);
    }

    [Theory]
    [InlineData(2966.04, "2966,04")]
    [InlineData(-1234567.5, "-1234567,5")]
    [InlineData(0, "0")]
    public void Format_DecimalUsesCommaWithoutGrouping(double value, string expected) =>
        Assert.Equal(expected, LegacyCsvWriter.Format((decimal)value));

    [Fact]
    public void Format_DatesBooleansAndNulls()
    {
        Assert.Equal("5.2.2026", LegacyCsvWriter.Format(new DateOnly(2026, 2, 5)));
        Assert.Equal("5.2.2026", LegacyCsvWriter.Format(new DateTime(2026, 2, 5)));
        Assert.Equal("5.2.2026 7:03:09", LegacyCsvWriter.Format(new DateTime(2026, 2, 5, 7, 3, 9)));
        // UTC 23:30 on 4.2. is 00:30 on 5.2. in Belgrade (CET, +1).
        Assert.Equal("5.2.2026 0:30:00", LegacyCsvWriter.Format(new DateTimeOffset(2026, 2, 4, 23, 30, 0, TimeSpan.Zero)));
        Assert.Equal("-1", LegacyCsvWriter.Format(true));
        Assert.Equal("0", LegacyCsvWriter.Format(false));
        Assert.Equal("", LegacyCsvWriter.Format(null));
        Assert.Equal("1234567", LegacyCsvWriter.Format(1234567));
    }

    [Theory]
    [InlineData("plain", "plain")]
    [InlineData("a;b", "\"a;b\"")]
    [InlineData("say \"hi\"", "\"say \"\"hi\"\"\"")]
    [InlineData("line1\r\nline2", "\"line1\r\nline2\"")]
    public void Quote_EscapesSpecialCharacters(string input, string expected) =>
        Assert.Equal(expected, LegacyCsvWriter.Quote(input));

    [Fact]
    public void ParseDate_AcceptsExportedDateTime()
    {
        Assert.Equal(new DateOnly(2026, 2, 5), SerbianLegacyValueParser.ParseDate("5.2.2026 7:03:09"));
        Assert.Equal(new DateOnly(2026, 2, 5), SerbianLegacyValueParser.ParseDate("5.2.2026. 7:03"));
    }

    [Theory]
    [InlineData("cp1250", "windows-1250")]
    [InlineData("utf8", "utf-8")]
    public async Task RoundTrip_WriterOutputParsesAndValidatesWithoutQuarantine(string exportEncoding, string importEncoding)
    {
        object?[] header = ["Id", "Name", "Note", "IssueDate", "EntryDate", "Amount", "IsActive", "PartnerId", "K1"];
        object?[] row1 = [1, "Đorđević; \"Žika\"", "red1\r\nred2", new DateOnly(2026, 2, 5),
            new DateTimeOffset(2026, 2, 5, 10, 15, 0, TimeSpan.Zero), -2966.04m, true, null, 64.5m];
        object?[] row2 = [2, "Čačak ćošak", "", null, null, 0m, false, 7, null];
        var bytes = await WriteAsync(exportEncoding, header, row1, row2);

        await using var stream = new MemoryStream(bytes);
        var document = await new LegacyCsvParser().ParseAsync(stream, importEncoding);

        Assert.Equal(header.Cast<string>(), document.Headers);
        Assert.Equal(2, document.Rows.Count);
        Assert.All(document.Rows, row =>
        {
            Assert.Null(row.StructuralError);
            Assert.Empty(GenericRowValidator.Validate(row.Values));
        });
        var first = document.Rows[0].Values;
        Assert.Equal("Đorđević; \"Žika\"", first["Name"]);
        Assert.Equal("red1\r\nred2", first["Note"]);
        Assert.Equal(-2966.04m, SerbianLegacyValueParser.ParseDecimal(first["Amount"]));
        Assert.Equal(new DateOnly(2026, 2, 5), SerbianLegacyValueParser.ParseDate(first["EntryDate"]));
        Assert.True(SerbianLegacyValueParser.ParseAccessBoolean(first["IsActive"]));
        Assert.Null(SerbianLegacyValueParser.ParseNullableForeignKey(first["PartnerId"]));
        Assert.Equal("Čačak ćošak", document.Rows[1].Values["Name"]);
    }
}
