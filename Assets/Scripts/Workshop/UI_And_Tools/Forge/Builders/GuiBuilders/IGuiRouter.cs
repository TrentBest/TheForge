using System;
using System.Collections.Generic;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    public interface IGuiRouter
    {
        // The Mediator command. Tell the router where to go next!
        void NavigateTo(string routeId);
        string CurrentRoute { get; }
        IEnumerable<string> GetAvailableRoutes();
        IGuiProvider GetProvider(string routeId);
        void RegisterRoute(string routeName, Func<IGuiRouter, IGuiProvider> providerFactory);
        void NavigateBack();
    }
}