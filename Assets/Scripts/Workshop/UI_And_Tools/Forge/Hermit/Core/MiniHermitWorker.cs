using TheSingularityWorkshop.FSM_API;
using TheSingularityWorkshop.FSM_API.Scripts;
using UnityEngine;

namespace Workshop.UI_And_Tools.Forge.Hermit.Core
{
    /// <summary>
    /// The Dozer Shell. A pure data container and IStateContext.
    /// Relies on the Central API for execution and provides IsValid to gate logic.
    /// </summary>
    public class MiniHermitWorker : MonoBehaviour, IStateContext
    {
        // --- IContext Implementation ---
        public string Name { get; set; } = "MiniHermit without a Name";

        // --- IStateContext Implementation ---
        public FSMHandle Status { get; private set; }

        // CRITICAL ENGINE GATE: Allows the API to cleanly cull dead contexts

        public bool IsValid { get; set; } = false;
       

        // --- Worker Properties ---
        public string CurrentTaskName { get; private set; }

        // --- Wander FSM State Variables ---
        public Vector3 WanderTarget;
        public float WanderSpeed = 1.5f;
        public float WanderRadius = 5.0f;
        public float ReachThreshold = 0.2f;

        public void Initialize(string taskDescription, string taskType)
        {
            CurrentTaskName = taskType;

            string fsmName = $"MiniHermit_{taskType}";
            string processGroup = "MiniHermit_Movement";
            int processRate = -1; // Execute every time the processing group updates

            // 1. Check if the Blueprint already exists in the Central API
            if (!FSM_API.Interaction.Exists(fsmName, processGroup))
            {
                // 2. Register the Process Group to the correct Unity execution loop
                if (FSM_UnityIntegrationAdvanced.Instance != null)
                {
                    FSM_UnityIntegrationAdvanced.Instance.AddProcessingGroup("FixedUpdate", processGroup);
                }

                // 3. Construct the FSM Blueprint using the strict Fluent Builder API
                FSM_API.Create.CreateFiniteStateMachine(fsmName, processRate, processGroup)
                    .State(taskType, OnEnterWanderState, OnUpdateWanderState, OnExitWanderState)
                    .Transition(taskType, taskType, ReachedDestination)
                    .BuildDefinition();
            }

            // 4. Create the instance, binding this script as the Context
            Status = FSM_API.Create.CreateInstance(fsmName, this, processGroup);
            IsValid = true;
        }

        private bool ReachedDestination(IStateContext context)
        {
            return Vector3.Distance(transform.position, WanderTarget) <= ReachThreshold;
        }

        // ==========================================
        // WANDER TASK: FSM STATE LOGIC
        // Note: Signatures accept IStateContext per the API contract!
        // ==========================================

        public void OnEnterWanderState(IStateContext context)
        {
            PickNewWanderTarget();
        }

        public void OnUpdateWanderState(IStateContext context)
        {
            // Move towards the target
            transform.position = Vector3.MoveTowards(transform.position, WanderTarget, WanderSpeed * Time.deltaTime);

            // Look at the target
            Vector3 direction = (WanderTarget - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
            }

           
        }

        public void OnExitWanderState(IStateContext context)
        {
            WanderTarget = transform.position;
        }

        // --- Helper Methods ---

        public void PickNewWanderTarget()
        {
            Vector2 randomCircle = Random.insideUnitCircle * WanderRadius;
            WanderTarget = new Vector3(transform.position.x + randomCircle.x, transform.position.y, transform.position.z + randomCircle.y);
        }
    }
}