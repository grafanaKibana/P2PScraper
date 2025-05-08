using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace P2PScraper.Jobs;

using P2PScraper.Models;
using P2PScraper.Scrapers;

public class TelegramJob
{
    protected TelegramJob(ITelegramBotClient botClient, IOptions<AppConfig> appConfig)
    {
        this.ApplicationConfig = appConfig.Value;
        this.AdvertiserName = this.ApplicationConfig.P2PAccountName;
        this.Bot = botClient;
    }

    protected AppConfig ApplicationConfig { get; }
    protected string AdvertiserName { get; init; }
    protected ByBitP2POrdersScraper P2POrdersScraper => new();
    protected ITelegramBotClient Bot { get; }
}