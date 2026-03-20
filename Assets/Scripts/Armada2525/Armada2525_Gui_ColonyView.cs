using TheSingularityWorkshop.Armada2525;
using TheSingularityWorkshop.Builders.GuiBuilders;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Armada2525_Gui_ColonyView : IGuiProvider
{
    public string Title => "Imperial Domain: " + (string.IsNullOrEmpty(_colony.Name) ? "UNNAMED COLONY" : _colony.Name.ToUpper());

    public ColonyData _colony;
    private Armada2525 _game;
    private GuiContext _lastCtx;
    private VisualElement _mainContentArea;

    // --- STATE ---
    private enum ColonyTab { Overview, Petitions, Orbital, Palace }
    private ColonyTab _currentTab = ColonyTab.Overview;
    private float _taxRate = 0.15f; // 15% default tithe

    // Mock Petition Class for Character Interactions
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

    public Armada2525_Gui_ColonyView()
    {
        // Setup mock data for the Editor UI preview
        _colony = new ColonyData
        {
            Name = "Aegis Prime",
            PopulationMillions = 14.2f,
            GrowthRate = 0.04f,
            Morale = 0.68f, // Slightly low to justify petitions
            GovernorName = "Vance (Industrialist)",
            Focus = ColonyFocus.Industrial,
            RawResourceOutput = 1200f,
            FoundryCapacity = 800f,
            FactoryCapacity = 450f,
            AssemblerCapacity = 100f,
            HasOrbitalShipyard = false,
            BuildQueue = new List<string> { "Deep Core Mine Expansion" }
        };

        _activePetitions = new List<Petition>
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
        _game = UnityEngine.Object.FindAnyObjectByType<Armada2525>();

        var builder = new GraphicalUserInterfaceBuilder("ColonyView_Root")
            .WithBackgroundColor(new Color(0.02f, 0.03f, 0.05f))
            .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
            .WithPercentSize(100, 100);

        // --- LEFT SIDEBAR: NAVIGATION & GOVERNANCE (25%) ---
        builder.AddChild(c => {
            var sidebar = new VisualElement
            {
                style = {
                    width = Length.Percent(25), height = Length.Percent(100),
                    backgroundColor = new Color(0.05f, 0.07f, 0.1f),
                    borderRightWidth = 2, borderRightColor = Color.cyan,
                    paddingTop = 20, paddingBottom = 20, paddingLeft = 15, paddingRight = 15
                }
            };

            // 1. Back Button
            var backBtn = new Button(() => _game?.SwitchGui("GalaxyView"))
            {
                text = "<< SYSTEM COMMAND",
                style = { backgroundColor = Color.clear, color = Color.gray, borderBottomWidth = 1, borderBottomColor = Color.gray, marginBottom = 20, height = 30 }
            };
            sidebar.Add(backBtn);

            // 2. Colony Title
            sidebar.Add(new Label(Title) { style = { color = Color.cyan, fontSize = 22, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });

            // 3. Demographics & Taxes
            var demoBox = new VisualElement { style = { backgroundColor = new Color(0, 0, 0, 0.5f), paddingLeft = 10, paddingTop = 10, paddingBottom = 10, marginBottom = 20, borderLeftWidth = 2, borderLeftColor = Color.green } };
            demoBox.Add(new Label($"POPULATION: {_colony.PopulationMillions:F1}M") { style = { color = Color.white, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 5 } });
            demoBox.Add(new Label($"GROWTH: +{_colony.GrowthRate:P1} / turn") { style = { color = Color.green, fontSize = 10, marginBottom = 5 } });

            Color moraleColor = _colony.Morale > 0.8f ? Color.green : (_colony.Morale > 0.4f ? Color.yellow : Color.red);
            demoBox.Add(new Label($"MORALE: {_colony.Morale:P0}") { style = { color = moraleColor, fontSize = 10 } });
            sidebar.Add(demoBox);

            // 4. Taxation Control
            sidebar.Add(new Label("IMPERIAL TITHE (TAX RATE)") { style = { color = Color.gray, fontSize = 10, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 5 } });
            var taxLbl = new Label($"{_taxRate:P0}") { style = { color = Color.white, unityTextAlign = TextAnchor.MiddleRight, marginBottom = 5 } };
            var taxSlider = new Slider(0f, 0.5f) { value = _taxRate };
            taxSlider.RegisterValueChangedCallback(evt => {
                _taxRate = evt.newValue;
                taxLbl.text = $"{_taxRate:P0}";
                // Logic to lower morale prediction here based on tax
            });
            sidebar.Add(taxLbl);
            sidebar.Add(taxSlider);
            sidebar.Add(new VisualElement { style = { height = 20 } }); // spacer

            // 5. Governor Appointment
            sidebar.Add(new Label("APPOINTED GOVERNOR") { style = { color = Color.gray, fontSize = 10, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 5 } });
            var governorDropdown = new PopupField<string>(new List<string> { "Vance (Industrialist)", "Olorin (Technocrat)", "Sylar (Hedonist)" }, _colony.GovernorName);
            governorDropdown.RegisterValueChangedCallback(evt => {
                _colony.GovernorName = evt.newValue;
                RenderCurrentTab();
            });
            governorDropdown.style.marginBottom = 20;
            sidebar.Add(governorDropdown);

            // 6. Tabs
            sidebar.Add(new Label("TERMINAL ACCESS") { style = { color = Color.gray, fontSize = 10, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });
            sidebar.Add(CreateTabButton("MACRO-ECONOMY", ColonyTab.Overview));

            // Highlight Petitions if there are any
            string petitionText = _activePetitions.Count > 0 ? $"PETITIONS ({_activePetitions.Count}) [!]" : "PETITIONS";
            sidebar.Add(CreateTabButton(petitionText, ColonyTab.Petitions));

            sidebar.Add(CreateTabButton("ORBITAL COMMAND", ColonyTab.Orbital));
            sidebar.Add(CreateTabButton("IMPERIAL PALACE", ColonyTab.Palace));

            return sidebar;
        });

        // --- RIGHT AREA: DYNAMIC CONTENT (75%) ---
        builder.AddChild(c => {
            var rightCol = new VisualElement { style = { flexGrow = 1, flexDirection = FlexDirection.Column } };

            // Content Area
            _mainContentArea = new VisualElement { style = { flexGrow = 1, paddingTop = 20, paddingBottom = 20, paddingLeft = 20, paddingRight = 20 } };
            RenderCurrentTab();
            rightCol.Add(_mainContentArea);

            // Footer: Current Governor Mandate
            var footer = new VisualElement
            {
                style = {
                    height = 100, backgroundColor = new Color(0.05f, 0.07f, 0.1f),
                    borderTopWidth = 2, borderTopColor = Color.cyan,
                    paddingTop = 15, paddingRight = 20, paddingLeft = 20, paddingBottom = 15,
                    flexDirection = FlexDirection.Row, alignItems = Align.Center
                }
            };

            string mandate = (_colony.BuildQueue != null && _colony.BuildQueue.Count > 0) ? _colony.BuildQueue[0] : "AWAITING IMPERIAL ORDERS";

            var queueInfo = new VisualElement { style = { flexGrow = 1 } };
            queueInfo.Add(new Label("GOVERNOR's CURRENT MANDATE") { style = { color = Color.gray, fontSize = 10, unityFontStyleAndWeight = FontStyle.Bold } });
            queueInfo.Add(new Label(mandate.ToUpper()) { style = { color = Color.cyan, fontSize = 18, unityFontStyleAndWeight = FontStyle.Bold, marginTop = 5 } });
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

        var btn = new Button(() => {
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
        return btn;
    }

    private void RenderCurrentTab()
    {
        if (_mainContentArea == null) return;
        _mainContentArea.Clear();

        switch (_currentTab)
        {
            case ColonyTab.Overview:
                RenderMacroEconomy();
                break;
            case ColonyTab.Petitions:
                RenderPetitions();
                break;
            case ColonyTab.Orbital:
                RenderOrbitalCommand();
                break;
            case ColonyTab.Palace:
                RenderPalaceDesigner();
                break;
        }
    }

    // --------------------------------------------------------------------------------
    // SUB-VIEWS
    // --------------------------------------------------------------------------------

    private void RenderMacroEconomy()
    {
        _mainContentArea.Add(new Label("IMPERIAL SUPPLY CHAIN CONTRIBUTION") { style = { color = Color.white, fontSize = 18, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 20 } });
        _mainContentArea.Add(new Label("This visualizes how this colony's outputs feed into the greater imperial war machine.") { style = { color = Color.gray, fontSize = 12, marginBottom = 30 } });

        var pipeline = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween, alignItems = Align.Center, flexGrow = 1 } };

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
            var exportBox = new VisualElement { style = { width = 120, alignItems = Align.Center } };
            exportBox.Add(CreateFlowArrow());
            exportBox.Add(new Label("EXPORTS TO\nSHIPYARDS") { style = { color = Color.gray, unityTextAlign = TextAnchor.MiddleCenter, unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10 } });
            pipeline.Add(exportBox);
        }

        _mainContentArea.Add(pipeline);
    }

    private void RenderPetitions()
    {
        _mainContentArea.Add(new Label("IMPERIAL GRANTS & PETITIONS") { style = { color = Color.white, fontSize = 18, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });
        _mainContentArea.Add(new Label("Citizens and leaders bring matters to your attention. Choose carefully where to allocate the Empire's wealth.") { style = { color = Color.gray, fontSize = 12, marginBottom = 20 } });

        var scroll = new ScrollView { style = { flexGrow = 1 } };

        if (_activePetitions.Count == 0)
        {
            scroll.Add(new Label("The populace is quiet. No active petitions at this time.") { style = { color = Color.gray, marginTop = 50, alignSelf = Align.Center, unityFontStyleAndWeight = FontStyle.Italic } });
        }

        foreach (var petition in _activePetitions)
        {
            scroll.Add(CreatePetitionCard(petition));
        }

        _mainContentArea.Add(scroll);
    }

    private void RenderOrbitalCommand()
    {
        _mainContentArea.Add(new Label("ORBITAL COMMAND & DEFENSE") { style = { color = Color.cyan, fontSize = 18, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });
        _mainContentArea.Add(new Label("View defensive platforms, shipyards, and orbital traffic.") { style = { color = Color.gray, fontSize = 12, marginBottom = 20 } });

        var orbitalContainer = new VisualElement
        {
            style = {
                flexGrow = 1, backgroundColor = new Color(0.01f, 0.01f, 0.03f),
                borderTopWidth = 1, borderBottomWidth = 1, borderLeftWidth = 1, borderRightWidth = 1,
                borderTopColor = Color.cyan, borderBottomColor = Color.cyan, borderLeftColor = Color.cyan, borderRightColor = Color.cyan,
                alignItems = Align.Center, justifyContent = Justify.Center
            }
        };

        orbitalContainer.Add(new Label("[ 3D ORBITAL TACTICAL RENDERER GOES HERE ]") { style = { color = new Color(1, 1, 1, 0.3f), fontSize = 16, unityFontStyleAndWeight = FontStyle.Bold } });
        _mainContentArea.Add(orbitalContainer);
        var launchBtn = new Button(() => {
            if (!_game.GUI.ContainsKey("TrafficController"))
                _game.GUI["TrafficController"] = new Armada2525_Gui_SpaceTrafficController();
            _game.SwitchGui("TrafficController");
        })
        {
            text = "LAUNCH TRAFFIC CONTROL SIMULATOR",
            style = { backgroundColor = Color.cyan, color = Color.black, height = 50, width = 400, unityFontStyleAndWeight = FontStyle.Bold, marginTop = 20 }
        };
        orbitalContainer.Add(launchBtn);
    }

    private void RenderPalaceDesigner()
    {
        _mainContentArea.Add(new Label("IMPERIAL PALACE PLANNER") { style = { color = new Color(0.8f, 0.6f, 0f), fontSize = 18, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });
        _mainContentArea.Add(new Label("Spend idle time designing your legacy while opponents finish their turns.") { style = { color = Color.gray, fontSize = 12, marginBottom = 20 } });

        var palaceContainer = new VisualElement
        {
            style = {
                flexGrow = 1, backgroundColor = new Color(0.05f, 0.05f, 0.05f),
                borderTopWidth = 1, borderBottomWidth = 1, borderLeftWidth = 1, borderRightWidth = 1,
                borderTopColor = new Color(0.8f, 0.6f, 0f), borderBottomColor = new Color(0.8f, 0.6f, 0f), borderLeftColor = new Color(0.8f, 0.6f, 0f), borderRightColor = new Color(0.8f, 0.6f, 0f),
                alignItems = Align.Center, justifyContent = Justify.Center
            }
        };

        palaceContainer.Add(new Label("[ 3D PALACE BLUEPRINT RENDERER GOES HERE ]") { style = { color = new Color(1, 1, 1, 0.3f), fontSize = 16, unityFontStyleAndWeight = FontStyle.Bold } });
        _mainContentArea.Add(palaceContainer);
    }

    // --------------------------------------------------------------------------------
    // UI COMPONENTS
    // --------------------------------------------------------------------------------

    private VisualElement CreatePetitionCard(Petition p)
    {
        var card = new VisualElement
        {
            style = {
                flexDirection = FlexDirection.Row,
                backgroundColor = new Color(0.08f, 0.08f, 0.12f),
                marginBottom = 15, paddingLeft = 15, paddingRight = 15, paddingTop = 15, paddingBottom = 15,
                borderLeftWidth = 4, borderLeftColor = p.ThemeColor
            }
        };

        // Character Portrait Placeholder
        var portrait = new VisualElement
        {
            style = {
                width = 80, height = 80, backgroundColor = new Color(p.ThemeColor.r, p.ThemeColor.g, p.ThemeColor.b, 0.3f),
                marginRight = 20, alignItems = Align.Center, justifyContent = Justify.Center,
                borderTopWidth = 1, borderBottomWidth = 1, borderLeftWidth = 1, borderRightWidth = 1, borderTopColor = p.ThemeColor, borderBottomColor = p.ThemeColor, borderLeftColor = p.ThemeColor, borderRightColor = p.ThemeColor
            }
        };
        portrait.Add(new Label(p.PetitionerName.Substring(0, 1)) { style = { fontSize = 36, color = p.ThemeColor, unityFontStyleAndWeight = FontStyle.Bold } });
        card.Add(portrait);

        // Dialogue Content
        var content = new VisualElement { style = { flexGrow = 1, flexDirection = FlexDirection.Column, justifyContent = Justify.SpaceBetween } };

        var header = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween } };
        header.Add(new Label(p.PetitionerName.ToUpper()) { style = { color = Color.white, fontSize = 16, unityFontStyleAndWeight = FontStyle.Bold } });
        header.Add(new Label(p.RoleTitle.ToUpper()) { style = { color = Color.gray, fontSize = 10 } });
        content.Add(header);

        var dialogue = new Label($"\"{p.Message}\"") { style = { color = new Color(0.8f, 0.8f, 0.8f), whiteSpace = WhiteSpace.Normal, marginTop = 10, marginBottom = 10, unityFontStyleAndWeight = FontStyle.Italic } };
        content.Add(dialogue);

        // Action Row
        var actionRow = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween, alignItems = Align.Center, borderTopWidth = 1, borderTopColor = new Color(1, 1, 1, 0.1f), paddingTop = 10 } };

        var rewardLbl = new Label(p.RewardText) { style = { color = Color.green, fontSize = 10 } };
        actionRow.Add(rewardLbl);

        var actions = new VisualElement { style = { flexDirection = FlexDirection.Row } };
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
        var node = new VisualElement
        {
            style = {
                width = 130, height = 150, backgroundColor = new Color(0.05f, 0.05f, 0.08f),
                borderTopWidth = 2, borderTopColor = themeColor,
                flexDirection = FlexDirection.Column, alignItems = Align.Center, justifyContent = Justify.SpaceBetween,
                paddingTop = 15, paddingBottom = 15
            }
        };

        node.Add(new Label(title) { style = { color = Color.white, unityFontStyleAndWeight = FontStyle.Bold } });

        var gauge = new VisualElement { style = { width = 60, height = 60, backgroundColor = new Color(themeColor.r, themeColor.g, themeColor.b, 0.2f), borderLeftWidth = 2, borderRightWidth = 2, borderTopWidth = 2, borderBottomWidth = 2, borderLeftColor = themeColor, borderRightColor = themeColor, borderTopColor = themeColor, borderBottomColor = themeColor, alignItems = Align.Center, justifyContent = Justify.Center } };
        gauge.Add(new Label($"{capacity:F0}") { style = { color = Color.white, unityFontStyleAndWeight = FontStyle.Bold, fontSize = 16 } });
        node.Add(gauge);

        node.Add(new Label(subtitle) { style = { color = Color.gray, fontSize = 10 } });

        if (capacity <= 0) node.style.opacity = 0.3f;
        return node;
    }

    private VisualElement CreateFlowArrow() => new Label(">>>") { style = { color = Color.cyan, fontSize = 24, unityFontStyleAndWeight = FontStyle.Bold } };

    // --------------------------------------------------------------------------------
    // STANDARDIZED BUILDER PATTERN
    // --------------------------------------------------------------------------------

    public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));

#if UNITY_EDITOR
    public void ToUIDocument(string assetPath)
    {
        VisualElement myTree = CreateGui(new GuiContext());
        GraphicalUserInterfaceBuilder.ConvertToUIDocument(myTree, assetPath);
    }
#endif

    public void FromUIDocument(string assetPath)
    {
        VisualElement hydratedUi = GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath);
        _lastCtx?.OnBuilt?.Invoke(hydratedUi);
    }
}