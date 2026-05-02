using System.Collections.Generic;
using UnityEngine;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.UI_And_Tools.Showcase
{
    /// <summary>
    /// Identifies a primary Showcase application. 
    /// Discovered automatically by the Singularity Lobby via Reflection.
    /// </summary>
    public interface IShowcasePortal : IGuiProvider
    {
        // Set to false to shunt discovery during reflection (e.g., for WIP domains)
        bool IsDiscoverable { get; }
        bool IsUnderConstruction { get; }
        string Category { get; }
        string Overview { get; }
        Color AccentColor { get; }
        List<string> CoreTechnologies { get; }
        string TelemetryState { get; }

        // Allows the lobby to pass the router down to the portal before it boots
        void InjectRouter(IGuiRouter router);
    }
}