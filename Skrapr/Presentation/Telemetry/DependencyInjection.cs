using Azure.Monitor.OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Skrapr.Presentation.Telemetry;

public static class DependencyInjection
{
    private const string ApplicationInsightsConnectionStringKey = "ApplicationInsights:ConnectionString";
    private static readonly string[] HealthCheckRoutes = ["/hc", "/liveness"];

    public static IServiceCollection AddTelemetryServices(
        this IServiceCollection services,
        IHostEnvironment hostEnvironment,
        IConfiguration configuration
    )
    {
        services.AddSingleton<Instrumentation>();

        services
            .AddTransient<CompleteHttpRequestTelemetryMiddleware>()
            .AddTransient<IActivityAccessor, StaticActivityAccessor>();

        services
            .AddOpenTelemetry()
            .ConfigureResource(builder => builder.AddService(hostEnvironment.ApplicationName))
            .WithTracing(builder =>
            {
                builder = builder
                    .AddSource(Instrumentation.ActivitySourceName)
                    .SetSampler(new AlwaysOnSampler())
                    .SetErrorStatusOnException()
                    .AddAspNetCoreInstrumentation(opt =>
                    {
                        opt.RecordException = true;
                        opt.Filter = context => !IsHealthCheckRequest(context.Request);
                    })
                    .AddHttpClientInstrumentation();
                
                if (configuration.TryGetConnectionString(out var connectionString))
                {
                    builder.AddAzureMonitorTraceExporter(o => o.ConnectionString = connectionString);
                }
                else
                {
                    builder.AddConsoleExporter();
                }
            });

        return services;
    }

    private static bool IsHealthCheckRequest(HttpRequest request) =>
        !HealthCheckRoutes.Equals(request.Path);

    public static ILoggingBuilder ConfigureTelemetryLogging(this ILoggingBuilder builder, IConfiguration configuration)
    {
        if (!configuration.TryGetConnectionString(out var connectionString))
        {
            return builder;
        }

        return builder
            .AddOpenTelemetry(options =>
            {
                options.IncludeFormattedMessage = true;
                options.IncludeScopes = true;
                options.AddAzureMonitorLogExporter(o => o.ConnectionString = connectionString);
            });
    }

    public static IApplicationBuilder ConfigureTelemetry(this IApplicationBuilder app, IConfiguration configuration)
    {
        if (!configuration.TryGetConnectionString(out _))
        {
            return app;
        }

        app.UseMiddleware<CompleteHttpRequestTelemetryMiddleware>();

        return app;
    }

    private static bool TryGetConnectionString(this IConfiguration configuration, out string? connectionString)
    {
        connectionString = configuration.GetSection(ApplicationInsightsConnectionStringKey).Value;

        return !string.IsNullOrWhiteSpace(connectionString);
    }
}
