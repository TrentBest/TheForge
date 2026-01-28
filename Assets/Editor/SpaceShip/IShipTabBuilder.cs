using Assets.Scripts;

public interface IShipTabBuilder : IGuiProvider
{
    string TabName { get; }
    string TabIcon { get; } // Optional: For Mermaid.js or UI icons
}
