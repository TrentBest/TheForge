using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    /// <summary>
    /// A fluent builder and runtime implementation for UI navigation and experience orchestration.
    /// Manages the lifecycle of IGuiProviders within a designated container.
    /// </summary>
    public class GuiFlowRouterBuilder : IGuiRouter, IGuiProvider
    {
        private readonly string _routerName;
        private string _currentRoute;
        private VisualElement _container;

        // Stores registered routes and their factory functions
        private readonly Dictionary<string, Func<IGuiRouter, IGuiProvider>> _routes = new Dictionary<string, Func<IGuiRouter, IGuiProvider>>();

        public string CurrentRoute => _currentRoute;
        public string Title => _routerName.ToUpper();

        public GuiFlowRouterBuilder(string routerName, string initialRoute)
        {
            _routerName = routerName;
            _currentRoute = initialRoute;
        }

        // ==========================================
        // FLUENT BUILDER METHODS
        // ==========================================

        /// <summary>
        /// Binds the router to an existing VisualElement root (e.g., from a UIDocument).
        /// </summary>
        public GuiFlowRouterBuilder WithRoot(VisualElement root)
        {
            _container = root;
            return this;
        }

        /// <summary>
        /// Registers a named route to the router.
        /// </summary>
        public GuiFlowRouterBuilder AddRoute(string routeName, Func<IGuiRouter, IGuiProvider> providerFactory)
        {
            RegisterRoute(routeName, providerFactory);
            return this;
        }

        /// <summary>
        /// Finalizes the builder and returns the functional IGuiRouter instance.
        /// </summary>
        public IGuiRouter Build()
        {
            // If no container was provided via WithRoot, we initialize a default container
            if (_container == null)
            {
                _container = new GraphicalUserInterfaceBuilder(_routerName)
                    .WithFlexGrow(1)
                    .Build();
            }

            // Perform initial navigation if a route is set
            if (!string.IsNullOrEmpty(_currentRoute) && _routes.ContainsKey(_currentRoute))
            {
                NavigateTo(_currentRoute);
            }

            return this;
        }

        // ==========================================
        // IGUIROUTER IMPLEMENTATION
        // ==========================================

        /// <summary>
        /// Directly navigates to a specific provider instance, bypassing the route registry.
        /// Useful for one-off sequences like Boot or Dialogs.
        /// </summary>
        public void Navigate(IGuiProvider provider)
        {
            if (_container == null || provider == null) return;

            _container.Clear();
            _container.Add(provider.CreateGui(new GuiContext()));

            // Update the current route to reflect an external/anonymous state
            _currentRoute = provider.Title;
        }

        /// <summary>
        /// Navigates to a registered route by name.
        /// </summary>
        public void NavigateTo(string routeName)
        {
            if (_container == null) return;
            if (!_routes.TryGetValue(routeName, out var factory)) return;

            _currentRoute = routeName;
            _container.Clear();

            var provider = factory(this);
            _container.Add(provider.CreateGui(new GuiContext()));
        }

        public void RegisterRoute(string routeName, Func<IGuiRouter, IGuiProvider> providerFactory)
        {
            _routes[routeName] = providerFactory;
        }

        public IGuiProvider GetProvider(string routeName)
        {
            return _routes.TryGetValue(routeName, out var factory) ? factory(this) : null;
        }

        public IEnumerable<string> GetAvailableRoutes() => _routes.Keys;

        // ==========================================
        // IGUIPROVIDER IMPLEMENTATION
        // ==========================================

        public VisualElement CreateGui(GuiContext ctx)
        {
            // Returns the managed container as the GUI representation of the router itself
            if (_container == null)
            {
                _container = new GraphicalUserInterfaceBuilder(_routerName)
                    .WithFlexGrow(1)
                    .Build();
            }
            return _container;
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));

        public void ToUIDocument(string assetPath)
        {
            // Implementation for serializing router state to a UXML asset if required by the Forge Tooling
            Debug.Log($"Serializing Router {_routerName} to {assetPath}");
        }

        public void FromUIDocument(string assetPath)
        {
            // Implementation for restoring router state from a UXML asset
            Debug.Log($"Restoring Router {_routerName} from {assetPath}");
        }

        public void NavigateBack()
        {
            throw new NotImplementedException();
        }
    }
}