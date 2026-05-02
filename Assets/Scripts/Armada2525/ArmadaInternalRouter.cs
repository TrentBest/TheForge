using System;
using System.Collections.Generic;
using UnityEngine;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.Armada2525
{
    /// <summary>
    /// The ArmadaInternalRouter serves as the navigational spine for the Armada 2525 Experience.
    /// It bridges the local MicroPackage state machine with the global Singularity Forge Hub,
    /// providing history tracking and semantic cross-linking for the Encyclopedia Galactica.
    /// </summary>
    public class ArmadaInternalRouter : IGuiRouter
    {
        private readonly GuiFlowRouterBuilder _localSandbox;
        private readonly IGuiRouter _globalForge;

        // --- 🏛️ GLORY FEATURE: NAVIGATION HISTORY ---
        // Allows for recursive "Go Back" functionality in deep-drill encyclopedia entries.
        private readonly Stack<string> _navigationHistory = new Stack<string>();

        public string CurrentRoute => _localSandbox.CurrentRoute;

        public ArmadaInternalRouter(GuiFlowRouterBuilder local, IGuiRouter global)
        {
            _localSandbox = local ?? throw new ArgumentNullException(nameof(local));
            _globalForge = global;
        }

        /// <summary>
        /// Standardized route navigation with Global Interceptors and Semantic Discovery.
        /// </summary>
        public void NavigateTo(string routeId)
        {
            // --- 1. GLOBAL EXIT INTERCEPTOR ---
            // Bridges the exit request back to the primary Forge Showcase Hub.
            if (routeId == "ExitToForge" || routeId == "MainMenu" || routeId == "ExitPackage")
            {
                Debug.Log($"[Armada Router] Releasing local thread. Returning to Forge Hub.");
                _globalForge?.NavigateTo("ShowcaseHub");
                return;
            }

            // --- 2. SEMANTIC DISCOVERY INTERCEPTOR ---
            // GLORY: Allows the Archive to link directly to other Forge Knowledge Base nodes.
            // Example: Navigating to "ForgeLink:PeriodicTable" from a planet detail view.
            if (routeId.StartsWith("ForgeLink:"))
            {
                string target = routeId.Substring("ForgeLink:".Length);
                Debug.Log($"[Armada Router] Semantic Jump to Knowledge Node: {target}");
                _globalForge?.NavigateTo(target);
                return;
            }

            // --- 3. LOCAL NAVIGATION & HISTORY LOGGING ---
            if (!string.IsNullOrEmpty(CurrentRoute) && CurrentRoute != routeId)
            {
                _navigationHistory.Push(CurrentRoute);
            }

            Debug.Log($"[Armada Router] Internal Transition: {CurrentRoute} -> {routeId}");
            _localSandbox.NavigateTo(routeId);
        }

        /// <summary>
        /// Direct provider injection for ad-hoc sequences (like modals, alerts, or mid-sim dialogs).
        /// </summary>
        public void Navigate(IGuiProvider provider)
        {
            if (provider == null) return;
            Debug.Log($"[Armada Router] Injecting Ad-hoc Provider: {provider.Title}");
            _localSandbox.Navigate(provider);
        }

        /// <summary>
        /// GLORY FEATURE: Reverts to the previous state.
        /// Ideal for returning from a planet-level detail view back to the galactic map.
        /// </summary>
        public void GoBack()
        {
            if (_navigationHistory.Count > 0)
            {
                string prev = _navigationHistory.Pop();
                Debug.Log($"[Armada Router] Reverting History: Returning to {prev}");
                _localSandbox.NavigateTo(prev);
            }
            else
            {
                // Default escape if history is empty
                NavigateTo("ExitToForge");
            }
        }

        // --- IGuiRouter Passthrough Implementations ---

        public IEnumerable<string> GetAvailableRoutes() => _localSandbox.GetAvailableRoutes();

        public IGuiProvider GetProvider(string routeId) => _localSandbox.GetProvider(routeId);

        public void RegisterRoute(string routeName, Func<IGuiRouter, IGuiProvider> providerFactory)
        {
            _localSandbox.RegisterRoute(routeName, providerFactory);
        }

        public void NavigateBack()
        {
            throw new NotImplementedException();
        }
    }
}