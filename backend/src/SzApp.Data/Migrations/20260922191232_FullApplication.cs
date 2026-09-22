using System;
using Microsoft.EntityFrameworkCore.Migrations;
using SzApp.Data.Configurations.LedgerBanking;

#nullable disable

namespace SzApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class FullApplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JournalEntry_ReversalOfId",
                schema: "finance",
                table: "JournalEntry");

            migrationBuilder.DropIndex(
                name: "IX_EtlRun_SourceSystem_ContentHash",
                schema: "etl",
                table: "EtlRun");

            migrationBuilder.EnsureSchema(
                name: "reports");

            migrationBuilder.EnsureSchema(
                name: "billing");

            migrationBuilder.EnsureSchema(
                name: "platform");

            migrationBuilder.AddColumn<int>(
                name: "BankStatementLineId",
                schema: "finance",
                table: "LedgerEntry",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PartnerAccountId",
                schema: "finance",
                table: "LedgerEntry",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubAccountId",
                schema: "finance",
                table: "LedgerEntry",
                type: "varchar(10)",
                unicode: false,
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                schema: "finance",
                table: "InvoiceLine",
                type: "varchar(3)",
                unicode: false,
                maxLength: 3,
                nullable: true,
                defaultValue: "RSD");

            migrationBuilder.AddColumn<int>(
                name: "InvoiceBatchId",
                schema: "finance",
                table: "InvoiceLine",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PartnerId",
                schema: "finance",
                table: "InvoiceLine",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SupplierInvoiceId",
                schema: "finance",
                table: "InvoiceLine",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "BalanceAsOfDate",
                schema: "finance",
                table: "Invoice",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancelReason",
                schema: "finance",
                table: "Invoice",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryLocation",
                schema: "finance",
                table: "Invoice",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InvoiceBatchId",
                schema: "finance",
                table: "Invoice",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InvoiceDeliveryUnitId",
                schema: "finance",
                table: "Invoice",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InvoiceLayoutId",
                schema: "finance",
                table: "Invoice",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InvoiceLegacyMasterId",
                schema: "finance",
                table: "Invoice",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InvoiceParentId",
                schema: "finance",
                table: "Invoice",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                schema: "finance",
                table: "Invoice",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NoticeId",
                schema: "finance",
                table: "Invoice",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PageCount",
                schema: "finance",
                table: "Invoice",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Pak",
                schema: "finance",
                table: "Invoice",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentReference",
                schema: "finance",
                table: "Invoice",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlaceOfIssue",
                schema: "finance",
                table: "Invoice",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PreviousBalance",
                schema: "finance",
                table: "Invoice",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PrintNote",
                schema: "finance",
                table: "Invoice",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ServiceDateFrom",
                schema: "finance",
                table: "Invoice",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ServiceDateTo",
                schema: "finance",
                table: "Invoice",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SortIndex",
                schema: "finance",
                table: "Invoice",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateOnly>(
                name: "TransactionDate",
                schema: "finance",
                table: "Invoice",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                schema: "etl",
                table: "EtlRun",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AnalysisReportDefinition",
                schema: "reports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    DataGroup = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsCriticalGroup = table.Column<bool>(type: "bit", nullable: false),
                    QueryName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    QuerySql = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReportName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FilterCaption = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FilterQuerySql = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LegacyActionQueryName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    LegacyActionQuerySql = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AutoRunAll = table.Column<bool>(type: "bit", nullable: false),
                    SortIndex = table.Column<int>(type: "int", nullable: false),
                    LinkCreationTag = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    LinkFormName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    LinkOpenArgs = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    LastRunAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastRowCount = table.Column<int>(type: "int", nullable: true),
                    AdminAlert = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalysisReportDefinition", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnalysisReportDefinition_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "core",
                        principalTable: "Company",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BankAccount",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountNumber = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    PartnerId = table.Column<int>(type: "int", nullable: true),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    SortIndex = table.Column<int>(type: "int", nullable: true),
                    Currency = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankAccount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankAccount_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "core",
                        principalTable: "Company",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BankAccount_Partner_PartnerId",
                        column: x => x.PartnerId,
                        principalSchema: "core",
                        principalTable: "Partner",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BuildingEntrance",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    BuildingName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    EntranceName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    AddressId = table.Column<int>(type: "int", nullable: true),
                    BuildingLabel = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    SortIndex = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingEntrance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BuildingEntrance_Address_AddressId",
                        column: x => x.AddressId,
                        principalSchema: "core",
                        principalTable: "Address",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BuildingEntrance_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "core",
                        principalTable: "Company",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CalculationType",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SupplierAmountRule = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    AllocationRule = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    QuantityRule = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    UnitOfMeasureId = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalculationType", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalculationType_ShortList_UnitOfMeasureId",
                        column: x => x.UnitOfMeasureId,
                        principalSchema: "core",
                        principalTable: "ShortList",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ChartOfAccounts",
                schema: "finance",
                columns: table => new
                {
                    Account = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    ShortName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ParentAccount = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Sign = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSynthetic = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChartOfAccounts", x => x.Account);
                    table.CheckConstraint("CK_ChartOfAccounts_Sign", "[Sign] IN (-1, 1)");
                    table.ForeignKey(
                        name: "FK_ChartOfAccounts_ChartOfAccounts_ParentAccount",
                        column: x => x.ParentAccount,
                        principalSchema: "finance",
                        principalTable: "ChartOfAccounts",
                        principalColumn: "Account");
                });

            migrationBuilder.CreateTable(
                name: "Document",
                schema: "platform",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    OwnerStaffId = table.Column<int>(type: "int", nullable: false),
                    DocumentTypeId = table.Column<int>(type: "int", nullable: true),
                    SourceTable = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    ReferenceId = table.Column<int>(type: "int", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    RelativePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "varchar(127)", unicode: false, maxLength: 127, nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    Sha256 = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    DocumentDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Document", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Document_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "core",
                        principalTable: "Company",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Document_Staff_OwnerStaffId",
                        column: x => x.OwnerStaffId,
                        principalSchema: "auth",
                        principalTable: "Staff",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DocumentCategory",
                schema: "platform",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Code = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RetentionPeriodYears = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentCategory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EtlParityAssessment",
                schema: "etl",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EtlRunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ArtifactKind = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ExpectedCount = table.Column<int>(type: "int", nullable: false),
                    AssessedCount = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AssessedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EtlParityAssessment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EtlParityAssessment_EtlRun_EtlRunId",
                        column: x => x.EtlRunId,
                        principalSchema: "etl",
                        principalTable: "EtlRun",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EtlRunContext",
                schema: "etl",
                columns: table => new
                {
                    EtlRunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    SourceTable = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    StoredRelativePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    EncodingName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Delimiter = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    Stage = table.Column<int>(type: "int", nullable: false),
                    PipelineVersion = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EtlRunContext", x => x.EtlRunId);
                    table.ForeignKey(
                        name: "FK_EtlRunContext_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "core",
                        principalTable: "Company",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EtlRunContext_EtlRun_EtlRunId",
                        column: x => x.EtlRunId,
                        principalSchema: "etl",
                        principalTable: "EtlRun",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Event",
                schema: "platform",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    RequestedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DecidedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ExecutedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RequestedByStaffId = table.Column<int>(type: "int", nullable: false),
                    DecidedByStaffId = table.Column<int>(type: "int", nullable: true),
                    ExecutedByStaffId = table.Column<int>(type: "int", nullable: true),
                    ContractId = table.Column<int>(type: "int", nullable: true),
                    PartnerId = table.Column<int>(type: "int", nullable: true),
                    UnitId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PreviousValue = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    FieldsRelated = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    RequestTypeId = table.Column<int>(type: "int", nullable: true),
                    RequestBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    RequestThrough = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DecisionReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EmergencyOverride = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Event", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExchangeRate",
                schema: "finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Rate = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    RateDateFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExchangeRate", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FiscalYear",
                schema: "finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    Display = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Folder = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FiscalYear", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FiscalYear_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "core",
                        principalTable: "Company",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ImportDefinition",
                schema: "platform",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Code = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    FileMask = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ImportSourceId = table.Column<int>(type: "int", nullable: true),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TargetHeaderTable = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    TargetLineTable = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortIndex = table.Column<int>(type: "int", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportDefinition", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InterestRate",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TimeCode = table.Column<string>(type: "varchar(1)", unicode: false, maxLength: 1, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterestRate", x => x.Id);
                    table.CheckConstraint("CK_InterestRate_Rate", "[Rate] >= 0 AND [TimeCode] IN ('M', 'G')");
                });

            migrationBuilder.CreateTable(
                name: "InvoiceBatch",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    PeriodYYMM = table.Column<int>(type: "int", nullable: false),
                    Caption = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Place = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IssueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ServiceDateFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    ServiceDateTo = table.Column<DateOnly>(type: "date", nullable: false),
                    TransactionDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ExchangeRateNbs = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    JournalEntryId = table.Column<int>(type: "int", nullable: true),
                    EntryDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    StaffId = table.Column<int>(type: "int", nullable: false),
                    ExtraordinaryInvoiceMarker = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    BalanceAsOfDate = table.Column<DateOnly>(type: "date", nullable: true),
                    PreviousValueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsInterestCalculated = table.Column<bool>(type: "bit", nullable: false),
                    PaymentPurpose = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    GenerationFingerprint = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    GeneratedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    PostedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceBatch", x => x.Id);
                    table.CheckConstraint("CK_InvoiceBatch_Dates", "[ServiceDateFrom] <= [ServiceDateTo] AND [IssueDate] <= [DueDate]");
                    table.CheckConstraint("CK_InvoiceBatch_Period", "[Month] BETWEEN 1 AND 12 AND [PeriodYYMM] = ([Year] % 100) * 100 + [Month]");
                    table.ForeignKey(
                        name: "FK_InvoiceBatch_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "core",
                        principalTable: "Company",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InvoiceBatch_Staff_StaffId",
                        column: x => x.StaffId,
                        principalSchema: "auth",
                        principalTable: "Staff",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Language",
                schema: "platform",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    SortIndex = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Language", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "LedgerSourcePosting",
                schema: "finance",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    SourceType = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    SourceId = table.Column<int>(type: "int", nullable: false),
                    JournalEntryId = table.Column<int>(type: "int", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LedgerSourcePosting", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LedgerSourcePosting_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "core",
                        principalTable: "Company",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LedgerSourcePosting_JournalEntry_JournalEntryId_CompanyId",
                        columns: x => new { x.JournalEntryId, x.CompanyId },
                        principalSchema: "finance",
                        principalTable: "JournalEntry",
                        principalColumns: new[] { "Id", "CompanyId" });
                });

            migrationBuilder.CreateTable(
                name: "LocationCategory",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    SortIndex = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationCategory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocationCategory_LocationCategory_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "core",
                        principalTable: "LocationCategory",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "NoticeTemplate",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoticeTemplate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NoticeTemplate_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "core",
                        principalTable: "Company",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PartnerAddress",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartnerId = table.Column<int>(type: "int", nullable: false),
                    AddressId = table.Column<int>(type: "int", nullable: false),
                    AddressTypeId = table.Column<int>(type: "int", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartnerAddress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartnerAddress_Address_AddressId",
                        column: x => x.AddressId,
                        principalSchema: "core",
                        principalTable: "Address",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PartnerAddress_Partner_PartnerId",
                        column: x => x.PartnerId,
                        principalSchema: "core",
                        principalTable: "Partner",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PartnerAddress_ShortList_AddressTypeId",
                        column: x => x.AddressTypeId,
                        principalSchema: "core",
                        principalTable: "ShortList",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PartnerComms",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartnerId = table.Column<int>(type: "int", nullable: false),
                    ChannelId = table.Column<int>(type: "int", nullable: false),
                    ValueNormalized = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    SortIndex = table.Column<int>(type: "int", nullable: false),
                    IsRegisteredForInvoiceReceipt = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartnerComms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartnerComms_Partner_PartnerId",
                        column: x => x.PartnerId,
                        principalSchema: "core",
                        principalTable: "Partner",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PartnerComms_ShortList_ChannelId",
                        column: x => x.ChannelId,
                        principalSchema: "core",
                        principalTable: "ShortList",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PaymentOrder",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    TemplateTitle = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PayerName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PaymentPurpose = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    RecipientName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PaymentCode = table.Column<int>(type: "int", nullable: false),
                    Currency = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PayerAccountNumber = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    PayerModelNumber = table.Column<int>(type: "int", nullable: true),
                    PayerPaymentReference = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RecipientAccountNumber = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    RecipientModelNumber = table.Column<int>(type: "int", nullable: true),
                    RecipientPaymentReference = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Place = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    ValueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    IsUrgent = table.Column<bool>(type: "bit", nullable: false),
                    PaymentOrderTypeId = table.Column<int>(type: "int", nullable: false),
                    CreatedTimestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsFavorite = table.Column<bool>(type: "bit", nullable: false),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentOrder", x => x.Id);
                    table.CheckConstraint("CK_PaymentOrder_Amount", "[Amount] > 0");
                    table.ForeignKey(
                        name: "FK_PaymentOrder_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "core",
                        principalTable: "Company",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PaymentOrder_ShortList_PaymentOrderTypeId",
                        column: x => x.PaymentOrderTypeId,
                        principalSchema: "core",
                        principalTable: "ShortList",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PendingLegacyRelationship",
                schema: "etl",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EtlRunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceTable = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    SourceKey = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    RelationshipName = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    TargetSourceTable = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    TargetSourceKey = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsResolved = table.Column<bool>(type: "bit", nullable: false),
                    ResolutionError = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingLegacyRelationship", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PendingLegacyRelationship_EtlRun_EtlRunId",
                        column: x => x.EtlRunId,
                        principalSchema: "etl",
                        principalTable: "EtlRun",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RawStagingRow",
                schema: "etl",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EtlRunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceSystem = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    SourceTable = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    SourceRowNumber = table.Column<int>(type: "int", nullable: false),
                    SourceKey = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    RowHash = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    RawText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ValuesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StagedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RawStagingRow", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RawStagingRow_EtlRun_EtlRunId",
                        column: x => x.EtlRunId,
                        principalSchema: "etl",
                        principalTable: "EtlRun",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ReconciliationRecord",
                schema: "etl",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EtlRunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Metric = table.Column<int>(type: "int", nullable: false),
                    Scope = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    ExpectedValue = table.Column<decimal>(type: "decimal(28,4)", precision: 28, scale: 4, nullable: false),
                    ActualValue = table.Column<decimal>(type: "decimal(28,4)", precision: 28, scale: 4, nullable: false),
                    IsMatch = table.Column<bool>(type: "bit", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CheckedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReconciliationRecord", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReconciliationRecord_EtlRun_EtlRunId",
                        column: x => x.EtlRunId,
                        principalSchema: "etl",
                        principalTable: "EtlRun",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ReportDefinition",
                schema: "reports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Datasheet = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    SortIndex = table.Column<int>(type: "int", nullable: false),
                    FilterQuerySql = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FilterCaption = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportDefinition", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportDefinition_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "core",
                        principalTable: "Company",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ReportExecutionAudit",
                schema: "reports",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    StaffId = table.Column<int>(type: "int", nullable: false),
                    DefinitionType = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    DefinitionId = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DurationMs = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RowCount = table.Column<int>(type: "int", nullable: true),
                    WasTruncated = table.Column<bool>(type: "bit", nullable: false),
                    ParameterHash = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    CorrelationId = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    ErrorCode = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportExecutionAudit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportExecutionAudit_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "core",
                        principalTable: "Company",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReportExecutionAudit_Staff_StaffId",
                        column: x => x.StaffId,
                        principalSchema: "auth",
                        principalTable: "Staff",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SelectionBasket",
                schema: "platform",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    OwnerStaffId = table.Column<int>(type: "int", nullable: false),
                    TargetType = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    TargetId = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SelectionBasket", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SelectionBasket_Staff_OwnerStaffId",
                        column: x => x.OwnerStaffId,
                        principalSchema: "auth",
                        principalTable: "Staff",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SentEmail",
                schema: "platform",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    CreatedByStaffId = table.Column<int>(type: "int", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ToAddress = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Cc = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Bcc = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    BodyHtml = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    SentAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SendDescription = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AttemptCount = table.Column<int>(type: "int", nullable: false),
                    NextAttemptAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SentEmail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SentEmail_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "core",
                        principalTable: "Company",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SentEmail_Staff_CreatedByStaffId",
                        column: x => x.CreatedByStaffId,
                        principalSchema: "auth",
                        principalTable: "Staff",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Setting",
                schema: "platform",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Key = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ValueMax = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Setting", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Setting_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "core",
                        principalTable: "Company",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BankStatement",
                schema: "finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    BankAccountId = table.Column<int>(type: "int", nullable: false),
                    LedgerAccount = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    StatementNumber = table.Column<int>(type: "int", nullable: false),
                    StatementSuffix = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    PreviousBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NewBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Debit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Credit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CountDebitEntry = table.Column<int>(type: "int", nullable: false),
                    CountCreditEntry = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    JournalEntryId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankStatement", x => x.Id);
                    table.UniqueConstraint("AK_BankStatement_Id_CompanyId", x => new { x.Id, x.CompanyId });
                    table.CheckConstraint("CK_BankStatement_Amounts", "[PreviousBalance] + [Credit] - [Debit] = [NewBalance] AND [Debit] >= 0 AND [Credit] >= 0");
                    table.CheckConstraint("CK_BankStatement_Counts", "[CountDebitEntry] >= 0 AND [CountCreditEntry] >= 0");
                    table.ForeignKey(
                        name: "FK_BankStatement_BankAccount_BankAccountId",
                        column: x => x.BankAccountId,
                        principalSchema: "core",
                        principalTable: "BankAccount",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BankStatement_ChartOfAccounts_LedgerAccount",
                        column: x => x.LedgerAccount,
                        principalSchema: "finance",
                        principalTable: "ChartOfAccounts",
                        principalColumn: "Account");
                    table.ForeignKey(
                        name: "FK_BankStatement_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "core",
                        principalTable: "Company",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BankStatement_JournalEntry_JournalEntryId_CompanyId",
                        columns: x => new { x.JournalEntryId, x.CompanyId },
                        principalSchema: "finance",
                        principalTable: "JournalEntry",
                        principalColumns: new[] { "Id", "CompanyId" });
                });

            migrationBuilder.CreateTable(
                name: "PostingScheme",
                schema: "finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    SourceType = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    DebitAccount = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    CreditAccount = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    DescriptionTemplate = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    SortIndex = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostingScheme", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostingScheme_ChartOfAccounts_CreditAccount",
                        column: x => x.CreditAccount,
                        principalSchema: "finance",
                        principalTable: "ChartOfAccounts",
                        principalColumn: "Account");
                    table.ForeignKey(
                        name: "FK_PostingScheme_ChartOfAccounts_DebitAccount",
                        column: x => x.DebitAccount,
                        principalSchema: "finance",
                        principalTable: "ChartOfAccounts",
                        principalColumn: "Account");
                    table.ForeignKey(
                        name: "FK_PostingScheme_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "core",
                        principalTable: "Company",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ImportMappingGroup",
                schema: "platform",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImportDefinitionId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TargetTable = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    SourcePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsRepeating = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportMappingGroup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImportMappingGroup_ImportDefinition_ImportDefinitionId",
                        column: x => x.ImportDefinitionId,
                        principalSchema: "platform",
                        principalTable: "ImportDefinition",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Translation",
                schema: "platform",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LanguageCode = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    ResourceKey = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    ResourceId = table.Column<int>(type: "int", nullable: true),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Translation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Translation_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "core",
                        principalTable: "Company",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Translation_Language_LanguageCode",
                        column: x => x.LanguageCode,
                        principalSchema: "platform",
                        principalTable: "Language",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "NoticeBatch",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    MinUnpaidInvoiceCount = table.Column<int>(type: "int", nullable: false),
                    DebtTolerance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DebtToleranceByMonth = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NoticeTemplateId = table.Column<int>(type: "int", nullable: false),
                    NoticeTypeId = table.Column<int>(type: "int", nullable: false),
                    UpToClaimDate = table.Column<DateOnly>(type: "date", nullable: false),
                    UpToPaymentDate = table.Column<DateOnly>(type: "date", nullable: false),
                    InvoiceBatchId = table.Column<int>(type: "int", nullable: true),
                    CustomCaptionOnSlip = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    GenerationFingerprint = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoticeBatch", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NoticeBatch_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "core",
                        principalTable: "Company",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_NoticeBatch_InvoiceBatch_InvoiceBatchId",
                        column: x => x.InvoiceBatchId,
                        principalSchema: "billing",
                        principalTable: "InvoiceBatch",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_NoticeBatch_NoticeTemplate_NoticeTemplateId",
                        column: x => x.NoticeTemplateId,
                        principalSchema: "billing",
                        principalTable: "NoticeTemplate",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_NoticeBatch_ShortList_NoticeTypeId",
                        column: x => x.NoticeTypeId,
                        principalSchema: "core",
                        principalTable: "ShortList",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ReportDefinitionButton",
                schema: "reports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportDefinitionId = table.Column<int>(type: "int", nullable: false),
                    Caption = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Function = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    FunctionTypeId = table.Column<int>(type: "int", nullable: false),
                    SortIndex = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportDefinitionButton", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportDefinitionButton_ReportDefinition_ReportDefinitionId",
                        column: x => x.ReportDefinitionId,
                        principalSchema: "reports",
                        principalTable: "ReportDefinition",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReportDefinitionButton_ShortList_FunctionTypeId",
                        column: x => x.FunctionTypeId,
                        principalSchema: "core",
                        principalTable: "ShortList",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ReportDefinitionDetail",
                schema: "reports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportDefinitionId = table.Column<int>(type: "int", nullable: false),
                    QuerySql = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QueryName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    SortIndex = table.Column<int>(type: "int", nullable: false),
                    Function = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportDefinitionDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportDefinitionDetail_ReportDefinition_ReportDefinitionId",
                        column: x => x.ReportDefinitionId,
                        principalSchema: "reports",
                        principalTable: "ReportDefinition",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SentEmailAttachment",
                schema: "platform",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SentEmailId = table.Column<long>(type: "bigint", nullable: false),
                    DocumentId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SentEmailAttachment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SentEmailAttachment_Document_DocumentId",
                        column: x => x.DocumentId,
                        principalSchema: "platform",
                        principalTable: "Document",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SentEmailAttachment_SentEmail_SentEmailId",
                        column: x => x.SentEmailId,
                        principalSchema: "platform",
                        principalTable: "SentEmail",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ImportMapping",
                schema: "platform",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImportMappingGroupId = table.Column<int>(type: "int", nullable: false),
                    TargetField = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    SourcePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SourceNode = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    MappingTypeId = table.Column<int>(type: "int", nullable: true),
                    DataTypeId = table.Column<int>(type: "int", nullable: true),
                    DefaultValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    IsKey = table.Column<bool>(type: "bit", nullable: false),
                    IsLookup = table.Column<bool>(type: "bit", nullable: false),
                    LookupTable = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    LookupField = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    LookupValueField = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Format = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SortIndex = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportMapping", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImportMapping_ImportMappingGroup_ImportMappingGroupId",
                        column: x => x.ImportMappingGroupId,
                        principalSchema: "platform",
                        principalTable: "ImportMappingGroup",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BankInFlow",
                schema: "finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    BankAccountId = table.Column<int>(type: "int", nullable: false),
                    DateInFlow = table.Column<DateOnly>(type: "date", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Currency = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    OriginalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AmountLocalCurrency = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PartnerAccountId = table.Column<int>(type: "int", nullable: true),
                    InvoiceDescription = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BankStatementLineId = table.Column<int>(type: "int", nullable: false),
                    SentToManagerAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankInFlow", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankInFlow_BankAccount_BankAccountId",
                        column: x => x.BankAccountId,
                        principalSchema: "core",
                        principalTable: "BankAccount",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BankInFlow_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "core",
                        principalTable: "Company",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BankStatementLine",
                schema: "finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    BankStatementId = table.Column<int>(type: "int", nullable: false),
                    LineNumber = table.Column<int>(type: "int", nullable: false),
                    PayerRecipientName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    BankAccountNumber = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Debit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Credit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Info = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Code = table.Column<int>(type: "int", nullable: true),
                    PaymentReference = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PaymentReferenceOut = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PartnerAccountId = table.Column<int>(type: "int", nullable: true),
                    SubAccountId = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    CounterAccount = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    BankRef = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankStatementLine", x => x.Id);
                    table.UniqueConstraint("AK_BankStatementLine_Id_CompanyId", x => new { x.Id, x.CompanyId });
                    table.CheckConstraint("CK_BankStatementLine_Amounts", "[Debit] >= 0 AND [Credit] >= 0 AND (([Debit] > 0 AND [Credit] = 0) OR ([Debit] = 0 AND [Credit] > 0))");
                    table.ForeignKey(
                        name: "FK_BankStatementLine_BankStatement_BankStatementId_CompanyId",
                        columns: x => new { x.BankStatementId, x.CompanyId },
                        principalSchema: "finance",
                        principalTable: "BankStatement",
                        principalColumns: new[] { "Id", "CompanyId" });
                    table.ForeignKey(
                        name: "FK_BankStatementLine_ChartOfAccounts_CounterAccount",
                        column: x => x.CounterAccount,
                        principalSchema: "finance",
                        principalTable: "ChartOfAccounts",
                        principalColumn: "Account");
                    table.ForeignKey(
                        name: "FK_BankStatementLine_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "core",
                        principalTable: "Company",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BankStatementPostingTemplate",
                schema: "finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    TemplateName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FieldName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    FieldValue = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Function = table.Column<int>(type: "int", nullable: false),
                    SetPartnerAccountId = table.Column<int>(type: "int", nullable: true),
                    SetSubAccountId = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    SetAccountCode = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    SortIndex = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankStatementPostingTemplate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankStatementPostingTemplate_BankStatementPostingTemplate_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "finance",
                        principalTable: "BankStatementPostingTemplate",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BankStatementPostingTemplate_ChartOfAccounts_SetAccountCode",
                        column: x => x.SetAccountCode,
                        principalSchema: "finance",
                        principalTable: "ChartOfAccounts",
                        principalColumn: "Account");
                    table.ForeignKey(
                        name: "FK_BankStatementPostingTemplate_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "core",
                        principalTable: "Company",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Benefit",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    ContractId = table.Column<int>(type: "int", nullable: false),
                    PeriodYYMM = table.Column<int>(type: "int", nullable: false),
                    EntryDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    InvoiceId = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Benefit", x => x.Id);
                    table.CheckConstraint("CK_Benefit_Amount", "[Amount] >= 0");
                    table.ForeignKey(
                        name: "FK_Benefit_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "core",
                        principalTable: "Company",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Benefit_Invoice_InvoiceId",
                        column: x => x.InvoiceId,
                        principalSchema: "finance",
                        principalTable: "Invoice",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Contract",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    AccountNumber = table.Column<int>(type: "int", nullable: true),
                    OwnerPartnerId = table.Column<int>(type: "int", nullable: true),
                    InvoicePartnerId = table.Column<int>(type: "int", nullable: true),
                    TenantPartnerId = table.Column<int>(type: "int", nullable: true),
                    ContractDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ContractEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    InvoiceStartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    InvoiceEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    InvoiceDeliveryLocation = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    InvoiceDeliveryUnitId = table.Column<int>(type: "int", nullable: true),
                    InvoiceLegacyMasterId = table.Column<int>(type: "int", nullable: true),
                    IsPrintInvoiceMandatory = table.Column<bool>(type: "bit", nullable: false),
                    IsPrintInvoiceToPostOffice = table.Column<bool>(type: "bit", nullable: false),
                    IsPrintInvoiceSkipped = table.Column<bool>(type: "bit", nullable: false),
                    ExportExternalAccount = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contract", x => x.Id);
                    table.CheckConstraint("CK_Contract_InvoicePeriod", "[InvoiceEndDate] IS NULL OR ([InvoiceStartDate] IS NOT NULL AND [InvoiceEndDate] >= [InvoiceStartDate])");
                    table.CheckConstraint("CK_Contract_Period", "[ContractEndDate] IS NULL OR [ContractEndDate] >= [ContractDate]");
                    table.ForeignKey(
                        name: "FK_Contract_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "core",
                        principalTable: "Company",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Contract_Partner_InvoicePartnerId",
                        column: x => x.InvoicePartnerId,
                        principalSchema: "core",
                        principalTable: "Partner",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Contract_Partner_OwnerPartnerId",
                        column: x => x.OwnerPartnerId,
                        principalSchema: "core",
                        principalTable: "Partner",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Contract_Partner_TenantPartnerId",
                        column: x => x.TenantPartnerId,
                        principalSchema: "core",
                        principalTable: "Partner",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "InvoiceUnit",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    InvoiceId = table.Column<int>(type: "int", nullable: false),
                    ContractId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceUnit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceUnit_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "core",
                        principalTable: "Company",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InvoiceUnit_Contract_ContractId",
                        column: x => x.ContractId,
                        principalSchema: "core",
                        principalTable: "Contract",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InvoiceUnit_Invoice_InvoiceId",
                        column: x => x.InvoiceId,
                        principalSchema: "finance",
                        principalTable: "Invoice",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PartnerAccount",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    Account = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    PartnerId = table.Column<int>(type: "int", nullable: false),
                    ContractId = table.Column<int>(type: "int", nullable: true),
                    AccountNumber = table.Column<int>(type: "int", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartnerAccount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartnerAccount_ChartOfAccounts_Account",
                        column: x => x.Account,
                        principalSchema: "finance",
                        principalTable: "ChartOfAccounts",
                        principalColumn: "Account");
                    table.ForeignKey(
                        name: "FK_PartnerAccount_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "core",
                        principalTable: "Company",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PartnerAccount_Contract_ContractId",
                        column: x => x.ContractId,
                        principalSchema: "core",
                        principalTable: "Contract",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PartnerAccount_Partner_PartnerId",
                        column: x => x.PartnerId,
                        principalSchema: "core",
                        principalTable: "Partner",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Unit",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ContractId = table.Column<int>(type: "int", nullable: true),
                    UnitTypeId = table.Column<int>(type: "int", nullable: true),
                    BuildingEntranceId = table.Column<int>(type: "int", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    SortingNumber = table.Column<int>(type: "int", nullable: true),
                    K1 = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: true),
                    K2 = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: true),
                    K3 = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: true),
                    K4 = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: true),
                    K5 = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: true),
                    FloorNumber = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Unit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Unit_BuildingEntrance_BuildingEntranceId",
                        column: x => x.BuildingEntranceId,
                        principalSchema: "core",
                        principalTable: "BuildingEntrance",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Unit_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "core",
                        principalTable: "Company",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Unit_Contract_ContractId",
                        column: x => x.ContractId,
                        principalSchema: "core",
                        principalTable: "Contract",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Unit_ShortList_UnitTypeId",
                        column: x => x.UnitTypeId,
                        principalSchema: "core",
                        principalTable: "ShortList",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "InterestStatement",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    Account = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Days = table.Column<int>(type: "int", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Coefficient = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Interest = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PartnerAccountId = table.Column<int>(type: "int", nullable: false),
                    SubAccountId = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    InvoiceBatchId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterestStatement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InterestStatement_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "core",
                        principalTable: "Company",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InterestStatement_InvoiceBatch_InvoiceBatchId",
                        column: x => x.InvoiceBatchId,
                        principalSchema: "billing",
                        principalTable: "InvoiceBatch",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InterestStatement_PartnerAccount_PartnerAccountId",
                        column: x => x.PartnerAccountId,
                        principalSchema: "core",
                        principalTable: "PartnerAccount",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Notice",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    NoticeBatchId = table.Column<int>(type: "int", nullable: false),
                    PartnerAccountId = table.Column<int>(type: "int", nullable: false),
                    UnpaidInvoiceCount = table.Column<int>(type: "int", nullable: false),
                    Debt = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    InvoiceText = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    PaymentReference = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AdditionalCosts = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DeliveryStatus = table.Column<int>(type: "int", nullable: false),
                    RenderedDocumentPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SentAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notice", x => x.Id);
                    table.CheckConstraint("CK_Notice_Amounts", "[Debt] >= 0 AND [AdditionalCosts] >= 0 AND [Total] = [Debt] + [AdditionalCosts]");
                    table.ForeignKey(
                        name: "FK_Notice_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "core",
                        principalTable: "Company",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Notice_NoticeBatch_NoticeBatchId",
                        column: x => x.NoticeBatchId,
                        principalSchema: "billing",
                        principalTable: "NoticeBatch",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Notice_PartnerAccount_PartnerAccountId",
                        column: x => x.PartnerAccountId,
                        principalSchema: "core",
                        principalTable: "PartnerAccount",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SubAccount",
                schema: "finance",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ParentSubAccountId = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    CostToSubAccountId = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    InterestSubAccountId = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DefaultSupplierPartnerAccountId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubAccount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubAccount_PartnerAccount_DefaultSupplierPartnerAccountId",
                        column: x => x.DefaultSupplierPartnerAccountId,
                        principalSchema: "core",
                        principalTable: "PartnerAccount",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SubAccount_SubAccount_CostToSubAccountId",
                        column: x => x.CostToSubAccountId,
                        principalSchema: "finance",
                        principalTable: "SubAccount",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SubAccount_SubAccount_InterestSubAccountId",
                        column: x => x.InterestSubAccountId,
                        principalSchema: "finance",
                        principalTable: "SubAccount",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SubAccount_SubAccount_ParentSubAccountId",
                        column: x => x.ParentSubAccountId,
                        principalSchema: "finance",
                        principalTable: "SubAccount",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SupplierInvoice",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    InvoiceNo = table.Column<int>(type: "int", nullable: false),
                    CodeName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Caption = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    SupplierPartnerAccountId = table.Column<int>(type: "int", nullable: false),
                    CalculationTypeId = table.Column<int>(type: "int", nullable: false),
                    PeriodYYMM = table.Column<int>(type: "int", nullable: false),
                    InvoiceTotalCalculationAmountEur = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    InvoiceTotalCalculationAmountRsd = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CalculationAmountByCoefficientEur = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CalculationAmountByCoefficientRsd = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PaymentPriority = table.Column<int>(type: "int", nullable: false),
                    SubAccountId = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    DocumentTypeId = table.Column<int>(type: "int", nullable: false),
                    ExtraordinaryInvoiceMarker = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    InvoiceNameFunction = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    PostedInvoiceAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    InvoiceDate = table.Column<DateOnly>(type: "date", nullable: false),
                    TransactionDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PaymentDate = table.Column<DateOnly>(type: "date", nullable: true),
                    InvoiceDescription = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PaymentReference = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PreviousSupplierInvoiceId = table.Column<int>(type: "int", nullable: true),
                    NewSupplierInvoiceId = table.Column<int>(type: "int", nullable: true),
                    JournalEntryId = table.Column<int>(type: "int", nullable: true),
                    ClosesAccount = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierInvoice", x => x.Id);
                    table.CheckConstraint("CK_SupplierInvoice_Amounts", "[InvoiceTotalCalculationAmountEur] >= 0 AND [InvoiceTotalCalculationAmountRsd] >= 0 AND [PostedInvoiceAmount] >= 0");
                    table.CheckConstraint("CK_SupplierInvoice_Period", "[PeriodYYMM] BETWEEN 1001 AND 9912 AND [PeriodYYMM] % 100 BETWEEN 1 AND 12");
                    table.ForeignKey(
                        name: "FK_SupplierInvoice_CalculationType_CalculationTypeId",
                        column: x => x.CalculationTypeId,
                        principalSchema: "billing",
                        principalTable: "CalculationType",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SupplierInvoice_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "core",
                        principalTable: "Company",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SupplierInvoice_PartnerAccount_SupplierPartnerAccountId",
                        column: x => x.SupplierPartnerAccountId,
                        principalSchema: "core",
                        principalTable: "PartnerAccount",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SupplierInvoice_ShortList_DocumentTypeId",
                        column: x => x.DocumentTypeId,
                        principalSchema: "core",
                        principalTable: "ShortList",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SupplierInvoice_SupplierInvoice_NewSupplierInvoiceId",
                        column: x => x.NewSupplierInvoiceId,
                        principalSchema: "billing",
                        principalTable: "SupplierInvoice",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SupplierInvoice_SupplierInvoice_PreviousSupplierInvoiceId",
                        column: x => x.PreviousSupplierInvoiceId,
                        principalSchema: "billing",
                        principalTable: "SupplierInvoice",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "NoticeLine",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NoticeId = table.Column<int>(type: "int", nullable: false),
                    Parameters = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    DocumentRef = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Debit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Credit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Sum = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Text = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    InvoiceId = table.Column<int>(type: "int", nullable: true),
                    InvoiceDate = table.Column<DateOnly>(type: "date", nullable: true),
                    UnitAddress = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoticeLine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NoticeLine_Invoice_InvoiceId",
                        column: x => x.InvoiceId,
                        principalSchema: "finance",
                        principalTable: "Invoice",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_NoticeLine_Notice_NoticeId",
                        column: x => x.NoticeId,
                        principalSchema: "billing",
                        principalTable: "Notice",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SupplierInvoiceUnitType",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierInvoiceId = table.Column<int>(type: "int", nullable: false),
                    UnitTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierInvoiceUnitType", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierInvoiceUnitType_ShortList_UnitTypeId",
                        column: x => x.UnitTypeId,
                        principalSchema: "core",
                        principalTable: "ShortList",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SupplierInvoiceUnitType_SupplierInvoice_SupplierInvoiceId",
                        column: x => x.SupplierInvoiceId,
                        principalSchema: "billing",
                        principalTable: "SupplierInvoice",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntry_BankStatementLineId",
                schema: "finance",
                table: "LedgerEntry",
                column: "BankStatementLineId");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntry_PartnerAccountId",
                schema: "finance",
                table: "LedgerEntry",
                column: "PartnerAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntry_SubAccountId",
                schema: "finance",
                table: "LedgerEntry",
                column: "SubAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntry_ReversalOfId",
                schema: "finance",
                table: "JournalEntry",
                column: "ReversalOfId",
                unique: true,
                filter: "[ReversalOfId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLine_InvoiceBatchId",
                schema: "finance",
                table: "InvoiceLine",
                column: "InvoiceBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLine_SupplierInvoiceId",
                schema: "finance",
                table: "InvoiceLine",
                column: "SupplierInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoice_CompanyId_InvoiceBatchId_PartnerId_SequenceNumber",
                schema: "finance",
                table: "Invoice",
                columns: new[] { "CompanyId", "InvoiceBatchId", "PartnerId", "SequenceNumber" },
                unique: true,
                filter: "[InvoiceBatchId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Invoice_InvoiceBatchId",
                schema: "finance",
                table: "Invoice",
                column: "InvoiceBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_EtlRun_CompanyId_SourceSystem_ContentHash",
                schema: "etl",
                table: "EtlRun",
                columns: new[] { "CompanyId", "SourceSystem", "ContentHash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisReportDefinition_CompanyId_IsActive_DataGroup_SortIndex",
                schema: "reports",
                table: "AnalysisReportDefinition",
                columns: new[] { "CompanyId", "IsActive", "DataGroup", "SortIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisReportDefinition_CompanyId_Name",
                schema: "reports",
                table: "AnalysisReportDefinition",
                columns: new[] { "CompanyId", "Name" },
                unique: true,
                filter: "[CompanyId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BankAccount_CompanyId_IsActive_SortIndex",
                schema: "core",
                table: "BankAccount",
                columns: new[] { "CompanyId", "IsActive", "SortIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_BankAccount_PartnerId",
                schema: "core",
                table: "BankAccount",
                column: "PartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_BankInFlow_BankAccountId",
                schema: "finance",
                table: "BankInFlow",
                column: "BankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_BankInFlow_BankStatementLineId",
                schema: "finance",
                table: "BankInFlow",
                column: "BankStatementLineId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BankInFlow_BankStatementLineId_CompanyId",
                schema: "finance",
                table: "BankInFlow",
                columns: new[] { "BankStatementLineId", "CompanyId" });

            migrationBuilder.CreateIndex(
                name: "IX_BankInFlow_CompanyId",
                schema: "finance",
                table: "BankInFlow",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_BankInFlow_PartnerAccountId",
                schema: "finance",
                table: "BankInFlow",
                column: "PartnerAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_BankStatement_BankAccountId",
                schema: "finance",
                table: "BankStatement",
                column: "BankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_BankStatement_CompanyId_BankAccountId_StatementNumber_StatementSuffix_Date",
                schema: "finance",
                table: "BankStatement",
                columns: new[] { "CompanyId", "BankAccountId", "StatementNumber", "StatementSuffix", "Date" },
                unique: true,
                filter: "[StatementSuffix] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BankStatement_CompanyId_Status_Date",
                schema: "finance",
                table: "BankStatement",
                columns: new[] { "CompanyId", "Status", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_BankStatement_JournalEntryId_CompanyId",
                schema: "finance",
                table: "BankStatement",
                columns: new[] { "JournalEntryId", "CompanyId" });

            migrationBuilder.CreateIndex(
                name: "IX_BankStatement_LedgerAccount",
                schema: "finance",
                table: "BankStatement",
                column: "LedgerAccount");

            migrationBuilder.CreateIndex(
                name: "IX_BankStatementLine_BankStatementId_CompanyId",
                schema: "finance",
                table: "BankStatementLine",
                columns: new[] { "BankStatementId", "CompanyId" });

            migrationBuilder.CreateIndex(
                name: "IX_BankStatementLine_BankStatementId_LineNumber",
                schema: "finance",
                table: "BankStatementLine",
                columns: new[] { "BankStatementId", "LineNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BankStatementLine_CompanyId_Status",
                schema: "finance",
                table: "BankStatementLine",
                columns: new[] { "CompanyId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_BankStatementLine_CounterAccount",
                schema: "finance",
                table: "BankStatementLine",
                column: "CounterAccount");

            migrationBuilder.CreateIndex(
                name: "IX_BankStatementLine_PartnerAccountId",
                schema: "finance",
                table: "BankStatementLine",
                column: "PartnerAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_BankStatementLine_PaymentReference",
                schema: "finance",
                table: "BankStatementLine",
                column: "PaymentReference");

            migrationBuilder.CreateIndex(
                name: "IX_BankStatementLine_SubAccountId",
                schema: "finance",
                table: "BankStatementLine",
                column: "SubAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_BankStatementPostingTemplate_CompanyId_IsActive_SortIndex",
                schema: "finance",
                table: "BankStatementPostingTemplate",
                columns: new[] { "CompanyId", "IsActive", "SortIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_BankStatementPostingTemplate_ParentId",
                schema: "finance",
                table: "BankStatementPostingTemplate",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_BankStatementPostingTemplate_SetAccountCode",
                schema: "finance",
                table: "BankStatementPostingTemplate",
                column: "SetAccountCode");

            migrationBuilder.CreateIndex(
                name: "IX_BankStatementPostingTemplate_SetPartnerAccountId",
                schema: "finance",
                table: "BankStatementPostingTemplate",
                column: "SetPartnerAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_BankStatementPostingTemplate_SetSubAccountId",
                schema: "finance",
                table: "BankStatementPostingTemplate",
                column: "SetSubAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Benefit_CompanyId_ContractId_PeriodYYMM",
                schema: "billing",
                table: "Benefit",
                columns: new[] { "CompanyId", "ContractId", "PeriodYYMM" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Benefit_ContractId",
                schema: "billing",
                table: "Benefit",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_Benefit_InvoiceId",
                schema: "billing",
                table: "Benefit",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingEntrance_AddressId",
                schema: "core",
                table: "BuildingEntrance",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingEntrance_CompanyId_SortIndex",
                schema: "core",
                table: "BuildingEntrance",
                columns: new[] { "CompanyId", "SortIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_CalculationType_Name",
                schema: "billing",
                table: "CalculationType",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CalculationType_UnitOfMeasureId",
                schema: "billing",
                table: "CalculationType",
                column: "UnitOfMeasureId");

            migrationBuilder.CreateIndex(
                name: "IX_ChartOfAccounts_IsActive_Account",
                schema: "finance",
                table: "ChartOfAccounts",
                columns: new[] { "IsActive", "Account" });

            migrationBuilder.CreateIndex(
                name: "IX_ChartOfAccounts_ParentAccount",
                schema: "finance",
                table: "ChartOfAccounts",
                column: "ParentAccount");

            migrationBuilder.CreateIndex(
                name: "IX_Contract_CompanyId_UnitId_ContractDate",
                schema: "core",
                table: "Contract",
                columns: new[] { "CompanyId", "UnitId", "ContractDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Contract_CompanyId_UnitId_IsActive",
                schema: "core",
                table: "Contract",
                columns: new[] { "CompanyId", "UnitId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Contract_InvoiceDeliveryUnitId",
                schema: "core",
                table: "Contract",
                column: "InvoiceDeliveryUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Contract_InvoicePartnerId",
                schema: "core",
                table: "Contract",
                column: "InvoicePartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Contract_OwnerPartnerId",
                schema: "core",
                table: "Contract",
                column: "OwnerPartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Contract_TenantPartnerId",
                schema: "core",
                table: "Contract",
                column: "TenantPartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Contract_UnitId",
                schema: "core",
                table: "Contract",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Document_CompanyId_CreatedAt",
                schema: "platform",
                table: "Document",
                columns: new[] { "CompanyId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Document_CompanyId_Sha256",
                schema: "platform",
                table: "Document",
                columns: new[] { "CompanyId", "Sha256" });

            migrationBuilder.CreateIndex(
                name: "IX_Document_OwnerStaffId",
                schema: "platform",
                table: "Document",
                column: "OwnerStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentCategory_Code",
                schema: "platform",
                table: "DocumentCategory",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EtlParityAssessment_EtlRunId_ArtifactKind",
                schema: "etl",
                table: "EtlParityAssessment",
                columns: new[] { "EtlRunId", "ArtifactKind" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EtlRunContext_CompanyId_SourceTable_UpdatedAt",
                schema: "etl",
                table: "EtlRunContext",
                columns: new[] { "CompanyId", "SourceTable", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Event_CompanyId_Status_RequestedAt",
                schema: "platform",
                table: "Event",
                columns: new[] { "CompanyId", "Status", "RequestedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRate_RateDateFrom",
                schema: "finance",
                table: "ExchangeRate",
                column: "RateDateFrom",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FiscalYear_CompanyId_Year",
                schema: "finance",
                table: "FiscalYear",
                columns: new[] { "CompanyId", "Year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImportDefinition_CompanyId_Code",
                schema: "platform",
                table: "ImportDefinition",
                columns: new[] { "CompanyId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImportMapping_ImportMappingGroupId",
                schema: "platform",
                table: "ImportMapping",
                column: "ImportMappingGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportMappingGroup_ImportDefinitionId",
                schema: "platform",
                table: "ImportMappingGroup",
                column: "ImportDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_InterestRate_Date_TimeCode",
                schema: "billing",
                table: "InterestRate",
                columns: new[] { "Date", "TimeCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InterestStatement_CompanyId_InvoiceBatchId_PartnerAccountId",
                schema: "billing",
                table: "InterestStatement",
                columns: new[] { "CompanyId", "InvoiceBatchId", "PartnerAccountId" });

            migrationBuilder.CreateIndex(
                name: "IX_InterestStatement_InvoiceBatchId",
                schema: "billing",
                table: "InterestStatement",
                column: "InvoiceBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_InterestStatement_PartnerAccountId",
                schema: "billing",
                table: "InterestStatement",
                column: "PartnerAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceBatch_CompanyId_PeriodYYMM_ExtraordinaryInvoiceMarker",
                schema: "billing",
                table: "InvoiceBatch",
                columns: new[] { "CompanyId", "PeriodYYMM", "ExtraordinaryInvoiceMarker" },
                unique: true,
                filter: "[ExtraordinaryInvoiceMarker] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceBatch_CompanyId_Status",
                schema: "billing",
                table: "InvoiceBatch",
                columns: new[] { "CompanyId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceBatch_StaffId",
                schema: "billing",
                table: "InvoiceBatch",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceUnit_CompanyId_ContractId",
                schema: "billing",
                table: "InvoiceUnit",
                columns: new[] { "CompanyId", "ContractId" });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceUnit_ContractId",
                schema: "billing",
                table: "InvoiceUnit",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceUnit_InvoiceId_ContractId",
                schema: "billing",
                table: "InvoiceUnit",
                columns: new[] { "InvoiceId", "ContractId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Language_IsDefault",
                schema: "platform",
                table: "Language",
                column: "IsDefault",
                unique: true,
                filter: "[IsDefault] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerSourcePosting_CompanyId_IdempotencyKey",
                schema: "finance",
                table: "LedgerSourcePosting",
                columns: new[] { "CompanyId", "IdempotencyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LedgerSourcePosting_CompanyId_SourceType_SourceId",
                schema: "finance",
                table: "LedgerSourcePosting",
                columns: new[] { "CompanyId", "SourceType", "SourceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LedgerSourcePosting_JournalEntryId_CompanyId",
                schema: "finance",
                table: "LedgerSourcePosting",
                columns: new[] { "JournalEntryId", "CompanyId" });

            migrationBuilder.CreateIndex(
                name: "IX_LocationCategory_ParentId_SortIndex",
                schema: "core",
                table: "LocationCategory",
                columns: new[] { "ParentId", "SortIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_Notice_CompanyId_NoticeBatchId_PartnerAccountId",
                schema: "billing",
                table: "Notice",
                columns: new[] { "CompanyId", "NoticeBatchId", "PartnerAccountId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notice_NoticeBatchId",
                schema: "billing",
                table: "Notice",
                column: "NoticeBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Notice_PartnerAccountId",
                schema: "billing",
                table: "Notice",
                column: "PartnerAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_NoticeBatch_CompanyId_Date_Title",
                schema: "billing",
                table: "NoticeBatch",
                columns: new[] { "CompanyId", "Date", "Title" });

            migrationBuilder.CreateIndex(
                name: "IX_NoticeBatch_InvoiceBatchId",
                schema: "billing",
                table: "NoticeBatch",
                column: "InvoiceBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_NoticeBatch_NoticeTemplateId",
                schema: "billing",
                table: "NoticeBatch",
                column: "NoticeTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_NoticeBatch_NoticeTypeId",
                schema: "billing",
                table: "NoticeBatch",
                column: "NoticeTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_NoticeLine_InvoiceId",
                schema: "billing",
                table: "NoticeLine",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_NoticeLine_NoticeId",
                schema: "billing",
                table: "NoticeLine",
                column: "NoticeId");

            migrationBuilder.CreateIndex(
                name: "IX_NoticeTemplate_CompanyId_Name",
                schema: "billing",
                table: "NoticeTemplate",
                columns: new[] { "CompanyId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartnerAccount_Account",
                schema: "core",
                table: "PartnerAccount",
                column: "Account");

            migrationBuilder.CreateIndex(
                name: "IX_PartnerAccount_CompanyId_AccountNumber",
                schema: "core",
                table: "PartnerAccount",
                columns: new[] { "CompanyId", "AccountNumber" },
                unique: true,
                filter: "[CompanyId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PartnerAccount_CompanyId_PartnerId",
                schema: "core",
                table: "PartnerAccount",
                columns: new[] { "CompanyId", "PartnerId" });

            migrationBuilder.CreateIndex(
                name: "IX_PartnerAccount_ContractId",
                schema: "core",
                table: "PartnerAccount",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_PartnerAccount_PartnerId",
                schema: "core",
                table: "PartnerAccount",
                column: "PartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_PartnerAddress_AddressId",
                schema: "core",
                table: "PartnerAddress",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_PartnerAddress_AddressTypeId",
                schema: "core",
                table: "PartnerAddress",
                column: "AddressTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PartnerAddress_PartnerId_AddressTypeId_AddressId",
                schema: "core",
                table: "PartnerAddress",
                columns: new[] { "PartnerId", "AddressTypeId", "AddressId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartnerComms_ChannelId",
                schema: "core",
                table: "PartnerComms",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_PartnerComms_PartnerId_ChannelId_ValueNormalized",
                schema: "core",
                table: "PartnerComms",
                columns: new[] { "PartnerId", "ChannelId", "ValueNormalized" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOrder_CompanyId_IsArchived_Date",
                schema: "billing",
                table: "PaymentOrder",
                columns: new[] { "CompanyId", "IsArchived", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOrder_PaymentOrderTypeId",
                schema: "billing",
                table: "PaymentOrder",
                column: "PaymentOrderTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PendingLegacyRelationship_EtlRunId_IsResolved",
                schema: "etl",
                table: "PendingLegacyRelationship",
                columns: new[] { "EtlRunId", "IsResolved" });

            migrationBuilder.CreateIndex(
                name: "IX_PendingLegacyRelationship_EtlRunId_SourceTable_SourceKey_RelationshipName",
                schema: "etl",
                table: "PendingLegacyRelationship",
                columns: new[] { "EtlRunId", "SourceTable", "SourceKey", "RelationshipName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PostingScheme_CompanyId_SourceType_IsActive",
                schema: "finance",
                table: "PostingScheme",
                columns: new[] { "CompanyId", "SourceType", "IsActive" },
                unique: true,
                filter: "[CompanyId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PostingScheme_CreditAccount",
                schema: "finance",
                table: "PostingScheme",
                column: "CreditAccount");

            migrationBuilder.CreateIndex(
                name: "IX_PostingScheme_DebitAccount",
                schema: "finance",
                table: "PostingScheme",
                column: "DebitAccount");

            migrationBuilder.CreateIndex(
                name: "IX_RawStagingRow_EtlRunId_SourceRowNumber",
                schema: "etl",
                table: "RawStagingRow",
                columns: new[] { "EtlRunId", "SourceRowNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RawStagingRow_RowHash",
                schema: "etl",
                table: "RawStagingRow",
                column: "RowHash");

            migrationBuilder.CreateIndex(
                name: "IX_RawStagingRow_SourceTable_SourceKey",
                schema: "etl",
                table: "RawStagingRow",
                columns: new[] { "SourceTable", "SourceKey" });

            migrationBuilder.CreateIndex(
                name: "IX_ReconciliationRecord_EtlRunId_Metric_Scope",
                schema: "etl",
                table: "ReconciliationRecord",
                columns: new[] { "EtlRunId", "Metric", "Scope" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReportDefinition_CompanyId_IsActive_SortIndex",
                schema: "reports",
                table: "ReportDefinition",
                columns: new[] { "CompanyId", "IsActive", "SortIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_ReportDefinition_CompanyId_Name",
                schema: "reports",
                table: "ReportDefinition",
                columns: new[] { "CompanyId", "Name" },
                unique: true,
                filter: "[CompanyId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ReportDefinitionButton_FunctionTypeId",
                schema: "reports",
                table: "ReportDefinitionButton",
                column: "FunctionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportDefinitionButton_ReportDefinitionId_SortIndex",
                schema: "reports",
                table: "ReportDefinitionButton",
                columns: new[] { "ReportDefinitionId", "SortIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReportDefinitionDetail_ReportDefinitionId_SortIndex",
                schema: "reports",
                table: "ReportDefinitionDetail",
                columns: new[] { "ReportDefinitionId", "SortIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReportExecutionAudit_CompanyId_StartedAt",
                schema: "reports",
                table: "ReportExecutionAudit",
                columns: new[] { "CompanyId", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ReportExecutionAudit_DefinitionType_DefinitionId_StartedAt",
                schema: "reports",
                table: "ReportExecutionAudit",
                columns: new[] { "DefinitionType", "DefinitionId", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ReportExecutionAudit_StaffId",
                schema: "reports",
                table: "ReportExecutionAudit",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_SelectionBasket_OwnerStaffId_CompanyId_ExpiresAt",
                schema: "platform",
                table: "SelectionBasket",
                columns: new[] { "OwnerStaffId", "CompanyId", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SelectionBasket_OwnerStaffId_CompanyId_TargetType_TargetId",
                schema: "platform",
                table: "SelectionBasket",
                columns: new[] { "OwnerStaffId", "CompanyId", "TargetType", "TargetId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SentEmail_CompanyId_Status_NextAttemptAt",
                schema: "platform",
                table: "SentEmail",
                columns: new[] { "CompanyId", "Status", "NextAttemptAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SentEmail_CreatedByStaffId",
                schema: "platform",
                table: "SentEmail",
                column: "CreatedByStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_SentEmailAttachment_DocumentId",
                schema: "platform",
                table: "SentEmailAttachment",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_SentEmailAttachment_SentEmailId_DocumentId",
                schema: "platform",
                table: "SentEmailAttachment",
                columns: new[] { "SentEmailId", "DocumentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Setting_CompanyId_Key",
                schema: "platform",
                table: "Setting",
                columns: new[] { "CompanyId", "Key" },
                unique: true,
                filter: "[CompanyId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SubAccount_CostToSubAccountId",
                schema: "finance",
                table: "SubAccount",
                column: "CostToSubAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_SubAccount_DefaultSupplierPartnerAccountId",
                schema: "finance",
                table: "SubAccount",
                column: "DefaultSupplierPartnerAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_SubAccount_InterestSubAccountId",
                schema: "finance",
                table: "SubAccount",
                column: "InterestSubAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_SubAccount_IsActive_Id",
                schema: "finance",
                table: "SubAccount",
                columns: new[] { "IsActive", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_SubAccount_ParentSubAccountId",
                schema: "finance",
                table: "SubAccount",
                column: "ParentSubAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierInvoice_CalculationTypeId",
                schema: "billing",
                table: "SupplierInvoice",
                column: "CalculationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierInvoice_CompanyId_PeriodYYMM_InvoiceNo",
                schema: "billing",
                table: "SupplierInvoice",
                columns: new[] { "CompanyId", "PeriodYYMM", "InvoiceNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupplierInvoice_DocumentTypeId",
                schema: "billing",
                table: "SupplierInvoice",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierInvoice_NewSupplierInvoiceId",
                schema: "billing",
                table: "SupplierInvoice",
                column: "NewSupplierInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierInvoice_PreviousSupplierInvoiceId",
                schema: "billing",
                table: "SupplierInvoice",
                column: "PreviousSupplierInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierInvoice_SupplierPartnerAccountId",
                schema: "billing",
                table: "SupplierInvoice",
                column: "SupplierPartnerAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierInvoiceUnitType_SupplierInvoiceId_UnitTypeId",
                schema: "billing",
                table: "SupplierInvoiceUnitType",
                columns: new[] { "SupplierInvoiceId", "UnitTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupplierInvoiceUnitType_UnitTypeId",
                schema: "billing",
                table: "SupplierInvoiceUnitType",
                column: "UnitTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Translation_CompanyId",
                schema: "platform",
                table: "Translation",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Translation_LanguageCode_ResourceKey_ResourceId_CompanyId",
                schema: "platform",
                table: "Translation",
                columns: new[] { "LanguageCode", "ResourceKey", "ResourceId", "CompanyId" },
                unique: true,
                filter: "[ResourceId] IS NOT NULL AND [CompanyId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Unit_BuildingEntranceId",
                schema: "core",
                table: "Unit",
                column: "BuildingEntranceId");

            migrationBuilder.CreateIndex(
                name: "IX_Unit_CompanyId_SortingNumber",
                schema: "core",
                table: "Unit",
                columns: new[] { "CompanyId", "SortingNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_Unit_ContractId",
                schema: "core",
                table: "Unit",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_Unit_UnitTypeId",
                schema: "core",
                table: "Unit",
                column: "UnitTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_EtlRun_Company_CompanyId",
                schema: "etl",
                table: "EtlRun",
                column: "CompanyId",
                principalSchema: "core",
                principalTable: "Company",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoice_InvoiceBatch_InvoiceBatchId",
                schema: "finance",
                table: "Invoice",
                column: "InvoiceBatchId",
                principalSchema: "billing",
                principalTable: "InvoiceBatch",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceLine_InvoiceBatch_InvoiceBatchId",
                schema: "finance",
                table: "InvoiceLine",
                column: "InvoiceBatchId",
                principalSchema: "billing",
                principalTable: "InvoiceBatch",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceLine_SupplierInvoice_SupplierInvoiceId",
                schema: "finance",
                table: "InvoiceLine",
                column: "SupplierInvoiceId",
                principalSchema: "billing",
                principalTable: "SupplierInvoice",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LedgerEntry_BankStatementLine_BankStatementLineId",
                schema: "finance",
                table: "LedgerEntry",
                column: "BankStatementLineId",
                principalSchema: "finance",
                principalTable: "BankStatementLine",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LedgerEntry_PartnerAccount_PartnerAccountId",
                schema: "finance",
                table: "LedgerEntry",
                column: "PartnerAccountId",
                principalSchema: "core",
                principalTable: "PartnerAccount",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LedgerEntry_SubAccount_SubAccountId",
                schema: "finance",
                table: "LedgerEntry",
                column: "SubAccountId",
                principalSchema: "finance",
                principalTable: "SubAccount",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BankInFlow_BankStatementLine_BankStatementLineId_CompanyId",
                schema: "finance",
                table: "BankInFlow",
                columns: new[] { "BankStatementLineId", "CompanyId" },
                principalSchema: "finance",
                principalTable: "BankStatementLine",
                principalColumns: new[] { "Id", "CompanyId" });

            migrationBuilder.AddForeignKey(
                name: "FK_BankInFlow_PartnerAccount_PartnerAccountId",
                schema: "finance",
                table: "BankInFlow",
                column: "PartnerAccountId",
                principalSchema: "core",
                principalTable: "PartnerAccount",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BankStatementLine_PartnerAccount_PartnerAccountId",
                schema: "finance",
                table: "BankStatementLine",
                column: "PartnerAccountId",
                principalSchema: "core",
                principalTable: "PartnerAccount",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BankStatementLine_SubAccount_SubAccountId",
                schema: "finance",
                table: "BankStatementLine",
                column: "SubAccountId",
                principalSchema: "finance",
                principalTable: "SubAccount",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BankStatementPostingTemplate_PartnerAccount_SetPartnerAccountId",
                schema: "finance",
                table: "BankStatementPostingTemplate",
                column: "SetPartnerAccountId",
                principalSchema: "core",
                principalTable: "PartnerAccount",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BankStatementPostingTemplate_SubAccount_SetSubAccountId",
                schema: "finance",
                table: "BankStatementPostingTemplate",
                column: "SetSubAccountId",
                principalSchema: "finance",
                principalTable: "SubAccount",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Benefit_Contract_ContractId",
                schema: "billing",
                table: "Benefit",
                column: "ContractId",
                principalSchema: "core",
                principalTable: "Contract",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Contract_Unit_InvoiceDeliveryUnitId",
                schema: "core",
                table: "Contract",
                column: "InvoiceDeliveryUnitId",
                principalSchema: "core",
                principalTable: "Unit",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Contract_Unit_UnitId",
                schema: "core",
                table: "Contract",
                column: "UnitId",
                principalSchema: "core",
                principalTable: "Unit",
                principalColumn: "Id");

            migrationBuilder.Sql(LedgerBankingDbGuardSql.CreateJournalGuard);
            migrationBuilder.Sql(LedgerBankingDbGuardSql.CreateLedgerGuard);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(LedgerBankingDbGuardSql.DropGuards);

            migrationBuilder.DropForeignKey(
                name: "FK_EtlRun_Company_CompanyId",
                schema: "etl",
                table: "EtlRun");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoice_InvoiceBatch_InvoiceBatchId",
                schema: "finance",
                table: "Invoice");

            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceLine_InvoiceBatch_InvoiceBatchId",
                schema: "finance",
                table: "InvoiceLine");

            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceLine_SupplierInvoice_SupplierInvoiceId",
                schema: "finance",
                table: "InvoiceLine");

            migrationBuilder.DropForeignKey(
                name: "FK_LedgerEntry_BankStatementLine_BankStatementLineId",
                schema: "finance",
                table: "LedgerEntry");

            migrationBuilder.DropForeignKey(
                name: "FK_LedgerEntry_PartnerAccount_PartnerAccountId",
                schema: "finance",
                table: "LedgerEntry");

            migrationBuilder.DropForeignKey(
                name: "FK_LedgerEntry_SubAccount_SubAccountId",
                schema: "finance",
                table: "LedgerEntry");

            migrationBuilder.DropForeignKey(
                name: "FK_Unit_Contract_ContractId",
                schema: "core",
                table: "Unit");

            migrationBuilder.DropTable(
                name: "AnalysisReportDefinition",
                schema: "reports");

            migrationBuilder.DropTable(
                name: "BankInFlow",
                schema: "finance");

            migrationBuilder.DropTable(
                name: "BankStatementPostingTemplate",
                schema: "finance");

            migrationBuilder.DropTable(
                name: "Benefit",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "DocumentCategory",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "EtlParityAssessment",
                schema: "etl");

            migrationBuilder.DropTable(
                name: "EtlRunContext",
                schema: "etl");

            migrationBuilder.DropTable(
                name: "Event",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "ExchangeRate",
                schema: "finance");

            migrationBuilder.DropTable(
                name: "FiscalYear",
                schema: "finance");

            migrationBuilder.DropTable(
                name: "ImportMapping",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "InterestRate",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "InterestStatement",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "InvoiceUnit",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "LedgerSourcePosting",
                schema: "finance");

            migrationBuilder.DropTable(
                name: "LocationCategory",
                schema: "core");

            migrationBuilder.DropTable(
                name: "NoticeLine",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "PartnerAddress",
                schema: "core");

            migrationBuilder.DropTable(
                name: "PartnerComms",
                schema: "core");

            migrationBuilder.DropTable(
                name: "PaymentOrder",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "PendingLegacyRelationship",
                schema: "etl");

            migrationBuilder.DropTable(
                name: "PostingScheme",
                schema: "finance");

            migrationBuilder.DropTable(
                name: "RawStagingRow",
                schema: "etl");

            migrationBuilder.DropTable(
                name: "ReconciliationRecord",
                schema: "etl");

            migrationBuilder.DropTable(
                name: "ReportDefinitionButton",
                schema: "reports");

            migrationBuilder.DropTable(
                name: "ReportDefinitionDetail",
                schema: "reports");

            migrationBuilder.DropTable(
                name: "ReportExecutionAudit",
                schema: "reports");

            migrationBuilder.DropTable(
                name: "SelectionBasket",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "SentEmailAttachment",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "Setting",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "SupplierInvoiceUnitType",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "Translation",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "BankStatementLine",
                schema: "finance");

            migrationBuilder.DropTable(
                name: "ImportMappingGroup",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "Notice",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "ReportDefinition",
                schema: "reports");

            migrationBuilder.DropTable(
                name: "Document",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "SentEmail",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "SupplierInvoice",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "Language",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "BankStatement",
                schema: "finance");

            migrationBuilder.DropTable(
                name: "SubAccount",
                schema: "finance");

            migrationBuilder.DropTable(
                name: "ImportDefinition",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "NoticeBatch",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "CalculationType",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "BankAccount",
                schema: "core");

            migrationBuilder.DropTable(
                name: "PartnerAccount",
                schema: "core");

            migrationBuilder.DropTable(
                name: "InvoiceBatch",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "NoticeTemplate",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "ChartOfAccounts",
                schema: "finance");

            migrationBuilder.DropTable(
                name: "Contract",
                schema: "core");

            migrationBuilder.DropTable(
                name: "Unit",
                schema: "core");

            migrationBuilder.DropTable(
                name: "BuildingEntrance",
                schema: "core");

            migrationBuilder.DropIndex(
                name: "IX_LedgerEntry_BankStatementLineId",
                schema: "finance",
                table: "LedgerEntry");

            migrationBuilder.DropIndex(
                name: "IX_LedgerEntry_PartnerAccountId",
                schema: "finance",
                table: "LedgerEntry");

            migrationBuilder.DropIndex(
                name: "IX_LedgerEntry_SubAccountId",
                schema: "finance",
                table: "LedgerEntry");

            migrationBuilder.DropIndex(
                name: "IX_JournalEntry_ReversalOfId",
                schema: "finance",
                table: "JournalEntry");

            migrationBuilder.DropIndex(
                name: "IX_InvoiceLine_InvoiceBatchId",
                schema: "finance",
                table: "InvoiceLine");

            migrationBuilder.DropIndex(
                name: "IX_InvoiceLine_SupplierInvoiceId",
                schema: "finance",
                table: "InvoiceLine");

            migrationBuilder.DropIndex(
                name: "IX_Invoice_CompanyId_InvoiceBatchId_PartnerId_SequenceNumber",
                schema: "finance",
                table: "Invoice");

            migrationBuilder.DropIndex(
                name: "IX_Invoice_InvoiceBatchId",
                schema: "finance",
                table: "Invoice");

            migrationBuilder.DropIndex(
                name: "IX_EtlRun_CompanyId_SourceSystem_ContentHash",
                schema: "etl",
                table: "EtlRun");

            migrationBuilder.DropColumn(
                name: "BankStatementLineId",
                schema: "finance",
                table: "LedgerEntry");

            migrationBuilder.DropColumn(
                name: "PartnerAccountId",
                schema: "finance",
                table: "LedgerEntry");

            migrationBuilder.DropColumn(
                name: "SubAccountId",
                schema: "finance",
                table: "LedgerEntry");

            migrationBuilder.DropColumn(
                name: "Currency",
                schema: "finance",
                table: "InvoiceLine");

            migrationBuilder.DropColumn(
                name: "InvoiceBatchId",
                schema: "finance",
                table: "InvoiceLine");

            migrationBuilder.DropColumn(
                name: "PartnerId",
                schema: "finance",
                table: "InvoiceLine");

            migrationBuilder.DropColumn(
                name: "SupplierInvoiceId",
                schema: "finance",
                table: "InvoiceLine");

            migrationBuilder.DropColumn(
                name: "BalanceAsOfDate",
                schema: "finance",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "CancelReason",
                schema: "finance",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "DeliveryLocation",
                schema: "finance",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "InvoiceBatchId",
                schema: "finance",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "InvoiceDeliveryUnitId",
                schema: "finance",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "InvoiceLayoutId",
                schema: "finance",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "InvoiceLegacyMasterId",
                schema: "finance",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "InvoiceParentId",
                schema: "finance",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "Note",
                schema: "finance",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "NoticeId",
                schema: "finance",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "PageCount",
                schema: "finance",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "Pak",
                schema: "finance",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "PaymentReference",
                schema: "finance",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "PlaceOfIssue",
                schema: "finance",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "PreviousBalance",
                schema: "finance",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "PrintNote",
                schema: "finance",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "ServiceDateFrom",
                schema: "finance",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "ServiceDateTo",
                schema: "finance",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "SortIndex",
                schema: "finance",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "TransactionDate",
                schema: "finance",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                schema: "etl",
                table: "EtlRun");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntry_ReversalOfId",
                schema: "finance",
                table: "JournalEntry",
                column: "ReversalOfId");

            migrationBuilder.CreateIndex(
                name: "IX_EtlRun_SourceSystem_ContentHash",
                schema: "etl",
                table: "EtlRun",
                columns: new[] { "SourceSystem", "ContentHash" },
                unique: true);
        }
    }
}
