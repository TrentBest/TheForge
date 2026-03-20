// File: Assets/Scripts/Workshop/Forge/Builders/IBuilder.cs
using System;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;

namespace TheSingularityWorkshop.Forge.Builders
{
    /// <summary>
    /// A Builder knows how to construct a specific Type of Product (T).
    /// It also knows how to provide a UI to configure that construction.
    /// </summary>
    public interface IBuilder
    {
        /// <summary>
        /// The Display Name in the Workshop (e.g., "Cast & Crew")
        /// </summary>
        string ToolName { get; }

        /// <summary>
        /// What Type does this builder produce? (e.g., typeof(Casting))
        /// Used for the "Volunteering" registry lookups.
        /// </summary>
        Type GetProductType();

        /// <summary>
        /// Creates the actual runtime object (The Product).
        /// </summary>
        object Build();

        /// <summary>
        /// Returns the Visual Provider (The CRUD UI) for this specific builder instance.
        /// </summary>
        IGuiProvider GetGuiProvider();
    }
}