using TheSingularityWorkshop.FSM_API;
using UnityEngine;

namespace yWorkshop.Forge.Builders.GuiBuilders.Physics
{
    public class OrbitalRotator : MonoBehaviour, IStateContext
    {
        // --- IStateContext Implementation ---
        public bool IsValid { get; set; } = false;
        public string Name { get; set; } = "OrbitalRotatorFSM";

        // --- Rotator Properties ---
        public Vector3 Axis { get; set; } = Vector3.up;
        public float Speed { get; set; } = 90.0f; // Degrees per second

        // FSM Handle tracking
        public FSMHandle Status { get; private set; }

        private void Awake()
        {
            // 1. Define the FSM blueprint if it hasn't been injected into the ecosystem yet.
            // This FSM is incredibly simple: it just stays in a "Rotating" state endlessly.
            if (!FSM_API.Interaction.Exists("OrbitalRotatorFSM"))
            {
                FSM_API.Create.CreateFiniteStateMachine("OrbitalRotatorFSM", -1, "Update")
                    .State("Rotating", null, OnUpdateRotating, null)
                    .BuildDefinition();
            }

            // 2. Instantiate the FSM for this specific MonoBehavior context
            Status = FSM_API.Create.CreateInstance("OrbitalRotatorFSM", this, "Update");

            Name = $"OrbitalRotator_{gameObject.name}";
            IsValid = true;
        }

        // --- FSM Behaviors ---

        /// <summary>
        /// The continuous update loop for the rotator. 
        /// Since it is decoupled from bounds, an external Oscillator can just modify rotator.Speed!
        /// </summary>
        private static void OnUpdateRotating(IStateContext context)
        {
            if (context is OrbitalRotator rotator)
            {
                // Calculate the delta for this frame based on the current Speed
                float angleDelta = rotator.Speed * Time.deltaTime;

                // Apply the rotation using the exact same quaternion math from your RotationContext demo
                rotator.transform.localRotation *= Quaternion.AngleAxis(angleDelta, rotator.Axis);
            }
        }
    }
}