namespace P2PScraper.Jobs;

using Quartz;
using Telegram.Bot;
using P2PScraper.DataAccess;
using P2PScraper.Scrapers;

public class SendTelegramMessagesJob(BotContext context) : IJob
{
    private IList<ITelegramBotClient> BotClients { get; set; } = new List<ITelegramBotClient>();

    private BotContext DbContext { get; set; } = context;

    public async Task Execute(IJobExecutionContext context)
    {
        using var cts = new CancellationTokenSource();

        var chats = this.DbContext.Chats
            .Where(x => x.IsBotEnabled)
            .Where(x => !x.LastNotificationDate.HasValue || x.LastNotificationDate.Value.AddMinutes(x.NotificationIntervalInMinutes) > DateTime.Now);

        foreach (var chat in chats)
        {
            ArgumentNullException.ThrowIfNull(chat.BotChatId);
            var bot = new TelegramBotClient(chat.BotToken!);
        }

        var orders = new ByBitP2POrdersScraper().CheckOffers(chats.Select(x => x.P2PUsername).ToList());
    }
}