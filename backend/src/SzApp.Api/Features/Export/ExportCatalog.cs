using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using SzApp.Data;
using SzApp.Data.Entities;
using SzApp.Data.Entities.Billing;
using SzApp.Data.Entities.LedgerBanking;

namespace SzApp.Api.Features.Export;

public enum ExportGroup { CodeLists, MasterData, Transactions }

public sealed record ExportColumn(string Name, PropertyInfo Property, bool IsPersonal);

/// <summary>
/// One CSV file. <see cref="ImportTable"/> names the ETL topology table (LegacyImportTopology) the
/// file can be re-imported as; null = export-only (the ETL has no importer for it yet).
/// </summary>
public sealed record ExportTable(
    string Name,
    ExportGroup Group,
    string? ImportTable,
    IReadOnlyList<ExportColumn> Columns,
    Func<SzAppDbContext, int[], IQueryable> Query,
    Func<SzAppDbContext, int[], CancellationToken, IAsyncEnumerable<object>> Rows,
    Func<object, bool>? RowFilter = null);

public static class ExportCatalog
{
    // Never exported, whatever the table: credentials and concurrency tokens.
    private static readonly string[] SecretFragments = ["Password", "Hash", "SecurityStamp", "ConcurrencyStamp", "RowVersion", "Token"];
    private static readonly HashSet<string> PersonalColumns = new(StringComparer.OrdinalIgnoreCase) { "Jmbg", "IdCardNumber" };
    private static readonly string[] SecretSettingFragments = ["password", "secret", "token", "key"];

    public static readonly IReadOnlyList<ExportTable> Tables =
    [
        // --- šifarnici (global code lists) ---
        Def<ShortList>("ShortList", ExportGroup.CodeLists, "ShortList", (db, _) => db.ShortLists),
        Def<Language>("Languages", ExportGroup.CodeLists, "Languages", (db, _) => db.Set<Language>()),
        Def<LocationCategory>("LocationCategory", ExportGroup.CodeLists, null, (db, _) => db.Set<LocationCategory>()),
        Def<ChartAccount>("ChartAccount", ExportGroup.CodeLists, null, (db, _) => db.Set<ChartAccount>()),
        Def<SubAccount>("SubAccount", ExportGroup.CodeLists, null, (db, _) => db.Set<SubAccount>()),
        Def<CalculationType>("CalculationType", ExportGroup.CodeLists, null, (db, _) => db.Set<CalculationType>()),
        Def<InterestRate>("InterestRate", ExportGroup.CodeLists, null, (db, _) => db.Set<InterestRate>()),
        Def<ExchangeRate>("ExchangeRate", ExportGroup.CodeLists, null, (db, _) => db.Set<ExchangeRate>()),

        // --- matični podaci ---
        Def<ApplicationUser>("Staff", ExportGroup.MasterData, "Staff",
            (db, ids) => db.Users.Where(x => x.CompanyAccess.Any(a => ids.Contains(a.CompanyId))),
            rename: new() { ["Id"] = "StaffId", ["LastIp"] = "LastIP", ["LastLoginAt"] = "LastLoginTimeStamp" },
            only: ["Id", "UserName", "PreferredLanguage", "LastIp", "LastLoginAt", "IsActive"]),
        Def<StaffAccess>("StaffAccess", ExportGroup.MasterData, null, (db, ids) => db.StaffAccess.Where(x => ids.Contains(x.CompanyId))),
        Def<Address>("Address", ExportGroup.MasterData, "Address",
            (db, ids) => db.Addresses.Where(x =>
                db.Set<BuildingEntrance>().Any(b => ids.Contains(b.CompanyId) && b.AddressId == x.Id) ||
                db.Set<PartnerAddress>().Any(p => p.AddressId == x.Id && p.Partner.CompanyId != null && ids.Contains(p.Partner.CompanyId.Value))),
            rename: new() { ["StreetAddress"] = "Address" }),
        Def<Partner>("Partner", ExportGroup.MasterData, "Partner",
            (db, ids) => db.Partners.Where(x => x.CompanyId != null && ids.Contains(x.CompanyId.Value))),
        // Partners owned by no (or another) company but referenced by the exported companies.
        Def<Partner>("Partner-shared", ExportGroup.MasterData, "Partner",
            (db, ids) => db.Partners.Where(x => (x.CompanyId == null || !ids.Contains(x.CompanyId.Value)) && (
                db.Companies.Any(c => ids.Contains(c.Id) && (c.PartnerId == x.Id || c.ManagerId == x.Id)) ||
                db.Set<Contract>().Any(c => ids.Contains(c.CompanyId) && (c.OwnerPartnerId == x.Id || c.InvoicePartnerId == x.Id || c.TenantPartnerId == x.Id)) ||
                db.Set<PartnerAccount>().Any(a => a.CompanyId != null && ids.Contains(a.CompanyId.Value) && a.PartnerId == x.Id) ||
                db.Set<BankAccount>().Any(a => a.CompanyId != null && ids.Contains(a.CompanyId.Value) && a.PartnerId == x.Id) ||
                db.Invoices.Any(i => ids.Contains(i.CompanyId) && i.PartnerId == x.Id)))),
        Def<PartnerAddress>("PartnerAddress", ExportGroup.MasterData, null,
            (db, ids) => db.Set<PartnerAddress>().Where(x => x.Partner.CompanyId != null && ids.Contains(x.Partner.CompanyId.Value))),
        Def<PartnerCommunication>("PartnerCommunication", ExportGroup.MasterData, null,
            (db, ids) => db.Set<PartnerCommunication>().Where(x => x.Partner.CompanyId != null && ids.Contains(x.Partner.CompanyId.Value))),
        Def<Company>("Company", ExportGroup.MasterData, "Company", (db, ids) => db.Companies.Where(x => ids.Contains(x.Id))),
        Def<FiscalYear>("FiscalYear", ExportGroup.MasterData, null, (db, ids) => db.Set<FiscalYear>().Where(x => ids.Contains(x.CompanyId))),
        Def<BuildingEntrance>("BuildingEntrance", ExportGroup.MasterData, "BuildingEntrance",
            (db, ids) => db.Set<BuildingEntrance>().Where(x => ids.Contains(x.CompanyId)),
            rename: new() { ["AddressId"] = "AdressId" }),
        Def<Unit>("Unit", ExportGroup.MasterData, "Unit", (db, ids) => db.Set<Unit>().Where(x => ids.Contains(x.CompanyId))),
        Def<Contract>("Contract", ExportGroup.MasterData, "Contract",
            (db, ids) => db.Set<Contract>().Where(x => ids.Contains(x.CompanyId)),
            rename: new() { ["TenantPartnerId"] = "TenetPartnerId", ["IsPrintInvoiceSkipped"] = "IsPrintInvoiceSkiped" }),
        Def<PartnerAccount>("PartnerAccount", ExportGroup.MasterData, "PartnerAccount",
            (db, ids) => db.Set<PartnerAccount>().Where(x => x.CompanyId != null && ids.Contains(x.CompanyId.Value))),
        Def<BankAccount>("BankAccount", ExportGroup.MasterData, "BankAccount",
            (db, ids) => db.Set<BankAccount>().Where(x => x.CompanyId != null && ids.Contains(x.CompanyId.Value)),
            rename: new() { ["Id"] = "BankAccountId" }),
        Def<Setting>("Settings", ExportGroup.MasterData, null,
            (db, ids) => db.Set<Setting>().Where(x => x.CompanyId != null && ids.Contains(x.CompanyId.Value)),
            rowFilter: row => row is Setting s && !IsSecretSetting(s)),

        // --- transakcije ---
        Def<InvoiceBatch>("InvoiceBatch", ExportGroup.Transactions, "InvoiceBatch", (db, ids) => db.Set<InvoiceBatch>().Where(x => ids.Contains(x.CompanyId))),
        Def<Invoice>("Invoice", ExportGroup.Transactions, "Invoice",
            (db, ids) => db.Invoices.Where(x => ids.Contains(x.CompanyId)),
            rename: new() { ["CancelledAt"] = "CancelledDate" }),
        Def<InvoiceLine>("InvoiceLine", ExportGroup.Transactions, null, (db, ids) => db.InvoiceLines.Where(x => ids.Contains(x.CompanyId))),
        Def<InvoiceUnit>("InvoiceUnit", ExportGroup.Transactions, null, (db, ids) => db.Set<InvoiceUnit>().Where(x => ids.Contains(x.CompanyId))),
        Def<Benefit>("Benefit", ExportGroup.Transactions, null, (db, ids) => db.Set<Benefit>().Where(x => ids.Contains(x.CompanyId))),
        Def<SupplierInvoice>("SupplierInvoice", ExportGroup.Transactions, "SupplierInvoice", (db, ids) => db.Set<SupplierInvoice>().Where(x => ids.Contains(x.CompanyId))),
        Def<SupplierInvoiceUnitType>("SupplierInvoiceUnitType", ExportGroup.Transactions, null,
            (db, ids) => db.Set<SupplierInvoiceUnitType>().Where(x => ids.Contains(x.SupplierInvoice.CompanyId))),
        Def<BankStatement>("BankStatement", ExportGroup.Transactions, "BankStatement", (db, ids) => db.Set<BankStatement>().Where(x => ids.Contains(x.CompanyId))),
        Def<BankStatementLine>("BankStatementLine", ExportGroup.Transactions, null, (db, ids) => db.Set<BankStatementLine>().Where(x => ids.Contains(x.CompanyId))),
        Def<BankInFlow>("BankInFlow", ExportGroup.Transactions, null, (db, ids) => db.Set<BankInFlow>().Where(x => ids.Contains(x.CompanyId))),
        Def<JournalEntry>("JournalEntry", ExportGroup.Transactions, "JournalEntry",
            (db, ids) => db.JournalEntries.Where(x => ids.Contains(x.CompanyId)),
            rename: new() { ["PostedAt"] = "PostedDate" }),
        Def<LedgerEntry>("LedgerEntry", ExportGroup.Transactions, "LedgerEntry", (db, ids) => db.LedgerEntries.Where(x => ids.Contains(x.CompanyId))),
        Def<PostingPeriodLock>("PostingPeriodLock", ExportGroup.Transactions, null, (db, ids) => db.PostingPeriodLocks.Where(x => ids.Contains(x.CompanyId))),
        Def<NoticeTemplate>("NoticeTemplate", ExportGroup.Transactions, null, (db, ids) => db.Set<NoticeTemplate>().Where(x => ids.Contains(x.CompanyId))),
        Def<NoticeBatch>("NoticeBatch", ExportGroup.Transactions, null, (db, ids) => db.Set<NoticeBatch>().Where(x => ids.Contains(x.CompanyId))),
        Def<Notice>("Notice", ExportGroup.Transactions, "Notice",
            (db, ids) => db.Set<Notice>().Where(x => ids.Contains(x.CompanyId)),
            rename: new() { ["AdditionalCosts"] = "AditionalCosts" }),
        Def<NoticeLine>("NoticeLine", ExportGroup.Transactions, null, (db, ids) => db.Set<NoticeLine>().Where(x => ids.Contains(x.Notice.CompanyId))),
        Def<InterestStatement>("InterestStatement", ExportGroup.Transactions, "InterestStatement", (db, ids) => db.Set<InterestStatement>().Where(x => ids.Contains(x.CompanyId))),
        Def<PaymentOrder>("PaymentOrder", ExportGroup.Transactions, null, (db, ids) => db.Set<PaymentOrder>().Where(x => ids.Contains(x.CompanyId))),
        Def<SentEmail>("SentEmail", ExportGroup.Transactions, "SentEmail",
            (db, ids) => db.Set<SentEmail>().Where(x => ids.Contains(x.CompanyId)),
            rename: new() { ["ToAddress"] = "To", ["CreatedAt"] = "Created", ["SentAt"] = "Sent", ["IsArchived"] = "Archive" }),
        Def<DocumentRecord>("Documents", ExportGroup.Transactions, "Documents",
            (db, ids) => db.Set<DocumentRecord>().Where(x => ids.Contains(x.CompanyId)),
            rename: new() { ["DocumentDate"] = "Date", ["CreatedAt"] = "TimeStamp" }),
    ];

    public static ExportTable? Find(string name) =>
        Tables.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));

    public static bool IsSecretSetting(Setting s) =>
        SecretSettingFragments.Any(f => s.Key.Contains(f, StringComparison.OrdinalIgnoreCase) ||
                                        s.Name.Contains(f, StringComparison.OrdinalIgnoreCase));

    private static ExportTable Def<T>(
        string name, ExportGroup group, string? importTable,
        Func<SzAppDbContext, int[], IQueryable<T>> query,
        Dictionary<string, string>? rename = null,
        string[]? only = null,
        Func<object, bool>? rowFilter = null) where T : class
    {
        var columns = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.GetIndexParameters().Length == 0 && IsScalar(p.PropertyType))
            .Where(p => only is null ? !SecretFragments.Any(f => p.Name.Contains(f, StringComparison.OrdinalIgnoreCase)) : only.Contains(p.Name))
            .Select(p => new ExportColumn(rename?.GetValueOrDefault(p.Name) ?? p.Name, p, PersonalColumns.Contains(p.Name)))
            .ToArray();
        return new ExportTable(name, group, importTable, columns, (db, ids) => query(db, ids), (db, ids, ct) => Stream(query(db, ids).AsNoTracking(), ct), rowFilter);
    }

    private static async IAsyncEnumerable<object> Stream<T>(IQueryable<T> query, [EnumeratorCancellation] CancellationToken ct) where T : class
    {
        await foreach (var row in query.AsAsyncEnumerable().WithCancellation(ct)) yield return row;
    }

    private static bool IsScalar(Type type)
    {
        var t = Nullable.GetUnderlyingType(type) ?? type;
        return t == typeof(string) || (t.IsValueType && t != typeof(byte[]));
    }
}
