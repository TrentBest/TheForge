using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders
{
    public class ModalOverlayBuilder : IGuiProvider
    {
        public string Title { get; set; } = "Modal Overlay";

        private IGuiProvider _contentProvider;
        private Color _backdropColor = new Color(0, 0, 0, 0.85f);
        private bool _closeOnBackdropClick = true;
        private VisualElement _modalRoot;

        public ModalOverlayBuilder(IGuiProvider contentProvider)
        {
            _contentProvider = contentProvider;
        }

        public ModalOverlayBuilder WithBackdropColor(Color color) { _backdropColor = color; return this; }
        public ModalOverlayBuilder CloseOnBackdropClick(bool allowClose) { _closeOnBackdropClick = allowClose; return this; }

        public VisualElement CreateGui(GuiContext ctx)
        {
            // 1. The Full-Screen Backdrop
            _modalRoot = new VisualElement { name = "ModalBackdrop" };
            _modalRoot.style.position = Position.Absolute;
            _modalRoot.style.top = 0; _modalRoot.style.bottom = 0;
            _modalRoot.style.left = 0; _modalRoot.style.right = 0;
            _modalRoot.style.backgroundColor = _backdropColor;
            _modalRoot.style.justifyContent = Justify.Center;
            _modalRoot.style.alignItems = Align.Center;
            _modalRoot.style.display = DisplayStyle.None; // Hidden until Show() is called

            // 2. Backdrop Click-to-Close Logic
            if (_closeOnBackdropClick)
            {
                _modalRoot.RegisterCallback<ClickEvent>(evt => {
                    // Only close if we clicked the backdrop itself, not the content inside it
                    if (evt.target == _modalRoot) Hide();
                });
            }

            // 3. The Content Container
            var contentContainer = new VisualElement { name = "ModalContentWrapper" };
            // CRITICAL: Stop clicks on the content from bubbling up to the backdrop!
            contentContainer.RegisterCallback<ClickEvent>(evt => evt.StopPropagation());

            // 4. Inject the actual UI
            if (_contentProvider != null)
            {
                contentContainer.Add(_contentProvider.CreateGui(ctx));
            }

            _modalRoot.Add(contentContainer);
            return _modalRoot;
        }

        public void Show() { if (_modalRoot != null) { _modalRoot.style.display = DisplayStyle.Flex; _modalRoot.BringToFront(); } }
        public void Hide() { if (_modalRoot != null) _modalRoot.style.display = DisplayStyle.None; }
        public void Close() { if (_modalRoot?.parent != null) _modalRoot.parent.Remove(_modalRoot); }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) { }
        public void FromUIDocument(string path) { }
    }
}