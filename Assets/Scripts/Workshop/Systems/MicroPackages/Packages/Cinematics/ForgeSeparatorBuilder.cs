using UnityEngine;
using UnityEngine.UIElements;

namespace Workshop.Systems.MicroPackages.Packages.Cinematics
{
    /// <summary>
    /// A tactical UI component used to demarcate logical sections within the Forge.
    /// Manifests as a thin, high-contrast line slaved to the package's accent color.
    /// </summary>
    internal class ForgeSeparatorBuilder : VisualElement
    {
        /// <summary>
        /// Constructor: Fabricates the visual divider.
        /// </summary>
        /// <param name="accentColor">The color signature of the owning package.</param>
        /// <param name="thickness">The vertical height of the line in pixels.</param>
        public ForgeSeparatorBuilder(Color accentColor, float thickness = 1f)
        {
            name = "Forge_Separator";

            // --- Styling the Line ---
            style.backgroundColor = accentColor;
            style.height = thickness;

            // --- Spatial Orientation ---
            // We give it a standard 8px margin to ensure the UI breathes 
            // without losing its tight, industrial feel.
            style.marginTop = 8;
            style.marginBottom = 8;

            // Ensure the line spans the full width of its parent container
            style.width = Length.Percent(100);

            // Set an subtle opacity so it doesn't overwhelm the text
            style.opacity = 0.6f;
        }
    }
}