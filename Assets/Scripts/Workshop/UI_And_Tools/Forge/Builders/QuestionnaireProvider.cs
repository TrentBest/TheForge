using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.UI_And_Tools.Forge.Builders
{
    public class QuestionnaireProvider : IGuiProvider
    {
        public string Title => "GENESIS";

        // --- Data ---
        private string _experienceName = "New Experience";

        // Infrastructure Flags
        private bool _useMultiplayer;
        private bool _useVoice;
        private bool _useAnalytics;

        // Selected Mediums
        private HashSet<string> _selectedMediums = new HashSet<string>();

        // Callback to tell The Forge we are done
        private Action _onComplete;

        // Visual Refs
        private VisualElement _manifestPreview;

        public QuestionnaireProvider() { }

        public QuestionnaireProvider(Action onComplete)
        {
            _onComplete = onComplete;
        }

        public Action<VisualElement> GetGuiBuilder() => null;

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new GraphicalUserInterfaceBuilder("GenesisUI")
                .WithPadding(20)
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch);

            // 1. HEADER
            var header = new Label("INITIALIZE EXPERIENCE");
            header.style.fontSize = 22;
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.style.color = new Color(0, 0.8f, 1f); // Cyan title
            header.style.marginBottom = 15;
            header.style.unityTextAlign = TextAnchor.MiddleCenter;
            root.AddChild(header);

            // 2. IDENTITY
            root.AddChild(new Label("Experience Identity") { style = { fontSize = 10, opacity = 0.6f } });

            var nameField = new TextField { value = _experienceName };
            nameField.style.fontSize = 14;
            nameField.style.unityFontStyleAndWeight = FontStyle.Bold;
            nameField.RegisterValueChangedCallback(e => _experienceName = e.newValue);
            root.AddChild(nameField);

            // 3. INFRASTRUCTURE (Foldout)
            root.AddChild(CreateSeparator());

            var infraFold = new Foldout { text = "Infrastructure & Services (Unity Cloud)" };
            infraFold.value = false; // Collapsed by default
            infraFold.style.marginTop = 10;

            infraFold.Add(CreateServiceToggle("Multiplayer (Netcode)", v => _useMultiplayer = v));
            infraFold.Add(CreateServiceToggle("Vivox Voice Chat", v => _useVoice = v));
            infraFold.Add(CreateServiceToggle("Analytics & Insights", v => _useAnalytics = v));

            // Add a hint that these spawn panels
            var serviceHint = new Label("Selecting services will spawn their respective configuration consoles.")
            { style = { fontSize = 9, opacity = 0.5f, marginTop = 5, whiteSpace = WhiteSpace.Normal } };
            infraFold.Add(serviceHint);

            root.AddChild(infraFold);

            // 4. THE MEDIUM (Robust List)
            root.AddChild(CreateSeparator());
            root.AddChild(new Label("Target Medium") { style = { fontSize = 12, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 5 } });

            var grid = new VisualElement { style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap } };

            // Audio
            grid.Add(CreateMediumToggle("Audio Book"));
            grid.Add(CreateMediumToggle("Radio Drama"));

            // Visual
            grid.Add(CreateMediumToggle("Animated Series"));
            grid.Add(CreateMediumToggle("Short Film"));
            grid.Add(CreateMediumToggle("Graphic Novel"));

            // Interactive
            grid.Add(CreateMediumToggle("Game (FPS)"));
            grid.Add(CreateMediumToggle("Game (RPG)"));
            grid.Add(CreateMediumToggle("Simulation"));

            root.AddChild(grid);

            // 5. MANIFEST PREVIEW (Reactive)
            root.AddChild(CreateSeparator());
            _manifestPreview = new VisualElement { style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap, minHeight = 40 } };
            root.AddChild(_manifestPreview);
            UpdateManifestPreview();

            // 6. APPLY BUTTON
            var applyBtn = new Button(Commit)
            {
                text = "ESTABLISH CONSTRUCT",
                style = {
                    marginTop = 20, height = 45,
                    backgroundColor = new Color(0, 0.6f, 0.2f),
                    color = Color.white,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 14
                }
            };
            root.AddChild(applyBtn);

            return root.Build();
        }

        // --- HELPERS ---

        private Toggle CreateServiceToggle(string label, Action<bool> onSet)
        {
            var t = new Toggle(label);
            t.RegisterValueChangedCallback(e => {
                onSet(e.newValue);
                UpdateManifestPreview(); // Services also add to the manifest
            });
            return t;
        }

        private VisualElement CreateMediumToggle(string name)
        {
            var t = new Toggle(name) { style = { width = 130, marginBottom = 5 } };
            t.RegisterValueChangedCallback(e => {
                if (e.newValue) _selectedMediums.Add(name);
                else _selectedMediums.Remove(name);
                UpdateManifestPreview();
            });
            return t;
        }

        private void UpdateManifestPreview()
        {
            _manifestPreview.Clear();

            if (_selectedMediums.Count == 0 && !_useMultiplayer && !_useVoice)
            {
                _manifestPreview.Add(new Label("Select mediums to see required tools..") { style = { opacity = 0.4f, fontSize = 10 } });
                return;
            }

            _manifestPreview.Add(new Label("QUEUED FORGES: ") { style = { fontSize = 9, opacity = 0.7f, marginRight = 5, alignSelf = Align.Center } });

            // Always standard tools
            AddBadge("Cast & Crew", new Color(0, 0.5f, 1f));
            AddBadge("Timeline", new Color(1f, 0.5f, 0));

            // Dynamic Tools
            if (_useMultiplayer) AddBadge("Netcode", new Color(0.8f, 0, 1f));
            if (_useVoice) AddBadge("Vivox", new Color(0.8f, 0, 1f));

            foreach (var m in _selectedMediums)
            {
                if (m.Contains("Game")) AddBadge("Logic Graph", new Color(0, 0.8f, 0.2f));
                if (m.Contains("Animated") || m.Contains("Film")) AddBadge("Cinematography", new Color(1f, 0, 0.2f));
                if (m.Contains("Audio")) AddBadge("Soundstage", new Color(1f, 1f, 0));
            }
        }

        private void AddBadge(string text, Color color)
        {
            var badge = new Label(text)
            {
                style = {
                    backgroundColor = new Color(color.r, color.g, color.b, 0.2f),
                    borderTopColor = color, borderBottomColor = color, borderLeftColor = color, borderRightColor = color,
                    borderTopWidth = 1, borderBottomWidth = 1, borderLeftWidth = 1, borderRightWidth = 1,
                    borderTopLeftRadius = 4, borderTopRightRadius = 4, borderBottomLeftRadius = 4, borderBottomRightRadius = 4,
                    paddingLeft = 6, paddingRight = 6, paddingTop = 2, paddingBottom = 2,
                    marginRight = 4, marginBottom = 4,
                    fontSize = 9, color = Color.white
                }
            };
            _manifestPreview.Add(badge);
        }

        private void Commit()
        {
            Debug.Log($"[Genesis] Establishing Experience: '{_experienceName}'");
            Debug.Log($"[Genesis] Provisioning Services: Multi:{_useMultiplayer}, Voice:{_useVoice}");
            Debug.Log($"[Genesis] Target Mediums: {string.Join(", ", _selectedMediums)}");

            // 1. Use the static Active property instead of searching the Unity Scene
            var context = ExperienceContext.Active;

            // 2. Explicitly check for null since it is no longer a Unity Object
            if (context != null && context.GetCurrentExperience() != null)
            {
                context.GetCurrentExperience().Name = _experienceName;
                // In a real implementation, we would inject the selected services into the Experience config here
            }

            _onComplete?.Invoke();
        }

        private VisualElement CreateSeparator()
        {
            var sep = new VisualElement();
            sep.style.height = 1;
            sep.style.backgroundColor = new Color(1, 1, 1, 0.1f);
            sep.style.marginTop = 10;
            sep.style.marginBottom = 10;
            return sep;
        }

        public void ToUIDocument(string assetPath)
        {
            
        }

        public void FromUIDocument(string assetPath)
        {
           
        }
    }
}