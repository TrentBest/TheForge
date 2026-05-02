using Assets.Scripts.Warlords;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.IO; // Assuming TerrainType

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    public class Workshop_Gui_TileConverter : IGuiProvider
    {
        public string Title => "Cartographer: Tile Image Ingestor";

        // Core Image Data
        private Texture2D _sourceImage;
        private Texture2D _splatDataTexture;
        private Color[] _splatPixels;
        private int _tileSize;

        // UI Elements
        private Image _2dSourcePreview;
        private Image _3dRenderPreview;
        private VisualElement _mappingContainer;
        private IForgeFileBrowser _fileBrowser = new EditorNativeFileBrowser();

        // Mapping Rules
        private class ColorRule
        {
            public Color SourceColor;
            public TerrainType TargetBiome = TerrainType.Plains;
            public float TargetHeight = 0.2f;
            public float TargetCohesion = 1.0f;
            public float TargetDivergence = 0.0f;
        }
        private List<ColorRule> _activeRules = new List<ColorRule>();

        // 3D Preview
        private const int PREVIEW_LAYER = 31;
        private RenderTexture _previewRenderTexture;
        private Camera _previewCamera;
        private Terrain _previewTerrain;
        private TerrainData _previewTerrainData;
        private Vector2 _cameraOrbit = new Vector2(45f, 30f);
        private float _cameraDistance = 150f;

        public Workshop_Gui_TileConverter()
        {
            _tileSize = 256;
            InitializeTextures();
            Setup3DPreviewScene();
        }

        public Workshop_Gui_TileConverter(int targetTileSize = 256)
        {
            _tileSize = targetTileSize;
            InitializeTextures();
            Setup3DPreviewScene();
        }

        private void InitializeTextures()
        {
            _splatDataTexture = new Texture2D(_tileSize, _tileSize, TextureFormat.RGBAFloat, false) { filterMode = FilterMode.Point };
            _splatPixels = new Color[_tileSize * _tileSize];

            // Default flat layout
            for (int i = 0; i < _splatPixels.Length; i++)
                _splatPixels[i] = new Color(0.2f, (float)TerrainType.Plains / 255f, 1f, 0f);

            _splatDataTexture.SetPixels(_splatPixels);
            _splatDataTexture.Apply();
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new ForgeSplitPanelBuilder(sidebarWidth: 450)
                .WithSidebar(BuildSidebar())
                .WithMain(BuildMainArea())
                .CreateGui(ctx);

            return root;
        }

        private GraphicalUserInterfaceBuilder BuildSidebar()
        {
            var sidebar = new GraphicalUserInterfaceBuilder("Sidebar")
                .WithPadding(15).WithScrollable(true)
                .WithBackgroundColor(new Color(0.12f, 0.12f, 0.14f));

            sidebar.AddChild(new Label("TILE INGESTOR") { style = { fontSize = 18, color = new Color(0.8f, 0.6f, 0.3f), unityFontStyleAndWeight = FontStyle.Bold } });

            sidebar.AddButton("1. Load 2D Tile Image", () => {
                _fileBrowser.OpenImageFile(tex => {
                    _sourceImage = tex;
                    _2dSourcePreview.image = _sourceImage;
                    ExtractPaletteAndBuildRules();
                }, () => Debug.Log("Image load cancelled."));
            });

            sidebar.AddSeparator(Color.gray, 2);
            sidebar.AddChild(new Label("2. MAP COLORS TO DATA") { style = { color = Color.cyan, marginTop = 10, marginBottom = 10, unityFontStyleAndWeight = FontStyle.Bold } });

            // Container where we will inject the dynamically generated mapping rules
            sidebar.OnBuild(ve => {
                _mappingContainer = new VisualElement();
                ve.Add(_mappingContainer);
            });

            sidebar.AddSeparator(Color.gray, 2);
            sidebar.AddButton("3. BAKE TILE TERRAIN", BakeRulesToSplatMap);

            return sidebar;
        }

        private GraphicalUserInterfaceBuilder BuildMainArea()
        {
            var mainArea = new GraphicalUserInterfaceBuilder("MainArea")
                .WithFlexLayout(FlexDirection.Column, Justify.SpaceBetween, Align.Stretch);

            // TOP: Source Image View
            _2dSourcePreview = new Image { scaleMode = ScaleMode.ScaleToFit, style = { flexGrow = 1, backgroundColor = Color.black } };
            mainArea.AddChild(_2dSourcePreview);

            // BOTTOM: 3D Live Render Preview
            _3dRenderPreview = new Image { image = _previewRenderTexture, scaleMode = ScaleMode.ScaleAndCrop, style = { flexGrow = 1, borderTopWidth = 2, borderTopColor = Color.cyan } };
            Register3DCameraEvents(_3dRenderPreview);
            mainArea.AddChild(_3dRenderPreview);

            return mainArea;
        }

        // ==========================================
        // COLOR ANALYSIS & RULE GENERATION
        // ==========================================

        private void ExtractPaletteAndBuildRules()
        {
            if (_sourceImage == null) return;
            _mappingContainer.Clear();
            _activeRules.Clear();

            // Extract the most common colors to form a palette
            var pixels = _sourceImage.GetPixels();
            var colorFrequency = new Dictionary<Color, int>();

            foreach (var p in pixels)
            {
                // Round slightly to group near-identical anti-aliased pixels
                Color rounded = new Color(Mathf.Round(p.r * 20f) / 20f, Mathf.Round(p.g * 20f) / 20f, Mathf.Round(p.b * 20f) / 20f, 1f);
                if (colorFrequency.ContainsKey(rounded)) colorFrequency[rounded]++;
                else colorFrequency[rounded] = 1;
            }

            // Take the top 10 most prominent colors in the tile
            var palette = colorFrequency.OrderByDescending(kv => kv.Value).Take(10).Select(kv => kv.Key).ToList();

            foreach (var color in palette)
            {
                var rule = new ColorRule { SourceColor = color };
                _activeRules.Add(rule);

                var row = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 8, backgroundColor = new Color(0.2f, 0.2f, 0.2f), paddingBottom = 5, paddingTop = 5 } };

                // Color Swatch
                row.Add(new VisualElement { style = { width = 30, height = 30, backgroundColor = color, marginRight = 10, marginLeft = 5, borderBottomColor = Color.black, borderBottomWidth = 1 } });

                // Controls Column
                var controlsCol = new VisualElement { style = { flexGrow = 1 } };

                // Biome Dropdown
                var drop = new DropdownField(Enum.GetNames(typeof(TerrainType)).ToList(), 0);
                drop.RegisterValueChangedCallback(evt => {
                    rule.TargetBiome = (TerrainType)Enum.Parse(typeof(TerrainType), evt.newValue);
                    BakeRulesToSplatMap(); // Auto-update preview!
                });
                controlsCol.Add(drop);

                // Height Slider
                var heightSlider = new Slider("Height", 0f, 1f, SliderDirection.Horizontal) { value = rule.TargetHeight };
                heightSlider.RegisterValueChangedCallback(evt => {
                    rule.TargetHeight = evt.newValue;
                    BakeRulesToSplatMap(); // Auto-update preview!
                });
                controlsCol.Add(heightSlider);

                row.Add(controlsCol);
                _mappingContainer.Add(row);
            }

            // Run initial bake
            BakeRulesToSplatMap();
        }

        private void BakeRulesToSplatMap()
        {
            if (_sourceImage == null || _activeRules.Count == 0) return;

            for (int y = 0; y < _tileSize; y++)
            {
                for (int x = 0; x < _tileSize; x++)
                {
                    // Sample source image (supports scaling if tile != source image size)
                    float u = (float)x / _tileSize;
                    float v = (float)y / _tileSize;
                    Color srcPixel = _sourceImage.GetPixelBilinear(u, v);

                    // Find closest rule by color distance
                    ColorRule closestRule = _activeRules.OrderBy(r => Vector4.Distance(r.SourceColor, srcPixel)).First();

                    // Encode data: R=Height, G=Biome, B=Cohesion, A=Divergence
                    _splatPixels[y * _tileSize + x] = new Color(
                        closestRule.TargetHeight,
                        (float)closestRule.TargetBiome / 255f,
                        closestRule.TargetCohesion,
                        closestRule.TargetDivergence
                    );
                }
            }

            _splatDataTexture.SetPixels(_splatPixels);
            _splatDataTexture.Apply();

            SyncTerrainData();
        }

        // ==========================================
        // 3D PREVIEW (Terrain Generation)
        // ==========================================

        private void Setup3DPreviewScene()
        {
            _previewRenderTexture = new RenderTexture(1024, 512, 24);

            GameObject camObj = new GameObject("TileConverterPreviewCamera") { hideFlags = HideFlags.HideAndDontSave, layer = PREVIEW_LAYER };
            _previewCamera = camObj.AddComponent<Camera>();
            _previewCamera.targetTexture = _previewRenderTexture;
            _previewCamera.clearFlags = CameraClearFlags.SolidColor;
            _previewCamera.backgroundColor = new Color(0.05f, 0.05f, 0.08f);
            _previewCamera.cullingMask = 1 << PREVIEW_LAYER;

            _previewTerrainData = new TerrainData
            {
                heightmapResolution = _tileSize + 1,
                alphamapResolution = _tileSize,
                size = new Vector3(_tileSize, 50f, _tileSize) // X, Max Height, Z
            };

            SetupTerrainLayers();

            GameObject terrainObj = Terrain.CreateTerrainGameObject(_previewTerrainData);
            terrainObj.name = "Forge_TileConverter_Preview";
            terrainObj.hideFlags = HideFlags.HideAndDontSave;
            terrainObj.layer = PREVIEW_LAYER;

            _previewTerrain = terrainObj.GetComponent<Terrain>();
            _previewTerrain.drawInstanced = true;
            terrainObj.transform.position = new Vector3(-_tileSize / 2f, 0, -_tileSize / 2f);

            UpdateCameraPosition();
        }

        private void SetupTerrainLayers()
        {
            int numBiomes = Enum.GetValues(typeof(TerrainType)).Length;
            TerrainLayer[] layers = new TerrainLayer[numBiomes];
            for (int i = 0; i < numBiomes; i++)
            {
                layers[i] = new TerrainLayer { name = ((TerrainType)i).ToString(), diffuseTexture = Texture2D.whiteTexture };
            }
            _previewTerrainData.terrainLayers = layers;
        }

        private void SyncTerrainData()
        {
            if (_previewTerrainData == null) return;

            float[,] heights = new float[_tileSize + 1, _tileSize + 1];
            int layerCount = _previewTerrainData.alphamapLayers;
            float[,,] alphaMaps = new float[_tileSize, _tileSize, layerCount];

            for (int y = 0; y < _tileSize; y++)
            {
                for (int x = 0; x < _tileSize; x++)
                {
                    Color pixelData = _splatPixels[y * _tileSize + x];

                    // Map Red channel to physical Terrain height
                    heights[y, x] = pixelData.r;

                    // Map Green channel to splat layer
                    int biomeIndex = Mathf.RoundToInt(pixelData.g * 255f);
                    for (int l = 0; l < layerCount; l++)
                    {
                        alphaMaps[y, x, l] = (l == biomeIndex % layerCount) ? 1.0f : 0.0f;
                    }
                }
            }

            _previewTerrainData.SetHeightsDelayLOD(0, 0, heights);
            _previewTerrainData.SetAlphamaps(0, 0, alphaMaps);
            _previewTerrain.Flush();

            if (_previewCamera != null) _previewCamera.Render();
        }

        // Camera dragging logic
        private void Register3DCameraEvents(Image imgElement)
        {
            bool isDraggingCam = false; Vector2 lastMouse = Vector2.zero;
            imgElement.RegisterCallback<PointerDownEvent>(e => { if (e.button == 1 || e.button == 0) { isDraggingCam = true; lastMouse = e.position; imgElement.CapturePointer(e.pointerId); } });
            imgElement.RegisterCallback<PointerMoveEvent>(e => {
                if (isDraggingCam)
                {
                    Vector2 delta = (Vector2)e.position - lastMouse;
                    _cameraOrbit.x += delta.x * 0.5f; _cameraOrbit.y = Mathf.Clamp(_cameraOrbit.y - delta.y * 0.5f, 5f, 85f);
                    lastMouse = e.position; UpdateCameraPosition();
                }
            });
            imgElement.RegisterCallback<PointerUpEvent>(e => { isDraggingCam = false; imgElement.ReleasePointer(e.pointerId); });
            imgElement.RegisterCallback<WheelEvent>(e => { _cameraDistance = Mathf.Clamp(_cameraDistance + e.delta.y * 2f, 20f, 300f); UpdateCameraPosition(); });
        }

        private void UpdateCameraPosition()
        {
            if (_previewCamera == null) return;
            Quaternion rot = Quaternion.Euler(_cameraOrbit.y, _cameraOrbit.x, 0);
            _previewCamera.transform.position = rot * new Vector3(0, 0, -_cameraDistance) + Vector3.zero;
            _previewCamera.transform.LookAt(Vector3.zero);
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}