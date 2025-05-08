namespace P2PScraper.Models;

public record P2POrderModel(string AdvertiserName, string Price, string AvailableAmount, string Limits, IEnumerable<string> PaymentMethods);