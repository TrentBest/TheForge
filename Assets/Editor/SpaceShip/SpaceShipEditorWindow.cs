#if UNITY_EDITOR
using Assets.Scripts;
using Assets.Scripts.Builders.GuiBuilders;
using Assets.Scripts.Builders.GuiBuilders.PanelBuilders;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class SpaceShipEditorWindow : EditorWindow
{
    [MenuItem("SS/Space Ship Editor Window", false, 0)]
    public static void ShowWindow() => GetWindow<SpaceShipEditorWindow>("SS Terminal");

    private readonly List<IShipTabBuilder> _tabs = new List<IShipTabBuilder>
{
    new FlightControlsTab(),   // Refined logic
    new ShipStatusTabBuilder(),     // Diagnostic readout
    new NavigationTab(),        // Long-range travel
    new WeaponsControlTab(),    // Fire control
    new DockingManeuversTab()   // Proximity & Clamps
};

    private string _activeTab = "Flight Controls";

    private void OnEnable() => Refresh();

    private void Refresh()
    {
        rootVisualElement.Clear();
        var sidebarWidth = 220;

        var rootLayout = new SplitPanelBuilder(sidebarWidth)
            .WithSidebar(CreateSidebar())
            .WithMain(CreateMainViewport());

        rootVisualElement.Add(rootLayout.CreateGui(new GuiContext { EditMode = true }));
    }

    private IGuiProvider CreateSidebar()
    {
       var sideBar= new GraphicalUserInterfaceBuilder("NavSideBar")
            .WithBackgroundColor(new Color(0.12f, 0.12f, 0.15f))
            .WithPadding(15)
            .AddChild(new Label("COMMAND CONSOLE") { style = { color = Color.cyan, marginBottom = 20 } });
        // Updated Tabs
        //.AddChild(CreateTabBtn("Status"))
        //.AddChild(CreateTabBtn("Flight Controls"))
        //.AddChild(CreateTabBtn("Navigation"))
        //.AddChild(CreateTabBtn("Weapons Control"))
        //.AddChild(CreateTabBtn("Docking Maneuvers"));
        foreach (var tab in _tabs)
        {
            sideBar.AddChild(CreateTabBtn($"{tab.TabIcon} - {tab.TabName}"));
        }
        return sideBar;
    }

    private VisualElement CreateTabBtn(string name) => new Button(() => { _activeTab = name; Refresh(); })
    {
        text = name,
        style = { height = 35, marginBottom = 5, backgroundColor = _activeTab == name ? new Color(0.3f, 0.3f, 0.4f) : Color.clear }
    };

    private IGuiProvider CreateMainViewport()
    {
        var viewport = new GraphicalUserInterfaceBuilder("MainContent")
            .WithPadding(20)
            .WithTitle(_activeTab.ToUpper());

     var currentTab = _tabs.FirstOrDefault(s=>s.TabName == name);
        viewport.AddChild(currentTab);
        return viewport;
    }
}
#endif