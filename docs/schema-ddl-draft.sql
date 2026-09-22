-- =====================================================================
-- SZ App -- target SQL Server schema draft -- v4 (rebuilt from the boss-authored docs/data-model.md)
-- =====================================================================
-- v4 supersedes v3. The boss (šef) rewrote docs/data-model.md directly -- it is now the source of
-- truth for the target schema, ahead of this file. v4 is a straight SQL Server translation of every
-- table in docs/data-model.md (Access type -> SQL Server type per docs/tehnicki-plan-faza1.md section
-- 1), using the FK targets already written as comments in data-model.md wherever given. Business
-- rename decisions (Skustina->Company, Kupac->Partner, GK->LedgerEntry, Racun->Invoice, Nalog->
-- JournalEntry, Izvod->BankStatement, Opomena->Notice, Dobavljac_Racuni->SupplierInvoice, Kurs->
-- ExchangeRate, KontniOkvir->ChartOfAccounts, Virman->PaymentOrder, tblShortList->ShortList, etc.)
-- were made by the boss in data-model.md itself, not re-decided here.
--
-- docs/data-model.md is the source of truth, in full -- if a v3 table/column has no counterpart
-- there, that means the boss deliberately dropped or merged it, not that it's missing. Four small
-- gaps found while translating it were fixed directly below rather than left as open questions:
--   - NoticeBatch.NoticeTemplateId had no matching table -- added a minimal dbo.NoticeTemplate (not
--     in data-model.md, flag it to the boss whenever convenient in case he has a different shape).
--   - Contract is one row with three parallel Partner FKs (Owner/Invoice/Tenant), not one row per
--     role -- added an explicit application-level rule comment on the table: changes INSERT a new
--     row and close the old one via ContractEndDate, never UPDATE in place, so history is kept.
--   - Contract.AccountNumber joins to PartnerAccount via the business key (CompanyId, AccountNumber),
--     not a surrogate FK -- left as a plain column, not a FOREIGN KEY; this is fine as-is.
--   - LedgerEntry.LineTypeId is typed "Double" in data-model.md despite being an FK to a code list --
--     a typo in the source doc; modeled as INT below (data-model.md itself is left untouched).
-- PenaltyInterestStaging/"ZK" is confirmed dropped on purpose (data-model.md's own comment: "POTPUNO
-- ISTA TABELA KAO GK LEDGER" -- the staging step is just LedgerEntry rows on an unposted
-- JournalEntry, no separate table). UnitBillingAllocation, StaffCompany (covered by StaffAccess),
-- SubAccountDefaultSupplier, BenefitGroup, BenefitUsageUpdate, EPaymentOrderSetting(Group),
-- PartnerBankAccount, StaffPermission, EmailSetting are all confirmed intentionally absent too.
--
-- The dynamic reporting system (AnalysisReportDefinition/ReportDefinition/ReportDefinitionDetail/
-- ReportDefinitionButtons) now has a real schema, transcribed below -- but adding columns does not by
-- itself solve safe execution of the raw SQL text stored in SourceSql/QuerySql; that's a genuine
-- follow-up implementation task (read-only connection / whitelisting), not a business-logic question.
--
-- Everything from preostala-pitanja.md A-1/A-2/A-3/A-4/C that data-model.md answers by construction
-- (e.g. Company no longer has UplatnicaTip/SkStatus/TipSubjekta at all; Kurs/ExchangeRate already has
-- the requested Id/Rate/Date/EntryType/UpdateDateTime shape; Konta and Godina and KnjiznaDokumenta and
-- RacunStavke_Troskovi are simply absent, matching "delete it") is resolved.
--
-- Money: kept at the precision data-model.md actually specifies per column (mostly DECIMAL(18,x) or
-- DECIMAL(19,4)), rather than forcing one blanket precision -- data-model.md is authoritative on this
-- now, see docs/tehnicki-plan-faza1.md section 1 for the general Access->SQL Server mapping rules
-- still in force for anything not spelled out in data-model.md.
-- =====================================================================

-- =====================================================================
-- 1. CODE LISTS
-- =====================================================================

CREATE TABLE dbo.ShortList ( -- generic shared code-list mechanism, used across many unrelated small
    -- lists (distinguished by TableName), e.g. TableName='AuditActionType', IndexValue=1, Caption='...'
    ShortListId  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ShortList PRIMARY KEY,
    TableName    NVARCHAR(255) NOT NULL, -- logical list this row belongs to (e.g. "UnitType", "ChannelComms")
    Caption      NVARCHAR(255) NOT NULL,
    ShortName    NVARCHAR(50) NULL,
    Description  NVARCHAR(255) NULL,
    IndexValue   INT NOT NULL, -- the actual code value stored on FK columns elsewhere
    IndexSort    INT NOT NULL,
    IndexKey     NVARCHAR(50) NULL, -- looks up a function/setting by key for this specific value
    TranslationId INT NULL -- if > 0, use Translations instead of Caption
);

CREATE TABLE dbo.Languages (
    Code       NVARCHAR(10) NOT NULL CONSTRAINT PK_Languages PRIMARY KEY, -- srLat / srCyr / en
    Name       NVARCHAR(50) NULL,
    IsActive   BIT NOT NULL,
    IsDefault  BIT NOT NULL,
    SortIndex  INT NULL
);

CREATE TABLE dbo.Translations (
    TranslationId INT NOT NULL CONSTRAINT PK_Translations PRIMARY KEY,
    LanguageCode  NVARCHAR(10) NOT NULL,
    ResourceKey   NVARCHAR(100) NULL, -- e.g. "Common.Save"
    ResourceId    INT NULL, -- e.g. 1 for "Months" -> "Januar"; 0/NULL = generic key
    CompanyId     INT NULL, -- per-company override; 0/NULL = default for all companies
    Translation   NVARCHAR(MAX) NULL,
    CONSTRAINT FK_Translations_Languages FOREIGN KEY (LanguageCode) REFERENCES dbo.Languages(Code)
);

CREATE TABLE dbo.CalculationType ( -- billing/calculation method (by area, by coefficient, fixed, ...)
    CalculationTypeId INT NOT NULL CONSTRAINT PK_CalculationType PRIMARY KEY,
    Name             NVARCHAR(100) NULL,
    SupplierInvoiceAmount NVARCHAR(255) NULL, -- function name: total invoice amount per tenant
    Amount           NVARCHAR(255) NULL, -- function name: amount per tenant invoice
    Quantity         NVARCHAR(255) NULL, -- function name: quantity per tenant invoice
    UnitOfMeasureId  INT NULL, -- FK -> ShortList (TableName='UnitOfMeasure')
    Note             NVARCHAR(255) NULL
);

CREATE TABLE dbo.DocumentCategory ( -- optional category for e-archive documents
    DocumentCategoryId INT NOT NULL CONSTRAINT PK_DocumentCategory PRIMARY KEY,
    GroupName          NVARCHAR(255) NULL,
    Name               NVARCHAR(255) NULL,
    Code               NVARCHAR(10) NULL,
    Description        NVARCHAR(255) NULL,
    IsActive           BIT NULL,
    RetentionPeriodYy  INT NULL
);

CREATE TABLE dbo.ChartOfAccounts (
    Account         NVARCHAR(10) NOT NULL CONSTRAINT PK_ChartOfAccounts PRIMARY KEY,
    ShortName       NVARCHAR(50) NULL, -- used in the app when a shorter name is needed; NULL falls back to Name
    Name            NVARCHAR(255) NULL,
    ParentAccount   NVARCHAR(10) NULL,
    Level           INT NULL, -- possibly redundant with Account length, kept as-is
    Sign            INT NULL, -- default 1 or -1, sign flip for reports
    IsActive        BIT NULL,
    IsSinteticAccount BIT NULL,
    CONSTRAINT FK_ChartOfAccounts_Parent FOREIGN KEY (ParentAccount) REFERENCES dbo.ChartOfAccounts(Account)
);

CREATE TABLE dbo.SubAccount (
    SubAccountId        NVARCHAR(10) NOT NULL CONSTRAINT PK_SubAccount PRIMARY KEY,
    Name                 NVARCHAR(50) NULL,
    ParentSubAccountId   NVARCHAR(10) NULL,
    CostToSubAccountId   NVARCHAR(10) NULL,
    InterestSubAccountId NVARCHAR(10) NULL,
    IsActive              BIT NULL, -- default TRUE
    DefaultSupplierPartnerAccountId INT NULL, -- FK -> PartnerAccount, added below (PartnerAccount is defined later)
    CONSTRAINT FK_SubAccount_Parent FOREIGN KEY (ParentSubAccountId) REFERENCES dbo.SubAccount(SubAccountId),
    CONSTRAINT FK_SubAccount_CostTo FOREIGN KEY (CostToSubAccountId) REFERENCES dbo.SubAccount(SubAccountId),
    CONSTRAINT FK_SubAccount_Interest FOREIGN KEY (InterestSubAccountId) REFERENCES dbo.SubAccount(SubAccountId)
);

CREATE TABLE dbo.ExchangeRate (
    ExchangeRateId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ExchangeRate PRIMARY KEY,
    Rate           DECIMAL(19,4) NULL, -- always 4 decimals
    RateDateFrom   DATE NULL,
    UpdatedAt      DATETIME2(0) NULL -- formerly "TimeStamp"
    -- NOTE: data-model.md's ExchangeRate does not carry an EntryTypeId (manual/auto/import) column even
    -- though preostala-pitanja C asked for one ("IdEntryType iz tblShortList") -- treated as dropped
    -- on purpose, per data-model.md being the final source of truth.
);

CREATE TABLE dbo.LocationCategory (
    LocationCategoryId INT NOT NULL CONSTRAINT PK_LocationCategory PRIMARY KEY,
    Name               NVARCHAR(255) NULL, -- e.g. "Belville", "BW", "Plot24", "Plot25"
    ParentId           INT NULL, -- self FK
    SortIndex          INT NULL,
    CONSTRAINT FK_LocationCategory_Parent FOREIGN KEY (ParentId) REFERENCES dbo.LocationCategory(LocationCategoryId)
);

-- =====================================================================
-- 2. ADDRESS / STAFF
-- =====================================================================

CREATE TABLE dbo.Address (
    AddressId   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Address PRIMARY KEY,
    Address      NVARCHAR(255) NULL,
    PostalCode   INT NULL,
    City         NVARCHAR(255) NULL,
    CountryCode  NVARCHAR(2) NULL -- default from Settings, e.g. "RS"
);

CREATE TABLE dbo.Staff (
    StaffId    INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Staff PRIMARY KEY,
    UserName   NVARCHAR(255) NULL, -- email
    -- Password goes through ASP.NET Core Identity, not a plain-text column -- see CLAUDE.md.
    PreferredLanguage NVARCHAR(50) NULL,
    LastIp     NVARCHAR(255) NULL,
    LastLoginTimeStamp DATETIME2(0) NULL,
    IsActive   BIT NULL
);

-- =====================================================================
-- 3. PARTNER / COMPANY / PARTNERACCOUNT
-- =====================================================================

CREATE TABLE dbo.Partner (
    PartnerId          INT NOT NULL CONSTRAINT PK_Partner PRIMARY KEY,
    CompanyId          INT NULL, -- NULL = usable across all companies (default for suppliers)
    ShortName          NVARCHAR(100) NULL, -- used in the app
    Name               NVARCHAR(255) NULL, -- full legal name for companies
    -- legal entities
    RegistrationNumber NVARCHAR(10) NULL, -- MB, 8 chars
    TaxNumber          NVARCHAR(10) NULL, -- PIB, 9 chars, also used for foreign entities
    Jbkjs              NVARCHAR(10) NULL, -- budget-beneficiary registry code, 5 chars
    IsSefUser          BIT NULL, -- eFaktura (SEF); check by TaxNumber/Jbkjs before sending
    IsCrfUser          BIT NULL, -- manual entry -- possibly CRF if it has a TaxNumber and SEF
    SkipAutoCheckSef   BIT NULL, -- locked out of the auto pre-send SEF check (per-partner exception)
    -- individuals
    IdCardNumber       NVARCHAR(10) NULL, -- 9 chars
    Jmbg               NVARCHAR(15) NULL, -- 13 chars
    PartnerTypeId      INT NULL, -- FK -> ShortList (TableName='PartnerType')
    Language           NVARCHAR(10) NULL, -- default srLat
    Note               NVARCHAR(MAX) NULL,
    CONSTRAINT FK_Partner_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId) -- forward ref, added via ALTER at the bottom
);

CREATE TABLE dbo.PartnerAddress ( -- separate from Address because Address is also used by BuildingEntrance
    PartnerAddressId INT NOT NULL CONSTRAINT PK_PartnerAddress PRIMARY KEY,
    PartnerId        INT NULL,
    AddressId        INT NULL,
    AddressTypeId    INT NULL, -- FK -> ShortList (TableName='AddressType')
    IsDefault        BIT NULL, -- if more than one row of the same AddressTypeId
    UpdatedAt        DATETIME2(0) NULL, -- formerly "TimeStamp"
    CONSTRAINT FK_PartnerAddress_Partner FOREIGN KEY (PartnerId) REFERENCES dbo.Partner(PartnerId),
    CONSTRAINT FK_PartnerAddress_Address FOREIGN KEY (AddressId) REFERENCES dbo.Address(AddressId)
);

CREATE TABLE dbo.PartnerComms ( -- unified phone/email/other contact channels, replaces the old
    -- single-purpose Mail table
    PartnerCommsId   INT NOT NULL CONSTRAINT PK_PartnerComms PRIMARY KEY,
    PartnerId        INT NULL,
    ChannelId        INT NULL, -- FK -> ShortList (TableName='ChannelComms')
    ValueNormalized  NVARCHAR(255) NULL, -- digits only for phone, lower-case email, "http..." for web
    Note             NVARCHAR(255) NULL, -- e.g. contact person for a legal entity
    IsActive         BIT NULL,
    IsPrimary        BIT NULL,
    SortIndex        INT NULL,
    IsRegisterToInvoiceReceive BIT NULL,
    CONSTRAINT FK_PartnerComms_Partner FOREIGN KEY (PartnerId) REFERENCES dbo.Partner(PartnerId)
);

CREATE TABLE dbo.Company (
    CompanyId          INT NOT NULL CONSTRAINT PK_Company PRIMARY KEY,
    PartnerId          INT NOT NULL, -- FK -> Partner
    ManagerId          INT NULL, -- FK -> Partner (the property manager acting for this HOA)
    ShortName          NVARCHAR(50) NULL,
    PrintName          NVARCHAR(50) NULL,
    RelativeFolderName NVARCHAR(50) NULL,
    LocationCategoryId INT NULL,
    Note               NVARCHAR(255) NULL,
    SortIndex          INT NULL,
    CompanyTypeId      INT NULL, -- FK -> ShortList (TableName='CompanyType')
    ExternalAccount    NVARCHAR(255) NULL, -- external bookkeeping agency reference
    LedgerEntryDate    DATE NULL, -- posting start date
    VatTypeId          INT NULL, -- FK -> ShortList (TableName='VatType')
    CONSTRAINT FK_Company_Partner FOREIGN KEY (PartnerId) REFERENCES dbo.Partner(PartnerId),
    CONSTRAINT FK_Company_Manager FOREIGN KEY (ManagerId) REFERENCES dbo.Partner(PartnerId),
    CONSTRAINT FK_Company_LocationCategory FOREIGN KEY (LocationCategoryId) REFERENCES dbo.LocationCategory(LocationCategoryId)
);
-- resolves the Partner <-> Company circular dependency (Partner.CompanyId, Company.PartnerId)
ALTER TABLE dbo.Partner ADD CONSTRAINT FK_Partner_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId);

-- =====================================================================
-- 4. BUILDINGENTRANCE / UNIT / CONTRACT / PARTNERACCOUNT
-- =====================================================================

CREATE TABLE dbo.BuildingEntrance (
    BuildingEntranceId INT NOT NULL CONSTRAINT PK_BuildingEntrance PRIMARY KEY,
    CompanyId          INT NULL,
    BuildingName       NVARCHAR(255) NULL,
    EntranceName       NVARCHAR(255) NULL,
    AddressId          INT NULL,
    BuildingLabel      NVARCHAR(255) NULL, -- official building marker in the land registry (RGZ)
    Description        NVARCHAR(255) NULL,
    SortIndex          INT NULL,
    CONSTRAINT FK_BuildingEntrance_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId),
    CONSTRAINT FK_BuildingEntrance_Address FOREIGN KEY (AddressId) REFERENCES dbo.Address(AddressId)
);

CREATE TABLE dbo.Unit (
    UnitId              INT NOT NULL CONSTRAINT PK_Unit PRIMARY KEY,
    CompanyId           INT NOT NULL,
    Name                NVARCHAR(255) NULL,
    ContractId          INT NULL, -- unassigned units are NULL
    UnitTypeId          INT NULL, -- FK -> ShortList (TableName='UnitType') -- apartment/unit/business space
    BuildingEntranceId  INT NULL, -- FK -> BuildingEntrance (also where the address comes from)
    Note                NVARCHAR(255) NULL,
    SortingNumber       INT NULL, -- apartments 1-999, business spaces 1000-9999, garage spots 10000*level+seq
    K1 DECIMAL(9,4) NULL, -- area used for billing
    K2 DECIMAL(9,4) NULL, -- quantity for billing, default 1, can be scaled (e.g. 1.5 for accessible spot)
    K3 DECIMAL(9,4) NULL, -- free coefficient, e.g. ground-floor yards, mowing billing
    K4 DECIMAL(9,4) NULL, -- free coefficient, e.g. garage tags, retiree discount for management fee only
    K5 DECIMAL(9,4) NULL,
    FloorNumber         INT NULL,
    CONSTRAINT FK_Unit_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId),
    CONSTRAINT FK_Unit_Contract FOREIGN KEY (ContractId) REFERENCES dbo.Contract(ContractId), -- forward ref, added via ALTER at the bottom
    CONSTRAINT FK_Unit_BuildingEntrance FOREIGN KEY (BuildingEntranceId) REFERENCES dbo.BuildingEntrance(BuildingEntranceId)
);

CREATE TABLE dbo.Contract ( -- one row per Unit per validity period, carrying owner/tenant/invoice-recipient
    -- together (NOT one row per role -- data-model.md's deliberate design)
    ContractId           INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Contract PRIMARY KEY,
    AccountNumber        INT NULL, -- NOT a formal FK -- business-key join to PartnerAccount.AccountNumber (see docs/preostala-pitanja.md section D)
    UnitId               INT NULL, -- FK -> Unit
    OwnerPartnerId       INT NULL, -- FK -> Partner
    InvoicePartnerId     INT NULL, -- FK -> Partner, defaults to owner; if a tenant exists the user picks between the two
    TenetPartnerId       INT NULL, -- FK -> Partner
    ContractDate         DATE NULL, -- contract date or hand-over date
    ContractEndDate      DATE NULL,
    InvoiceStartDate     DATE NULL,
    InvoiceEndDate       DATE NULL,
    IsActive             BIT NULL,
    Note                 NVARCHAR(MAX) NULL,
    -- invoice delivery
    InvoiceDeliveryLocation NVARCHAR(50) NULL, -- text printed before the unit on the invoice, e.g. "BW-ETE"
    InvoiceDeliveryUnitId   INT NULL, -- FK -> Unit
    InvoiceLegacyMasterId   INT NULL, -- id of the group invoice master record, if used
    -- default is to print the invoice
    IsPrintInvoiceMandatory   BIT NULL, -- printing explicitly required by the client
    IsPrintInvoiceToPostOffice BIT NULL, -- sent via post
    IsPrintInvoiceSkiped      BIT NULL, -- request to skip printing (requires an e-invoice, and both of the above false)
    ExportExternalAccount     NVARCHAR(255) NULL, -- link to an external bookkeeping agency
    CONSTRAINT FK_Contract_Unit FOREIGN KEY (UnitId) REFERENCES dbo.Unit(UnitId),
    CONSTRAINT FK_Contract_Owner FOREIGN KEY (OwnerPartnerId) REFERENCES dbo.Partner(PartnerId),
    CONSTRAINT FK_Contract_InvoiceRecipient FOREIGN KEY (InvoicePartnerId) REFERENCES dbo.Partner(PartnerId),
    CONSTRAINT FK_Contract_Tenant FOREIGN KEY (TenetPartnerId) REFERENCES dbo.Partner(PartnerId),
    CONSTRAINT FK_Contract_InvoiceDeliveryUnit FOREIGN KEY (InvoiceDeliveryUnitId) REFERENCES dbo.Unit(UnitId)
);
-- Rule (application-level, not enforceable as a DB constraint): a change of owner/tenant/invoice
-- recipient always INSERTs a new Contract row (close the old one via ContractEndDate) -- never UPDATE
-- OwnerPartnerId/InvoicePartnerId/TenetPartnerId on an existing row in place. That's what keeps the
-- ownership/tenancy history readable now that there's one row per period instead of one row per role.

-- resolves the Unit <-> Contract circular dependency (Unit.ContractId, Contract.UnitId)
ALTER TABLE dbo.Unit ADD CONSTRAINT FK_Unit_Contract FOREIGN KEY (ContractId) REFERENCES dbo.Contract(ContractId);

CREATE TABLE dbo.PartnerAccount ( -- partner's accounting role per company (the "who owes/is owed what,
    -- through which account" record) -- formerly modeled as "PartnerAccounting" in v3, boss shortened
    -- the name to PartnerAccount in data-model.md
    PartnerAccountId INT NOT NULL CONSTRAINT PK_PartnerAccount PRIMARY KEY,
    CompanyId        INT NULL, -- NULL for suppliers used across all companies
    Account          NVARCHAR(10) NOT NULL, -- FK -> ChartOfAccounts, e.g. "2040" (customer) / "4350" (supplier)
    PartnerId        INT NOT NULL,
    ContractId       INT NULL, -- suppliers have no contract, NULL in that case
    AccountNumber    INT NOT NULL, -- generated, unique per CompanyId: "2040" -> 1001-5999, "4350" -> 9001+ -- used for search
    CONSTRAINT FK_PartnerAccount_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId),
    CONSTRAINT FK_PartnerAccount_ChartOfAccounts FOREIGN KEY (Account) REFERENCES dbo.ChartOfAccounts(Account),
    CONSTRAINT FK_PartnerAccount_Partner FOREIGN KEY (PartnerId) REFERENCES dbo.Partner(PartnerId),
    CONSTRAINT FK_PartnerAccount_Contract FOREIGN KEY (ContractId) REFERENCES dbo.Contract(ContractId),
    CONSTRAINT UQ_PartnerAccount_AccountNumber UNIQUE (CompanyId, AccountNumber)
);
ALTER TABLE dbo.SubAccount ADD CONSTRAINT FK_SubAccount_DefaultSupplierPartnerAccount
    FOREIGN KEY (DefaultSupplierPartnerAccountId) REFERENCES dbo.PartnerAccount(PartnerAccountId);

CREATE TABLE dbo.BankAccount ( -- formerly "TekuciRacun"
    BankAccountId  INT NOT NULL CONSTRAINT PK_BankAccount PRIMARY KEY,
    AccountNumber  NVARCHAR(50) NULL,
    IsActive       INT NULL,
    PartnerId      INT NULL, -- identity level, not per-company: a bank account is a property of the partner
    CompanyId      INT NULL, -- only set for the company's own account (used when importing its bank statements)
    SortIndex      INT NULL,
    Currency       NVARCHAR(3) NULL, -- default from Settings, RSD
    CONSTRAINT FK_BankAccount_Partner FOREIGN KEY (PartnerId) REFERENCES dbo.Partner(PartnerId),
    CONSTRAINT FK_BankAccount_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId)
);

-- =====================================================================
-- 5. STAFF ACCESS
-- =====================================================================

CREATE TABLE dbo.StaffAccess ( -- which companies a staff member can see, and their role there
    -- (replaces v3's "StaffCompany"; StaffRole is a plain INT for now -- role model still deferred, B-2)
    StaffAccessId INT NOT NULL CONSTRAINT PK_StaffAccess PRIMARY KEY,
    StaffId       INT NULL,
    CompanyId     INT NULL,
    StaffRole     INT NULL,
    CONSTRAINT FK_StaffAccess_Staff FOREIGN KEY (StaffId) REFERENCES dbo.Staff(StaffId),
    CONSTRAINT FK_StaffAccess_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId)
);

-- =====================================================================
-- 6. JOURNALENTRY (posting voucher)
-- =====================================================================

CREATE TABLE dbo.JournalEntry ( -- a posting voucher: a batch of LedgerEntry rows that must balance
    -- before posting. See the posting procedure/trigger design in docs/tehnicki-plan-faza1.md section
    -- 2.4 -- unchanged in v4: Draft rows are free to edit, balance is only checked at Draft->Posted.
    JournalEntryId     INT NOT NULL CONSTRAINT PK_JournalEntry PRIMARY KEY,
    CompanyId          INT NULL,
    PostingDate        DATE NULL,
    DueDate            DATE NULL,
    Balance            DECIMAL(19,4) NULL,
    Note               NVARCHAR(255) NULL,
    Description        NVARCHAR(50) NULL,
    JournalEntryTypeId INT NULL, -- FK -> ShortList (TableName='LedgerLineType')
    Currency           NVARCHAR(3) NULL, -- default from Settings, RSD
    IsPosted           BIT NOT NULL DEFAULT 0,
    PostedDate         DATETIME2(0) NULL,
    PostedUserId       INT NULL, -- FK -> Staff
    CONSTRAINT FK_JournalEntry_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId),
    CONSTRAINT FK_JournalEntry_PostedBy FOREIGN KEY (PostedUserId) REFERENCES dbo.Staff(StaffId)
);
-- NOTE: v3's separate JournalEntryNumber-per-(CompanyId,Year) business number is not present in
-- data-model.md's JournalEntry -- treated as dropped on purpose, JournalEntryId serves that role now.

-- =====================================================================
-- 7. INVOICE BATCHES + NOTICES (before Invoice, since Invoice.NoticeId points at Notice)
-- =====================================================================

CREATE TABLE dbo.InvoiceBatch ( -- one monthly billing run, formerly "GrupaRacuna"
    InvoiceBatchId     INT NOT NULL CONSTRAINT PK_InvoiceBatch PRIMARY KEY,
    CompanyId          INT NULL,
    PeriodYyMm         INT NULL, -- e.g. 2609
    Caption            NVARCHAR(50) NULL, -- e.g. "Septembar 2026"
    Month              INT NULL,
    Year               INT NULL,
    Place              NVARCHAR(50) NULL, -- always the company's registered seat
    IssueDate          DATE NULL,
    ServiceDateFrom    DATE NULL,
    ServiceDateTo      DATE NULL,
    TransactionDate    DATE NULL,
    DueDate            DATE NULL,
    ExchangeRateNbs    DECIMAL(18,4) NULL,
    JournalEntryId     INT NULL,
    EntryDate          DATETIME2(0) NULL,
    StaffId            INT NULL,
    ExtraordinaryInvoiceMarker NVARCHAR(255) NULL,
    BalanceAsOfDate    DATE NULL,
    PreviousValueDate  DATE NULL, -- previous value date used for interest calculation
    IsInterestCalculated INT NULL,
    PaymentPurpose     NVARCHAR(255) NULL, -- text printed on the payment slip
    CONSTRAINT FK_InvoiceBatch_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId),
    CONSTRAINT FK_InvoiceBatch_JournalEntry FOREIGN KEY (JournalEntryId) REFERENCES dbo.JournalEntry(JournalEntryId),
    CONSTRAINT FK_InvoiceBatch_Staff FOREIGN KEY (StaffId) REFERENCES dbo.Staff(StaffId)
);

CREATE TABLE dbo.NoticeTemplate ( -- NOT in data-model.md -- added here as a minimal gap-fill because
    -- NoticeBatch.NoticeTemplateId needs somewhere to point; mention it to the boss whenever
    -- convenient in case he already has a different shape in mind (e.g. the old OpomenaSabloni).
    NoticeTemplateId INT NOT NULL CONSTRAINT PK_NoticeTemplate PRIMARY KEY,
    Name             NVARCHAR(255) NULL,
    Body             NVARCHAR(MAX) NULL,
    IsActive         BIT NULL
);

CREATE TABLE dbo.NoticeBatch ( -- formerly "GrupaOpomena" -- standalone dunning run, distinct from a
    -- notice attached directly to an invoice (see NoticeTypeId)
    NoticeBatchId          INT NOT NULL CONSTRAINT PK_NoticeBatch PRIMARY KEY,
    CompanyId              INT NULL,
    Title                  NVARCHAR(50) NULL,
    NoticeDate             DATE NULL, -- formerly "Date"
    MinUnpaidInvoiceCount  INT NULL,
    DebtTolerance          INT NULL,
    DebtToleranceByMonth   INT NULL,
    NoticeTemplateId       INT NULL,
    NoticeTypeId           INT NULL, -- FK -> ShortList: standalone notice vs. notice attached to an invoice
    UpToClaimDate          DATE NULL,
    UpToPaymentDate        DATE NULL,
    InvoiceBatchId         INT NULL,
    CustomCaptionOnSlip    NVARCHAR(255) NULL, -- formerly "CustomCaptionOnOnSlip" (typo fixed)
    CONSTRAINT FK_NoticeBatch_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId),
    CONSTRAINT FK_NoticeBatch_InvoiceBatch FOREIGN KEY (InvoiceBatchId) REFERENCES dbo.InvoiceBatch(InvoiceBatchId),
    CONSTRAINT FK_NoticeBatch_NoticeTemplate FOREIGN KEY (NoticeTemplateId) REFERENCES dbo.NoticeTemplate(NoticeTemplateId)
);

CREATE TABLE dbo.Notice ( -- formerly "Opomena"
    NoticeId           INT NOT NULL CONSTRAINT PK_Notice PRIMARY KEY,
    NoticeBatchId      INT NULL,
    PartnerAccountId   INT NULL,
    UnpaidInvoiceCount INT NULL,
    Debt               DECIMAL(18,2) NULL,
    InvoiceText        NVARCHAR(255) NULL,
    IsActive           INT NULL,
    PaymentReference   NVARCHAR(50) NULL,
    AdditionalCosts    INT NULL, -- formerly "AditionalCosts" (typo fixed) -- e.g. lawyer's fees
    Total              INT NULL, -- AdditionalCosts + Debt
    CONSTRAINT FK_Notice_NoticeBatch FOREIGN KEY (NoticeBatchId) REFERENCES dbo.NoticeBatch(NoticeBatchId),
    CONSTRAINT FK_Notice_PartnerAccount FOREIGN KEY (PartnerAccountId) REFERENCES dbo.PartnerAccount(PartnerAccountId)
);

CREATE TABLE dbo.NoticeLine ( -- formerly "OpomenaStavke"
    NoticeLineId  INT NOT NULL CONSTRAINT PK_NoticeLine PRIMARY KEY,
    NoticeId      INT NULL,
    Parameters    NVARCHAR(25) NULL,
    DocumentRef   NVARCHAR(255) NULL,
    Debit         DECIMAL(19,4) NULL,
    Credit        DECIMAL(19,4) NULL,
    Sum           DECIMAL(19,4) NULL,
    Text          NVARCHAR(255) NULL,
    DueDate       DATE NULL,
    InvoiceId     INT NULL,
    InvoiceDate   DATE NULL,
    UnitAddress   NVARCHAR(255) NULL,
    CONSTRAINT FK_NoticeLine_Notice FOREIGN KEY (NoticeId) REFERENCES dbo.Notice(NoticeId)
    -- FK_NoticeLine_Invoice added via ALTER below (Invoice is defined after Notice)
);

-- =====================================================================
-- 8. INVOICE / INVOICELINE
-- =====================================================================

CREATE TABLE dbo.Invoice ( -- formerly "Racun"
    InvoiceId          INT NOT NULL CONSTRAINT PK_Invoice PRIMARY KEY,
    CompanyId          INT NULL,
    PartnerId          INT NULL, -- taken literally per data-model.md: Partner, not PartnerAccount
    InvoiceBatchId     INT NULL,
    SequenceNumber     NVARCHAR(20) NULL, -- format CompanyId-PartnerAccountId-YYMM, e.g. "101-1234-2609"
    IssueDate          DATE NULL,
    PlaceOfIssue       NVARCHAR(50) NULL, -- default: the company's city
    ServiceDateFrom    DATE NULL, -- copied from InvoiceBatch, but kept per-invoice since it can differ
    ServiceDateTo      DATE NULL,
    TransactionDate    DATE NULL,
    DueDate            DATE NULL,
    -- snapshot of the partner's data at issue time
    PartnerName        NVARCHAR(255) NULL,
    Address            NVARCHAR(255) NULL,
    PostalCode         NVARCHAR(50) NULL,
    City               NVARCHAR(50) NULL,
    Pak                NVARCHAR(50) NULL,
    TaxNumber          NVARCHAR(50) NULL,
    RegistrationNumber NVARCHAR(255) NULL,
    Currency           NVARCHAR(3) NULL,
    Amount             DECIMAL(18,2) NULL, -- base amount before VAT
    VatRate            DECIMAL(18,2) NULL,
    VatAmount          DECIMAL(18,2) NULL,
    Total              DECIMAL(18,2) NULL, -- base + VAT
    InterestAmount     DECIMAL(18,2) NULL, -- late-payment interest
    InvoiceTotal       DECIMAL(18,2) NULL, -- base + VAT + interest
    BalanceAsOfDate    DATE NULL,
    PreviousBalance    DECIMAL(18,2) NULL, -- outstanding debt
    NoticeId           INT NULL,
    PrintNote          NVARCHAR(MAX) NULL, -- note printed on the invoice/notice
    InvoiceDeliveryLocation NVARCHAR(10) NULL, -- text printed in front of units on the invoice
    InvoiceDeliveryUnitId   INT NULL,
    DeliveryLocation   NVARCHAR(255) NULL, -- InvoiceDeliveryLocation + BuildingEntrance.EntranceName + Unit.Name
    InvoiceLayoutId    INT NULL, -- FK -> ShortList (TableName='InvoiceLayout') -- e.g. SzRacun/SzGrupniRacun/SzZakup/UpravnikRacun
    InvoiceLegacyMasterId INT NULL, -- FK -> PartnerAccount, id of the group invoice master record
    InvoiceParentId    INT NULL, -- self FK -- line-item specification of a group invoice (SzGrupniRacun)
    PaymentReference   NVARCHAR(50) NULL,
    Note               NVARCHAR(255) NULL,
    PageCount          INT NULL, -- kept for the e-archive; likely not needed otherwise
    CancelledDate      DATE NULL,
    IsCancelled        BIT NOT NULL,
    SortIndex          INT NULL, -- IMPORTANT: printing must follow entrance order, then unit order
    CONSTRAINT FK_Invoice_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId),
    CONSTRAINT FK_Invoice_Partner FOREIGN KEY (PartnerId) REFERENCES dbo.Partner(PartnerId),
    CONSTRAINT FK_Invoice_InvoiceBatch FOREIGN KEY (InvoiceBatchId) REFERENCES dbo.InvoiceBatch(InvoiceBatchId),
    CONSTRAINT FK_Invoice_Notice FOREIGN KEY (NoticeId) REFERENCES dbo.Notice(NoticeId),
    CONSTRAINT FK_Invoice_InvoiceDeliveryUnit FOREIGN KEY (InvoiceDeliveryUnitId) REFERENCES dbo.Unit(UnitId),
    CONSTRAINT FK_Invoice_LegacyMaster FOREIGN KEY (InvoiceLegacyMasterId) REFERENCES dbo.PartnerAccount(PartnerAccountId),
    CONSTRAINT FK_Invoice_Parent FOREIGN KEY (InvoiceParentId) REFERENCES dbo.Invoice(InvoiceId)
);
ALTER TABLE dbo.NoticeLine ADD CONSTRAINT FK_NoticeLine_Invoice FOREIGN KEY (InvoiceId) REFERENCES dbo.Invoice(InvoiceId);

CREATE TABLE dbo.InvoiceUnit ( -- links an invoice to units for printing, formerly "RacunObjekti"
    InvoiceId  INT NOT NULL,
    ContractId INT NOT NULL,
    CONSTRAINT PK_InvoiceUnit PRIMARY KEY (InvoiceId, ContractId),
    CONSTRAINT FK_InvoiceUnit_Invoice FOREIGN KEY (InvoiceId) REFERENCES dbo.Invoice(InvoiceId),
    CONSTRAINT FK_InvoiceUnit_Contract FOREIGN KEY (ContractId) REFERENCES dbo.Contract(ContractId)
);

CREATE TABLE dbo.InvoiceLine ( -- formerly "RacunStavke"
    InvoiceLineId    INT NOT NULL CONSTRAINT PK_InvoiceLine PRIMARY KEY,
    InvoiceId        INT NULL,
    InvoiceBatchId   INT NULL,
    PartnerId        INT NULL, -- DELIBERATE denormalization, direct link (group invoices are summed
    -- across units and can't be reached via ContractId) -- per data-model.md's own comment
    CompanyId        INT NULL,
    SupplierInvoiceId INT NULL,
    Name             NVARCHAR(255) NULL,
    K1 DECIMAL(18,4) NULL, K2 DECIMAL(18,4) NULL, K3 DECIMAL(18,4) NULL, K4 DECIMAL(18,4) NULL, K5 DECIMAL(18,4) NULL,
    Quantity         DECIMAL(18,2) NULL,
    UnitOfMeasureId  INT NULL, -- FK -> ShortList (TableName='UnitOfMeasure')
    PriceEur         DECIMAL(18,4) NULL,
    ExchangeRateNbs  DECIMAL(18,4) NULL,
    PricePcs         DECIMAL(18,4) NULL, -- RSD unit price
    PriceTotal       DECIMAL(18,2) NULL, -- RSD total amount
    VatRate          DECIMAL(18,2) NULL,
    VatAmount        DECIMAL(18,2) NULL,
    TotalAmount      DECIMAL(18,2) NULL, -- including VAT
    SortIndex        INT NULL,
    CONSTRAINT FK_InvoiceLine_Invoice FOREIGN KEY (InvoiceId) REFERENCES dbo.Invoice(InvoiceId),
    CONSTRAINT FK_InvoiceLine_InvoiceBatch FOREIGN KEY (InvoiceBatchId) REFERENCES dbo.InvoiceBatch(InvoiceBatchId),
    CONSTRAINT FK_InvoiceLine_Partner FOREIGN KEY (PartnerId) REFERENCES dbo.Partner(PartnerId),
    CONSTRAINT FK_InvoiceLine_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId)
    -- FK_InvoiceLine_SupplierInvoice added via ALTER below (SupplierInvoice is defined later)
);

-- =====================================================================
-- 9. SUPPLIERS
-- =====================================================================

CREATE TABLE dbo.SupplierInvoice ( -- formerly "Dobavljac_Racuni"
    SupplierInvoiceId  INT NOT NULL CONSTRAINT PK_SupplierInvoice PRIMARY KEY,
    CompanyId          INT NULL,
    InvoiceNo          INT NULL, -- sequence number within PeriodYyMm, used as sort order on the invoice
    CodeName           NVARCHAR(255) NULL, -- mandatory sequence code for DocumentTypeId=2
    Caption            NVARCHAR(255) NULL, -- printed on the invoice
    SupplierPartnerAccountId INT NULL, -- FK -> PartnerAccount
    CalculationTypeId  INT NULL,
    PeriodYyMm         INT NULL, -- e.g. 2604
    InvoiceTotalCalculationAmountEur DECIMAL(18,4) NULL,
    InvoiceTotalCalculationAmountRsd DECIMAL(18,4) NULL,
    CalculationAmountByCoefficientEur DECIMAL(18,4) NULL,
    CalculationAmountByCoefficientRsd DECIMAL(18,4) NULL,
    PaymentPriority    INT NULL,
    SubAccountId       NVARCHAR(10) NULL,
    DocumentTypeId     INT NULL, -- FK -> ShortList (TableName='SupplierDocumentType') -- 1=planned cost, 2=incurred cost, ...
    ExtraordinaryInvoiceMarker NVARCHAR(10) NULL, -- "v01", "o01" etc.
    InvoiceNameFunction NVARCHAR(255) NULL, -- e.g. "Investiciono odrzavanje za #YYMM#"
    PostedInvoiceAmount DECIMAL(18,2) NULL, -- populated after posting/invoice generation
    InvoiceDate        DATE NULL,
    TransactionDate    DATE NULL, -- same as PostingDate
    PaymentDate        DATE NULL,
    InvoiceDescription NVARCHAR(255) NULL,
    PaymentReference   NVARCHAR(255) NULL,
    PreviousSupplierInvoiceId INT NULL,
    NewSupplierInvoiceId INT NULL,
    JournalEntryId     INT NULL,
    ClosesAccount      NVARCHAR(10) NULL, -- special closing scheme for DocumentTypeId=9
    CONSTRAINT FK_SupplierInvoice_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId),
    CONSTRAINT FK_SupplierInvoice_PartnerAccount FOREIGN KEY (SupplierPartnerAccountId) REFERENCES dbo.PartnerAccount(PartnerAccountId),
    CONSTRAINT FK_SupplierInvoice_CalculationType FOREIGN KEY (CalculationTypeId) REFERENCES dbo.CalculationType(CalculationTypeId),
    CONSTRAINT FK_SupplierInvoice_SubAccount FOREIGN KEY (SubAccountId) REFERENCES dbo.SubAccount(SubAccountId),
    CONSTRAINT FK_SupplierInvoice_JournalEntry FOREIGN KEY (JournalEntryId) REFERENCES dbo.JournalEntry(JournalEntryId),
    CONSTRAINT FK_SupplierInvoice_Previous FOREIGN KEY (PreviousSupplierInvoiceId) REFERENCES dbo.SupplierInvoice(SupplierInvoiceId),
    CONSTRAINT FK_SupplierInvoice_New FOREIGN KEY (NewSupplierInvoiceId) REFERENCES dbo.SupplierInvoice(SupplierInvoiceId)
);
ALTER TABLE dbo.InvoiceLine ADD CONSTRAINT FK_InvoiceLine_SupplierInvoice
    FOREIGN KEY (SupplierInvoiceId) REFERENCES dbo.SupplierInvoice(SupplierInvoiceId);

CREATE TABLE dbo.SupplierInvoiceUnitType ( -- which unit types (apartment/business space/...) a supplier
    -- invoice applies to, formerly "Dobavljaci_Racun_TipObjekta"
    SupplierInvoiceUnitTypeId INT NOT NULL CONSTRAINT PK_SupplierInvoiceUnitType PRIMARY KEY,
    SupplierInvoiceId  INT NULL,
    UnitTypeId         INT NULL, -- FK -> ShortList (TableName='UnitType')
    CONSTRAINT FK_SupplierInvoiceUnitType_SupplierInvoice FOREIGN KEY (SupplierInvoiceId) REFERENCES dbo.SupplierInvoice(SupplierInvoiceId)
);

-- =====================================================================
-- 10. LEDGERENTRY -- general ledger
-- =====================================================================

CREATE TABLE dbo.LedgerEntry ( -- formerly "GK" (Glavna Knjiga)
    LedgerEntryId       INT NOT NULL CONSTRAINT PK_LedgerEntry PRIMARY KEY,
    JournalEntryId      INT NULL,
    Account             NVARCHAR(10) NULL, -- FK -> ChartOfAccounts
    PostingDate         DATE NULL, -- transaction/posting date
    DueDate             DATE NULL,
    DebitAmount         DECIMAL(19,4) NULL,
    CreditAmount        DECIMAL(19,4) NULL,
    LineTypeId          INT NULL, -- typed "Double" in data-model.md (likely a typo there), taken as INT -- FK -> ShortList (TableName='LedgerLineType')
    DocumentRef         NVARCHAR(50) NULL,
    CompanyId           INT NULL,
    PartnerAccountId    INT NULL,
    BankStatementLineId INT NULL,
    Note                NVARCHAR(255) NULL, -- used for report-specific comments on specific transactions
    Parameters          NVARCHAR(25) NULL, -- terse reference to a payment number on the invoice
    Description         NVARCHAR(255) NULL,
    SubAccountId        NVARCHAR(10) NULL,
    SupplierInvoiceId   INT NULL,
    InvoiceId           INT NULL,
    Priority            INT NULL,
    CONSTRAINT FK_LedgerEntry_JournalEntry FOREIGN KEY (JournalEntryId) REFERENCES dbo.JournalEntry(JournalEntryId),
    CONSTRAINT FK_LedgerEntry_ChartOfAccounts FOREIGN KEY (Account) REFERENCES dbo.ChartOfAccounts(Account),
    CONSTRAINT FK_LedgerEntry_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId),
    CONSTRAINT FK_LedgerEntry_PartnerAccount FOREIGN KEY (PartnerAccountId) REFERENCES dbo.PartnerAccount(PartnerAccountId),
    CONSTRAINT FK_LedgerEntry_SubAccount FOREIGN KEY (SubAccountId) REFERENCES dbo.SubAccount(SubAccountId),
    CONSTRAINT FK_LedgerEntry_SupplierInvoice FOREIGN KEY (SupplierInvoiceId) REFERENCES dbo.SupplierInvoice(SupplierInvoiceId),
    CONSTRAINT FK_LedgerEntry_Invoice FOREIGN KEY (InvoiceId) REFERENCES dbo.Invoice(InvoiceId)
    -- FK_LedgerEntry_BankStatementLine added via ALTER below (BankStatementLine is defined later)
);
GO

-- =====================================================================
-- POSTING -- balance is only checked when a JournalEntry is posted, not on every LedgerEntry write.
-- Unchanged in substance from v3 -- see docs/tehnicki-plan-faza1.md section 2.4 for the full rationale
-- (client-confirmed: "trigger ne sme da radi prilikom unosa stavki, kontrola ravnoteze se tek radi
-- kada se nalog pokusa proknjiziti").
-- =====================================================================

CREATE PROCEDURE dbo.sp_PostJournalEntry
    @JournalEntryId INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    DECLARE @Diff DECIMAL(19,4);
    DECLARE @AlreadyPosted BIT;

    SELECT @AlreadyPosted = IsPosted
    FROM dbo.JournalEntry WITH (UPDLOCK, HOLDLOCK)
    WHERE JournalEntryId = @JournalEntryId;

    IF @AlreadyPosted IS NULL
    BEGIN
        ROLLBACK TRANSACTION;
        THROW 50002, N'Journal entry does not exist.', 1;
        RETURN;
    END

    IF @AlreadyPosted = 1
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

    UPDATE dbo.JournalEntry SET IsPosted = 1, PostedDate = SYSDATETIME() WHERE JournalEntryId = @JournalEntryId;

    COMMIT TRANSACTION;
END;
GO

CREATE TRIGGER dbo.TR_LedgerEntry_BlockEditOfPostedJournalEntry ON dbo.LedgerEntry
AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (
        SELECT 1
        FROM deleted d
        JOIN dbo.JournalEntry je ON je.JournalEntryId = d.JournalEntryId
        WHERE je.IsPosted = 1
    )
        THROW 50004, N'Cannot change/delete LedgerEntry rows of a posted journal entry -- use a reversal.', 1;
END;
GO

-- =====================================================================
-- 11. BANK STATEMENTS
-- =====================================================================

CREATE TABLE dbo.BankStatement ( -- formerly "Izvod"
    BankStatementId  INT NOT NULL CONSTRAINT PK_BankStatement PRIMARY KEY,
    BankAccountId    INT NULL, -- also identifies which Company this statement belongs to, via BankAccount
    StatementNumber  INT NULL,
    StatementSuffix  NVARCHAR(50) NULL,
    StatementDate    DATE NULL, -- formerly "Date"
    PreviousBalance  DECIMAL(19,4) NULL,
    NewBalance       DECIMAL(19,4) NULL,
    Debit            DECIMAL(19,4) NULL,
    Credit           DECIMAL(19,4) NULL,
    CountDebitEntry  INT NULL,
    CountCreditEntry INT NULL,
    Note             NVARCHAR(50) NULL,
    JournalEntryId   INT NULL,
    IsPosted         BIT NOT NULL,
    CONSTRAINT FK_BankStatement_BankAccount FOREIGN KEY (BankAccountId) REFERENCES dbo.BankAccount(BankAccountId),
    CONSTRAINT FK_BankStatement_JournalEntry FOREIGN KEY (JournalEntryId) REFERENCES dbo.JournalEntry(JournalEntryId)
);

CREATE TABLE dbo.BankStatementLine ( -- formerly "IzvodStavke"
    BankStatementLineId INT NOT NULL CONSTRAINT PK_BankStatementLine PRIMARY KEY,
    BankStatementId     INT NULL,
    LineNumber          INT NULL,
    PayerRecipientName  NVARCHAR(255) NULL,
    BankAccountNumber   NVARCHAR(50) NULL,
    Debit               DECIMAL(19,4) NULL,
    Credit              DECIMAL(19,4) NULL,
    Info                NVARCHAR(255) NULL,
    Code                INT NULL,
    PaymentReference    NVARCHAR(50) NULL,
    PaymentReferenceOut NVARCHAR(50) NULL,
    PartnerAccountId    INT NULL,
    SubAccountId        NVARCHAR(10) NULL,
    IsPosted            BIT NOT NULL,
    IsIgnored           BIT NOT NULL,
    IsMatched           BIT NOT NULL,
    BankRef             NVARCHAR(50) NULL, -- the bank's own reference for this line
    CONSTRAINT FK_BankStatementLine_BankStatement FOREIGN KEY (BankStatementId) REFERENCES dbo.BankStatement(BankStatementId),
    CONSTRAINT FK_BankStatementLine_PartnerAccount FOREIGN KEY (PartnerAccountId) REFERENCES dbo.PartnerAccount(PartnerAccountId),
    CONSTRAINT FK_BankStatementLine_SubAccount FOREIGN KEY (SubAccountId) REFERENCES dbo.SubAccount(SubAccountId)
);
ALTER TABLE dbo.LedgerEntry ADD CONSTRAINT FK_LedgerEntry_BankStatementLine
    FOREIGN KEY (BankStatementLineId) REFERENCES dbo.BankStatementLine(BankStatementLineId);

CREATE TABLE dbo.BankInFlow ( -- incoming-payment tracking, distinct from BankStatementLine -- records
    -- that a payment was realized before/independent of the matching statement line landing
    BankInFlowId       INT NOT NULL CONSTRAINT PK_BankInFlow PRIMARY KEY,
    BankAccountId      INT NULL,
    DateInFlow         DATE NULL,
    ReferenceNumber    NVARCHAR(255) NULL,
    Currency           NVARCHAR(3) NULL,
    OriginalAmount     DECIMAL(19,4) NULL,
    AmountLocalCurrency DECIMAL(19,4) NULL,
    PartnerAccountId   INT NULL,
    InvoiceDescription NVARCHAR(50) NULL, -- text for the manager's NBS statistics report, e.g. "Po racunu 251-1234-2609"
    BankStatementLineId INT NULL, -- set once matched to the statement line where the money actually landed
    SentToManager      DATETIME2(0) NULL,
    CONSTRAINT FK_BankInFlow_BankAccount FOREIGN KEY (BankAccountId) REFERENCES dbo.BankAccount(BankAccountId),
    CONSTRAINT FK_BankInFlow_PartnerAccount FOREIGN KEY (PartnerAccountId) REFERENCES dbo.PartnerAccount(PartnerAccountId),
    CONSTRAINT FK_BankInFlow_BankStatementLine FOREIGN KEY (BankStatementLineId) REFERENCES dbo.BankStatementLine(BankStatementLineId)
);

CREATE TABLE dbo.BankStatementPostingTemplate ( -- formerly "TemplateIzvodaKnjizenje"
    BankStatementPostingTemplateId INT NOT NULL CONSTRAINT PK_BankStatementPostingTemplate PRIMARY KEY,
    CompanyId          INT NULL, -- NULL = applies to all companies
    ParentId           INT NULL,
    TemplateName       NVARCHAR(255) NULL,
    FieldName          NVARCHAR(255) NULL,
    FieldValue         NVARCHAR(255) NULL,
    Function           NVARCHAR(255) NULL,
    SetPartnerAccountId INT NULL,
    SetSubAccountId    NVARCHAR(10) NULL,
    SetAccountCode     NVARCHAR(10) NULL,
    SortIndex          INT NULL,
    IsActive           BIT NULL,
    CONSTRAINT FK_BankStatementPostingTemplate_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId),
    CONSTRAINT FK_BankStatementPostingTemplate_Parent FOREIGN KEY (ParentId) REFERENCES dbo.BankStatementPostingTemplate(BankStatementPostingTemplateId),
    CONSTRAINT FK_BankStatementPostingTemplate_PartnerAccount FOREIGN KEY (SetPartnerAccountId) REFERENCES dbo.PartnerAccount(PartnerAccountId),
    CONSTRAINT FK_BankStatementPostingTemplate_SubAccount FOREIGN KEY (SetSubAccountId) REFERENCES dbo.SubAccount(SubAccountId),
    CONSTRAINT FK_BankStatementPostingTemplate_ChartOfAccounts FOREIGN KEY (SetAccountCode) REFERENCES dbo.ChartOfAccounts(Account)
);

-- =====================================================================
-- 12. BENEFITS / INTEREST
-- =====================================================================

CREATE TABLE dbo.Benefit ( -- generated then cancelled invoice, with the discounted amount tracked here
    BenefitId    INT NOT NULL CONSTRAINT PK_Benefit PRIMARY KEY,
    ContractId   INT NULL, -- still FK -> Contract (carries both UnitId and PartnerId)
    PeriodYyMm   INT NULL,
    EntryDate    DATE NULL, -- entry date; a row is entered per period covered, e.g. one row per month 2602..2607
    InvoiceId    INT NULL,
    CONSTRAINT FK_Benefit_Contract FOREIGN KEY (ContractId) REFERENCES dbo.Contract(ContractId),
    CONSTRAINT FK_Benefit_Invoice FOREIGN KEY (InvoiceId) REFERENCES dbo.Invoice(InvoiceId)
);

CREATE TABLE dbo.InterestRate ( -- formerly "Stope"
    InterestRateId INT NOT NULL CONSTRAINT PK_InterestRate PRIMARY KEY,
    RateDate       DATE NULL,
    Rate           DECIMAL(18,4) NOT NULL,
    TimeCode       NVARCHAR(1) NULL -- "M" (monthly) / "G" (yearly)
);

CREATE TABLE dbo.InterestStatement ( -- formerly "KamatniList"
    InterestStatementId INT NOT NULL CONSTRAINT PK_InterestStatement PRIMARY KEY,
    CompanyId       INT NULL,
    Account         NVARCHAR(50) NULL,
    StatementDate   DATE NULL, -- formerly "Date"
    Amount          DECIMAL(18,2) NULL,
    Balance         DECIMAL(18,2) NULL,
    Days            INT NULL,
    Rate            DECIMAL(18,4) NULL, -- from InterestRate.Rate
    Coefficient     DECIMAL(18,8) NULL, -- for yearly rates: (days / days-in-year) * Rate / 100
    Interest        DECIMAL(18,2) NULL,
    PartnerAccountId INT NULL,
    SubAccountId    NVARCHAR(10) NULL,
    InvoiceBatchId  INT NULL,
    CONSTRAINT FK_InterestStatement_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId),
    CONSTRAINT FK_InterestStatement_PartnerAccount FOREIGN KEY (PartnerAccountId) REFERENCES dbo.PartnerAccount(PartnerAccountId),
    CONSTRAINT FK_InterestStatement_SubAccount FOREIGN KEY (SubAccountId) REFERENCES dbo.SubAccount(SubAccountId),
    CONSTRAINT FK_InterestStatement_InvoiceBatch FOREIGN KEY (InvoiceBatchId) REFERENCES dbo.InvoiceBatch(InvoiceBatchId)
);
-- NOTE: no "ZK"/PenaltyInterestStaging-equivalent staging table in data-model.md -- confirmed dropped
-- on purpose (data-model.md's own comment: "POTPUNO ISTA TABELA KAO GK LEDGER" -- the staging step
-- is just LedgerEntry rows on an unposted JournalEntry, no separate table needed).

-- =====================================================================
-- 13. PAYMENT ORDERS / MAIL / DOCUMENTS / SELECTIONBASKET
-- =====================================================================

CREATE TABLE dbo.PaymentOrder ( -- formerly "Virman" -- payment slip/QR/order for settling a supplier invoice
    PaymentOrderId   INT NOT NULL CONSTRAINT PK_PaymentOrder PRIMARY KEY,
    TemplateTitle    NVARCHAR(50) NULL,
    PayerName        NVARCHAR(255) NULL,
    PaymentPurpose   NVARCHAR(255) NULL,
    RecipientName    NVARCHAR(255) NULL,
    PaymentCode      INT NULL,
    Currency         NVARCHAR(3) NULL,
    Amount           DECIMAL(18,2) NULL,
    PayerAccountNumber NVARCHAR(50) NULL,
    PayerModelNumber INT NULL,
    PayerPaymentReference NVARCHAR(50) NULL,
    RecipientAccountNumber NVARCHAR(50) NULL,
    RecipientModelNumber INT NULL,
    RecipientPaymentReference NVARCHAR(50) NULL,
    Place            NVARCHAR(50) NULL,
    OrderDate        DATE NULL, -- formerly "Date"
    ValueDate        DATE NULL,
    IsUrgent         BIT NOT NULL,
    PaymentOrderTypeId INT NULL, -- FK -> ShortList (TableName='PaymentOrderType')
    CreatedTimestamp DATETIME2(0) NULL,
    IsFavorite       BIT NOT NULL,
    IsArchived       BIT NOT NULL
);

CREATE TABLE dbo.SentEmail ( -- formerly "Mail_Send"
    SentEmailId      INT NOT NULL CONSTRAINT PK_SentEmail PRIMARY KEY,
    Subject          NVARCHAR(255) NULL,
    ToAddress        NVARCHAR(255) NULL, -- formerly "To" (reserved word)
    Cc               NVARCHAR(255) NULL,
    Bcc              NVARCHAR(255) NULL,
    BodyHtml         NVARCHAR(MAX) NULL,
    Created          DATETIME2(0) NULL,
    Sent             DATETIME2(0) NULL,
    Archive          BIT NULL,
    SendStatusId     NVARCHAR(255) NULL, -- FK -> ShortList (TableName='SendEmailStatus')
    SendDescription  NVARCHAR(MAX) NULL
);

CREATE TABLE dbo.SentEmailAttachment ( -- formerly "Mail_Send_Attachment"
    SentEmailAttachmentId INT NOT NULL CONSTRAINT PK_SentEmailAttachment PRIMARY KEY,
    SentEmailId  INT NULL,
    FilePath     NVARCHAR(255) NULL,
    CONSTRAINT FK_SentEmailAttachment_SentEmail FOREIGN KEY (SentEmailId) REFERENCES dbo.SentEmail(SentEmailId)
);

CREATE TABLE dbo.Documents ( -- formerly "Files" -- e-archive; polymorphic reference by design
    DocumentId       INT NOT NULL CONSTRAINT PK_Documents PRIMARY KEY,
    DocumentTypeId   INT NULL, -- FK -> ShortList (TableName='DocumentType')
    SourceTable      NVARCHAR(255) NULL,
    ReferenceId      INT NULL,
    FileName         NVARCHAR(255) NULL,
    RelativePath     NVARCHAR(255) NULL, -- B-3: where files physically live is still not decided
    DocumentDate     DATE NULL, -- formerly "Date"
    Description      NVARCHAR(255) NULL,
    FileNameSuffix   NVARCHAR(10) NULL,
    FileExtension    NVARCHAR(10) NULL,
    Registrar        NVARCHAR(4) NULL,
    RegistrarLocation NVARCHAR(255) NULL,
    IsEDocument      BIT NULL,
    CategoryId       INT NULL,
    CreatedAt        DATETIME2(0) NULL, -- formerly "TimeStamp"
    CONSTRAINT FK_Documents_DocumentCategory FOREIGN KEY (CategoryId) REFERENCES dbo.DocumentCategory(DocumentCategoryId)
);

CREATE TABLE dbo.SelectionBasket ( -- "corpa" of ids collected before a batch operation (e.g. all
    -- invoices for a group PDF, mass email) -- boss confirmed this is worth keeping
    SelectionBasketId INT NOT NULL CONSTRAINT PK_SelectionBasket PRIMARY KEY,
    TargetId          INT NULL,
    TypeIndex         INT NULL -- which kind of target this is (invoice/unit/partner/...)
);

-- =====================================================================
-- 14. EVENTS (workflow, formerly "Promene")
-- =====================================================================

CREATE TABLE dbo.Events (
    EventId          INT NOT NULL CONSTRAINT PK_Events PRIMARY KEY,
    DateOfRequest    DATETIME2(0) NULL,
    DateOfExecution  DATETIME2(0) NULL,
    ContractId       INT NULL,
    PartnerId        INT NULL,
    UnitId           INT NULL,
    Description      NVARCHAR(255) NULL,
    PreviousValue    NVARCHAR(255) NULL,
    NewValue         NVARCHAR(255) NULL,
    FieldsRelated    NVARCHAR(255) NULL,
    RequestTypeId    INT NULL, -- FK -> ShortList (TableName='EventRequestType')
    RequestBy        NVARCHAR(255) NULL, -- who requested it -- email/phone/...
    RequestThrough   NVARCHAR(255) NULL, -- channel it came through, e.g. manager's email
    CONSTRAINT FK_Events_Contract FOREIGN KEY (ContractId) REFERENCES dbo.Contract(ContractId),
    CONSTRAINT FK_Events_Partner FOREIGN KEY (PartnerId) REFERENCES dbo.Partner(PartnerId),
    CONSTRAINT FK_Events_Unit FOREIGN KEY (UnitId) REFERENCES dbo.Unit(UnitId)
);
-- NOTE: still no StatusId/ApprovedByStaffId/ApprovedDate columns for the approval workflow (B-1 in
-- preostala-pitanja.md) -- that question is still open, data-model.md doesn't answer it.

-- =====================================================================
-- 15. SETTINGS / AUDIT
-- =====================================================================

CREATE TABLE dbo.Setting (
    SettingId    INT NOT NULL CONSTRAINT PK_Setting PRIMARY KEY,
    CompanyId    INT NULL,
    Name         NVARCHAR(255) NULL,
    SettingKey   NVARCHAR(50) NULL, -- formerly "Key" (reserved word)
    Value        NVARCHAR(255) NULL,
    Description  NVARCHAR(255) NULL,
    Category     NVARCHAR(50) NULL,
    ValueMax     NVARCHAR(MAX) NULL,
    CONSTRAINT FK_Setting_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId)
);
-- NOTE: no separate EmailSetting/"Settings_eMail" table in data-model.md -- mail account credentials
-- are meant to live in this generic Setting table. SECURITY (unconditional, regardless of that):
-- Value/ValueMax must not hold plain-text passwords/tokens in production -- user-secrets/Key Vault or
-- an encrypted column, per CLAUDE.md.

CREATE TABLE dbo.AuditLog ( -- formerly "_Log"
    AuditLogId    INT NOT NULL CONSTRAINT PK_AuditLog PRIMARY KEY,
    StaffId       INT NULL,
    EventTimeStamp DATETIME2(0) NULL, -- formerly "TimeStamp"
    ItemId        INT NULL,
    ActionTypeId  INT NULL, -- FK -> ShortList (TableName='AuditActionType') -- Create/Read/Update/Delete
    EventAction   NVARCHAR(255) NULL, -- message
    EventById     NVARCHAR(50) NULL, -- FK -> ShortList (TableName='AuditEventType') -- User/System/Cron
    CONSTRAINT FK_AuditLog_Staff FOREIGN KEY (StaffId) REFERENCES dbo.Staff(StaffId)
);
-- NOTE: the app-level role that writes to this table must not have UPDATE/DELETE (insert-only) -- same
-- rule as v3, data-model.md doesn't restate it but nothing has changed the reasoning.

-- =====================================================================
-- 16. IMPORT PIPELINE (new subsystem in v4 -- generic file-import mapping, not present in v3)
-- =====================================================================

CREATE TABLE dbo.ImportDefinition ( -- formerly written "ImportDefintion" in data-model.md (typo fixed here)
    ImportDefinitionId INT NOT NULL CONSTRAINT PK_ImportDefinition PRIMARY KEY,
    Name               NVARCHAR(255) NULL,
    Code               NVARCHAR(50) NULL, -- e.g. "OFFICE_EMAIL_XML"
    FileMask           NVARCHAR(50) NULL, -- e.g. "*.xml"
    ImportSourceId     INT NULL, -- FK -> ShortList (TableName='ImportSource') -- email/gdrive/folder/drag&drop
    FilePath           NVARCHAR(255) NULL,
    TargetHeaderTable  NVARCHAR(255) NULL,
    TargetLineTable    NVARCHAR(255) NULL,
    IsActive           BIT NULL,
    SortIndex          INT NOT NULL
);

CREATE TABLE dbo.ImportMappingGroup (
    ImportMappingGroupId INT NOT NULL CONSTRAINT PK_ImportMappingGroup PRIMARY KEY,
    ImportDefinitionId   INT NOT NULL,
    Name                 NVARCHAR(255) NULL,
    TargetTable          NVARCHAR(255) NULL,
    SourcePath           NVARCHAR(255) NULL,
    IsRepeating          BIT NULL,
    CONSTRAINT FK_ImportMappingGroup_ImportDefinition FOREIGN KEY (ImportDefinitionId) REFERENCES dbo.ImportDefinition(ImportDefinitionId)
);

CREATE TABLE dbo.ImportMapping (
    ImportMappingId      INT NOT NULL CONSTRAINT PK_ImportMapping PRIMARY KEY,
    ImportMappingGroupId INT NULL,
    TargetField          NVARCHAR(255) NULL,
    SourcePath            NVARCHAR(255) NULL,
    SourceNode            NVARCHAR(255) NULL,
    MappingTypeId         INT NULL, -- FK -> ShortList (TableName='MappingType') -- Identity/Header/Line
    DataTypeId            INT NULL, -- FK -> ShortList (TableName='DataType')
    DefaultValue          NVARCHAR(255) NULL,
    IsRequired            BIT NULL,
    IsKey                 BIT NULL, -- untyped in data-model.md, assumed BIT to match IsRequired/IsLookup
    -- lookup support, e.g. resolve a bank account number to BankAccount.Id
    IsLookup              BIT NULL,
    LookupTable           NVARCHAR(255) NULL,
    LookupField           NVARCHAR(255) NULL,
    LookupValueField      NVARCHAR(255) NULL,
    Format                NVARCHAR(50) NULL,
    SortIndex             INT NOT NULL,
    Description            NVARCHAR(255) NULL,
    CONSTRAINT FK_ImportMapping_ImportMappingGroup FOREIGN KEY (ImportMappingGroupId) REFERENCES dbo.ImportMappingGroup(ImportMappingGroupId)
);

-- =====================================================================
-- 17. POSTING SCHEME (mechanical GL posting rules, not used yet -- kept as-is)
-- =====================================================================

CREATE TABLE dbo.PostingScheme ( -- formerly "SemaKnjizenja"
    PostingSchemeId  INT NOT NULL CONSTRAINT PK_PostingScheme PRIMARY KEY,
    Name             NVARCHAR(255) NULL,
    SourceTable      NVARCHAR(255) NULL,
    SourceTableWhereField NVARCHAR(255) NULL,
    Line             NVARCHAR(255) NULL,
    Account          NVARCHAR(255) NULL,
    BaseAmount       NVARCHAR(255) NULL,
    Sign             INT NULL,
    SubAnalyticField NVARCHAR(255) NULL,
    SubAnalyticFieldSource NVARCHAR(255) NULL,
    Description      NVARCHAR(255) NULL,
    LedgerLineTypeId INT NULL,
    LedgerDocumentFunction NVARCHAR(255) NULL,
    LedgerPaymentReference NVARCHAR(255) NULL,
    LedgerPartnerId  NVARCHAR(255) NULL,
    LedgerInvoiceId  NVARCHAR(255) NULL,
    SortIndex        INT NULL,
    JournalEntryDescription NVARCHAR(255) NULL,
    SourceSql        NVARCHAR(MAX) NULL,
    SourceSqlValue   NVARCHAR(255) NULL
);

-- =====================================================================
-- 18. DYNAMIC REPORTING SYSTEM (B-5) -- now has a concrete shape in data-model.md; the SAFE-EXECUTION
-- design (SourceSql/QuerySql hold raw SQL that gets run against the database) is still NOT solved just
-- by having these columns -- see the top-of-file note and docs/preostala-pitanja.md section D.
-- =====================================================================

CREATE TABLE dbo.AnalysisReportDefinition ( -- formerly "tblAnaliza" -- actively used daily
    AnalysisReportDefinitionId INT NOT NULL CONSTRAINT PK_AnalysisReportDefinition PRIMARY KEY,
    DataGroup         NVARCHAR(255) NULL,
    Name              NVARCHAR(255) NULL,
    IsCriticalGroup   BIT NULL,
    QueryName         NVARCHAR(255) NULL,
    QuerySql          NVARCHAR(MAX) NULL,
    ReportName        NVARCHAR(50) NULL,
    StatWhereCaption  NVARCHAR(50) NULL,
    StatWhereComboSql NVARCHAR(MAX) NULL,
    StatSqlName       NVARCHAR(50) NULL,
    ExecuteQueryDefName NVARCHAR(255) NULL, -- executed before the main query runs
    ExecuteQueryDefSql  NVARCHAR(MAX) NULL,
    AutoRunAll        INT NULL,
    SortIndex         INT NULL,
    LinkCreationTag   NVARCHAR(255) NULL, -- used to open/show the underlying data
    LinkFormName      NVARCHAR(255) NULL,
    LinkOpenArgs      NVARCHAR(255) NULL,
    LastRunAt         DATETIME2(0) NULL,
    RowCountLast      INT NULL, -- formerly "RowCount" (reserved word)
    AdminAlert        BIT NULL, -- send an email to root as a critical alert
    Description       NVARCHAR(MAX) NULL,
    IsActive          BIT NULL
);

CREATE TABLE dbo.ReportDefinition ( -- formerly "tblIzvestaj"
    ReportDefinitionId INT NOT NULL CONSTRAINT PK_ReportDefinition PRIMARY KEY,
    Name             NVARCHAR(255) NULL,
    Datasheet        NVARCHAR(255) NULL,
    Title            NVARCHAR(255) NULL,
    SortIndex        INT NULL,
    FilterComboSql   NVARCHAR(MAX) NULL,
    FilterComboCaption NVARCHAR(255) NULL
);

CREATE TABLE dbo.ReportDefinitionDetail (
    ReportDefinitionDetailId INT NOT NULL CONSTRAINT PK_ReportDefinitionDetail PRIMARY KEY,
    ReportDefinitionId INT NULL,
    QuerySql         NVARCHAR(MAX) NULL,
    QueryName        NVARCHAR(255) NULL,
    SortIndex        INT NULL,
    Function         NVARCHAR(255) NULL,
    CONSTRAINT FK_ReportDefinitionDetail_ReportDefinition FOREIGN KEY (ReportDefinitionId) REFERENCES dbo.ReportDefinition(ReportDefinitionId)
);

CREATE TABLE dbo.ReportDefinitionButtons (
    ReportDefinitionButtonsId INT NOT NULL CONSTRAINT PK_ReportDefinitionButtons PRIMARY KEY,
    ReportDefinitionId INT NULL,
    Caption          NVARCHAR(255) NULL,
    Function         NVARCHAR(255) NULL,
    FunctionTypeId   INT NULL, -- FK -> ShortList (TableName='ReportFunctionType')
    SortIndex        INT NULL,
    CONSTRAINT FK_ReportDefinitionButtons_ReportDefinition FOREIGN KEY (ReportDefinitionId) REFERENCES dbo.ReportDefinition(ReportDefinitionId)
);

-- =====================================================================
-- 19. NOT YET MIGRATED FROM data-model.md -- transcribed as-is, no known use in the new app yet
-- =====================================================================

CREATE TABLE dbo.FiscalYear ( -- data-model.md: "necemo za sad koristiti ali neka ostane" (not used for
    -- now, but keep it) -- kept per the boss's own comment, not wired to anything else here
    FiscalYearId  INT NOT NULL CONSTRAINT PK_FiscalYear PRIMARY KEY,
    CompanyId     INT NOT NULL,
    Year          INT NOT NULL,
    StartDate     DATE NULL,
    EndDate       DATE NULL,
    IsArchived    BIT NULL,
    Display       NVARCHAR(255) NULL,
    Folder        NVARCHAR(255) NULL,
    FileName      NVARCHAR(255) NULL,
    IsCurrent     BIT NULL,
    CONSTRAINT FK_FiscalYear_Company FOREIGN KEY (CompanyId) REFERENCES dbo.Company(CompanyId)
);

-- =====================================================================
-- END OF v4 DRAFT
-- =====================================================================
-- Deliberately OMITTED, unchanged from v3 (still true, data-model.md doesn't touch these):
--   - InboundInvoice (formerly "RacunIN"): removed, confirmed abandoned.
--   - Indexes, UNIQUE (besides the one on PartnerAccount) and CHECK constraints (B-4): deliberately
--     deferred until real data is profiled in Phase 4.
--
-- docs/data-model.md is accepted as final; docs/preostala-pitanja.md section D has a short list of
-- implementation-level suggestions worth a look, not open business questions.
