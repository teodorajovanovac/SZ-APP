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
        // Legacy ImportIzvod_180 is only a MsgBox ("NEMA PROCESA ZA BANKU 180") -- nothing to port.
        new UnsupportedParser(180, "Alpha banka (TXT)", name => name.StartsWith('I') && name.Length == 16),
        new PostanskaXmlParser(),
        new OfxStatementParser(205, "Komercijalna banka (XML)", "refnumber",
            name => name.IndexOf(" 205", StringComparison.Ordinal) == 8,
            name => Field(Stem(name), ' ', 1), name => StatementText.ParseDate(Field(Stem(name), ' ', 0))),
        new RaiffeisenXmlParser(),
        new SocieteGeneraleXmlParser(),
        new OtpXmlParser(),
        new OfxStatementParser(330, "Credit Agricole (XML)", "payeerefnumber",
            name => name.IndexOf("_330", StringComparison.Ordinal) == 5 && name.Length == 37,
            name => Field(name, '_', 1), null),
        new AssecoOfficeBankingParser(),
        new PostanskaKs2Parser(),
    ];

    /// <summary>
    /// Chosen format wins; otherwise legacy file-name detection (IzvodIzBanke), then -- like legacy --
    /// content sniffing for Asseco Office Banking and Poštanska KS2IZVPLKK (no file-name rule).
    /// </summary>
    public static IBankStatementParser Resolve(string fileName, int? bankCode, byte[]? content = null)
    {
        if (bankCode is { } code)
        {
            return All.FirstOrDefault(x => x.BankCode == code)
                   ?? throw new DomainRuleException("statement.unknown-format", $"Nepoznat format izvoda {code}.");
        }

        return All.FirstOrDefault(x => x.MatchesFileName(fileName))
               ?? (content is null ? null : All.FirstOrDefault(x => x.MatchesContent(content)))
               ?? throw new DomainRuleException("statement.format-not-detected", "Banka nije prepoznata po imenu fajla — izaberite format ručno.");
    }

    public static ParsedStatement Parse(string fileName, byte[] content, int? bankCode = null)
    {
        var parser = Resolve(fileName, bankCode, content);
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

    /// <summary>Legacy mBank for content-detected formats = first 3 digits of the account.</summary>
    internal static int BankOf(string? account, int fallback) =>
        account is { Length: >= 3 } && account[..3].All(char.IsAsciiDigit) ? int.Parse(account[..3], System.Globalization.CultureInfo.InvariantCulture) : fallback;

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

/// <summary>
/// 325 OTP banka XML (legacy ImportIzvod_325). File "yyyy-{18-digit account}-n[-SG].xml"; the account
/// is taken from the name (Mid(6, 18)). Two layouts: V1 (1.5.-29.7.2021, ex-SOGE attributes
/// TransakcioniRacunPrivredaIzvod/Zaglavlje + Stavke Opis3/5/6/8/9) and V2 (from 30.7.2021, izvod
/// elements + stavke/transakcija). Payment code strips "SIF-" (ObradiSifruPlacanjaOTP).
/// </summary>
internal sealed class OtpXmlParser : IBankStatementParser
{
    public int BankCode => 325;
    public string Name => "OTP banka (XML V1/V2)";
    public bool IsSupported => true;
    public bool MatchesFileName(string fileName) => fileName.IndexOf("325", StringComparison.Ordinal) == 5 && fileName.Length > 28;

    public ParsedStatement Parse(string fileName, byte[] content)
    {
        var name = Path.GetFileName(fileName);
        var account = name.Length >= 23 ? name.Substring(5, 18) : null;
        var doc = StatementText.LoadXml(content);
        return doc.Find("TransakcioniRacunPrivredaIzvod") is { } v1 ? ParseV1(v1, account) : ParseV2(doc.Require("izvod"), account);
    }

    private ParsedStatement ParseV1(XElement root, string? account)
    {
        var header = root.Require("Zaglavlje");
        var lines = root.Children("Stavke").Select((x, i) => BankStatementParsers.Line(i + 1, BankCode,
            x.Attr("Opis3"),
            x.Attr("Opis5"),
            x.Attr("Opis9"),
            StatementText.ParseCode(x.Attr("Opis6")?.Replace("SIF-", string.Empty)),
            x.Attr("Opis8"),
            StatementText.ParseAmount(x.Attr("Duguje")),
            StatementText.ParseAmount(x.Attr("Potrazuje")))).ToArray();

        return new ParsedStatement(
            BankCode,
            account ?? header.Attr("Partija"),
            StatementText.ParseInt(header.RequireAttr("IzvodID"), "broj izvoda"),
            StatementText.ParseDate(header.RequireAttr("DatumIzvoda")),
            StatementText.ParseAmount(header.RequireAttr("PrethodnoStanje")),
            StatementText.ParseAmount(header.RequireAttr("NovoStanje")),
            StatementText.ParseAmount(header.Attr("UkupnoZaduzenje")),
            StatementText.ParseAmount(header.Attr("UkupnoOdobrenje")),
            Sum(header.Attr("BrojStavkiPotrazuje"), header.Attr("BrojStavkiDuguje")),
            lines);
    }

    private ParsedStatement ParseV2(XElement root, string? account)
    {
        var lines = (root.Children("stavke").FirstOrDefault()?.Children("transakcija") ?? []).Select((x, i) => BankStatementParsers.Line(i + 1, BankCode,
            x.Text("komitent"),
            x.Text("racun"),
            x.Text("svrhaDoznake"),
            StatementText.ParseCode(x.Text("sifraPlacanja")?.Replace("SIF-", string.Empty)),
            x.Text("pozivNaBrojOdobrenje"),
            StatementText.ParseAmount(x.Text("duguje")),
            StatementText.ParseAmount(x.Text("potrazuje")))).ToArray();

        return new ParsedStatement(
            BankCode,
            account,
            StatementText.ParseInt(root.Text("brojIzvoda"), "broj izvoda"),
            StatementText.ParseDate(root.Text("datum")),
            StatementText.ParseAmount(root.Text("prethodnoStanje")),
            StatementText.ParseAmount(root.Text("novoStanje")),
            StatementText.ParseAmount(root.Text("DnevniPrometDugovni")),
            StatementText.ParseAmount(root.Text("DnevniPrometPotrazni")),
            Sum(root.Text("BrojNalogaOdobrenja"), root.Text("BrojNalogaZaduzenja")),
            lines);
    }

    private static int? Sum(string? a, string? b) =>
        a is null || b is null ? null : StatementText.ParseInt(a, "broj stavki") + StatementText.ParseInt(b, "broj stavki");
}

/// <summary>
/// Asseco Office Banking XML (legacy IsAssecoOfficeBankig + ImportIzvod_AssecoOfficeBanking): any bank
/// whose e-banking is Asseco, recognized by &lt;rstype&gt;ibank.payment.stmtrs.past&lt;/rstype&gt;.
/// stmtrslist/stmtrs (or stmtrs), account = first acctid, date = dtasof, balances ledgerbal/availbal,
/// trnlist@count, lines use payeerefnumber. Legacy bank (for reference cleanup) = first 3 account digits.
/// </summary>
internal sealed class AssecoOfficeBankingParser : IBankStatementParser
{
    public int BankCode => 9001;
    public string Name => "Asseco Office Banking (XML)";
    public bool IsSupported => true;
    public bool MatchesFileName(string fileName) => false;

    public bool MatchesContent(byte[] content) =>
        StatementText.DecodeText(content).Contains("<rstype>ibank.payment.stmtrs.past</rstype>", StringComparison.Ordinal);

    public ParsedStatement Parse(string fileName, byte[] content)
    {
        var root = StatementText.LoadXml(content).Require("stmtrs");
        var account = root.Descendants().FirstOrDefault(x => x.Name.LocalName == "acctid" && x.Parent?.Name.LocalName != "payeeaccountinfo")?.Value.Trim();
        var bank = BankStatementParsers.BankOf(account, BankCode);
        var list = root.Require("trnlist");
        var lines = list.Children("stmttrn").Select((x, i) =>
        {
            var amount = StatementText.ParseAmount(x.Text("trnamt"));
            var benefit = x.Text("benefit")?.Trim();
            return BankStatementParsers.Line(i + 1, bank,
                x.Find("payeeinfo")?.Text("name"),
                x.Find("payeeaccountinfo")?.Text("acctid"),
                x.Text("purpose"),
                StatementText.ParseCode(x.Text("purposecode")),
                x.Text("payeerefnumber"),
                benefit == "debit" ? amount : 0m,
                benefit == "credit" ? amount : 0m);
        }).ToArray();

        var count = list.Attr("count");
        return new ParsedStatement(
            bank,
            account,
            StatementText.ParseInt(root.Text("stmtnumber"), "broj izvoda"),
            StatementText.ParseDate(root.Text("dtasof")),
            StatementText.ParseAmount(root.Require("ledgerbal").Text("balamt")),
            StatementText.ParseAmount(root.Require("availbal").Text("balamt")),
            null,
            null,
            count is null ? null : StatementText.ParseInt(count, "broj stavki"),
            lines);
    }
}

/// <summary>
/// Poštanska štedionica KS2IZVPLKK XML (legacy IsPostanskaImport + ImportIzvod_PostanskaImport),
/// recognized by &lt;MATICNI_BANKE&gt;07004893&lt;/MATICNI_BANKE&gt;. KS2IZVPLKK/IZVOD header
/// (PARTIJA, DATUM_IZVODA, BROJ_IZVODA, stanja, prometi) + STAVKE/STAVKA lines. Legacy's declared
/// count was the STAVKA count itself, so no count is declared here.
/// </summary>
internal sealed class PostanskaKs2Parser : IBankStatementParser
{
    public int BankCode => 9002;
    public string Name => "Poštanska štedionica KS2IZVPLKK";
    public bool IsSupported => true;
    public bool MatchesFileName(string fileName) => false;

    public bool MatchesContent(byte[] content) =>
        StatementText.DecodeText(content).Contains("<MATICNI_BANKE>07004893</MATICNI_BANKE>", StringComparison.Ordinal);

    public ParsedStatement Parse(string fileName, byte[] content)
    {
        var root = StatementText.LoadXml(content).Require("IZVOD");
        var account = root.Text("PARTIJA")?.Trim();
        var bank = BankStatementParsers.BankOf(account, 200);
        var lines = (root.Children("STAVKE").FirstOrDefault()?.Children("STAVKA") ?? []).Select((x, i) => BankStatementParsers.Line(i + 1, bank,
            x.Text("KORISNIK-NALOGODAVAC"),
            x.Text("RACUN"),
            x.Text("SVRHA_PLACANJA"),
            StatementText.ParseCode(x.Text("SIFRA_PLACANJA")),
            x.Text("POZIVKORISNIK"),
            StatementText.ParseAmount(x.Text("IZNOS_DUGUJE")),
            StatementText.ParseAmount(x.Text("IZNOS_POTRAZUJE")))).ToArray();

        return new ParsedStatement(
            bank,
            account,
            StatementText.ParseInt(root.Text("BROJ_IZVODA"), "broj izvoda"),
            StatementText.ParseDate(root.Text("DATUM_IZVODA")),
            StatementText.ParseAmount(root.Text("PRETHODNO_STANJE")),
            StatementText.ParseAmount(root.Text("NOVO_STANJE")),
            StatementText.ParseAmount(root.Text("DUGOVNI_PROMET")),
            StatementText.ParseAmount(root.Text("POTRAZNI_PROMET")),
            null,
            lines);
    }
}
