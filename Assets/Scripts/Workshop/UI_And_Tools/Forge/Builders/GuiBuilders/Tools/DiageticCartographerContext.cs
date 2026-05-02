using UnityEngine;
using TheSingularityWorkshop.FSM_API;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Tools
{
    /// <summary>
    /// Decoupled state context for the Diegetic Cartographer.
    /// Safely tracks the current focus target without bloating the UI logic.
    /// </summary>
    public class DiegeticCartographerContext : IStateContext
    {
        public bool IsValid { get; set; } = true;
        public string Name { get; set; } = "Ctx_DiegeticCartographer";

        // The entity currently being X-Rayed
        public Transform SelectedEntity { get; set; }
        public string SelectedType { get; set; }
    }
}