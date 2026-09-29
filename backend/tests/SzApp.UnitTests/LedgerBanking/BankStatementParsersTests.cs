using System.Text;
using SzApp.Domain;
using SzApp.Domain.LedgerBanking.StatementParsing;

namespace SzApp.UnitTests.LedgerBanking;

/// <summary>
/// Synthetic samples built field-by-field from the legacy VBA (cIzvod.txt) -- no real bank file
/// exists in the repo yet (P16), so every case here is unverified against a real file.
/// </summary>
public sealed class BankStatementParsersTests
{
    private static byte[] Bytes(string s) => Encoding.UTF8.GetBytes(s);

    // 160 Banca Intesa: root pmtnotification|stmtrs, ledgerbal/balamt = previous, availbal/balamt =
    // new, trnlist@count, trnlist/stmttrn lines (legacy ImportIzvod_160, line 1414).
    [Fact]
    public void Intesa160_ParsesHeaderAndLines()
    {
        const string xml = """
            <pmtnotification>
              <stmtnumber>15</stmtnumber>
              <ledgerbal><balamt>1000,00</balamt></ledgerbal>
              <availbal><balamt>1250,50</balamt></availbal>
              <trnlist count="1">
                <stmttrn>
                  <dtasof>20220115</dtasof>
                  <trnamt>250,50</trnamt>
                  <benefit>credit</benefit>
                  <purpose>Uplata odrzavanja</purpose>
                  <purposecode>221ABC</purposecode>
                  <refnumber>97123456</refnumber>
                  <payeeinfo><name>Petar Petrovic</name></payeeinfo>
                  <payeeaccountinfo><acctid>1600000269501</acctid></payeeaccountinfo>
                </stmttrn>
              </trnlist>
            </pmtnotification>
            """;
        var parser = BankStatementParsers.All.Single(x => x.BankCode == 160);
        Assert.True(parser.MatchesFileName("[160-259497-10][15].xml"));
        var parsed = parser.Parse("[160-259497-10][15].xml", Bytes(xml));

        Assert.Equal(15, parsed.StatementNumber);
        Assert.Equal(new DateOnly(2022, 1, 15), parsed.Date);
        Assert.Equal(1000.00m, parsed.PreviousBalance);
        Assert.Equal(1250.50m, parsed.NewBalance);
        Assert.Equal("160-259497-10", parsed.AccountNumber);
        var line = Assert.Single(parsed.Lines);
        Assert.Equal(250.50m, line.Credit);
        Assert.Equal(0m, line.Debit);
        Assert.Equal("Petar Petrovic", line.PayerName);
        Assert.Equal(221, line.Code);
        Assert.Equal("97123456", line.PaymentReference);
    }

    // 205 Komercijalna banka shares the same trnlist XML; account + date come from the file name
    // ("yyyyMMdd bankcode-account.XML", legacy ImamLiIzvod_205).
    [Fact]
    public void Komercijalna205_TakesDateAndAccountFromFileName()
    {
        const string xml = """
            <stmtrs>
              <stmtnumber>3</stmtnumber>
              <ledgerbal><balamt>500.00</balamt></ledgerbal>
              <availbal><balamt>500.00</balamt></availbal>
              <trnlist count="0"></trnlist>
            </stmtrs>
            """;
        var parser = BankStatementParsers.All.Single(x => x.BankCode == 205);
        const string fileName = "20140808 205000000020819392.XML";
        Assert.True(parser.MatchesFileName(fileName));
        var parsed = parser.Parse(fileName, Bytes(xml));
        Assert.Equal(new DateOnly(2014, 8, 8), parsed.Date);
        Assert.Equal("205000000020819392", parsed.AccountNumber);
    }

    // 200 Poštanska štedionica: root Izvod, Prom lines, IznosDug>0 debit else IznosPot credit
    // (legacy ImportIzvod_200).
    [Fact]
    public void Postanska200_DebitWinsWhenPositive()
    {
        const string xml = """
            <Izvod>
              <BrojIzvoda>1</BrojIzvoda>
              <PrethodnoStanje>100,00</PrethodnoStanje>
              <NovoStanje>50,00</NovoStanje>
              <DugovniPromet>50,00</DugovniPromet>
              <PotrazniPromet>0,00</PotrazniPromet>
              <BrojNaloga>1</BrojNaloga>
              <Racun>314578010100491</Racun>
              <Prom>
                <NazivNal>JP Infostan</NazivNal>
                <RacunNal>205000000020819392</RacunNal>
                <Opis1>Racun</Opis1>
                <Opis2>za struju</Opis2>
                <Osnov>221</Osnov>
                <Poziv>97-1-2</Poziv>
                <IznosDug>50,00</IznosDug>
                <IznosPot>0,00</IznosPot>
              </Prom>
            </Izvod>
            """;
        var parser = BankStatementParsers.All.Single(x => x.BankCode == 200);
        const string fileName = "Izvod_200314578010100491_20220118_000001.XML";
        Assert.True(parser.MatchesFileName(fileName));
        var parsed = parser.Parse(fileName, Bytes(xml));
        Assert.Equal(new DateOnly(2022, 1, 18), parsed.Date);
        Assert.Equal("200314578010100491", parsed.AccountNumber);
        var line = Assert.Single(parsed.Lines);
        Assert.Equal(50.00m, line.Debit);
        Assert.Equal(0m, line.Credit);
        Assert.Equal("Racun za struju", line.Info);
        Assert.Equal("9712", line.PaymentReference);
    }

    // 265 Raiffeisen: TransakcioniRacunPrivredaIzvod/Zaglavlje attributes, Stavke attributes
    // (legacy ImportIzvod_265).
    [Fact]
    public void Raiffeisen265_ReadsAttributesFromZaglavljeAndStavke()
    {
        const string xml = """
            <TransakcioniRacunPrivredaIzvod>
              <Zaglavlje BrojIzvoda="7" DatumIzvoda="20220301" PrethodnoStanje="2000,00" NovoStanje="1800,00"
                         PotrazniPromet="0,00" DugovniPromet="200,00" Partija="3300310037746-68" />
              <Stavke NalogKorisnik="Jovan Jovanovic" BrojRacunaPrimaocaPosiljaoca="265330031003774668"
                      PozivNaBrojKorisnika="97-55-1" Potrazuje="0,00" Duguje="200,00" SifraPlacanja="253" Opis="Uplata" />
            </TransakcioniRacunPrivredaIzvod>
            """;
        var parser = BankStatementParsers.All.Single(x => x.BankCode == 265);
        const string fileName = "265104031000153415 izvod br.1-Realizovani.xml";
        Assert.True(parser.MatchesFileName(fileName));
        var parsed = parser.Parse(fileName, Bytes(xml));
        Assert.Equal(7, parsed.StatementNumber);
        Assert.Equal(new DateOnly(2022, 3, 1), parsed.Date);
        Assert.Equal(2000.00m, parsed.PreviousBalance);
        Assert.Equal(1800.00m, parsed.NewBalance);
        var line = Assert.Single(parsed.Lines);
        Assert.Equal(200.00m, line.Debit);
        Assert.Equal("Jovan Jovanovic", line.PayerName);
        Assert.Equal(253, line.Code);
    }

    // 275 SOGE: Zaglavlje attributes, Stavke Opis1..9; name falls back Opis3 -> Opis2 -> Opis1
    // (legacy ImportIzvod_275).
    [Fact]
    public void SocieteGenerale275_NameFallsBackThroughOpisFields()
    {
        const string xml = """
            <TransakcioniRacunPrivredaIzvod>
              <Zaglavlje IzvodID="4" DatumIzvoda="20220410" PrethodnoStanje="300,00" NovoStanje="400,00"
                         UkupnoOdobrenje="100,00" UkupnoZaduzenje="0,00" BrojStavkiPotrazuje="1" BrojStavkiDuguje="0"
                         Partija="1022809827897" />
              <Stavke Opis1="" Opis2="" Opis3="Marko Markovic" Opis5="275001022809827897" Opis6="PBO-97 sif221"
                      Opis8="PBO-97001234" Opis9="Uplata odrzavanja" Potrazuje="100,00" Duguje="0,00" />
            </TransakcioniRacunPrivredaIzvod>
            """;
        var parser = BankStatementParsers.All.Single(x => x.BankCode == 275);
        const string fileName = "275001022809827897 Izvod br. 1.XML";
        Assert.True(parser.MatchesFileName(fileName));
        var parsed = parser.Parse(fileName, Bytes(xml));
        Assert.Equal(4, parsed.StatementNumber);
        var line = Assert.Single(parsed.Lines);
        Assert.Equal("Marko Markovic", line.PayerName);
        Assert.Equal(100.00m, line.Credit);
        Assert.Equal("1234", line.PaymentReference); // PBO-97 + leading zeros stripped
    }

    // 330 Credit Agricole shares the 160/205 XML shape, uses payeerefnumber for the reference
    // (legacy ImportIzvod_330).
    [Fact]
    public void CreditAgricole330_UsesPayeeRefNumber()
    {
        const string xml = """
            <stmtrs>
              <stmtnumber>2</stmtnumber>
              <dtasof>20201215</dtasof>
              <ledgerbal><balamt>0,00</balamt></ledgerbal>
              <availbal><balamt>100,00</balamt></availbal>
              <trnlist count="1">
                <stmttrn>
                  <purpose>Clanarina</purpose>
                  <purposecode>221x</purposecode>
                  <payeerefnumber>97-99-2</payeerefnumber>
                  <benefit>credit</benefit>
                  <trnamt>100,00</trnamt>
                  <payeeinfo><name>Ana Anic</name></payeeinfo>
                  <payeeaccountinfo><acctid>908340011901</acctid></payeeaccountinfo>
                </stmttrn>
              </trnlist>
            </stmtrs>
            """;
        var parser = BankStatementParsers.All.Single(x => x.BankCode == 330);
        const string fileName = "Izvod_330000000400572356_20201215.xml"; // legacy IzvodIzBanke requires Len = 37
        Assert.True(parser.MatchesFileName(fileName));
        var parsed = parser.Parse(fileName, Bytes(xml));
        Assert.Equal("330000000400572356", parsed.AccountNumber);
        var line = Assert.Single(parsed.Lines);
        Assert.Equal("97992", line.PaymentReference);
    }

    // 170 UniCredit TXT: '#' separated, header row skipped, no balances in the file (legacy
    // ImportIzvod_170) -- balances are filled by the caller from the previous statement.
    [Fact]
    public void UniCredit170_ParsesHashSeparatedRows()
    {
        var header = string.Join('#', Enumerable.Range(0, 16).Select(i => $"h{i}"));
        var fields = new string[16];
        fields[5] = "150,00";  // debit
        fields[6] = "0,00";    // credit
        fields[7] = "1";       // 2 = credit, else debit
        fields[10] = "160000002695";
        fields[11] = "Firma DOO";
        fields[12] = "Nabavka";
        fields[13] = "289x";
        fields[15] = "97-1-1";
        var row = string.Join('#', fields);
        var txt = header + "\n" + row + "\n";

        var parser = BankStatementParsers.All.Single(x => x.BankCode == 170);
        const string fileName = "20171222_30032591000_00073.TXT";
        Assert.True(parser.MatchesFileName(fileName));
        var parsed = parser.Parse(fileName, Bytes(txt));
        Assert.Equal(new DateOnly(2017, 12, 22), parsed.Date);
        Assert.Null(parsed.PreviousBalance);
        Assert.Null(parsed.NewBalance);
        var line = Assert.Single(parsed.Lines);
        Assert.Equal(150.00m, line.Debit);
        Assert.Equal(0m, line.Credit);
        Assert.Equal("Firma DOO", line.PayerName);
        Assert.Equal(289, line.Code);
    }

    [Theory]
    [InlineData(180)]
    [InlineData(325)]
    public void StubFormats_ReportNotSupported(int bankCode)
    {
        var parser = BankStatementParsers.All.Single(x => x.BankCode == bankCode);
        Assert.False(parser.IsSupported);
        var ex = Assert.Throws<DomainRuleException>(() => BankStatementParsers.Parse("whatever", [], bankCode));
        Assert.Equal("statement.format-not-supported", ex.Code);
    }

    [Fact]
    public void Resolve_UnrecognizedFileName_Throws()
    {
        var ex = Assert.Throws<DomainRuleException>(() => BankStatementParsers.Resolve("random.xml", null));
        Assert.Equal("statement.format-not-detected", ex.Code);
    }
}
