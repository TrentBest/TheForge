namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.WorldBuilding
{
    public interface ICartographicInterpreter
    {
        // High-level style ID (e.g., "Parchment", "Blueprint", "Hologram")
        string StyleIdentity { get; }

        // Logic for simplifying complex world meshes into map icons or lines
        void ProcessTerrain(PlanetContext worldData, CartoCanvas canvas);

        // Logic for adding "Cultural" layers (Nations, Roads, Ley Lines)
        void ProcessInfrastructure(PlanetContext worldData, CartoCanvas canvas);
    }
}