using System;
using System.Collections.Generic;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.Ants
{
    public partial class Ants_ShowcaseProvider
    {
        // ====================================================================
        // THE ROUTER BRIDGE (Inner Class)
        // ====================================================================
        private class MyrmecologyInternalRouter : IGuiRouter
        {
            private GuiFlowRouterBuilder _localSandbox;
            private IGuiRouter _globalForge;

            public string CurrentRoute => _localSandbox.CurrentRoute;

            public MyrmecologyInternalRouter(GuiFlowRouterBuilder local, IGuiRouter global)
            {
                _localSandbox = local;
                _globalForge = global;
            }

            // The Interceptor
            public void NavigateTo(string routeName)
            {
                if (routeName == "ExitPackage" || routeName == "Intro")
                {
                    _globalForge?.NavigateTo("ShowcaseHub");
                }
                else
                {
                    _localSandbox.NavigateTo(routeName);
                }
            }

            // Pass-throughs to satisfy the IGuiRouter interface natively
            public void Navigate(IGuiProvider provider) => _localSandbox.Navigate(provider);
            public void RegisterRoute(string routeName, Func<IGuiRouter, IGuiProvider> providerFactory) => _localSandbox.RegisterRoute(routeName, providerFactory);
            public IGuiProvider GetProvider(string routeName) => _localSandbox.GetProvider(routeName);
            public IEnumerable<string> GetAvailableRoutes() => _localSandbox.GetAvailableRoutes();

            public void NavigateBack()
            {
                throw new NotImplementedException();
            }
        }
    }
}