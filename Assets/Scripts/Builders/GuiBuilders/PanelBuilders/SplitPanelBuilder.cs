using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.Builders.GuiBuilders.PanelBuilders
{
    public class SplitPanelBuilder : IGuiProvider
    {
        private IGuiProvider _left;
        private IGuiProvider _right;
        private int _splitWidth;

        public SplitPanelBuilder(int sidebarWidth = 250) => _splitWidth = sidebarWidth;

        public SplitPanelBuilder WithSidebar(IGuiProvider left) { _left = left; return this; }
        public SplitPanelBuilder WithMain(IGuiProvider right) { _right = right; return this; }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new VisualElement { style = { flexDirection = FlexDirection.Row, flexGrow = 1 } };

            var leftPane = new VisualElement { style = { width = _splitWidth, borderRightWidth = 1, borderRightColor = Color.black } };
            if (_left != null) leftPane.Add(_left.CreateGui(ctx));

            var rightPane = new VisualElement { style = { flexGrow = 1 } };
            if (_right != null) rightPane.Add(_right.CreateGui(ctx));

            root.Add(leftPane);
            root.Add(rightPane);
            return root;
        }
    }
}
