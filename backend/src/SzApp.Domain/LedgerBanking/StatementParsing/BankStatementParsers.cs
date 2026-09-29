using System.Xml.Linq;

namespace SzApp.Domain.LedgerBanking.StatementParsing;

/// <summary>
/// Parsers for the legacy <c>cIzvod</c> formats (9.3). All of them are written from the VBA only --
/// no real bank file exists in the repo yet (P16), so every one is "unverified against a real file".
/// </summary>
public static class BankStatementParsers
{
    public static IReadOnlyList<IBankStatementParser> All { get; } =
    [
        new OfxStatementParser(160, "Banca Intesa (XML)", "refnumber",
            name => name.Contains("[160", StringComparison.Ordinal),
            name => Between(name, '[', ']'), null),
        new UniCreditTxtParser(),
        new UnsupportedParser(180, "Alpha banka (TXT)", name => name.StartsWith('I') && name.Length == 16),
        new PostanskaXmlParser(),
        new OfxStatementParser(205, "Komercijalna banka (XML)", "refnumber",
            name => name.IndexOf(" 205", StringComparison.Ordinal) == 8,
            name => Field(Stem(name), ' ', 1), name => StatementText.ParseDate(Field(Stem(name), ' ', 0))),
        new RaiffeisenXmlParser(),
        new SocieteGeneraleXmlParser(),
        new UnsupportedParser(325, "OTP banka (XML V1/V2)", name => name.IndexOf("325", StringComparison.Ordinal) == 5 && name.Length > 28),
        new OfxStatementParser(330, "Credit Agricole (XML)", "payeerefnumber",
            name => name.IndexOf("_330", StringComparison.Ordinal) == 5 && name.Length == 37,
            name => Field(name, '_', 1), null),
        new UnsupportedParser(9001, "Asseco Office Banking (XML)", _ => false),
        new UnsupportedParser(9002, "Poštanska štedionica KS2IZVPLKK", _ => false),
    ];

    /// <summary>Chosen format wins; otherwise legacy file-name detection (IzvodIzBanke).</summary>
    public static IBankStatementParser Resolve(string fileName, int? bankCode)
    {
        if (bankCode is { } code)
        {
            return All.FirstOrDefault(x => x.BankCode == code)
                   ?? throw new DomainRuleException("statement.unknown-format", $"Nepoznat format izvoda {code}.");
        }

        return All.FirstOrDefault(x => x.MatchesFileName(fileName))
               ?? throw new DomainRuleException("statement.format-not-detected", "Banka nije prepoznata po imenu fajla — izaberite format ručno.");
    }

    public static ParsedStatement Parse(string fileName, byte[] content, int? bankCode = null)
    {
        var parser = Resolve(fileName, bankCode);
        if (!parser.IsSupported)
        {
            throw new DomainRuleException("statement.format-not-supported", $"Format nije podržan: {parser.Name}.");
        }

        var parsed = parser.Parse(fileName, content);
        if (parsed.Lines.Count == 0)
        {
            throw new DomainRuleException("statement.empty", "Izvod nema nijednu stavku.");
        }

        return parsed;
    }

    internal static string Stem(string fileName) => Path.GetFileNameWithoutExtension(fileName);

    internal static string? Field(string value, char separator, int index)
    {
        var parts = value.Split(separator);
        return index < parts.Length ? parts[index] : null;
    }

    internal static string? Between(string value, char open, char close)
    {
        var start = value.IndexOf(open);
        var end = start < 0 ? -1 : value.IndexOf(close, start + 1);
        return end > start ? value[(start + 1)..end] : null;
    }

    internal static ParsedStatementLine Line(int number, int bankCode, string? name, string? account, string? info, int? code, string? reference, decimal debit, decimal credit) =>
        new(number,
            StatementText.Clip(name, 255),
            StatementText.ClipOrNull(StatementText.NormalizeAccount(account), 50),
            StatementText.ClipOrNull(info, 255),
            code,
            StatementText.CleanPaymentReference(reference, bankCode),
            debit,
            credit);
}

internal sealed class UnsupportedParser(int bankCode, string name, Func<string, bool> matches) : IBankStatementParser
{
    public int BankCode => bankCode;
    public string Name => name;
    public bool IsSupported => false;
    public bool MatchesFileName(string fileName) => matches(fileName);
    public ParsedStatement Parse(string fileName, byte[] content) =>
        throw new DomainRuleException("statement.format-not-supported", $"Format nije podržan: {name}.");
}

/// <summary>
/// 160 Intesa / 205 Komercijalna / 330 Credit Agricole share one OFX-like XML (legacy ImportIzvod_160/205/330):
/// root pmtnotification|stmtrs, stmtnumber, ledgerbal/balamt = previous, availbal/balamt = new,
/// trnlist@count, trnlist/stmttrn lines. Legacy dropped the last character of payeeinfo/name; the VBA
/// doesn't show it is a delimiter, so the name is only trimmed here.
/// </summary>
internal sealed class OfxStatementParser(
    int bankCode,
    string name,
    string referenceField,
    Func<string, bool> matches,
    Func<string, string?> fileAccount,
    Func<string, DateOnly>? fileDate) : IBankStatementParser
{
    public int BankCode => bankCode;
    public string Name => name;
    public bool IsSupported => true;
    public bool MatchesFileName(string fileName) => matches(fileName);

    public ParsedStatement Parse(string fileName, byte[] content)
    {
        var doc = StatementText.LoadXml(content);
        var root = doc.Find("pmtnotification") ?? doc.Require("stmtrs");
        var list = root.Require("trnlist");
        var lines = list.Children("stmttrn").Select((x, i) =>
        {
            var amount = StatementText.ParseAmount(x.Text("trnamt"));
            var credit = string.Equals(x.Text("benefit")?.Trim(), "credit", StringComparison.OrdinalIgnoreCase);
            return BankStatementParsers.Line(i + 1, bankCode,
                x.Find("payeeinfo")?.Text("name"),
                x.Find("payeeaccountinfo")?.Text("acctid"),
                x.Text("purpose"),
                StatementText.ParseCode(x.Text("purposecode")),
                x.Text(referenceField),
                credit ? 0m : amount,
                credit ? amount : 0m);
        }).ToArray();

        var count = list.Attr("count");
        return new ParsedStatement(
            bankCode,
            fileAccount(Path.GetFileName(fileName)),
            StatementText.ParseInt(root.Text("stmtnumber"), "broj izvoda"),
            fileDate?.Invoke(Path.GetFileName(fileName)) ?? StatementText.ParseDate(root.Text("dtasof")),
            StatementText.ParseAmount(root.Require("ledgerbal").Text("balamt")),
            StatementText.ParseAmount(root.Require("availbal").Text("balamt")),
            null,
            null,
            count is null ? null : StatementText.ParseInt(count, "broj stavki"),
            lines);
    }
}

/// <summary>200 Poštanska štedionica XML (legacy ImportIzvod_200): Izvod + Prom lines; account/date from file name.</summary>
internal sealed class PostanskaXmlParser : IBankStatementParser
{
    public int BankCode => 200;
    public string Name => "Poštanska štedionica (XML)";
    public bool IsSupported => true;
    public bool MatchesFileName(string fileName) => fileName.StartsWith("Izvod_200", StringComparison.Ordinal) && fileName.Length == 44;

    public ParsedStatement Parse(string fileName, byte[] content)
    {
        var root = StatementText.LoadXml(content).Require("Izvod");
        var lines = root.Children("Prom").Select((x, i) =>
        {
            var debit = StatementText.ParseAmount(x.Text("IznosDug"));
            return BankStatementParsers.Line(i + 1, BankCode,
                x.Text("NazivNal"),
                x.Text("RacunNal"),
                $"{x.Text("Opis1")} {x.Text("Opis2")}".Trim(),
                StatementText.ParseCode(x.Text("Osnov")),
                x.Text("Poziv"),
                debit > 0m ? debit : 0m,
                debit > 0m ? 0m : StatementText.ParseAmount(x.Text("IznosPot")));
        }).ToArray();

        var name = Path.GetFileName(fileName);
        var date = BankStatementParsers.Field(name, '_', 2) ?? root.Text("DatumIzvoda");
        return new ParsedStatement(
            BankCode,
            BankStatementParsers.Field(name, '_', 1) ?? root.Text("Racun"),
            StatementText.ParseInt(root.Text("BrojIzvoda"), "broj izvoda"),
            StatementText.ParseDate(date),
            StatementText.ParseAmount(root.Text("PrethodnoStanje")),
            StatementText.ParseAmount(root.Text("NovoStanje")),
            StatementText.ParseAmount(root.Text("DugovniPromet")),
            StatementText.ParseAmount(root.Text("PotrazniPromet")),
            root.Text("BrojNaloga") is { } count ? StatementText.ParseInt(count, "broj naloga") : null,
            lines);
    }
}

/// <summary>265 Raiffeisen XML (legacy ImportIzvod_265): everything in attributes of Zaglavlje / Stavke.</summary>
internal sealed class RaiffeisenXmlParser : IBankStatementParser
{
    public int BankCode => 265;
    public string Name => "Raiffeisen banka (XML)";
    public bool IsSupported => true;

    public bool MatchesFileName(string fileName) =>
        fileName.StartsWith("265", StringComparison.Ordinal) &&
        fileName.Contains("izvod br.", StringComparison.OrdinalIgnoreCase) &&
        fileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase);

    public ParsedStatement Parse(string fileName, byte[] content)
    {
        var root = StatementText.LoadXml(content).Require("TransakcioniRacunPrivredaIzvod");
        var header = root.Require("Zaglavlje");
        var lines = root.Children("Stavke").Select((x, i) => BankStatementParsers.Line(i + 1, BankCode,
            x.Attr("NalogKorisnik"),
            x.Attr("BrojRacunaPrimaocaPosiljaoca"),
            x.Attr("Opis"),
            StatementText.ParseCode(x.Attr("SifraPlacanja")),
            x.Attr("PozivNaBrojKorisnika"),
            StatementText.ParseAmount(x.Attr("Duguje")),
            StatementText.ParseAmount(x.Attr("Potrazuje")))).ToArray();

        return new ParsedStatement(
            BankCode,
            BankStatementParsers.Field(Path.GetFileName(fileName), ' ', 0) ?? header.Attr("Partija"),
            StatementText.ParseInt(header.RequireAttr("BrojIzvoda"), "broj izvoda"),
            StatementText.ParseDate(header.RequireAttr("DatumIzvoda")),
            StatementText.ParseAmount(header.RequireAttr("PrethodnoStanje")),
            StatementText.ParseAmount(header.RequireAttr("NovoStanje")),
            StatementText.ParseAmount(header.Attr("DugovniPromet")),
            StatementText.ParseAmount(header.Attr("PotrazniPromet")),
            null,
            lines);
    }
}

/// <summary>275 Societe Generale / OTP (ex-SOGE) XML (legacy ImportIzvod_275): Stavke attributes Opis1..9.</summary>
internal sealed class SocieteGeneraleXmlParser : IBankStatementParser
{
    public int BankCode => 275;
    public string Name => "Societe Generale (XML)";
    public bool IsSupported => true;
    public bool MatchesFileName(string fileName) => fileName.StartsWith("275", StringComparison.Ordinal) && fileName.Length > 18;

    public ParsedStatement Parse(string fileName, byte[] content)
    {
        var doc = StatementText.LoadXml(content);
        var root = (XContainer?)doc.Find("TransakcioniRacunPrivredaIzvod") ?? doc;
        var header = root.Require("Zaglavlje");
        var lines = root.Descendants().Where(x => x.Name.LocalName == "Stavke").Select((x, i) =>
        {
            var name = new[] { x.Attr("Opis3"), x.Attr("Opis2"), x.Attr("Opis1") }.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
            var credit = StatementText.ParseAmount(x.Attr("Potrazuje"));
            var code = x.Attr("Opis6")?.Trim();
            return BankStatementParsers.Line(i + 1, BankCode,
                name,
                x.Attr("Opis5"),
                x.Attr("Opis9"),
                StatementText.ParseCode(code is { Length: > 3 } ? code[^3..] : code),
                x.Attr("Opis8"),
                credit == 0m ? StatementText.ParseAmount(x.Attr("Duguje")) : 0m,
                credit);
        }).ToArray();

        var creditCount = header.Attr("BrojStavkiPotrazuje");
        var debitCount = header.Attr("BrojStavkiDuguje");
        return new ParsedStatement(
            BankCode,
            BankStatementParsers.Field(Path.GetFileName(fileName), ' ', 0) ?? header.Attr("Partija"),
            StatementText.ParseInt(header.RequireAttr("IzvodID"), "broj izvoda"),
            StatementText.ParseDate(header.RequireAttr("DatumIzvoda")),
            StatementText.ParseAmount(header.RequireAttr("PrethodnoStanje")),
            StatementText.ParseAmount(header.RequireAttr("NovoStanje")),
            StatementText.ParseAmount(header.Attr("UkupnoZaduzenje")),
            StatementText.ParseAmount(header.Attr("UkupnoOdobrenje")),
            creditCount is null || debitCount is null
                ? null
                : StatementText.ParseInt(creditCount, "broj stavki") + StatementText.ParseInt(debitCount, "broj stavki"),
            lines);
    }
}

/// <summary>
/// 170 UniCredit TXT, '#' separated, first line is a header (legacy ImportIzvod_170).
/// File name yyyyMMdd_account_nnnnn.txt: date, account (control digits computed, legacy KontrolniBroj 97)
/// and statement number (legacy never read it). No balances in the file -- the caller takes the
/// previous statement's closing balance (owner decision P16/3).
/// </summary>
internal sealed class UniCreditTxtParser : IBankStatementParser
{
    public int BankCode => 170;
    public string Name => "UniCredit (TXT)";
    public bool IsSupported => true;

    public bool MatchesFileName(string fileName) =>
        fileName.Length == 30 && fileName[8] == '_' && fileName[20] == '_' &&
        fileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase);

    public ParsedStatement Parse(string fileName, byte[] content)
    {
        var rows = StatementText.DecodeText(content)
            .Split('\n')
            .Skip(1)
            .Select(x => x.TrimEnd('\r'))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToArray();
        var lines = rows.Select((row, i) =>
        {
            var f = row.Split('#');
            if (f.Length < 16)
            {
                throw new DomainRuleException("statement.invalid-row", $"Red {i + 2} izvoda ima {f.Length} polja, očekivano najmanje 16.");
            }

            var credit = f[7].Trim() == "2";
            return BankStatementParsers.Line(i + 1, BankCode, f[11], f[10], f[12], StatementText.ParseCode(f[13]), f[15],
                credit ? 0m : StatementText.ParseAmount(f[5]),
                credit ? StatementText.ParseAmount(f[6]) : 0m);
        }).ToArray();

        var stem = BankStatementParsers.Stem(Path.GetFileName(fileName));
        var account = BankStatementParsers.Field(stem, '_', 1) ?? string.Empty;
        return new ParsedStatement(
            BankCode,
            $"170-{account}-{StatementText.ControlDigits("170", account)}",
            StatementText.ParseInt(BankStatementParsers.Field(stem, '_', 2), "broj izvoda"),
            StatementText.ParseDate(BankStatementParsers.Field(stem, '_', 0)),
            null,
            null,
            null,
            null,
            null,
            lines);
    }
}
