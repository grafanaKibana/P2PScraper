namespace P2PScraper.Services.Receiver;

using Microsoft.Extensions.Logging;
using P2PScraper.Handlers;
using Telegram.Bot;

// Compose Receiver and UpdateHandler implementation
public class ReceiverService(
    ITelegramBotClient botClient,
    BotMessageHandler messageHandler,
    ILogger<ReceiverServiceBase<BotMessageHandler>> logger)
    : ReceiverServiceBase<BotMessageHandler>(botClient, messageHandler, logger) ;