namespace SzApp.Etl.Pipeline;

public sealed record DeferredRelationship(string Name, string TargetTable);

public sealed record ImportNode(
    string Table,
    IReadOnlyCollection<string> Dependencies,
    IReadOnlyCollection<DeferredRelationship> DeferredRelationships,
    // CSV column holding the row's legacy key (LegacyKeyMap.SourceKey). Export renames some ids
    // (Staff -> StaffId, BankAccount -> BankAccountId); code lists use their natural key.
    string KeyColumn = "Id",
    // Global code list (no CompanyId): a completed run under any company satisfies the dependency.
    bool IsGlobal = false);

public static class LegacyImportTopology
{
    // Canonical (new-schema, export round-trip) tables in dependency order. The raw Access export
    // is first turned into these files by Legacy.AccessExportTransformer (ETL-01).
    private static readonly ImportNode[] Nodes =
    [
        new("ShortList", [], [], IsGlobal: true),
        new("Languages", [], [], KeyColumn: "Code", IsGlobal: true),
        new("ChartAccount", [], [], KeyColumn: "Account", IsGlobal: true),
        new("SubAccount", [], [], IsGlobal: true),
        new("CalculationType", ["ShortList"], [], IsGlobal: true),
        new("InterestRate", [], [], IsGlobal: true),
        new("Address", [], []),
        new("Partner", ["ShortList"], [new("CompanyId", "Company")]),
        new("Company", ["Partner", "ShortList"], []),
        new("Staff", ["Company"], [], KeyColumn: "StaffId"),
        new("BuildingEntrance", ["Company", "Address"], []),
        new("Unit", ["Company", "BuildingEntrance", "ShortList"], [new("ContractId", "Contract")]),
        new("Contract", ["Unit", "Partner"], []),
        new("PartnerAccount", ["Company", "Partner", "Contract", "ChartAccount"], []),
        new("BankAccount", ["Company", "Partner"], [], KeyColumn: "BankAccountId"),
        new("JournalEntry", ["Company", "Staff", "ShortList"], []),
        new("InvoiceBatch", ["Company", "Staff", "JournalEntry"], []),
        new("Invoice", ["Company", "Partner", "InvoiceBatch"], []),
        new("SupplierInvoice", ["Company", "PartnerAccount", "CalculationType", "SubAccount", "JournalEntry"], []),
        new("SupplierInvoiceUnitType", ["SupplierInvoice", "ShortList"], []),
        new("InvoiceLine", ["Invoice", "SupplierInvoice"], []),
        new("BankStatement", ["Company", "BankAccount", "JournalEntry", "ChartAccount"], []),
        new("BankStatementLine", ["BankStatement", "PartnerAccount", "SubAccount"], []),
        new("LedgerEntry", ["JournalEntry", "Company", "PartnerAccount", "BankStatementLine", "Invoice", "SupplierInvoice", "SubAccount"], []),
        new("NoticeBatch", ["Company", "InvoiceBatch"], []),
        new("Notice", ["NoticeBatch", "PartnerAccount"], []),
        new("InterestStatement", ["Company", "PartnerAccount", "InvoiceBatch"], []),
        new("SentEmail", ["Company"], []),
        new("Documents", ["Company"], [])
    ];

    /// <summary>Tables in declaration order — already a valid dependency order, and the order
    /// the legacy transformer writes its files in.</summary>
    public static IReadOnlyList<ImportNode> All => Nodes;

    public static IReadOnlyList<ImportNode> OrderedPassOne()
    {
        var byName = Nodes.ToDictionary(x => x.Table, StringComparer.OrdinalIgnoreCase);
        var remaining = Nodes.ToDictionary(
            x => x.Table,
            x => x.Dependencies.Where(byName.ContainsKey).ToHashSet(StringComparer.OrdinalIgnoreCase),
            StringComparer.OrdinalIgnoreCase);
        var ordered = new List<ImportNode>(Nodes.Length);

        while (remaining.Count > 0)
        {
            var ready = remaining.Where(x => x.Value.Count == 0)
                .Select(x => x.Key)
                .OrderBy(x => x, StringComparer.Ordinal)
                .ToArray();
            if (ready.Length == 0)
            {
                throw new InvalidOperationException($"Nerazrešiv ciklus u import grafu: {string.Join(", ", remaining.Keys)}");
            }

            foreach (var table in ready)
            {
                ordered.Add(byName[table]);
                remaining.Remove(table);
                foreach (var dependencies in remaining.Values)
                {
                    dependencies.Remove(table);
                }
            }
        }

        return ordered;
    }

    public static IReadOnlyList<(string SourceTable, DeferredRelationship Relationship)> OrderedPassTwo() =>
        OrderedPassOne()
            .SelectMany(x => x.DeferredRelationships.Select(relationship => (x.Table, relationship)))
            .ToArray();

    public static ImportNode? Find(string table) =>
        Nodes.SingleOrDefault(x => string.Equals(x.Table, table, StringComparison.OrdinalIgnoreCase));
}
