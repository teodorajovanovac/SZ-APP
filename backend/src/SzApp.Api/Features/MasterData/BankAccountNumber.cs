namespace SzApp.Api.Features.MasterData;

/// <summary>
/// Port of the legacy VBA TR_Formatiraj: normalises a Serbian bank account number to
/// "XXX-NNNNNNNNNNNNN-KK" (3-digit bank, middle part left-padded with zeros to 13, 2-digit check).
/// The mod-97 check-digit control is done by the frontend; this only validates the shape.
/// </summary>
public static class BankAccountNumber
{
    public static bool TryFormat(string? input, out string formatted)
    {
        formatted = input?.Trim() ?? string.Empty;
        if (formatted.Length == 0)
        {
            return false;
        }

        var text = formatted;
        var start = text.IndexOf('-');
        var end = start < 0 ? -1 : text.IndexOf('-', start + 1);
        string bank, middle, check;

        if (start >= 0 && end > 0)
        {
            var middleLength = end - start - 1;
            if (start != 3 || middleLength is < 1 or > 13 || text.Length - end - 1 != 2 ||
                text.Length is < 8 or > 20 || !AllDigits(text.Replace("-", "")))
            {
                return false;
            }
            bank = text[..3];
            middle = text.Substring(4, text.Length - 7);
            check = text[^2..];
        }
        else
        {
            if (text.Length is < 6 or > 18 || !AllDigits(text))
            {
                return false;
            }
            bank = text[..3];
            middle = text.Substring(3, text.Length - 5);
            check = text[^2..];
        }

        formatted = $"{bank}-{middle.PadLeft(13, '0')}-{check}";
        return true;
    }

    private static bool AllDigits(string value) => value.Length > 0 && value.All(char.IsAsciiDigit);
}
