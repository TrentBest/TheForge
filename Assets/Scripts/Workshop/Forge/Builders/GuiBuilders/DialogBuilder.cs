using System;
using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders
{
    public class DialogBuilder : IGuiProvider
    {
        public string Title { get; private set; } = "NOTIFICATION";

        private string _message;
        private Action<VisualElement> _customContentInjector;
        private Action<VisualElement, Action> _buttonInjector;
        private Color _accentColor = Color.cyan;

        // Internal state to handle closing
        private VisualElement _rootModalElement;

        public DialogBuilder(string title)
        {
            Title = title;
        }

        public DialogBuilder WithMessage(string message)
        {
            _message = message;
            return this;
        }

        public DialogBuilder AsError()
        {
            _accentColor = new Color(0.9f, 0.2f, 0.2f); // Red
            return this;
        }

        public DialogBuilder AsWarning()
        {
            _accentColor = new Color(0.9f, 0.7f, 0.1f); // Yellow
            return this;
        }

        // --- CUSTOM CONTENT INJECTORS ---

        public DialogBuilder WithProgressBar(float progress, string label = "Calculating...")
        {
            _customContentInjector = (container) => {
                container.Add(new Label(label) { style = { color = Color.white, marginBottom = 5 } });

                var track = new VisualElement { style = { height = 10, backgroundColor = new Color(0.1f, 0.1f, 0.1f), borderBottomRightRadius = 5, borderTopRightRadius = 5, borderBottomLeftRadius = 5, borderTopLeftRadius = 5, overflow = Overflow.Hidden } };
                var fill = new VisualElement { style = { height = 10, width = Length.Percent(progress * 100f), backgroundColor = _accentColor } };

                track.Add(fill);
                container.Add(track);
            };
            return this;
        }

        public DialogBuilder WithSpinner(string label = "Please Wait...")
        {
            _customContentInjector = (container) => {
                container.Add(new Label(label) { style = { color = Color.gray, alignSelf = Align.Center, marginTop = 10, marginBottom = 10, unityFontStyleAndWeight = FontStyle.Italic } });
                // Note: You could add an animated element here using UIToolkit's schedule.Execute block
            };
            return this;
        }

        // --- RESOLUTION BUTTONS ---

        public DialogBuilder WithOk(Action onOk = null)
        {
            _buttonInjector = (container, closeAction) => {
                container.Add(CreateButton("OK", _accentColor, () => { onOk?.Invoke(); closeAction(); }));
            };
            return this;
        }

        public DialogBuilder WithYesNo(Action onYes, Action onNo = null)
        {
            _buttonInjector = (container, closeAction) => {
                var row = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween } };
                row.Add(CreateButton("NO", Color.gray, () => { onNo?.Invoke(); closeAction(); }));
                row.Add(CreateButton("YES", _accentColor, () => { onYes?.Invoke(); closeAction(); }));
                container.Add(row);
            };
            return this;
        }

        // --- IGuiProvider Implementation ---

        public VisualElement CreateGui(GuiContext ctx)
        {
            // 1. The Full-Screen Dark Overlay (Blocks clicks to UI underneath)
            _rootModalElement = new VisualElement
            {
                style = {
                    position = Position.Absolute, top = 0, bottom = 0, left = 0, right = 0,
                    backgroundColor = new Color(0, 0, 0, 0.7f),
                    justifyContent = Justify.Center, alignItems = Align.Center
                }
            };

            // Capture clicks on the background to prevent them from passing through, 
            // but we won't auto-close unless explicitly designed to.
            _rootModalElement.RegisterCallback<PointerDownEvent>(e => e.StopPropagation());

            // 2. The Dialog Box
            Action closeDialog = () => {
                if (_rootModalElement.parent != null) _rootModalElement.parent.Remove(_rootModalElement);
            };

            var dialogBox = new GraphicalUserInterfaceBuilder("DialogBox")
                .WithBackgroundColor(new Color(0.12f, 0.12f, 0.14f))
                .WithPadding(20).WithWidth(400)
                .WithBorderTopWidth(3).WithBorderTopColor(_accentColor) // Accent stripe

                // Title
                .AddChild(new Label(Title.ToUpper()) { style = { color = Color.white, fontSize = 16, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } })

                // Message
                .OnBuild(ve => {
                    if (!string.IsNullOrEmpty(_message))
                    {
                        ve.Add(new Label(_message) { style = { color = Color.gray, whiteSpace = WhiteSpace.Normal, marginBottom = 15 } });
                    }

                    // Inject custom payload (progress bar, spinners, etc)
                    _customContentInjector?.Invoke(ve);
                })

                // Footer Buttons
                .OnBuild(ve => {
                    if (_buttonInjector != null)
                    {
                        var footer = new VisualElement { style = { marginTop = 20, paddingTop = 15, borderTopWidth = 1, borderTopColor = new Color(0.2f, 0.2f, 0.2f) } };
                        _buttonInjector.Invoke(footer, closeDialog);
                        ve.Add(footer);
                    }
                })
                .Build();

            _rootModalElement.Add(dialogBox);
            return _rootModalElement;
        }

        private Button CreateButton(string text, Color color, Action onClick)
        {
            var btn = new Button(onClick) { text = text, style = { backgroundColor = new Color(0.2f, 0.2f, 0.2f), color = color, paddingBottom = 8, paddingTop = 8, paddingLeft = 20, paddingRight = 20, unityFontStyleAndWeight = FontStyle.Bold, borderBottomColor = Color.clear, borderTopColor = Color.clear, borderLeftColor = Color.clear, borderRightColor = Color.clear } };

            // Simple hover effect
            btn.RegisterCallback<PointerEnterEvent>(e => btn.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f));
            btn.RegisterCallback<PointerLeaveEvent>(e => btn.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f));

            return btn;
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}