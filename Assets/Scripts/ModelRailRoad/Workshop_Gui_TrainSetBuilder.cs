using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using Workshop.UI_And_Tools.Forge.IO;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    public enum ModelScale { Real_World_1to1, O_Scale_1to48, HO_Scale_1to87, N_Scale_1to160, Z_Scale_1to220 }
    public enum ViewPerspective { Architect_Macro, Modeler_Micro, FirstPerson_Explorer }
    public enum TrackLayingMode { Spline_Procedural, Sectional_Snap }
    public enum SceneryBrushType { Splat_Texture, Raise_Lower_Terrain, Paint_Trees, Place_Structures }

    public class Workshop_Gui_TrainSetBuilder : IGuiProvider, IDisposable
    {
        public string Title => "Digital RailWorks: SST Editor";

        private string _processGroup;

        // --- CORE DATA (SINGLE SOURCE OF TRUTH) ---
        // Always 1:1 meters. A 2000x2000 terrain = 2km x 2km real world.
        private Vector2Int _terrainSSTSize = new Vector2Int(2000, 2000);
        private ModelScale _targetModelScale = ModelScale.HO_Scale_1to87;
        private ViewPerspective _activePerspective = ViewPerspective.Modeler_Micro;

        // --- UI CONTAINERS ---
        private VisualElement _tabEnvironment;
        private VisualElement _tabTerraform;
        private VisualElement _tabTracks;
        private VisualElement _tabRollingStock;

        // --- COMMON UI ELEMENTS ---
        private Image _3dPreviewElement;

        // --- BRUSH & TOOL STATE ---
        private TrackLayingMode _activeTrackMode = TrackLayingMode.Spline_Procedural;
        private SceneryBrushType _activeSceneryBrush = SceneryBrushType.Raise_Lower_Terrain;
        private float _brushSize = 10f;
        private float _brushStrength = 0.5f;

        // --- 3D PROXY PREVIEW ---
        private const int PREVIEW_LAYER = 31;
        private GameObject _previewRoot;
        private RenderTexture _previewRenderTexture;
        private Camera _previewCamera;

        // Scene Objects
        private GameObject _roomContextObj; // Walls, Floor, Table
        private Terrain _previewTerrain;
        private TerrainData _previewTerrainData;

        // Camera State
        private bool _isDraggingCam = false;
        private Vector2 _lastMousePos;
        private Vector2 _cameraOrbit = new Vector2(45f, 45f);
        private float _cameraDistance = 500f;

        public Workshop_Gui_TrainSetBuilder()
        {
            _processGroup = "RailWorks_" + Guid.NewGuid().ToString().Substring(0, 6);
            DestroyGhostObjects();
            Setup3DPreviewScene();
        }

        private void DestroyGhostObjects()
        {
            var allGos = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var go in allGos)
            {
                if (go != null && (go.hideFlags & HideFlags.HideAndDontSave) != 0)
                {
                    if (go.name.StartsWith("Forge_RailWorks_Root_"))
                        UnityEngine.Object.DestroyImmediate(go);
                }
            }
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new ForgeSplitPanelBuilder(sidebarWidth: 420)
                .WithSidebar(BuildSidebar())
                .WithMain(BuildMainArea())
                .CreateGui(ctx);

            root.schedule.Execute(() =>
            {
                if (!_isDraggingCam && _previewCamera != null && _activePerspective != ViewPerspective.FirstPerson_Explorer)
                {
                    _cameraOrbit.x += 2f * Time.deltaTime;
                    UpdateCameraPosition();
                }
            }).Every(16);

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

                    Button btnEnv = CreateTabButton("1. Scope");
                    Button btnTerr = CreateTabButton("2. Terraform");
                    Button btnTrk = CreateTabButton("3. Rails");
                    Button btnLife = CreateTabButton("4. Life");

                    tabRow.Add(btnEnv); tabRow.Add(btnTerr); tabRow.Add(btnTrk); tabRow.Add(btnLife);
                    sidebarRoot.Add(tabRow);

                    var contentArea = new ScrollView { style = { flexGrow = 1, paddingLeft = 15, paddingRight = 15, paddingTop = 15 } };

                    _tabEnvironment = BuildEnvironmentTab();
                    _tabTerraform = BuildTerraformTab();
                    _tabTracks = BuildTracksTab();
                    _tabRollingStock = BuildRollingStockTab();

                    contentArea.Add(_tabEnvironment);
                    contentArea.Add(_tabTerraform);
                    contentArea.Add(_tabTracks);
                    contentArea.Add(_tabRollingStock);

                    Action<VisualElement> SwitchTab = (activeTab) =>
                    {
                        _tabEnvironment.style.display = (activeTab == _tabEnvironment) ? DisplayStyle.Flex : DisplayStyle.None;
                        _tabTerraform.style.display = (activeTab == _tabTerraform) ? DisplayStyle.Flex : DisplayStyle.None;
                        _tabTracks.style.display = (activeTab == _tabTracks) ? DisplayStyle.Flex : DisplayStyle.None;
                        _tabRollingStock.style.display = (activeTab == _tabRollingStock) ? DisplayStyle.Flex : DisplayStyle.None;

                        btnEnv.style.borderBottomColor = (activeTab == _tabEnvironment) ? Color.cyan : Color.clear;
                        btnTerr.style.borderBottomColor = (activeTab == _tabTerraform) ? Color.cyan : Color.clear;
                        btnTrk.style.borderBottomColor = (activeTab == _tabTracks) ? Color.cyan : Color.clear;
                        btnLife.style.borderBottomColor = (activeTab == _tabRollingStock) ? Color.cyan : Color.clear;
                    };

                    btnEnv.clicked += () => SwitchTab(_tabEnvironment);
                    btnTerr.clicked += () => SwitchTab(_tabTerraform);
                    btnTrk.clicked += () => SwitchTab(_tabTracks);
                    btnLife.clicked += () => SwitchTab(_tabRollingStock);

                    SwitchTab(_tabEnvironment);
                });
        }

        private Button CreateTabButton(string text) => new Button { text = text, style = { flexGrow = 1, backgroundColor = Color.clear, color = Color.white, borderBottomWidth = 2, borderBottomColor = Color.clear, borderTopWidth = 0, borderLeftWidth = 0, borderRightWidth = 0, paddingBottom = 10, paddingTop = 10, unityFontStyleAndWeight = FontStyle.Bold, fontSize = 11 } };

        private GraphicalUserInterfaceBuilder BuildMainArea()
        {
            return new GraphicalUserInterfaceBuilder("MainArea")
                .WithPadding(0)
                .OnBuild(ve =>
                {
                    ve.style.backgroundColor = Color.black;
                    ve.style.flexGrow = 1;
                    ve.style.position = Position.Relative;

                    _3dPreviewElement = new Image { image = _previewRenderTexture, scaleMode = ScaleMode.ScaleAndCrop };
                    _3dPreviewElement.style.position = Position.Absolute;
                    _3dPreviewElement.style.top = 0; _3dPreviewElement.style.bottom = 0; _3dPreviewElement.style.left = 0; _3dPreviewElement.style.right = 0;
                    Register3DCameraEvents(_3dPreviewElement);

                    ve.Add(_3dPreviewElement);
                });
        }

        // ==========================================
        // TAB 1: ENVIRONMENT & SCOPE (The SST Manager)
        // ==========================================
        private VisualElement BuildEnvironmentTab()
        {
            return new GraphicalUserInterfaceBuilder("TabEnv")
                .AddChild(new Label("SST PROJECTION MATRIX") { style = { fontSize = 16, color = Color.cyan, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 15 } })

                .AddChild(new Label("Data is always 1:1. These settings control how the player perceives it.") { style = { color = Color.gray, fontSize = 10, marginBottom = 10, whiteSpace = WhiteSpace.Normal } })

                .AddDropdownData("Target Miniaturization", Enum.GetNames(typeof(ModelScale)).ToList(), (int)_targetModelScale, val => {
                    _targetModelScale = (ModelScale)Enum.Parse(typeof(ModelScale), val);
                    UpdateProjectionScale();
                })

                .AddDropdownData("Current Perspective", Enum.GetNames(typeof(ViewPerspective)).ToList(), (int)_activePerspective, val => {
                    _activePerspective = (ViewPerspective)Enum.Parse(typeof(ViewPerspective), val);
                    UpdateProjectionScale();
                })

                .AddSeparator(Color.gray, 1)

                .AddChild(new Label("BASE TOPOGRAPHY GENERATION") { style = { color = Color.white, unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10, marginBottom = 5 } })
                .AddIntegerData("Width (Meters)", _terrainSSTSize.x, v => _terrainSSTSize.x = v)
                .AddIntegerData("Depth (Meters)", _terrainSSTSize.y, v => _terrainSSTSize.y = v)
                .AddButton("Regenerate 1:1 Base Terrain", RebuildTerrainProxy)
                .Build();
        }

        // ==========================================
        // TAB 2: TERRAFORMING
        // ==========================================
        private VisualElement BuildTerraformTab()
        {
            return new GraphicalUserInterfaceBuilder("TabTerr")
                .AddChild(new Label("SCENERY & SCULPTING") { style = { fontSize = 16, color = new Color(0.4f, 0.9f, 0.4f), unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 15 } })

                .AddDropdownData("Active Tool", Enum.GetNames(typeof(SceneryBrushType)).ToList(), (int)_activeSceneryBrush, val => _activeSceneryBrush = (SceneryBrushType)Enum.Parse(typeof(SceneryBrushType), val))

                .AddSeparator(new Color(0.2f, 0.2f, 0.2f), 1)

                .AddChild(new Label("BRUSH SETTINGS") { style = { color = Color.white, unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10 } })
                .AddSliderData("Radius (Meters)", 1f, 500f, _brushSize, v => _brushSize = v)
                .AddSliderData("Strength/Opacity", 0f, 1f, _brushStrength, v => _brushStrength = v)

                .AddSeparator(new Color(0.2f, 0.2f, 0.2f), 1)

                .AddButton("Import Heightmap (.raw)", () => Debug.Log("Opening file browser for heightmap..."))
                .Build();
        }

        // ==========================================
        // TAB 3: TRACKS & RAILS
        // ==========================================
        private VisualElement BuildTracksTab()
        {
            return new GraphicalUserInterfaceBuilder("TabTracks")
                .AddChild(new Label("RAILWAY ENGINEERING") { style = { fontSize = 16, color = new Color(0.9f, 0.6f, 0.2f), unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 15 } })

                .AddDropdownData("Laying Paradigm", Enum.GetNames(typeof(TrackLayingMode)).ToList(), (int)_activeTrackMode, val => _activeTrackMode = (TrackLayingMode)Enum.Parse(typeof(TrackLayingMode), val))

                .OnBuild(ve => {
                    var splineHint = new Label("Spline Mode: Click points to generate continuous curve rails matching real-world engineering standards.") { style = { color = Color.gray, fontSize = 10, marginTop = 5, marginBottom = 10, whiteSpace = WhiteSpace.Normal } };
                    ve.Add(splineHint);
                })

                .AddDropdownData("Rail Standard", new List<string> { "Standard Gauge (1,435 mm)", "Narrow Gauge (914 mm)", "Broad Gauge (1,676 mm)" }, 0, null)
                .AddToggleData("Enforce Max Grade (%)", true, v => { })
                .AddSliderData("Max Grade", 1f, 8f, 2.5f, v => { })
                .Build();
        }

        // ==========================================
        // TAB 4: ROLLING STOCK
        // ==========================================
        private VisualElement BuildRollingStockTab()
        {
            return new GraphicalUserInterfaceBuilder("TabLife")
                .AddChild(new Label("SIMULATION & LOGIC") { style = { fontSize = 16, color = new Color(0.8f, 0.4f, 0.8f), unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 15 } })

                .AddDropdownData("Consist Spawner", new List<string> { "Union Pacific SD40-2", "Amtrak Acela", "BNSF Dash 9" }, 0, null)
                .AddButton("Spawn on Active Track", () => Debug.Log("Spawning Consist..."))

                .AddSeparator(Color.gray, 1)

                .AddChild(new Label("AUTOMATION") { style = { color = Color.white, unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10, marginBottom = 5 } })
                .AddButton("Open Signal Block FSM", () => Debug.Log("Opening Signal Logic..."))
                .AddButton("Open Timetable Editor", () => Debug.Log("Opening Timetable..."))
                .Build();
        }

        // ==========================================
        // 3D PROXY PREVIEW & ILLUSION OF SCALE
        // ==========================================

        private void Setup3DPreviewScene()
        {
            _previewRoot = new GameObject("Forge_RailWorks_Root_" + _processGroup) { hideFlags = HideFlags.HideAndDontSave };
            _previewRenderTexture = new RenderTexture(1920, 1080, 24);

            GameObject camObj = new GameObject("RailCamera") { layer = PREVIEW_LAYER };
            camObj.transform.SetParent(_previewRoot.transform);
            _previewCamera = camObj.AddComponent<Camera>();
            _previewCamera.targetTexture = _previewRenderTexture;
            _previewCamera.clearFlags = CameraClearFlags.SolidColor;
            _previewCamera.cullingMask = 1 << PREVIEW_LAYER;

            RebuildTerrainProxy();
        }

        private void RebuildTerrainProxy()
        {
            if (_previewTerrain != null) UnityEngine.Object.DestroyImmediate(_previewTerrain.gameObject);

            // 1. Create the SST 1:1 Terrain
            _previewTerrainData = new TerrainData
            {
                heightmapResolution = 513,
                size = new Vector3(_terrainSSTSize.x, 100f, _terrainSSTSize.y)
            };

            var terrainObj = Terrain.CreateTerrainGameObject(_previewTerrainData);
            terrainObj.transform.SetParent(_previewRoot.transform);
            terrainObj.layer = PREVIEW_LAYER;

            // Center the terrain
            terrainObj.transform.localPosition = new Vector3(-_terrainSSTSize.x / 2f, 0, -_terrainSSTSize.y / 2f);

            _previewTerrain = terrainObj.GetComponent<Terrain>();
            _previewTerrain.drawInstanced = true;

            UpdateProjectionScale();
        }

        private void UpdateProjectionScale()
        {
            if (_previewCamera == null || _previewTerrain == null) return;

            if (_roomContextObj != null) UnityEngine.Object.DestroyImmediate(_roomContextObj);

            float scaleMultiplier = GetScaleMultiplier(_targetModelScale);

            if (_activePerspective == ViewPerspective.Architect_Macro)
            {
                // REAL WORLD VIEW: No table, natural lighting, high altitude camera
                _previewCamera.backgroundColor = new Color(0.4f, 0.6f, 0.9f); // Sky blue
                _cameraDistance = Mathf.Max(_terrainSSTSize.x, _terrainSSTSize.y) * 0.8f;
                _cameraOrbit = new Vector2(45f, 60f); // High angle
            }
            else if (_activePerspective == ViewPerspective.Modeler_Micro)
            {
                // MINIATURE VIEW: The trick is to scale UP the room around the 1:1 terrain.
                // If terrain is 2000m wide, and scale is 1:87 (HO), the room needs to be massive.
                _previewCamera.backgroundColor = new Color(0.05f, 0.05f, 0.08f); // Basement dark

                _roomContextObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _roomContextObj.name = "ScaledTableProxy";
                _roomContextObj.transform.SetParent(_previewRoot.transform);
                _roomContextObj.layer = PREVIEW_LAYER;

                // Scale the table up so the 1:1 terrain looks like it's sitting on a workbench
                float tableWidth = _terrainSSTSize.x * 1.05f;
                float tableDepth = _terrainSSTSize.y * 1.05f;
                float tableHeight = 50f * scaleMultiplier; // Giant table legs

                _roomContextObj.transform.localScale = new Vector3(tableWidth, tableHeight, tableDepth);
                _roomContextObj.transform.position = new Vector3(0, -(tableHeight / 2f), 0);

                // Push camera relatively close to make the terrain feel small
                _cameraDistance = Mathf.Max(_terrainSSTSize.x, _terrainSSTSize.y) * 1.2f;
                _cameraOrbit = new Vector2(45f, 30f); // Lower workbench angle
            }
            else if (_activePerspective == ViewPerspective.FirstPerson_Explorer)
            {
                // FIRST PERSON: Drop to the surface level
                _previewCamera.backgroundColor = new Color(0.4f, 0.6f, 0.9f);
                _cameraDistance = 2f; // Right on the ground
                _cameraOrbit = new Vector2(0f, 5f); // Looking straight ahead
            }

            UpdateCameraPosition();
        }

        private float GetScaleMultiplier(ModelScale scale)
        {
            switch (scale)
            {
                case ModelScale.O_Scale_1to48: return 48f;
                case ModelScale.HO_Scale_1to87: return 87.1f;
                case ModelScale.N_Scale_1to160: return 160f;
                case ModelScale.Z_Scale_1to220: return 220f;
                default: return 1f;
            }
        }

        private void Register3DCameraEvents(Image imgElement)
        {
            imgElement.RegisterCallback<PointerDownEvent>(e => {
                if (e.button == 1)
                {
                    _isDraggingCam = true;
                    _lastMousePos = e.position;
                    imgElement.CapturePointer(e.pointerId);
                }
            });
            imgElement.RegisterCallback<PointerMoveEvent>(e =>
            {
                if (_isDraggingCam)
                {
                    Vector2 delta = (Vector2)e.position - _lastMousePos;

                    if (_activePerspective == ViewPerspective.FirstPerson_Explorer)
                    {
                        // FPS Look
                        _cameraOrbit.x += delta.x * 0.2f;
                        _cameraOrbit.y = Mathf.Clamp(_cameraOrbit.y - delta.y * 0.2f, -80f, 80f);
                    }
                    else
                    {
                        // Orbit Look
                        _cameraOrbit.x += delta.x * 0.5f;
                        _cameraOrbit.y = Mathf.Clamp(_cameraOrbit.y - delta.y * 0.5f, 5f, 85f);
                    }

                    _lastMousePos = e.position;
                    UpdateCameraPosition();
                }
            });
            imgElement.RegisterCallback<PointerUpEvent>(e => {
                if (e.button == 1)
                {
                    _isDraggingCam = false;
                    imgElement.ReleasePointer(e.pointerId);
                }
            });
            imgElement.RegisterCallback<WheelEvent>(e => {
                if (_activePerspective != ViewPerspective.FirstPerson_Explorer)
                {
                    _cameraDistance = Mathf.Clamp(_cameraDistance + e.delta.y * (_cameraDistance * 0.05f), 10f, 5000f);
                    UpdateCameraPosition();
                }
            });
        }

        private void UpdateCameraPosition()
        {
            if (_previewCamera == null) return;

            Quaternion rot = Quaternion.Euler(_cameraOrbit.y, _cameraOrbit.x, 0);

            if (_activePerspective == ViewPerspective.FirstPerson_Explorer)
            {
                // Pivot from the center of the terrain, slightly elevated
                Vector3 fpsPos = new Vector3(0, 2f, 0);
                if (_previewTerrain != null) fpsPos.y = _previewTerrain.SampleHeight(Vector3.zero) + 2f;

                _previewCamera.transform.position = fpsPos;
                _previewCamera.transform.rotation = rot;
            }
            else
            {
                // Standard Orbit
                _previewCamera.transform.position = rot * new Vector3(0, 0, -_cameraDistance);
                _previewCamera.transform.LookAt(Vector3.zero);
            }

            _previewCamera.Render();
        }

        public void Dispose()
        {
            if (_previewCamera != null) _previewCamera.targetTexture = null;
            if (_previewRenderTexture != null) { _previewRenderTexture.Release(); UnityEngine.Object.DestroyImmediate(_previewRenderTexture); }
            if (_previewRoot != null) UnityEngine.Object.DestroyImmediate(_previewRoot);
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}