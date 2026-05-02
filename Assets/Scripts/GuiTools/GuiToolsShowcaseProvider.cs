using System;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.UI_And_Tools.Showcase
{
    public class GuiToolsShowcaseProvider : IGuiProvider
    {
        public string Title => "Workshop: GUI Hub";

        public VisualElement CreateGui(GuiContext ctx)
        {
            // Wraps the Hub in a showcase-friendly container
            var hub = new GuiTools_Hub();
            return hub.CreateGui(ctx);
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}