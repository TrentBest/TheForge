#if UNITY_EDITOR
using Assets.Editor.Singularity;
using TheSingularityWorkshop;
using TheSingularityWorkshop.Builders.GuiBuilders;

using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Workshop_Gui_FsmStateCrudTabBuilder : IGuiProvider
{
    public string TabName => "States";
    public string TabIcon => "🧩";

    public string Title => TabName;

    public VisualElement CreateGui(GuiContext ctx)
    {
        FSM_DefinitionsLibrary.LoadRegistry();

        var stateNames = FSM_DefinitionsLibrary.GetRegisteredStateNames();
        var initialState = stateNames.Count > 0
            ? FSM_DefinitionsLibrary.LoadState(stateNames[0])
            : new StateDefinition { Name = "New State" };

        var builder = new GraphicalUserInterfaceBuilder("StateTab");
        var sidebarProvider = new GraphicalUserInterfaceBuilder("Sidebar")
            .AddChild(new ListView(stateNames, 20, () => new Label(), (e, i) => (e as Label).text = stateNames[i]));

        builder.AddChild(new SplitPanelBuilder(300)
            .WithSidebar(sidebarProvider)
            .WithMain(CreateStateDetailArea(ctx, initialState)));

        builder.WithPanel("CRUD").WithFlexLayout(FlexDirection.Row, Justify.SpaceEvenly, Align.Auto);
        builder.AddChild(new Button(() => { new GraphicalUserInterfaceBuilder("Popup!").CreateGui(ctx); }) { text = "+" });
        builder.AddChild(new Button(() => { }) { text = "-" });
        builder.ContinueWithParentPanel();

        return builder.Build();
    }

    private IGuiProvider CreateStateDetailArea(GuiContext ctx, StateDefinition selectedState)
    {
        var detailBuilder = new GraphicalUserInterfaceBuilder("Detail")
            .WithPadding(15)
            .AddChild(new TextField("State Name:") { value = selectedState.Name })
            .AddChild(new DropDownBuilder("On Enter", FSM_DefinitionsLibrary.GetEnterMethods())
                .WithSelection(selectedState.EnterMethodName)
                .OnSelectionChanged(val => {
                    selectedState.EnterMethodName = val;
                    FSM_DefinitionsLibrary.SaveState(selectedState);
                }))
            .AddChild(new DropDownBuilder("On Update", FSM_DefinitionsLibrary.GetUpdateMethods())
                .WithSelection(selectedState.UpdateMethodName)
                .OnSelectionChanged(val => {
                    selectedState.UpdateMethodName = val;
                    FSM_DefinitionsLibrary.SaveState(selectedState);
                }))
             .AddChild(new DropDownBuilder("On Exit", FSM_DefinitionsLibrary.GetExitMethods())
                .WithSelection(selectedState.ExitMethodName)
                .OnSelectionChanged(val => {
                    selectedState.ExitMethodName = val;
                    FSM_DefinitionsLibrary.SaveState(selectedState);
                }));

        // Integrating the DLL Binding Area with equal column expansion
        detailBuilder.AddChild(CreateDllBindingArea(selectedState));

        return detailBuilder;
    }

    private VisualElement CreateDllBindingArea(StateDefinition state)
    {
        var row = new VisualElement();
        row.style.flexDirection = FlexDirection.Row;
        row.style.marginTop = 10;

        // Fix: Declare button first so the lambda can capture the reference
        Button bindButton = null;

        bindButton = new Button(() => {
            string path = UnityEditor.EditorUtility.OpenFilePanel("Select Behavior DLL", "", "dll");

            if (!string.IsNullOrEmpty(path))
            {
                bindButton.style.backgroundColor = new StyleColor(Color.green);
                bindButton.text = "DLL Loaded: " + System.IO.Path.GetFileName(path);
                // Reflection loading logic goes here
            }
        })
        { text = "Select Behavior DLL" };

        // Initial visual state
        bindButton.style.backgroundColor = new StyleColor(Color.red);
        bindButton.style.color = new StyleColor(Color.white);

        // Layout: flex-basis 0 and flex-grow 1 forces equal width regardless of content
        bindButton.style.flexGrow = 1;
        bindButton.style.flexBasis = 0;

        var statusLabel = new Label("Status: Disconnected");
        statusLabel.style.flexGrow = 1;
        statusLabel.style.flexBasis = 0;
        statusLabel.style.unityTextAlign = TextAnchor.MiddleCenter;

        row.Add(bindButton);
        row.Add(statusLabel);

        return row;
    }

    private List<string> GetMethodsWithNone(List<string> source)
    {
        var list = new List<string> { "None" };
        list.AddRange(source);
        return list;
    }

    private VisualElement CreateSmartDropdown(string label, List<string> options, GuiContext ctx)
    {
        var container = new VisualElement();
        container.Add(new PopupField<string>(label, options, 0));

        if (options.Count == 1)
        {
            container.Add(new Button(() => { })
            {
                text = "Register Methods..",
                style = { fontSize = 9, color = new StyleColor(Color.cyan) }
            });
        }
        return container;
    }

    public Action<VisualElement> GetGuiBuilder()
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
#endif