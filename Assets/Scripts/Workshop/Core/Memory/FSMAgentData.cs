// File: Assets/Scripts/Workshop/Core/Memory/FSMAgentData.cs
using UnityEngine;

namespace Workshop.Core.Memory
{
    /// <summary>
    ///
    /// </summary>
    public struct FSMAgentData
    {
        public Vector3 Position; // R, G, B
        public Quaternion Rotation;
        public float Scale;      // Alpha
        public Color Color;
        public int RenderStyleID;
    }
}