using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using SzApp.Etl.Parsing;

namespace SzApp.Etl.Pipeline.Materialization;

public sealed record BindIssue(string Code, string Message);

/// <summary>An optional FK whose target isn't imported yet — kept as PendingLegacyRelationship and
/// patched in once the target table arrives (e.g. Unit.ContractId, Partner.CompanyId).</summary>
public sealed record UnresolvedReference(string Property, string TargetTable, string TargetKey);

public sealed class BindResult
{
    public List<BindIssue> Errors { get; } = [];
    public List<UnresolvedReference> Deferred { get; } = [];
    /// <summary>Self-references (SubAccount.ParentSubAccountId…) applied in a second pass, once
    /// every row of the table exists — insert order inside a chunk isn't guaranteed.</summary>
    public List<(IProperty Property, ForeignKey Fk, string RawKey)> SelfReferences { get; } = [];
}

/// <summary>
/// Generic, EF-metadata-driven CSV -> entity binder (shadow properties included). Same rules for
/// insert and delta update: a column absent from the CSV leaves the property untouched.
/// </summary>
public static class RowBinder
{
    private static readonly TimeZoneInfo Belgrade = TimeZoneInfo.FindSystemTimeZoneById("Europe/Belgrade");

    public static async Task<BindResult> BindAsync(
        ImportContext ctx, TableSpec spec, EntityEntry entry, IReadOnlyDictionary<string, string> values,
        string sourceKey, bool isInsert, CancellationToken ct)
    {
        var result = new BindResult();
        var entityType = entry.Metadata;
        var keyNames = entityType.FindPrimaryKey()!.Properties.Select(x => x.Name).ToHashSet();
        var fks = spec.ForeignKeys.ToDictionary(x => x.Property);

        foreach (var property in entityType.GetProperties())
        {
            var name = property.Name;
            if (TableSpecs.IsSkipped(name) || property.IsConcurrencyToken || fks.ContainsKey(name) ||
                spec.Ignore?.Contains(name) == true || (spec.Only is not null && !spec.Only.Contains(name)))
            {
                continue;
            }

            if (keyNames.Contains(name))
            {
                continue; // keys are set by TableMaterializer before the entity is tracked
            }

            var column = spec.Aliases?.GetValueOrDefault(name) ?? name;
            if (values.TryGetValue(column, out var raw))
            {
                TrySet(entry, property, raw, result);
            }
        }

        var missingRequired = new List<(ForeignKey Fk, string? LegacyKey)>();
        foreach (var fk in spec.ForeignKeys)
        {
            var property = entityType.FindProperty(fk.Property);
            if (property is null || spec.Ignore?.Contains(fk.Property) == true)
            {
                continue;
            }

            var required = !property.IsNullable;
            if (!values.TryGetValue(fk.CsvColumn, out var raw))
            {
                if (required && IsDefault(entry.Property(fk.Property).CurrentValue)) missingRequired.Add((fk, null));
                continue;
            }

            var legacyKey = ImportContext.NormalizeFk(raw, keepZero: fk.ShortListTable is not null && required);
            if (legacyKey is null)
            {
                if (required) missingRequired.Add((fk, null));
                else entry.Property(fk.Property).CurrentValue = null;
                continue;
            }

            if (fk.TargetTable.Equals(spec.Table, StringComparison.OrdinalIgnoreCase))
            {
                result.SelfReferences.Add((property, fk, legacyKey));
                continue;
            }

            var target = ctx.Resolve(fk, legacyKey);
            if (target is null)
            {
                if (required)
                {
                    missingRequired.Add((fk, legacyKey));
                }
                else
                {
                    entry.Property(fk.Property).CurrentValue = null;
                    result.Deferred.Add(new(fk.Property, fk.TargetTable, legacyKey));
                }

                continue;
            }

            entry.Property(fk.Property).CurrentValue = ConvertKey(property, target);
        }

        // CSV without CompanyId (older canonical examples): take it from the parent row.
        if (spec.CompanyFrom is { } parentProperty &&
            missingRequired.FindIndex(x => x.Fk.Property == "CompanyId") is var companyIndex && companyIndex >= 0 &&
            entry.Property(parentProperty).CurrentValue is int parentId && parentId > 0 &&
            spec.ForeignKeys.FirstOrDefault(x => x.Property == parentProperty) is { } parentFk &&
            TableSpecs.Find(parentFk.TargetTable) is { } parentSpec &&
            await ctx.CompanyOfAsync(parentSpec.Entity, parentId, ct) is { } companyId)
        {
            entry.Property("CompanyId").CurrentValue = companyId;
            missingRequired.RemoveAt(companyIndex);
        }

        foreach (var (fk, legacyKey) in missingRequired)
        {
            result.Errors.Add(legacyKey is null
                ? new("etl.fk-required", $"{fk.CsvColumn} je obavezan.")
                : new("etl.fk-unresolved", $"{fk.CsvColumn}={legacyKey}: {fk.TargetTable} nije uvezen."));
        }

        return result;
    }

    public static object ConvertKey(IProperty property, string targetKey) =>
        (Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType) == typeof(string)
            ? targetKey
            : int.Parse(targetKey, CultureInfo.InvariantCulture);

    private static void TrySet(EntityEntry entry, IProperty property, string raw, BindResult result)
    {
        try
        {
            var trimmed = raw.Trim();
            var type = property.ClrType;
            var underlying = Nullable.GetUnderlyingType(type);
            if (trimmed.Length == 0)
            {
                if (underlying is not null || (type == typeof(string) && property.IsNullable))
                {
                    entry.Property(property.Name).CurrentValue = null;
                }
                else if (type == typeof(string))
                {
                    entry.Property(property.Name).CurrentValue = string.Empty;
                }
                else if (type == typeof(DateOnly) || type == typeof(DateTime) || type == typeof(DateTimeOffset))
                {
                    result.Errors.Add(new("etl.required-value", $"Polje {property.Name} je obavezno."));
                }

                return;
            }

            entry.Property(property.Name).CurrentValue = Parse(underlying ?? type, trimmed);
        }
        catch (Exception exception) when (exception is FormatException or OverflowException or ArgumentException)
        {
            result.Errors.Add(new("etl.invalid-value", $"Polje {property.Name}: {exception.Message}"));
        }
    }

    public static object Parse(Type type, string raw)
    {
        if (type == typeof(string)) return raw;
        if (type == typeof(int)) return (int)ParseInteger(raw);
        if (type == typeof(long)) return ParseInteger(raw);
        if (type == typeof(short)) return (short)ParseInteger(raw);
        if (type == typeof(byte)) return (byte)ParseInteger(raw);
        if (type == typeof(decimal)) return SerbianLegacyValueParser.ParseDecimal(raw);
        if (type == typeof(double)) return (double)SerbianLegacyValueParser.ParseDecimal(raw);
        if (type == typeof(bool)) return SerbianLegacyValueParser.ParseAccessBoolean(raw);
        if (type == typeof(DateOnly)) return SerbianLegacyValueParser.ParseDate(raw);
        if (type == typeof(DateTime)) return SerbianLegacyValueParser.ParseDateTime(raw);
        if (type == typeof(DateTimeOffset))
        {
            var local = SerbianLegacyValueParser.ParseDateTime(raw);
            return new DateTimeOffset(local, Belgrade.GetUtcOffset(local));
        }

        if (type == typeof(Guid)) return Guid.Parse(raw);
        if (type.IsEnum) return Enum.Parse(type, raw, ignoreCase: true);
        throw new FormatException($"Tip {type.Name} nije podržan za uvoz.");
    }

    private static long ParseInteger(string raw) =>
        long.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value
            : SerbianLegacyValueParser.ParseDecimal(raw) is var d && d == decimal.Truncate(d)
                ? (long)d
                : throw new FormatException($"'{raw}' nije ceo broj.");

    private static bool IsDefault(object? value) => value is null or 0 or "";
}
