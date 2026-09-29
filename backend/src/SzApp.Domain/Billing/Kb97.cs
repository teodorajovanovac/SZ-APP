using System.Numerics;
using System.Text;

namespace SzApp.Domain.Billing;

/// <summary>
/// KB97 control number (ISO 7064 MOD 97-10), used as the "poziv na broj" checksum
/// prefix on invoices (RBR = "{SZ}-{ID_K}-{YYMM}[-{marker}]") and notices.
/// Ported from legacy modKontrolniBroj — digits pass through, letters map A=10..Z=35,
/// separators are ignored. control = 98 - ((number * 100) mod 97), always 2 digits.
/// </summary>
public static class Kb97
{
    public static string Compute(string reference)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reference);

        var digits = new StringBuilder();
        foreach (var ch in reference)
        {
            if (char.IsAsciiDigit(ch))
            {
                digits.Append(ch);
            }
            else if (char.IsAsciiLetter(ch))
            {
                digits.Append(char.ToUpperInvariant(ch) - 'A' + 10);
            }
            // separators (e.g. '-') are ignored
        }

        if (digits.Length == 0)
        {
            throw new ArgumentException("Reference must contain at least one digit or letter.", nameof(reference));
        }

        var number = BigInteger.Parse(digits.ToString());
        var control = 98 - (int)(number * 100 % 97);
        return control.ToString("D2");
    }

    /// <summary>Poziv na broj = KB97(RBR) & "-" & RBR.</summary>
    public static string PaymentReference(string rbr) => $"{Compute(rbr)}-{rbr}";
}
