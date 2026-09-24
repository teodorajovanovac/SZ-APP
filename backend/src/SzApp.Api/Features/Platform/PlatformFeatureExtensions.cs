using SzApp.Api.Security;
using SzApp.Domain.Platform;

namespace SzApp.Api.Features.Platform;

public static class PlatformFeatureExtensions
{
    public static IServiceCollection AddPlatformFeature(this IServiceCollection services, IConfiguration configuration)
    {
        var options = new PlatformStorageOptions();
        configuration.GetSection("Platform:Storage").Bind(options);
        services.AddSingleton(options);
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IPlatformDocumentStorage, LocalDocumentStorage>();
        services.AddScoped<IImportAllowlistRegistry, ImportAllowlistRegistry>();
        services.AddScoped<IEmailTransport, DisabledEmailTransport>();
        return services;
    }

    public static IEndpointRouteBuilder MapPlatformEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var companies = endpoints.MapGroup("/api/v1/companies/{companyId:int}")
            .WithTags("Platform")
            .RequireAuthorization(SecurityConstants.CompanyAccessPolicy);
        companies.MapDocumentEndpoints();
        companies.MapEmailEndpoints();
        companies.MapWorkflowEndpoints();
        companies.MapAdministrationEndpoints();
        companies.MapReferenceDataEndpoints();
        companies.MapMenuEndpoints();
        return endpoints;
    }
}
