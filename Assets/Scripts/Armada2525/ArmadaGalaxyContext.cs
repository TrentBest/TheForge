using Assets.Scripts.MastersOfOrionII;
using TheSingularityWorkshop.FSM_API;
using System.Collections.Generic;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.WorldBuilding;

namespace Workshop.Armada2525
{
    /// <summary>
    /// Advanced Integration Context for the Armada 2525 Simulation.
    /// Drives the background simulation of the galaxy.
    /// </summary>
    public class ArmadaGalaxyContext : IStateContext
    {
        public string Name { get; set; } = "Armada_Galaxy_Sim";
        public bool IsValid { get; set; } = true;

        // --- NEW SOVEREIGN STATE FLAGS ---
        public uint UniversalSeed { get; set; } = 0;
        public bool IsGalaxyGenerated { get; set; } = false;

        // Scavenged from MoOII DataModels
        public Galaxy GalaxyConfig;
        public List<Civilization> Players = new List<Civilization>();

        // Star System tracking for the 2D Tactical Grid
        public Dictionary<int, StarSystemContext> StarSystems = new Dictionary<int, StarSystemContext>();
    }
}