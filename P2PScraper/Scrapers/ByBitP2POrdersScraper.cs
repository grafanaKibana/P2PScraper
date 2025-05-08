namespace P2PScraper.Scrapers;

using OpenQA.Selenium;
using P2PScraper.Models;

public class ByBitP2POrdersScraper(string fiatCurrency = "UAH", string cryptoCurrency = "USDT") : BaseP2PScraper
{
    private string BaseUrl =>
        $"https://www.bybit.com/fiat/trade/otc/?actionType=1&token={cryptoCurrency}&fiat={fiatCurrency}&paymentMethod=";

    public P2POrderModel? CheckOffers(string advertiserName)
    {
        using var driver = SetUpDriver(this.BaseUrl);

        /*
            Screenshot ss = ((ITakesScreenshot)driver).GetScreenshot();
            ss.SaveAsFile($"p2pScreen{Guid.NewGuid()}.png");
        */

        var page = 1;
        while (true)
        {
            var rows = driver.FindElements(By.CssSelector("tbody.trade-table__tbody > tr"));
            var matchedOffers = rows
                .Where(row => row.FindElement(By.CssSelector("div.advertiser-name > span")).Text == advertiserName)
                .ToList();

            if (matchedOffers.Count != 0)
            {
                foreach (var matchedOffer in matchedOffers)
                {
                    Console.WriteLine($"Offer of {advertiserName} advertiser has been found!\n");

                    var name = matchedOffer.FindElement(By.CssSelector("div.advertiser-name > span")).Text;
                    var price = matchedOffer.FindElement(By.CssSelector("span.price-amount")).Text;
                    var available = matchedOffer.FindElement(By.XPath(".//td[3]/div/div[2]/div/div[1]/div")).Text;
                    var limits = matchedOffer.FindElement(By.XPath(".//td[3]/div/div[2]/div/div[2]/div")).Text;
                    var paymentMethods = matchedOffer
                        .FindElements(By.CssSelector("div.trade-list-tag"))
                        .Select(x => x.Text)
                        .ToList();

                    var order = new P2POrderModel(name, price, available, limits, paymentMethods);
                    return order;
                }
            }
            Console.WriteLine($"Advertiser`s offers not found on page #{page}");
            page += 1;

            var nextPageButton = driver
                .FindElement(By.CssSelector("div.trade-table__pagination > ul > li.pagination-next > button"));

            if (!nextPageButton.Enabled)
            {
                Console.WriteLine($"Offer of {advertiserName} advertiser not found!\n");
                return null;
            }

            nextPageButton.Click();
            Thread.Sleep(3000);
        }
    }

    public IEnumerable<P2POrderModel> CheckOffers(List<string?> advertiserNames)
    {
        var resultOffers = new List<P2POrderModel>();
        using var driver = SetUpDriver(this.BaseUrl);

        var page = 1;
        while (true)
        {
            var rows = driver.FindElements(By.CssSelector("tbody.trade-table__tbody > tr"));
            var matchedOffers = rows
                .Where(row => advertiserNames.Contains(row.FindElement(By.CssSelector("div.advertiser-name > span")).Text))
                .ToList();

            if (matchedOffers.Count != 0)
            {
                foreach (var matchedOffer in matchedOffers)
                {
                    var name = matchedOffer.FindElement(By.CssSelector("div.advertiser-name > span")).Text;
                    var price = matchedOffer.FindElement(By.CssSelector("span.price-amount")).Text;
                    var available = matchedOffer.FindElement(By.XPath(".//td[3]/div/div[2]/div/div[1]/div")).Text;
                    var limits = matchedOffer.FindElement(By.XPath(".//td[3]/div/div[2]/div/div[2]/div")).Text;
                    var paymentMethods = matchedOffer
                        .FindElements(By.CssSelector("div.trade-list-tag"))
                        .Select(x => x.Text)
                        .ToList();

                    Console.WriteLine($"Offer of {name} advertiser has been found!\n");

                    resultOffers.Add(new P2POrderModel(name, price, available, limits, paymentMethods));
                }
            }
            Console.WriteLine($"Advertisers offers not found on page #{page}");
            page += 1;

            var nextPageButton = driver
                .FindElement(By.CssSelector("div.trade-table__pagination > ul > li.pagination-next > button"));

            if (!nextPageButton.Enabled)
            {
                Console.WriteLine($"Offer of {advertiserNames} advertiser not found!\n");
                return resultOffers;
            }

            nextPageButton.Click();
            Thread.Sleep(3000);
        }
    }
}