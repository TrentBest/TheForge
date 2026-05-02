using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Diagnostics;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    /// <summary>
    /// A pure builder for generic layout containers (Rows/Columns), eliminating 
    /// the need for raw VisualElement instantiation within providers.
    /// Reforged to follow the Granular Padding Protocol.
    /// </summary>
    public class ForgeContainerBuilder : IGuiProvider
    {
        public string Title => "Forge Container";
        private FlexDirection _direction = FlexDirection.Column;
        private readonly List<Action<VisualElement>> _onBuildActions = new();
        private readonly List<Func<GuiContext, VisualElement>> _childrenFactories = new();
        private readonly List<VisualElement> _directChildren = new();

        private string _containerName;

        public ForgeContainerBuilder(string name = null)
        {
            _containerName = name;
        }

        public ForgeContainerBuilder WithDirection(FlexDirection dir) { _direction = dir; return this; }

        public ForgeContainerBuilder OnBuild(Action<VisualElement> onBuildAction)
        {
            if (onBuildAction != null) _onBuildActions.Add(onBuildAction);
            return this;
        }

        public ForgeContainerBuilder AddChild(IGuiProvider provider)
        {
            if (provider != null) _childrenFactories.Add(ctx => provider.CreateGui(ctx));
            return this;
        }

        public ForgeContainerBuilder AddChild(VisualElement child)
        {
            if (child != null) _directChildren.Add(child);
            return this;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var container = new VisualElement();
            if (!string.IsNullOrEmpty(_containerName)) container.name = _containerName;

            container.style.flexDirection = _direction;

            // Populate GUI Provider Children
            foreach (var factory in _childrenFactories)
            {
                try { var child = factory?.Invoke(ctx); if (child != null) container.Add(child); }
                catch (Exception ex) { ctx?.Log?.Invoke($"[ForgeContainer] Child build failed: {ex.Message}"); }
            }

            // Populate Direct VisualElement Children
            foreach (var child in _directChildren)
            {
                container.Add(child);
            }

            // Execute delayed styling
            foreach (var act in _onBuildActions) act?.Invoke(container);

            ctx?.OnBuilt?.Invoke(container);
            return container;
        }
        public ForgeContainerBuilder WithPercentSize(float widthPct, float heightPct)
        {
            return OnBuild(ve => {
                ve.style.width = new StyleLength(new Length(widthPct, LengthUnit.Percent));
                ve.style.height = new StyleLength(new Length(heightPct, LengthUnit.Percent));
            });
        }

        public ForgeContainerBuilder WithPercentWidth(float widthPct)
        {
            return OnBuild(ve => ve.style.width = new StyleLength(new Length(widthPct, LengthUnit.Percent)));
        }

        public ForgeContainerBuilder WithPercentHeight(float heightPct)
        {
            return OnBuild(ve => ve.style.height = new StyleLength(new Length(heightPct, LengthUnit.Percent)));
        }
#if UNITY_EDITOR
        public void ToUIDocument(string assetPath)
        {
            var root = CreateGui(new GuiContext());
            string fileName = string.IsNullOrEmpty(assetPath) ? (_containerName ?? "ForgeContainer_Export") : System.IO.Path.GetFileNameWithoutExtension(assetPath);
            WorkshopUxmlBaker.Bake(root, fileName);
        }
#else
        public void ToUIDocument(string assetPath) { }
#endif

        public void FromUIDocument(string assetPath)
        {
            ForgeLogger.LogWarning("[ForgeContainer] FromUIDocument is bypassed. Containers are procedurally generated from their fluent definitions.");
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));

        // ==============================================================================
        // STYLING & LAYOUT
        // ==============================================================================

        public ForgeContainerBuilder WithFlexWrap(Wrap wrap) => OnBuild(ve => ve.style.flexWrap = wrap);
        public ForgeContainerBuilder WithPosition(Position position) => OnBuild(ve => ve.style.position = position);
        public ForgeContainerBuilder WithBackgroundColor(Color color) => OnBuild(ve => ve.style.backgroundColor = color);
        public ForgeContainerBuilder WithWidth(StyleLength width) => OnBuild(ve => ve.style.width = width);
        public ForgeContainerBuilder WithHeight(StyleLength height) => OnBuild(ve => ve.style.height = height);
        public ForgeContainerBuilder WithAlignItems(Align align) => OnBuild(ve => ve.style.alignItems = align);
        public ForgeContainerBuilder WithJustifyContent(Justify justify) => OnBuild(ve => ve.style.justifyContent = justify);
        public ForgeContainerBuilder WithFlexGrow(float grow) => OnBuild(ve => ve.style.flexGrow = grow);
        public ForgeContainerBuilder WithFlexShrink(float shrink) => OnBuild(ve => ve.style.flexShrink = shrink);
        public ForgeContainerBuilder WithVisibility(Visibility visibility) => OnBuild(ve => ve.style.visibility = visibility);
        public ForgeContainerBuilder WithDisplay(DisplayStyle display) => OnBuild(ve => ve.style.display = display);

        public ForgeContainerBuilder WithFlexLayout(FlexDirection dir, Justify justify, Align align)
        {
            _direction = dir;
            return OnBuild(ve => {
                ve.style.flexDirection = dir;
                ve.style.justifyContent = justify;
                ve.style.alignItems = align;
            });
        }

        // --- BORDERS & RADII ---
        public ForgeContainerBuilder WithBorderColor(Color color) => WithBorderColor(color, color, color, color);
        public ForgeContainerBuilder WithBorderColor(Color top, Color right, Color bottom, Color left)
        {
            return OnBuild(ve => {
                ve.style.borderTopColor = top; ve.style.borderRightColor = right;
                ve.style.borderBottomColor = bottom; ve.style.borderLeftColor = left;
            });
        }

        public ForgeContainerBuilder WithBorderWidth(float width) => WithBorderWidth(width, width, width, width);
        public ForgeContainerBuilder WithBorderWidth(float top, float right, float bottom, float left)
        {
            return OnBuild(ve => {
                ve.style.borderTopWidth = top; ve.style.borderRightWidth = right;
                ve.style.borderBottomWidth = bottom; ve.style.borderLeftWidth = left;
            });
        }

        public ForgeContainerBuilder WithBorderRadius(float radius) => WithBorderRadius(radius, radius, radius, radius);
        public ForgeContainerBuilder WithBorderRadius(float topLeft, float topRight, float bottomLeft, float bottomRight)
        {
            return OnBuild(ve => {
                ve.style.borderTopLeftRadius = topLeft; ve.style.borderTopRightRadius = topRight;
                ve.style.borderBottomLeftRadius = bottomLeft; ve.style.borderBottomRightRadius = bottomRight;
            });
        }

        // --- PADDING (REFORGED GRANULAR CONTROL) ---
        public ForgeContainerBuilder WithPadding(float all) => WithPadding(all, all, all, all);
        public ForgeContainerBuilder WithPadding(float top, float right, float bottom, float left)
        {
            return OnBuild(ve => {
                ve.style.paddingTop = top; ve.style.paddingRight = right;
                ve.style.paddingBottom = bottom; ve.style.paddingLeft = left;
            });
        }

        public ForgeContainerBuilder WithPaddingTop(float top) => OnBuild(ve => ve.style.paddingTop = top);
        public ForgeContainerBuilder WithPaddingBottom(float bottom) => OnBuild(ve => ve.style.paddingBottom = bottom);
        public ForgeContainerBuilder WithPaddingLeft(float left) => OnBuild(ve => ve.style.paddingLeft = left);
        public ForgeContainerBuilder WithPaddingRight(float right) => OnBuild(ve => ve.style.paddingRight = right);

        // --- MARGINS ---
        public ForgeContainerBuilder WithMargin(StyleLength all) => OnBuild(ve => ve.style.marginTop = ve.style.marginBottom = ve.style.marginLeft = ve.style.marginRight = all);
        public ForgeContainerBuilder WithMarginTop(StyleLength top) => OnBuild(ve => ve.style.marginTop = top);
        public ForgeContainerBuilder WithMarginBottom(StyleLength bottom) => OnBuild(ve => ve.style.marginBottom = bottom);
        public ForgeContainerBuilder WithMarginLeft(StyleLength left) => OnBuild(ve => ve.style.marginLeft = left);
        public ForgeContainerBuilder WithMarginRight(StyleLength right) => OnBuild(ve => ve.style.marginRight = right);

        public ForgeContainerBuilder AddSeparator(Color color, float thickness = 1f)
        {
            return OnBuild(container =>
            {
                var separator = new VisualElement { name = "Forge_Separator" };
                separator.style.backgroundColor = color;

                if (_direction == FlexDirection.Row || _direction == FlexDirection.RowReverse)
                {
                    separator.style.width = thickness;
                    separator.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
                    separator.style.marginLeft = 5f;
                    separator.style.marginRight = 5f;
                }
                else
                {
                    separator.style.height = thickness;
                    separator.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
                    separator.style.marginTop = 5f;
                    separator.style.marginBottom = 5f;
                }

                container.Add(separator);
            });
        }

        /// <summary>
        /// Sets the flex-basis using an integer (pixel value).
        /// </summary>
        public ForgeContainerBuilder WithFlexBasis(int v)
        {
            return WithFlexBasis((StyleLength)v);
        }

        /// <summary>
        /// Sets the flex-basis using a StyleLength (supporting pixels, percentages, or keywords).
        /// This follows the standard established in ForgeButtonBuilder and ForgeTextFieldBuilder.
        /// </summary>
        public ForgeContainerBuilder WithFlexBasis(StyleLength basis)
        {
            return OnBuild(ve => ve.style.flexBasis = basis);
        }

        // ==============================================================================
        // GRANULAR BORDER CONTROL (Forge Standard)
        // ==============================================================================

        // Cardinal Width Overrides
        public ForgeContainerBuilder WithBorderTopWidth(float width) => OnBuild(ve => ve.style.borderTopWidth = width);
        public ForgeContainerBuilder WithBorderRightWidth(float width) => OnBuild(ve => ve.style.borderRightWidth = width);
        public ForgeContainerBuilder WithBorderBottomWidth(float width) => OnBuild(ve => ve.style.borderBottomWidth = width);
        public ForgeContainerBuilder WithBorderLeftWidth(float width) => OnBuild(ve => ve.style.borderLeftWidth = width);

        // Cardinal Color Overrides
        public ForgeContainerBuilder WithBorderTopColor(Color color) => OnBuild(ve => ve.style.borderTopColor = color);
        public ForgeContainerBuilder WithBorderRightColor(Color color) => OnBuild(ve => ve.style.borderRightColor = color);
        public ForgeContainerBuilder WithBorderBottomColor(Color color) => OnBuild(ve => ve.style.borderBottomColor = color);
        public ForgeContainerBuilder WithBorderLeftColor(Color color) => OnBuild(ve => ve.style.borderLeftColor = color);
    }
}