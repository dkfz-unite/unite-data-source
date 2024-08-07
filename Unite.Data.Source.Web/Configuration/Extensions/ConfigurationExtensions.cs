using Unite.Data.Source.Web.Configuration.Options;
using Unite.Data.Source.Web.Handlers;
using Unite.Data.Source.Web.Workers;

namespace Unite.Data.Source.Web.Configuration.Extensions;

public static class ConfigurationExtensions
{
    public static void Configure(this IServiceCollection services)
    {
        services.AddOptions();
        services.AddValidators();

        services.AddTransient<ExploringHandler>();
        services.AddHostedService<ExploringWorker>();
    }


    private static IServiceCollection AddOptions(this IServiceCollection services)
    {
        services.AddTransient<WorkerOptions>();
        services.AddTransient<AuthOptions>();
        services.AddTransient<ConfigOptions>();
        services.AddTransient<FeedOptions>();
        
        return services;
    }

    private static IServiceCollection AddValidators(this IServiceCollection services)
    {
        return services;
    }
}
