namespace P2PScraper.DataAccess.Entities;

public class Chat
{
    public long Id { get; init; }

    public ChatState State { get; set; }

    public string? BotToken { get; set; }

    public long BotChatId { get; set; }

    public bool IsBotEnabled { get; set; }

    public string? P2PUsername { get; set; }

    public int NotificationIntervalInMinutes { get; set; }

    public DateTime? LastNotificationDate { get; set; }
}

public enum ChatState
{
    NewUser,
    RegistrationProcessingBotToken,
    RegistrationProcessingChatId,
    RegistrationProcessingP2PUserName,
    Registered
}