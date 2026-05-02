using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.PillVirus
{
    // ====================================================================
    // STANDARDIZED ALPHA ENTRY POINT
    // ====================================================================
    public class PillVirus_Playable_Alpha : IGuiProvider
    {
        public string Title => "PillVirus Alpha";

        public VisualElement CreateGui(GuiContext ctx)
        {
            // Instead of a stub, we instantiate the App router and hand over control
            var app = new PillVirus_App();
            var root = app.CreateGui(ctx);

            // Optional: You can force a background color here if the app doesn't set one
            root.style.backgroundColor = new Color(0.08f, 0.08f, 0.1f);

            return root;
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}