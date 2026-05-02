using Assets.Scripts.PacMan;
using TheSingularityWorkshop.FSM_API; // Corrected namespace
using Workshop.Systems.FSMs;
using UnityEngine;

namespace Assets.Scripts.PacMan
{
    public static class PacManFsmBuilder
    {
        public static void BuildDefinition()
        {
            // processRate: 1 (Every tick), processingGroup: "PacMan_Simulation"
            FSM_API.Create.CreateFiniteStateMachine("PacMan_Core", 1, "PacMan_Simulation")
                .State("Ready", OnEnterReady, null, null)
                .State("Playing", null, OnUpdatePlaying, null)
                .State("Energized", OnEnterEnergized, OnUpdateEnergized, OnExitEnergized)
                .State("GameOver", OnEnterGameOver, null, null)
                .BuildDefinition();
        }

        private static void OnEnterReady(IStateContext ctx)
        {
            Debug.Log("PAC-MAN: READY!");
            // Potential UI trigger for the 'Ready!' overlay
        }

        private static void OnUpdatePlaying(IStateContext ctx)
        {
            if (ctx is not PacManContext pCtx) return;

            // 1. Process Input & Navigation
            MazeNavigationProvider.ProcessPacManMovement(pCtx);

            // 2. Process Ghost Swarm Logic
            foreach (var ghost in pCtx.Ghosts)
            {
                MazeNavigationProvider.ProcessGhostMovement(pCtx, ghost);
            }

            // 3. Evaluate Consumption & Game Rules
            MazeNavigationProvider.EvaluateGameState(pCtx);
        }

        private static void OnEnterEnergized(IStateContext ctx)
        {
            if (ctx is not PacManContext pCtx) return;
            pCtx.IsEnergized = true;
            pCtx.EnergizerTimer = 10f; // 10-second duration
            PacManAcousticEngine.Play(PacManAcousticEngine.PowerUp);
            Debug.Log("POWER PELLET ENGAGED: GHOSTS ARE VULNERABLE");
        }

        private static void OnUpdateEnergized(IStateContext ctx)
        {
            if (ctx is not PacManContext pCtx) return;

            pCtx.EnergizerTimer -= Time.deltaTime;

            // Standard movement still occurs during energized state
            OnUpdatePlaying(ctx);

            if (pCtx.EnergizerTimer <= 0)
            {
                pCtx.Handle.TransitionTo("Playing");
            }
        }

        private static void OnExitEnergized(IStateContext ctx)
        {
            if (ctx is not PacManContext pCtx) return;
            pCtx.IsEnergized = false;
        }

        private static void OnEnterGameOver(IStateContext ctx)
        {
            PacManAcousticEngine.Play(PacManAcousticEngine.Death);
            Debug.Log("GAME OVER. INITIALIZING DATAWAREHOUSE HANDOFF.");
        }
    }
}