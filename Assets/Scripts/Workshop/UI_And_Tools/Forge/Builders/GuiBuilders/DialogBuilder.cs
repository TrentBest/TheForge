using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    /// <summary>
    /// A high-level modal dialog orchestrator that leverages the Forge's standard UI builders.
    /// </summary>
    public class DialogBuilder : IGuiProvider
    {
        public string Title { get; private set; } = "NOTIFICATION";

        private string _message;
        private Action<GraphicalUserInterfaceBuilder> _customContentInjector;
        private Action<GraphicalUserInterfaceBuilder, Action> _buttonInjector;
        private Color _accentColor = Color.cyan;
        private VisualElement _rootModalElement;

        public DialogBuilder() { }

        public DialogBuilder(string title)
        {
            Title = title;
        }

        public DialogBuilder WithTitle(string title)
        {
            Title = title;
            return this;
        }

        public DialogBuilder WithMessage(string message)
        {
            _message = message;
            return this;
        }

        /// <summary>
        /// Injects a standard IGuiProvider into the dialog's content area.
        /// </summary>
        public DialogBuilder WithContent(IGuiProvider provider)
        {
            _customContentInjector = (container) => container.AddChild(provider);
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

        public DialogBuilder WithProgressBar(float progress, string label = "Calculating...")
        {
            _customContentInjector = (container) => {
                container.AddChild(new ForgeLabelBuilder(label)
                    .WithColor(Color.white)
                    .WithMargin(0, 5, 0, 0));

                container.AddChild(new GraphicalUserInterfaceBuilder("ProgressBarTrack")
                    .WithHeight(10)
                    .WithBackgroundColor(new Color(0.1f, 0.1f, 0.1f))
                    .WithBorderRadius(5)
                    .OnBuild(track => {
                        var fill = new VisualElement
                        {
                            style = {
                                height = 10,
                                width = Length.Percent(progress * 100f),
                                backgroundColor = _accentColor,
                                borderTopLeftRadius = 5,
                                borderBottomLeftRadius = 5
                            }
                        };
                        track.Add(fill);
                    }));
            };
            return this;
        }

        public DialogBuilder WithButton(string text, Action onClick)
        {
            _buttonInjector = (container, closeAction) => {
                container.AddChild(CreateForgeButton(text, _accentColor, () => { onClick?.Invoke(); closeAction(); }));
            };
            return this;
        }

        public DialogBuilder WithOk(Action onOk = null)
        {
            _buttonInjector = (container, closeAction) => {
                container.AddChild(CreateForgeButton("OK", _accentColor, () => { onOk?.Invoke(); closeAction(); }));
            };
            return this;
        }

        public DialogBuilder WithYesNo(Action onYes, Action onNo = null)
        {
            _buttonInjector = (container, closeAction) => {
                container.AddChild(new GraphicalUserInterfaceBuilder("ButtonRow")
                    .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween)
                    .AddChild(CreateForgeButton("NO", Color.gray, () => { onNo?.Invoke(); closeAction(); }))
                    .AddChild(CreateForgeButton("YES", _accentColor, () => { onYes?.Invoke(); closeAction(); }))
                );
            };
            return this;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            // 1. Create the full-screen blocking overlay via the GuiBuilder
            var overlayBuilder = new GraphicalUserInterfaceBuilder("ModalOverlay")
                .WithPosition(Position.Absolute)
                .WithWidth(Length.Percent(100))
                .WithHeight(Length.Percent(100))
                .WithBackgroundColor(new Color(0, 0, 0, 0.7f))
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center);

            _rootModalElement = overlayBuilder.Build();
            _rootModalElement.RegisterCallback<PointerDownEvent>(e => e.StopPropagation());

            Action closeDialog = () => {
                if (_rootModalElement.parent != null) _rootModalElement.parent.Remove(_rootModalElement);
            };

            // 2. Build the main dialog window
            var dialogBox = new GraphicalUserInterfaceBuilder("DialogWindow")
                .WithBackgroundColor(new Color(0.12f, 0.12f, 0.14f))
                .WithPadding(20)
                .WithWidth(450)
                .WithBorderTopWidth(3)
                .WithBorderTopColor(_accentColor);

            // Header Title
            dialogBox.AddChild(new ForgeLabelBuilder(Title.ToUpper())
                .WithColor(Color.white)
                .WithFontSize(16)
                .WithFontStyle(FontStyle.Bold)
                .WithMargin(0, 10, 0, 0));

            // Message Body
            if (!string.IsNullOrEmpty(_message))
            {
                dialogBox.AddChild(new ForgeLabelBuilder(_message)
                    .WithColor(Color.gray)
                    .WithWhiteSpace(WhiteSpace.Normal)
                    .WithMargin(0, 15, 0, 0));
            }

            // Custom Content Injection (Deep Dive technical data)
            _customContentInjector?.Invoke(dialogBox);

            // Footer Buttons
            if (_buttonInjector != null)
            {
                var footer = new GraphicalUserInterfaceBuilder("DialogFooter")
                    .WithMargin(0)
                    .WithMarginTop(20)
                    .WithPadding(0)
                    .WithPaddingTop(15)
                    .WithBorderTopWidth(1)
                    .WithBorderTopColor(new Color(0.2f, 0.2f, 0.2f));

                _buttonInjector.Invoke(footer, closeDialog);
                dialogBox.AddChild(footer);
            }

            _rootModalElement.Add(dialogBox.Build());
            return _rootModalElement;
        }

        private IGuiProvider CreateForgeButton(string text, Color accent, Action onClick)
        {
            return new ForgeButtonBuilder(text, onClick)
                .WithBackgroundColor(new Color(0.2f, 0.2f, 0.2f))
                .WithTextColor(accent)
                .WithHeight(36)
                .WithFontStyle(FontStyle.Bold);
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}