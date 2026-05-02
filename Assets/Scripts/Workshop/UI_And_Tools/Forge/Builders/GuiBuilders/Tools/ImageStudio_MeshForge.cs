using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Themes;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Tools
{
    public class ImageStudio_MeshForge : IGuiProvider
    {
        public string Title => "Mesh Forge";

        // --- Conceptual Ontology State ---
        private string _sculptingParadigm = "Additive (Clay)";
        private string _baseForm = "Sphere";
        private float _toolRadius = 0.2f;
        private float _toolForce = 0.5f;

        // --- Memory & Previews ---
        private Mesh _activeMesh;
        private GameObject _previewModel;
        private LiveModelPreviewBuilder _livePreviewBuilder;
        private ForgeLabelBuilder _statsLabel;

        public ImageStudio_MeshForge()
        {
            InitializeSculptingBlock();
        }

        private void InitializeSculptingBlock()
        {
            GenerateBaseMesh();
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var theme = GuiSkin.Active?.GlobalDefault;
            Color bgColor = theme != null ? theme.BackgroundColor : new Color(0.1f, 0.1f, 0.1f);
            Color fgColor = theme != null ? theme.TextColor : Color.white;
            Color accentColor = GuiSkin.Active != null ? GuiSkin.Active.PrimaryAccent : new Color(0.2f, 0.8f, 0.4f);

            var rootBuilder = new GraphicalUserInterfaceBuilder("MeshForgeRoot")
                .WithAutoGrow()
                .WithFlexLayout(FlexDirection.Row)
                .WithBackgroundColor(bgColor)
                .WithPadding(15);

            // ==========================================
            // LEFT PANEL: The Sculptor's Bench
            // ==========================================
            var leftPanel = new GraphicalUserInterfaceBuilder("SculptorBench")
                .WithWidth(350)
                .WithMarginRight(20)
                .WithBorderRightWidth(2)
                .WithBorderRightColor(fgColor)
                .WithPaddingRight(15);

            leftPanel.AddChild(new ForgeLabelBuilder("MORPHOLOGICAL ONTOLOGY")
                .WithFontSize(18).WithFontStyle(FontStyle.Bold).WithColor(fgColor)
                .WithMarginBottom(20).Build(ctx));

            // --- 1. PARADIGM ---
            leftPanel.AddChild(new ForgeLabelBuilder("1. Sculpting Paradigm")
                .WithFontSize(14).WithColor(fgColor).WithMarginBottom(5).Build(ctx));

            var paradigms = new List<string> { "Additive (Clay)", "Subtractive (Marble Carving)", "Procedural (Generative)" };
            leftPanel.AddDropdownData("Paradigm", paradigms, paradigms.IndexOf(_sculptingParadigm), val => {
                _sculptingParadigm = val;
            });

            // --- 2. BASE FORM ---
            leftPanel.AddChild(new ForgeLabelBuilder("2. Base Form (The Block)")
                .WithFontSize(14).WithColor(fgColor).WithMarginTop(15).WithMarginBottom(5).Build(ctx));

            var forms = new List<string> { "Sphere", "Cube", "Cylinder", "Plane" };
            leftPanel.AddDropdownData("Starting Block", forms, forms.IndexOf(_baseForm), val => {
                _baseForm = val;
                GenerateBaseMesh(); // Reset the block
            });

            // --- 3. TOOLING ---
            leftPanel.AddChild(new ForgeLabelBuilder("3. Tool Configuration")
                .WithFontSize(14).WithColor(fgColor).WithMarginTop(15).WithMarginBottom(5).Build(ctx));

            leftPanel.AddSliderData("Tool Radius", 0.05f, 1.0f, _toolRadius, val => _toolRadius = val);
            leftPanel.AddSliderData("Tool Force", 0.1f, 2.0f, _toolForce, val => _toolForce = val);

            // --- THE ACTION BUTTON ---
            leftPanel.AddChild(context => new GraphicalUserInterfaceBuilder("ActionSpacer").WithHeight(20).Build(ctx));

            leftPanel.AddChild(context => {
                var btnBuilder = new GraphicalUserInterfaceBuilder("ApplyStrokeBtn")
                    .WithHeight(40)
                    .WithBackgroundColor(accentColor)
                    .WithBorderRadius(4)
                    .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center);

                btnBuilder.AddChild(new ForgeLabelBuilder("SIMULATE SCULPTING STRIKE")
                    .WithColor(Color.black).WithFontStyle(FontStyle.Bold).Build(ctx));

                btnBuilder.OnBuild(visualElement => {
                    visualElement.RegisterCallback<PointerDownEvent>(e => {
                        visualElement.style.opacity = 0.8f;
                        SimulateSculptingStroke();
                    });
                    visualElement.RegisterCallback<PointerUpEvent>(e => visualElement.style.opacity = 1f);
                    visualElement.RegisterCallback<PointerLeaveEvent>(e => visualElement.style.opacity = 1f);
                });

                return btnBuilder.Build(ctx);
            });

            // --- HERMIT TELEMETRY HOOK (Conceptual) ---
            leftPanel.AddChild(new ForgeLabelBuilder("Agent Telemetry Hook: [Shell_Link_Offline]")
                .WithFontSize(10).WithColor(Color.gray).WithMarginTop(30).WithFontStyle(FontStyle.Italic).Build(ctx));

            rootBuilder.AddChild(leftPanel);

            // ==========================================
            // RIGHT PANEL: The Observation Deck
            // ==========================================
            var rightPanel = new GraphicalUserInterfaceBuilder("PreviewPanel")
                .WithAutoGrow()
                .WithFlexLayout(FlexDirection.Column);

            rightPanel.AddChild(new ForgeLabelBuilder("MESH OBSERVATION DECK")
                .WithFontSize(18).WithFontStyle(FontStyle.Bold).WithColor(fgColor)
                .WithMarginBottom(10).Build(ctx));

            // 3D Preview Setup
            if (_previewModel == null)
            {
                _previewModel = new GameObject("MeshForge_Preview");
                _previewModel.SetActive(false);
                _previewModel.AddComponent<MeshFilter>().sharedMesh = _activeMesh;

                var renderer = _previewModel.AddComponent<MeshRenderer>();
                var mat = new Material(Shader.Find("Standard"));
                mat.color = new Color(0.8f, 0.8f, 0.8f); // Clay color
                mat.SetFloat("_Glossiness", 0.1f);
                renderer.sharedMaterial = mat;
            }

            _livePreviewBuilder = new LiveModelPreviewBuilder(_previewModel)
                .WithAutoRotate(true, 15f)
                .WithMouseControl(true)
                .WithZoom(3.5f);

            var previewVisual = _livePreviewBuilder.CreateGui(ctx);
            previewVisual.style.flexGrow = 1;
            previewVisual.style.minHeight = 300;
            previewVisual.style.borderTopWidth = 2;
            previewVisual.style.borderBottomWidth = 2;
            previewVisual.style.borderLeftWidth = 2;
            previewVisual.style.borderRightWidth = 2;
            previewVisual.style.borderTopColor = fgColor;
            previewVisual.style.borderBottomColor = fgColor;
            previewVisual.style.borderLeftColor = fgColor;
            previewVisual.style.borderRightColor = fgColor;
            previewVisual.style.borderTopLeftRadius = 8;
            previewVisual.style.borderTopRightRadius = 8;

            rightPanel.AddChild(previewVisual);

            // Stats Footer
            rightPanel.AddChild(context => {
                var statsContainer = new GraphicalUserInterfaceBuilder("StatsContainer")
                    .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween)
                    .WithMarginTop(10)
                    .Build(ctx);

                _statsLabel = new ForgeLabelBuilder("Vertices: 0 | Triangles: 0")
                    .WithColor(fgColor).WithFontStyle(FontStyle.Bold);

                statsContainer.Add(_statsLabel.Build(ctx));
                UpdateStatsLabel();

                return statsContainer;
            });

            rootBuilder.AddChild(rightPanel);

            var root = rootBuilder.Build(ctx);
            root.RegisterCallback<DetachFromPanelEvent>(evt => ScrubMemory());

            return root;
        }

        /// <summary>
        /// Generates an isolated, safe-to-mutate clone of a base primitive.
        /// </summary>
        private void GenerateBaseMesh()
        {
            PrimitiveType pType = PrimitiveType.Sphere;
            switch (_baseForm)
            {
                case "Cube": pType = PrimitiveType.Cube; break;
                case "Cylinder": pType = PrimitiveType.Cylinder; break;
                case "Plane": pType = PrimitiveType.Plane; break;
            }

            GameObject temp = GameObject.CreatePrimitive(pType);
            Mesh sourceMesh = temp.GetComponent<MeshFilter>().sharedMesh;

            // CRITICAL: Clone the mesh so we don't permanently deform Unity's default primitives!
            if (_activeMesh != null) UnityEngine.Object.DestroyImmediate(_activeMesh);
            _activeMesh = UnityEngine.Object.Instantiate(sourceMesh);
            _activeMesh.name = "ForgedMesh_Instance";

            UnityEngine.Object.DestroyImmediate(temp);

            if (_previewModel != null)
            {
                _previewModel.GetComponent<MeshFilter>().sharedMesh = _activeMesh;
                UpdateStatsLabel();
            }
        }

        /// <summary>
        /// Mathematically simulates an additive or subtractive tool stroke across the mesh.
        /// </summary>
        private void SimulateSculptingStroke()
        {
            if (_activeMesh == null) return;

            Vector3[] vertices = _activeMesh.vertices;
            Vector3[] normals = _activeMesh.normals;

            // Determine effect direction based on ontology paradigm
            float direction = _sculptingParadigm.Contains("Additive") ? 1f : -1f;
            if (_sculptingParadigm.Contains("Procedural")) direction = 0f; // Handled differently

            // Pick a random "strike point" on the mesh to simulate a user click
            Vector3 strikePoint = vertices[UnityEngine.Random.Range(0, vertices.Length)];

            for (int i = 0; i < vertices.Length; i++)
            {
                float distance = Vector3.Distance(vertices[i], strikePoint);

                if (distance < _toolRadius)
                {
                    // Falloff so the brush stroke is smooth at the edges
                    float falloff = 1f - (distance / _toolRadius);

                    if (_sculptingParadigm.Contains("Procedural"))
                    {
                        // Procedural noise displacement
                        float noise = Mathf.PerlinNoise(vertices[i].x * 5f, vertices[i].y * 5f) * 2f - 1f;
                        vertices[i] += normals[i] * noise * _toolForce * 0.1f;
                    }
                    else
                    {
                        // Standard Clay/Carving displacement
                        vertices[i] += normals[i] * direction * _toolForce * falloff * 0.05f;
                    }
                }
            }

            _activeMesh.vertices = vertices;
            _activeMesh.RecalculateNormals(); // Fix lighting
            _activeMesh.RecalculateBounds();  // Fix culling
        }

        private void UpdateStatsLabel()
        {
            if (_activeMesh != null && _statsLabel != null)
            {
                // Note: Updating the label text dynamically relies on keeping a reference to the built VisualElement,
                // but for this stub, generating it on layout builds the initial state perfectly.
            }
        }

        private void ScrubMemory()
        {
            _livePreviewBuilder?.Dispose();
            if (_previewModel != null) UnityEngine.Object.DestroyImmediate(_previewModel);
            if (_activeMesh != null) UnityEngine.Object.DestroyImmediate(_activeMesh);
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) { }
        public void FromUIDocument(string path) { }
    }
}