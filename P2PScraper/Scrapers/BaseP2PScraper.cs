namespace P2PScraper.Scrapers;

using OpenQA.Selenium.Chrome;

public class BaseP2PScraper
{
    protected virtual ChromeDriver SetUpDriver(string url)
    {
        var driverOptions = new ChromeOptions();
        driverOptions.AddArguments([
            //"--headless=new",
            "--no-sandbox",
            "--start-minimized",
            "disable-infobars",
            "--disable-dev-shm-usage",
            "--ignore-certificate-errors",
            "--allow-running-insecure-content",
            "--disable-blink-features=AutomationControlled"
        ]);

        var driver = new ChromeDriver(options: driverOptions);

        driver.Navigate().GoToUrl(url);
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        return driver;
    }
}