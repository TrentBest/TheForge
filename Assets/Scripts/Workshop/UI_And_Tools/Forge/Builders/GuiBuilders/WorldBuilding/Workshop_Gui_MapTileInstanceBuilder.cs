using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

// --- THE FIX: Wrap the Editor namespace ---
#if UNITY_EDITOR
using UnityEditor.UIElements; // For ObjectField in Editor
#endif

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.WorldBuilding
{
    public class Workshop_Gui_MapTileInstanceBuilder : IGuiProvider, IDisposable
    {
        public string Title => "Cartographer: Map Tile Instance (Construction)";

        private string _processGroup;

        // Construction State
        private GameObject _activeConstructPrefab;
        private GameObject _ghostConstruct;
        private List<GameObject> _placedConstructs = new List<GameObject>();
        private float _constructRotation = 0f;
        private bool _snapToGrid = false;
        private float _gridSize = 2f;

        // 3D Preview (Isolated)
        private const int PREVIEW_LAYER = 31;
        private GameObject _previewRoot;
        private RenderTexture _previewRenderTexture;
        private Camera _previewCamera;
        private Terrain _previewTerrain;
        private GameObject _previewTerrainObj;

        // Camera State
        private Vector2 _cameraOrbit = new Vector2(45f, 45f);
        private float _cameraDistance = 150f;

        public Workshop_Gui_MapTileInstanceBuilder()
        {
            _processGroup = "TileInstance_" + Guid.NewGuid().ToString().Substring(0, 6);
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
                    if (go.name.StartsWith("Forge_Instance_Root_") || go.name.StartsWith("InstanceCamera_"))
                    {
                        UnityEngine.Object.DestroyImmediate(go);
                    }
                }
            }
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new ForgeSplitPanelBuilder(sidebarWidth: 400)
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

            sidebar.AddChild(new Label("CONSTRUCTION MANIFEST") { style = { fontSize = 18, color = new Color(0.8f, 0.5f, 0.2f), unityFontStyleAndWeight = FontStyle.Bold } });

            sidebar.AddSeparator(Color.gray, 1);
            sidebar.AddChild(new Label("1. Select Blueprint") { style = { color = Color.cyan, marginTop = 10, marginBottom = 5 } });

#if UNITY_EDITOR
            sidebar.OnBuild(ve => {
                var objField = new ObjectField("Blueprint (Prefab)") { objectType = typeof(GameObject), allowSceneObjects = false };
                objField.RegisterValueChangedCallback(evt => {
                    _activeConstructPrefab = evt.newValue as GameObject;
                    UpdateGhostConstruct();
                });
                ve.Add(objField);
            });
#else
            sidebar.AddChild(new Label("[Runtime Prefab Loader Goes Here]") { style = { color = Color.gray }});
#endif

            sidebar.AddSeparator(Color.gray, 1);
            sidebar.AddChild(new Label("2. Placement Settings") { style = { color = Color.cyan, marginTop = 10 } });

            sidebar.AddSliderData("Rotation (Yaw)", 0f, 360f, _constructRotation, v => {
                _constructRotation = v;
                if (_ghostConstruct != null) _ghostConstruct.transform.rotation = Quaternion.Euler(0, _constructRotation, 0);
                if (_previewCamera != null) _previewCamera.Render();
            });

            sidebar.AddToggleData("Snap to Grid", _snapToGrid, v => _snapToGrid = v);
            sidebar.AddSliderData("Grid Size", 0.5f, 10f, _gridSize, v => _gridSize = v);

            sidebar.AddSeparator(Color.gray, 2);
            sidebar.AddButton("Clear All Constructs", ClearPlacedConstructs);
            sidebar.AddButton("SAVE INSTANCE", () => Debug.Log("[Forge] Tile Instance Saved with " + _placedConstructs.Count + " structures."));

            return sidebar;
        }

        private GraphicalUserInterfaceBuilder BuildMainArea()
        {
            var mainArea = new GraphicalUserInterfaceBuilder("MainArea")
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Stretch);

            Image _3dRenderPreview = new Image { image = _previewRenderTexture, scaleMode = ScaleMode.ScaleAndCrop, style = { flexGrow = 1, backgroundColor = Color.black } };
            Register3DInteractionEvents(_3dRenderPreview);
            mainArea.AddChild(_3dRenderPreview);

            return mainArea;
        }

        // ==========================================
        // 3D PREVIEW & RAYCASTING
        // ==========================================

        private void Setup3DPreviewScene()
        {
            _previewRoot = new GameObject("Forge_Instance_Root_" + _processGroup);
            _previewRoot.hideFlags = HideFlags.HideAndDontSave;

            _previewRenderTexture = new RenderTexture(1024, 1024, 24);

            GameObject camObj = new GameObject("InstanceCamera_" + _processGroup);
            camObj.transform.SetParent(_previewRoot.transform);
            camObj.layer = PREVIEW_LAYER;
            _previewCamera = camObj.AddComponent<Camera>();
            _previewCamera.targetTexture = _previewRenderTexture;
            _previewCamera.clearFlags = CameraClearFlags.SolidColor;
            _previewCamera.backgroundColor = new Color(0.15f, 0.2f, 0.25f);
            _previewCamera.cullingMask = 1 << PREVIEW_LAYER;

            GameObject lightObj = new GameObject("InstanceLight");
            lightObj.transform.SetParent(_previewRoot.transform);
            lightObj.layer = PREVIEW_LAYER;
            Light l = lightObj.AddComponent<Light>();
            l.type = LightType.Directional;
            l.cullingMask = 1 << PREVIEW_LAYER;
            l.intensity = 1.2f;
            lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);

            TerrainData td = new TerrainData { heightmapResolution = 129, alphamapResolution = 128, size = new Vector3(128, 20f, 128) };
            td.terrainLayers = new TerrainLayer[] { new TerrainLayer { diffuseTexture = Texture2D.whiteTexture } };

            _previewTerrainObj = Terrain.CreateTerrainGameObject(td);
            _previewTerrainObj.transform.SetParent(_previewRoot.transform);
            _previewTerrainObj.layer = PREVIEW_LAYER;

            TerrainCollider tc = _previewTerrainObj.GetComponent<TerrainCollider>();
            if (tc == null) tc = _previewTerrainObj.AddComponent<TerrainCollider>();
            tc.terrainData = td;

            _previewTerrainObj.transform.localPosition = new Vector3(-64f, 0, -64f);

            UpdateCameraPosition();
        }

        // ==========================================
        // CONSTRUCTION LOGIC
        // ==========================================

        private void UpdateGhostConstruct()
        {
            if (_ghostConstruct != null) UnityEngine.Object.DestroyImmediate(_ghostConstruct);
            if (_activeConstructPrefab == null) return;

            _ghostConstruct = UnityEngine.Object.Instantiate(_activeConstructPrefab);
            _ghostConstruct.transform.SetParent(_previewRoot.transform);
            SetLayerRecursively(_ghostConstruct.transform, PREVIEW_LAYER);

            foreach (var renderer in _ghostConstruct.GetComponentsInChildren<Renderer>())
            {
                if (renderer.material.HasProperty("_Color"))
                {
                    Color c = renderer.material.color;
                    renderer.material.color = new Color(c.r, c.g, c.b, 0.5f);
                }
            }
            if (_previewCamera != null) _previewCamera.Render();
        }

        private void Register3DInteractionEvents(Image imgElement)
        {
            bool isDraggingCam = false;
            Vector2 lastMouse = Vector2.zero;

            imgElement.RegisterCallback<PointerDownEvent>(e => {
                if (e.button == 1)
                {
                    isDraggingCam = true;
                    lastMouse = e.position;
                    imgElement.CapturePointer(e.pointerId);
                }
                else if (e.button == 0)
                {
                    PlaceConstructAtGhost();
                }
            });

            imgElement.RegisterCallback<PointerMoveEvent>(e => {
                if (isDraggingCam)
                {
                    Vector2 delta = (Vector2)e.position - lastMouse;
                    _cameraOrbit.x += delta.x * 0.4f;
                    _cameraOrbit.y = Mathf.Clamp(_cameraOrbit.y - delta.y * 0.4f, 10f, 85f);
                    lastMouse = e.position;
                    UpdateCameraPosition();
                }
                else
                {
                    HandleRaycastPlacement(e.localPosition, imgElement);
                }
            });

            imgElement.RegisterCallback<PointerUpEvent>(e => {
                if (isDraggingCam && e.button == 1)
                {
                    isDraggingCam = false;
                    imgElement.ReleasePointer(e.pointerId);
                }
            });

            imgElement.RegisterCallback<WheelEvent>(e => {
                _cameraDistance = Mathf.Clamp(_cameraDistance + e.delta.y * 2f, 20f, 300f);
                UpdateCameraPosition();
            });
        }

        private void HandleRaycastPlacement(Vector2 localPos, Image img)
        {
            if (_ghostConstruct == null || _previewCamera == null) return;

            float xPct = localPos.x / img.layout.width;
            float yPct = 1.0f - (localPos.y / img.layout.height);
            Ray ray = _previewCamera.ViewportPointToRay(new Vector3(xPct, yPct, 0));

            if (UnityEngine.Physics.Raycast(ray, out RaycastHit hit, 1000f, 1 << PREVIEW_LAYER))
            {
                Vector3 targetPos = hit.point;

                if (_snapToGrid)
                {
                    targetPos.x = Mathf.Round(targetPos.x / _gridSize) * _gridSize;
                    targetPos.z = Mathf.Round(targetPos.z / _gridSize) * _gridSize;
                    if (_previewTerrainObj != null && _previewTerrainObj.GetComponent<Terrain>() != null)
                    {
                        targetPos.y = _previewTerrainObj.GetComponent<Terrain>().SampleHeight(targetPos) + _previewTerrainObj.transform.position.y;
                    }
                }

                _ghostConstruct.transform.position = targetPos;
                _ghostConstruct.transform.rotation = Quaternion.Euler(0, _constructRotation, 0);
            }

            _previewCamera.Render();
        }

        private void PlaceConstructAtGhost()
        {
            if (_ghostConstruct == null || _activeConstructPrefab == null) return;

            GameObject newConstruct = UnityEngine.Object.Instantiate(_activeConstructPrefab, _ghostConstruct.transform.position, _ghostConstruct.transform.rotation);
            newConstruct.transform.SetParent(_previewRoot.transform);
            SetLayerRecursively(newConstruct.transform, PREVIEW_LAYER);

            _placedConstructs.Add(newConstruct);
            Debug.Log($"[Forge] Placed '{_activeConstructPrefab.name}' at {newConstruct.transform.position}");

            _previewCamera.Render();
        }

        private void ClearPlacedConstructs()
        {
            foreach (var c in _placedConstructs)
            {
                if (c != null) UnityEngine.Object.DestroyImmediate(c);
            }
            _placedConstructs.Clear();
            if (_previewCamera != null) _previewCamera.Render();
        }

        private void UpdateCameraPosition()
        {
            if (_previewCamera == null) return;
            Quaternion rot = Quaternion.Euler(_cameraOrbit.y, _cameraOrbit.x, 0);
            _previewCamera.transform.position = rot * new Vector3(0, 0, -_cameraDistance) + Vector3.zero;
            _previewCamera.transform.LookAt(Vector3.zero);
            _previewCamera.Render();
        }

        private void SetLayerRecursively(Transform trans, int layer)
        {
            trans.gameObject.layer = layer;
            foreach (Transform child in trans) SetLayerRecursively(child, layer);
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