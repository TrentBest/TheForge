#if UNITY_EDITOR
using TheSingularityWorkshop.FSM_API;
using UnityEngine;

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders
{
    /// <summary>
    /// The Context that bridges UI Toolkit mouse events with the FSM logic.
    /// </summary>
    public class LiveModelPreviewContext : IStateContext
    {
        public string Name { get; set; } = "LiveModelPreview";
        public bool IsValid { get; set; } = true;
        public FSMHandle Status { get; set; }

        public GameObject TargetModel { get; set; }
        public Camera PreviewCamera { get; set; }
        public Light PreviewLight { get; set; }
        // Settings
        public bool AutoOrbitEnabled { get; set; } = true;
        public Vector3 OrbitAxis { get; set; } = Vector3.up;
        public float OrbitSpeed { get; set; } = 1f;

        public bool MouseControlEnabled { get; set; } = true;
        public bool IsDragging { get; set; }
        public Vector2 LastMousePosition { get; set; }
        public bool IsInteracting { get; set; } // Flag to pause the FSM Auto-Orbit

        public Quaternion TargetRotation { get; set; } = Quaternion.identity;
        public bool MultiAxisEnabled { get; set; } = false;
        public bool ShowGizmos { get; set; } = false;

        public double LastTickTime { get; set; }
        public RenderTexture RenderTexture { get; set; }

        // Transform State
        public float Zoom { get; set; } = 1.5f;
        public float Pitch { get; set; } = 25f;
        public float Yaw { get; set; } = 45f;

        // UI Interaction State
        public bool IsDirty { get; set; } = false;
        public bool AutoRotate { get; set; } = true;
        public float RotationSpeed { get; set; } = 1f;
        public Color BackgroundColor { get; set; } = Color.black;
        public float LightIntensity { get; set; } = 1.0f;
    }
}
#endif