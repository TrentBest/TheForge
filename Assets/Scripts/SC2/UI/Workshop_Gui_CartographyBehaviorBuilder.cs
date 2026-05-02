using UnityEngine.UIElements;
using System;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.SC2.UI
{
    public class Workshop_Gui_CartographyBehaviorBuilder : IGuiProvider
    {
        private CartographyBehaviorData _data;

        public Workshop_Gui_CartographyBehaviorBuilder(CartographyBehaviorData data)
        {
            _data = data;
        }

        public string Title => "Cartography & Scouting Config";

        public VisualElement CreateGui(GuiContext context)
        {
            var guiBuilder = new GraphicalUserInterfaceBuilder(context.Name)
                .WithTitle(Title)
                .WithPadding(10)
                .WithAutoGrow();

            // Hand the data object to the ReflectiveGuiBuilder to handle the 0-100 sliders
            var reflector = new ReflectiveGuiBuilder<CartographyBehaviorData>(_data)
                .WithRecursion(2)
                .WithTitle("Tactical Scouting Parameters");

            guiBuilder.AddChild(reflector);

            return guiBuilder.CreateGui(context);
        }

        public Action<VisualElement> GetGuiBuilder()
        {
            return (container) => container.Add(CreateGui(new GuiContext { Name = "Cartography_Panel" }));
        }

        public void FromUIDocument(string assetPath)
        {
            // Trigger a refresh from a pre-authored UXML template if available
            var visualTree = UnityEngine.Resources.Load<VisualTreeAsset>(assetPath);
            if (visualTree != null) this.CreateGui(new GuiContext { Name = "Hydrated_Cartography" });
        }

#if UNITY_EDITOR
        public void ToUIDocument(string assetPath)
        {
            // Serialize the dynamically generated layout to a physical UXML asset
            var root = CreateGui(new GuiContext());
            GraphicalUserInterfaceBuilder.ConvertToUIDocument(root, assetPath);
        }
#endif
    }
}