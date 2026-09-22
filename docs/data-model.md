# Database schema — extracted directly from the .mdb files (mdbtools, JET4)

Table and column names below have been translated from the original Serbian/legacy Access names to
English, using the same naming convention as `schema-ddl-draft.sql` (see that file's header for the
full old-name -> new-name rationale). The structure, column order and column types are otherwise an
exact, unmodified copy of the legacy database — this file still documents the real, physical shape of
the old Access files, including framework/temp/backup tables that will not carry over to the new
schema; it is not itself the new schema design.

No Access Relationships or indexes are defined in any of the three files — verified directly at the
binary level (mdb-schema --relations --indexes), not just in the export. This is the actual state of
the database, not a gap in the export.

## data.mdb — 73 tables (real data)
```sql
-- ----------------------------------------------------------
-- MDB Tools - A library for reading MS Access database files
-- Copyright (C) 2000-2011 Brian Bruns and others.
-- Files in libmdb are licensed under LGPL and the utilities under
-- the GPL, see COPYING.LIB and COPYING files respectively.
-- Check out http://mdbtools.sourceforge.net
-- ----------------------------------------------------------

-- That file uses encoding UTF-8

-- ovo je bilo bitno na racunarima sad i nema smisla, widows user name, comp name ... 
-- ActionType - User, System cron,...
-- EventAction: message
-- ActionTypeId CRUD  / ShortList.TableName: AuditActionType
-- EventById CRUD  / ShortList.TableName: AuditEventType


CREATE TABLE [AuditLog]
 (
	[Id]				Long Integer, 
	[StaffId]			Long Integer, 
	[TimeStamp]			DateTime, 
	[ItemId]			Long Integer, 
	[ActionTypeId]		Long Integer,   -- Create, Read, Update, Delete...
	[EventAction]		Text (255), 
	[EventById]			Text (50) 		-- User, System, Cron...
);

-- Translations.LanguageCode FK - Languages.Code PK 
-- ResourceKey npr: Common.Save ResourceId:null or 0 ==> Translation: Snimi
-- ResourceId npr:  ResourceKey: Months  ResourceId:1 ==> Translation: Januar
-- Za spicificne unos prevoda koji se menjaju po Company koristiti CompanyId - 0 je def, 
-- ResourceKey npr: Common.Save ResourceId:0 CompanyId:0 ==> Translation:  Snimi  / def za sve
-- ResourceKey npr: Common.Save ResourceId:0 CompanyId:101 ==> Translation:  Snimaj  / samo 101 ima poseban proveod

CREATE TABLE [Translations]
 (
	[Id]				Long Integer NOT NULL,
	[LanguageCode]		Text (10) NOT NULL, 
	[ResourceKey]		text(100),
	[ResourceId]		Long Integer,
	[CompanyId]			Long Integer,
	[Translation]		Text (max),
);
-- Language Code: srLat, srCyr, en

CREATE TABLE [Languages]
 (
	[Code]			Text (10) NOT NULL, 
	[Name]			Text (50), 
	[IsActive]			Boolean NOT NULL, 
	[IsDefault]			Boolean NOT NULL
	[SortIndex]			Integer
);

-- Tabela BenefitGroup ima samo text za izveštaj ako je pod benefitom, prebaci u Translations / BRISEM
-- ResourceKey npr: Benefit.ReportDescription 
-- srLat /en: Na računu je primenjen popust na uslugu održavanja stanova i garažnih mesta. /A discount has been applied to the service of maintenance of apartments and garage spaces.

-- IsUsed MAREKER nakon izdavanja računa da li je iskorišćen benefit na računu za PeriodYYMM, nema potrebe za ovim poljem ako je InvoiceId unet iskorišćen je
-- EntryDate datum unosa benefita, npr 11.1.2026 unose se benefiti - 6 redova za svaki mesec po jedan: 2602, 2603....

CREATE TABLE [Benefit]
 (
	[Id]					Long Integer, 
	[ContractId]			Long Integer,  -- ContractId ima i UnitId i PartnerId, i dalje je FK na Contract.Id
	[PeriodYYMM]			Integer, 
	[EntryDate]				DateTime, 
	[InvoiceId]				Long Integer, 
);

-- InvoiceNo - redni broj u PeriodYYMM - tretira se kao Sortiranje na računu.
-- PaymentPriority - sortiranje za rasknjižavanje kod naplate - obično je isto što i InvoiceNo
-- InvoiceNameFunction_ naziv funkcije koja se primenjuje na Caption, Investiciono održavanje za #YYMM#
-- Caption ide na štampu računa
-- CodeName ide na knjiženje.... 2026-09-30 /AJSASOFT /EVIDENCIJA RAČUNA 2609 - OBAVEZNO ZA DOCUMENTTYPE 2
-- PostedInvoiceAmount - KOLIKO JE KNJIŽENO - nakon generisanja računa, povratna informacija generisanih računa (0.12 EUR po srednjem kursu 117.1234 Din po kvadratu za stanove i lokale..... nema info koliko je to para dok god se ne generisu realni iznosi, zaokruženi po svakom računu)
-- PreviousSupplierInvoiceId, NewSupplierInvoiceId - vezano za izdavanje računa u toku, avansi, konačni itd. Obično se ne koristi ali neka ostane.
-- JournalEntryId - nakon knjiženja, broj naloga
-- ClosesAccount kod DocumentTypeId:9 postoji specifična šema za zatvaranje naloga 
-- DocumentTypeId  / ShortList.TableName: SupplierDocumentType



CREATE TABLE [SupplierInvoice]
 (
	[Id]							Long Integer, 
	[CompanyId]						Long Integer, 
	[InvoiceNo]						Long Integer, 
	[CodeName]						Text (255),  -- RBR CODE: obavezan unos za DocumentTypeId: 2 
	[Caption]						Text (255), 
	[SupplierPartnerAccountId]		Long Integer,   -- Fk na PartnerAccount
	[CalculationTypeId]				Long Integer,  -- FK to CalculationType.Id
	[PeriodYYMM]					Integer, -- 2604
	[InvoiceTotalCalculationAmountEur]		Decimal(18,4), 
	[InvoiceTotalCalculationAmountRsd]		Decimal(18,4), 
	[CalculationAmountByCoefficientEur]		Decimal(18,4), 
	[CalculationAmountByCoefficientRsd]		Decimal(18,4), 
	[PaymentPriority]					Long Integer, 
	[SubAccountId]						Text (10),  -- FK to SubAccount.Id  -- podkonto knjiženja: 11302 - Upravljanje zgradom
	[DocumentTypeId]					Long Integer, -- FK  ShortList.TableName: SupplierDocumentType 1 - Predviđeni troškovi, 2 - Izvršeni troškovi, ...
	[ExtraordinaryInvoiceMarker]			Text (10),  -- MVR: marker vandrednih računa.. v01, o01
	[InvoiceNameFunction]				Text (255), 	
	[PostedInvoiceAmount]				Decimal(18,2),
	[InvoiceDate]					DateTime, 
	[TransactionDate]		DateTime,  -- isto kako i PostingDate
	[PaymentDate]			DateTime, 
	[InvoiceDescription]			Text (255), 
	[PaymentReference]			Text (255), 
	[PreviousSupplierInvoiceId]		Long Integer,  
	[NewSupplierInvoiceId]			Long Integer, 
	[JournalEntryId]			Long Integer,  -- broj naloga
	[ClosesAccount]			Text (10)
);

-- tip obracuna dobavljaca racuna
-- UnitOfMeasureId / ShortList.TableName: UnitOfMeasure  / m2, kom,  / SEF poseduje celokupnu listu jedinica mera na racunu

CREATE TABLE [CalculationType]
 (
	[Id]						Long Integer NOT NULL, 
	[Name]						Text (100), 
	[SupplierInvoiceAmount]		Text (255),  -- function to get total invoice
	[Amount]					Text (255),  -- function to amaount per tenet invoice
	[Quantity]					Text (255),  -- function what is quantity per tenet invoice
	[UnitOfMeasureId]			Long Integer,  -- ShortList.TableName: UnitOfMeasure
	[Note]						Text (255), 
);

-- lista na koji tip objekta se odnosi racun /tipovi objekata / stanovi, lokali, garaze... 
-- UnitTypeId / ShortList: TableName: UnitType 
CREATE TABLE [SupplierInvoiceUnitType]
 (
	[Id]					Long Integer, 
	[SupplierInvoiceId]		Long Integer, 
	[UnitTypeId]			Long Integer -- FK ShortList.TableName: UnitType
);

-- DA - digitalna arhiva dokumenata
-- CategoryId opciona gategorija na eArhiv
-- DocumentTypeId / ShortList: TableName: DocumentType

CREATE TABLE [Documents]
 (
	[Id]						Long Integer, 
	[DocumentTypeId]			Long Integer,  -- ShortList: TableName: DocumentType
	[SourceTable]				Text (255),  
	[ReferenceId]				Long Integer, 
	[FileName]					Text (255), 
	[RelativePath]				Text (255), 
	[Date]						DateTime, 
	[Description]				Text (255), 
	[FileNameSuffix]			Text (10), 
	[FileExtension]				Text (10), 
	[Registrar]					Text (4)
	[RegistrarLocation]			Text (255), 
	[IsEDocument]				Boolean,
	[CategoryId]				Long Integer -- FK to DocumentCategory.Id
	[TimeStamp]					Date
);

CREATE TABLE [DocumentCategory]
 (
	[Id]				Long Integer, 
	[GroupName]			Text (255), 
	[Name]				Text (255), 
	[Code]				Text (10), 
	[Description]		Text (255), 
	[IsActive]			Boolean
	[RetentionPeriodYY]	Integer
);

-- LedgerEntry.Parameters - ogoljen poziv na broj plaćanja sa računa
-- Note se korisiti za opise na izveštajima kod specificnih transakcija
-- Description ostavljeno za komentare 
-- LineTypeId / tblShortList.TableName: LedgerLineType
-- gk tabela
CREATE TABLE [LedgerEntry]
 (
	[Id]					Long Integer, 
	[JournalEntryId]		Long Integer, 	-- FK to JournalEntry.Id
	[Account]				Text (10),  --FK to ChartOfAccounts.Account
	[PostingDate]			DateTime,  -- posting date, transaction date
	[DueDate]				DateTime, 
	[DebitAmount]			Double, 
	[CreditAmount]			Double, 
	[LineTypeId]			Double, 	--	FK to ShortList: TableName: LedgerLineType
	[DocumentRef]			Text (50), 
	[CompanyId]				Long Integer, 
	[PartnerAccountId]		Long Integer,  -- FK TO PartnerAccount.Id
	[BankStatementLineId]	Long Integer, -- FK to BankStatementLine
	[Note]					Text (255),  
	[Parameters]			Text (25), 
	[Description]			Text (255),  
	[SubAccountId]			Text (10),  -- FK to SubAccount.Id --  kontro troškova 
	[SupplierInvoiceId]		Long Integer, -- FK to SupplierInvoice
	[InvoiceId]				Long Integer, --	FK to invoice
	[Priority]				Long Integer, 
);

-- necemo za sad koristiti ali neka ostane
CREATE TABLE [FiscalYear]
 (
	[Id]				Long Integer,
	[CompanyId]			Long Integer NOT NULL, 
	[Year]				Long Integer NOT NULL, 
	[StartDate]			DateTime, 
	[EndDate]			DateTime, 
	[IsArchived]			Long Integer, 
	[Display]			Text (255), 
	[Folder]			Text (255), 
	[FileName]			Text (255), 
	[IsCurrent]			Long Integer
);


-- OPOMENE
-- NoticeTypeId - Samostalna opomene, Notifikacija na računu
CREATE TABLE [NoticeBatch]
 (
	[Id]			Long Integer, 
	[CompanyId]			Long Integer, 
	[Title]			Text (50), 
	[Date]			DateTime, 
	[MinUnpaidInvoiceCount]			Long Integer, 
	[DebtTolerance]			Long Integer, 
	[DebtToleranceByMonth]			Long Integer, 
	[NoticeTemplateId]			Long Integer, 
	[NoticeTypeId]			Long Integer, 
	[UpToClaimDate]			DateTime, 
	[UpToPaymentDate]			DateTime, 
	[InvoiceBatchId]			Long Integer, 
	[CustomCaptionOnOnSlip]			Text (255), 
);

CREATE TABLE [InvoiceBatch]
 (
	[Id]			Long Integer, 
	[CompanyId]					Long Integer, 
	[PeriodYYMM]	Integer, -- 2609
	[Caption]		Text (50),  -- Septembar 2026
	[Month]			Integer,  -- 9
	[Year]			Integer,  -- 2026
	[Place]			Text (50),  -- 	sedište kompanije uvek!
	[IssueDate]					DateTime,   -- 05.10.2026
	[ServiceDateFrom]			DateTime,  
	[ServiceDateTo]				DateTime,  
	[TransactionDate]			DateTime, 
	[DueDate]					DateTime, 
	[ExchangeRateNbs]			Decimal(18,4), 
	[JournalEntryId]			Long Integer,  -- NALOG KNJIŽENJA
	[EntryDate]					DateTime, 
	[StaffId]					Long Integer, 
	[ExtraordinaryInvoiceMarker]	Text (255),  -- MVR
	[BalanceAsOfDate]			DateTime,  -- Datum preseka stanja
	[PreviousValueDate]			DateTime,  --Prethodna valuta za obračun kamate
	[IsInterestCalculated]		Long Integer -- Da li raditi obračun kamate
);

CREATE TABLE [BankStatement]
 (
	[Id]			Long Integer, 
	[BankAccountId]			Long Integer,  -- preko ovaga imammo koja je companyId
	[StatementNumber]			Long Integer, 
	[StatementSuffix]			Text (50), 
	[Date]			DateTime, 
	[PreviousBalance]			Currency, 
	[NewBalance]			Currency, 
	[Debit]			Currency, 
	[Credit]			Currency, 
	[CountDebitEntry]			Long Integer, 
	[CountCreditEntry]			Long Integer, 
	[Note]			Text (50), 
	[JournalEntryId]			Long Integer, 
	[IsPosted]			Boolean NOT NULL
);

CREATE TABLE [BankStatementLine]
 (
	[Id]			Long Integer, 
	[BankStatementId]			Long Integer, 
	[LineNumber]			Long Integer, 
	[PayerRecipientName]			Text (255), 
	[BankAccountNumber]			Text (50), 
	[Debit]						Currency, 
	[Credit]					Currency, 
	[Info]			Text (255), 
	[Code]			Long Integer, 
	[PaymentReference]			Text (50), 
	[PaymentReferenceOut]			Text (50), 
	[PartnerAccountId]			Long Integer, 
	[SubAccountId]			Text (10),  -- FK to SubAccount.Id -- kontro troškova
	[IsPosted]			Boolean NOT NULL, 
	[IsIgnored]			Boolean NOT NULL, 
	[IsMatched]			Boolean NOT NULL,
	[BankRef]			Text (50) -- bank reference 
);


-- KontniOkvir / KontniPlan

CREATE TABLE [ChartOfAccounts]
 (
	[Account]			Text (10) NOT NULL, 
	[ShortName]			Text (50),  -- Za upotrebu u App kad treba kraći naziv, ako je null koristi Name
	[Name]				Text (255), 
	[ParentAccount]		Text (10), 
	[Level]				Long Integer, 	-- Možda nepotrebno, broj karaktera u Account
	[Sign]				Integer,  -- def 1 ili je-1 / rotacija za izveštaje
	[IsActive]			Boolean, 	
	[IsSinteticAccount]	Boolean,     
);

--
CREATE TABLE [SubAccount]
 (
	[Id]					Text (10) NOT NULL,  -- PK
	[Name]					Text (50),
	[ParentSubAccountId]	Text (10), -- FK to Id 
	[CostToSubAccountId]	Text (10), -- FK to Id
	[InterestSubAccountId]	Text (10), -- FK to Id
	[IsActive]				Boolean, 	--def TRUE
	
);


-- PartnerAccount
CREATE TABLE [PartnerAccount]
 (
	[Id]				Long Integer NOT NULL, 
	[CompanyId]			Long Integer,  -- FK to Companies.Id, MOŽE BITI NULL ZA DOBAVLJAČE JER SE KORISTI U SVI KOMPANIJAMA
	[Account]			Text (10) NOT NULL, --FK to ChartOfAccounts.Account
	[PartnerId]			Long Integer NOT NULL,  -- FK to Partners.Id	 
	[ContractId]		Long Integer,  -- FK to Contracts.Id  -- ako je dobavljač nema ugovora, ugovor je null
	[AccountNumber]		Long Integer NOT NULL,  -- Generated UNIQU IN CompanyId / 2040 - 1001-5999 / 4350 - 9001 -...  -- OVO SE KORISTI ZA PRETRAGU
);

-- PartnerTypeId - ShortList: TableName: PartnerType / Pravno lice, ovlašćeno lice
CREATE TABLE [Partner]
 (
	[Id]			Long Integer NOT NULL, 
	[CompanyId]		Long Integer,  -- If IsNull used in all companies, svi dobavljači po def su Null
	[ShortName]		Text (100),  -- use in App
	[Name]			Text (255),  -- Apr full name for pravna lica
	--za pravna lica
	[RegistrationNumber]	Text (10),  -- MB: MATIČNI BROJ KOMPANIJE / 8 KARATERA
	[TaxNumber]				Text (10), 	-- PIB: KOMPANIJA, STRANO LICE SA PIBom / 9 KARAKTERA
	[Jbkjs]					Text (10),  -- JBKJS: PRAVNO LICE, Javni broj korisnika javnih sredstava / 5 karatera
	[IsSefUser]				Boolean,    -- SEF, i ako im info pre slanja treba proveriti po pibu / jbkjs
	[IsCrfUser]				Boolean,  	-- CRF ako ima PIB i SEF možda je CRF - ručni unos
	[SkipAutoCheckSef]		Boolean, 	-- Lockovano da se proverava pre slanja, iskustveno GU
	--za fitička lica
	[IdCardNumber]	Text(10), -- Broj licne karte / 9 KARATERA
	[Jmbg] 			Text(15), -- 13 karaktera
	
	[PartnerTypeId]			Long Integer,  -- FK ShortList: TableName: PartnerType 
	[Language]			Text (10),  -- def srLat / settings
	[Note]			Text (max),  -- mEMO	
);

-- AddressTypeId - ShortList: TableName: AddressType
-- Neophodna podela Adress i PartnerAdress, jer se Adress korisiti i u Building Enerance
CREATE TABLE [PartnerAddress] 
(
	[Id]			Long Integer NOT NULL, 
	[PartnerId]		Long Integer,  
	[AddressId]		Long Integer,  
	[AddressTypeId]	Long Integer,  -- FK do shortlist
	[IsDefault]		Boolean,  -- if more then 1 in same AddressTypeId
	[TimeStamp]		DateTime,  -- last update date/time
)
-- ovde može i postalCode i City da ide u posebnu tabelu, pa onda i ContryCode
CREATE TABLE [Address] 
 (
 	[Id]			Long Integer NOT NULL, 
 	[Address]		Text (255), 
 	[PostalCode]	Long Integer, 
 	[City]			Text (255), 
 	[CountryCode]	Text (2),  -- def in Settings ..... RS
);



CREATE TABLE [ExchangeRate]
 (
	[Id]				Long Integer, 
	[Rate]				Decimal(19,4),  -- obavezno 4 decimale
	[RateDateFrom]		DateTime, 
	[TimeStamp]			DateTime
);

CREATE TABLE [PartnerComms]
 (
	[Id]				Long Integer NOT NULL, 
	[PartnerId]			Long Integer,  
	[ChannelId]			Long Integer,   -- FK to ShortList.TableName: ChannelComms
	[ValueNormalized]	Text (255),  -- samo cifre za telefon, email lower case, http 	
	[Note] 				Text (255),  -- Dodatna napomena, ima osobe za pravna lica....
	[IsActive]			Boolean,
	[IsPrimary]			Boolean,
	[SortIndex]			Integer,
	[IsRegisterToInvoiceReceive]	Boolean,
);


CREATE TABLE [SentEmail]
 (
	[Id]			Long Integer, 
	[Subject]		Text (255), 
	[To]			Text (255), 
	[Cc]			Text (255), 
	[Bcc]			Text (255), 
	[BodyHtml]		Text (max),
	[Created]		DateTime, 
	[Sent]			DateTime, 
	[Archive]		Boolean, 
	[SendStatusId]		Text (255),  -- FK to ShortList.TableName: SendEmailStatus
	[SendDescription]	Text (max),
	
);

CREATE TABLE [SentEmailAttachment]
 (
	[Id]			Long Integer, 
	[SentEmailId]	Long Integer, 
	[FilePath]		Text (255), 
);

-- Nalog
CREATE TABLE [JournalEntry]
 (
	[Id]					Long Integer, 
	[CompanyId]				Long Integer, 
	[PostingDate]			DateTime, 
	[DueDate]				DateTime, 
	[Balance]				Currency, 
	[Note]					Text (255), 
	[Description]			Text (50), 
	[JournalEntryTypeId]	Long Integer, -- FK to ShortList: TableName: LedgerLineType / ABSOLUTNO ISTO
	[Currency] 				Text(3)	  -- on new get def value from settings(CurrencyDef) - RSD
	[IsPosted]				Boolean	-- default false
	[PostedDate]			DateTime	
	[PostedUserId]			Long Integer, -- FK to Users/Staff
);

CREATE TABLE [LocationCategory]
 (
	[Id]			Long Integer NOT NULL, 
	[Name]			Text (255)
	[ParentId]      Long Integer, -- FK to ShortList: CategoryLocation.Id
	[SortIndex]		Integer -- Sort index
);


CREATE TABLE [Unit]
 (
	[Id]				Long Integer NOT NULL, 
	[CompanyId]			Long Integer NOT NULL,  
	[Name]				Text (255), 
	[ContractId]		Long Integer,  -- not assigned unit are Null
	[UnitTypeId]		Long Integer,  -- FK to  ShortList: UnitType / Stan, Lokal, Poslovni prostor...
	[BuildingEntranceId]	Long Integer, -- FK to BuildingEntrance .... ovde je i veza sa adresom
	[Note]				Text (255),  -- interna napomena
	[SortingNumber] 	Long Integer, -- Stanovi 1 - 999, Lokali 1000-9999, GM 10000 x nivo + redni broj
	[K1]			Double,  -- KVADRATURA ZA OBRAČUN
	[K2]			Double,  -- KOLIČINA ZA OBRAČUN / DEF 1 ali može biti umanjeno ili uvećano npr invalidsko p mesto 1.5
	[K3]			Double,  -- slobodan K - NPR dvorišta prizmelje, fakturisanje za košenje
	[K4]			Double,  --  -- || -- - tagovi za garažu, popust za penzionere samo za upravljanje....
	[K5]			Double, 
	[FloorNumber]			Long Integer,  
);



CREATE TABLE [Contract]
 (
	[Id]				Long Integer, 
	[AccountNumber]		Long Integer,  -- ZAMENA ZA PARTNER ACCOUNTING ID
	[UnitId]			Long Integer,  -- FK to Units
	[OwnerPartnerId]	Long Integer,  -- FK to Partners / 
	[InvoicePartnerId]	Long Integer,  -- FK to Partners / def owner / ako je unet tenet onda korsinik bira izmedju dva
	[TenetPartnerId]    Long Integer,  -- FK to Partners 
	[ContractDate]		Date, -- datum ugovora ili datium primo predaje
	[ContractEndDate] 	Date, -- datum kraja ugovora
	[InvoiceStartDate]	Date, -- datum pocetka fakturisanja
	[InvoiceEndDate]	Date, -- datum pocetka fakturisanja
	[IsActive]			Boolean
	[Note]				Text (Max) -- interna napomena

	--INVOICE DATA
	[InvoiceDeliveryLocation]		Text (10),    -- Text in front of units in Invoice.... npr BW-ETE za Beograd na vodi 
	[InvoiceDeliveryUnitId]			Long Integer,  -- FK to Units....
	[InvoiceLegacyMasterId]			Long Integer,  -- ID od grupnog računa ako se koristi
	
	-- defaultna vrednost je štampa računa
	[IsPrintInvoiceMandatory]		Boolean,  -- obavezno štampanje zahtevano od korisnika
	[IsPrintInvoiceToPostOffice]	Boolean,  -- da se šalje preko pošte
	[IsPrintInvoiceSkiped]			Boolean,  -- zahtev da se ne štampa račun, kontrola da li ima elektronski račun, kontrola da IsPrintInvoiceMandatory mora biti false, IsPrintInvoiceToPostOffice false.

	[ExportExternalAccount]			Text (255),  -- povezivanje sa spoljnim knjigovodstvenim agencijama
);




-- OPOMENA
CREATE TABLE [Notice]
 (
	[Id]					Long Integer, 
	[NoticeBatchId]			Long Integer,  -- FK to NoticeBatch.Id
	[PartnerAccountId]		Long Integer,  -- FK to PartnerAccount.Id
	[UnpaidInvoiceCount]	Long Integer, 
	[Debt]					Decimal(18,2), 
	[InvoiceText]			Text (255), 
	[IsActive]				Long Integer, 
	[PaymentReference]		Text (50), 
	[AditionalCosts]		Long Integer,  -- Advokatski troškovi
	[Total]					Long Integer, -- AditionalCosts + Debt
);

CREATE TABLE [NoticeLine]
 (
	[Id]			Long Integer, 
	[NoticeId]			Long Integer,  -- FK to Notice.Id
	[Parameters]			Text (25), 
	[DocumentRef]			Text (255), 
	[Debit]			Double, 
	[Credit]			Double, 
	[Sum]			Double, 
	[Text]			Text (255), 
	[DueDate]			DateTime, 
	[InvoiceId]			Long Integer, 
	[InvoiceDate]			DateTime, 
	[UnitAddress]			Text (255)
);


CREATE TABLE [SelectionBasket]
 (
	[SelectionBasketId]	Long Integer, 
	[TargetId]			Long Integer, 
	[TypeIndex]			Long Integer
);

CREATE TABLE [Events]
 (
	[EventId]			Long Integer, 
	[DateOfRequest]		DateTime, 
	[DateOfExecution]	DateTime, 
	[ContractId]		Long Integer,  -- FK to Contract.Id
	[PartnerId]			Long Integer,  -- FK to Partner.Id
	[UnitId]			Long Integer,  -- FK to Unit.Id
	[Description]		Text (255),  -- opis događaja
	[PreviousValue]		Text (255),  -- vrednost pre promene
	[NewValue]			Text (255),  -- vrednost posle promene
	[FieldsRelated]		Text (255),  -- polja koja su vezana za događaj
	[RequestTypeId]		Long Integer,  -- FK to ShortList: TableName: EventRequestType
	[RequestBy]			Text (255),  -- ko je zahtevao / emial, tel....
	[RequestThrough]	Text (255)  -- preko čega je zahtevao / email upravnika 
);


CREATE TABLE [Invoice]
 (
	[Id]				Long Integer, 
	[SequenceNumber]	Text (20), 
	[InvoiceBatchId]			Long Integer, 
	[IssueDate]			DateTime, 
	[PlaceOfIssue]			Text (50), 
	[ServiceDate]			Text (50), 
	[TransactionDate]			DateTime, 
	[ValueDate]			DateTime, 
	[PartnerId]			Long Integer, 
	[CompanyId]			Long Integer, 
	[PartnerName]			Text (255), 
	[PostalCode]			Text (50), 
	[Address]			Text (50), 
	[TaxId]			Text (50), 
	[RegistrationNumber]			Text (255), 
	[Sum]			Double, 
	[VatRate]			Double, 
	[VatAmount]			Double, 
	[Total]			Double, 
	[PrethodniDug]			Single, 
	[PaymentPurpose]			Text (255), 
	[Currency]			Text (50), 
	[PaymentReference]			Text (50), 
	[PaymentSlipText]			Text (255), 
	[Note]			Text (255), 
	[PageCount]			Long Integer, 
	[IsCancelled]			Boolean NOT NULL, 
	[AdditionalNote]			Text (255), 
	[UnitName]			Text (50), 
	[Manager]			Long Integer, 
	[PaymentPurpose2]			Text (255), 
	[UnitId]			Long Integer, 
	[UnitAddress]			Text (255), 
	[AmountDue]			Double, 
	[MailingName]			Text (50), 
	[MailingAddress]			Text (50), 
	[MailingPostalCode]			Text (50), 
	[MailingTaxId]			Text (50), 
	[LegacyTempId1]			Long Integer, 
	[LegacyTempId2]			Long Integer, 
	[LegacyTempCode1]			Text (50), 
	[LegacyTempCode2]			Text (50), 
	[QrCodeFlag]			Long Integer, 
	[PaymentSlipAmount]			Double, 
	[PaymentFieldTypeId]			Long Integer, 
	[ReminderId]			Long Integer, 
	[GroupInvoiceId]			Long Integer, 
	[City]			Text (255), 
	[Location]			Text (255), 
	[SortOrder]			Long Integer, 
	[CancelledDate]			DateTime, 
	[InvoiceLayout]			Text (255), 
	[BalanceAsOfDate]			DateTime, 
	[InterestAmount]			Double, 
	[InvoiceTotal]			Double
);


CREATE TABLE [InvoiceUnit]
 (
	[Id]				Long Integer,
	[InvoiceId]			Long Integer, 
	[UnitId]			Long Integer -- mozda može da se koristi i contractId umesto ovoga
);



CREATE TABLE [InvoiceLine]
 (
	[Id]							Long Integer,
	[InvoiceId]						Long Integer, 
	[InvoiceBatchId]				Long Integer, 
	[PartnerId]						Long Integer, 
	[CompanyId]						Long Integer, 
	[SupplierInvoiceId]				Long Integer, 
	[Name]						Text(255), 
	[K1]			Double, 
	[K2]			Double, 
	[K3]			Double, 
	[K4]			Double, 
	[K5]			Double, 
	[Quantity]			Double, 
	[PriceEur]			Double, 
	[ExchangeRateNbs]			Double, 
	[Amount]			Double, 
	[Sum]			Double, 
	[VatRate]			Double, 
	[VatAmount]			Double, 
	[TotalRsd]			Double, 
	[SortOrder]			Long Integer, 
	
	[InvoiceAmount]			Double, 
	[K1xK2]				Double, 
	[K2xK3]			Double, 
	[K2xK4]			Double, 
	[K2xK5]			Double, 
	[UnitOfMeasure]			Text (50), 
	[QuantityAlt]			Double
);

CREATE TABLE [Setting]
 (
	[Id]			Long Integer, 
	[CompanyId]		Long Integer, 
	[Name]			Text (255), 
	[Key]			Text (50), 
	[Value]			Text (255), 
	[Description]	Text (255), 
	[Category]		Text (50), 
	[ValueMax]		Text (Max)
);


CREATE TABLE [ImportMapping]
(

)

CREATE TABLE [ImportMappingGroup]
 (
	[SortIndex]			Long Integer NOT NULL, 
	[Category]			Text (50), 
	[SqlMemo]			Memo/Hyperlink (255), 
	[Sql]			Text (255)
);

CREATE TABLE [FormGridSetting]
 (
	[FormGridSettingId]			Long Integer, 
	[FormName]			Text (255), 
	[ParentFormName]			Text (255), 
	[ComponentName]			Text (255), 
	[StaffId]			Long Integer, 
	[LayoutIndex]			Long Integer, 
	[ColumnName]			Text (255), 
	[ColumnOrder]			Long Integer, 
	[ColumnWidth]			Long Integer, 
	[ColumnHidden]			Long Integer
);

CREATE TABLE [Company]
 (
	[Id]				Long Integer NOT NULL, 
	[PartnerId]			Long Integer NOT NULL, -- FK Partner.Id
	[ManagerId]			Long Integer, -- FK Partner.Id
	[ShortName]			Text (50), 
	[PrintName]			Text (50), 
	[RelativeFolderName] 	Text (50), 


	[PrimaryBankAccountNumber]			Text (50), 
	
	
	[Note]			Text (255), 
	
	[SortOrder]			Long Integer, 
	
	
	
	
	[CategoryLocationId]			Long Integer, 
	
	[Account]			Long Integer, 
	[CompanyStatusId]			Long Integer, 
	[ExternalAccount]			Text (255), 
	[PaymentSlipText]			Text (255), 
	[Manager]			Long Integer, 
	[ContractDate]			DateTime, 
	[IsVatPayer]			Long Integer, 
	[SubjectTypeId]			Long Integer, 
	[InvoiceIssuerId]			Long Integer, 
	[RemittanceInfo]			Text (255), 
	[InvoiceComplaintInfo]			Text (255), 
	[Email]			Text (255), 
	[EmailDisplay]			Text (255), 
	[InvoicePostalCode]			Text (255), 
	[InvoiceCity]			Text (255), 
	[Logo]			Text (255), 
	[Field1Legacy]			Text (255), 
	[QrName]			Text (255)
);

CREATE TABLE [Staff]
 (
	[StaffId]			Long Integer, 
	[UserName]			Text (50), 
	[StaffLogin]			Text (50), 
	[Level]			Long Integer, 
	[LastLoginAt]			Text (50), 
	[PreferredLanguage]			Text (50), 
	[AccessRestriction]			Text (255), 
	[LastComputerName]			Text (255)
);

CREATE TABLE [StaffPermission]
 (
	[StaffPermissionId]			Long Integer, 
	[StaffId]			Long Integer, 
	[KeyName]			Text (255), 
	[FormName]			Text (255), 
	[PermissionValue]			Long Integer, 
	[IsDisabled]			Long Integer
);



CREATE TABLE [BuildingEntrance]
 (
	[Id]			Long Integer NOT NULL, 
	[CompanyId]			Long Integer, 
	[BuildingName]		Text (255), 
	[EntranceName]		Text (255), 
	[AdressId]			Long Integer, -- FK to Adress
	[Label]			Text (255), 
	[Description]			Text (255), 
	[SortOrder]			Long Integer
);

CREATE TABLE [ContactImport]
 (
	[LegacyRowId]			Long Integer, 
	[Id]			Long Integer, 
	[Phone]			Text (255), 
	[Language]			Text (255), 
	[Email]			Text (255)
);


CREATE TABLE [StatReportDefinition]
 (
	[StatReportDefinitionId]			Long Integer, 
	[StatGroup]			Text (255), 
	[StatName]			Text (255), 
	[StatQueryName]			Text (255), 
	[StatSql]			Memo/Hyperlink (255), 
	[StatReportName]			Text (50), 
	[StatWhereCaption]			Text (50), 
	[StatWhereComboSql]			Memo/Hyperlink (255), 
	[StatSqlName]			Text (50)
);

CREATE TABLE [WhereClauseTemplate]
 (
	[WhereClauseTemplateId]			Long Integer, 
	[Title]			Text (50), 
	[WhereExpression]			Memo/Hyperlink (255), 
	[TargetFormName]			Text (50)
);


-- ok
CREATE TABLE [BankAccount]
 (
	[BankAccountId]			Long Integer, 
	[AccountNumber]			Text (50), 
	[IsActive]				Long Integer, 
	[PartnerId]				Long Integer, 
	[CompanyId]				Long Integer, 
	[SortIndex]				Long Integer,
	[Currency]				Text (3), -- on new get def value from settings(CurrencyDef) - RSD
);

CREATE TABLE [AdditionalTextType]
 (
	[AdditionalTextTypeId]			Long Integer NOT NULL, 
	[Text]			Text (255)
);

CREATE TABLE [UnitType]
 (
	[UnitTypeId]			Long Integer NOT NULL, 
	[Name]			Text (50), 
	[PrintLabel]			Text (50), 
	[SortOrder]			Long Integer, 
	[BinaryValue]			Long Integer, 
	[InvoiceTitle]			Text (50), 
	[Disclaimer]			Memo/Hyperlink (255), 
	[DisclaimerSpc]			Memo/Hyperlink (255), 
	[InvoiceTitleSpc]			Text (50), 
	[CbValue]			Double, 
	[IgValue]			Double, 
	[CbAccount]			Long Integer, 
	[IgAccount]			Long Integer
);


CREATE TABLE [PartnerCategory]
 (
	[PartnerCategoryId]			Long Integer NOT NULL, 
	[Name]			Text (255)
);

CREATE TABLE [LineItemType]
 (
	[LineItemTypeId]			Long Integer NOT NULL, 
	[Name]			Text (50)
);

CREATE TABLE [TaskType]
 (
	[TaskTypeId]			Long Integer NOT NULL, 
	[Name]			Text (50)
);

CREATE TABLE [PaymentSlipType]
 (
	[PaymentSlipTypeId]			Long Integer, 
	[Name]			Text (255), 
	[Description]			Text (50)
);

CREATE TABLE [SubAccount]
 (
	[SubAccountCode]			Text (255) NOT NULL, 
	[Name]			Text (255), 
	[PreviousSubAccountCode]			Text (255), 
	[CostAllocationTarget]			Text (255), 
	[Interest]			Text (255)
);

CREATE TABLE [SubAccountDefaultSupplier]
 (
	[CompanyId]			Long Integer NOT NULL, 
	[Account]			Text (255) NOT NULL, 
	[DefaultSupplierPartnerAccountingId]			Text (255)
);

CREATE TABLE [PartnerBankAccount]
 (
	[PartnerBankAccountId]			Long Integer, 
	[AccountNumber]			Text (50), 
	[PartnerId]			Long Integer
);

CREATE TABLE [PaymentOrder]
 (
	[Id]			Long Integer, 
	[TemplateTitle]			Text (50), 
	[PayerName]			Text (255), 
	[PaymentPurpose]			Text (255), 
	[RecipientName]			Text (255), 
	[PaymentCode]			Integer, 
	[Currency]			Text (3), 
	[Amount]			Currency, 
	[PayerAccountNumber]			Text (50), 
	[PayerModelNumber]			Integer, 
	[PayerPaymentReference]			Text (50), 
	[RecipientAccountNumber]			Text (50), 
	[RecipientModelNumber]			Integer, 
	[RecipientPaymentReference]			Text (50), 
	[Place]			Text (50), 
	[Date]			DateTime, 
	[ValueDate]			DateTime, 
	[IsUrgent]			Boolean NOT NULL, 
	[Type]			Long Integer, 
	[ArchivedAt]			DateTime, 
	[RefSourceId]			Long Integer, 
	[RefSourceTag]			Text (255), 
	[PrintMe]			Boolean NOT NULL, 
	[IsFavorite]			Boolean NOT NULL
);

CREATE TABLE [PenaltyInterestStaging]
 (
	[PenaltyInterestStagingId]			Long Integer, 
	[CompanyId]			Long Integer, 
	[JournalEntryId]			Double, 
	[Account]			Text (50), 
	[Date]			DateTime, 
	[CreditAmount]			Double, 
	[DebitAmount]			Double, 
	[LineTypeId]			Double, 
	[Parameters]			Text (25), 
	[Note]			Text (20), 
	[PartnerAccountingId]			Long Integer, 
	[ValueDate]			DateTime, 
	[InvoiceId]			Long Integer, 
	[Document]			Text (255), 
	[SubAccountCode]			Text (255), 
	[InvoiceBatchId]			Long Integer, 
	[PostingSubAccount]			Text (255), 
	[PostingSupplierInvoiceId]			Long Integer, 
	[SupplierInvoiceId]			Long Integer, 
	[Priority]			Long Integer
);

CREATE TABLE [InterestStatement]
 (
	[InterestStatementId]			Long Integer, 
	[CompanyId]			Long Integer, 
	[Account]			Text (50), 
	[Date]			DateTime, 
	[BaseAmount]			Double, 
	[Balance]			Double, 
	[Days]			Long Integer, 
	[Rate]			Double, 
	[Coefficient]			Numeric (18, 8), 
	[Interest]			Double, 
	[ReferenceTag]			Text (50), 
	[PartnerAccountingId]			Long Integer, 
	[UnitId]			Long Integer, 
	[SubAccountCode]			Text (255), 
	[InvoiceBatchId]			Long Integer
);


CREATE TABLE [EPaymentOrderSetting]
 (
	[EPaymentOrderSettingId]			Long Integer, 
	[SortIndex]			Long Integer, 
	[CharacterCount]			Long Integer, 
	[CharacterType]			Text (50), 
	[Function]			Text (50), 
	[Category]			Text (50), 
	[Description]			Text (255), 
	[Defaults]			Text (50), 
	[FieldName]			Text (50), 
	[Format]			Text (50), 
	[RowNumber]			Long Integer
);


CREATE TABLE [BankStatementPostingTemplate]
 (
	[BankStatementPostingTemplateId]			Long Integer, 
	[TemplateGroupId]			Long Integer, 
	[Template]			Text (255), 
	[FieldName]			Text (255), 
	[FieldValue]			Text (255), 
	[Function]			Text (255), 
	[SetId]			Long Integer, 
	[OverrideFunction]			Long Integer, 
	[SetAccount]			Text (50)
);


-- insted of 30 short table, use one for multy purpose
-- Id:AutoNumber; TableName: AddressType; Caption: Sedište; IndexValue: 1
-- Id:AutoNumber; TableName: AddressType; Caption: Prebivalište; IndexValue: 2
-- povezivanje se vrši sa Id PK

CREATE TABLE [ShortList]
 (
	[Id]				Long Integer NOT NULL, 
	[TableName]			Text (255) NOT NULL, 
	[Caption]			Text (255) NOT NULL, 
	[ShortName]			Text (50),
	[Description]		Text (255),
	[IndexValue]		Long Integer NOT NULL, 
	[IndexSort]			Long Integer NOT NULL, 
	[IndexKey]			Text (50),   -- use for geting function or settings by key for each table value
	[TranslationId]		Long Integer -- insteed of Caption if TranslationId > 0 then use Translation
);


CREATE TABLE [BenefitUsageUpdate]
 (
	[Id]			Long Integer, 
	[Date]			DateTime, 
	[UnitId]			Text (255), 
	[PeriodYyMm]			Long Integer, 
	[MonthCount]			Long Integer
);

CREATE TABLE [UnitAreaImport]
 (
	[UnitLabel]			Text (255) NOT NULL, 
	[ClientName]			Text (255), 
	[HandOverDate]			DateTime, 
	[Percentage1]			Double, 
	[Percentage2]			Double, 
	[Percentage3]			Double, 
	[PercentageTotal]			Double
);




CREATE TABLE [BalanceCarryForward]
 (
	[Id]			Long Integer, 
	[ReferenceCode]			Text (255), 
	[PartnerId]			Long Integer, 
	[LegacyRow]			Text (255), 
	[AmountUnit1]			Double, 
	[AmountUnit2]			Double, 
	[PreviousBalance]			Double, 
	[Text]			Text (255), 
	[CompanyId]			Long Integer
);



CREATE TABLE [PostingScheme]
 (
	[PostingSchemeId]			Long Integer, 
	[Name]			Text (255), 
	[SourceTable]			Text (255), 
	[SourceTableWhereField]			Text (255), 
	[Line]			Text (255), 
	[Account]			Text (255), 
	[BaseAmount]			Text (255), 
	[Sign]			Long Integer, 
	[SubAnalyticField]			Text (255), 
	[SubAnalyticFieldSource]			Text (255), 
	[Description]			Text (255), 
	[LedgerLineTypeId]			Long Integer, 
	[LedgerDocumentFunction]			Text (255), 
	[LedgerPaymentReference]			Text (255), 
	[LedgerPartnerId]			Text (255), 
	[LedgerInvoiceId]			Text (255), 
	[SortOrder]			Long Integer, 
	[JournalEntryDescription]			Text (255), 
	[SourceSql]			Memo/Hyperlink (255), 
	[SourceSqlValue]			Text (255)
);



CREATE TABLE [AnalysisReportDefinition]
 (
	[Id]			Long Integer, 
	[DataGroup]			Text (255), 
	[Name]			Text (255), 
	[IsCriticalGroup]			Long Integer, 
	[QueryName]			Text (255), 
	[QuerySql]			Memo/Hyperlink (255), 
	[ReportName]			Text (50), 
	[StatWhereCaption]			Text (50), 
	[StatWhereComboSql]			Memo/Hyperlink (255), 
	[StatSqlName]			Text (50), 
	[LastRunAt]			DateTime, 
	[RowCount]			Long Integer, 
	[AutoRunAll]			Long Integer, 
	[SortOrder]			Long Integer, 
	[LinkCreationTag]			Text (255), 
	[LinkFormName]			Text (255), 
	[IsActive]			Long Integer, 
	[AdminAlert]			Long Integer, 
	[OpenArgs]			Text (255), 
	[Description]			Memo/Hyperlink (255), 
	[DeleteDataInQuery]			Long Integer, 
	[ExecuteQueryDefName]			Text (255)
);

CREATE TABLE [ReportDefinition]
 (
	[Id]			Long Integer, 
	[Name]			Text (255), 
	[Datasheet]			Text (255), 
	[Title]			Text (255), 
	[Button1Caption]			Text (255), 
	[Button1Function]			Text (255), 
	[Button1FunctionId]			Long Integer, 
	[Button2Caption]			Text (255), 
	[Button2Function]			Text (255), 
	[Button2FunctionId]			Long Integer, 
	[Button3Caption]			Text (255), 
	[Button3Function]			Text (255), 
	[Button3FunctionId]			Long Integer, 
	[SortOrder]			Long Integer, 
	[FilterComboSql]			Memo/Hyperlink (255), 
	[FilterComboCaption]			Text (255), 
	[FilterComboId]			Long Integer
);

CREATE TABLE [ReportDefinitionDetail]
 (
	[ReportDefinitionDetailId]			Long Integer, 
	[ReportDefinitionId]			Long Integer, 
	[QuerySql]			Memo/Hyperlink (255), 
	[QueryName]			Text (255), 
	[SortOrder]			Long Integer, 
	[Function]			Text (255)
);

CREATE TABLE [CodeListCatalog]
 (
	[Id]			Long Integer, 
	[Name]			Text (255), 
	[Datasheet]			Text (255), 
	[Button1Caption]			Text (255), 
	[Button1Function]			Text (255), 
	[Button2Caption]			Text (255), 
	[Button2Function]			Text (255), 
	[Button3Caption]			Text (255), 
	[Button3Function]			Text (255)
);

CREATE TABLE [CodeListCatalogDetail]
 (
	[CodeListCatalogDetailId]			Long Integer, 
	[CodeListCatalogId]			Long Integer, 
	[Sql]			Text (255), 
	[Type]			Text (255), 
	[Caption]			Text (255), 
	[SortOrder]			Long Integer
);

CREATE TABLE [WhereClauseTemplate]
 (
	[WhereClauseTemplateId]			Long Integer, 
	[Title]			Text (50), 
	[WhereExpression]			Memo/Hyperlink (255), 
	[TargetFormName]			Text (50)
);



```
