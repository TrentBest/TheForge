using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    // 1. Upgraded to implement IGuiProvider
    public class ForgeRibbonMenuBuilder : IGuiProvider
    {
        public string Title => "Forge Ribbon Menu";

        private GraphicalUserInterfaceBuilder _builder;
        private Action<string> _onTabSelected;
        private string _activeTab;

        public ForgeRibbonMenuBuilder(string activeTab, Action<string> onTabSelected)
        {
            _activeTab = activeTab;
            _onTabSelected = onTabSelected;
            _builder = new GraphicalUserInterfaceBuilder("RibbonMenu")
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Center)
                .WithFlexWrap(Wrap.Wrap)
                .WithMarginBottom(10); // Added breathing room below the ribbon
        }

        public ForgeRibbonMenuBuilder AddTab(string tabName, Color activeColor)
        {
            bool isActive = _activeTab == tabName;

            // 2. Pure ForgeButtonBuilder implementation
            var btn = new ForgeButtonBuilder(tabName, () =>
            {
                _activeTab = tabName; // Update local state for redraw logic if needed
                _onTabSelected?.Invoke(tabName);
            })
            .WithBackgroundColor(isActive ? activeColor : new Color(0.2f, 0.2f, 0.2f))
            .WithTextColor(Color.white)
            .WithMargin(0, 5, 0, 0); // Adding standard margin between the pills

            _builder.AddChild(btn);
            return this;
        }

        // 3. IGuiProvider Interface Fulfillment
        public VisualElement CreateGui(GuiContext ctx)
        {
            return _builder.CreateGui(ctx);
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) { }
        public void FromUIDocument(string path) { }
    }
}