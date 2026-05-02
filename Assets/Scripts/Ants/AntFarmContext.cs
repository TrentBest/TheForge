using System;
using TheSingularityWorkshop.FSM_API;
using Workshop.Core.Math; // Injected for Rng.Squirrel3
using UnityEngine; // Kept only for Mathf.Clamp

namespace Assets.Scripts.Ants
{
    // --- 1. THE DATA CONTEXT ---
    public class AntFarmContext : IStateContext
    {
        public bool IsValid { get; set; } = false;
        public string Name { get; set; } = "Ant_Colony_Context";

        public string FarmName;
        public int Width;
        public int Height;

        // GPU Aligned Memory: Flattened 1D arrays are directly translatable to HLSL StructuredBuffers.
        public byte[] Grid;
        public byte[] ProcessedMask;

        // Chronos state for deterministic sequence generation
        public uint CurrentTick = 0;

        public bool IsPaused = false;
        public bool IsShaking = false;
        public float ShakeTimer = 0f;

        // The Safe Default Constructor
        public AntFarmContext()
        {
            Initialize("Default Sandbox", 128, 128);
        }

        // The Parametrized Constructor
        public AntFarmContext(string name, int width, int height)
        {
            Initialize(name, width, height);
        }

        private void Initialize(string name, int width, int height)
        {
            FarmName = name;
            Width = width;
            Height = height;

            int totalCells = Width * Height;
            Grid = new byte[totalCells];
            ProcessedMask = new byte[totalCells];

            // Fill bottom 60% with Sand (1), scattered Stone (2), Air (0) above.
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    int idx = x + (y * Width);

                    if (y < Height * 0.6f)
                    {
                        // 5% chance of an immovable Stone to force complex digging paths
                        Grid[idx] = Rng.GetFloat(idx, 1337) > 0.95f ? (byte)2 : (byte)1;
                    }
                    else
                    {
                        Grid[idx] = 0;
                    }
                }
            }

            // Spawn 200 Ants (4) on the surface deterministically
            for (int i = 0; i < 200; i++)
            {
                // Math scales the random float (0.0 to 1.0) to grid width, keeping a 5-unit margin
                int spawnX = (int)(Rng.GetFloat(i, 999) * (Width - 10)) + 5;
                int spawnY = (int)(Height * 0.61f);
                Grid[spawnX + (spawnY * Width)] = 4;
            }

            IsValid = true;
        }
    }

    // --- 2. THE FSM LOGIC ---
    public static class AntColonyLogic
    {
        public static void InitializeFSM()
        {
            FSM_API.Create.CreateFiniteStateMachine("AntColonySim", processRate: 1, processingGroup: "AntFarmLogic")
                .State("Initializing", OnEnterInitializing, null, null)
                .State("Simulating", OnEnterSimulating, OnUpdateSimulating, OnExitSimulating)
                .State("Paused", OnEnterPaused, OnUpdatePaused, OnExitPaused)
                .Transition("Initializing", "Simulating", (ctx) => true)
                .Transition("Simulating", "Paused", ShouldPause)
                .Transition("Paused", "Simulating", ShouldUnpause)
                .BuildDefinition();
        }

        private static void OnEnterInitializing(IStateContext context) { }
        private static void OnEnterSimulating(IStateContext context) { }

        private static void OnUpdateSimulating(IStateContext context)
        {
            if (context is AntFarmContext farm)
            {
                ProcessAutomata(farm);
            }
        }

        private static void OnExitSimulating(IStateContext context) { }
        private static void OnEnterPaused(IStateContext context) { }
        private static void OnUpdatePaused(IStateContext context) { }
        private static void OnExitPaused(IStateContext context) { }

        private static bool ShouldPause(IStateContext context) => context is AntFarmContext farm && farm.IsPaused;
        private static bool ShouldUnpause(IStateContext context) => context is AntFarmContext farm && !farm.IsPaused;

        private static void ProcessAutomata(AntFarmContext farm)
        {
            farm.CurrentTick++;
            int w = farm.Width;
            int h = farm.Height;

            // O(1) Memory clear instead of allocating a new array every frame
            Array.Clear(farm.ProcessedMask, 0, farm.ProcessedMask.Length);

            // Alternating sweep direction based on tick prevents particles from pulling diagonally 
            // over time due to array processing order.
            bool leftToRight = (farm.CurrentTick % 2 == 0);

            for (int y = 1; y < h; y++)
            {
                for (int i = 0; i < w; i++)
                {
                    int x = leftToRight ? i : (w - 1 - i);
                    int idx = x + (y * w);

                    if (farm.ProcessedMask[idx] == 1) continue;

                    byte state = farm.Grid[idx];

                    // Air (0) or Stone (2) do not process physics.
                    if (state == 0 || state == 2) continue;

                    uint seed = farm.CurrentTick;

                    // --- PHYSICS: Sand (1) or Water (3) Falls ---
                    if (state == 1 || state == 3)
                    {
                        int belowIdx = x + ((y - 1) * w);

                        if (farm.Grid[belowIdx] == 0)
                        {
                            Swap(farm, idx, belowIdx);
                        }
                        else if (state == 3) // Water flows sideways
                        {
                            int dir = Rng.GetFloat(idx, seed) > 0.5f ? -1 : 1;
                            int sideIdx = (x + dir) + (y * w);

                            if (x + dir > 0 && x + dir < w && farm.Grid[sideIdx] == 0)
                            {
                                Swap(farm, idx, sideIdx);
                            }
                        }
                    }

                    // --- BEHAVIOR STATE A: Worker Ant (4) ---
                    else if (state == 4)
                    {
                        if (ApplyGravity(farm, x, y, w, idx)) continue;

                        // Bias movement slightly downwards to encourage tunnel digging
                        int dx = (int)(Rng.GetFloat(idx, seed + 1) * 3) - 1; // -1, 0, 1
                        int dy = Rng.GetFloat(idx, seed + 2) > 0.6f ? -1 : (int)(Rng.GetFloat(idx, seed + 3) * 2);

                        int nx = Mathf.Clamp(x + dx, 0, w - 1);
                        int ny = Mathf.Clamp(y + dy, 0, h - 1);
                        int targetIdx = nx + (ny * w);

                        if (farm.Grid[targetIdx] == 0) // Move into air
                        {
                            Swap(farm, idx, targetIdx);
                        }
                        else if (farm.Grid[targetIdx] == 1 && Rng.GetFloat(idx, seed + 4) > 0.85f)
                        {
                            // DIGGING: Turn sand to air, move in, and transition to State 6 (Carrying)
                            farm.Grid[targetIdx] = 0;
                            Swap(farm, idx, targetIdx);
                            farm.Grid[targetIdx] = 6;
                        }
                    }

                    // --- BEHAVIOR STATE B: Hauler Ant (6) ---
                    else if (state == 6)
                    {
                        if (ApplyGravity(farm, x, y, w, idx)) continue;

                        // Bias movement UPWARDS to reach the surface
                        int dx = (int)(Rng.GetFloat(idx, seed + 5) * 3) - 1;
                        int dy = Rng.GetFloat(idx, seed + 6) > 0.2f ? 1 : (int)(Rng.GetFloat(idx, seed + 7) * 2) - 1;

                        int nx = Mathf.Clamp(x + dx, 0, w - 1);
                        int ny = Mathf.Clamp(y + dy, 0, h - 1);
                        int targetIdx = nx + (ny * w);

                        if (farm.Grid[targetIdx] == 0)
                        {
                            // DROP HEURISTIC
                            float dropChance = (y > h * 0.61f) ? 0.4f : 0.01f;

                            if (Rng.GetFloat(idx, seed + 8) < dropChance)
                            {
                                // Drop the sand in the cell we are leaving!
                                farm.Grid[idx] = 1;
                                farm.Grid[targetIdx] = 4; // Back to empty Worker Ant
                                farm.ProcessedMask[idx] = 1;
                                farm.ProcessedMask[targetIdx] = 1;
                            }
                            else
                            {
                                Swap(farm, idx, targetIdx);
                            }
                        }
                    }
                }
            }
        }

        private static bool ApplyGravity(AntFarmContext farm, int x, int y, int w, int idx)
        {
            if (y > 0)
            {
                int belowIdx = x + ((y - 1) * w);
                if (farm.Grid[belowIdx] == 0)
                {
                    Swap(farm, idx, belowIdx);
                    return true;
                }
            }
            return false;
        }

        private static void Swap(AntFarmContext farm, int idx1, int idx2)
        {
            byte temp = farm.Grid[idx1];
            farm.Grid[idx1] = farm.Grid[idx2];
            farm.Grid[idx2] = temp;
            farm.ProcessedMask[idx1] = 1;
            farm.ProcessedMask[idx2] = 1;
        }
    }
}