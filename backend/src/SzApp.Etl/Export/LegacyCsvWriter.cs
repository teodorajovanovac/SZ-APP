using System.Globalization;
using System.Text;

namespace SzApp.Etl.Export;

/// <summary>
/// Writes CSV in exactly the format LegacyCsvParser + GenericRowValidator accept: ';' delimiter,
/// header row, comma decimals, d.M.yyyy dates (d.M.yyyy H:mm:ss when there is a time part),
/// Access booleans -1/0, RFC4180-style quoting. Streams row by row; CRLF line endings for Excel.
/// </summary>
public sealed class LegacyCsvWriter(TextWriter writer)
{
    public const char Delimiter = ';';
    private static readonly CultureInfo SerbianLatin = CultureInfo.GetCultureInfo("sr-Latn-RS");
    private static readonly TimeZoneInfo Belgrade = TimeZoneInfo.FindSystemTimeZoneById("Europe/Belgrade");

    static LegacyCsvWriter() => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

    /// <summary>"cp1250" (default) or "utf8" (with BOM so Excel detects it).</summary>
    public static Encoding ResolveEncoding(string? name, CountingReplacementFallback? fallback = null) =>
        (name ?? "cp1250").Trim().ToLowerInvariant() switch
        {
            "cp1250" or "windows-1250" or "1250" or "" =>
                Encoding.GetEncoding(1250, (EncoderFallback?)fallback ?? new EncoderReplacementFallback("?"), DecoderFallback.ReplacementFallback),
            "utf8" or "utf-8" => new UTF8Encoding(encoderShouldEmitUTF8Identifier: true),
            _ => throw new ArgumentException($"Kodiranje '{name}' nije podržano. Dozvoljeni su cp1250 i utf8.", nameof(name))
        };

    public Task WriteRowAsync(IEnumerable<object?> values, CancellationToken ct = default) =>
        writer.WriteAsync((string.Join(Delimiter, values.Select(x => Quote(Format(x)))) + "\r\n").AsMemory(), ct);

    public static string Format(object? value) => value switch
    {
        null => string.Empty,
        string s => s,
        bool b => b ? "-1" : "0",
        decimal d => d.ToString(SerbianLatin),
        double d => d.ToString("R", SerbianLatin),
        float f => f.ToString("R", SerbianLatin),
        DateOnly d => d.ToString("d.M.yyyy", CultureInfo.InvariantCulture),
        DateTime dt => dt.TimeOfDay == TimeSpan.Zero
            ? dt.ToString("d.M.yyyy", CultureInfo.InvariantCulture)
            : dt.ToString("d.M.yyyy H:mm:ss", CultureInfo.InvariantCulture),
        DateTimeOffset dto => Format(TimeZoneInfo.ConvertTime(dto, Belgrade).DateTime),
        Enum e => e.ToString(),
        IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? string.Empty
    };

    public static string Quote(string field) =>
        field.AsSpan().IndexOfAny(";\"\r\n") >= 0 || field != field.Trim()
            ? "\"" + field.Replace("\"", "\"\"") + "\""
            : field;
}

/// <summary>Replaces characters the target code page can't represent with '?' and counts them.</summary>
public sealed class CountingReplacementFallback : EncoderFallback
{
    private int count;
    public int Count => count;
    public override int MaxCharCount => 1;
    public override EncoderFallbackBuffer CreateFallbackBuffer() => new Buffer(this);

    private sealed class Buffer(CountingReplacementFallback owner) : EncoderFallbackBuffer
    {
        private bool pending;
        public override int Remaining => pending ? 1 : 0;

        public override bool Fallback(char charUnknown, int index) => Mark();
        public override bool Fallback(char high, char low, int index) => Mark();

        public override char GetNextChar()
        {
            if (!pending) return '\0';
            pending = false;
            return '?';
        }

        public override bool MovePrevious() => false;
        public override void Reset() => pending = false;

        private bool Mark()
        {
            Interlocked.Increment(ref owner.count);
            pending = true;
            return true;
        }
    }
}
