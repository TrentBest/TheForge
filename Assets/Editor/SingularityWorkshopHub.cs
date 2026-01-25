#if UNITY_EDITOR
using Assets.Scripts;
using Assets.Scripts.Builders.GuiBuilders;
using System;
using System.Collections.Generic;
using System.Linq;
using TheSingularityWorkshop.FSM_API;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class SingularityWorkshopHub : EditorWindow
{
    [MenuItem("Singularity/Workshop Hub", false, 0)]
    public static void ShowWindow() => GetWindow<SingularityWorkshopHub>("Singularity Hub");

    // --- Tab Registry ---
    private readonly List<IHubTabBuilder> _tabs = new List<IHubTabBuilder>
    {
        new StatusTabBuilder(),       
        new FsmCrudTabBuilder(),      
        new StateCrudTabBuilder(),    
        new StateMethodsCrudTabBuilder(),
        new LogicCrudTabBuilder(),     
        new LogicMethodsCrudTabBuilder()
    };

    private string _activeTabName;
    private VisualElement _contentViewport;

    private void OnEnable()
    {
        if (string.IsNullOrEmpty(_activeTabName))
            _activeTabName = _tabs[0].TabName;

        Refresh();
    }

    private void Refresh()
    {
        rootVisualElement.Clear();

        var hubLayout = new GraphicalUserInterfaceBuilder("HubRoot")
            .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
            .WithAutoGrow()
            .WithBackgroundColor(new Color(0.12f, 0.12f, 0.12f))
            .AddChild(CreateTopTabs())        // Step 1: Horizontal Navigation
            .AddChild(CreateHeaderBranding()) // Step 2: Metrics & Logo
            .AddChild(CreateMainViewport());  // Step 3: Tab Content

        rootVisualElement.Add(hubLayout.Build());
    }

    private IGuiProvider CreateMainViewport()
    {
        return new GraphicalUserInterfaceBuilder("ViewportContainer")
         .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
         .WithAutoGrow() // Ensures it pushes to the bottom of the window
         .AddChild(ctx =>
         {
             // We initialize the viewport reference so RenderActiveTab can find it
             _contentViewport = new VisualElement
             {
                 name = "Viewport",
                 style = { flexGrow = 1 }
             };

             // Immediately fill it with the current tab's GUI
             RenderActiveTab();

             return _contentViewport;
         });
    }

    private IGuiProvider CreateTopTabs()
    {
        var navBar = new GraphicalUserInterfaceBuilder("TopTabs")
        .WithBackgroundColor(new Color(0.2f, 0.2f, 0.2f))
        .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
        .WithBorderBottomWidth(1)
        .WithBorderColor(Color.black);

        foreach (var tab in _tabs)
        {
            navBar.AddChild(ctx => CreateTabButton(tab));
        }

        return navBar;
    }

    private IGuiProvider CreateHeaderBranding()
    {
        return new GraphicalUserInterfaceBuilder("Header")
            .WithBackgroundColor(new Color(0.08f, 0.08f, 0.08f))
            .WithPadding(10)
            .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
            .AddChild(new Label("THE SINGULARITY WORKSHOP")
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 18, color = Color.white }
            })
            .AddChild(ctx => {
                var stats = new VisualElement { style = { flexDirection = FlexDirection.Row } };
                stats.Add(CreateHeaderStat("FSMs", FSM_API.Internal.TotalFsmDefinitionCount));
                stats.Add(CreateHeaderStat("Handles", FSM_API.Internal.TotalFsmHandleCount));
                return stats;
            });
    }


    private void RenderActiveTab()
    {
        _contentViewport.Clear();
        var activeBuilder = _tabs.FirstOrDefault(t => t.TabName == _activeTabName);
        if (activeBuilder != null)
        {
            // We pass a context that allows the tab to trigger a full window refresh if needed
            var ctx = new GuiContext { EditMode = true, OnBuilt = _ => Refresh() };
            _contentViewport.Add(activeBuilder.CreateGui(ctx));
        }
    }

    // --- Helpers ---
    private VisualElement CreateTabButton(IHubTabBuilder tab)
    {
        bool isActive = _activeTabName == tab.TabName;
        // Basic button styling to look like a tab
        var btn = new Button(() => { _activeTabName = tab.TabName; Refresh(); })
        {
            text = tab.TabIcon + "  " + tab.TabName,
            style = {
                height = 30,
                paddingLeft = 15, paddingRight = 15,
                backgroundColor = isActive ? new Color(0.3f, 0.3f, 0.3f) : Color.clear,
                color = isActive ? new Color(0f, 0.8f, 1f) : Color.gray,
                borderBottomWidth = isActive ? 2 : 0,
                borderBottomColor = new Color(0f, 0.8f, 1f),
                unityFontStyleAndWeight = isActive ? FontStyle.Bold : FontStyle.Normal,
                marginRight = 2
            }
        };
        return btn;
    }

    private VisualElement CreateHeaderStat(string label, int value)
    {
        var el = new VisualElement { style = { marginLeft = 15, alignItems = Align.Center } };
        el.Add(new Label(value.ToString()) { style = { color = Color.white, unityFontStyleAndWeight = FontStyle.Bold } });
        el.Add(new Label(label) { style = { color = Color.gray, fontSize = 9 } });
        return el;
    }
}

// Interface for Hub Tabs
public interface IHubTabBuilder : IGuiProvider
{
    string TabName { get; }
    string TabIcon { get; }
}
#endif