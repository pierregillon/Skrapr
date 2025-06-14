using Skrapr;
using Skrapr.Domain;
using Skrapr.Infra;
using Skrapr.Infra.Playwright;

var builder = WebApplication.CreateBuilder(args);

builder
    .Configuration
    .AddEnvironmentVariables();

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithTools<ScrapingTools>();

builder.Services
    .AddOptions<AzureOpenAiConfiguration>()
    .BindConfiguration(AzureOpenAiConfiguration.SectionName)
    .ValidateDataAnnotations();

builder.Services
    .AddOptions<PlaywrightMcpConfiguration>()
    .BindConfiguration(PlaywrightMcpConfiguration.SectionName)
    .ValidateDataAnnotations();

builder.Services
    .AddScoped<IAgent, SemanticKernelAgent>()
    .AddScoped<IPlaywrightMcpClient, LocalMcpServerPlaywrightConfigurator>();

var app = builder.Build();

app.MapMcp();

app.Run();

namespace Skrapr
{
    public partial class Program { }
}