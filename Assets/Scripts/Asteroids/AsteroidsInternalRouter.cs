using System;
using System.Collections.Generic;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.Asteroids
{
    public partial class Asteroids_ShowcaseProvider
    {
        // Local Router Bridge Implementation
        private class AsteroidsInternalRouter : IGuiRouter
        {
            private GuiFlowRouterBuilder _local;
            private IGuiRouter _global;
            public string CurrentRoute => _local.CurrentRoute;
            public AsteroidsInternalRouter(GuiFlowRouterBuilder local, IGuiRouter global) { _local = local; _global = global; }
            public void NavigateTo(string routeName)
            {
                if (routeName == "ExitToForge") _global?.NavigateTo("ShowcaseHub");
                else _local.NavigateTo(routeName);
            }
            public void Navigate(IGuiProvider provider) => _local.Navigate(provider);
            public void RegisterRoute(string n, Func<IGuiRouter, IGuiProvider> f) => _local.RegisterRoute(n, f);
            public IGuiProvider GetProvider(string n) => _local.GetProvider(n);
            public IEnumerable<string> GetAvailableRoutes() => _local.GetAvailableRoutes();

            public void NavigateBack()
            {
                throw new NotImplementedException();
            }
        }
    }
}