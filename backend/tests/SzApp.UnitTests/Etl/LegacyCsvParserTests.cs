using System.Text;
using SzApp.Etl.Csv;

namespace SzApp.UnitTests.Etl;

public sealed class LegacyCsvParserTests
{
    private readonly LegacyCsvParser _parser = new();

    [Fact]
    public async Task ParseAsync_RoundTripsWindows1250SerbianCharacters()
    {
        const string name = "Đorđević Šumadija Ćoškić Žikić Čačak";
        var csv = $"Id;Name\r\n1;{name}\r\n";
        await using var stream = new MemoryStream(LegacyEncoding.Resolve("windows-1250").GetBytes(csv));

        var document = await _parser.ParseAsync(stream, "windows-1250", ';');

        Assert.Single(document.Rows);
        Assert.Equal(name, document.Rows[0].Values["Name"]);
    }

    [Fact]
    public async Task ParseAsync_QuarantinesStructuralMismatch_AndKeepsParsingRestOfFile()
    {
        // row 2 has one field too few (a missing ";") — must not abort rows 1 and 3 (item #5)
        const string csv = "Id;Name;Amount\r\n1;First;10\r\n2;OnlyTwoFields\r\n3;Third;30\r\n";
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        var document = await _parser.ParseAsync(stream, "utf-8", ';');

        Assert.Equal(3, document.Rows.Count);
        Assert.Null(document.Rows[0].StructuralError);
        Assert.NotNull(document.Rows[1].StructuralError);
        Assert.Empty(document.Rows[1].Values);
        Assert.Null(document.Rows[2].StructuralError);
        Assert.Equal("Third", document.Rows[2].Values["Name"]);
    }

    [Fact]
    public async Task ParseAsync_ThrowsForMissingHeader()
    {
        await using var stream = new MemoryStream();
        await Assert.ThrowsAsync<InvalidDataException>(() => _parser.ParseAsync(stream, "utf-8", ';'));
    }
}
