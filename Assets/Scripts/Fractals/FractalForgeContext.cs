using UnityEngine;
using TheSingularityWorkshop.FSM_API;

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders
{

    

    public class FractalForgeContext : IStateContext
    {
        public ComputeShader FractalShader;
        public RenderTexture TargetTexture;
        public TexturePreviewContext ViewContext;

        public int MaxIterations = 100;
        public Color ColorA = Color.cyan;
        public Color ColorB = Color.blue;

        public bool IsValid { get; set; } = false;
        public string Name { get; set; }

        // Tracks the instance handle so we can cleanly destroy it later
        public FSMHandle Status { get; set; }
    }


    public static class FractalRenderFSM
    {
        public static void OnRenderTick(object context)
        {
            if (!(context is FractalForgeContext ctx)) return;
            if (ctx.TargetTexture == null || ctx.FractalShader == null) return;

            // Only dispatch if the user is panning/zooming, or parameters changed
            if (!ctx.ViewContext.IsDirty) return;

            int kernel = ctx.FractalShader.FindKernel("CSMain");

            // Update GPU Uniforms
            ctx.FractalShader.SetTexture(kernel, "Result", ctx.TargetTexture);
            ctx.FractalShader.SetVector("_PanOffset", ctx.ViewContext.PanOffset);
            ctx.FractalShader.SetFloat("_Zoom", ctx.ViewContext.ZoomLevel);
            ctx.FractalShader.SetInt("_MaxIterations", ctx.MaxIterations);
            ctx.FractalShader.SetInt("_ResolutionX", ctx.TargetTexture.width);
            ctx.FractalShader.SetInt("_ResolutionY", ctx.TargetTexture.height);
            ctx.FractalShader.SetVector("_ColorA", ctx.ColorA);
            ctx.FractalShader.SetVector("_ColorB", ctx.ColorB);

            // Dispatch
            int threadGroupsX = Mathf.CeilToInt(ctx.TargetTexture.width / 8.0f);
            int threadGroupsY = Mathf.CeilToInt(ctx.TargetTexture.height / 8.0f);
            ctx.FractalShader.Dispatch(kernel, threadGroupsX, threadGroupsY, 1);

            // Mark clean so we don't cook the GPU when sitting idle!
            ctx.ViewContext.IsDirty = false;
        }
    }
}