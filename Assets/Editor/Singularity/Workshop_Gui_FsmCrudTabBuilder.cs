using Assets.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders.Themes;
using UnityEngine;
using UnityEngine.UIElements;

namespace Singularity
{
    public class Workshop_Gui_FsmCrudTabBuilder : IGuiProvider
    {
        public string TabName => "FSM Definitions";
        public string TabIcon => "🛠️";
        public string Title => TabName;

        // Mock Data - These would pull from your Registry/Reflection systems
        private List<string> _behaviorMethods = new() { "None", "MoveToTarget", "AttackTarget", "PlayIdleAnim", "LogStatus" };
        private List<string> _conditionMethods = new() { "None", "ShouldReset", "IsTargetDead", "TimerExpired" };
        private List<string> _processGroups = new() { "EditorUpdate", "Forge_Cluster_01", "BIM_Automation_Sync" };

        private List<string> _definedStates = new(); // Local state tracker for the FSM being forged

        public VisualElement CreateGui(GuiContext ctx)
        {
            var theme = GuiSkin.Active;

            // MASTER LAYOUT: Header + Split Body
            var root = new GraphicalUserInterfaceBuilder("FsmForgeRoot")
                .WithPadding(10)
                .AddChild(BuildFsmHeader()) // Name, Rate, Group
                .AddChild(new ActionGuiProvider(context =>
                {
                    // MISSION: 2 Columns, 1 Row for States vs Transitions
                    var split = new SplitPanelBuilder(sidebarWidth: 500, side: Side.Left)
                        .WithSidebar(new ActionGuiProvider(BuildStateAssembly))
                        .WithMain(new ActionGuiProvider(BuildTransitionLogic));

                    return split.CreateGui(context);
                }))
                .Build();

            return root;
        }

        private IGuiProvider BuildFsmHeader()
        {
            return new GraphicalUserInterfaceBuilder("FsmHeader")
                .WithPadding(15)
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                .AddChild(new TextField("FSM NAME") { value = "NewExperienceFSM" }.FlexGrow(1).Margin(0, 10, 0, 0))
                .AddChild(new ActionGuiProvider(c => new PopupField<string>("PROCESS GROUP", _processGroups, 0).Width(200)))
                .AddChild(new ActionGuiProvider(c => new IntegerField("TICK RATE (MS)") { value = 33 }.Width(120)));
        }

        private VisualElement BuildStateAssembly(GuiContext ctx)
        {
            var container = new VisualElement().Padding(10).ApplyProfile(GuiElementType.Window);
            container.Add(new Label("STATE ASSEMBLY").Bold().Color(GuiSkin.Active.PrimaryAccent).FontSize(14).Margin(0, 0, 0, 10));

            var stateList = new ScrollView { name = "StateList" };
            container.Add(stateList);

            var addBtn = new Button(() => {
                stateList.Add(CreateStateRow("NewState", stateList));
            })
            { text = "+ ADD STATE FILAMENT" }.ApplyProfile(GuiElementType.ButtonSecondary);

            container.Add(addBtn);
            return container;
        }

        private VisualElement CreateStateRow(string defaultName, VisualElement list)
        {
            var row = new GraphicalUserInterfaceBuilder("StateRow")
                .WithPadding(8)
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                .AddChild(new ActionGuiProvider(c => {
                    var nameField = new TextField("State Name") { value = defaultName };
                    nameField.RegisterValueChangedCallback(evt => UpdateStateRegistry(evt.newValue));
                    _definedStates.Add(defaultName);
                    return nameField;
                }))
                // The 3 Behavior Filaments: OnEnter, OnUpdate, OnExit
                .AddChild(new ActionGuiProvider(c => new PopupField<string>("OnEnter", _behaviorMethods, 0)))
                .AddChild(new ActionGuiProvider(c => new PopupField<string>("OnUpdate", _behaviorMethods, 0)))
                .AddChild(new ActionGuiProvider(c => new PopupField<string>("OnExit", _behaviorMethods, 0)))
                .Build();

            row.Border(1, new Color(1, 1, 1, 0.1f)).Margin(0, 0, 0, 5);
            return row;
        }

        private VisualElement BuildTransitionLogic(GuiContext ctx)
        {
            var container = new VisualElement().Padding(10).ApplyProfile(GuiElementType.Window).Margin(10, 0, 0, 0);
            container.Add(new Label("TRANSITION RESET LOGIC").Bold().Color(GuiSkin.Active.SecondaryAccent).FontSize(14).Margin(0, 0, 0, 10));

            var transList = new ScrollView { name = "TransList" };
            container.Add(transList);

            var addBtn = new Button(() => {
                transList.Add(CreateTransitionRow(transList));
            })
            { text = "+ ADD LOGIC FILAMENT" }.ApplyProfile(GuiElementType.ButtonSecondary);

            container.Add(addBtn);
            return container;
        }

        private VisualElement CreateTransitionRow(VisualElement list)
        {
            var row = new GraphicalUserInterfaceBuilder("TransRow")
                .WithPadding(8)
                .AddChild(new TextField("Transition Name") { value = "Jump" })
                .AddChild(new ActionGuiProvider(c => new PopupField<string>("FROM STATE", _definedStates, 0)))
                .AddChild(new ActionGuiProvider(c => new PopupField<string>("TO STATE", _definedStates, 0)))
                .AddChild(new ActionGuiProvider(c => new PopupField<string>("CONDITION", _conditionMethods, 0)))
                .Build();

            row.Border(1, new Color(1, 1, 1, 0.1f)).Margin(0, 0, 0, 5);
            return row;
        }

        private void UpdateStateRegistry(string newName)
        {
            // Logic to keep the Transition dropdowns synced with defined state names
        }

        // --- Boilerplate Implementation ---
        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string p) { }
        public void FromUIDocument(string p) { }
    }
}