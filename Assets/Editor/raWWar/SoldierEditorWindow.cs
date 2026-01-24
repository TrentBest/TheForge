using Assets.Scripts.Builders.GuiBuilders;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Editor.raWWar
{
    public class SoldierEditorWindow : EditorWindow
    {
        // --- Data Model ---
        [Serializable]
        private class SoldierData
        {
            public string Name = "Pvt. Jane Doe";
            public GameObject VisualPrefab;
        }
        private SoldierData _soldier = new SoldierData();

        // --- UI State ---
        private VisualElement _contentArea;
        private ListView _hierarchyList;
        private List<Transform> _flatHierarchy = new List<Transform>();
        private Transform _selectedPart;
        private Transform _hoverPart; // The part currently under the laser
        private bool _isInternalSelection;

        // --- Preview & Tool State ---
        private PreviewRenderUtility _previewUtility;
        private GameObject _previewInstance;
        private Vector2 _orbitAngles = new Vector2(20, 0);
        private float _zoomDistance = 4.0f;
        private Vector3 _cameraPivot = Vector3.up;
        private Rect _previewRect;

        // Laser / Highlight Tools
        private bool _laserMode = true; // Default to on so you can see it
        private Material _highlightMaterial;
        private Dictionary<Renderer, Material[]> _originalMaterials = new Dictionary<Renderer, Material[]>();

        [MenuItem("raWWar/Soldier Editor (Laser Tech)")]
        public static void ShowWindow()
        {
            var wnd = GetWindow<SoldierEditorWindow>();
            wnd.titleContent = new GUIContent("Soldier Editor");
            wnd.minSize = new Vector2(1000, 600);
            wnd.wantsMouseMove = true; // CRITICAL: Allows us to track the mouse without clicking
        }

        private void OnEnable()
        {
            if (_previewUtility == null)
            {
                _previewUtility = new PreviewRenderUtility();
                _previewUtility.camera.fieldOfView = 30f;
                _previewUtility.camera.nearClipPlane = 0.01f;
                _previewUtility.camera.farClipPlane = 100f;
                _previewUtility.camera.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 1f);
            }

            // Create a "Hologram" material for selection
            var shader = Shader.Find("Unlit/Color");
            _highlightMaterial = new Material(shader) { color = new Color(0f, 1f, 0f, 0.5f) };
        }

        private void OnDisable()
        {
            if (_previewUtility != null) _previewUtility.Cleanup();
            if (_previewInstance != null) DestroyImmediate(_previewInstance);
            if (_highlightMaterial != null) DestroyImmediate(_highlightMaterial);
        }

        public void CreateGUI()
        {
            var root = new GraphicalUserInterfaceBuilder("CommandCenter")
                .WithEditorMode(true)
                .WithAutoGrow()
                .WithPadding(0)
                .WithBackgroundColor(new Color(0.18f, 0.18f, 0.18f));

            root.AddChild(CreateHeader());
            root.AddChild(BuildEditorLayout());

            rootVisualElement.Add(root.Build());

            // Force initial refresh
            RefreshPreviewInstance();
        }

        private VisualElement CreateHeader()
        {
            return new GraphicalUserInterfaceBuilder("Header")
                .WithBackgroundColor(Color.black)
                .WithPadding(10)
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                .AddChild(ctx => new Label("SOLDIER EDITOR") { style = { unityFontStyleAndWeight = FontStyle.Bold, color = Color.white } })
                .AddChild(ctx => {
                    var t = new Toggle("Activate Laser Pointer") { value = _laserMode };
                    t.RegisterValueChangedCallback(e => _laserMode = e.newValue);
                    return t;
                })
                .Build();
        }

        private VisualElement BuildEditorLayout()
        {
            var split = new GraphicalUserInterfaceBuilder("Split")
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                .WithAutoGrow();

            // LEFT: 3D Viewport
            var left = split.WithPanel("ViewPort").WithPercentSize(70, 100);

            left.AddChild(ctx => {
                var overlay = new VisualElement { style = { position = Position.Absolute, top = 10, left = 10, right = 10, flexDirection = FlexDirection.Row } };
                var field = new ObjectField { objectType = typeof(GameObject), value = _soldier.VisualPrefab, style = { flexGrow = 1 } };
                field.RegisterValueChangedCallback(e => { _soldier.VisualPrefab = (GameObject)e.newValue; RefreshPreviewInstance(); });
                overlay.Add(field);
                return overlay;
            });

            left.AddChild(ctx => {
                var imgui = new IMGUIContainer(OnPreviewGUI);
                imgui.style.flexGrow = 1;
                return imgui;
            });

            // RIGHT: Hierarchy
            var right = split.WithPanel("Inspector").WithPercentSize(30, 100).WithBackgroundColor(new Color(0.22f, 0.22f, 0.22f));

            right.AddChild(ctx => {
                _hierarchyList = new ListView();
                _hierarchyList.style.flexGrow = 1;
                _hierarchyList.makeItem = () => new Label();
                _hierarchyList.bindItem = (el, i) => {
                    if (i >= _flatHierarchy.Count) return;
                    var t = _flatHierarchy[i];
                    int d = 0; var p = t.parent;
                    while (p != null && p != _previewInstance.transform.parent) { d++; p = p.parent; }
                    (el as Label).text = new string(' ', d * 4) + t.name;
                    // Color Logic: Yellow if Selected, Cyan if Hovered
                    if (t == _selectedPart) (el as Label).style.color = Color.yellow;
                    else if (t == _hoverPart) (el as Label).style.color = Color.cyan;
                    else (el as Label).style.color = Color.white;
                };

                _hierarchyList.selectionChanged += (o) => {
                    if (_isInternalSelection) return;
                    if (_hierarchyList.selectedIndex >= 0 && _hierarchyList.selectedIndex < _flatHierarchy.Count)
                    {
                        var t = _flatHierarchy[_hierarchyList.selectedIndex];
                        _selectedPart = t;
                        SelectPart(_selectedPart); // Persistent highlight
                    }
                };
                return _hierarchyList;
            });

            return split.Build();
        }

       

        // -----------------------------------------------------------------------------------
        // PREVIEW LOOP (The Heart of the Tool)
        // -----------------------------------------------------------------------------------
        private void OnPreviewGUI()
        {
            if (_previewUtility == null) return;

            _previewRect = GUILayoutUtility.GetRect(GUILayoutUtility.GetLastRect().width, GUILayoutUtility.GetLastRect().height, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            if (Event.current.type == EventType.Repaint)
            {
                _previewUtility.BeginPreview(_previewRect, GUIStyle.none);

                // 1. Sync Camera
                var rot = Quaternion.Euler(_orbitAngles.x, _orbitAngles.y, 0);
                var pos = _cameraPivot + (rot * new Vector3(0, 0, -_zoomDistance));
                _previewUtility.camera.transform.SetPositionAndRotation(pos, Quaternion.LookRotation(_cameraPivot - pos));

                // 2. Render Scene
                DrawGrid();
                if (_previewInstance != null)
                {
                    _previewInstance.transform.rotation = Quaternion.identity;
                    _previewUtility.Render(true);
                }

                _previewUtility.camera.Render();
                var tex = _previewUtility.EndPreview();
                GUI.DrawTexture(_previewRect, tex, ScaleMode.StretchToFill, false);
            }

            // 3. Handle Inputs (Mouse Move for Laser, Click for Select)
            HandleCameraInput(Event.current);
            HandleLaserAndSelection(Event.current);
        }

        private void HandleLaserAndSelection(Event evt)
        {
            // Only process if mouse is in the view
            if (!_previewRect.Contains(evt.mousePosition)) return;

            // 1. Calculate Ray
            // Editor GUI (Top-Left 0,0) -> Camera Viewport (Bottom-Left 0,0)
            float relX = evt.mousePosition.x - _previewRect.x;
            float relY = evt.mousePosition.y - _previewRect.y;
            float u = relX / _previewRect.width;
            float v = 1.0f - (relY / _previewRect.height);

            // Sync camera for math (since we are outside Repaint)
            var rot = Quaternion.Euler(_orbitAngles.x, _orbitAngles.y, 0);
            var pos = _cameraPivot + (rot * new Vector3(0, 0, -_zoomDistance));
            _previewUtility.camera.transform.SetPositionAndRotation(pos, Quaternion.LookRotation(_cameraPivot - pos));

            Ray ray = _previewUtility.camera.ViewportPointToRay(new Vector3(u, v, 0));

            // 2. Raycast (Using Physics because we added MeshColliders!)
            RaycastHit hit;
            bool didHit = Physics.Raycast(ray, out hit, 100f);

            // 3. Visual Feedback (The Laser)
            if (_laserMode)
            {
                Handles.SetCamera(_previewUtility.camera);

                // Draw line from camera to hit point (or far away)
                Vector3 endPoint = didHit ? hit.point : ray.GetPoint(10f);

                // Draw the Laser Beam
                Handles.color = didHit ? Color.green : new Color(1, 0, 0, 0.2f);
                Handles.DrawLine(_previewUtility.camera.transform.position - (Vector3.up * 0.2f), endPoint);

                if (didHit)
                {
                    // Draw Hit Point
                    Handles.color = Color.green;
                    Handles.SphereHandleCap(0, hit.point, Quaternion.identity, 0.05f, EventType.Repaint);

                    // Draw Normal Vector (Crucial for Socket Placement!)
                    Handles.color = Color.yellow;
                    Handles.DrawLine(hit.point, hit.point + hit.normal * 0.3f);
                }

                // Force repaint to animate the laser moving
                if (evt.type == EventType.MouseMove || evt.type == EventType.MouseDrag) Repaint();
            }

            // 4. Hover Logic
            if (didHit && hit.collider != null)
            {
                Transform target = hit.collider.transform;
                if (_hoverPart != target)
                {
                    _hoverPart = target;
                    // Trigger hierarchy refresh to show cyan color
                    if (_hierarchyList != null) _hierarchyList.RefreshItems();
                }
            }
            else
            {
                if (_hoverPart != null)
                {
                    _hoverPart = null;
                    if (_hierarchyList != null) _hierarchyList.RefreshItems();
                }
            }

            // 5. Selection Logic (Click)
            if (evt.type == EventType.MouseDown && evt.button == 0)
            {
                if (didHit)
                {
                    SelectPart(hit.collider.transform);
                    evt.Use();
                }
                else
                {
                    SelectPart(null);
                }
            }
        }

        private void SelectPart(Transform t)
        {
            // Restore old material
            if (_selectedPart != null) RestoreMaterial(_selectedPart);

            _selectedPart = t;

            // Highlight new material (Visual)
            if (_selectedPart != null) ApplyHighlightMaterial(_selectedPart);

            // Sync List
            if (_selectedPart != null && _hierarchyList != null)
            {
                _isInternalSelection = true;
                int idx = _flatHierarchy.IndexOf(_selectedPart);
                if (idx != -1)
                {
                    _hierarchyList.SetSelection(idx);
                    _hierarchyList.ScrollToItem(idx);
                }
                _isInternalSelection = false;
            }
            Repaint();
        }

        // --- Material Swapping Helpers ---
        private void ApplyHighlightMaterial(Transform t)
        {
            var r = t.GetComponent<Renderer>();
            if (r == null) return;

            // Cache original materials if not already cached
            if (!_originalMaterials.ContainsKey(r)) _originalMaterials[r] = r.sharedMaterials;

            // Replace with highlight
            Material[] highlights = new Material[r.sharedMaterials.Length];
            for (int i = 0; i < highlights.Length; i++) highlights[i] = _highlightMaterial;
            r.sharedMaterials = highlights;
        }

        private void RestoreMaterial(Transform t)
        {
            var r = t.GetComponent<Renderer>();
            if (r == null) return;
            if (_originalMaterials.ContainsKey(r))
            {
                r.sharedMaterials = _originalMaterials[r];
                _originalMaterials.Remove(r);
            }
        }

        // --- Standard Helpers ---
        private void RefreshPreviewInstance()
        {
            if (_previewInstance) DestroyImmediate(_previewInstance);
            _originalMaterials.Clear();
            _selectedPart = null;

            if (_soldier.VisualPrefab) _previewInstance = Instantiate(_soldier.VisualPrefab);
            else { _previewInstance = GameObject.CreatePrimitive(PrimitiveType.Capsule); }

            _previewInstance.hideFlags = HideFlags.HideAndDontSave;

            // AUTOMATIC COLLIDER GENERATION
            // We iterate every renderer and attach a MeshCollider so our Raycast works perfectly
            foreach (var r in _previewInstance.GetComponentsInChildren<MeshRenderer>())
            {
                if (r.GetComponent<Collider>() == null) r.gameObject.AddComponent<MeshCollider>();
            }
            // SkinnedMeshRenderers (Characters) are trickier, usually need a CapsuleCollider or BoxCollider approximation
            foreach (var smr in _previewInstance.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if (smr.GetComponent<Collider>() == null)
                {
                    // Approximate with BoxCollider for now, as MeshCollider on SkinnedMesh is expensive/complex in editor
                    var box = smr.gameObject.AddComponent<BoxCollider>();
                    box.center = smr.bounds.center - smr.transform.position;
                    box.size = smr.bounds.size;
                }
            }

            _previewUtility.AddSingleGO(_previewInstance);

            // Calculate Center
            var bounds = new Bounds(_previewInstance.transform.position, Vector3.zero);
            foreach (var r in _previewInstance.GetComponentsInChildren<Renderer>()) bounds.Encapsulate(r.bounds);
            _cameraPivot = bounds.center;

            RefreshHierarchyData();
        }

        private void RefreshHierarchyData()
        {
            _flatHierarchy.Clear();
            if (_previewInstance) AddRec(_previewInstance.transform);
            if (_hierarchyList != null) { _hierarchyList.itemsSource = _flatHierarchy; _hierarchyList.Rebuild(); }
        }
        private void AddRec(Transform t) { _flatHierarchy.Add(t); foreach (Transform c in t) AddRec(c); }

        private void HandleCameraInput(Event evt)
        {
            if (_previewRect.Contains(evt.mousePosition))
            {
                if (evt.type == EventType.ScrollWheel)
                {
                    _zoomDistance = Mathf.Clamp(_zoomDistance + evt.delta.y * 0.1f, 1f, 20f); evt.Use();
                }
                else if (evt.type == EventType.MouseDrag && (evt.button == 1 || evt.button == 2))
                {
                    _orbitAngles.y += evt.delta.x; _orbitAngles.x = Mathf.Clamp(_orbitAngles.x + evt.delta.y, -89, 89); evt.Use();
                }
            }
        }

        private void DrawGrid()
        {
            Handles.color = new Color(1, 1, 1, 0.1f);
            for (int i = -5; i <= 5; i++)
            {
                Handles.DrawLine(new Vector3(i, 0, -5), new Vector3(i, 0, 5));
                Handles.DrawLine(new Vector3(-5, 0, i), new Vector3(5, 0, i));
            }
        }
    }
}