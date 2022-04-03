using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;

namespace CSharpSeleniumFramework.Engine.Mobile
{
    public static class AndroidAuthentication
    {
        private static string[] _acceptedScreens = new string[]
        {
            ".password.ConfirmLockPassword",
            ".password.ConfirmDeviceCredentialActivity",
            ".password.ConfirmLockPassword$InternalActivity"
        };

        public static void ValidFingerprint(this AndroidDriver<AppiumWebElement> driver)
        {
            driver.FingerPrint(1);
        }

        public static void InvalidFingerprint(this AndroidDriver<AppiumWebElement> driver)
        {
            driver.FingerPrint(9);
        }

        public static void ValidPinCode(this AndroidDriver<AppiumWebElement> androidDriver)
        {
            Assert.IsTrue(androidDriver.AreWeOnPincodeScreen(), "Expect to be on PIN code screen");
            androidDriver.PressKeyCode(AndroidKeyCode.KeycodeNumpad_0);
            androidDriver.PressKeyCode(AndroidKeyCode.KeycodeNumpad_0);
            androidDriver.PressKeyCode(AndroidKeyCode.KeycodeNumpad_0);
            androidDriver.PressKeyCode(AndroidKeyCode.KeycodeNumpad_0);
            androidDriver.PressKeyCode(AndroidKeyCode.Keycode_ENTER);
            Assert.IsTrue(androidDriver.WaitUntilWeLeftPincodeScreen(), "Expect to have exited PIN code screen");
        }

        public static void InvalidPinCode(this AndroidDriver<AppiumWebElement> androidDriver)
        {
            Assert.IsTrue(androidDriver.AreWeOnPincodeScreen(), "Expect to be on PIN code screen");
            androidDriver.PressKeyCode(AndroidKeyCode.KeycodeNumpad_1);
            androidDriver.PressKeyCode(AndroidKeyCode.Keycode_ENTER);
        }

        public static bool AreWeOnPincodeScreen(this AndroidDriver<AppiumWebElement> androidDriver, int maxWait = -1)
        {
            if (maxWait == -1)
            {
                maxWait = TestRunSettings.DefaultWaitTime;
            }

            var sw = new Stopwatch();
            sw.Start();
            while (!_acceptedScreens.Contains(androidDriver.CurrentActivity) && sw.Elapsed.Seconds < maxWait)
            {
                Console.WriteLine(androidDriver.CurrentActivity);
            }
            return _acceptedScreens.Contains(androidDriver.CurrentActivity);
        }

        private static bool WaitUntilWeLeftPincodeScreen(this AndroidDriver<AppiumWebElement> androidDriver, int maxWait = -1)
        {
            if (maxWait == -1)
            {
                maxWait = TestRunSettings.DefaultWaitTime;
            }

            var sw = new Stopwatch();
            sw.Start();
            while (_acceptedScreens.Contains(androidDriver.CurrentActivity) && sw.Elapsed.Seconds < maxWait)
            {
                // do nothing
            }
            return !_acceptedScreens.Contains(androidDriver.CurrentActivity);
        }

        public static void zetFingerprintOpJuisteSetting(this AndroidDriver<AppiumWebElement> androidDriver, GewensteFingerprintSetting fingerprintSetting)
        {
            openAndroidSettings();
            waitUntilVisible(By.Id("com.android.settings:id/search_action_bar")).Click();
            waitUntilVisible(By.Id("android:id/search_src_text")).SendKeys("Fingerprint");
            Thread.Sleep(1000);
            findItemAndClick(By.Id("android:id/title"), "Fingerprint");

            waitUntilVisible(By.Id("com.android.settings:id/action_bar"));
            findItemAndClick(By.Id("android:id/title"), "Fingerprint");

            androidDriver.ValidPinCode();

            Console.WriteLine("Desired situation is fingerprint enabled {0}", fingerprintSetting.ToString());
            if (fingerprintSetting == GewensteFingerprintSetting.enabled)
            {
                if (findItemInList(
                    By.Id("com.android.settings:id/suc_layout_title"),
                    "Unlock with fingerprint",
                    withAssert: false,
                    waitTime: 2
                ) != null)
                {
                    Console.WriteLine("No Fingerprint has been set yet, set it now.");
                    setupFingerprint();
                    Console.WriteLine("Fingerprint set.");
                }
                else
                {
                    Console.WriteLine("A Fingerprint has already been set, do not set anything.");
                }
            }
            else
            {
                if (findItemInList(
                    By.Id("com.android.settings:id/suc_layout_title"),
                    "Unlock with fingerprint",
                    withAssert: false,
                    waitTime: 2
                ) == null)
                {
                    Console.WriteLine("Another Fingerprint has been set, delete now.");
                    verwijderFingerprint();
                    Console.WriteLine("Fingerprint removed.");
                }
                else
                {
                    Console.WriteLine("No Fingerprint has been set, nothing needs to be deleted.");
                }
            }

            androidDriver.ActivateApp(TestRunSettings.AndroidAppIdentifier);

            void findItemAndClick(By by, string item)
            {
                AppiumWebElement foundSearchResult = findItemInList(by, item);
                foundSearchResult.Click();
            }
            AppiumWebElement findItemInList(
                By by,
                string item,
                bool withAssert = true,
                int waitTime = -1
            )
            {
                if (item == null)
                    throw new ArgumentNullException(nameof(item));
                if (waitTime == -1)
                {
                    waitTime = TestRunSettings.DefaultWaitTime;
                }

                AppiumWebElement foundSearchResult;
                var sw = new Stopwatch();
                sw.Restart();
                do
                {
                    try
                    {
                        var searchResults = androidDriver.FindElements(by);
                        foundSearchResult = searchResults.FirstOrDefault(e => e.Text == item);
                    }
                    catch
                    {
                        foundSearchResult = null;
                    }
                }
                while (foundSearchResult == null && sw.Elapsed.Seconds < waitTime);

                sw.Stop();
                if (withAssert)
                {
                    Assert.IsNotNull(
                        foundSearchResult,
                        "Expected element with text {0} to be found in list of items searched based on {1}",
                        item,
                        by.ToString()
                    );
                }

                return foundSearchResult;
            }
            AppiumWebElement waitUntilVisible(By by, bool withAssert = true, int waitTime = -1)
            {
                var sw = new Stopwatch();
                sw.Start();
                AppiumWebElement element = null;
                if (waitTime == -1)
                {
                    waitTime = TestRunSettings.DefaultWaitTime;
                }

                do
                {
                    try
                    {
                        element = androidDriver.FindElement(by);
                    }
                    catch
                    {
                        element = null;
                    }
                }
                while (element == null && sw.Elapsed.Seconds < waitTime);

                if (withAssert)
                {
                    Assert.IsNotNull(element, "Expect to find element based on {0}", by.ToString());
                }

                return element;
            }
            void setupFingerprint()
            {
                findItemInList(By.Id("com.android.settings:id/suc_layout_title"), "Unlock with fingerprint");
                findItemAndClick(By.ClassName("android.widget.Button"), "NEXT");

                findItemInList(By.Id("com.android.settings:id/suc_layout_title"), "Touch the sensor");
                do
                {
                    androidDriver.ValidFingerprint();
                }
                while (findItemInList(
                    By.Id("com.android.settings:id/suc_layout_title"),
                    "Lift, then touch again",
                    withAssert: false,
                    waitTime: 1
                ) != null);

                findItemInList(By.Id("com.android.settings:id/suc_layout_title"), "Fingerprint added");
                findItemAndClick(By.ClassName("android.widget.Button"), "DONE");
            }
            void verwijderFingerprint()
            {
                do
                {
                    Console.WriteLine("Fingerprint removed");
                    waitUntilVisible(By.Id("com.android.settings:id/delete_button")).Click();
                    waitUntilVisible(By.Id("com.android.settings:id/alertTitle"));
                    androidDriver.FindElement(By.Id("android:id/button1")).Click();
                }
                while (waitUntilVisible(
                    By.Id("com.android.settings:id/delete_button"),
                    withAssert: false,
                    waitTime: 1
                ) != null);
            }
            void openAndroidSettings(int tries = 0)
            {
                try
                {
                    androidDriver.StartActivity(
                        "com.android.settings",
                        "com.android.settings.Settings",
                        "com.android.settings",
                        "com.android.settings.Settings"
                    );
                    Thread.Sleep(tries * 100 + 100);
                    androidDriver.FindElement(By.Id("com.android.settings:id/search_action_bar"));
                }
                catch
                {
                    if (tries < 5)
                    {
                        openAndroidSettings(tries++);
                    }
                    else
                    {
                        Assert.Fail("Failed to open settings");
                    }
                }
            }
        }

        public enum GewensteFingerprintSetting
        {
            enabled,
            disabled
        }
    }
}
