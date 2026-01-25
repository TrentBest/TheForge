#if UNITY_EDITOR
using Assets.Scripts;
using Assets.Scripts.Builders.GuiBuilders;
using Assets.Scripts.Builders.GuiBuilders.PanelBuilders;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class StateCrudTabBuilder : IHubTabBuilder
{
    public string TabName => "States";
    public string TabIcon => "🧩";

    private List<string> _stateRegistry = new List<string>(); // Mock registry
    private ListView _listView;
    private VisualElement _editorContainer;
    private Button _removeButton;


    public VisualElement CreateGui(GuiContext ctx)
    {
        FSM_DefinitionsLibrary.LoadRegistry();

        var builder = new GraphicalUserInterfaceBuilder("StateTab");
        builder.AddChild(new SplitPanelBuilder(300)
            .WithSidebar(FSM_DefinitionsLibrary.GetStates())
            .WithMain(CreateStateDetailArea(ctx, FSM_DefinitionsLibrary.GetStates()));

        
        builder.WithPanel("CRUD").WithFlexLayout(FlexDirection.Row, Justify.SpaceEvenly, Align.Auto);
        builder.AddChild(new Button(() => { AddStateEditor(builder); }) { text = "+" });
        builder.AddChild(new Button(() => { }) { text = "-" });
        builder.ContinueWithParentPanel();
        return builder.Build();
    }

    private IGuiProvider CreateStateDetailArea(GuiContext ctx, StateDefinition selectedState)
    {
        return new GraphicalUserInterfaceBuilder("Detail")
            .WithPadding(15)
            .AddChild(new TextField("State Name:") { value = selectedState.Name })

            // Purely Fluent Configuration
            .AddChild(new DropDownBuilder("On Enter", FSM_DefinitionsLibrary.GetEnterMethods())
                .WithSelection(selectedState.OnEnterMethod)
                .WithShortcut("Method Registry")
                .OnSelectionChanged(val => {
                    selectedState.OnEnterMethod = val;
                    FSM_DefinitionsLibrary.AddState(selectedState);
                }))

            .AddChild(new DropDownBuilder("On Update", FSM_DefinitionsLibrary.GetUpdateMethods())
                .WithSelection(selectedState.OnUpdateMethod)
                .OnSelectionChanged(val => {
                    selectedState.OnUpdateMethod = val;
                    SaveStateToFile(selectedState);
                }))

            .Build();
    }

    private List<string> GetMethodsWithNone(List<string> source)
    {
        var list = new List<string> { "None" }; // Always the default
        list.AddRange(source);
        return list;
    }

    private VisualElement CreateSmartDropdown(string label, List<string> options, GuiContext ctx)
    {
        var container = new VisualElement();
        container.Add(new PopupField<string>(label, options, 0));

        // Shortcut clue if registry is empty
        if (options.Count == 1) // Only "None" exists
        {
            container.Add(new Button(() => { /* Logic to switch Hub tabs */ })
            { text = "Register Methods...", style = { fontSize = 9, color = Color.cyan } });
        }
        return container;
    }

    public IGuiProvider AddStateEditor(GraphicalUserInterfaceBuilder builder)
    {
        var availableOnEnterMethods = FSM_DefinitionsLibrary.GetEnterMethods();
        var availableOnUpdateMethods = FSM_DefinitionsLibrary.GetUpdateMethods();
        var availableOnExitMethods = FSM_DefinitionsLibrary.GetExitMethods();

        builder.WithPanel("StateEditorPanel").WithFlexLayout(FlexDirection.Row, Justify.SpaceEvenly, Align.Auto);
        builder.AddChild(new TextField("State Name:"));
        builder.AddChild(new PopupField<string>("On Enter", availableOnEnterMethods, 0));
        builder.AddChild(new PopupField<string>("On Update", availableOnUpdateMethods, 0));
        builder.AddChild(new PopupField<string>("On Exit", availableOnExitMethods, 0));
        return builder;
    }


}
[System.Serializable]
public class StateDefinition
{
    public string Name;
    public string OnEnterMethod; // Maps to your registry strings
    public string OnUpdateMethod;
    public string OnExitMethod;
    public string Guid; // Useful for persistent linking in FSM Blueprints
}
#endif