using UnityEngine;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    /// <summary>
    /// The bridge between domain data (Ships, Tanks, Characters) and the Hangar Builder.
    /// </summary>
    public interface IAvatarIdentity
    {
        string UniqueId { get; }
        string DisplayName { get; }
        string Subtitle { get; }
        string Description { get; }
        GameObject PreviewPrefab { get; }
        Color BrandColor { get; }
    }
}