using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace AQAutomatedTests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            IWebDriver driver = new ChromeDriver();

            driver.Navigate().GoToUrl("");

            driver.Manage().Window.Maximize();
        }
    }
}