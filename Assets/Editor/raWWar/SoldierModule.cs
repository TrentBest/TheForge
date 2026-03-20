using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using System.Collections.Generic;
using TheSingularityWorkshop.Builders.GuiBuilders;
using System.Linq;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders; // Needed for sorting raycasts

namespace Assets.Editor.raWWar
{
    public class SoldierModule : IEditorModule
    {
        public string ModuleName => "Soldiers";

        // --- Visual Settings ---
        public Color FactionPrimaryColor = new Color(0.2f, 0.4f, 0.8f);
        public Color FactionSecondaryColor = Color.white;

        // --- Data ---
        private GameObject _visualPrefab;
        private List<Transform> _flatHierarchy = new List<Transform>();
        private ListView _hierarchyList;

        // --- Preview State ---
        private PreviewRenderUtility _previewUtility;
        private GameObject _previewInstance;

        // Camera Defaults: Pitch 20, Yaw 180 (Usually faces front for Z-forward models)
        private Vector2 _orbitAngles = new Vector2(20, 180);
        private float _zoomDistance = 2.5f;
        private Vector3 _cameraPivot = Vector3.up; // Aim at chest height approx

        // Interaction State
        private Vector3? _lastHitPoint;
        private string _lastHitName;

        public SoldierModule()
        {
            _visualPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Soldier.prefab");
        }

        public VisualElement CreateGui()
        {
            if (_previewUtility == null) InitializePreview();

            var builder = new GraphicalUserInterfaceBuilder("Soldiers")
                .WithPercentSize(100, 100)
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                .WithPadding(5)
                .WithEditorMode(true);

            // ------------------------------------------------------------
            // TOP PANEL (Identity & Preview)
            // ------------------------------------------------------------
            var topSuperPanel = new GraphicalUserInterfaceBuilder("TopSuperPanel")
                .WithPercentSize(100, 60) // Gave more room to 3D view
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch);

            // Left: Identity Controls
            var topLeftPanel = new GraphicalUserInterfaceBuilder("LeftPanel")
                .WithPercentSize(30, 100)
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                .WithPadding(10)
                .WithBackgroundColor(new Color(0.2f, 0.2f, 0.2f));

            topLeftPanel.AddChild(new Label("UNIT IDENTITY") { style = { unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 5 } });

            topLeftPanel.AddChild(ctx =>
            {
                var field = new ObjectField("Visual Model")
                {
                    objectType = typeof(GameObject),
                    allowSceneObjects = false,
                    value = _visualPrefab
                };
                field.RegisterValueChangedCallback(evt => { _visualPrefab = evt.newValue as GameObject; RefreshPreview(); });
                return field;
            });

            topLeftPanel.AddStringData("Class Name", "Heavy Infantry", null);
            topLeftPanel.AddChild(ctx => new ColorField("Primary Color") { value = FactionPrimaryColor });
            topLeftPanel.AddChild(ctx => new ColorField("Secondary Color") { value = FactionSecondaryColor });

            // Interaction Feedback Label
            topLeftPanel.AddChild(ctx =>
            {
                var l = new Label("Interact with model to place sockets..")
                {
                    style = { marginTop = 20, color = Color.yellow, whiteSpace = WhiteSpace.Normal }
                };
                // Store ref to update later? For now just static instruction
                return l;
            });

            // Right: 3D Preview
            var topRightPanel = new GraphicalUserInterfaceBuilder("RightPanel")
                .WithPercentSize(70, 100)
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                .WithBackgroundColor(Color.black)
                .WithBorderWidth(1).WithBorderColor(Color.gray);

            topRightPanel.AddChild(ctx =>
            {
                // This container drives the RenderPreview loop
                var container = new IMGUIContainer(RenderPreview);
                container.style.flexGrow = 1;
                return container;
            });

            topSuperPanel.AddChild(topLeftPanel);
            topSuperPanel.AddChild(topRightPanel);

            // ------------------------------------------------------------
            // BOTTOM PANEL (Sockets List)
            // ------------------------------------------------------------
            var bottomSuperPanel = new GraphicalUserInterfaceBuilder("BottomSuperPanel")
                .WithPercentSize(100, 40)
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                .WithMarginTop(5);

            var bottomLeftPanel = new GraphicalUserInterfaceBuilder("LeftPanel")
                .WithPercentSize(100, 100)
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                .WithPadding(10)
                .WithBackgroundColor(new Color(0.25f, 0.25f, 0.25f));

            bottomLeftPanel.AddChild(new Label("ACTIVE SOCKETS") { style = { unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 5 } });
            bottomLeftPanel.AddChild(ctx =>
            {
                _hierarchyList = new ListView(_flatHierarchy, 20,
                    makeItem: () => new Label(),
                    bindItem: (el, i) =>
                    {
                        if (i >= 0 && i < _flatHierarchy.Count && _flatHierarchy[i] != null)
                        {
                            var t = _flatHierarchy[i];
                            var lbl = (el as Label);
                            lbl.text = t.name;
                            // Highlight the bone if it was the last one clicked
                            lbl.style.color = (t.name == _lastHitName) ? Color.yellow : new Color(0.8f, 0.8f, 0.8f);
                        }
                    }
                );
                _hierarchyList.style.flexGrow = 1;
                return _hierarchyList;
            });

            bottomSuperPanel.AddChild(bottomLeftPanel);
            builder.AddChild(topSuperPanel);
            builder.AddChild(bottomSuperPanel);

            RefreshPreview();
            return builder.Build();
        }

        // ------------------------------------------------------------
        // PREVIEW & INTERACTION LOGIC
        // ------------------------------------------------------------

        private void InitializePreview()
        {
            if (_previewUtility != null) return;
            _previewUtility = new PreviewRenderUtility();
            _previewUtility.camera.fieldOfView = 40f; // Slightly wider FOV for better closeups
            _previewUtility.camera.farClipPlane = 100f;
            _previewUtility.camera.nearClipPlane = 0.01f;

            _previewUtility.lights[0].intensity = 1.2f;
            _previewUtility.lights[0].transform.rotation = Quaternion.Euler(40f, 40f, 0f);
            _previewUtility.lights[1].intensity = 0.5f;
        }

        private void RenderPreview()
        {
            if (_previewUtility == null) InitializePreview();

            Rect rect = GUILayoutUtility.GetRect(GUILayoutUtility.GetLastRect().width, GUILayoutUtility.GetLastRect().height, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            if (Event.current.type == EventType.Repaint)
            {
                _previewUtility.BeginPreview(rect, GUIStyle.none);

                // 1. Position Camera (Orbit Logic)
                var rot = Quaternion.Euler(_orbitAngles.x, _orbitAngles.y, 0);
                var pos = _cameraPivot + (rot * new Vector3(0, 0, -_zoomDistance));
                _previewUtility.camera.transform.SetPositionAndRotation(pos, Quaternion.LookRotation(_cameraPivot - pos));

                // 2. Render Scene
                _previewUtility.camera.Render();

                // 3. Draw Interaction Gizmos (Visualizing the Socket)
                if (_lastHitPoint.HasValue)
                {
                    // Draw a socket marker in the preview world
                    Handles.SetCamera(_previewUtility.camera);
                    Handles.color = Color.yellow;
                    Handles.DrawWireDisc(_lastHitPoint.Value, Vector3.up, 0.1f);
                    Handles.DrawWireDisc(_lastHitPoint.Value, Vector3.right, 0.1f);
                    Handles.DrawWireDisc(_lastHitPoint.Value, Vector3.forward, 0.1f);
                }

                // 4. Paint Texture
                var tex = _previewUtility.EndPreview();
                GUI.DrawTexture(rect, tex, ScaleMode.StretchToFill, false);
            }

            // Handle Mouse Input
            HandleInput(rect);
        }

        private void HandleInput(Rect rect)
        {
            Event e = Event.current;

            // Must be inside the view to interact
            if (!rect.Contains(e.mousePosition)) return;

            // A. Zoom (Scroll Wheel)
            if (e.type == EventType.ScrollWheel)
            {
                _zoomDistance += e.delta.y * 0.05f;
                _zoomDistance = Mathf.Clamp(_zoomDistance, 0.2f, 10f); // "Within reason"
                e.Use();
                GUI.changed = true;
            }

            // B. Orbit (Right Mouse Drag)
            if (e.type == EventType.MouseDrag && e.button == 1)
            {
                _orbitAngles.y += e.delta.x * 1.5f; // Speed up Yaw
                _orbitAngles.x += e.delta.y * 1.5f; // Speed up Pitch

                // Clamp Pitch so we don't flip upside down
                _orbitAngles.x = Mathf.Clamp(_orbitAngles.x, -85f, 85f);

                e.Use();
                GUI.changed = true;
            }

            // C. Pan (Middle Mouse Drag)
            if (e.type == EventType.MouseDrag && e.button == 2)
            {
                var rot = Quaternion.Euler(_orbitAngles.x, _orbitAngles.y, 0);
                // Move pivot relative to camera rotation
                _cameraPivot -= rot * new Vector3(e.delta.x, -e.delta.y, 0) * 0.005f;
                e.Use();
                GUI.changed = true;
            }

            // D. Interact / Place Socket (Left Click)
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                PerformRaycast(e.mousePosition, rect);
                e.Use();
                GUI.changed = true;
            }
        }

        private void PerformRaycast(Vector2 mousePos, Rect rect)
        {
            if (_previewInstance == null) return;

            // 1. Convert Mouse Pos to Ray
            // PreviewRenderUtility camera calculates projection based on the rect provided to BeginPreview
            // Note: e.mousePosition is relative to the GUI Container (Top-Left 0,0)

            // We need to map 0.width to 0.1 for viewport
            float x = (mousePos.x - rect.x) / rect.width;
            float y = (mousePos.y - rect.y) / rect.height;

            // In Unity Camera Viewport, Y is up (0 at bottom, 1 at top), but GUI Y is down (0 at top).
            // So we flip Y.
            Ray ray = _previewUtility.camera.ViewportPointToRay(new Vector3(x, 1f - y, 0));

            // 2. Brute Force Raycast against Mesh Renderers
            // (Since Preview Scene has no physics, we iterate renderers and check bounds/mesh)
            var renderers = _previewInstance.GetComponentsInChildren<Renderer>();

            float closestDist = float.MaxValue;
            Transform closestBone = null;
            Vector3 hitPoint = Vector3.zero;

            foreach (var r in renderers)
            {
                Bounds b = r.bounds;
                if (b.IntersectRay(ray, out float dist))
                {
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestBone = r.transform;
                        hitPoint = ray.GetPoint(dist);
                    }
                }
            }

            // 3. Register Hit
            if (closestBone != null)
            {
                _lastHitPoint = hitPoint;
                _lastHitName = closestBone.name;
                Debug.Log($"[Socket Editor] Selected Part: {_lastHitName} at {hitPoint}");

                // Refresh list UI to highlight the bone
                _hierarchyList?.RefreshItems();
            }
            else
            {
                _lastHitPoint = null;
                _lastHitName = null;
            }
        }

        private void RefreshPreview()
        {
            if (_previewUtility == null) return;
            if (_previewInstance) Object.DestroyImmediate(_previewInstance);

            if (_visualPrefab != null)
                _previewInstance = _previewUtility.InstantiatePrefabInScene(_visualPrefab);
            else
            {
                _previewInstance = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                _previewUtility.AddSingleGO(_previewInstance);
            }

            if (_previewInstance != null)
            {
                _previewInstance.transform.position = Vector3.zero;
                _previewInstance.transform.rotation = Quaternion.identity;

                // Auto-center camera on Bounds
                var bounds = new Bounds(Vector3.zero, Vector3.zero);
                var renderers = _previewInstance.GetComponentsInChildren<Renderer>();
                foreach (var r in renderers) bounds.Encapsulate(r.bounds);

                if (bounds.size.magnitude > 0)
                {
                    _cameraPivot = bounds.center;
                    _zoomDistance = bounds.size.magnitude * 1.5f;
                }

                // Refresh list
                _flatHierarchy.Clear();
                GetChildrenRecursive(_previewInstance.transform);
                if (_hierarchyList != null) _hierarchyList.Rebuild();
            }
        }

        private void GetChildrenRecursive(Transform t)
        {
            _flatHierarchy.Add(t);
            foreach (Transform child in t) GetChildrenRecursive(child);
        }

        public void Cleanup()
        {
            _previewUtility?.Cleanup();
            if (_previewInstance) Object.DestroyImmediate(_previewInstance);
        }
    }
}