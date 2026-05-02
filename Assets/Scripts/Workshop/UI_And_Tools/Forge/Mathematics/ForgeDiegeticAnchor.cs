// File: Assets/Scripts/Workshop/UI_And_Tools/Forge/ForgeDiegeticAnchor.cs
using TheSingularityWorkshop.FSM_API;
using UnityEngine;

namespace Workshop.UI_And_Tools.Forge
{
    /// <summary>
    /// Acts as the spatial tether for the Forge within the experience world.
    /// Provides telemetry to the Spatial OS, which handles the logic.
    /// </summary>
    public class ForgeDiegeticAnchor : MonoBehaviour, IStateContext
    {
        [Header("Spatial Telemetry")]
        public Transform ObserverTransform;
        public float CollapseRadius = 3.5f;
        public GameObject ForgeVisualsRoot;

        public bool IsValid { get; set; } = true;
        public string Name { get; set; } = "ForgeDiegeticAnchor";

        public FSMHandle Status { get; private set; }
        public Vector3 AnchorCoordinates { get; private set; }

        void Start()
        {
            AnchorCoordinates = transform.position;
            if (!ObserverTransform && Camera.main) ObserverTransform = Camera.main.transform;

            InitializeSpatialFsm();
        }

        private void InitializeSpatialFsm()
        {
            if (!FSM_API.Interaction.Exists("Forge_Spatial_OS"))
            {
                FSM_API.Create.CreateFiniteStateMachine("Forge_Spatial_OS", -1, "Workshop")
                    .State("Manifested", ctx => ForgeVisualsRoot?.SetActive(true), null, null)
                    .State("Collapsed", ctx => ForgeVisualsRoot?.SetActive(false), null, null)
                    .Transition("Manifested", "Collapsed", ctx => IsObserverOutsideBoundary())
                    // Intent-driven return (Summoning)
                    .BuildDefinition();
            }
            Status = FSM_API.Create.CreateInstance("Forge_Spatial_OS", this, "Workshop");
        }

        public bool IsObserverOutsideBoundary()
        {
            if (!ObserverTransform) return false;

            // Fast flat-plane distance check
            float dx = ObserverTransform.position.x - AnchorCoordinates.x;
            float dz = ObserverTransform.position.z - AnchorCoordinates.z;
            return (dx * dx + dz * dz) > (CollapseRadius * CollapseRadius);
        }

        public void Summon(Vector3 targetPosition, Quaternion targetRotation)
        {
            transform.SetPositionAndRotation(targetPosition, targetRotation);
            AnchorCoordinates = targetPosition;
            Status.TransitionTo("Manifested");
        }
    }
}