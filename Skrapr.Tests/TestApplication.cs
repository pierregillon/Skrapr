using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Skrapr.Tests.Utils;
using Xunit.Abstractions;

namespace Skrapr.Tests;

public class TestApplication(ITestOutputHelper output) : WebApplicationFactory<Program>
{
    public async Task<HttpClient> SafeCreateClient()
    {
        HttpClient? client = null;

        for (var i = 0; i < 10; i++)
        {
            try
            {
                client = Server.CreateClient();
            }
            catch (InvalidOperationException) when (i < 9)
            {
                await Task.Delay(50);
            }
        }

        return client ?? throw new InvalidOperationException("Cannot create client");
    }
    
    protected override IHost CreateHost(IHostBuilder builder)
    {
        DotEnv.Load(".env");
        
        return builder
            .ConfigureAppConfiguration(config => config.AddEnvironmentVariables())
            .ConfigureServices(services => { })
            .ConfigureLogging(x =>
            {
                x.AddSimpleConsole(option =>
                {
                    option.IncludeScopes = false;
                    option.TimestampFormat = "hh:mm:ss ";
                });
                x.Services.AddSingleton<ILoggerProvider>(BuildDelegateLoggerProvider);
            })
            .Build();
    }

    private DelegateLoggerProvider BuildDelegateLoggerProvider(IServiceProvider serviceProvider)
    {
        var action = new Action<string>(output.WriteLine);
        return ActivatorUtilities.CreateInstance<DelegateLoggerProvider>(serviceProvider, action);
    }
}

public static class DotEnv
{
    public static void Load(string filePath)
    {
        if (!File.Exists(filePath))
            return;

        foreach (var line in File.ReadAllLines(filePath))
        {
            var parts = line.Split(
                '=',
                StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2)
                continue;

            Environment.SetEnvironmentVariable(parts[0], parts[1]);
        }
    }
}
