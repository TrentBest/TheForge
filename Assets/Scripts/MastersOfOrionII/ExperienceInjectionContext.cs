using UnityEngine;
using Workshop.UI_And_Tools.Forge;

namespace Assets.Scripts.MastersOfOrionII
{
    public struct ExperienceInjectionContext
    {
        // Physical & Digital Display Properties
        public Vector2 ScreenDimensions; // In pixels (e.g., 1920x1080)
        public float DPI;                // Dots per inch for scaling UI
        public Vector2 PhysicalSize;     // In meters or inches (e.g., 20ft x 14ft)
        public DisplayPreset Presentation; // Handheld, Billboard, InWorldProp, VR

        // Timing & Synchronization
        public float TargetFrameRate;    // The rate at which the Host will step the FSMs
        public float FixedDeltaTime;     // Physics/Logic step timing provided by the host

        // Identity within the Experience
        public string InstanceId;        // Unique ID for this specific app instance
                                         // Physics & Environment
        public Vector3 Gravity;          // The host's current gravity (e.g., Vector3.zero for Zero-G)
        public float PhysicsStep;        // How often the physics engine is ticking

        // Reskinning & Aesthetics
        public string ThemeId;           // e.g., "Industrial_SciFi", "Classic_Wood", "Cyberpunk"
        public Color PrimaryAccent;      // Host-defined color palette for sub-apps to match
    }
}