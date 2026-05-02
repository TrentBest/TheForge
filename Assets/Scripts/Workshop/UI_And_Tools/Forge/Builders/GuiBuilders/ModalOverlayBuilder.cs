using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Tools
{
    /// <summary>
    /// Manages full-screen modal overlays to focus user attention and block background interaction.
    /// Reforged to follow the Forge Protocol and kill lambda ambiguity.
    /// </summary>
    public class ModalOverlayBuilder : IGuiProvider
    {
        public string Title => "Modal Overlay";

        private readonly IGuiProvider _contentProvider;
        private readonly Action _onClose;
        private string _modalTitle = "SYSTEM MODAL";
        private Color _backdropColor = new Color(0.01f, 0.01f, 0.01f, 0.85f);

        // Tracking the manifested element for the Show/Hide lifecycle
        private VisualElement _manifestedOverlay;

        public ModalOverlayBuilder(IGuiProvider contentProvider, Action onClose = null)
        {
            _contentProvider = contentProvider;
            _onClose = onClose;
        }

        // --- FLUENT API ---

        public ModalOverlayBuilder WithTitle(string title)
        {
            _modalTitle = title;
            return this;
        }

        public ModalOverlayBuilder WithBackdropColor(Color color)
        {
            _backdropColor = color;
            return this;
        }

        // --- LIFECYCLE API ---

        /// <summary>
        /// Injects the modal into the specified parent container (usually the UI Root).
        /// </summary>
        public void Show(VisualElement parent)
        {
            if (parent == null) return;
            _manifestedOverlay = CreateGui(new GuiContext());
            parent.Add(_manifestedOverlay);
        }

        /// <summary>
        /// Removes the modal from the UI hierarchy.
        /// </summary>
        public void Hide()
        {
            if (_manifestedOverlay != null && _manifestedOverlay.parent != null)
            {
                _manifestedOverlay.RemoveFromHierarchy();
            }
            _onClose?.Invoke();
        }

        // --- IGUI-PROVIDER IMPLEMENTATION ---

        public VisualElement CreateGui(GuiContext ctx)
        {
            // 1. The Blocker (Dark Glass Background)
            var overlayBuilder = new ForgeContainerBuilder("ModalBlocker")
                .WithPosition(Position.Absolute)
                .OnBuild(ve => {
                    ve.style.left = 0; ve.style.right = 0;
                    ve.style.top = 0; ve.style.bottom = 0;
                })
                .WithBackgroundColor(_backdropColor)
                .WithAlignItems(Align.Center)
                .WithJustifyContent(Justify.Center);

            // 2. The Floating Window
            var windowBuilder = new ForgeContainerBuilder("ModalWindow")
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f))
                .WithBorderWidth(1f)
                .WithBorderColor(Color.cyan)
                .WithBorderRadius(8f)
                .OnBuild(ve => {
                    ve.style.maxHeight = new StyleLength(Length.Percent(90f));
                    ve.style.maxWidth = new StyleLength(Length.Percent(90f));
                });

            // 3. Modal Header
            var headerBuilder = new ForgeContainerBuilder("ModalHeader")
                .WithHeight(40f)
                .WithDirection(FlexDirection.Row)
                .WithJustifyContent(Justify.SpaceBetween)
                .WithAlignItems(Align.Center)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.06f))
                .WithPaddingLeft(15f).WithPaddingRight(10f)
                .OnBuild(ve => {
                    ve.style.borderBottomWidth = 1f;
                    ve.style.borderBottomColor = Color.cyan;
                })
                .AddChild(new ForgeLabelBuilder(_modalTitle).WithColor(Color.cyan).WithBold())
                .AddChild(new ForgeButtonBuilder("✖")
                    .WithBackgroundColor(Color.clear)
                    .WithColor(Color.gray)
                    .WithFontSize(16)
                    .OnClick(Hide)); // Close button triggers the Hide logic

            // 4. Content Assembly
            var contentWrapper = new ForgeContainerBuilder("ModalContentWrapper")
                .WithFlexGrow(1f)
                .WithPadding(10f)
                .AddChild(_contentProvider);

            windowBuilder.AddChild(headerBuilder);
            windowBuilder.AddChild(contentWrapper);
            overlayBuilder.AddChild(windowBuilder);

            return overlayBuilder.Build();
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }

        internal void Show()
        {
            throw new NotImplementedException();
        }
    }
}