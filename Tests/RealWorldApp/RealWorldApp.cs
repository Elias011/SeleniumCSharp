using NUnit.Framework;
using CSharpSeleniumFramework.Engine;
using Assert = CSharpSeleniumFramework.Engine.AssertWithScreenshot;
using static CSharpSeleniumFramework.Engine.WebdriverDefinitions;
using static CSharpSeleniumFramework.Engine.SeleniumHelpers;

namespace CSharpSeleniumFramework.Tests.RealWorldApp
{
    /// <summary>
    /// Go to real world app
    /// Open contact form
    /// Fill the contact form
    /// Sen the message
    /// </summary>
    [TestFixture(BrowserTypes.ChromeDesktop, WebdriverDefinitions.RunType.LocalRun)]
    public class RealWorldApp
    {
        private readonly BrowserTypes _browser;
        private readonly RunType _runtype;

        public RealWorldApp(BrowserTypes browser, RunType runType)
        {
            _browser = browser;
            _runtype = runType;
            var container = TestScope.InitializeContainer(_browser, _runtype);
        }

        [Test]
        public void RealworldAppTest()
        {
            using var scope = TestScope.CreateInstance(_browser, _runtype);
            var webDriver = scope.WebDriver;
            var webApp = scope.WebApp;

            OpenRealWorldApp(webDriver);

            webApp.GeneralElements.Contact.Click();
            Assert.IsTrue(webApp.ContactElements.ContactModal.WaitUntil(SeleniumExtensions.WaitUntilTypes.Visible).Exists(), "Expect: the contact form is exist");
            webApp.ContactElements.ContactModalEmailField.SendKeysByCharacter("Selenium@test.com");
            webApp.ContactElements.ContactModalContactNameField.SendKeysByCharacter("Selenium");
            webApp.ContactElements.ContactModalMessageField.SendKeysByCharacter("Selenium using C#");
            webApp.ContactElements.ContactModalSendMessageButton.Click();
            scope.WebDriver.WaitForJavascriptAlert().Accept();
        }
    }
}
