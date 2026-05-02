using Assets.Scripts.Asteroids;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using TheSingularityWorkshop.FSM_API;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Workshop.Asteroids
{
    public class Asteroids_Gui_Hangar : IGuiProvider
    {
        public string Title => "STAR SPARROW ARCHIVE";

        private AsteroidsContext _lastCtx;
        private GuiContext _guiContext;
        private VisualElement _rootContainer;
        private GameObject _selectedPrefab;
        private IGuiRouter _router;
        private HangarTheme _theme;

        private GameObject _mainPreviewPivot; // The "Gimbal" for the tour
        private GameObject _mainModelInstance; // The actual model for user interaction
        private List<GameObject> _previewInstances = new List<GameObject>();

        public Asteroids_Gui_Hangar(IGuiRouter router)
        {
            _router = router;
            _theme = new HangarTheme();
            _lastCtx = UnityEngine.Object.FindAnyObjectByType<AsteroidsContext>();

            if (_lastCtx?.fighterPrefabs != null && _lastCtx.fighterPrefabs.Count > 0)
                _selectedPrefab = _lastCtx.fighterPrefabs[0];
        }

        public VisualElement CreateGui(GuiContext context)
        {
            _guiContext = context;
            _rootContainer = new ForgeContainerBuilder("HangarRoot")
                .WithPercentSize(100, 100)
                .CreateGui(context);

            Refresh();

            _rootContainer.RegisterCallback<DetachFromPanelEvent>(evt => CleanUpInstances());
            return _rootContainer;
        }

        private void Refresh()
        {
            CleanUpInstances();
            _rootContainer.Clear();

            // Establish the Macro Layout using the ForgeSplitPanelBuilder
            var splitPanel = new ForgeSplitPanelBuilder(350, Side.Left)
                .WithSidebar(BuildFighterColumn())
                .WithMain(BuildMainInspectionView());

            _rootContainer.Add(splitPanel.CreateGui(_guiContext));
        }

        private IGuiProvider BuildFighterColumn()
        {
            var column = new ForgeContainerBuilder("FighterColumn")
                .WithBackgroundColor(_theme.BaseBackground)
                .WithPadding(15)
                .AddChild(new ForgeLabelBuilder("FLEET REGISTRY")
                    .WithBold()
                    .WithColor(_theme.TitleText)
                    .WithFontSize(18)
                    .WithMarginBottom(10)
                    .OnBuild(ve => ve.style.letterSpacing = 2))
                .AddSeparator(_theme.PrimaryAccent, 2);

            var scroll = new ForgeScrollViewBuilder("FighterScrollList").WithFlexGrow(1);

            if (_lastCtx?.fighterPrefabs != null)
            {
                for (int i = 0; i < _lastCtx.fighterPrefabs.Count; i++)
                {
                    var prefab = _lastCtx.fighterPrefabs[i];
                    bool isSelected = _selectedPrefab == prefab;

                    // DATA CARD: Top-down thumbnail view
                    var card = new ForgeContainerBuilder($"Card_{i}")
                        .WithHeight(160)
                        .WithMarginBottom(10)
                        .WithBackgroundColor(isSelected ? _theme.PrimaryAccent : _theme.PanelBackground)
                        .WithBorderColor(isSelected ? Color.cyan : Color.clear)
                        .WithBorderWidth(isSelected ? 2 : 0)
                        .OnBuild(cardVe =>
                        {
                            cardVe.RegisterCallback<ClickEvent>(e => {
                                _selectedPrefab = prefab;
                                Refresh();
                            });
                        });

                    // Nested small LMPB for the "Top Down Image" look
                    card.AddChild(new LiveModelPreviewBuilder(InstantiatePreview(prefab))
                        .WithPitch(-90)
                        .WithZoom(2.5f)
                        .WithBackgroundColor(Color.clear)
                        .WithGizmos(false));

                    card.AddChild(new ForgeLabelBuilder(prefab.name.ToUpper())
                        .WithBold()
                        .WithAlignment(TextAnchor.MiddleCenter)
                        .WithColor(isSelected ? Color.black : Color.white)
                        .WithMarginBottom(5));

                    scroll.AddChild(card);
                }
            }

            column.AddChild(scroll);
            return column;
        }

        private IGuiProvider BuildMainInspectionView()
        {
            var mainView = new ForgeContainerBuilder("InspectionView")
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.07f))
                .WithPadding(20)
                .WithFlexGrow(1)
                .AddChild(new ForgeLabelBuilder($"INSPECTING: {_selectedPrefab?.name.ToUpper() ?? "NONE"}")
                    .WithFontSize(32)
                    .WithBold()
                    .WithColor(_theme.TitleText)
                    .WithMarginBottom(10)
                    .OnBuild(ve => ve.style.letterSpacing = 3));

            mainView.OnBuild(ve =>
            {
                if (_selectedPrefab == null) return;

                // 1. Create the Gimbal/Pivot for the Auto-Tour
                _mainPreviewPivot = new GameObject("[TOUR PIVOT]");
                _mainPreviewPivot.transform.position = new Vector3(0, -1000, 0); // Hide from main camera

                // 2. Add the FSM Brain to the PIVOT
                var fsmDriver = _mainPreviewPivot.AddComponent<FighterPreviewFSM>();
                fsmDriver.Initialize();

                // 3. Create the actual ship model inside the pivot
                _mainModelInstance = UnityEngine.Object.Instantiate(_selectedPrefab, _mainPreviewPivot.transform);
                _mainModelInstance.transform.localPosition = Vector3.zero;

                // 4. Wrap it in a massive LMPB
                var previewElement = new LiveModelPreviewBuilder(_mainPreviewPivot)
                    .WithZoom(2.0f)
                    .WithMouseControl(true) // This allows the Orbit Logic
                    .WithBackgroundColor(new Color(0.02f, 0.02f, 0.03f))
                    .WithGizmos(false)
                    .CreateGui(_guiContext);

                // 5. Wire the LMPB Mouse Input directly to the FSM's Interaction State!
                previewElement.RegisterCallback<PointerDownEvent>(e => fsmDriver.IsInteracting = true);
                previewElement.RegisterCallback<PointerUpEvent>(e => fsmDriver.IsInteracting = false);
                previewElement.RegisterCallback<PointerLeaveEvent>(e => fsmDriver.IsInteracting = false);

                // Update the HangarPreview FSM every frame
                ve.schedule.Execute(() => FSM_API.Interaction.Update("HangarPreview")).Every(16);

                ve.Add(previewElement);
            });

            // Action Footer
            mainView.AddChild(new ForgeContainerBuilder("Actions")
                .WithDirection(FlexDirection.Row)
                .WithJustifyContent(Justify.Center)
                .WithAlignItems(Align.Center)
                .WithMarginTop(20)
                // LINT FIX: Updated Route to "InGame" to match Asteroids_Playable_Alpha
                .AddChild(new ForgeButtonBuilder(">> LAUNCH MISSION <<", () => _router?.NavigateTo("InGame"))
                    .WithWidth(300)
                    .WithHeight(50)
                    .WithBold()
                    .WithFontSize(14)
                    .WithTextColor(Color.black)
                    .WithBackgroundColor(_theme.PrimaryAccent)
                    .OnBuild(ve => ve.style.letterSpacing = 2))
            );

            return mainView;
        }

        private GameObject InstantiatePreview(GameObject prefab)
        {
            var inst = UnityEngine.Object.Instantiate(prefab);
            inst.transform.position = new Vector3(0, -5000, 0); // Hide bleed
            _previewInstances.Add(inst);
            return inst;
        }

        private void CleanUpInstances()
        {
            foreach (var inst in _previewInstances)
            {
                if (inst != null)
                {
                    if (Application.isPlaying) UnityEngine.Object.Destroy(inst);
                    else UnityEngine.Object.DestroyImmediate(inst);
                }
            }

            if (_mainPreviewPivot != null)
            {
                if (Application.isPlaying) UnityEngine.Object.Destroy(_mainPreviewPivot);
                else UnityEngine.Object.DestroyImmediate(_mainPreviewPivot);
            }

            _previewInstances.Clear();
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));

#if UNITY_EDITOR
        public void ToUIDocument(string assetPath)
        {
            WorkshopUxmlBaker.Bake(CreateGui(new GuiContext()), "HangarRegistry");
        }
#else
        public void ToUIDocument(string assetPath) { }
#endif

        public void FromUIDocument(string assetPath) { }
    }
}