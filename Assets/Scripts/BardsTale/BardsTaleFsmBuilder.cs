using Assets.Scripts.GURPS;
using TheSingularityWorkshop.FSM_API;
using TheSingularityWorkshop.FSM_API.Scripts;
using Workshop.BardsTale.World;

namespace Workshop.BardsTale
{
    /// <summary>
    /// Constructs and initializes the Main Experience FSM for the game loop.
    /// </summary>
    public static class BardsTaleFsmBuilder
    {
        public const string GROUP = "BardsTale_Loop";
        public const string FSM = "BardsTale_Main";

        public static FSMHandle Boot(GURPS_Party party, DungeonMapContext map)
        {
            // Register group to Unity Update loop
            if (FSM_UnityIntegrationAdvanced.Instance != null)
            {
                FSM_UnityIntegrationAdvanced.Instance.AddProcessingGroup("Update", GROUP);
            }

            // Define the FSM logic
            FSM_API.Create.CreateFiniteStateMachine(FSM, 1, GROUP)
                .State("Exploration", null, (ctx) => {
                    // Exploration logic will go here
                }, null)
                .State("Combat", null, null, null)
                .WithInitialState("Exploration")
                .BuildDefinition();

            var context = new BardsTaleExperienceContext(party, map);
            return FSM_API.Create.CreateInstance(FSM, context, GROUP);
        }
    }
}