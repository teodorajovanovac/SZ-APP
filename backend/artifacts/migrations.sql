IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    IF SCHEMA_ID(N'core') IS NULL EXEC(N'CREATE SCHEMA [core];');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    IF SCHEMA_ID(N'ops') IS NULL EXEC(N'CREATE SCHEMA [ops];');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    IF SCHEMA_ID(N'etl') IS NULL EXEC(N'CREATE SCHEMA [etl];');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    IF SCHEMA_ID(N'finance') IS NULL EXEC(N'CREATE SCHEMA [finance];');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    IF SCHEMA_ID(N'auth') IS NULL EXEC(N'CREATE SCHEMA [auth];');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE TABLE [core].[Address] (
        [Id] int NOT NULL IDENTITY,
        [Address] nvarchar(255) NOT NULL,
        [PostalCode] varchar(20) NULL,
        [City] nvarchar(255) NOT NULL,
        [CountryCode] varchar(2) NOT NULL,
        CONSTRAINT [PK_Address] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE TABLE [etl].[EtlRun] (
        [Id] uniqueidentifier NOT NULL,
        [SourceSystem] varchar(100) NOT NULL,
        [SourceFile] nvarchar(500) NOT NULL,
        [ContentHash] varchar(64) NOT NULL,
        [Status] int NOT NULL,
        [StartedAt] datetimeoffset NOT NULL,
        [CompletedAt] datetimeoffset NULL,
        [SourceRowCount] int NOT NULL,
        [ImportedRowCount] int NOT NULL,
        [QuarantinedRowCount] int NOT NULL,
        [Error] nvarchar(max) NULL,
        CONSTRAINT [PK_EtlRun] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE TABLE [auth].[Role] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_Role] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE TABLE [core].[ShortList] (
        [Id] int NOT NULL IDENTITY,
        [TableName] varchar(100) NOT NULL,
        [Caption] nvarchar(255) NOT NULL,
        [ShortName] nvarchar(50) NULL,
        [Description] nvarchar(255) NULL,
        [IndexValue] int NOT NULL,
        [IndexSort] int NOT NULL,
        [IndexKey] varchar(50) NULL,
        CONSTRAINT [PK_ShortList] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE TABLE [auth].[Staff] (
        [Id] int NOT NULL IDENTITY,
        [PreferredLanguage] varchar(10) NOT NULL,
        [LastIp] varchar(45) NULL,
        [LastLoginAt] datetimeoffset NULL,
        [IsActive] bit NOT NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_Staff] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE TABLE [etl].[LegacyKeyMap] (
        [Id] bigint NOT NULL IDENTITY,
        [EtlRunId] uniqueidentifier NOT NULL,
        [SourceTable] varchar(128) NOT NULL,
        [SourceKey] nvarchar(255) NOT NULL,
        [TargetTable] varchar(128) NOT NULL,
        [TargetKey] nvarchar(255) NOT NULL,
        CONSTRAINT [PK_LegacyKeyMap] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_LegacyKeyMap_EtlRun_EtlRunId] FOREIGN KEY ([EtlRunId]) REFERENCES [etl].[EtlRun] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE TABLE [etl].[QuarantineRecord] (
        [Id] bigint NOT NULL IDENTITY,
        [EtlRunId] uniqueidentifier NOT NULL,
        [SourceTable] varchar(128) NOT NULL,
        [SourceKey] nvarchar(255) NULL,
        [RawJson] nvarchar(max) NOT NULL,
        [ErrorCode] varchar(100) NOT NULL,
        [ErrorMessage] nvarchar(2000) NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_QuarantineRecord] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_QuarantineRecord_EtlRun_EtlRunId] FOREIGN KEY ([EtlRunId]) REFERENCES [etl].[EtlRun] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE TABLE [auth].[RoleClaim] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] int NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_RoleClaim] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RoleClaim_Role_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [auth].[Role] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE TABLE [auth].[StaffClaim] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_StaffClaim] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_StaffClaim_Staff_UserId] FOREIGN KEY ([UserId]) REFERENCES [auth].[Staff] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE TABLE [auth].[StaffLogin] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] int NOT NULL,
        CONSTRAINT [PK_StaffLogin] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_StaffLogin_Staff_UserId] FOREIGN KEY ([UserId]) REFERENCES [auth].[Staff] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE TABLE [auth].[StaffRole] (
        [UserId] int NOT NULL,
        [RoleId] int NOT NULL,
        CONSTRAINT [PK_StaffRole] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_StaffRole_Role_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [auth].[Role] ([Id]),
        CONSTRAINT [FK_StaffRole_Staff_UserId] FOREIGN KEY ([UserId]) REFERENCES [auth].[Staff] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE TABLE [auth].[StaffToken] (
        [UserId] int NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_StaffToken] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_StaffToken_Staff_UserId] FOREIGN KEY ([UserId]) REFERENCES [auth].[Staff] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE TABLE [ops].[AuditLog] (
        [Id] bigint NOT NULL IDENTITY,
        [CompanyId] int NULL,
        [StaffId] int NULL,
        [Timestamp] datetimeoffset NOT NULL,
        [CorrelationId] varchar(64) NOT NULL,
        [EntityType] varchar(100) NOT NULL,
        [ItemId] varchar(100) NULL,
        [Action] varchar(50) NOT NULL,
        [EventSource] varchar(50) NOT NULL,
        [DetailsJson] nvarchar(max) NULL,
        CONSTRAINT [PK_AuditLog] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AuditLog_Staff_StaffId] FOREIGN KEY ([StaffId]) REFERENCES [auth].[Staff] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE TABLE [core].[Company] (
        [Id] int NOT NULL IDENTITY,
        [PartnerId] int NOT NULL,
        [ManagerId] int NULL,
        [ShortName] nvarchar(50) NOT NULL,
        [PrintName] nvarchar(50) NOT NULL,
        [RelativeFolderName] nvarchar(50) NULL,
        [CompanyTypeId] int NULL,
        [VatTypeId] int NULL,
        [LedgerEntryDate] date NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Company] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Company_ShortList_CompanyTypeId] FOREIGN KEY ([CompanyTypeId]) REFERENCES [core].[ShortList] ([Id]),
        CONSTRAINT [FK_Company_ShortList_VatTypeId] FOREIGN KEY ([VatTypeId]) REFERENCES [core].[ShortList] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE TABLE [ops].[IdempotencyRequest] (
        [Id] bigint NOT NULL IDENTITY,
        [StaffId] int NOT NULL,
        [CompanyId] int NULL,
        [Key] varchar(128) NOT NULL,
        [RequestHash] varchar(64) NOT NULL,
        [ResponseStatusCode] int NULL,
        [ResponseJson] nvarchar(max) NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [ExpiresAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_IdempotencyRequest] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_IdempotencyRequest_Company_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [core].[Company] ([Id]),
        CONSTRAINT [FK_IdempotencyRequest_Staff_StaffId] FOREIGN KEY ([StaffId]) REFERENCES [auth].[Staff] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE TABLE [finance].[JournalEntry] (
        [Id] int NOT NULL IDENTITY,
        [CompanyId] int NOT NULL,
        [PostingDate] date NOT NULL,
        [DueDate] date NULL,
        [Balance] decimal(18,4) NOT NULL,
        [Note] nvarchar(255) NULL,
        [Description] nvarchar(255) NOT NULL,
        [JournalEntryTypeId] int NULL,
        [Currency] varchar(3) NOT NULL,
        [IsPosted] bit NOT NULL,
        [PostedAt] datetimeoffset NULL,
        [PostedUserId] int NULL,
        [ReversalOfId] int NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_JournalEntry] PRIMARY KEY ([Id]),
        CONSTRAINT [AK_JournalEntry_Id_CompanyId] UNIQUE ([Id], [CompanyId]),
        CONSTRAINT [CK_JournalEntry_Posted] CHECK ([IsPosted] = 0 OR ([PostedAt] IS NOT NULL AND [PostedUserId] IS NOT NULL)),
        CONSTRAINT [FK_JournalEntry_Company_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [core].[Company] ([Id]),
        CONSTRAINT [FK_JournalEntry_JournalEntry_ReversalOfId] FOREIGN KEY ([ReversalOfId]) REFERENCES [finance].[JournalEntry] ([Id]),
        CONSTRAINT [FK_JournalEntry_ShortList_JournalEntryTypeId] FOREIGN KEY ([JournalEntryTypeId]) REFERENCES [core].[ShortList] ([Id]),
        CONSTRAINT [FK_JournalEntry_Staff_PostedUserId] FOREIGN KEY ([PostedUserId]) REFERENCES [auth].[Staff] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE TABLE [ops].[OutboxMessage] (
        [Id] uniqueidentifier NOT NULL,
        [CompanyId] int NULL,
        [OccurredAt] datetimeoffset NOT NULL,
        [Type] varchar(255) NOT NULL,
        [DedupeKey] varchar(128) NOT NULL,
        [PayloadJson] nvarchar(max) NOT NULL,
        [CorrelationId] varchar(64) NOT NULL,
        [ProcessedAt] datetimeoffset NULL,
        [AttemptCount] int NOT NULL,
        [LastError] nvarchar(max) NULL,
        CONSTRAINT [PK_OutboxMessage] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OutboxMessage_Company_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [core].[Company] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE TABLE [core].[Partner] (
        [Id] int NOT NULL IDENTITY,
        [CompanyId] int NULL,
        [ShortName] nvarchar(100) NOT NULL,
        [Name] nvarchar(255) NOT NULL,
        [RegistrationNumber] varchar(10) NULL,
        [TaxNumber] varchar(10) NULL,
        [Jbkjs] varchar(10) NULL,
        [IdCardNumber] varchar(20) NULL,
        [Jmbg] varchar(15) NULL,
        [PartnerTypeId] int NULL,
        [Language] varchar(10) NOT NULL,
        [Note] nvarchar(max) NULL,
        CONSTRAINT [PK_Partner] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Partner_Company_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [core].[Company] ([Id]),
        CONSTRAINT [FK_Partner_ShortList_PartnerTypeId] FOREIGN KEY ([PartnerTypeId]) REFERENCES [core].[ShortList] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE TABLE [auth].[StaffAccess] (
        [Id] int NOT NULL IDENTITY,
        [StaffId] int NOT NULL,
        [CompanyId] int NOT NULL,
        [StaffRole] int NOT NULL,
        CONSTRAINT [PK_StaffAccess] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_StaffAccess_Company_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [core].[Company] ([Id]),
        CONSTRAINT [FK_StaffAccess_Staff_StaffId] FOREIGN KEY ([StaffId]) REFERENCES [auth].[Staff] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE TABLE [finance].[LedgerEntry] (
        [Id] int NOT NULL IDENTITY,
        [JournalEntryId] int NOT NULL,
        [CompanyId] int NOT NULL,
        [Account] varchar(10) NOT NULL,
        [PostingDate] date NOT NULL,
        [DueDate] date NULL,
        [DebitAmount] decimal(18,4) NOT NULL,
        [CreditAmount] decimal(18,4) NOT NULL,
        [LineTypeId] int NULL,
        [DocumentRef] nvarchar(50) NULL,
        [Note] nvarchar(255) NULL,
        [Parameters] nvarchar(25) NULL,
        [Description] nvarchar(255) NULL,
        [Priority] int NOT NULL,
        CONSTRAINT [PK_LedgerEntry] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_LedgerEntry_Amounts] CHECK ([DebitAmount] >= 0 AND [CreditAmount] >= 0 AND NOT ([DebitAmount] > 0 AND [CreditAmount] > 0)),
        CONSTRAINT [FK_LedgerEntry_Company_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [core].[Company] ([Id]),
        CONSTRAINT [FK_LedgerEntry_JournalEntry_JournalEntryId_CompanyId] FOREIGN KEY ([JournalEntryId], [CompanyId]) REFERENCES [finance].[JournalEntry] ([Id], [CompanyId]),
        CONSTRAINT [FK_LedgerEntry_ShortList_LineTypeId] FOREIGN KEY ([LineTypeId]) REFERENCES [core].[ShortList] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE TABLE [finance].[Invoice] (
        [Id] int NOT NULL IDENTITY,
        [CompanyId] int NOT NULL,
        [PartnerId] int NOT NULL,
        [SequenceNumber] varchar(20) NOT NULL,
        [IssueDate] date NOT NULL,
        [DueDate] date NOT NULL,
        [PartnerName] nvarchar(255) NOT NULL,
        [Address] nvarchar(255) NOT NULL,
        [PostalCode] varchar(50) NULL,
        [City] nvarchar(50) NOT NULL,
        [TaxNumber] varchar(50) NULL,
        [RegistrationNumber] varchar(50) NULL,
        [Currency] varchar(3) NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [VatRate] decimal(18,2) NOT NULL,
        [VatAmount] decimal(18,2) NOT NULL,
        [Total] decimal(18,2) NOT NULL,
        [InterestAmount] decimal(18,2) NOT NULL,
        [InvoiceTotal] decimal(18,2) NOT NULL,
        [InvoiceDeliveryLocation] nvarchar(50) NULL,
        [CancelledAt] datetimeoffset NULL,
        [IsCancelled] bit NOT NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Invoice] PRIMARY KEY ([Id]),
        CONSTRAINT [AK_Invoice_Id_CompanyId] UNIQUE ([Id], [CompanyId]),
        CONSTRAINT [CK_Invoice_Totals] CHECK ([Amount] >= 0 AND [VatAmount] >= 0 AND [InterestAmount] >= 0 AND [InvoiceTotal] = [Total] + [InterestAmount]),
        CONSTRAINT [FK_Invoice_Company_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [core].[Company] ([Id]),
        CONSTRAINT [FK_Invoice_Partner_PartnerId] FOREIGN KEY ([PartnerId]) REFERENCES [core].[Partner] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE TABLE [finance].[InvoiceLine] (
        [Id] int NOT NULL IDENTITY,
        [InvoiceId] int NOT NULL,
        [CompanyId] int NOT NULL,
        [Name] nvarchar(255) NOT NULL,
        [K1] decimal(18,4) NOT NULL,
        [K2] decimal(18,4) NOT NULL,
        [K3] decimal(18,4) NOT NULL,
        [K4] decimal(18,4) NOT NULL,
        [K5] decimal(18,4) NOT NULL,
        [Quantity] decimal(18,2) NOT NULL,
        [UnitOfMeasureId] int NULL,
        [PriceEur] decimal(18,4) NOT NULL,
        [ExchangeRateNbs] decimal(18,4) NOT NULL,
        [PricePcs] decimal(18,4) NOT NULL,
        [PriceTotal] decimal(18,2) NOT NULL,
        [VatRate] decimal(18,2) NOT NULL,
        [VatAmount] decimal(18,2) NOT NULL,
        [TotalAmount] decimal(18,2) NOT NULL,
        [SortIndex] int NOT NULL,
        CONSTRAINT [PK_InvoiceLine] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_InvoiceLine_Company_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [core].[Company] ([Id]),
        CONSTRAINT [FK_InvoiceLine_Invoice_InvoiceId_CompanyId] FOREIGN KEY ([InvoiceId], [CompanyId]) REFERENCES [finance].[Invoice] ([Id], [CompanyId]),
        CONSTRAINT [FK_InvoiceLine_ShortList_UnitOfMeasureId] FOREIGN KEY ([UnitOfMeasureId]) REFERENCES [core].[ShortList] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE INDEX [IX_Address_PostalCode_City] ON [core].[Address] ([PostalCode], [City]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE INDEX [IX_AuditLog_CompanyId_Timestamp] ON [ops].[AuditLog] ([CompanyId], [Timestamp]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE INDEX [IX_AuditLog_CorrelationId] ON [ops].[AuditLog] ([CorrelationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE INDEX [IX_AuditLog_StaffId] ON [ops].[AuditLog] ([StaffId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE INDEX [IX_Company_CompanyTypeId] ON [core].[Company] ([CompanyTypeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE INDEX [IX_Company_ManagerId] ON [core].[Company] ([ManagerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE INDEX [IX_Company_PartnerId] ON [core].[Company] ([PartnerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE INDEX [IX_Company_ShortName] ON [core].[Company] ([ShortName]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE INDEX [IX_Company_VatTypeId] ON [core].[Company] ([VatTypeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE UNIQUE INDEX [IX_EtlRun_SourceSystem_ContentHash] ON [etl].[EtlRun] ([SourceSystem], [ContentHash]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE INDEX [IX_IdempotencyRequest_CompanyId] ON [ops].[IdempotencyRequest] ([CompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE INDEX [IX_IdempotencyRequest_ExpiresAt] ON [ops].[IdempotencyRequest] ([ExpiresAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_IdempotencyRequest_StaffId_CompanyId_Key] ON [ops].[IdempotencyRequest] ([StaffId], [CompanyId], [Key]) WHERE [CompanyId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE INDEX [IX_Invoice_CompanyId_IssueDate] ON [finance].[Invoice] ([CompanyId], [IssueDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Invoice_CompanyId_SequenceNumber] ON [finance].[Invoice] ([CompanyId], [SequenceNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE INDEX [IX_Invoice_PartnerId] ON [finance].[Invoice] ([PartnerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE INDEX [IX_InvoiceLine_CompanyId] ON [finance].[InvoiceLine] ([CompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE INDEX [IX_InvoiceLine_InvoiceId_CompanyId] ON [finance].[InvoiceLine] ([InvoiceId], [CompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE INDEX [IX_InvoiceLine_InvoiceId_SortIndex] ON [finance].[InvoiceLine] ([InvoiceId], [SortIndex]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE INDEX [IX_InvoiceLine_UnitOfMeasureId] ON [finance].[InvoiceLine] ([UnitOfMeasureId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE INDEX [IX_JournalEntry_CompanyId_IsPosted] ON [finance].[JournalEntry] ([CompanyId], [IsPosted]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE INDEX [IX_JournalEntry_CompanyId_PostingDate] ON [finance].[JournalEntry] ([CompanyId], [PostingDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE INDEX [IX_JournalEntry_JournalEntryTypeId] ON [finance].[JournalEntry] ([JournalEntryTypeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE INDEX [IX_JournalEntry_PostedUserId] ON [finance].[JournalEntry] ([PostedUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE INDEX [IX_JournalEntry_ReversalOfId] ON [finance].[JournalEntry] ([ReversalOfId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE INDEX [IX_LedgerEntry_CompanyId_PostingDate] ON [finance].[LedgerEntry] ([CompanyId], [PostingDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE INDEX [IX_LedgerEntry_JournalEntryId] ON [finance].[LedgerEntry] ([JournalEntryId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE INDEX [IX_LedgerEntry_JournalEntryId_CompanyId] ON [finance].[LedgerEntry] ([JournalEntryId], [CompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE INDEX [IX_LedgerEntry_LineTypeId] ON [finance].[LedgerEntry] ([LineTypeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE INDEX [IX_LegacyKeyMap_EtlRunId] ON [etl].[LegacyKeyMap] ([EtlRunId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE UNIQUE INDEX [IX_LegacyKeyMap_SourceTable_SourceKey_TargetTable] ON [etl].[LegacyKeyMap] ([SourceTable], [SourceKey], [TargetTable]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE INDEX [IX_OutboxMessage_CompanyId_OccurredAt] ON [ops].[OutboxMessage] ([CompanyId], [OccurredAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE UNIQUE INDEX [IX_OutboxMessage_DedupeKey] ON [ops].[OutboxMessage] ([DedupeKey]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE INDEX [IX_OutboxMessage_ProcessedAt_OccurredAt] ON [ops].[OutboxMessage] ([ProcessedAt], [OccurredAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE INDEX [IX_Partner_CompanyId] ON [core].[Partner] ([CompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE INDEX [IX_Partner_PartnerTypeId] ON [core].[Partner] ([PartnerTypeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE INDEX [IX_Partner_TaxNumber] ON [core].[Partner] ([TaxNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE INDEX [IX_QuarantineRecord_EtlRunId_SourceTable] ON [etl].[QuarantineRecord] ([EtlRunId], [SourceTable]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [auth].[Role] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE INDEX [IX_RoleClaim_RoleId] ON [auth].[RoleClaim] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE INDEX [IX_ShortList_TableName_IndexKey] ON [core].[ShortList] ([TableName], [IndexKey]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ShortList_TableName_IndexValue] ON [core].[ShortList] ([TableName], [IndexValue]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [auth].[Staff] ([NormalizedEmail]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [auth].[Staff] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE INDEX [IX_StaffAccess_CompanyId_StaffRole] ON [auth].[StaffAccess] ([CompanyId], [StaffRole]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE UNIQUE INDEX [IX_StaffAccess_StaffId_CompanyId] ON [auth].[StaffAccess] ([StaffId], [CompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE INDEX [IX_StaffClaim_UserId] ON [auth].[StaffClaim] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE INDEX [IX_StaffLogin_UserId] ON [auth].[StaffLogin] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    CREATE INDEX [IX_StaffRole_RoleId] ON [auth].[StaffRole] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    ALTER TABLE [ops].[AuditLog] ADD CONSTRAINT [FK_AuditLog_Company_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [core].[Company] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    ALTER TABLE [core].[Company] ADD CONSTRAINT [FK_Company_Partner_ManagerId] FOREIGN KEY ([ManagerId]) REFERENCES [core].[Partner] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    ALTER TABLE [core].[Company] ADD CONSTRAINT [FK_Company_Partner_PartnerId] FOREIGN KEY ([PartnerId]) REFERENCES [core].[Partner] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260922181533_InitialFoundation'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260922181533_InitialFoundation', N'10.0.10');
END;

COMMIT;
GO

