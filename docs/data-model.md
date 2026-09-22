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
	[PaymentPurpose]			Text (255),  -- text na uplatnici, opis
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

-- priiv, samo evidencija da je realizovano
CREATE TABLE [BankInFlow]
(
	[Id]				Long Integer
	[BankAccountId]		Long Integer
	[DateInFlow]		Date
	[ReferenceNumber]	Text(255)
	[Currency]			Text(3)
	[OriginalAmount]	Currency
	[AmountLocalCurrency]	Currency
	[PartnerAccountId]	Long Integer, -- FK PartnerAccount.Id
	[InvoiceDescription]	Text(50), --Tekst za upravnika za NBS stat: Po računu 251-1234-2609,
	[BankStatmentLineId] Long Integer, -- da ne bi pisali datum, broj izvoda, racun.... povezivanje sa stavkom gde su legle pare, ako je upisan onda je završen
	[SentToManager]		DateTime 
	

)

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
	[DefaultSupplierPartnerAccountId]	Text (10)
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
	[Name]			Text (255), -- BELVILLE, BW, PLOT24, PLOT25----
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
	[InvoiceDeliveryLocation]		Text (50),    -- Text in front of units in Invoice.... npr BW-ETE za Beograd na vodi 
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
	[CompanyId]			Long Integer,  -- MOŽDA NEMA POTREBE, InvoiceBatchId IM INFO
	[PartnerId]			Long Integer, 
	[InvoiceBatchId]	Long Integer, 

	[SequenceNumber]	Text (20),  -- RBR
	
	[IssueDate]			DateTime, 
	[PlaceOfIssue]		Text (50), -- DEF. Partner City of CompanyId 
	[ServiceDateFrom]	DateTime,  -- kopira se iz InvoiceBatch ali pojedinačno jer je moguće jedan da nema isti interval
	[ServiceDateTo]		DateTime,  
	[TransactionDate]	DateTime, 
	[DueDate]			DateTime, 
	
	[PartnerName]			Text (255),  -- kopiran podaci sa partnera 
	[Address]				Text (255), 
	[PostalCode]			Text (50), 
	[City]					Text (50), 
	[PAK]					Text (50), 
	[TaxNumber]				Text (50), 
	[RegistrationNumber]	Text (255), 

	[Currency]			Text (3), 

	[Amount]			Decimal (18,2), -- osnovica
	[VatRate]			Decimal (18,2),  -- stopa pdv 
	[VatAmount]			Decimal (18,2),  -- iznos pdv
	[Total]				Decimal (18,2), -- ukupno osnovica + pdv
	[InterestAmount]	Decimal (18,2), -- zatezna kamata
	[InvoiceTotal]		Decimal (18,2), -- total osnovica + pdv + kamata

	[BalanceAsOfDate]	DateTime, -- datum salda
	[PreviousBalance]	Decimal (18,2), -- dug

	[NoticeId]			Long Integer, -- FK to Notice.Id
	
	[PrintNote]			Text (max), -- napomena uz račun / opomena

	[InvoiceDeliveryLocation]		Text (10),    -- Text in front of units in Invoice.... npr BW-ETE za Beograd na vodi 
	[InvoiceDeliveryUnitId]			Long Integer,  -- FK to Units....

	[DeliveryLocation]		Text (255), -- lokacija dostave / InvoiceDeliveryLocation + / + BuildingEnterance.Name + / +  Unit.Name

	[InvoiceLayoutId]	Long Integer -- FK to ShortList: TableName: InvoiceLayout /SzRacun, SzGrupniRačun,  SzZakup ,UpravnikRačun
 
	[InvoiceLegacyMasterId]		Long Integer,  -- FK PartnerAccount.Id
	[InvoiceParentId]   		Long Integer -- SzGrupniRačun, specifakacije od Invoice.Id

	[PaymentReference]	Text (50), -- poziv na broj

	[Note]				Text (255),  --interna napoena
	[PageCount]			Long Integer, -- ostavljamo zbog eArhive, mada verovatno netreba

	[CancelledDate]			DateTime, 
	[IsCancelled]		Boolean NOT NULL,  -- storno
	
	[SortIndex]			Long Integer,  -- JAKO BITNO, ŠTAMPA MORA BITI PO ULAZIMA I REDNO PO STANOVIMA
	
);

--ok
CREATE TABLE [InvoiceUnit]
 (
	[Id]				Long Integer,
	[InvoiceId]			Long Integer, 
	[ContractId]		Long Integer 
);

CREATE TABLE [InvoiceLine]
 (
	[Id]							Long Integer,
	[InvoiceId]						Long Integer, 
	[InvoiceBatchId]				Long Integer, 
	[PartnerId]						Long Integer,  -- direktna veza na partneru jer GM  se sabiraju pa ne može contractId
	[CompanyId]						Long Integer, --mozda netreba
	[SupplierInvoiceId]				Long Integer, 
	[Name]						Text(255), 
	[K1]			Decimal(18,4), 
	[K2]			Decimal(18,4), 
	[K3]			Decimal(18,4), 
	[K4]			Decimal(18,4), 
	[K5]			Decimal(18,4), 
	[Quantity]			Decimal(18,2), 
	[UnitOfMeasureId]	Long Integer, -- FK ShortList: TableName: UnitOfMeasure
	[PriceEur]			Decimal(18,4), 
	[ExchangeRateNbs]	Decimal(18,4), 
	[PricePcs]			Decimal(18,4), -- RSD POJEDIAČNI IZNOS PO KOMADU
	[PriceTotal]		Decimal(18,2), -- RSD TOTAL IZNOS 
	[VatRate]			Decimal(18,2), 
	[VatAmount]			Decimal(18,2), 
	[TotalAmount]		Decimal(18,2), -- Ukupno sa PDV
	[SortIndex]			Long Integer, 
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
	[Id]					Long Integer,
	[ImportMappingGroupId]  Long Integer,
	[TargetField]			Text(255),
	[SourcePath]			Text(255),
	[SourceNode]			Text(255),
	[MappingTypeId]			Long Integer, -- FK ShortList: TableName: MappingType /Identity, Header, Line
	[DataTypeId]			Long Integer, -- FK ShortList: TableName: DataType	 
	[DefaultValue]			Text(255),
	
	[IsRequired]			Boolean,
	[IsKey]

	-- pretaga za bankaccount da vrati bankAccount.Id /Identity
	[IsLookup]				Boolean,		-- true
	[LookupTable]			Text(255),		-- BancAccount
	[LookupField]			Text(255),		-- AccountNumber
	[LookupValueField]		Text(255),		-- Id
	
	[Format]				Text(50),		-- 
	[SortIndex]				Long Integer NOT NULL,
	[Description]			Text(255)	
)

CREATE TABLE [ImportMappingGroup]
(
	[Id]					Long Integer NOT NULL,
	[ImportDefinitionId]	Long Integer NOT NULL,
	[Name]					Text(255),
	[TargetTable]			Text(255),
	[SourcePath]			Text(255),
	[IsRepeating]			Boolean,
)
CREATE TABLE [ImportDefintion]
 (
	[Id]				Long Integer NOT NULL, 			
	[Name]				Text(255)			
	[Code]				Text(50), -- npr OFFICE_EMAIL_XML, 
	[FileMask]			Text(50), -- *.xml
	[ImportSourceId]	Long Integer, -- FK ShortList: TableName: ImportSource / email, gdrive, folder, dragNdrop
	[FilePath]			Text(255), -- 
	[TargetHeaderTable]	Text(255), -- 
	[TargetLineTable]	Text(255), -- 
	[IsActive]			Boolean,
	[SortIndex]			Long Integer NOT NULL, 
);


CREATE TABLE [Company]
 (
	[Id]				Long Integer NOT NULL, 
	[PartnerId]			Long Integer NOT NULL, -- FK Partner.Id
	[ManagerId]			Long Integer, -- FK Partner.Id -- Upravnik za SZ
	[ShortName]			Text (50), 
	[PrintName]			Text (50), 
	[RelativeFolderName] 	Text (50), 
	[LocationCategoryId]			Long Integer, 
	[Note]			Text (255), 
	[SortIndex]			Long Integer, 
	[CompanyTypeId]			Long Integer,  -- FK to ShortList: TableName: CompanyType
	[ExternalAccount]			Text (255),  -- knjigovodstvene agencije
	[LedgerEntryDate]			DateTime,  -- početni datum knjiženja
	[VatTypeId]					Long Integer,  -- FK to ShortList: TableName: VatType
);


-- PREUZETI KAKO TREBA DA PREK OVOGA MOŽE DA SE LOGUJE, SLANJE POZIVA ZA LOGOVANJA, RESETOVANJE SIFRE ITD
-- IDEJA JE KAD SE ULOGUJE NA OSNOVU StaffAccess IMA PRISTUP SVI PODACI U ISTOM PRIKAZI
-- NPR SVI UGOVORI, JEDINICE, PARTERI NA ISTOM PRIKAZU / NARAVNO KOJI SU TIP SZ

CREATE TABLE [Staff]
 (
	[StaffId]			Long Integer, 
	[UserName]				Text (255),  -- email
	[Password]				Text (255), 
	[PreferredLanguage]		Text (50), 
	[LastIP]				Text (255)
	[LastLoginTimeStamp]	Date, 
	[IsActive]				Boolean,
);

CREATE TABLE [StaffAccess]
(
	[Id] 					Long Integer, 	
	[StaffId] 				Long Integer, 
	[CompanyId] 			Long Integer,
	[StaffRole]				Long Integer,
);


CREATE TABLE [BuildingEntrance]
 (
	[Id]				Long Integer NOT NULL, 
	[CompanyId]			Long Integer, -- FK Company.Id
	[BuildingName]		Text (255), 		-- Sole
	[EntranceName]		Text (255), 		-- Sole 1
	[AdressId]			Long Integer, 		-- FK to Adress
	[BuildingLabel]		Text (255), 		-- B1 * oznaka objekta u RGZ
	[Description]		Text (255), 		-- 
	[SortIndex]			Long Integer        --
);

-- ok
CREATE TABLE [BankAccount]
 (
	[BankAccountId]			Long Integer, 
	[AccountNumber]			Text (50), 
	[IsActive]				Long Integer, 
	[PartnerId]				Long Integer, 
	[CompanyId]				Long Integer,   -- only for company account to proces in import bank statments
	[SortIndex]				Long Integer,
	[Currency]				Text (3), -- on new get def value from settings(CurrencyDef) - RSD
);



-- TABLE virmaN ŠTAMPA, ILI ELEKTRONSKI NALOG

CREATE TABLE [PaymentOrder]
 (
	[Id]				Long Integer, 
	[TemplateTitle]		Text (50), 
	[PayerName]			Text (255), 
	[PaymentPurpose]	Text (255), 
	[RecipientName]		Text (255), 
	[PaymentCode]		Integer, 
	[Currency]			Text (3), 
	[Amount]			Decimal(18,2), 
	[PayerAccountNumber]	Text (50), 
	[PayerModelNumber]		Integer, 
	[PayerPaymentReference]	Text (50), 
	[RecipientAccountNumber]Text (50), 
	[RecipientModelNumber]	Integer, 
	[RecipientPaymentReference]	Text (50), 
	[Place]					Text (50), 
	[Date]					DateTime, 
	[ValueDate]				DateTime, 
	[IsUrgent]				Boolean NOT NULL, 
	[PaymentOrderTypeId]	Long Integer,  -- FK ShortList: TableName: PaymentOrderType

	[CreatedTimestamp]		DateTime, 	
	[IsFavorite]			Boolean NOT NULL,
	[IsArchived]			Boolean NOT NULL
);


-- kamata

CREATE TABLE [InterestRate]
(
	[Id]			Long Integer,
	[Date]			Date,
	[Rate]			Decimal(18,4) NOT NULL, -- 13,7500
	[TimeCode]		Text(1) -- M / G
)

-- ZK - PenaltyInterestStaging POTPUNO ISTA TABELA KAO GK lADGER

-- KamatniList
CREATE TABLE [InterestStatement]
 (
	[Id]				Long Integer, 
	[CompanyId]			Long Integer, 
	[Account]			Text (50),  -- FK ChartOfAccounts.AccountCode
	[Date]				DateTime, 
	[Amount]			Decimal(18,2), 
	[Balance]			Decimal(18,2), 
	[Days]				Long Integer, 
	[Rate]				Decimal(18,4),  -- InterestRate.Rate
	[Coefficient]		Decimal(18,8),  -- calc  InterestRate.Rate za G: (brojDana / brojdanaugodini) * stopaRate ... / 100
	[Interest]			Decimal(18,2),  -- iznos kamate
	[PartnerAccountId]	Long Integer,  -- FK PartnerAccount.Id
	[SubAccountId]		Text (10),  -- FK SubAccount.Id
	[InvoiceBatchId]	Long Integer
);



CREATE TABLE [BankStatementPostingTemplate]
 (
	[Id]				Long Integer, 
	[CompanyId]			Long Integer,  -- if null its for all
	[ParentId]			Long Integer, 
	[TemplateName]		Text (255), 
	[FieldName]			Text (255), 
	[FieldValue]		Text (255), 
	[Function]			Text (255), 
	[SetPartnerAccountId]	Long Integer, -- FK to PartnerAccount.Id
	[SetSubAccountId]		Text (10), -- FK to SubAccount.Id
	[SetAccountCode]		Text(10), -- FK to ChartOfAccounts.AccountCode
	[SortIndex]			Long Integer, 
	[IsActive]			Boolean,
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



-- OVO NECEMO KORISTITI ZA SAD, KNIŽENJE KROZ BACK

CREATE TABLE [PostingScheme]
 (
	[Id]			Long Integer, 
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
	[SortIndex]			Long Integer, 
	[JournalEntryDescription]			Text (255), 
	[SourceSql]			Memo/Hyperlink (255), 
	[SourceSqlValue]			Text (255)
);



CREATE TABLE [AnalysisReportDefinition]
 (
	[Id]			Long Integer, 
	[DataGroup]			Text (255), 
	[Name]				Text (255), 
	[IsCriticalGroup]	Boolean
	[QueryName]			Text (255), 
	[QuerySql]			Text(Max),
	[ReportName]		Text (50), 
	[StatWhereCaption]	Text (50), 
	[StatWhereComboSql]	Text(Max),
	[StatSqlName]		Text (50), 
	[ExecuteQueryDefName]			Text (255) -- execute SQL before run
	[ExecuteQueryDefSQL]			Text (Max)
	
	[AutoRunAll]		Long Integer, 
	[SortIndex]			Long Integer, 

	[LinkCreationTag]		Text (255), -- used to open and show data
	[LinkFormName]			Text (255), 
	[LinkOpenArgs]			Text (255), 
	
	-- after runing this is last info
	[LastRunAt]			DateTime, 
	[RowCount]			Long Integer, 

	[AdminAlert]		Boolean, -- send email to root as critiacl alert
	[Description]		Text (Max), 
	[IsActive]			Boolean,
	
);

CREATE TABLE [ReportDefinition]
 (
	[Id]			Long Integer, 
	[Name]			Text (255), 
	[Datasheet]			Text (255), 
	[Title]			Text (255), 
	[SortIndex]			Long Integer, 
	[FilterComboSql]	Text(Max), 
	[FilterComboCaption]	Text (255), 
);

CREATE TABLE [ReportDefinitionDetail]
 (
	[Id]			Long Integer, 
	[ReportDefinitionId]			Long Integer, 
	[QuerySql]			Text(Max), 
	[QueryName]			Text (255), 
	[SortIndex]			Long Integer, 
	[Function]			Text (255)
);

CREATE TABLE [ReportDefinitionButtons]
 (
	[Id]					Long Integer, 
	[ReportDefinitionId]	Long Integer, 
	[Caption]			Text (255), 
	[Function]			Text (255), 
	[FunctionTypeId]	Long Integer,  -- ShortList TableName: ReportFunctionType
	[SortIndex]			Long Integer, 
	
);



```
