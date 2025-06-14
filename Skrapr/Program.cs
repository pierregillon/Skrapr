using Skrapr;
using Skrapr.Domain;
using Skrapr.Infra;

var builder = WebApplication.CreateBuilder(args);

builder
    .Configuration
    .AddEnvironmentVariables();

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithTools<ScrapingTools>();

builder.Services
    .AddDomain()
    .AddInfrastructure();

var app = builder.Build();

app.MapMcp();

app.Run();

namespace Skrapr
{
    public partial class Program { }
}