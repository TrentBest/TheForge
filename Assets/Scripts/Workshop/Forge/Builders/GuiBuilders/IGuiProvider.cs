using System;
using UnityEngine.UIElements;

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders
{
    /// <summary>
    /// The contract for any object that wants to be edited in the Singularity Hub.
    /// This fulfills the "Concrete Builder-Editor Duality".
    /// </summary>
    public interface IGuiProvider
    {
        /// <summary>
        /// The name to display in the Hub's list view.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Returns a delegate that constructs the UI into a provided root container.
        /// </summary>
        Action<VisualElement> GetGuiBuilder();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>

        VisualElement CreateGui(GuiContext ctx);

#if UNITY_EDITOR
        // Bake the C# Builder logic into a static UXML file
        void ToUIDocument(string assetPath);
#endif

        // Hydrate the logic from an existing UXML file (Runtime)
        void FromUIDocument(string assetPath);
    }
}