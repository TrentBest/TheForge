#if UNITY_EDITOR
using Assets.Scripts.Builders.GuiBuilders;
using Assets.Scripts.Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Diagnostics;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    public class Workshop_Gui_FsmStateCrudTabBuilder : IGuiProvider
    {
        public string TabName => "States";
        public string TabIcon => "🧩";
        public string Title => TabName;

        private GuiContext _lastCtx;
        private StateDefinition _selectedState;

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;
            FSM_DefinitionsLibrary.LoadRegistry();

            var stateNames = FSM_DefinitionsLibrary.GetRegisteredStateNames();
            if (_selectedState == null && stateNames.Count > 0)
            {
                _selectedState = FSM_DefinitionsLibrary.LoadState(stateNames[0]);
            }

            // ROOT: Forge Container using granular layout protocols
            var root = new ForgeContainerBuilder("StateTab_Root")
                .WithPadding(10)
                .WithFlexGrow(1);

            // 1. Sidebar: State Registry List
            var sidebar = new ForgeContainerBuilder("StateSidebar")
                .WithPadding(5)
                .WithBackgroundColor(new Color(0.15f, 0.15f, 0.15f, 0.5f))
                .OnBuild(ve => {
                    var listView = new ListView(stateNames, 25, () => new Label(), (e, i) => {
                        var lbl = e as Label;
                        lbl.text = stateNames[i];
                        lbl.style.color = Color.white;
                        lbl.style.paddingLeft = 5;
                    });

                    listView.onSelectionChange += (objs) => {
                        if (objs.FirstOrDefault() is string sName)
                        {
                            _selectedState = FSM_DefinitionsLibrary.LoadState(sName);
                            Refresh();
                        }
                    };

                    ve.Add(listView);
                });

            // 2. Main Interface: State Detail Editor
            var splitBuilder = new ForgeSplitPanelBuilder(300)
                .WithSidebar(sidebar)
                .WithMain(CreateStateDetailArea(ctx, _selectedState ?? new StateDefinition { Name = "New State" }));

            root.AddChild(splitBuilder);

            // 3. CRUD Toolbar
            var toolbar = new ForgeContainerBuilder("CrudToolbar")
                .WithDirection(FlexDirection.Row)
                .WithJustifyContent(Justify.SpaceEvenly)
                .WithPadding(5)
                .WithMarginTop(10)
                .AddChild(new ForgeButtonBuilder("+", () => { /* Create Logic */ })
                    .WithFlexGrow(1).WithMarginRight(5).WithBackgroundColor(new Color(0.2f, 0.4f, 0.2f)))
                .AddChild(new ForgeButtonBuilder("-", () => { /* Delete Logic */ })
                    .WithFlexGrow(1).WithBackgroundColor(new Color(0.4f, 0.2f, 0.2f)));

            root.AddChild(toolbar);

            return root.CreateGui(ctx);
        }

        private IGuiProvider CreateStateDetailArea(GuiContext ctx, StateDefinition selectedState)
        {
            var detail = new ForgeContainerBuilder("StateDetail")
                .WithPadding(15)
                .WithFlexGrow(1);

            detail.AddChild(new ForgeLabelBuilder($"EDITING: {selectedState.Name}")
                .WithFontSize(18).WithBold().WithMarginBottom(15).WithColor(Color.cyan));

            // Identity Binding
            detail.AddChild(new ForgeTextFieldBuilder("State Name", selectedState.Name)
                .OnChanged(val => { selectedState.Name = val; FSM_DefinitionsLibrary.SaveState(selectedState); }));

            // Logic Hook Bindings using Forge Dropdowns
            detail.AddChild(new ForgeDropdownBuilder("On Enter", FSM_DefinitionsLibrary.GetEnterMethods())
                .WithSelection(selectedState.EnterMethodName)
                .OnChanged(val => { selectedState.EnterMethodName = val; FSM_DefinitionsLibrary.SaveState(selectedState); }));

            detail.AddChild(new ForgeDropdownBuilder("On Update", FSM_DefinitionsLibrary.GetUpdateMethods())
                .WithSelection(selectedState.UpdateMethodName)
                .OnChanged(val => { selectedState.UpdateMethodName = val; FSM_DefinitionsLibrary.SaveState(selectedState); }));

            detail.AddChild(new ForgeDropdownBuilder("On Exit", FSM_DefinitionsLibrary.GetExitMethods())
                .WithSelection(selectedState.ExitMethodName)
                .OnChanged(val => { selectedState.ExitMethodName = val; FSM_DefinitionsLibrary.SaveState(selectedState); }));

            // DLL Reflection Area
            detail.AddChild(CreateDllBindingArea(selectedState));

            return detail;
        }

        private IGuiProvider CreateDllBindingArea(StateDefinition state)
        {
            var bindingRow = new ForgeContainerBuilder("DllBinding")
                .WithDirection(FlexDirection.Row)
                .WithMarginTop(20)
                .WithPadding(10)
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.1f))
                .WithBorderRadius(5);

            string currentDll = "Select Behavior DLL";
            Color btnColor = Color.red;

            var bindButton = new ForgeButtonBuilder(currentDll, () => {
                string path = UnityEditor.EditorUtility.OpenFilePanel("Select Behavior DLL", "", "dll");
                if (!string.IsNullOrEmpty(path))
                {
                    ForgeLogger.Log($"[FSM_FORGE] DLL Linked: {System.IO.Path.GetFileName(path)}");
                    Refresh();
                }
            })
            .WithFlexGrow(1)
            .WithBackgroundColor(btnColor)
            .WithTextColor(Color.white);

            bindingRow.AddChild(bindButton);
            bindingRow.AddChild(new ForgeLabelBuilder("Status: Disconnected")
                .WithFlexGrow(1).WithTextAlign(TextAnchor.MiddleCenter));

            return bindingRow;
        }

        private void Refresh() => _lastCtx?.OnBuilt?.Invoke(CreateGui(_lastCtx));

        public Action<VisualElement> GetGuiBuilder() => container => container.Add(CreateGui(new GuiContext()));

        public void ToUIDocument(string assetPath)
        {
            var root = CreateGui(_lastCtx ?? new GuiContext());
            GraphicalUserInterfaceBuilder.ConvertToUIDocument(root, assetPath);
        }

        public void FromUIDocument(string assetPath)
        {
            var hydrated = GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath);
            _lastCtx?.OnBuilt?.Invoke(hydrated);
        }
    }
}
#endif