using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace P2PScraper.Extensions;

using P2PScraper.Models;

public static class AddConfigurationExtension
{
    public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfigurationSection config)
    {
        services.AddOptions();
        services.Configure<AppConfig>(config);

        return services;
    }
}