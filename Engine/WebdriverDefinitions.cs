using System;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Enums;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Chromium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Safari;
using Assert = CSharpSeleniumFramework.Engine.AssertWithScreenshot;

namespace CSharpSeleniumFramework.Engine
{
    public static class WebdriverDefinitions
    {
        public enum BrowserTypes
        {
            ChromeDesktop,
            ChromeNexus5,
            FirefoxDesktop,
            InternetExplorerDesktop,
            ChromeiPhoneXSimulatie,
            ChromeSamsungS10Simulatie,
            AndroidEmulatorChrome,
            AndroidEmulatorApp,
            IosEmulatorApp,
            IosEmulatorAppNoFaceID,
            SafariMac
        }

        public enum RunType
        {
            LocalRun,
            ClusterRun
        }

        public static IWebDriver StartBrowser(BrowserTypes browser = BrowserTypes.ChromeDesktop,
            RunType runtype = RunType.ClusterRun)
        {
            // Always run via Cluster with Release builds
#if (!DEBUG)
            runtype = RunType.ClusterRun;
#endif
#if (DEBUG)
            if (TestRunSettings.ForceLocalRun && TestRunSettings.ForceCluster)
            {
                throw new ArgumentException("Cannot run ForceLocalRun and ForceCluster at the same time.");
            }

            if (TestRunSettings.ForceLocalRun)
            {
                Console.WriteLine($"Force Run Type to {RunType.LocalRun.ToString()}");
                runtype = RunType.LocalRun;
            }
            if (TestRunSettings.ForceCluster)
            {
                Console.WriteLine($"Force Run Type to {RunType.ClusterRun.ToString()}");
                runtype = RunType.ClusterRun;
            }

            if (TestRunSettings.ForceBrowser != string.Empty)
            {
                Console.WriteLine($"Force Run Type to {browser}");
                BrowserTypes.TryParse(TestRunSettings.ForceBrowser, out browser);
            }
#endif
            Console.WriteLine($"Launching web browser {DateTime.UtcNow} (UTC)");
            try
            {
                IWebDriver webDriver;
                switch (browser)
                {
                    case BrowserTypes.ChromeDesktop:
                    {
                        webDriver = ChromeDesktop(runtype);
                        break;
                    }
                    case BrowserTypes.ChromeNexus5:
                    {
                        webDriver = ChromeNexus5(runtype);
                        break;
                    }
                    case BrowserTypes.FirefoxDesktop:
                    {
                        webDriver = FireFoxDesktop(runtype);
                        break;
                    }
                    case BrowserTypes.InternetExplorerDesktop:
                    {
                        webDriver = InternetExplorerDesktop(runtype);
                        break;
                    }
                    case BrowserTypes.ChromeSamsungS10Simulatie:
                    {
                        webDriver = ChromeSamsungS10(runtype);
                        break;
                    }
                    case BrowserTypes.ChromeiPhoneXSimulatie:
                    {
                        webDriver = ChromeiPhoneX(runtype);
                        break;
                    }
                    case BrowserTypes.SafariMac:
                    {
                        webDriver = SafariMac(runtype);
                        break;
                        }
                    default:
                    {
                        webDriver = null;
                        Assert.NotNull(webDriver,
                            "No web driver has been started, the selected browser was not recognized.");
                        break;
                    }
                }

                Assert.NotNull(webDriver, "No web driver has been started.");
                webDriver.Manage().Timeouts().AsynchronousJavaScript.Add(TimeSpan.FromSeconds(5));
                Console.WriteLine($"Browser started at {DateTime.UtcNow} (UTC)");
                return webDriver;
            }
            catch (WebDriverException ex)
            {
                Console.WriteLine(ex);
                Assert.Fail("Webdriver could not be started due to exception.");
                return null;
            }
        }

        private static IWebDriver SafariMac(RunType runtype)
        {
            var safariCapabilities = new SafariOptions();
            switch (runtype)
            {
                case RunType.LocalRun:
                {
                    throw new NotImplementedException();
                }
                case RunType.ClusterRun:
                {
                    var driver = new RemoteWebDriver(
                        TestRunSettings.SeleniumCluster,
                        safariCapabilities.ToCapabilities(),
                        TimeSpan.FromMinutes(3)
                    );
                    driver.Manage().Window.Maximize();
                    return driver;
                }
                default:
                {
                    Assert.Fail("Specified run type is not supported for this browser type.");
                    return null;
                }
            }
        }

        private static IWebDriver ChromeDesktop(RunType runtype = RunType.ClusterRun)
        {
            var chromeCapabilities = new ChromeOptions();
            chromeCapabilities.AddArgument("start-maximized");
            chromeCapabilities.AddArgument("no-sandbox");

            return StartChrome(runtype, chromeCapabilities);
        }

        private static IWebDriver ChromeNexus5(RunType runtype = RunType.ClusterRun)
        {
            var chromeCapabilities = new ChromeOptions();
            chromeCapabilities.EnableMobileEmulation("Nexus 5");
            /*var nexus5 = new ChromeMobileEmulationDeviceSettings
            {
                Height = 640,
                Width = 360,
                PixelRatio = 1,
                UserAgent = "Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/93.0.4577.63 Mobile Safari/537.36"
            };*/
            //chromeCapabilities.EnableMobileEmulation(nexus5);
            chromeCapabilities.AddArgument("no-sandbox");

            return StartChrome(runtype, chromeCapabilities);
        }

        private static IWebDriver ChromeiPhoneX(RunType runtype = RunType.ClusterRun)
        {
            var chromeCapabilities = new ChromeOptions();
            chromeCapabilities.EnableMobileEmulation("iPhone X");
            chromeCapabilities.AddArgument("no-sandbox");

            return StartChrome(runtype, chromeCapabilities);
        }

       private static IWebDriver ChromeSamsungS10(RunType runtype = RunType.ClusterRun)
        {
            var chromeCapabilities = new ChromeOptions();
            var samsungS10 = new ChromiumMobileEmulationDeviceSettings
            {
                Height = 740,
                Width = 360,
                PixelRatio = 4,
                UserAgent = "mozilla/5.0 (linux; android 5.0; samsung-s10 build/jdq39) applewebkit/537.36 (khtml, like gecko) chrome/56.0.2924.87 safari/537.36"
            };
            chromeCapabilities.EnableMobileEmulation(samsungS10);
            chromeCapabilities.AddArgument("no-sandbox");

            return StartChrome(runtype, chromeCapabilities);
        }

        public static AndroidDriver<AppiumWebElement> StartAndroid(BrowserTypes browser = BrowserTypes.AndroidEmulatorChrome, RunType runtype = RunType.ClusterRun, bool retry = false)
        {
            // Bij Release builds altijd via Cluster runnen
#if (!DEBUG)
            runtype = RunType.ClusterRun;
#endif
            Console.WriteLine($"Launching web browser {DateTime.UtcNow} (UTC)");
            try
            {
                AndroidDriver<AppiumWebElement> webDriver;
                switch (browser)
                {
                    case BrowserTypes.AndroidEmulatorApp:
                    {
                        webDriver = AndroidEmulatorApp(runtype, retry);
                        break;
                    }
                    case BrowserTypes.AndroidEmulatorChrome:
                    {
                        webDriver = AndroidEmulatorChrome(runtype);
                        break;
                    }
                    default:
                    {
                        webDriver = null;
                        Assert.NotNull(webDriver,
                            "No web driver has been started, the selected browser was not recognized.");
                        break;
                    }
                }

                Assert.NotNull(webDriver, "No web driver has been started.");
                Console.WriteLine($"Browser started at{DateTime.UtcNow} (UTC)");
                return webDriver;
            }
            catch (WebDriverException ex)
            {
                Console.WriteLine("Webdriver could not be started due to exception.");
                Console.WriteLine(ex);

                if (retry)
                {
                    
                    Assert.Fail("Webdriver could not be started due to exception.");
                }

                Console.WriteLine("Second attempt to start web driver.");
                return StartAndroid(browser, runtype, retry: true);
            }
        }


        private static AndroidDriver<AppiumWebElement> AndroidEmulatorChrome(RunType runtype = RunType.ClusterRun)
        {
            var androidAppiumOptions = new AppiumOptions();
            androidAppiumOptions.AddAdditionalCapability(MobileCapabilityType.BrowserName, "Chrome");
            androidAppiumOptions.AddAdditionalCapability(MobileCapabilityType.PlatformName, "Android");

            return new AndroidDriver<AppiumWebElement>(TestRunSettings.SeleniumCluster, androidAppiumOptions, TimeSpan.FromSeconds(300));
        }

        private static AndroidDriver<AppiumWebElement> AndroidEmulatorApp(RunType runtype = RunType.ClusterRun, bool retry = false)
        {
            Uri clusterUrl;
            if (runtype == RunType.ClusterRun)
            {
                clusterUrl = TestRunSettings.SeleniumCluster;
            }
            else
            {
                clusterUrl = TestRunSettings.AppiumLokaalDev;
            }

            AndroidDriver<AppiumWebElement> driver;
            var androidAppiumOptions = new AppiumOptions();
            androidAppiumOptions.AddAdditionalCapability(MobileCapabilityType.PlatformName, "Android");
            androidAppiumOptions.AddAdditionalCapability(MobileCapabilityType.App, TestRunSettings.AndroidAppLocation);
            androidAppiumOptions.AddAdditionalCapability("avd", "Samsung_S10");
            androidAppiumOptions.AddAdditionalCapability("disableWindowAnimation", false);
            androidAppiumOptions.AddAdditionalCapability("unlockType", "pin");
            androidAppiumOptions.AddAdditionalCapability("unlockKey", "0000");
            androidAppiumOptions.AddAdditionalCapability("nativeWebScreenshot", true);
            androidAppiumOptions.AddAdditionalCapability("noReset", true);
            androidAppiumOptions.AddAdditionalCapability("newCommandTimeout", 60);
            if (retry)
            {
                // Start driver with fullreset enabled, discard result and reset the reset variables
                androidAppiumOptions.AddAdditionalCapability("noReset", false);
                androidAppiumOptions.AddAdditionalCapability("fullReset", true);
                driver = new AndroidDriver<AppiumWebElement>(clusterUrl, androidAppiumOptions, TimeSpan.FromSeconds(300)); 
                driver.Dispose();
                androidAppiumOptions.AddAdditionalCapability("noReset", true);
                androidAppiumOptions.AddAdditionalCapability("fullReset", false);
            }

            driver = new AndroidDriver<AppiumWebElement>(clusterUrl, androidAppiumOptions, TimeSpan.FromSeconds(300));
            driver.RemoveApp(TestRunSettings.AndroidAppIdentifier);
            driver.InstallApp(TestRunSettings.AndroidAppLocation);
            if (TestRunSettings.AppiumScreenRecording)
            {
                driver.StartRecordingScreen(
                    AndroidStartScreenRecordingOptions.GetAndroidStartScreenRecordingOptions()
                                                      .WithTimeLimit(TimeSpan.FromSeconds(240))
                                                      .WithBitRate(4000000)
                                                      .WithVideoSize("720x1280")
                );
            }

            driver.LaunchApp();
            return driver;
        }
        


        private static IWebDriver FireFoxDesktop(RunType runtype = RunType.ClusterRun)
        {
            IWebDriver driver;
            var firefoxCapabilities = new FirefoxOptions();

            switch (runtype)
            {
                case RunType.LocalRun:
                {
                    var service = FirefoxDriverService.CreateDefaultService();
                    service.Host = "::1";
                    driver = new FirefoxDriver(service, firefoxCapabilities);
                    break;
                }
                case RunType.ClusterRun:
                {
                    driver = new RemoteWebDriver(TestRunSettings.SeleniumCluster, firefoxCapabilities.ToCapabilities(),
                        TimeSpan.FromMinutes(3));
                    break;
                }
                default:
                {
                    Assert.Fail("Specified run type is not supported for this browser type.");
                    return null;
                }
            }

            driver.Manage().Window.Maximize();
            return driver;
        }

        private static IWebDriver InternetExplorerDesktop(RunType runtype = RunType.ClusterRun)
        {
            IWebDriver driver;
            var ieOptions = new InternetExplorerOptions
            {
                IgnoreZoomLevel = true,
                IntroduceInstabilityByIgnoringProtectedModeSettings = true,
                RequireWindowFocus = true
            };

            switch (runtype)
            {
                case RunType.LocalRun:
                {
                    driver = new InternetExplorerDriver(ieOptions);
                    break;
                }
                case RunType.ClusterRun:
                {
                    driver = new RemoteWebDriver(TestRunSettings.SeleniumCluster, ieOptions.ToCapabilities(),
                        TimeSpan.FromMinutes(3));
                    break;
                }
                default:
                {
                    Assert.Fail("Specified run type is not supported for this browser type.");
                    return null;
                }
            }

            driver.Manage().Window.Maximize();
            return driver;
        }

        private static IWebDriver StartChrome(RunType runtype, ChromeOptions chromeCapabilities)
        {
            chromeCapabilities.Proxy = null;
            switch (runtype)
            {
                case RunType.LocalRun:
                {
                    return new ChromeDriver(chromeCapabilities);
                }
                case RunType.ClusterRun:
                {
                    var driver = new RemoteWebDriver(
                        TestRunSettings.SeleniumCluster,
                        chromeCapabilities.ToCapabilities(),
                        TimeSpan.FromMinutes(3)
                    );
                    new RemoteWebdriverSessionInfo().WriteRemoteSessionInfo(driver);
                    return driver;
                }
                default:
                {
                    Assert.Fail("Specified run type is not supported for this browser type.");
                    return null;
                }
            }
        }
    }
}