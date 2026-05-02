// File: Assets/Scripts/Workshop/Core/Rendering/Splash/SplashAnimatorContext.cs
using System;
using System.Diagnostics;
using UnityEngine;
using TheSingularityWorkshop.FSM_API;
using Workshop.Core.Diagnostics;
using Workshop.Core.Memory;
using Workshop.Core.Chronos;

namespace Workshop.Core.Rendering.Splash
{
    public class SplashAnimatorContext : IStateContext
    {
        public bool IsValid { get; set; } = true;
        public string Name { get; set; } = "SplashAnimatorContext";

        // INJECTED: The systemic clock that drives the FSM
        public TimeContext Chronos { get; private set; }

        public SplashAnimatorContext()
        {
            ForgeLogger.Log(">> ENTER: SplashAnimatorContext Constructor").WithHeader("Temporal").WithColor("#AAAAAA").SendToUnity();
            var sw = Stopwatch.StartNew();

            try
            {
                // Instantiate our local time clock
                Chronos = new TimeContext { Name = "Splash_Clock", LocalTimeScale = 1.0f, TotalSeconds = 0 };

                if (!FSM_API.Interaction.Exists("SplashAnimatorFSM"))
                {
                    FSM_API.Create.CreateFiniteStateMachine("SplashAnimatorFSM", -1, "Update")
                        // Enforcing zero-allocation static methods
                        .State("Oscillating", OnEnter, OnUpdate, null)
                        .BuildDefinition();
                }

                FSM_API.Create.CreateInstance("SplashAnimatorFSM", this, "Update");
            }
            finally
            {
                sw.Stop();
                ForgeLogger.Log($"<< EXIT: SplashAnimatorContext Constructor (Elapsed: {sw.ElapsedMilliseconds}ms)").WithHeader("Temporal").WithColor("#AAAAAA").SendToUnity();
            }
        }

        private static void OnEnter(IStateContext ctx)
        {
            ForgeLogger.Log(">> ENTER: SplashAnimator OnEnter").WithHeader("Temporal").WithColor("#AAAAAA").SendToUnity();
            ForgeLogger.Log("<< EXIT: SplashAnimator OnEnter").WithHeader("Temporal").WithColor("#AAAAAA").SendToUnity();
        }

        private static void OnUpdate(IStateContext ctx)
        {
            // NOTE: No Temporal Mesh here. High-frequency tick.

            // THE FIX: Cast correctly to ourselves, then access our Chronos instance
            var animCtx = (SplashAnimatorContext)ctx;
            if (animCtx == null || animCtx.Chronos == null) return;

            // 1. Tick our deterministic time state
            animCtx.Chronos.LocalDeltaTime = Time.deltaTime;
            animCtx.Chronos.TotalSeconds += (Time.deltaTime * animCtx.Chronos.LocalTimeScale);

            float t = (float)animCtx.Chronos.TotalSeconds;

            // 2. Fetch the memory block
            var shelf = SingularityBootloader.MainWarehouse?.GetShelf<SplashLetterEntity>();
            if (shelf == null) return;

            // 3. Execute the systemic animation math
            for (int i = 0; i < shelf.PageSize; i++)
            {
                if (!shelf.IsActive(i)) continue;

                ref var letter = ref shelf.GetRef(i);
                float offset = letter.LetterIndex * 0.5f;

                // SCALAR: Breathing / Pumping effect
                float scalePulse = 1.0f + (Mathf.Sin(t * 2f + offset) * 0.15f);
                letter.Scale = letter.OriginalScale * scalePulse;

                // TRANSLATOR: Floating / Orbiting effect
                float shiftX = Mathf.Sin(t * 1.5f + offset) * 0.2f;
                float shiftY = Mathf.Cos(t * 1.2f + offset) * 0.2f;
                letter.Position = letter.OriginalPosition + new Vector3(shiftX, shiftY, 0);

                // ROTATOR: The Multi-Dimensional Gears
                float spinDir = (letter.LetterIndex % 2 == 0) ? 1f : -1f;

                Quaternion tumble = Quaternion.Euler(
                    Mathf.Sin(t + offset) * 15f,
                    180f + Mathf.Cos(t + offset) * 15f,
                    t * 45f * spinDir
                );

                letter.Rotation = tumble;
            }
        }
    }
}