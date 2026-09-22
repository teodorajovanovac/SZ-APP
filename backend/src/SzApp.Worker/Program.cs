using Microsoft.EntityFrameworkCore;
using SzApp.Data;
using SzApp.Worker;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("SzApp")
    ?? throw new InvalidOperationException("Connection string 'SzApp' is required.");

builder.Services.AddDbContextFactory<SzAppDbContext>(options =>
    options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IOutboxMessageHandler, EmailOutboxMessageHandler>();
builder.Services.AddHostedService<OutboxWorker>();
builder.Services.AddHealthChecks();

var app = builder.Build();
app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");
await app.RunAsync();
