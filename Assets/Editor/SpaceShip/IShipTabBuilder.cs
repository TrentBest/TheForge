using TheSingularityWorkshop;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;

public interface IShipTabBuilder : IGuiProvider
{
    string TabName { get; }
    string TabIcon { get; } // Optional: For Mermaid.js or UI icons
}
