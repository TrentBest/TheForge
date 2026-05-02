// File: Assets/Scripts/Workshop/UI_And_Tools/Forge/IO/JsonManifestDecoder.cs
// (Adjust path/namespace to match exactly where your file lives)

using UnityEngine;
using Workshop.Core; // Required for AnyAppContext and ExperienceContext
using Workshop.Core.Memory;

namespace Workshop.Core.IO
{
    public class JsonManifestDecoder : IManifestDecoder
    {
        private string _jsonPayload;

        public JsonManifestDecoder(string jsonPayload)
        {
            _jsonPayload = jsonPayload;
        }

        public ExperienceContext DecodeExperience()
        {
            // 1. Get the Kernel Context that the Bootloader created
            var kernelContext = SingularityBootloader.AppContext;

            if (kernelContext == null)
            {
                Diagnostics.ForgeLogger.LogError("Cannot decode Experience Manifest: Kernel AppContext is null!").SendToUnity();
                return null;
            }

            // 2. Create the Experience Context using the Kernel
            var experience = kernelContext.CreateExperienceContext();

            // 3. Set the name if needed (since your old constructor did this)
            experience.Name = "Showcase Runtime";

            // Note: If you still need to track HostEnvironment.MyVR_Showcase, 
            // add it to the DynamicState dictionary of the new context.
            // experience.DynamicState["HostEnvironment"] = HostEnvironment.MyVR_Showcase;

            return experience;
        }

        public void ApplyMemoryAllocations(DataWarehouse warehouse)
        {
            // Will eventually parse _jsonPayload to allocate shelves
        }

        public void HydrateStateZero(DataWarehouse warehouse)
        {
            // Will eventually parse _jsonPayload to populate entity data
        }
    }
}