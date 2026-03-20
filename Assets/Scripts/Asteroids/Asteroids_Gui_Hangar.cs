#if UNITY_EDITOR
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using System.Collections.Generic;
using TheSingularityWorkshop.FSM_API;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheSingularityWorkshop.Asteroids.Editor
{
    public class Asteroids_Gui_Hangar : IGuiProvider
    {
        public string Title => "FIGHTER SELECTION";

        private AsteroidsContext _lastCtx;
        private GuiContext _guiContext;
        private VisualElement _rootContainer;
        private GameObject _selectedPrefab;
        private HangarTheme _theme;

        private Action _onBackCallback;
        private Action _onLaunchCallback;
        private Action _onOpenBuilderCallback;

        // Tracks the dynamically instantiated prefabs so we don't leak memory
        private List<GameObject> _previewInstances = new List<GameObject>();

        public Asteroids_Gui_Hangar()
        {
            _theme = new HangarTheme();
        }

        public Asteroids_Gui_Hangar(AsteroidsContext context, Action onBack = null, Action onLaunch = null, Action onOpenBuilder = null, HangarTheme theme = null)
        {
            _lastCtx = context;
            _onBackCallback = onBack;
            _onLaunchCallback = onLaunch;
            _theme = theme ?? new HangarTheme();
            _onOpenBuilderCallback = onOpenBuilder;

            if (_lastCtx != null && _lastCtx.fighterPrefabs != null && _lastCtx.fighterPrefabs.Count > 0)
                _selectedPrefab = _lastCtx.fighterPrefabs[0];
        }

        public VisualElement CreateGui(GuiContext context)
        {
            _guiContext = context;

            // Define the Root using a Builder
            _rootContainer = new GraphicalUserInterfaceBuilder("HangarRoot")
                .WithFlexGrow(1)
                .WithPercentSize(100, 100)
                .Build();

            Refresh();

            // When the panel is closed, clean up all the hidden 3D objects
            _rootContainer.RegisterCallback<DetachFromPanelEvent>(evt => CleanUpInstances());

            return _rootContainer;
        }

        private void Refresh()
        {
            CleanUpInstances();
            _rootContainer.Clear();

            var splitPanel = new SplitPanelBuilder(520, Side.Left)
                .WithSidebar(CreateSidebar())
                .WithMain(CreateMainView());

            _rootContainer.Add(splitPanel.CreateGui(_guiContext));
        }

        private void CleanUpInstances()
        {
            foreach (var inst in _previewInstances)
            {
                if (inst != null) UnityEngine.Object.DestroyImmediate(inst);
            }
            _previewInstances.Clear();
        }

        // Creates a safe instance for the preview builder to modify
        private GameObject CreatePreviewInstance(GameObject prefab)
        {
            if (prefab == null) return null;
            var inst = UnityEngine.Object.Instantiate(prefab);
            inst.name = "[HANGAR PREVIEW] " + prefab.name;
            _previewInstances.Add(inst);
            return inst;
        }

        private IGuiProvider CreateSidebar()
        {
            var sidebarBuilder = new GraphicalUserInterfaceBuilder("Hangar_Sidebar")
                .WithBackgroundColor(_theme.BaseBackground)
                .WithPadding(15)
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                .AddChild(new Label("HANGAR BAYS") { style = { color = _theme.TitleText, marginBottom = 15, fontSize = 18, unityFontStyleAndWeight = FontStyle.Bold } });

            // The Scrollable Grid Area
            sidebarBuilder.AddChild(guiCtx =>
            {
                var scroll = new ScrollView(ScrollViewMode.Vertical) { style = { flexGrow = 1 } };

                var gridBuilder = new GraphicalUserInterfaceBuilder("CardGrid")
                    .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.FlexStart)
                    .OnBuild(ve => { ve.style.flexWrap = Wrap.Wrap; ve.style.paddingBottom = 20; });

                if (_lastCtx != null && _lastCtx.fighterPrefabs != null)
                {
                    for (int i = 0; i < _lastCtx.fighterPrefabs.Count; i++)
                    {
                        var prefab = _lastCtx.fighterPrefabs[i];
                        if (prefab == null) continue;

                        bool isSelected = _selectedPrefab == prefab;
                        string mkDesignation = $"MK-{ToRoman(i + 1)}";

                        // Create the isolated 3D instance for this specific thumbnail
                        var thumbInstance = CreatePreviewInstance(prefab);

                        // The Interactive Card Builder
                        var cardBuilder = new GraphicalUserInterfaceBuilder($"Card_{mkDesignation}")
                            .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Center)
                            .WithBackgroundColor(isSelected ? _theme.PrimaryAccent : _theme.PanelBackground)
                            .OnBuild(ve => {
                                // 4 Columns math: 23% width + 1% margin on each side = 25% total per cell
                                ve.style.width = Length.Percent(23);
                                ve.style.marginLeft = Length.Percent(1); ve.style.marginRight = Length.Percent(1);
                                ve.style.height = 140; ve.style.marginBottom = 15;
                                ve.style.paddingTop = 10;
                                ve.style.borderTopLeftRadius = 10; ve.style.borderTopRightRadius = 10; ve.style.borderBottomLeftRadius = 10; ve.style.borderBottomRightRadius = 10;
                                ve.style.borderTopWidth = isSelected ? 2 : 1; ve.style.borderBottomWidth = isSelected ? 4 : 2;
                                ve.style.borderTopColor = isSelected ? _theme.BorderLight : _theme.BorderDark;
                                ve.style.borderBottomColor = _theme.BorderDark;

                                // Turn the element into a clickable button
                                ve.RegisterCallback<ClickEvent>(evt => { _selectedPrefab = prefab; Refresh(); });
                                ve.RegisterCallback<MouseEnterEvent>(evt => { if (!isSelected) ve.style.backgroundColor = new Color(0.3f, 0.3f, 0.35f); });
                                ve.RegisterCallback<MouseLeaveEvent>(evt => { if (!isSelected) ve.style.backgroundColor = _theme.PanelBackground; });
                            })
                            .AddChild(new Label(mkDesignation) { style = { color = isSelected ? _theme.TextActive : _theme.TextNormal, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } })

                            // Inject the LiveModelPreviewBuilder as a Thumbnail (Top-down, static)
                            .AddChild(new GraphicalUserInterfaceBuilder("ThumbWrapper")
                                .OnBuild(ve => {
                                    ve.style.width = 90; ve.style.height = 90;
                                    ve.style.borderTopLeftRadius = 45; ve.style.borderTopRightRadius = 45; ve.style.borderBottomLeftRadius = 45; ve.style.borderBottomRightRadius = 45;
                                    ve.style.overflow = Overflow.Hidden; // Makes the square render texture round!
                                    ve.style.borderTopWidth = 2; ve.style.borderBottomWidth = 2; ve.style.borderLeftWidth = 2; ve.style.borderRightWidth = 2;
                                    ve.style.borderTopColor = _theme.BorderDark; ve.style.borderBottomColor = _theme.BorderDark; ve.style.borderLeftColor = _theme.BorderDark; ve.style.borderRightColor = _theme.BorderDark;
                                })
                                .AddChild(new LiveModelPreviewBuilder(thumbInstance)
                                    .WithZoom(2.5f)
                                    .WithPitch(-90f) // Exact Top-Down angle requested
                                    .WithYaw(0f)
                                    .WithAutoOrbit(Vector3.up, 0f) // Disable rotation for thumbnails
                                    .WithMouseControl(false)
                                    .WithBackgroundColor(_theme.PanelBackground)
                                )
                            );

                        gridBuilder.AddChild(cardBuilder.Build());
                    }
                }

                scroll.Add(gridBuilder.Build());
                return scroll;
            });

            // Action Buttons
            sidebarBuilder
                .AddChild(new Button(() => _onOpenBuilderCallback?.Invoke()) { text = "CONSTRUCT CUSTOM FIGHTER", style = { height = 50, marginTop = 20, marginBottom = 10, backgroundColor = _theme.PrimaryAccent, color = _theme.TextActive, unityFontStyleAndWeight = FontStyle.Bold } })
                .AddChild(new Button(() => _onBackCallback?.Invoke()) { text = $"<< RETURN TO DECK.." });

            return sidebarBuilder;
        }

        private IGuiProvider CreateMainView()
        {
            int selectedIndex = _lastCtx?.fighterPrefabs?.IndexOf(_selectedPrefab) ?? -1;
            string designation = selectedIndex >= 0 ? $"MK-{ToRoman(selectedIndex + 1)}" : "NO SHIP";

            var mainBuilder = new GraphicalUserInterfaceBuilder("Hangar_Main")
                .WithBackgroundColor(_theme.BorderDark)
                .WithFlexLayout(FlexDirection.Column, Justify.SpaceBetween, Align.Center)
                .WithPadding(20)
                .WithPercentSize(100, 100)
                .AddChild(new Label($"INSPECTION: {designation}") { style = { color = _theme.TitleText, fontSize = 28, unityFontStyleAndWeight = FontStyle.Bold, letterSpacing = 3 } });

            if (_selectedPrefab != null)
            {
                var mainInstance = CreatePreviewInstance(_selectedPrefab);

                mainBuilder.AddChild(new GraphicalUserInterfaceBuilder("MainViewportWrapper")
                    .OnBuild(ve => {
                        ve.style.width = Length.Percent(95);
                        ve.style.minHeight = 400;
                        ve.style.flexGrow = 1;
                        ve.style.marginTop = 20; ve.style.marginBottom = 20;
                        ve.style.borderTopWidth = 2; ve.style.borderBottomWidth = 2;
                        ve.style.borderTopColor = _theme.PrimaryAccent; ve.style.borderBottomColor = _theme.PrimaryAccent;
                    })
                    // Inject the Main LiveModelPreviewBuilder 
                    // Starts Top-Down (-90) and auto-orbits so the user can inspect it
                    .AddChild(new LiveModelPreviewBuilder(mainInstance)
                        .WithZoom(1.8f)
                        .WithPitch(-90f) // Matches the thumbnail exactly as requested
                        .WithYaw(0f)
                        .WithAutoOrbit(Vector3.up, 15f) // Slowly spins
                        .WithMouseControl(true)
                        .WithBackgroundColor(new Color(0.05f, 0.05f, 0.06f)) // Slightly darker for depth
                    )
                );
            }

            // Controls Row
            mainBuilder.AddChild(new GraphicalUserInterfaceBuilder("ControlsRow")
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceEvenly, Align.Center)
                .WithWidth((Mathf.CeilToInt(Length.Percent(100).value)))
                .AddChild(new Button(() => Debug.Log("PEW PEW!")) { text = "TEST WEAPON SYSTEMS", style = { height = 45, width = 220, backgroundColor = _theme.PanelBackground, color = _theme.TextNormal } })
                .AddChild(new Button(() => _onLaunchCallback?.Invoke()) { text = "INITIATE LAUNCH", style = { height = 45, width = 280, backgroundColor = _theme.PrimaryAccent, color = _theme.TextActive, unityFontStyleAndWeight = FontStyle.Bold, fontSize = 16 } })
            );

            return mainBuilder;
        }

        private string ToRoman(int number)
        {
            if (number < 1) return string.Empty;
            if (number >= 10) return "X" + ToRoman(number - 10);
            if (number >= 9) return "IX" + ToRoman(number - 9);
            if (number >= 5) return "V" + ToRoman(number - 5);
            if (number >= 4) return "IV" + ToRoman(number - 4);
            if (number >= 1) return "I" + ToRoman(number - 1);
            return string.Empty;
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void FromUIDocument(string assetPath) => _guiContext?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
    }
}
#endif