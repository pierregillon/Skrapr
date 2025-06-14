namespace Skrapr.Domain;

public static class DependencyInjection
{
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        services
            .AddScoped<IAgent, SemanticKernelAgent>();

        services
            .AddOptions<AzureOpenAiConfiguration>()
            .BindConfiguration(AzureOpenAiConfiguration.SectionName)
            .ValidateDataAnnotations();

        return services;
    }
}