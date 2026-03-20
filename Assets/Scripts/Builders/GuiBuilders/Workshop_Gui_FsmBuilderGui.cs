using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

using TheSingularityWorkshop.Forge.Builders.GuiBuilders.Themes;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;




#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
#endif

namespace TheSingularityWorkshop.Builders.GuiBuilders
{
    public class Workshop_Gui_FsmBuilderGui : IGuiProvider
    {
        public string Title => "FSM Command Deck";

        private FsmDefinition _activeDefinition;
        private GuiContext _lastCtx;
        private string _searchFilter = "";

        public IGuiProvider GetGuiBuilder(Action externalRefresh = null)
        {
            return new ThemeConfigurationGui(externalRefresh ?? Refresh);
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;
            FSM_DefinitionsLibrary.LoadRegistry();

            if (_activeDefinition == null)
            {
                var names = FSM_DefinitionsLibrary.GetRegisteredFsmNames();
                if (names.Count > 0) _activeDefinition = FSM_DefinitionsLibrary.LoadFsm(names[0]);
            }

            var container = new VisualElement()
                .Padding(10)
                .FlexGrow(1)
                .Background(GuiSkin.Active.GetProfile(GuiElementType.Window).BackgroundColor);

            container.userData = GuiElementType.Window;

            var splitBuilder = new SplitPanelBuilder(320)
                .WithSidebar(CreateRegistrySidebar())
                .WithMain(CreateConfigurationInterface());

            container.Add(splitBuilder.CreateGui(ctx));
            return container;
        }

        private IGuiProvider CreateRegistrySidebar()
        {
            var sidebar = new GraphicalUserInterfaceBuilder("Sidebar")
                .ApplySkin(GuiElementType.Panel)
                .WithPadding(15);

            sidebar.AddChild(c => {
                var header = new VisualElement().Margin(0).Padding(0, 0, 0, 15);
                header.userData = GuiElementType.Panel;

                header.Add(new Label("BLUEPRINT DATABASE")
                {
                    style = {
                        color = GuiSkin.Active.PrimaryAccent,
                        unityFontStyleAndWeight = FontStyle.Bold,
                        letterSpacing = 2
                    },
                    userData = GuiElementType.Label
                });

                var search = new TextField { value = _searchFilter, isDelayed = true };
                search.RegisterValueChangedCallback(e => { _searchFilter = e.newValue; Refresh(); });
                StyleGlassField(search);
                search.userData = GuiElementType.InputField;

                header.Add(search);
                return header;
            });

            sidebar.AddChild(c => {
                var btnProfile = GuiSkin.Active.GetProfile(GuiElementType.ButtonPrimary);
                var btn = new Button(CreateNewEmptyFsm) { text = "[ + ]  INITIALIZE SEQUENCE" }
                    .Height(35).Margin(15)
                    .Background(Color.clear)
                    .Border(1, GuiSkin.Active.PrimaryAccent)
                    .Radius(btnProfile.CornerRadius)
                    .Padding(0).Color(GuiSkin.Active.PrimaryAccent).Bold();

                btn.userData = GuiElementType.ButtonPrimary;
                return btn;
            });

            sidebar.AddChild(c => {
                var scroll = new ScrollView();
                var fsmNames = FSM_DefinitionsLibrary.GetRegisteredFsmNames()
                    .Where(n => string.IsNullOrEmpty(_searchFilter) || n.ToLower().Contains(_searchFilter.ToLower()))
                    .ToList();

                foreach (var name in fsmNames)
                {
                    bool isActive = _activeDefinition != null && _activeDefinition.Name == name;

                    var card = new Button(() => SelectFsm(name))
                        .Row().AlignCenter().Padding(10).Margin(4)
                        .Background(isActive ? (Color)(GuiSkin.Active.PrimaryAccent * 0.2f) : Color.clear)
                        .BorderWidth(0);

                    card.userData = GuiElementType.ButtonGhost;

                    if (isActive)
                    {
                        card.style.borderLeftWidth = 3;
                        card.style.borderLeftColor = GuiSkin.Active.PrimaryAccent;
                    }

                    var lblProfile = GuiSkin.Active.GetProfile(GuiElementType.Label);
                    card.Add(new Label(name)
                    {
                        style = {
                            color = isActive ? Color.white : lblProfile.TextColor,
                            unityFontStyleAndWeight = isActive ? FontStyle.Bold : FontStyle.Normal
                        },
                        userData = GuiElementType.Label
                    });
                    scroll.Add(card);
                }
                return scroll;
            });

            return sidebar;
        }

        private IGuiProvider CreateConfigurationInterface()
        {
            if (_activeDefinition == null)
                return new GraphicalUserInterfaceBuilder("Empty").AddChild(new Label("AWAITING SELECTION..") { style = { alignSelf = Align.Center, opacity = 0.5f, color = GuiSkin.Active.PrimaryAccent } });

            var editor = new GraphicalUserInterfaceBuilder("Editor")
                .WithPadding(0).WithMarginLeft(20).WithBackgroundColor(Color.clear);

            editor.AddChild(c => {
                var p = GuiSkin.Active.GetProfile(GuiElementType.Panel);
                var header = new VisualElement()
                    .Padding(20).Background(p.BackgroundColor)
                    .Border(1, p.BorderColor).Radius(0);

                header.userData = GuiElementType.Panel;
                header.style.borderBottomRightRadius = p.CornerRadius * 2;
                header.style.marginBottom = 20;

                var topRow = new VisualElement().Row().Padding(0, 0, 0, 15);
                var nameField = new TextField("IDENTITY") { value = _activeDefinition.Name }.FlexGrow(1);
                nameField.RegisterValueChangedCallback(e => _activeDefinition.Name = e.newValue);
                nameField.style.marginRight = 20;
                StyleGlassField(nameField);
                nameField.userData = GuiElementType.InputField;

                var groupField = new TextField("PROCESS GROUP") { value = _activeDefinition.Group }.Width(250);
                groupField.RegisterValueChangedCallback(e => _activeDefinition.Group = e.newValue);
                StyleGlassField(groupField);
                groupField.userData = GuiElementType.InputField;

                topRow.Add(nameField);
                topRow.Add(groupField);
                header.Add(topRow);

                var botRow = new VisualElement().Row().AlignCenter();
                var rateField = new IntegerField("CLOCK (HZ)") { value = _activeDefinition.Rate }.Width(150);
                rateField.RegisterValueChangedCallback(e => { _activeDefinition.Rate = e.newValue; Refresh(); });
                StyleGlassField(rateField);
                rateField.userData = GuiElementType.InputField;

                string rateHelp = _activeDefinition.Rate == -1 ? ">> CONTINUOUS <<" : $". THROTTLED: {_activeDefinition.Rate} .";
                Color helpColor = _activeDefinition.Rate == -1 ? GuiSkin.Active.PrimaryAccent : GuiSkin.Active.SecondaryAccent;
                var helpLabel = new Label(rateHelp) { style = { marginLeft = 15, fontSize = 9, color = helpColor, letterSpacing = 2, unityFontStyleAndWeight = FontStyle.Bold } };

                botRow.Add(rateField);
                botRow.Add(helpLabel);
                header.Add(botRow);
                return header;
            });

            editor.AddChild(c => {
                var workspace = new VisualElement().Row().FlexGrow(1);
                workspace.style.minHeight = 400;
                workspace.Add(CreateStateColumn().FlexGrow(1).Margin(5));
                workspace.Add(CreateTransitionColumn().FlexGrow(1).Margin(5));
                return workspace;
            });

            editor.AddChild(c => {
                var footer = new VisualElement().Row().Padding(10);
                footer.style.justifyContent = Justify.FlexEnd;

                var delBtn = new Button(DeleteCurrent) { text = "DELETE" }
                    .Background(new Color(GuiSkin.Active.AlertColor.r, GuiSkin.Active.AlertColor.g, GuiSkin.Active.AlertColor.b, 0.1f))
                    .Border(0, Color.clear);
                delBtn.style.color = GuiSkin.Active.AlertColor;
                delBtn.style.marginRight = 15;
                delBtn.userData = GuiElementType.ButtonAlert;

                var saveBtn = new Button(SaveCurrent) { text = "COMPILE DEFINITION" }
                    .Width(180).Height(35)
                    .Background(GuiSkin.Active.SecondaryAccent)
                    .Radius(GuiSkin.Active.GetProfile(GuiElementType.ButtonPrimary).CornerRadius);
                saveBtn.style.color = Color.black;
                saveBtn.style.unityFontStyleAndWeight = FontStyle.Bold;
                saveBtn.userData = GuiElementType.ButtonPrimary;

                footer.Add(delBtn);
                footer.Add(saveBtn);
                return footer;
            });

            return editor;
        }

        private VisualElement CreateStateColumn()
        {
            var p = GuiSkin.Active.GetProfile(GuiElementType.Panel);
            var col = new VisualElement()
                .Background(p.BackgroundColor)
                .Border(1, p.BorderColor)
                .Radius(p.CornerRadius)
                .Padding(15);
            col.userData = GuiElementType.Panel;

            col.Add(new Label("STATE_NODES")
            { style = { unityFontStyleAndWeight = FontStyle.Bold, color = GuiSkin.Active.PrimaryAccent, marginBottom = 10, letterSpacing = 2 } });

            var listScroll = new ScrollView().FlexGrow(1);
            var availableStates = FSM_DefinitionsLibrary.GetRegisteredStateNames();

            for (int i = 0; i < _activeDefinition.StateNames.Count; i++)
            {
                int idx = i;
                string sName = _activeDefinition.StateNames[i];
                bool isInit = sName == _activeDefinition.InitialState;
                bool exists = availableStates.Contains(sName);

                var row = new VisualElement().Row().AlignCenter().Padding(0, 0, 5, 5);
                row.style.borderBottomWidth = 1;
                row.style.borderBottomColor = new Color(1, 1, 1, 0.05f);

                var initBtn = new Button(() => { _activeDefinition.InitialState = sName; Refresh(); }) { text = isInit ? "★" : "○" }
                    .Width(25).Background(Color.clear).Border(0, Color.clear);
                initBtn.style.color = isInit ? new Color(1f, 0.8f, 0f) : Color.gray;

                var dd = new PopupField<string>(availableStates, sName).FlexGrow(1);
                dd.RegisterValueChangedCallback(e => { _activeDefinition.StateNames[idx] = e.newValue; Refresh(); });
                StyleGlassField(dd);
                dd.userData = GuiElementType.InputField;

                var actionBtn = new Button(exists ? (Action)(() => Debug.Log("Edit")) : () => ForgeNewState(sName))
                { text = exists ? "EDIT" : "FORGE" }
                    .Background(exists ? Color.clear : GuiSkin.Active.PrimaryAccent)
                    .Border(exists ? 1 : 0, GuiSkin.Active.PrimaryAccent);
                actionBtn.style.color = exists ? GuiSkin.Active.PrimaryAccent : Color.black;
                actionBtn.style.fontSize = 9;
                actionBtn.style.marginLeft = 5;
                actionBtn.userData = GuiElementType.ButtonSecondary;

                var delBtn = new Button(() => { _activeDefinition.StateNames.RemoveAt(idx); Refresh(); }) { text = "×" }
                    .Background(Color.clear).Border(0, Color.clear);
                delBtn.style.color = GuiSkin.Active.AlertColor;

                row.Add(initBtn);
                row.Add(dd);
                row.Add(actionBtn);
                row.Add(delBtn);
                listScroll.Add(row);
            }
            col.Add(listScroll);

            var addBtn = new Button(() => { _activeDefinition.StateNames.Add("NewState"); Refresh(); }) { text = "+ ADD NODE" }
                .Background(Color.clear).Border(1, GuiSkin.Active.PrimaryAccent).Padding(5).Margin(10)
                .Color(GuiSkin.Active.PrimaryAccent);
            addBtn.userData = GuiElementType.ButtonGhost;

            col.Add(addBtn);

            return col;
        }

        private VisualElement CreateTransitionColumn()
        {
            var p = GuiSkin.Active.GetProfile(GuiElementType.Panel);
            var col = new VisualElement()
                .Background(p.BackgroundColor)
                .Border(1, p.BorderColor)
                .Radius(p.CornerRadius)
                .Padding(15);
            col.userData = GuiElementType.Panel;

            col.Add(new Label("LOGIC_FLOW")
            { style = { unityFontStyleAndWeight = FontStyle.Bold, color = GuiSkin.Active.SecondaryAccent, marginBottom = 10, letterSpacing = 2 } });

            var listScroll = new ScrollView().FlexGrow(1);
            var localStates = new List<string>(_activeDefinition.StateNames);
            if (localStates.Count == 0) localStates.Add("No States");

            foreach (var t in _activeDefinition.Transitions)
            {
                var row = new VisualElement().Row().AlignCenter().Padding(5).Margin(4).Background(new Color(0, 0, 0, 0.2f));
                row.style.borderLeftWidth = 2;
                row.style.borderLeftColor = GuiSkin.Active.SecondaryAccent;

                var ddFrom = new PopupField<string>(localStates, localStates.Contains(t.FromState) ? t.FromState : localStates[0]).Width(70);
                ddFrom.RegisterValueChangedCallback(e => t.FromState = e.newValue);
                StyleGlassField(ddFrom);

                var ddTo = new PopupField<string>(localStates, localStates.Contains(t.ToState) ? t.ToState : localStates[0]).Width(70);
                ddTo.RegisterValueChangedCallback(e => t.ToState = e.newValue);
                StyleGlassField(ddTo);

                var cond = new TextField { value = t.ConditionMethod }.FlexGrow(1);
                cond.RegisterValueChangedCallback(e => t.ConditionMethod = e.newValue);
                StyleGlassField(cond);

                row.Add(ddFrom);
                row.Add(new Label(">") { style = { color = GuiSkin.Active.SecondaryAccent, fontSize = 8, marginLeft = 2, marginRight = 2 } });
                row.Add(cond);
                row.Add(new Label(">") { style = { color = GuiSkin.Active.SecondaryAccent, fontSize = 8, marginLeft = 2, marginRight = 2 } });
                row.Add(ddTo);

                var delBtn = new Button(() => { _activeDefinition.Transitions.Remove(t); Refresh(); }) { text = "×" }
                    .Background(Color.clear).Border(0, Color.clear).Width(20);
                delBtn.userData = GuiElementType.ButtonAlert;
                row.Add(delBtn);

                listScroll.Add(row);
            }
            col.Add(listScroll);

            col.Add(new Button(() => {
                string def = localStates[0];
                _activeDefinition.Transitions.Add(new FsmTransitionDefinition { FromState = def, ToState = def, ConditionMethod = "True" });
                Refresh();
            })
            { text = "+ ADD LINK" }
                .Background(Color.clear).Border(1, GuiSkin.Active.SecondaryAccent).Padding(5).Margin(10)
                .Color(GuiSkin.Active.SecondaryAccent));

            return col;
        }

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

        private void CreateNewEmptyFsm() { _activeDefinition = new FsmDefinition { Name = "New_FSM", Rate = -1 }; Refresh(); }
        private void SelectFsm(string name) { _activeDefinition = FSM_DefinitionsLibrary.LoadFsm(name); Refresh(); }
        private void ForgeNewState(string name) { FSM_DefinitionsLibrary.SaveState(new StateDefinition { Name = name }); Refresh(); }
        private void SaveCurrent() { if (_activeDefinition != null) FSM_DefinitionsLibrary.SaveFsm(_activeDefinition); Refresh(); }
        private void DeleteCurrent() { FSM_DefinitionsLibrary.DeleteFsm(_activeDefinition.Name); _activeDefinition = null; Refresh(); }
        private void Refresh() => _lastCtx?.OnBuilt?.Invoke(CreateGui(_lastCtx));

        public Action<VisualElement> GetGuiBuilder()
        {
            throw new NotImplementedException();
        }

#if UNITY_EDITOR
        public void ToUIDocument(string assetPath)
        {
            // Capture the live tree using the instance's context
            VisualElement myTree = CreateGui(_lastCtx ?? new GuiContext());
            GraphicalUserInterfaceBuilder.ConvertToUIDocument(myTree, assetPath);
        }
#endif

        public void FromUIDocument(string assetPath)
        {
            // Hydrate structure
            VisualElement hydratedUi = GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath);

            // Bind logic and inform the display system
            _lastCtx?.OnBuilt?.Invoke(hydratedUi);
        }

        // -------------------------------------------------------------------------
        // THE COWBELL 4.0: THE VISUAL STYLE DEBUGGER
        // -------------------------------------------------------------------------
        private class ThemeConfigurationGui : IGuiProvider
        {
            private Action _refreshCallback;
            private GuiElementType _activeTarget = GuiElementType.GlobalDefault;
            private IGuiProvider _selectedPreviewProvider;
            private Workshop_Gui_FsmBuilderGui _parent;

            // Hardcoded registry for now
            private List<IGuiProvider> _availableProviders = new List<IGuiProvider> {
                new Workshop_Gui_FsmBuilderGui()
            };

            public string Title => "Theme Calibration";
            public ThemeConfigurationGui(Action refresh)
            {
                _refreshCallback = refresh;
                if (_availableProviders.Count > 0) _selectedPreviewProvider = _availableProviders[0];
            }

            public VisualElement CreateGui(GuiContext ctx)
            {
                var theme = GuiSkin.Active;

                var root = new GraphicalUserInterfaceBuilder("ThemeConfig")
                    .ApplySkin(GuiElementType.Window)
                    .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch);

                // --- CAPTURE REFS FOR CROSS-PANE POPULATION ---
                ScrollView treeScrollRef = null;

                // --- 1. LEFT PANE: VISUAL HIERARCHY ---
                root.AddChild(c => {
                    var hierarchyPane = new VisualElement()
                        .FlexGrow(3).Padding(10).Margin(0, 10, 0, 0)
                        .Background(theme.GetProfile(GuiElementType.Panel).BackgroundColor)
                        .Border(1, theme.GetProfile(GuiElementType.Panel).BorderColor)
                        .Radius(5);

                    hierarchyPane.Add(new Label("TARGET INTERFACE") { style = { fontSize = 10, color = Color.gray, unityFontStyleAndWeight = FontStyle.Bold } });

                    var guiScroll = new ScrollView();
                    guiScroll.style.height = 100;
                    foreach (var prov in _availableProviders)
                    {
                        var btn = new Button(() => { _selectedPreviewProvider = prov; Refresh(); }) { text = prov.Title };
                        btn.style.backgroundColor = prov == _selectedPreviewProvider ? theme.PrimaryAccent : Color.clear;
                        guiScroll.Add(btn);
                    }
                    hierarchyPane.Add(guiScroll);

                    hierarchyPane.Add(new Label("VISUAL TREE INSPECTOR") { style = { marginTop = 10, fontSize = 10, color = Color.gray, unityFontStyleAndWeight = FontStyle.Bold } });

                    // Create and Capture the scroll view
                    treeScrollRef = new ScrollView();
                    treeScrollRef.FlexGrow(1);
                    hierarchyPane.Add(treeScrollRef);

                    return hierarchyPane;
                });

                // --- 2. MIDDLE PANE: LIVE PREVIEW ---
                VisualElement previewRoot = null;

                root.AddChild(c => {
                    var previewPane = new VisualElement()
                        .FlexGrow(4).Padding(20)
                        .Background(new Color(0.1f, 0.1f, 0.1f));

                    previewPane.generateVisualContent += DrawCheckerboard;

                    previewPane.Add(new Label("LIVE PREVIEW (Interactive)")
                    { style = { color = theme.PrimaryAccent, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10, alignSelf = Align.Center } });

                    if (_selectedPreviewProvider != null)
                    {
                        var previewCtx = new GuiContext { EditMode = false };
                        previewRoot = _selectedPreviewProvider.CreateGui(previewCtx);
                        AttachInspectors(previewRoot);
                        previewPane.Add(previewRoot);

                        // POPULATE TREE HERE (Now we have both previewRoot and treeScrollRef)
                        if (treeScrollRef != null) PopulateHierarchyTree(previewRoot, treeScrollRef, 0);
                    }

                    return previewPane;
                });

                // --- 3. RIGHT PANE: DNA INSPECTOR ---
                root.AddChild(c => {
                    var inspector = new VisualElement()
                        .FlexGrow(3).Padding(15)
                        .Background(new Color(0.15f, 0.15f, 0.15f))
                        .Border(0, Color.black);
                    inspector.style.borderLeftWidth = 1;

                    inspector.Add(new Label("STYLE DNA")
                    { style = { color = Color.white, unityFontStyleAndWeight = FontStyle.Bold, fontSize = 14, marginBottom = 5 } });

                    inspector.Add(new Label($"EDITING: {_activeTarget}")
                    { style = { color = theme.PrimaryAccent, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 15, fontSize = 10 } });

                    var profile = theme.GetOrCreateProfile(_activeTarget);
                    RenderProfileEditor(inspector, profile, theme);

                    var applyBtn = new Button(() => {
#if UNITY_EDITOR
                        EditorUtility.SetDirty(theme);
                        AssetDatabase.SaveAssets();
                        _refreshCallback?.Invoke();
#endif
                    })
                    { text = "APPLY THEME TO HUB" }
                    .Height(40).Margin(20, 0, 0, 0)
                    .Background(theme.SecondaryAccent).Color(Color.black).Bold();

                    inspector.Add(applyBtn);

                    return inspector;
                });

                return root.Build();
            }

            private void AttachInspectors(VisualElement root)
            {
                if (root.userData is GuiElementType type)
                {
                    root.RegisterCallback<MouseDownEvent>(e => {
                        _activeTarget = type;
                        Refresh();
                        e.StopPropagation();
                    }, TrickleDown.NoTrickleDown);
                }
                foreach (var child in root.Children()) AttachInspectors(child);
            }

            private void PopulateHierarchyTree(VisualElement root, VisualElement container, int depth)
            {
                if (root.userData is GuiElementType type)
                {
                    var label = new Button(() => { _activeTarget = type; Refresh(); })
                    {
                        text = $"{new string(' ', depth * 2)}[{type}] {root.name ?? root.GetType().Name}",
                        style = {
                            unityTextAlign = TextAnchor.MiddleLeft,
                            backgroundColor = _activeTarget == type ? new Color(0,1,1,0.2f) : Color.clear,
                             height = 18, fontSize = 10
                        }
                    };
                    container.Add(label);
                }
                foreach (var child in root.Children()) PopulateHierarchyTree(child, container, depth + 1);
            }

            private void DrawCheckerboard(MeshGenerationContext mgc)
            {
                var painter = mgc.painter2D;
                float size = 20f;
                var rect = mgc.visualElement.contentRect;
                int cols = Mathf.CeilToInt(rect.width / size);
                int rows = Mathf.CeilToInt(rect.height / size);

                for (int y = 0; y < rows; y++)
                {
                    for (int x = 0; x < cols; x++)
                    {
                        if ((x + y) % 2 == 0)
                        {
                            painter.fillColor = new Color(0.15f, 0.15f, 0.15f);
                            painter.BeginPath();
                            float px = x * size; float py = y * size;
                            painter.MoveTo(new Vector2(px, py));
                            painter.LineTo(new Vector2(px + size, py));
                            painter.LineTo(new Vector2(px + size, py + size));
                            painter.LineTo(new Vector2(px, py + size));
                            painter.ClosePath();
                            painter.Fill();
                        }
                    }
                }
            }

            private void RenderProfileEditor(VisualElement container, StyleProfile profile, GuiTheme theme)
            {
#if UNITY_EDITOR
                container.Add(new Label("SURFACE") { style = { color = Color.gray, fontSize = 10, marginTop = 5 } });
                container.Add(MakeColorField("Background", profile.BackgroundColor, v => profile.BackgroundColor = v));
                container.Add(MakeColorField("Border Color", profile.BorderColor, v => profile.BorderColor = v));

                container.Add(new Label("METRICS") { style = { color = Color.gray, fontSize = 10, marginTop = 10 } });
                container.Add(MakeSlider("Border Width", 0, 10, profile.BorderWidth, v => profile.BorderWidth = v));
                container.Add(MakeSlider("Corner Radius", 0, 50, profile.CornerRadius, v => profile.CornerRadius = v));

                container.Add(new Label("CONTENT") { style = { color = Color.gray, fontSize = 10, marginTop = 10 } });
                container.Add(MakeColorField("Text Color", profile.TextColor, v => profile.TextColor = v));

                container.Add(new Label("GLOBAL PALETTE") { style = { color = Color.gray, fontSize = 10, marginTop = 20 } });
                container.Add(MakeColorField("Primary", theme.PrimaryAccent, v => theme.PrimaryAccent = v));
                container.Add(MakeColorField("Secondary", theme.SecondaryAccent, v => theme.SecondaryAccent = v));
                container.Add(MakeColorField("Alert", theme.AlertColor, v => theme.AlertColor = v));
#endif
            }

#if UNITY_EDITOR
            private VisualElement MakeColorField(string label, Color val, Action<Color> set)
            {
                var cf = new ColorField(label) { value = val };
                cf.RegisterValueChangedCallback(e => { set(e.newValue); });
                return cf;
            }
            private VisualElement MakeSlider(string label, float min, float max, float val, Action<float> set)
            {
                var sl = new Slider(label, min, max) { value = val };
                sl.RegisterValueChangedCallback(e => { set(e.newValue); });
                return sl;
            }
#endif
            private void Refresh() => _refreshCallback?.Invoke();

            public Action<VisualElement> GetGuiBuilder()
            {
                throw new NotImplementedException();
            }

#if UNITY_EDITOR
            public void ToUIDocument(string assetPath)
            {
                // Use the stored _parent reference to access its instance fields
                VisualElement myTree = CreateGui(_parent._lastCtx ?? new GuiContext());
                GraphicalUserInterfaceBuilder.ConvertToUIDocument(myTree, assetPath);
            }
#endif

            public void FromUIDocument(string assetPath)
            {
                VisualElement hydratedUi = GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath);
                // Access parent instance field via reference
                _parent._lastCtx?.OnBuilt?.Invoke(hydratedUi);
            }
        }
    }
}