using Skrapr;

var builder = WebApplication.CreateBuilder(args);

builder
    .Configuration
    .AddEnvironmentVariables();

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithTools<ExtractWebSiteDataTool>();

builder.Services
    .AddOptions<AzureOpenAiConfiguration>()
    .BindConfiguration(AzureOpenAiConfiguration.SectionName)
    .ValidateDataAnnotations();

builder.Services
    .AddOptions<PlaywrightMcpConfiguration>()
    .BindConfiguration(PlaywrightMcpConfiguration.SectionName)
    .ValidateDataAnnotations();

var app = builder.Build();

app.MapMcp();

app.Run();

public partial class Program { }