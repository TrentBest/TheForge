using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core;
using Workshop.Core.Memory;
using Workshop.Core.Diagnostics; // Injected for ForgeLogger
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.UI_And_Tools.Forge.Diagnostics
{
    /// <summary>
    /// A live architect for the Moniker splash. 
    /// Routes to the 'Rendering' tab via the inference engine prefix.
    /// </summary>
    public class Rendering_Gui_MonikerArchitect : IGuiProvider
    {
        // Rule: The string before the underscore dictates the editor tab
        public string Title => "Rendering_Gui_MonikerArchitect";

        // Default constructor for safe stable state and serialization
        public Rendering_Gui_MonikerArchitect() { }

        public VisualElement CreateGui(GuiContext ctx)
        {
            ForgeLogger.Log("Moniker Architect UI Generation Initiated.")
                .WithHeader("Architect")
                .WithColor("#00FFCC") // Neon Cyan
                .SendToUnity();

            var root = new GraphicalUserInterfaceBuilder("ArchitectRoot")
                .WithPadding(20).WithBackgroundColor(new Color(0.1f, 0.1f, 0.12f));

            root.AddHeader("MONIKER ARCHITECT", Color.cyan);

            // --- 1. OBSERVER DEPTH (Z) ---
            root.AddChild(new ForgeLabelBuilder("Observer Depth (Z)").WithColor(Color.gray));
            root.AddSliderData("Z-Axis", -100f, -5f, -18f, val => {
                var shelf = DataWarehouse.Default.GetShelf<WorldObserver>();
                if (shelf != null && shelf.IsActive(0))
                {
                    // Directly edit the unmanaged memory ref
                    shelf.GetRef(0).Position.z = val;

                    ForgeLogger.Log($"Observer depth adjusted to: {val:F2}m")
                        .WithHeader("Telemetry")
                        .WithColor(Color.cyan)
                        .SendToUnity();
                }
                else
                {
                    ForgeLogger.LogError("Observer update failed: WorldObserver shelf is null or inactive.")
                        .WithHeader("Memory")
                        .SendToUnity();
                }
            });

            // --- 2. HORIZONTAL SPACING ---
            root.AddChild(new ForgeLabelBuilder("Horizontal Spacing").WithColor(Color.gray));
            root.AddSliderData("Spacing", 0.1f, 5.0f, 1.2f, val => {
                UpdateMonikerSpacing(val);
            });

            return root.Build();
        }

        private void UpdateMonikerSpacing(float spacing)
        {
            var shelf = DataWarehouse.Default.GetShelf<SplashLetterEntity>();
            if (shelf == null)
            {
                ForgeLogger.LogError("Kerning update aborted: SplashLetterEntity shelf missing from Warehouse.")
                    .WithHeader("Memory")
                    .SendToUnity();
                return;
            }

            ForgeLogger.Log($"Recalculating Moniker layout with spacing: {spacing:F2}")
                .WithHeader("Architect")
                .WithColor(Color.yellow)
                .SendToUnity();

            string[] wordLines = { "THE", "SINGULARITY", "WORKSHOP" };
            int globalIndex = 0;
            float ySpacing = 1.5f; // Keep consistent with Bootloader

            for (int l = 0; l < wordLines.Length; l++)
            {
                string word = wordLines[l];
                float xStart = -((word.Length - 1) * spacing) / 2f;
                float yPos = ((wordLines.Length - 1) * ySpacing) / 2f - (l * ySpacing);

                for (int c = 0; c < word.Length; c++)
                {
                    if (shelf.IsActive(globalIndex))
                    {
                        ref var letter = ref shelf.GetRef(globalIndex);
                        letter.Position.x = xStart + (c * spacing);
                        letter.Position.y = yPos; // Ensure vertical position is preserved!
                    }
                    globalIndex++;
                }
            }
        }

        // --- IGuiProvider Lifecycle Requirements ---

        public Action<VisualElement> GetGuiBuilder()
            => root => root.Add(CreateGui(new GuiContext()));

        public void ToUIDocument(string assetPath) { }

        public void FromUIDocument(string assetPath) { }
    }
}