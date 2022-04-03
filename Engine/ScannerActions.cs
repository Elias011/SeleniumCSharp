using System;
using System.IO;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.Extensions;
using CSharpSeleniumFramework.Pages.algemeen;
using WDSE;
using WDSE.Decorators;
using WDSE.ScreenshotMaker;

namespace CSharpSeleniumFramework.Engine
{
    public class ScannerActions
    {
        private readonly IWebDriver _driver;
        private string screenshotsPath;
        public ScannerActions(IWebDriver driver)
        {
            _driver = driver; 
            screenshotsPath = "./Screenshots"; // the place where you want to store screenshots
        }

        public void Scanner(string directoryName, string browserType, string scannerType)
        {
            try
            { 
                var algemeenElements = new GeneralElements(_driver);
                int iterationNumber = 0;
                string[] mobileView = { "ChromeNexus5", "AndroidEmulatorChrome", "ChromeiPhoneXSimulatie", "ChromeSamsungS10Simulatie", "AndroidEmulatorApp", "IosEmulatorApp", "IosEmulatorAppNoFaceID"};
                string subdir = @$"{screenshotsPath}/{scannerType}/{browserType}/{directoryName}";
                // If directory is not exist, create it. 
                if (!Directory.Exists(subdir))
                {
                    Directory.CreateDirectory(subdir);
                }
                else
                {
                    iterationNumber = Directory.GetFiles(subdir, "*.png", SearchOption.TopDirectoryOnly).Length;
                }

                if (mobileView.Contains(browserType))
                {
                    _driver.TakeScreenshot().SaveAsFile($"{screenshotsPath}/{scannerType}/{browserType}/{directoryName}/{browserType}-{directoryName}{iterationNumber}.png");
                }
                else
                {
                    VerticalCombineDecorator vcs = new VerticalCombineDecorator(new ScreenshotMaker());
                    var content = _driver.TakeScreenshot(vcs).ToMagickImage().ToBase64();
                    var bytes = Convert.FromBase64String(content);
                    using (var imageFile = new FileStream($"{screenshotsPath}/{scannerType}/{browserType}/{directoryName}/{browserType}-{directoryName}{iterationNumber}.png", FileMode.Create))
                    {
                        imageFile.Write(bytes, 0, bytes.Length);
                        imageFile.Flush();
                    }
                }
            }
            catch (Exception exception)
            {
                if (_driver != null)
                { 
                    Console.WriteLine("Web driver is closed by scanner due to null response");
                }
                else
                {
                    Console.WriteLine(exception);
                }
            }
        }

        public void RemoveDirectory(string directoryName, string browserType, string scannerType)
        {
            string subdir = @$"{screenshotsPath}/{scannerType}/{browserType}/{directoryName}";
            if (Directory.Exists(subdir))
            {
                Directory.Delete(subdir, true);
            }

        }
    }
}
