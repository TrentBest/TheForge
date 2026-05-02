using System;
using UnityEngine;
using Workshop.Core.Diagnostics; // Injected for ForgeLogger

namespace Workshop.UI_And_Tools.Forge.Hermit.Core
{
    /// <summary>
    /// The standard JSON payload format received from the LLM via the network bridge.
    /// </summary>
    [Serializable]
    public class HermitAction
    {
        public string Type;
        public Vector3 Target;
        public string AssetId;
        public string Value;

        public HermitAction()
        {
            // Log the manifestation of a network-driven action payload
            ForgeLogger.Log("Hermit Action Payload manifested from Bridge.")
                .WithHeader("Network")
                .WithColor("#FFA500") // Vibrant Orange for external/network data
                .SendToUnity();
        }
    }
}