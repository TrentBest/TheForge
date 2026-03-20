#if UNITY_EDITOR
using TheSingularityWorkshop.Armada2525;
using TheSingularityWorkshop.Armada2525.Editor;
using TheSingularityWorkshop.Armada2525.Government;
using TheSingularityWorkshop.Builders.GuiBuilders;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


public class BuilderModule
{
    public string TabName { get; set; }
    public string Description { get; set; }
    public Func<GuiContext, VisualElement> InjectGui { get; set; }
}

public class Armada2525_Gui_EmpireBuilder : IGuiProvider
{
    public string Title => "GALACTIC EMPIRE FORGE";

    private GuiContext _guiContext;
    private Armada2525 _game;
    private VisualElement _rootContainer;

    // Layout zones
    private VisualElement _sidebarPanel;
    private VisualElement _activeWorkspace;

    private GalacticEmpireContext _empireContext;
    private Action _onBack;

    // The dynamic list of tools available on the workbench
    private List<BuilderModule> _availableBuilders = new List<BuilderModule>();
    private BuilderModule _activeModule;

    public Armada2525_Gui_EmpireBuilder() : this(null, null) { }

    public Armada2525_Gui_EmpireBuilder(GalacticEmpireContext context, Action onBack)
    {
        _empireContext = context ?? new GalacticEmpireContext();
        _onBack = onBack;

        InitializeWorkbenchModules();
    }

    private void InitializeWorkbenchModules()
    {
        // Registering our CRUD Builders as dynamic modules
        _availableBuilders.Add(new BuilderModule
        {
            TabName = "Political Factions",
            Description = "Hierarchy and Leaders",
            InjectGui = (ctx) => new Armada2525_Gui_FactionBuilder().CreateGui(ctx)
        });

        // Assuming you saved the FactionTraitBuilder from the previous step!
        _availableBuilders.Add(new BuilderModule
        {
            TabName = "Faction Traits",
            Description = "Buffs, Debuffs & Math",
            InjectGui = (ctx) => new Armada2525_Gui_FactionTraitBuilder().CreateGui(ctx)
        });

        _availableBuilders.Add(new BuilderModule
        {
            TabName = "Macro-Industries",
            Description = "Economic Sectors",
            InjectGui = (ctx) => new Armada2525_Gui_IndustryBuilder().CreateGui(ctx)
        });

        // If you haven't made FirmBuilder yet, you can comment this out
        _availableBuilders.Add(new BuilderModule
        {
            TabName = "Corporate Firms",
            Description = "The Mega-Corps",
            InjectGui = (ctx) => new Armada2525_Gui_FirmBuilder().CreateGui(ctx)
        });

        _availableBuilders.Add(new BuilderModule
        {
            TabName = "Linguistic Archives",
            Description = "Generative Lexicons",
            InjectGui = (ctx) => new Armada2525_Gui_LexiconBuilder().CreateGui(ctx)
        });

        // Set default tab
        if (_availableBuilders.Count > 0) _activeModule = _availableBuilders[0];
    }

    public VisualElement CreateGui(GuiContext context)
    {
        _guiContext = context;
        _game = UnityEngine.Object.FindAnyObjectByType<Armada2525>();

        var builder = new GraphicalUserInterfaceBuilder("EmpireBuilder_Root")
            .WithBackgroundColor(new Color(0.01f, 0.01f, 0.02f))
            .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
            .WithPercentSize(100, 100);

        // --- HEADER ---
        builder.AddChild(c => {
            var header = new VisualElement { style = { height = 70, flexDirection = FlexDirection.Row, backgroundColor = new Color(0.05f, 0.05f, 0.08f), borderBottomWidth = 2, borderBottomColor = new Color(0.6f, 0.1f, 0.1f), paddingLeft = 20, paddingRight = 20, alignItems = Align.Center } };

            header.Add(new Label("MACRO-POLITICAL ARCHITECT") { style = { color = new Color(0.8f, 0.2f, 0.2f), fontSize = 24, unityFontStyleAndWeight = FontStyle.Bold, flexGrow = 1, letterSpacing = 2 } });

            header.Add(new Button(() => {
                SaveEmpireContext();
                if (_onBack != null) _onBack();
                else _game?.SwitchGui("GovernmentView");
            })
            { text = "<< SAVE UNIVERSE & INITIALIZE", style = { height = 40, width = 250, backgroundColor = new Color(0.4f, 0.1f, 0.1f), color = Color.white, unityFontStyleAndWeight = FontStyle.Bold } });

            return header;
        });

        // --- MAIN LAYOUT ---
        builder.AddChild(c => {
            var mainLayout = new VisualElement { style = { flexGrow = 1, flexDirection = FlexDirection.Row } };

            // LEFT: THE WORKBENCH CONTROLS
            _sidebarPanel = new VisualElement { style = { width = 300, backgroundColor = new Color(0.03f, 0.03f, 0.04f), borderRightWidth = 2, borderRightColor = new Color(0.6f, 0.1f, 0.1f), paddingLeft = 15, paddingRight = 15, paddingTop = 15 } };
            RenderSidebar();
            mainLayout.Add(_sidebarPanel);

            // RIGHT: THE ACTIVE CRUD BUILDER
            _activeWorkspace = new VisualElement { style = { flexGrow = 1, backgroundColor = new Color(0.02f, 0.02f, 0.02f) } };
            RenderWorkspace();
            mainLayout.Add(_activeWorkspace);

            return mainLayout;
        });

        _rootContainer = builder.Build();
        return _rootContainer;
    }

    private void RenderSidebar()
    {
        _sidebarPanel.Clear();

        // 1. The Crown Overview
        _sidebarPanel.Add(new Label("THE IMPERIAL CROWN") { style = { color = new Color(0.8f, 0.2f, 0.2f), fontSize = 16, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });

        var crownBox = new VisualElement { style = { backgroundColor = Color.black, paddingLeft = 10, paddingRight = 10, paddingTop = 10, paddingBottom = 10, borderLeftWidth = 2, borderLeftColor = new Color(0.8f, 0.2f, 0.2f), marginBottom = 20 } };

        var empressField = new TextField("Empress") { value = _empireContext.EmpressName };
        empressField.RegisterValueChangedCallback(e => _empireContext.EmpressName = e.newValue);
        crownBox.Add(empressField);

        var approvalField = new DoubleField("Global Approval") { value = _empireContext.GlobalApprovalRating };
        approvalField.RegisterValueChangedCallback(e => _empireContext.GlobalApprovalRating = e.newValue);
        crownBox.Add(approvalField);

        _sidebarPanel.Add(crownBox);

        // 2. The Dynamic Modules (Tabs)
        _sidebarPanel.Add(new Label("ARCHITECTURE MODULES") { style = { color = Color.cyan, fontSize = 16, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });

        var scroll = new ScrollView(ScrollViewMode.Vertical) { style = { flexGrow = 1 } };

        foreach (var module in _availableBuilders)
        {
            bool isActive = _activeModule == module;

            var btn = new Button(() => { _activeModule = module; RenderSidebar(); RenderWorkspace(); })
            {
                style = {
                        height = 60, marginBottom = 5, paddingLeft = 15, justifyContent = Justify.Center, alignItems = Align.FlexStart,
                        backgroundColor = isActive ? new Color(0.1f, 0.2f, 0.3f) : new Color(0.1f, 0.1f, 0.15f),
                        borderLeftWidth = isActive ? 4 : 0, borderLeftColor = Color.cyan
                    }
            };

            btn.Add(new Label(module.TabName) { style = { color = Color.white, fontSize = 14, unityFontStyleAndWeight = FontStyle.Bold } });
            btn.Add(new Label(module.Description) { style = { color = Color.gray, fontSize = 10 } });

            scroll.Add(btn);
        }

        _sidebarPanel.Add(scroll);

        // 3. The Expandable Horizon
        var injectBtn = new Button(() => Debug.Log("Future Feature: Load external builder assembly"))
        {
            text = "+ MOUNT EXTERNAL BUILDER",
            style = { height = 40, marginTop = 10, marginBottom = 10, backgroundColor = new Color(0.1f, 0.3f, 0.1f), color = Color.white, unityFontStyleAndWeight = FontStyle.Bold }
        };
        _sidebarPanel.Add(injectBtn);
    }

    private void RenderWorkspace()
    {
        _activeWorkspace.Clear();

        if (_activeModule == null || _activeModule.InjectGui == null)
        {
            _activeWorkspace.Add(new Label("NO MODULE MOUNTED") { style = { color = Color.red, fontSize = 24, unityTextAlign = TextAnchor.MiddleCenter, flexGrow = 1 } });
            return;
        }

        // Execute the factory function to build the CRUD GUI, and mount it to our workspace
        var injectedGui = _activeModule.InjectGui(_guiContext);
        injectedGui.style.flexGrow = 1;

        _activeWorkspace.Add(injectedGui);
    }

    private void SaveEmpireContext()
    {
        _empireContext.MajorFactions.Clear();
        foreach (var faction in Armada2525_Gui_FactionBuilder.FactionDatabase)
        {
            if (faction.ParentFaction == null)
            {
                _empireContext.MajorFactions.Add(faction);
            }
        }
        Debug.Log($"[Empire Forge] Saved {_empireContext.EmpressName}'s Empire. Initialized {_empireContext.MajorFactions.Count} Major Factions directly under the Crown.");
    }

    public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
    public void FromUIDocument(string assetPath) => _guiContext?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
    public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
}

#endif