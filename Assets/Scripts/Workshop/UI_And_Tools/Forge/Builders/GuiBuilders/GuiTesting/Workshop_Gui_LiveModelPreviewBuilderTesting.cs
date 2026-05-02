using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.GuiTesting
{
    /// <summary>
    /// The Ultimate Interactive Test Laboratory for the LiveModelPreviewBuilder.
    /// Utilizes a nested Gimbal Hierarchy to provide real-time, zero-rebuild manipulation.
    /// </summary>
    public class Workshop_Gui_LiveModelPreviewBuilderTesting : IGuiProvider
    {
        public string Title => "FORGE CORE: LMP LABORATORY";

        private GuiContext _guiContext;
        private VisualElement _rootContainer;
        private VisualElement _previewContainer;
        private VisualElement _inspectorContainer;

        // --- GIMBAL RIG HIERARCHY ---
        private GameObject _rigRoot;
        private Transform _gimbalPitch;
        private Transform _gimbalYaw;
        private Transform _gimbalScale;
        private Transform _targetContainer;

        // --- ACTIVE TARGET MEMORY ---
        private GameObject _activeTarget;
        private Texture2D _activeTexture;
        private Material _activeMaterial;

        // --- UI STATE ---
        private int _targetMode = 0; // 0 = 3D Primitive, 1 = 3D Station, 2 = 2D Image Quad
        private float _zoom = 1.0f;
        private float _pitch = -25f;
        private float _yaw = 45f;
        private bool _mouseControl = true;
        private bool _viewCube = true;
        private Color _bgColor = new Color(0.02f, 0.01f, 0.05f); // Nebula Dark

        // --- ORBIT STATE ---
        private bool _orbitEngaged = false;
        private float _orbitSpeed = 45f;
        private int _orbitAxis = 1; // 0=X, 1=Y, 2=Z

        // --- ENTITY SPECIFIC STATE ---
        private float _stationRingSpeed = 20f;
        private Color _imageColor = Color.white;

        public VisualElement CreateGui(GuiContext ctx)
        {
            _guiContext = ctx;

            _rootContainer = new VisualElement { style = { flexGrow = 1, flexDirection = FlexDirection.Row, width = Length.Percent(100), height = Length.Percent(100) } };

            // Initialize the 3D Gimbal System
            InitializeGimbalRig();

            // 1. Left Sidebar (Master Controls)
            var leftSidebar = BuildControlPanel();
            leftSidebar.style.width = 350;
            leftSidebar.style.borderRightWidth = 2;
            leftSidebar.style.borderRightColor = new Color(0.4f, 0.8f, 1f, 0.3f);

            // 2. Center Area (Live Preview)
            _previewContainer = new VisualElement { style = { flexGrow = 1, backgroundColor = Color.black } };

            // 3. Right Sidebar (Pluggable Entity Inspector)
            _inspectorContainer = new VisualElement { style = { width = 300, backgroundColor = new Color(0.1f, 0.1f, 0.12f), borderLeftWidth = 2, borderLeftColor = new Color(0.4f, 0.8f, 1f, 0.3f) } };

            _rootContainer.Add(leftSidebar);
            _rootContainer.Add(_previewContainer);
            _rootContainer.Add(_inspectorContainer);

            // Initial Build
            SwapTargetMode();
            RebuildLMP();

            // Start the Real-Chronos Update Loop
            _rootContainer.RegisterCallback<AttachToPanelEvent>(e => {
                _rootContainer.schedule.Execute(UpdateSimulation).Every(16); // ~60 FPS
            });

            // Memory Protection
            _rootContainer.RegisterCallback<DetachFromPanelEvent>(evt => CleanUpMemory());

            return _rootContainer;
        }

        // --- THE GIMBAL SYSTEM ---

        private void InitializeGimbalRig()
        {
            // The Master Root (What the LMP Camera looks at)
            _rigRoot = new GameObject("[LMP_RIG_ROOT]");
            _rigRoot.transform.position = new Vector3(0, -5000, 0); // Hide far below

            // Pitch (X-Axis)
            _gimbalPitch = new GameObject("Gimbal_Pitch").transform;
            _gimbalPitch.SetParent(_rigRoot.transform, false);

            // Yaw (Y-Axis)
            _gimbalYaw = new GameObject("Gimbal_Yaw").transform;
            _gimbalYaw.SetParent(_gimbalPitch, false);

            // Scale (Zoom)
            _gimbalScale = new GameObject("Gimbal_Scale").transform;
            _gimbalScale.SetParent(_gimbalYaw, false);

            // The container for the actual mesh
            _targetContainer = new GameObject("Target_Container").transform;
            _targetContainer.SetParent(_gimbalScale, false);
        }

        // --- MASTER CONTROL PANEL (LEFT) ---

        private VisualElement BuildControlPanel()
        {
            var controls = new ScrollView(ScrollViewMode.Vertical);
            controls.style.paddingLeft = 15; controls.style.paddingRight = 15; controls.style.paddingTop = 15; controls.style.paddingBottom = 20;
            controls.style.backgroundColor = new Color(0.05f, 0.05f, 0.06f);

            controls.Add(new Label("LMP LABORATORY") { style = { fontSize = 22, unityFontStyleAndWeight = FontStyle.Bold, color = new Color(0.4f, 0.8f, 1f), letterSpacing = 2, marginBottom = 15 } });

            Action<string> AddHeader = (text) => {
                controls.Add(new Label(text) { style = { marginTop = 20, marginBottom = 10, unityFontStyleAndWeight = FontStyle.Bold, color = Color.gray } });
                var line = new VisualElement { style = { height = 1, backgroundColor = new Color(0.2f, 0.2f, 0.3f), marginBottom = 10 } };
                controls.Add(line);
            };

            // --- TARGET SELECTION ---
            AddHeader("TARGET ENTITY");
            var modeDropdown = new DropdownField(new List<string> { "3D Primitive Data", "3D Complex Station", "2D Image Quad (Interactive)" }, _targetMode);
            modeDropdown.RegisterValueChangedCallback(evt => {
                _targetMode = modeDropdown.index;
                SwapTargetMode();
            });
            controls.Add(modeDropdown);

            // --- REAL-TIME CAMERA GIMBALS ---
            AddHeader("REAL-TIME GIMBALS (NO REBUILD)");

            var zoomSlider = new Slider("Optical Zoom", 0.1f, 5f) { value = _zoom };
            zoomSlider.RegisterValueChangedCallback(e => _zoom = e.newValue);
            controls.Add(zoomSlider);

            var pitchSlider = new Slider("Pitch Gimbal (X)", -90f, 90f) { value = _pitch };
            pitchSlider.RegisterValueChangedCallback(e => _pitch = e.newValue);
            controls.Add(pitchSlider);

            var yawSlider = new Slider("Yaw Gimbal (Y)", -180f, 180f) { value = _yaw };
            yawSlider.RegisterValueChangedCallback(e => _yaw = e.newValue);
            controls.Add(yawSlider);

            // --- ORBIT INJECTION ---
            AddHeader("ORBITAL INJECTION");

            var orbitToggle = new Toggle("Engage Continuous Orbit") { value = _orbitEngaged };
            orbitToggle.RegisterValueChangedCallback(e => _orbitEngaged = e.newValue);
            controls.Add(orbitToggle);

            var orbitAxis = new DropdownField("Axis of Rotation", new List<string> { "X-Axis", "Y-Axis (Standard)", "Z-Axis" }, _orbitAxis);
            orbitAxis.RegisterValueChangedCallback(e => _orbitAxis = orbitAxis.index);
            controls.Add(orbitAxis);

            var speedSlider = new Slider("Rotational Velocity", 1f, 360f) { value = _orbitSpeed };
            speedSlider.RegisterValueChangedCallback(e => _orbitSpeed = e.newValue);
            controls.Add(speedSlider);

            // --- HARD REBUILD CONTROLS ---
            AddHeader("CORE LMP ARGS (REQUIRES REBUILD)");

            var mouseToggle = new Toggle("Native Mouse Overrides") { value = _mouseControl };
            mouseToggle.RegisterValueChangedCallback(e => _mouseControl = e.newValue);
            controls.Add(mouseToggle);

            var cubeToggle = new Toggle("Render View Cube") { value = _viewCube };
            cubeToggle.RegisterValueChangedCallback(e => _viewCube = e.newValue);
            controls.Add(cubeToggle);

            var colorRow = new VisualElement { style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap, marginBottom = 20, marginTop = 10 } };
            Color[] presets = { new Color(0.02f, 0.01f, 0.05f), new Color(0.2f, 0.05f, 0.05f), new Color(0.05f, 0.2f, 0.05f), new Color(0.2f, 0.2f, 0.2f), Color.black };

            foreach (var c in presets)
            {
                var swatch = new Button(() => { _bgColor = c; RebuildLMP(); }) { text = "" };
                swatch.style.width = 40; swatch.style.height = 40; swatch.style.backgroundColor = c; swatch.style.marginRight = 5;
                colorRow.Add(swatch);
            }
            controls.Add(colorRow);

            var applyBtn = new Button(RebuildLMP) { text = "HARD REBUILD PIPELINE" };
            applyBtn.style.height = 40; applyBtn.style.backgroundColor = new Color(0.6f, 0.1f, 0.1f); applyBtn.style.color = Color.white; applyBtn.style.unityFontStyleAndWeight = FontStyle.Bold;
            controls.Add(applyBtn);

            return controls;
        }

        // --- LMP CONSTRUCTION ---

        private void RebuildLMP()
        {
            if (_previewContainer == null) return;
            _previewContainer.Clear();

            // We pass the MASTER RIG ROOT to the LMP. 
            // It will render whatever we dynamically slot into the _targetContainer!
            var lmpBuilder = new LiveModelPreviewBuilder(_rigRoot)
                .WithZoom(2.5f) // Base zoom is handled by LMP, dynamic zoom by Gimbal Scale
                .WithPitch(0f)  // Base rotation handled by LMP, dynamic by Gimbal
                .WithYaw(0f)
                .WithMouseControl(_mouseControl)
                .WithGizmos(_viewCube)
                .WithBackgroundColor(_bgColor);

            var previewVe = lmpBuilder.Build();
            previewVe.style.flexGrow = 1;

            // UI Overlay for Telemetry
            var overlay = new Label("[LIVE SENSOR FEED ACTIVE]") { style = { position = Position.Absolute, top = 20, left = 20, color = new Color(0.4f, 0.8f, 1f, 0.5f), unityFontStyleAndWeight = FontStyle.Bold, letterSpacing = 2 } };
            previewVe.Add(overlay);

            _previewContainer.Add(previewVe);

            // ERADICATE MAGENTA!
            // We must wait a tiny fraction for the LMP to actually spawn the View Cube in the scene
            _previewContainer.schedule.Execute(SanitizeAllPinkMaterials).StartingIn(100);
        }

        // --- PLUGGABLE TARGET SWAPPING ---

        private void SwapTargetMode()
        {
            // Destroy old target
            if (_activeTarget != null) UnityEngine.Object.DestroyImmediate(_activeTarget);
            _activeTarget = null;

            // Build new target
            if (_targetMode == 0) _activeTarget = CreateSimpleModel();
            else if (_targetMode == 1) _activeTarget = CreateComplexModel();
            else if (_targetMode == 2) _activeTarget = CreateImageQuad();

            // Insert into the Gimbal System
            if (_activeTarget != null)
            {
                _activeTarget.transform.SetParent(_targetContainer, false);
                _activeTarget.transform.localPosition = Vector3.zero;
            }

            // Ensure materials are pipeline compliant
            SanitizeAllPinkMaterials();

            // Update the Pluggable Inspector Panel
            RefreshInspector();
        }

        private GameObject CreateSimpleModel()
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            return cube;
        }

        private GameObject CreateComplexModel()
        {
            var root = new GameObject("StationRoot");
            var core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            core.transform.SetParent(root.transform);
            core.transform.localScale = Vector3.one * 1.5f;

            for (int i = 0; i < 6; i++)
            {
                var satellite = GameObject.CreatePrimitive(PrimitiveType.Cube);
                satellite.name = "RingSegment"; // Tagged so we can animate it later
                satellite.transform.SetParent(root.transform);
                float angle = i * (Mathf.PI * 2f) / 6f;
                satellite.transform.localPosition = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * 2f;
                satellite.transform.localScale = new Vector3(0.8f, 0.2f, 0.8f);
                satellite.transform.LookAt(root.transform);
            }
            return root;
        }

        private GameObject CreateImageQuad()
        {
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            // Size it like a classic portrait card
            quad.transform.localScale = new Vector3(2f, 3f, 1f);

            // Generate Synthetic Texture
            _activeTexture = CreateSyntheticTexture();

            // Apply to material
            Shader s = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null ? Shader.Find("Universal Render Pipeline/Unlit") : Shader.Find("Unlit/Texture");
            _activeMaterial = new Material(s != null ? s : Shader.Find("Standard"));

            _activeMaterial.mainTexture = _activeTexture;
            _activeMaterial.color = _imageColor;

            if (s != null && s.name.Contains("Universal Render Pipeline"))
                _activeMaterial.SetTexture("_BaseMap", _activeTexture);

            quad.GetComponent<Renderer>().sharedMaterial = _activeMaterial;

            // Make it double sided by duplicating and flipping
            var backQuad = GameObject.Instantiate(quad, quad.transform);
            backQuad.transform.localPosition = new Vector3(0, 0, 0.01f);
            backQuad.transform.localRotation = Quaternion.Euler(0, 180, 0);

            return quad;
        }

        private Texture2D CreateSyntheticTexture()
        {
            int size = 256;
            Texture2D tex = new Texture2D(size, size);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / size;
                    float v = (float)y / size;
                    bool isChecker = ((x / 32) + (y / 32)) % 2 == 0;
                    Color color = isChecker ? new Color(u, 0.2f, 1f) : new Color(0.1f, v, 0.5f);
                    if (x < 5 || x > size - 5 || y < 5 || y > size - 5) color = Color.cyan;
                    tex.SetPixel(x, y, color);
                }
            }
            tex.Apply();
            return tex;
        }

        // --- PLUGGABLE ENTITY INSPECTOR (RIGHT) ---

        private void RefreshInspector()
        {
            if (_inspectorContainer == null) return;
            _inspectorContainer.Clear();

            _inspectorContainer.Add(new Label("ENTITY INSPECTOR") { style = {  fontSize = 16, unityFontStyleAndWeight = FontStyle.Bold, color = Color.white, backgroundColor = new Color(0.2f, 0.2f, 0.25f) } });

            var scroll = new ScrollView { style = { paddingTop = 15, paddingBottom = 15, paddingLeft = 15, paddingRight = 15 } };

            if (_targetMode == 0) // Primitive
            {
                scroll.Add(new Label("No complex properties available for Basic Primitive.") { style = { color = Color.gray, whiteSpace = WhiteSpace.Normal } });
            }
            else if (_targetMode == 1) // Station
            {
                scroll.Add(new Label("STATION CONTROLS") { style = { color = new Color(0.4f, 0.8f, 1f), unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });
                var speedSlider = new Slider("Internal Ring Velocity", -100f, 100f) { value = _stationRingSpeed };
                speedSlider.RegisterValueChangedCallback(e => _stationRingSpeed = e.newValue);
                scroll.Add(speedSlider);
            }
            else if (_targetMode == 2) // 2D Image
            {
                scroll.Add(new Label("IMAGE QUAD CONTROLS") { style = { color = new Color(0.4f, 0.8f, 1f), unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });

                var rSlider = new Slider("Tint R", 0f, 1f) { value = _imageColor.r };
                rSlider.RegisterValueChangedCallback(e => { _imageColor.r = e.newValue; ApplyImageTint(); });
                scroll.Add(rSlider);

                var gSlider = new Slider("Tint G", 0f, 1f) { value = _imageColor.g };
                gSlider.RegisterValueChangedCallback(e => { _imageColor.g = e.newValue; ApplyImageTint(); });
                scroll.Add(gSlider);

                var bSlider = new Slider("Tint B", 0f, 1f) { value = _imageColor.b };
                bSlider.RegisterValueChangedCallback(e => { _imageColor.b = e.newValue; ApplyImageTint(); });
                scroll.Add(bSlider);
            }

            _inspectorContainer.Add(scroll);
        }

        private void ApplyImageTint()
        {
            if (_activeMaterial != null)
            {
                _activeMaterial.color = _imageColor;
                if (_activeMaterial.HasProperty("_BaseColor")) _activeMaterial.SetColor("_BaseColor", _imageColor);
            }
        }

        // --- REAL-TIME UPDATE LOOP ---

        private void UpdateSimulation()
        {
            float dt = 0.016f;

            // 1. Apply UI Gimbal Overrides (Real-time Zoom, Pitch, Yaw)
            if (_gimbalScale != null) _gimbalScale.localScale = Vector3.one * _zoom;
            if (_gimbalPitch != null) _gimbalPitch.localRotation = Quaternion.Euler(_pitch, 0, 0);
            if (_gimbalYaw != null) _gimbalYaw.localRotation = Quaternion.Euler(0, _yaw, 0);

            // 2. Apply Rotational Orbit
            if (_orbitEngaged && _targetContainer != null)
            {
                Vector3 axis = _orbitAxis == 0 ? Vector3.right : (_orbitAxis == 1 ? Vector3.up : Vector3.forward);
                _targetContainer.Rotate(axis * _orbitSpeed * dt, Space.Self);
            }

            // 3. Apply Pluggable Entity Animations
            if (_targetMode == 1 && _activeTarget != null) // Station Ring Animation
            {
                foreach (Transform child in _activeTarget.transform)
                {
                    if (child.name == "RingSegment")
                    {
                        // Orbit the segment around the core
                        child.RotateAround(_activeTarget.transform.position, Vector3.up, _stationRingSpeed * dt);
                    }
                }
            }
        }

        // --- UTILITIES & MEMORY ---

        private void SanitizeAllPinkMaterials()
        {
            bool isURP = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null;
            Shader targetShader = isURP ? Shader.Find("Universal Render Pipeline/Lit") : Shader.Find("Standard");
            if (targetShader == null) return;

            // Deep sweep for everything rendered, including the generated View Cube!
            foreach (var renderer in UnityEngine.Object.FindObjectsOfType<Renderer>(true))
            {
                // Only sanitize our test objects and the ViewCube, don't break the whole project
                if (renderer.gameObject.name.Contains("ViewCube") || renderer.gameObject.name.Contains("Gizmo") || renderer.transform.IsChildOf(_rigRoot.transform))
                {
                    foreach (var mat in renderer.materials) // Mutates instance
                    {
                        if (mat != null && (mat.shader.name == "Hidden/InternalErrorShader" || mat.shader.name == "Standard"))
                        {
                            if (isURP && mat.shader.name == "Standard") mat.shader = targetShader;
                        }
                    }
                }
            }
        }

        private void CleanUpMemory()
        {
            if (_rigRoot != null) UnityEngine.Object.DestroyImmediate(_rigRoot);
            if (_activeTexture != null) UnityEngine.Object.DestroyImmediate(_activeTexture);
            if (_activeMaterial != null) UnityEngine.Object.DestroyImmediate(_activeMaterial);
        }

        // Standard Bridge Methods
        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
#if UNITY_EDITOR
        public void ToUIDocument(string path) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), path);
#endif
        public void FromUIDocument(string path) => _guiContext?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(path));
    }
}