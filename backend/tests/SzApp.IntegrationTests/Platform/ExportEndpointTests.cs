using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using SzApp.Api.Security;

namespace SzApp.IntegrationTests.Platform;

public sealed class ExportEndpointTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private IAuthorizeData[] AuthFor(string route)
    {
        factory.CreateClient(); // builds the host
        return factory.Services.GetRequiredService<EndpointDataSource>().Endpoints
            .OfType<RouteEndpoint>()
            .Single(x => x.RoutePattern.RawText == route)
            .Metadata.GetOrderedMetadata<IAuthorizeData>().ToArray();
    }

    [Fact]
    public void CompanyExport_RequiresCompanyAdmin()
    {
        Assert.Contains(SecurityConstants.CompanyAdminPolicy, AuthFor("/api/v1/companies/{companyId:int}/export").Select(x => x.Policy));
        Assert.Contains(SecurityConstants.CompanyAdminPolicy, AuthFor("/api/v1/companies/{companyId:int}/export/tables").Select(x => x.Policy));
    }

    [Fact]
    public async Task MultiCompanyExport_IsRootOnly_AndRejectsAnonymous()
    {
        // Policy-builder lambdas surface as an AuthorizationPolicy, not IAuthorizeData.Roles.
        factory.CreateClient();
        var endpoint = factory.Services.GetRequiredService<EndpointDataSource>().Endpoints
            .OfType<RouteEndpoint>().Single(x => x.RoutePattern.RawText == "/api/v1/export");
        var roles = endpoint.Metadata.GetOrderedMetadata<AuthorizationPolicy>()
            .SelectMany(p => p.Requirements.OfType<Microsoft.AspNetCore.Authorization.Infrastructure.RolesAuthorizationRequirement>())
            .SelectMany(r => r.AllowedRoles);
        Assert.Equal([SecurityConstants.RootRole], roles);

        var response = await factory.CreateClient().GetAsync("/api/v1/export?companyIds=all");
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
