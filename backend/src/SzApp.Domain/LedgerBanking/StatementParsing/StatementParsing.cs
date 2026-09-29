using System.Globalization;
using System.Text;
using System.Xml.Linq;

namespace SzApp.Domain.LedgerBanking.StatementParsing;

public sealed record ParsedStatementLine(
    int LineNumber,
    string PayerName,
    string? PayerAccount,
    string? Info,
    int? Code,
    string? PaymentReference,
    decimal Debit,
    decimal Credit);

/// <summary>
/// One bank statement as read from a file (legacy cIzvod). Header totals/counts are null when the
/// format does not carry them; balances null = not in the file (170 TXT) -- the caller fills them
/// from the previous statement. StatementNumber 0 = not in the file.
/// </summary>
public sealed record ParsedStatement(
    int BankCode,
    string? AccountNumber,
    int StatementNumber,
    DateOnly Date,
    decimal? PreviousBalance,
    decimal? NewBalance,
    decimal? DeclaredDebit,
    decimal? DeclaredCredit,
    int? DeclaredCount,
    IReadOnlyList<ParsedStatementLine> Lines);

public interface IBankStatementParser
{
    /// <summary>Legacy bank code (first 3 digits of the account), e.g. 160 Intesa.</summary>
    int BankCode { get; }
    string Name { get; }
    bool IsSupported { get; }
    /// <summary>Legacy <c>IzvodIzBanke</c> detection by file name.</summary>
    bool MatchesFileName(string fileName);
    ParsedStatement Parse(string fileName, byte[] content);
}

/// <summary>String cleanup shared by all parsers (legacy cIzvod helpers, minus their bugs).</summary>
public static class StatementText
{
    private static readonly string[] DateFormats =
    [
        "d.M.yyyy", "d.M.yyyy.", "dd.MM.yyyy", "dd.MM.yyyy.", "d.M.yyyy HH:mm:ss", "dd.MM.yyyy HH:mm:ss",
        "yyyy-MM-dd", "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-ddTHH:mm:ss.fff", "yyyyMMdd", "yyyyMMddHHmmss"
    ];

    /// <summary>
    /// Money from a bank file. Legacy <c>DodeliVrednostNovca</c> read "12.5" as 12,05 (it added the
    /// fraction as cents) -- here a single '.' or ',' is always the decimal separator, a repeated
    /// one or the earlier of the two is a thousands separator.
    /// </summary>
    public static decimal ParseAmount(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return 0m;
        var s = raw.Trim().Replace(" ", string.Empty).Replace(" ", string.Empty);
        var lastDot = s.LastIndexOf('.');
        var lastComma = s.LastIndexOf(',');
        char? decimalSeparator = null;
        if (lastDot >= 0 && lastComma >= 0) decimalSeparator = lastDot > lastComma ? '.' : ',';
        else if (lastDot >= 0 && s.IndexOf('.') == lastDot) decimalSeparator = '.';
        else if (lastComma >= 0 && s.IndexOf(',') == lastComma) decimalSeparator = ',';

        var cut = decimalSeparator is { } sep ? s.LastIndexOf(sep) : -1;
        var integer = (cut >= 0 ? s[..cut] : s).Replace(".", string.Empty).Replace(",", string.Empty);
        var fraction = cut >= 0 ? s[(cut + 1)..] : string.Empty;
        var normalized = fraction.Length > 0 ? $"{integer}.{fraction}" : integer;
        if (!decimal.TryParse(normalized, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var value))
        {
            throw new DomainRuleException("statement.parse-amount", $"Neispravan iznos u izvodu: '{raw}'.");
        }

        return value;
    }

    /// <summary>Legacy <c>DodeliVrednostDatuma</c> (yyyyMMdd after removing ' ' and '-') plus dd.MM.yyyy.</summary>
    public static DateOnly ParseDate(string? raw)
    {
        var s = raw?.Trim() ?? string.Empty;
        var compact = s.Replace(" ", string.Empty).Replace("-", string.Empty);
        if (compact.Length >= 8 && compact[..8].All(char.IsAsciiDigit) &&
            DateOnly.TryParseExact(compact[..8], "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var compactDate))
        {
            return compactDate;
        }

        if (DateTime.TryParseExact(s, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            return DateOnly.FromDateTime(parsed);
        }

        throw new DomainRuleException("statement.parse-date", $"Neispravan datum u izvodu: '{raw}'.");
    }

    public static int ParseInt(string? raw, string what)
    {
        var digits = new string((raw ?? string.Empty).Trim().TakeWhile(char.IsAsciiDigit).ToArray());
        return int.TryParse(digits, NumberStyles.None, CultureInfo.InvariantCulture, out var value)
            ? value
            : throw new DomainRuleException("statement.parse-number", $"Neispravan {what} u izvodu: '{raw}'.");
    }

    /// <summary>Payment code (šifra plaćanja): first run of digits, at most 3 (legacy VratiSifru).</summary>
    public static int? ParseCode(string? raw)
    {
        var digits = new string((raw ?? string.Empty).SkipWhile(c => !char.IsAsciiDigit(c)).TakeWhile(char.IsAsciiDigit).Take(3).ToArray());
        return digits.Length == 0 ? null : int.Parse(digits, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Serbian account to the legacy "bbb-n-cc" form with leading zeros of the middle part removed
    /// (legacy RacunRemovePreviseNula / TR_DodeliStilTR). 18 digits without dashes are split 3/13/2.
    /// Anything with letters (IBAN, foreign) is returned trimmed.
    /// </summary>
    public static string? NormalizeAccount(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var s = raw.Trim();
        if (s.Any(char.IsLetter)) return s;
        string bank, account, control;
        if (s.Contains('-'))
        {
            var parts = s.Split('-', StringSplitOptions.TrimEntries);
            if (parts.Length != 3) return s;
            (bank, account, control) = (parts[0], parts[1], parts[2]);
        }
        else
        {
            var digits = new string(s.Where(char.IsAsciiDigit).ToArray());
            if (digits.Length < 6) return s;
            (bank, account, control) = (digits[..3], digits[3..^2], digits[^2..]);
        }

        account = account.TrimStart('0');
        return $"{bank}-{(account.Length == 0 ? "0" : account)}-{control}";
    }

    /// <summary>Serbian account control digits: 98 - (bank+account digits * 100 mod 97) (ISO 7064 MOD 97-10).</summary>
    public static string ControlDigits(string bank, string account)
    {
        var number = bank + account.PadLeft(13, '0');
        var remainder = 0;
        foreach (var digit in number + "00") remainder = (remainder * 10 + (digit - '0')) % 97;
        return (98 - remainder).ToString("00", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Legacy <c>OcistiPozivNaBroj</c>: 275 strips "PBO-97"/"PBO-" and leading zeros; 325 strips
    /// "PBO-" and the "(97)" model prefix; every bank removes ' ', '-', '/', '\'. Model 97 is not
    /// verified (same as legacy).
    /// </summary>
    public static string? CleanPaymentReference(string? raw, int bankCode)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var s = raw;
        if (bankCode == 275)
        {
            s = s.Replace("PBO-97", string.Empty).Replace("PBO-", string.Empty).TrimStart('0');
        }
        else if (bankCode == 325)
        {
            s = s.Replace("PBO-", string.Empty);
            var open = s.IndexOf('(');
            var close = s.IndexOf(')');
            if (open >= 0 && close > open) s = s[(close + 1)..];
        }

        s = s.Replace(" ", string.Empty).Replace("-", string.Empty).Replace("/", string.Empty).Replace("\\", string.Empty);
        if (s.Length == 0) return null;
        return s.Length > 50 ? s[..50] : s;
    }

    public static string Clip(string? value, int max)
    {
        var s = value?.Trim() ?? string.Empty;
        return s.Length > max ? s[..max] : s;
    }

    public static string? ClipOrNull(string? value, int max)
    {
        var s = Clip(value, max);
        return s.Length == 0 ? null : s;
    }

    /// <summary>Text files: UTF-8 when valid, otherwise Windows-1250 (legacy bank exports).</summary>
    public static string DecodeText(byte[] content)
    {
        try
        {
            return new UTF8Encoding(false, true).GetString(content).TrimStart('﻿');
        }
        catch (DecoderFallbackException)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            return Encoding.GetEncoding(1250).GetString(content);
        }
    }

    public static XDocument LoadXml(byte[] content)
    {
        try
        {
            using var stream = new MemoryStream(content);
            return XDocument.Load(stream);
        }
        catch (System.Xml.XmlException exception)
        {
            throw new DomainRuleException("statement.invalid-xml", $"Fajl nije ispravan XML: {exception.Message}");
        }
    }
}

internal static class XmlExtensions
{
    public static XElement? Find(this XContainer container, string localName) =>
        container.Descendants().FirstOrDefault(x => x.Name.LocalName == localName);

    public static XElement Require(this XContainer container, string localName) =>
        container.Find(localName) ?? throw new DomainRuleException("statement.missing-element", $"U izvodu nedostaje element <{localName}>.");

    public static string? Text(this XContainer container, string localName) => container.Find(localName)?.Value;

    public static IEnumerable<XElement> Children(this XContainer container, string localName) =>
        container.Elements().Where(x => x.Name.LocalName == localName);

    public static string? Attr(this XElement element, string name) => element.Attribute(name)?.Value;

    public static string RequireAttr(this XElement element, string name) =>
        element.Attribute(name)?.Value ?? throw new DomainRuleException("statement.missing-attribute", $"U izvodu nedostaje atribut {name}.");
}
