using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Appium.MultiTouch;
using OpenQA.Selenium.Support.UI;
using Assert = CSharpSeleniumFramework.Engine.AssertWithScreenshot;

namespace CSharpSeleniumFramework.Engine.Mobile
{
    public static class MobileHelpers
    {
        public static AppiumWebElement WaitUntilVisible(this AndroidDriver<AppiumWebElement> driver, By by, int waitTime = -1)
        {
            return WaitUntilVisibleExecutable(driver, by, waitTime);
        }

        public static AppiumWebElement WaitUntilVisible(this IOSDriver<AppiumWebElement> driver, By by, int waitTime = -1)
        {
            return WaitUntilVisibleExecutable(driver, by, waitTime);
        }
        public static bool SwitchContext(this IOSDriver<AppiumWebElement> driver, AppContext.Contexts context,string AssertMelding = "", bool withAssert = true, int waitTime = -1 )
        {
            var switched = false;
            if (waitTime == -1)
            {
                waitTime = TestRunSettings.DefaultWaitTime;
            }


            switch (context)
            {
                case AppContext.Contexts.Native:
                    switched = CheckAndSwitchContext(driver, "NATIVE_APP", withAssert, waitTime, AssertMelding);
                    break;
                case AppContext.Contexts.Webview:
                    var sw = new Stopwatch();
                    sw.Start();
                    var webviewContext = "";
                    while (webviewContext == "" && sw.Elapsed.Seconds < waitTime)
                    {
                        try
                        {
                            webviewContext = (from c in driver.Contexts where c.StartsWith("WEBVIEW_") select c).First();
                        }
                        catch
                        {
                            Thread.Sleep(100);
                        }
                    }

                    if (withAssert)
                    {
                        Assert.IsNotEmpty(webviewContext,
                            $"Expect webview available. Available contexts were: {driver.Contexts.ToString()}");
                    }
                    switched = CheckAndSwitchContext(driver, webviewContext, withAssert, -1, AssertMelding);
                    break;
            }
            return switched;
        }

        public static bool SwitchContext(this AndroidDriver<AppiumWebElement> driver, AppContext.Contexts context, string AssertMelding = "", bool withAssert = true, int waitTime = -1)
        {
            Thread.Sleep(2000);
            var switched = false;
            switch (context)
            {
                case AppContext.Contexts.Native:
                    switched = CheckAndSwitchContext(driver, "NATIVE_APP", withAssert, waitTime, AssertMelding);
                    break;
                case AppContext.Contexts.Webview:
                    switched = CheckAndSwitchContext(driver, "WEBVIEW_" + TestRunSettings.AndroidAppIdentifier, withAssert, waitTime, AssertMelding);
                    break;
                case AppContext.Contexts.InApp_Chrome:
                    switched = CheckAndSwitchContext(driver, "WEBVIEW_chrome", withAssert, waitTime, AssertMelding);
                    break;
            }
            return switched;
        }

        public static AppiumWebElement WaitUntilContainsText(this AndroidDriver<AppiumWebElement> driver, By by, string verwachteText, bool exacteMatch, int waitTime = -1)
        {
            return WaitUntilContainsTextExecutable(
                driver,
                by,
                verwachteText,
                exacteMatch,
                waitTime
            );
        }

        public static AppiumWebElement WaitUntilContainsText(this IOSDriver<AppiumWebElement> driver, By by, string verwachteText, bool exacteMatch, int waitTime = -1)
        {
            return WaitUntilContainsTextExecutable(
                driver,
                by,
                verwachteText,
                exacteMatch,
                waitTime
            );
        }

        public static void ClickWhenVisibleElseFail(this AppiumWebElement element, dynamic driver, string assertMessage, bool longPress = false)
        {
            var sw = new Stopwatch();
            sw.Start();
            while (element == null && sw.Elapsed.Seconds < TestRunSettings.DefaultWaitTime)
            {
                Thread.Sleep(200);
            }
            Assert.IsNotNull(element, assertMessage);

            if (longPress)
            {
                var touchAction = new TouchAction(driver);
                touchAction.Press(element).Wait(3000).Release().Perform();
            }
            else
            {
                element.Click();
            }
        }


        private static AppiumWebElement WaitUntilVisibleExecutable(dynamic driver, By by, int waitTime = -1)
        {
            if (waitTime == -1)
            {
                waitTime = TestRunSettings.DefaultWaitTime;
            }
            try
            {
                try
                {
                    var wait = new WebDriverWait(driver, new TimeSpan(0, 0, waitTime));
                    wait.Until(webDriver =>
                    {
                        try
                        {
                            var findElement = webDriver.FindElement(by);
                            // If the element is in the correct state within the wait Time return the element, otherwise null
                            return findElement.Displayed ? findElement : null;
                        }
                        catch (StaleElementReferenceException)
                        {
                            return null;
                        }
                        catch (NoSuchElementException)
                        {
                            return null;
                        }
                    });
                    return driver.FindElement(by);
                }
                catch (WebDriverTimeoutException)
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static AppiumWebElement WaitUntilContainsTextExecutable(dynamic driver, By by, string verwachteText, bool exacteMatch ,int waitTime = -1)
        {
            if (waitTime == -1)
            {
                waitTime = TestRunSettings.DefaultWaitTime;
            }
            try
            {
                try
                {
                    var wait = new WebDriverWait(driver, new TimeSpan(0, 0, waitTime));
                    var foundElement = wait.Until(FindElementWithText);

                    return foundElement != null ? FindElementWithText(driver) : null;
                }
                catch (WebDriverTimeoutException)
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }

            AppiumWebElement FindElementWithText(dynamic webDriver)
            {
                try
                {
                    var findElements = webDriver.FindElements(by);
                    // Go through each element found and return the first one that matches expected text.
                    foreach (var findElement in findElements)
                    {
                        // If the element contains the correct text within the wait Time return the element, otherwise null
                        if (exacteMatch && findElement.Text == verwachteText)
                        {
                            return findElement;
                        }

                        if (!exacteMatch && findElement.Text.Contains(verwachteText))
                        {
                            return findElement;
                        }
                    }
                    return null;
                }
                catch (StaleElementReferenceException)
                {
                    return null;
                }
                catch (NoSuchElementException)
                {
                    return null;
                }
            }
        }
        
        private static bool CheckAndSwitchContext(dynamic driver, string context, bool withAssert = true, int waitTime = -1, string AssertMelding = "")
        {
            if (waitTime == -1)
            {
                waitTime = TestRunSettings.DefaultWaitTime;
            }

            var sw = new Stopwatch();
            sw.Start();
            while (sw.Elapsed.Seconds <= waitTime && !driver.Contexts.Contains(context))
            {
                Thread.Sleep(100);
            }

            if (driver.Contexts.Contains(context))
            {
                driver.Context = context;
            }


            if (withAssert)
            {
                if (AssertMelding != "")
                {
                    AssertMelding = AssertMelding + " - ";
                }
                Assert.AreEqual(
                    context, driver.Context,
                    $"{AssertMelding}Context switch naar {context.ToString()} is niet gelukt binnen {TestRunSettings.DefaultWaitTime} seconden."
                );
            }

            return driver.Context == context;

        }

    }
}
