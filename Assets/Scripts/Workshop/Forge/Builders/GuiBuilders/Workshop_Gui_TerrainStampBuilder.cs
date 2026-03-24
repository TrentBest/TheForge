using System;
using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.Forge.IO;

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders
{
    public class Workshop_Gui_TerrainStampBuilder : IGuiProvider, IDisposable
    {
        public string Title => "Cartographer: Procedural Stamp Definer";

        // Core Data
        private Texture2D _sourceImage;
        private int _gridSize;
        private string _processGroup;

        // Procedural Parameters
        private int _seed = 1337;
        private float _minHeight = 0.0f;
        private float _medianHeight = 0.3f;
        private float _maxHeight = 0.8f;
        private float _noiseFrequency = 5.0f;

        // Art Alignment
        private Vector2 _imageScale = Vector2.one;
        private Vector2 _imageOffset = Vector2.zero;

        // UI Components
        private IForgeFileBrowser _fileBrowser = new EditorNativeFileBrowser();
        private Texture2D _graphTexture;
        private Image _graphPreviewElement;

        // 3D Preview (Isolated)
        private const int PREVIEW_LAYER = 31;
        private GameObject _previewRoot; // Container for easy cleanup
        private RenderTexture _previewRenderTexture;
        private Camera _previewCamera;
        private Terrain _previewTerrain;
        private TerrainData _previewTerrainData;

        // Camera State
        private Vector2 _cameraOrbit = new Vector2(45f, 45f);
        private float _cameraDistance = 200f;

        public Workshop_Gui_TerrainStampBuilder() : this(256) { }

        public Workshop_Gui_TerrainStampBuilder(int targetGridSize = 256)
        {
            _gridSize = targetGridSize;
            _processGroup = "Stamp_" + Guid.NewGuid().ToString().Substring(0, 6);

            InitializeGraphTexture();
            DestroyGhostObjects(); // Trigger the aggressive sweeper
            Setup3DPreviewScene();
        }

        private void DestroyGhostObjects()
        {
            // 1. Ultra-aggressive sweep for old immortal Terrains from previous prompts
            var allTerrains = Resources.FindObjectsOfTypeAll<Terrain>();
            foreach (var t in allTerrains)
            {
                if (t != null && t.gameObject != null && (t.gameObject.hideFlags & HideFlags.HideAndDontSave) != 0)
                {
                    if (t.name.Contains("Forge") || t.name.Contains("Preview") || t.name.Contains("Isolated"))
                    {
                        Debug.Log($"[Forge] Exorcising Ghost Terrain: {t.name}");
                        UnityEngine.Object.DestroyImmediate(t.gameObject);
                    }
                }
            }

            // 2. Sweep for orphaned root containers or cameras
            var allGos = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var go in allGos)
            {
                if (go != null && (go.hideFlags & HideFlags.HideAndDontSave) != 0)
                {
                    if (go.name.StartsWith("Forge_Stamp_Root_") || go.name.StartsWith("StampPreviewCamera"))
                    {
                        UnityEngine.Object.DestroyImmediate(go);
                    }
                }
            }
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new SplitPanelBuilder(sidebarWidth: 400)
                .WithSidebar(BuildSidebar())
                .WithMain(BuildMainArea())
                .CreateGui(ctx);

            root.RegisterCallback<DetachFromPanelEvent>(e => Dispose());

            return root;
        }

        private GraphicalUserInterfaceBuilder BuildSidebar()
        {
            var sidebar = new GraphicalUserInterfaceBuilder("Sidebar")
                .WithPadding(15).WithScrollable(true)
                .WithBackgroundColor(new Color(0.12f, 0.12f, 0.14f));

            sidebar.AddChild(new Label("TERRAIN STAMP DEFINER") { style = { fontSize = 18, color = new Color(0.3f, 0.8f, 0.5f), unityFontStyleAndWeight = FontStyle.Bold } });

            sidebar.AddButton("Load 2D Overlay Tile", () => {
                _fileBrowser.OpenImageFile(tex => {
                    _sourceImage = tex;
                    _imageScale.y = (float)tex.height / _gridSize;
                    _imageScale.x = (float)tex.width / _gridSize;
                    ApplyOverlayTexture();
                    GenerateProceduralHeightmap();
                }, () => { });
            });

            sidebar.AddSeparator(Color.gray, 1);
            sidebar.AddChild(new Label("ART OVERLAY ALIGNMENT") { style = { color = Color.cyan, marginTop = 10, unityFontStyleAndWeight = FontStyle.Bold } });

            sidebar.AddSliderData("Scale X", 0.1f, 3.0f, _imageScale.x, v => { _imageScale.x = v; ApplyOverlayTexture(); });
            sidebar.AddSliderData("Scale Y", 0.1f, 3.0f, _imageScale.y, v => { _imageScale.y = v; ApplyOverlayTexture(); });
            sidebar.AddSliderData("Offset X", -1f, 1f, _imageOffset.x, v => { _imageOffset.x = v; ApplyOverlayTexture(); });
            sidebar.AddSliderData("Offset Y", -1f, 1f, _imageOffset.y, v => { _imageOffset.y = v; ApplyOverlayTexture(); });

            sidebar.AddSeparator(Color.gray, 1);
            sidebar.AddChild(new Label("TOPOLOGY EQUATION (Squirrel PRNG)") { style = { color = new Color(0.8f, 0.6f, 0.3f), marginTop = 10, unityFontStyleAndWeight = FontStyle.Bold } });

            _graphPreviewElement = new Image { image = _graphTexture, scaleMode = ScaleMode.StretchToFill, style = { height = 80, marginTop = 10, marginBottom = 10, backgroundColor = Color.black, borderBottomWidth = 1, borderTopWidth = 1,  } };
            sidebar.AddChild(_graphPreviewElement);

            sidebar.AddIntegerData("Seed", _seed, v => { _seed = v; GenerateProceduralHeightmap(); });
            sidebar.AddButton("Randomize Seed", () => { _seed = UnityEngine.Random.Range(0, 999999); GenerateProceduralHeightmap(); });

            sidebar.AddSliderData("Min Height", 0f, 1f, _minHeight, v => { _minHeight = v; GenerateProceduralHeightmap(); });
            sidebar.AddSliderData("Median Height", 0f, 1f, _medianHeight, v => { _medianHeight = v; GenerateProceduralHeightmap(); });
            sidebar.AddSliderData("Max Height", 0f, 1f, _maxHeight, v => { _maxHeight = v; GenerateProceduralHeightmap(); });
            sidebar.AddSliderData("Frequency (Bumps)", 1f, 30f, _noiseFrequency, v => { _noiseFrequency = v; GenerateProceduralHeightmap(); });

            sidebar.AddSeparator(Color.gray, 2);
            sidebar.AddButton("BAKE STAMP DEFINITION", SerializeAndSaveStamp);

            return sidebar;
        }

        private GraphicalUserInterfaceBuilder BuildMainArea()
        {
            var mainArea = new GraphicalUserInterfaceBuilder("MainArea")
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Stretch);

            Image _3dRenderPreview = new Image { image = _previewRenderTexture, scaleMode = ScaleMode.ScaleAndCrop, style = { flexGrow = 1, backgroundColor = Color.black } };
            Register3DCameraEvents(_3dRenderPreview);
            mainArea.AddChild(_3dRenderPreview);

            return mainArea;
        }

        public void Dispose()
        {
            // FIX: Prevent RenderTexture warning
            if (_previewCamera != null) _previewCamera.targetTexture = null;

            if (_previewRenderTexture != null)
            {
                _previewRenderTexture.Release();
                UnityEngine.Object.DestroyImmediate(_previewRenderTexture);
            }
            if (_graphTexture != null) UnityEngine.Object.DestroyImmediate(_graphTexture);
            if (_previewTerrainData != null) UnityEngine.Object.DestroyImmediate(_previewTerrainData);

            // Nuke the root, instantly killing the camera and the terrain GameObject
            if (_previewRoot != null) UnityEngine.Object.DestroyImmediate(_previewRoot);
        }

        private void SerializeAndSaveStamp()
        {
            Debug.Log($"[Forge] Saving Stamp! Seed: {_seed}");
        }

        private void InitializeGraphTexture()
        {
            _graphTexture = new Texture2D(256, 64, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
        }

        private void UpdateEquationGraph()
        {
            if (_graphTexture == null) return;

            int w = _graphTexture.width;
            int h = _graphTexture.height;
            Color bgColor = new Color(0.1f, 0.1f, 0.15f);
            Color curveColor = new Color(0.3f, 0.9f, 0.5f);
            Color rawNoiseColor = new Color(1f, 1f, 1f, 0.2f);

            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                    _graphTexture.SetPixel(x, y, bgColor);

            for (int x = 0; x < w; x++)
            {
                float t = (float)x / w;
                float rawNoise = GetSeededValueNoise(t * _noiseFrequency, 0, (uint)_seed);

                float finalHeight;
                if (rawNoise < 0.5f)
                {
                    finalHeight = Mathf.Lerp(_minHeight, _medianHeight, rawNoise * 2f);
                }
                else
                {
                    finalHeight = Mathf.Lerp(_medianHeight, _maxHeight, (rawNoise - 0.5f) * 2f);
                }

                int rawY = Mathf.Clamp(Mathf.RoundToInt(rawNoise * (h - 1)), 0, h - 1);
                int finalY = Mathf.Clamp(Mathf.RoundToInt(finalHeight * (h - 1)), 0, h - 1);

                _graphTexture.SetPixel(x, rawY, rawNoiseColor);
                _graphTexture.SetPixel(x, finalY, curveColor);

                for (int fillY = 0; fillY < finalY; fillY += 2)
                {
                    _graphTexture.SetPixel(x, fillY, new Color(curveColor.r, curveColor.g, curveColor.b, 0.3f));
                }
            }

            _graphTexture.Apply();
        }

        private void Setup3DPreviewScene()
        {
            _previewRoot = new GameObject("Forge_Stamp_Root_" + _processGroup);
            _previewRoot.hideFlags = HideFlags.HideAndDontSave;

            _previewRenderTexture = new RenderTexture(1024, 1024, 24);

            GameObject camObj = new GameObject("StampPreviewCamera_" + _processGroup);
            camObj.transform.SetParent(_previewRoot.transform);
            camObj.layer = PREVIEW_LAYER;

            _previewCamera = camObj.AddComponent<Camera>();
            _previewCamera.targetTexture = _previewRenderTexture;
            _previewCamera.clearFlags = CameraClearFlags.SolidColor;
            _previewCamera.backgroundColor = new Color(0.1f, 0.1f, 0.15f);
            _previewCamera.cullingMask = 1 << PREVIEW_LAYER;

            _previewTerrainData = new TerrainData
            {
                heightmapResolution = _gridSize + 1,
                alphamapResolution = _gridSize,
                size = new Vector3(_gridSize, 50f, _gridSize)
            };

            _previewTerrainData.terrainLayers = new TerrainLayer[] { new TerrainLayer { diffuseTexture = Texture2D.whiteTexture } };

            GameObject terrainObj = Terrain.CreateTerrainGameObject(_previewTerrainData);
            terrainObj.name = "Forge_Stamp_Terrain_" + _processGroup;
            terrainObj.transform.SetParent(_previewRoot.transform);
            terrainObj.layer = PREVIEW_LAYER;

            _previewTerrain = terrainObj.GetComponent<Terrain>();
            _previewTerrain.drawInstanced = true;
            terrainObj.transform.localPosition = new Vector3(-_gridSize / 2f, 0, -_gridSize / 2f);

            // Prevent absolute black shadows
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.4f, 0.4f, 0.4f);

            UpdateCameraPosition();
            GenerateProceduralHeightmap();
        }

        private void GenerateProceduralHeightmap()
        {
            UpdateEquationGraph();

            if (_previewTerrainData == null) return;
            float[,] heights = new float[_gridSize + 1, _gridSize + 1];

            for (int y = 0; y < _gridSize + 1; y++)
            {
                for (int x = 0; x < _gridSize + 1; x++)
                {
                    float nx = (float)x / _gridSize * _noiseFrequency;
                    float ny = (float)y / _gridSize * _noiseFrequency;
                    float rawNoise = GetSeededValueNoise(nx, ny, (uint)_seed);

                    float finalHeight;
                    if (rawNoise < 0.5f)
                    {
                        finalHeight = Mathf.Lerp(_minHeight, _medianHeight, rawNoise * 2f);
                    }
                    else
                    {
                        finalHeight = Mathf.Lerp(_medianHeight, _maxHeight, (rawNoise - 0.5f) * 2f);
                    }
                    heights[y, x] = finalHeight;
                }
            }

            _previewTerrainData.SetHeightsDelayLOD(0, 0, heights);
            _previewTerrain.Flush();
            if (_previewCamera != null) _previewCamera.Render();
        }

        private static uint Squirrel3(int position, uint seed)
        {
            uint BIT_NOISE1 = 0x68E31DA4; uint BIT_NOISE2 = 0xB5297A4D; uint BIT_NOISE3 = 0x1B56C4E9;
            uint m = (uint)position; m *= BIT_NOISE1; m += seed; m ^= (m >> 8); m += BIT_NOISE2; m ^= (m << 8); m *= BIT_NOISE3; m ^= (m >> 8); return m;
        }

        private float GetSeededValueNoise(float x, float y, uint seed)
        {
            int ix = Mathf.FloorToInt(x); int iy = Mathf.FloorToInt(y);
            float fx = x - ix; float fy = y - iy;
            float sx = fx * fx * (3.0f - 2.0f * fx); float sy = fy * fy * (3.0f - 2.0f * fy);
            int p00 = ix + iy * 100019; int p10 = (ix + 1) + iy * 100019;
            int p01 = ix + (iy + 1) * 100019; int p11 = (ix + 1) + (iy + 1) * 100019;
            float n00 = (Squirrel3(p00, seed) % 10000) / 10000f; float n10 = (Squirrel3(p10, seed) % 10000) / 10000f;
            float n01 = (Squirrel3(p01, seed) % 10000) / 10000f; float n11 = (Squirrel3(p11, seed) % 10000) / 10000f;
            float nx0 = Mathf.Lerp(n00, n10, sx); float nx1 = Mathf.Lerp(n01, n11, sx);
            return Mathf.Lerp(nx0, nx1, sy);
        }

        private void ApplyOverlayTexture()
        {
            if (_sourceImage == null || _previewTerrainData == null) return;
            TerrainLayer layer = _previewTerrainData.terrainLayers[0];
            layer.diffuseTexture = _sourceImage;
            layer.tileSize = new Vector2(_gridSize * _imageScale.x, _gridSize * _imageScale.y);
            layer.tileOffset = new Vector2(_gridSize * _imageOffset.x, _gridSize * _imageOffset.y);
            _previewTerrainData.terrainLayers = new TerrainLayer[] { layer };
            if (_previewCamera != null) _previewCamera.Render();
        }

        private void Register3DCameraEvents(Image imgElement)
        {
            bool isDraggingCam = false;
            Vector2 lastMouse = Vector2.zero;

            imgElement.RegisterCallback<PointerDownEvent>(e => {
                if (e.button == 1 || e.button == 0)
                {
                    isDraggingCam = true;
                    lastMouse = e.position;
                    imgElement.CapturePointer(e.pointerId);
                }
            });

            imgElement.RegisterCallback<PointerMoveEvent>(e => {
                if (isDraggingCam)
                {
                    Vector2 delta = (Vector2)e.position - lastMouse;
                    _cameraOrbit.x += delta.x * 0.3f;
                    _cameraOrbit.y -= delta.y * 0.3f;
                    _cameraOrbit.y = Mathf.Clamp(_cameraOrbit.y, 5f, 85f);
                    lastMouse = e.position;
                    UpdateCameraPosition();
                }
            });

            imgElement.RegisterCallback<PointerUpEvent>(e => {
                if (isDraggingCam)
                {
                    isDraggingCam = false;
                    imgElement.ReleasePointer(e.pointerId);
                }
            });

            imgElement.RegisterCallback<WheelEvent>(e => {
                _cameraDistance = Mathf.Clamp(_cameraDistance + e.delta.y * 5f, 20f, 500f);
                UpdateCameraPosition();
            });
        }

        private void UpdateCameraPosition()
        {
            if (_previewCamera == null) return;
            Quaternion rot = Quaternion.Euler(_cameraOrbit.y, _cameraOrbit.x, 0);
            _previewCamera.transform.position = rot * new Vector3(0, 0, -_cameraDistance) + Vector3.zero;
            _previewCamera.transform.LookAt(Vector3.zero);
            _previewCamera.Render();
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}