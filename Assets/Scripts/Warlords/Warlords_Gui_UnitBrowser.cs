using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.Warlords
{
    public class Warlords_Gui_UnitBrowser : IGuiProvider
    {
        private GuiContext _lastCtx;

        public string Title => "Unit Browser";

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;
            var root = new VisualElement { style = { flexGrow = 1, backgroundColor = new Color(0.05f, 0.05f, 0.05f) } };

            // Header
            var title = new Label("UNIT ARCHIVES") { style = { fontSize = 32, color = Color.white, paddingTop = 20, paddingRight = 20, paddingBottom = 20, paddingLeft = 20, unityFontStyleAndWeight = FontStyle.Bold } };
            root.Add(title);

            // THE SCROLLING GRID
            var scrollView = new ScrollView(ScrollViewMode.Vertical);
            scrollView.style.flexGrow = 1;
            scrollView.style.paddingLeft = 20; scrollView.style.paddingRight = 20;

            // Populate with dummy unit cards
            scrollView.Add(new Warlords_UnitCard("Knight", 13, 12, 10, 12, "Broadsword", "Plate", 14, "2d+1 cut"));
            scrollView.Add(new Warlords_UnitCard("Archer", 10, 13, 10, 11, "Longbow", "Leather", 10, "1d+2 imp"));
            scrollView.Add(new Warlords_UnitCard("Wizard", 9, 11, 15, 10, "Staff", "None", 8, "1d-1 cr"));
            scrollView.Add(new Warlords_UnitCard("Dragon", 25, 14, 12, 18, "Breath", "Scales", 24, "4d burn"));

            root.Add(scrollView);
            return root;
        }


        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));

#if UNITY_EDITOR
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
#endif
        public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
    }
}