-- =====================================================================
-- SZ App — nacrt ciljane SQL Server šeme — v2 (usaglašeno sa šefom)
-- =====================================================================
-- Prati docs/tehnicki-plan-faza1.md v2. Sve promene u odnosu na v1 su
-- objašnjene u sekciji 0 tog dokumenta (preimenovanja: Skustina->Company,
-- Kupac->Partner, Objekti->Unit, Naselje->CategoryLocation,
-- SzUlaz->BuildingEntrance, Troskovi_PodKonta->SubAccount, Promene->Events,
-- PrinterBinLOCAL->SelectionBasket, ugovori->Contract; nove tabele:
-- PartnerAccounting, Contract, UnitBillingAllocation, StaffCompany,
-- ContractRole; uklonjeno: RacunIN).
--
-- Mesta gde je nešto još pretpostavka (ne potvrđeno eksplicitno) su
-- označena "-- ASSUMPTION" ili brojem otvorenog pitanja iz sekcije 4
-- tog dokumenta (npr. "O-4").
--
-- VAŽNO: trigger za ravnotežu naloga iz v1 (AFTER INSERT/UPDATE/DELETE na
-- svaki red) je UKLONJEN — pogrešan dizajn, blokirao bi normalan unos.
-- Ravnoteža se proverava samo pri knjiženju (Draft->Posted), vidi
-- sekciju 8 ovog fajla (oko definicije GK/Nalog).
--
-- Novac: DECIMAL(19,4) svuda (razlog nepromenjen iz v1 — Access je čuvao
-- novac kao float).

-- =====================================================================
-- 1. ŠIFARNICI BEZ FK (ili sa self-FK)
-- =====================================================================

CREATE TABLE dbo.CategoryLocation ( -- bivši "Naselje", sad hijerarhijski
    CategoryLocationId       INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CategoryLocation PRIMARY KEY,
    Naziv                     NVARCHAR(100) NULL,
    ParentCategoryLocationId  INT NULL, -- self FK, NULL dozvoljen (nema roditelja)
    CONSTRAINT FK_CategoryLocation_Parent FOREIGN KEY (ParentCategoryLocationId) REFERENCES dbo.CategoryLocation(CategoryLocationId)
);

CREATE TABLE dbo.PartnerCategory ( -- bivši "TipPartnera", cilj Partner.CategoryId (pravna forma)
    PartnerCategoryId INT NOT NULL CONSTRAINT PK_PartnerCategory PRIMARY KEY,
    Naziv NVARCHAR(255) NULL -- npr. "Fizičko lice - Domaće", "Fizičko lice - Stranac", "Kompanija", "SZ", "Upravnik"
);

CREATE TABLE dbo.ContractRole ( -- nova mala šifra: uloga u ugovoru
    ContractRoleId INT NOT NULL CONSTRAINT PK_ContractRole PRIMARY KEY,
    Naziv NVARCHAR(50) NULL -- "Vlasnik", "Zakupac", "Primalac računa"
);

CREATE TABLE dbo.TipObjekta (
    TipObjektaId    INT NOT NULL CONSTRAINT PK_TipObjekta PRIMARY KEY,
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

CREATE TABLE dbo.TipStavke (
    TipStavkeId INT NOT NULL CONSTRAINT PK_TipStavke PRIMARY KEY, -- ID_TIP
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

CREATE TABLE dbo.TipStatus (
    TipStatusId INT NOT NULL CONSTRAINT PK_TipStatus PRIMARY KEY,
    Status NVARCHAR(255) NULL,
    LimitTable NVARCHAR(255) NULL,
    Opis NVARCHAR(255) NULL
);

CREATE TABLE dbo.Konta (
    Konto INT NOT NULL CONSTRAINT PK_Konta PRIMARY KEY,
    Opis NVARCHAR(50) NULL,
    PrintText NVARCHAR(255) NULL
);

CREATE TABLE dbo.KontniOkvir (
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
    Kurs DECIMAL(19,4) NULL, -- 4 decimale, potvrđeno pravilo
    DatumOd DATE NULL,
    Skolska INT NULL, -- ASSUMPTION: nejasna namena
    RefCena INT NULL,
    DatumUpisa DATETIME2(0) NULL
);

-- =====================================================================
-- 2. STAFF
-- =====================================================================

CREATE TABLE dbo.Staff (
    StaffId      INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Staff PRIMARY KEY,
    UserName     NVARCHAR(50) NULL,
    -- Lozinka ide kroz ASP.NET Core Identity (AspNetUsers), ne kroz ovu tabelu.
    Level        INT NULL, -- ASSUMPTION: model uloga je odložen za Fazu 4/5 (O-6), ovo polje je privremeno
    LastLog      NVARCHAR(50) NULL,
    UseLang      NVARCHAR(50) NULL,
    Restrict     NVARCHAR(255) NULL,
    LastPC       NVARCHAR(255) NULL
);

-- =====================================================================
-- 3. PARTNER / COMPANY / PARTNERACCOUNTING
-- =====================================================================

CREATE TABLE dbo.Company ( -- bivša "Skustina"
    CompanyId            INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Company PRIMARY KEY,
    LegacyId             NVARCHAR(50) NULL, -- bivši SZID, za ETL mapiranje (princip "TransferId")
    NazivSS              NVARCHAR(255) NULL,
    Zgrada                NVARCHAR(50) NULL,
    Adresa                NVARCHAR(50) NULL,
    Opstina               NVARCHAR(255) NULL,
    PBroj                 NVARCHAR(50) NULL,
    PredstavnikId         INT NULL, -- FK -> Partner, dodato niže (kružna zavisnost)
    PIB                   NVARCHAR(20) NULL, -- REŠENO: PIB je tekst, ne broj (v1 greška ispravljena)
    TR                     NVARCHAR(50) NULL,
    MB                     NVARCHAR(50) NULL,
    Napomena               NVARCHAR(255) NULL,
    RB                     INT NULL,
    PrintNaziv             NVARCHAR(255) NULL,
    Folder                 NVARCHAR(50) NULL,
    UplatnicaTip           INT NULL, -- O-5: šifarnik nije definisan
    CategoryLocationId     INT NULL,
    Konto                  INT NULL,
    SkStatus               INT NULL, -- O-5: šifarnik nije definisan
    ExterniKonto           NVARCHAR(255) NULL,
    PDtext                 NVARCHAR(255) NULL,
    UpravnikId             INT NULL, -- FK -> Staff
    DatumUgovora            DATE NULL,
    SZPDV                   BIT NULL,
    TipSubjekta              INT NULL, -- O-5: šifarnik nije definisan
    InvoiceIssuer             INT NULL,
    Doznaka                   NVARCHAR(255) NULL,
    RacunInfoReklamacija        NVARCHAR(255) NULL,
    Email                        NVARCHAR(255) NULL,
    EmailDisplay                  NVARCHAR(255) NULL,
    PBrojSZ                        NVARCHAR(255) NULL,
    GradSZ                          NVARCHAR(255) NULL,
    Logo                            NVARCHAR(255) NULL,
    Field1                           NVARCHAR(255) NULL, -- ASSUMPTION: nejasna namena
    QRName                            NVARCHAR(255) NULL,
    CONSTRAINT FK_Company_CategoryLocation FOREIGN KEY (CategoryLocationId) REFERENCES dbo.CategoryLocation(CategoryLocationId),
    CONSTRAINT FK_Company_Staff FOREIGN KEY (UpravnikId) REFERENCES dbo.Staff(StaffId)
);

CREATE TABLE dbo.Partner ( -- bivši "Kupac"
    PartnerId           INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Partner PRIMARY KEY,
    LegacyId             NVARCHAR(50) NULL, -- bivši ID_K
    Naziv               NVARCHAR(100) NULL,
    PBroj               NVARCHAR(50) NULL,
    Adresa              NVARCHAR(50) NULL,
    MB                  NVARCHAR(50) NULL,
    PIB                 NVARCHAR(20) NULL,
    AdresaUgovor        NVARCHAR(255) NULL,
    Telefon             NVARCHAR(50) NULL,
    Email               NVARCHAR(100) NULL,
    Napomena            NVARCHAR(255) NULL,
    Web                 NVARCHAR(50) NULL,
    CategoryLocationId  INT NULL,
    CategoryId          INT NULL, -- pravna forma — REŠENO (bivši nejasan "Tip")
    PrintNaziv          NVARCHAR(255) NULL,
    PAK                 NVARCHAR(50) NULL,
    PDPrefix            NVARCHAR(255) NULL,
    LK                  NVARCHAR(255) NULL,
    -- Konto/ExterniKonto/AutoKontoTroska PREMEŠTENI na PartnerAccounting (knjigovodstvena
    -- konfiguracija je po firmi, ne osobina partnera) — REŠENO.
    GrupniRacunGrupaId  INT NULL, -- bivši IDGrupniRacunMaster — O-2: FK cilj nije 100% potvrđen
    SkipPrintRacunGrupa BIT NULL,
    PDVObaveznik        BIT NULL,
    JBJKS                NVARCHAR(255) NULL,
    PartnerGrad           NVARCHAR(255) NULL, -- bivši KupacGrad
    ZemljaKod              NVARCHAR(255) NULL,
    LokacijaDostava           NVARCHAR(255) NULL,
    DostavaSifraPD              INT NULL,
    PrintInvoiceMandatory        BIT NULL,
    SendToPostOffice               BIT NULL,
    Language                        NVARCHAR(255) NULL,
    NapomenaExtended                  NVARCHAR(MAX) NULL,
    -- IDMaster iz v1 je UKINUT (potvrđeno) — jedan Partner = jedan zapis, ponavljanje po firmi
    -- ide kroz PartnerAccounting, ne kroz self-FK ovde.
    CONSTRAINT FK_Partner_CategoryLocation FOREIGN KEY (CategoryLocationId) REFERENCES dbo.CategoryLocation(CategoryLocationId),
    CONSTRAINT FK_Partner_PartnerCategory FOREIGN KEY (CategoryId) REFERENCES dbo.PartnerCategory(PartnerCategoryId),
    CONSTRAINT FK_Partner_GrupniRacunGrupa FOREIGN KEY (GrupniRacunGrupaId) REFERENCES dbo.Partner(PartnerId) -- O-2
);

-- razrešava kružnu zavisnost Company <-> Partner
ALTER TABLE dbo.Company ADD CONSTRAINT FK_Company_Partner_Predstavnik
    FOREIGN KEY (PredstavnikId) REFERENCES dbo.Partner(PartnerId);

CREATE TABLE dbo.PartnerAccounting ( -- NOVA TABELA — knjigovodstvena uloga partnera po firmi
    PartnerAccountingId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PartnerAccounting PRIMARY KEY,
    LegacyId             NVARCHAR(50) NULL, -- ETL: originalni ID_K/DobavljacKonto vrednost gde je primenjivo
    PartnerId            INT NOT NULL,
    CompanyId            INT NOT NULL,
    Account              NVARCHAR(10) NULL, -- bivši "Konto" — npr. "2040" (kupac) / "4350" (dobavljač)
    DefaultSubAccount    NVARCHAR(255) NULL, -- FK -> SubAccount, dodato niže (SubAccount se definiše kasnije)
    ExterniKonto         NVARCHAR(255) NULL, -- premešteno sa Partner
    AutoKontoTroska      NVARCHAR(50) NULL,  -- premešteno sa Partner
    CONSTRAINT FK_PartnerAccounting_Partner FOREIGN KEY (PartnerId) REFERENCES dbo.Partner(PartnerId),
    CONSTRAINT FK_PartnerAccounting_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId),
    CONSTRAINT UQ_PartnerAccounting UNIQUE (PartnerId, CompanyId, Account)
);

-- =====================================================================
-- 4. BUILDINGENTRANCE / UNIT / CONTRACT / UNITBILLINGALLOCATION
-- =====================================================================

CREATE TABLE dbo.BuildingEntrance ( -- bivši "SzUlaz"; "SzObjekat" UKINUT (pokriven Unit.CompanyId+BuildingEntranceId)
    BuildingEntranceId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_BuildingEntrance PRIMARY KEY,
    CompanyId  INT NOT NULL,
    Ulaz       NVARCHAR(255) NULL,
    Zgrada     NVARCHAR(255) NULL,
    Adresa     NVARCHAR(255) NULL,
    Oznaka     NVARCHAR(255) NULL,
    Opis       NVARCHAR(255) NULL,
    Sort       INT NULL,
    CONSTRAINT FK_BuildingEntrance_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId)
);

CREATE TABLE dbo.Unit ( -- bivši "Objekti"
    UnitId          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Unit PRIMARY KEY,
    LegacyId        NVARCHAR(50) NULL, -- bivši ID_O
    CompanyId       INT NOT NULL,
    BuildingEntranceId INT NOT NULL, -- REŠENO: svaki Unit mora biti povezan sa ulazom
    Naziv           NVARCHAR(255) NULL,
    TipObjektaId    INT NULL,
    Status          INT NULL,
    IO              DECIMAL(19,4) NULL, -- ASSUMPTION: nejasna namena kolone "IO"
    RF_DIN          DECIMAL(19,4) NULL,
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
    BRGM            INT NULL,
    K1 DECIMAL(9,4) NULL, K2 DECIMAL(9,4) NULL, K3 DECIMAL(9,4) NULL, K4 DECIMAL(9,4) NULL, K5 DECIMAL(9,4) NULL,
    PakSlanja       NVARCHAR(50) NULL,
    KV              DECIMAL(9,4) NULL,
    BrStanara       INT NULL,
    BrojPD          INT NULL,
    SifraPD         NVARCHAR(255) NULL,
    NetoKV          DECIMAL(9,4) NULL,
    Terasa          DECIMAL(9,4) NULL,
    NetoKVSaTerasom DECIMAL(9,4) NULL,
    HandOverDate    DATE NULL,
    SpratN          INT NULL,
    SpratT          NVARCHAR(255) NULL,
    -- UKLONJENO u odnosu na v1: lnk_ID_K/IDVlasnik/IDZakupac (sad kroz Contract),
    -- PLATILAC_lnk_ID_K/_K2 (sad kroz UnitBillingAllocation), GrupniRacunId (grupno
    -- fakturisanje je na nivou Partner/Racun, ne Unit).
    CONSTRAINT FK_Unit_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId),
    CONSTRAINT FK_Unit_BuildingEntrance FOREIGN KEY (BuildingEntranceId) REFERENCES dbo.BuildingEntrance(BuildingEntranceId),
    CONSTRAINT FK_Unit_TipObjekta FOREIGN KEY (TipObjektaId) REFERENCES dbo.TipObjekta(TipObjektaId)
);

CREATE TABLE dbo.Contract ( -- NOVA TABELA — vremenska istorija vlasnik/zakupac/primalac računa (bivša "ugovori")
    ContractId           INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Contract PRIMARY KEY,
    UnitId               INT NOT NULL,
    PartnerAccountingId  INT NOT NULL, -- O-4: PartnerId vs PartnerAccountingId nije eksplicitno potvrđeno
    RoleId               INT NOT NULL, -- FK -> ContractRole (Vlasnik/Zakupac/Primalac računa)
    StartInvoicingDate   DATE NULL,
    EndInvoicingDate     DATE NULL,
    ContractStartDate    DATE NULL,
    ContractEndDate      DATE NULL,
    Napomena             NVARCHAR(255) NULL,
    StatusId             INT NULL, -- O-5-slično: tačan šifarnik statusa ugovora nije definisan
    CreatedDate          DATETIME2(0) NULL,
    CONSTRAINT FK_Contract_Unit FOREIGN KEY (UnitId) REFERENCES dbo.Unit(UnitId),
    CONSTRAINT FK_Contract_PartnerAccounting FOREIGN KEY (PartnerAccountingId) REFERENCES dbo.PartnerAccounting(PartnerAccountingId),
    CONSTRAINT FK_Contract_ContractRole FOREIGN KEY (RoleId) REFERENCES dbo.ContractRole(ContractRoleId)
);
-- Pravilo (aplikativno, ne šema): kad se doda ugovor sa RoleId=Zakupac, primalac računa
-- (aktivni ugovor sa RoleId=Primalac računa) se po defaultu prebacuje na zakupca osim ako
-- se eksplicitno ne zadrži na vlasniku. Vidi tehnicki-plan-faza1.md sekcija 2.1.

CREATE TABLE dbo.UnitBillingAllocation ( -- NOVA TABELA — zamenjuje ideju duplog unosa jedinice
    UnitBillingAllocationId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_UnitBillingAllocation PRIMARY KEY,
    UnitId               INT NOT NULL,
    PartnerAccountingId  INT NOT NULL,
    Percentage           DECIMAL(7,4) NOT NULL,
    ValidFrom            DATE NOT NULL,
    ValidTo              DATE NULL,
    CONSTRAINT FK_UnitBillingAllocation_Unit FOREIGN KEY (UnitId) REFERENCES dbo.Unit(UnitId),
    CONSTRAINT FK_UnitBillingAllocation_PartnerAccounting FOREIGN KEY (PartnerAccountingId) REFERENCES dbo.PartnerAccounting(PartnerAccountingId),
    CONSTRAINT CK_UnitBillingAllocation_Percentage CHECK (Percentage > 0 AND Percentage <= 1)
);
-- Pravilo (aplikativno, cross-row, nije DB CHECK): zbir aktivnih Percentage za isti Unit u
-- istom periodu mora biti 1.0000 (uz toleranciju rešenu preko konta zaokruživanja).

-- =====================================================================
-- 5. MULTI-TENANT VIDLJIVOST + GODINA
-- =====================================================================

CREATE TABLE dbo.StaffCompany ( -- NOVA TABELA — koje Company zapise korisnik sme da vidi
    StaffId   INT NOT NULL,
    CompanyId INT NOT NULL,
    CONSTRAINT PK_StaffCompany PRIMARY KEY (StaffId, CompanyId),
    CONSTRAINT FK_StaffCompany_Staff FOREIGN KEY (StaffId) REFERENCES dbo.Staff(StaffId),
    CONSTRAINT FK_StaffCompany_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId)
);

CREATE TABLE dbo.Godina (
    -- ASSUMPTION (nepromenjeno iz v1): IDSZ u izvoru sugeriše 1 red po Company po godini.
    GodinaId    INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Godina PRIMARY KEY,
    CompanyId   INT NOT NULL,
    Godina      INT NOT NULL,
    StartDatum  DATE NULL,
    EndDatum    DATE NULL,
    Arhivirana  BIT NULL,
    Prikaz      NVARCHAR(255) NULL,
    Folder      NVARCHAR(255) NULL,
    Datoteka    NVARCHAR(255) NULL,
    Aktuelna    BIT NULL,
    CONSTRAINT FK_Godina_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId)
);

-- =====================================================================
-- 6. NALOG (redizajnirano) + SUBACCOUNT
-- =====================================================================

CREATE TABLE dbo.Nalog (
    -- REDIZAJNIRANO u odnosu na v1: NalogId (surogat) razdvojen od NalogNumber (poslovni broj,
    -- po CompanyId po Godina, sa godišnjim resetom). CompanyId je sad NOT NULL (v1 greška
    -- ispravljena). Dodat NalogStatus — vidi napomenu o trigeru iznad definicije GK tabele niže.
    NalogId       INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Nalog PRIMARY KEY,
    LegacyId      NVARCHAR(50) NULL, -- bivši Br_Nalog (Double) — čuva originalnu vrednost za ETL
    CompanyId     INT NOT NULL,
    Godina        INT NOT NULL,
    NalogNumber   INT NOT NULL, -- redni broj po (CompanyId, Godina)
    NalogStatus   TINYINT NOT NULL DEFAULT 0, -- 0 = Draft, 1 = Posted
    Datum         DATE NULL,
    Saldo         DECIMAL(19,4) NULL,
    Napomena      NVARCHAR(255) NULL,
    Reserve       NVARCHAR(50) NULL,
    OpisNaloga    NVARCHAR(50) NULL,
    DodatneNapomene NVARCHAR(255) NULL,
    TipNaloga     INT NULL,
    RowVersion    ROWVERSION NOT NULL,
    CONSTRAINT FK_Nalog_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId),
    CONSTRAINT UQ_Nalog_Broj UNIQUE (CompanyId, Godina, NalogNumber),
    CONSTRAINT CK_Nalog_Status CHECK (NalogStatus IN (0,1))
);

CREATE TABLE dbo.SubAccount ( -- bivši "Troskovi_PodKonta"
    PodKonto           NVARCHAR(255) NOT NULL CONSTRAINT PK_SubAccount PRIMARY KEY,
    Naziv              NVARCHAR(255) NULL,
    SifraKnjPrethodni  NVARCHAR(255) NULL, -- self FK, hijerarhija
    TrosakNa           NVARCHAR(255) NULL,
    Kamata             NVARCHAR(255) NULL,
    CONSTRAINT FK_SubAccount_Prethodni FOREIGN KEY (SifraKnjPrethodni) REFERENCES dbo.SubAccount(PodKonto)
);
-- Pravilo (REŠENO, iz pitanjaZaContext.md): konto 2410 NIKAD nema SubAccount; konto 2040 ima
-- SubAccount samo kad je u pitanju pretplata; konto 4350 ima SubAccount samo kad je privremeno
-- kod partnera (i tad mora biti vidljivo u error-listingu ako nedostaje). Ovo je aplikativno
-- pravilo (zavisi od Konto vrednosti na GK redu), ne DB CHECK.

-- FK sa PartnerAccounting.DefaultSubAccount ka SubAccount (deferred, SubAccount definisan tek sad)
ALTER TABLE dbo.PartnerAccounting ADD CONSTRAINT FK_PartnerAccounting_SubAccount
    FOREIGN KEY (DefaultSubAccount) REFERENCES dbo.SubAccount(PodKonto);

CREATE TABLE dbo.SubAccountDefaultSupplier ( -- bivši "Troskovi_PodKonta_DefDob"
    CompanyId                        INT NOT NULL,
    SubAccountCode                   NVARCHAR(255) NOT NULL,
    DefaultSupplierPartnerAccountingId INT NULL, -- bivši tekstualni DefDob, sad prava FK veza
    CONSTRAINT PK_SubAccountDefaultSupplier PRIMARY KEY (CompanyId, SubAccountCode),
    CONSTRAINT FK_SADS_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId),
    CONSTRAINT FK_SADS_SubAccount FOREIGN KEY (SubAccountCode) REFERENCES dbo.SubAccount(PodKonto),
    CONSTRAINT FK_SADS_PartnerAccounting FOREIGN KEY (DefaultSupplierPartnerAccountingId) REFERENCES dbo.PartnerAccounting(PartnerAccountingId)
);

-- =====================================================================
-- 7. FAKTURISANJE + OPOMENE (pre GK i Dobavljac_Racuni, jer GK zavisi od Racun-a;
--    opomene idu pre Racun jer Racun.OpomenaId pokazuje na Opomena)
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
    CompanyId           INT NOT NULL,
    NalogId             INT NULL,
    DatumSys            DATETIME2(0) NULL,
    StaffId             INT NULL,
    MarkerVanrednihRacuna NVARCHAR(255) NULL,
    VrstaRacuna         NVARCHAR(255) NULL,
    DatumStanja         DATE NULL,
    PrethodnaValuta     DATE NULL,
    ObracunKamate       BIT NULL,
    CONSTRAINT FK_GrupaRacuna_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId),
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

CREATE TABLE dbo.OpomenaSablonEx ( -- VRAĆENA iz isključenih — rad u toku, fleksibilni tekstovi opomena
    OpomenaSablonExId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_OpomenaSablonEx PRIMARY KEY,
    OpomenaSablonId   INT NOT NULL,
    KeyName           NVARCHAR(255) NULL,
    KeyIndex          INT NULL,
    SablonText        NVARCHAR(MAX) NULL,
    CONSTRAINT FK_OpomenaSablonEx_OpomenaSabloni FOREIGN KEY (OpomenaSablonId) REFERENCES dbo.OpomenaSabloni(OpomenaSablonId)
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
    CompanyId         INT NULL,
    GrupaRacunaId     INT NULL,
    GrupaOpomenaTxt   NVARCHAR(255) NULL,
    SablonTextOpomene NVARCHAR(255) NULL,
    Doznaka           NVARCHAR(255) NULL,
    CONSTRAINT FK_GrupaOpomena_OpomenaSabloni FOREIGN KEY (OpomenaSablonId) REFERENCES dbo.OpomenaSabloni(OpomenaSablonId),
    CONSTRAINT FK_GrupaOpomena_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId),
    CONSTRAINT FK_GrupaOpomena_GrupaRacuna FOREIGN KEY (GrupaRacunaId) REFERENCES dbo.GrupaRacuna(GrupaRacunaId)
);

CREATE TABLE dbo.Opomena (
    OpomenaId        INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Opomena PRIMARY KEY,
    GrupaOpomenaId   INT NULL,
    PartnerAccountingId INT NULL,
    BNR              INT NULL,
    Dug              DECIMAL(19,4) NULL,
    TxtRacunOp       NVARCHAR(255) NULL,
    AktivnaOpomena   BIT NULL,
    SumaPoStavkama   DECIMAL(19,4) NULL,
    PozivNaBroj      NVARCHAR(255) NULL,
    Troskovi         DECIMAL(19,4) NULL,
    Ukupno           DECIMAL(19,4) NULL,
    CONSTRAINT FK_Opomena_GrupaOpomena FOREIGN KEY (GrupaOpomenaId) REFERENCES dbo.GrupaOpomena(GrupaOpomenaId),
    CONSTRAINT FK_Opomena_PartnerAccounting FOREIGN KEY (PartnerAccountingId) REFERENCES dbo.PartnerAccounting(PartnerAccountingId)
);

CREATE TABLE dbo.Racun (
    RacunId             INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Racun PRIMARY KEY,
    LegacyId             NVARCHAR(50) NULL,
    RBR                 NVARCHAR(20) NULL, -- format CompanyId-PartnerAccountingId-GGMM
    GrupaRacunaId        INT NOT NULL,
    DatumIzdavanja       DATE NULL,
    MestoIzdavanja       NVARCHAR(50) NULL,
    DatumUsluge          NVARCHAR(50) NULL,
    DatumPrometa         DATE NULL,
    DatumValute          DATE NULL,
    PartnerAccountingId  INT NOT NULL,
    CompanyId            INT NOT NULL,
    PartnerNaziv          NVARCHAR(255) NULL, -- snapshot naziva partnera u trenutku izdavanja (bivša kolona "Kupac")
    PBrojK               NVARCHAR(50) NULL,
    AdresaK              NVARCHAR(50) NULL,
    PIB                  NVARCHAR(20) NULL,
    MB                   NVARCHAR(255) NULL,
    Suma                 DECIMAL(19,4) NULL,
    PDVStopa             DECIMAL(9,4) NULL,
    PDVIznos             DECIMAL(19,4) NULL,
    Ukupno               DECIMAL(19,2) NULL, -- REŠENO: konačna suma na 2 decimale
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
    UnitId                INT NULL,
    AdresaProstora        NVARCHAR(255) NULL,
    ZaUplatu               DECIMAL(19,2) NULL,
    NazivSlanja            NVARCHAR(50) NULL,
    AdresaSlanja            NVARCHAR(50) NULL,
    PBrojSlanja              NVARCHAR(50) NULL,
    PIBSlanja                NVARCHAR(50) NULL,
    SPC                       INT NULL,
    PDIznos                   DECIMAL(19,4) NULL,
    TipPoljaZaUplatu           INT NULL,
    OpomenaId                   INT NULL,
    GrupniRacunId                INT NULL, -- ISPRAVLJENO iz v1 (bio pogrešno FK->Kupac) — sad self FK na grupni račun
    GradK                         NVARCHAR(255) NULL,
    Lokacija                      NVARCHAR(255) NULL,
    SortRacun                      INT NULL,
    DatumStorno                     DATE NULL,
    RacunShema                       NVARCHAR(255) NULL,
    DatumStanja                       DATE NULL,
    KamataIznos                       DECIMAL(19,4) NULL,
    UkupnoRacun                        DECIMAL(19,2) NULL, -- 2 decimale
    RowVersion                          ROWVERSION NOT NULL,
    -- tmpID/tmpID2/tmpPB/tmpPB2 iz izvora namerno izostavljene (radne kolone stare aplikacije).
    CONSTRAINT FK_Racun_GrupaRacuna FOREIGN KEY (GrupaRacunaId) REFERENCES dbo.GrupaRacuna(GrupaRacunaId),
    CONSTRAINT FK_Racun_PartnerAccounting FOREIGN KEY (PartnerAccountingId) REFERENCES dbo.PartnerAccounting(PartnerAccountingId),
    CONSTRAINT FK_Racun_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId),
    CONSTRAINT FK_Racun_Unit FOREIGN KEY (UnitId) REFERENCES dbo.Unit(UnitId),
    CONSTRAINT FK_Racun_Opomena FOREIGN KEY (OpomenaId) REFERENCES dbo.Opomena(OpomenaId),
    CONSTRAINT FK_Racun_GrupniRacun FOREIGN KEY (GrupniRacunId) REFERENCES dbo.Racun(RacunId)
);

CREATE TABLE dbo.OpomenaStavke (
    OpomenaStavkaId  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_OpomenaStavke PRIMARY KEY,
    OpomenaId        INT NULL,
    GrupaOpomenaId   INT NULL,
    PartnerAccountingId INT NULL,
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
    -- NAPOMENA: ON DELETE CASCADE UKLONJEN u odnosu na v1 — izdate opomene se ne brišu fizički
    -- (samo storniraju); brisanje dozvoljeno jedino za dokumente koji nikad nisu izdati
    -- (aplikativna provera, ne FK cascade).
    CONSTRAINT FK_OpomenaStavke_Opomena FOREIGN KEY (OpomenaId) REFERENCES dbo.Opomena(OpomenaId),
    CONSTRAINT FK_OpomenaStavke_GrupaOpomena FOREIGN KEY (GrupaOpomenaId) REFERENCES dbo.GrupaOpomena(GrupaOpomenaId),
    CONSTRAINT FK_OpomenaStavke_PartnerAccounting FOREIGN KEY (PartnerAccountingId) REFERENCES dbo.PartnerAccounting(PartnerAccountingId),
    CONSTRAINT FK_OpomenaStavke_Racun FOREIGN KEY (RacunId) REFERENCES dbo.Racun(RacunId)
);

-- =====================================================================
-- 8. BANKARSKI IZVODI
-- =====================================================================

CREATE TABLE dbo.Izvod (
    IzvodId               INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Izvod PRIMARY KEY,
    LegacyId               NVARCHAR(50) NULL,
    BrojIzvoda            INT NULL,
    SufixIzvoda           NVARCHAR(50) NULL,
    CompanyId             INT NOT NULL,
    Datum                 DATE NULL,
    PrethodnoStanjeIzvoda DECIMAL(19,4) NULL,
    NovoStanje            DECIMAL(19,4) NULL,
    Duguje                DECIMAL(19,4) NULL,
    Potrazuje             DECIMAL(19,4) NULL,
    NalogaZaduzenja       INT NULL,
    NalogaOdobranja       INT NULL,
    Napomena              NVARCHAR(50) NULL,
    NalogId               INT NULL,
    Rasknjizen            BIT NOT NULL DEFAULT 0,
    RowVersion            ROWVERSION NOT NULL,
    CONSTRAINT FK_Izvod_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId),
    CONSTRAINT FK_Izvod_Nalog FOREIGN KEY (NalogId) REFERENCES dbo.Nalog(NalogId)
);

CREATE TABLE dbo.IzvodStavke (
    IzvodStavkaId    INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_IzvodStavke PRIMARY KEY,
    IzvodId          INT NOT NULL,
    CompanyId        INT NULL,
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
    PartnerAccountingId INT NULL,
    Knjizeno         BIT NOT NULL DEFAULT 0,
    Ignore           BIT NOT NULL DEFAULT 0,
    SetP             BIT NOT NULL DEFAULT 0,
    RowVersion       ROWVERSION NOT NULL,
    -- NAPOMENA: ON DELETE CASCADE UKLONJEN (bila je greška u v1 za knjigovodstvene dokumente).
    CONSTRAINT FK_IzvodStavke_Izvod FOREIGN KEY (IzvodId) REFERENCES dbo.Izvod(IzvodId),
    CONSTRAINT FK_IzvodStavke_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId),
    CONSTRAINT FK_IzvodStavke_PartnerAccounting FOREIGN KEY (PartnerAccountingId) REFERENCES dbo.PartnerAccounting(PartnerAccountingId)
);

CREATE TABLE dbo.TekuciRacun (
    TekuciRacunId  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TekuciRacun PRIMARY KEY,
    TR             NVARCHAR(255) NULL,
    Aktivan        BIT NULL,
    SortOrder      INT NULL,
    PartnerId      INT NULL, -- ASSUMPTION: identitetski nivo, ne PartnerAccounting — bankovni račun firme
    CompanyId      INT NULL,
    ManagerId      INT NULL, -- bivši IDUPRAVNIK -> Staff
    CONSTRAINT FK_TekuciRacun_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId),
    CONSTRAINT FK_TekuciRacun_Partner FOREIGN KEY (PartnerId) REFERENCES dbo.Partner(PartnerId),
    CONSTRAINT FK_TekuciRacun_Staff FOREIGN KEY (ManagerId) REFERENCES dbo.Staff(StaffId)
);

CREATE TABLE dbo.PartnerTekuciRacuni ( -- bivši "TRs"
    PartnerTekuciRacunId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PartnerTekuciRacuni PRIMARY KEY,
    TekuciRacunBroj       NVARCHAR(50) NULL,
    PartnerId             INT NULL, -- identitetski nivo, ne po firmi
    CONSTRAINT FK_PartnerTekuciRacuni_Partner FOREIGN KEY (PartnerId) REFERENCES dbo.Partner(PartnerId)
);

-- =====================================================================
-- 9. DOBAVLJAČI (pre GK — GK.DobavljacRacunId pokazuje ovde). RacunIN UKLONJENA (napuštena).
-- =====================================================================

CREATE TABLE dbo.Dobavljac_Racuni (
    DobavljacRacunId  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Dobavljac_Racuni PRIMARY KEY,
    LegacyId           NVARCHAR(50) NULL,
    CompanyId          INT NULL,
    RacunNo           INT NULL,
    NazivRacuna       NVARCHAR(255) NULL,
    Napomena          NVARCHAR(255) NULL,
    DobavljacNaziv    NVARCHAR(50) NULL,
    DobavljacPartnerAccountingId INT NULL,
    TipObracunaId     INT NULL,
    MesecRacuna       NVARCHAR(50) NULL,
    IznosRacunaEUR    DECIMAL(19,4) NULL,
    IznosRacunaRSD    DECIMAL(19,4) NULL,
    IznosPoKoefEUR    DECIMAL(19,4) NULL,
    IznosPoKoefRSD    DECIMAL(19,4) NULL,
    PrioritetNaplate  INT NULL,
    SifraKN           NVARCHAR(6) NULL,
    TmpPrevId         INT NULL, -- ASSUMPTION: "tmp" u nazivu — proveriti
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
    PrethodniDobavljacRacunId INT NULL,
    NoviDobavljacRacunId      INT NULL,
    NalogId           INT NULL,
    PDV               INT NULL,
    ZatvaraKonto      NVARCHAR(255) NULL,
    CONSTRAINT FK_Dobavljac_Racuni_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId),
    CONSTRAINT FK_Dobavljac_Racuni_PartnerAccounting FOREIGN KEY (DobavljacPartnerAccountingId) REFERENCES dbo.PartnerAccounting(PartnerAccountingId),
    CONSTRAINT FK_Dobavljac_Racuni_Nalog FOREIGN KEY (NalogId) REFERENCES dbo.Nalog(NalogId),
    CONSTRAINT FK_Dobavljac_Racuni_Prethodni FOREIGN KEY (PrethodniDobavljacRacunId) REFERENCES dbo.Dobavljac_Racuni(DobavljacRacunId),
    CONSTRAINT FK_Dobavljac_Racuni_Novi FOREIGN KEY (NoviDobavljacRacunId) REFERENCES dbo.Dobavljac_Racuni(DobavljacRacunId)
);

CREATE TABLE dbo.KnjiznaDokumenta (
    KnjiznoDokumentId  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_KnjiznaDokumenta PRIMARY KEY,
    CompanyId          INT NULL,
    KnjiznoDokumentIdRef INT NULL, -- ASSUMPTION: nejasno na šta IDKR tačno pokazuje
    TipDokumentaId     INT NULL,
    Iznos              DECIMAL(19,4) NULL,
    NalogId            INT NULL,
    Datum              DATE NULL,
    DokumentNaziv      NVARCHAR(255) NULL,
    DokumentOpis       NVARCHAR(255) NULL,
    KontoTroska        INT NULL,
    TipGrupaId         INT NULL,
    RefFromKnjiznoDokumentId INT NULL,
    PB                 NVARCHAR(30) NULL,
    CONSTRAINT FK_KnjiznaDokumenta_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId),
    CONSTRAINT FK_KnjiznaDokumenta_Nalog FOREIGN KEY (NalogId) REFERENCES dbo.Nalog(NalogId),
    CONSTRAINT FK_KnjiznaDokumenta_Self FOREIGN KEY (RefFromKnjiznoDokumentId) REFERENCES dbo.KnjiznaDokumenta(KnjiznoDokumentId)
);

-- =====================================================================
-- 10. GK — glavna knjiga
-- =====================================================================

CREATE TABLE dbo.GK (
    GKStavkaId        INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_GK PRIMARY KEY,
    LegacyId          NVARCHAR(50) NULL,
    NalogId           INT NOT NULL,
    Konto             NVARCHAR(50) NOT NULL,
    Datum             DATE NOT NULL,
    Diznos            DECIMAL(19,4) NOT NULL DEFAULT 0,
    Piznos            DECIMAL(19,4) NOT NULL DEFAULT 0,
    TipStavke         INT NULL, -- FK -> TipStavke.TipStavkeId — DODATO (šef je tražio)
    Dok               NVARCHAR(50) NULL,
    CompanyId         INT NOT NULL,
    PartnerAccountingId INT NULL, -- bivši lnkKUPACID (SIFRAKONTA uklonjena, potvrđeno)
    IzvodStavkaId     INT NULL,
    Napomena          NVARCHAR(255) NULL,
    Parametri         NVARCHAR(25) NULL,
    Opis              NVARCHAR(255) NULL,
    Dpo               DATE NULL, -- ASSUMPTION: tačno poslovno značenje "DPO" i dalje nepotvrđeno
    SifraKN           NVARCHAR(6) NULL,
    DobavljacRacunId  INT NULL, -- bivši RDOB
    RacunId           INT NULL,
    Prioritet         INT NULL,
    KnZaTip           INT NULL,
    SubAccount        NVARCHAR(255) NULL, -- bivši KontoTroska
    KnjiznoDokumentId INT NULL,
    -- RacunInId (RacunIN FK) UKLONJEN — tabela RacunIN je napuštena.
    CONSTRAINT FK_GK_Nalog FOREIGN KEY (NalogId) REFERENCES dbo.Nalog(NalogId),
    CONSTRAINT FK_GK_Konta FOREIGN KEY (Konto) REFERENCES dbo.Konta(Konto), -- DODATO (šef je tražio)
    CONSTRAINT FK_GK_TipStavke FOREIGN KEY (TipStavke) REFERENCES dbo.TipStavke(TipStavkeId), -- DODATO (šef je tražio)
    CONSTRAINT FK_GK_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId),
    CONSTRAINT FK_GK_PartnerAccounting FOREIGN KEY (PartnerAccountingId) REFERENCES dbo.PartnerAccounting(PartnerAccountingId),
    CONSTRAINT FK_GK_IzvodStavke FOREIGN KEY (IzvodStavkaId) REFERENCES dbo.IzvodStavke(IzvodStavkaId),
    CONSTRAINT FK_GK_Racun FOREIGN KEY (RacunId) REFERENCES dbo.Racun(RacunId),
    CONSTRAINT FK_GK_DobavljacRacuni FOREIGN KEY (DobavljacRacunId) REFERENCES dbo.Dobavljac_Racuni(DobavljacRacunId),
    CONSTRAINT FK_GK_SubAccount FOREIGN KEY (SubAccount) REFERENCES dbo.SubAccount(PodKonto),
    CONSTRAINT FK_GK_KnjiznaDokumenta FOREIGN KEY (KnjiznoDokumentId) REFERENCES dbo.KnjiznaDokumenta(KnjiznoDokumentId)
);
GO

-- =====================================================================
-- KNJIŽENJE (POST) — ISPRAVLJEN DIZAJN, zamenjuje pogrešan trigger iz v1
-- =====================================================================
-- v1 GREŠKA: AFTER INSERT/UPDATE/DELETE trigger je proveravao ravnotežu posle SVAKE izmene
-- GK reda — ovo bi blokiralo normalan unos (npr. kad EF unese duguje/potražuje kao dva
-- odvojena INSERT-a u istoj transakciji, prvi bi odmah pao kao neuravnotežen).
--
-- ISPRAVKA (šef): dok je Nalog.NalogStatus = Draft (0), GK redovi se slobodno unose/menjaju/
-- brišu BEZ IKAKVE provere ravnoteže. Provera se radi TEK pri pokušaju knjiženja (Post), kroz
-- posebnu proceduru koja radi sve u jednoj transakciji: proveri ravnotežu, i tek ako je OK,
-- postavi NalogStatus = Posted. Docnije izmene proknjiženog naloga se blokiraju (storno umesto
-- direktne izmene) — to je bezbedno kao trigger jer samo čita status roditelja, ne agregira.

CREATE PROCEDURE dbo.sp_PostNalog
    @NalogId INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    DECLARE @Diff DECIMAL(19,4);
    DECLARE @TrenutniStatus TINYINT;

    -- UPDLOCK/HOLDLOCK sprečava da neko drugi menja GK redove ovog naloga dok traje provera
    SELECT @TrenutniStatus = NalogStatus
    FROM dbo.Nalog WITH (UPDLOCK, HOLDLOCK)
    WHERE NalogId = @NalogId;

    IF @TrenutniStatus IS NULL
    BEGIN
        ROLLBACK TRANSACTION;
        THROW 50002, N'Nalog ne postoji.', 1;
        RETURN;
    END

    IF @TrenutniStatus = 1
    BEGIN
        ROLLBACK TRANSACTION;
        THROW 50003, N'Nalog je već proknjižen — koristi storno za izmenu.', 1;
        RETURN;
    END

    SELECT @Diff = ROUND(SUM(Diznos) - SUM(Piznos), 2)
    FROM dbo.GK WITH (UPDLOCK)
    WHERE NalogId = @NalogId;

    IF @Diff IS NULL OR @Diff <> 0
    BEGIN
        ROLLBACK TRANSACTION;
        THROW 50001, N'Nalog nije u ravnoteži (duguje <> potražuje) — knjiženje odbijeno.', 1;
        RETURN;
    END

    UPDATE dbo.Nalog SET NalogStatus = 1 WHERE NalogId = @NalogId;

    COMMIT TRANSACTION;
END;
GO

-- Blokira izmenu/brisanje GK redova čiji je Nalog već proknjižen (jednostavna provera statusa
-- roditelja po redu — bezbedna, ne agregira sestrinske redove, ne smeta unosu u Draft nalogu).
CREATE TRIGGER dbo.TR_GK_BlokirajIzmenuProknjizenogNaloga ON dbo.GK
AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (
        SELECT 1
        FROM deleted d
        JOIN dbo.Nalog n ON n.NalogId = d.NalogId
        WHERE n.NalogStatus = 1
    )
        THROW 50004, N'Ne mogu se menjati/brisati GK redovi proknjiženog naloga — koristi storno.', 1;
END;
GO

-- =====================================================================
-- 11. STAVKE FAKTURE, BENEFITI, KAMATA
-- =====================================================================

CREATE TABLE dbo.RacunStavke (
    RacunStavkeId   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_RacunStavke PRIMARY KEY,
    RacunId         INT NOT NULL,
    -- VRAĆENO u v2 (bilo uklonjeno u v1): CompanyId/PartnerAccountingId/GrupaRacunaId su NAMERNA
    -- denormalizacija — šef je objasnio da se ciljni partner/firma stavke ponekad menja POSLE
    -- izdavanja računa (kasno stigao kupoprodajni ugovor i sl.), pa stavka mora zadržati
    -- ORIGINALNU vrednost nezavisno od trenutnog stanja na Racun. Ne izvoditi JOIN-om na Racun.
    CompanyId            INT NOT NULL,
    PartnerAccountingId  INT NOT NULL,
    GrupaRacunaId        INT NOT NULL,
    Naziv           NVARCHAR(255) NULL,
    K1 DECIMAL(9,4) NULL, K2 DECIMAL(9,4) NULL, K3 DECIMAL(9,4) NULL, K4 DECIMAL(9,4) NULL, K5 DECIMAL(9,4) NULL,
    Kolicina        DECIMAL(9,4) NULL,
    CenaE           DECIMAL(19,4) NULL,
    NBS             DECIMAL(19,4) NULL,
    Iznos           DECIMAL(19,4) NULL,
    Suma            DECIMAL(19,4) NULL,
    PDVStopa        DECIMAL(9,4) NULL,
    PDVIznos        DECIMAL(19,4) NULL,
    UkupnoRSD       DECIMAL(19,2) NULL, -- REŠENO: stavka-ukupno na 2 decimale
    Sort            INT NULL,
    DobavljacKontoPartnerAccountingId INT NULL,
    IznosRacuna     DECIMAL(19,4) NULL,
    K1xK2 DECIMAL(9,4) NULL, K2xK3 DECIMAL(9,4) NULL, K2xK4 DECIMAL(9,4) NULL, K2xK5 DECIMAL(9,4) NULL,
    JM              NVARCHAR(255) NULL,
    UnitId          INT NULL,
    M               DECIMAL(9,4) NULL,
    DobavljacRacunId INT NULL,
    -- TipObracunaId UKLONJEN (šef: veza preko DobavljacRacunId je dovoljna).
    -- NAPOMENA: ON DELETE CASCADE UKLONJEN (izdate stavke se ne brišu fizički).
    CONSTRAINT FK_RacunStavke_Racun FOREIGN KEY (RacunId) REFERENCES dbo.Racun(RacunId),
    CONSTRAINT FK_RacunStavke_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId),
    CONSTRAINT FK_RacunStavke_PartnerAccounting FOREIGN KEY (PartnerAccountingId) REFERENCES dbo.PartnerAccounting(PartnerAccountingId),
    CONSTRAINT FK_RacunStavke_GrupaRacuna FOREIGN KEY (GrupaRacunaId) REFERENCES dbo.GrupaRacuna(GrupaRacunaId),
    CONSTRAINT FK_RacunStavke_Unit FOREIGN KEY (UnitId) REFERENCES dbo.Unit(UnitId),
    CONSTRAINT FK_RacunStavke_DobavljacRacuni FOREIGN KEY (DobavljacRacunId) REFERENCES dbo.Dobavljac_Racuni(DobavljacRacunId),
    CONSTRAINT FK_RacunStavke_DobavljacKontoPartnerAccounting FOREIGN KEY (DobavljacKontoPartnerAccountingId) REFERENCES dbo.PartnerAccounting(PartnerAccountingId)
);

CREATE TABLE dbo.RacunUnit ( -- bivši "RacunObjekti" — veza računa sa jedinicama radi štampe
    RacunId    INT NOT NULL,
    UnitId     INT NOT NULL,
    CONSTRAINT PK_RacunUnit PRIMARY KEY (RacunId, UnitId), -- composite PK dovoljan, nije potreban surogat
    CONSTRAINT FK_RacunUnit_Racun FOREIGN KEY (RacunId) REFERENCES dbo.Racun(RacunId),
    CONSTRAINT FK_RacunUnit_Unit FOREIGN KEY (UnitId) REFERENCES dbo.Unit(UnitId)
);

CREATE TABLE dbo.RacunStavkeBenefitArhiva (
    -- Arhivski snapshot — zadržava originalni ID iz izvora, NAMERNO bez FK prema živim tabelama.
    RacunStavkeId   INT NOT NULL CONSTRAINT PK_RacunStavkeBenefitArhiva PRIMARY KEY,
    RacunId         INT NULL,
    GrupaRacunaId   INT NULL,
    PartnerAccountingId INT NULL,
    CompanyId       INT NULL,
    DobavljacRacunId INT NULL,
    Naziv           NVARCHAR(255) NULL,
    K1 DECIMAL(9,4) NULL, K2 DECIMAL(9,4) NULL, K3 DECIMAL(9,4) NULL, K4 DECIMAL(9,4) NULL, K5 DECIMAL(9,4) NULL,
    Kolicina DECIMAL(9,4) NULL, CenaE DECIMAL(19,4) NULL, NBS DECIMAL(19,4) NULL,
    Iznos DECIMAL(19,4) NULL, Suma DECIMAL(19,4) NULL, PDVStopa DECIMAL(9,4) NULL, PDVIznos DECIMAL(19,4) NULL,
    UkupnoRSD DECIMAL(19,2) NULL, Sort INT NULL, DobavljacKontoPartnerAccountingId INT NULL, IznosRacuna DECIMAL(19,4) NULL,
    K1xK2 DECIMAL(9,4) NULL, K2xK3 DECIMAL(9,4) NULL, K2xK4 DECIMAL(9,4) NULL, K2xK5 DECIMAL(9,4) NULL,
    JM NVARCHAR(255) NULL, UnitId INT NULL
);

CREATE TABLE dbo.BenefitGrupa (
    BenefitGrupaId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_BenefitGrupa PRIMARY KEY,
    BenefitNaziv   NVARCHAR(255) NULL,
    BenefitPrint   NVARCHAR(255) NULL
);

CREATE TABLE dbo.Benefiti (
    BenefitId       INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Benefiti PRIMARY KEY,
    UnitId          INT NULL,
    PartnerAccountingId INT NULL,
    MesecYYMM       NVARCHAR(255) NULL,
    Used            BIT NULL,
    DateEntry       DATETIME2(0) NULL,
    DateUsed        DATETIME2(0) NULL,
    RacunId         INT NULL,
    BenefitGrupaId  INT NULL,
    RacunStornoId   INT NULL,
    CONSTRAINT FK_Benefiti_Unit FOREIGN KEY (UnitId) REFERENCES dbo.Unit(UnitId),
    CONSTRAINT FK_Benefiti_PartnerAccounting FOREIGN KEY (PartnerAccountingId) REFERENCES dbo.PartnerAccounting(PartnerAccountingId),
    CONSTRAINT FK_Benefiti_Racun FOREIGN KEY (RacunId) REFERENCES dbo.Racun(RacunId),
    CONSTRAINT FK_Benefiti_RacunStorno FOREIGN KEY (RacunStornoId) REFERENCES dbo.Racun(RacunId),
    CONSTRAINT FK_Benefiti_BenefitGrupa FOREIGN KEY (BenefitGrupaId) REFERENCES dbo.BenefitGrupa(BenefitGrupaId)
);

CREATE TABLE dbo.BenefitUpdate ( -- VRAĆENA iz isključenih — AKTIVNA (jedinice koje ne plaćaju, benefit)
    BenefitUpdateId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_BenefitUpdate PRIMARY KEY,
    Datum           DATE NULL,
    UnitId          INT NULL, -- bivši tekstualni "UnitApp"
    MesecYYMM       INT NULL, -- bivši "YYMM"
    CountMM         INT NULL,
    CONSTRAINT FK_BenefitUpdate_Unit FOREIGN KEY (UnitId) REFERENCES dbo.Unit(UnitId)
);

CREATE TABLE dbo.KamatniList (
    KamatniListId   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_KamatniList PRIMARY KEY,
    CompanyId       INT NULL,
    Konto           NVARCHAR(50) NULL,
    Datum           DATE NULL,
    DIPI            DECIMAL(19,4) NULL,
    Saldo           DECIMAL(19,4) NULL,
    Dana            INT NULL,
    Stopa           DECIMAL(9,4) NULL,
    Koeficijent     DECIMAL(18,8) NULL,
    Kamata          DECIMAL(19,4) NULL,
    Par             NVARCHAR(50) NULL,
    PartnerAccountingId INT NULL,
    UnitId          INT NULL,
    PodKonto        NVARCHAR(255) NULL,
    GrupaRacunaId   INT NULL,
    CONSTRAINT FK_KamatniList_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId),
    CONSTRAINT FK_KamatniList_PartnerAccounting FOREIGN KEY (PartnerAccountingId) REFERENCES dbo.PartnerAccounting(PartnerAccountingId),
    CONSTRAINT FK_KamatniList_Unit FOREIGN KEY (UnitId) REFERENCES dbo.Unit(UnitId),
    CONSTRAINT FK_KamatniList_GrupaRacuna FOREIGN KEY (GrupaRacunaId) REFERENCES dbo.GrupaRacuna(GrupaRacunaId),
    CONSTRAINT FK_KamatniList_SubAccount FOREIGN KEY (PodKonto) REFERENCES dbo.SubAccount(PodKonto)
);

CREATE TABLE dbo.Stope (
    StopaId  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Stope PRIMARY KEY,
    Datum    DATE NULL,
    Stopa    DECIMAL(19,4) NULL,
    Period   NVARCHAR(50) NULL
    -- Trenutno ručni unos; šef ima postojeći kod za automatsko preuzimanje sa NBS
    -- (planirano za kasniju fazu, ne menja šemu).
);

CREATE TABLE dbo.ZK ( -- POTVRĐENO: radna/privremena tabela zatezne kamate pre knjiženja u GK
    ZKId          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ZK PRIMARY KEY,
    CompanyId     INT NULL,
    NalogId       INT NULL,
    Konto         NVARCHAR(50) NULL,
    Datum         DATE NULL,
    Piznos        DECIMAL(19,4) NULL,
    Diznos        DECIMAL(19,4) NULL,
    TipStavke     INT NULL,
    Parametri     NVARCHAR(25) NULL,
    Napomena      NVARCHAR(20) NULL,
    PartnerAccountingId INT NULL,
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
    -- ASSUMPTION: agregatni "cache" prema RacunStavke — proveriti da li je uopšte potreban u
    -- novoj šemi ili se svodi na SUM upit u realnom vremenu.
    RacunStavkeId  INT NOT NULL CONSTRAINT PK_RacunStavke_Troskovi PRIMARY KEY,
    RacunId        INT NULL,
    GrupaRacunaId  INT NULL,
    PartnerAccountingId INT NULL,
    CompanyId      INT NULL,
    Ukupno         DECIMAL(19,4) NULL,
    CONSTRAINT FK_RacunStavke_Troskovi_RacunStavke FOREIGN KEY (RacunStavkeId) REFERENCES dbo.RacunStavke(RacunStavkeId),
    CONSTRAINT FK_RacunStavke_Troskovi_Racun FOREIGN KEY (RacunId) REFERENCES dbo.Racun(RacunId),
    CONSTRAINT FK_RacunStavke_Troskovi_GrupaRacuna FOREIGN KEY (GrupaRacunaId) REFERENCES dbo.GrupaRacuna(GrupaRacunaId),
    CONSTRAINT FK_RacunStavke_Troskovi_PartnerAccounting FOREIGN KEY (PartnerAccountingId) REFERENCES dbo.PartnerAccounting(PartnerAccountingId),
    CONSTRAINT FK_RacunStavke_Troskovi_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId)
);

CREATE TABLE dbo.SemaKnjizenja (
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
-- 12. VIRMAN, MAIL, FAJLOVI, SELECTIONBASKET
-- =====================================================================

CREATE TABLE dbo.Virman ( -- potvrđeno: uplatnica/QR/list za plaćanje ulaznog računa, preuzeto iz druge app
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
    RefSourceId       INT NULL, -- polimorfna referenca (aplikativna validacija, isto kao Files)
    RefSourceTag      NVARCHAR(255) NULL,
    PrintMe           BIT NOT NULL DEFAULT 0,
    Favorit           BIT NOT NULL DEFAULT 0
);

CREATE TABLE dbo.Mail (
    MailId         INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Mail PRIMARY KEY,
    PartnerId      INT NULL, -- identitetski nivo — email ne zavisi od knjigovodstvene uloge
    Email          NVARCHAR(255) NULL,
    SortOrder      INT NULL,
    LoginAppMaster BIT NULL,
    LoginAppView   BIT NULL,
    SendMailRacun  BIT NULL,
    CONSTRAINT FK_Mail_Partner FOREIGN KEY (PartnerId) REFERENCES dbo.Partner(PartnerId)
);

CREATE TABLE dbo.Mail_Send (
    MailSendId   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Mail_Send PRIMARY KEY,
    Subject      NVARCHAR(255) NULL,
    ToAddress    NVARCHAR(255) NULL,
    Cc           NVARCHAR(255) NULL,
    Bcc          NVARCHAR(255) NULL,
    Body         NVARCHAR(MAX) NULL,
    BodyHtml     NVARCHAR(MAX) NULL,
    DateCreated  DATETIME2(0) NULL,
    DateSend     DATETIME2(0) NULL,
    Archive      BIT NOT NULL DEFAULT 0,
    ErrorDescription NVARCHAR(MAX) NULL,
    ErrorStatus  NVARCHAR(255) NULL
);

CREATE TABLE dbo.Mail_Send_Attachment (
    MailSendAttachmentId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Mail_Send_Attachment PRIMARY KEY,
    MailSendId    INT NOT NULL,
    AttachmentFilePath  NVARCHAR(255) NULL,
    AttachmentFilePath2 NVARCHAR(255) NULL,
    CONSTRAINT FK_Mail_Send_Attachment_MailSend FOREIGN KEY (MailSendId) REFERENCES dbo.Mail_Send(MailSendId) ON DELETE CASCADE
    -- CASCADE OK ovde — prilog nema samostalan poslovni/knjigovodstveni značaj bez svog mejla.
);

CREATE TABLE dbo.Files ( -- potvrđeno: polimorfna referenca je namerna (npr. digitalna arhiva partnera)
    FileId       INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Files PRIMARY KEY,
    DocTypeId    INT NULL,
    RefItemId    INT NULL,
    FileNameSufix NVARCHAR(255) NULL,
    FileName     NVARCHAR(255) NULL,
    RelPathName  NVARCHAR(255) NULL, -- O-7: gde fajlovi fizički žive nije odlučeno
    DateAdd      DATETIME2(0) NULL,
    Opis         NVARCHAR(255) NULL,
    Ext          NVARCHAR(255) NULL,
    TabSource    NVARCHAR(255) NULL,
    Registrator  NVARCHAR(4) NULL
);

CREATE TABLE dbo.SelectionBasket ( -- VRAĆENA iz isključenih (bivši "PrinterBinLOCAL") — šef: dobra praksa, zadržati
    SelectionBasketId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SelectionBasket PRIMARY KEY,
    StaffId    INT NULL,
    TargetTable NVARCHAR(255) NULL,
    TargetId    INT NULL,
    BatchTag    NVARCHAR(255) NULL,
    CreatedDate DATETIME2(0) NULL,
    CONSTRAINT FK_SelectionBasket_Staff FOREIGN KEY (StaffId) REFERENCES dbo.Staff(StaffId)
);

-- =====================================================================
-- 13. SISTEM / AUTH / AUDIT
-- =====================================================================

CREATE TABLE dbo.StaffPermition (
    -- Model ovlašćenja je namerno ODLOŽEN za Fazu 4/5 (O-6) — šef: root/upravnik/moderator/
    -- review/stanari, ali "ostavljeno za budućnost". Ova tabela ostaje fleksibilna, ne mapira se
    -- 1:1 na stare Access forme.
    StaffPermitionId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_StaffPermition PRIMARY KEY,
    StaffId          INT NULL,
    KeyName          NVARCHAR(255) NULL,
    FormName         NVARCHAR(255) NULL,
    PermitionVal     INT NULL,
    DisablePermition BIT NULL,
    CONSTRAINT FK_StaffPermition_Staff FOREIGN KEY (StaffId) REFERENCES dbo.Staff(StaffId)
);

CREATE TABLE dbo.AuditLog ( -- bivši "_Log"
    AuditLogId    INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AuditLog PRIMARY KEY,
    PC            NVARCHAR(15) NULL,
    WinUser       NVARCHAR(50) NULL,
    StaffId       INT NULL,
    UserTxt       NVARCHAR(50) NULL,
    Datum         DATETIME2(0) NULL,
    Forma         NVARCHAR(60) NULL,
    TabCode       NVARCHAR(50) NULL,
    ItemId        INT NULL,
    ActionType    NVARCHAR(20) NULL,
    MsgExtra      NVARCHAR(255) NULL,
    MsgErrNum     NVARCHAR(255) NULL,
    Msg           NVARCHAR(255) NULL,
    MsgPromene    NVARCHAR(MAX) NULL,
    Module        NVARCHAR(50) NULL,
    CompanyId     INT NULL, -- DODATO — neke akcije su cross-company
    CorrelationId UNIQUEIDENTIFIER NULL, -- DODATO
    CONSTRAINT FK_AuditLog_Staff FOREIGN KEY (StaffId) REFERENCES dbo.Staff(StaffId),
    CONSTRAINT FK_AuditLog_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId)
);
-- NAPOMENA: aplikativna rola za pisanje u ovu tabelu ne sme imati UPDATE/DELETE (insert-only).

CREATE TABLE dbo.Events ( -- bivša "Promene" — NIJE sistemska tabela, workflow zahteva korisnika
    EventId         INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Events PRIMARY KEY, -- IDChange
    DateOfRequest   DATETIME2(0) NULL,
    DateOfExecution DATETIME2(0) NULL,
    PartnerId       INT NULL, -- IDK
    UnitId          INT NULL, -- IDO
    Description     NVARCHAR(255) NULL,
    PreviousValue   NVARCHAR(255) NULL,
    NewValue        NVARCHAR(255) NULL,
    FieldsRelated   NVARCHAR(255) NULL,
    RequestType     NVARCHAR(255) NULL,
    RequestBy       NVARCHAR(255) NULL,
    RequestThrough  NVARCHAR(255) NULL,
    StatusId          INT NULL, -- DODATO: O-3, tačan šifarnik statusa nije potvrđen
    ApprovedByStaffId INT NULL, -- DODATO: O-3
    ApprovedDate      DATETIME2(0) NULL, -- DODATO: O-3
    CONSTRAINT FK_Events_Partner FOREIGN KEY (PartnerId) REFERENCES dbo.Partner(PartnerId),
    CONSTRAINT FK_Events_Unit FOREIGN KEY (UnitId) REFERENCES dbo.Unit(UnitId),
    CONSTRAINT FK_Events_Staff FOREIGN KEY (ApprovedByStaffId) REFERENCES dbo.Staff(StaffId)
);

CREATE TABLE dbo.Notes (
    NoteId    INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Notes PRIMARY KEY,
    Datum     NVARCHAR(50) NULL,
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
    -- BEZBEDNOSNA NAPOMENA: SettingVal može sadržati lozinke/tokene za mejl naloge. NE SME
    -- ostati u čistom tekstu u produkciji — koristiti user-secrets/Key Vault ili enkriptovanu
    -- kolonu pre nego što se ova tabela napuni pravim kredencijalima.
    SettingsEmailId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Settings_eMail PRIMARY KEY,
    SettingName   NVARCHAR(255) NULL,
    SettingVal    NVARCHAR(MAX) NULL,
    Category      NVARCHAR(255) NULL,
    Description   NVARCHAR(255) NULL,
    CompanyId     INT NULL,
    CONSTRAINT FK_Settings_eMail_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId)
);

-- =====================================================================
-- KRAJ NACRTA v2
-- =====================================================================
-- Namerno IZOSTAVLJENO iz ovog nacrta (vidi tehnicki-plan-faza1.md sekcije 4-5):
--   - RacunIN: uklonjena, potvrđeno napuštena.
--   - Dinamički izveštajni sistem (tblIzvestaj/tblAnaliza/tblSifrarnik/tblSTATS/tblWhrEx/
--     tblShortList) — aktivno se koristi, ali zahteva poseban dizajn (bezbedno izvršavanje
--     korisnički unetog SQL-a), planirano kao posebna faza, ne prosta migracija tabele.
--   - Indeksi, dodatna UNIQUE i CHECK ograničenja (O-8) — namerno odloženo do profilisanja
--     stvarnih podataka u Fazi 4, nije napravljeno napamet.
--
-- Otvorena pitanja koja i dalje utiču na ovaj nacrt: O-1 (zaokruživanje), O-2
-- (Partner.GrupniRacunGrupaId FK semantika), O-3 (Events workflow polja), O-4
-- (Contract.PartnerAccountingId vs PartnerId), O-5 (Company šifarnici). Vidi
-- tehnicki-plan-faza1.md sekcija 4 za pun opis i sekciju 8 za verdikt spremnosti.
