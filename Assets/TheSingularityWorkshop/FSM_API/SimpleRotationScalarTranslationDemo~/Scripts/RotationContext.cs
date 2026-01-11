using UnityEngine;
using TheSingularityWorkshop.FSM_API;


namespace TheSingularityWorkshop.FSM_API.SimpleRotationScalarTranslationDemo
{

    public class RotationContext : IStateContext
    {
        // Rotation Parameters (Degrees)
        public float RotationSpeed = 90.0f; // degrees per second
        public float MaxAngle = 45.0f;      // Max boundary angle (e.g., +45 degrees)
        public float MinAngle = -45.0f;     // Min boundary angle (e.g., -45 degrees)

        public Vector3 RotationAxis;
        public Quaternion OriginalRotation = Quaternion.identity;

        public Transform TransformHandle;
        public FSMHandle Status { get; private set; }

        // IStateContext Properties
        public bool IsValid { get; set; } = true;
        public string Name { get; set; } = "RotationFSM";

        public RotationContext(Transform transform, Vector3 rotationAxis, float rotationSpeed = 90f, float maxAngle = 45f, float minAngle = -45f)
        {
            TransformHandle = transform;
            OriginalRotation = transform.localRotation; // Start rotation
            RotationSpeed = rotationSpeed;

            // We set MaxAngle and MinAngle directly based on the scene's starting rotation.
            this.MaxAngle = maxAngle;
            this.MinAngle = minAngle;

            RotationAxis = rotationAxis.normalized;

            if (!FSM_API.Interaction.Exists("RotationFSM"))
            {
                FSM_API.Create.CreateFiniteStateMachine("RotationFSM", -1, "Update")
                    .State("RotatingForward", OnEnterRotatingForward, OnUpdateRotatingForward, OnExitRotatingForward)
                    .State("RotatingBackward", OnEnterRotatingBackward, OnUpdateRotatingBackward, OnExitRotatingBackward)
                    .Transition("RotatingForward", "RotatingBackward", MaxReached)
                    .Transition("RotatingBackward", "RotatingForward", MinReached)
                    .BuildDefinition();
            }

            Status = FSM_API.Create.CreateInstance("RotationFSM", this, "Update");
            Status.TransitionTo("RotatingForward");
        }

        // --- Helper to get current angle along the axis ---
        private float GetCurrentAngle(RotationContext rc)
        {
            // Calculate the current rotation relative to the object's starting rotation.
            Quaternion relativeRotation = rc.TransformHandle.localRotation * Quaternion.Inverse(rc.OriginalRotation);

            // This converts the relative rotation to Euler angles in the -180 to 180 range
            Vector3 euler = relativeRotation.eulerAngles;
            euler.x = Mathf.DeltaAngle(0, euler.x);
            euler.y = Mathf.DeltaAngle(0, euler.y);
            euler.z = Mathf.DeltaAngle(0, euler.z);

            // Use Dot Product to extract the scalar angle along the specified axis
            return Vector3.Dot(euler, rc.RotationAxis);
        }

        // --- State Logic ---

        private void OnEnterRotatingForward(IStateContext context) { }

        private void OnUpdateRotatingForward(IStateContext context)
        {
            if (context is RotationContext rc)
            {
                float angleDelta = rc.RotationSpeed * Time.deltaTime;
                // Apply rotation using AngleAxis, which is efficient and quaternion-safe
                rc.TransformHandle.localRotation *= Quaternion.AngleAxis(angleDelta, rc.RotationAxis);
            }
        }
        private void OnExitRotatingForward(IStateContext context) { }

        private void OnEnterRotatingBackward(IStateContext context) { }

        private void OnUpdateRotatingBackward(IStateContext context)
        {
            if (context is RotationContext rc)
            {
                // Move in the opposite direction (negative angle)
                float angleDelta = -rc.RotationSpeed * Time.deltaTime;
                rc.TransformHandle.localRotation *= Quaternion.AngleAxis(angleDelta, rc.RotationAxis);
            }
        }
        private void OnExitRotatingBackward(IStateContext context) { }

        // --- Transition Predicates ---

        private bool MaxReached(IStateContext context)
        {
            if (context is RotationContext rc)
            {
                // Use GetCurrentAngle to check against the scalar MaxAngle boundary
                float currentAngle = GetCurrentAngle(rc);
                // Use a small tolerance (0.1f) for float comparison
                return currentAngle >= rc.MaxAngle - 0.1f;
            }
            return false;
        }

        private bool MinReached(IStateContext context)
        {
            if (context is RotationContext tc)
            {
                // Use GetCurrentAngle to check against the scalar MinAngle boundary
                float currentAngle = GetCurrentAngle(tc);

                return currentAngle <= tc.MinAngle + 0.1f;
            }
            return false;
        }
    }
}