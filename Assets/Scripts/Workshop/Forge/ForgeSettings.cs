using System;
using UnityEngine;

namespace TheSingularityWorkshop.Forge
{
    [Serializable]
    public class ForgeSettings
    {
        /// <summary>
        /// This is the user selected material to use as an override of the construct's material.
        /// </summary>
        public Material ConstructMaterial { get;  set; }
        /// <summary>
        /// This is the default material, if Forge Settings are reset, this will be what the construct's material presents itself as.
        /// </summary>
        public Material DefaultConstruct { get;  set; }

        /// <summary>
        /// Reset to hardcoded defaults
        /// </summary>
        public void ResetToDefault()
        {
            ConstructMaterial = DefaultConstruct;
        }
    }
}