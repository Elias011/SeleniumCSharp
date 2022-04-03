using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;

namespace CSharpSeleniumFramework.Engine.Mobile
{
    public class AndroidHelpers
    {
        private AndroidDriver<AppiumWebElement> _driver;

        public AndroidHelpers(AndroidDriver<AppiumWebElement> driver)
        {
            _driver = driver;
        }

    }
}
