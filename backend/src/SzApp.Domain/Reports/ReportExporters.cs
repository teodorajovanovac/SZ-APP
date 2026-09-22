using System.Globalization;
using System.Net;
using System.Text;

namespace SzApp.Domain.Reports;

public sealed record TabularReportData(
    IReadOnlyList<string> Columns,
    IReadOnlyList<IReadOnlyList<object?>> Rows,
    bool IsTruncated = false);

public static class ReportResultLimiter
{
    public static TabularReportData Limit(
        IReadOnlyList<string> columns,
        IEnumerable<IReadOnlyList<object?>> rows,
        int maximumRows)
    {
        if (maximumRows is < 1 or > 100_000)
            throw new ArgumentOutOfRangeException(nameof(maximumRows));
        var materialized = rows.Take(maximumRows + 1).ToArray();
        return new(columns, materialized.Take(maximumRows).ToArray(), materialized.Length > maximumRows);
    }
}

public static class CsvReportExporter
{
    public static byte[] Export(TabularReportData data)
    {
        var result = new StringBuilder();
        result.AppendLine(string.Join(',', data.Columns.Select(Escape)));
        foreach (var row in data.Rows)
            result.AppendLine(string.Join(',', row.Select(x => Escape(Format(x)))));
        return new UTF8Encoding(encoderShouldEmitUTF8Identifier: true).GetBytes(result.ToString());
    }

    private static string Format(object? value) => value switch
    {
        null or DBNull => string.Empty,
        DateTime dateTime => dateTime.ToString("O", CultureInfo.InvariantCulture),
        DateOnly date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? string.Empty
    };

    private static string Escape(string value)
    {
        if (value.Length > 0 && value[0] is '=' or '+' or '-' or '@' or '\t' or '\r') value = "'" + value;
        return '"' + value.Replace("\"", "\"\"") + '"';
    }
}

public interface IReportFunctionRegistry
{
    bool IsAllowed(string functionName);
    bool IsAllowedAction(string functionName);
    string RenderHtml(string functionName, string title, TabularReportData data);
}

public sealed class DefaultReportFunctionRegistry : IReportFunctionRegistry
{
    public static readonly IReadOnlySet<string> AllowedFunctions = new HashSet<string>(StringComparer.Ordinal)
    {
        "Table", "AccountCard", "InvoiceSummary", "NoticeSummary"
    };
    public static readonly IReadOnlySet<string> AllowedActions = new HashSet<string>(StringComparer.Ordinal)
    {
        "Run", "ExportCsv", "PrintHtml"
    };

    public bool IsAllowed(string functionName) => AllowedFunctions.Contains(functionName);
    public bool IsAllowedAction(string functionName) => AllowedActions.Contains(functionName);

    public string RenderHtml(string functionName, string title, TabularReportData data)
    {
        if (!IsAllowed(functionName))
            throw new DomainRuleException("reports.function-not-allowed", "Report funkcija nije u dozvoljenom registru.");

        var html = new StringBuilder("<!doctype html><html lang=\"sr-Latn\"><head><meta charset=\"utf-8\">")
            .Append("<meta name=\"viewport\" content=\"width=device-width,initial-scale=1\">")
            .Append("<title>").Append(WebUtility.HtmlEncode(title)).Append("</title>")
            .Append("<style>body{font-family:Arial,sans-serif;margin:24px;color:#111}table{border-collapse:collapse;width:100%}th,td{border:1px solid #bbb;padding:6px;text-align:left}th{background:#eee}@media print{body{margin:0}}</style></head><body>")
            .Append("<h1>").Append(WebUtility.HtmlEncode(title)).Append("</h1><table><thead><tr>");
        foreach (var column in data.Columns) html.Append("<th>").Append(WebUtility.HtmlEncode(column)).Append("</th>");
        html.Append("</tr></thead><tbody>");
        foreach (var row in data.Rows)
        {
            html.Append("<tr>");
            foreach (var value in row) html.Append("<td>").Append(WebUtility.HtmlEncode(Convert.ToString(value, CultureInfo.InvariantCulture))).Append("</td>");
            html.Append("</tr>");
        }
        html.Append("</tbody></table>");
        if (data.IsTruncated) html.Append("<p><strong>Rezultat je skraćen na dozvoljeni broj redova.</strong></p>");
        return html.Append("</body></html>").ToString();
    }
}
