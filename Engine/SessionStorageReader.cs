using OpenQA.Selenium;
using OpenQA.Selenium.DevTools.V85.Debugger;

namespace CSharpSeleniumFramework.Engine
{
    public class SessionStorageReader
    {
        private readonly IWebDriver _driver;

        public SessionStorageReader(IWebDriver driver)
        {
            _driver = driver;
        }

        public string Read(string key)
        {
            var cookie = _driver.Manage().Cookies.GetCookieNamed($"{key}").ToString();
            var js = _driver.AsJavascriptExecutor();
            return cookie;
        }
    }
}
