using System;
using SimpleInjector;

namespace CSharpSeleniumFramework.Engine
{
    public static class Extensions
    {
        public static void RegisterLazy<TService>(this Container container, Func<TService> factory,
            Lifestyle lifestyle = null)
            where TService : class
        {
            lifestyle = lifestyle ?? Lifestyle.Singleton;
            container.Register(factory, lifestyle);
            container.Register(() => new Lazy<TService>(() => container.GetInstance<TService>()), lifestyle);
        }

        public static void RegisterLazy<TService, TImplementation>(this Container container, Lifestyle lifestyle = null)
            where TImplementation : class, TService
            where TService : class
        {
            lifestyle = lifestyle ?? Lifestyle.Singleton;
            container.Register<TService, TImplementation>(lifestyle);
            container.Register(() => new Lazy<TService>(() => container.GetInstance<TService>()), lifestyle);
        }

        public static void RegisterLazy<TService>(this Container container, Lifestyle lifestyle = null)
            where TService : class
        {
            container.RegisterLazy<TService, TService>(lifestyle);
        }

        public static void RegisterLazyForReflectionPurposesDoNotChangeName<TService>(this Container container,
            Lifestyle lifestyle = null)
            where TService : class
        {
            container.RegisterLazy<TService, TService>(lifestyle);
        }
    }
}