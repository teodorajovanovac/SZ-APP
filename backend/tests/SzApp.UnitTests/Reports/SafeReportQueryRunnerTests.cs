using Microsoft.Extensions.Options;
using SzApp.Api.Features.Reports;

namespace SzApp.UnitTests.Reports;

public sealed class SafeReportQueryRunnerTests
{
    [Fact]
    public void Constructing_WithUnconfiguredConnectionString_DoesNotThrow()
    {
        // This is the regression: ReportService (and therefore SafeReportQueryRunner) is
        // constructed by DI on every request to /reports and /analyses, including plain
        // listing endpoints that never touch the runner. Eager validation in the
        // constructor broke those listings whenever Reports:ReadOnlyConnectionString was
        // unset. Validation must be deferred to RunAsync.
        var exception = Record.Exception(() => new SafeReportQueryRunner(Options.Create(new ReportsOptions())));

        Assert.Null(exception);
    }

    [Fact]
    public async Task RunAsync_WithUnconfiguredConnectionString_ThrowsClearError()
    {
        var runner = new SafeReportQueryRunner(Options.Create(new ReportsOptions()));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            runner.RunAsync("SELECT 1 WHERE CompanyId=@CompanyId", companyId: 1, parameters: null, CancellationToken.None));

        Assert.Equal("Reports:ReadOnlyConnectionString je obavezan.", exception.Message);
    }
}
