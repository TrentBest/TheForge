using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    public class ForgeScrollViewBuilder : IGuiProvider
    {
        public string Title => "Forge ScrollView";

        private string _name;
        private ScrollViewMode _mode = ScrollViewMode.Vertical;
        private bool _showVerticalScrollbar = true;
        private bool _showHorizontalScrollbar = false;

        private readonly List<Action<VisualElement>> _onBuildActions = new();
        private readonly List<Func<GuiContext, VisualElement>> _children = new();

        public ForgeScrollViewBuilder(string name = null)
        {
            _name = name;
        }

        // --- CORE CONFIGURATION ---

        public ForgeScrollViewBuilder WithMode(ScrollViewMode mode)
        {
            _mode = mode;
            return this;
        }

        public ForgeScrollViewBuilder WithScrollbars(bool vertical, bool horizontal)
        {
            _showVerticalScrollbar = vertical;
            _showHorizontalScrollbar = horizontal;
            return this;
        }

        public ForgeScrollViewBuilder OnBuild(Action<VisualElement> onBuildAction)
        {
            if (onBuildAction != null) _onBuildActions.Add(onBuildAction);
            return this;
        }

        public ForgeScrollViewBuilder AddChild(IGuiProvider provider)
        {
            if (provider != null) _children.Add(ctx => provider.CreateGui(ctx));
            return this;
        }

        public ForgeScrollViewBuilder AddChild(VisualElement element)
        {
            if (element != null) _children.Add(ctx => element);
            return this;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var sv = new ScrollView(_mode);

            if (!string.IsNullOrEmpty(_name)) sv.name = _name;

            // Apply ScrollView Specifics
            sv.verticalScrollerVisibility = _showVerticalScrollbar ? ScrollerVisibility.Auto : ScrollerVisibility.Hidden;
            sv.horizontalScrollerVisibility = _showHorizontalScrollbar ? ScrollerVisibility.Auto : ScrollerVisibility.Hidden;

            // Default behavior for Forge
            sv.style.flexGrow = 1;

            // Populate Children
            foreach (var factory in _children)
            {
                try
                {
                    var child = factory?.Invoke(ctx);
                    if (child != null) sv.Add(child);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[Forge ScrollView] Child build failed: {ex.Message}");
                }
            }

            // Execute delayed evaluation actions (Layout/Style)
            foreach (var act in _onBuildActions) act?.Invoke(sv);

            ctx?.OnBuilt?.Invoke(sv);
            return sv;
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }

        // ==============================================================================
        // SIZING & SPACING (THE ELBOW GREASE)
        // ==============================================================================

        public ForgeScrollViewBuilder WithWidth(StyleLength width) => OnBuild(ve => ve.style.width = width);
        public ForgeScrollViewBuilder WithHeight(StyleLength height) => OnBuild(ve => ve.style.height = height);
        public ForgeScrollViewBuilder WithMinWidth(StyleLength minWidth) => OnBuild(ve => ve.style.minWidth = minWidth);
        public ForgeScrollViewBuilder WithMinHeight(StyleLength minHeight) => OnBuild(ve => ve.style.minHeight = minHeight);

        // This fixes the 'padding' error by mapping one call to all four style properties
        public ForgeScrollViewBuilder WithPadding(float all) => WithPadding(all, all, all, all);
        public ForgeScrollViewBuilder WithPadding(float top, float right, float bottom, float left)
        {
            return OnBuild(ve => {
                ve.style.paddingTop = top;
                ve.style.paddingRight = right;
                ve.style.paddingBottom = bottom;
                ve.style.paddingLeft = left;
            });
        }

        public ForgeScrollViewBuilder WithMargin(float all) => WithMargin(all, all, all, all);
        public ForgeScrollViewBuilder WithMargin(float top, float right, float bottom, float left)
        {
            return OnBuild(ve => {
                ve.style.marginTop = top;
                ve.style.marginRight = right;
                ve.style.marginBottom = bottom;
                ve.style.marginLeft = left;
            });
        }

        // ==============================================================================
        // COLORS & BORDERS
        // ==============================================================================

        public ForgeScrollViewBuilder WithBackgroundColor(Color color) => OnBuild(ve => ve.style.backgroundColor = color);

        public ForgeScrollViewBuilder WithBorderColor(Color color)
        {
            return OnBuild(ve => {
                ve.style.borderTopColor = color;
                ve.style.borderRightColor = color;
                ve.style.borderBottomColor = color;
                ve.style.borderLeftColor = color;
            });
        }

        public ForgeScrollViewBuilder WithBorderWidth(float width)
        {
            return OnBuild(ve => {
                ve.style.borderTopWidth = width;
                ve.style.borderRightWidth = width;
                ve.style.borderBottomWidth = width;
                ve.style.borderLeftWidth = width;
            });
        }

        public ForgeScrollViewBuilder WithBorderRadius(float radius)
        {
            return OnBuild(ve => {
                ve.style.borderTopLeftRadius = radius;
                ve.style.borderTopRightRadius = radius;
                ve.style.borderBottomRightRadius = radius;
                ve.style.borderBottomLeftRadius = radius;
            });
        }

        // ==============================================================================
        // FLEXBOX LAYOUT
        // ==============================================================================

        public ForgeScrollViewBuilder WithFlexGrow(float grow) => OnBuild(ve => ve.style.flexGrow = grow);
        public ForgeScrollViewBuilder WithFlexShrink(float shrink) => OnBuild(ve => ve.style.flexShrink = shrink);
        public ForgeScrollViewBuilder WithAlignItems(Align align) => OnBuild(ve => ve.style.alignItems = align);
        public ForgeScrollViewBuilder WithJustifyContent(Justify justify) => OnBuild(ve => ve.style.justifyContent = justify);
    }
}