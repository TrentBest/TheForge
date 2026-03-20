using System;
using System.Collections.Generic;
using System.Text;
using TheSingularityWorkshop.FSM_API;
using UnityEngine;

namespace TheSingularityWorkshop.Stage
{
    [Serializable]
    public class Prop
    {
        public string Id = Guid.NewGuid().ToString();
        public string Name;

        // --- TEXT RENDERING (The Book) ---
        public string[] TextualDescriptors; // e.g., ["rusted", "looming", "ancient"]

        // --- SPATIAL DATA (The Matrix) ---
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 PhysicalBounds; // Determines volume, regardless of mesh

        // --- 3D RENDERING (The Game) ---
        // This is optional! A book doesn't need this, but raWWar does.
        public string PrefabResourcePath;

        /// <summary>
        /// "Renders" the prop to a string for literary output.
        /// </summary>
        public string RenderToString()
        {
            string adjectives = TextualDescriptors != null && TextualDescriptors.Length > 0
                ? string.Join(", ", TextualDescriptors) + " "
                : "";

            // Example: "A rusted, looming Windtrap"
            return $"A {adjectives}{Name}";
        }
    }
}
