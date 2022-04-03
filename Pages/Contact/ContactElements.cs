using OpenQA.Selenium;
using CSharpSeleniumFramework.Engine;

namespace CSharpSeleniumFramework.Pages.Contact
{
    public class ContactElements
    {
        private readonly IWebDriver _driver;

        public ContactElements(IWebDriver driver)
        {
            _driver = driver;
        }

        public IWebElement ContactModal => _driver.FindElementSafe(By.Id("exampleModal"));
        public IWebElement ContactModalEmailField => _driver.FindElementSafe(By.Id("recipient-email"));
        public IWebElement ContactModalContactNameField => _driver.FindElementSafe(By.Id("recipient-name"));
        public IWebElement ContactModalMessageField => _driver.FindElementSafe(By.Id("message-text"));
        public IWebElement ContactModalSendMessageButton => _driver.FindElementSafe(By.ClassName("btn-primary"));
    }
}