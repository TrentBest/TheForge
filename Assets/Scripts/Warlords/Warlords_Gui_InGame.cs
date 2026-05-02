using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Diagnostics;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Tools;
using static Assets.Scripts.Warlords.Warlords_Gui_GameSetup;

namespace Assets.Scripts.Warlords
{
    public class Warlords_Gui_InGame : IGuiProvider
    {
        public string Title => "WARLORDS: ILLURIA";
        private GuiContext _lastCtx;

        // Faction / Turn tracking
        private VisualElement _turnOverlay;
        private int _currentFactionIndex = 0;
        private List<WarlordFactionData> _factions;

        // UI / Map Tracking
        private VisualElement _mapViewport;
        private ScrollView _mapScrollView;
        private Image _minimapImage;
        private VisualElement _minimapRect;
        private ModalOverlayBuilder _stackModal;

        // Map Data
        private WarlordsMapData _mapData;
        private TileManager _tileManager;
        private int _tileSize = 64; // Adjust this if your tile sprites are a different size

        public Warlords_Gui_InGame(IGuiRouter r)
        {
        }

        public Warlords_Gui_InGame()
        {
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            // 1. LOAD ASSETS
            LoadMapData();

            // 2. HARDENED ROOT
            var rootBuilder = new GraphicalUserInterfaceBuilder("Warlords_InGame_Root")
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                .OnBuild(ve => {
                    ve.style.width = Length.Percent(100);
                    ve.style.height = Length.Percent(100);
                    ve.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f);
                });

            // 3. MIDDLE SECTION
            var middleSplit = new ForgeSplitPanelBuilder(310, Side.Right);

            // ---> Main Map Viewport
            middleSplit.WithMain(new GraphicalUserInterfaceBuilder("MapViewport")
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.05f)) // Darker behind map
                .OnBuild(ve => {
                    _mapViewport = ve;
                    ve.style.flexGrow = 1;
                    ve.style.overflow = Overflow.Hidden; // Keep map contained
                })
                .AddChild(guiCtx => BuildMainMap())
            );

            // ---> Right Sidebar
            middleSplit.WithSidebar(new GraphicalUserInterfaceBuilder("InGame_Sidebar")
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                .WithBackgroundColor(new Color(0.15f, 0.15f, 0.15f))
                .AddChild(CreateCommandStripBuilder().Build())
                .AddChild(new GraphicalUserInterfaceBuilder("MiniMap_Area")
                    .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                    .OnBuild(ve => { ve.style.flexGrow = 1; ve.style.paddingLeft = 10; ve.style.paddingRight = 10; })
                    .AddChild(new Label("WORLD MAP") { style = { unityTextAlign = TextAnchor.MiddleCenter, color = Color.gray, marginTop = 10 } })
                    .AddChild(guiCtx => BuildMinimap()) // Inject Minimap here
                    .Build())
            );

            rootBuilder.AddChild(middleSplit.CreateGui(ctx));
            rootBuilder.AddChild(CreateClassicBottomBarBuilder().Build());

            var finalRoot = rootBuilder.Build();

            // 4. SAFETY OVERLAY: Stack Selection Modal
            finalRoot.schedule.Execute(() => {
                var stackContent = new GraphicalUserInterfaceBuilder("StackContent")
                    .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                    .WithBackgroundColor(new Color(0.12f, 0.12f, 0.15f))
                    .OnBuild(ve => {
                        ve.style.width = 400;
                        ve.style.paddingTop = 20; ve.style.paddingBottom = 20;
                        ve.style.paddingRight = 20; ve.style.paddingLeft = 20;
                    })
                    .AddChild(new Label("UNIT STACK") { style = { color = Color.yellow, marginBottom = 15, unityFontStyleAndWeight = FontStyle.Bold } })
                    .AddButton("GROUP 1: Cavalry x2, Infantry x4", () => _stackModal.Hide())
                    .AddButton("GROUP 2: Hero, Dragon", () => _stackModal.Hide())
                    .AddSeparator()
                    .AddButton("CLOSE", () => _stackModal.Hide());

                _stackModal = new ModalOverlayBuilder(stackContent)
                    .WithBackdropColor(new Color(0, 0, 0, 0.8f));

                var modalElement = _stackModal.CreateGui(ctx);
                _stackModal.Hide();
                finalRoot.Add(modalElement);
            }).ExecuteLater(50);

            // Call initial minimap update once layout completes
            finalRoot.schedule.Execute(UpdateMinimapRect).ExecuteLater(100);

            return finalRoot;
        }

        private void LoadMapData()
        {
            _tileManager = UnityEngine.Object.FindFirstObjectByType<TileManager>();

            TextAsset jsonAsset = UnityEngine.Resources.Load<TextAsset>("Json/Illuria_Extracted");
            if (jsonAsset != null)
            {
                _mapData = JsonUtility.FromJson<WarlordsMapData>(jsonAsset.text);
            }
            else
            {
                ForgeLogger.LogError("Could not find map JSON at Assets/Resources/Json/Illuria_Extracted.json");
            }
        }

        private VisualElement BuildMainMap()
        {
            // FIX: 'Both' is replaced with 'VerticalAndHorizontal'
            _mapScrollView = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
            _mapScrollView.style.flexGrow = 1;

            if (_mapData == null) return _mapScrollView;

            var mapContainer = new VisualElement();
            // Size the container exactly to the map dimensions
            mapContainer.style.width = _mapData.Width * _tileSize;
            mapContainer.style.height = _mapData.Height * _tileSize;
            mapContainer.style.flexDirection = FlexDirection.Row;
            mapContainer.style.flexWrap = Wrap.Wrap;

            // Unity Texture coordinates (and your Importer) start 0,0 at bottom-left.
            // UI Elements draw top-to-bottom. So we iterate Y backwards to ensure the map isn't upside down.
            for (int y = _mapData.Height - 1; y >= 0; y--)
            {
                for (int x = 0; x < _mapData.Width; x++)
                {
                    int index = y * _mapData.Width + x;
                    TerrainType t = _mapData.TerrainData[index];
                    var tileImage = new Image();

                    if (_tileManager != null && _tileManager.IsValid)
                    {
                        // This now works for BOTH Terrain and POI because of the new overloads
                        tileImage.sprite = _tileManager.GetSprite(t);
                    }

                    mapContainer.Add(tileImage);
                }
            }

            _mapScrollView.Add(mapContainer);

            // Link ScrollView movement to the Minimap Rectangle
            _mapScrollView.RegisterCallback<GeometryChangedEvent>(e => UpdateMinimapRect());
            _mapScrollView.verticalScroller.valueChanged += (v) => UpdateMinimapRect();
            _mapScrollView.horizontalScroller.valueChanged += (v) => UpdateMinimapRect();

            return _mapScrollView;
        }

        private VisualElement BuildMinimap()
        {
            var minimapContainer = new VisualElement();
            minimapContainer.style.width = 200; // Fixed width constraint for sidebar
            minimapContainer.style.alignSelf = Align.Center;
            minimapContainer.style.marginTop = 10;
            minimapContainer.style.position = Position.Relative;

            if (_mapData == null) return minimapContainer;

            // Generate Map Texture
            Texture2D minimapTex = new Texture2D(_mapData.Width, _mapData.Height, TextureFormat.RGBA32, false);
            minimapTex.filterMode = FilterMode.Point; // Keep it crisp

            Color[] pixels = new Color[_mapData.Width * _mapData.Height];
            for (int y = 0; y < _mapData.Height; y++)
            {
                for (int x = 0; x < _mapData.Width; x++)
                {
                    int index = y * _mapData.Width + x;
                    pixels[index] = GetColorForTerrain(_mapData.TerrainData[index]);
                }
            }
            minimapTex.SetPixels(pixels);
            minimapTex.Apply();

            _minimapImage = new Image { image = minimapTex, scaleMode = ScaleMode.ScaleToFit };
            _minimapImage.style.flexGrow = 1;

            // FIX: Pass the Vector2 localPosition directly using lambda expressions
            _minimapImage.RegisterCallback<PointerDownEvent>(e => OnMinimapPointer(e.localPosition));
            _minimapImage.RegisterCallback<PointerMoveEvent>(e => { if (e.pressedButtons == 1) OnMinimapPointer(e.localPosition); });

            minimapContainer.Add(_minimapImage);

            // Viewport Rect (The White Box)
            _minimapRect = new VisualElement();
            _minimapRect.style.position = Position.Absolute;
            _minimapRect.style.borderTopWidth = 2; _minimapRect.style.borderBottomWidth = 2;
            _minimapRect.style.borderLeftWidth = 2; _minimapRect.style.borderRightWidth = 2;
            _minimapRect.style.borderTopColor = Color.white; _minimapRect.style.borderBottomColor = Color.white;
            _minimapRect.style.borderLeftColor = Color.white; _minimapRect.style.borderRightColor = Color.white;

            minimapContainer.Add(_minimapRect);

            return minimapContainer;
        }

        // FIX: Replaced 'PointerEventBase' with 'Vector2' to bypass generic issues
        private void OnMinimapPointer(Vector2 localPos)
        {
            if (_mapScrollView == null || _minimapImage == null) return;

            // Normalize (0 to 1)
            float nx = localPos.x / _minimapImage.layout.width;
            float ny = localPos.y / _minimapImage.layout.height;

            float contentW = _mapScrollView.contentContainer.layout.width;
            float contentH = _mapScrollView.contentContainer.layout.height;
            float viewW = _mapScrollView.layout.width;
            float viewH = _mapScrollView.layout.height;

            // Center the main viewport on the clicked relative coordinate
            float targetX = (nx * contentW) - (viewW / 2f);
            float targetY = (ny * contentH) - (viewH / 2f);

            // Clamp so we don't scroll past boundaries
            _mapScrollView.scrollOffset = new Vector2(
                Mathf.Clamp(targetX, 0, contentW - viewW),
                Mathf.Clamp(targetY, 0, contentH - viewH)
            );
        }

        private void UpdateMinimapRect()
        {
            if (_minimapRect == null || _mapScrollView == null || _minimapImage == null || float.IsNaN(_minimapImage.layout.width)) return;

            float contentW = _mapScrollView.contentContainer.layout.width;
            float contentH = _mapScrollView.contentContainer.layout.height;
            float viewW = _mapScrollView.layout.width;
            float viewH = _mapScrollView.layout.height;

            if (contentW <= 0 || contentH <= 0) return;

            float offX = _mapScrollView.scrollOffset.x;
            float offY = _mapScrollView.scrollOffset.y;

            // Calculate proportions relative to total content size
            float rx = offX / contentW;
            float ry = offY / contentH;
            float rw = viewW / contentW;
            float rh = viewH / contentH;

            // Apply to white rectangle based on rendered minimap size
            _minimapRect.style.left = rx * _minimapImage.layout.width;
            _minimapRect.style.top = ry * _minimapImage.layout.height;
            _minimapRect.style.width = rw * _minimapImage.layout.width;
            _minimapRect.style.height = rh * _minimapImage.layout.height;
        }

        private Color GetColorForTerrain(TerrainType t)
        {
            // Fallback colors mapping exactly to the WarlordsMapImporter
            switch (t)
            {
                case TerrainType.OpenWater: return new Color(0, 0, 1f);
                case TerrainType.Plains: return new Color(0, 1f, 0);
                case TerrainType.Forest: return new Color(0, 0.5f, 0);
                case TerrainType.Hills: return new Color(0.6f, 0.4f, 0);
                case TerrainType.Mountains: return new Color(0.5f, 0.5f, 0.5f);
                case TerrainType.Swamp: return new Color(0.5f, 0, 0.5f);
                case TerrainType.Road: return new Color(1f, 1f, 0);
                default: return Color.black;
            }
        }

        private GraphicalUserInterfaceBuilder CreateCommandStripBuilder()
        {
            return new GraphicalUserInterfaceBuilder("CommandStrip")
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Center)
                .WithBackgroundColor(new Color(0.45f, 0.45f, 0.45f))
                .OnBuild(ve => {
                    ve.style.width = 60;
                    ve.style.borderRightWidth = 2; ve.style.borderRightColor = Color.black;
                    ve.style.paddingTop = 10;
                })
                .AddChild(CreateStripButton("⚔️", null))
                .AddChild(CreateStripButton("❓", null))
                .AddChild(CreateStripButton("Ctr", null))
                .AddChild(CreateStripButton("Nxt", ShowStackSelectionPopup))
                .AddChild(new GraphicalUserInterfaceBuilder("Spacer").OnBuild(ve => ve.style.flexGrow = 1).Build())
                .AddChild(CreateStripButton("Quit", () => ForgeLogger.Log("Exit Game Requested")));
        }

        private VisualElement CreateStripButton(string label, Action onClick)
        {
            var btn = new Button { text = label };
            btn.style.width = 50; btn.style.height = 45;
            btn.style.marginBottom = 5;
            btn.style.fontSize = 13;
            btn.style.backgroundColor = new Color(0.35f, 0.35f, 0.35f);
            btn.style.borderTopColor = Color.white; btn.style.borderLeftColor = Color.white;
            btn.style.borderBottomColor = Color.black; btn.style.borderRightColor = Color.black;
            btn.style.borderTopWidth = 1; btn.style.borderLeftWidth = 1;
            btn.style.borderBottomWidth = 3; btn.style.borderRightWidth = 3;

            if (onClick != null) btn.clicked += onClick;
            return btn;
        }

        private GraphicalUserInterfaceBuilder CreateClassicBottomBarBuilder()
        {
            return new GraphicalUserInterfaceBuilder("BottomStatsBar")
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                .WithBackgroundColor(new Color(0.3f, 0.3f, 0.3f))
                .OnBuild(ve => {
                    ve.style.height = 140;
                    ve.style.borderTopWidth = 4; ve.style.borderTopColor = Color.black;
                    ve.style.paddingLeft = 20; ve.style.paddingRight = 20;
                })
                .AddChild(new GraphicalUserInterfaceBuilder("UnitDisplay")
                    .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Center)
                    .AddChild(new Label("SELECTED UNIT: ") { style = { color = Color.yellow, unityFontStyleAndWeight = FontStyle.Bold } })
                    .AddChild(new Label("HERO (Lvl 1)") { style = { color = Color.white, marginLeft = 10 } })
                    .Build())
                .AddChild(new GraphicalUserInterfaceBuilder("CenterProduction")
                    .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)
                    .AddButton("CITY PRODUCTION", () => ForgeLogger.Log("Opening City Screen..."))
                    .AddChild(new Label("Current: Light Infantry (2 turns)") { style = { color = Color.gray, fontSize = 11, marginTop = 5 } })
                    .Build())
                .AddChild(new Label("💰 540 (+24)") { style = { fontSize = 22, color = new Color(1, 0.8f, 0), unityFontStyleAndWeight = FontStyle.Bold } });
        }

        private void ShowStackSelectionPopup()
        {
            if (_stackModal != null)
            {
                _stackModal.Show();
            }
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
#if UNITY_EDITOR
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
#endif
        public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
    }
}