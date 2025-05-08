namespace P2PScraper.Extensions;

using Microsoft.Extensions.DependencyInjection;
using P2PScraper.Handlers;
using P2PScraper.Services.Pooling;
using P2PScraper.Services.Receiver;

public static class AddDependencyInjectionExtension
{
    public static IServiceCollection AddDependencyInjection(this IServiceCollection services)
    {
        services.AddScoped<BotMessageHandler>();
        services.AddScoped<ReceiverService>();
        services.AddHostedService<PollingService>();

        return services;
    }
}