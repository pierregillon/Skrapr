using Skrapr;
using Skrapr.Domain;
using Skrapr.Infra;
using Skrapr.Presentation.HealthCheck;
using Skrapr.Presentation.Telemetry;

var builder = WebApplication.CreateBuilder(args);

builder
    .Configuration
    .AddEnvironmentVariables();

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithTools<ScrapingTools>();

builder.Services
    .AddHealthCheckServices()
    .AddTelemetryServices(builder.Environment, builder.Configuration)
    .AddDomain()
    .AddInfrastructure();

builder.Logging
    .ConfigureTelemetryLogging(builder.Configuration);

var app = builder.Build();

app.MapMcp();
app
    .ConfigureHealthChecksRoutes()
    .ConfigureTelemetry(app.Configuration);

app.Run();

namespace Skrapr
{
    public partial class Program { }
}