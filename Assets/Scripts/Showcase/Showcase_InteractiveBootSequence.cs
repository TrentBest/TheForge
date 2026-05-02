using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.UI_And_Tools.Showcase
{
    using global::Workshop.UI_And_Tools.Showcase.Workshop.UI_And_Tools.Showcase;
    using System;
    using System.Collections.Generic;


    public class Showcase_InteractiveBootSequence : IGuiProvider
    {
        public string Title => "BOOT SEQUENCE";
        public IGuiProvider CurrentGuiProvider { get; internal set; }

        private Action _onSequenceComplete;
        private GuiContext _context;
        private BootSequenceManifest _manifest; // The loaded data

        // --- Sequence State Machine ---
        private enum BootPhase
        {
            WaitingForBreach,
            TerminalUplink,
            GlobeTargeting,
            MissileLaunch,
            Deflection,
            TransitionToLobby
        }

        private BootPhase _currentPhase = BootPhase.WaitingForBreach;
        private float _phaseTimer = 0f;

        // --- UI References ---
        private VisualElement _buttonPhaseContainer;
        private VisualElement _cinematicContainer;
        private Label _terminalText;
        private VisualElement _lmpbContainer;

        // H.E.R.M.I.T. Script State
        private Queue<string> _terminalScript = new Queue<string>();
        private float _textCharTimer = 0f;
        private string _currentLineTarget = "";
        private string _currentLineDisplay = "";

        public Showcase_InteractiveBootSequence(Action onSequenceComplete)
        {
            _onSequenceComplete = onSequenceComplete;
            LoadManifest();
        }

        private void LoadManifest()
        {
            // Load the JSON directly from the Resources folder
            TextAsset manifestAsset = Resources.Load<TextAsset>("Manifests/ShowcaseBoot");

            if (manifestAsset != null)
            {
                // Hydrate our data model
                _manifest = JsonUtility.FromJson<BootSequenceManifest>(manifestAsset.text);

                // Queue the lines dynamically
                foreach (var line in _manifest.TerminalLines)
                {
                    _terminalScript.Enqueue(line);
                }

                Debug.Log($"[Showcase] Boot Manifest '{_manifest.SequenceID}' loaded successfully.");
            }
            else
            {
                Debug.LogError("[Showcase] CRITICAL: Failed to load Boot Manifest! Falling back to defaults.");
                // Fallback safeguards could go here
            }
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _context = ctx;

            var rootBuilder = new GraphicalUserInterfaceBuilder("BootSequenceRoot")
                .WithFlexGrow(1)
                .WithBackgroundColor(Color.black);

            // ==========================================
            // PHASE 1: THE BUTTON ROOM (Driven by Manifest)
            // ==========================================
            var buttonPhase = new GraphicalUserInterfaceBuilder("ButtonPhase")
                .WithFlexGrow(1)
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)
                .WithBackgroundColor(new Color(0.04f, 0.04f, 0.05f))
                .OnBuild(ve => _buttonPhaseContainer = ve);

            buttonPhase.AddChild(new ForgeLabelBuilder(_manifest.WarningTitle)
                .WithColor(Color.yellow).WithFontSize(48).WithFontStyle(FontStyle.Bold).WithMarginBottom(20));

            buttonPhase.AddChild(new ForgeLabelBuilder(_manifest.WarningMessage)
                .WithColor(new Color(1.0f, 0.3f, 0.3f)).WithFontSize(24).WithFontStyle(FontStyle.Bold).WithMarginBottom(15));

            buttonPhase.AddChild(new ForgeButtonBuilder(_manifest.ButtonText)
                .WithBackgroundColor(new Color(0.7f, 0.05f, 0.05f))
                .WithColor(Color.white)
                .WithFontSize(32).WithFontStyle(FontStyle.Bold)
                .WithHeight(120).WithWidth(400).WithBorderRadius(60)
                .OnClick(InitiateBreach));

            rootBuilder.AddChild(buttonPhase);

            // ==========================================
            // PHASE 2+: CINEMATIC CANVAS
            // ==========================================
            var cinematicPhase = new GraphicalUserInterfaceBuilder("CinematicPhase")
                .WithFlexGrow(1)
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                .OnBuild(ve =>
                {
                    _cinematicContainer = ve;
                    ve.style.display = DisplayStyle.None;
                });

            cinematicPhase.AddChild(new GraphicalUserInterfaceBuilder("TerminalArea")
                .WithHeight(150).WithPadding(20)
                .AddChild(new ForgeLabelBuilder(">_")
                    .WithColor(Color.green).WithFontSize(18).WithFontStyle(FontStyle.Bold)
                    .OnBuild(ve => _terminalText = ve as Label)));

            cinematicPhase.AddChild(new GraphicalUserInterfaceBuilder("GlobeArea")
                .WithFlexGrow(1)
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)
                .OnBuild(ve => _lmpbContainer = ve));

            rootBuilder.AddChild(cinematicPhase);

            var root = rootBuilder.Build();

            // ==========================================
            // THE CONDUCTOR TICK
            // ==========================================
            root.schedule.Execute(() =>
            {
                float dt = Time.deltaTime;
                _phaseTimer += dt;

                switch (_currentPhase)
                {
                    case BootPhase.WaitingForBreach: break;
                    case BootPhase.TerminalUplink: ProcessTerminalUplink(dt); break;
                    case BootPhase.GlobeTargeting: break;
                    case BootPhase.MissileLaunch: break;
                    case BootPhase.Deflection: break;
                    case BootPhase.TransitionToLobby: _onSequenceComplete?.Invoke(); break;
                }
            }).Every(16);

            return root;
        }

        private void InitiateBreach()
        {
            _buttonPhaseContainer.style.display = DisplayStyle.None;
            _cinematicContainer.style.display = DisplayStyle.Flex;

            _currentPhase = BootPhase.TerminalUplink;
            _phaseTimer = 0f;

            if (_terminalScript.Count > 0) _currentLineTarget = _terminalScript.Dequeue();
        }

        private void ProcessTerminalUplink(float dt)
        {
            _textCharTimer += dt;

            // Use manifest data for speed
            if (_textCharTimer > _manifest.TypewriterSpeed)
            {
                _textCharTimer = 0f;
                if (_currentLineDisplay.Length < _currentLineTarget.Length)
                {
                    _currentLineDisplay += _currentLineTarget[_currentLineDisplay.Length];
                    _terminalText.text = ">_ " + _currentLineDisplay;
                }
                else
                {
                    // Use manifest data for delay
                    if (_phaseTimer > _manifest.LineDelay)
                    {
                        if (_terminalScript.Count > 0)
                        {
                            _currentLineTarget = _terminalScript.Dequeue();
                            _currentLineDisplay = "";
                            _phaseTimer = 0f;
                        }
                        else
                        {
                            _currentPhase = BootPhase.GlobeTargeting;
                            _phaseTimer = 0f;
                        }
                    }
                }
            }
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}