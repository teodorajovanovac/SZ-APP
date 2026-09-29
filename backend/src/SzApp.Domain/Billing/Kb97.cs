namespace SzApp.Domain.Billing;

/// <summary>
/// ISO 7064 MOD 97-10 check digits (legacy KB97), used for the payment reference (9.1/9.5):
/// digits as-is, letters A=10..Z=35, control = 98 − (numeral × 100 mod 97), 2 digits.
/// Separators ('-', spaces) are ignored, matching the legacy implementation.
/// </summary>
// ponytail: not yet deduped with the invoice-engine agent's copy (concurrent work) -- lead merges.
public static class Kb97
{
    public static string Compute(string raw)
    {
        var acc = 0L;
        foreach (var ch in raw)
        {
            if (char.IsAsciiDigit(ch))
            {
                acc = Append(acc, ch - '0');
            }
            else if (char.IsAsciiLetter(ch))
            {
                var value = 10 + (char.ToUpperInvariant(ch) - 'A');
                acc = Append(acc, value / 10);
                acc = Append(acc, value % 10);
            }
        }
        acc = Append(acc, 0);
        acc = Append(acc, 0);
        return (98 - acc).ToString("D2");
    }

    /// <summary>KB97(raw) & "-" & raw, as used for RBR poziv na broj and the notice payment reference.</summary>
    public static string WithControl(string raw) => $"{Compute(raw)}-{raw}";

    private static long Append(long acc, int digit) => (acc * 10 + digit) % 97;
}
