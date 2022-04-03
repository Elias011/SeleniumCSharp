using OpenQA.Selenium;
using CSharpSeleniumFramework.Engine;

namespace CSharpSeleniumFramework.Pages.algemeen
{
    public class GeneralElements
    {
        private readonly IWebDriver _driver;

        public GeneralElements(IWebDriver driver)
        {
            _driver = driver;
        }

        public IWebElement Contact => _driver.FindElementSafe(By.XPath("/html/body/nav/div[1]/ul/li[2]/a"));

    }
}