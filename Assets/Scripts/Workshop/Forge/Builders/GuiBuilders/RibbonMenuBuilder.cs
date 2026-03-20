using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders
{
    public class RibbonMenuBuilder// : IBuilder<VisualElement> // Assuming your generic IBuilder interface
    {
        private GraphicalUserInterfaceBuilder _builder;
        private Action<string> _onTabSelected;
        private string _activeTab;

        public RibbonMenuBuilder(string activeTab, Action<string> onTabSelected)
        {
            _activeTab = activeTab;
            _onTabSelected = onTabSelected;
            _builder = new GraphicalUserInterfaceBuilder("RibbonMenu")
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Center)
                .WithFlexWrap(Wrap.Wrap);
        }

        public RibbonMenuBuilder AddTab(string tabName, Color activeColor)
        {
            bool isActive = _activeTab == tabName;
            var btn = new Button(() => _onTabSelected?.Invoke(tabName)) { text = tabName };
            // ... apply the sleek pill-shaped styling here ...
            btn.style.backgroundColor = isActive ? activeColor : new Color(0.2f, 0.2f, 0.2f);

            _builder.AddChild(btn);
            return this;
        }

        public VisualElement Build() => _builder.Build();
    }
}