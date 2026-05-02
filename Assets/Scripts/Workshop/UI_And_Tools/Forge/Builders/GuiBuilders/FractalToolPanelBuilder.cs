using System;
using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.FSM_API;
using Assets.Scripts.Fractals;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    public class FractalToolPanelBuilder : IGuiProvider
    {
        public string Title => "Fractal Generator Tool";

        private FractalForgeContext _fractalContext;
        private RenderTexture _targetImageLayer;
        private bool _ownsTexture = false; // Tracks if we created a temporary texture

        // Default Constructor for the Singularity Ecosystem / Catalog
        public FractalToolPanelBuilder() { }

        // The Image Editor passes the layer it wants us to draw on
        public FractalToolPanelBuilder(RenderTexture targetLayer)
        {
            _targetImageLayer = targetLayer;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var container = new VisualElement { style = { paddingBottom = 10, paddingTop = 10, flexDirection = FlexDirection.Column } };

            var title = new Label("MANDELBROT GENERATOR") { style = { color = Color.cyan, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } };
            container.Add(title);

            // 1. SELF-HEALING FALLBACK: If spawned via default constructor, make a temp texture and preview
            if (_targetImageLayer == null)
            {
                _targetImageLayer = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGBFloat) { enableRandomWrite = true };
                _targetImageLayer.Create();
                _ownsTexture = true; // Flag it so we know to destroy it later!

                var standalonePreview = new Image
                {
                    image = _targetImageLayer,
                    style = { width = Length.Percent(100), height = 250, marginBottom = 15, backgroundColor = Color.black }
                };
                container.Add(standalonePreview);
            }

            // 2. Initialize Context
            // CRITICAL FIX: Ensure 'Fractal.compute' is physically located inside a 'Resources/Shaders/' folder in your Unity project!
            var fractalShader = Resources.Load<ComputeShader>("Shaders/Fractal");
            if (fractalShader == null) Debug.LogError("[FractalTool] Compute Shader not found at Resources/Shaders/Fractal.compute");

            _fractalContext = new FractalForgeContext
            {
                Name = "ImageEditor_FractalContext_" + Guid.NewGuid().ToString(),
                FractalShader = fractalShader,
                TargetTexture = _targetImageLayer,
                ViewContext = new TexturePreviewContext(),
                MaxIterations = 100,
                ColorA = Color.cyan,
                ColorB = Color.blue
            };

            // 3. Ensure FSM is running
            string machineName = "FractalRenderFSM";
            string poolName = "ImageEditorPool";

            if (!FSM_API.Interaction.Exists(machineName))
            {
                FSM_API.Create.CreateFiniteStateMachine(machineName, -1, poolName)
                    .State("Idle", null, null, null)
                    .State("Rendering", null, FractalRenderFSM.OnRenderTick, null) // Reference to your compute dispatch method
                    .Transition("Idle", "Rendering", c => true)
                    .BuildDefinition();
            }

            _fractalContext.Status = FSM_API.Create.CreateInstance(machineName, _fractalContext, poolName);

            // 4. The Tool UI
            var iterLabel = new Label($"Iterations: {_fractalContext.MaxIterations}") { style = { color = Color.white } };
            var iterSlider = new Slider(10, 500) { value = _fractalContext.MaxIterations };
            iterSlider.RegisterValueChangedCallback(evt => {
                _fractalContext.MaxIterations = (int)evt.newValue;
                iterLabel.text = $"Iterations: {_fractalContext.MaxIterations}";
                _fractalContext.ViewContext.IsDirty = true;
            });
            container.Add(iterLabel);
            container.Add(iterSlider);

            var zoomLabel = new Label("Zoom Level") { style = { color = Color.white, marginTop = 10 } };
            var zoomSlider = new Slider(0.1f, 100f) { value = 1.0f };
            zoomSlider.RegisterValueChangedCallback(evt => {
                _fractalContext.ViewContext.ZoomLevel = evt.newValue;
                _fractalContext.ViewContext.IsDirty = true;
            });
            container.Add(zoomLabel);
            container.Add(zoomSlider);

            var applyBtn = new Button(() => {
                Cleanup(); // Kill FSM
                Debug.Log("Fractal Applied to Layer!");
            })
            { text = "APPLY TO LAYER", style = { marginTop = 20, backgroundColor = new Color(0.0f, 0.4f, 0.0f), color = Color.white, unityFontStyleAndWeight = FontStyle.Bold } };
            container.Add(applyBtn);

            // 5. SAFETY CLEANUP: If the user closes this tab/panel without clicking apply
            container.RegisterCallback<DetachFromPanelEvent>(evt => Cleanup());

            // Kickoff
            _fractalContext.ViewContext.IsDirty = true;

            return container;
        }

        private void Cleanup()
        {
            // Stop the state machine from cooking the GPU
            if (_fractalContext?.Status != null)
            {
                FSM_API.Interaction.DestroyInstance(_fractalContext.Status);
                _fractalContext.Status = null;
            }

            // Only destroy the texture if we created it for standalone preview
            if (_ownsTexture && _targetImageLayer != null)
            {
                _targetImageLayer.Release();
                UnityEngine.Object.DestroyImmediate(_targetImageLayer);
                _targetImageLayer = null;
            }
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}