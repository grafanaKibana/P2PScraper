namespace P2PScraper.Models;

public record TelegramConfig
{
    public required string BotToken { get; set; }
    public required string ChatId { get; set; }
}