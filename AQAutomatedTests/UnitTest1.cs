using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace AirQualityDashboardAutomated.Tests
{
    [TestFixture]
    public class BasicTests
    {
        [Test]
        public void AlwaysPasses()
        {
            IWebDriver driver = new ChromeDriver();

            driver.Navigate().GoToUrl("");
            driver.Manage().Window.Maximize();
        }
    }
}
