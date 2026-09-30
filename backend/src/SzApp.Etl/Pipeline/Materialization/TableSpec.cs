using SzApp.Data.Entities;
using SzApp.Data.Entities.Billing;
using SzApp.Data.Entities.LedgerBanking;

namespace SzApp.Etl.Pipeline.Materialization;

/// <summary>How the target row's primary key relates to the CSV key column.</summary>
public enum KeyMode
{
    /// <summary>IDENTITY, new id per row; legacy key lives only in LegacyKeyMap.</summary>
    Identity,
    /// <summary>ETL-03: legacy id is kept as the real id (IDENTITY_INSERT) — Company, CalculationType.</summary>
    Explicit,
    /// <summary>The key is a natural business key (ChartAccount.Account, SubAccount.Id, Language.Code).</summary>
    Natural
}

/// <summary>
/// A CSV column that holds a *legacy* key of another imported table, resolved through
/// LegacyKeyMap (ETL-04). <paramref name="ShortListTable"/> set = ShortList code, which also
/// resolves by (TableName, IndexValue) when the value isn't a mapped ShortList id.
/// </summary>
public sealed record ForeignKey(string Property, string TargetTable, string? ShortListTable = null, string? Column = null)
{
    public string CsvColumn => Column ?? Property;
}

/// <summary>
/// Declarative mapping of one canonical CSV table onto its EF entity. Column names are the EF
/// property names (shadow properties included) — the same names ExportCatalog writes, so an
/// export re-imports as-is; <see cref="Aliases"/> covers the few renamed columns.
/// </summary>
public sealed record TableSpec(
    string Table,
    Type Entity,
    ForeignKey[] ForeignKeys,
    KeyMode Key = KeyMode.Identity,
    IReadOnlyDictionary<string, string>? Aliases = null,
    IReadOnlyCollection<string>? Ignore = null,
    IReadOnlyCollection<string>? Only = null,
    // Required CompanyId missing from the CSV (older canonical examples omit it) is taken from
    // this FK's parent row.
    string? CompanyFrom = null,
    int ChunkSize = 1000)
{
    public string KeyColumn => LegacyImportTopology.Find(Table)?.KeyColumn ?? "Id";
}

public static class TableSpecs
{
    private static readonly string[] SkippedEverywhere = ["RowVersion"];

    private static readonly TableSpec[] Specs =
    [
        new("ShortList", typeof(ShortList), []),
        new("Languages", typeof(Language), [], Key: KeyMode.Natural),
        new("ChartAccount", typeof(ChartAccount), [new("ParentAccount", "ChartAccount")], Key: KeyMode.Natural),
        new("SubAccount", typeof(SubAccount),
            [new("ParentSubAccountId", "SubAccount"), new("CostToSubAccountId", "SubAccount"), new("InterestSubAccountId", "SubAccount")],
            Key: KeyMode.Natural, Ignore: ["DefaultSupplierPartnerAccountId"]),
        new("CalculationType", typeof(CalculationType), [new("UnitOfMeasureId", "ShortList", "UnitOfMeasure")], Key: KeyMode.Explicit),
        new("InterestRate", typeof(InterestRate), []),
        new("Address", typeof(Address), [], Aliases: new Dictionary<string, string> { ["StreetAddress"] = "Address" }),
        new("Partner", typeof(Partner),
            [new("CompanyId", "Company"), new("PartnerTypeId", "ShortList", "PartnerType")]),
        new("Company", typeof(Company),
            [new("PartnerId", "Partner"), new("ManagerId", "Partner"),
             new("CompanyTypeId", "ShortList", "CompanyType"), new("VatTypeId", "ShortList", "VatType")],
            Key: KeyMode.Explicit, Ignore: ["LocationCategoryId"]),
        new("Staff", typeof(ApplicationUser), [],
            Aliases: new Dictionary<string, string> { ["LastIp"] = "LastIP", ["LastLoginAt"] = "LastLoginTimeStamp" },
            Only: ["UserName", "Email", "PreferredLanguage", "LastIp", "LastLoginAt", "IsActive"]),
        new("BuildingEntrance", typeof(BuildingEntrance),
            [new("CompanyId", "Company"), new("AddressId", "Address", Column: "AdressId")]),
        new("Unit", typeof(Unit),
            [new("CompanyId", "Company"), new("ContractId", "Contract"), new("UnitTypeId", "ShortList", "UnitType"),
             new("BuildingEntranceId", "BuildingEntrance")]),
        new("Contract", typeof(Contract),
            [new("CompanyId", "Company"), new("UnitId", "Unit"), new("OwnerPartnerId", "Partner"), new("InvoicePartnerId", "Partner"),
             new("TenantPartnerId", "Partner", Column: "TenetPartnerId"), new("InvoiceDeliveryUnitId", "Unit")],
            Aliases: new Dictionary<string, string> { ["IsPrintInvoiceSkipped"] = "IsPrintInvoiceSkiped" },
            CompanyFrom: "UnitId"),
        new("PartnerAccount", typeof(PartnerAccount),
            [new("CompanyId", "Company"), new("PartnerId", "Partner"), new("ContractId", "Contract"), new("Account", "ChartAccount")]),
        new("BankAccount", typeof(BankAccount), [new("CompanyId", "Company"), new("PartnerId", "Partner")]),
        new("JournalEntry", typeof(JournalEntry),
            [new("CompanyId", "Company"), new("PostedUserId", "Staff"), new("ReversalOfId", "JournalEntry"),
             new("JournalEntryTypeId", "ShortList", "LedgerLineType")],
            Aliases: new Dictionary<string, string> { ["PostedAt"] = "PostedDate" },
            // ETL-14: Nalog.Saldo is always 0 in Access (bug) — never imported. IsPosted is forced.
            Ignore: ["Balance", "IsPosted"]),
        new("InvoiceBatch", typeof(InvoiceBatch),
            [new("CompanyId", "Company"), new("StaffId", "Staff"), new("JournalEntryId", "JournalEntry")]),
        new("Invoice", typeof(Invoice),
            [new("CompanyId", "Company"), new("PartnerId", "Partner"), new("InvoiceBatchId", "InvoiceBatch"),
             new("InvoiceDeliveryUnitId", "Unit")],
            Aliases: new Dictionary<string, string> { ["CancelledAt"] = "CancelledDate", ["Pak"] = "PAK" },
            Ignore: ["NoticeId", "InvoiceParentId"], CompanyFrom: "InvoiceBatchId"),
        new("SupplierInvoice", typeof(SupplierInvoice),
            [new("CompanyId", "Company"), new("SupplierPartnerAccountId", "PartnerAccount"), new("CalculationTypeId", "CalculationType"),
             new("SubAccountId", "SubAccount"), new("DocumentTypeId", "ShortList", "SupplierDocumentType"),
             new("PreviousSupplierInvoiceId", "SupplierInvoice"), new("NewSupplierInvoiceId", "SupplierInvoice"),
             new("JournalEntryId", "JournalEntry")]),
        new("SupplierInvoiceUnitType", typeof(SupplierInvoiceUnitType),
            [new("SupplierInvoiceId", "SupplierInvoice"), new("UnitTypeId", "ShortList", "UnitType")]),
        new("InvoiceLine", typeof(InvoiceLine),
            [new("InvoiceId", "Invoice"), new("CompanyId", "Company"), new("UnitOfMeasureId", "ShortList", "UnitOfMeasure"),
             new("InvoiceBatchId", "InvoiceBatch"), new("PartnerId", "Partner"), new("SupplierInvoiceId", "SupplierInvoice")],
            CompanyFrom: "InvoiceId", ChunkSize: 2000),
        new("BankStatement", typeof(BankStatement),
            [new("CompanyId", "Company"), new("BankAccountId", "BankAccount"), new("JournalEntryId", "JournalEntry"),
             new("LedgerAccount", "ChartAccount")],
            CompanyFrom: "BankAccountId"),
        new("BankStatementLine", typeof(BankStatementLine),
            [new("CompanyId", "Company"), new("BankStatementId", "BankStatement"), new("PartnerAccountId", "PartnerAccount"),
             new("SubAccountId", "SubAccount"), new("CounterAccount", "ChartAccount")],
            CompanyFrom: "BankStatementId", ChunkSize: 2000),
        new("LedgerEntry", typeof(LedgerEntry),
            [new("JournalEntryId", "JournalEntry"), new("CompanyId", "Company"), new("LineTypeId", "ShortList", "LedgerLineType"),
             new("PartnerAccountId", "PartnerAccount"), new("BankStatementLineId", "BankStatementLine"),
             new("SubAccountId", "SubAccount"), new("InvoiceId", "Invoice"), new("SupplierInvoiceId", "SupplierInvoice")],
            CompanyFrom: "JournalEntryId", ChunkSize: 2000),
        new("NoticeBatch", typeof(NoticeBatch),
            [new("CompanyId", "Company"), new("NoticeTypeId", "ShortList", "NoticeType"), new("InvoiceBatchId", "InvoiceBatch")],
            // NoticeTemplate isn't a legacy table (see schema-ddl-draft) — a per-company placeholder is used.
            Ignore: ["NoticeTemplateId"]),
        new("Notice", typeof(Notice),
            [new("CompanyId", "Company"), new("NoticeBatchId", "NoticeBatch"), new("PartnerAccountId", "PartnerAccount")],
            Aliases: new Dictionary<string, string> { ["AdditionalCosts"] = "AditionalCosts" },
            Ignore: ["RenderedDocumentPath"], CompanyFrom: "NoticeBatchId"),
        new("InterestStatement", typeof(InterestStatement),
            [new("CompanyId", "Company"), new("PartnerAccountId", "PartnerAccount"), new("SubAccountId", "SubAccount"),
             new("InvoiceBatchId", "InvoiceBatch")],
            ChunkSize: 2000),
    ];

    private static readonly Dictionary<string, TableSpec> ByTable =
        Specs.ToDictionary(x => x.Table, StringComparer.OrdinalIgnoreCase);

    public static IReadOnlyCollection<TableSpec> All => Specs;

    public static TableSpec? Find(string table) => ByTable.GetValueOrDefault(table);

    /// <summary>False only for SentEmail / Documents, which still stage without a live table.</summary>
    public static bool IsMaterialized(string table) => ByTable.ContainsKey(table);

    public static bool IsSkipped(string property) => SkippedEverywhere.Contains(property);
}
