using UnityEngine.UIElements;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using TheSingularityWorkshop.Forge.SC2.Data;
using System;

namespace TheSingularityWorkshop.Forge.SC2.UI
{
    public class Workshop_Gui_CartographyBehaviorBuilder : IGuiProvider
    {
        private CartographyBehaviorData _data;

        public Workshop_Gui_CartographyBehaviorBuilder(CartographyBehaviorData data)
        {
            _data = data;
        }

        // 1. Fulfill the contract property
        public string Title => "Cartography & Scouting Config";

        public VisualElement CreateGui(GuiContext context)
        {
            // LEVERAGING THE WORKSHOP ECOSYSTEM:
            // We use your GraphicalUserInterfaceBuilder (and underlying IControlFactory)
            // so we don't have to care if this is rendering in a Unity EditorWindow 
            // or inside the diegetic in-game AR terminal.

            var guiBuilder = new GraphicalUserInterfaceBuilder(context.Name);

            // Because you have solved reflective GUI generation, we don't need to manually 
            // declare sliders and text fields. We just hand the builder the data object!

            // Example of how your fluent GraphicalUserInterfaceBuilder / Reflective builder handles it:
            //var root = guiBuilder
            //    .BeginPanel(Title)
            //    .InjectReflectiveControls(_data) // Auto-generates the 0-100 Sliders and binds the callbacks
            //    .EndPanel()
            //    .Build();

            //return root;
            return guiBuilder.CreateGui(context);
        }

        // --- Implementing the rest of your IGuiProvider Contract ---

        public Action<VisualElement> GetGuiBuilder()
        {
            // Returns the delegate to hydrate a parent container on demand
            return (container) =>
            {
                container.Clear();
                container.Add(CreateGui(new GuiContext()));
            };
        }

        public void FromUIDocument(string assetPath)
        {
            // Bypasses dynamic generation and loads the UI from a pre-authored 
            // UXML file via your Workshop's IO/Asset tools.
            throw new NotImplementedException("Cartography UI is currently dynamically generated via Workshop Factories.");
        }

        public void ToUIDocument(string assetPath)
        {
            // The true power of your system: taking the dynamically generated 
            // GraphicalUserInterfaceBuilder layout and serializing it out to a physical UXML asset!
            // Workshop_Gui_AssetIngestor or ForgeManipulator would handle the disk write here.
            throw new NotImplementedException("Serialization of this layout to disk is pending.");
        }
    }
}