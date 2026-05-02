// File: Assets/Scripts/Workshop/Core/Rendering/SingularityPipelineAsset.cs
using UnityEngine;
using UnityEngine.Rendering;

namespace Workshop.Core.Rendering
{
    [CreateAssetMenu(menuName = "Singularity/FSM Render Pipeline Asset")]
    public class SingularityPipelineAsset : RenderPipelineAsset<SingularityRenderPipeline>
    {
        public override string renderPipelineShaderTag => "Singularity";

        protected override RenderPipeline CreatePipeline()
        {
            // Removed the mesh/material arguments. The pipeline is now blind!
            return new SingularityRenderPipeline();
        }
    }
}