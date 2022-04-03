using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.iOS;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace CSharpSeleniumFramework.Engine
{
    public class ContainerFactory
    {
        public static ContainerFactory Instance = new ContainerFactory();
        public Container CreateContainer(Func<dynamic> webDriverFactory)
        {
            var inputDriver = webDriverFactory();

            var container = new Container();
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            container.RegisterLazy(ToWebDriver(), Lifestyle.Singleton);
            container.RegisterLazy(ReturnAndroidDriver(), Lifestyle.Singleton);
            container.RegisterLazy<WebAppInitializer>();
            WebAppInitializer.RegisterDependencies(container);
            container.Register<TestScope>(Lifestyle.Transient);
            return container;


            Func<IWebDriver> ToWebDriver()
            {
                IWebDriver result = inputDriver;
                return () => result;
            }
            Func<AndroidDriver<AppiumWebElement>> ReturnAndroidDriver()
            {
                try
                {
                    AndroidDriver<AppiumWebElement> result = inputDriver;
                    return () => result;
                }
                catch
                {
                    return () => null;
                }
            }
        }
    }
}