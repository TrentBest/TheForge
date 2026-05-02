using TheSingularityWorkshop.FSM_API;
using UnityEngine;
using Workshop.Core.Diagnostics;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.Fractals
{
    public enum FractalFormula { Mandelbrot = 0, Julia = 1, BurningShip = 2, Tricorn = 3 }

    public class FractalForgeContext : IStateContext
    {
        public ComputeShader FractalShader;
        public RenderTexture TargetTexture;
        public TexturePreviewContext ViewContext; // Assuming this exists from your TexturePreviewBuilder

        // Expanded Math
        public FractalFormula CurrentFormula = FractalFormula.Mandelbrot;
        public float Power = 2.0f;
        public Vector2 JuliaConstant = new Vector2(-0.7f, 0.27015f);
        public int MaxIterations = 200;

        // Expanded Aesthetics
        public Color ColorA = new Color(0.0f, 0.0f, 0.1f); // Deep Space Blue
        public Color ColorB = new Color(0.0f, 1.0f, 1.0f); // Cyan Core
        public float ColorStretch = 0.45f;

        // Diagnostics
        public bool HasDispatchedOnce = false;
        public bool IsValid { get; set; } = true;
        public string Name { get; set; }
        public FSMHandle Status { get; set; }
    }

    public static class FractalRenderFSM
    {
        public static void OnRenderTick(object context)
        {
            if (!(context is FractalForgeContext ctx)) return;

            // --- AGGRESSIVE TELEMETRY & SAFETY CHECKS ---
            if (ctx.FractalShader == null) { ForgeLogger.LogError("[FractalFSM] Shader is missing!"); return; }
            if (ctx.TargetTexture == null) { ForgeLogger.LogError("[FractalFSM] TargetTexture is missing!"); return; }
            if (ctx.ViewContext == null) { ForgeLogger.LogError("[FractalFSM] ViewContext is missing!"); return; }

            if (!ctx.TargetTexture.IsCreated())
            {
                ForgeLogger.LogWarning("[FractalFSM] RenderTexture was lost. Recreating...");
                ctx.TargetTexture.Create();
            }

            // Only dispatch if dirty to prevent GPU melting
            if (!ctx.ViewContext.IsDirty && ctx.HasDispatchedOnce) return;

            int kernel = ctx.FractalShader.FindKernel("CSMain");
            if (kernel < 0) { ForgeLogger.LogError("[FractalFSM] Could not find kernel 'CSMain'."); return; }

            // Uniform Binding
            ctx.FractalShader.SetTexture(kernel, "Result", ctx.TargetTexture);
            ctx.FractalShader.SetVector("_PanOffset", ctx.ViewContext.PanOffset);
            ctx.FractalShader.SetFloat("_Zoom", ctx.ViewContext.ZoomLevel);
            ctx.FractalShader.SetInt("_MaxIterations", ctx.MaxIterations);
            ctx.FractalShader.SetInt("_ResolutionX", ctx.TargetTexture.width);
            ctx.FractalShader.SetInt("_ResolutionY", ctx.TargetTexture.height);

            ctx.FractalShader.SetVector("_ColorA", ctx.ColorA);
            ctx.FractalShader.SetVector("_ColorB", ctx.ColorB);
            ctx.FractalShader.SetFloat("_ColorStretch", ctx.ColorStretch);

            ctx.FractalShader.SetInt("_FractalType", (int)ctx.CurrentFormula);
            ctx.FractalShader.SetFloat("_Power", ctx.Power);
            ctx.FractalShader.SetVector("_JuliaConstant", ctx.JuliaConstant);

            int threadGroupsX = Mathf.CeilToInt(ctx.TargetTexture.width / 8.0f);
            int threadGroupsY = Mathf.CeilToInt(ctx.TargetTexture.height / 8.0f);

            // Log the first dispatch so we know the math actually ran
            if (!ctx.HasDispatchedOnce)
            {
                ForgeLogger.Log($"[FractalFSM] First Dispatch! Res: {ctx.TargetTexture.width}x{ctx.TargetTexture.height}, Threads: {threadGroupsX}x{threadGroupsY}");
                ctx.HasDispatchedOnce = true;
            }

            ctx.FractalShader.Dispatch(kernel, threadGroupsX, threadGroupsY, 1);
            ctx.ViewContext.IsDirty = false;
        }
    }
}