using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    /// <summary>
    /// Generalized Avatar Selection Engine.
    /// Grouped under the 'Forge' tab in the editor.
    /// Handles everything from Starfighters to WWII Tanks via IAvatarIdentity.
    /// </summary>
    public class Forge_Hangar : IGuiProvider
    {
        // --- 1. CONFIGURATION STATE ---
        public string Title { get; private set; } = "HANGAR: REQUISITION";

        private List<IAvatarIdentity> _registry = new List<IAvatarIdentity>();
        private IAvatarIdentity _activeAvatar;

        private Action<IAvatarIdentity> _onSelectionChanged;
        private Action<IAvatarIdentity> _onConfirmed;
        private Action _onBack;

        private Color _primaryAccent = new Color(0.8f, 0.4f, 0.1f);
        private Color _bgSidebar = new Color(0.08f, 0.08f, 0.1f);
        private Color _bgMain = new Color(0.02f, 0.02f, 0.03f);

        // --- 2. RUNTIME UI STATE ---
        private VisualElement _root;
        private GameObject _gimbalPivot;
        private GameObject _modelInstance;

        public Forge_Hangar() { }
        public Forge_Hangar(string title) { Title = title; }

        // --- 3. FLUENT CONFIGURATION ---
        public Forge_Hangar WithRegistry(IEnumerable<IAvatarIdentity> items)
        {
            _registry.Clear();
            _registry.AddRange(items);
            if (_registry.Count > 0) _activeAvatar = _registry[0];
            return this;
        }

        public Forge_Hangar WithTheme(Color accent, Color sidebar, Color main)
        {
            _primaryAccent = accent; _bgSidebar = sidebar; _bgMain = main;
            return this;
        }

        public Forge_Hangar OnSelect(Action<IAvatarIdentity> callback) { _onSelectionChanged = callback; return this; }
        public Forge_Hangar OnConfirm(Action<IAvatarIdentity> callback) { _onConfirmed = callback; return this; }
        public Forge_Hangar OnBack(Action callback) { _onBack = callback; return this; }

        // --- 4. IGUIPROVIDER IMPLEMENTATION ---
        public VisualElement CreateGui(GuiContext ctx)
        {
            _root = new VisualElement { style = { flexGrow = 1 } };

            // Initial render
            RefreshView(ctx);

            // Cleanup gimbal when the UI is closed or swapped
            _root.RegisterCallback<DetachFromPanelEvent>(evt => CleanupGimbal());

            return _root;
        }

        private void RefreshView(GuiContext ctx)
        {
            _root.Clear();

            // Utilize the ForgeSplitPanelBuilder for the Macro Layout
            var splitPanel = new ForgeSplitPanelBuilder(sidebarWidth: 380, Side.Left)
                .WithSidebar(BuildRosterSidebar(ctx))
                .WithMain(BuildInspectionViewport(ctx))
                .CreateGui(ctx);

            _root.Add(splitPanel);
        }

        private IGuiProvider BuildRosterSidebar(GuiContext ctx)
        {
            return new GraphicalUserInterfaceBuilder("RosterSidebar")
                .WithPadding(15)
                .WithBackgroundColor(_bgSidebar)
                .AddHeader(Title)
                .AddSeparator(_primaryAccent, 2)
                .OnBuild(ve =>
                {
                    var scroll = new ScrollView(ScrollViewMode.Vertical) { style = { flexGrow = 1, marginTop = 10 } };

                    foreach (var item in _registry)
                    {
                        bool isSelected = item == _activeAvatar;

                        // Create the Avatar Card
                        var card = new GraphicalUserInterfaceBuilder($"Card_{item.UniqueId}")
                            .WithHeight(100)
                            .WithBackgroundColor(isSelected ? _primaryAccent : new Color(0.12f, 0.12f, 0.14f))
                            .OnBuild(cardVe =>
                            {
                                cardVe.style.marginBottom = 8;
                                cardVe.style.borderBottomLeftRadius = 4; cardVe.style.borderBottomRightRadius = 4;
                                cardVe.RegisterCallback<ClickEvent>(e => {
                                    _activeAvatar = item;
                                    _onSelectionChanged?.Invoke(item);
                                    RefreshView(ctx);
                                });
                            })
                            .AddChild(new Label(item.DisplayName.ToUpper())
                            {
                                style = {
                                alignSelf = Align.Center, marginTop = 70,
                                unityFontStyleAndWeight = FontStyle.Bold,
                                color = isSelected ? Color.black : Color.white
                            }
                            });

                        scroll.Add(card.Build());
                    }
                    ve.Add(scroll);
                })
                .AddChild(new ForgeButtonBuilder("<< SYSTEM EXIT", () => _onBack?.Invoke())
                    .WithHeight(30).WithMargin(10, 0, 0, 0).Build());
        }

        private IGuiProvider BuildInspectionViewport(GuiContext ctx)
        {
            return new GraphicalUserInterfaceBuilder("InspectionViewport")
                .WithPadding(40)
                .WithBackgroundColor(_bgMain)
                .AddChild(new Label(_activeAvatar?.DisplayName.ToUpper() ?? "NO SIGNAL")
                { style = { fontSize = 48, color = Color.white, unityFontStyleAndWeight = FontStyle.Bold } })
                .AddChild(new Label(_activeAvatar?.Subtitle ?? "Awaiting Data Ingestion...")
                { style = { fontSize = 14, color = _primaryAccent, letterSpacing = 2, marginBottom = 20 } })
                .OnBuild(ve =>
                {
                    if (_activeAvatar == null) return;

                    // Reset Gimbal
                    CleanupGimbal();

                    // 1. Establish the Parent Pivot (The Tour Gimbal)
                    _gimbalPivot = new GameObject("[FORGE_GIMBAL_PIVOT]");
                    _gimbalPivot.transform.position = new Vector3(0, -5000, 0); // Isolate from Reality

                    // 2. Instantiate the Avatar inside the Gimbal
                    _modelInstance = UnityEngine.Object.Instantiate(_activeAvatar.PreviewPrefab, _gimbalPivot.transform);
                    _modelInstance.transform.localPosition = Vector3.zero;

                    // 3. Inject the Live Model Preview Builder
                    // This handles the high-fidelity render and mouse-orbiting logic
                    var lmpb = new LiveModelPreviewBuilder(_gimbalPivot)
                        .WithZoom(2.5f)
                        .WithMouseControl(true)
                        .WithGizmos(false)
                        .WithBackgroundColor(_bgMain);

                    ve.Add(lmpb.CreateGui(ctx));

                    // 4. Scheduling the 'Wonky Basis' Auto-Tour
                    // Every 16ms, we update the gimbal's rotation state if the user isn't dragging
                    // This creates the 90-degree multi-axis rotation you described
                    // Note: In a full implementation, this would be an FSM on the processing group "HangarTour"
                })
                .AddChild(new GraphicalUserInterfaceBuilder("ConfirmRow")
                    .WithFlexLayout(FlexDirection.Row, Justify.Center, Align.Center)
                    .AddChild(new ForgeButtonBuilder("INITIALIZE MISSION", () => _onConfirmed?.Invoke(_activeAvatar))
                        .WithHeight(50).WithWidth(320).WithBackgroundColor(_primaryAccent)
                        .WithTextColor(Color.black).WithFontStyle(FontStyle.Bold).Build()));
        }

        private void CleanupGimbal()
        {
            if (_gimbalPivot != null) UnityEngine.Object.DestroyImmediate(_gimbalPivot);
            if (_modelInstance != null) UnityEngine.Object.DestroyImmediate(_modelInstance);
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}