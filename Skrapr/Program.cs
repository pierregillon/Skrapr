using Skrapr.Domain;
using Skrapr.Infra;
using Skrapr.Infra.Telemetry;
using Skrapr.Presentation;

var builder = WebApplication.CreateBuilder(args);

builder
    .Configuration
    .AddEnvironmentVariables();

builder.Services
    .AddPresentation()
    .AddDomain()
    .AddInfrastructure(builder.Environment, builder.Configuration);

builder.Logging
    .ConfigureTelemetryLogging(builder.Configuration);

var app = builder.Build();

app
    .ConfigurePresentation()
    .ConfigureInfrastructure();

app.Run();

namespace Skrapr
{
    public class Program { }
}