using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.MastersOfOrionII
{
    public class MastersOfOrionII_Gui_ColonyView : IGuiProvider
    {
        // FIX 1: Structs can never be null. Check if the Name is empty to determine if it's uninitialized.
        public string Title => "Imperial Domain: " + (string.IsNullOrEmpty(_colony.Name) ? "UNNAMED COLONY" : _colony.Name.ToUpper());

        public ColonyData _colony;
        private MastersOfOrionII_Game _game;
        private GuiContext _lastCtx;
        private VisualElement _mainContentArea;

        // --- STATE ---
        private enum ColonyTab { Overview, Petitions, Orbital, Palace }
        private ColonyTab _currentTab = ColonyTab.Overview;
        private float _taxRate = 0.15f;

        private class Petition
        {
            public string PetitionerName;
            public string RoleTitle;
            public string Message;
            public int Cost;
            public string RewardText;
            public Color ThemeColor;
        }
        private List<Petition> _activePetitions;

        public MastersOfOrionII_Gui_ColonyView()
        {
            AllocateSafeState();
        }

        private void AllocateSafeState()
        {
            // FIX 2: Check for empty struct instead of using the ??= null-coalescing operator
            if (string.IsNullOrEmpty(_colony.Name))
            {
                _colony = new ColonyData
                {
                    Name = "Aegis Prime",
                    PopulationMillions = 14.2f,
                    GrowthRate = 0.04f,
                    Morale = 0.68f,
                    GovernorName = "Vance (Industrialist)",
                    Focus = ColonyFocus.Industrial,
                    RawResourceOutput = 1200f,
                    FoundryCapacity = 800f,
                    FactoryCapacity = 450f,
                    AssemblerCapacity = 100f,
                    HasOrbitalShipyard = false,
                    BuildQueue = new List<string> { "Deep Core Mine Expansion" }
                };
            }

            _activePetitions ??= new List<Petition>
            {
                new Petition {
                    PetitionerName = "Foreman Kael", RoleTitle = "Union Representative", ThemeColor = new Color(0.8f, 0.4f, 0.1f),
                    Message = "My Lord, the air scrubbers in the lower hab-blocks are failing. The workers are choking on industrial runoff. We desperately need funds to overhaul the filtration network, or strikes are inevitable.",
                    Cost = 1500, RewardText = "+15% Morale, Prevents 'Worker Strike' event."
                },
                new Petition {
                    PetitionerName = "Magistrate Livia", RoleTitle = "Entertainment Director", ThemeColor = new Color(0.8f, 0.2f, 0.6f),
                    Message = "If we are to transition this sector into a Leisure World for our decorated officers, we must construct a Zero-G Casino. The preliminary permits are approved, we simply require the Imperial Grant.",
                    Cost = 4500, RewardText = "Converts planet focus to 'Leisure'. +2 Empire-wide Fleet Morale."
                }
            };
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;
            _game = UnityEngine.Object.FindAnyObjectByType<MastersOfOrionII_Game>();

            // FIX 3: Nullity check replaced with string validation
            if (string.IsNullOrEmpty(_colony.Name)) AllocateSafeState();

            var builder = new GraphicalUserInterfaceBuilder("ColonyView_Root")
                .WithBackgroundColor(new Color(0.02f, 0.03f, 0.05f))
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                .WithPercentSize(100, 100);

            // --- LEFT SIDEBAR: NAVIGATION & GOVERNANCE ---
            builder.AddChild(c =>
            {
                var sidebar = new GraphicalUserInterfaceBuilder("Sidebar")
                    .WithBackgroundColor(new Color(0.05f, 0.07f, 0.1f))
                    .WithPadding(20)
                    .Build();

                sidebar.style.width = Length.Percent(25);
                sidebar.style.height = Length.Percent(100);
                sidebar.style.borderRightWidth = 2;
                sidebar.style.borderRightColor = Color.cyan;
                sidebar.style.paddingLeft = 15;
                sidebar.style.paddingRight = 15;

                var backBtn = new Button(() => _game?.SwitchGui("GalaxyView"))
                {
                    text = "<< SYSTEM COMMAND",
                    style = { backgroundColor = Color.clear, color = Color.gray, borderBottomWidth = 1, borderBottomColor = Color.gray, marginBottom = 20, height = 30 }
                };
                sidebar.Add(backBtn);

                // REFACTORED: ForgeLabelBuilder
                sidebar.Add(new ForgeLabelBuilder(Title).WithColor(Color.cyan).WithFontSize(22).WithBold().WithMarginBottom(10).Build());

                var demoBox = new GraphicalUserInterfaceBuilder("DemographicsBox")
                    .WithBackgroundColor(new Color(0, 0, 0, 0.5f))
                    .WithPadding(10)
                    .Build();

                demoBox.style.marginBottom = 20;
                demoBox.style.borderLeftWidth = 2;
                demoBox.style.borderLeftColor = Color.green;

                // REFACTORED: ForgeLabelBuilders
                demoBox.Add(new ForgeLabelBuilder($"POPULATION: {_colony.PopulationMillions:F1}M").WithColor(Color.white).WithBold().WithMarginBottom(5).Build());
                demoBox.Add(new ForgeLabelBuilder($"GROWTH: +{_colony.GrowthRate:P1} / turn").WithColor(Color.green).WithFontSize(10).WithMarginBottom(5).Build());

                Color moraleColor = _colony.Morale > 0.8f ? Color.green : (_colony.Morale > 0.4f ? Color.yellow : Color.red);
                demoBox.Add(new ForgeLabelBuilder($"MORALE: {_colony.Morale:P0}").WithColor(moraleColor).WithFontSize(10).Build());
                sidebar.Add(demoBox);

                // 4. Taxation Control
                sidebar.Add(new ForgeLabelBuilder("IMPERIAL TITHE (TAX RATE)").WithColor(Color.gray).WithFontSize(10).WithBold().WithMarginBottom(5).Build());

                var taxLblWrapper = new VisualElement();
                var taxLbl = new ForgeLabelBuilder($"{_taxRate:P0}").WithColor(Color.white).WithMarginBottom(5).WithAlignment(TextAnchor.MiddleRight).Build() as Label;
                var taxSlider = new Slider(0f, 0.5f) { value = _taxRate };
                taxSlider.RegisterValueChangedCallback(evt =>
                {
                    _taxRate = evt.newValue;
                    if (taxLbl != null) taxLbl.text = $"{_taxRate:P0}";
                });

                taxLblWrapper.Add(taxLbl);
                taxLblWrapper.Add(taxSlider);
                sidebar.Add(taxLblWrapper);

                sidebar.Add(new GraphicalUserInterfaceBuilder("Spacer").WithHeight(20).Build());

                // 5. Governor Appointment
                sidebar.Add(new ForgeLabelBuilder("APPOINTED GOVERNOR").WithColor(Color.gray).WithFontSize(10).WithBold().WithMarginBottom(5).Build());
                var governorDropdown = new PopupField<string>(new List<string> { "Vance (Industrialist)", "Olorin (Technocrat)", "Sylar (Hedonist)" }, _colony.GovernorName);
                governorDropdown.RegisterValueChangedCallback(evt =>
                {
                    _colony.GovernorName = evt.newValue;
                    RenderCurrentTab();
                });
                governorDropdown.style.marginBottom = 20;
                sidebar.Add(governorDropdown);

                // 6. Tabs
                sidebar.Add(new ForgeLabelBuilder("TERMINAL ACCESS").WithColor(Color.gray).WithFontSize(10).WithBold().WithMarginBottom(10).Build());
                sidebar.Add(CreateTabButton("MACRO-ECONOMY", ColonyTab.Overview));

                string petitionText = _activePetitions.Count > 0 ? $"PETITIONS ({_activePetitions.Count}) [!]" : "PETITIONS";
                sidebar.Add(CreateTabButton(petitionText, ColonyTab.Petitions));

                sidebar.Add(CreateTabButton("ORBITAL COMMAND", ColonyTab.Orbital));
                sidebar.Add(CreateTabButton("IMPERIAL PALACE", ColonyTab.Palace));

                return sidebar;
            });

            // --- RIGHT AREA: DYNAMIC CONTENT ---
            builder.AddChild(c =>
            {
                var rightCol = new GraphicalUserInterfaceBuilder("RightColumn")
                    .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                    .Build();
                rightCol.style.flexGrow = 1;

                _mainContentArea = new GraphicalUserInterfaceBuilder("MainContentArea")
                    .WithPadding(20)
                    .Build();
                _mainContentArea.style.flexGrow = 1;
                RenderCurrentTab();
                rightCol.Add(_mainContentArea);

                var footer = new GraphicalUserInterfaceBuilder("FooterMandate")
                    .WithBackgroundColor(new Color(0.05f, 0.07f, 0.1f))
                    .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Center)
                    .Build();

                footer.style.height = 100;
                footer.style.borderTopWidth = 2; footer.style.borderTopColor = Color.cyan;
                footer.style.paddingTop = 15; footer.style.paddingRight = 20;
                footer.style.paddingLeft = 20; footer.style.paddingBottom = 15;

                string mandate = (_colony.BuildQueue != null && _colony.BuildQueue.Count > 0) ? _colony.BuildQueue[0] : "AWAITING IMPERIAL ORDERS";

                var queueInfo = new GraphicalUserInterfaceBuilder("QueueInfo").Build();
                queueInfo.style.flexGrow = 1;

                // REFACTORED: ForgeLabelBuilders
                queueInfo.Add(new ForgeLabelBuilder("GOVERNOR's CURRENT MANDATE").WithColor(Color.gray).WithFontSize(10).WithBold().Build());
                queueInfo.Add(new ForgeLabelBuilder(mandate.ToUpper()).WithColor(Color.cyan).WithFontSize(18).WithBold().WithMarginTop(5).Build());

                footer.Add(queueInfo);
                rightCol.Add(footer);
                return rightCol;
            });

            return builder.Build();
        }

        // --------------------------------------------------------------------------------
        // TAB MANAGEMENT
        // --------------------------------------------------------------------------------

        private VisualElement CreateTabButton(string text, ColonyTab targetTab)
        {
            bool isAttention = text.Contains("[!]");
            Color baseBg = isAttention ? new Color(0.4f, 0.1f, 0.1f) : new Color(0.1f, 0.1f, 0.15f);
            Color activeBg = isAttention ? new Color(0.6f, 0.2f, 0.2f) : new Color(0, 0.4f, 0.6f);

            return new Button(() =>
            {
                _currentTab = targetTab;
                RenderCurrentTab();
            })
            {
                text = text,
                style = {
                    height = 40, marginBottom = 5, unityTextAlign = TextAnchor.MiddleLeft, paddingLeft = 15,
                    backgroundColor = _currentTab == targetTab ? activeBg : baseBg,
                    color = isAttention ? Color.yellow : Color.white,
                    borderLeftWidth = _currentTab == targetTab ? 4 : 0,
                    borderLeftColor = isAttention ? Color.yellow : Color.cyan,
                    unityFontStyleAndWeight = isAttention ? FontStyle.Bold : FontStyle.Normal
                }
            };
        }

        private void RenderCurrentTab()
        {
            if (_mainContentArea == null) return;
            _mainContentArea.Clear();

            switch (_currentTab)
            {
                case ColonyTab.Overview: RenderMacroEconomy(); break;
                case ColonyTab.Petitions: RenderPetitions(); break;
                case ColonyTab.Orbital: RenderOrbitalCommand(); break;
                case ColonyTab.Palace: RenderPalaceDesigner(); break;
            }
        }

        // --------------------------------------------------------------------------------
        // SUB-VIEWS
        // --------------------------------------------------------------------------------

        private void RenderMacroEconomy()
        {
            _mainContentArea.Add(new ForgeLabelBuilder("IMPERIAL SUPPLY CHAIN CONTRIBUTION").WithColor(Color.white).WithFontSize(18).WithBold().WithMarginBottom(20).Build());
            _mainContentArea.Add(new ForgeLabelBuilder("This visualizes how this colony's outputs feed into the greater imperial war machine.").WithColor(Color.gray).WithFontSize(12).WithMarginBottom(30).Build());

            var pipeline = new GraphicalUserInterfaceBuilder("PipelineContainer")
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                .Build();
            pipeline.style.flexGrow = 1;

            pipeline.Add(CreatePipelineNode("MINING", "Raw Ores", _colony.RawResourceOutput, new Color(0.6f, 0.4f, 0.2f)));
            pipeline.Add(CreateFlowArrow());
            pipeline.Add(CreatePipelineNode("FOUNDRY", "Refined Alloys", _colony.FoundryCapacity, new Color(0.8f, 0.3f, 0.1f)));
            pipeline.Add(CreateFlowArrow());
            pipeline.Add(CreatePipelineNode("FACTORY", "Ship Components", _colony.FactoryCapacity, new Color(0.2f, 0.6f, 0.8f)));
            pipeline.Add(CreateFlowArrow());
            pipeline.Add(CreatePipelineNode("ASSEMBLER", "Hull Modules", _colony.AssemblerCapacity, new Color(0.1f, 0.8f, 0.4f)));

            if (_colony.HasOrbitalShipyard)
            {
                pipeline.Add(CreateFlowArrow());
                pipeline.Add(CreatePipelineNode("ORBITAL YARD", "Capital Ships", 100f, new Color(0.8f, 0.8f, 0f)));
            }
            else
            {
                var exportBox = new GraphicalUserInterfaceBuilder("ExportBox").Build();
                exportBox.style.width = 120; exportBox.style.alignItems = Align.Center;
                exportBox.Add(CreateFlowArrow());
                exportBox.Add(new ForgeLabelBuilder("EXPORTS TO\nSHIPYARDS").WithColor(Color.gray).WithAlignment(TextAnchor.MiddleCenter).WithBold().WithMarginTop(10).Build());
                pipeline.Add(exportBox);
            }

            _mainContentArea.Add(pipeline);
        }

        private void RenderPetitions()
        {
            _mainContentArea.Add(new ForgeLabelBuilder("IMPERIAL GRANTS & PETITIONS").WithColor(Color.white).WithFontSize(18).WithBold().WithMarginBottom(10).Build());
            _mainContentArea.Add(new ForgeLabelBuilder("Citizens and leaders bring matters to your attention. Choose carefully where to allocate the Empire's wealth.").WithColor(Color.gray).WithFontSize(12).WithMarginBottom(20).Build());

            var scroll = new ScrollView { style = { flexGrow = 1 } };

            if (_activePetitions.Count == 0)
            {
                scroll.Add(new ForgeLabelBuilder("The populace is quiet. No active petitions at this time.")
                    .WithColor(Color.gray).WithMarginTop(50).WithFontStyle(FontStyle.Italic).Build());
            }

            foreach (var petition in _activePetitions)
            {
                scroll.Add(CreatePetitionCard(petition));
            }

            _mainContentArea.Add(scroll);
        }

        private void RenderOrbitalCommand()
        {
            _mainContentArea.Add(new ForgeLabelBuilder("ORBITAL COMMAND & DEFENSE").WithColor(Color.cyan).WithFontSize(18).WithBold().WithMarginBottom(10).Build());
            _mainContentArea.Add(new ForgeLabelBuilder("View defensive platforms, shipyards, and orbital traffic.").WithColor(Color.gray).WithFontSize(12).WithMarginBottom(20).Build());

            var orbitalContainer = new GraphicalUserInterfaceBuilder("OrbitalContainer")
                .WithBackgroundColor(new Color(0.01f, 0.01f, 0.03f))
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)
                .Build();

            orbitalContainer.style.flexGrow = 1;
            orbitalContainer.style.borderTopWidth = 1; orbitalContainer.style.borderBottomWidth = 1;
            orbitalContainer.style.borderLeftWidth = 1; orbitalContainer.style.borderRightWidth = 1;
            orbitalContainer.style.borderTopColor = Color.cyan; orbitalContainer.style.borderBottomColor = Color.cyan;
            orbitalContainer.style.borderLeftColor = Color.cyan; orbitalContainer.style.borderRightColor = Color.cyan;

            orbitalContainer.Add(new ForgeLabelBuilder("[ 3D ORBITAL TACTICAL RENDERER GOES HERE ]").WithColor(new Color(1, 1, 1, 0.3f)).WithFontSize(16).WithBold().Build());
            _mainContentArea.Add(orbitalContainer);

            var launchBtn = new Button(() =>
            {
                if (_game != null)
                {
                    if (!_game.GUI.ContainsKey("TrafficController"))
                        _game.GUI["TrafficController"] = new MastersOfOrionII_Gui_SpaceTrafficController();
                    _game.SwitchGui("TrafficController");
                }
            })
            {
                text = "LAUNCH TRAFFIC CONTROL SIMULATOR",
                style = { backgroundColor = Color.cyan, color = Color.black, height = 50, width = 400, unityFontStyleAndWeight = FontStyle.Bold, marginTop = 20 }
            };
            orbitalContainer.Add(launchBtn);
        }

        private void RenderPalaceDesigner()
        {
            _mainContentArea.Add(new ForgeLabelBuilder("IMPERIAL PALACE PLANNER").WithColor(new Color(0.8f, 0.6f, 0f)).WithFontSize(18).WithBold().WithMarginBottom(10).Build());
            _mainContentArea.Add(new ForgeLabelBuilder("Spend idle time designing your legacy while opponents finish their turns.").WithColor(Color.gray).WithFontSize(12).WithMarginBottom(20).Build());

            var palaceContainer = new GraphicalUserInterfaceBuilder("PalaceContainer")
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.05f))
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)
                .Build();

            palaceContainer.style.flexGrow = 1;
            palaceContainer.style.borderTopWidth = 1; palaceContainer.style.borderBottomWidth = 1;
            palaceContainer.style.borderLeftWidth = 1; palaceContainer.style.borderRightWidth = 1;
            palaceContainer.style.borderTopColor = new Color(0.8f, 0.6f, 0f); palaceContainer.style.borderBottomColor = new Color(0.8f, 0.6f, 0f);
            palaceContainer.style.borderLeftColor = new Color(0.8f, 0.6f, 0f); palaceContainer.style.borderRightColor = new Color(0.8f, 0.6f, 0f);

            palaceContainer.Add(new ForgeLabelBuilder("[ 3D PALACE BLUEPRINT RENDERER GOES HERE ]").WithColor(new Color(1, 1, 1, 0.3f)).WithFontSize(16).WithBold().Build());
            _mainContentArea.Add(palaceContainer);
        }

        // --------------------------------------------------------------------------------
        // UI COMPONENTS
        // --------------------------------------------------------------------------------

        private VisualElement CreatePetitionCard(Petition p)
        {
            var card = new GraphicalUserInterfaceBuilder($"Petition_{p.PetitionerName}")
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.12f))
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.FlexStart)
                .WithPadding(15)
                .Build();

            card.style.marginBottom = 15;
            card.style.borderLeftWidth = 4;
            card.style.borderLeftColor = p.ThemeColor;

            var portrait = new GraphicalUserInterfaceBuilder("Portrait")
                .WithBackgroundColor(new Color(p.ThemeColor.r, p.ThemeColor.g, p.ThemeColor.b, 0.3f))
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)
                .Build();

            portrait.style.width = 80; portrait.style.height = 80;
            portrait.style.marginRight = 20;
            portrait.style.borderTopWidth = 1; portrait.style.borderBottomWidth = 1;
            portrait.style.borderLeftWidth = 1; portrait.style.borderRightWidth = 1;
            portrait.style.borderTopColor = p.ThemeColor; portrait.style.borderBottomColor = p.ThemeColor;
            portrait.style.borderLeftColor = p.ThemeColor; portrait.style.borderRightColor = p.ThemeColor;

            portrait.Add(new ForgeLabelBuilder(p.PetitionerName.Substring(0, 1)).WithColor(p.ThemeColor).WithFontSize(36).WithBold().Build());
            card.Add(portrait);

            var content = new GraphicalUserInterfaceBuilder("ContentBlock")
                .WithFlexLayout(FlexDirection.Column, Justify.SpaceBetween, Align.Stretch)
                .Build();
            content.style.flexGrow = 1;

            var header = new GraphicalUserInterfaceBuilder("HeaderRow").WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.FlexStart).Build();
            header.Add(new ForgeLabelBuilder(p.PetitionerName.ToUpper()).WithColor(Color.white).WithFontSize(16).WithBold().Build());
            header.Add(new ForgeLabelBuilder(p.RoleTitle.ToUpper()).WithColor(Color.gray).WithFontSize(10).Build());
            content.Add(header);

            content.Add(new ForgeLabelBuilder($"\"{p.Message}\"")
                .WithColor(new Color(0.8f, 0.8f, 0.8f))
                .WithWhiteSpace(WhiteSpace.Normal)
                .WithMarginTop(10).WithMarginBottom(10)
                .WithFontStyle(FontStyle.Italic).Build());

            var actionRow = new GraphicalUserInterfaceBuilder("ActionRow")
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                .Build();

            actionRow.style.borderTopWidth = 1; actionRow.style.borderTopColor = new Color(1, 1, 1, 0.1f);
            actionRow.style.paddingTop = 10;

            actionRow.Add(new ForgeLabelBuilder(p.RewardText).WithColor(Color.green).WithFontSize(10).Build());

            var actions = new GraphicalUserInterfaceBuilder("ActionButtons").WithFlexLayout(FlexDirection.Row, Justify.FlexEnd, Align.Center).Build();
            var dismissBtn = new Button(() => { _activePetitions.Remove(p); RenderCurrentTab(); }) { text = "DISMISS", style = { backgroundColor = Color.clear, color = Color.gray, borderTopWidth = 1, borderBottomWidth = 1, borderLeftWidth = 1, borderRightWidth = 1, borderTopColor = Color.gray, borderBottomColor = Color.gray, borderLeftColor = Color.gray, borderRightColor = Color.gray, width = 80 } };
            var grantBtn = new Button(() => { _activePetitions.Remove(p); RenderCurrentTab(); }) { text = $"GRANT ({p.Cost}cr)", style = { backgroundColor = new Color(0, 0.4f, 0.2f), color = Color.white, unityFontStyleAndWeight = FontStyle.Bold, width = 120, marginLeft = 10 } };

            actions.Add(dismissBtn);
            actions.Add(grantBtn);
            actionRow.Add(actions);

            content.Add(actionRow);
            card.Add(content);

            return card;
        }

        private VisualElement CreatePipelineNode(string title, string subtitle, float capacity, Color themeColor)
        {
            var node = new GraphicalUserInterfaceBuilder($"Node_{title}")
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.08f))
                .WithFlexLayout(FlexDirection.Column, Justify.SpaceBetween, Align.Center)
                .Build();

            node.style.width = 130; node.style.height = 150;
            node.style.borderTopWidth = 2; node.style.borderTopColor = themeColor;
            node.style.paddingTop = 15; node.style.paddingBottom = 15;

            node.Add(new ForgeLabelBuilder(title).WithColor(Color.white).WithBold().Build());

            var gauge = new GraphicalUserInterfaceBuilder("Gauge")
                .WithBackgroundColor(new Color(themeColor.r, themeColor.g, themeColor.b, 0.2f))
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)
                .Build();

            gauge.style.width = 60; gauge.style.height = 60;
            gauge.style.borderLeftWidth = 2; gauge.style.borderRightWidth = 2;
            gauge.style.borderTopWidth = 2; gauge.style.borderBottomWidth = 2;
            gauge.style.borderLeftColor = themeColor; gauge.style.borderRightColor = themeColor;
            gauge.style.borderTopColor = themeColor; gauge.style.borderBottomColor = themeColor;

            gauge.Add(new ForgeLabelBuilder($"{capacity:F0}").WithColor(Color.white).WithBold().WithFontSize(16).Build());
            node.Add(gauge);

            node.Add(new ForgeLabelBuilder(subtitle).WithColor(Color.gray).WithFontSize(10).Build());

            if (capacity <= 0) node.style.opacity = 0.3f;
            return node;
        }

        private VisualElement CreateFlowArrow() => new ForgeLabelBuilder(">>>").WithColor(Color.cyan).WithFontSize(24).WithBold().Build();

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}