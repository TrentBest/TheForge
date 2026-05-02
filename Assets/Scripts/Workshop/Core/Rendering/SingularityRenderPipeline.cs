// File: Assets/Scripts/Workshop/Core/Rendering/SingularityRenderPipeline.cs
using System.Collections.Generic;
using System.Linq;
using TheSingularityWorkshop.FSM_API;
using UnityEngine;
using UnityEngine.Rendering;
using Workshop.Core.Diagnostics;
using Workshop.Core.Memory;
using Workshop.MicroPackages.Providers;

namespace Workshop.Core.Rendering
{
    public class SingularityRenderPipeline : RenderPipeline, IStateContext
    {
        private CommandBuffer _cmd;
        private List<ISingularityDrawPass> _drawPasses = new List<ISingularityDrawPass>();
        private bool _isInitialized = false;
        private bool _hasLoggedFirstFrame = false;

        public bool IsValid { get; set; } = true;
        public string Name { get; set; } = "Sovereign_Render_Pipeline";

        public SingularityRenderPipeline()
        {
            // Pipeline is now blind. It holds no assets.
        }

        public void RegisterDrawPass(ISingularityDrawPass pass)
        {
            if (!_drawPasses.Contains(pass))
            {
                _drawPasses.Add(pass);
                _drawPasses = _drawPasses.OrderBy(p => p.RenderQueue).ToList();
                ForgeLogger.Log($"Render Pass Injected: {pass.GetType().Name}").WithHeader("Pipeline").WithColor(Color.green).SendToUnity();
            }
        }

        private void EnsureInitialized()
        {
            if (_isInitialized) return;
            _cmd = new CommandBuffer { name = "Singularity_FSM_Render" };
            _isInitialized = true;
        }

        protected override void Render(ScriptableRenderContext context, List<Camera> cameras)
        {
            if (SingularityBootloader.AppContext != null && SingularityBootloader.AppContext.Pipeline == null)
            {
                SingularityBootloader.FinalizeBootSequence(this);
            }
            if (!_hasLoggedFirstFrame)
            {
                ForgeLogger.Log("PIPELINE IGNITION: Unity successfully called Render(). SRP is active.")
                    .WithHeader("Pipeline").WithColor(Color.yellow).SendToUnity();
                _hasLoggedFirstFrame = true;
            }

            EnsureInitialized();

            // FIX: Grab the warehouse from the active AppContext
            DataWarehouse warehouse = null;
            if (SingularityBootloader.AppContext != null)
            {
                warehouse = SingularityBootloader.AppContext.Warehouse;
            }
            // Fallback during initial boot tick before AppContext is assigned
            else
            {
                warehouse = SingularityBootloader.MainWarehouse;
            }

            foreach (var camera in cameras)
            {
                BeginCameraRendering(context, camera);
                context.SetupCameraProperties(camera);

                _cmd.Clear();
                _cmd.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
                _cmd.ClearRenderTarget(true, true, new Color(0.02f, 0.02f, 0.03f)); // A very dark void

                // --- INJECT GLOBAL LIGHTING FOR THE PIPELINE ---
                // A dramatic light coming from the top-right, pointing slightly down and into the screen
                Vector4 sunDirection = new Vector4(0.5f, -0.8f, 0.5f, 0.0f).normalized;
                _cmd.SetGlobalVector("_WorldSpaceLightPos0", -sunDirection); // Unity shaders expect light vector pointing TOWARDS the light
                _cmd.SetGlobalColor("_LightColor0", Color.white * 1.5f);     // Bright intensity
                _cmd.SetGlobalColor("unity_AmbientSky", new Color(0.1f, 0.1f, 0.15f)); // Subtle dark blue ambient
                                                                                       // -----------------------------------------------

                context.ExecuteCommandBuffer(_cmd);
                _cmd.Clear();

                if (warehouse == null || warehouse.IsDisposed)
                {
                    context.Submit();
                    EndCameraRendering(context, camera);
                    continue;
                }

                // 1. Extract Perspective
                var observers = warehouse.GetShelf<WorldObserver>();
                if (observers == null || !observers.IsActive(0))
                {
                    context.Submit();
                    EndCameraRendering(context, camera);
                    continue;
                }

                ref var observer = ref observers.GetRef(0);
                Matrix4x4 view = observer.ViewMatrix;
                Matrix4x4 proj = observer.ProjectionMatrix;
                if (camera.cameraType == CameraType.SceneView)
                {
                    view = camera.worldToCameraMatrix;
                    proj = camera.projectionMatrix;
                }
                else if (view.m00 == 0 && view.m11 == 0) // Fallback for uninitialized observer
                {
                    view = camera.worldToCameraMatrix;
                    proj = camera.projectionMatrix;
                }

                _cmd.SetViewProjectionMatrices(view, proj);

                // 2. ORCHESTRATE THE MICRO-PACKAGES
                foreach (var pass in _drawPasses)
                {
                    pass.ExecuteDraw(_cmd, warehouse, view, proj);
                }

                context.ExecuteCommandBuffer(_cmd);
                _cmd.Clear();
                context.Submit();
                EndCameraRendering(context, camera);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (_isInitialized)
            {
                _cmd?.Release();
                _isInitialized = false;
            }

            foreach (var pass in _drawPasses)
            {
                pass.Dispose();
            }
        }
    }
}