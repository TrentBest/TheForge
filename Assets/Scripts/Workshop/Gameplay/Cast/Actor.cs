// File: Assets/Scripts/Workshop/Forge/Builders/Casting/Actor.cs
using UnityEngine;
using TheSingularityWorkshop.FSM_API;

namespace Workshop.Gameplay.Cast
{
    /// <summary>
    /// An Actor is a dynamic agent in the experience.
    /// It implements IStateContext so it can be driven by a Singularity FSM.
    /// </summary>
    public class Actor : MonoBehaviour, IStateContext
    {
        [Header("Identity")]
        public string StageName;     // e.g. "Jared"
        public Role NarrativeRole;   // e.g. Protagonist
        public string Occupation;    // e.g. "Space Marine"

        // --- IStateContext Implementation ---

        // The unique ID used by the FSM Registry to find this instance
        public string Name { get; set; }

        // If false, the FSM system will stop processing this actor
        public bool IsValid { get; set; } = true;

        // The handle to the running FSM (The "Soul")
        public FSMHandle Status { get; private set; }

        public void Initialize(string name, Role role, string occupation)
        {
            StageName = name;
            NarrativeRole = role;
            Occupation = occupation;

            // Set the Context Name (Must be unique enough for the registry)
            Name = $"{role}_{name}_{System.Guid.NewGuid().ToString().Substring(0, 4)}";
            gameObject.name = $"[{role}] {name}";

            // Boot the Brain
            InitializeBrain();
        }

        private void InitializeBrain()
        {
            // Example: Bind to a default "ActorBehavior" FSM
            // In a real scenario, 'Occupation' might dictate which FSM Definition to load
            string fsmId = "GenericActorBehavior";

            if ( !FSM_API.Interaction.Exists(fsmId))
            {
                // Auto-generate a default brain if none exists
                FSM_API.Create.CreateFiniteStateMachine(fsmId, -1, "Update")
                    .State("Idle", null, null, null)
                    .BuildDefinition();
            }

            // Possess this Actor with the FSM
            Status = FSM_API.Create.CreateInstance(fsmId, this);
        }

        private void OnDestroy()
        {
            IsValid = false; // Tell the FSM to let go
        }
    }
}