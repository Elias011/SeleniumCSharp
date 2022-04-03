using System.Runtime.CompilerServices;
using NUnit.Framework;
using OpenQA.Selenium;

namespace CSharpSeleniumFramework.Engine
{
    /// <summary>
    /// Assert with screenshot makes a screenshot when the for the entire page after the assertion get succeeded
    /// </summary>
    public class AssertWithScreenshot : Assert
    {
        private static ScannerActions _scannerActions;
        private static string _scannerType;
        private static IWebDriver _driver;
        private static bool _scanner;
        private static string _browserType;

        public static void SetDriver(IWebDriver webDriver, WebdriverDefinitions.BrowserTypes browserType, string callingMethod)
        {
            _driver = webDriver;
            _scannerActions = new ScannerActions(_driver);
            _browserType = browserType.ToString();
            _scannerType = TestRunSettings.ScannerType;
            _scannerActions.RemoveDirectory(callingMethod, _browserType, _scannerType);
            _scanner = true;
        }

        public static void IsTrue(bool statement, string foutmeldingVerwachtResultaat = "The validation was not successful", [CallerMemberName] string callingMethod = "")
        {
            Assert.IsTrue(statement, foutmeldingVerwachtResultaat);
            if (TestRunSettings.ScannerEnabled == false || TestRunSettings.ScannerType == "No Scanner Type Set")
            {
                _scanner = false;
            }
            if (_scanner)
            {
                _scannerActions = new ScannerActions(_driver);
                _scannerActions.Scanner($"{callingMethod}", _browserType, _scannerType);
            }
        }

        public static void True(bool statement, string errorMessageExpectedResult = "The validation was not successful", [CallerMemberName] string callingMethod = "")
        {
            Assert.True(statement, errorMessageExpectedResult);
            if (TestRunSettings.ScannerEnabled == false || TestRunSettings.ScannerType == "NoneScannerTypeSet")
            {
                _scanner = false;
            }
            if (_scanner)
            {
                _scannerActions = new ScannerActions(_driver);
                _scannerActions.Scanner($"{callingMethod}", _browserType, _scannerType);
            }
        }
        public static void AreEqual(string verwachteTekst, string teVergelijken,
            string foutmeldingVerwachtResultaat = "The validation was not successful", [CallerMemberName] string callingMethod = "")
        {
            Assert.AreEqual(verwachteTekst, teVergelijken, foutmeldingVerwachtResultaat);
            if (TestRunSettings.ScannerEnabled == false || TestRunSettings.ScannerType == "NoneScannerTypeSet")
            {
                _scanner = false;
            }
            if (_scanner)
            {
                _scannerActions = new ScannerActions(_driver);
                _scannerActions.Scanner($"{callingMethod}", _browserType, _scannerType);
            }
        }

        public static void AreNotEqual(string expectedText, string Compare,
            string foutmeldingVerwachtResultaat = "The validation was not successful", [CallerMemberName] string callingMethod = "")
        {
            Assert.AreNotEqual(expectedText, Compare, foutmeldingVerwachtResultaat);
            if (TestRunSettings.ScannerEnabled == false || TestRunSettings.ScannerType == "NoneScannerTypeSet")
            {
                _scanner = false;
            }
            if (_scanner)
            {
                _scannerActions = new ScannerActions(_driver);
                _scannerActions.Scanner($"{callingMethod}", _browserType, _scannerType);
            }
        }
        public static void IsFalse(bool statement,
        string foutmeldingVerwachtResultaat = "The validation was not successful", [CallerMemberName] string callingMethod = "")
        {
            Assert.IsFalse(statement, foutmeldingVerwachtResultaat);
            if (TestRunSettings.ScannerEnabled == false || TestRunSettings.ScannerType == "NoneScannerTypeSet")
            {
                _scanner = false;
            }
            if (_scanner)
            {
                _scannerActions = new ScannerActions(_driver);
                _scannerActions.Scanner($"{callingMethod}", _browserType, _scannerType);
            }
        }

        public new static void Fail(string errorMessageExpectedResult = "The validation was not successful")
        {
            Assert.Fail(errorMessageExpectedResult);
        }
        public static void Contains(string verwachteTekst, string teVergelijken,
            string foutmeldingVerwachtResultaat = "The validation was not successful", [CallerMemberName] string callingMethod = "")
        {
            StringAssert.Contains(verwachteTekst, teVergelijken, foutmeldingVerwachtResultaat);
            if (TestRunSettings.ScannerEnabled == false || TestRunSettings.ScannerType == "NoneScannerTypeSet")
            {
                _scanner = false;
            }
            if (_scanner)
            {
                _scannerActions = new ScannerActions(_driver);
                _scannerActions.Scanner($"{callingMethod}", _browserType, _scannerType);
            }
        }
    }
}
