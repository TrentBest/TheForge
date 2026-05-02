using System;
using System.Collections.Generic;
using UnityEngine;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Showcase;

namespace Workshop.UI_And_Tools.Forge.Builders.Staging
{
    public class ReflectiveRouteGuiBuilder : IGuiRouter
    {
        private readonly Dictionary<string, Func<IGuiRouter, IGuiProvider>> _routes = new();
        private string _currentRoute;

        public string CurrentRoute => _currentRoute;

        public ReflectiveRouteGuiBuilder()
        {
            DiscoverPortalRoutes();
        }

        public IEnumerable<string> GetAvailableRoutes() => _routes.Keys;

        public IGuiProvider GetProvider(string routeId)
        {
            if (_routes.TryGetValue(routeId, out var factory)) return factory.Invoke(this);
            Debug.LogWarning($"[Router] Route '{routeId}' not found.");
            return null;
        }

        public void NavigateTo(string routeId)
        {
            if (_routes.ContainsKey(routeId))
            {
                _currentRoute = routeId;
                Debug.Log($"[Router] Navigating to active route: {routeId}");
            }
        }

        public void RegisterRoute(string routeName, Func<IGuiRouter, IGuiProvider> providerFactory)
        {
            if (!_routes.ContainsKey(routeName)) _routes[routeName] = providerFactory;
        }

        private void DiscoverPortalRoutes()
        {
            Debug.Log("[Router] Requesting curated Portals from ExperienceRegistry...");

            // We leverage your existing Registry! It already did the hard work of reflecting and validating.
            var registry = ExperienceRegistry.GetDiscoveredPortals(this);

            int routeCount = 0;
            foreach (var category in registry.Values)
            {
                foreach (var portal in category)
                {
                    string routeName = portal.GetType().Name;

                    // Register the portal itself as a valid route destination
                    _routes[routeName] = _ => portal;
                    routeCount++;
                }
            }

            // You will now see ~17 routes instead of 234!
            Debug.Log($"[Router] Assimilated {routeCount} top-level Showcase Routes. Bypassed low-level GuiProviders.");
        }

        public void NavigateBack()
        {
            throw new NotImplementedException();
        }
    }
}