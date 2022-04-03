using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.iOS;

namespace CSharpSeleniumFramework.Engine.Mobile
{
    public class IOSHelpers
    {
        private IOSDriver<AppiumWebElement> _driver;

        public IOSHelpers(IOSDriver<AppiumWebElement> driver)
        {
            _driver = driver;
        }

    }
}
