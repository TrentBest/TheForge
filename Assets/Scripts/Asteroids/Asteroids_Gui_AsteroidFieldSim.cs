using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.Asteroids
{
    public class Asteroids_Gui_AsteroidFieldSim : IGuiProvider
    {
        public string Title => "ASTEROID FIELD SIMULATOR";

        private GameObject _simRoot;
        private VisualElement _rootElement;
        private GuiContext _guiContext;
        private List<SimAsteroid> _asteroids = new List<SimAsteroid>();

        private class SimAsteroid
        {
            public Transform Transform;
            public Vector3 Velocity;
            public Vector3 RotationSpeed;
        }

        // Required Parameterless Constructor for Editor Reflection
        public Asteroids_Gui_AsteroidFieldSim() { }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _guiContext = ctx;
            _rootElement = new VisualElement { style = { flexGrow = 1, overflow = Overflow.Hidden } };

            try
            {
                // 1. Setup the isolated 3D Scene
                _simRoot = new GameObject("[ASTEROID_FIELD_SIM_ROOT]");
                _simRoot.transform.position = new Vector3(0, -10000, 0); // Hide far below the world

                // Determine pipeline-safe material safely
                var astMat = new Material(Shader.Find("Standard"));
                astMat.color = new Color(0.2f, 0.2f, 0.25f); // Deep rocky grey

                if (UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null)
                {
                    var urpShader = Shader.Find("Universal Render Pipeline/Lit");
                    if (urpShader != null)
                    {
                        astMat = new Material(urpShader) { color = new Color(0.2f, 0.2f, 0.25f) };
                    }
                }

                System.Random rand = new System.Random();

                // Generate Asteroids
                for (int i = 0; i < 35; i++)
                {
                    var ast = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    ast.transform.SetParent(_simRoot.transform);

                    // Random position within a bounding box
                    ast.transform.localPosition = new Vector3(
                        (float)(rand.NextDouble() * 60 - 30),
                        (float)(rand.NextDouble() * 40 - 20),
                        (float)(rand.NextDouble() * 20 - 10)
                    );

                    // Uneven scaling creates crude, asteroid-like potatoes from spheres
                    ast.transform.localScale = new Vector3(
                        (float)(rand.NextDouble() * 2 + 0.5),
                        (float)(rand.NextDouble() * 2 + 0.5),
                        (float)(rand.NextDouble() * 2 + 0.5)
                    );

                    if (ast.TryGetComponent<Renderer>(out var rend))
                    {
                        // Use sharedMaterial to avoid memory leaks on primitive instances
                        rend.sharedMaterial = astMat;
                    }

                    _asteroids.Add(new SimAsteroid
                    {
                        Transform = ast.transform,
                        Velocity = new Vector3(
                            (float)(rand.NextDouble() * 8 - 4),
                            (float)(rand.NextDouble() * 8 - 4),
                            (float)(rand.NextDouble() * 4 - 2)
                        ),
                        RotationSpeed = new Vector3(
                            (float)(rand.NextDouble() * 90 - 45),
                            (float)(rand.NextDouble() * 90 - 45),
                            (float)(rand.NextDouble() * 90 - 45)
                        )
                    });
                }

                // 2. Capture the Scene with LiveModelPreview
                var preview = new LiveModelPreviewBuilder(_simRoot)
                    .WithZoom(40f)
                    .WithPitch(0f)
                    .WithYaw(0f)
                    .WithMouseControl(false) // Purely visual background
                    .WithBackgroundColor(new Color(0.02f, 0.01f, 0.05f)) // Deep Nebula Purple/Black
                    .Build();

                preview.style.position = Position.Absolute;
                preview.style.left = 0; preview.style.top = 0; preview.style.right = 0; preview.style.bottom = 0;

                _rootElement.Add(preview);

                // 3. Drive the Simulation safely ONLY after attaching to panel
                _rootElement.RegisterCallback<AttachToPanelEvent>(e => {
                    _rootElement.schedule.Execute(UpdateSimulation).Every(16);
                });

                // 4. Memory Management: Obliterate the 3D scene when the UI closes
                _rootElement.RegisterCallback<DetachFromPanelEvent>(e => {
                    if (_simRoot != null) UnityEngine.Object.DestroyImmediate(_simRoot);
                });
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AsteroidFieldSim] Initialization failed: {ex.Message}");
                // Return an empty element so the parent layout doesn't break
            }

            return _rootElement;
        }

        private void UpdateSimulation()
        {
            float dt = 0.016f;
            float boundX = 35f;
            float boundY = 25f;

            foreach (var ast in _asteroids)
            {
                if (ast.Transform == null) continue;

                // Newtonian Integration
                ast.Transform.localPosition += ast.Velocity * dt;
                ast.Transform.Rotate(ast.RotationSpeed * dt);

                // Screen Wrap
                Vector3 pos = ast.Transform.localPosition;
                if (pos.x > boundX) pos.x = -boundX;
                else if (pos.x < -boundX) pos.x = boundX;

                if (pos.y > boundY) pos.y = -boundY;
                else if (pos.y < -boundY) pos.y = boundY;

                ast.Transform.localPosition = pos;
            }
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
#if UNITY_EDITOR
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
#endif
        public void FromUIDocument(string assetPath) => _guiContext?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
    }
}