using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor; // Required for DisplayDialog
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using TheSingularityWorkshop.Warlords;
using TheSingularityWorkshop.Forge.IO;
using TheSingularityWorkshop.Forge.WorldBuilding;

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders
{

    public enum MapEditorMode { GeographySplat, UrbanZoning, DiscreteNodePlacement }
    // Math strategy for collapsing millions of topography points into an icosphere LOD cascade
    public enum LODStrategy { Average, PreservePeaks, PreserveValleys }

    public class Workshop_Gui_MapBuilder : IGuiProvider, IDisposable
    {
        public string Title => "Cartographer: Master Map Builder";

        private string _processGroup;

        // --- MAP DATA (GLOBAL) ---
        public Texture2D MapDataTexture { get; private set; }
        private Texture2D _displayTexture;
        private Vector2Int _mapSize = new Vector2Int(256, 256);
        private int _mapSeed = 1337;
        private string _activeTileSet = "Standard Fantasy";
        private List<string> _availableTileSets = new List<string> { "Standard Fantasy", "Volcanic Ash", "Winter Plains" };

        // --- UI CONTAINERS ---
        private VisualElement _tabGeneration;
        private VisualElement _tabRefinement;
        private VisualElement _tabDetailing;
        private VisualElement _tabPublishing;
        private VisualElement _geographyBrushPanel;
        private VisualElement _zoningBrushPanel;
        private VisualElement _discreteNodePanel;

        // --- COMMON UI ELEMENTS ---
        private Image _2dCanvasElement;
        private Image _3dPreviewElement;
        private VisualElement _lodPreviewContainer;
        private IForgeFileBrowser _fileBrowser = new EditorNativeFileBrowser();

        // --- ACTIVE BRUSH STATE ---
        private TerrainType _activeBiome = TerrainType.Plains;
        private float _activeHeight = 0.5f, _activeCohesion = 0.8f, _activeDivergence = 0.2f;
        private int _brushSize = 8;
        private float _brushHardness = 0.5f;
        private bool _isDrawing = false;
        private Vector2? _lastMousePos = null;
        private MapEditorMode _activeEditorMode = MapEditorMode.GeographySplat;
        private bool _isDraggingCam = false;

        // --- DYNAMIC COLOR MAPPING ---
        // Links a conceptual Biome to an EXACT color (derived from ingested images)
        private Dictionary<TerrainType, Color> _biomeColors = new Dictionary<TerrainType, Color>();

        // --- ZONING PALETTE (Dynamic) ---
        private float _activeUrbanDensity = 0.5f;
        private UrbanZoneType _activeUrbanZone;
        private List<UrbanZoneType> _zonePalette = new List<UrbanZoneType>
        {
            new UrbanZoneType { Id = 0, Name = "Wilderness", DisplayColor = Color.clear },
            new UrbanZoneType { Id = 1, Name = "Residential", DisplayColor = new Color(0.2f, 0.5f, 1f) },
            new UrbanZoneType { Id = 2, Name = "Industrial", DisplayColor = new Color(0.9f, 0.5f, 0.1f) },
            new UrbanZoneType { Id = 3, Name = "Commercial", DisplayColor = new Color(0.2f, 0.8f, 0.2f) },
            new UrbanZoneType { Id = 4, Name = "Military", DisplayColor = new Color(0.8f, 0.1f, 0.1f) }
        };

        // --- IMAGE MAPPER & LOD STATE ---
        private Texture2D _sourceImage;
        private Dictionary<Color, TerrainType> _compositeColorMap = new Dictionary<Color, TerrainType>();
        private LODStrategy _lodStrategy = LODStrategy.PreservePeaks;

        // --- PIXEL BUFFERS ---
        private Color[] _dataPixels;
        private Color[] _displayPixels;

        // --- 3D PROXY PREVIEW ---
        private const int PREVIEW_LAYER = 31;
        private const int MAX_PREVIEW_RES = 512;
        private GameObject _previewRoot;
        private RenderTexture _previewRenderTexture;
        private Camera _previewCamera;
        private Terrain _previewTerrain;
        private TerrainData _previewTerrainData;
        private GameObject _previewTerrainObj;

        // Camera State
        private Vector2 _cameraOrbit = new Vector2(45f, 45f);
        private float _cameraDistance = 200f;

        public Action<Texture2D> OnMapDataUpdated;
        private bool _paintHeight = false;
        private bool _paintBiome = true;
        private VisualElement _compositeMappingContainer;

        public Workshop_Gui_MapBuilder()
        {
            _processGroup = "MapBuilder_" + Guid.NewGuid().ToString().Substring(0, 6);
            _activeUrbanZone = _zonePalette[1]; // Default to Residential
            DestroyGhostObjects();

            InitializeDefaultBiomeColors();
            Setup3DPreviewScene();
            InitializeTextures(_mapSize.x, _mapSize.y);
        }

        private void InitializeDefaultBiomeColors()
        {
            _biomeColors[TerrainType.OpenWater] = new Color(0.1f, 0.3f, 0.8f);
            _biomeColors[TerrainType.Plains] = new Color(0.3f, 0.7f, 0.3f);
            _biomeColors[TerrainType.Forest] = new Color(0.1f, 0.5f, 0.1f);
            _biomeColors[TerrainType.Hills] = new Color(0.6f, 0.5f, 0.3f);
            _biomeColors[TerrainType.Mountains] = new Color(0.5f, 0.5f, 0.5f);
            _biomeColors[TerrainType.Swamp] = new Color(0.2f, 0.4f, 0.3f);
        }

        private void DestroyGhostObjects()
        {
            var allGos = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var go in allGos)
            {
                if (go != null && (go.hideFlags & HideFlags.HideAndDontSave) != 0)
                {
                    if (go.name.StartsWith("Forge_MapBuilder_Root_")) UnityEngine.Object.DestroyImmediate(go);
                }
            }
        }

        private void InitializeTextures(int width, int height)
        {
            if (MapDataTexture != null) UnityEngine.Object.DestroyImmediate(MapDataTexture);
            if (_displayTexture != null) UnityEngine.Object.DestroyImmediate(_displayTexture);

            MapDataTexture = new Texture2D(width, height, TextureFormat.RGBAFloat, false) { filterMode = FilterMode.Bilinear };
            _displayTexture = new Texture2D(width, height, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };

            _dataPixels = new Color[width * height];
            _displayPixels = new Color[width * height];

            ClearMapData(TerrainType.Plains, 0.2f, _zonePalette[0], 0.0f);
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new SplitPanelBuilder(sidebarWidth: 420)
                .WithSidebar(BuildSidebar())
                .WithMain(BuildMainArea())
                .CreateGui(ctx);

            // --- AUTO ORBIT LOOP ---
            root.schedule.Execute(() =>
            {
                if (!_isDraggingCam && _previewCamera != null)
                {
                    _cameraOrbit.x += 10f * Time.deltaTime; // Rotate 10 degrees per second
                    UpdateCameraPosition();
                }
            }).Every(16); // ~60fps

            root.RegisterCallback<DetachFromPanelEvent>(e => Dispose());
            return root;
        }

        private GraphicalUserInterfaceBuilder BuildSidebar()
        {
            return new GraphicalUserInterfaceBuilder("Sidebar")
                .WithPadding(0)
                .WithBackgroundColor(new Color(0.12f, 0.12f, 0.14f))
                .OnBuild(sidebarRoot =>
                {
                    var tabRow = new VisualElement { style = { flexDirection = FlexDirection.Row, backgroundColor = new Color(0.08f, 0.08f, 0.1f), borderBottomWidth = 2, borderBottomColor = Color.gray } };

                    Button btnGen = CreateTabButton("1. Cartography");
                    Button btnRef = CreateTabButton("2. Refinement");
                    Button btnDet = CreateTabButton("3. Detailing");
                    Button btnPub = CreateTabButton("4. Publishing");

                    tabRow.Add(btnGen); tabRow.Add(btnRef); tabRow.Add(btnDet); tabRow.Add(btnPub);
                    sidebarRoot.Add(tabRow);

                    var contentArea = new ScrollView { style = { flexGrow = 1, paddingLeft = 15, paddingRight = 15, paddingTop = 15 } };

                    _tabGeneration = BuildGenerationTab();
                    _tabRefinement = BuildRefinementTab();
                    _tabDetailing = BuildDetailingTab();
                    _tabPublishing = BuildPublishingTab();

                    contentArea.Add(_tabGeneration);
                    contentArea.Add(_tabRefinement);
                    contentArea.Add(_tabDetailing);
                    contentArea.Add(_tabPublishing);

                    sidebarRoot.Add(contentArea);

                    Action<VisualElement> SwitchTab = (activeTab) =>
                    {
                        _tabGeneration.style.display = (activeTab == _tabGeneration) ? DisplayStyle.Flex : DisplayStyle.None;
                        _tabRefinement.style.display = (activeTab == _tabRefinement) ? DisplayStyle.Flex : DisplayStyle.None;
                        _tabDetailing.style.display = (activeTab == _tabDetailing) ? DisplayStyle.Flex : DisplayStyle.None;
                        _tabPublishing.style.display = (activeTab == _tabPublishing) ? DisplayStyle.Flex : DisplayStyle.None;

                        btnGen.style.borderBottomColor = (activeTab == _tabGeneration) ? Color.cyan : Color.clear;
                        btnRef.style.borderBottomColor = (activeTab == _tabRefinement) ? Color.cyan : Color.clear;
                        btnDet.style.borderBottomColor = (activeTab == _tabDetailing) ? Color.cyan : Color.clear;
                        btnPub.style.borderBottomColor = (activeTab == _tabPublishing) ? Color.cyan : Color.clear;

                        UpdateMainAreaVisibility();
                    };

                    btnGen.clicked += () => SwitchTab(_tabGeneration);
                    btnRef.clicked += () => SwitchTab(_tabRefinement);
                    btnDet.clicked += () => SwitchTab(_tabDetailing);
                    btnPub.clicked += () => SwitchTab(_tabPublishing);

                    SwitchTab(_tabGeneration);
                });
        }

        private void UpdateMainAreaVisibility()
        {
            if (_tabGeneration == null || _tabPublishing == null) return;

            if (_tabPublishing.style.display == DisplayStyle.Flex)
            {
                // Publishing hides the live preview
                _2dCanvasElement.style.display = DisplayStyle.None;
                _3dPreviewElement.style.display = DisplayStyle.None;
                _lodPreviewContainer.style.display = DisplayStyle.Flex;
            }
            else
            {
                // 3D is ALWAYS visible as the live background
                _3dPreviewElement.style.display = DisplayStyle.Flex;
                _lodPreviewContainer.style.display = DisplayStyle.None;

                // 2D Canvas is a floating overlay, only visible during Generation
                _2dCanvasElement.style.display = (_tabGeneration.style.display == DisplayStyle.Flex) ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private GraphicalUserInterfaceBuilder BuildMainArea()
        {
            return new GraphicalUserInterfaceBuilder("MainArea")
                .WithPadding(0)
                .OnBuild(ve =>
                {
                    ve.style.backgroundColor = Color.black;
                    ve.style.flexGrow = 1;
                    ve.style.position = Position.Relative; // Set to relative to contain absolute children

                    // 1. 3D Live Preview (Full Background)
                    _3dPreviewElement = new Image { image = _previewRenderTexture, scaleMode = ScaleMode.ScaleAndCrop };
                    _3dPreviewElement.style.position = Position.Absolute;
                    _3dPreviewElement.style.top = 0; _3dPreviewElement.style.bottom = 0; _3dPreviewElement.style.left = 0; _3dPreviewElement.style.right = 0;
                    Register3DCameraEvents(_3dPreviewElement);
                    ve.Add(_3dPreviewElement);

                    // 2. 2D Canvas (Floating Interactive Minimap in Bottom Left)
                    _2dCanvasElement = new Image { image = _displayTexture, scaleMode = ScaleMode.ScaleToFit };
                    _2dCanvasElement.style.position = Position.Absolute;
                    _2dCanvasElement.style.bottom = 20; _2dCanvasElement.style.left = 20;
                    _2dCanvasElement.style.width = 300; _2dCanvasElement.style.height = 300;
                    _2dCanvasElement.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);

                    // Cyberpunk frame for the floating widget
                    _2dCanvasElement.style.borderTopWidth = 2; _2dCanvasElement.style.borderBottomWidth = 2; _2dCanvasElement.style.borderLeftWidth = 2; _2dCanvasElement.style.borderRightWidth = 2;
                    _2dCanvasElement.style.borderTopColor = Color.cyan; _2dCanvasElement.style.borderBottomColor = Color.cyan; _2dCanvasElement.style.borderLeftColor = Color.cyan; _2dCanvasElement.style.borderRightColor = Color.cyan;
                    _2dCanvasElement.style.borderTopLeftRadius = 5; _2dCanvasElement.style.borderTopRightRadius = 5; _2dCanvasElement.style.borderBottomLeftRadius = 5; _2dCanvasElement.style.borderBottomRightRadius = 5;

                    Register2DDrawingEvents(_2dCanvasElement);
                    ve.Add(_2dCanvasElement);

                    // 3. Publishing LOD cascade preview
                    _lodPreviewContainer = new ScrollView { style = { display = DisplayStyle.None, position = Position.Absolute, top = 0, bottom = 0, left = 0, right = 0, backgroundColor = Color.black, paddingLeft = 15, paddingRight = 15, paddingTop = 15 } };
                    ve.Add(_lodPreviewContainer);

                    UpdateMainAreaVisibility();
                });
        }

        private Button CreateTabButton(string text)
        {
            return new Button { text = text, style = { flexGrow = 1, backgroundColor = Color.clear, color = Color.white, borderBottomWidth = 2, borderBottomColor = Color.clear, borderTopWidth = 0, borderLeftWidth = 0, borderRightWidth = 0, paddingBottom = 10, paddingTop = 10, unityFontStyleAndWeight = FontStyle.Bold } };
        }

        // ==========================================
        // TAB 1: CARTOGRAPHY
        // ==========================================
        private VisualElement BuildGenerationTab()
        {
            Foldout imageInjestFoldout = null;

            VisualElement content = new GraphicalUserInterfaceBuilder("TabGen")
                .AddChild(new Label("1. WORLD CARTOGRAPHY") { style = { fontSize = 16, color = Color.cyan, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 15 } })

                .AddIntegerData("Width (Tiles)", _mapSize.x, v => _mapSize.x = v)
                .AddIntegerData("Height (Tiles)", _mapSize.y, v => _mapSize.y = v)
                .AddDropdownData("Tile Set DB", _availableTileSets, 0, v => _activeTileSet = v)
                .AddButton("Resize & Clear Map", () =>
                {
                    if (EditorUtility.DisplayDialog("Destructive Resize", "Resizing will clear the map data texture. Proceed?", "Yes", "Cancel"))
                    {
                        Rebuild3DTerrainProxy(); InitializeTextures(_mapSize.x, _mapSize.y);
                    }
                })

                .AddSeparator(Color.gray, 1)

                .OnBuild(genVe =>
                {
                    imageInjestFoldout = new Foldout { text = "LOAD SOURCE IMAGE / PALETTE", value = false, style = { unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10, marginBottom = 10, borderBottomWidth = 2, borderBottomColor = Color.gray } };
                    var imageContent = BuildImageIngestionUI();
                    imageInjestFoldout.Add(imageContent);
                    genVe.Add(imageInjestFoldout);
                })

                .AddSeparator(Color.gray, 1)

                .AddDropdownData("Editor Mode", Enum.GetNames(typeof(MapEditorMode)).ToList(), (int)_activeEditorMode, val =>
                {
                    _activeEditorMode = (MapEditorMode)Enum.Parse(typeof(MapEditorMode), val);
                    UpdateBrushPanels();
                })

                .OnBuild(genVe =>
                {
                    _geographyBrushPanel = new GraphicalUserInterfaceBuilder("GeoBrush")
                        .AddChild(new Label("GEOGRAPHY SPLAT BRUSH") { style = { color = Color.white, unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10 } })
                        .AddToggleData("Paint Biome", _paintBiome, v => _paintBiome = v)
                        .AddDropdownData("Target Biome", Enum.GetNames(typeof(TerrainType)).ToList(), (int)_activeBiome, val => _activeBiome = (TerrainType)Enum.Parse(typeof(TerrainType), val))
                        .AddToggleData("Paint Height", _paintHeight, v => _paintHeight = v)
                        .AddSliderData("Height (R)", 0f, 1f, _activeHeight, v => _activeHeight = v)
                        .Build();

                    _zoningBrushPanel = new GraphicalUserInterfaceBuilder("ZoningBrush")
                        .AddChild(new Label("URBAN ZONING BRUSH") { style = { color = new Color(0.3f, 0.6f, 1f), unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10 } })
                        .AddChild(new Label("Paints demographic intent into the Blue/Alpha channels.") { style = { color = Color.gray, fontSize = 10, marginBottom = 5 } })
                        .AddDropdownData("Zone Type",
                            _zonePalette.Select(z => z.Name).ToList(),
                            _zonePalette.IndexOf(_activeUrbanZone),
                            val => _activeUrbanZone = _zonePalette.FirstOrDefault(z => z.Name == val) ?? _zonePalette[0]
                        )
                        .AddSliderData("Urban Density (A)", 0f, 1f, _activeUrbanDensity, v => _activeUrbanDensity = v)
                        .Build();

                    _discreteNodePanel = new GraphicalUserInterfaceBuilder("DiscreteNode")
                        .AddChild(new Label("DISCRETE NODE PLACEMENT") { style = { color = new Color(0.9f, 0.4f, 0.4f), unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10 } })
                        .AddChild(new Label("Click on the canvas to place a specific entity bypassing procedural splat.") { style = { color = Color.gray, fontSize = 10, marginBottom = 5, whiteSpace = WhiteSpace.Normal } })
                        .AddButton("Open Entity Library", () => Debug.Log("Opening Entity Library for Node Placement..."))
                        .Build();

                    genVe.Add(_geographyBrushPanel);
                    genVe.Add(_zoningBrushPanel);
                    genVe.Add(_discreteNodePanel);
                })

                .AddSeparator(new Color(0.2f, 0.2f, 0.2f), 1)

                .AddChild(new Label("BRUSH SETTINGS") { style = { color = Color.white, unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10 } })
                .AddIntSliderData("Radius", 1, 100, _brushSize, s => _brushSize = s)
                .AddSliderData("Hardness", 0f, 1f, _brushHardness, h => _brushHardness = h)
                .Build();

            _tabGeneration = content;
            UpdateBrushPanels();
            return content;
        }

        private void UpdateBrushPanels()
        {
            if (_geographyBrushPanel == null) return;

            _geographyBrushPanel.style.display = _activeEditorMode == MapEditorMode.GeographySplat ? DisplayStyle.Flex : DisplayStyle.None;
            _zoningBrushPanel.style.display = _activeEditorMode == MapEditorMode.UrbanZoning ? DisplayStyle.Flex : DisplayStyle.None;
            _discreteNodePanel.style.display = _activeEditorMode == MapEditorMode.DiscreteNodePlacement ? DisplayStyle.Flex : DisplayStyle.None;
        }

        // ==========================================
        // IMAGE INGESTION 
        // ==========================================
        private VisualElement BuildImageIngestionUI()
        {
            VisualElement container = null;
            return new GraphicalUserInterfaceBuilder("ImageInjestContent")
                .AddButton("Load Source Image", HandleDestructiveImageLoad)
                .OnBuild(ve =>
                {
                    container = new VisualElement { style = { marginTop = 10, paddingBottom = 10 } };
                    ve.Add(container);
                })
                .AddButton("GENERATE BIOME MAP", GenerateMapFromImage)
                .OnBuild(ve => { _compositeMappingContainer = container; })
                .Build();
        }

        private void HandleDestructiveImageLoad()
        {
            _fileBrowser.OpenImageFile(tex =>
            {
                if (EditorUtility.DisplayDialog("Destructive Action", "Loading this image will resize the map texture and clear all current data. Proceed?", "Overwrite", "Cancel"))
                {
                    _sourceImage = tex;
                    _mapSize = new Vector2Int(tex.width, tex.height);
                    Rebuild3DTerrainProxy(); InitializeTextures(_mapSize.x, _mapSize.y);
                    _2dCanvasElement.image = _sourceImage;
                    ExtractQuantizedPaletteAndBuildRules();
                }
            }, () => { });
        }

        private void ExtractQuantizedPaletteAndBuildRules()
        {
            if (_sourceImage == null || _compositeMappingContainer == null) return;
            _compositeMappingContainer.Clear();
            _compositeColorMap.Clear();

            var pixels = _sourceImage.GetPixels();
            var distinctColors = pixels.Distinct().Take(8).ToList();

            foreach (var col in distinctColors)
            {
                // Default assignment
                TerrainType defaultType = TerrainType.Plains;
                _compositeColorMap[col] = defaultType;
                _biomeColors[defaultType] = col; // Link EXACT color to TerrainType

                var rowBuilder = new GraphicalUserInterfaceBuilder("ColorMappingRow")
                    .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Center)
                    .WithMarginTop(8)
                    .OnBuild(rowVe =>
                    {
                        var swatch = new VisualElement { style = { width = 26, height = 26, backgroundColor = col, marginRight = 10, borderTopWidth = 2, borderBottomWidth = 2, borderLeftWidth = 2, borderRightWidth = 2, borderTopColor = Color.white, borderBottomColor = Color.white, borderLeftColor = Color.white, borderRightColor = Color.white } };
                        rowVe.Add(swatch);
                    })
                    .AddDropdownData("", Enum.GetNames(typeof(TerrainType)).ToList(), (int)defaultType, evt =>
                    {
                        TerrainType t = (TerrainType)Enum.Parse(typeof(TerrainType), evt);
                        _compositeColorMap[col] = t;
                        _biomeColors[t] = col; // Update exact color mapping
                        UpdateTerrainLayers(); // Physically update 3D materials
                        ApplyPixels(); // Re-render preview
                    })
                    .Build();

                _compositeMappingContainer.Add(rowBuilder);
            }

            UpdateTerrainLayers();
        }

        private void GenerateMapFromImage()
        {
            if (_sourceImage == null) return;

            for (int y = 0; y < _mapSize.y; y++)
            {
                for (int x = 0; x < _mapSize.x; x++)
                {
                    Color srcColor = _sourceImage.GetPixel(x, y);
                    TerrainType targetTerrain = _compositeColorMap.OrderBy(kvp => Vector4.Distance(kvp.Key, srcColor)).First().Value;

                    int index = y * _mapSize.x + x;
                    _dataPixels[index] = new Color(_activeHeight, (float)targetTerrain / 255f, ZoningMath.EncodeZone(0), 0f);
                    _displayPixels[index] = GeneratePreviewColor(_dataPixels[index]);
                }
            }

            _2dCanvasElement.image = _displayTexture;
            ApplyPixels();
            Debug.Log($"[Forge] Ingestion Complete.");
        }

        private VisualElement BuildRefinementTab() => new GraphicalUserInterfaceBuilder("TabRef").AddChild(new Label("2. MAP REFINEMENT (Global)") { style = { fontSize = 16, color = new Color(0.8f, 0.6f, 0.2f), unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 15 } }).AddIntegerData("World Seed", _mapSeed, v => { _mapSeed = v; Rebuild3DTerrainProxy(); }).Build();
        private VisualElement BuildDetailingTab() => new GraphicalUserInterfaceBuilder("TabDet").AddChild(new Label("3. MICRO-DETAIL (Icosphere)") { style = { fontSize = 16, color = new Color(0.8f, 0.4f, 0.8f), unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 15 } }).Build();
        private VisualElement BuildPublishingTab() => new GraphicalUserInterfaceBuilder("TabPub").AddChild(new Label("4. LOD PUBLISHING") { style = { fontSize = 16, color = new Color(0.4f, 0.8f, 0.4f), unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 15 } }).AddButton("BAKE LOD CASCADE", () => { }).Build();

        // ==========================================
        // CORE RENDERING & PREVIEW SCALING
        // ==========================================

        private void ClearMapData(TerrainType type, float height, UrbanZoneType zone, float density)
        {
            float encodedType = (float)type / 255f;
            Color baseData = new Color(height, encodedType, ZoningMath.EncodeZone(zone.Id), density);

            for (int i = 0; i < _dataPixels.Length; i++)
            {
                _dataPixels[i] = baseData;
                _displayPixels[i] = GeneratePreviewColor(baseData);
            }
            ApplyPixels();
        }

        private void ApplyPixels()
        {
            MapDataTexture.SetPixels(_dataPixels);
            MapDataTexture.Apply();

            _displayTexture.SetPixels(_displayPixels);
            _displayTexture.Apply();

            SyncDownsampledTerrainData();
            OnMapDataUpdated?.Invoke(MapDataTexture);
        }

        private Color GeneratePreviewColor(Color data)
        {
            int biomeInt = Mathf.RoundToInt(data.g * 255f);
            TerrainType type = (TerrainType)biomeInt;

            // USE EXACT PALETTE COLOR
            Color baseColor = _biomeColors.ContainsKey(type) ? _biomeColors[type] : Color.magenta;

            // APPLY HEIGHT DARKENING
            float heightDarken = Mathf.Lerp(0.3f, 1.2f, data.r);
            baseColor = new Color(baseColor.r * heightDarken, baseColor.g * heightDarken, baseColor.b * heightDarken);

            // APPLY URBAN ZONING OVERLAY
            byte zoneId = ZoningMath.DecodeZone(data.b);
            float density = data.a;

            if (zoneId != 0 && density > 0.05f)
            {
                var zoneDef = _zonePalette.FirstOrDefault(z => z.Id == zoneId);
                if (zoneDef != null)
                {
                    baseColor = Color.Lerp(baseColor, zoneDef.DisplayColor, density * 0.8f);
                }
            }

            return baseColor;
        }

        // ==========================================
        // 3D PROXY PREVIEW
        // ==========================================

        private void Setup3DPreviewScene()
        {
            _previewRoot = new GameObject("Forge_MapBuilder_Root_" + _processGroup) { hideFlags = HideFlags.HideAndDontSave };
            _previewRenderTexture = new RenderTexture(1920, 1080, 24);

            GameObject camObj = new GameObject("MapBuilderCamera") { layer = PREVIEW_LAYER };
            camObj.transform.SetParent(_previewRoot.transform);
            _previewCamera = camObj.AddComponent<Camera>();
            _previewCamera.targetTexture = _previewRenderTexture;
            _previewCamera.clearFlags = CameraClearFlags.SolidColor;
            _previewCamera.backgroundColor = new Color(0.05f, 0.05f, 0.08f);
            _previewCamera.cullingMask = 1 << PREVIEW_LAYER;

            Rebuild3DTerrainProxy();
        }

        private void Rebuild3DTerrainProxy()
        {
            if (_previewTerrainObj != null) UnityEngine.Object.DestroyImmediate(_previewTerrainObj);

            int proxyRes = Mathf.Min(MAX_PREVIEW_RES, Mathf.Max(_mapSize.x, _mapSize.y));
            if (proxyRes < 32) proxyRes = 32;

            _previewTerrainData = new TerrainData { heightmapResolution = proxyRes + 1, alphamapResolution = proxyRes, size = new Vector3(_mapSize.x, 50f, _mapSize.y) };

            UpdateTerrainLayers();

            _previewTerrainObj = Terrain.CreateTerrainGameObject(_previewTerrainData);
            _previewTerrainObj.transform.SetParent(_previewRoot.transform);
            _previewTerrainObj.layer = PREVIEW_LAYER;

            _previewTerrain = _previewTerrainObj.GetComponent<Terrain>();
            _previewTerrain.drawInstanced = true;
            _previewTerrainObj.transform.localPosition = new Vector3(-_mapSize.x / 2f, 0, -_mapSize.y / 2f);

            _cameraDistance = Mathf.Max(_mapSize.x, _mapSize.y) * 0.8f;
            UpdateCameraPosition();
        }

        // Creates perfectly matched solid textures for the 3D Terrain
        private void UpdateTerrainLayers()
        {
            if (_previewTerrainData == null) return;

            int numBiomes = Enum.GetValues(typeof(TerrainType)).Length;
            TerrainLayer[] layers = new TerrainLayer[numBiomes];

            for (int i = 0; i < numBiomes; i++)
            {
                TerrainType t = (TerrainType)i;
                Color c = _biomeColors.ContainsKey(t) ? _biomeColors[t] : Color.magenta;

                // Create a pure solid color texture
                Texture2D tex = new Texture2D(2, 2, TextureFormat.RGB24, false);
                tex.SetPixels(new Color[] { c, c, c, c });
                tex.Apply();

                // Assign to terrain with tiling matching map size to prevent seams
                layers[i] = new TerrainLayer { name = t.ToString(), diffuseTexture = tex, tileSize = new Vector2(_mapSize.x, _mapSize.y) };
            }

            _previewTerrainData.terrainLayers = layers;
        }

        private void SyncDownsampledTerrainData()
        {
            if (_previewTerrainData == null || MapDataTexture == null) return;

            int res = _previewTerrainData.alphamapResolution;
            float[,] heights = new float[res + 1, res + 1];
            int layerCount = _previewTerrainData.alphamapLayers;
            float[,,] alphaMaps = new float[res, res, layerCount];

            for (int y = 0; y < res; y++)
            {
                float normalizedY = (float)y / res;
                for (int x = 0; x < res; x++)
                {
                    float normalizedX = (float)x / res;
                    Color pixelData = MapDataTexture.GetPixelBilinear(normalizedX, normalizedY);
                    heights[y, x] = pixelData.r;

                    int biomeIndex = Mathf.RoundToInt(pixelData.g * 255f);
                    for (int l = 0; l < layerCount; l++) { alphaMaps[y, x, l] = (l == biomeIndex % layerCount) ? 1.0f : 0.0f; }
                }
            }

            _previewTerrainData.SetHeightsDelayLOD(0, 0, heights);
            _previewTerrainData.SetAlphamaps(0, 0, alphaMaps);
            _previewTerrain.Flush();
            if (_previewCamera != null) _previewCamera.Render();
        }

        // ==========================================
        // PAINTING LOGIC
        // ==========================================

        private void Register2DDrawingEvents(Image img)
        {
            img.RegisterCallback<PointerDownEvent>(e =>
            {
                if (_tabGeneration == null || _tabGeneration.style.display == DisplayStyle.None) return;

                if (_activeEditorMode == MapEditorMode.DiscreteNodePlacement)
                {
                    HandleDiscreteNodePlacement(e.localPosition, img);
                    return;
                }

                _isDrawing = true; _lastMousePos = e.localPosition;
                DrawInterpolatedStroke(_lastMousePos.Value, e.localPosition, img);
                img.CapturePointer(e.pointerId);
            });

            img.RegisterCallback<PointerMoveEvent>(e =>
            {
                if (_isDrawing && _lastMousePos.HasValue && _activeEditorMode != MapEditorMode.DiscreteNodePlacement)
                {
                    DrawInterpolatedStroke(_lastMousePos.Value, e.localPosition, img);
                    _lastMousePos = e.localPosition;
                }
            });

            img.RegisterCallback<PointerUpEvent>(e =>
            {
                if (_activeEditorMode == MapEditorMode.DiscreteNodePlacement) return;

                _isDrawing = false; _lastMousePos = null; img.ReleasePointer(e.pointerId);
                ApplyPixels();
            });
        }

        private void HandleDiscreteNodePlacement(Vector2 localPos, Image img)
        {
            float xPct = localPos.x / img.layout.width;
            float yPct = 1.0f - (localPos.y / img.layout.height);

            float worldX = xPct * _mapSize.x;
            float worldY = yPct * _mapSize.y;

            Debug.Log($"[Forge] Placing discrete node at precise coordinates: X:{worldX}, Y:{worldY}");
        }

        private void DrawInterpolatedStroke(Vector2 startPos, Vector2 endPos, Image img)
        {
            float dist = Vector2.Distance(startPos, endPos);
            int steps = Mathf.Max(1, Mathf.CeilToInt(dist / (_brushSize * 0.2f)));

            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                Vector2 lerpedPos = Vector2.Lerp(startPos, endPos, t);
                PaintAtPosition(lerpedPos, img);
            }

            _displayTexture.SetPixels(_displayPixels);
            _displayTexture.Apply();
        }

        private void PaintAtPosition(Vector2 localPos, Image img)
        {
            float xPct = localPos.x / img.layout.width;
            float yPct = 1.0f - (localPos.y / img.layout.height);

            int texX = Mathf.Clamp((int)(xPct * _mapSize.x), 0, _mapSize.x - 1);
            int texY = Mathf.Clamp((int)(yPct * _mapSize.y), 0, _mapSize.y - 1);

            float halfBrush = _brushSize / 2f;
            int bound = Mathf.CeilToInt(halfBrush);

            float encodedBiome = (float)_activeBiome / 255f;
            float encodedZone = ZoningMath.EncodeZone(_activeUrbanZone.Id);

            for (int x = -bound; x <= bound; x++)
            {
                for (int y = -bound; y <= bound; y++)
                {
                    float dist = Mathf.Sqrt((x * x) + (y * y));
                    float normDist = dist / halfBrush;

                    if (normDist <= 1f)
                    {
                        int px = Mathf.Clamp(texX + x, 0, _mapSize.x - 1);
                        int py = Mathf.Clamp(texY + y, 0, _mapSize.y - 1);

                        float strength = 1f;
                        if (_brushHardness < 1f && normDist > _brushHardness) { strength = 1f - ((normDist - _brushHardness) / (1f - _brushHardness)); }

                        if (strength > 0.01f)
                        {
                            int index = py * _mapSize.x + px;
                            Color currentData = _dataPixels[index];

                            float r = currentData.r; float g = currentData.g; float b = currentData.b; float a = currentData.a;

                            if (_activeEditorMode == MapEditorMode.GeographySplat)
                            {
                                r = _paintHeight ? Mathf.Lerp(currentData.r, _activeHeight, strength) : currentData.r;
                                if (_paintBiome && strength > 0.5f) g = encodedBiome;
                            }
                            else if (_activeEditorMode == MapEditorMode.UrbanZoning)
                            {
                                if (strength > 0.5f) b = encodedZone;
                                a = Mathf.Lerp(currentData.a, _activeUrbanDensity, strength);
                            }

                            Color newData = new Color(r, g, b, a);
                            _dataPixels[index] = newData;
                            _displayPixels[index] = GeneratePreviewColor(newData);
                        }
                    }
                }
            }
        }

        private void Register3DCameraEvents(Image imgElement)
        {
            imgElement.RegisterCallback<PointerDownEvent>(e => { if (e.button == 1) { _isDraggingCam = true; _lastMousePos = e.position; imgElement.CapturePointer(e.pointerId); } });
            imgElement.RegisterCallback<PointerMoveEvent>(e =>
            {
                if (_isDraggingCam)
                {
                    Vector2 delta = (Vector2)e.position - _lastMousePos.Value;
                    _cameraOrbit.x += delta.x * 0.5f; _cameraOrbit.y = Mathf.Clamp(_cameraOrbit.y - delta.y * 0.5f, 5f, 85f);
                    _lastMousePos = e.position; UpdateCameraPosition();
                }
            });
            imgElement.RegisterCallback<PointerUpEvent>(e => { if (e.button == 1) { _isDraggingCam = false; imgElement.ReleasePointer(e.pointerId); } });
            imgElement.RegisterCallback<WheelEvent>(e => { _cameraDistance = Mathf.Clamp(_cameraDistance + e.delta.y * (_cameraDistance * 0.05f), 20f, 2000f); UpdateCameraPosition(); });
        }

        private void UpdateCameraPosition()
        {
            if (_previewCamera == null) return;
            Quaternion rot = Quaternion.Euler(_cameraOrbit.y, _cameraOrbit.x, 0);
            _previewCamera.transform.position = rot * new Vector3(0, 0, -_cameraDistance) + Vector3.zero;
            _previewCamera.transform.LookAt(Vector3.zero);
            _previewCamera.Render();
        }

        public void Dispose()
        {
            if (_previewCamera != null) _previewCamera.targetTexture = null;
            if (_previewRenderTexture != null) { _previewRenderTexture.Release(); UnityEngine.Object.DestroyImmediate(_previewRenderTexture); }
            if (MapDataTexture != null) UnityEngine.Object.DestroyImmediate(MapDataTexture);
            if (_displayTexture != null) UnityEngine.Object.DestroyImmediate(_displayTexture);
            if (_previewRoot != null) UnityEngine.Object.DestroyImmediate(_previewRoot);
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}