using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SzApp.Api.Features.Billing;
using SzApp.Api.Features.LedgerBanking;
using SzApp.Api.Features.MasterData;
using SzApp.Api.Features.Platform;
using SzApp.Api.Features.Reports;
using SzApp.Api.Features.Etl;
using SzApp.Api.Infrastructure;
using SzApp.Api.Security;
using SzApp.Contracts;
using SzApp.Data;
using SzApp.Data.Entities;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("SzApp")
    ?? throw new InvalidOperationException("Connection string 'SzApp' is required.");
var requireHttps = builder.Configuration.GetValue("Security:RequireHttps", !builder.Environment.IsDevelopment());

builder.Services.AddDbContext<SzAppDbContext>(options =>
    options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));
builder.Services.AddScoped<IShortListValidator, ShortListValidator>();
builder.Services.AddMasterDataFeature();
builder.Services.AddBillingFeature();
builder.Services.AddLedgerBankingFeature(builder.Configuration);
builder.Services.AddPlatformFeature(builder.Configuration);
builder.Services.AddReportsFeature(builder.Configuration);
builder.Services.AddEtlFeature(builder.Configuration);

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedAccount = true;
        options.Password.RequiredLength = 12;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    })
    .AddEntityFrameworkStores<SzAppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = requireHttps ? "__Host-szapp-auth" : "szapp-auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = requireHttps ? CookieSecurePolicy.Always : CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.Path = "/";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = requireHttps ? "__Host-szapp-csrf" : "szapp-csrf";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = requireHttps ? CookieSecurePolicy.Always : CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.Path = "/";
});

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(SecurityConstants.CompanyAccessPolicy, policy =>
        policy.RequireAuthenticatedUser().AddRequirements(new CompanyAccessRequirement()));
builder.Services.AddScoped<IAuthorizationHandler, CompanyAccessHandler>();

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
        context.ProblemDetails.Extensions["correlationId"] = context.HttpContext.TraceIdentifier;
    };
});
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"])
    .AddCheck<DatabaseHealthCheck>("sqlserver", tags: ["ready"]);

var app = builder.Build();

app.Use(async (context, next) =>
{
    const string headerName = "X-Correlation-ID";
    var correlationId = context.Request.Headers[headerName].FirstOrDefault();
    context.TraceIdentifier = string.IsNullOrWhiteSpace(correlationId)
        ? Guid.NewGuid().ToString("N")
        : correlationId[..Math.Min(correlationId.Length, 64)];
    context.Response.Headers[headerName] = context.TraceIdentifier;

    using (app.Logger.BeginScope(
        new Dictionary<string, object?>
        {
            ["CorrelationId"] = context.TraceIdentifier,
            ["StaffId"] = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
        }))
    {
        await next();
    }
});

app.UseExceptionHandler();
if (requireHttps)
{
    app.UseHttpsRedirection();
}
app.UseAuthentication();
app.Use(async (context, next) =>
{
    var isUnsafeApiRequest = context.Request.Path.StartsWithSegments("/api") &&
        !HttpMethods.IsGet(context.Request.Method) &&
        !HttpMethods.IsHead(context.Request.Method) &&
        !HttpMethods.IsOptions(context.Request.Method) &&
        !HttpMethods.IsTrace(context.Request.Method) &&
        !context.Request.Path.Equals("/api/v1/auth/antiforgery");
    if (isUnsafeApiRequest)
    {
        await context.RequestServices.GetRequiredService<IAntiforgery>().ValidateRequestAsync(context);
    }
    await next();
});
app.UseAuthorization();
app.UseAntiforgery();

app.MapOpenApi();
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("live")
}).AllowAnonymous();
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("ready")
}).AllowAnonymous();

app.MapMasterDataEndpoints();
app.MapBillingEndpoints();
app.MapLedgerBankingEndpoints();
app.MapPlatformEndpoints();
app.MapReportsEndpoints();
app.MapEtlEndpoints();

var auth = app.MapGroup("/api/v1/auth").WithTags("Auth");

auth.MapGet("/antiforgery", (HttpContext context, IAntiforgery antiforgery) =>
{
    var tokens = antiforgery.GetAndStoreTokens(context);
    return TypedResults.Ok(new AntiforgeryTokenResponse(tokens.RequestToken!, "X-CSRF-TOKEN"));
}).AllowAnonymous();

auth.MapPost("/login", async (
    LoginRequest request,
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    SzAppDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var user = await userManager.FindByEmailAsync(request.Email);
    if (user is null || !user.IsActive)
    {
        return Results.Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Neispravni podaci za prijavu.");
    }

    var result = await signInManager.PasswordSignInAsync(user, request.Password, request.RememberMe, lockoutOnFailure: true);
    if (!result.Succeeded)
    {
        return Results.Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Neispravni podaci za prijavu.");
    }

    return Results.Ok(await BuildCurrentUserAsync(user, userManager, dbContext, cancellationToken));
}).AddEndpointFilter<AntiforgeryEndpointFilter>().AllowAnonymous();

auth.MapPost("/logout", async (SignInManager<ApplicationUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.NoContent();
}).AddEndpointFilter<AntiforgeryEndpointFilter>().RequireAuthorization();

auth.MapGet("/me", async (
    ClaimsPrincipal principal,
    UserManager<ApplicationUser> userManager,
    SzAppDbContext dbContext) =>
{
    var user = await userManager.GetUserAsync(principal);
    if (user is null)
    {
        return Results.Unauthorized();
    }

    return Results.Ok(await BuildCurrentUserAsync(user, userManager, dbContext, CancellationToken.None));
}).RequireAuthorization();

app.MapGet("/api/v1/companies", async (
    ClaimsPrincipal principal,
    SzAppDbContext dbContext) =>
{
    var query = dbContext.Companies.AsNoTracking();
    if (!principal.IsInRole(SecurityConstants.RootRole))
    {
        var staffId = int.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
        query = query.Where(x => x.StaffAccess.Any(access => access.StaffId == staffId));
    }

    var companies = await query.OrderBy(x => x.ShortName)
        .Select(x => new CompanySummaryResponse(x.Id, x.ShortName, x.Partner.RegistrationNumber))
        .ToArrayAsync();
    return TypedResults.Ok(companies);
}).WithTags("Companies").RequireAuthorization();

app.MapGet("/api/v1/companies/{companyId:int}/context", async (
    int companyId,
    ClaimsPrincipal principal,
    SzAppDbContext dbContext) =>
{
    var company = await dbContext.Companies.AsNoTracking().SingleOrDefaultAsync(x => x.Id == companyId);
    if (company is null)
    {
        return Results.NotFound();
    }

    var role = SecurityConstants.RootRole;
    if (!principal.IsInRole(SecurityConstants.RootRole))
    {
        var staffId = int.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
        role = await dbContext.StaffAccess.AsNoTracking()
            .Where(x => x.CompanyId == companyId && x.StaffId == staffId)
            .Select(x => x.StaffRole.ToString())
            .SingleAsync();
    }

    return Results.Ok(new CompanyContextResponse(company.Id, company.ShortName, role));
}).WithTags("Companies").RequireAuthorization(SecurityConstants.CompanyAccessPolicy);

if (app.Configuration.GetValue<bool>("Database:ApplyMigrations"))
{
    await InitializeDatabaseAsync(app.Services);
}

app.Run();

static async Task InitializeDatabaseAsync(IServiceProvider services)
{
    await using var scope = services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<SzAppDbContext>();
    await dbContext.Database.MigrateAsync();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
    foreach (var roleName in SecurityConstants.AllRoles)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            var result = await roleManager.CreateAsync(new IdentityRole<int>(roleName));
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Kreiranje role '{roleName}' nije uspelo: {string.Join(", ", result.Errors.Select(x => x.Description))}");
            }
        }
    }

    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var bootstrapEmail = configuration["BootstrapAdmin:Email"];
    var bootstrapPassword = configuration["BootstrapAdmin:Password"];
    if (!string.IsNullOrWhiteSpace(bootstrapEmail) && !string.IsNullOrWhiteSpace(bootstrapPassword))
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByEmailAsync(bootstrapEmail);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = bootstrapEmail,
                Email = bootstrapEmail,
                EmailConfirmed = true,
                IsActive = true,
                PreferredLanguage = "sr-Latn"
            };
            var create = await userManager.CreateAsync(user, bootstrapPassword);
            if (!create.Succeeded)
                throw new InvalidOperationException($"Bootstrap administrator nije kreiran: {string.Join(", ", create.Errors.Select(x => x.Code))}");
        }
        if (!await userManager.IsInRoleAsync(user, SecurityConstants.RootRole))
            await userManager.AddToRoleAsync(user, SecurityConstants.RootRole);
    }

    if (configuration.GetValue<bool>("BootstrapDemoData") && !await dbContext.Companies.AnyAsync())
    {
        var organization = new Partner { ShortName = "SZ Primer", Name = "Stambena zajednica Primer", RegistrationNumber = "12345678", TaxNumber = "109876543" };
        var owner = new Partner { ShortName = "Petrović Marko", Name = "Marko Petrović", Language = "sr-Latn" };
        dbContext.AddRange(organization, owner);
        await dbContext.SaveChangesAsync();

        var company = new Company { PartnerId = organization.Id, ShortName = "SZ Primer", PrintName = "Stambena zajednica Primer", RelativeFolderName = "sz-primer" };
        dbContext.Add(company);
        await dbContext.SaveChangesAsync();
        organization.CompanyId = company.Id;
        owner.CompanyId = company.Id;

        var address = new Address { StreetAddress = "Bulevar primera 1", PostalCode = "11000", City = "Beograd", CountryCode = "RS" };
        dbContext.Add(address);
        await dbContext.SaveChangesAsync();
        var entrance = new BuildingEntrance { CompanyId = company.Id, BuildingName = "SZ Primer", EntranceName = "Ulaz A", BuildingLabel = "A", AddressId = address.Id, SortIndex = 1 };
        dbContext.Add(entrance);
        await dbContext.SaveChangesAsync();
        var unit = new Unit { CompanyId = company.Id, Name = "Stan 12", BuildingEntranceId = entrance.Id, SortingNumber = 12, K1 = 64.50m, K2 = 1m, FloorNumber = 3 };
        dbContext.Add(unit);
        await dbContext.SaveChangesAsync();
        var contract = new Contract { CompanyId = company.Id, UnitId = unit.Id, AccountNumber = 100012, OwnerPartnerId = owner.Id, InvoicePartnerId = owner.Id, ContractDate = new DateOnly(2026, 1, 1), InvoiceStartDate = new DateOnly(2026, 1, 1), IsActive = true, IsPrintInvoiceMandatory = true };
        dbContext.Add(contract);
        await dbContext.SaveChangesAsync();
        unit.ContractId = contract.Id;
        await dbContext.SaveChangesAsync();
    }
}

static async Task<CurrentUserResponse> BuildCurrentUserAsync(
    ApplicationUser user,
    UserManager<ApplicationUser> userManager,
    SzAppDbContext dbContext,
    CancellationToken cancellationToken)
{
    var roles = (await userManager.GetRolesAsync(user)).ToArray();
    var companies = dbContext.Companies.AsNoTracking();
    if (!roles.Contains(SecurityConstants.RootRole, StringComparer.Ordinal))
    {
        companies = companies.Where(x => x.StaffAccess.Any(access => access.StaffId == user.Id));
    }

    var allowedCompanies = await companies.OrderBy(x => x.ShortName)
        .Select(x => new CompanySummaryResponse(x.Id, x.ShortName, x.Partner.RegistrationNumber))
        .ToArrayAsync(cancellationToken);

    return new CurrentUserResponse(
        user.Id,
        user.UserName ?? user.Email ?? string.Empty,
        user.Email ?? string.Empty,
        user.PreferredLanguage,
        roles,
        allowedCompanies);
}

public partial class Program;
