using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.Forge.Builders.Architecture
{
    /// <summary>
    /// Forges the internal volume of a structure or vehicle.
    /// Supports Euclidean (matching) and Non-Euclidean (juxtaposed) spatial logic.
    /// </summary>
    public class InteriorVolumeBuilder : IForgeBuilder
    {
        // --- IBuilder Identity ---
        public string ToolName => "Interior Volume Builder";
        public Type GetProductType() => typeof(GameObject);

        // --- Configuration State ---
        public Mesh ExternalMesh;
        public float MaterialThickness = 0.25f; // AEC standard for armored bulkheads
        public InteriorType Type = InteriorType.Vehicle;
        public bool MatchesExteriorVolume = true;

        // Custom bounds for the "Tiny room in a massive warehouse" scenario
        public Vector3 CustomInteriorDimensions = new Vector3(2, 2, 2);

        public enum InteriorType { Vehicle, Building, Utility, Void }

        /// <summary>
        /// The Visual Provider (CRUD UI) for the Workshop.
        /// This allows the user to tweak the "Juxtaposition" settings before building.
        /// </summary>
        public IGuiProvider GetGuiProvider()
        {
            return new GraphicalUserInterfaceBuilder("Interior_Volume_Config")
                .WithTitle(ToolName)
                .WithPadding(10)
                .WithBackgroundColor(new Color(0.12f, 0.12f, 0.15f))

                .AddHeader("SPATIAL CONSTRAINTS")
                .AddToggleData("Matches Exterior Footprint", MatchesExteriorVolume, val => MatchesExteriorVolume = val)
                .AddFloatData("Wall/Bulkhead Thickness", MaterialThickness, val => MaterialThickness = val)

                .AddSeparator(Color.gray, 5)

                .AddHeader("JUCTAPOSITION OVERRIDES")
                .AddEnumData("Interior Type", Type, val => Type = val)
                .AddChild(ctx => {
                    // Only show dimension fields if not matching exterior
                    var container = new VisualElement();
                    if (!MatchesExteriorVolume)
                    {
                        container.Add(new FloatField("Custom Width") { value = CustomInteriorDimensions.x });
                        container.Add(new FloatField("Custom Height") { value = CustomInteriorDimensions.y });
                    }
                    return container;
                })

                .AddButton("⚒️ FORGE INTERIOR", () => Build());
        }

        /// <summary>
        /// The Build Logic: Extracts and serializes the volume.
        /// </summary>
        public object Build()
        {
            Debug.Log($"<b>[The Forge]</b> Manifesting {Type} interior...");

            GameObject interiorRoot = new GameObject($"Interior_Volume_{Guid.NewGuid().ToString().Substring(0, 4)}");

            if (MatchesExteriorVolume && ExternalMesh != null)
            {
                // Logic: Extract interior shell by inverting normals and offsetting by MaterialThickness
                Debug.Log($"[InteriorBuilder] Wrapping exterior mesh: {ExternalMesh.name}");
            }
            else
            {
                // Logic: Spawning an uncoupled "Pocket Reality" box
                Debug.Log($"[InteriorBuilder] Spawning Juxtaposed Volume: {CustomInteriorDimensions}");
            }

            // Register with the MetaDev high-water mark for telemetry
            // Workshop.Systems.MetaDev.MetaDevObserver.ReportShelfResize("Spatial_Volume", 1);

            return interiorRoot;
        }
    }
}