using System;
using System.Collections.Generic;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheSingularityWorkshop.Forge.GuiBuilders
{
    // The overarching orchestrator for the Forge's entry sequence
    public class ForgeQuestionnaireGuiProvider : IGuiProvider
    {
        private VisualElement _rootContainer;
        private int _currentPhaseIndex = 0;
        private List<PhaseBuilder> _phases;

        public string Title => "throw new NotImplementedException()";

        public VisualElement GetGui()
        {
            _rootContainer = new VisualElement();
            ApplyHolographicVoidStyle(_rootContainer);

            // Defining the phases, their questions, and the immediate world-building consequences
            _phases = new List<PhaseBuilder>
            {
                new PhaseBuilder(
                    "Phase 1: The Dimensional Scope",
                    "Define the boundaries of your reality.",
                    new Dictionary<string, Action>
                    {
                        { "A Single Stage (Arena/City)", () => TriggerConsequence("Localized Grid Proximities Activated. Basic NavMesh tools loaded.") },
                        { "Planetary", () => TriggerConsequence("Atmospheric Shaders & Icosphere Forge Online.") },
                        { "Solar / Interstellar", () => TriggerConsequence("Orbital Mechanics & Cosmic Grid Projected.") },
                        { "Multiversal / Singularity Scale", () => TriggerConsequence("WARNING: Multiversal Causality Tracking requires scaling authorization. Probability Graph Builders initialized.") }
                    }
                ),
                new PhaseBuilder(
                    "Phase 2: The Law of the Land",
                    "How do the entities within this reality function?",
                    new Dictionary<string, Action>
                    {
                        { "Standard Physics / Arcade", () => TriggerConsequence("RigidBody Push & standard FSM behavior tabs loaded.") },
                        { "The Singularity Way (AI Agents)", () => TriggerConsequence("Complex Sensor Arrays, Drone Controllers, and Memory CRUD Editors Manifested.") },
                        { "Universal Roleplaying Logic", () => TriggerConsequence("GURPS Ruleset, Qualification APIs, and Trait Webs Manifested.") }
                    }
                )
                // Phases 3 and 4 (Persistence and Sourcing) drop in seamlessly here
            };

            RenderCurrentPhase();
            return _rootContainer;
        }

        private void RenderCurrentPhase()
        {
            _rootContainer.Clear();

            if (_currentPhaseIndex < _phases.Count)
            {
                // The PhaseBuilder returns the visual element for editing/answering the phase
                var phaseGui = _phases[_currentPhaseIndex].BuildPhaseGui(OnPhaseCompleted);
                _rootContainer.Add(phaseGui);
            }
            else
            {
                RenderFinalManifestation();
            }
        }

        private void OnPhaseCompleted()
        {
            _currentPhaseIndex++;
            RenderCurrentPhase();
        }

        private void TriggerConsequence(string logMessage)
        {
            // In the actual Forge, this hooks into your environmental projection API
            // so the world literally shifts around the user as they click.
            UnityEngine.Debug.Log($"[The Forge Diagnostics] {logMessage}");
        }

        private void RenderFinalManifestation()
        {
            var manifestLabel = new Label("MANIFESTATION COMPLETE.\nTHE FORGE CONSOLES ARE READY.");
            manifestLabel.style.color = new StyleColor(new UnityEngine.Color(0.64f, 0.17f, 0.77f)); // Signature Deep Purple
            manifestLabel.style.fontSize = 24;
            manifestLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            _rootContainer.Add(manifestLabel);
        }

        private void ApplyHolographicVoidStyle(VisualElement element)
        {
            // Setting the baseline aesthetic for the Singularity Workshop tools
            element.style.backgroundColor = new StyleColor(new UnityEngine.Color(0.05f, 0.05f, 0.08f, 0.95f)); // Obsidian void
            element.style.borderBottomColor = new StyleColor(new UnityEngine.Color(0.64f, 0.17f, 0.77f, 0.8f)); // Neon Purple framing
            element.style.borderTopColor = new StyleColor(new UnityEngine.Color(0.64f, 0.17f, 0.77f, 0.8f));
            element.style.borderLeftColor = new StyleColor(new UnityEngine.Color(0.0f, 0.9f, 1.0f, 0.4f)); // Cyan structural accents
            element.style.borderRightColor = new StyleColor(new UnityEngine.Color(0.0f, 0.9f, 1.0f, 0.4f));
            element.style.borderBottomWidth = 2;
            element.style.borderTopWidth = 2;
            element.style.borderLeftWidth = 1;
            element.style.borderRightWidth = 1;
            element.style.paddingBottom = 20;
            element.style.paddingTop = 20;
            element.style.paddingLeft = 20;
            element.style.paddingRight = 20;
        }

        public Action<VisualElement> GetGuiBuilder()
        {
            throw new NotImplementedException();
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            throw new NotImplementedException();
        }

        public void ToUIDocument(string assetPath)
        {
            throw new NotImplementedException();
        }

        public void FromUIDocument(string assetPath)
        {
            throw new NotImplementedException();
        }
    }

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