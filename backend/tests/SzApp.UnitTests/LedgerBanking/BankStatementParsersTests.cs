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

    // 325 OTP V1 (ImportIzvod_325_V1): SOGE-like attributes, name Opis3, code "SIF-289", reference
    // strips "PBO-" and the "(97)" model; account from the file name Mid(6, 18).
    [Fact]
    public void Otp325V1_ParsesAttributes()
    {
        const string xml = """
            <TransakcioniRacunPrivredaIzvod>
              <Zaglavlje IzvodID="7" DatumIzvoda="15.06.2021" PrethodnoStanje="100.00" NovoStanje="150.00"
                UkupnoOdobrenje="80.00" UkupnoZaduzenje="30.00" BrojStavkiPotrazuje="1" BrojStavkiDuguje="1" />
              <Stavke Opis3="Marko Markovic" Opis5="160000000026950155" Opis6="SIF-289" Opis8="PBO-(97)12-345"
                Opis9="Odrzavanje" Potrazuje="80.00" Duguje="0.00" />
              <Stavke Opis3="EPS" Opis5="170-30032591000-11" Opis6="SIF-221" Opis8="" Opis9="Struja" Potrazuje="0.00" Duguje="30.00" />
            </TransakcioniRacunPrivredaIzvod>
            """;
        const string fileName = "2021-325950050028777962-7-SG.xml";
        var parser = BankStatementParsers.Resolve(fileName, null);
        Assert.Equal(325, parser.BankCode);
        var parsed = BankStatementParsers.Parse(fileName, Bytes(xml));
        Assert.Equal("325950050028777962", parsed.AccountNumber);
        Assert.Equal(7, parsed.StatementNumber);
        Assert.Equal(new DateOnly(2021, 6, 15), parsed.Date);
        Assert.Equal(30m, parsed.DeclaredDebit);
        Assert.Equal(80m, parsed.DeclaredCredit);
        Assert.Equal(2, parsed.DeclaredCount);
        Assert.Equal("Marko Markovic", parsed.Lines[0].PayerName);
        Assert.Equal("160-269501-55", parsed.Lines[0].PayerAccount);
        Assert.Equal(289, parsed.Lines[0].Code);
        Assert.Equal("12345", parsed.Lines[0].PaymentReference);
        Assert.Equal(80m, parsed.Lines[0].Credit);
        Assert.Equal(30m, parsed.Lines[1].Debit);
    }

    // 325 OTP V2 (ImportIzvod_325_V2, from 30.7.2021): izvod elements + stavke/transakcija.
    [Fact]
    public void Otp325V2_ParsesElements()
    {
        const string xml = """
            <izvod>
              <brojIzvoda>120</brojIzvoda>
              <datum>2021-08-02</datum>
              <prethodnoStanje>1000,00</prethodnoStanje>
              <novoStanje>1100,00</novoStanje>
              <DnevniPrometPotrazni>100,00</DnevniPrometPotrazni>
              <DnevniPrometDugovni>0,00</DnevniPrometDugovni>
              <BrojNalogaOdobrenja>1</BrojNalogaOdobrenja>
              <BrojNalogaZaduzenja>0</BrojNalogaZaduzenja>
              <stavke>
                <transakcija>
                  <komitent>Jovana Jovic</komitent>
                  <racun>265000000012345678</racun>
                  <pozivNaBrojOdobrenje>(97) 44-1001</pozivNaBrojOdobrenje>
                  <potrazuje>100,00</potrazuje>
                  <duguje>0,00</duguje>
                  <sifraPlacanja>289</sifraPlacanja>
                  <svrhaDoznake>Uplata</svrhaDoznake>
                </transakcija>
              </stavke>
            </izvod>
            """;
        var parsed = BankStatementParsers.Parse("2021-325950050028777962-120.xml", Bytes(xml));
        Assert.Equal(120, parsed.StatementNumber);
        Assert.Equal(new DateOnly(2021, 8, 2), parsed.Date);
        Assert.Equal(1, parsed.DeclaredCount);
        var line = Assert.Single(parsed.Lines);
        Assert.Equal("441001", line.PaymentReference);
        Assert.Equal("265-123456-78", line.PayerAccount);
        Assert.Equal(100m, line.Credit);
    }

    // Asseco Office Banking: no file-name rule, detected by rstype (IsAssecoOfficeBankig).
    [Fact]
    public void Asseco_DetectedByContent_UsesPayeeRefNumber()
    {
        const string xml = """
            <stmtrslist>
              <rstype>ibank.payment.stmtrs.past</rstype>
              <stmtrs>
                <acctid>340-11001234-56</acctid>
                <stmtnumber>45</stmtnumber>
                <dtasof>2023-03-10</dtasof>
                <ledgerbal><balamt>200.00</balamt></ledgerbal>
                <availbal><balamt>150.00</balamt></availbal>
                <trnlist count="1">
                  <stmttrn>
                    <benefit>debit</benefit>
                    <trnamt>50.00</trnamt>
                    <purpose>Provizija</purpose>
                    <purposecode>221</purposecode>
                    <payeerefnumber>97 11-22</payeerefnumber>
                    <payeeinfo><name> Banka </name></payeeinfo>
                    <payeeaccountinfo><acctid>340-0000011-99</acctid></payeeaccountinfo>
                  </stmttrn>
                </trnlist>
              </stmtrs>
            </stmtrslist>
            """;
        var content = Bytes(xml);
        var parser = BankStatementParsers.Resolve("izvod.xml", null, content);
        Assert.Equal(9001, parser.BankCode);
        var parsed = BankStatementParsers.Parse("izvod.xml", content);
        Assert.Equal(340, parsed.BankCode);
        Assert.Equal("340-11001234-56", parsed.AccountNumber);
        Assert.Equal(new DateOnly(2023, 3, 10), parsed.Date);
        Assert.Equal(200m, parsed.PreviousBalance);
        var line = Assert.Single(parsed.Lines);
        Assert.Equal(50m, line.Debit);
        Assert.Equal("Banka", line.PayerName);
        Assert.Equal("971122", line.PaymentReference);
        Assert.Equal("340-11-99", line.PayerAccount);
    }

    // Poštanska KS2IZVPLKK: detected by MATICNI_BANKE 07004893 (IsPostanskaImport).
    [Fact]
    public void PostanskaKs2_DetectedByContent()
    {
        const string xml = """
            <KS2IZVPLKK>
              <MATICNI_BANKE>07004893</MATICNI_BANKE>
              <IZVOD>
                <PARTIJA>200-2345678-10</PARTIJA>
                <DATUM_IZVODA>18.01.2022</DATUM_IZVODA>
                <BROJ_IZVODA>3</BROJ_IZVODA>
                <PRETHODNO_STANJE>0,00</PRETHODNO_STANJE>
                <NOVO_STANJE>75,00</NOVO_STANJE>
                <POTRAZNI_PROMET>75,00</POTRAZNI_PROMET>
                <DUGOVNI_PROMET>0,00</DUGOVNI_PROMET>
                <STAVKE>
                  <STAVKA>
                    <KORISNIK-NALOGODAVAC>Ivan Ivic</KORISNIK-NALOGODAVAC>
                    <RACUN>160-5555-11</RACUN>
                    <SVRHA_PLACANJA>Odrzavanje</SVRHA_PLACANJA>
                    <SIFRA_PLACANJA>189</SIFRA_PLACANJA>
                    <POZIVKORISNIK>12/34</POZIVKORISNIK>
                    <IZNOS_POTRAZUJE>75,00</IZNOS_POTRAZUJE>
                    <IZNOS_DUGUJE></IZNOS_DUGUJE>
                  </STAVKA>
                </STAVKE>
              </IZVOD>
            </KS2IZVPLKK>
            """;
        var content = Bytes(xml);
        Assert.Equal(9002, BankStatementParsers.Resolve("x.xml", null, content).BankCode);
        var parsed = BankStatementParsers.Parse("x.xml", content);
        Assert.Equal(200, parsed.BankCode);
        Assert.Equal("200-2345678-10", parsed.AccountNumber);
        Assert.Equal(new DateOnly(2022, 1, 18), parsed.Date);
        Assert.Equal(75m, parsed.DeclaredCredit);
        var line = Assert.Single(parsed.Lines);
        Assert.Equal(75m, line.Credit);
        Assert.Equal(0m, line.Debit);
        Assert.Equal("1234", line.PaymentReference);
        Assert.Equal(189, line.Code);
    }

    [Theory]
    [InlineData(180)]
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
