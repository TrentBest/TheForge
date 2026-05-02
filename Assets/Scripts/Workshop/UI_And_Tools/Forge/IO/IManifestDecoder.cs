

using Workshop.Core.Memory;

namespace Workshop.Core.IO
{
    /// <summary>
    /// The strict contract for hydrating the Singularity runtime. 
    /// Decouples the storage format (JSON vs Binary/Texture) from the boot process.
    /// </summary>
    public interface IManifestDecoder
    {
        /// <summary>
        /// Retrieves the foundational ontological rules (Senses, Scale, Engine)
        /// </summary>
        ExperienceContext DecodeExperience();

        /// <summary>
        /// Instructs the DataWarehouse to pre-allocate its DataShelves to specific High-Water Marks.
        /// </summary>
        void ApplyMemoryAllocations(DataWarehouse warehouse);

        /// <summary>
        /// Bypasses the ForgeArbitrator by writing post-convergence state data 
        /// directly into the unmanaged DataShelf pointers.
        /// </summary>
        void HydrateStateZero(DataWarehouse warehouse);
    }
}