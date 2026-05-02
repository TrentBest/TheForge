using TheSingularityWorkshop.MicroPackages.Providers;
using UnityEngine;
using Workshop.Core.Math;
using Workshop.Systems.MicroPackages;

// Assuming these are your implementation namespaces for the Providers

namespace Workshop.UI_And_Tools.Showcase
{
    public static class CosmicDescentFsmBuilder
    {
        /// <summary>
        /// Compiles the Cinematic Director FSM into a standardized MicroPackage 
        /// ready for the Gondola transport and global Arbitrator.
        /// </summary>
        public static DynamicMicroPackage BuildDirectorPackage()
        {
            var packageBuilder = new MicroPackageBuilder();
            packageBuilder.WithPackageId("com.singularity.showcase.cosmicdescent");

            // 1. Define the core FSM Provider
            var fsmProvider = new FsmProvider
            {
                FsmName = "CosmicDescentDirector",
                InitialState = "ComputeLightContribution"
            };
            packageBuilder.AddProvider(fsmProvider);

            // ==========================================
            // STATE: COMPUTE GPU LIGHT
            // ==========================================
            packageBuilder.AddProvider(new FsmStateProvider("CosmicDescentDirector", "ComputeLightContribution"));
            packageBuilder.AddProvider(new FsmStateOnEnterActionProvider("CosmicDescentDirector", "ComputeLightContribution", ctx =>
            {
                var context = ctx as CosmicDescentContext;
                if (context == null) return;

                context.IsBufferReady = false;
                context.DiveProgress = 0f;
                context.IsCameraAtThreshold = false;
                context.IsAligned = false;

                // Simulated Raycast to Compute Shader
                context.BrightestPointVector = Random.onUnitSphere;
                context.IsBufferReady = true;
            }));

            // Transition -> Align
            packageBuilder.AddProvider(new FsmTransitionProvider("CosmicDescentDirector", "ComputeLightContribution", "AlignToBrightestPoint"));
            packageBuilder.AddProvider(new FsmStateConditionProvider("CosmicDescentDirector", "ComputeLightContribution", "AlignToBrightestPoint",
              ctx => ((CosmicDescentContext)ctx).IsBufferReady));

            // ==========================================
            // STATE: ALIGN (Rotating the LMPB)
            // ==========================================
            packageBuilder.AddProvider(new FsmStateProvider("CosmicDescentDirector", "AlignToBrightestPoint"));
            packageBuilder.AddProvider(new FsmStateOnEnterActionProvider("CosmicDescentDirector", "AlignToBrightestPoint", ctx =>
            {
                var context = ctx as CosmicDescentContext;
                if (context?.LmpbContext != null)
                {
                    context.StartRotation = context.LmpbContext.CurrentCameraRotation;
                    context.TargetRotation = Quaternion.LookRotation(-context.BrightestPointVector);
                }
            }));

            packageBuilder.AddProvider(new FsmStateOnUpdateActionProvider("CosmicDescentDirector", "AlignToBrightestPoint", ctx =>
            {
                var context = ctx as CosmicDescentContext;
                if (context == null) return;

                context.DiveProgress += Time.deltaTime * 0.5f;

                if (context.LmpbContext != null)
                {
                    context.LmpbContext.CurrentCameraRotation = Cinemathematics.EaseOutRotation(
                      context.StartRotation,
                      context.TargetRotation,
                      Mathf.Clamp01(context.DiveProgress));
                }

                if (context.DiveProgress >= 1.0f)
                {
                    context.DiveProgress = 0f;
                    context.IsAligned = true;
                }
            }));

            // Transition -> Dive
            packageBuilder.AddProvider(new FsmTransitionProvider("CosmicDescentDirector", "AlignToBrightestPoint", "NonLinearDive"));
            packageBuilder.AddProvider(new FsmStateConditionProvider("CosmicDescentDirector", "AlignToBrightestPoint", "NonLinearDive",
              ctx => ((CosmicDescentContext)ctx).IsAligned));

            // ==========================================
            // STATE: THE DIVE (-x^2 Easing)
            // ==========================================
            packageBuilder.AddProvider(new FsmStateProvider("CosmicDescentDirector", "NonLinearDive"));
            packageBuilder.AddProvider(new FsmStateOnUpdateActionProvider("CosmicDescentDirector", "NonLinearDive", ctx =>
            {
                var context = ctx as CosmicDescentContext;
                if (context == null) return;

                context.DiveProgress += Time.deltaTime * 0.3f;
                float curveMult = Cinemathematics.InverseQuadraticDive(context.DiveProgress);
                float currentDist = Mathf.Lerp(context.TargetCameraDistance, context.StartCameraDistance, curveMult);

                if (context.LmpbContext != null)
                {
                    context.LmpbContext.CameraDistance = currentDist;
                }

                if (context.DiveProgress >= 1.0f)
                {
                    context.IsCameraAtThreshold = true;
                }
            }));

            // Transition -> Evaluate Handoff
            packageBuilder.AddProvider(new FsmTransitionProvider("CosmicDescentDirector", "NonLinearDive", "EvaluateScaleHandoff"));
            packageBuilder.AddProvider(new FsmStateConditionProvider("CosmicDescentDirector", "NonLinearDive", "EvaluateScaleHandoff",
              ctx => ((CosmicDescentContext)ctx).IsCameraAtThreshold));

            // ==========================================
            // STATE: FRACTAL RECURSION OR HALT
            // ==========================================
            packageBuilder.AddProvider(new FsmStateProvider("CosmicDescentDirector", "EvaluateScaleHandoff"));
            packageBuilder.AddProvider(new FsmStateOnEnterActionProvider("CosmicDescentDirector", "EvaluateScaleHandoff", ctx =>
            {
                var context = ctx as CosmicDescentContext;
                if (context == null) return;

                context.CurrentScaleLevel++;
                context.StartCameraDistance = context.TargetCameraDistance;
                context.TargetCameraDistance *= 0.1f;
            }));

            // Branching Transitions
            packageBuilder.AddProvider(new FsmTransitionProvider("CosmicDescentDirector", "EvaluateScaleHandoff", "ComputeLightContribution"));
            packageBuilder.AddProvider(new FsmStateConditionProvider("CosmicDescentDirector", "EvaluateScaleHandoff", "ComputeLightContribution",
              ctx => !((CosmicDescentContext)ctx).IsAtStarSystem));

            packageBuilder.AddProvider(new FsmTransitionProvider("CosmicDescentDirector", "EvaluateScaleHandoff", "SequenceComplete"));
            packageBuilder.AddProvider(new FsmStateConditionProvider("CosmicDescentDirector", "EvaluateScaleHandoff", "SequenceComplete",
              ctx => ((CosmicDescentContext)ctx).IsAtStarSystem));

            // ==========================================
            // STATE: COMPLETE
            // ==========================================
            packageBuilder.AddProvider(new FsmStateProvider("CosmicDescentDirector", "SequenceComplete"));
            packageBuilder.AddProvider(new FsmStateOnEnterActionProvider("CosmicDescentDirector", "SequenceComplete", ctx =>
            {
                Debug.Log("STAR SYSTEM REACHED. MONIKER ENGAGED.");
            }));

            // Compile the package
            return (DynamicMicroPackage)packageBuilder.Build();
        }
    }
}
