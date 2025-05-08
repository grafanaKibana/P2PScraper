using Microsoft.Extensions.Options;
using Quartz;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace P2PScraper.Jobs;

using P2PScraper.Models;
using P2PScraper.Scrapers;

public class SendTelegramMessageJob(ITelegramBotClient botClient, IOptions<AppConfig> appConfig) : IJob
{
    private readonly ITelegramBotClient botClient = botClient ?? throw new ArgumentNullException(nameof(botClient));
    private readonly AppConfig applicationConfig = appConfig.Value ?? throw new ArgumentNullException(nameof(appConfig));

    public async Task Execute(IJobExecutionContext context)
    {
        var order = new ByBitP2POrdersScraper().CheckOffers(appConfig.Value.P2PAccountName);
        using var cts = new CancellationTokenSource();
        
        if (order != null)
        {
            await this.botClient.SendTextMessageAsync(
                chatId: this.applicationConfig.TelegramConfig.ChatId,
                text: $"Offer of {this.applicationConfig.P2PAccountName} advertiser has been found!\n\n" +
                      $"*Price:* {order.Price}\n" +
                      $"*Available:* {order.AvailableAmount}\n" +
                      $"*Limits:* {order.Limits}\n" +
                      $"*Payment Methods:* {string.Join(", ", order.PaymentMethods)}\n",
                parseMode: ParseMode.Markdown,
                cancellationToken: cts.Token);
            
            
        }
        else
        {
            await this.botClient.SendTextMessageAsync(
                chatId: this.applicationConfig.TelegramConfig.ChatId,
                text: $"No P2P orders for now =(",
                cancellationToken: cts.Token);
        }
        await cts.CancelAsync();
    }
}