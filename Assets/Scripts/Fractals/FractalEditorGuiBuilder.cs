using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements; // For ColorField and IntegerField
using TheSingularityWorkshop.FSM_API;
using UnityEditor;

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders
{
    public class FractalEditorGuiBuilder : IGuiProvider
    {
        public string Title => "MANDELBROT FORGE";

        private RenderTexture _renderTexture;
        private FractalForgeContext _fractalContext;
        private TexturePreviewBuilder _texturePreviewBuilder;

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new VisualElement { style = { flexGrow = 1, flexDirection = FlexDirection.Row, backgroundColor = new Color(0.12f, 0.12f, 0.15f) } };

            // 1. Initialize GPU Memory (The Canvas)
            _renderTexture = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGBFloat)
            {
                enableRandomWrite = true,
                filterMode = FilterMode.Bilinear
            };
            _renderTexture.Create();

            // 2. Initialize the 2D Viewport Builder
            _texturePreviewBuilder = new TexturePreviewBuilder(_renderTexture)
                .WithZoomSensitivity(0.1f)
                .WithPanSensitivity(2.0f);

            // 3. Load the Compute Shader (Adjust path as necessary for your project)
            var fractalShader = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/Scripts/Fractals/Fractal.compute");
            if (fractalShader == null)
            {
                Debug.LogError("FractalForge: Could not find Fractal.compute! Please check the path.");
            }

            // 4. Setup the FSM Context
            _fractalContext = new FractalForgeContext
            {
                Name = "FractalContext_" + Guid.NewGuid().ToString(),
                FractalShader = fractalShader,
                TargetTexture = _renderTexture,
                ViewContext = _texturePreviewBuilder.GetContext(),
                MaxIterations = 100,
                ColorA = Color.cyan,
                ColorB = Color.blue
            };

            // 5. Spin up the FSM
            string machineName = "FractalRenderFSM";
            string poolName = "FractalForgePool";

            if ( !FSM_API.FSM_API.Interaction.Exists(machineName))
            {
                FSM_API.FSM_API.Create.CreateFiniteStateMachine(machineName, -1, poolName)
                    .State("Idle", null, null, null)
                    .State("Rendering", null, FractalRenderFSM.OnRenderTick, null)
                    .Transition("Idle", "Rendering", c => true)
                    .BuildDefinition();
            }

            FSM_API.FSM_API.Create.CreateInstance(machineName, _fractalContext, poolName);

            // --- UI LAYOUT ---

            // Left Panel: Controls
            var sidebar = new VisualElement { style = { width = 300, paddingBottom = 15, paddingTop = 15, paddingLeft = 15, paddingRight = 15, borderRightWidth = 2, borderRightColor = Color.cyan, backgroundColor = new Color(0.1f, 0.1f, 0.1f) } };

            sidebar.Add(new Label("FRACTAL PARAMETERS") { style = { fontSize = 18, color = Color.white, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 20 } });

            var iterField = new IntegerField("Max Iterations") { value = _fractalContext.MaxIterations };
            iterField.RegisterValueChangedCallback(evt => {
                _fractalContext.MaxIterations = evt.newValue;
                _fractalContext.ViewContext.IsDirty = true; // Tell FSM to re-render
            });
            sidebar.Add(iterField);

            var colorAField = new ColorField("Deep Color (A)") { value = _fractalContext.ColorA };
            colorAField.RegisterValueChangedCallback(evt => {
                _fractalContext.ColorA = evt.newValue;
                _fractalContext.ViewContext.IsDirty = true;
            });
            sidebar.Add(colorAField);

            var colorBField = new ColorField("Edge Color (B)") { value = _fractalContext.ColorB };
            colorBField.RegisterValueChangedCallback(evt => {
                _fractalContext.ColorB = evt.newValue;
                _fractalContext.ViewContext.IsDirty = true;
            });
            sidebar.Add(colorBField);

            var resetBtn = new Button(() => {
                _fractalContext.ViewContext.ZoomLevel = 1.0f;
                _fractalContext.ViewContext.PanOffset = Vector2.zero;
                _fractalContext.ViewContext.IsDirty = true;
            })
            { text = "Reset View", style = { marginTop = 20, height = 30, backgroundColor = new Color(0.3f, 0.3f, 0.3f) } };
            sidebar.Add(resetBtn);

            root.Add(sidebar);

            // Right Panel: Viewport
            var viewport = new VisualElement { style = { flexGrow = 1 } };
            viewport.Add(_texturePreviewBuilder.CreateGui(ctx)); // Inject our 2D builder!
            root.Add(viewport);

            // Schedule the FSM Tick
            root.schedule.Execute(() => {
                FSM_API.FSM_API.Interaction.Update(poolName);
            }).Every(16); // ~60fps checking

            // Cleanup when the tab is closed!
            root.RegisterCallback<DetachFromPanelEvent>(evt => Cleanup());

            // Initial Kickoff
            _fractalContext.ViewContext.IsDirty = true;

            return root;
        }

        private void Cleanup()
        {
            if (_renderTexture != null)
            {
                _renderTexture.Release();
                UnityEngine.Object.DestroyImmediate(_renderTexture);
            }
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}