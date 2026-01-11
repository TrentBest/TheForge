using UnityEngine;
using TheSingularityWorkshop.FSM_API;


namespace TheSingularityWorkshop.FSM_API.SimpleRotationDemo
{
    public class SimpleRotationDemo : MonoBehaviour, IStateContext
    {
        // Public references for spheres/objects in the Unity Inspector
        public Transform Xaxis;
        public Transform Yaxis;
        public Transform Zaxis;

        // FSM Handles
        public FSMHandle XaxisHandle;
        public FSMHandle YaxisHandle;
        public FSMHandle ZaxisHandle;

        // IStateContext Properties
        public bool IsValid { get; set; }
        public string Name { get; set; }

        void Awake()
        {
            // 1. Setup the main FSM that controls the demo lifecycle
            if (!FSM_API.Interaction.Exists("SimpleRotationDemoFSM"))
            {
                FSM_API.Create.CreateFiniteStateMachine("SimpleRotationDemoFSM", -1, "Update")
                    .State("Executing", OnEnterExecuting, null, null)
                    .BuildDefinition();
            }
            FSM_API.Create.CreateInstance("SimpleRotationDemoFSM", this, "Update");
            Name = "SimpleRotationDemoFSM";
            IsValid = true;
        }

        private void OnEnterExecuting(IStateContext context)
        {
            if (context is SimpleRotationDemo srd)
            {
                // Parameters for movement: Speed 90 degrees/sec, Range +/- 45 degrees
                float rotationSpeed = 90.0f;
                float maxAngle = 45.0f;
                float minAngle = -45.0f;

                // 1. X-axis Rotation
                RotationContext xAxisContext = new RotationContext(srd.Xaxis, new Vector3(1, 0, 0), rotationSpeed, maxAngle, minAngle);
                srd.XaxisHandle = xAxisContext.Status;

                // 2. Y-axis Rotation
                RotationContext yAxisContext = new RotationContext(srd.Yaxis, new Vector3(0, 1, 0), rotationSpeed, maxAngle, minAngle);
                srd.YaxisHandle = yAxisContext.Status;

                // 3. Z-axis Rotation
                RotationContext zAxisContext = new RotationContext(srd.Zaxis, new Vector3(0, 0, 1), rotationSpeed, maxAngle, minAngle);
                srd.ZaxisHandle = zAxisContext.Status;
            }
        }

        // Update is called once per frame by Unity and drives all FSMs in the "Update" group

    }
}