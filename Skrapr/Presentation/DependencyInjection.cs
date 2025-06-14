using Skrapr.Infra.Telemetry;
using Skrapr.Presentation.HealthCheck;

namespace Skrapr.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(
        this IServiceCollection services
    )
    {
        services
            .AddMcpServer()
            .WithHttpTransport()
            .WithTools<ScrapingTools>();
            
        services
            .AddHealthCheckServices();

        return services;
    }

    public static WebApplication ConfigurePresentation(this WebApplication app)
    {
        app.MapMcp();
        app.ConfigureHealthChecksRoutes();
        
        return app;
    }
}