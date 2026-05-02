using System;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using UnityEngine;
using UnityEngine.UIElements;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    public class UniverseProjectionForge : IGuiProvider
    {
        public string Title => "COSMIC PROJECTION FORGE";

        private float _sphereRadius = 5000f;
        private int _textureResolution = 2048;
        private float _updateThreshold = 100f;

        private VisualElement _previewContainer;
        private GameObject _starSphereModel;

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new VisualElement { style = { flexGrow = 1, flexDirection = FlexDirection.Row, backgroundColor = new Color(0.02f, 0.02f, 0.05f) } };

            // --- LEFT PANEL: Controls ---
            var controlPanel = new GraphicalUserInterfaceBuilder("CosmicControls")
                .WithWidth(350).WithPadding(20)
                .WithBorderRightWidth(2).WithBorderRightColor(Color.black)
                .Build();

            controlPanel.Add(new Label("SPHERICAL CACHE ENGINE") { style = { fontSize = 24, color = Color.cyan, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 20 } });
            controlPanel.Add(new Label("PROJECTION BOUNDARIES") { style = { color = Color.yellow, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });

            var radiusSlider = new Slider("Sphere Radius (m)", 1000, 20000) { value = _sphereRadius };
            radiusSlider.RegisterValueChangedCallback(e => _sphereRadius = e.newValue);
            controlPanel.Add(radiusSlider);

            var resField = new EnumField("Cache Resolution", (TextureSize)_textureResolution);
            controlPanel.Add(resField);

            var thresholdField = new FloatField("Parallax Update Threshold") { value = _updateThreshold };
            thresholdField.RegisterValueChangedCallback(e => _updateThreshold = e.newValue);
            controlPanel.Add(thresholdField);

            controlPanel.Add(new VisualElement { style = { flexGrow = 1 } }); // Spacer

            var forgeBtn = new Button(() => GenerateUniverseBridge(ctx))
            {
                text = "SUBSTANTIATE COSMIC BRIDGE",
                style = { height = 50, backgroundColor = new Color(0.1f, 0.6f, 0.2f), color = Color.white, unityFontStyleAndWeight = FontStyle.Bold, marginTop = 20 }
            };
            controlPanel.Add(forgeBtn);

            root.Add(controlPanel);

            // --- RIGHT PANEL: Live Projection ---
            _previewContainer = new VisualElement { style = { flexGrow = 1, backgroundColor = Color.black } };
            root.Add(_previewContainer);

            // Auto-Initialize the projection on load
            GenerateUniverseBridge(ctx);

            return root;
        }

        private void GenerateUniverseBridge(GuiContext ctx)
        {
            _previewContainer.Clear();

            if (_starSphereModel != null)
            {
                UnityEngine.Object.DestroyImmediate(_starSphereModel);
            }

            _starSphereModel = BuildProceduralStarSphere();

            // Set up the LiveModelPreviewBuilder with Auto-Rotation (drifting right) and Auto-Zoom (diving in)
            var lmp = new LiveModelPreviewBuilder(_starSphereModel)
                .WithBackgroundColor(Color.black)
                .WithAutoRotate(true, 5f)         // Incrementing to the right (Yaw)
                .WithAutoZoom(true, -0.2f)        // Slowly zooming in towards the surface
                .WithMouseControl(true)
                .WithControls(true)
                .WithGizmos(false)                // Hide Gizmo for a cleaner space vibe
                .WithPreviewLifecycle((ghost, processGroup) => {
                    // Let the projection take up the whole screen safely
                });

            _previewContainer.Add(lmp.CreateGui(ctx));

            Debug.Log("<color=cyan><b>COSMOS:</b></color> Projection sphere established at radius " + _sphereRadius);
        }

        private GameObject BuildProceduralStarSphere()
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = "Cosmic_Background_Sphere";
            UnityEngine.Object.DestroyImmediate(sphere.GetComponent<Collider>());

            // Pipeline-Agnostic Material Cloning
            var renderer = sphere.GetComponent<MeshRenderer>();
            Material mat = new Material(renderer.sharedMaterial);

            // Procedurally generate a starfield texture
            Texture2D starTex = new Texture2D(1024, 1024, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[1024 * 1024];

            // Base Deep Space Color
            for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color(0.01f, 0.01f, 0.02f);

            // Randomly scatter 3000 stars
            for (int i = 0; i < 3000; i++)
            {
                int x = UnityEngine.Random.Range(0, 1024);
                int y = UnityEngine.Random.Range(0, 1024);

                // 10% chance for a cyan/blue star, 90% chance for white
                pixels[y * 1024 + x] = UnityEngine.Random.value > 0.9f ? new Color(0.5f, 0.8f, 1f) : Color.white;
            }

            starTex.SetPixels(pixels);
            starTex.Apply();

            // Assign the texture to whichever pipeline property is active
            if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", starTex); // URP
            if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", starTex); // Standard

            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", Color.white);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0f);

            renderer.sharedMaterial = mat;
            return sphere;
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }

        private enum TextureSize { Res_1024 = 1024, Res_2048 = 2048, Res_4096 = 4096 }
    }
}