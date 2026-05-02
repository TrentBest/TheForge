using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.WorldBuilding
{
    public class Workshop_Gui_GlobeEditor : IGuiProvider, IDisposable
    {
        public string Title => "CARTOGRAPHER: MASTER GLOBE EDITOR";

        private string _processGroup;

        // --- CONTEXT & STATE ---
        private float _planetRadius = 1000f; // Dwarf to Gas Giant
        private int _subdivisionLevel = 3;   // Defines "Chunk" size
        private bool _autoSpin = true;
        private float _spinSpeed = 15f;

        // --- 3D PREVIEW RESOURCES ---
        private const int PREVIEW_LAYER = 31;
        private GameObject _previewRoot;
        private RenderTexture _previewRenderTexture;
        private Camera _previewCamera;
        private GameObject _planetObject;
        private MeshCollider _planetCollider;

        // --- HIGHLIGHTING ---
        private GameObject _highlightObject;
        private MeshFilter _highlightMeshFilter;
        private int _lastHoveredTriangle = -1;

        // --- UI ---
        private Image _3dPreviewElement;
        private Label _selectedChunkLabel;

        public Workshop_Gui_GlobeEditor()
        {
            _processGroup = "GlobeEditor_" + Guid.NewGuid().ToString().Substring(0, 6);
            Setup3DPreviewScene();
            RebuildPlanetGeometry();
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var splitPanel = new ForgeSplitPanelBuilder(sidebarWidth: 350)
                .WithSidebar(BuildSidebar())
                .WithMain(BuildMainArea())
                .CreateGui(ctx);

            splitPanel.RegisterCallback<DetachFromPanelEvent>(e => Dispose());

            // FSM/Update loop for auto-spin
            splitPanel.schedule.Execute(() => {
                if (_autoSpin && _planetObject != null)
                {
                    _planetObject.transform.Rotate(Vector3.up, _spinSpeed * Time.deltaTime, Space.World);
                }
            }).Every(16); // ~60fps

            return splitPanel;
        }

        private GraphicalUserInterfaceBuilder BuildSidebar()
        {
            return new GraphicalUserInterfaceBuilder("Sidebar")
                .WithPadding(15).WithBackgroundColor(new Color(0.12f, 0.12f, 0.14f))
                .AddChild(new Label("GLOBE GENERATOR") { style = { color = Color.cyan, fontSize = 16, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 15 } })

                .AddChild(new Label("Planetary Scale:") { style = { color = Color.white } })
                .AddSliderData("Radius (km)", 100f, 10000f, _planetRadius, v => {
                    _planetRadius = v;
                    RebuildPlanetGeometry();
                })

                .AddSeparator()

                .AddChild(new Label("Chunk Resolution:") { style = { color = Color.white } })
                .AddChild(new Label("Higher division = smaller editable map regions.") { style = { color = Color.gray, fontSize = 10 } })
                .AddIntSliderData("Subdivision Level", 1, 6, _subdivisionLevel, v => {
                    _subdivisionLevel = v;
                    RebuildPlanetGeometry();
                })

                .AddSeparator()

                .AddToggleData("Auto-Orbit", _autoSpin, v => _autoSpin = v)
                .AddSliderData("Orbit Speed", 1f, 100f, _spinSpeed, v => _spinSpeed = v)

                .AddSeparator()
                .OnBuild(ve => {
                    _selectedChunkLabel = new Label("Selected Chunk: NONE") { style = { color = Color.yellow, marginTop = 20, unityFontStyleAndWeight = FontStyle.Bold } };
                    ve.Add(_selectedChunkLabel);
                })

                .AddButton("OPEN CHUNK IN MAP EDITOR", () => {
                    if (_lastHoveredTriangle != -1)
                    {
                        Debug.Log($"[Forge] Opening MapBuilder for Chunk ID: {_lastHoveredTriangle} at SubDiv {_subdivisionLevel}");
                        // TODO: Trigger FSM transition to load the Workshop_Gui_MapBuilder 
                        // and pass the Triangle Index / Centroid to it!
                    }
                    else
                    {
                        Debug.LogWarning("No chunk selected!");
                    }
                });
        }

        private GraphicalUserInterfaceBuilder BuildMainArea()
        {
            return new GraphicalUserInterfaceBuilder("MainArea")
                .WithBackgroundColor(Color.black)
                .WithFlexGrow(1).WithFlexShrink(1)
                .OnBuild(ve =>
                {
                    _3dPreviewElement = new Image { image = _previewRenderTexture, scaleMode = ScaleMode.ScaleAndCrop, style = { flexGrow = 1 } };
                    RegisterPointerEvents(_3dPreviewElement);
                    ve.Add(_3dPreviewElement);
                });
        }

        // ==========================================
        // 3D SCENE & RAYCASTING LOGIC
        // ==========================================

        private void Setup3DPreviewScene()
        {
            _previewRoot = new GameObject("Forge_GlobeEditor_Root_" + _processGroup) { hideFlags = HideFlags.HideAndDontSave };
            _previewRenderTexture = new RenderTexture(1920, 1080, 24);

            GameObject camObj = new GameObject("GlobeEditorCamera") { layer = PREVIEW_LAYER };
            camObj.transform.SetParent(_previewRoot.transform);
            _previewCamera = camObj.AddComponent<Camera>();
            _previewCamera.targetTexture = _previewRenderTexture;
            _previewCamera.clearFlags = CameraClearFlags.SolidColor;
            _previewCamera.backgroundColor = new Color(0.02f, 0.02f, 0.05f); // Deep space background
            _previewCamera.cullingMask = 1 << PREVIEW_LAYER;

            // Add a directional light
            GameObject lightObj = new GameObject("Sunlight") { layer = PREVIEW_LAYER };
            lightObj.transform.SetParent(_previewRoot.transform);
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private void RebuildPlanetGeometry()
        {
            if (_planetObject != null) UnityEngine.Object.DestroyImmediate(_planetObject);

            _planetObject = new GameObject("MasterPlanet");
            _planetObject.transform.SetParent(_previewRoot.transform, false);
            _planetObject.layer = PREVIEW_LAYER;

            Mesh rawMesh = IcosphereGenerator.Create(_subdivisionLevel, _planetRadius);

            // We must Flat Shade it so each triangle chunk is discrete
            Mesh flatMesh = ApplyFlatShading(rawMesh);

            var mf = _planetObject.AddComponent<MeshFilter>();
            mf.sharedMesh = flatMesh;

            var mr = _planetObject.AddComponent<MeshRenderer>();
            Material planetMat = new Material(Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit"));
            planetMat.color = new Color(0.2f, 0.2f, 0.2f); // Base crust color
            mr.sharedMaterial = planetMat;

            // CRITICAL: We need a mesh collider to raycast against the chunks
            _planetCollider = _planetObject.AddComponent<MeshCollider>();
            _planetCollider.sharedMesh = flatMesh;

            // Pull camera back to fit the planet
            _previewCamera.transform.position = new Vector3(0, 0, -(_planetRadius * 2.5f));
            _previewCamera.transform.LookAt(Vector3.zero);

            CreateHighlightObject();
        }

        private void CreateHighlightObject()
        {
            if (_highlightObject != null) UnityEngine.Object.DestroyImmediate(_highlightObject);

            _highlightObject = new GameObject("ChunkHighlight");
            _highlightObject.transform.SetParent(_planetObject.transform, false);
            _highlightObject.layer = PREVIEW_LAYER;

            _highlightMeshFilter = _highlightObject.AddComponent<MeshFilter>();
            var mr = _highlightObject.AddComponent<MeshRenderer>();

            Material highlightMat = new Material(Shader.Find("Unlit/Color") ?? Shader.Find("Sprites/Default"));
            highlightMat.color = new Color(0.6f, 0.1f, 0.9f, 0.5f); // Deep purple highlight
            // Ensure it renders slightly on top of the base mesh
            highlightMat.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);

            mr.sharedMaterial = highlightMat;
            _highlightObject.SetActive(false);
        }

        // ==========================================
        // UI TO 3D TRANSLATION
        // ==========================================

        private void RegisterPointerEvents(Image imgElement)
        {
            imgElement.RegisterCallback<PointerMoveEvent>(e => {
                if (_previewCamera == null || _planetCollider == null) return;

                // 1. Convert UI Local Mouse Position to a Normalized Viewport Coordinate (0.0 to 1.0)
                float xPct = e.localPosition.x / imgElement.layout.width;
                float yPct = 1.0f - (e.localPosition.y / imgElement.layout.height); // UI Y is inverted from Viewport Y

                // 2. Fire a Ray from the isolated camera
                Ray ray = _previewCamera.ViewportPointToRay(new Vector3(xPct, yPct, 0));

                if (_planetCollider.Raycast(ray, out RaycastHit hit, _planetRadius * 5f))
                {
                    if (hit.triangleIndex != _lastHoveredTriangle)
                    {
                        _lastHoveredTriangle = hit.triangleIndex;
                        HighlightChunk(hit.triangleIndex);
                    }
                }
                else
                {
                    ClearHighlight();
                }
            });

            // Allow manual drag rotation if user clicks and drags
            bool isDragging = false;
            Vector2 lastMouse = Vector2.zero;

            imgElement.RegisterCallback<PointerDownEvent>(e => {
                if (e.button == 1 || e.button == 0) { isDragging = true; lastMouse = e.position; imgElement.CapturePointer(e.pointerId); _autoSpin = false; }
            });

            imgElement.RegisterCallback<PointerMoveEvent>(e => {
                if (isDragging && _planetObject != null)
                {
                    Vector2 delta = (Vector2)e.position - lastMouse;
                    _planetObject.transform.Rotate(Vector3.up, -delta.x * 0.5f, Space.World);
                    _planetObject.transform.Rotate(Vector3.right, delta.y * 0.5f, Space.World);
                    lastMouse = e.position;
                }
            });

            imgElement.RegisterCallback<PointerUpEvent>(e => { isDragging = false; imgElement.ReleasePointer(e.pointerId); });
        }

        private void HighlightChunk(int triangleIndex)
        {
            _highlightObject.SetActive(true);
            _selectedChunkLabel.text = $"Selected Chunk: {triangleIndex}";

            Mesh baseMesh = _planetCollider.sharedMesh;
            Vector3[] baseVerts = baseMesh.vertices;
            int[] baseTris = baseMesh.triangles;

            // Extract the 3 vertices of the specific triangle the raycast hit
            int v1 = baseTris[triangleIndex * 3];
            int v2 = baseTris[triangleIndex * 3 + 1];
            int v3 = baseTris[triangleIndex * 3 + 2];

            // Build a mini-mesh just for the highlight
            Mesh highlightMesh = new Mesh();
            // Push the vertices out along their normals slightly (1.005x) to prevent Z-fighting
            highlightMesh.vertices = new Vector3[] {
                baseVerts[v1] * 1.005f,
                baseVerts[v2] * 1.005f,
                baseVerts[v3] * 1.005f
            };
            highlightMesh.triangles = new int[] { 0, 1, 2 };
            highlightMesh.RecalculateNormals();

            _highlightMeshFilter.mesh = highlightMesh;
        }

        private void ClearHighlight()
        {
            if (_lastHoveredTriangle != -1)
            {
                _lastHoveredTriangle = -1;
                _highlightObject.SetActive(false);
                if (_selectedChunkLabel != null) _selectedChunkLabel.text = "Selected Chunk: NONE";
            }
        }

        private Mesh ApplyFlatShading(Mesh smoothMesh)
        {
            Vector3[] oldVerts = smoothMesh.vertices;
            int[] triangles = smoothMesh.triangles;
            Vector3[] flatVerts = new Vector3[triangles.Length];
            int[] flatTriangles = new int[triangles.Length];

            for (int i = 0; i < triangles.Length; i++)
            {
                flatVerts[i] = oldVerts[triangles[i]];
                flatTriangles[i] = i;
            }

            Mesh flatMesh = new Mesh();
            flatMesh.vertices = flatVerts;
            flatMesh.triangles = flatTriangles;
            flatMesh.RecalculateNormals();
            return flatMesh;
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