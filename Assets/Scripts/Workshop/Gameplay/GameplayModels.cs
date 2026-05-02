using System;
using UnityEngine;

namespace Workshop.raWWar.Gameplay
{
    /// <summary>
    /// Represents a pre-calculated frame of animation.
    /// This gets flattened into a massive 1D array or a Texture2D for the GPU.
    /// </summary>
    [Serializable]
    public struct BakedFrameData
    {
        // For 1M agents, we typically bake vertex positions, 
        // but if the mesh is uniform, we can bake bone matrices and do skinning on the GPU.
        public Vector3[] VertexPositions;
    }

    /// <summary>
    /// The cached "Ideal Motion" lookup table.
    /// </summary>
    public class GestureData
    {
        public string GestureName; // e.g., "March", "Aim", "Die"
        public int TotalFrames;
        public float FrameRate = 30f;

        // O(1) Lookup: BakedFrames[frameIndex]
        public BakedFrameData[] BakedFrames;
    }

    /// <summary>
    /// UPDATED GPU STRUCT: What we send across the PCIe bus per agent.
    /// </summary>
    public struct FSMAgentData
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Color FactionColor;

        // --- NEW: GESTURE STATE ---
        public int CurrentGestureId;
        public float CurrentFrameTime; // e.g., 14.5
        public float SkillLevel;       // 1.0 = Perfect execution, 0.1 = Sloppy/out of sync
        public float RandomSeed;       // Unique offset per agent to break visual uniformity
    }
}