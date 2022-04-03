using System;
using System.Linq;
using CSharpSeleniumFramework.Pages.algemeen;
using CSharpSeleniumFramework.Pages.Contact;
using SimpleInjector;

using Assert = CSharpSeleniumFramework.Engine.AssertWithScreenshot;

namespace CSharpSeleniumFramework.Engine
{
    public class WebAppInitializer
    {

        public GeneralActions GeneralActions => Resolve<GeneralActions>();
        public GeneralElements GeneralElements => Resolve<GeneralElements>();
        public ContactElements ContactElements => Resolve<ContactElements>();

        #region automagische registraties

        private readonly Container _container;

        public WebAppInitializer(Container container)
        {
            _container = container;
        }


        

        private T Resolve<T>()
        {
            return _container.GetInstance<Lazy<T>>().Value;
        }
        public static void RegisterDependencies(Container container)
        {
            container.RegisterLazyForReflectionPurposesDoNotChangeName<GeneralActions>();
            container.RegisterLazyForReflectionPurposesDoNotChangeName<GeneralElements>();
            container.RegisterLazyForReflectionPurposesDoNotChangeName<ContactElements>();
        }

        #endregion
    }
}