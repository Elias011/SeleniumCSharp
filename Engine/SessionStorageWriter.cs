using OpenQA.Selenium;

namespace CSharpSeleniumFramework.Engine
{
    public class SessionStorageWriter
    {
        private readonly IWebDriver _driver;

        public SessionStorageWriter(IWebDriver driver)
        {
            _driver = driver;
        }

        public void Write(string key, string value)
        {
            var js = _driver.AsJavascriptExecutor();
            js.ExecuteScript(
                $"window.sessionStorage.setItem('{key}','{value}');");
        }
    }
}