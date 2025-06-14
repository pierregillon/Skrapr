using Microsoft.Extensions.Options;
using Skrapr.Domain;

namespace Skrapr.Infra.Playwright;

public static class DependencyInjection
{
    public static IServiceCollection AddPlaywright(this IServiceCollection services)
    {
        services
            .AddOptions<PlaywrightMcpConfiguration>()
            .BindConfiguration(PlaywrightMcpConfiguration.SectionName)
            .ValidateDataAnnotations()
            .PostConfigure(x => x.EnsureValid());

        services
            .AddScoped<LocalPlaywrightMcpClient>()
            .AddScoped<RemotePlaywrightMcpClient>()
            .AddScoped<IPlaywrightMcpClient>(sp =>
            {
                var configuration = sp.GetRequiredService<IOptions<PlaywrightMcpConfiguration>>();

                return configuration.Value.IsLocal
                    ? sp.GetRequiredService<LocalPlaywrightMcpClient>()
                    : sp.GetRequiredService<RemotePlaywrightMcpClient>();
            });

        return services;
    }
}