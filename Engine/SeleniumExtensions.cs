using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Internal;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using Assert = CSharpSeleniumFramework.Engine.AssertWithScreenshot;

namespace CSharpSeleniumFramework.Engine
{
    public static class SeleniumExtensions
    {
        public enum WaitUntilTypes
        {
            Visible,
            Interactable,
            ContainsText,
            HasValue,
            HasClass,
            ClassAbsent,
            HadClass,
            NotPresent,
            NotVisible
        }

        private static readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            StringEscapeHandling = StringEscapeHandling.EscapeHtml,
        };

        public enum ByType
        {
            Id,
            Name,
            ClassName
        }

        /// <summary>
        ///     Haal element op van pagina op veilige methode welke element of null terug geeft indien niet gevonden. Hierbij is
        ///     wordt er standaard gewacht tot hij zichtbaar is of de wachttijd verstreken is.
        /// </summary>
        /// <param name="driver">referentie naar webDriver</param>
        /// <param name="by">By object waarmee het element moet worden opgezocht</param>
        /// <param name="withWait">
        ///     Moet er gewacht worden tot het element aanwezig is op de pagina (in de code, niet persee
        ///     zichtbaar)
        /// </param>
        /// <param name="waitTime">Hoelang moet er gewacht worden, bij geen waarde wordt de standaard gebruikt.</param>
        /// <returns>Referentie naar het element of null indien het element niet bestaat/gevonden is</returns>
        public static IWebElement FindElementSafe(this IWebDriver driver, By by, bool withWait = true,
            int waitTime = -1)
        {
            if (withWait)
            {
                if (waitTime == -1) waitTime = TestRunSettings.DefaultWaitTime;
                var wait = new WebDriverWait(driver, new TimeSpan(0, 0, waitTime));
                try
                {
                    var foundElement = wait.Until(webDriver =>
                    {
                        try
                        {
                            IWebElement webElement = null;
                            var elementSearch = webDriver.FindElements(by);
                            // Ga alle gevonden elementen langs, return de eerste die zichtbaar is anders de eerste die gevonden is.
                            foreach (var element in elementSearch)
                            {
                                if (webElement == null)
                                {
                                    webElement = element.Exists() ? element : null;
                                }

                                if (element.Exists())
                                {
                                    if (element.Displayed)
                                    {
                                        return element;
                                    } 
                                }
                            }

                            // Als het element gevonden is binnen de waitTime geef het element terug, anders null
                            return webElement;

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

                    return foundElement;
                }

                catch (WebDriverTimeoutException)
                {
                    return null;
                }
            }


            try
            {
                return driver.FindElement(by);
            }
            catch (NoSuchElementException)
            {
                return null;
            }
        }

        public static IWebElement FindElementInContainerById(
            this IWebDriver driver, string containerId, string elementId,
            int maxMatchAttempts = 3,int msBetweenAttempts = 100,int maxWaitTimeForContainer = -1
        )
        {
            var containerSelector = By.Id(containerId);
            var containingElementsBy = By.XPath("//*[@id='" + containerId +  "']//*");
            return driver.FindElementInContainer(
                containerSelector,
                containingElementsBy,
                elementId,
                maxMatchAttempts,
                msBetweenAttempts,
                maxWaitTimeForContainer
            );
        }

        public static IWebElement FindElementInContainerByTagName(
            this IWebDriver driver, string containerTagName, string elementId,
            int maxMatchAttempts = 3,int msBetweenAttempts = 100,int maxWaitTimeForContainer = -1
        )
        {
            var containerSelector = By.TagName(containerTagName);
            var containingElementsBy = By.XPath("//" + containerTagName + "//*");
            return driver.FindElementInContainer(
                containerSelector,
                containingElementsBy,
                elementId,
                maxMatchAttempts,
                msBetweenAttempts,
                maxWaitTimeForContainer
            );
        }

        public static IWebElement FindElementInContainer(this IWebDriver driver, By containerBy, By containingElementsBy, string elementId, int maxMatchAttempts = 3,int msBetweenAttempts = 100 ,int maxWaitTimeForContainer = -1)
        {
            if (maxWaitTimeForContainer == -1) maxWaitTimeForContainer = TestRunSettings.DefaultWaitTime;
            var sw = new Stopwatch();
            sw.Start();
            IWebElement[] matchingElements = null;
            IWebElement container;
            int tryCounter = 0;
            do
            {
                container = FindElementSafe(driver, containerBy, false);
                if (container.Exists())
                {
                    var foundElements = driver.GetElementsInContainer(containingElementsBy);
                    matchingElements = FindMatchingElementsInArray(foundElements, elementId);

                    if (matchingElements == null || matchingElements.Length == 0)
                    {
                        tryCounter++;
                        Thread.Sleep(msBetweenAttempts);
                    }
                }
            }
            while (tryCounter < maxMatchAttempts && (sw.Elapsed.Seconds < maxWaitTimeForContainer || container.Exists()));

            Assert.IsTrue(container.Exists(),  "Expected the container to be found");
            Assert.NotNull(matchingElements, "Expected to find 1 element with the requested ID inside the container.");
            Assert.AreEqual(1, matchingElements.Count(), "Expected to find only 1 element with the requested ID inside the container.");
            
            return matchingElements.FirstOrDefault();
        }

        private static IWebElement[] GetElementsInContainer(this IWebDriver driver, By containingElementsBy)
        {
            return driver.FindElements(containingElementsBy).ToArray();
        }

        private static IWebElement[] FindMatchingElementsInArray(IWebElement[] elements, string elementId)
        {
            if (elements.Length == 0)
            {
                return null;
            }

            return elements.Where(x => x.GetAttribute("Id") == elementId || x.GetProperty("Id") == elementId).ToArray();
        }

        /// <summary>
        /// Haal element op van pagina op basis van een baseId, variabele en optionele postfix
        /// </summary>
        /// <param name="driver">referentie naar webDriver</param>
        /// <param name="baseId">vaste deel van ID voor de variabele</param>
        /// <param name="variable">variabele deel van de ID (vaak een regelnummer/id)</param>
        /// <param name="postfix">vaste deel van ID na de variabele (optioneel)</param>
        /// <param name="withWait">
        ///     Moet er gewacht worden tot het element aanwezig is op de pagina (in de code, niet persee
        ///     zichtbaar)
        /// </param>
        /// <param name="waitTime">Hoelang moet er gewacht worden, bij geen waarde wordt de standaard gebruikt.</param>
        /// <returns>Referentie naar het element of null indien het element niet bestaat/gevonden is</returns>
        public static IWebElement FindElementWithVariableId(this IWebDriver driver, string baseId, dynamic variable, string postfix = "", bool withWait = true, int waitTime = -1, string prefix = "")
        {
            return FindElementSafe(driver,By.Id(prefix + baseId + variable.ToString() + postfix), withWait, waitTime);
        }

        /// <summary>
        /// Haal element op van pagina op basis van een baseId, variabele
        /// en optionele postfix met een bool om een korte wait te doen ipv de normale
        /// </summary>
        /// <param name="driver">referentie naar webDriver</param>
        /// <param name="baseId">vaste deel van ID voor de variabele</param>
        /// <param name="variable">variabele deel van de ID (vaak een regelnummer/id)</param>
        /// <param name="postfix">vaste deel van ID na de variabele (optioneel)</param>
        /// <param name="withShortWait">Als dit true is wordt er maar 1 seconden gezocht ipv de default</param>
        /// <returns>Referentie naar het element of null indien het element niet bestaat/gevonden is</returns>
        public static IWebElement FindElementWithVariableIdOptionalShortWait(this IWebDriver driver, string baseId, dynamic variable, string postfix = "", bool withShortWait = false)
        {
            if (withShortWait)
            {
                return FindElementWithVariableId(driver,baseId,variable,postfix, waitTime: 1);
            }
            return FindElementWithVariableId(driver, baseId, variable, postfix);
        }

        /// <summary>
        ///     Wacht tot een element de gewenste status heeft
        /// </summary>
        /// <param name="element">Het element welke gecontroleerd moet worden</param>
        /// <param name="type">Vanuit de WaitUntilTypes enum gewenste status selecteren</param>
        /// <param name="expectedText">Benodigde aditionele parameters</param>
        /// <param name="waitTime">Optionele override van maximal wachttijd</param>
        /// <param name="byType">Als er een andere selectie methode is gebruikt dan ID kan hiermee override worden gedaan.</param>
        /// <returns></returns>
        public static IWebElement WaitUntil(this IWebElement element, WaitUntilTypes type, string expected = "", 
            int waitTime = -1, ByType byType = ByType.Id)
        {
            if (waitTime == -1) waitTime = TestRunSettings.DefaultWaitTime;

            IWebDriver driver;
            By by = null;

            try
            {
                // Haal eigenschappen op van het ingevoerde IWebElement element.
                driver = ((IWrapsDriver) element).WrappedDriver;
                switch (byType)
                {
                    case ByType.Id:
                        by = By.Id(element.GetAttribute("Id"));
                        break;
                    case ByType.ClassName:
                        by = By.ClassName(element.GetAttribute("Class"));
                        break;
                    case ByType.Name:
                        by = By.Name(element.GetAttribute("Name"));
                        break;
                    default:
                        Assert.Fail($"This type of By '{byType.ToString()}' is not implemented in the WaitUntil function.");
                        break;
                }
                
            }
            catch
            {
                return null;
            }

            switch (type)
            {
                case WaitUntilTypes.Interactable:
                case WaitUntilTypes.Visible:
                    return WaitUntilInteractableVisible(driver, waitTime, type, by);
                case WaitUntilTypes.NotPresent:
                case WaitUntilTypes.NotVisible:
                    return WaitUntilNot(driver, type, waitTime, by);
                case WaitUntilTypes.ContainsText:
                    return WaitUntilContainsTextHelper(driver, by, expected, waitTime);
                case WaitUntilTypes.HasValue:
                    return WaitUntilHasValueHelper(driver, by, expected, waitTime);
                case WaitUntilTypes.HasClass:
                    return WaitUntilHasClass(driver, by, expected, waitTime);
                case WaitUntilTypes.ClassAbsent:
                    return WaitUntilClassAbsent(driver, by, expected, waitTime);
                case WaitUntilTypes.HadClass:
                    return WaitUntilHadClass(driver, by, expected, waitTime);
                default:
                    return null;
            }
        }

        /// <summary>
        ///     Geeft aan of dit element bestaat
        /// </summary>
        /// <param name="element">Referentie naar het element</param>
        /// <returns>True als element bestaat anders false</returns>
        public static bool Exists(this IWebElement element)
        {
            if (element == null) return false;
            return true;
        }


        /// <summary>
        ///     Upload een bestand via het opgegeven veld en bestandnaam. De bestanden moeten in de UploadsFile map worden gezet in
        ///     deze solution.
        /// </summary>
        /// <param name="element">
        ///     Element dat verantwoordelijk is voor het uploaden (niet perse hetzelfde als de link om te
        ///     klikken)
        /// </param>
        /// <param name="filename">
        ///     Gewenste bestand in de UploadFiles map. Vul de volledige naam in, zoals deze weergegeven is in
        ///     de map. (inclusief extentie)
        /// </param>
        public static void UploadFileInputBox(this IWebElement element, string filename)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var driver = ((IWrapsDriver)element).WrappedDriver;
            if (driver is IAllowsFileDetection allowsDetection) allowsDetection.FileDetector = new LocalFileDetector();
            var filepath = AppDomain.CurrentDomain.BaseDirectory + "UploadFiles\\" + filename;
            // Controleer of bestand wel in build aanwezig is
            Assert.That(filepath, Does.Exist, "Het geselecteerde bestand is niet gevonden op de opgegeven locatie.");
            if (element.GetAttribute("value") != "")
            {
                element.ClearElementWithJavascript();
            }
            element.SendKeys(filepath);
        }

        /// <summary>
        /// Clear veld met javascript.
        /// Deze method volgt niet hetzelfde gebruikersgedrag, gebruik het maar allen als het nodig is!
        /// </summary>
        /// <param name="element"></param>
        public static void ClearElementWithJavascript(this IWebElement element)
        {
            var driver = ((IWrapsDriver)element).WrappedDriver;
            var js = (IJavaScriptExecutor)driver;
            var elementId = element.GetAttribute("Id");
            js.ExecuteScript("var elements = document.querySelectorAll('[id=hiddenFileInput]'); elements.forEach((element) => {element.value = '';});");
        }

        /// <summary>
        /// Upload meer bestanden via het opgeven veld en bestandnaam.De bestanden moeten in de UploadsFile map worden gezet in
        /// </summary>
        /// <param name="element">
        ///     Element dat verantwoordelijk is voor het uploaden (niet perse hetzelfde als de link om te
        ///     klikken)
        /// </param>
        /// <param name="filenames">
        /// Gewenste bestand in de UploadFiles map. Vul de volledige naam in de Files array in, zoals deze weergeven is in de map. 
        /// bijv: string Files = {"file1.jpg", "file2.png"}
        /// </param>
        public static void UploadMultipleFiles(this IWebElement element, string[] filenames)
        {
            for (var File = 0; File < filenames.Length; File++)
            {
                 UploadFileInputBox(element, filenames[File]);
            }
        }

        /// <summary>
        ///     Controleert of een element specifieke class bevat. Let op, het moet een exacte match met 1 van de classes zijn.
        ///     Bij fail wordt er een assert gefaild.
        /// </summary>
        /// <param name="element">Welk IWebElement moet gecontroleer worden</param>
        /// <param name="expectedClass">Op welke class controleren we</param>
        /// <param name="withAsserts">Voer asserts uit op check</param>
        public static bool ValidateElementClassContains(this IWebElement element, string expectedClass,
            bool withAsserts = true)
        {
            try
            {
                var classes = element.GetAttribute("class").Split(" ");
                var containsClass = classes.Contains(expectedClass);
                if (withAsserts)
                {
                    Assert.Contains(expectedClass, element.GetAttribute("class").Split(" "),
                            $"Verwacht dat element met ID '{element.GetAttribute("Id")}' de opgegeven class bevat.");
                }
                return containsClass;
            }
            catch (StaleElementReferenceException) //Als het orginele element niet meer te vinden is
            {
                if (withAsserts) Assert.Fail("Kon element niet controleren op bevatten van class " + expectedClass);
                return false;
            }
            catch (NullReferenceException) //Als element input niet bestaat
            {
                if (withAsserts) Assert.Fail("Kon element niet controleren op bevatten van class " + expectedClass);
                return false;
            }
        }

        /// <summary>
        ///     Voer de tekst per character 1 voor 1 in, is zeer langzaam in IE maar wel 100% betrouwbaar. Valideerd of de input
        ///     gelijk is aan de veld waarde.
        /// </summary>
        /// <param name="element">IWebElement</param>
        /// <param name="input">Gewenste invoer</param>
        public static void SendKeysByCharacter(this IWebElement element, string value)
        { 
            element.WaitUntil(WaitUntilTypes.Interactable);
            var numberOfRetries = 0;
            while (element.GetAttribute("value") != value && numberOfRetries < 5)
            {
                element.Clear();
                try
                {
                    element.Click();
                    element.SendKeys(value);
                }
                catch (Exception exception)
                {
                    Thread.Sleep(500);
                    Console.WriteLine(exception);
                }
                numberOfRetries++;
            }
        }

        /// <summary>
        ///     Scroll het element in beeld.
        /// </summary>
        /// <param name="element">Welk element moet in beeld komen</param>
        /// <returns></returns>
        public static IWebElement ScrollToElement(this IWebElement element)
        {
            // Als element null is kan deze functie niet worden uitgevoerd.
            Assert.NotNull(element, "Kan niet naar element scrollen omdat het element object null is.");

            // Haal eigenschappen op van het ingevoerde IWebElement element.
            var driver = ((IWrapsDriver) element).WrappedDriver;
            var js = (IJavaScriptExecutor) driver;
            try
            {
                if (element.Location.Y > 200) js.ExecuteScript($"window.scrollTo({0}, {element.Location.Y - 200})");
                return element;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Doe click via javscript
        /// LET OP, DIT IS NIET 100% GELIJK AAN HOE EEN GEBRUIKER HET DOET
        /// </summary>
        /// <param name="element">Welk element met geclicked worden</param>
        /// <returns></returns>
        public static IWebElement JavascriptClick(this IWebElement element, bool retry = false)
        {
            // Als element null is kan deze functie niet worden uitgevoerd.
            Assert.NotNull(element, "Kan niet worden geclicked omdat het element object null is.");

            // Haal eigenschappen op van het ingevoerde IWebElement element.
            var driver = ((IWrapsDriver)element).WrappedDriver;
            var js = (IJavaScriptExecutor)driver;
            var id = element.GetAttribute("id");
            try
            {
                if (element.Location.Y > 200)
                {
                    js.ExecuteScript("document.getElementById('"+ id + "').click()");
                } else if (retry == false)
                {
                    ScrollToElement(element);
                    JavascriptClick(element, true);
                }
                else
                {
                    js.ExecuteScript("document.getElementById('" + id + "').click()");
                }
                return element;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Haal value op van het element, zoals inputboxen
        /// </summary>
        /// <param name="element">IWebElement</param>
        /// <returns>string met de inhoud</returns>
        public static string GetValue(this IWebElement element)
        {
            return element.GetAttribute("value");
        }

        /// <summary>
        ///     Hiermee kun je in een keer van een element de verschillende attributen ophalen.
        ///     Handig als je wilt weten in welk attribuut die bepaalde tekst die je zoekt nou is opgeslagen.
        /// </summary>
        /// <param name="element"></param>
        public static void FindProperty(this IWebElement element)
        {
            Console.WriteLine("Attribute value = " + element.GetAttribute("value"));
            Console.WriteLine("Attribute innerText = " + element.GetAttribute("innerText"));
            Console.WriteLine("Attribute text = " + element.GetAttribute("text"));
            Console.WriteLine("Attribute class = " + element.GetAttribute("class"));
            Console.WriteLine("Attribute outerHTML = " + element.GetAttribute("outerHTML"));
            Console.WriteLine();
            Console.WriteLine("Property value = " + element.GetProperty("value"));
            Console.WriteLine("Property innerText = " + element.GetProperty("innerText"));
            Console.WriteLine("Property text = " + element.GetProperty("text"));
            Console.WriteLine("Property class = " + element.GetProperty("class"));
            Console.WriteLine("Property outerHTML = " + element.GetProperty("outerHTML"));
            Console.WriteLine();
            Console.WriteLine(".Text = " + element.Text);
        }


        /// <summary>
        ///     Dit haalt alle javascript attributen op.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static List<string> GetElementAttributes(this IWebElement element)
        {
            var driver = ((IWrapsDriver) element).WrappedDriver;
            var js = (IJavaScriptExecutor) driver;
            var attributesAndValues = (Dictionary<string, object>) js.ExecuteScript(
                "var items = { }; for (index = 0; index < arguments[0].attributes.length; ++index) { items[arguments[0].attributes[index].name] = arguments[0].attributes[index].value }; return items;",
                element);
            var attributes = attributesAndValues.Keys.ToList();
            return attributes;
        }

        public static IJavaScriptExecutor AsJavascriptExecutor(this IWebDriver webDriver)
        {
            return (IJavaScriptExecutor)webDriver;
        }

        public static TResult TryUntil<TResult>(this WebDriverWait webDriverWait, Func<IWebDriver, TResult> condition)
        {
            try
            {
                return webDriverWait.Until(condition);
            }
            catch (WebDriverTimeoutException)
            {
                return default(TResult);
            }
        }

        private static IWebElement WaitUntilInteractableVisible(IWebDriver driver, int waitTime, WaitUntilTypes type,
            By by)
        {
            try
            {
                var wait = new WebDriverWait(driver, new TimeSpan(0, 0, waitTime));
                var foundElement = wait.Until(webDriver =>
                {
                    try
                    {
                        var findElement = webDriver.FindElement(by);
                        // Als het element de juiste status heeft binnen de waitTime geef het element terug, anders null
                        switch (type)
                        {
                            case WaitUntilTypes.Interactable:
                            {
                                return findElement.Displayed && findElement.Enabled ? findElement : null;
                            }
                            case WaitUntilTypes.Visible:
                            {
                                return findElement.Displayed ? findElement : null;
                            }
                            default:
                                return null;
                        }
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
                return foundElement;
            }
            catch (WebDriverTimeoutException)
            {
                return null;
            }
        }

        private static IWebElement WaitUntilHasClass(IWebDriver driver, By by, string expectedClass, int waitTime)
        {
            IWebElement element = null;

            var wait = new WebDriverWait(driver, new TimeSpan(0, 0, waitTime));
            wait.TryUntil(driver =>
            {
                try
                {
                    element = driver.FindElementSafe(by, false);
                    if (element.ValidateElementClassContains(expectedClass, false))
                    {
                        return element;
                    }
                    element = null;
                }
                catch (StaleElementReferenceException)
                {
                    element = null;
                }
                catch (NullReferenceException)
                {
                    element = null;
                }

                return element;
            });

            return element;
        }

        private static IWebElement WaitUntilClassAbsent(IWebDriver driver, By by, string expectedClass, int waitTime)
        {
            IWebElement element = null;

            var wait = new WebDriverWait(driver, new TimeSpan(0, 0, waitTime));
            wait.TryUntil(driver =>
            {
                try
                {
                    element = driver.FindElementSafe(by, false);
                    if (!element.ValidateElementClassContains(expectedClass, false))
                    {
                        return element;
                    }
                    element = null;
                }
                catch (StaleElementReferenceException)
                {
                    element = null;
                }
                catch (NullReferenceException)
                {
                    element = null;
                }

                return element;
            });

            return element;
        }

        private static IWebElement WaitUntilHadClass(IWebDriver driver, By by, string expectedClass, int waitTime)
        {
            WaitUntilHasClass(driver, by, expectedClass, waitTime);
            return WaitUntilClassAbsent(driver, by, expectedClass, waitTime);
        }

        private static IWebElement WaitUntilContainsTextHelper(IWebDriver driver, By by, string expectedText,
            int waitTime)
        {
            IWebElement element = null;
            var startTime = DateTime.UtcNow;
            var foundIt = 0;
            while (foundIt <= 2 && DateTime.UtcNow - startTime < TimeSpan.FromSeconds(waitTime))
                try
                {
                    element = driver.FindElementSafe(by, false);
                    if (element.Text.Contains(expectedText) ||
                        element.GetAttribute("value").Contains(expectedText) ||
                        element.GetAttribute("text").Contains(expectedText) ||
                        element.GetAttribute("innerText").Contains(expectedText))
                    {
                        foundIt++;
                    }
                    else
                    {
                        foundIt = 0;
                        element = null;
                    }
                }
                catch (StaleElementReferenceException)
                {
                    element = null;
                    foundIt = 0;
                }
                catch (NullReferenceException)
                {
                    element = null;
                    foundIt = 0;
                }

            return element;
        }

        private static IWebElement WaitUntilHasValueHelper(IWebDriver driver, By by, string expectedValue, int waitTime)
        {
            IWebElement element = null;
            var wait = new WebDriverWait(driver, new TimeSpan(0, 0, waitTime));
            wait.TryUntil(driver =>
            {
                try
                {
                    element = driver.FindElementSafe(by, false);
                    if (element.GetAttribute("value") != expectedValue)
                    {
                        element = null;
                    }
                }
                catch (StaleElementReferenceException)
                {
                    element = null;
                }
                catch (NullReferenceException)
                {
                    element = null;
                }

                return element;
            });

            return element;
        }

        private static IWebElement WaitUntilNot(IWebDriver driver, WaitUntilTypes type, int waitTime, By by)
        {
            var startTime = DateTime.UtcNow;
            var elementExists = true;
            IWebElement element = null;
            try
            {
                while (elementExists && DateTime.UtcNow - startTime < TimeSpan.FromSeconds(waitTime))
                {
                    element = driver.FindElementSafe(by, false);
                    if (element == null) return null;

                    switch (type)
                    {
                        case WaitUntilTypes.NotVisible:
                        {
                            elementExists = element.Displayed;
                            break;
                        }
                        case WaitUntilTypes.NotPresent:
                        {
                            elementExists = element.Exists();
                            break;
                        }
                    }
                }
            }
            catch (StaleElementReferenceException) //Als het orginele element niet meer te vinden is
            {
                elementExists = false;
            }
            catch (NullReferenceException) //Als het orginele element niet meer te vinden is
            {
                elementExists = false;
            }
            catch (NoSuchElementException) //Als het orginele element niet meer te vinden is
            {
                elementExists = false;
            }

            if (elementExists == false) return null;

            return element;
        }

        public static IAlert WaitForJavascriptAlert(this IWebDriver driver, int waitTime = -1)
        {
            if (waitTime == -1) waitTime = TestRunSettings.DefaultWaitTime;
            try
            {
                var wait = new WebDriverWait(driver, new TimeSpan(0, 0, waitTime));
                var foundAlert = wait.Until(drv => AlertIsPresent(drv));
                return foundAlert;
            }
            catch (WebDriverTimeoutException)
            {
                return null;
            }


            IAlert AlertIsPresent(IWebDriver drv)
            {
                try
                {
                    // Attempt to switch to an alert
                    return drv.SwitchTo().Alert();
                }
                catch (OpenQA.Selenium.NoAlertPresentException)
                {
                    // We ignore this execption, as it means there is no alert present...yet.
                    return null;
                }
            }
        }

        public static void SelectDropdown(this IWebElement element, string option)
        {
            new SelectElement(element).SelectByText(option);
        }

        /// <summary>
        /// Haal het nummer uit de string
        /// </summary>
        /// <param name="element">Het element welke gecontroleerd moet worden</param>
        /// <returns>het laatste nummer in de string </returns>
        public static int GetNumber(this IWebElement element)
        {
            string[] elementText = Regex.Split(element.Text, @"\D+");
            int number = 0;
            foreach (string value in elementText)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    number = Int32.Parse(value);
                }
            }
            return number;
        }

        /// <summary>
        /// Voer toetsenbord toets + letter uit op het geselecteerde element
        /// </summary>
        /// <param name="element">Het element welke gecontroleerd moet worden</param>
        /// <param name="modifierKey">Toets die ingedrukt moet zijn i.c.m. een letter toets (ctrl, alt, etc.)</param>
        /// <param name="letterKey">Letter toets die ingedrukt moet worden</param>
        public static void PressKeyAndLetter(this IWebElement element, string modifierKey,  string letterKey)
        {
            try
            {
                // Haal eigenschappen op van het ingevoerde IWebElement element.
                var driver = ((IWrapsDriver)element).WrappedDriver;
                new Actions(driver)
                    .KeyDown(element, modifierKey)
                    .SendKeys(letterKey)
                    .Perform();

                new Actions(driver)
                    .KeyUp(element, modifierKey)
                    .Perform();
            }
            catch
            {
                Assert.Fail("Functie kan niet worden gebruikt als element niet gevonden is");
            }
        }

        /// <summary>
        /// Voer hotkey op een bedrag veld uit voor bedrag inclusief btw
        /// </summary>
        /// <param name="element"></param>
        public static void PressInclusiefBtwHotKey(this IWebElement element)
        {
            element.PressKeyAndLetter(Keys.Control, "i");
        }

        /// <summary>
        /// Voer hotkey op een bedrag veld uit voor bedrag exclusief btw
        /// </summary>
        /// <param name="element"></param>
        public static void PressExclusiefBtwHotKey(this IWebElement element)
        {
            element.PressKeyAndLetter(Keys.Control, "e");
        }

        /// <summary>
        /// Klikt op element en controleert of het element na de ingesteld tijd nog zichtbaar is
        /// als het element nog zichtbaar is gaat hij nogmaals klikken tot maximale aantal pogingen bereikt is
        /// </summary>
        /// <param name="element">Welk element moet aangeklikt worden</param>
        /// <param name="retryAfter">Na hoeveel seconden moet er nieuwe poging worden gedaan</param>
        /// <param name="maxRetryAttempts">Hoeveel pogingen moet hij doen</param>
        public static void ClickWithRetry(this IWebElement element, int retryAfter = 5, int maxRetryAttempts = 3)
        {
            var attempt = 0;
            do
            {
                if (attempt > 0)
                {
                    Console.WriteLine($"ClickWithRetry: {attempt + 1}e poging om te klikken op element.");
                }
                try
                {
                    element.Click();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"ClickWithRetry: Klikken tijdens de {0}e poging ging mis. Met exception {1}", attempt + 1, e.ToString());
                }
            }
            while (element.WaitUntil(WaitUntilTypes.NotVisible, waitTime: retryAfter).Exists() && attempt++ < maxRetryAttempts);
        }
        /// <summary>
        /// A method to simulate the user experience of clearing a text field using the keyboard
        /// On Windows by pressing ctrl + a
        /// On Mac by pressing cmd + a 
        /// </summary>
        /// <param name="element">Which element containing the text to be selected</param>
        /// <param name="browserTypes">The type of browser used in the test</param>
        public static void ClearTextFieldUsingKeyboard(this IWebElement element, WebdriverDefinitions.BrowserTypes browserTypes)
        {
           string browserName =  browserTypes.ToString();
            if (browserName == "SafariMac")
            {
                element.SendKeys(Keys.Command + "a");
                element.SendKeys(Keys.Command + Keys.Backspace);
            }
            else
            {
                element.SendKeys(Keys.Control + "a");
                element.SendKeys( Keys.Backspace);
            }
        }
    }
}