using UnityEngine;
using TheSingularityWorkshop.FSM_API;


namespace TheSingularityWorkshop.FSM_API.SimpleTranslationDemo
{
    public class TranslationContext : IStateContext
    {
        public float MoveSpeed = 1.0f;
        public float MaxPosition = 5.0f;
        public float MinPosition = -5.0f;

        public Vector3 MoveAxis;
        public Vector3 OriginalPosition = Vector3.zero;
        public Transform TransformHandle;
        public FSMHandle Status { get; private set; }

        // IStateContext Properties
        public bool IsValid { get; set; } = true; // Changed to true for simplicity
        public string Name { get; set; } = "TranslationFSM";

        public TranslationContext(Transform transform, Vector3 moveAxis, float moveSpeed = 1f, float maxPos = 2f, float minPos = -2f)
        {
            TransformHandle = transform;
            OriginalPosition = transform.localPosition;
            MoveSpeed = moveSpeed;

            MoveAxis = moveAxis.normalized;

            // 1. Get the current scalar coordinate along the intended axis of movement
            float initialCoord = Vector3.Dot(OriginalPosition, MoveAxis);

            // 2. Calculate the absolute Max/Min boundaries based on the initial position and the +/- 2 unit displacement
            this.MaxPosition = initialCoord + maxPos; // e.g., -3 + 2 = -1
            this.MinPosition = initialCoord + minPos; // e.g., -3 + (-2) = -5

            if (!FSM_API.Interaction.Exists("TranslationFSM"))
            {
                FSM_API.Create.CreateFiniteStateMachine("TranslationFSM", -1, "Update")
                    .State("MovingForward", OnEnterMovingForward, OnUpdateMovingForward, OnExitMovingForward)
                    .State("MovingBackward", OnEnterMovingBackward, OnUpdateMovingBackward, OnExitMovingBackward)
                    .Transition("MovingForward", "MovingBackward", MaxReached)
                    .Transition("MovingBackward", "MovingForward", MinReached)
                    .BuildDefinition();
            }

            Status = FSM_API.Create.CreateInstance("TranslationFSM", this, "Update");
            // Name and IsValid are set at the start of the constructor
            IsValid = true;
        }

        // --- State Logic (Uses Time.deltaTime for smooth, frame-rate independent movement) ---

        private void OnEnterMovingForward(IStateContext context) { }

        private void OnUpdateMovingForward(IStateContext context)
        {
            if (context is TranslationContext tc)
            {
                tc.TransformHandle.localPosition += tc.MoveAxis * (tc.MoveSpeed * Time.deltaTime);
            }
        }
        private void OnExitMovingForward(IStateContext context) { }

        private void OnEnterMovingBackward(IStateContext context) { }

        private void OnUpdateMovingBackward(IStateContext context)
        {
            if (context is TranslationContext tc)
            {
                tc.TransformHandle.localPosition -= tc.MoveAxis * (tc.MoveSpeed * Time.deltaTime);
            }
        }
        private void OnExitMovingBackward(IStateContext context) { }

        // --- Transition Predicates (Uses Dot Product for boundary check) ---

        private bool MaxReached(IStateContext context)
        {
            if (context is TranslationContext tc)
            {
                float currentCoord = Vector3.Dot(tc.TransformHandle.localPosition, tc.MoveAxis);
                return currentCoord >= tc.MaxPosition;
            }
            return false;
        }

        private bool MinReached(IStateContext context)
        {
            if (context is TranslationContext tc)
            {
                float currentCoord = Vector3.Dot(tc.TransformHandle.localPosition, tc.MoveAxis);
                return currentCoord <= tc.MinPosition;
            }
            return false;
        }
    }
}