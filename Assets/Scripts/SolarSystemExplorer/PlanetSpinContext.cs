using UnityEngine;
using TheSingularityWorkshop.FSM_API;

namespace Assets.Scripts.SolarSystem
{
    // The Context holding the state data
    public class PlanetSpinContext : IStateContext
    {
        public Transform TransformHandle;
        public float RotationSpeed;
        public Vector3 RotationAxis;

        public bool IsValid { get; set; } = true;
        public string Name { get; set; } = "PlanetSpinFSM";

        public PlanetSpinContext(Transform transform, float speed, Vector3 axis)
        {
            TransformHandle = transform;
            RotationSpeed = speed;
            RotationAxis = axis.normalized;
        }
    }

    // The Monobehavior that boots the FSM and attaches the instance
    public class PlanetSpinBehavior : MonoBehaviour, IStateContext
    {
        public float Speed = 15.0f;
        public Vector3 Axis = Vector3.up;

        public bool IsValid { get; set; } = true;
        public string Name { get; set; } = "PlanetSpinInstance";

        void Awake()
        {
            // 1. Define the FSM once
            if (!FSM_API.Interaction.Exists("PlanetSpinDefinition"))
            {
                FSM_API.Create.CreateFiniteStateMachine("PlanetSpinDefinition", -1, "Update")
                    .State("Spinning", null, OnUpdateSpinning, null)
                    .BuildDefinition();
            }

            // 2. Create the unique context for this specific planet
            PlanetSpinContext ctx = new PlanetSpinContext(this.transform, Speed, Axis);

            // 3. Instantiate the FSM
            FSM_API.Create.CreateInstance("PlanetSpinDefinition", ctx, "Update");
        }

        private void OnUpdateSpinning(IStateContext context)
        {
            if (context is PlanetSpinContext pc && pc.TransformHandle != null)
            {
                // Simple, continuous rotation without boundaries
                pc.TransformHandle.Rotate(pc.RotationAxis, pc.RotationSpeed * Time.deltaTime, Space.Self);
            }
        }
    }
}