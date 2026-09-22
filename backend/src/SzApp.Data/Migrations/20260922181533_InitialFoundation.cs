using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SzApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "core");

            migrationBuilder.EnsureSchema(
                name: "ops");

            migrationBuilder.EnsureSchema(
                name: "etl");

            migrationBuilder.EnsureSchema(
                name: "finance");

            migrationBuilder.EnsureSchema(
                name: "auth");

            migrationBuilder.CreateTable(
                name: "Address",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PostalCode = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    City = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CountryCode = table.Column<string>(type: "varchar(2)", unicode: false, maxLength: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Address", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EtlRun",
                schema: "etl",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceSystem = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    SourceFile = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ContentHash = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    SourceRowCount = table.Column<int>(type: "int", nullable: false),
                    ImportedRowCount = table.Column<int>(type: "int", nullable: false),
                    QuarantinedRowCount = table.Column<int>(type: "int", nullable: false),
                    Error = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EtlRun", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Role",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShortList",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TableName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Caption = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ShortName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IndexValue = table.Column<int>(type: "int", nullable: false),
                    IndexSort = table.Column<int>(type: "int", nullable: false),
                    IndexKey = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShortList", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Staff",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PreferredLanguage = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    LastIp = table.Column<string>(type: "varchar(45)", unicode: false, maxLength: 45, nullable: true),
                    LastLoginAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Staff", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LegacyKeyMap",
                schema: "etl",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EtlRunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceTable = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    SourceKey = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TargetTable = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    TargetKey = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegacyKeyMap", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LegacyKeyMap_EtlRun_EtlRunId",
                        column: x => x.EtlRunId,
                        principalSchema: "etl",
                        principalTable: "EtlRun",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "QuarantineRecord",
                schema: "etl",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EtlRunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceTable = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    SourceKey = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    RawJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ErrorCode = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuarantineRecord", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuarantineRecord_EtlRun_EtlRunId",
                        column: x => x.EtlRunId,
                        principalSchema: "etl",
                        principalTable: "EtlRun",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RoleClaim",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleClaim", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleClaim_Role_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "auth",
                        principalTable: "Role",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StaffClaim",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffClaim", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffClaim_Staff_UserId",
                        column: x => x.UserId,
                        principalSchema: "auth",
                        principalTable: "Staff",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StaffLogin",
                schema: "auth",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffLogin", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_StaffLogin_Staff_UserId",
                        column: x => x.UserId,
                        principalSchema: "auth",
                        principalTable: "Staff",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StaffRole",
                schema: "auth",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffRole", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_StaffRole_Role_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "auth",
                        principalTable: "Role",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StaffRole_Staff_UserId",
                        column: x => x.UserId,
                        principalSchema: "auth",
                        principalTable: "Staff",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StaffToken",
                schema: "auth",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffToken", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_StaffToken_Staff_UserId",
                        column: x => x.UserId,
                        principalSchema: "auth",
                        principalTable: "Staff",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AuditLog",
                schema: "ops",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    StaffId = table.Column<int>(type: "int", nullable: true),
                    Timestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CorrelationId = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    EntityType = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    ItemId = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Action = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    EventSource = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    DetailsJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLog_Staff_StaffId",
                        column: x => x.StaffId,
                        principalSchema: "auth",
                        principalTable: "Staff",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Company",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartnerId = table.Column<int>(type: "int", nullable: false),
                    ManagerId = table.Column<int>(type: "int", nullable: true),
                    ShortName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PrintName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RelativeFolderName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CompanyTypeId = table.Column<int>(type: "int", nullable: true),
                    VatTypeId = table.Column<int>(type: "int", nullable: true),
                    LedgerEntryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Company", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Company_ShortList_CompanyTypeId",
                        column: x => x.CompanyTypeId,
                        principalSchema: "core",
                        principalTable: "ShortList",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Company_ShortList_VatTypeId",
                        column: x => x.VatTypeId,
                        principalSchema: "core",
                        principalTable: "ShortList",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "IdempotencyRequest",
                schema: "ops",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StaffId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    Key = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    RequestHash = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    ResponseStatusCode = table.Column<int>(type: "int", nullable: true),
                    ResponseJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdempotencyRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IdempotencyRequest_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "core",
                        principalTable: "Company",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IdempotencyRequest_Staff_StaffId",
                        column: x => x.StaffId,
                        principalSchema: "auth",
                        principalTable: "Staff",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "JournalEntry",
                schema: "finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    PostingDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Balance = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    JournalEntryTypeId = table.Column<int>(type: "int", nullable: true),
                    Currency = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    IsPosted = table.Column<bool>(type: "bit", nullable: false),
                    PostedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    PostedUserId = table.Column<int>(type: "int", nullable: true),
                    ReversalOfId = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalEntry", x => x.Id);
                    table.UniqueConstraint("AK_JournalEntry_Id_CompanyId", x => new { x.Id, x.CompanyId });
                    table.CheckConstraint("CK_JournalEntry_Posted", "[IsPosted] = 0 OR ([PostedAt] IS NOT NULL AND [PostedUserId] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_JournalEntry_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "core",
                        principalTable: "Company",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_JournalEntry_JournalEntry_ReversalOfId",
                        column: x => x.ReversalOfId,
                        principalSchema: "finance",
                        principalTable: "JournalEntry",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_JournalEntry_ShortList_JournalEntryTypeId",
                        column: x => x.JournalEntryTypeId,
                        principalSchema: "core",
                        principalTable: "ShortList",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_JournalEntry_Staff_PostedUserId",
                        column: x => x.PostedUserId,
                        principalSchema: "auth",
                        principalTable: "Staff",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessage",
                schema: "ops",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Type = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    DedupeKey = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CorrelationId = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    AttemptCount = table.Column<int>(type: "int", nullable: false),
                    LastError = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OutboxMessage_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "core",
                        principalTable: "Company",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Partner",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    ShortName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    RegistrationNumber = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    TaxNumber = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    Jbkjs = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    IdCardNumber = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    Jmbg = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: true),
                    PartnerTypeId = table.Column<int>(type: "int", nullable: true),
                    Language = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Partner", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Partner_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "core",
                        principalTable: "Company",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Partner_ShortList_PartnerTypeId",
                        column: x => x.PartnerTypeId,
                        principalSchema: "core",
                        principalTable: "ShortList",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StaffAccess",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StaffId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    StaffRole = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffAccess", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffAccess_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "core",
                        principalTable: "Company",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StaffAccess_Staff_StaffId",
                        column: x => x.StaffId,
                        principalSchema: "auth",
                        principalTable: "Staff",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LedgerEntry",
                schema: "finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JournalEntryId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    Account = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    PostingDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    DebitAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CreditAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LineTypeId = table.Column<int>(type: "int", nullable: true),
                    DocumentRef = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Parameters = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LedgerEntry", x => x.Id);
                    table.CheckConstraint("CK_LedgerEntry_Amounts", "[DebitAmount] >= 0 AND [CreditAmount] >= 0 AND NOT ([DebitAmount] > 0 AND [CreditAmount] > 0)");
                    table.ForeignKey(
                        name: "FK_LedgerEntry_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "core",
                        principalTable: "Company",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LedgerEntry_JournalEntry_JournalEntryId_CompanyId",
                        columns: x => new { x.JournalEntryId, x.CompanyId },
                        principalSchema: "finance",
                        principalTable: "JournalEntry",
                        principalColumns: new[] { "Id", "CompanyId" });
                    table.ForeignKey(
                        name: "FK_LedgerEntry_ShortList_LineTypeId",
                        column: x => x.LineTypeId,
                        principalSchema: "core",
                        principalTable: "ShortList",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Invoice",
                schema: "finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    PartnerId = table.Column<int>(type: "int", nullable: false),
                    SequenceNumber = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    IssueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PartnerName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PostalCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    City = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TaxNumber = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    RegistrationNumber = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Currency = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    VatRate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    VatAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    InterestAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    InvoiceTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    InvoiceDeliveryLocation = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CancelledAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsCancelled = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoice", x => x.Id);
                    table.UniqueConstraint("AK_Invoice_Id_CompanyId", x => new { x.Id, x.CompanyId });
                    table.CheckConstraint("CK_Invoice_Totals", "[Amount] >= 0 AND [VatAmount] >= 0 AND [InterestAmount] >= 0 AND [InvoiceTotal] = [Total] + [InterestAmount]");
                    table.ForeignKey(
                        name: "FK_Invoice_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "core",
                        principalTable: "Company",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Invoice_Partner_PartnerId",
                        column: x => x.PartnerId,
                        principalSchema: "core",
                        principalTable: "Partner",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "InvoiceLine",
                schema: "finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    K1 = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    K2 = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    K3 = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    K4 = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    K5 = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitOfMeasureId = table.Column<int>(type: "int", nullable: true),
                    PriceEur = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ExchangeRateNbs = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PricePcs = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PriceTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    VatRate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    VatAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SortIndex = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceLine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceLine_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "core",
                        principalTable: "Company",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InvoiceLine_Invoice_InvoiceId_CompanyId",
                        columns: x => new { x.InvoiceId, x.CompanyId },
                        principalSchema: "finance",
                        principalTable: "Invoice",
                        principalColumns: new[] { "Id", "CompanyId" });
                    table.ForeignKey(
                        name: "FK_InvoiceLine_ShortList_UnitOfMeasureId",
                        column: x => x.UnitOfMeasureId,
                        principalSchema: "core",
                        principalTable: "ShortList",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Address_PostalCode_City",
                schema: "core",
                table: "Address",
                columns: new[] { "PostalCode", "City" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_CompanyId_Timestamp",
                schema: "ops",
                table: "AuditLog",
                columns: new[] { "CompanyId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_CorrelationId",
                schema: "ops",
                table: "AuditLog",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_StaffId",
                schema: "ops",
                table: "AuditLog",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Company_CompanyTypeId",
                schema: "core",
                table: "Company",
                column: "CompanyTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Company_ManagerId",
                schema: "core",
                table: "Company",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Company_PartnerId",
                schema: "core",
                table: "Company",
                column: "PartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Company_ShortName",
                schema: "core",
                table: "Company",
                column: "ShortName");

            migrationBuilder.CreateIndex(
                name: "IX_Company_VatTypeId",
                schema: "core",
                table: "Company",
                column: "VatTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_EtlRun_SourceSystem_ContentHash",
                schema: "etl",
                table: "EtlRun",
                columns: new[] { "SourceSystem", "ContentHash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IdempotencyRequest_CompanyId",
                schema: "ops",
                table: "IdempotencyRequest",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_IdempotencyRequest_ExpiresAt",
                schema: "ops",
                table: "IdempotencyRequest",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_IdempotencyRequest_StaffId_CompanyId_Key",
                schema: "ops",
                table: "IdempotencyRequest",
                columns: new[] { "StaffId", "CompanyId", "Key" },
                unique: true,
                filter: "[CompanyId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Invoice_CompanyId_IssueDate",
                schema: "finance",
                table: "Invoice",
                columns: new[] { "CompanyId", "IssueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Invoice_CompanyId_SequenceNumber",
                schema: "finance",
                table: "Invoice",
                columns: new[] { "CompanyId", "SequenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoice_PartnerId",
                schema: "finance",
                table: "Invoice",
                column: "PartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLine_CompanyId",
                schema: "finance",
                table: "InvoiceLine",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLine_InvoiceId_CompanyId",
                schema: "finance",
                table: "InvoiceLine",
                columns: new[] { "InvoiceId", "CompanyId" });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLine_InvoiceId_SortIndex",
                schema: "finance",
                table: "InvoiceLine",
                columns: new[] { "InvoiceId", "SortIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLine_UnitOfMeasureId",
                schema: "finance",
                table: "InvoiceLine",
                column: "UnitOfMeasureId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntry_CompanyId_IsPosted",
                schema: "finance",
                table: "JournalEntry",
                columns: new[] { "CompanyId", "IsPosted" });

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntry_CompanyId_PostingDate",
                schema: "finance",
                table: "JournalEntry",
                columns: new[] { "CompanyId", "PostingDate" });

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntry_JournalEntryTypeId",
                schema: "finance",
                table: "JournalEntry",
                column: "JournalEntryTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntry_PostedUserId",
                schema: "finance",
                table: "JournalEntry",
                column: "PostedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntry_ReversalOfId",
                schema: "finance",
                table: "JournalEntry",
                column: "ReversalOfId");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntry_CompanyId_PostingDate",
                schema: "finance",
                table: "LedgerEntry",
                columns: new[] { "CompanyId", "PostingDate" });

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntry_JournalEntryId",
                schema: "finance",
                table: "LedgerEntry",
                column: "JournalEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntry_JournalEntryId_CompanyId",
                schema: "finance",
                table: "LedgerEntry",
                columns: new[] { "JournalEntryId", "CompanyId" });

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntry_LineTypeId",
                schema: "finance",
                table: "LedgerEntry",
                column: "LineTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_LegacyKeyMap_EtlRunId",
                schema: "etl",
                table: "LegacyKeyMap",
                column: "EtlRunId");

            migrationBuilder.CreateIndex(
                name: "IX_LegacyKeyMap_SourceTable_SourceKey_TargetTable",
                schema: "etl",
                table: "LegacyKeyMap",
                columns: new[] { "SourceTable", "SourceKey", "TargetTable" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_CompanyId_OccurredAt",
                schema: "ops",
                table: "OutboxMessage",
                columns: new[] { "CompanyId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_DedupeKey",
                schema: "ops",
                table: "OutboxMessage",
                column: "DedupeKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_ProcessedAt_OccurredAt",
                schema: "ops",
                table: "OutboxMessage",
                columns: new[] { "ProcessedAt", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Partner_CompanyId",
                schema: "core",
                table: "Partner",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Partner_PartnerTypeId",
                schema: "core",
                table: "Partner",
                column: "PartnerTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Partner_TaxNumber",
                schema: "core",
                table: "Partner",
                column: "TaxNumber");

            migrationBuilder.CreateIndex(
                name: "IX_QuarantineRecord_EtlRunId_SourceTable",
                schema: "etl",
                table: "QuarantineRecord",
                columns: new[] { "EtlRunId", "SourceTable" });

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "auth",
                table: "Role",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RoleClaim_RoleId",
                schema: "auth",
                table: "RoleClaim",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_ShortList_TableName_IndexKey",
                schema: "core",
                table: "ShortList",
                columns: new[] { "TableName", "IndexKey" });

            migrationBuilder.CreateIndex(
                name: "IX_ShortList_TableName_IndexValue",
                schema: "core",
                table: "ShortList",
                columns: new[] { "TableName", "IndexValue" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "auth",
                table: "Staff",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "auth",
                table: "Staff",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_StaffAccess_CompanyId_StaffRole",
                schema: "auth",
                table: "StaffAccess",
                columns: new[] { "CompanyId", "StaffRole" });

            migrationBuilder.CreateIndex(
                name: "IX_StaffAccess_StaffId_CompanyId",
                schema: "auth",
                table: "StaffAccess",
                columns: new[] { "StaffId", "CompanyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StaffClaim_UserId",
                schema: "auth",
                table: "StaffClaim",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffLogin_UserId",
                schema: "auth",
                table: "StaffLogin",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffRole_RoleId",
                schema: "auth",
                table: "StaffRole",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLog_Company_CompanyId",
                schema: "ops",
                table: "AuditLog",
                column: "CompanyId",
                principalSchema: "core",
                principalTable: "Company",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Company_Partner_ManagerId",
                schema: "core",
                table: "Company",
                column: "ManagerId",
                principalSchema: "core",
                principalTable: "Partner",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Company_Partner_PartnerId",
                schema: "core",
                table: "Company",
                column: "PartnerId",
                principalSchema: "core",
                principalTable: "Partner",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Partner_Company_CompanyId",
                schema: "core",
                table: "Partner");

            migrationBuilder.DropTable(
                name: "Address",
                schema: "core");

            migrationBuilder.DropTable(
                name: "AuditLog",
                schema: "ops");

            migrationBuilder.DropTable(
                name: "IdempotencyRequest",
                schema: "ops");

            migrationBuilder.DropTable(
                name: "InvoiceLine",
                schema: "finance");

            migrationBuilder.DropTable(
                name: "LedgerEntry",
                schema: "finance");

            migrationBuilder.DropTable(
                name: "LegacyKeyMap",
                schema: "etl");

            migrationBuilder.DropTable(
                name: "OutboxMessage",
                schema: "ops");

            migrationBuilder.DropTable(
                name: "QuarantineRecord",
                schema: "etl");

            migrationBuilder.DropTable(
                name: "RoleClaim",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "StaffAccess",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "StaffClaim",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "StaffLogin",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "StaffRole",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "StaffToken",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "Invoice",
                schema: "finance");

            migrationBuilder.DropTable(
                name: "JournalEntry",
                schema: "finance");

            migrationBuilder.DropTable(
                name: "EtlRun",
                schema: "etl");

            migrationBuilder.DropTable(
                name: "Role",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "Staff",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "Company",
                schema: "core");

            migrationBuilder.DropTable(
                name: "Partner",
                schema: "core");

            migrationBuilder.DropTable(
                name: "ShortList",
                schema: "core");
        }
    }
}
