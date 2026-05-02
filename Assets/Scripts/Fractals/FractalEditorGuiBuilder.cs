using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using TheSingularityWorkshop.FSM_API;

// --- 1. Isolate the Editor namespaces ---
#if UNITY_EDITOR
using UnityEditor.UIElements;
using UnityEditor;
#endif

namespace Assets.Scripts.Fractals
{
    public class FractalEditorGuiBuilder : IGuiProvider
    {
        public string Title => "FRACTAL RENDER ENGINE";

        private RenderTexture _renderTexture;
        private FractalForgeContext _fractalContext;
        private TexturePreviewBuilder _texturePreviewBuilder;

        public FractalEditorGuiBuilder(FractalForgeContext context)
        {
            _fractalContext = context ?? new FractalForgeContext();
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            Debug.Log("[FractalBuilder] Booting Render Engine...");

            // 1. Guaranteed GPU Memory Allocation
            _renderTexture = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGBFloat)
            {
                enableRandomWrite = true,
                filterMode = FilterMode.Bilinear,
                name = "Fractal_RT"
            };
            _renderTexture.Create();

            _texturePreviewBuilder = new TexturePreviewBuilder(_renderTexture)
                .WithZoomSensitivity(0.1f)
                .WithPanSensitivity(2.0f);

            // --- 2. Fix the AssetDatabase Trap ---
            ComputeShader fractalShader = null;
#if UNITY_EDITOR
            // Editor pathway (can stay as is, but consider moving it to Resources anyway!)
            fractalShader = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/Scripts/Fractals/Fractal.compute");
#else
            // WebGL / Player Pathway: The file MUST be inside a "Resources" folder!
            fractalShader = UnityEngine.Resources.Load<ComputeShader>("Fractal");
#endif

            if (fractalShader == null) Debug.LogError("[FractalBuilder] CRITICAL: Fractal.compute NOT FOUND!");

            _fractalContext.FractalShader = fractalShader;
            _fractalContext.TargetTexture = _renderTexture;

            // Link the preview context so mouse pans/zooms directly affect the shader inputs
            _fractalContext.ViewContext = _texturePreviewBuilder.GetContext();
            _fractalContext.ViewContext.IsDirty = true; // Force first frame

            // 2. FSM Initialization
            string machineName = "FractalRenderFSM";
            string poolName = "FractalForgePool";

            if (!FSM_API.Interaction.Exists(machineName))
            {
                FSM_API.Create.CreateFiniteStateMachine(machineName, -1, poolName)
                    .State("Rendering", null, FractalRenderFSM.OnRenderTick, null)
                    .WithInitialState("Rendering")
                    .BuildDefinition();
            }

            _fractalContext.Status = FSM_API.Create.CreateInstance(machineName, _fractalContext, poolName);

            // 3. UI Composition
            VisualElement root = new VisualElement { style = { flexGrow = 1 } };

            var previewElement = _texturePreviewBuilder.CreateGui(ctx);
            previewElement.style.flexGrow = 1;
            previewElement.style.position = Position.Absolute;
            previewElement.style.top = 0;
            previewElement.style.bottom = 0;
            previewElement.style.left = 0;
            previewElement.style.right = 0;
            root.Add(previewElement);

            // The Aesthetics HUD
            var hud = new ForgeContainerBuilder("AestheticsHUD")
                .WithPosition(Position.Absolute)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.08f, 0.85f))
                .WithPadding(10)
                .WithBorderRadius(8)
                .WithBorderColor(Color.cyan).WithBorderWidth(1)
                .AddChild(new ForgeLabelBuilder("UNIVERSAL AESTHETICS")
                    .WithColor(Color.cyan).WithBold().WithMarginBottom(5))
                .OnBuild(ve =>
                {
                    // --- 3. Fix the ColorField Editor Leak ---
#if UNITY_EDITOR
                    var colorAField = new ColorField("Void / Core Color") { value = _fractalContext.ColorA };
                    colorAField.RegisterValueChangedCallback(evt => {
                        _fractalContext.ColorA = evt.newValue;
                        _fractalContext.ViewContext.IsDirty = true;
                    });
                    colorAField.labelElement.style.color = Color.white;
                    ve.Add(colorAField);

                    var colorBField = new ColorField("Energy / Escape Color") { value = _fractalContext.ColorB };
                    colorBField.RegisterValueChangedCallback(evt => {
                        _fractalContext.ColorB = evt.newValue;
                        _fractalContext.ViewContext.IsDirty = true;
                    });
                    colorBField.labelElement.style.color = Color.white;
                    ve.Add(colorBField);
#else
                    // Fallback for WebGL since ColorField is an Editor-only control
                    ve.Add(new Label("Color editing requires a custom runtime picker.") { style = { color = Color.gray, fontSize = 10 } });
#endif
                })
                .CreateGui(ctx);

            root.Add(hud);

            // 4. The Critical Tick Hook
            root.RegisterCallback<AttachToPanelEvent>(evt =>
            {
                root.schedule.Execute(() =>
                {
                    try { FSM_API.Interaction.Update(poolName); }
                    catch (Exception ex) { Debug.LogError($"[FractalTickError] {ex.Message}"); }
                }).Every(16); // ~60fps
            });

            root.RegisterCallback<DetachFromPanelEvent>(evt => Cleanup());

            return root;
        }

        private void Cleanup()
        {
            if (_fractalContext?.Status != null) FSM_API.Interaction.DestroyInstance(_fractalContext.Status);
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