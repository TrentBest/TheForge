// Create: WarlordsContext.cs
using System.Collections.Generic;
using TheSingularityWorkshop.FSM_API;
using UnityEngine;

namespace Assets.Scripts.Warlords
{
    public class WarlordsContext : MonoBehaviour, IStateContext
    {
        public FSMHandle Status { get; private set; }
        public bool IsValid { get; set; } = false;
        public string Name { get; set; } = "Warlords_Campaign";

        // Core Data
        public WarlordsMapData MapData { get; set; }
        public List<WarlordFaction> ActiveFactions { get; set; } = new List<WarlordFaction>();
        public int CurrentTurn { get; set; } = 1;
        public int ActiveFactionIndex { get; set; } = 0;

        public void InitializeFSM()
        {
            if (!FSM_API.Interaction.Exists("Warlords_FSM", "TurnLogic"))
            {
                FSM_API.Create.CreateFiniteStateMachine("Warlords_FSM", -1, "TurnLogic")
                    .State("Deployment", OnEnterDeployment, null, null)
                    .State("PlayerTurn", OnEnterPlayerTurn, null, null)
                    .State("AITurn", OnEnterAITurn, null, null)
                    .State("GameOver", null, null, null)

                    // Basic Turn Loop
                    .Transition("Deployment", "PlayerTurn", (ctx) => true) // Auto-transition after setup
                    .Transition("PlayerTurn", "AITurn", (ctx) => ((WarlordsContext)ctx).ActiveFactions[((WarlordsContext)ctx).ActiveFactionIndex].IsAI)
                    .Transition("AITurn", "PlayerTurn", (ctx) => !((WarlordsContext)ctx).ActiveFactions[((WarlordsContext)ctx).ActiveFactionIndex].IsAI)
                    .BuildDefinition();
            }
            Status = FSM_API.Create.CreateInstance("Warlords_FSM", this, "TurnLogic");
            IsValid = true;
        }

        private void OnEnterDeployment(IStateContext context) { Debug.Log("[FSM] Entering Deployment Phase"); }
        private void OnEnterPlayerTurn(IStateContext context) { Debug.Log($"[FSM] Turn {CurrentTurn}: Player Phase"); }
        private void OnEnterAITurn(IStateContext context) { Debug.Log($"[FSM] Turn {CurrentTurn}: AI Phase"); }
    }

    public class WarlordFaction
    {
        public string FactionName { get; set; }
        public Color FactionColor { get; set; }
        public bool IsAI { get; set; }
        public string Difficulty { get; internal set; }
    }
}