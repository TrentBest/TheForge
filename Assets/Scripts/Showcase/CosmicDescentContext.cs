using UnityEngine;
using Workshop.Systems.FSMs.Contexts;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.UI_And_Tools.Showcase
{
    public class CosmicDescentContext : DigitalStateContext
    {
        // Integration with the UI's 3D render canvas
        public LiveModelPreviewContext LmpbContext { get; set; }

        // Recursive Fractal Scales
        public int CurrentScaleLevel { get; set; } = 0; // 0 = Universe, 1 = Galaxy
        public int MaxScaleLevel { get; set; } = 2;     // 2 = Star System (Halt condition)

        // GPU Compute Signals
        public bool IsBufferReady { get; set; }
        public Vector3 BrightestPointVector { get; set; }

        // FSM Threshold Flags
        public bool IsAligned { get; set; }
        public bool IsCameraAtThreshold { get; set; }
        public bool IsAtStarSystem => CurrentScaleLevel >= MaxScaleLevel;

        // Active Dive Variables
        public float StartCameraDistance { get; set; } = 1000f;
        public float TargetCameraDistance { get; set; } = 10f;
        public float DiveProgress { get; set; } = 0f;
        public Quaternion StartRotation { get; set; }
        public Quaternion TargetRotation { get; set; }
    }
}