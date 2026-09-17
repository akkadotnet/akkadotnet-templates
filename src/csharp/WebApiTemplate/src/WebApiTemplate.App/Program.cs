using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using WebApiTemplate.App.Configuration;

var builder = WebApplication.CreateBuilder(args);

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

/*
 * CONFIGURATION SOURCES
 */
builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{environment}.json", optional: true)
    .AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddHealthChecks();
builder.Services.ConfigureWebApiAkka(builder.Configuration, (akkaConfigurationBuilder, serviceProvider) =>
{
    // we configure instrumentation separately from the internals of the ActorSystem
    akkaConfigurationBuilder.ConfigurePetabridgeCmd();
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName.Equals("Azure"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Only redirect to HTTPS when an HTTPS port is actually configured. Under .NET Aspire
// (and the container image) only an http endpoint is wired, so there is no 443 to redirect to.
var httpsPort = builder.Configuration["ASPNETCORE_HTTPS_PORT"];
if (!string.IsNullOrEmpty(httpsPort))
{
    app.UseHttpsRedirection();
}

// Akka.NET liveness + cluster membership health checks are registered by
// WithAspireClusterBootstrap; map ASP.NET Core endpoints for them here.
app.MapHealthChecks("/healthz");
app.MapHealthChecks("/healthz/live", new HealthCheckOptions
{
    Predicate = c => c.Tags.Contains("liveness")
});
app.MapHealthChecks("/healthz/ready", new HealthCheckOptions
{
    Predicate = c => c.Tags.Contains("readiness")
});

app.UseAuthorization();

app.MapControllers();

app.Run();
