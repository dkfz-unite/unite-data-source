using Unite.Data.Source.Web.Configuration.Options;

namespace Unite.Data.Source.Web.Configuration.Extensions;

public static class ConfigurationExtensions
{
    public static void Configure(this IServiceCollection services)
    {
        services.AddOptions();
        services.AddValidators();
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
