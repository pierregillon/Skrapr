using Microsoft.Extensions.Options;
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
    .ValidateDataAnnotations()
    .PostConfigure(x => x.EnsureValid());

builder.Services
    .AddScoped<IAgent, SemanticKernelAgent>()
    .AddScoped<LocalPlaywrightMcpClient>()
    .AddScoped<RemotePlaywrightMcpClient>()
    .AddScoped<IPlaywrightMcpClient>(sp =>
    {
        var configuration = sp.GetRequiredService<IOptions<PlaywrightMcpConfiguration>>();

        return configuration.Value.IsLocal
            ? sp.GetRequiredService<LocalPlaywrightMcpClient>()
            : sp.GetRequiredService<RemotePlaywrightMcpClient>();
    });

var app = builder.Build();

app.MapMcp();

app.Run();

namespace Skrapr
{
    public partial class Program { }
}