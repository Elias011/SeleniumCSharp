using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using CSharpSeleniumFramework.Engine;

namespace CSharpSeleniumFramework.Pages.algemeen
{
    public class GeneralActions
    {
        private readonly IWebDriver _driver;
        

        public GeneralActions(IWebDriver driver)
        {
            _driver = driver;
        }

        public void ClickOnEmptySpace(WebdriverDefinitions.BrowserTypes browserTypes)
        {
            string browserName = browserTypes.ToString();
            if (browserName == "SafariMac")
            {
                _driver.FindElementSafe(By.XPath("/html/body")).SendKeys(Keys.Tab);
            }
            else
            {
                Actions action = new Actions(_driver);
                action.MoveByOffset(0, 0).Click().Build().Perform();
            }
        }
    }
}