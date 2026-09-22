using System.Text.RegularExpressions;

namespace SzApp.Domain.Reports;

public sealed record ValidatedReportQuery(string Sql, IReadOnlyList<string> ParameterNames);

public static partial class ReportQueryPolicy
{
    private static readonly string[] ForbiddenTokens =
    [
        "INSERT", "UPDATE", "DELETE", "MERGE", "DROP", "ALTER", "CREATE", "TRUNCATE",
        "EXEC", "EXECUTE", "GRANT", "REVOKE", "DENY", "BACKUP", "RESTORE", "DBCC",
        "BULK", "OPENROWSET", "OPENDATASOURCE", "WAITFOR", "KILL", "USE", "SET", "INTO"
    ];

    public static ValidatedReportQuery Validate(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
            throw Violation("reports.empty-query", "SQL upit je obavezan.");

        var normalized = sql.Trim();
        if (normalized.Length > 100_000)
            throw Violation("reports.query-too-long", "SQL upit je predugačak.");

        if (normalized.Contains(';') || normalized.Contains("--", StringComparison.Ordinal) ||
            normalized.Contains("/*", StringComparison.Ordinal) || normalized.Contains("*/", StringComparison.Ordinal))
            throw Violation("reports.multiple-or-commented-query", "Komentari i višestruki SQL izrazi nisu dozvoljeni.");

        if (!StartRegex().IsMatch(normalized))
            throw Violation("reports.select-only", "Dozvoljen je samo SELECT ili CTE upit.");

        foreach (var token in ForbiddenTokens)
        {
            if (Regex.IsMatch(normalized, $@"\b{token}\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                throw Violation("reports.forbidden-token", $"SQL token '{token}' nije dozvoljen.");
        }

        if (Regex.IsMatch(normalized, @"\b(?:xp_|sp_)[A-Za-z0-9_]*", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            throw Violation("reports.procedure-not-allowed", "Pozivanje procedura nije dozvoljeno.");

        var parameters = ParameterRegex().Matches(normalized).Select(x => x.Groups[1].Value)
            .Distinct(StringComparer.OrdinalIgnoreCase).Order(StringComparer.OrdinalIgnoreCase).ToArray();
        if (!parameters.Contains("CompanyId", StringComparer.OrdinalIgnoreCase))
            throw Violation("reports.company-parameter-required", "Upit mora sadržati parametar @CompanyId.");

        return new(normalized, parameters);
    }

    private static DomainRuleException Violation(string code, string message) => new(code, message);

    [GeneratedRegex(@"\A(?:SELECT\b|WITH\b)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex StartRegex();

    [GeneratedRegex(@"(?<!@)@([A-Za-z][A-Za-z0-9_]*)", RegexOptions.CultureInvariant)]
    private static partial Regex ParameterRegex();
}
