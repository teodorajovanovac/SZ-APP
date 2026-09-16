-- =====================================================================
-- SZ App — nacrt ciljane SQL Server šeme (Faza 1, radna verzija)
-- =====================================================================
-- Ovo je RADNI NACRT, ne finalna migracija. Prati narativni plan u
-- docs/tehnicki-plan-faza1.md (sekcije 2 i 3). Mesta gde sam morala da
-- pretpostavim nešto (umesto da to bude eksplicitno potvrđeno JOIN-om iz
-- queries-sql.md ili tekstom u project-brief.md) su označena komentarom
-- "-- ASSUMPTION" ili referencom na broj pitanja iz sekcije 3 tog dokumenta
-- (npr. "pitanje 3.1"). Tabele iz sekcije 4 tog dokumenta (privremene/
-- backup/probne) su namerno izostavljene.
--
-- Fizički redosled CREATE TABLE ovde NIJE isti kao redosled domena u
-- narativnom planu (2.1–2.10) — tabele su preložene tako da FK uvek
-- pokazuje na već definisanu tabelu, osim jednog kružnog slučaja
-- (Skustina <-> Kupac), rešenog sa ALTER TABLE na kraju fajla.
--
-- Novac: svuda DECIMAL(19,4) umesto originalnog Access Double/Currency
-- (vidi tehnicki-plan-faza1.md sekcija 1 — razlog je precizna aritmetika
-- bez float grešaka koje su original ERROR_ upiti morali da zaobilaze
-- ručnim Round()-ovanjem).

-- =====================================================================
-- 1. ŠIFARNICI BEZ FK (mogu ići u bilo kom redosledu)
-- =====================================================================

CREATE TABLE dbo.Naselje (
    NaseljeId   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Naselje PRIMARY KEY,
    Naselje     NVARCHAR(50) NULL
);

CREATE TABLE dbo.TipObjekta (
    TipObjektaId    INT NOT NULL CONSTRAINT PK_TipObjekta PRIMARY KEY, -- šifarnik, bez identity
    TipObj          NVARCHAR(50) NULL,
    Print           NVARCHAR(50) NULL,
    SortObj         INT NULL,
    BinVrednost     INT NULL,
    NaslovRC        NVARCHAR(50) NULL,
    Disclaimer      NVARCHAR(MAX) NULL,
    DisclaimerSpc   NVARCHAR(MAX) NULL,
    NaslovRCSpc     NVARCHAR(50) NULL,
    CB_V            DECIMAL(9,4) NULL,
    IG_V            DECIMAL(9,4) NULL,
    CB_Konto        INT NULL,
    IG_Konto        INT NULL
);

CREATE TABLE dbo.TipPartnera (
    TipPartneraId INT NOT NULL CONSTRAINT PK_TipPartnera PRIMARY KEY,
    TipNaziv NVARCHAR(255) NULL
);

CREATE TABLE dbo.TipStavke (
    TipStavkeId INT NOT NULL CONSTRAINT PK_TipStavke PRIMARY KEY,
    TipStavke NVARCHAR(50) NULL
);

CREATE TABLE dbo.TipObracuna (
    TipObracunaId INT NOT NULL CONSTRAINT PK_TipObracuna PRIMARY KEY,
    TipObracuna NVARCHAR(100) NULL,
    IznosRacunaDobavljac NVARCHAR(100) NULL,
    Napomena NVARCHAR(255) NULL,
    Iznos NVARCHAR(255) NULL,
    Kolicina NVARCHAR(255) NULL,
    JM NVARCHAR(255) NULL,
    JMIndex INT NULL
);

CREATE TABLE dbo.TipADDTXT (
    TipAddTxtId INT NOT NULL CONSTRAINT PK_TipADDTXT PRIMARY KEY,
    AddTxt NVARCHAR(255) NULL
);

CREATE TABLE dbo.TipUplatnice (
    TipUplatniceId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TipUplatnice PRIMARY KEY,
    TipUpl NVARCHAR(255) NULL,
    Opis NVARCHAR(50) NULL
);

CREATE TABLE dbo.TipTODO (
    TipToDoId INT NOT NULL CONSTRAINT PK_TipTODO PRIMARY KEY,
    ToDo NVARCHAR(50) NULL
);

CREATE TABLE dbo.TipStatus ( -- izvorno "tipStatus"
    TipStatusId INT NOT NULL CONSTRAINT PK_TipStatus PRIMARY KEY,
    Status NVARCHAR(255) NULL,
    LimitTable NVARCHAR(255) NULL,
    Opis NVARCHAR(255) NULL -- [desc] je rezervisana reč u SQL Serveru, preimenovano
);

CREATE TABLE dbo.Konta ( -- interni kontni šifarnik SZ aplikacije
    Konto INT NOT NULL CONSTRAINT PK_Konta PRIMARY KEY, -- šifra konta, nije surogat
    Opis NVARCHAR(50) NULL,
    PrintText NVARCHAR(255) NULL
);

CREATE TABLE dbo.KontniOkvir ( -- ASSUMPTION: puni standardni kontni plan, odvojeno od Konta — proveriti razliku
    Konto NVARCHAR(255) NOT NULL CONSTRAINT PK_KontniOkvir PRIMARY KEY,
    SkraceniNaziv NVARCHAR(255) NULL,
    Naziv NVARCHAR(255) NULL,
    Prethodni NVARCHAR(255) NULL,
    Nivo INT NULL,
    Znak NVARCHAR(255) NULL,
    Aktivan BIT NULL
);

CREATE TABLE dbo.UserLevelList (
    UserLevelId INT NOT NULL CONSTRAINT PK_UserLevelList PRIMARY KEY,
    Caption     NVARCHAR(255) NULL
);

CREATE TABLE dbo.Kurs (
    KursId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Kurs PRIMARY KEY,
    Kurs DECIMAL(19,4) NULL,
    DatumOd DATE NULL,
    Skolska INT NULL, -- ASSUMPTION: nejasna namena ("školska godina"?)
    RefCena INT NULL,
    DatumUpisa DATETIME2(0) NULL
);

-- =====================================================================
-- 2. STAFF (korisnici) — bez izlaznih FK, referenciran svuda
-- =====================================================================

CREATE TABLE dbo.Staff (
    StaffId      INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Staff PRIMARY KEY, -- IDUser
    UserName     NVARCHAR(50) NULL, -- "User" je rezervisana reč u SQL-u
    -- StaffLogin i lozinka SE NE PRENOSE 1:1 (plain-text u izvoru, vidi CLAUDE.md) —
    -- zamenjuju se ASP.NET Core Identity tabelama (AspNetUsers sa PasswordHash).
    -- Ova tabela ostaje "profil" povezan sa Identity nalogom, ne mesto za lozinku.
    Level        INT NULL,
    LastLog      NVARCHAR(50) NULL,
    UseLang      NVARCHAR(50) NULL,
    Restrict     NVARCHAR(255) NULL,
    LastPC       NVARCHAR(255) NULL
);

-- =====================================================================
-- 3. ORGANIZACIONA HIJERARHIJA (Skustina / Kupac / Objekti)
-- =====================================================================

CREATE TABLE dbo.Skustina (
    SkustinaId          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Skustina PRIMARY KEY,
    NazivSS              NVARCHAR(255) NULL,
    Zgrada                NVARCHAR(50) NULL,
    Adresa                NVARCHAR(50) NULL,
    Opstina               NVARCHAR(255) NULL,
    PBroj                 NVARCHAR(50) NULL,
    PredstavnikSSId       INT NULL, -- FK -> Kupac, ASSUMPTION — dodato niže (kružna zavisnost)
    PIB                   INT NULL,
    TR                     NVARCHAR(50) NULL,
    MB                     NVARCHAR(50) NULL,
    Napomena               NVARCHAR(255) NULL,
    RB                     INT NULL,
    PrintNaziv             NVARCHAR(255) NULL,
    Folder                 NVARCHAR(50) NULL,
    UplatnicaTip           INT NULL,
    NaseljeId              INT NULL,
    Konto                  INT NULL,
    SkStatus               INT NULL,
    ExterniKonto           NVARCHAR(255) NULL,
    PDtext                 NVARCHAR(255) NULL,
    UpravnikId             INT NULL, -- FK -> Staff
    DatumUgovora            DATE NULL,
    SZPDV                   BIT NULL,
    TipSubjekta              INT NULL,
    InvoiceIssuer             INT NULL,
    Doznaka                   NVARCHAR(255) NULL,
    RacunInfoReklamacija        NVARCHAR(255) NULL,
    Email                        NVARCHAR(255) NULL,
    EmailDisplay                  NVARCHAR(255) NULL,
    PBrojSZ                        NVARCHAR(255) NULL,
    GradSZ                          NVARCHAR(255) NULL,
    Logo                            NVARCHAR(255) NULL,
    Field1                           NVARCHAR(255) NULL, -- ASSUMPTION: nejasna namena, proveriti
    QRName                            NVARCHAR(255) NULL,
    CONSTRAINT FK_Skustina_Naselje FOREIGN KEY (NaseljeId) REFERENCES dbo.Naselje(NaseljeId),
    CONSTRAINT FK_Skustina_Staff FOREIGN KEY (UpravnikId) REFERENCES dbo.Staff(StaffId)
);

CREATE TABLE dbo.Kupac (
    KupacId             INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Kupac PRIMARY KEY,
    Naziv               NVARCHAR(100) NULL,
    PBroj               NVARCHAR(50) NULL,
    Adresa              NVARCHAR(50) NULL,
    MB                  NVARCHAR(50) NULL,
    PIB                 NVARCHAR(50) NULL,
    AdresaUgovor        NVARCHAR(255) NULL,
    Telefon             NVARCHAR(50) NULL,
    Email               NVARCHAR(100) NULL,
    Napomena            NVARCHAR(255) NULL,
    TekuciRacunKupca    NVARCHAR(50) NULL, -- TR_K
    Web                 NVARCHAR(50) NULL,
    SkustinaId          INT NULL, -- lnk_ID_SK
    NaseljeId           INT NULL,
    PrintNaziv          NVARCHAR(255) NULL,
    PAK                 NVARCHAR(50) NULL,
    Konto               NVARCHAR(50) NULL,
    ExterniKonto        NVARCHAR(255) NULL,
    PDPrefix            NVARCHAR(255) NULL,
    LK                  NVARCHAR(255) NULL,
    Tip                 INT NULL, -- diskriminator kupac/dobavljač/drugo — PITANJE 3.1
    AutoKontoTroska     NVARCHAR(50) NULL,
    GrupniRacunMasterKupacId INT NULL, -- self FK — PITANJE 3.2
    SkipPrintRacunGrupa BIT NULL,
    PDVObaveznik        BIT NULL,
    JBJKS                NVARCHAR(255) NULL,
    KupacGrad             NVARCHAR(255) NULL,
    ZemljaKod              NVARCHAR(255) NULL,
    MasterKupacId            INT NULL, -- self FK — PITANJE 3.2
    LokacijaDostava           NVARCHAR(255) NULL,
    DostavaSifraPD              INT NULL,
    PrintInvoiceMandatory        BIT NULL,
    SendToPostOffice               BIT NULL,
    Language                        NVARCHAR(255) NULL,
    NapomenaExtended                  NVARCHAR(MAX) NULL,
    CONSTRAINT FK_Kupac_Skustina FOREIGN KEY (SkustinaId) REFERENCES dbo.Skustina(SkustinaId),
    CONSTRAINT FK_Kupac_Naselje FOREIGN KEY (NaseljeId) REFERENCES dbo.Naselje(NaseljeId),
    CONSTRAINT FK_Kupac_MasterKupac FOREIGN KEY (MasterKupacId) REFERENCES dbo.Kupac(KupacId),
    CONSTRAINT FK_Kupac_GrupniRacunMasterKupac FOREIGN KEY (GrupniRacunMasterKupacId) REFERENCES dbo.Kupac(KupacId)
);

-- razrešava kružnu zavisnost Skustina <-> Kupac
ALTER TABLE dbo.Skustina ADD CONSTRAINT FK_Skustina_Kupac_Predstavnik
    FOREIGN KEY (PredstavnikSSId) REFERENCES dbo.Kupac(KupacId);

CREATE TABLE dbo.Objekti (
    ObjekatId       INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Objekti PRIMARY KEY,
    SkustinaId      INT NOT NULL, -- ERROR_020
    Naziv           NVARCHAR(255) NULL,
    KupacId         INT NOT NULL, -- lnk_ID_K, vlasnik — ERROR_005/ERROR_020
    TipObjektaId    INT NULL,
    Status          INT NULL,
    IO              DECIMAL(19,4) NULL, -- ASSUMPTION: nejasna namena kolone "IO", proveriti pre finalizacije tipa
    RF_DIN          DECIMAL(19,4) NULL, -- rezervni fond u dinarima (pretpostavka)
    IONaslov        NVARCHAR(50) NULL,
    StaraKv         DECIMAL(9,4) NULL,
    StatusPromene   NVARCHAR(50) NULL,
    Koeficijent     DECIMAL(9,4) NULL,
    Ukupno          DECIMAL(19,4) NULL,
    Ulaz            NVARCHAR(50) NULL,
    Kategorija      NVARCHAR(5) NULL,
    Adresa          NVARCHAR(50) NULL,
    Napomena        NVARCHAR(255) NULL,
    NazivSlanja     NVARCHAR(50) NULL,
    AdresaSlanja    NVARCHAR(50) NULL,
    PBrojSlanja     NVARCHAR(50) NULL,
    PIBSlanja       NVARCHAR(50) NULL,
    PlatilacKupacId  INT NULL, -- PITANJE 3.3
    PlatilacKupacId2 INT NULL, -- PITANJE 3.3
    BRGM            INT NULL,
    K1 DECIMAL(9,4) NULL, K2 DECIMAL(9,4) NULL, K3 DECIMAL(9,4) NULL, K4 DECIMAL(9,4) NULL, K5 DECIMAL(9,4) NULL,
    PakSlanja       NVARCHAR(50) NULL,
    KV              DECIMAL(9,4) NULL,
    BrStanara       INT NULL,
    BrojPD          INT NULL,
    VlasnikKupacId  INT NULL,
    ZakupacKupacId  INT NULL,
    GrupniRacunId   INT NULL, -- ASSUMPTION: FK cilj nepotvrđen (GrupaRacuna ili Kupac?), ostaje bez FK constrainta za sada
    SifraPD         NVARCHAR(255) NULL,
    NetoKV          DECIMAL(9,4) NULL,
    Terasa          DECIMAL(9,4) NULL,
    NetoKVSaTerasom DECIMAL(9,4) NULL,
    HandOverDate    DATE NULL,
    SpratN          INT NULL,
    SpratT          NVARCHAR(255) NULL,
    CONSTRAINT FK_Objekti_Skustina FOREIGN KEY (SkustinaId) REFERENCES dbo.Skustina(SkustinaId),
    CONSTRAINT FK_Objekti_Kupac FOREIGN KEY (KupacId) REFERENCES dbo.Kupac(KupacId),
    CONSTRAINT FK_Objekti_TipObjekta FOREIGN KEY (TipObjektaId) REFERENCES dbo.TipObjekta(TipObjektaId),
    CONSTRAINT FK_Objekti_PlatilacKupac FOREIGN KEY (PlatilacKupacId) REFERENCES dbo.Kupac(KupacId),
    CONSTRAINT FK_Objekti_PlatilacKupac2 FOREIGN KEY (PlatilacKupacId2) REFERENCES dbo.Kupac(KupacId),
    CONSTRAINT FK_Objekti_VlasnikKupac FOREIGN KEY (VlasnikKupacId) REFERENCES dbo.Kupac(KupacId),
    CONSTRAINT FK_Objekti_ZakupacKupac FOREIGN KEY (ZakupacKupacId) REFERENCES dbo.Kupac(KupacId)
);

CREATE TABLE dbo.SzUlaz (
    SzUlazId    INT NOT NULL CONSTRAINT PK_SzUlaz PRIMARY KEY, -- IDUlaz
    SkustinaId  INT NULL,
    Ulaz        NVARCHAR(255) NULL,
    Zgrada      NVARCHAR(255) NULL,
    Adresa      NVARCHAR(255) NULL,
    Oznaka      NVARCHAR(255) NULL,
    Opis        NVARCHAR(255) NULL,
    Sort        INT NULL,
    CONSTRAINT FK_SzUlaz_Skustina FOREIGN KEY (SkustinaId) REFERENCES dbo.Skustina(SkustinaId)
);

CREATE TABLE dbo.SzObjekat (
    SzObjekatId INT NOT NULL CONSTRAINT PK_SzObjekat PRIMARY KEY, -- Id
    SkustinaId  INT NULL, -- SzId
    Objekat     NVARCHAR(255) NULL, -- ASSUMPTION: tekstualno polje, ne FK na Objekti — proveriti
    CONSTRAINT FK_SzObjekat_Skustina FOREIGN KEY (SkustinaId) REFERENCES dbo.Skustina(SkustinaId)
);

CREATE TABLE dbo.Godina (
    -- ASSUMPTION: kolona IDSZ u izvoru sugeriše da je PK ove tabele zapravo SkustinaId
    -- (1 red po skupštini po godini), ne nezavisan surogat — proveriti pre finalizacije.
    GodinaId    INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Godina PRIMARY KEY,
    SkustinaId  INT NOT NULL,
    Godina      INT NOT NULL,
    StartDatum  DATE NULL,
    EndDatum    DATE NULL,
    Arhivirana  BIT NULL,
    Prikaz      NVARCHAR(255) NULL,
    Folder      NVARCHAR(255) NULL,
    Datoteka    NVARCHAR(255) NULL,
    Aktuelna    BIT NULL,
    CONSTRAINT FK_Godina_Skustina FOREIGN KEY (SkustinaId) REFERENCES dbo.Skustina(SkustinaId)
);

-- =====================================================================
-- 4. KNJIGOVODSTVENA OSNOVA (Nalog, šifarnik podkonta) — pre GK, jer GK
--    zavisi od obe, ali nijedna od njih ne zavisi od GK
-- =====================================================================

CREATE TABLE dbo.Nalog (
    -- ASSUMPTION (pitanje 3.7): Br_Nalog je Double u izvoru; pretpostavljam da su u praksi
    -- svi brojevi naloga celi, pa prelazi u INT IDENTITY. Ako postoje decimalni brojevi
    -- naloga (npr. 123.1 za anex/storno), ovo treba promeniti pre finalizacije.
    NalogId       INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Nalog PRIMARY KEY,
    Datum         DATE NULL,
    Saldo         DECIMAL(19,4) NULL,
    Napomena      NVARCHAR(255) NULL,
    Reserve       NVARCHAR(50) NULL,
    OpisNaloga    NVARCHAR(50) NULL,
    SkustinaId    INT NULL, -- SZID
    DodatneNapomene NVARCHAR(255) NULL,
    TipNaloga     INT NULL,
    CONSTRAINT FK_Nalog_Skustina FOREIGN KEY (SkustinaId) REFERENCES dbo.Skustina(SkustinaId)
);

CREATE TABLE dbo.Troskovi_PodKonta (
    PodKonto           NVARCHAR(255) NOT NULL CONSTRAINT PK_Troskovi_PodKonta PRIMARY KEY,
    Naziv              NVARCHAR(255) NULL,
    SifraKnjPrethodni  NVARCHAR(255) NULL, -- self FK, hijerarhija — ERROR_102
    TrosakNa           NVARCHAR(255) NULL,
    Kamata             NVARCHAR(255) NULL,
    CONSTRAINT FK_Troskovi_PodKonta_Prethodni FOREIGN KEY (SifraKnjPrethodni) REFERENCES dbo.Troskovi_PodKonta(PodKonto)
);
-- NAPOMENA: ERROR_102 upoređuje kolone TKONTO.GRUPA1/GRUPA4 koje ne postoje kao fizičke kolone
-- ovde (TKONTO je verovatno UPIT, ne fizička tabela) — hijerarhija od 2 nivoa iz tog upita
-- treba potvrditi sa korisnicom pre finalizacije (možda nedostaje kolona Grupa1PodKonto/Grupa4PodKonto).

CREATE TABLE dbo.Troskovi_PodKonta_DefDob (
    SkustinaId       INT NOT NULL,
    PodKonto         NVARCHAR(255) NOT NULL,
    DefaultDobavljac NVARCHAR(255) NULL,
    CONSTRAINT PK_Troskovi_PodKonta_DefDob PRIMARY KEY (SkustinaId, PodKonto),
    CONSTRAINT FK_TPKDD_Skustina FOREIGN KEY (SkustinaId) REFERENCES dbo.Skustina(SkustinaId),
    CONSTRAINT FK_TPKDD_PodKonto FOREIGN KEY (PodKonto) REFERENCES dbo.Troskovi_PodKonta(PodKonto)
);

-- =====================================================================
-- 5. FAKTURISANJE + OPOMENE (opomene idu pre Racun jer Racun.OpomenaId
--    pokazuje na Opomena, a Opomena ne zavisi od Racun-a)
-- =====================================================================

CREATE TABLE dbo.GrupaRacuna (
    GrupaRacunaId       INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_GrupaRacuna PRIMARY KEY,
    GrupaRacunaFXN      NVARCHAR(50) NULL,
    GrupaRacunaFXT      NVARCHAR(50) NULL,
    Mesec               NVARCHAR(50) NULL,
    Godina              NVARCHAR(50) NULL,
    Mesto               NVARCHAR(50) NULL,
    DatumIzdavanja      DATE NULL,
    DatumUsluge         NVARCHAR(50) NULL,
    DatumPrometa        DATE NULL,
    DatumValute         DATE NULL,
    NBS                 DECIMAL(19,4) NULL,
    SkustinaId          INT NOT NULL,
    NalogId             INT NULL, -- NalogKN
    DatumSys            DATETIME2(0) NULL,
    StaffId             INT NULL, -- UserSys
    MarkerVanrednihRacuna NVARCHAR(255) NULL,
    VrstaRacuna         NVARCHAR(255) NULL,
    DatumStanja         DATE NULL,
    PrethodnaValuta     DATE NULL,
    ObracunKamate       BIT NULL,
    CONSTRAINT FK_GrupaRacuna_Skustina FOREIGN KEY (SkustinaId) REFERENCES dbo.Skustina(SkustinaId),
    CONSTRAINT FK_GrupaRacuna_Nalog FOREIGN KEY (NalogId) REFERENCES dbo.Nalog(NalogId),
    CONSTRAINT FK_GrupaRacuna_Staff FOREIGN KEY (StaffId) REFERENCES dbo.Staff(StaffId)
);

CREATE TABLE dbo.OpomenaSabloni (
    OpomenaSablonId  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_OpomenaSabloni PRIMARY KEY,
    VrstaOpomeneId   INT NULL,
    ReportName       NVARCHAR(255) NULL,
    Naslov           NVARCHAR(255) NULL,
    RptField01 NVARCHAR(255) NULL, RptField02 NVARCHAR(255) NULL,
    RptField03 NVARCHAR(MAX) NULL, RptField04 NVARCHAR(MAX) NULL, RptField05 NVARCHAR(MAX) NULL, RptField06 NVARCHAR(MAX) NULL,
    RptField07 NVARCHAR(255) NULL, RptField08 NVARCHAR(255) NULL, RptField09 NVARCHAR(255) NULL
);

CREATE TABLE dbo.GrupaOpomena (
    GrupaOpomenaId    INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_GrupaOpomena PRIMARY KEY,
    Naslov            NVARCHAR(50) NULL,
    Datum             DATE NULL,
    MinBNR            INT NULL,
    TolerancijaDuga   INT NULL,
    TolerancijaDugaPoMesecu INT NULL,
    OpomenaSablonId   INT NULL,
    VrstaOpomeneId    INT NULL,
    DatumDI           DATE NULL,
    DatumPI           DATE NULL,
    SkustinaId        INT NULL,
    GrupaRacunaId     INT NULL,
    GrupaOpomenaTxt   NVARCHAR(255) NULL,
    SablonTextOpomene NVARCHAR(255) NULL,
    Doznaka           NVARCHAR(255) NULL,
    CONSTRAINT FK_GrupaOpomena_OpomenaSabloni FOREIGN KEY (OpomenaSablonId) REFERENCES dbo.OpomenaSabloni(OpomenaSablonId),
    CONSTRAINT FK_GrupaOpomena_Skustina FOREIGN KEY (SkustinaId) REFERENCES dbo.Skustina(SkustinaId),
    CONSTRAINT FK_GrupaOpomena_GrupaRacuna FOREIGN KEY (GrupaRacunaId) REFERENCES dbo.GrupaRacuna(GrupaRacunaId)
);

CREATE TABLE dbo.Opomena (
    OpomenaId        INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Opomena PRIMARY KEY,
    GrupaOpomenaId   INT NULL,
    KupacId          INT NULL,
    BNR              INT NULL,
    Dug              DECIMAL(19,4) NULL,
    TxtRacunOp       NVARCHAR(255) NULL,
    AktivnaOpomena   BIT NULL,
    SumaPoStavkama   DECIMAL(19,4) NULL,
    PozivNaBroj      NVARCHAR(255) NULL,
    Troskovi         DECIMAL(19,4) NULL, -- bio Long Integer u izvoru, normalizovano u money
    Ukupno           DECIMAL(19,4) NULL, -- isto
    CONSTRAINT FK_Opomena_GrupaOpomena FOREIGN KEY (GrupaOpomenaId) REFERENCES dbo.GrupaOpomena(GrupaOpomenaId),
    CONSTRAINT FK_Opomena_Kupac FOREIGN KEY (KupacId) REFERENCES dbo.Kupac(KupacId)
);

CREATE TABLE dbo.Racun (
    RacunId             INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Racun PRIMARY KEY,
    RBR                 NVARCHAR(20) NULL,
    GrupaRacunaId        INT NOT NULL, -- ERROR_060: ne sme biti 0/nepovezano
    DatumIzdavanja       DATE NULL,
    MestoIzdavanja       NVARCHAR(50) NULL,
    DatumUsluge          NVARCHAR(50) NULL,
    DatumPrometa         DATE NULL,
    DatumValute          DATE NULL,
    KupacId              INT NOT NULL, -- ERROR_060
    SkustinaId           INT NOT NULL, -- ERROR_060
    Kupac                NVARCHAR(255) NULL, -- snapshot naziva kupca u trenutku izdavanja (namerna denormalizacija)
    PBrojK               NVARCHAR(50) NULL,
    AdresaK              NVARCHAR(50) NULL,
    PIB                  NVARCHAR(50) NULL,
    MB                   NVARCHAR(255) NULL,
    Suma                 DECIMAL(19,4) NULL,
    PDVStopa             DECIMAL(9,4) NULL,
    PDVIznos             DECIMAL(19,4) NULL,
    Ukupno               DECIMAL(19,4) NULL,
    PrethodniDug         DECIMAL(19,4) NULL,
    SvrhaUplate          NVARCHAR(255) NULL,
    Valuta               NVARCHAR(50) NULL,
    PozivNaBroj          NVARCHAR(50) NULL,
    PDText               NVARCHAR(255) NULL,
    Napomena             NVARCHAR(255) NULL,
    Co                   INT NULL, -- ASSUMPTION: nejasna namena
    Storno               BIT NOT NULL DEFAULT 0,
    ExtraNapomena        NVARCHAR(255) NULL,
    ObjekatNaziv         NVARCHAR(50) NULL,
    Upravnik              INT NULL,
    SvrhaUplate2          NVARCHAR(255) NULL,
    ObjekatId             INT NULL, -- ID_OX
    AdresaProstora        NVARCHAR(255) NULL,
    ZaUplatu               DECIMAL(19,4) NULL,
    NazivSlanja            NVARCHAR(50) NULL,
    AdresaSlanja            NVARCHAR(50) NULL,
    PBrojSlanja              NVARCHAR(50) NULL,
    PIBSlanja                NVARCHAR(50) NULL,
    SPC                       INT NULL,
    PDIznos                   DECIMAL(19,4) NULL,
    TipPoljaZaUplatu           INT NULL,
    OpomenaId                   INT NULL,
    GrupniRacunKupacId           INT NULL, -- IDKGrupniRacun -> Kupac
    GradK                         NVARCHAR(255) NULL,
    Lokacija                      NVARCHAR(255) NULL,
    SortRacun                      INT NULL,
    DatumStorno                     DATE NULL,
    RacunShema                       NVARCHAR(255) NULL,
    DatumStanja                       DATE NULL,
    KamataIznos                       DECIMAL(19,4) NULL,
    UkupnoRacun                        DECIMAL(19,4) NULL,
    -- NAPOMENA: kolone tmpID/tmpID2/tmpPB/tmpPB2 iz izvora namerno izostavljene — po imenu su
    -- to radne/privremene kolone iz stare aplikacije, ne trajni poslovni podaci.
    CONSTRAINT FK_Racun_GrupaRacuna FOREIGN KEY (GrupaRacunaId) REFERENCES dbo.GrupaRacuna(GrupaRacunaId),
    CONSTRAINT FK_Racun_Kupac FOREIGN KEY (KupacId) REFERENCES dbo.Kupac(KupacId),
    CONSTRAINT FK_Racun_Skustina FOREIGN KEY (SkustinaId) REFERENCES dbo.Skustina(SkustinaId),
    CONSTRAINT FK_Racun_Objekti FOREIGN KEY (ObjekatId) REFERENCES dbo.Objekti(ObjekatId),
    CONSTRAINT FK_Racun_Opomena FOREIGN KEY (OpomenaId) REFERENCES dbo.Opomena(OpomenaId),
    CONSTRAINT FK_Racun_GrupniRacunKupac FOREIGN KEY (GrupniRacunKupacId) REFERENCES dbo.Kupac(KupacId)
);

CREATE TABLE dbo.OpomenaStavke (
    OpomenaStavkaId  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_OpomenaStavke PRIMARY KEY,
    OpomenaId        INT NULL,
    GrupaOpomenaId   INT NULL,
    KupacId          INT NULL,
    Parametri        NVARCHAR(25) NULL,
    Dok              NVARCHAR(255) NULL,
    Di               DECIMAL(19,4) NULL,
    Pi               DECIMAL(19,4) NULL,
    Suma             DECIMAL(19,4) NULL,
    MTxt             NVARCHAR(255) NULL,
    DatumDospeca     DATE NULL,
    RacunId          INT NULL,
    DatumRacuna      DATE NULL,
    PProstor         NVARCHAR(255) NULL,
    CONSTRAINT FK_OpomenaStavke_Opomena FOREIGN KEY (OpomenaId) REFERENCES dbo.Opomena(OpomenaId) ON DELETE CASCADE,
    CONSTRAINT FK_OpomenaStavke_GrupaOpomena FOREIGN KEY (GrupaOpomenaId) REFERENCES dbo.GrupaOpomena(GrupaOpomenaId),
    CONSTRAINT FK_OpomenaStavke_Kupac FOREIGN KEY (KupacId) REFERENCES dbo.Kupac(KupacId),
    CONSTRAINT FK_OpomenaStavke_Racun FOREIGN KEY (RacunId) REFERENCES dbo.Racun(RacunId)
);

-- =====================================================================
-- 6. BANKARSKI IZVODI
-- =====================================================================

CREATE TABLE dbo.Izvod (
    IzvodId               INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Izvod PRIMARY KEY,
    BrojIzvoda            INT NULL,
    SufixIzvoda           NVARCHAR(50) NULL,
    SkustinaId            INT NOT NULL, -- ERROR_009
    Datum                 DATE NULL,
    PrethodnoStanjeIzvoda DECIMAL(19,4) NULL,
    NovoStanje            DECIMAL(19,4) NULL,
    Duguje                DECIMAL(19,4) NULL,
    Potrazuje             DECIMAL(19,4) NULL,
    NalogaZaduzenja       INT NULL,
    NalogaOdobranja       INT NULL,
    Napomena              NVARCHAR(50) NULL,
    NalogId               INT NULL, -- NalogZaKnjizenje
    Rasknjizen            BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_Izvod_Skustina FOREIGN KEY (SkustinaId) REFERENCES dbo.Skustina(SkustinaId),
    CONSTRAINT FK_Izvod_Nalog FOREIGN KEY (NalogId) REFERENCES dbo.Nalog(NalogId)
);

CREATE TABLE dbo.IzvodStavke (
    IzvodStavkaId    INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_IzvodStavke PRIMARY KEY,
    IzvodId          INT NOT NULL, -- ERROR_006
    SkustinaId       INT NULL, -- mora = Izvod.SkustinaId, ERROR_012
    RbStavke         INT NULL,
    RbNaloga         INT NULL,
    NazivPN          NVARCHAR(255) NULL,
    BrojRacuna       NVARCHAR(50) NULL,
    Poreklo          NVARCHAR(50) NULL,
    DatumRealizacije DATE NULL,
    Zaduzenje        DECIMAL(19,4) NULL,
    Odobrenje        DECIMAL(19,4) NULL,
    Doznaka          NVARCHAR(255) NULL,
    Sifra            INT NULL,
    PozivNaBroj      NVARCHAR(50) NULL,
    PozivNaBrojO     NVARCHAR(50) NULL,
    KupacId          INT NULL, -- opt_lnk_Kupac, mora = GK.KupacId povezane stavke, ERROR_013
    Knjizeno         BIT NOT NULL DEFAULT 0,
    Ignore           BIT NOT NULL DEFAULT 0,
    SetP             BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_IzvodStavke_Izvod FOREIGN KEY (IzvodId) REFERENCES dbo.Izvod(IzvodId) ON DELETE CASCADE,
    CONSTRAINT FK_IzvodStavke_Skustina FOREIGN KEY (SkustinaId) REFERENCES dbo.Skustina(SkustinaId),
    CONSTRAINT FK_IzvodStavke_Kupac FOREIGN KEY (KupacId) REFERENCES dbo.Kupac(KupacId)
);

-- =====================================================================
-- 7. DOBAVLJAČI (pre GK — GK.DobavljacRacunId pokazuje ovde)
-- =====================================================================

CREATE TABLE dbo.Dobavljac_Racuni (
    DobavljacRacunId  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Dobavljac_Racuni PRIMARY KEY, -- IDTRRAC
    SkustinaId        INT NULL, -- SK_ID
    RacunNo           INT NULL,
    NazivRacuna       NVARCHAR(255) NULL,
    Napomena          NVARCHAR(255) NULL,
    DobavljacNaziv    NVARCHAR(50) NULL, -- tekstualni naziv, verovatno redundantan sa DobavljacKupacId.Naziv
    DobavljacKupacId  INT NULL, -- DobavljacKonto -> Kupac; ERROR_032/042 potvrđuju da su dobavljači redovi u Kupac (pitanje 3.1)
    TipObracunaId     INT NULL,
    MesecRacuna       NVARCHAR(50) NULL,
    IznosRacunaEUR    DECIMAL(19,4) NULL,
    IznosRacunaRSD    DECIMAL(19,4) NULL,
    IznosPoKoefEUR    DECIMAL(19,4) NULL,
    IznosPoKoefRSD    DECIMAL(19,4) NULL,
    PrioritetNaplate  INT NULL,
    SifraKN           NVARCHAR(6) NULL,
    TmpPrevId         INT NULL, -- ASSUMPTION: "tmp" u nazivu — proveriti da li je zaista privremeno polje
    KontoKnjizenja    NVARCHAR(255) NULL,
    TipDokumentaId    INT NULL,
    MarkerVanrednogRacuna NVARCHAR(255) NULL,
    FunkcijaNazivaRacuna NVARCHAR(255) NULL,
    RBR               NVARCHAR(50) NULL,
    IznosRacunaKN     DECIMAL(19,4) NULL,
    DatumRacuna       DATE NULL,
    DatumKnjizenja    DATE NULL,
    DatumPlacanja     DATE NULL,
    OpisRacuna        NVARCHAR(255) NULL,
    PozivNaBroj       NVARCHAR(255) NULL,
    PrethodniDobavljacRacunId INT NULL, -- self FK
    NoviDobavljacRacunId      INT NULL, -- self FK
    NalogId           INT NULL, -- NalogKnjizenja
    PDV               INT NULL,
    ZatvaraKonto      NVARCHAR(255) NULL,
    CONSTRAINT FK_Dobavljac_Racuni_Skustina FOREIGN KEY (SkustinaId) REFERENCES dbo.Skustina(SkustinaId),
    CONSTRAINT FK_Dobavljac_Racuni_Kupac FOREIGN KEY (DobavljacKupacId) REFERENCES dbo.Kupac(KupacId),
    CONSTRAINT FK_Dobavljac_Racuni_Nalog FOREIGN KEY (NalogId) REFERENCES dbo.Nalog(NalogId),
    CONSTRAINT FK_Dobavljac_Racuni_Prethodni FOREIGN KEY (PrethodniDobavljacRacunId) REFERENCES dbo.Dobavljac_Racuni(DobavljacRacunId),
    CONSTRAINT FK_Dobavljac_Racuni_Novi FOREIGN KEY (NoviDobavljacRacunId) REFERENCES dbo.Dobavljac_Racuni(DobavljacRacunId)
);

CREATE TABLE dbo.RacunIN ( -- STATUS NEPOZNAT — pitanje 3.9: da li se i dalje koristi paralelno sa Dobavljac_Racuni
    RacunInId       INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_RacunIN PRIMARY KEY,
    PartnerKupacId  INT NULL,
    SkustinaId      INT NULL,
    BrojRacunaIn    NVARCHAR(255) NULL,
    DatumRacuna     DATE NULL,
    DatumKnjizenja  DATE NULL,
    DatumPlacanja   DATE NULL,
    KontoTroska     NVARCHAR(255) NULL,
    IznosRacuna     DECIMAL(19,4) NULL,
    CONSTRAINT FK_RacunIN_Kupac FOREIGN KEY (PartnerKupacId) REFERENCES dbo.Kupac(KupacId),
    CONSTRAINT FK_RacunIN_Skustina FOREIGN KEY (SkustinaId) REFERENCES dbo.Skustina(SkustinaId)
);

CREATE TABLE dbo.KnjiznaDokumenta (
    KnjiznoDokumentId  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_KnjiznaDokumenta PRIMARY KEY,
    SkustinaId         INT NULL,
    KnjiznoDokumentIdRef INT NULL, -- ASSUMPTION: nejasno na šta IDKR tačno pokazuje, proveriti
    TipDokumentaId     INT NULL,
    Iznos              DECIMAL(19,4) NULL,
    NalogId            INT NULL,
    Datum              DATE NULL,
    DokumentNaziv      NVARCHAR(255) NULL,
    DokumentOpis       NVARCHAR(255) NULL,
    KontoTroska        INT NULL,
    TipGrupaId         INT NULL,
    RefFromKnjiznoDokumentId INT NULL, -- self FK
    PB                 NVARCHAR(30) NULL,
    CONSTRAINT FK_KnjiznaDokumenta_Skustina FOREIGN KEY (SkustinaId) REFERENCES dbo.Skustina(SkustinaId),
    CONSTRAINT FK_KnjiznaDokumenta_Nalog FOREIGN KEY (NalogId) REFERENCES dbo.Nalog(NalogId),
    CONSTRAINT FK_KnjiznaDokumenta_Self FOREIGN KEY (RefFromKnjiznoDokumentId) REFERENCES dbo.KnjiznaDokumenta(KnjiznoDokumentId)
);

-- =====================================================================
-- 8. GK — glavna knjiga (centralna tabela; sve zavisnosti sada zadovoljene)
-- =====================================================================

CREATE TABLE dbo.GK (
    GKStavkaId        INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_GK PRIMARY KEY, -- STAVKAID
    NalogId           INT NOT NULL,
    Konto             NVARCHAR(50) NOT NULL,
    Datum             DATE NOT NULL, -- ERROR_902: ne sme biti NULL
    Diznos            DECIMAL(19,4) NOT NULL DEFAULT 0,
    Piznos            DECIMAL(19,4) NOT NULL DEFAULT 0,
    TipStavke         INT NULL,
    Dok               NVARCHAR(50) NULL,
    SkustinaId        INT NOT NULL,
    KupacId           INT NULL, -- lnkKUPACID (= SIFRAKONTA u izvoru — ta kolona namerno izostavljena, pitanje 3.4)
    IzvodStavkaId     INT NULL,
    Napomena          NVARCHAR(255) NULL,
    Parametri         NVARCHAR(25) NULL,
    Opis              NVARCHAR(255) NULL,
    Dpo               DATE NULL, -- ASSUMPTION: tačno poslovno značenje "DPO" nije potvrđeno — proveriti (ERROR_016 ga tretira kao obavezan uz konto 204x)
    SifraKN           NVARCHAR(6) NULL,
    DobavljacRacunId  INT NULL, -- RDOB u izvoru
    RacunId           INT NULL,
    Prioritet         INT NULL,
    KnZaTip           INT NULL,
    RacunInId         INT NULL,
    KontoTroska       NVARCHAR(255) NULL,
    KnjiznoDokumentId INT NULL,
    CONSTRAINT FK_GK_Nalog FOREIGN KEY (NalogId) REFERENCES dbo.Nalog(NalogId),
    CONSTRAINT FK_GK_Skustina FOREIGN KEY (SkustinaId) REFERENCES dbo.Skustina(SkustinaId),
    CONSTRAINT FK_GK_Kupac FOREIGN KEY (KupacId) REFERENCES dbo.Kupac(KupacId),
    CONSTRAINT FK_GK_IzvodStavke FOREIGN KEY (IzvodStavkaId) REFERENCES dbo.IzvodStavke(IzvodStavkaId),
    CONSTRAINT FK_GK_Racun FOREIGN KEY (RacunId) REFERENCES dbo.Racun(RacunId),
    CONSTRAINT FK_GK_DobavljacRacuni FOREIGN KEY (DobavljacRacunId) REFERENCES dbo.Dobavljac_Racuni(DobavljacRacunId),
    CONSTRAINT FK_GK_RacunIN FOREIGN KEY (RacunInId) REFERENCES dbo.RacunIN(RacunInId),
    CONSTRAINT FK_GK_Troskovi_PodKonta FOREIGN KEY (KontoTroska) REFERENCES dbo.Troskovi_PodKonta(PodKonto),
    CONSTRAINT FK_GK_KnjiznaDokumenta FOREIGN KEY (KnjiznoDokumentId) REFERENCES dbo.KnjiznaDokumenta(KnjiznoDokumentId)
);
GO

-- ERROR_104: nalog mora biti u ravnoteži (SUM(Diznos) = SUM(Piznos) po NalogId).
-- Ovo je NACRT triggera, ne konačna odluka — pre uvođenja proveriti da li postoje legitimni
-- prelazni koraci (knjiženje stavka-po-stavku unutar iste transakcije) koje bi ovo lažno oborilo;
-- alternativa je provera na nivou aplikacione transakcije umesto AFTER triggera po redu.
CREATE TRIGGER dbo.TR_GK_ProveriRavnotezuNaloga ON dbo.GK
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (
        SELECT NalogId
        FROM dbo.GK
        WHERE NalogId IN (SELECT NalogId FROM inserted UNION SELECT NalogId FROM deleted)
        GROUP BY NalogId
        HAVING ROUND(SUM(Diznos) - SUM(Piznos), 2) <> 0
    )
        THROW 50001, N'Nalog nije u ravnoteži (duguje <> potražuje).', 1;
END;
GO

-- =====================================================================
-- 9. STAVKE FAKTURE, BENEFITI, KAMATA
-- =====================================================================

CREATE TABLE dbo.RacunStavke (
    RacunStavkeId   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_RacunStavke PRIMARY KEY,
    RacunId         INT NOT NULL, -- ERROR_071
    Naziv           NVARCHAR(255) NULL,
    TipObracunaId   INT NULL,
    K1 DECIMAL(9,4) NULL, K2 DECIMAL(9,4) NULL, K3 DECIMAL(9,4) NULL, K4 DECIMAL(9,4) NULL, K5 DECIMAL(9,4) NULL,
    Kolicina        DECIMAL(9,4) NULL,
    CenaE           DECIMAL(19,4) NULL,
    NBS             DECIMAL(19,4) NULL,
    Iznos           DECIMAL(19,4) NULL,
    Suma            DECIMAL(19,4) NULL,
    PDVStopa        DECIMAL(9,4) NULL,
    PDVIznos        DECIMAL(19,4) NULL,
    UkupnoRSD       DECIMAL(19,4) NULL,
    Sort            INT NULL,
    DobavljacKontoKupacId INT NULL,
    IznosRacuna     DECIMAL(19,4) NULL,
    K1xK2 DECIMAL(9,4) NULL, K2xK3 DECIMAL(9,4) NULL, K2xK4 DECIMAL(9,4) NULL, K2xK5 DECIMAL(9,4) NULL,
    JM              NVARCHAR(255) NULL,
    ObjekatId       INT NULL,
    M               DECIMAL(9,4) NULL,
    DobavljacRacunId INT NULL, -- ID_RDOB
    -- NAPOMENA VAŽNE ODLUKE (pitanje 3.5, primenjena moja preporuka u ovom nacrtu):
    -- lnkGR/ID_K/ID_SK iz izvora su NAMERNO IZOSTAVLJENI — izvode se JOIN-om preko RacunId -> Racun,
    -- čime ERROR_062/063/064 (stavka se razminula sa zaglavljem) postaju strukturno nemoguće.
    -- Ako postoji poslovni razlog da stavka "zamrzne" svoju skupštinu/kupca nezavisno od zaglavlja
    -- (npr. promena vlasništva posle izdavanja), ovo treba vratiti kao denormalizovane kolone.
    CONSTRAINT FK_RacunStavke_Racun FOREIGN KEY (RacunId) REFERENCES dbo.Racun(RacunId) ON DELETE CASCADE,
    CONSTRAINT FK_RacunStavke_Objekti FOREIGN KEY (ObjekatId) REFERENCES dbo.Objekti(ObjekatId),
    CONSTRAINT FK_RacunStavke_DobavljacRacuni FOREIGN KEY (DobavljacRacunId) REFERENCES dbo.Dobavljac_Racuni(DobavljacRacunId),
    CONSTRAINT FK_RacunStavke_DobavljacKontoKupac FOREIGN KEY (DobavljacKontoKupacId) REFERENCES dbo.Kupac(KupacId)
);

CREATE TABLE dbo.RacunObjekti ( -- M:N most, bez sopstvenog PK u izvoru
    RacunId    INT NOT NULL,
    ObjekatId  INT NOT NULL,
    CONSTRAINT PK_RacunObjekti PRIMARY KEY (RacunId, ObjekatId),
    CONSTRAINT FK_RacunObjekti_Racun FOREIGN KEY (RacunId) REFERENCES dbo.Racun(RacunId),
    CONSTRAINT FK_RacunObjekti_Objekti FOREIGN KEY (ObjekatId) REFERENCES dbo.Objekti(ObjekatId)
);

CREATE TABLE dbo.RacunStavkeBenefitArhiva (
    -- Arhivski snapshot — zadržava originalni ID iz izvora (ne identity), i NAMERNO nema FK
    -- prema živim tabelama (istorijski zapis sme nadživeti obrisane/izmenjene redove).
    RacunStavkeId   INT NOT NULL CONSTRAINT PK_RacunStavkeBenefitArhiva PRIMARY KEY,
    RacunId         INT NULL,
    GrupaRacunaId   INT NULL,
    KupacId         INT NULL,
    SkustinaId      INT NULL,
    DobavljacRacunId INT NULL,
    Naziv           NVARCHAR(255) NULL,
    TipObracunaId   INT NULL,
    K1 DECIMAL(9,4) NULL, K2 DECIMAL(9,4) NULL, K3 DECIMAL(9,4) NULL, K4 DECIMAL(9,4) NULL, K5 DECIMAL(9,4) NULL,
    Kolicina DECIMAL(9,4) NULL, CenaE DECIMAL(19,4) NULL, NBS DECIMAL(19,4) NULL,
    Iznos DECIMAL(19,4) NULL, Suma DECIMAL(19,4) NULL, PDVStopa DECIMAL(9,4) NULL, PDVIznos DECIMAL(19,4) NULL,
    UkupnoRSD DECIMAL(19,4) NULL, Sort INT NULL, DobavljacKontoKupacId INT NULL, IznosRacuna DECIMAL(19,4) NULL,
    K1xK2 DECIMAL(9,4) NULL, K2xK3 DECIMAL(9,4) NULL, K2xK4 DECIMAL(9,4) NULL, K2xK5 DECIMAL(9,4) NULL,
    JM NVARCHAR(255) NULL, ObjekatId INT NULL
);

CREATE TABLE dbo.BenefitGrupa (
    BenefitGrupaId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_BenefitGrupa PRIMARY KEY,
    BenefitNaziv   NVARCHAR(255) NULL,
    BenefitPrint   NVARCHAR(255) NULL
);

CREATE TABLE dbo.Benefiti (
    BenefitId       INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Benefiti PRIMARY KEY,
    ObjekatId       INT NULL,
    KupacId         INT NULL,
    MesecYYMM       NVARCHAR(255) NULL,
    Used            BIT NULL,
    DateEntry       DATETIME2(0) NULL,
    DateUsed        DATETIME2(0) NULL,
    RacunId         INT NULL,
    BenefitGrupaId  INT NULL,
    RacunStornoId   INT NULL, -- -> Racun
    CONSTRAINT FK_Benefiti_Objekti FOREIGN KEY (ObjekatId) REFERENCES dbo.Objekti(ObjekatId),
    CONSTRAINT FK_Benefiti_Kupac FOREIGN KEY (KupacId) REFERENCES dbo.Kupac(KupacId),
    CONSTRAINT FK_Benefiti_Racun FOREIGN KEY (RacunId) REFERENCES dbo.Racun(RacunId),
    CONSTRAINT FK_Benefiti_RacunStorno FOREIGN KEY (RacunStornoId) REFERENCES dbo.Racun(RacunId),
    CONSTRAINT FK_Benefiti_BenefitGrupa FOREIGN KEY (BenefitGrupaId) REFERENCES dbo.BenefitGrupa(BenefitGrupaId)
);

CREATE TABLE dbo.KamatniList (
    KamatniListId   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_KamatniList PRIMARY KEY,
    SkustinaId      INT NULL,
    Konto           NVARCHAR(50) NULL,
    Datum           DATE NULL,
    DIPI            DECIMAL(19,4) NULL,
    Saldo           DECIMAL(19,4) NULL,
    Dana            INT NULL,
    Stopa           DECIMAL(9,4) NULL,
    Koeficijent     DECIMAL(18,8) NULL,
    Kamata          DECIMAL(19,4) NULL,
    Par             NVARCHAR(50) NULL,
    PartnerKupacId  INT NULL,
    ObjekatId       INT NULL,
    PodKonto        NVARCHAR(255) NULL,
    GrupaRacunaId   INT NULL,
    CONSTRAINT FK_KamatniList_Skustina FOREIGN KEY (SkustinaId) REFERENCES dbo.Skustina(SkustinaId),
    CONSTRAINT FK_KamatniList_Kupac FOREIGN KEY (PartnerKupacId) REFERENCES dbo.Kupac(KupacId),
    CONSTRAINT FK_KamatniList_Objekti FOREIGN KEY (ObjekatId) REFERENCES dbo.Objekti(ObjekatId),
    CONSTRAINT FK_KamatniList_GrupaRacuna FOREIGN KEY (GrupaRacunaId) REFERENCES dbo.GrupaRacuna(GrupaRacunaId),
    CONSTRAINT FK_KamatniList_PodKonto FOREIGN KEY (PodKonto) REFERENCES dbo.Troskovi_PodKonta(PodKonto)
);

CREATE TABLE dbo.Stope ( -- istorijat zakonske/ugovorne kamatne stope
    -- NAPOMENA: u izvoru fizički živi u SZAPP.mdb (front-end), ne u data.mdb, iako je prava
    -- referentna poslovna tabela — u novoj šemi seli se u istu bazu kao i ostalo.
    StopaId  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Stope PRIMARY KEY,
    Datum    DATE NULL,
    Stopa    DECIMAL(19,4) NULL,
    Period   NVARCHAR(50) NULL
);

-- =====================================================================
-- 10. BANKOVNI RAČUNI (firme i partnera) + preostale knjigovodstvene
--     pomoćne tabele
-- =====================================================================

CREATE TABLE dbo.TekuciRacun (
    TekuciRacunId  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TekuciRacun PRIMARY KEY,
    TR             NVARCHAR(255) NULL,
    Aktivan        BIT NULL,
    SortOrder      INT NULL,
    PartnerKupacId INT NULL,
    SkustinaId     INT NULL,
    UpravnikId     INT NULL, -- -> Staff
    CONSTRAINT FK_TekuciRacun_Skustina FOREIGN KEY (SkustinaId) REFERENCES dbo.Skustina(SkustinaId),
    CONSTRAINT FK_TekuciRacun_Kupac FOREIGN KEY (PartnerKupacId) REFERENCES dbo.Kupac(KupacId),
    CONSTRAINT FK_TekuciRacun_Staff FOREIGN KEY (UpravnikId) REFERENCES dbo.Staff(StaffId)
);

CREATE TABLE dbo.PartnerTekuciRacuni ( -- izvorno "TRs"
    PartnerTekuciRacunId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PartnerTekuciRacuni PRIMARY KEY,
    TekuciRacunBroj       NVARCHAR(50) NULL, -- TR_L
    KupacId                INT NULL,
    CONSTRAINT FK_PartnerTekuciRacuni_Kupac FOREIGN KEY (KupacId) REFERENCES dbo.Kupac(KupacId)
);

CREATE TABLE dbo.SemaKnjizenja (
    -- Konfiguracioni podaci (šablon automatskog knjiženja). SourceSQL kolona sadrži stari Access
    -- SQL i BIĆE ODBAČENA/PREPISANA kao deo migracije logike, ne prenosi se doslovno.
    SemaKnjizenjaId  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SemaKnjizenja PRIMARY KEY,
    SemaKnjizenja    NVARCHAR(255) NULL,
    SourceTable      NVARCHAR(255) NULL,
    SourceTableWhrField NVARCHAR(255) NULL,
    Stavka           NVARCHAR(255) NULL,
    Konto            NVARCHAR(255) NULL,
    DIPI             NVARCHAR(255) NULL,
    Znak             INT NULL,
    SubAnlField      NVARCHAR(255) NULL,
    SubAnlFieldSource NVARCHAR(255) NULL,
    Opis             NVARCHAR(255) NULL,
    GKTipStavke      INT NULL,
    GKDokFn          NVARCHAR(255) NULL,
    GKPNB            NVARCHAR(255) NULL,
    GKIDPartner      NVARCHAR(255) NULL,
    GKRacunId        NVARCHAR(255) NULL,
    Sort             INT NULL,
    NalogOpis        NVARCHAR(255) NULL,
    SourceSQL        NVARCHAR(MAX) NULL,
    SourceSQLValue   NVARCHAR(255) NULL
);

CREATE TABLE dbo.TemplateIzvodaKnjizenje (
    TemplateIzvodaKnjizenjeId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TemplateIzvodaKnjizenje PRIMARY KEY,
    TemplateId    INT NULL,
    Template      NVARCHAR(255) NULL,
    FieldName     NVARCHAR(255) NULL,
    FieldValue    NVARCHAR(255) NULL,
    Function      NVARCHAR(255) NULL,
    SetId         INT NULL,
    FFunction     INT NULL,
    SetKonto      NVARCHAR(50) NULL
);

CREATE TABLE dbo.ZK ( -- STATUS NEPOZNAT — pitanje 3.8: da li se i dalje koristi. Struktura skoro identična GK.
    ZKId          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ZK PRIMARY KEY,
    SkustinaId    INT NULL,
    NalogId       INT NULL,
    Konto         NVARCHAR(50) NULL,
    Datum         DATE NULL,
    Piznos        DECIMAL(19,4) NULL,
    Diznos        DECIMAL(19,4) NULL,
    TipStavke     INT NULL,
    Parametri     NVARCHAR(25) NULL,
    Napomena      NVARCHAR(20) NULL,
    PartnerKupacId INT NULL,
    DatumValute   DATE NULL,
    RacunId       INT NULL,
    Dokument      NVARCHAR(255) NULL,
    PodKonto      NVARCHAR(255) NULL,
    GrupaRacunaId INT NULL,
    KNPodKonto    NVARCHAR(255) NULL,
    KNDob         INT NULL,
    RDOB          INT NULL,
    Prioritet     INT NULL
);

CREATE TABLE dbo.RacunStavke_Troskovi (
    -- ASSUMPTION: izgleda kao agregatni "cache" prema RacunStavke — proveriti da li je uopšte
    -- potreban u novoj šemi ili se svodi na SUM upit u realnom vremenu (verovatno potonje).
    RacunStavkeId  INT NOT NULL CONSTRAINT PK_RacunStavke_Troskovi PRIMARY KEY,
    RacunId        INT NULL,
    GrupaRacunaId  INT NULL,
    KupacId        INT NULL,
    SkustinaId     INT NULL,
    Ukupno         DECIMAL(19,4) NULL,
    CONSTRAINT FK_RacunStavke_Troskovi_RacunStavke FOREIGN KEY (RacunStavkeId) REFERENCES dbo.RacunStavke(RacunStavkeId),
    CONSTRAINT FK_RacunStavke_Troskovi_Racun FOREIGN KEY (RacunId) REFERENCES dbo.Racun(RacunId),
    CONSTRAINT FK_RacunStavke_Troskovi_GrupaRacuna FOREIGN KEY (GrupaRacunaId) REFERENCES dbo.GrupaRacuna(GrupaRacunaId),
    CONSTRAINT FK_RacunStavke_Troskovi_Kupac FOREIGN KEY (KupacId) REFERENCES dbo.Kupac(KupacId),
    CONSTRAINT FK_RacunStavke_Troskovi_Skustina FOREIGN KEY (SkustinaId) REFERENCES dbo.Skustina(SkustinaId)
);

CREATE TABLE dbo.Settings_eNalog_Grupa (
    SettingsENalogGrupaId INT NOT NULL CONSTRAINT PK_Settings_eNalog_Grupa PRIMARY KEY,
    Kategorija    NVARCHAR(50) NULL,
    SQLMemo       NVARCHAR(MAX) NULL,
    SQL           NVARCHAR(255) NULL
);

CREATE TABLE dbo.Settings_eNalog (
    SettingsENalogId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Settings_eNalog PRIMARY KEY,
    SettingsENalogGrupaIndex INT NULL,
    BrCHR         INT NULL,
    TipCHR        NVARCHAR(50) NULL,
    Funkcija      NVARCHAR(50) NULL,
    Kategorija    NVARCHAR(50) NULL,
    Opis          NVARCHAR(255) NULL,
    Defs          NVARCHAR(50) NULL,
    FieldName     NVARCHAR(50) NULL,
    Format        NVARCHAR(50) NULL,
    RN            INT NULL,
    CONSTRAINT FK_Settings_eNalog_Grupa FOREIGN KEY (SettingsENalogGrupaIndex) REFERENCES dbo.Settings_eNalog_Grupa(SettingsENalogGrupaId)
);

CREATE TABLE dbo.Dobavljaci_Racun_TipObjekta (
    DobavljaciRacunTipObjektaId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Dobavljaci_Racun_TipObjekta PRIMARY KEY,
    DobavljacRacunId  INT NULL,
    TipObjektaId      INT NULL,
    CONSTRAINT FK_DRTO_DobavljacRacuni FOREIGN KEY (DobavljacRacunId) REFERENCES dbo.Dobavljac_Racuni(DobavljacRacunId),
    CONSTRAINT FK_DRTO_TipObjekta FOREIGN KEY (TipObjektaId) REFERENCES dbo.TipObjekta(TipObjektaId)
);

-- =====================================================================
-- 11. VIRMAN, MAIL, FAJLOVI
-- =====================================================================

CREATE TABLE dbo.Virman (
    VirmanId          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Virman PRIMARY KEY,
    NaslovSablona     NVARCHAR(50) NULL,
    Nalogodavac       NVARCHAR(255) NULL,
    SvrhaPlacanja     NVARCHAR(255) NULL,
    Primalac          NVARCHAR(255) NULL,
    SifraPlacanja     SMALLINT NULL,
    Valuta            NVARCHAR(3) NULL,
    Iznos             DECIMAL(19,4) NULL,
    BrojRacunaNalogodavca NVARCHAR(50) NULL,
    BrojModelaNal     SMALLINT NULL,
    PozivNaBrojNal    NVARCHAR(50) NULL,
    BrojRacunaPrimaoca NVARCHAR(50) NULL,
    BrojModelaPrim    SMALLINT NULL,
    PozivNaBrojPrim   NVARCHAR(50) NULL,
    Mesto             NVARCHAR(50) NULL,
    Datum             DATE NULL,
    DatumV            DATE NULL,
    Hitno             BIT NOT NULL DEFAULT 0,
    Tip               INT NULL,
    Arhivirano        DATETIME2(0) NULL,
    -- RefSourceId + RefSourceTag = polimorfna referenca (RefSourceTag određuje ciljnu tabelu,
    -- npr. "Racun" ili "Dobavljac_Racuni") — nema prirodan FK za ovo u SQL Serveru. Ostaje kao
    -- par kolona sa aplikativnom validacijom, osim ako se pokaže da RefSourceTag ima mali,
    -- fiksni skup vrednosti — tada bi vredelo razdvojiti u tipizirane FK kolone.
    RefSourceId       INT NULL,
    RefSourceTag      NVARCHAR(255) NULL,
    PrintMe           BIT NOT NULL DEFAULT 0,
    Favorit           BIT NOT NULL DEFAULT 0
);

CREATE TABLE dbo.Mail (
    MailId         INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Mail PRIMARY KEY,
    PartnerKupacId INT NULL,
    Email          NVARCHAR(255) NULL,
    SortOrder      INT NULL,
    LoginAppMaster BIT NULL,
    LoginAppView   BIT NULL,
    SendMailRacun  BIT NULL,
    CONSTRAINT FK_Mail_Kupac FOREIGN KEY (PartnerKupacId) REFERENCES dbo.Kupac(KupacId)
);

CREATE TABLE dbo.Mail_Send (
    MailSendId   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Mail_Send PRIMARY KEY,
    Subject      NVARCHAR(255) NULL,
    ToAddress    NVARCHAR(255) NULL, -- "To" je rezervisana reč
    Cc           NVARCHAR(255) NULL,
    Bcc          NVARCHAR(255) NULL,
    Body         NVARCHAR(MAX) NULL,
    BodyHtml     NVARCHAR(MAX) NULL,
    DateCreated  DATETIME2(0) NULL,
    DateSend     DATETIME2(0) NULL,
    Archive      BIT NOT NULL DEFAULT 0,
    ErrorDescription NVARCHAR(MAX) NULL,
    ErrorStatus  NVARCHAR(255) NULL
    -- ERROR_022: DateSend IS NULL AND ErrorStatus IS NULL = zaglavljen u redu za slanje —
    -- status/health-check upit u novom mail sistemu, ne šematsko ograničenje.
);

CREATE TABLE dbo.Mail_Send_Attachment (
    MailSendAttachmentId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Mail_Send_Attachment PRIMARY KEY,
    MailSendId    INT NOT NULL,
    AttachmentFilePath  NVARCHAR(255) NULL,
    AttachmentFilePath2 NVARCHAR(255) NULL,
    CONSTRAINT FK_Mail_Send_Attachment_MailSend FOREIGN KEY (MailSendId) REFERENCES dbo.Mail_Send(MailSendId) ON DELETE CASCADE
);

CREATE TABLE dbo.Files (
    FileId       INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Files PRIMARY KEY,
    DocTypeId    INT NULL,
    RefItemId    INT NULL, -- polimorfna referenca preko TabSource, ista napomena kao kod Virman
    FileNameSufix NVARCHAR(255) NULL,
    FileName     NVARCHAR(255) NULL,
    RelPathName  NVARCHAR(255) NULL,
    DateAdd      DATETIME2(0) NULL,
    Opis         NVARCHAR(255) NULL,
    Ext          NVARCHAR(255) NULL,
    TabSource    NVARCHAR(255) NULL,
    Registrator  NVARCHAR(4) NULL
);

-- =====================================================================
-- 12. SISTEM / AUTH / AUDIT
-- =====================================================================

CREATE TABLE dbo.StaffPermition (
    StaffPermitionId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_StaffPermition PRIMARY KEY,
    StaffId          INT NULL,
    KeyName          NVARCHAR(255) NULL,
    FormName         NVARCHAR(255) NULL, -- ASSUMPTION: vezano za Access forme — verovatno postaje role/claim na "ekran/funkciju" u novom auth modelu, ne 1:1 kopija
    PermitionVal     INT NULL,
    DisablePermition BIT NULL,
    CONSTRAINT FK_StaffPermition_Staff FOREIGN KEY (StaffId) REFERENCES dbo.Staff(StaffId)
);

CREATE TABLE dbo.AuditLog ( -- izvorno "_Log"
    AuditLogId   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AuditLog PRIMARY KEY,
    PC           NVARCHAR(15) NULL,
    WinUser      NVARCHAR(50) NULL,
    StaffId      INT NULL,
    UserTxt      NVARCHAR(50) NULL,
    Datum        DATETIME2(0) NULL,
    Forma        NVARCHAR(60) NULL,
    TabCode      NVARCHAR(50) NULL,
    ItemId       INT NULL,
    ActionType   NVARCHAR(20) NULL,
    MsgExtra     NVARCHAR(255) NULL,
    MsgErrNum    NVARCHAR(255) NULL,
    Msg          NVARCHAR(255) NULL,
    MsgPromene   NVARCHAR(MAX) NULL,
    Module       NVARCHAR(50) NULL,
    CONSTRAINT FK_AuditLog_Staff FOREIGN KEY (StaffId) REFERENCES dbo.Staff(StaffId)
);

CREATE TABLE dbo.Promene (
    PromenaId       INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Promene PRIMARY KEY,
    DateOfRequest   DATETIME2(0) NULL,
    DateOfExecution DATETIME2(0) NULL,
    KupacId         INT NULL,
    ObjekatId       INT NULL,
    Description     NVARCHAR(255) NULL,
    PreviousValue   NVARCHAR(255) NULL,
    NewValue        NVARCHAR(255) NULL,
    FieldsRelated   NVARCHAR(255) NULL,
    RequestType     NVARCHAR(255) NULL,
    RequestBy       NVARCHAR(255) NULL,
    RequestThrough  NVARCHAR(255) NULL,
    CONSTRAINT FK_Promene_Kupac FOREIGN KEY (KupacId) REFERENCES dbo.Kupac(KupacId),
    CONSTRAINT FK_Promene_Objekti FOREIGN KEY (ObjekatId) REFERENCES dbo.Objekti(ObjekatId)
);

CREATE TABLE dbo.Notes (
    NoteId    INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Notes PRIMARY KEY,
    Datum     NVARCHAR(50) NULL, -- ASSUMPTION: tekstualni datum u izvoru, razmotriti konverziju u DATE pri migraciji
    Korisnik  NVARCHAR(50) NULL,
    Notes     NVARCHAR(MAX) NULL
);

CREATE TABLE dbo.Settings (
    SettingsId    INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Settings PRIMARY KEY,
    SettingName   NVARCHAR(255) NULL,
    SettingVal    NVARCHAR(255) NULL,
    Description   NVARCHAR(255) NULL,
    Category      NVARCHAR(50) NULL,
    MFF           NVARCHAR(50) NULL,
    DefVal        NVARCHAR(255) NULL,
    FilterUserId  INT NULL,
    FilterPC      NVARCHAR(255) NULL,
    FilterCustom1Num INT NULL, FilterCustom2Num INT NULL, FilterCustom3Num INT NULL,
    FilterCustom1Txt NVARCHAR(255) NULL, FilterCustom2Txt NVARCHAR(255) NULL, FilterCustom3Txt NVARCHAR(255) NULL,
    SettingValLT  NVARCHAR(MAX) NULL,
    CONSTRAINT FK_Settings_Staff FOREIGN KEY (FilterUserId) REFERENCES dbo.Staff(StaffId)
);

CREATE TABLE dbo.Settings_eMail (
    SettingsEmailId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Settings_eMail PRIMARY KEY,
    SettingName   NVARCHAR(255) NULL,
    SettingVal    NVARCHAR(MAX) NULL,
    Category      NVARCHAR(255) NULL,
    Description   NVARCHAR(255) NULL,
    SkustinaId    INT NULL,
    CONSTRAINT FK_Settings_eMail_Skustina FOREIGN KEY (SkustinaId) REFERENCES dbo.Skustina(SkustinaId)
);

-- =====================================================================
-- KRAJ NACRTA
-- =====================================================================
-- Tabele koje ZAVISE od odgovora na otvorena pitanja (docs/tehnicki-plan-faza1.md,
-- sekcija 3) pre nego što se ovaj DDL pusti: Kupac.Tip (3.1), Kupac self-FK-ovi (3.2),
-- Objekti.PlatilacKupacId/2 (3.3), GK bez SIFRAKONTA (3.4), RacunStavke bez lnkGR/ID_K/ID_SK
-- (3.5), zaokruživanje novca (3.6), Nalog kao INT IDENTITY (3.7), ZK tabela (3.8),
-- RacunIN tabela (3.9).
--
-- Dinamički report/filter builder (tblIzvestaj, tblSifrarnik, tblAnaliza, tblSTATS, tblWhrEx,
-- tblShortList) i tabela "ugovori" NISU uključeni u ovaj nacrt — zavise od pitanja 3.10/3.11.
