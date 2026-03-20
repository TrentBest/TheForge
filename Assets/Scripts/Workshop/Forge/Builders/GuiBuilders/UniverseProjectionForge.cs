
using System;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using TheSingularityWorkshop.FSM_API;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheSingularityWorkshop.Sandbox.Builders
{
    public class UniverseProjectionForge : IGuiProvider
    {
        public string Title => "COSMIC PROJECTION FORGE";

        private float _sphereRadius = 5000f; // The "Atmosphere Boundary"
        private int _textureResolution = 2048; // Cache quality
        private float _updateThreshold = 100f; // Meters moved before redraw

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new VisualElement { style = { flexGrow = 1, paddingTop = 20, paddingBottom = 20, paddingLeft = 20, paddingRight = 20, backgroundColor = new Color(0.02f, 0.02f, 0.05f) } };

            root.Add(new Label("SPHERICAL CACHE ENGINE") { style = { fontSize = 24, color = Color.cyan, unityFontStyleAndWeight = FontStyle.Bold } });

            // Sphere Settings
            root.Add(new Label("\nPROJECTION BOUNDARIES") { style = { color = Color.yellow, unityFontStyleAndWeight = FontStyle.Bold } });

            var radiusSlider = new Slider("Sphere Radius (m)", 1000, 20000) { value = _sphereRadius };
            radiusSlider.RegisterValueChangedCallback(e => _sphereRadius = e.newValue);
            root.Add(radiusSlider);

            var resField = new EnumField("Cache Resolution", (TextureSize)_textureResolution); // Helper enum
            root.Add(resField);

            var thresholdField = new FloatField("Parallax Update Threshold") { value = _updateThreshold };
            thresholdField.RegisterValueChangedCallback(e => _updateThreshold = e.newValue);
            root.Add(thresholdField);

            root.Add(new VisualElement { style = { flexGrow = 1 } });

            // THE GENERATOR BUTTON
            var forgeBtn = new Button(() => GenerateUniverseBridge())
            {
                text = "SUBSTANTIATE COSMIC BRIDGE",
                style = { height = 50, backgroundColor = new Color(0.1f, 0.6f, 0.2f), color = Color.white, unityFontStyleAndWeight = FontStyle.Bold }
            };
            root.Add(forgeBtn);

            return root;
        }

        private void GenerateUniverseBridge()
        {
            // Logic to create a 'UniverseManager' MonoBehaviour in the scene
            // that handles the RenderTexture and the 'Diesel Generator' dispatch.
            Debug.Log("<color=cyan><b>COSMOS:</b></color> Projection sphere established at radius " + _sphereRadius);
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }

        private enum TextureSize { Res_1024 = 1024, Res_2048 = 2048, Res_4096 = 4096 }
    }
}