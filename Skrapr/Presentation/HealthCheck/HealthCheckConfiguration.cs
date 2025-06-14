using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Skrapr.Presentation.HealthCheck;

public static class HealthCheckConfiguration
{
    public static IServiceCollection AddHealthCheckServices(this IServiceCollection services)
    {
        services
            .AddHealthChecks()
            .AddLivenessChecks()
            .AddReadinessChecks();

        return services;
    }

    private static IHealthChecksBuilder AddLivenessChecks(this IHealthChecksBuilder builder) =>
        builder.AddCheck(
            "Api liveness check",
            () => HealthCheckResult.Healthy("the service is live."),
            ["api", "liveness"]
        );

    private static IHealthChecksBuilder AddReadinessChecks(this IHealthChecksBuilder builder) =>
        builder;

    public static IApplicationBuilder ConfigureHealthChecksRoutes(this IApplicationBuilder builder) =>
        builder
            .UseHealthChecks(
                "/liveness",
                new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("liveness"),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                    ResultStatusCodes =
                    {
                        [HealthStatus.Healthy] = StatusCodes.Status200OK,
                        [HealthStatus.Degraded] = StatusCodes.Status200OK,
                        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
                    }
                }
            )
            .UseHealthChecks(
                "/hc",
                new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("readiness"),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                    ResultStatusCodes =
                    {
                        [HealthStatus.Healthy] = StatusCodes.Status200OK,
                        [HealthStatus.Degraded] = StatusCodes.Status200OK,
                        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
                    }
                }
            );
}
