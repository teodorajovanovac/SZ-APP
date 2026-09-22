namespace SzApp.Etl.Pipeline;

public sealed record DeferredRelationship(string Name, string TargetTable);

public sealed record ImportNode(
    string Table,
    IReadOnlyCollection<string> Dependencies,
    IReadOnlyCollection<DeferredRelationship> DeferredRelationships);

public static class LegacyImportTopology
{
    private static readonly ImportNode[] Nodes =
    [
        new("ShortList", [], []),
        new("Languages", [], []),
        new("Staff", ["ShortList"], []),
        new("Address", [], []),
        new("Partner", ["ShortList"], [new("CompanyId", "Company")]),
        new("Company", ["Partner", "ShortList"], []),
        new("BuildingEntrance", ["Company", "Address"], []),
        new("Unit", ["Company", "BuildingEntrance", "ShortList"], [new("ContractId", "Contract")]),
        new("Contract", ["Unit", "Partner"], []),
        new("PartnerAccount", ["Company", "Partner", "Contract"], []),
        new("BankAccount", ["Company", "Partner"], []),
        new("InvoiceBatch", ["Company", "Staff"], []),
        new("Invoice", ["Company", "Partner", "InvoiceBatch"], []),
        new("SupplierInvoice", ["Company", "PartnerAccount"], []),
        new("BankStatement", ["Company", "BankAccount"], []),
        new("JournalEntry", ["Company", "Staff"], []),
        new("LedgerEntry", ["JournalEntry", "Company", "PartnerAccount"], []),
        new("Notice", ["Company", "PartnerAccount"], []),
        new("InterestStatement", ["Company", "PartnerAccount"], []),
        new("SentEmail", ["Company"], []),
        new("Documents", ["Company"], [])
    ];

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
