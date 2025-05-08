namespace P2PScraper;

using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using P2PScraper.DataAccess;
using P2PScraper.Extensions;
using P2PScraper.Models;
using Telegram.Bot;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = CreateBuilder(args);

        try
        {
            var host = builder.Build();
            await host.RunAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private static IHostBuilder CreateBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureHostConfiguration(cfg => cfg.SetBasePath(AppDomain.CurrentDomain.BaseDirectory).AddJsonFile("appconfig.json"))
            .ConfigureServices((ctx, services) =>
            {
                var appConfig = ctx.Configuration.GetRequiredSection(nameof(AppConfig));

                services.AddDependencyInjection();
                services.AddConfiguration(appConfig);
                services.AddDbContext<BotContext>(options => options.UseSqlite($"Data Source={Assembly.GetExecutingAssembly().GetName().Name}.db"));

                //services.AddQuartz(appConfig);

                services.AddHttpClient("telegram_bot_client")
                    .AddTypedClient<ITelegramBotClient>((httpClient, _) =>
                    {
                        var botToken = appConfig
                            .GetRequiredSection(nameof(TelegramConfig))
                            .GetValue<string>(nameof(TelegramConfig.BotToken));

                        if (string.IsNullOrEmpty(botToken))
                        {
                            throw new ArgumentNullException(nameof(botToken));
                        }

                        TelegramBotClientOptions options = new(botToken);
                        return new TelegramBotClient(options, httpClient);
                    });
            });
    }
}