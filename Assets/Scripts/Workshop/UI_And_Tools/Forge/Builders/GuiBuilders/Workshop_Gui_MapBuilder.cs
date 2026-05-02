using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.IO;
using Assets.Scripts.Warlords;
using Workshop.Core;
using Workshop.Core.Diagnostics;
using Assets.Scripts.Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    public enum MapEditorMode { GeographySplat, UrbanZoning, DiscreteNodePlacement }
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

        // --- UI REFS ---
        private VisualElement _sidebarContent;
        private Image _2dCanvasElement;
        private Image _3dPreviewElement;
        private VisualElement _lodPreviewContainer;

#if UNITY_EDITOR
        private IForgeFileBrowser _fileBrowser = new EditorNativeFileBrowser();
#else
        private IForgeFileBrowser _fileBrowser = null;
#endif

        // --- ACTIVE BRUSH STATE ---
        private TerrainType _activeBiome = TerrainType.Plains;
        private float _activeHeight = 0.5f;
        private int _brushSize = 8;
        private float _brushHardness = 0.5f;
        private bool _isDrawing = false;
        private Vector2? _lastMousePos = null;
        private MapEditorMode _activeEditorMode = MapEditorMode.GeographySplat;
        private bool _isDraggingCam = false;

        private Dictionary<TerrainType, Color> _biomeColors = new Dictionary<TerrainType, Color>();

        // --- ZONING PALETTE ---
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

        private Texture2D _sourceImage;
        private Dictionary<Color, TerrainType> _compositeColorMap = new Dictionary<Color, TerrainType>();
        private Color[] _dataPixels;
        private Color[] _displayPixels;

        // --- 3D PROXY PREVIEW ---
        private const int PREVIEW_LAYER = 31;
        private GameObject _previewRoot;
        private RenderTexture _previewRenderTexture;
        private Camera _previewCamera;
        private Terrain _previewTerrain;
        private TerrainData _previewTerrainData;
        private GameObject _previewTerrainObj;

        private Vector2 _cameraOrbit = new Vector2(45f, 45f);
        private float _cameraDistance = 200f;

        public Workshop_Gui_MapBuilder()
        {
            _processGroup = "MapBuilder_" + Guid.NewGuid().ToString().Substring(0, 6);
            _activeUrbanZone = _zonePalette[1];

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

        public void ClearMapData(TerrainType type, float height, UrbanZoneType zone, float density)
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

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new ForgeSplitPanelBuilder(sidebarWidth: 420)
                .WithSidebar(BuildSidebar(ctx))
                .WithMain(BuildMainArea(ctx))
                .CreateGui(ctx);

            root.schedule.Execute(() =>
            {
                if (!_isDraggingCam && _previewCamera != null)
                {
                    _cameraOrbit.x += 10f * Time.deltaTime;
                    UpdateCameraPosition();
                }
            }).Every(16);

            root.RegisterCallback<DetachFromPanelEvent>(e => Dispose());
            return root;
        }

        private GraphicalUserInterfaceBuilder BuildSidebar(GuiContext ctx)
        {
            Color navAccent = new Color(0f, 1f, 1f, 1f); // Singularity Cyan

            return new GraphicalUserInterfaceBuilder("Sidebar")
                .WithPadding(0)
                .WithBackgroundColor(new Color(0.12f, 0.12f, 0.14f))
                .OnBuild(sidebarRoot =>
                {
                    var tabRow = new VisualElement { style = { flexDirection = FlexDirection.Row, backgroundColor = new Color(0.08f, 0.08f, 0.1f) } };
                    tabRow.style.borderBottomWidth = 2;
                    tabRow.style.borderBottomColor = Color.gray;

                    tabRow.Add(new ForgeNavButtonBuilder("1. Cartography", navAccent, () => SwitchTab(0)).CreateGui(ctx));
                    tabRow.Add(new ForgeNavButtonBuilder("2. Refinement", navAccent, () => SwitchTab(1)).CreateGui(ctx));
                    tabRow.Add(new ForgeNavButtonBuilder("3. Detailing", navAccent, () => SwitchTab(2)).CreateGui(ctx));
                    tabRow.Add(new ForgeNavButtonBuilder("4. Publishing", navAccent, () => SwitchTab(3)).CreateGui(ctx));

                    sidebarRoot.Add(tabRow);

                    var contentArea = new ScrollView { style = { flexGrow = 1, paddingLeft = 15, paddingRight = 15, paddingTop = 15 } };
                    _sidebarContent = contentArea;
                    sidebarRoot.Add(contentArea);

                    SwitchTab(0);
                });
        }

        private GraphicalUserInterfaceBuilder BuildMainArea(GuiContext ctx)
        {
            return new GraphicalUserInterfaceBuilder("MainArea")
                .WithPadding(0)
                .OnBuild(ve =>
                {
                    ve.style.backgroundColor = Color.black;
                    ve.style.flexGrow = 1;

                    _3dPreviewElement = new Image { image = _previewRenderTexture, scaleMode = ScaleMode.ScaleAndCrop };
                    _3dPreviewElement.style.position = Position.Absolute;
                    _3dPreviewElement.style.top = 0; _3dPreviewElement.style.bottom = 0; _3dPreviewElement.style.left = 0; _3dPreviewElement.style.right = 0;
                    Register3DCameraEvents(_3dPreviewElement);
                    ve.Add(_3dPreviewElement);

                    _2dCanvasElement = new Image { image = _displayTexture, scaleMode = ScaleMode.ScaleToFit };
                    _2dCanvasElement.style.position = Position.Absolute;
                    _2dCanvasElement.style.bottom = 20; _2dCanvasElement.style.left = 20;
                    _2dCanvasElement.style.width = 300; _2dCanvasElement.style.height = 300;
                    _2dCanvasElement.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);

                    // Style the minimap frame
                    _2dCanvasElement.style.borderLeftWidth = 2; _2dCanvasElement.style.borderLeftColor = Color.cyan;
                    _2dCanvasElement.style.borderRightWidth = 2; _2dCanvasElement.style.borderRightColor = Color.cyan;
                    _2dCanvasElement.style.borderTopWidth = 2; _2dCanvasElement.style.borderTopColor = Color.cyan;
                    _2dCanvasElement.style.borderBottomWidth = 2; _2dCanvasElement.style.borderBottomColor = Color.cyan;

                    Register2DDrawingEvents(_2dCanvasElement);
                    ve.Add(_2dCanvasElement);

                    _lodPreviewContainer = new ScrollView { style = { display = DisplayStyle.None, position = Position.Absolute, top = 0, bottom = 0, left = 0, right = 0, backgroundColor = Color.black } };
                    ve.Add(_lodPreviewContainer);
                });
        }

        private void SwitchTab(int index)
        {
            if (_sidebarContent == null) return;
            _sidebarContent.Clear();

            switch (index)
            {
                case 0: _sidebarContent.Add(BuildCartographyTab()); break;
                case 3: _sidebarContent.Add(BuildPublishingTab()); break;
            }
        }

        private VisualElement BuildCartographyTab()
        {
            var ctx = new GuiContext();
            return new GraphicalUserInterfaceBuilder("TabGen")
                .AddChild(new ForgeLabelBuilder("1. WORLD CARTOGRAPHY").WithHeaderStyle().CreateGui(ctx))
                .AddIntegerData("Width (Tiles)", _mapSize.x, v => _mapSize.x = v)
                .AddIntegerData("Height (Tiles)", _mapSize.y, v => _mapSize.y = v)
                .AddChild(new ForgeButtonBuilder("Resize & Clear Map", HandleDestructiveClear).CreateGui(ctx))
                .AddSeparator(Color.gray, 1)
                .AddChild(new ForgeFoldoutBuilder("IMAGE INGESTION")
                    .AddChild(new ForgeButtonBuilder("Load Source Image", HandleSourceImageLoad).CreateGui(ctx))
                    .AddChild(new ForgeButtonBuilder("GENERATE BIOME MAP", GenerateMapFromImage).CreateGui(ctx))
                    .CreateGui(ctx))
                .Build();
        }

        private VisualElement BuildPublishingTab()
        {
            var ctx = new GuiContext();
            return new GraphicalUserInterfaceBuilder("TabPub")
                .AddChild(new ForgeLabelBuilder("4. LOD PUBLISHING").WithHeaderStyle().CreateGui(ctx))
                .AddChild(new ForgeButtonBuilder("BAKE LOD CASCADE", () => BakeLOD()).CreateGui(ctx))
                .Build();
        }

        private void HandleDestructiveClear()
        {
#if UNITY_EDITOR
            if (EditorUtility.DisplayDialog("Destructive Resize", "Resizing will clear the map data texture. Proceed?", "Yes", "Cancel"))
            {
                Rebuild3DTerrainProxy(); InitializeTextures(_mapSize.x, _mapSize.y);
            }
#endif
        }

        private void HandleSourceImageLoad()
        {
            if (_fileBrowser == null) return;
            _fileBrowser.OpenImageFile(tex =>
            {
                _sourceImage = tex;
                _mapSize = new Vector2Int(tex.width, tex.height);
                Rebuild3DTerrainProxy(); InitializeTextures(_mapSize.x, _mapSize.y);
            }, () => { });
        }

        private void GenerateMapFromImage()
        {
            if (_sourceImage == null) return;
            for (int y = 0; y < _mapSize.y; y++)
            {
                for (int x = 0; x < _mapSize.x; x++)
                {
                    Color srcColor = _sourceImage.GetPixel(x, y);
                    int index = y * _mapSize.x + x;
                    _dataPixels[index] = new Color(_activeHeight, (float)TerrainType.Plains / 255f, 0, 0f);
                    _displayPixels[index] = GeneratePreviewColor(_dataPixels[index]);
                }
            }
            ApplyPixels();
        }

        private void BakeLOD()
        {
            ForgeLogger.Log("Initiating LOD Cascade Bake for SST Repository...").WithHeader("Publishing").SendToUnity();
        }

        private void ApplyPixels()
        {
            MapDataTexture.SetPixels(_dataPixels);
            MapDataTexture.Apply();
            _displayTexture.SetPixels(_displayPixels);
            _displayTexture.Apply();
            if (_previewCamera != null) _previewCamera.Render();
        }

        private Color GeneratePreviewColor(Color data)
        {
            int biomeInt = Mathf.RoundToInt(data.g * 255f);
            TerrainType type = (TerrainType)biomeInt;
            Color baseColor = _biomeColors.ContainsKey(type) ? _biomeColors[type] : Color.magenta;
            return baseColor * (data.r + 0.2f);
        }

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
            _previewTerrainData = new TerrainData { heightmapResolution = 257, alphamapResolution = 256, size = new Vector3(_mapSize.x, 50f, _mapSize.y) };
            _previewTerrainObj = Terrain.CreateTerrainGameObject(_previewTerrainData);
            _previewTerrainObj.transform.SetParent(_previewRoot.transform);
            _previewTerrainObj.layer = PREVIEW_LAYER;
            _previewTerrain = _previewTerrainObj.GetComponent<Terrain>();
            _previewTerrainObj.transform.localPosition = new Vector3(-_mapSize.x / 2f, 0, -_mapSize.y / 2f);
        }

        private void UpdateCameraPosition()
        {
            if (_previewCamera == null) return;
            Quaternion rot = Quaternion.Euler(_cameraOrbit.y, _cameraOrbit.x, 0);
            _previewCamera.transform.position = rot * new Vector3(0, 0, -_cameraDistance);
            _previewCamera.transform.LookAt(Vector3.zero);
        }

        private void Register2DDrawingEvents(Image img)
        {
            img.RegisterCallback<PointerDownEvent>(e => { _isDrawing = true; PaintAtPosition(e.localPosition, img); });
            img.RegisterCallback<PointerMoveEvent>(e => { if (_isDrawing) PaintAtPosition(e.localPosition, img); });
            img.RegisterCallback<PointerUpEvent>(e => { _isDrawing = false; ApplyPixels(); });
        }

        private void PaintAtPosition(Vector2 localPos, Image img)
        {
            float xPct = localPos.x / img.layout.width;
            float yPct = 1.0f - (localPos.y / img.layout.height);
            int px = Mathf.Clamp((int)(xPct * _mapSize.x), 0, _mapSize.x - 1);
            int py = Mathf.Clamp((int)(yPct * _mapSize.y), 0, _mapSize.y - 1);
            int index = py * _mapSize.x + px;
            _dataPixels[index] = new Color(_activeHeight, (float)_activeBiome / 255f, 0, 0);
            _displayPixels[index] = GeneratePreviewColor(_dataPixels[index]);
            _displayTexture.SetPixels(_displayPixels);
            _displayTexture.Apply();
        }

        private void Register3DCameraEvents(Image img)
        {
            img.RegisterCallback<PointerDownEvent>(e => { if (e.button == 1) _isDraggingCam = true; });
            img.RegisterCallback<PointerUpEvent>(e => _isDraggingCam = false);
            img.RegisterCallback<PointerMoveEvent>(e => { if (_isDraggingCam) { _cameraOrbit.x += e.deltaPosition.x; UpdateCameraPosition(); } });
        }

        public void Dispose()
        {
            if (_previewRenderTexture != null) _previewRenderTexture.Release();
            if (_previewRoot != null) UnityEngine.Object.DestroyImmediate(_previewRoot);
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) => WorkshopUxmlBaker.Bake(CreateGui(new GuiContext()), "MapBuilderRegistry");
        public void FromUIDocument(string assetPath) { }
    }
}