using Skrapr.Infra.Playwright;
using Skrapr.Infra.Telemetry;

namespace Skrapr.Infra;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IWebHostEnvironment environment,
        ConfigurationManager configuration
    ) =>
        services
            .AddPlaywright()
            .AddTelemetryServices(environment, configuration);

    public static WebApplication ConfigureInfrastructure(this WebApplication app)
    {
        app.ConfigureTelemetry(app.Configuration);
        
        return app;
    }
}