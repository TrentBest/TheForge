using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Extensions;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Themes;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Assets.Scripts.Builders.GuiBuilders;

namespace TAssets.Scripts.Builders.GuiBuilders
{
    public class Workshop_Gui_FsmBuilderGui : IGuiProvider
    {
        public string Title => "FSM Command Deck";

        private FsmDefinition _activeDefinition;
        private GuiContext _lastCtx;
        private string _searchFilter = "";

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;
            FSM_DefinitionsLibrary.LoadRegistry();

            if (_activeDefinition == null)
            {
                var names = FSM_DefinitionsLibrary.GetRegisteredFsmNames();
                if (names.Count > 0) _activeDefinition = FSM_DefinitionsLibrary.LoadFsm(names[0]);
            }

            // ROOT: Forge Container replacing raw VisualElement with extensions
            var root = new ForgeContainerBuilder("FsmCommandDeck_Root")
                .WithPadding(10)
                .WithFlexGrow(1)
                .WithBackgroundColor(GuiSkin.Active.GetProfile(GuiElementType.Window).BackgroundColor)
                .OnBuild(ve => ve.userData = GuiElementType.Window);

            var splitBuilder = new ForgeSplitPanelBuilder(320)
                .WithSidebar(CreateRegistrySidebar())
                .WithMain(CreateConfigurationInterface());

            root.AddChild(splitBuilder);
            return root.CreateGui(ctx);
        }

        private IGuiProvider CreateRegistrySidebar()
        {
            var sidebar = new GraphicalUserInterfaceBuilder("Sidebar")
                .WithPadding(15)
                .WithBackgroundColor(GuiSkin.Active.GetProfile(GuiElementType.Panel).BackgroundColor);

            // 1. Sidebar Header & Search
            sidebar.AddChild(ctx => new ForgeContainerBuilder("SidebarHeader")
                .WithPadding(0, 0, 0, 15)
                .OnBuild(ve => ve.userData = GuiElementType.Panel)
                .AddChild(new ForgeLabelBuilder("BLUEPRINT DATABASE")
                    .WithColor(GuiSkin.Active.PrimaryAccent)
                    .WithBold()
                    .OnBuild(ve => {
                        ve.style.letterSpacing = 2;
                        ve.userData = GuiElementType.Label;
                    }))
                .AddChild(new ForgeTextFieldBuilder("Search", _searchFilter)
                    .OnChanged(val => { _searchFilter = val; Refresh(); })
                    .OnBuild(ve => {
                        StyleGlassField(ve);
                        ve.userData = GuiElementType.InputField;
                    }))
                .CreateGui(ctx));

            // 2. Initialize Button
            sidebar.AddChild(ctx => new ForgeButtonBuilder("[ + ]  INITIALIZE SEQUENCE", CreateNewEmptyFsm)
                .WithHeight(35).WithMargin(15)
                .WithBackgroundColor(Color.clear)
                .WithBorderWidth(1).WithBorderColor(GuiSkin.Active.PrimaryAccent)
                .WithBorderRadius(GuiSkin.Active.GetProfile(GuiElementType.ButtonPrimary).CornerRadius)
                .WithTextColor(GuiSkin.Active.PrimaryAccent).WithBold()
                .OnBuild(ve => ve.userData = GuiElementType.ButtonPrimary)
                .CreateGui(ctx));

            // 3. Scrollable List of FSMs
            sidebar.AddChild(ctx => {
                // LINT FIX: Replaced raw new ScrollView() with ForgeScrollViewBuilder
                var scrollBuilder = new ForgeScrollViewBuilder("FsmListScroll").WithFlexGrow(1);

                var fsmNames = FSM_DefinitionsLibrary.GetRegisteredFsmNames()
                    .Where(n => string.IsNullOrEmpty(_searchFilter) || n.ToLower().Contains(_searchFilter.ToLower()))
                    .ToList();

                foreach (var name in fsmNames)
                {
                    bool isActive = _activeDefinition != null && _activeDefinition.Name == name;
                    var cardColor = isActive ? (Color)(GuiSkin.Active.PrimaryAccent * 0.2f) : Color.clear;

                    var card = new ForgeButtonBuilder(name, () => SelectFsm(name))
                        .WithPadding(10).WithMargin(4)
                        .WithBackgroundColor(cardColor)
                        .WithBorderWidth(0)
                        .WithTextAlign(TextAnchor.MiddleLeft)
                        .OnBuild(ve => {
                            ve.userData = GuiElementType.ButtonGhost;
                            if (isActive)
                            {
                                ve.style.borderLeftWidth = 3;
                                ve.style.borderLeftColor = GuiSkin.Active.PrimaryAccent;
                            }
                        });

                    scrollBuilder.AddChild(card);
                }
                return scrollBuilder.CreateGui(ctx);
            });

            return sidebar;
        }

        private IGuiProvider CreateConfigurationInterface()
        {
            if (_activeDefinition == null)
                return new ForgeContainerBuilder("Empty")
                    .WithJustifyContent(Justify.Center)
                    .AddChild(new ForgeLabelBuilder("AWAITING SELECTION..")
                        .WithColor(GuiSkin.Active.PrimaryAccent).WithOpacity(0.5f).WithAlignment(TextAnchor.MiddleCenter));

            var editor = new GraphicalUserInterfaceBuilder("Editor")
                .WithPadding(0).WithMarginLeft(20).WithBackgroundColor(Color.clear);

            // 1. Editor Header: Identity & Clock Frequency
            editor.AddChild(ctx => {
                var p = GuiSkin.Active.GetProfile(GuiElementType.Panel);
                return new ForgeContainerBuilder("EditorHeader")
                    .WithPadding(20).WithBackgroundColor(p.BackgroundColor)
                    .WithBorderWidth(1).WithBorderColor(p.BorderColor)
                    .WithMarginBottom(20)
                    .OnBuild(ve => {
                        ve.userData = GuiElementType.Panel;
                        ve.style.borderBottomRightRadius = p.CornerRadius * 2;
                    })
                    .AddChild(new ForgeContainerBuilder("TopRow").WithDirection(FlexDirection.Row).WithPaddingBottom(15)
                        .AddChild(new ForgeTextFieldBuilder("IDENTITY", _activeDefinition.Name)
                            .WithFlexGrow(1).WithMarginRight(20)
                            .OnChanged(val => _activeDefinition.Name = val)
                            .OnBuild(ve => { StyleGlassField(ve); ve.userData = GuiElementType.InputField; }))
                        .AddChild(new ForgeTextFieldBuilder("PROCESS GROUP", _activeDefinition.Group)
                            .WithWidth(250)
                            .OnChanged(val => _activeDefinition.Group = val)
                            .OnBuild(ve => { StyleGlassField(ve); ve.userData = GuiElementType.InputField; })))
                    .AddChild(new ForgeContainerBuilder("BotRow").WithDirection(FlexDirection.Row).WithAlignItems(Align.Center)
                        .AddChild(new ForgeTextFieldBuilder("CLOCK (HZ)", _activeDefinition.Rate.ToString())
                            .WithWidth(150)
                            .OnChanged(val => { if (int.TryParse(val, out int r)) _activeDefinition.Rate = r; Refresh(); })
                            .OnBuild(ve => { StyleGlassField(ve); ve.userData = GuiElementType.InputField; }))
                        .AddChild(new ForgeLabelBuilder(_activeDefinition.Rate == -1 ? ">> CONTINUOUS <<" : $". THROTTLED: {_activeDefinition.Rate} .")
                            .WithMarginLeft(15).WithFontSize(9).WithBold()
                            .WithColor(_activeDefinition.Rate == -1 ? GuiSkin.Active.PrimaryAccent : GuiSkin.Active.SecondaryAccent)))
                    .CreateGui(ctx);
            });

            // 2. Logic Workspace
            editor.AddChild(ctx => new ForgeContainerBuilder("Workspace")
                .WithDirection(FlexDirection.Row).WithFlexGrow(1)
                .OnBuild(ve => ve.style.minHeight = 400)
                .AddChild(CreateStateColumn())
                .AddChild(CreateTransitionColumn())
                .CreateGui(ctx));

            // 3. Command Footer
            editor.AddChild(ctx => new ForgeContainerBuilder("Footer")
                .WithDirection(FlexDirection.Row).WithPadding(10).WithJustifyContent(Justify.FlexEnd)
                .AddChild(new ForgeButtonBuilder("DELETE", DeleteCurrent)
                    .WithMarginRight(15)
                    .WithBackgroundColor(new Color(GuiSkin.Active.AlertColor.r, GuiSkin.Active.AlertColor.g, GuiSkin.Active.AlertColor.b, 0.1f))
                    .WithTextColor(GuiSkin.Active.AlertColor)
                    .OnBuild(ve => ve.userData = GuiElementType.ButtonAlert))
                .AddChild(new ForgeButtonBuilder("COMPILE DEFINITION", SaveCurrent)
                    .WithWidth(180).WithHeight(35).WithBold()
                    .WithBackgroundColor(GuiSkin.Active.SecondaryAccent).WithTextColor(Color.black)
                    .WithBorderRadius(GuiSkin.Active.GetProfile(GuiElementType.ButtonPrimary).CornerRadius)
                    .OnBuild(ve => ve.userData = GuiElementType.ButtonPrimary))
                .CreateGui(ctx));

            return editor;
        }

        private IGuiProvider CreateStateColumn()
        {
            var p = GuiSkin.Active.GetProfile(GuiElementType.Panel);
            var col = new ForgeContainerBuilder("StateColumn")
                .WithPadding(15).WithMargin(5).WithFlexGrow(1)
                .WithBackgroundColor(p.BackgroundColor).WithBorderWidth(1).WithBorderColor(p.BorderColor).WithBorderRadius(p.CornerRadius)
                .OnBuild(ve => ve.userData = GuiElementType.Panel)
                .AddChild(new ForgeLabelBuilder("STATE_NODES").WithBold().WithColor(GuiSkin.Active.PrimaryAccent).WithMarginBottom(10));

            var availableStates = FSM_DefinitionsLibrary.GetRegisteredStateNames();

            // LINT FIX: Replaced raw ScrollView
            var listScroll = new ForgeScrollViewBuilder("StateScrollView").WithFlexGrow(1);

            for (int i = 0; i < _activeDefinition.StateNames.Count; i++)
            {
                int idx = i;
                string sName = _activeDefinition.StateNames[idx];
                bool isInit = sName == _activeDefinition.InitialState;

                var row = new ForgeContainerBuilder($"State_{idx}").WithDirection(FlexDirection.Row).WithAlignItems(Align.Center).WithPaddingBottom(5).WithMarginBottom(5)
                    .OnBuild(ve => { ve.style.borderBottomWidth = 1; ve.style.borderBottomColor = new Color(1, 1, 1, 0.05f); })
                    .AddChild(new ForgeButtonBuilder(isInit ? "★" : "○", () => { _activeDefinition.InitialState = sName; Refresh(); })
                        .WithWidth(25).WithBackgroundColor(Color.clear).WithTextColor(isInit ? new Color(1f, 0.8f, 0f) : Color.gray))
                    .AddChild(new ForgeLabelBuilder(sName).WithFlexGrow(1).OnBuild(ve => { ve.userData = GuiElementType.Label; }))
                    .AddChild(new ForgeButtonBuilder("×", () => { _activeDefinition.StateNames.RemoveAt(idx); Refresh(); })
                        .WithBackgroundColor(Color.clear).WithTextColor(GuiSkin.Active.AlertColor));

                listScroll.AddChild(row);
            }

            col.AddChild(listScroll);
            col.AddChild(new ForgeButtonBuilder("+ ADD NODE", () => { _activeDefinition.StateNames.Add("NewState"); Refresh(); })
                .WithMargin(10).WithPadding(5).WithBorderWidth(1).WithBorderColor(GuiSkin.Active.PrimaryAccent)
                .WithBackgroundColor(Color.clear).WithTextColor(GuiSkin.Active.PrimaryAccent));

            return col;
        }

        private IGuiProvider CreateTransitionColumn()
        {
            var p = GuiSkin.Active.GetProfile(GuiElementType.Panel);
            var col = new ForgeContainerBuilder("LogicFlowColumn")
                .WithPadding(15).WithMargin(5).WithFlexGrow(1)
                .WithBackgroundColor(p.BackgroundColor).WithBorderWidth(1).WithBorderColor(p.BorderColor).WithBorderRadius(p.CornerRadius)
                .OnBuild(ve => ve.userData = GuiElementType.Panel)
                .AddChild(new ForgeLabelBuilder("LOGIC_FLOW").WithBold().WithColor(GuiSkin.Active.SecondaryAccent).WithMarginBottom(10));

            // LINT FIX: Replaced raw ScrollView
            var listScroll = new ForgeScrollViewBuilder("TransitionScrollView").WithFlexGrow(1);

            var localStates = new List<string>(_activeDefinition.StateNames);
            if (localStates.Count == 0) localStates.Add("No States");

            foreach (var t in _activeDefinition.Transitions)
            {
                var row = new ForgeContainerBuilder("TransRow").WithDirection(FlexDirection.Row).WithAlignItems(Align.Center).WithPadding(5).WithMargin(4)
                    .WithBackgroundColor(new Color(0, 0, 0, 0.2f))
                    .OnBuild(ve => { ve.style.borderLeftWidth = 2; ve.style.borderLeftColor = GuiSkin.Active.SecondaryAccent; })
                    .AddChild(new ForgeLabelBuilder(t.FromState).WithWidth(70).WithFontSize(10))
                    .AddChild(new ForgeLabelBuilder(">").WithColor(GuiSkin.Active.SecondaryAccent).WithMargin(0, 0, 2, 2))
                    .AddChild(new ForgeTextFieldBuilder("Condition", t.ConditionMethod).WithFlexGrow(1).OnChanged(val => t.ConditionMethod = val).OnBuild(StyleGlassField))
                    .AddChild(new ForgeLabelBuilder(">").WithColor(GuiSkin.Active.SecondaryAccent).WithMargin(0, 0, 2, 2))
                    .AddChild(new ForgeLabelBuilder(t.ToState).WithWidth(70).WithFontSize(10))
                    .AddChild(new ForgeButtonBuilder("×", () => { _activeDefinition.Transitions.Remove(t); Refresh(); })
                        .WithBackgroundColor(Color.clear).WithTextColor(GuiSkin.Active.AlertColor).WithWidth(20));

                listScroll.AddChild(row);
            }

            col.AddChild(listScroll);
            col.AddChild(new ForgeButtonBuilder("+ ADD LINK", () => {
                string def = localStates[0];
                _activeDefinition.Transitions.Add(new FsmTransitionDefinition { FromState = def, ToState = def, ConditionMethod = "True" });
                Refresh();
            }).WithMargin(10).WithPadding(5).WithBorderWidth(1).WithBorderColor(GuiSkin.Active.SecondaryAccent)
              .WithBackgroundColor(Color.clear).WithTextColor(GuiSkin.Active.SecondaryAccent));

            return col;
        }

        public Action<VisualElement> GetGuiBuilder() => container => container.Add(CreateGui(new GuiContext()));

#if UNITY_EDITOR
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(_lastCtx ?? new GuiContext()), assetPath);
#endif

        public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));

        private void Refresh() => _lastCtx?.OnBuilt?.Invoke(CreateGui(_lastCtx));
        private void SelectFsm(string name) { _activeDefinition = FSM_DefinitionsLibrary.LoadFsm(name); Refresh(); }
        private void CreateNewEmptyFsm() { _activeDefinition = new FsmDefinition { Name = "New_FSM", Rate = -1 }; Refresh(); }
        private void SaveCurrent() { if (_activeDefinition != null) FSM_DefinitionsLibrary.SaveFsm(_activeDefinition); Refresh(); }
        private void DeleteCurrent() { if (_activeDefinition != null) { FSM_DefinitionsLibrary.DeleteFsm(_activeDefinition.Name); _activeDefinition = null; Refresh(); } }

        public static void StyleGlassField(VisualElement field)
        {
            field.schedule.Execute(() => {
                var input = field.Q(className: "unity-base-field__input");
                if (input != null)
                {
                    var p = GuiSkin.Active.GetProfile(GuiElementType.InputField);
                    input.style.backgroundColor = p.BackgroundColor;
                    input.style.borderTopWidth = 0; input.style.borderBottomWidth = 1; input.style.borderLeftWidth = 0; input.style.borderRightWidth = 0;
                    input.style.borderBottomColor = p.BorderColor;
                    input.style.color = p.TextColor;
                }
            });
        }
    }
}