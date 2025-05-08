namespace P2PScraper.Models;

// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
public record AppConfig
{
    public required string P2PAccountName { get; set; }
    public required int NotificationIntervalInMinutes { get; set; }
    public required TelegramConfig TelegramConfig { get; set; }
}