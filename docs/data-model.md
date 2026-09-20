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
-- EventAction former message
-- ActionType CRUD - enum?
CREATE TABLE [AuditLog]
 (
	[Id]			Long Integer, 
	[StaffId]			Long Integer, 
	[TimeStamp]			DateTime, 
	[ItemId]			Long Integer, 
	[ActionType]			Text (20),  
	[EventAction]			Text (255), 
	[ActionType]			Text (50)
);

-- Translations.LanguageCode FK - Languages.Code PK 

CREATE TABLE [Translations]
 (
	[Id]				Long Integer,
	[LanguageCode]		Text (10), 
	[ResourceKey]		text(100),
	[ResourceId]		Long Integer,  
	[Translation]		Text (max),
);

CREATE TABLE [Languages]
 (
	[Code]			Text (10) NOT NULL, 
	[Name]			Text (50), 
	[IsActive]			Boolean NOT NULL, 
	[IsDefault]			Boolean NOT NULL
	[SortIndex]			Integer
);


CREATE TABLE [BenefitGroup]
 (
	[BenefitGroupId]			Long Integer, 
	[Name]			Text (255), 
	[PrintLabel]			Text (255)
);

CREATE TABLE [Benefit]
 (
	[BenefitId]			Long Integer, 
	[UnitId]			Long Integer, 
	[PartnerAccountingId]			Long Integer, 
	[PeriodYyMm]			Text (255), 
	[IsUsed]			Long Integer, 
	[EntryDate]			DateTime, 
	[UsedDate]			DateTime, 
	[InvoiceId]			Long Integer, 
	[BenefitGroupId]			Long Integer, 
	[CancelledInvoiceId]			Long Integer
);

-- da li da ja brisem Supplier nema potrebe, 
-- InvoiceMonth ovo je bolje InvoiceYYMM jer je unos marker 2609 - 4 cifre uvek
-- CollectionPriority
-- PostingCode
-- LegacyTempPrevId - BRISI
-- SequenceNumber
-- PostedInvoiceAmount
-- nNote / InvoiceDescription - zasto dva?

CREATE TABLE [SupplierInvoice]
 (
	[SupplierInvoiceId]			Long Integer, 
	[CompanyId]			Long Integer, 
	[InvoiceNumber]			Long Integer, 
	[InvoiceCaption]			Text (255), 
	[Note]			Text (255), 
	[Supplier]			Text (50), 
	[SupplierAccountPartnerAccountingId]			Long Integer, 
	[CalculationTypeId]			Long Integer, 
	[InvoiceMonth]			Text (50), 
	[InvoiceAmountEur]			Double, 
	[InvoiceAmountRsd]			Currency, 
	[AmountByCoefficientEur]			Double, 
	[AmountByCoefficientRsd]			Currency, 
	[CollectionPriority]			Long Integer, 
	[PostingCode]			Text (6), 
	[LegacyTempPrevId]			Long Integer, 
	[PostingAccount]			Text (255), 
	[DocumentTypeId]			Long Integer, 
	[ExtraordinaryInvoiceMarker]			Text (255), 
	[InvoiceNameFunction]			Text (255), 
	[SequenceNumber]			Text (50), 
	[PostedInvoiceAmount]			Currency, 
	[InvoiceDate]			DateTime, 
	[PostingDate]			DateTime, 
	[PaymentDate]			DateTime, 
	[InvoiceDescription]			Text (255), 
	[PaymentReference]			Text (255), 
	[PreviousSupplierInvoiceId]			Long Integer, 
	[NewSupplierInvoiceId]			Long Integer, 
	[JournalEntryId]			Long Integer, 
	[Vat]			Long Integer, 
	[ClosesAccount]			Text (255)
);

CREATE TABLE [SupplierInvoiceUnitType]
 (
	[Id]			Long Integer, 
	[SupplierInvoiceId]			Long Integer, 
	[UnitTypeId]			Long Integer
);

CREATE TABLE [Attachment]
 (
	[AttachmentId]			Long Integer, 
	[DocumentTypeId]			Long Integer, 
	[ReferenceItemId]			Long Integer, 
	[FileNameSuffix]			Text (255), 
	[FileName]			Text (255), 
	[RelativePath]			Text (255), 
	[DateAdded]			DateTime, 
	[Description]			Text (255), 
	[FileExtension]			Text (255), 
	[SourceTable]			Text (255), 
	[Registrar]			Text (4)
);

CREATE TABLE [LedgerEntry]
 (
	[LedgerEntryId]			Long Integer, 
	[JournalEntryNumber]			Double, 
	[Account]			Text (50), 
	[Date]			DateTime, 
	[DebitAmount]			Double, 
	[CreditAmount]			Double, 
	[LineTypeId]			Double, 
	[DocumentRef]			Text (50), 
	[CompanyId]			Long Integer, 
	[PartnerAccountingId]			Long Integer, 
	[BankStatementLineId]			Long Integer, 
	[Note]			Text (255), 
	[Parameters]			Text (25), 
	[Description]			Text (255), 
	[AccountCode]			Long Integer, 
	[DueDate]			DateTime, 
	[PostingCode]			Text (6), 
	[SupplierInvoiceId]			Long Integer, 
	[InvoiceId]			Long Integer, 
	[Priority]			Long Integer, 
	[PostingTypeCode]			Long Integer, 
	[InboundInvoiceId]			Long Integer, 
	[SubAccount]			Text (255), 
	[AccountingDocumentId]			Long Integer
);

CREATE TABLE [LedgerEntryTemp]
 (
	[LedgerEntryId]			Long Integer, 
	[JournalEntryNumber]			Double, 
	[Account]			Text (50), 
	[Date]			DateTime, 
	[DebitAmount]			Double, 
	[CreditAmount]			Double, 
	[LineTypeId]			Double, 
	[DocumentRef]			Text (50), 
	[CompanyId]			Long Integer, 
	[PartnerAccountingId]			Long Integer, 
	[BankStatementLineId]			Long Integer, 
	[Note]			Text (255), 
	[Parameters]			Text (25), 
	[Description]			Long Integer, 
	[AccountCode]			Long Integer, 
	[DueDate]			DateTime, 
	[FromLedgerEntryId]			Long Integer, 
	[PostingCode]			Text (6), 
	[SupplierInvoiceId]			Long Integer, 
	[InvoiceId]			Long Integer, 
	[Priority]			Long Integer, 
	[PostingTypeCode]			Long Integer, 
	[InboundInvoiceId]			Long Integer, 
	[SubAccount]			Text (255), 
	[AccountingDocumentId]			Text (255)
);

CREATE TABLE [FiscalYear]
 (
	[CompanyId]			Long Integer NOT NULL, 
	[Year]			Long Integer NOT NULL, 
	[StartDate]			DateTime, 
	[EndDate]			DateTime, 
	[IsArchived]			Long Integer, 
	[Display]			Text (255), 
	[Folder]			Text (255), 
	[FileName]			Text (255), 
	[IsCurrent]			Long Integer
);

CREATE TABLE [ReminderBatch]
 (
	[ReminderBatchId]			Long Integer, 
	[Title]			Text (50), 
	[Date]			DateTime, 
	[MinUnpaidInvoiceCount]			Long Integer, 
	[DebtTolerance]			Long Integer, 
	[DebtToleranceByMonth]			Long Integer, 
	[ReminderTemplateId]			Long Integer, 
	[ReminderTypeId]			Long Integer, 
	[DebitPeriodDate]			DateTime, 
	[CreditPeriodDate]			DateTime, 
	[CompanyId]			Long Integer, 
	[InvoiceBatchId]			Long Integer, 
	[Label]			Text (255), 
	[ReminderTemplateText]			Text (255), 
	[RemittanceInfo]			Text (255)
);

CREATE TABLE [InvoiceBatch]
 (
	[InvoiceBatchId]			Long Integer, 
	[Code]			Text (50), 
	[Label]			Text (50), 
	[Month]			Text (50), 
	[Year]			Text (50), 
	[Place]			Text (50), 
	[IssueDate]			DateTime, 
	[ServiceDate]			Text (50), 
	[TransactionDate]			DateTime, 
	[ValueDate]			DateTime, 
	[ExchangeRateNbs]			Currency, 
	[CompanyId]			Long Integer, 
	[PostingJournalEntryId]			Long Integer, 
	[SystemDate]			DateTime, 
	[StaffId]			Long Integer, 
	[ExtraordinaryInvoiceMarker]			Text (255), 
	[InvoiceKind]			Text (255), 
	[BalanceAsOfDate]			DateTime, 
	[PreviousValueDate]			DateTime, 
	[IsInterestCalculated]			Long Integer
);

CREATE TABLE [BankStatement]
 (
	[BankStatementId]			Long Integer, 
	[StatementNumber]			Long Integer, 
	[StatementSuffix]			Text (50), 
	[CompanyId]			Long Integer, 
	[Date]			DateTime, 
	[PreviousBalance]			Currency, 
	[NewBalance]			Currency, 
	[Debit]			Currency, 
	[Credit]			Currency, 
	[DebitJournalEntryId]			Long Integer, 
	[CreditJournalEntryId]			Long Integer, 
	[Note]			Text (50), 
	[JournalEntryId]			Long Integer, 
	[IsUnposted]			Boolean NOT NULL
);

CREATE TABLE [BankStatementLine]
 (
	[Id]			Long Integer, 
	[BankStatementId]			Long Integer, 
	[CompanyId]			Long Integer, 
	[LineNumber]			Long Integer, 
	[JournalEntryLineNumber]			Long Integer, 
	[PayerRecipientName]			Text (255), 
	[BankAccountNumber]			Text (50), 
	[Origin]			Text (50), 
	[ExecutionDate]			DateTime, 
	[Debit]			Currency, 
	[Credit]			Currency, 
	[RemittanceInfo]			Text (255), 
	[Code]			Long Integer, 
	[PaymentReference]			Text (50), 
	[PaymentReferenceOut]			Text (50), 
	[PartnerAccountingId]			Long Integer, 
	[IsPosted]			Boolean NOT NULL, 
	[IsIgnored]			Boolean NOT NULL, 
	[IsMatched]			Boolean NOT NULL
);

CREATE TABLE [AccountingDocument]
 (
	[AttachmentId]			Long Integer, 
	[CompanyId]			Long Integer, 
	[AccountingDocumentIdRef]			Long Integer, 
	[DocumentTypeId]			Long Integer, 
	[Amount]			Currency, 
	[PostingJournalEntryId]			Long Integer, 
	[Date]			DateTime, 
	[DocumentName]			Text (255), 
	[DocumentDescription]			Text (255), 
	[SubAccount]			Long Integer, 
	[TypeGroupId]			Long Integer, 
	[RefFromAccountingDocumentId]			Long Integer, 
	[ReferenceCode]			Text (30)
);

CREATE TABLE [Account]
 (
	[Account]			Long Integer NOT NULL, 
	[Description]			Text (50), 
	[PrintText]			Text (255)
);

CREATE TABLE [ChartOfAccounts]
 (
	[Account]			Text (255) NOT NULL, 
	[ShortName]			Text (255), 
	[Name]			Text (255), 
	[ParentAccount]			Text (255), 
	[Level]			Long Integer, 
	[Sign]			Text (255), 
	[IsActive]			Long Integer
);

CREATE TABLE [Partner]
 (
	[PartnerId]			Long Integer NOT NULL, 
	[Name]			Text (100), 
	[PostalCode]			Text (50), 
	[Address]			Text (50), 
	[RegistrationNumber]			Text (50), 
	[TaxId]			Text (50), 
	[ContractAddress]			Text (255), 
	[Phone]			Text (50), 
	[Email]			Text (100), 
	[Note]			Text (255), 
	[PrimaryBankAccountNumber]			Text (50), 
	[Website]			Text (50), 
	[CompanyId]			Long Integer, 
	[CategoryLocationId]			Long Integer, 
	[PrintName]			Text (255), 
	[LegacyShortCode]			Text (50), 
	[Account]			Text (50), 
	[ExternalAccount]			Text (255), 
	[PaymentSlipPrefix]			Text (255), 
	[LegalRepresentative]			Text (255), 
	[Type]			Long Integer, 
	[AutoSubAccount]			Text (50), 
	[GroupInvoiceTagId]			Long Integer, 
	[SkipPrintInvoiceGroup]			Long Integer, 
	[IsVatPayer]			Long Integer, 
	[Jbjks]			Text (255), 
	[PartnerCity]			Text (255), 
	[CountryCode]			Text (255), 
	[LegacyMasterId]			Long Integer, 
	[DeliveryLocation]			Text (255), 
	[DeliveryUnitTypeCode]			Long Integer, 
	[PrintInvoiceMandatory]			Long Integer, 
	[SendToPostOffice]			Long Integer, 
	[Language]			Text (255), 
	[ExtendedNote]			Memo/Hyperlink (255)
);

CREATE TABLE [ExchangeRate]
 (
	[ExchangeRateId]			Long Integer, 
	[Rate]			Currency, 
	[RateDateFrom]			DateTime, 
	[SchoolYearLegacy]			Long Integer, 
	[ReferencePriceLegacy]			Long Integer, 
	[EntryDateLegacy]			DateTime
);

CREATE TABLE [PartnerEmail]
 (
	[PartnerEmailId]			Long Integer, 
	[PartnerId]			Long Integer, 
	[Email]			Text (255), 
	[SortOrder]			Long Integer, 
	[LoginAppMaster]			Long Integer, 
	[LoginAppView]			Long Integer, 
	[SendInvoiceByEmail]			Long Integer
);

CREATE TABLE [SentEmail]
 (
	[SentEmailId]			Long Integer, 
	[Subject]			Text (255), 
	[ToAddress]			Text (255), 
	[Cc]			Text (255), 
	[Bcc]			Text (255), 
	[Body]			Memo/Hyperlink (255), 
	[BodyHtml]			Memo/Hyperlink (255), 
	[DateCreated]			DateTime, 
	[DateSent]			DateTime, 
	[Archive]			Boolean NOT NULL, 
	[ErrorDescription]			Memo/Hyperlink (255), 
	[ErrorStatus]			Text (255)
);

CREATE TABLE [SentEmailAttachment]
 (
	[SentEmailAttachmentId]			Long Integer, 
	[SentEmailId]			Long Integer, 
	[AttachmentFilePath]			Text (255), 
	[AttachmentFilePath2]			Text (255)
);

CREATE TABLE [JournalEntry]
 (
	[LegacyJournalEntryNumber]			Double NOT NULL, 
	[Date]			DateTime, 
	[Balance]			Currency, 
	[Note]			Text (255), 
	[Reserved]			Text (50), 
	[Description]			Text (50), 
	[CompanyId]			Long Integer, 
	[AdditionalNotes]			Text (255), 
	[JournalEntryTypeId]			Long Integer
);

CREATE TABLE [CategoryLocation]
 (
	[CategoryLocationId]			Long Integer NOT NULL, 
	[Name]			Text (50)
);

CREATE TABLE [Note]
 (
	[Id]			Long Integer, 
	[Date]			Text (50), 
	[UserName]			Text (50), 
	[Text]			Memo/Hyperlink (255)
);

CREATE TABLE [Unit]
 (
	[UnitId]			Long Integer, 
	[CompanyId]			Long Integer, 
	[Name]			Text (255), 
	[PartnerId]			Long Integer NOT NULL, 
	[UnitTypeId]			Long Integer, 
	[Status]			Long Integer, 
	[IoLegacy]			Double, 
	[InvestmentMaintenanceReserve]			Double, 
	[InvestmentMaintenanceTitle]			Text (50), 
	[PreviousArea]			Double, 
	[StatusChangeNote]			Text (50), 
	[Coefficient]			Double, 
	[Total]			Currency, 
	[Entrance]			Text (50), 
	[Category]			Text (5), 
	[Address]			Text (50), 
	[Note]			Text (255), 
	[MailingName]			Text (50), 
	[MailingAddress]			Text (50), 
	[MailingPostalCode]			Text (50), 
	[MailingTaxId]			Text (50), 
	[LegacyPayerPartnerId]			Long Integer, 
	[LegacyPayerPartnerId2]			Long Integer, 
	[GarageSpotNumber]			Long Integer, 
	[K1]			Double, 
	[K2]			Double, 
	[K3]			Double, 
	[K4]			Double, 
	[K5]			Double, 
	[MailingLegacyCode]			Text (50), 
	[Area]			Double, 
	[ResidentCount]			Long Integer, 
	[UnitTypeCode]			Long Integer, 
	[LegacyOwnerPartnerId]			Long Integer, 
	[LegacyTenantPartnerId]			Long Integer, 
	[LegacyGroupInvoiceId]			Long Integer, 
	[UnitTypeShortCode]			Text (255), 
	[NetArea]			Double, 
	[Terrace]			Double, 
	[NetAreaWithTerrace]			Double, 
	[HandOverDate]			DateTime, 
	[FloorNumber]			Long Integer, 
	[FloorText]			Text (255)
);

CREATE TABLE [Reminder]
 (
	[ReminderId]			Long Integer, 
	[ReminderBatchId]			Long Integer, 
	[PartnerAccountingId]			Long Integer, 
	[UnpaidInvoiceCount]			Long Integer, 
	[Debt]			Double, 
	[InvoiceText]			Text (255), 
	[IsActive]			Long Integer, 
	[LineItemSum]			Double, 
	[PaymentReference]			Text (255), 
	[Costs]			Long Integer, 
	[Total]			Long Integer
);

CREATE TABLE [ReminderLine]
 (
	[ReminderLineId]			Long Integer, 
	[ReminderId]			Long Integer, 
	[ReminderBatchId]			Long Integer, 
	[PartnerAccountingId]			Long Integer, 
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

CREATE TABLE [ReminderTemplate]
 (
	[ReminderTemplateId]			Long Integer, 
	[ReminderTypeId]			Long Integer, 
	[ReportName]			Text (255), 
	[Title]			Text (255), 
	[ReportField01]			Text (255), 
	[ReportField02]			Text (255), 
	[ReportField03]			Memo/Hyperlink (255), 
	[ReportField04]			Memo/Hyperlink (255), 
	[ReportField05]			Memo/Hyperlink (255), 
	[ReportField06]			Memo/Hyperlink (255), 
	[ReportField07]			Text (255), 
	[ReportField08]			Text (255), 
	[ReportField09]			Text (255)
);

CREATE TABLE [SelectionBasket]
 (
	[SelectionBasketId]			Long Integer, 
	[TargetId]			Long Integer, 
	[TypeIndex]			Long Integer
);

CREATE TABLE [Events]
 (
	[EventId]			Long Integer, 
	[DateOfRequest]			DateTime, 
	[DateOfExecution]			DateTime, 
	[PartnerId]			Long Integer, 
	[UnitId]			Long Integer, 
	[Description]			Text (255), 
	[PreviousValue]			Text (255), 
	[NewValue]			Text (255), 
	[FieldsRelated]			Text (255), 
	[RequestType]			Text (255), 
	[RequestBy]			Text (255), 
	[RequestThrough]			Text (255)
);

CREATE TABLE [Invoice]
 (
	[InvoiceId]			Long Integer, 
	[SequenceNumber]			Text (20), 
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

CREATE TABLE [InboundInvoice]
 (
	[InboundInvoiceId]			Long Integer, 
	[PartnerId]			Long Integer, 
	[CompanyId]			Long Integer, 
	[InboundInvoiceNumber]			Text (255), 
	[InvoiceDate]			DateTime, 
	[PostingDate]			DateTime, 
	[PaymentDate]			DateTime, 
	[SubAccount]			Text (255), 
	[InvoiceAmount]			Currency
);

CREATE TABLE [InvoiceUnit]
 (
	[InvoiceId]			Long Integer, 
	[UnitId]			Long Integer
);

CREATE TABLE [InvoiceLine]
 (
	[InvoiceLineId]			Long Integer, 
	[InvoiceId]			Long Integer, 
	[InvoiceBatchId]			Long Integer, 
	[PartnerId]			Long Integer, 
	[CompanyId]			Long Integer, 
	[SupplierInvoiceId]			Long Integer, 
	[Name]			Text (255), 
	[CalculationTypeId]			Long Integer, 
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
	[SupplierAccountPartnerAccountingId]			Long Integer, 
	[InvoiceAmount]			Double, 
	[K1xK2]			Double, 
	[K2xK3]			Double, 
	[K2xK4]			Double, 
	[K2xK5]			Double, 
	[UnitOfMeasure]			Text (255), 
	[UnitId]			Long Integer, 
	[QuantityAlt]			Double
);

CREATE TABLE [InvoiceLineBenefitArchive]
 (
	[InvoiceLineId]			Long Integer NOT NULL, 
	[InvoiceId]			Long Integer, 
	[InvoiceBatchId]			Long Integer, 
	[PartnerId]			Long Integer, 
	[CompanyId]			Long Integer, 
	[SupplierInvoiceId]			Long Integer, 
	[Name]			Text (255), 
	[CalculationTypeId]			Long Integer, 
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
	[SupplierAccountPartnerAccountingId]			Long Integer, 
	[InvoiceAmount]			Double, 
	[K1xK2]			Double, 
	[K2xK3]			Double, 
	[K2xK4]			Double, 
	[K2xK5]			Double, 
	[UnitOfMeasure]			Text (255), 
	[UnitId]			Long Integer
);

CREATE TABLE [Setting]
 (
	[SettingId]			Long Integer, 
	[SettingName]			Text (255), 
	[SettingValue]			Text (255), 
	[Description]			Text (255), 
	[Category]			Text (50), 
	[ModuleFormField]			Text (50), 
	[DefaultValue]			Text (255), 
	[FilterUserId]			Long Integer, 
	[FilterComputerName]			Text (255), 
	[FilterCustom1Num]			Long Integer, 
	[FilterCustom2Num]			Long Integer, 
	[FilterCustom3Num]			Long Integer, 
	[FilterCustom1Text]			Text (255), 
	[FilterCustom2Text]			Text (255), 
	[FilterCustom3Text]			Text (255), 
	[SettingValueLong]			Memo/Hyperlink (255)
);

CREATE TABLE [EmailSetting]
 (
	[EmailSettingId]			Long Integer NOT NULL, 
	[SettingName]			Text (255), 
	[SettingValue]			Memo/Hyperlink (255), 
	[Category]			Text (255), 
	[Description]			Text (255), 
	[CompanyId]			Long Integer
);

CREATE TABLE [EPaymentOrderSettingGroup]
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
	[CompanyId]			Long Integer NOT NULL, 
	[Name]			Text (255), 
	[Building]			Text (50), 
	[Address]			Text (50), 
	[Municipality]			Text (255), 
	[PostalCode]			Text (50), 
	[RepresentativeId]			Long Integer, 
	[TaxId]			Long Integer, 
	[PrimaryBankAccountNumber]			Text (50), 
	[RegistrationNumber]			Text (50), 
	[Note]			Text (255), 
	[SortOrder]			Long Integer, 
	[PrintName]			Text (255), 
	[Folder]			Text (50), 
	[PaymentSlipTypeId]			Long Integer, 
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

CREATE TABLE [StaffTemp]
 (
	[StaffId]			Long Integer, 
	[UserName]			Text (50), 
	[Level]			Text (50), 
	[PreferredLanguage]			Text (50), 
	[AccessRestriction]			Text (255), 
	[LastComputerName]			Text (255)
);

CREATE TABLE [CompanyBuilding]
 (
	[Id]			Long Integer NOT NULL, 
	[CompanyId]			Long Integer, 
	[Unit]			Text (255)
);

CREATE TABLE [BuildingEntrance]
 (
	[BuildingEntranceId]			Long Integer NOT NULL, 
	[CompanyId]			Long Integer, 
	[Entrance]			Text (255), 
	[Building]			Text (255), 
	[Address]			Text (255), 
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

CREATE TABLE [CodeList]
 (
	[SortIndex]			Long Integer NOT NULL, 
	[Category]			Text (255) NOT NULL, 
	[Caption]			Text (255), 
	[ShortName]			Text (255), 
	[Description]			Text (255), 
	[SortOrder]			Long Integer, 
	[GroupCode]			Long Integer
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

CREATE TABLE [BankAccount]
 (
	[BankAccountId]			Long Integer, 
	[PrimaryBankAccountNumber]			Text (255), 
	[IsActive]			Long Integer, 
	[SortOrder]			Long Integer, 
	[PartnerId]			Long Integer, 
	[CompanyId]			Long Integer, 
	[ManagerId]			Long Integer
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

CREATE TABLE [CalculationType]
 (
	[CalculationTypeId]			Long Integer NOT NULL, 
	[Name]			Text (100), 
	[SupplierInvoiceAmountLabel]			Text (100), 
	[Note]			Text (255), 
	[AmountLabel]			Text (255), 
	[QuantityLabel]			Text (255), 
	[UnitOfMeasure]			Text (255), 
	[UnitOfMeasureIndex]			Long Integer
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

CREATE TABLE [VersionHistory]
 (
	[VersionDate]			DateTime, 
	[VersionNumber]			Text (50), 
	[ShortDescription]			Text (255), 
	[FullDescription]			Memo/Hyperlink (255)
);

CREATE TABLE [PaymentOrder]
 (
	[PaymentOrderId]			Long Integer, 
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

CREATE TABLE [InvoiceLineCostCache]
 (
	[InvoiceLineId]			Long Integer, 
	[InvoiceId]			Long Integer, 
	[InvoiceBatchId]			Long Integer, 
	[PartnerId]			Long Integer, 
	[CompanyId]			Long Integer, 
	[Total]			Currency
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

CREATE TABLE [SwitchboardMenuItem]
 (
	[SwitchboardMenuItemId]			Long Integer NOT NULL, 
	[ItemNumber]			Integer NOT NULL, 
	[ItemText]			Text (255), 
	[Command]			Integer, 
	[Argument]			Text (255), 
	[FormLevel]			Text (50)
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

CREATE TABLE [Contract]
 (
	[LegacyRowId]			Long Integer, 
	[Company municipality]			Text (255), 
	[Company legacy id]			Double, 
	[Seller (Company)]			Text (255), 
	[Company address]			Text (255), 
	[Company tax id]			Double, 
	[Postal code]			Text (255), 
	[Company bank account]			Text (255), 
	[Company registration number]			Double, 
	[Invoice number]			Text (255), 
	[Place of invoice issue]			Text (255), 
	[Invoice issue date]			Text (255), 
	[Unit number]			Text (255), 
	[Service date]			Text (255), 
	[Unit]			Text (255), 
	[Partner]			Text (255), 
	[Field16Legacy]			Text (255), 
	[Partner address]			Text (255), 
	[Registration number]			Double, 
	[Tax id name]			Text (255), 
	[Tax id]			Text (255), 
	[Quantity in sqm]			Double, 
	[Unit of measure]			Text (255), 
	[Calculation unit coefficient]			Double, 
	[Monthly building fund per unit (Dec 2019 - Feb 2822019)]			Double, 
	[Control value]			Double, 
	[Coefficient]			Double, 
	[Common area insurance]			Double, 
	[Monthly management fee]			Double, 
	[Regular monthly passenger elevator service]			Double, 
	[Regular monthly car elevator service]			Double, 
	[Extraordinary car elevator service]			Double, 
	[Building hygiene maintenance with caretaker]			Double, 
	[Garage hygiene maintenance]			Double, 
	[Building hygiene maintenance]			Double, 
	[Technical maintenance]			Double, 
	[Extraordinary service]			Double, 
	[Field37Legacy]			Double, 
	[Total]			Double, 
	[Balance carried forward as of]			Text (255), 
	[Balance carried forward amount (Maxi manager)]			Text (255), 
	[Id]			Text (255), 
	[Balance carried forward as of 2018-12-31 (Company)]			Text (255), 
	[Total debt for 2018]			Text (255), 
	[Payment deadline]			Text (255), 
	[Model and payment reference]			Text (255), 
	[Payment purpose]			Text (255), 
	[Value date]			Text (255), 
	[Please settle the debt relating to the period up to (note]			Text (255), 
	[Payment reference for debt as of 2018-12-31]			Text (255), 
	[Note]			Text (255), 
	[Field51Legacy]			Text (255), 
	[PartnerName]			Text (255)
);


```

## SZAPP.mdb — 27 local/helper tables (front-end, temp/LedgerEntry-backup/switchboard tables)
```sql
-- ----------------------------------------------------------
-- MDB Tools - A library for reading MS Access database files
-- Copyright (C) 2000-2011 Brian Bruns and others.
-- Files in libmdb are licensed under LGPL and the utilities under
-- the GPL, see COPYING.LIB and COPYING files respectively.
-- Check out http://mdbtools.sourceforge.net
-- ----------------------------------------------------------

-- That file uses encoding UTF-8

CREATE TABLE [BenefitUsageUpdate]
 (
	[Id]			Long Integer, 
	[Date]			DateTime, 
	[UnitId]			Text (255), 
	[PeriodYyMm]			Long Integer, 
	[MonthCount]			Long Integer
);

CREATE TABLE [LedgerEntryBackupPrk]
 (
	[LedgerEntryId]			Long Integer, 
	[JournalEntryNumber]			Double, 
	[Account]			Text (50), 
	[Date]			DateTime, 
	[DebitAmount]			Double, 
	[CreditAmount]			Double, 
	[LineTypeId]			Double, 
	[DocumentRef]			Text (50), 
	[CompanyId]			Long Integer, 
	[PartnerAccountingId]			Long Integer, 
	[BankStatementLineId]			Long Integer, 
	[Note]			Text (255), 
	[Parameters]			Text (25), 
	[Description]			Text (255), 
	[AccountCode]			Long Integer, 
	[DueDate]			DateTime, 
	[PostingCode]			Text (6), 
	[SupplierInvoiceId]			Long Integer, 
	[InvoiceId]			Long Integer, 
	[Priority]			Long Integer, 
	[PostingTypeCode]			Long Integer, 
	[InboundInvoiceId]			Long Integer, 
	[SubAccount]			Text (255)
);

CREATE TABLE [LedgerEntryTemp]
 (
	[LedgerEntryId]			Long Integer, 
	[JournalEntryNumber]			Double, 
	[Account]			Text (50), 
	[Date]			DateTime, 
	[CreditAmount]			Double, 
	[DebitAmount]			Double, 
	[LineTypeId]			Double, 
	[DocumentRef]			Text (50), 
	[CompanyId]			Long Integer, 
	[PartnerAccountingId]			Long Integer, 
	[BankStatementLineId]			Long Integer, 
	[Note]			Text (255), 
	[Parameters]			Text (25), 
	[Description]			Text (255), 
	[AccountCode]			Long Integer, 
	[DueDate]			DateTime, 
	[FromLedgerEntryId]			Long Integer, 
	[PostingCode]			Text (6), 
	[SupplierInvoiceId]			Long Integer, 
	[InvoiceId]			Long Integer, 
	[Priority]			Long Integer, 
	[PostingTypeCode]			Long Integer, 
	[InboundInvoiceId]			Long Integer, 
	[SubAccount]			Text (255)
);

CREATE TABLE [BenefitImport]
 (
	[BenefitImportRow]			Text (255) NOT NULL
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

CREATE TABLE [LedgerEntry_Backup_20250531_BeforeManagementSplit]
 (
	[LedgerEntryId]			Long Integer NOT NULL, 
	[JournalEntryNumber]			Double, 
	[Account]			Text (50), 
	[Date]			DateTime, 
	[DebitAmount]			Double, 
	[CreditAmount]			Double, 
	[LineTypeId]			Double, 
	[DocumentRef]			Text (50), 
	[CompanyId]			Long Integer, 
	[PartnerAccountingId]			Long Integer, 
	[BankStatementLineId]			Long Integer, 
	[Note]			Text (255), 
	[Parameters]			Text (25), 
	[Description]			Text (255), 
	[AccountCode]			Long Integer, 
	[DueDate]			DateTime, 
	[PostingCode]			Text (6), 
	[SupplierInvoiceId]			Long Integer, 
	[InvoiceId]			Long Integer, 
	[Priority]			Long Integer, 
	[PostingTypeCode]			Long Integer, 
	[InboundInvoiceId]			Long Integer, 
	[SubAccount]			Text (255), 
	[AccountingDocumentId]			Long Integer
);

CREATE TABLE [ReminderTemplateField]
 (
	[ReminderTemplateFieldId]			Long Integer, 
	[ReminderTemplateId]			Long Integer, 
	[KeyName]			Text (255), 
	[KeyIndex]			Long Integer, 
	[TemplateText]			Text (255)
);

CREATE TABLE [PasteErrorLog]
 (
	[SupplierInvoiceId]			Long Integer, 
	[CollectionPriority]			Long Integer, 
	[CompanyId]			Long Integer, 
	[InvoiceNumber]			Long Integer, 
	[PostingAccount]			Text (255), 
	[InvoiceName]			Text (255), 
	[Note]			Text (255), 
	[Supplier]			Text (255), 
	[SupplierAccountPartnerAccountingId]			Long Integer, 
	[CalculationTypeId]			Long Integer, 
	[InvoiceMonth]			Text (255), 
	[InvoiceAmountEur]			Double, 
	[InvoiceAmountRsd]			Currency, 
	[AmountByCoefficientEur]			Double, 
	[AmountByCoefficientRsd]			Currency, 
	[PostingCode]			Text (255), 
	[LegacyTempPrevId]			Long Integer, 
	[DocumentTypeId]			Long Integer, 
	[ExtraordinaryInvoiceMarker]			Text (255), 
	[InvoiceNameFunction]			Text (255), 
	[SequenceNumber]			Text (255), 
	[PostedInvoiceAmount]			Currency, 
	[InvoiceDate]			DateTime, 
	[PostingDate]			DateTime, 
	[PaymentDate]			DateTime, 
	[InvoiceDescription]			Text (255), 
	[PaymentReference]			Text (255), 
	[PreviousSupplierInvoiceId]			Long Integer, 
	[NewSupplierInvoiceId]			Long Integer, 
	[JournalEntryId]			Long Integer, 
	[Vat]			Long Integer, 
	[ClosesAccount]			Text (255)
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

CREATE TABLE [SelectionBasket]
 (
	[SelectionBasketId]			Long Integer, 
	[TargetId]			Long Integer, 
	[TypeIndex]			Long Integer
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

CREATE TABLE [StaffTemp]
 (
	[StaffId]			Long Integer, 
	[UserName]			Text (50), 
	[Level]			Text (50), 
	[PreferredLanguage]			Text (50), 
	[AccessRestriction]			Text (50), 
	[LastComputerName]			Text (255)
);

CREATE TABLE [InterestRate]
 (
	[Id]			Long Integer, 
	[Date]			DateTime, 
	[Rate]			Currency, 
	[Period]			Text (50)
);

CREATE TABLE [SwitchboardMenuItem]
 (
	[SwitchboardMenuItemId]			Long Integer, 
	[ItemNumber]			Integer, 
	[ItemText]			Text (255), 
	[Command]			Integer, 
	[Argument]			Text (255), 
	[FormLevel]			Text (255)
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

CREATE TABLE [StatReportDefinitionBackup]
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

CREATE TABLE [ScratchSum]
 (
	[Id]			Long Integer, 
	[Sum]			Currency
);

CREATE TABLE [StatusType]
 (
	[StatusTypeId]			Long Integer NOT NULL, 
	[Status]			Text (255), 
	[LimitTable]			Text (255), 
	[Description]			Text (255)
);

CREATE TABLE [VersionHistory]
 (
	[VersionDate]			DateTime, 
	[VersionNumber]			Text (50), 
	[ShortDescription]			Text (255), 
	[FullDescription]			Memo/Hyperlink (255)
);

CREATE TABLE [LedgerEntry_Backup_20250531_ManagementSplit]
 (
	[LedgerEntryId]			Long Integer NOT NULL, 
	[JournalEntryNumber]			Double, 
	[Account]			Text (50), 
	[Date]			DateTime, 
	[DebitAmount]			Double, 
	[CreditAmount]			Double, 
	[LineTypeId]			Double, 
	[DocumentRef]			Text (50), 
	[CompanyId]			Long Integer, 
	[PartnerAccountingId]			Long Integer, 
	[BankStatementLineId]			Long Integer, 
	[Note]			Text (255), 
	[Parameters]			Text (25), 
	[Description]			Text (255), 
	[AccountCode]			Long Integer, 
	[DueDate]			DateTime, 
	[PostingCode]			Text (6), 
	[SupplierInvoiceId]			Long Integer, 
	[InvoiceId]			Long Integer, 
	[Priority]			Long Integer, 
	[PostingTypeCode]			Long Integer, 
	[InboundInvoiceId]			Long Integer, 
	[SubAccount]			Text (255), 
	[AccountingDocumentId]			Long Integer
);

CREATE TABLE [LedgerEntrySnapshot]
 (
	[LedgerEntryId]			Long Integer, 
	[JournalEntryNumber]			Double, 
	[Account]			Text (50), 
	[Date]			DateTime, 
	[DebitAmount]			Double, 
	[CreditAmount]			Double, 
	[LineTypeId]			Double, 
	[DocumentRef]			Text (50), 
	[CompanyId]			Long Integer, 
	[PartnerAccountingId]			Long Integer, 
	[BankStatementLineId]			Long Integer, 
	[Note]			Text (255), 
	[Parameters]			Text (25), 
	[Description]			Text (255), 
	[AccountCode]			Long Integer, 
	[DueDate]			DateTime, 
	[PostingCode]			Text (6), 
	[SupplierInvoiceId]			Long Integer, 
	[InvoiceId]			Long Integer, 
	[Priority]			Long Integer, 
	[PostingTypeCode]			Long Integer, 
	[InboundInvoiceId]			Long Integer, 
	[SubAccount]			Text (255), 
	[AccountingDocumentId]			Long Integer
);

CREATE TABLE [ContactImport]
 (
	[LedgerEntryId]			Long Integer NOT NULL, 
	[SupplierInvoiceId]			Long Integer, 
	[PostingSubAccount]			Text (255)
);


```
