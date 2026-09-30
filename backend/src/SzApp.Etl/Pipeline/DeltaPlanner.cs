namespace SzApp.Etl.Pipeline;

public sealed record StagedRow(string SourceKey, string RowHash, int RowNumber, IReadOnlyDictionary<string, string> Values);

/// <summary>What LegacyKeyMap already knows about one legacy key of the table.</summary>
public sealed record MappedKey(long MapId, string TargetKey, string? RowHash, bool InScope);

public sealed record DeltaPlan(
    IReadOnlyList<StagedRow> Inserts,
    IReadOnlyList<(StagedRow Row, MappedKey Map)> Updates,
    IReadOnlyList<StagedRow> Unchanged,
    IReadOnlyList<StagedRow> DuplicateKeys,
    IReadOnlyList<string> DeletedInSource);

/// <summary>
/// ETL-05 delta: classify the file's rows against the key map. New key = insert; known key with a
/// different row hash = update in place; same hash = unchanged (a re-run is a no-op, which is what
/// makes the import idempotent and crash-resumable). Keys mapped by earlier runs of the same
/// company but missing from the file are reported, never deleted — Access deletes are a business
/// event ("rasknjižavanje") that needs a human decision in the new system.
/// </summary>
public static class DeltaPlanner
{
    public static DeltaPlan Plan(IEnumerable<StagedRow> rows, IReadOnlyDictionary<string, MappedKey> existing)
    {
        var inserts = new List<StagedRow>();
        var updates = new List<(StagedRow, MappedKey)>();
        var unchanged = new List<StagedRow>();
        var duplicates = new List<StagedRow>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            if (!seen.Add(row.SourceKey))
            {
                duplicates.Add(row);
            }
            else if (!existing.TryGetValue(row.SourceKey, out var map))
            {
                inserts.Add(row);
            }
            else if (string.Equals(map.RowHash, row.RowHash, StringComparison.Ordinal))
            {
                unchanged.Add(row);
            }
            else
            {
                updates.Add((row, map));
            }
        }

        var deleted = existing.Where(x => x.Value.InScope && !seen.Contains(x.Key))
            .Select(x => x.Key)
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToArray();
        return new DeltaPlan(inserts, updates, unchanged, duplicates, deleted);
    }
}
