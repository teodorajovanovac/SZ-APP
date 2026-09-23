using System.Globalization;
using Microsoft.EntityFrameworkCore;
using SzApp.Data;
using SzApp.Data.Entities;
using SzApp.Etl.Parsing;

namespace SzApp.Etl.Pipeline;

// ponytail: the ETL pipeline used to only ever write LegacyKeyMap rows pointing at
// "staging:{SourceTable}" — nothing reached a real domain table. Full materialization of all
// ~40 target tables is a much bigger, riskier job (financial tables especially need a confirmed
// posting/GL mapping from the business owner — see CLAUDE.md's "don't invent FK/business logic"
// rule). This class materializes only the low-risk master-data tables: Address, Partner, Company,
// BuildingEntrance, Unit — plain reference data with no money/ledger semantics.
//
// STILL STAGING-ONLY (deliberately, not yet materialized — needs confirmed mapping logic first):
// ShortList, Languages, Staff, Contract, PartnerAccount, BankAccount, InvoiceBatch, Invoice,
// SupplierInvoice, BankStatement, JournalEntry, LedgerEntry, Notice, InterestStatement, SentEmail,
// Documents. EtlPipelineService.ReconcileAsync short-circuits reconciliation for the transactional
// ones (Invoice/LedgerEntry/BankStatement) instead of comparing against tables that can never be
// non-empty.
//
// ShortList-backed lookup columns (PartnerTypeId, CompanyTypeId, VatTypeId, UnitTypeId, ManagerId's
// ShortList siblings, etc.) are left null here — ShortList itself isn't materialized (its rows are
// keyed by TableName+IndexValue, not a simple legacy id, so it needs its own mapping pass), and the
// FK columns are nullable so leaving them null doesn't violate any constraint.
public static class MasterDataMaterializer
{
    private static readonly HashSet<string> SupportedTables = new(StringComparer.OrdinalIgnoreCase)
    {
        "Address", "Partner", "Company", "BuildingEntrance", "Unit"
    };

    public static bool IsSupported(string sourceTable) => SupportedTables.Contains(sourceTable);

    /// <summary>
    /// Materializes one row into a real domain table, resolving its FK columns via the global
    /// LegacyKeyMap (not scoped to the current run — a dependency table may have been imported in
    /// an earlier run). Returns the new entity's id, or null if a *required* FK isn't resolvable
    /// yet (dependency not imported) — the caller leaves that row staging-only rather than failing
    /// the whole run.
    /// </summary>
    public static Task<int?> TryMaterializeAsync(
        SzAppDbContext db, string sourceTable, IReadOnlyDictionary<string, string> values, CancellationToken ct) =>
        sourceTable.ToLowerInvariant() switch
        {
            "address" => MaterializeAddressAsync(db, values, ct),
            "partner" => MaterializePartnerAsync(db, values, ct),
            "company" => MaterializeCompanyAsync(db, values, ct),
            "buildingentrance" => MaterializeBuildingEntranceAsync(db, values, ct),
            "unit" => MaterializeUnitAsync(db, values, ct),
            _ => Task.FromResult<int?>(null)
        };

    private static async Task<int?> MaterializeAddressAsync(
        SzAppDbContext db, IReadOnlyDictionary<string, string> v, CancellationToken ct)
    {
        var entity = new Address
        {
            StreetAddress = Get(v, "Address") ?? string.Empty,
            PostalCode = Get(v, "PostalCode"),
            City = Get(v, "City") ?? string.Empty,
            CountryCode = Get(v, "CountryCode") is { Length: > 0 } cc ? cc : "RS"
        };
        db.Add(entity);
        await db.SaveChangesAsync(ct);
        return entity.Id;
    }

    private static async Task<int?> MaterializePartnerAsync(
        SzAppDbContext db, IReadOnlyDictionary<string, string> v, CancellationToken ct)
    {
        // ponytail: CompanyId is optional and gets no deferred backfill pass — if the owning
        // Company is imported in a *later* run, this Partner row's CompanyId stays null. Upgrade
        // path: a pass-two job walking resolved PendingLegacyRelationship rows to patch it in.
        var companyId = await ResolveAsync(db, "Company", Get(v, "CompanyId"), ct);
        var entity = new Partner
        {
            CompanyId = companyId,
            ShortName = Get(v, "ShortName") ?? string.Empty,
            Name = Get(v, "Name") ?? string.Empty,
            RegistrationNumber = Get(v, "RegistrationNumber"),
            TaxNumber = Get(v, "TaxNumber"),
            Jbkjs = Get(v, "Jbkjs"),
            IdCardNumber = Get(v, "IdCardNumber"),
            Jmbg = Get(v, "Jmbg"),
            PartnerTypeId = null, // ShortList lookup — not materialized yet
            Language = Get(v, "Language") is { Length: > 0 } lang ? lang : "sr-Latn",
            Note = Get(v, "Note")
        };
        db.Add(entity);
        await db.SaveChangesAsync(ct);
        return entity.Id;
    }

    private static async Task<int?> MaterializeCompanyAsync(
        SzAppDbContext db, IReadOnlyDictionary<string, string> v, CancellationToken ct)
    {
        var partnerId = await ResolveAsync(db, "Partner", Get(v, "PartnerId"), ct);
        if (partnerId is null)
        {
            return null; // Company.PartnerId is required — Partner must be imported first
        }

        var entity = new Company
        {
            PartnerId = partnerId.Value,
            ManagerId = await ResolveAsync(db, "Partner", Get(v, "ManagerId"), ct),
            ShortName = Get(v, "ShortName") ?? string.Empty,
            PrintName = Get(v, "PrintName") ?? string.Empty,
            RelativeFolderName = Get(v, "RelativeFolderName"),
            CompanyTypeId = null, // ShortList lookup — not materialized yet
            VatTypeId = null,     // ShortList lookup — not materialized yet
            LedgerEntryDate = ParseNullableDate(Get(v, "LedgerEntryDate"))
        };
        db.Add(entity);
        await db.SaveChangesAsync(ct);
        return entity.Id;
    }

    private static async Task<int?> MaterializeBuildingEntranceAsync(
        SzAppDbContext db, IReadOnlyDictionary<string, string> v, CancellationToken ct)
    {
        var companyId = await ResolveAsync(db, "Company", Get(v, "CompanyId"), ct);
        if (companyId is null)
        {
            return null; // BuildingEntrance.CompanyId is required
        }

        // legacy column is misspelled "AdressId" in the source schema (see docs/data-model.md)
        var addressLegacyKey = Get(v, "AdressId") ?? Get(v, "AddressId");
        var entity = new BuildingEntrance
        {
            CompanyId = companyId.Value,
            BuildingName = Get(v, "BuildingName"),
            EntranceName = Get(v, "EntranceName"),
            AddressId = await ResolveAsync(db, "Address", addressLegacyKey, ct),
            BuildingLabel = Get(v, "BuildingLabel"),
            Description = Get(v, "Description"),
            SortIndex = ParseNullableInt(Get(v, "SortIndex"))
        };
        db.Add(entity);
        await db.SaveChangesAsync(ct);
        return entity.Id;
    }

    private static async Task<int?> MaterializeUnitAsync(
        SzAppDbContext db, IReadOnlyDictionary<string, string> v, CancellationToken ct)
    {
        var companyId = await ResolveAsync(db, "Company", Get(v, "CompanyId"), ct);
        if (companyId is null)
        {
            return null; // Unit.CompanyId is required
        }

        var entity = new Unit
        {
            CompanyId = companyId.Value,
            Name = Get(v, "Name"),
            ContractId = null, // Contract isn't materialized yet (out of scope — see class comment)
            UnitTypeId = null, // ShortList lookup — not materialized yet
            BuildingEntranceId = await ResolveAsync(db, "BuildingEntrance", Get(v, "BuildingEntranceId"), ct),
            Note = Get(v, "Note"),
            SortingNumber = ParseNullableInt(Get(v, "SortingNumber")),
            K1 = ParseNullableDecimal(Get(v, "K1")),
            K2 = ParseNullableDecimal(Get(v, "K2")),
            K3 = ParseNullableDecimal(Get(v, "K3")),
            K4 = ParseNullableDecimal(Get(v, "K4")),
            K5 = ParseNullableDecimal(Get(v, "K5")),
            FloorNumber = ParseNullableInt(Get(v, "FloorNumber"))
        };
        db.Add(entity);
        await db.SaveChangesAsync(ct);
        return entity.Id;
    }

    private static async Task<int?> ResolveAsync(SzAppDbContext db, string targetTable, string? legacyKey, CancellationToken ct)
    {
        var normalized = SerbianLegacyValueParser.ZeroToNull(legacyKey);
        if (normalized is null)
        {
            return null;
        }

        var targetKey = await db.LegacyKeyMaps.AsNoTracking()
            .Where(x => x.SourceTable == targetTable && x.SourceKey == normalized && x.TargetTable == targetTable)
            .Select(x => x.TargetKey)
            .FirstOrDefaultAsync(ct);
        return targetKey is null ? null : int.Parse(targetKey, CultureInfo.InvariantCulture);
    }

    private static string? Get(IReadOnlyDictionary<string, string> values, string key) =>
        values.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value.Trim() : null;

    private static int? ParseNullableInt(string? raw) =>
        raw is null ? null : int.Parse(raw, CultureInfo.InvariantCulture);

    private static decimal? ParseNullableDecimal(string? raw) =>
        raw is null ? null : SerbianLegacyValueParser.ParseDecimal(raw);

    private static DateOnly? ParseNullableDate(string? raw) =>
        raw is null ? null : SerbianLegacyValueParser.ParseDate(raw);
}
