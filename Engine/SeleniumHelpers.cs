using System;
using System.Threading;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;

namespace CSharpSeleniumFramework.Engine
{
    public class SeleniumHelpers
    {
        public static void OpenRealWorldApp(IWebDriver driver)
        {
            Console.WriteLine("Start open Real World App");
            Assert.NotNull(driver, "Browser (driver) not available, is it started correctly?, Browser is not available, is this correctly started?");
            driver.Navigate().GoToUrl(TestRunSettings.RealWorldApp);
            var url = driver.Url;
            Assert.AreEqual(url, TestRunSettings.RealWorldApp, "Expect: webdriver navigates to real world app page");
        }
    }
}