using System.Text;
using SzApp.Domain;
using SzApp.Domain.Reports;

namespace SzApp.UnitTests.Reports;

public sealed class ReportExportTests
{
    [Fact]
    public void RowLimiter_EnforcesHardLimitAndMarksTruncation()
    {
        var rows = Enumerable.Range(1, 4).Select(x => (IReadOnlyList<object?>)[x]);
        var result = ReportResultLimiter.Limit(["Id"], rows, 3);

        Assert.Equal(3, result.Rows.Count);
        Assert.True(result.IsTruncated);
    }

    [Fact]
    public void CsvExporter_PreventsSpreadsheetFormulaInjection()
    {
        var bytes = CsvReportExporter.Export(new TabularReportData(["Value"], [["=HYPERLINK(\"bad\")"]]));
        var csv = Encoding.UTF8.GetString(bytes);

        Assert.Contains("'=HYPERLINK", csv);
    }

    [Fact]
    public void HtmlRenderer_EncodesAllDataAndRejectsUnknownFunction()
    {
        var registry = new DefaultReportFunctionRegistry();
        var html = registry.RenderHtml("Table", "<Naslov>", new TabularReportData(["Ime"], [["<script>alert(1)</script>"]]));

        Assert.DoesNotContain("<script>", html);
        Assert.Contains("&lt;script&gt;", html);
        var exception = Assert.Throws<DomainRuleException>(() => registry.RenderHtml("Reflection.Call", "x", new TabularReportData([], [])));
        Assert.Equal("reports.function-not-allowed", exception.Code);
    }
}
