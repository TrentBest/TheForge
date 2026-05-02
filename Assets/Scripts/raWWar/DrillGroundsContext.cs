using Assets.Scripts.Workshop.Gameplay.Grid;
using UnityEngine;

namespace Assets.Scripts.raWWar.Drill
{
    public class DrillGroundsContext
    {
        public int MaxSoldiers = 10000;
        public int CurrentArmySize;
        public float GridSpacing = 2.5f;

        public GridMapData MapGrid; // Unified Data Structure

        public Vector3 CurrentDirection;
        public Vector3 RecruitPosition; // The "Ghost Soldier" target
        public bool IsGameOver;
        public float SpeedMultiplier = 1.0f;

        // Level Progression Data
        public int CurrentLevelId;
        public int MapWidth = 50;
        public int MapHeight = 50;

        // GPU Buffers
        public ComputeBuffer BufferA;
        public ComputeBuffer BufferB;
        public ComputeBuffer PropGridBuffer; // Still needed so the Shader can read the MapGrid!
        public bool UseBufferAAsRead = true;
        public ComputeShader DrillShader;

        // --- THE STANDALONE FALLBACK (SAFE DEFAULTS) ---
        public DrillGroundsContext()
        {
            CurrentArmySize = 1;
            CurrentDirection = Vector3.forward;
            IsGameOver = false;
            CurrentLevelId = 0; // 0 = Empty Training Grounds

            // Initialize the generic grid data
            MapGrid = new GridMapData(MapWidth, MapHeight, GridSpacing);
        }

        public void SwapBuffers() => UseBufferAAsRead = !UseBufferAAsRead;
        public ComputeBuffer GetReadBuffer() => UseBufferAAsRead ? BufferA : BufferB;
        public ComputeBuffer GetWriteBuffer() => UseBufferAAsRead ? BufferB : BufferA;
    }
}