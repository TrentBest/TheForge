using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.MastersOfOrionII
{
    public class MastersOfOrionII_Gui_RaceDesigner : IGuiProvider
    {
        public string Title => "GENETIC SEQUENCER";

        private VisualElement _root;
        private VisualElement _mainContent;
        private VisualElement _portraitView;
        private GuiContext _lastCtx;

        // --- STATE ---
        private int _dnaPoints = 150;
        private int _maxPoints = 150;
        private RaceData _currentRace;
        private List<string> _evolutionaryHistory = new List<string>();

        // --- PRESETS ---
        private List<RaceData> _presets;

        // --- ROUTING ACTIONS (Public for Reflection/Injection) ---
        public Action OnSaveAndLaunchClicked { get; set; }
        public Action OnBackClicked { get; set; }

        // Primary Default Constructor - Required for Reflection
        public MastersOfOrionII_Gui_RaceDesigner()
        {
            InitializeData();
        }

        // Convenience Constructor for Manual Routing Config
        public MastersOfOrionII_Gui_RaceDesigner(Action onSaveAndLaunchClicked, Action onBackClicked)
        {
            OnSaveAndLaunchClicked = onSaveAndLaunchClicked;
            OnBackClicked = onBackClicked;
            InitializeData();
        }

        public MastersOfOrionII_Gui_RaceDesigner(MastersOfOrionII_Gui_GameBuilder mastersOfOrionII_Gui_GameBuilder)
        {
        }

        private void InitializeData()
        {
            _presets = GeneratePresets();
            _currentRace = new RaceData { Name = "New Species", Description = "A blank slate awaiting evolutionary direction." };
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            var builder = new GraphicalUserInterfaceBuilder("RaceDesigner_Root")
                .WithPercentSize(100, 100)
                .WithBackgroundColor(new Color(0.02f, 0.02f, 0.05f))
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch);

            // --- 1. LEFT COLUMN: THE SPECIMEN TANK ---
            builder.AddChild(c =>
            {
                var tank = new GraphicalUserInterfaceBuilder("SpecimenTank")
                    .WithBackgroundColor(new Color(0, 0, 0, 0.5f))
                    .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)
                    .Build();

                tank.style.width = Length.Percent(35);
                tank.style.borderRightWidth = 2;
                tank.style.borderRightColor = Color.cyan;

                _portraitView = new GraphicalUserInterfaceBuilder("PortraitView")
                    .WithBackgroundColor(new Color(0.1f, 0.1f, 0.1f))
                    .Build();

                _portraitView.style.width = 250;
                _portraitView.style.height = 350;
                _portraitView.style.borderTopLeftRadius = 10; _portraitView.style.borderTopRightRadius = 10;
                _portraitView.style.borderBottomLeftRadius = 10; _portraitView.style.borderBottomRightRadius = 10;
                _portraitView.style.borderLeftWidth = 1; _portraitView.style.borderRightWidth = 1;
                _portraitView.style.borderTopWidth = 1; _portraitView.style.borderBottomWidth = 1;
                _portraitView.style.borderLeftColor = Color.white; _portraitView.style.borderRightColor = Color.white;
                _portraitView.style.borderTopColor = Color.white; _portraitView.style.borderBottomColor = Color.white;

                tank.Add(_portraitView);

                var nameField = new TextField { value = _currentRace.Name, style = { marginTop = 20, width = 250, fontSize = 18, unityTextAlign = TextAnchor.MiddleCenter } };
                nameField.RegisterValueChangedCallback(e => _currentRace.Name = e.newValue);
                tank.Add(nameField);

                tank.Add(CreateDNAMeter());

                tank.Add(new GraphicalUserInterfaceBuilder("BackBtnContainer")
                    .WithMarginTop(20)
                    .AddButton("<< RETURN TO SELECTION", () => OnBackClicked?.Invoke())
                    .Build());

                return tank;
            });

            // --- 2. RIGHT COLUMN: THE SEQUENCER ---
            builder.AddChild(c =>
            {
                var controls = new GraphicalUserInterfaceBuilder("SequencerControls")
                    .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                    .WithPadding(20)
                    .Build();
                controls.style.flexGrow = 1;

                controls.Add(new Label("EVOLUTIONARY PATHWAY") { style = { fontSize = 24, color = Color.green, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });

                var tabs = new GraphicalUserInterfaceBuilder("TabsContainer")
                    .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Center)
                    .Build();
                tabs.style.marginBottom = 20;
                tabs.style.borderBottomWidth = 1;
                tabs.style.borderBottomColor = Color.gray;

                tabs.Add(CreateTab("ARCHETYPE", () => RenderArchetypeSelector()));
                tabs.Add(CreateTab("BIOLOGY", () => RenderBiologyEditor()));
                tabs.Add(CreateTab("SOCIOLOGY", () => RenderSociologyEditor()));
                controls.Add(tabs);

                _mainContent = new GraphicalUserInterfaceBuilder("MainContentFrame").Build();
                _mainContent.style.flexGrow = 1;
                controls.Add(_mainContent);

                var footer = new GraphicalUserInterfaceBuilder("FooterControls")
                    .WithFlexLayout(FlexDirection.Row, Justify.FlexEnd, Align.Center)
                    .WithMarginTop(20)
                    .AddButton("LOAD PRESET", () => LoadPresetMenu())
                    .AddButton("FINALIZE GENOME", () => CommitRace())
                    .Build();

                controls.Add(footer);

                RenderArchetypeSelector();
                return controls;
            });

            _root = builder.Build();
            UpdatePortrait();
            return _root;
        }

        private void RenderArchetypeSelector()
        {
            _mainContent.Clear();
            _mainContent.Add(new Label("SELECT METABOLIC BASE") { style = { color = Color.gray, marginBottom = 10 } });

            var grid = new GraphicalUserInterfaceBuilder("ArchetypeGrid")
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.FlexStart)
                .Build();
            grid.style.flexWrap = Wrap.Wrap;

            grid.Add(CreateArchetypeCard("CARBON BASED", "Adaptable. The galactic standard.", 0, () => SetArchetype(RaceArchetype.Carbon)));
            grid.Add(CreateArchetypeCard("SILICON BASED", "Consumes minerals for food. Immune to radiation. Slow reproduction.", 40, () => SetArchetype(RaceArchetype.Silicon)));
            grid.Add(CreateArchetypeCard("SYNTHETIC", "Immortal leaders. Does not breathe. Requires Industrial maintenance.", 80, () => SetArchetype(RaceArchetype.Synthetic)));
            grid.Add(CreateArchetypeCard("ENERGY BEING", "Incorporeal. Ships require no fuel but massive shielding. Low population cap.", 100, () => SetArchetype(RaceArchetype.Energy)));

            _mainContent.Add(grid);
        }

        private VisualElement CreateArchetypeCard(string title, string desc, int cost, Action onSelect)
        {
            bool isSelected = _currentRace.Archetype.ToString().ToUpper() == title.Split(' ')[0];

            var card = new GraphicalUserInterfaceBuilder($"Card_{title.Replace(" ", "")}")
                .WithBackgroundColor(isSelected ? new Color(0, 0.3f, 0.5f) : new Color(0.1f, 0.1f, 0.15f))
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.FlexStart)
                .WithPadding(10)
                .Build();

            card.style.width = Length.Percent(48);
            card.style.height = 100;
            card.style.marginBottom = 10;
            card.style.marginRight = 5;
            card.style.borderLeftColor = isSelected ? Color.cyan : Color.gray;
            card.style.borderLeftWidth = 4;

            // Emulate Button Click
            card.RegisterCallback<ClickEvent>(e => onSelect?.Invoke());

            card.Add(new Label(title) { style = { color = Color.white, unityFontStyleAndWeight = FontStyle.Bold, fontSize = 14 } });
            card.Add(new Label(desc) { style = { color = new Color(1, 1, 1, 0.7f), fontSize = 10, whiteSpace = WhiteSpace.Normal } });
            card.Add(new Label($"Cost: {cost} DNA") { style = { color = cost > _maxPoints ? Color.red : Color.yellow, fontSize = 10, alignSelf = Align.FlexEnd } });

            return card;
        }

        private void RenderBiologyEditor()
        {
            _mainContent.Clear();
            _mainContent.Add(new Label("PHYSICAL ADAPTATIONS") { style = { color = Color.gray, marginBottom = 10 } });

            var scroll = new ScrollView();

            scroll.Add(new Label("ENVIRONMENTAL") { style = { color = Color.cyan, marginTop = 10 } });

            if (_currentRace.Archetype == RaceArchetype.Carbon || _currentRace.Archetype == RaceArchetype.Silicon)
                scroll.Add(CreateTraitToggle("Aquatic", "Ocean worlds are ideal. Desert worlds are uninhabitable.", 10, "IsAquatic"));
            else
                scroll.Add(CreateGhostTrait("Aquatic", "Requires Biological Body"));

            if (_currentRace.Archetype == RaceArchetype.Synthetic || _currentRace.Archetype == RaceArchetype.Energy)
                scroll.Add(CreateStandardTrait("Vacuum Native", "Does not require atmosphere.", 0));
            else
                scroll.Add(CreateTraitToggle("Vacuum Tolerant", "Can survive low atmosphere.", 30, "VacuumTolerant"));

            scroll.Add(new Label("MORPHOLOGY") { style = { color = Color.cyan, marginTop = 15 } });
            scroll.Add(CreateTraitToggle("Cybernetic Implants", "Increases worker efficiency +20%.", 40, "Cybernetic"));
            scroll.Add(CreateTraitToggle("Hive Mind", "No morale penalties. No independent thought.", 60, "HiveMind"));
            scroll.Add(CreateTraitToggle("Four Arms", "Ground combat +50%.", 20, "FourArms"));

            _mainContent.Add(scroll);
        }

        private void RenderSociologyEditor()
        {
            _mainContent.Clear();
            _mainContent.Add(new Label("SOCIETAL DOCTRINE") { style = { color = Color.gray, marginBottom = 10 } });

            bool isHive = _currentRace.Traits.Contains("HiveMind");
            var scroll = new ScrollView();

            scroll.Add(new Label("GOVERNMENT TYPE") { style = { color = Color.cyan } });

            if (isHive)
            {
                scroll.Add(CreateGhostTrait("Representative Democracy", "Incompatible with Hive Mind Biology"));
                scroll.Add(CreateStandardTrait("Gestalt Consciousness", "The only path for the swarm.", 0));
            }
            else
            {
                scroll.Add(CreateTraitToggle("Representative Democracy", "Morale +10%. Slower decisions.", 10, "Democracy"));
                scroll.Add(CreateTraitToggle("Military Dictatorship", "Ship building +20%. Lower Morale.", 10, "Dictatorship"));
                scroll.Add(CreateTraitToggle("Technocracy", "Research +20%.", 20, "Technocracy"));
            }

            scroll.Add(new Label("HISTORICAL ORIGIN") { style = { color = Color.cyan, marginTop = 15 } });
            scroll.Add(CreateTraitToggle("War Torn", "Start with veteran ships. Start with fewer population.", -10, "WarTorn"));
            scroll.Add(CreateTraitToggle("Post-Scarcity", "Start with extra credits. High maintenance costs.", 30, "RichStart"));

            _mainContent.Add(scroll);
        }

        private void SetArchetype(RaceArchetype type)
        {
            _currentRace.Archetype = type;
            int cost = type switch { RaceArchetype.Carbon => 0, RaceArchetype.Silicon => 40, RaceArchetype.Synthetic => 80, RaceArchetype.Energy => 100, _ => 0 };
            _dnaPoints = _maxPoints - cost;
            _currentRace.Traits.Clear();
            UpdatePortrait();
            RenderArchetypeSelector();
        }

        private void ToggleTrait(string id, int cost)
        {
            if (_currentRace.Traits.Contains(id))
            {
                _currentRace.Traits.Remove(id);
                _dnaPoints += cost;
            }
            else
            {
                if (_dnaPoints >= cost)
                {
                    _currentRace.Traits.Add(id);
                    _dnaPoints -= cost;
                }
            }
            UpdatePortrait();
        }

        private void UpdatePortrait()
        {
            _portraitView.Clear();

            var face = new GraphicalUserInterfaceBuilder("DynamicFace")
                .WithBackgroundColor(GetSkinColor())
                .Build();

            face.style.width = 100;
            face.style.height = 100;
            face.style.alignSelf = Align.Center;
            face.style.top = 50;
            face.style.borderBottomLeftRadius = 50; face.style.borderBottomRightRadius = 50;
            face.style.borderTopLeftRadius = 50; face.style.borderTopRightRadius = 50;

            int eyeCount = _currentRace.Traits.Contains("FourArms") ? 4 : 2;
            if (_currentRace.Archetype == RaceArchetype.Silicon) eyeCount = 1;

            for (int i = 0; i < eyeCount; i++)
            {
                var eye = new GraphicalUserInterfaceBuilder($"Eye_{i}").WithBackgroundColor(Color.black).Build();
                eye.style.width = 20; eye.style.height = 20;
                eye.style.position = Position.Absolute;
                eye.style.left = 20 + (i * 20);
                eye.style.top = 30;
                face.Add(eye);
            }

            if (_currentRace.Traits.Contains("Cybernetic"))
            {
                var borgEye = new GraphicalUserInterfaceBuilder("BorgEye").WithBackgroundColor(Color.clear).Build();
                borgEye.style.width = 40; borgEye.style.height = 40;
                borgEye.style.borderTopColor = Color.red; borgEye.style.borderRightColor = Color.red;
                borgEye.style.borderBottomColor = Color.red; borgEye.style.borderLeftColor = Color.red;
                borgEye.style.borderTopWidth = 2; borgEye.style.borderRightWidth = 2;
                borgEye.style.borderBottomWidth = 2; borgEye.style.borderLeftWidth = 2;
                borgEye.style.position = Position.Absolute;
                borgEye.style.left = 60; borgEye.style.top = 10;
                face.Add(borgEye);
            }

            if (_currentRace.Traits.Contains("IsAquatic"))
            {
                face.style.borderBottomLeftRadius = 0; face.style.borderBottomRightRadius = 0;
            }

            _portraitView.Add(face);
            _portraitView.Add(new Label(_currentRace.Archetype.ToString().ToUpper()) { style = { alignSelf = Align.Center, marginTop = 120, color = Color.gray } });
        }

        private Color GetSkinColor()
        {
            return _currentRace.Archetype switch
            {
                RaceArchetype.Carbon => new Color(0.8f, 0.6f, 0.4f),
                RaceArchetype.Silicon => new Color(0.5f, 0.5f, 0.5f),
                RaceArchetype.Synthetic => new Color(0.8f, 0.8f, 0.9f),
                RaceArchetype.Energy => new Color(0.2f, 0.8f, 1.0f, 0.5f),
                _ => Color.white
            };
        }

        private VisualElement CreateTraitToggle(string name, string desc, int cost, string id)
        {
            bool hasTrait = _currentRace.Traits.Contains(id);

            var box = new GraphicalUserInterfaceBuilder($"Toggle_{id}")
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                .WithBackgroundColor(hasTrait ? new Color(0, 0.4f, 0.2f) : new Color(0.2f, 0.2f, 0.2f))
                .WithPadding(8)
                .Build();

            box.style.marginBottom = 4;
            box.style.borderLeftWidth = 3;
            box.style.borderLeftColor = hasTrait ? Color.green : Color.gray;
            box.RegisterCallback<ClickEvent>(e => ToggleTrait(id, cost));

            var left = new GraphicalUserInterfaceBuilder("TextGroup").WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.FlexStart).Build();
            left.Add(new Label(name) { style = { color = Color.white, unityFontStyleAndWeight = FontStyle.Bold } });
            left.Add(new Label(desc) { style = { color = Color.gray, fontSize = 10 } });

            box.Add(left);
            box.Add(new Label(cost.ToString()) { style = { color = Color.yellow, unityFontStyleAndWeight = FontStyle.Bold } });

            return box;
        }

        private VisualElement CreateStandardTrait(string name, string desc, int cost)
        {
            var box = new GraphicalUserInterfaceBuilder($"Standard_{name.Replace(" ", "")}")
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.1f))
                .WithPadding(8)
                .Build();

            box.style.opacity = 0.8f;
            box.style.marginBottom = 4;

            var left = new GraphicalUserInterfaceBuilder("TextGroup").WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.FlexStart).Build();
            left.Add(new Label(name + " (Inherent)") { style = { color = Color.cyan, unityFontStyleAndWeight = FontStyle.Bold } });
            left.Add(new Label(desc) { style = { color = Color.gray, fontSize = 10 } });

            box.Add(left);
            return box;
        }

        private VisualElement CreateGhostTrait(string name, string reason)
        {
            var box = new GraphicalUserInterfaceBuilder($"Ghost_{name.Replace(" ", "")}")
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Center)
                .WithBackgroundColor(new Color(0.1f, 0, 0, 0.5f))
                .WithPadding(8)
                .Build();

            box.style.marginBottom = 4;

            box.Add(new Label($"LOCKED: {name}") { style = { color = Color.gray, flexGrow = 1 } });
            box.Add(new Label(reason.ToUpper()) { style = { color = Color.red, fontSize = 9 } });
            return box;
        }

        private VisualElement CreateTab(string text, Action onClick)
        {
            var btn = new Button(onClick)
            {
                text = text,
                style = { flexGrow = 1, height = 40, backgroundColor = Color.clear, borderBottomWidth = 0, color = Color.white }
            };
            return btn;
        }

        private VisualElement CreateDNAMeter()
        {
            var root = new GraphicalUserInterfaceBuilder("DNAMeter_Root")
                .WithMarginTop(20)
                .Build();
            root.style.width = Length.Percent(100);

            root.Add(new Label($"GENETIC STABILITY: {_dnaPoints}/{_maxPoints}") { style = { alignSelf = Align.Center, color = Color.yellow } });

            var bar = new GraphicalUserInterfaceBuilder("BarBackground")
                .WithBackgroundColor(Color.black)
                .WithMarginTop(5)
                .Build();
            bar.style.height = 10;

            root.Add(bar);
            return root;
        }

        private void LoadPresetMenu()
        {
            Debug.Log("Open Preset List..");
        }

        private void CommitRace()
        {
            Debug.Log($"Race Created: {_currentRace.Name} ({_currentRace.Archetype})");
            OnSaveAndLaunchClicked?.Invoke();
        }

        private List<RaceData> GeneratePresets()
        {
            return new List<RaceData> {
                new RaceData { Name = "Terran", Archetype = RaceArchetype.Carbon, Traits = new List<string> { "Democracy", "Diplomatic" } },
                new RaceData { Name = "Xenon Hive", Archetype = RaceArchetype.Silicon, Traits = new List<string> { "HiveMind", "WarTorn" } }
            };
        }

        public class RaceData
        {
            public string Name;
            public string Description;
            public RaceArchetype Archetype;
            public List<string> Traits = new List<string>();
            public Color PrimaryColor;
        }

        public enum RaceArchetype { Carbon, Silicon, Synthetic, Energy }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));

#if UNITY_EDITOR
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
#endif
        public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
    }
}