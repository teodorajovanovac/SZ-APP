-- =====================================================================
-- SZ App -- target SQL Server schema draft -- v3 (fully English, boss-approved fixes applied)
-- =====================================================================
-- Follows docs/tehnicki-plan-faza1.md and docs/preostala-pitanja.md. v3 changes vs v2:
--   1) Every remaining Serbian/abbreviated table and column name is translated to English
--      (v2 already had Company/Partner/Unit/Contract/etc. in English; v3 finishes the rest:
--      GK->LedgerEntry, Racun->Invoice, Nalog->JournalEntry, Izvod->BankStatement,
--      Opomena->Reminder, Dobavljac_Racuni->SupplierInvoice, Kurs->ExchangeRate,
--      KontniOkvir->ChartOfAccounts, Virman->PaymentOrder, tblShortList->CodeList, etc. --
--      full table-by-table rename log is in each table's comment).
--   2) All open items from docs/preostala-pitanja.md that the boss actually answered are
--      applied as real schema changes (not just comments) -- see "-- FIX (preostala-pitanja X)"
--      markers throughout:
--        A-1  standard rounding, no separate rounding account -- no schema impact, noted only.
--        A-2  GroupInvoiceTagId moved Partner -> PartnerAccounting, no self-FK (plain tag).
--        A-3  Contract.PartnerAccountingId confirmed correct.
--        C    Kurs/ExchangeRate fully restructured; Company.Field1 and Unit.IsInvestmentMaintenance
--             (formerly "IO") removed; SupplierInvoice.TmpPrevId removed; Invoice.PageCount (formerly
--             "Co") kept as INT default 0; TekuciRacun/BankAccount.PartnerId confirmed identity-level;
--             Konta table dropped (ChartOfAccounts is enough); Godina table dropped; KnjiznoDokument
--             table dropped; LedgerEntry.DueDate (formerly "Dpo") renamed; InvoiceLineCostCache
--             (RacunStavke_Troskovi) dropped; invoice number format confirmed YYMM (year-month).
--        A-4  still OPEN -- Company.PaymentSlipTypeId/CompanyStatusId/SubjectTypeId stay plain INT,
--             candidate for CodeList once the boss confirms the value sets.
--   3) Genuinely cryptic 2-4 letter legacy codes whose business meaning could not be confirmed from
--      any project document (same category as v2's "Skolska"/"IO"/"Co"/"Field1") are preserved as
--      PascalCase versions of the original abbreviation with an "-- ASSUMPTION" comment, exactly as
--      v2 already did -- NOT invented/guessed English words. Ask the client before relying on these.
--
-- IMPORTANT: the v1 balance trigger (AFTER INSERT/UPDATE/DELETE on every row) is REMOVED -- wrong
-- design, would block normal entry. Balance is only checked at posting time (Draft->Posted), see
-- the JournalEntry/LedgerEntry section below.
--
-- Money: DECIMAL(19,4) everywhere (unchanged reason from v1 -- Access stored money as float).
-- =====================================================================

-- =====================================================================
-- 1. CODE LISTS WITHOUT FK (or with self-FK only)
-- =====================================================================

CREATE TABLE dbo.CategoryLocation ( -- formerly "Naselje", now hierarchical
    CategoryLocationId       INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CategoryLocation PRIMARY KEY,
    Name                      NVARCHAR(100) NULL,
    ParentCategoryLocationId  INT NULL, -- self FK, NULL allowed (no parent)
    CONSTRAINT FK_CategoryLocation_Parent FOREIGN KEY (ParentCategoryLocationId) REFERENCES dbo.CategoryLocation(CategoryLocationId)
);

CREATE TABLE dbo.PartnerCategory ( -- formerly "TipPartnera", target of Partner.CategoryId (legal form)
    PartnerCategoryId INT NOT NULL CONSTRAINT PK_PartnerCategory PRIMARY KEY,
    Name NVARCHAR(255) NULL -- e.g. "Individual - Domestic", "Individual - Foreign", "Company", "HOA", "Property Manager"
);

CREATE TABLE dbo.ContractRole ( -- small new code list: role within a contract
    ContractRoleId INT NOT NULL CONSTRAINT PK_ContractRole PRIMARY KEY,
    Name NVARCHAR(50) NULL -- "Owner", "Tenant", "Invoice Recipient"
);

CREATE TABLE dbo.UnitType ( -- formerly "TipObjekta"
    UnitTypeId      INT NOT NULL CONSTRAINT PK_UnitType PRIMARY KEY,
    Name            NVARCHAR(50) NULL, -- formerly TipObj
    PrintLabel      NVARCHAR(50) NULL, -- formerly Print
    SortOrder       INT NULL, -- formerly SortObj
    BinaryValue     INT NULL, -- ASSUMPTION: formerly "binVrednost", exact use unconfirmed
    InvoiceTitle    NVARCHAR(50) NULL, -- ASSUMPTION: formerly "NaslovRC" (Racun?), likely invoice heading override
    Disclaimer      NVARCHAR(MAX) NULL,
    DisclaimerSpc   NVARCHAR(MAX) NULL, -- ASSUMPTION: formerly "Disclaimerspc", "Spc" variant unconfirmed
    InvoiceTitleSpc NVARCHAR(50) NULL, -- ASSUMPTION: formerly "NaslovRC-spc"
    CbValue         DECIMAL(9,4) NULL, -- ASSUMPTION: formerly "CB_V", meaning of "CB" prefix unconfirmed
    IgValue         DECIMAL(9,4) NULL, -- ASSUMPTION: formerly "IG_V", meaning of "IG" prefix unconfirmed
    CbAccount       INT NULL, -- ASSUMPTION: formerly "CB_KONTO"
    IgAccount       INT NULL  -- ASSUMPTION: formerly "IG_KONTO"
);

CREATE TABLE dbo.LineItemType ( -- formerly "TipStavke"
    LineItemTypeId INT NOT NULL CONSTRAINT PK_LineItemType PRIMARY KEY, -- formerly ID_TIP
    Name NVARCHAR(50) NULL -- formerly TipStavke
);

CREATE TABLE dbo.CalculationType ( -- formerly "TipObracuna" -- billing/calculation method (by area, by coefficient, fixed, ...)
    CalculationTypeId INT NOT NULL CONSTRAINT PK_CalculationType PRIMARY KEY,
    Name NVARCHAR(100) NULL, -- formerly TipObracuna
    SupplierInvoiceAmountLabel NVARCHAR(100) NULL, -- formerly IznosRacunaDobavljac
    Note NVARCHAR(255) NULL, -- formerly Napomena
    AmountLabel NVARCHAR(255) NULL, -- formerly IZNOS
    QuantityLabel NVARCHAR(255) NULL, -- formerly KOLICINA
    UnitOfMeasure NVARCHAR(255) NULL, -- formerly JM
    UnitOfMeasureIndex INT NULL -- formerly JMindex
);

CREATE TABLE dbo.AdditionalTextType ( -- formerly "TipADDTXT" -- extra text blocks addable to invoices
    AdditionalTextTypeId INT NOT NULL CONSTRAINT PK_AdditionalTextType PRIMARY KEY,
    Text NVARCHAR(255) NULL -- formerly AddTxT
);

CREATE TABLE dbo.PaymentSlipType ( -- formerly "TipUplatnice"
    PaymentSlipTypeId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PaymentSlipType PRIMARY KEY,
    Name NVARCHAR(255) NULL, -- formerly TipUpl
    Description NVARCHAR(50) NULL -- formerly Opis
);

CREATE TABLE dbo.TaskType ( -- formerly "TipTODO"
    TaskTypeId INT NOT NULL CONSTRAINT PK_TaskType PRIMARY KEY,
    Name NVARCHAR(50) NULL -- formerly ToDo
);

CREATE TABLE dbo.StatusType ( -- formerly "TipStatus" / "tipStatus" (duplicated table in legacy DBs, merged here)
    StatusTypeId INT NOT NULL CONSTRAINT PK_StatusType PRIMARY KEY, -- formerly IDStatus
    Name        NVARCHAR(255) NULL, -- formerly Status
    LimitTable  NVARCHAR(255) NULL, -- which table this status set applies to
    Description NVARCHAR(255) NULL -- formerly "desc"
);

CREATE TABLE dbo.ChartOfAccounts ( -- formerly "KontniOkvir" -- RESOLVED/FIX: legacy "Konta" table dropped (boss:
    -- unused duplicate, this is the one real chart of accounts).
    Account         NVARCHAR(255) NOT NULL CONSTRAINT PK_ChartOfAccounts PRIMARY KEY, -- formerly Konto
    ShortName       NVARCHAR(255) NULL, -- formerly SkraceniNaziv
    Name            NVARCHAR(255) NULL, -- formerly Naziv
    ParentAccount   NVARCHAR(255) NULL, -- formerly Prethodni
    Level           INT NULL, -- formerly Nivo
    Sign            NVARCHAR(255) NULL, -- formerly Znak (debit/credit sign convention)
    IsActive        BIT NULL -- formerly Aktivan
);

CREATE TABLE dbo.UserLevel ( -- formerly "UserLevelList"
    UserLevelId INT NOT NULL CONSTRAINT PK_UserLevel PRIMARY KEY,
    Caption     NVARCHAR(255) NULL
);

CREATE TABLE dbo.CodeList ( -- formerly "tblShortList" -- generic shared lookup-value mechanism, used across
    -- many unrelated small code lists (distinguished by Category); referenced by ExchangeRate.EntryTypeId
    -- below, and candidate for Company.PaymentSlipTypeId/CompanyStatusId/SubjectTypeId once O-5/A-4 is
    -- answered by the client.
    CodeListId  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CodeList PRIMARY KEY, -- formerly Index
    Category    NVARCHAR(255) NOT NULL, -- formerly TableFrom -- logical domain this code belongs to
    Caption     NVARCHAR(255) NULL,
    ShortName   NVARCHAR(255) NULL,
    Description NVARCHAR(255) NULL,
    SortOrder   INT NULL, -- formerly IndexValue
    GroupCode   INT NULL -- ASSUMPTION: formerly "Cat1"
);

CREATE TABLE dbo.ExchangeRate ( -- formerly "Kurs" -- FIX (preostala-pitanja C, Kurs.Skolska): full restructure
    -- per boss's explicit answer -- old columns (Kurs, DatumOd, Skolska, RefCena, DatumUpisa) replaced.
    ExchangeRateId  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ExchangeRate PRIMARY KEY,
    Rate            DECIMAL(19,4) NOT NULL, -- 4 decimal places, confirmed by boss
    RateDate        DATE NOT NULL,
    EntryTypeId     INT NOT NULL, -- FK -> CodeList; expected values: manual entry / automatic / DB import
    UpdatedAt       DATETIME2(0) NOT NULL,
    CONSTRAINT FK_ExchangeRate_CodeList FOREIGN KEY (EntryTypeId) REFERENCES dbo.CodeList(CodeListId)
);

-- =====================================================================
-- 2. STAFF
-- =====================================================================

CREATE TABLE dbo.Staff (
    StaffId      INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Staff PRIMARY KEY,
    UserName     NVARCHAR(50) NULL,
    -- Password goes through ASP.NET Core Identity (AspNetUsers), not through this table.
    Level        INT NULL, -- ASSUMPTION: role model deferred to Phase 4/5 (B-2), this field is a placeholder
    LastLoginAt  NVARCHAR(50) NULL, -- formerly LastLog
    PreferredLanguage NVARCHAR(50) NULL, -- formerly UseLang
    AccessRestriction NVARCHAR(255) NULL, -- formerly Restrict
    LastComputerName  NVARCHAR(255) NULL -- formerly LastPC
);

-- =====================================================================
-- 3. PARTNER / COMPANY / PARTNERACCOUNTING
-- =====================================================================

CREATE TABLE dbo.Company ( -- formerly "Skustina"
    CompanyId            INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Company PRIMARY KEY,
    LegacyId             NVARCHAR(50) NULL, -- formerly SZID, for ETL mapping ("TransferId" pattern)
    Name                 NVARCHAR(255) NULL, -- formerly NazivSS
    Building              NVARCHAR(50) NULL, -- formerly Zgrada
    Address                NVARCHAR(50) NULL, -- formerly Adresa
    Municipality            NVARCHAR(255) NULL, -- formerly Opstina
    PostalCode               NVARCHAR(50) NULL, -- formerly PBroj
    RepresentativeId          INT NULL, -- FK -> Partner, added below (circular dependency)
    TaxId                      NVARCHAR(20) NULL, -- formerly PIB -- RESOLVED: TaxId is text, not a number (v1 bug fixed)
    PrimaryBankAccountNumber    NVARCHAR(50) NULL, -- formerly TR
    RegistrationNumber            NVARCHAR(50) NULL, -- formerly MB
    Note                            NVARCHAR(255) NULL, -- formerly Napomena
    SortOrder                        INT NULL, -- formerly RB
    PrintName                         NVARCHAR(255) NULL, -- formerly PrintNaziv
    Folder                              NVARCHAR(50) NULL,
    PaymentSlipTypeId                    INT NULL, -- formerly UplatnicaTip -- A-4: code list not defined yet (OPEN)
    CategoryLocationId                     INT NULL,
    Account                                 INT NULL, -- formerly Konto
    CompanyStatusId                          INT NULL, -- formerly SkStatus -- A-4: code list not defined yet (OPEN)
    ExternalAccount                           NVARCHAR(255) NULL, -- formerly ExterniKonto
    PaymentSlipText                            NVARCHAR(255) NULL, -- ASSUMPTION: formerly PDtext, exact meaning of "PD" prefix unconfirmed
    ManagerId                                   INT NULL, -- formerly Upravnik -- FK -> Staff
    ContractDate                                 DATE NULL, -- formerly DatumUgovora
    IsVatPayer                                    BIT NULL, -- formerly SZPDV
    SubjectTypeId                                  INT NULL, -- formerly TipSubjekta -- A-4: code list not defined yet (OPEN)
    InvoiceIssuerId                                 INT NULL, -- formerly InvoiceIssuer
    RemittanceInfo                                   NVARCHAR(255) NULL, -- formerly Doznaka
    InvoiceComplaintInfo                              NVARCHAR(255) NULL, -- formerly RacunInfoReklamacija
    Email                                              NVARCHAR(255) NULL,
    EmailDisplay                                        NVARCHAR(255) NULL,
    InvoicePostalCode                                    NVARCHAR(255) NULL, -- formerly PBrojSZ
    InvoiceCity                                           NVARCHAR(255) NULL, -- formerly GradSZ
    Logo                                                   NVARCHAR(255) NULL,
    -- Field1 REMOVED -- FIX (preostala-pitanja C, Company.Field1): boss confirmed unused, no data.
    QrName                                                  NVARCHAR(255) NULL,
    CONSTRAINT FK_Company_CategoryLocation FOREIGN KEY (CategoryLocationId) REFERENCES dbo.CategoryLocation(CategoryLocationId),
    CONSTRAINT FK_Company_Staff FOREIGN KEY (ManagerId) REFERENCES dbo.Staff(StaffId)
);

CREATE TABLE dbo.Partner ( -- formerly "Kupac"
    PartnerId           INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Partner PRIMARY KEY,
    LegacyId             NVARCHAR(50) NULL, -- formerly ID_K
    Name                NVARCHAR(100) NULL, -- formerly Naziv
    PostalCode          NVARCHAR(50) NULL, -- formerly PBroj
    Address              NVARCHAR(50) NULL, -- formerly Adresa
    RegistrationNumber   NVARCHAR(50) NULL, -- formerly MB
    TaxId                NVARCHAR(20) NULL, -- formerly PIB
    ContractAddress      NVARCHAR(255) NULL, -- formerly Adresa_Ugovor
    Phone                NVARCHAR(50) NULL, -- formerly Telefon
    Email                NVARCHAR(100) NULL,
    Note                 NVARCHAR(255) NULL, -- formerly napomena
    Website              NVARCHAR(50) NULL, -- formerly web
    CategoryLocationId   INT NULL,
    CategoryId           INT NULL, -- legal form -- RESOLVED (formerly unclear "Tip")
    PrintName            NVARCHAR(255) NULL, -- formerly PrintNaziv
    LegacyShortCode      NVARCHAR(50) NULL, -- ASSUMPTION: formerly PAK, exact legacy use unconfirmed
    PaymentSlipPrefix    NVARCHAR(255) NULL, -- ASSUMPTION: formerly PDprefix
    LegalRepresentative  NVARCHAR(255) NULL, -- ASSUMPTION: formerly LK
    -- Account/ExternalAccount/AutoAccount MOVED to PartnerAccounting (accounting config is per
    -- company, not a property of the partner itself) -- RESOLVED.
    SkipPrintInvoiceGroup BIT NULL, -- formerly chkSkipPrintRacunGrupa
    IsVatPayer           BIT NULL, -- formerly PDVobaveznik
    Jbjks                NVARCHAR(255) NULL, -- formerly JBJKS (budget beneficiary registry code)
    PartnerCity          NVARCHAR(255) NULL, -- formerly KupacGrad
    CountryCode          NVARCHAR(255) NULL, -- formerly ZemljaKod
    DeliveryLocation     NVARCHAR(255) NULL, -- formerly LokacijaDostava
    DeliveryUnitTypeCode INT NULL, -- formerly DostavaSifraPD
    PrintInvoiceMandatory BIT NULL,
    SendToPostOffice     BIT NULL,
    Language             NVARCHAR(255) NULL,
    ExtendedNote         NVARCHAR(MAX) NULL, -- formerly NapomenaExtended
    -- v1's IDMaster is REMOVED (confirmed) -- one Partner = one record, repetition per company goes
    -- through PartnerAccounting, not a self-FK here.
    CONSTRAINT FK_Partner_CategoryLocation FOREIGN KEY (CategoryLocationId) REFERENCES dbo.CategoryLocation(CategoryLocationId),
    CONSTRAINT FK_Partner_PartnerCategory FOREIGN KEY (CategoryId) REFERENCES dbo.PartnerCategory(PartnerCategoryId)
    -- FK_Partner_GroupInvoiceTag REMOVED -- FIX (preostala-pitanja A-2): the group-invoice tag moved
    -- to PartnerAccounting.GroupInvoiceTagId, see below.
);

-- resolves the Company <-> Partner circular dependency
ALTER TABLE dbo.Company ADD CONSTRAINT FK_Company_Partner_Representative
    FOREIGN KEY (RepresentativeId) REFERENCES dbo.Partner(PartnerId);

CREATE TABLE dbo.PartnerAccounting ( -- NEW TABLE -- partner's accounting role per company
    PartnerAccountingId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PartnerAccounting PRIMARY KEY,
    LegacyId             NVARCHAR(50) NULL, -- ETL: original ID_K/DobavljacKonto value where applicable
    PartnerId            INT NOT NULL,
    CompanyId            INT NOT NULL,
    Account              NVARCHAR(10) NULL, -- formerly "Konto" -- e.g. "2040" (customer) / "4350" (supplier)
    DefaultSubAccount    NVARCHAR(255) NULL, -- FK -> SubAccount, added below (SubAccount is defined later)
    ExternalAccount      NVARCHAR(255) NULL, -- moved from Partner
    AutoSubAccount       NVARCHAR(50) NULL,  -- moved from Partner, formerly AutoKontoTroska
    GroupInvoiceTagId    INT NULL, -- FIX (preostala-pitanja A-2): moved from Partner (formerly
    -- IDGrupniRacunMaster / GrupniRacunGrupaId). Boss confirmed this is an arbitrary shared tag
    -- (e.g. one value shared by 9 partners on a group invoice) with NO confirmed FK target -- kept as
    -- a plain grouping value, deliberately not a foreign key.
    CONSTRAINT FK_PartnerAccounting_Partner FOREIGN KEY (PartnerId) REFERENCES dbo.Partner(PartnerId),
    CONSTRAINT FK_PartnerAccounting_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId),
    CONSTRAINT UQ_PartnerAccounting UNIQUE (PartnerId, CompanyId, Account)
);

-- =====================================================================
-- 4. BUILDINGENTRANCE / UNIT / CONTRACT / UNITBILLINGALLOCATION
-- =====================================================================

CREATE TABLE dbo.BuildingEntrance ( -- formerly "SzUlaz"; "SzObjekat" REMOVED (covered by Unit.CompanyId+BuildingEntranceId)
    BuildingEntranceId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_BuildingEntrance PRIMARY KEY,
    CompanyId  INT NOT NULL,
    Entrance   NVARCHAR(255) NULL, -- formerly Ulaz
    Building   NVARCHAR(255) NULL, -- formerly Zgrada
    Address    NVARCHAR(255) NULL, -- formerly Adresa
    Label      NVARCHAR(255) NULL, -- formerly Oznaka
    Description NVARCHAR(255) NULL, -- formerly Opis
    SortOrder  INT NULL, -- formerly Sort
    CONSTRAINT FK_BuildingEntrance_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId)
);

CREATE TABLE dbo.Unit ( -- formerly "Objekti"
    UnitId          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Unit PRIMARY KEY,
    LegacyId        NVARCHAR(50) NULL, -- formerly ID_O
    CompanyId       INT NOT NULL,
    BuildingEntranceId INT NOT NULL, -- RESOLVED: every Unit must be linked to an entrance
    Name            NVARCHAR(255) NULL, -- formerly naziv
    UnitTypeId      INT NULL, -- formerly TipObjekta
    Status          INT NULL,
    -- IO REMOVED -- FIX (preostala-pitanja C, Unit.IO): boss confirmed unused, no data.
    InvestmentMaintenanceReserve DECIMAL(19,4) NULL, -- formerly RF_DIN
    InvestmentMaintenanceTitle   NVARCHAR(50) NULL, -- formerly IONaslov (kept -- distinct field, still has data)
    PreviousArea    DECIMAL(9,4) NULL, -- formerly StaraKv
    StatusChangeNote NVARCHAR(50) NULL, -- formerly STatusPromene
    Coefficient     DECIMAL(9,4) NULL, -- formerly Koeficijent
    Total           DECIMAL(19,4) NULL, -- formerly Ukupno
    EntranceLabel   NVARCHAR(50) NULL, -- formerly Ulaz (free-text duplicate of BuildingEntranceId, kept as-is from legacy)
    Category        NVARCHAR(5) NULL, -- formerly Kategorija
    Address         NVARCHAR(50) NULL, -- formerly Adresa
    Note            NVARCHAR(255) NULL, -- formerly Napomena
    MailingName     NVARCHAR(50) NULL, -- formerly Naziv_Slanja
    MailingAddress  NVARCHAR(50) NULL, -- formerly Adresa_Slanja
    MailingPostalCode NVARCHAR(50) NULL, -- formerly PBroj_Slanja
    MailingTaxId    NVARCHAR(50) NULL, -- formerly PIB_Slanja
    GarageSpotNumber INT NULL, -- ASSUMPTION: formerly BRGM
    K1 DECIMAL(9,4) NULL, K2 DECIMAL(9,4) NULL, K3 DECIMAL(9,4) NULL, K4 DECIMAL(9,4) NULL, K5 DECIMAL(9,4) NULL,
    -- K1..K5: per-unit calculation coefficients, exact per-type meaning driven by CalculationType --
    -- kept as opaque K1..K5 (same as v2), no single English word improves clarity without client input.
    MailingLegacyCode NVARCHAR(50) NULL, -- ASSUMPTION: formerly PAK_SLANJA
    Area            DECIMAL(9,4) NULL, -- formerly KV
    ResidentCount   INT NULL, -- formerly BrStanara
    UnitTypeCode    INT NULL, -- formerly BrojPD
    UnitTypeShortCode NVARCHAR(255) NULL, -- formerly SifraPD
    NetArea         DECIMAL(9,4) NULL, -- formerly netoKV
    Terrace         DECIMAL(9,4) NULL, -- formerly terasa
    NetAreaWithTerrace DECIMAL(9,4) NULL, -- formerly netoKVsaTerasom
    HandOverDate    DATE NULL,
    FloorNumber     INT NULL, -- formerly SpratN
    FloorText       NVARCHAR(255) NULL, -- formerly SpratT
    -- REMOVED vs v1: lnk_ID_K/IDVlasnik/IDZakupac (now via Contract),
    -- PLATILAC_lnk_ID_K/_K2 (now via UnitBillingAllocation), GrupniRacunId (group invoicing is at
    -- Partner/Invoice level, not Unit).
    CONSTRAINT FK_Unit_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId),
    CONSTRAINT FK_Unit_BuildingEntrance FOREIGN KEY (BuildingEntranceId) REFERENCES dbo.BuildingEntrance(BuildingEntranceId),
    CONSTRAINT FK_Unit_UnitType FOREIGN KEY (UnitTypeId) REFERENCES dbo.UnitType(UnitTypeId)
);

CREATE TABLE dbo.Contract ( -- NEW TABLE -- time-versioned owner/tenant/invoice-recipient history (formerly "ugovori")
    ContractId           INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Contract PRIMARY KEY,
    UnitId               INT NOT NULL,
    PartnerAccountingId  INT NOT NULL, -- FIX (preostala-pitanja A-3): boss confirmed this targets
    -- PartnerAccounting, not Partner directly -- "PartnerAccountingId je Konto koje se dodeljuju
    -- Partneru po Ugovoru." (the account assigned to the partner per contract).
    RoleId               INT NOT NULL, -- FK -> ContractRole (Owner/Tenant/Invoice Recipient)
    StartInvoicingDate   DATE NULL,
    EndInvoicingDate     DATE NULL,
    ContractStartDate    DATE NULL,
    ContractEndDate      DATE NULL,
    Note                 NVARCHAR(255) NULL,
    StatusId             INT NULL, -- similar to A-4: exact contract-status code list not defined yet (OPEN)
    CreatedDate          DATETIME2(0) NULL,
    CONSTRAINT FK_Contract_Unit FOREIGN KEY (UnitId) REFERENCES dbo.Unit(UnitId),
    CONSTRAINT FK_Contract_PartnerAccounting FOREIGN KEY (PartnerAccountingId) REFERENCES dbo.PartnerAccounting(PartnerAccountingId),
    CONSTRAINT FK_Contract_ContractRole FOREIGN KEY (RoleId) REFERENCES dbo.ContractRole(ContractRoleId)
);
-- Rule (application-level, not schema): when a contract with RoleId=Tenant is added, the invoice
-- recipient (active contract with RoleId=Invoice Recipient) switches to the tenant by default,
-- unless explicitly kept on the owner. See tehnicki-plan-faza1.md section 2.1.

CREATE TABLE dbo.UnitBillingAllocation ( -- NEW TABLE -- replaces the "duplicate the unit" workaround
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
-- Rule (application-level, cross-row, not a DB CHECK): the sum of active Percentage values for the
-- same Unit in the same period must equal 1.0000 (with a tolerance resolved via the rounding rules).

-- =====================================================================
-- 5. MULTI-TENANT VISIBILITY
-- =====================================================================

CREATE TABLE dbo.StaffCompany ( -- NEW TABLE -- which Company records a user is allowed to see
    StaffId   INT NOT NULL,
    CompanyId INT NOT NULL,
    CONSTRAINT PK_StaffCompany PRIMARY KEY (StaffId, CompanyId),
    CONSTRAINT FK_StaffCompany_Staff FOREIGN KEY (StaffId) REFERENCES dbo.Staff(StaffId),
    CONSTRAINT FK_StaffCompany_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId)
);

-- "Godina" table REMOVED -- FIX (preostala-pitanja C, Godina.GodinaId vs IDSZ): boss confirmed this
-- table came from another (larger) application that partitioned its ledger by year for volume
-- reasons; not needed here, safe to drop entirely. JournalEntry/Invoice keep a plain Year INT column
-- where a year grouping is genuinely needed (see JournalEntry below), with no separate Year table.

-- =====================================================================
-- 6. JOURNALENTRY (redesigned) + SUBACCOUNT
-- =====================================================================

CREATE TABLE dbo.JournalEntry ( -- formerly "Nalog" -- a posting voucher: a balanced batch of LedgerEntry rows
    -- REDESIGNED vs v1: JournalEntryId (surrogate) separated from JournalEntryNumber (business number,
    -- per CompanyId per Year, reset yearly). CompanyId is now NOT NULL (v1 bug fixed). Added
    -- JournalEntryStatus -- see the posting-trigger note above the LedgerEntry table definition below.
    JournalEntryId      INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_JournalEntry PRIMARY KEY,
    LegacyId             NVARCHAR(50) NULL, -- formerly Br_Nalog (Double) -- keeps the original value for ETL
    CompanyId            INT NOT NULL,
    Year                 INT NOT NULL, -- formerly Godina (plain year number, no FK -- see note above)
    JournalEntryNumber   INT NOT NULL, -- sequence number per (CompanyId, Year)
    JournalEntryStatus   TINYINT NOT NULL DEFAULT 0, -- 0 = Draft, 1 = Posted
    EntryDate            DATE NULL, -- formerly Datum
    Balance              DECIMAL(19,4) NULL, -- formerly Saldo
    Note                 NVARCHAR(255) NULL, -- formerly Napomena
    Reserved             NVARCHAR(50) NULL, -- formerly Reserve
    Description          NVARCHAR(50) NULL, -- formerly OpisNaloga
    AdditionalNotes      NVARCHAR(255) NULL, -- formerly DodatneNapomene
    JournalEntryTypeId   INT NULL, -- formerly TipNaloga
    RowVersion           ROWVERSION NOT NULL,
    CONSTRAINT FK_JournalEntry_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId),
    CONSTRAINT UQ_JournalEntry_Number UNIQUE (CompanyId, Year, JournalEntryNumber),
    CONSTRAINT CK_JournalEntry_Status CHECK (JournalEntryStatus IN (0,1))
);

CREATE TABLE dbo.SubAccount ( -- formerly "Troskovi_PodKonta"
    SubAccountCode      NVARCHAR(255) NOT NULL CONSTRAINT PK_SubAccount PRIMARY KEY, -- formerly PodKonto
    Name                NVARCHAR(255) NULL, -- formerly Naziv
    PreviousSubAccountCode NVARCHAR(255) NULL, -- self FK, hierarchy -- formerly SifraKnj_Prethodni
    CostAllocationTarget NVARCHAR(255) NULL, -- ASSUMPTION: formerly TrosakNa, exact use unconfirmed
    Interest            NVARCHAR(255) NULL, -- formerly Kamata
    CONSTRAINT FK_SubAccount_Previous FOREIGN KEY (PreviousSubAccountCode) REFERENCES dbo.SubAccount(SubAccountCode)
);
-- Rule (RESOLVED, from pitanjaZaContext.md): account 2410 NEVER has a SubAccount; account 2040 only
-- has a SubAccount for prepayments; account 4350 only has a SubAccount when temporarily held at a
-- partner (and it must then show up in the error listing if missing). Application-level rule
-- (depends on the Account value on the ledger row), not a DB CHECK.

-- FK from PartnerAccounting.DefaultSubAccount to SubAccount (deferred, SubAccount is only just defined)
ALTER TABLE dbo.PartnerAccounting ADD CONSTRAINT FK_PartnerAccounting_SubAccount
    FOREIGN KEY (DefaultSubAccount) REFERENCES dbo.SubAccount(SubAccountCode);

CREATE TABLE dbo.SubAccountDefaultSupplier ( -- formerly "Troskovi_PodKonta_DefDob"
    CompanyId                        INT NOT NULL,
    SubAccountCode                   NVARCHAR(255) NOT NULL,
    DefaultSupplierPartnerAccountingId INT NULL, -- formerly text DefDob, now a real FK relationship
    CONSTRAINT PK_SubAccountDefaultSupplier PRIMARY KEY (CompanyId, SubAccountCode),
    CONSTRAINT FK_SADS_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId),
    CONSTRAINT FK_SADS_SubAccount FOREIGN KEY (SubAccountCode) REFERENCES dbo.SubAccount(SubAccountCode),
    CONSTRAINT FK_SADS_PartnerAccounting FOREIGN KEY (DefaultSupplierPartnerAccountingId) REFERENCES dbo.PartnerAccounting(PartnerAccountingId)
);

-- =====================================================================
-- 7. INVOICING + REMINDERS (before LedgerEntry and SupplierInvoice, since LedgerEntry depends on
--    Invoice; reminders go before Invoice because Invoice.ReminderId points at Reminder)
-- =====================================================================

CREATE TABLE dbo.InvoiceBatch ( -- formerly "GrupaRacuna" -- one monthly billing run
    InvoiceBatchId      INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_InvoiceBatch PRIMARY KEY,
    Code                NVARCHAR(50) NULL, -- formerly GrupaRacunaFXN
    Label               NVARCHAR(50) NULL, -- formerly GrupaRacunaFXT
    Month               NVARCHAR(50) NULL, -- formerly Mesec
    Year                NVARCHAR(50) NULL, -- formerly Godina
    Place               NVARCHAR(50) NULL, -- formerly Mesto
    IssueDate           DATE NULL, -- formerly DatumIzdavanja
    ServiceDate         NVARCHAR(50) NULL, -- formerly DatumUsluge
    TransactionDate     DATE NULL, -- formerly DatumPrometa
    ValueDate           DATE NULL, -- formerly DatumValute
    ExchangeRateNbs     DECIMAL(19,4) NULL, -- ASSUMPTION: formerly NBS, likely the NBS exchange rate snapshot used for this run
    CompanyId           INT NOT NULL,
    JournalEntryId       INT NULL, -- formerly NalogKN
    SystemDate           DATETIME2(0) NULL, -- formerly DatumSys
    StaffId               INT NULL, -- formerly UserSys
    ExtraordinaryInvoiceMarker NVARCHAR(255) NULL, -- formerly MarkerVanderdnihRacuna
    InvoiceKind           NVARCHAR(255) NULL, -- formerly VrstaRacuna
    BalanceAsOfDate        DATE NULL, -- formerly DatumStanja
    PreviousValueDate        DATE NULL, -- formerly PrethodnaValuta
    IsInterestCalculated       BIT NULL, -- formerly ObracunKamate
    CONSTRAINT FK_InvoiceBatch_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId),
    CONSTRAINT FK_InvoiceBatch_JournalEntry FOREIGN KEY (JournalEntryId) REFERENCES dbo.JournalEntry(JournalEntryId),
    CONSTRAINT FK_InvoiceBatch_Staff FOREIGN KEY (StaffId) REFERENCES dbo.Staff(StaffId)
);

CREATE TABLE dbo.ReminderTemplate ( -- formerly "OpomenaSabloni"
    ReminderTemplateId  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ReminderTemplate PRIMARY KEY,
    ReminderTypeId       INT NULL, -- formerly lnkVrstaOpomene
    ReportName           NVARCHAR(255) NULL,
    Title                NVARCHAR(255) NULL, -- formerly Naslov
    ReportField01 NVARCHAR(255) NULL, ReportField02 NVARCHAR(255) NULL,
    ReportField03 NVARCHAR(MAX) NULL, ReportField04 NVARCHAR(MAX) NULL, ReportField05 NVARCHAR(MAX) NULL, ReportField06 NVARCHAR(MAX) NULL,
    ReportField07 NVARCHAR(255) NULL, ReportField08 NVARCHAR(255) NULL, ReportField09 NVARCHAR(255) NULL
);

CREATE TABLE dbo.ReminderTemplateField ( -- formerly "OpomenaSablonEx" -- RESTORED from excluded -- work in
    -- progress, flexible per-field reminder text
    ReminderTemplateFieldId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ReminderTemplateField PRIMARY KEY,
    ReminderTemplateId   INT NOT NULL,
    KeyName              NVARCHAR(255) NULL,
    KeyIndex             INT NULL,
    TemplateText         NVARCHAR(MAX) NULL, -- formerly SablonText
    CONSTRAINT FK_ReminderTemplateField_ReminderTemplate FOREIGN KEY (ReminderTemplateId) REFERENCES dbo.ReminderTemplate(ReminderTemplateId)
);

CREATE TABLE dbo.ReminderBatch ( -- formerly "GrupaOpomena"
    ReminderBatchId     INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ReminderBatch PRIMARY KEY,
    Title               NVARCHAR(50) NULL, -- formerly Naslov
    BatchDate           DATE NULL, -- formerly Datum
    MinUnpaidInvoiceCount INT NULL, -- formerly MinBNR
    DebtTolerance       INT NULL, -- formerly TolerancijaDuga
    DebtToleranceByMonth INT NULL, -- formerly TolerancijaDugaPoMesecu
    ReminderTemplateId  INT NULL, -- formerly lnkSablonOpomene
    ReminderTypeId      INT NULL, -- formerly lnkVrstaOpomene
    DebitPeriodDate     DATE NULL, -- ASSUMPTION: formerly DatumDI
    CreditPeriodDate    DATE NULL, -- ASSUMPTION: formerly DatumPI
    CompanyId           INT NULL,
    InvoiceBatchId      INT NULL, -- formerly lnkGrupaRacuna
    Label               NVARCHAR(255) NULL, -- formerly GrupaOpomenaTXT
    ReminderTemplateText NVARCHAR(255) NULL, -- formerly SablonTextOpomene
    RemittanceInfo       NVARCHAR(255) NULL, -- formerly Doznaka
    CONSTRAINT FK_ReminderBatch_ReminderTemplate FOREIGN KEY (ReminderTemplateId) REFERENCES dbo.ReminderTemplate(ReminderTemplateId),
    CONSTRAINT FK_ReminderBatch_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId),
    CONSTRAINT FK_ReminderBatch_InvoiceBatch FOREIGN KEY (InvoiceBatchId) REFERENCES dbo.InvoiceBatch(InvoiceBatchId)
);

CREATE TABLE dbo.Reminder ( -- formerly "Opomena"
    ReminderId       INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Reminder PRIMARY KEY,
    ReminderBatchId  INT NULL,
    PartnerAccountingId INT NULL,
    UnpaidInvoiceCount INT NULL, -- formerly BNR
    Debt             DECIMAL(19,4) NULL, -- formerly Dug
    InvoiceText      NVARCHAR(255) NULL, -- formerly txtRacunOp
    IsActive         BIT NULL, -- formerly ActivnaOpomena
    LineItemSum      DECIMAL(19,4) NULL, -- formerly SumaPoStavkama
    PaymentReference NVARCHAR(255) NULL, -- formerly PozivNaBroj
    Costs            DECIMAL(19,4) NULL, -- formerly Troskovi
    Total            DECIMAL(19,4) NULL, -- formerly Ukupno
    CONSTRAINT FK_Reminder_ReminderBatch FOREIGN KEY (ReminderBatchId) REFERENCES dbo.ReminderBatch(ReminderBatchId),
    CONSTRAINT FK_Reminder_PartnerAccounting FOREIGN KEY (PartnerAccountingId) REFERENCES dbo.PartnerAccounting(PartnerAccountingId)
);

CREATE TABLE dbo.Invoice ( -- formerly "Racun"
    InvoiceId            INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Invoice PRIMARY KEY,
    LegacyId              NVARCHAR(50) NULL,
    SequenceNumber        NVARCHAR(20) NULL, -- formerly RBR -- format CompanyId-PartnerAccountingId-YYMM
    InvoiceBatchId         INT NOT NULL, -- formerly lnkGR
    IssueDate              DATE NULL, -- formerly DatumIzdavanja
    PlaceOfIssue            NVARCHAR(50) NULL, -- formerly MestoIzdavanja
    ServiceDate              NVARCHAR(50) NULL, -- formerly DatumUsluge
    TransactionDate           DATE NULL, -- formerly DatumPrometa
    ValueDate                  DATE NULL, -- formerly DatumValute
    PartnerAccountingId          INT NOT NULL,
    CompanyId                     INT NOT NULL,
    PartnerName                    NVARCHAR(255) NULL, -- snapshot of the partner name at issue time (formerly the "Kupac" column)
    PostalCode                      NVARCHAR(50) NULL, -- formerly PBroj_K
    Address                          NVARCHAR(50) NULL, -- formerly Adresa_K
    TaxId                             NVARCHAR(20) NULL, -- formerly PIB
    RegistrationNumber                 NVARCHAR(255) NULL, -- formerly MB
    Sum                                 DECIMAL(19,4) NULL, -- formerly Suma
    VatRate                              DECIMAL(9,4) NULL, -- formerly PDVStopa
    VatAmount                             DECIMAL(19,4) NULL, -- formerly PDVIznos
    Total                                  DECIMAL(19,2) NULL, -- RESOLVED: final total at 2 decimals
    PreviousDebt                            DECIMAL(19,4) NULL, -- formerly PrethodniDug
    PaymentPurpose                           NVARCHAR(255) NULL, -- formerly SvrhaUplate
    Currency                                  NVARCHAR(50) NULL, -- formerly Valuta
    PaymentReference                           NVARCHAR(50) NULL, -- formerly PozivNaBroj
    PaymentSlipText                             NVARCHAR(255) NULL, -- ASSUMPTION: formerly PD_text
    Note                                          NVARCHAR(255) NULL, -- formerly Napomena
    PageCount                                     INT NULL DEFAULT 0, -- FIX (preostala-pitanja C, Racun.Co):
    -- boss confirmed this is used for complex invoices that print double-sided on perforated stock
    -- at the printer, and is used to sort/group invoices by page count. Kept as INT, renamed, default 0.
    IsCancelled                                    BIT NOT NULL DEFAULT 0, -- formerly Storno
    AdditionalNote                                  NVARCHAR(255) NULL, -- formerly extraNapomena
    UnitName                                         NVARCHAR(50) NULL, -- formerly objekatNaziv
    ManagerId                                         INT NULL, -- formerly Upravnik
    PaymentPurpose2                                    NVARCHAR(255) NULL, -- formerly SvrhaUplate2
    UnitId                                              INT NULL, -- formerly ID_OX
    UnitAddress                                          NVARCHAR(255) NULL, -- formerly AdresaProstora
    AmountDue                                             DECIMAL(19,2) NULL, -- formerly ZaUplatu
    MailingName                                            NVARCHAR(50) NULL, -- formerly Naziv_Slanja
    MailingAddress                                          NVARCHAR(50) NULL, -- formerly Adresa_Slanja
    MailingPostalCode                                        NVARCHAR(50) NULL, -- formerly PBroj_Slanja
    MailingTaxId                                              NVARCHAR(50) NULL, -- formerly PIB_Slanja
    QrCodeFlag                                                 INT NULL, -- ASSUMPTION: formerly SPC
    PaymentSlipAmount                                           DECIMAL(19,4) NULL, -- ASSUMPTION: formerly PD_iznos
    PaymentFieldTypeId                                           INT NULL, -- formerly TipPoljaZaUplatu
    ReminderId                                                    INT NULL, -- formerly lnkOpomenaID
    GroupInvoiceId                                                 INT NULL, -- FIXED vs v1 (was wrongly FK->Kupac) -- self FK to the group invoice
    City                                                            NVARCHAR(255) NULL, -- formerly Grad_K
    Location                                                         NVARCHAR(255) NULL, -- formerly Lokacija
    SortOrder                                                         INT NULL, -- formerly SortRacun
    CancelledDate                                                      DATE NULL, -- formerly DatumStorno
    InvoiceLayout                                                       NVARCHAR(255) NULL, -- formerly RacunShema
    BalanceAsOfDate                                                      DATE NULL, -- formerly DatumStanja
    InterestAmount                                                        DECIMAL(19,4) NULL, -- formerly KamataIznos
    InvoiceTotal                                                           DECIMAL(19,2) NULL, -- 2 decimals, formerly UkupnoRacun
    RowVersion                                                              ROWVERSION NOT NULL,
    -- tmpID/tmpID2/tmpPB/tmpPB2 from the source intentionally omitted (old app's scratch columns).
    CONSTRAINT FK_Invoice_InvoiceBatch FOREIGN KEY (InvoiceBatchId) REFERENCES dbo.InvoiceBatch(InvoiceBatchId),
    CONSTRAINT FK_Invoice_PartnerAccounting FOREIGN KEY (PartnerAccountingId) REFERENCES dbo.PartnerAccounting(PartnerAccountingId),
    CONSTRAINT FK_Invoice_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId),
    CONSTRAINT FK_Invoice_Unit FOREIGN KEY (UnitId) REFERENCES dbo.Unit(UnitId),
    CONSTRAINT FK_Invoice_Reminder FOREIGN KEY (ReminderId) REFERENCES dbo.Reminder(ReminderId),
    CONSTRAINT FK_Invoice_GroupInvoice FOREIGN KEY (GroupInvoiceId) REFERENCES dbo.Invoice(InvoiceId)
);

CREATE TABLE dbo.ReminderLine ( -- formerly "OpomenaStavke"
    ReminderLineId   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ReminderLine PRIMARY KEY,
    ReminderId       INT NULL,
    ReminderBatchId  INT NULL,
    PartnerAccountingId INT NULL,
    Parameters       NVARCHAR(25) NULL, -- formerly PARAMETRI
    DocumentRef      NVARCHAR(255) NULL, -- formerly DOK
    Debit            DECIMAL(19,4) NULL, -- formerly Di
    Credit           DECIMAL(19,4) NULL, -- formerly Pi
    Sum              DECIMAL(19,4) NULL, -- formerly Suma
    Text             NVARCHAR(255) NULL, -- formerly mTXT
    DueDate          DATE NULL, -- formerly DatumDospeca
    InvoiceId        INT NULL, -- formerly IDRacun
    InvoiceDate      DATE NULL, -- formerly DatumRacuna
    UnitAddress      NVARCHAR(255) NULL, -- formerly PProstor
    -- NOTE: ON DELETE CASCADE REMOVED vs v1 -- issued reminders are not physically deleted (only
    -- cancelled); deletion is only allowed for documents that were never issued (application-level
    -- check, not an FK cascade).
    CONSTRAINT FK_ReminderLine_Reminder FOREIGN KEY (ReminderId) REFERENCES dbo.Reminder(ReminderId),
    CONSTRAINT FK_ReminderLine_ReminderBatch FOREIGN KEY (ReminderBatchId) REFERENCES dbo.ReminderBatch(ReminderBatchId),
    CONSTRAINT FK_ReminderLine_PartnerAccounting FOREIGN KEY (PartnerAccountingId) REFERENCES dbo.PartnerAccounting(PartnerAccountingId),
    CONSTRAINT FK_ReminderLine_Invoice FOREIGN KEY (InvoiceId) REFERENCES dbo.Invoice(InvoiceId)
);

-- =====================================================================
-- 8. BANK STATEMENTS
-- =====================================================================

CREATE TABLE dbo.BankStatement ( -- formerly "Izvod"
    BankStatementId       INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_BankStatement PRIMARY KEY,
    LegacyId               NVARCHAR(50) NULL,
    StatementNumber       INT NULL, -- formerly BrojIzvoda
    StatementSuffix       NVARCHAR(50) NULL, -- formerly SufixIzvoda
    CompanyId             INT NOT NULL,
    StatementDate         DATE NULL, -- formerly Datum
    PreviousBalance       DECIMAL(19,4) NULL, -- formerly PrethodnoStanjeIzvoda
    NewBalance            DECIMAL(19,4) NULL, -- formerly NovoStanje
    Debit                 DECIMAL(19,4) NULL, -- formerly Duguje
    Credit                DECIMAL(19,4) NULL, -- formerly Potrazuje
    DebitJournalEntryId   INT NULL, -- formerly NalogaZaduzenja
    CreditJournalEntryId  INT NULL, -- formerly NalogaOdobranja
    Note                  NVARCHAR(50) NULL, -- formerly Napomena
    JournalEntryId        INT NULL, -- formerly NalogZaKnjizenje
    IsUnposted            BIT NOT NULL DEFAULT 0, -- formerly Rasknjizen
    RowVersion            ROWVERSION NOT NULL,
    CONSTRAINT FK_BankStatement_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId),
    CONSTRAINT FK_BankStatement_JournalEntry FOREIGN KEY (JournalEntryId) REFERENCES dbo.JournalEntry(JournalEntryId)
);

CREATE TABLE dbo.BankStatementLine ( -- formerly "IzvodStavke"
    BankStatementLineId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_BankStatementLine PRIMARY KEY,
    BankStatementId    INT NOT NULL,
    CompanyId          INT NULL,
    LineNumber         INT NULL, -- formerly RbStavke
    JournalEntryLineNumber INT NULL, -- formerly RbNaloga
    PayerRecipientName NVARCHAR(255) NULL, -- formerly NazivPN
    BankAccountNumber  NVARCHAR(50) NULL, -- formerly BrojRacuna (the counterparty's bank account, not an invoice number)
    Origin             NVARCHAR(50) NULL, -- formerly Poreklo
    ExecutionDate      DATE NULL, -- formerly DatumRealizacije
    Debit              DECIMAL(19,4) NULL, -- formerly Zaduzenje
    Credit             DECIMAL(19,4) NULL, -- formerly Odobrenje
    RemittanceInfo     NVARCHAR(255) NULL, -- formerly Doznaka
    Code               INT NULL, -- formerly Sifra
    PaymentReference   NVARCHAR(50) NULL, -- formerly PozivNaBroj
    PaymentReferenceOut NVARCHAR(50) NULL, -- formerly PozivNaBrojO
    PartnerAccountingId INT NULL, -- formerly opt_lnk_Kupac
    IsPosted           BIT NOT NULL DEFAULT 0, -- formerly Knjizeno
    IsIgnored          BIT NOT NULL DEFAULT 0, -- formerly Ignore
    IsMatched          BIT NOT NULL DEFAULT 0, -- ASSUMPTION: formerly SetP
    RowVersion         ROWVERSION NOT NULL,
    -- NOTE: ON DELETE CASCADE REMOVED (was a v1 bug for accounting documents).
    CONSTRAINT FK_BankStatementLine_BankStatement FOREIGN KEY (BankStatementId) REFERENCES dbo.BankStatement(BankStatementId),
    CONSTRAINT FK_BankStatementLine_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId),
    CONSTRAINT FK_BankStatementLine_PartnerAccounting FOREIGN KEY (PartnerAccountingId) REFERENCES dbo.PartnerAccounting(PartnerAccountingId)
);

CREATE TABLE dbo.BankAccount ( -- formerly "TekuciRacun"
    BankAccountId  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_BankAccount PRIMARY KEY,
    AccountNumber  NVARCHAR(255) NULL, -- formerly TR
    IsActive       BIT NULL, -- formerly Aktivan
    SortOrder      INT NULL,
    PartnerId      INT NULL, -- CONFIRMED (preostala-pitanja C): identity level, not PartnerAccounting -- company's own bank account
    CompanyId      INT NULL,
    ManagerId      INT NULL, -- formerly IDUPRAVNIK -> Staff
    CONSTRAINT FK_BankAccount_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId),
    CONSTRAINT FK_BankAccount_Partner FOREIGN KEY (PartnerId) REFERENCES dbo.Partner(PartnerId),
    CONSTRAINT FK_BankAccount_Staff FOREIGN KEY (ManagerId) REFERENCES dbo.Staff(StaffId)
);

CREATE TABLE dbo.PartnerBankAccount ( -- formerly "TRs"
    PartnerBankAccountId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PartnerBankAccount PRIMARY KEY,
    AccountNumber         NVARCHAR(50) NULL, -- formerly TR_L
    PartnerId             INT NULL, -- identity level, not per-company
    CONSTRAINT FK_PartnerBankAccount_Partner FOREIGN KEY (PartnerId) REFERENCES dbo.Partner(PartnerId)
);

-- =====================================================================
-- 9. SUPPLIERS (before LedgerEntry -- LedgerEntry.SupplierInvoiceId points here). "RacunIN" (inbound
--    invoice, an abandoned parallel table) is REMOVED -- confirmed abandoned.
-- =====================================================================

CREATE TABLE dbo.SupplierInvoice ( -- formerly "Dobavljac_Racuni"
    SupplierInvoiceId  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SupplierInvoice PRIMARY KEY,
    LegacyId            NVARCHAR(50) NULL,
    CompanyId           INT NULL,
    InvoiceNumber       INT NULL, -- formerly RacunNO
    InvoiceName         NVARCHAR(255) NULL, -- formerly NazivRacuna
    Note                NVARCHAR(255) NULL, -- formerly Napomena
    SupplierName        NVARCHAR(50) NULL, -- formerly Dobavljac
    SupplierPartnerAccountingId INT NULL, -- formerly DobavljacKonto (kept name below, see FK)
    CalculationTypeId   INT NULL, -- formerly TipObracuna
    InvoiceMonth        NVARCHAR(50) NULL, -- formerly MesecRacuna
    InvoiceAmountEur    DECIMAL(19,4) NULL, -- formerly IznosRacunaEUR
    InvoiceAmountRsd    DECIMAL(19,4) NULL, -- formerly IznosRacunaRSD
    AmountByCoefficientEur DECIMAL(19,4) NULL, -- formerly IznosPoKoefEUR
    AmountByCoefficientRsd DECIMAL(19,4) NULL, -- formerly IznosPoKoefRSD
    CollectionPriority  INT NULL, -- formerly PrioritetNaplate
    PostingCode         NVARCHAR(6) NULL, -- formerly SifraKN
    -- TmpPrevId REMOVED -- FIX (preostala-pitanja C, Dobavljac_Racuni.TmpPrevId): boss confirmed this
    -- is a temporary/unimportant legacy field.
    PostingAccount      NVARCHAR(255) NULL, -- formerly KontoKnjizenja
    DocumentTypeId       INT NULL, -- formerly TipDokumenta
    ExtraordinaryInvoiceMarker NVARCHAR(255) NULL, -- formerly MarkerVandrednogRacuna
    InvoiceNameFunction  NVARCHAR(255) NULL, -- ASSUMPTION: formerly FunkcijaNazivaRacuna
    SequenceNumber       NVARCHAR(50) NULL, -- formerly RBR
    PostedInvoiceAmount  DECIMAL(19,4) NULL, -- ASSUMPTION: formerly IznosRacunaKN
    InvoiceDate          DATE NULL, -- formerly DatumRacuna
    PostingDate          DATE NULL, -- formerly DatumKnjizenja
    PaymentDate          DATE NULL, -- formerly DatumPlacanja
    InvoiceDescription   NVARCHAR(255) NULL, -- formerly OpisRacuna
    PaymentReference     NVARCHAR(255) NULL, -- formerly PozivNaBroj
    PreviousSupplierInvoiceId INT NULL, -- formerly PrethodniIDRdob
    NewSupplierInvoiceId INT NULL, -- formerly NoviIDRdob
    JournalEntryId       INT NULL, -- formerly NalogKnjizenja
    Vat                  INT NULL, -- formerly PDV
    ClosesAccount        NVARCHAR(255) NULL, -- formerly ZatvaraKonto
    CONSTRAINT FK_SupplierInvoice_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId),
    CONSTRAINT FK_SupplierInvoice_PartnerAccounting FOREIGN KEY (SupplierPartnerAccountingId) REFERENCES dbo.PartnerAccounting(PartnerAccountingId),
    CONSTRAINT FK_SupplierInvoice_JournalEntry FOREIGN KEY (JournalEntryId) REFERENCES dbo.JournalEntry(JournalEntryId),
    CONSTRAINT FK_SupplierInvoice_Previous FOREIGN KEY (PreviousSupplierInvoiceId) REFERENCES dbo.SupplierInvoice(SupplierInvoiceId),
    CONSTRAINT FK_SupplierInvoice_New FOREIGN KEY (NewSupplierInvoiceId) REFERENCES dbo.SupplierInvoice(SupplierInvoiceId)
);

-- "KnjiznaDokumenta" table REMOVED -- FIX (preostala-pitanja C, KnjiznaDokumenta.KnjiznoDokumentIdRef):
-- boss confirmed this table was carried over from another application (for entering things like
-- opening/carried-forward balances) and should be dropped -- SupplierInvoice can be adapted to cover
-- that need instead. LedgerEntry.AccountingDocumentId (formerly KnDokID) is REMOVED accordingly.

-- =====================================================================
-- 10. LEDGERENTRY -- general ledger
-- =====================================================================

CREATE TABLE dbo.LedgerEntry ( -- formerly "GK" (Glavna Knjiga = General Ledger)
    LedgerEntryId     INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_LedgerEntry PRIMARY KEY,
    LegacyId          NVARCHAR(50) NULL,
    JournalEntryId    INT NOT NULL,
    Account           NVARCHAR(50) NOT NULL, -- formerly KONTO
    EntryDate         DATE NOT NULL, -- formerly DATUM
    DebitAmount       DECIMAL(19,4) NOT NULL DEFAULT 0, -- formerly DIZNOS
    CreditAmount      DECIMAL(19,4) NOT NULL DEFAULT 0, -- formerly PIZNOS
    LineTypeId        INT NULL, -- FK -> LineItemType.LineItemTypeId -- ADDED (boss asked for it)
    DocumentRef       NVARCHAR(50) NULL, -- formerly DOK
    CompanyId         INT NOT NULL,
    PartnerAccountingId INT NULL, -- formerly lnkKUPACID (SIFRAKONTA removed, confirmed)
    BankStatementLineId INT NULL, -- formerly lnkIzvodStavkaID
    Note              NVARCHAR(255) NULL, -- formerly NAPOMENA
    Parameters        NVARCHAR(25) NULL, -- formerly PARAMETRI
    Description       NVARCHAR(255) NULL, -- formerly OPIS
    DueDate           DATE NULL, -- FIX (preostala-pitanja C, GK.Dpo): renamed from "Dpo" -- boss
    -- confirmed "DPO" was carried over from another application and really means the due date
    -- (an issued invoice is posted with the transaction date "Datum"; DueDate is the date by which
    -- it must be paid).
    PostingCode       NVARCHAR(6) NULL, -- formerly SIFRAKN
    SupplierInvoiceId INT NULL, -- formerly RDOB
    InvoiceId         INT NULL, -- formerly RACID
    Priority          INT NULL, -- formerly PRIORITET
    PostingTypeCode   INT NULL, -- formerly KNzaTIP
    SubAccount        NVARCHAR(255) NULL, -- formerly KontoTroska
    -- AccountingDocumentId REMOVED -- KnjiznaDokumenta table dropped, see note above.
    -- SupplierInvoiceInboundId (RacunIN FK) REMOVED -- the RacunIN table was abandoned.
    CONSTRAINT FK_LedgerEntry_JournalEntry FOREIGN KEY (JournalEntryId) REFERENCES dbo.JournalEntry(JournalEntryId),
    CONSTRAINT FK_LedgerEntry_ChartOfAccounts FOREIGN KEY (Account) REFERENCES dbo.ChartOfAccounts(Account), -- ADDED
    -- (boss asked for it); FIX (preostala-pitanja C, Konta vs KontniOkvir): points at ChartOfAccounts
    -- now, not at the dropped "Konta" table.
    CONSTRAINT FK_LedgerEntry_LineItemType FOREIGN KEY (LineTypeId) REFERENCES dbo.LineItemType(LineItemTypeId), -- ADDED (boss asked for it)
    CONSTRAINT FK_LedgerEntry_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId),
    CONSTRAINT FK_LedgerEntry_PartnerAccounting FOREIGN KEY (PartnerAccountingId) REFERENCES dbo.PartnerAccounting(PartnerAccountingId),
    CONSTRAINT FK_LedgerEntry_BankStatementLine FOREIGN KEY (BankStatementLineId) REFERENCES dbo.BankStatementLine(BankStatementLineId),
    CONSTRAINT FK_LedgerEntry_Invoice FOREIGN KEY (InvoiceId) REFERENCES dbo.Invoice(InvoiceId),
    CONSTRAINT FK_LedgerEntry_SupplierInvoice FOREIGN KEY (SupplierInvoiceId) REFERENCES dbo.SupplierInvoice(SupplierInvoiceId),
    CONSTRAINT FK_LedgerEntry_SubAccount FOREIGN KEY (SubAccount) REFERENCES dbo.SubAccount(SubAccountCode)
);
GO

-- =====================================================================
-- POSTING -- CORRECTED DESIGN, replaces the wrong v1 trigger
-- =====================================================================
-- v1 BUG: the AFTER INSERT/UPDATE/DELETE trigger checked balance after EVERY LedgerEntry row change --
-- this would block normal entry (e.g. when EF inserts debit/credit as two separate INSERTs in the
-- same transaction, the first one would immediately fail as unbalanced).
--
-- FIX (client-approved): while JournalEntry.JournalEntryStatus = Draft (0), LedgerEntry rows can be
-- freely inserted/changed/deleted WITHOUT ANY balance check. The check only happens when posting is
-- attempted, through a dedicated procedure that does everything in one transaction: verify balance,
-- and only if OK, set JournalEntryStatus = Posted. Later changes to a posted entry are blocked
-- (reversal instead of direct edit) -- that is safe as a trigger because it only reads the parent's
-- status, it does not aggregate sibling rows.

CREATE PROCEDURE dbo.sp_PostJournalEntry
    @JournalEntryId INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    DECLARE @Diff DECIMAL(19,4);
    DECLARE @CurrentStatus TINYINT;

    -- UPDLOCK/HOLDLOCK prevents anyone else from changing this entry's LedgerEntry rows while the check runs
    SELECT @CurrentStatus = JournalEntryStatus
    FROM dbo.JournalEntry WITH (UPDLOCK, HOLDLOCK)
    WHERE JournalEntryId = @JournalEntryId;

    IF @CurrentStatus IS NULL
    BEGIN
        ROLLBACK TRANSACTION;
        THROW 50002, N'Journal entry does not exist.', 1;
        RETURN;
    END

    IF @CurrentStatus = 1
    BEGIN
        ROLLBACK TRANSACTION;
        THROW 50003, N'Journal entry is already posted -- use a reversal to change it.', 1;
        RETURN;
    END

    SELECT @Diff = ROUND(SUM(DebitAmount) - SUM(CreditAmount), 2)
    FROM dbo.LedgerEntry WITH (UPDLOCK)
    WHERE JournalEntryId = @JournalEntryId;

    IF @Diff IS NULL OR @Diff <> 0
    BEGIN
        ROLLBACK TRANSACTION;
        THROW 50001, N'Journal entry is not balanced (debit <> credit) -- posting rejected.', 1;
        RETURN;
    END

    UPDATE dbo.JournalEntry SET JournalEntryStatus = 1 WHERE JournalEntryId = @JournalEntryId;

    COMMIT TRANSACTION;
END;
GO

-- Blocks changing/deleting LedgerEntry rows whose JournalEntry is already posted (simple per-row
-- check of the parent's status -- safe, does not aggregate sibling rows, does not get in the way of
-- entry on a Draft journal entry).
CREATE TRIGGER dbo.TR_LedgerEntry_BlockEditOfPostedJournalEntry ON dbo.LedgerEntry
AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (
        SELECT 1
        FROM deleted d
        JOIN dbo.JournalEntry je ON je.JournalEntryId = d.JournalEntryId
        WHERE je.JournalEntryStatus = 1
    )
        THROW 50004, N'Cannot change/delete LedgerEntry rows of a posted journal entry -- use a reversal.', 1;
END;
GO

-- =====================================================================
-- 11. INVOICE LINES, BENEFITS, INTEREST
-- =====================================================================

CREATE TABLE dbo.InvoiceLine ( -- formerly "RacunStavke"
    InvoiceLineId   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_InvoiceLine PRIMARY KEY,
    InvoiceId       INT NOT NULL,
    -- RESTORED in v2/v3 (had been removed in v1): CompanyId/PartnerAccountingId/InvoiceBatchId are a
    -- DELIBERATE denormalization -- the boss explained the target partner/company for a line
    -- sometimes changes AFTER the invoice was issued (a sale contract arriving late, etc.), so the
    -- line must keep its ORIGINAL value independent of the current state of Invoice. Do not derive
    -- this via a JOIN to Invoice.
    CompanyId            INT NOT NULL,
    PartnerAccountingId  INT NOT NULL,
    InvoiceBatchId        INT NOT NULL,
    Name            NVARCHAR(255) NULL, -- formerly Naziv
    K1 DECIMAL(9,4) NULL, K2 DECIMAL(9,4) NULL, K3 DECIMAL(9,4) NULL, K4 DECIMAL(9,4) NULL, K5 DECIMAL(9,4) NULL,
    Quantity        DECIMAL(9,4) NULL, -- formerly Kolicina
    PriceEur        DECIMAL(19,4) NULL, -- formerly CenaE
    ExchangeRateNbs DECIMAL(19,4) NULL, -- formerly NBS
    Amount          DECIMAL(19,4) NULL, -- formerly Iznos
    Sum             DECIMAL(19,4) NULL, -- formerly Suma
    VatRate         DECIMAL(9,4) NULL, -- formerly PDVStopa
    VatAmount       DECIMAL(19,4) NULL, -- formerly PDVIznos
    TotalRsd        DECIMAL(19,2) NULL, -- RESOLVED: line total at 2 decimals, formerly UkupnoRSD
    SortOrder       INT NULL, -- formerly Sort
    SupplierAccountPartnerAccountingId INT NULL, -- formerly DobavljacKonto
    InvoiceAmount   DECIMAL(19,4) NULL, -- formerly IZNOSRACUNA
    K1xK2 DECIMAL(9,4) NULL, K2xK3 DECIMAL(9,4) NULL, K2xK4 DECIMAL(9,4) NULL, K2xK5 DECIMAL(9,4) NULL,
    UnitOfMeasure   NVARCHAR(255) NULL, -- formerly JM
    UnitId          INT NULL,
    QuantityAlt     DECIMAL(9,4) NULL, -- ASSUMPTION: formerly "M", exact use unconfirmed
    SupplierInvoiceId INT NULL,
    -- CalculationTypeId REMOVED (boss: the link via SupplierInvoiceId is enough).
    -- NOTE: ON DELETE CASCADE REMOVED (issued lines are not physically deleted).
    CONSTRAINT FK_InvoiceLine_Invoice FOREIGN KEY (InvoiceId) REFERENCES dbo.Invoice(InvoiceId),
    CONSTRAINT FK_InvoiceLine_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId),
    CONSTRAINT FK_InvoiceLine_PartnerAccounting FOREIGN KEY (PartnerAccountingId) REFERENCES dbo.PartnerAccounting(PartnerAccountingId),
    CONSTRAINT FK_InvoiceLine_InvoiceBatch FOREIGN KEY (InvoiceBatchId) REFERENCES dbo.InvoiceBatch(InvoiceBatchId),
    CONSTRAINT FK_InvoiceLine_Unit FOREIGN KEY (UnitId) REFERENCES dbo.Unit(UnitId),
    CONSTRAINT FK_InvoiceLine_SupplierInvoice FOREIGN KEY (SupplierInvoiceId) REFERENCES dbo.SupplierInvoice(SupplierInvoiceId),
    CONSTRAINT FK_InvoiceLine_SupplierAccountPartnerAccounting FOREIGN KEY (SupplierAccountPartnerAccountingId) REFERENCES dbo.PartnerAccounting(PartnerAccountingId)
);

CREATE TABLE dbo.InvoiceUnit ( -- formerly "RacunObjekti" -- links an invoice to units for printing
    InvoiceId  INT NOT NULL,
    UnitId     INT NOT NULL,
    CONSTRAINT PK_InvoiceUnit PRIMARY KEY (InvoiceId, UnitId), -- composite PK is enough, no surrogate needed
    CONSTRAINT FK_InvoiceUnit_Invoice FOREIGN KEY (InvoiceId) REFERENCES dbo.Invoice(InvoiceId),
    CONSTRAINT FK_InvoiceUnit_Unit FOREIGN KEY (UnitId) REFERENCES dbo.Unit(UnitId)
);

CREATE TABLE dbo.InvoiceLineBenefitArchive ( -- formerly "RacunStavkeBenefitArhiva"
    -- Archive snapshot -- keeps the original id from the source, DELIBERATELY without FKs to live tables.
    InvoiceLineId   INT NOT NULL CONSTRAINT PK_InvoiceLineBenefitArchive PRIMARY KEY,
    InvoiceId       INT NULL,
    InvoiceBatchId  INT NULL,
    PartnerAccountingId INT NULL,
    CompanyId       INT NULL,
    SupplierInvoiceId INT NULL,
    Name            NVARCHAR(255) NULL,
    K1 DECIMAL(9,4) NULL, K2 DECIMAL(9,4) NULL, K3 DECIMAL(9,4) NULL, K4 DECIMAL(9,4) NULL, K5 DECIMAL(9,4) NULL,
    Quantity DECIMAL(9,4) NULL, PriceEur DECIMAL(19,4) NULL, ExchangeRateNbs DECIMAL(19,4) NULL,
    Amount DECIMAL(19,4) NULL, Sum DECIMAL(19,4) NULL, VatRate DECIMAL(9,4) NULL, VatAmount DECIMAL(19,4) NULL,
    TotalRsd DECIMAL(19,2) NULL, SortOrder INT NULL, SupplierAccountPartnerAccountingId INT NULL, InvoiceAmount DECIMAL(19,4) NULL,
    K1xK2 DECIMAL(9,4) NULL, K2xK3 DECIMAL(9,4) NULL, K2xK4 DECIMAL(9,4) NULL, K2xK5 DECIMAL(9,4) NULL,
    UnitOfMeasure NVARCHAR(255) NULL, UnitId INT NULL
);

CREATE TABLE dbo.BenefitGroup ( -- formerly "BenefitGrupa"
    BenefitGroupId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_BenefitGroup PRIMARY KEY,
    Name           NVARCHAR(255) NULL, -- formerly BenefitNaziv
    PrintLabel     NVARCHAR(255) NULL -- formerly BenefitPrint
);

CREATE TABLE dbo.Benefit ( -- formerly "Benefiti"
    BenefitId       INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Benefit PRIMARY KEY,
    UnitId          INT NULL,
    PartnerAccountingId INT NULL,
    PeriodYyMm      NVARCHAR(255) NULL, -- formerly MesecYYMM
    IsUsed          BIT NULL, -- formerly Used
    EntryDate       DATETIME2(0) NULL, -- formerly DateEntry
    UsedDate        DATETIME2(0) NULL, -- formerly DateUsed
    InvoiceId       INT NULL, -- formerly RacunId
    BenefitGroupId  INT NULL,
    CancelledInvoiceId INT NULL, -- formerly RacunStornoID
    CONSTRAINT FK_Benefit_Unit FOREIGN KEY (UnitId) REFERENCES dbo.Unit(UnitId),
    CONSTRAINT FK_Benefit_PartnerAccounting FOREIGN KEY (PartnerAccountingId) REFERENCES dbo.PartnerAccounting(PartnerAccountingId),
    CONSTRAINT FK_Benefit_Invoice FOREIGN KEY (InvoiceId) REFERENCES dbo.Invoice(InvoiceId),
    CONSTRAINT FK_Benefit_CancelledInvoice FOREIGN KEY (CancelledInvoiceId) REFERENCES dbo.Invoice(InvoiceId),
    CONSTRAINT FK_Benefit_BenefitGroup FOREIGN KEY (BenefitGroupId) REFERENCES dbo.BenefitGroup(BenefitGroupId)
);

CREATE TABLE dbo.BenefitUsageUpdate ( -- formerly "BenefitUpdate" -- RESTORED from excluded -- ACTIVE
    -- (units that do not pay, benefit tracking)
    BenefitUsageUpdateId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_BenefitUsageUpdate PRIMARY KEY,
    UpdateDate      DATE NULL, -- formerly Datum
    UnitId          INT NULL, -- formerly text "UnitApp"
    PeriodYyMm      INT NULL, -- formerly "YYMM"
    MonthCount      INT NULL, -- formerly CountMM
    CONSTRAINT FK_BenefitUsageUpdate_Unit FOREIGN KEY (UnitId) REFERENCES dbo.Unit(UnitId)
);

CREATE TABLE dbo.InterestStatement ( -- formerly "KamatniList"
    InterestStatementId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_InterestStatement PRIMARY KEY,
    CompanyId       INT NULL,
    Account         NVARCHAR(50) NULL, -- formerly Konto
    StatementDate   DATE NULL, -- formerly Datum
    BaseAmount      DECIMAL(19,4) NULL, -- ASSUMPTION: formerly DIPI
    Balance         DECIMAL(19,4) NULL, -- formerly Saldo
    Days            INT NULL, -- formerly Dana
    Rate            DECIMAL(9,4) NULL, -- formerly Stopa
    Coefficient     DECIMAL(18,8) NULL, -- formerly Koeficijent
    Interest        DECIMAL(19,4) NULL, -- formerly Kamata
    ReferenceTag    NVARCHAR(50) NULL, -- ASSUMPTION: formerly PAR
    PartnerAccountingId INT NULL, -- formerly partnerID
    UnitId          INT NULL, -- formerly prostorID
    SubAccountCode  NVARCHAR(255) NULL, -- formerly PodKonto
    InvoiceBatchId  INT NULL, -- formerly IDGR
    CONSTRAINT FK_InterestStatement_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId),
    CONSTRAINT FK_InterestStatement_PartnerAccounting FOREIGN KEY (PartnerAccountingId) REFERENCES dbo.PartnerAccounting(PartnerAccountingId),
    CONSTRAINT FK_InterestStatement_Unit FOREIGN KEY (UnitId) REFERENCES dbo.Unit(UnitId),
    CONSTRAINT FK_InterestStatement_InvoiceBatch FOREIGN KEY (InvoiceBatchId) REFERENCES dbo.InvoiceBatch(InvoiceBatchId),
    CONSTRAINT FK_InterestStatement_SubAccount FOREIGN KEY (SubAccountCode) REFERENCES dbo.SubAccount(SubAccountCode)
);

CREATE TABLE dbo.InterestRate ( -- formerly "Stope"
    InterestRateId  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_InterestRate PRIMARY KEY,
    RateDate        DATE NULL, -- formerly Datum
    Rate            DECIMAL(19,4) NULL, -- formerly Stopa
    Period          NVARCHAR(50) NULL
    -- Currently entered manually; the client has existing code to pull this automatically from the
    -- National Bank of Serbia (NBS) (planned for a later phase, no schema impact).
);

CREATE TABLE dbo.PenaltyInterestStaging ( -- formerly "ZK" (Zatezna Kamata = late-payment/default interest) --
    -- CONFIRMED: working/staging table for penalty interest before it is posted to LedgerEntry.
    PenaltyInterestStagingId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PenaltyInterestStaging PRIMARY KEY,
    CompanyId     INT NULL,
    JournalEntryId INT NULL,
    Account       NVARCHAR(50) NULL, -- formerly Konto
    EntryDate     DATE NULL, -- formerly Datum
    CreditAmount  DECIMAL(19,4) NULL, -- formerly Piznos
    DebitAmount   DECIMAL(19,4) NULL, -- formerly Diznos
    LineTypeId    INT NULL, -- formerly TipStavke
    Parameters    NVARCHAR(25) NULL, -- formerly Parametri
    Note          NVARCHAR(20) NULL, -- formerly Napomena
    PartnerAccountingId INT NULL,
    ValueDate     DATE NULL, -- formerly DatumValute
    InvoiceId     INT NULL,
    Document      NVARCHAR(255) NULL, -- formerly Dokument
    SubAccountCode NVARCHAR(255) NULL, -- formerly PodKonto
    InvoiceBatchId INT NULL, -- formerly IDGR
    PostingSubAccount NVARCHAR(255) NULL, -- formerly KNPodKonto
    PostingSupplierInvoiceId INT NULL, -- formerly KNDob
    SupplierInvoiceId INT NULL, -- formerly RDOB
    Priority      INT NULL -- formerly Prioritet
);

-- "RacunStavke_Troskovi" table REMOVED -- FIX (preostala-pitanja C, RacunStavke_Troskovi): boss
-- confirmed this aggregate "cache" table is a leftover concept from the previous SZ app version and
-- the plan is to drop it -- use a real-time SUM query against InvoiceLine instead.

CREATE TABLE dbo.PostingScheme ( -- formerly "SemaKnjizenja"
    PostingSchemeId  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PostingScheme PRIMARY KEY,
    Name             NVARCHAR(255) NULL, -- formerly SemaKnjizenja
    SourceTable      NVARCHAR(255) NULL,
    SourceTableWhereField NVARCHAR(255) NULL, -- formerly SourceTableWhrField
    Line             NVARCHAR(255) NULL, -- formerly Stavka
    Account          NVARCHAR(255) NULL, -- formerly Konto
    DebitCreditRule  NVARCHAR(255) NULL, -- formerly DIPI
    Sign             INT NULL, -- formerly Znak
    SubAnalyticField NVARCHAR(255) NULL, -- formerly SubAnlField
    SubAnalyticFieldSource NVARCHAR(255) NULL, -- formerly SubAnlFieldSource
    Description      NVARCHAR(255) NULL, -- formerly Opis
    LedgerLineTypeId INT NULL, -- formerly GK_TipStavke
    LedgerDocumentFunction NVARCHAR(255) NULL, -- formerly GK_DokFn
    LedgerPaymentReference NVARCHAR(255) NULL, -- formerly GK_PNB
    LedgerPartnerId  NVARCHAR(255) NULL, -- formerly GK_IDPrtner
    LedgerInvoiceId  NVARCHAR(255) NULL, -- formerly GK_RacunID
    SortOrder        INT NULL, -- formerly Sort
    JournalEntryDescription NVARCHAR(255) NULL, -- formerly Nalog_Opis
    SourceSql        NVARCHAR(MAX) NULL,
    SourceSqlValue   NVARCHAR(255) NULL
);

CREATE TABLE dbo.BankStatementPostingTemplate ( -- formerly "TemplateIzvodaKnjizenje"
    BankStatementPostingTemplateId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_BankStatementPostingTemplate PRIMARY KEY,
    TemplateGroupId INT NULL, -- formerly IDTemplate
    Template      NVARCHAR(255) NULL,
    FieldName     NVARCHAR(255) NULL,
    FieldValue    NVARCHAR(255) NULL,
    Function      NVARCHAR(255) NULL,
    SetId         INT NULL, -- formerly SETID
    OverrideFunction INT NULL, -- ASSUMPTION: formerly FFunction
    SetAccount    NVARCHAR(50) NULL -- formerly SETKONTO
);

CREATE TABLE dbo.EPaymentOrderSettingGroup ( -- formerly "Settings_eNalog_Grupa" -- field mapping/format
    -- settings used to generate electronic bank payment order files ("eNalog" = electronic payment order)
    EPaymentOrderSettingGroupId INT NOT NULL CONSTRAINT PK_EPaymentOrderSettingGroup PRIMARY KEY,
    Category    NVARCHAR(50) NULL, -- formerly Kategorija
    SqlMemo     NVARCHAR(MAX) NULL, -- formerly SQL_memo
    Sql         NVARCHAR(255) NULL
);

CREATE TABLE dbo.EPaymentOrderSetting ( -- formerly "Settings_eNalog"
    EPaymentOrderSettingId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_EPaymentOrderSetting PRIMARY KEY,
    EPaymentOrderSettingGroupIndex INT NULL,
    CharacterCount INT NULL, -- formerly BrCHR
    CharacterType NVARCHAR(50) NULL, -- formerly TipCHR
    Function      NVARCHAR(50) NULL, -- formerly Funkcija
    Category      NVARCHAR(50) NULL, -- formerly Kategorija
    Description   NVARCHAR(255) NULL, -- formerly Opis
    Defaults      NVARCHAR(50) NULL, -- formerly Defs
    FieldName     NVARCHAR(50) NULL,
    Format        NVARCHAR(50) NULL,
    RowNumber     INT NULL, -- formerly rN
    CONSTRAINT FK_EPaymentOrderSetting_Group FOREIGN KEY (EPaymentOrderSettingGroupIndex) REFERENCES dbo.EPaymentOrderSettingGroup(EPaymentOrderSettingGroupId)
);

CREATE TABLE dbo.SupplierInvoiceUnitType ( -- formerly "Dobavljaci_Racun_TipObjekta"
    SupplierInvoiceUnitTypeId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SupplierInvoiceUnitType PRIMARY KEY,
    SupplierInvoiceId  INT NULL,
    UnitTypeId         INT NULL,
    CONSTRAINT FK_SupplierInvoiceUnitType_SupplierInvoice FOREIGN KEY (SupplierInvoiceId) REFERENCES dbo.SupplierInvoice(SupplierInvoiceId),
    CONSTRAINT FK_SupplierInvoiceUnitType_UnitType FOREIGN KEY (UnitTypeId) REFERENCES dbo.UnitType(UnitTypeId)
);

-- =====================================================================
-- 12. PAYMENT ORDERS, MAIL, FILES, SELECTIONBASKET
-- =====================================================================

CREATE TABLE dbo.PaymentOrder ( -- formerly "Virman" -- CONFIRMED: payment slip/QR/payment order for
    -- settling a supplier invoice at the bank, carried over from another application
    PaymentOrderId    INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PaymentOrder PRIMARY KEY,
    TemplateTitle     NVARCHAR(50) NULL, -- formerly NaslovSablona
    PayerName         NVARCHAR(255) NULL, -- formerly Nalogodavac
    PaymentPurpose    NVARCHAR(255) NULL, -- formerly SvrhaPlacanja
    RecipientName     NVARCHAR(255) NULL, -- formerly Primalac
    PaymentCode       SMALLINT NULL, -- formerly SifraPlacanja
    Currency          NVARCHAR(3) NULL, -- formerly Valuta
    Amount            DECIMAL(19,4) NULL, -- formerly Iznos
    PayerAccountNumber NVARCHAR(50) NULL, -- formerly BrojRacunaNalogodavca
    PayerModelNumber  SMALLINT NULL, -- formerly BrojModelaNal
    PayerPaymentReference NVARCHAR(50) NULL, -- formerly PozivNaBrojNal
    RecipientAccountNumber NVARCHAR(50) NULL, -- formerly BrojRacunaPrimaoca
    RecipientModelNumber SMALLINT NULL, -- formerly BrojModelaPrim
    RecipientPaymentReference NVARCHAR(50) NULL, -- formerly PozivNaBrojPrim
    Place             NVARCHAR(50) NULL, -- formerly Mesto
    OrderDate         DATE NULL, -- formerly Datum
    ValueDate         DATE NULL, -- formerly DatumV
    IsUrgent          BIT NOT NULL DEFAULT 0, -- formerly Hitno
    TypeId            INT NULL, -- formerly Tip
    ArchivedAt        DATETIME2(0) NULL, -- formerly Arhivirano
    RefSourceId       INT NULL, -- polymorphic reference (application-level validation, same as Attachment)
    RefSourceTag      NVARCHAR(255) NULL,
    PrintMe           BIT NOT NULL DEFAULT 0,
    IsFavorite        BIT NOT NULL DEFAULT 0 -- formerly Favorit
);

CREATE TABLE dbo.PartnerEmail ( -- formerly "Mail"
    PartnerEmailId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PartnerEmail PRIMARY KEY,
    PartnerId      INT NULL, -- identity level -- email does not depend on the accounting role
    Email          NVARCHAR(255) NULL,
    SortOrder      INT NULL,
    LoginAppMaster BIT NULL,
    LoginAppView   BIT NULL,
    SendInvoiceByEmail BIT NULL, -- formerly SendMailRacun
    CONSTRAINT FK_PartnerEmail_Partner FOREIGN KEY (PartnerId) REFERENCES dbo.Partner(PartnerId)
);

CREATE TABLE dbo.SentEmail ( -- formerly "Mail_Send"
    SentEmailId  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SentEmail PRIMARY KEY,
    Subject      NVARCHAR(255) NULL,
    ToAddress    NVARCHAR(255) NULL,
    Cc           NVARCHAR(255) NULL,
    Bcc          NVARCHAR(255) NULL,
    Body         NVARCHAR(MAX) NULL,
    BodyHtml     NVARCHAR(MAX) NULL,
    DateCreated  DATETIME2(0) NULL,
    DateSent     DATETIME2(0) NULL, -- formerly DateSend
    Archive      BIT NOT NULL DEFAULT 0,
    ErrorDescription NVARCHAR(MAX) NULL,
    ErrorStatus  NVARCHAR(255) NULL
);

CREATE TABLE dbo.SentEmailAttachment ( -- formerly "Mail_Send_Attachment"
    SentEmailAttachmentId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SentEmailAttachment PRIMARY KEY,
    SentEmailId    INT NOT NULL,
    AttachmentFilePath  NVARCHAR(255) NULL,
    AttachmentFilePath2 NVARCHAR(255) NULL,
    CONSTRAINT FK_SentEmailAttachment_SentEmail FOREIGN KEY (SentEmailId) REFERENCES dbo.SentEmail(SentEmailId) ON DELETE CASCADE
    -- CASCADE is fine here -- an attachment has no independent business/accounting meaning without its email.
);

CREATE TABLE dbo.Attachment ( -- formerly "Files" -- CONFIRMED: the polymorphic reference is deliberate
    -- (e.g. a partner's digital document archive)
    AttachmentId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Attachment PRIMARY KEY, -- formerly IDDokument
    DocumentTypeId INT NULL, -- formerly IDDocType
    ReferenceItemId INT NULL, -- formerly IDRefItem
    FileNameSuffix NVARCHAR(255) NULL, -- formerly FileNameSufix
    FileName     NVARCHAR(255) NULL,
    RelativePath NVARCHAR(255) NULL, -- formerly RelPathName -- B-3: where files physically live is not decided yet
    DateAdded    DATETIME2(0) NULL, -- formerly DateAdd
    Description  NVARCHAR(255) NULL, -- formerly Opis
    FileExtension NVARCHAR(255) NULL, -- formerly Ext
    SourceTable  NVARCHAR(255) NULL, -- formerly TabSource
    Registrar    NVARCHAR(4) NULL -- formerly Registrator
);

CREATE TABLE dbo.SelectionBasket ( -- RESTORED from excluded (formerly "PrinterBinLOCAL") -- boss: good
    -- practice, keep it
    SelectionBasketId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SelectionBasket PRIMARY KEY,
    StaffId    INT NULL,
    TargetTable NVARCHAR(255) NULL,
    TargetId    INT NULL,
    BatchTag    NVARCHAR(255) NULL,
    CreatedDate DATETIME2(0) NULL,
    CONSTRAINT FK_SelectionBasket_Staff FOREIGN KEY (StaffId) REFERENCES dbo.Staff(StaffId)
);

-- =====================================================================
-- 13. SYSTEM / AUTH / AUDIT
-- =====================================================================

CREATE TABLE dbo.StaffPermission ( -- formerly "StaffPermition" (spelling fixed)
    -- The permission/role model is deliberately DEFERRED to Phase 4/5 (B-2) -- boss: root/property
    -- manager/moderator/review/residents, but "left for later". This table stays flexible, not
    -- mapped 1:1 to the old Access forms.
    StaffPermissionId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_StaffPermission PRIMARY KEY,
    StaffId          INT NULL,
    KeyName          NVARCHAR(255) NULL,
    FormName         NVARCHAR(255) NULL,
    PermissionValue  INT NULL, -- formerly PermitionVal
    IsDisabled       BIT NULL, -- formerly DisablePermition
    CONSTRAINT FK_StaffPermission_Staff FOREIGN KEY (StaffId) REFERENCES dbo.Staff(StaffId)
);

CREATE TABLE dbo.AuditLog ( -- formerly "_Log"
    AuditLogId    INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AuditLog PRIMARY KEY,
    ComputerName  NVARCHAR(15) NULL, -- formerly PC
    WindowsUsername NVARCHAR(50) NULL, -- formerly WinUser
    StaffId       INT NULL,
    UserDisplayName NVARCHAR(50) NULL, -- formerly UserTXT
    EventDate     DATETIME2(0) NULL, -- formerly Datum
    FormName      NVARCHAR(60) NULL, -- formerly Forma
    TableCode     NVARCHAR(50) NULL, -- formerly TabCode
    ItemId        INT NULL,
    ActionType    NVARCHAR(20) NULL,
    ExtraMessage  NVARCHAR(255) NULL, -- formerly msgExtra
    ErrorNumber   NVARCHAR(255) NULL, -- formerly msgErrNum
    Message       NVARCHAR(255) NULL, -- formerly msg
    ChangeMessage NVARCHAR(MAX) NULL, -- formerly msgPromene
    Module        NVARCHAR(50) NULL,
    CompanyId     INT NULL, -- ADDED -- some actions are cross-company
    CorrelationId UNIQUEIDENTIFIER NULL, -- ADDED
    CONSTRAINT FK_AuditLog_Staff FOREIGN KEY (StaffId) REFERENCES dbo.Staff(StaffId),
    CONSTRAINT FK_AuditLog_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId)
);
-- NOTE: the application role writing to this table must not have UPDATE/DELETE (insert-only).

CREATE TABLE dbo.Events ( -- formerly "Promene" -- NOT a system table, the workflow needs a human actor
    EventId         INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Events PRIMARY KEY, -- formerly IDChange
    DateOfRequest   DATETIME2(0) NULL,
    DateOfExecution DATETIME2(0) NULL,
    PartnerId       INT NULL, -- formerly IDK
    UnitId          INT NULL, -- formerly IDO
    Description     NVARCHAR(255) NULL,
    PreviousValue   NVARCHAR(255) NULL,
    NewValue        NVARCHAR(255) NULL,
    FieldsRelated   NVARCHAR(255) NULL,
    RequestType     NVARCHAR(255) NULL,
    RequestBy       NVARCHAR(255) NULL,
    RequestThrough  NVARCHAR(255) NULL,
    StatusId          INT NULL, -- ADDED: B-1, exact status code list not confirmed yet
    ApprovedByStaffId INT NULL, -- ADDED: B-1
    ApprovedDate      DATETIME2(0) NULL, -- ADDED: B-1
    CONSTRAINT FK_Events_Partner FOREIGN KEY (PartnerId) REFERENCES dbo.Partner(PartnerId),
    CONSTRAINT FK_Events_Unit FOREIGN KEY (UnitId) REFERENCES dbo.Unit(UnitId),
    CONSTRAINT FK_Events_Staff FOREIGN KEY (ApprovedByStaffId) REFERENCES dbo.Staff(StaffId)
);

CREATE TABLE dbo.Note (
    NoteId    INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Note PRIMARY KEY,
    NoteDate  NVARCHAR(50) NULL, -- formerly Datum
    UserName  NVARCHAR(50) NULL, -- formerly Korisnik
    Text      NVARCHAR(MAX) NULL -- formerly Notes
);

CREATE TABLE dbo.Setting ( -- formerly "Settings"
    SettingId     INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Setting PRIMARY KEY,
    SettingName   NVARCHAR(255) NULL,
    SettingValue  NVARCHAR(255) NULL, -- formerly SettingVal
    Description   NVARCHAR(255) NULL, -- formerly Descrition (typo fixed)
    Category      NVARCHAR(50) NULL,
    ModuleFormField NVARCHAR(50) NULL, -- ASSUMPTION: formerly MFF
    DefaultValue  NVARCHAR(255) NULL, -- formerly DefVal
    FilterUserId  INT NULL,
    FilterComputerName NVARCHAR(255) NULL, -- formerly FilterPC
    FilterCustom1Num INT NULL, FilterCustom2Num INT NULL, FilterCustom3Num INT NULL,
    FilterCustom1Text NVARCHAR(255) NULL, FilterCustom2Text NVARCHAR(255) NULL, FilterCustom3Text NVARCHAR(255) NULL,
    SettingValueLong NVARCHAR(MAX) NULL, -- formerly SettingVal_LT
    CONSTRAINT FK_Setting_Staff FOREIGN KEY (FilterUserId) REFERENCES dbo.Staff(StaffId)
);

CREATE TABLE dbo.EmailSetting ( -- formerly "Settings_eMail"
    -- SECURITY NOTE: SettingValue may contain passwords/tokens for mail accounts. MUST NOT remain
    -- plain text in production -- use user-secrets/Key Vault or an encrypted column before this
    -- table is populated with real credentials.
    EmailSettingId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_EmailSetting PRIMARY KEY,
    SettingName   NVARCHAR(255) NULL,
    SettingValue  NVARCHAR(MAX) NULL, -- formerly SettingVal
    Category      NVARCHAR(255) NULL,
    Description   NVARCHAR(255) NULL, -- formerly Descrition
    CompanyId     INT NULL,
    CONSTRAINT FK_EmailSetting_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId)
);

-- =====================================================================
-- END OF v3 DRAFT
-- =====================================================================
-- Deliberately OMITTED from this draft (see tehnicki-plan-faza1.md sections 4-5 and
-- preostala-pitanja.md B-5):
--   - InboundInvoice (formerly "RacunIN"): removed, confirmed abandoned.
--   - Dynamic reporting system (ReportDefinition/AnalysisReportDefinition/CodeListCatalog/
--     StatReportDefinition/WhereClauseTemplate/CodeList's report-adjacent siblings, formerly
--     tblIzvestaj/tblAnaliza/tblSifrarnik/tblSTATS/tblWhrEx/tblShortList) -- actively used daily,
--     but needs its own design (safe execution of user-entered SQL), planned as a separate phase,
--     not a plain table migration.
--   - Indexes, additional UNIQUE and CHECK constraints (B-4) -- deliberately deferred until real
--     data is profiled in Phase 4, not guessed ahead of time.
--
-- Open questions that still affect this draft: A-4 (Company code lists: PaymentSlipTypeId/
-- CompanyStatusId/SubjectTypeId). Everything else from the original open-questions list (A-1, A-2,
-- A-3, and the "C" column-level items) has been answered by the client and applied above. See
-- docs/preostala-pitanja.md for the exact answers and docs/tehnicki-plan-faza1.md section 8 for the
-- prior readiness verdict.
