using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using SzApp.Api.Features.LedgerBanking;
using SzApp.Api.Security;
using SzApp.Data.Entities;

namespace SzApp.IntegrationTests.LedgerBanking;

public sealed class PostingPeriodEndpointTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private string[] PoliciesFor(string routeSuffix, string method = "POST")
    {
        factory.CreateClient(); // builds the host
        var endpoint = factory.Services.GetRequiredService<EndpointDataSource>().Endpoints
            .OfType<RouteEndpoint>()
            .Single(x => x.RoutePattern.RawText!.EndsWith(routeSuffix, StringComparison.Ordinal) &&
                         (x.Metadata.GetMetadata<Microsoft.AspNetCore.Routing.HttpMethodMetadata>()?.HttpMethods.Contains(method) ?? false));
        return endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>().Select(x => x.Policy).OfType<string>().ToArray();
    }

    [Fact]
    public void Lock_RequiresPostingRight_Unlock_RequiresAdmin()
    {
        Assert.Contains(SecurityConstants.CompanyPostPolicy, PoliciesFor("/posting-periods/{periodYYMM:int}/lock"));
        var unlock = PoliciesFor("/posting-periods/{periodYYMM:int}/unlock");
        Assert.Contains(SecurityConstants.CompanyAdminPolicy, unlock);
        Assert.DoesNotContain(SecurityConstants.CompanyPostPolicy, unlock);

        // CompanyAdmin = Upravnik minimum: a Moderator (who may lock) may not unlock.
        Assert.False(CompanyRoleRequirement.Satisfies(StaffRole.Moderator, StaffRole.Upravnik));
        Assert.True(CompanyRoleRequirement.Satisfies(StaffRole.Upravnik, StaffRole.Upravnik));
    }

    [Fact]
    public void BusinessClock_UsesBelgradeDate_NotUtc()
    {
        // FIN-20: 23:30 UTC on New Year's Eve is already 1 January in Belgrade.
        var clock = new BelgradeBusinessClock(new FixedTime(new DateTimeOffset(2026, 12, 31, 23, 30, 0, TimeSpan.Zero)));
        Assert.Equal(new DateOnly(2027, 1, 1), clock.Today);
    }

    private sealed class FixedTime(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
