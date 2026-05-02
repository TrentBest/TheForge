using System;
using TheSingularityWorkshop.FSM_API;
using UnityEngine;

namespace Workshop.Asteroids
{
    public class FighterPreviewFSM : MonoBehaviour, IStateContext
    {
        public bool IsValid { get; set; } = true;
        public string Name { get; set; } = "FighterPreview";

        public bool IsInteracting { get; set; } = false;
        public FSMHandle Status { get; private set; }

        public double LastTime;
        public float Speed = 60f; // Degrees per second
        public float Timer = 0f;

        public int CurrentTargetIndex = 0;

        // The 9 exact 90-degree orthogonal showcase views
        public Quaternion[] ShowcaseAngles = new Quaternion[] {
            Quaternion.Euler(-90, 0, 0),   // 1. Top-Down (Nose Up)
            Quaternion.Euler(-90, 90, 0),  // 2. Spin 90 (Nose Right, still looking at roof)
            Quaternion.Euler(0, 90, 0),    // 3. Pitch 90 (Side Profile, Nose Right)
            Quaternion.Euler(0, 180, 0),   // 4. Yaw 90 (Front View, Nose pointing at Player)
            Quaternion.Euler(0, 270, 0),   // 5. Yaw 90 (Side Profile, Nose Left)
            Quaternion.Euler(0, 0, 0),     // 6. Yaw 90 (Rear View, Thrusters at Player)
            Quaternion.Euler(90, 0, 0),    // 7. Pitch 90 (Bottom-Up, Belly View, Nose Up)
            Quaternion.Euler(90, 270, 0),  // 8. Spin 90 (Belly View, Nose Left)
            Quaternion.Euler(90, 180, 0)   // 9. Spin 90 (Belly View, Nose Down)
        };

        public void Initialize()
        {
            // FIXED: Migrated from EditorApplication to universal runtime time tracking
            LastTime = Time.realtimeSinceStartup;
            transform.localRotation = ShowcaseAngles[0]; // Start exactly on waypoint 1

            if (!FSM_API.Interaction.Exists("FighterPreviewMode", "HangarPreview"))
            {
                FSM_API.Create.CreateFiniteStateMachine("FighterPreviewMode", -1, "HangarPreview")
                    .State("Tour_Moving", null, OnUpdateMoving, null)
                    .State("Tour_Paused", OnEnterPaused, OnUpdatePaused, null)
                    .State("Manual_Override", OnEnterManual, OnUpdateManual, null)

                    // Immediate interruption from user
                    .Transition("Tour_Moving", "Manual_Override", ctx => ((FighterPreviewFSM)ctx).IsInteracting)
                    .Transition("Tour_Paused", "Manual_Override", ctx => ((FighterPreviewFSM)ctx).IsInteracting)

                    // Moving -> Paused (When we reach the exact angle)
                    .Transition("Tour_Moving", "Tour_Paused", ctx =>
                    {
                        var fsm = (FighterPreviewFSM)ctx;
                        return Quaternion.Angle(fsm.transform.localRotation, fsm.ShowcaseAngles[fsm.CurrentTargetIndex]) < 0.5f;
                    })

                    // Paused -> Moving (After admiring the angle for 2 seconds)
                    .Transition("Tour_Paused", "Tour_Moving", ctx =>
                    {
                        var fsm = (FighterPreviewFSM)ctx;
                        if (fsm.Timer > 2.0f)
                        {
                            fsm.CurrentTargetIndex = (fsm.CurrentTargetIndex + 1) % fsm.ShowcaseAngles.Length;
                            return true;
                        }
                        return false;
                    })

                    // Manual -> Moving (Wait 2.5 seconds after they let go, then resume tour)
                    .Transition("Manual_Override", "Tour_Moving", ctx =>
                    {
                        var fsm = (FighterPreviewFSM)ctx;
                        return !fsm.IsInteracting && fsm.Timer > 2.5f;
                    })
                    .BuildDefinition();
            }

            Status = FSM_API.Create.CreateInstance("FighterPreviewMode", this, "HangarPreview");
        }

        private static float GetDeltaTime(FighterPreviewFSM fsm)
        {
            // FIXED: Safe for both Editor and Runtime
            double currentTime = Time.realtimeSinceStartup;
            float dt = (float)(currentTime - fsm.LastTime);
            fsm.LastTime = currentTime;
            return dt;
        }

        // --- STATE BEHAVIORS ---

        private static void OnUpdateMoving(IStateContext context)
        {
            var fsm = (FighterPreviewFSM)context;
            float dt = GetDeltaTime(fsm);

            // This specifically cures the "Wonky Basis" by calculating the absolute shortest path 
            // between where the ship is currently resting, and the exact absolute target angle.
            fsm.transform.localRotation = Quaternion.RotateTowards(
                fsm.transform.localRotation,
                fsm.ShowcaseAngles[fsm.CurrentTargetIndex],
                fsm.Speed * dt
            );
        }

        private static void OnEnterPaused(IStateContext context)
        {
            var fsm = (FighterPreviewFSM)context;
            fsm.Timer = 0f;

            // Snap to perfection to eliminate floating point rounding errors
            fsm.transform.localRotation = fsm.ShowcaseAngles[fsm.CurrentTargetIndex];
        }

        private static void OnUpdatePaused(IStateContext context)
        {
            var fsm = (FighterPreviewFSM)context;
            fsm.Timer += GetDeltaTime(fsm);
        }

        private static void OnEnterManual(IStateContext context)
        {
            var fsm = (FighterPreviewFSM)context;
            fsm.Timer = 0f;
        }

        private static void OnUpdateManual(IStateContext context)
        {
            var fsm = (FighterPreviewFSM)context;
            float dt = GetDeltaTime(fsm);

            if (!fsm.IsInteracting)
            {
                // Start ticking the timer once the user lets go of the mouse
                fsm.Timer += dt;
            }
            else
            {
                // Reset timer as long as they are holding it
                fsm.Timer = 0f;
            }
        }
    }
}