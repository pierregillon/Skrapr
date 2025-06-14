using Skrapr.Infra.Playwright;

namespace Skrapr.Infra;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services) =>
        services
            .AddPlaywright();
}