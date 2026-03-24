using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders.WorldBuilding;

namespace Assets.Scripts.Workshop.Forge.Builders.GuiBuilders.WorldBuilding
{
    public interface ICartographicInterpreter
    {
        // High-level style ID (e.g., "Parchment", "Blueprint", "Hologram")
        string StyleIdentity { get; }

        // Logic for simplifying complex world meshes into map icons or lines
        void ProcessTerrain(WorldSimulationData worldData, CartoCanvas canvas);

        // Logic for adding "Cultural" layers (Nations, Roads, Ley Lines)
        void ProcessInfrastructure(WorldSimulationData worldData, CartoCanvas canvas);
    }
}