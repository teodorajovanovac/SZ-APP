using System.Globalization;

namespace SzApp.Etl.Parsing;

public static class SerbianLegacyValueParser
{
    private static readonly CultureInfo SerbianLatin = CultureInfo.GetCultureInfo("sr-Latn-RS");
    private static readonly string[] DateFormats = ["d.M.yyyy", "dd.MM.yyyy", "d.M.yyyy.", "dd.MM.yyyy."];

    public static decimal ParseDecimal(string value)
    {
        var trimmed = value.Trim();
        if (decimal.TryParse(
                trimmed,
                NumberStyles.Number | NumberStyles.AllowLeadingSign,
                SerbianLatin,
                out var result))
        {
            return result;
        }

        // ponytail (#7): Access/Excel exports sometimes render negatives as "(123,45)" instead of
        // "-123,45". NumberStyles.AllowParentheses exists but doesn't compose with AllowLeadingSign,
        // so handle it explicitly rather than pulling in a bigger parsing dependency for one format.
        if (trimmed is ['(', .., ')'] &&
            decimal.TryParse(trimmed[1..^1], NumberStyles.Number, SerbianLatin, out var negated))
        {
            return -negated;
        }

        throw new FormatException($"'{value}' nije ispravan sr-Latn decimalni broj.");
    }

    public static DateOnly ParseDate(string value)
    {
        if (DateOnly.TryParseExact(
                value.Trim(),
                DateFormats,
                SerbianLatin,
                DateTimeStyles.AllowWhiteSpaces,
                out var result))
        {
            return result;
        }

        throw new FormatException($"'{value}' nije datum u formatu d.M.yyyy.");
    }

    public static bool ParseAccessBoolean(string value) => value.Trim().ToLowerInvariant() switch
    {
        "-1" or "true" or "da" or "yes" => true,
        "0" or "false" or "ne" or "no" => false,
        _ => throw new FormatException($"'{value}' nije Access boolean (-1/0).")
    };

    public static string? ZeroToNull(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrEmpty(normalized) || normalized == "0" ? null : normalized;
    }

    public static int? ParseNullableForeignKey(string? value)
    {
        var normalized = ZeroToNull(value);
        return normalized is null
            ? null
            : int.TryParse(normalized, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
                ? result
                : throw new FormatException($"'{value}' nije ispravan strani ključ.");
    }
}
