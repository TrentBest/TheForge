using UnityEngine;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.Systems.MicroPackages.Providers
{
    /// <summary>
    /// Dictates how a MicroPackage introduces itself to the creator after atmospheric drop.
    /// </summary>
    public interface IPackagePreview
    {
        // A 2D Manifest/Stat Sheet mapped to the physical terminal on the side of the crate
        IGuiProvider ManifestInterface { get; }

        // The 3D holographic or immersive experience projected from the open crate
        void IgniteDiegeticPreview(Transform crateProjectionAnchor);
    }
}