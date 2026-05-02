using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    // Modular builder for individual questioning phases
    public class PhaseBuilder
    {
        private string _phaseTitle;
        private string _phaseQuestion;
        private Dictionary<string, Action> _options;

        public PhaseBuilder(string title, string question, Dictionary<string, Action> options)
        {
            _phaseTitle = title;
            _phaseQuestion = question;
            _options = options;
        }

        // Returns the GUI which allows editing/answering of the phase
        public VisualElement BuildPhaseGui(Action onComplete)
        {
            var phaseContainer = new VisualElement();
            phaseContainer.style.flexDirection = FlexDirection.Column;
            phaseContainer.style.alignItems = Align.Center;

            // Holographic Title
            var titleLabel = new Label(_phaseTitle);
            titleLabel.style.color = new StyleColor(new UnityEngine.Color(0.8f, 0.5f, 1.0f)); // Soft purple luminescence
            titleLabel.style.fontSize = 28;
            titleLabel.style.textShadow = new TextShadow { color = UnityEngine.Color.magenta, blurRadius = 8 };
            phaseContainer.Add(titleLabel);

            // The Inquiry
            var questionLabel = new Label(_phaseQuestion);
            questionLabel.style.color = new StyleColor(UnityEngine.Color.white);
            questionLabel.style.fontSize = 18;
            questionLabel.style.marginTop = 15;
            questionLabel.style.marginBottom = 30;
            phaseContainer.Add(questionLabel);

            // Dynamic Options Grid
            var optionsContainer = new VisualElement();
            optionsContainer.style.flexDirection = FlexDirection.Row;
            optionsContainer.style.flexWrap = Wrap.Wrap;
            optionsContainer.style.justifyContent = Justify.Center;

            foreach (var option in _options)
            {
                var btn = CreateHolographicButton(option.Key, () =>
                {
                    option.Value.Invoke(); // Trigger the environmental consequence
                    onComplete?.Invoke();  // Move to the next phase
                });
                optionsContainer.Add(btn);
            }

            phaseContainer.Add(optionsContainer);
            return phaseContainer;
        }

        private Button CreateHolographicButton(string text, Action onClick)
        {
            var btn = new Button(onClick);
            btn.text = text;

            // Unselected Holographic State
            btn.style.backgroundColor = new StyleColor(new UnityEngine.Color(0.1f, 0.05f, 0.15f, 0.8f)); // Deep purple tint
            btn.style.color = new StyleColor(new UnityEngine.Color(0.0f, 0.9f, 1.0f)); // Cyan text
            var signaturePurple = new StyleColor(new UnityEngine.Color(0.64f, 0.17f, 0.77f)); // Purple border
            btn.style.borderTopColor = signaturePurple;
            btn.style.borderBottomColor = signaturePurple;
            btn.style.borderLeftColor = signaturePurple;
            btn.style.borderRightColor = signaturePurple;
            btn.style.borderBottomWidth = 1;
            btn.style.borderTopWidth = 1;
            btn.style.borderLeftWidth = 1;
            btn.style.borderRightWidth = 1;
            btn.style.paddingBottom = 15;
            btn.style.paddingTop = 15;
            btn.style.paddingLeft = 25;
            btn.style.paddingRight = 25;
            btn.style.marginTop = 10;
            btn.style.marginBottom = 10;
            btn.style.marginLeft = 15;
            btn.style.marginRight = 15;
            btn.style.fontSize = 14;

            // Simulating interactive hover states for that tactile in-world feel
            btn.RegisterCallback<MouseEnterEvent>(e =>
            {
                btn.style.backgroundColor = new StyleColor(new UnityEngine.Color(0.64f, 0.17f, 0.77f, 0.6f)); // Purple flare
                btn.style.color = new StyleColor(UnityEngine.Color.white);
            });
            btn.RegisterCallback<MouseLeaveEvent>(e =>
            {
                btn.style.backgroundColor = new StyleColor(new UnityEngine.Color(0.1f, 0.05f, 0.15f, 0.8f));
                btn.style.color = new StyleColor(new UnityEngine.Color(0.0f, 0.9f, 1.0f));
            });

            return btn;
        }
    }
}