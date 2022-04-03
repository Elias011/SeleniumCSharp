using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Support.Extensions;
using SimpleInjector;
using CSharpSeleniumFramework.Engine.Mobile;

namespace CSharpSeleniumFramework.Engine
{
    public class TestScope : IDisposable
    {
        private readonly Container _container;
        private readonly Lazy<IWebDriver> _webDriver;

        private Lazy<WebAppInitializer> _WebApp { get; }

        public IWebDriver WebDriver => _webDriver.Value;
        public DateTime StartTime { get; }
        public WebAppInitializer WebApp => _WebApp.Value;
        public WebdriverDefinitions.RunType RunType { get; set; }
#nullable enable
        private readonly Lazy<AndroidDriver<AppiumWebElement>>? _androidDriver;
        public AndroidDriver<AppiumWebElement>? AndroidDriver => _androidDriver!.Value;
#nullable disable

        public TestScope(
    Container container,
    Lazy<IWebDriver> webdriver,
    Lazy<WebAppInitializer> webapp,
    Lazy<AndroidDriver<AppiumWebElement>> androidDriver)
        {
            _container = container;
            _webDriver = webdriver;
            _androidDriver = androidDriver;
            _WebApp = webapp;
            StartTime = DateTime.UtcNow;

            var culture = CultureInfo.CreateSpecificCulture("nl-NL");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }
        
        public void Dispose()
        {
            try
            {
                string IncidentCode = null;
                var mobileDriver = false;
                try
                {
                    var switched = MobileHelpers.SwitchContext(AndroidDriver, Mobile.AppContext.Contexts.Webview, withAssert: false, waitTime: 5);
                    if (switched)
                    {
                       // IncidentCode = SeleniumHelpers.GetIncidentIdIfModalExcists(AndroidDriver);
                    }

                    var attachment = $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss.fffff}";
                    if (TestRunSettings.AppiumScreenRecording)
                    {
                        try
                        {
                            var screenrecording = AndroidDriver.StopRecordingScreen();
                            var screenrecodingFile = Convert.FromBase64String(screenrecording);
                            File.WriteAllBytes(attachment + ".mp4", screenrecodingFile);
                            TestContext.AddTestAttachment(attachment + ".mp4");
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine("Screen Recording failed due to exception, {exception}", ex);
                        }
                    }

                    AndroidDriver.TakeScreenshot().SaveAsFile(attachment+ ".png");
                    TestContext.AddTestAttachment(attachment+".png");

                    AndroidDriver.Dispose();
                    mobileDriver = true;
                }
                catch (Exception)
                {
                    try
                    {

                        var mobileScreenshot = $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss.fffff}.png";
                        TestContext.AddTestAttachment(mobileScreenshot);
                        mobileDriver = true;
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }

                if (!mobileDriver)
                {
                    // Get Incident code if Modal is visible
                 //   IncidentCode = SeleniumHelpers.GetIncidentIdIfModalExcists(WebDriver);
                    // By default, take screenshot of last screen because exceptions can't be recognized properly yet.
                    // Todo, ensure that exceptions are better recognized and only take a screenshot on fail and/or exception
                    var screenshotPath = $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss.fffff}.png";
                    try
                    {
                        WebDriver.TakeScreenshot().SaveAsFile(screenshotPath);
                        TestContext.AddTestAttachment(screenshotPath);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Taking screenshot failed due to exception: {e}");
                    }
                }
                WebDriver?.Dispose();

                var timeDiff = DateTime.UtcNow - StartTime;
                Console.WriteLine("Real runtime after browser session startup: " +
                                  $"{timeDiff.Hours:00}:{timeDiff.Minutes:00}:{timeDiff.Seconds + timeDiff.Milliseconds / 1000.0:00}");

                //Fail test if Incident is present.
                if (IncidentCode != null) Assert.Fail(IncidentCode);
            }
            finally
            {
                Console.WriteLine($"Einde test om {DateTime.UtcNow} (UTC)");
                _container.Dispose();
            }
        }

        private static Container Container;

        public static Container InitializeContainer(WebdriverDefinitions.BrowserTypes type, WebdriverDefinitions.RunType runType)
        {
            Container = ContainerFactory.Instance.CreateContainer(() => WebdriverDefinitions.StartBrowser(type, runType));
            Container.Options.EnableAutoVerification = false;
            return Container;
        }

        public static TestScope CreateInstance(WebdriverDefinitions.BrowserTypes type, WebdriverDefinitions.RunType runType, [CallerMemberName] string callingMethod = "")
        {
            var testScope = Container.GetInstance<TestScope>();
            testScope.RunType = RunSettingCheck(runType);
            AssertWithScreenshot.SetDriver(testScope.WebDriver, type, callingMethod);
            return testScope;
        }

        public static TestScope CreateInstance(WebdriverDefinitions.BrowserTypes type, WebdriverDefinitions.RunType runType, AndroidAuthentication.GewensteFingerprintSetting requiredFingerprintStatus ,[CallerMemberName] string callingMethod = "")
        {
            var ondersteundeBrowserTypes = new List<WebdriverDefinitions.BrowserTypes>
            {
                WebdriverDefinitions.BrowserTypes.AndroidEmulatorChrome,
                WebdriverDefinitions.BrowserTypes.AndroidEmulatorApp
            };

            if (!ondersteundeBrowserTypes.Contains(type))
            {
                Assert.Fail($"Setting fingerprint is only for {0}", ondersteundeBrowserTypes.ToString());
            }
            var returnContainer = CreateInstance(type, runType, callingMethod);

            return returnContainer;
        }

        public static WebdriverDefinitions.RunType RunSettingCheck(WebdriverDefinitions.RunType runType)
        {
            var returnval = runType;
#if (!DEBUG)
                returnval = WebdriverDefinitions.RunType.ClusterRun;
#endif

            if (TestRunSettings.ForceLocalRun)
            {
                returnval = WebdriverDefinitions.RunType.LocalRun;
            }

            return returnval;

        }
    }
}
