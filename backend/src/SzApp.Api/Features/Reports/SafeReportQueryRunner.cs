using System.Data;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using SzApp.Domain;
using SzApp.Domain.Reports;

namespace SzApp.Api.Features.Reports;

public interface ISafeReportQueryRunner
{
    Task<TabularReportData> RunAsync(
        string sql,
        int companyId,
        IReadOnlyDictionary<string, JsonElement>? parameters,
        CancellationToken cancellationToken);
}

public sealed class SafeReportQueryRunner(IOptions<ReportsOptions> options) : ISafeReportQueryRunner
{
    private ReportsOptions? _options;

    // ponytail: lazy validation so listing endpoints (which never call RunAsync) don't
    // eagerly fail when Reports:ReadOnlyConnectionString is unconfigured.
    private ReportsOptions EnsureValidated() => _options ??= ValidateOptions(options.Value);

    public async Task<TabularReportData> RunAsync(
        string sql,
        int companyId,
        IReadOnlyDictionary<string, JsonElement>? parameters,
        CancellationToken cancellationToken)
    {
        var _options = EnsureValidated();
        var validated = ReportQueryPolicy.Validate(sql);
        var supplied = new Dictionary<string, JsonElement>(parameters ?? new Dictionary<string, JsonElement>(), StringComparer.OrdinalIgnoreCase);
        if (supplied.ContainsKey("CompanyId"))
            throw new DomainRuleException("reports.company-parameter-reserved", "CompanyId određuje server i ne sme biti prosleđen.");

        var expected = validated.ParameterNames.Where(x => !x.Equals("CompanyId", StringComparison.OrdinalIgnoreCase)).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var unexpected = supplied.Keys.FirstOrDefault(x => !expected.Contains(x));
        if (unexpected is not null)
            throw new DomainRuleException("reports.unknown-parameter", $"Parametar '{unexpected}' nije definisan u upitu.");
        var missing = expected.FirstOrDefault(x => !supplied.ContainsKey(x));
        if (missing is not null)
            throw new DomainRuleException("reports.missing-parameter", $"Nedostaje parametar '{missing}'.");

        await using var connection = new SqlConnection(_options.ReadOnlyConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = validated.Sql;
        command.CommandType = CommandType.Text;
        command.CommandTimeout = _options.CommandTimeoutSeconds;
        command.Parameters.Add(new SqlParameter("@CompanyId", SqlDbType.Int) { Value = companyId });
        foreach (var name in expected)
            command.Parameters.Add(CreateParameter(name, supplied[name]));

        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess | CommandBehavior.SingleResult, cancellationToken);
        if (reader.FieldCount > _options.MaximumColumns)
            throw new DomainRuleException("reports.too-many-columns", "Rezultat ima više kolona od dozvoljenog maksimuma.");

        var columns = UniqueColumnNames(reader);
        var rows = new List<IReadOnlyList<object?>>();
        var truncated = false;
        while (await reader.ReadAsync(cancellationToken))
        {
            if (rows.Count == _options.MaximumRows)
            {
                truncated = true;
                break;
            }
            var row = new object?[reader.FieldCount];
            for (var i = 0; i < reader.FieldCount; i++) row[i] = NormalizeCell(reader.GetValue(i));
            rows.Add(row);
        }
        return new(columns, rows, truncated);
    }

    private object? NormalizeCell(object value) => value switch
    {
        DBNull => null,
        byte[] bytes => Convert.ToBase64String(bytes),
        string text when text.Length > _options!.MaximumCellCharacters => text[.._options.MaximumCellCharacters],
        DateTimeOffset or DateTime or DateOnly or TimeOnly or Guid or bool or byte or short or int or long or float or double or decimal or string => value,
        _ => Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture)
    };

    private static SqlParameter CreateParameter(string name, JsonElement value)
    {
        var parameter = new SqlParameter("@" + name, DBNull.Value);
        switch (value.ValueKind)
        {
            case JsonValueKind.Null:
                parameter.Value = DBNull.Value;
                break;
            case JsonValueKind.String:
                var text = value.GetString()!;
                if (text.Length > 4_000) throw new DomainRuleException("reports.parameter-too-long", $"Parametar '{name}' je predugačak.");
                parameter.SqlDbType = SqlDbType.NVarChar; parameter.Size = Math.Max(1, text.Length); parameter.Value = text;
                break;
            case JsonValueKind.Number when value.TryGetInt32(out var intValue):
                parameter.SqlDbType = SqlDbType.Int; parameter.Value = intValue;
                break;
            case JsonValueKind.Number when value.TryGetInt64(out var longValue):
                parameter.SqlDbType = SqlDbType.BigInt; parameter.Value = longValue;
                break;
            case JsonValueKind.Number when value.TryGetDecimal(out var decimalValue):
                parameter.SqlDbType = SqlDbType.Decimal; parameter.Precision = 38; parameter.Scale = 10; parameter.Value = decimalValue;
                break;
            case JsonValueKind.True:
            case JsonValueKind.False:
                parameter.SqlDbType = SqlDbType.Bit; parameter.Value = value.GetBoolean();
                break;
            default:
                throw new DomainRuleException("reports.invalid-parameter-type", $"Parametar '{name}' mora biti skalarna JSON vrednost.");
        }
        return parameter;
    }

    private static string[] UniqueColumnNames(SqlDataReader reader)
    {
        var seen = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var names = new string[reader.FieldCount];
        for (var i = 0; i < reader.FieldCount; i++)
        {
            var baseName = string.IsNullOrWhiteSpace(reader.GetName(i)) ? $"Column{i + 1}" : reader.GetName(i);
            seen.TryGetValue(baseName, out var count);
            seen[baseName] = count + 1;
            names[i] = count == 0 ? baseName : $"{baseName}_{count + 1}";
        }
        return names;
    }

    private static ReportsOptions ValidateOptions(ReportsOptions value)
    {
        if (string.IsNullOrWhiteSpace(value.ReadOnlyConnectionString))
            throw new InvalidOperationException("Reports:ReadOnlyConnectionString je obavezan.");
        var builder = new SqlConnectionStringBuilder(value.ReadOnlyConnectionString);
        if (builder.ApplicationIntent != ApplicationIntent.ReadOnly)
            throw new InvalidOperationException("Reports konekcija mora imati ApplicationIntent=ReadOnly i read-only SQL nalog.");
        if (value.CommandTimeoutSeconds is < 1 or > 300 || value.MaximumRows is < 1 or > 100_000 ||
            value.MaximumColumns is < 1 or > 500 || value.MaximumCellCharacters is < 1 or > 1_000_000)
            throw new InvalidOperationException("Reports bezbednosna ograničenja nisu ispravna.");
        return value;
    }
}
