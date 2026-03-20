using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.Builders.GuiBuilders; // Ensure factory namespace is here

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders
{
    public class GuiCatalogProvider : IGuiProvider
    {
        public string Title => "Component Catalog";
        private Action<string> _onComponentSelected;

        // Pass a callback so we know what the user clicked!
        public GuiCatalogProvider(Action<string> onComponentSelected)
        {
            _onComponentSelected = onComponentSelected;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var b = new GraphicalUserInterfaceBuilder("CatalogRoot")
                .WithWidth(600)
                .WithHeight(400)
                .WithBackgroundColor(new Color(0.15f, 0.15f, 0.18f))
                .WithBorderRadius(8)
                .WithBorderWidth(2)
                .WithBorderColor(Color.cyan)
                .WithPadding(15);

            b.AddChild(new Label("Select a GUI Component") { style = { fontSize = 18, unityFontStyleAndWeight = FontStyle.Bold, color = Color.white, marginBottom = 15 } });

            // The Scrolling Grid
            var grid = new GraphicalUserInterfaceBuilder("Grid")
                .WithScrollable(true)
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.FlexStart)
                .WithFlexWrap(Wrap.Wrap); // Wrap items like a grid!

            var factory = GuiFactoryProvider.GetFactory();

            // Populate Catalog Items (Live Previews!)
            grid.AddChild(CreateCatalogCard("Primary Button", factory.CreateLabel("Click Me"), "Button"));
            grid.AddChild(CreateCatalogCard("Text Input", factory.CreateTextField("Name", "Value", null), "TextField"));
            grid.AddChild(CreateCatalogCard("Toggle Switch", factory.CreateToggle("Enable", true, null), "Toggle"));
            grid.AddChild(CreateCatalogCard("Number Slider", factory.CreateSlider("Volume", 0, 100, 50, null), "Slider"));
            grid.AddChild(CreateCatalogCard("Color Picker", factory.CreateColorField("Tint", Color.red, null), "ColorField"));

            b.AddChild(grid);
            return b.Build();
        }

        private VisualElement CreateCatalogCard(string title, VisualElement livePreview, string componentId)
        {
            var card = new VisualElement();
            card.style.width = 170;
            card.style.height = 120;
            card.style.marginRight = 10;
            card.style.marginBottom = 10;
            card.style.paddingTop = 10;
            card.style.paddingRight = 10;
            card.style.paddingBottom = 10;
            card.style.paddingLeft = 10;
            card.style.backgroundColor = new Color(0.2f, 0.2f, 0.25f);
            card.style.borderTopLeftRadius = 6;
            card.style.borderTopRightRadius = 6;
            card.style.borderBottomLeftRadius = 6;
            card.style.borderBottomRightRadius = 6;

            // Title
            card.Add(new Label(title) { style = { unityFontStyleAndWeight = FontStyle.Bold, color = Color.gray, marginBottom = 10 } });

            // The Live Preview (We disable picking so the user can't accidentally interact with the slider inside the card)
            livePreview.pickingMode = PickingMode.Ignore;
            foreach (var child in livePreview.Children()) child.pickingMode = PickingMode.Ignore;
            card.Add(livePreview);

            // Hover effects and Click
            card.RegisterCallback<MouseEnterEvent>(e => card.style.backgroundColor = new Color(0.3f, 0.3f, 0.35f));
            card.RegisterCallback<MouseLeaveEvent>(e => card.style.backgroundColor = new Color(0.2f, 0.2f, 0.25f));
            card.RegisterCallback<ClickEvent>(e => _onComponentSelected?.Invoke(componentId));

            return card;
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}