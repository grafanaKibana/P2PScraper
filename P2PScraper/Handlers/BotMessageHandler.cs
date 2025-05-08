namespace P2PScraper.Handlers;

using Microsoft.Extensions.Logging;
using NeoSmart.Unicode;
using P2PScraper.DataAccess;
using P2PScraper.DataAccess.Entities;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using static System.Int64;
using Chat = P2PScraper.DataAccess.Entities.Chat;

public class BotMessageHandler(ITelegramBotClient botClient, ILogger<BotMessageHandler> logger, BotContext dbContext) : IUpdateHandler
{
    public async Task HandleUpdateAsync(ITelegramBotClient _, Update update, CancellationToken cancellationToken)
    {
        var handler = update switch
        {
            {Message: { } message} => BotOnMessageReceived(message, cancellationToken),
            {EditedMessage: { } message} => BotOnMessageReceived(message, cancellationToken),
            _ => UnknownUpdateHandlerAsync(update, cancellationToken)
        };

        await handler;
    }

    private async Task BotOnMessageReceived(Message message, CancellationToken ct)
    {
        logger.LogInformation($"Receive message type: {message.Type}; Chat ID: {message.Chat.Id}");

        if (message.Text is not { } messageText)
            return;

        var userExist = dbContext.Chats.Any(x => x.Id == message.Chat.Id);

        if (!userExist)
        {
            dbContext.Chats.Add(new Chat
            {
                Id = message.Chat.Id,
                State = ChatState.NewUser,
            });

            await dbContext.SaveChangesAsync(ct);
        }

        var chat = dbContext.Chats.Single(x => x.Id == message.Chat.Id);

        if (chat.State == ChatState.Registered)
        {
            var action = messageText switch
            {
                not null when messageText.Equals("/menu") || messageText.StartsWith("Menu") => Menu(message, ct),
                not null when messageText.Equals("/start_bot") || messageText.StartsWith("Start Bot") => StartBot(message, chat, ct),
                not null when messageText.Equals("/pause_bot") || messageText.StartsWith("Pause Bot") => PauseBot(message, chat, ct),
                not null when messageText.Equals("/edit_bot") || messageText.StartsWith("Edit Bot") => EditBot(message, chat, ct),
                not null when messageText.Equals("/delete_bot") || messageText.StartsWith("Delete Bot") => DeleteBot(message, chat, ct),
                _ => UnknownCommand(message, ct)
            };
            var sentMessage = await action;
            logger.LogInformation($"The message was sent with id: {sentMessage.MessageId}");
        }
        else
        {
            var action = messageText switch
            {
                not null when messageText.StartsWith("/start") => Start(message, chat, ct),
                not null when messageText.StartsWith("/register_bot") || messageText.StartsWith("Register") => RegisterBot(message, chat, ct),
                _ => RegisterBot(message, chat, ct)
            };
            var sentMessage = await action;
            logger.LogInformation($"The message was sent with id: {sentMessage.MessageId}");
        }
    }


    #region Commands

    private async Task<Message> Start(Message message, Chat chat, CancellationToken ct)
    {
        string welcomeText = $"""
                              Hi, {message.Chat.FirstName}!
                              Please complete registration {Emoji.BackhandIndexPointingDown}
                              """;


        return await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: welcomeText,
            replyMarkup: new ReplyKeyboardMarkup([new KeyboardButton($"Register {Emoji.Memo}")]),
            cancellationToken: ct
        );
    }

    private async Task<Message> Menu(Message message, CancellationToken ct)
    {
        var buttons = new ReplyKeyboardMarkup([
            [
                new KeyboardButton($"Start Bot {Emoji.GreenCircle}"), new KeyboardButton($"Pause Bot {Emoji.YellowCircle}")
            ],
            [
                new KeyboardButton($"Edit Bot {Emoji.Gear}"), new KeyboardButton($"Delete Bot {Emoji.CrossMark}")
            ]
        ]);
        return await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: $"Main menu {Emoji.Pushpin}",
            replyMarkup: buttons,
            cancellationToken: ct
        );
    }

    private async Task<Message> RegisterBot(Message message, Chat chat, CancellationToken ct)
    {
        switch (chat.State)
        {
            case ChatState.NewUser:
            {
                chat.State = ChatState.RegistrationProcessingBotToken;
                dbContext.Chats.Update(chat);
                await dbContext.SaveChangesAsync(ct);

                return await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: $"Please provide your bot`s token from @BotFather {Emoji.Robot}",
                    replyMarkup: new ReplyKeyboardRemove(),
                    cancellationToken: ct);
            }
            case ChatState.RegistrationProcessingBotToken:
            {
                try
                {
                    await new TelegramBotClient(message.Text ?? string.Empty).GetMeAsync(cancellationToken: ct);
                    chat.BotToken = message.Text ?? throw new ArgumentNullException(message.Text, "There is no message to set BotToken");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: $"Bot token is invalid {Emoji.SlightlyFrowningFace}. Please check and send it again.",
                        cancellationToken: ct);
                }

                chat.State = ChatState.RegistrationProcessingChatId;
                dbContext.Chats.Update(chat);
                await dbContext.SaveChangesAsync(ct);

                return await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: $"Thank you! Now please provide chat ID where you want to notify your users. {Emoji.SpeechBalloon}",
                    cancellationToken: ct);
            }
            case ChatState.RegistrationProcessingChatId:
            {
                var chatIdIsValid = TryParse(message.Text, out var chatId);

                if (!chatIdIsValid)
                {
                    return await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: $"Chat ID is invalid {Emoji.SlightlyFrowningFace}. Please check and send it again.",
                        cancellationToken: ct);
                }

                chat.BotChatId = chatId;
                chat.State = ChatState.RegistrationProcessingP2PUserName;
                dbContext.Chats.Update(chat);
                await dbContext.SaveChangesAsync(ct);

                return await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: $"Good job! Last but not least, send me your nickname on ByBit {Emoji.ChartIncreasing}",
                    cancellationToken: ct);
            }
            case ChatState.RegistrationProcessingP2PUserName:
            {
                chat.P2PUsername = message.Text;
                chat.State = ChatState.Registered;
                dbContext.Chats.Update(chat);
                await dbContext.SaveChangesAsync(ct);

                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: $"We`re done! Thank you {Emoji.SlightlySmilingFace}",
                    cancellationToken: ct);

                return await Menu(message, ct);
            }
            case ChatState.Registered:
            {
                await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: $"Thank you for your passion, but you`re already registered user, congratulations {Emoji.Fireworks}", cancellationToken: ct);
                return await Menu(message, ct);
            }
            default:
            {
                return await UnknownCommand(message, ct);
            }
        }
    }

    private async Task<Message> StartBot(Message message, Chat chat, CancellationToken ct)
    {
        chat.IsBotEnabled = true;
        dbContext.Update(chat);
        await dbContext.SaveChangesAsync(ct);

        return await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: $"Bot successfully enabled {Emoji.GreenCircle}",
            cancellationToken: ct
        );
    }

    private async Task<Message> PauseBot(Message message, Chat chat, CancellationToken ct)
    {
        chat.IsBotEnabled = false;
        dbContext.Update(chat);
        await dbContext.SaveChangesAsync(ct);

        return await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: $"Bot successfully disabled {Emoji.YellowCircle}",
            cancellationToken: ct
        );
    }

    private async Task<Message> EditBot(Message message, Chat chat, CancellationToken ct)
    {
        return await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: $"What do you want change?",
            replyMarkup: new ReplyKeyboardMarkup([new KeyboardButton($"Bot Token {Emoji.Robot}"), new KeyboardButton($"Chat ID {Emoji.SpeechBalloon}")]),
            cancellationToken: ct
        );
    }

    private async Task<Message> DeleteBot(Message message, Chat chat, CancellationToken ct)
    {
        chat.BotToken = null;
        chat.BotChatId = 0;
        chat.State = ChatState.NewUser;
        chat.IsBotEnabled = false;

        dbContext.Chats.Update(chat);
        await dbContext.SaveChangesAsync(ct);

        return await RegisterBot(message, chat, ct);
    }

    #endregion

    #region ErrorHandling

    private async Task<Message> UnknownCommand(Message message, CancellationToken ct)
    {
        return await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: $"Sorry I am not recognize this command {Emoji.ManFrowning}",
            cancellationToken: ct
        );
    }

    private Task UnknownUpdateHandlerAsync(Update update, CancellationToken cancellationToken)
    {
        logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
        return Task.CompletedTask;
    }

    public async Task HandlePollingErrorAsync(ITelegramBotClient _, Exception exception, CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        logger.LogInformation("HandleError: {ErrorMessage}", errorMessage);

        // Cooldown in case of network connection error
        if (exception is RequestException)
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    }

    #endregion
}