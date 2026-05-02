using System;
using Workshop.Core.Diagnostics;
using Workshop.UI_And_Tools.Forge.IO;

namespace Workshop
{
    public class ForgeInputContext
    {
        private readonly string _agentId;

        public ForgeInputContext(string agentId)
        {
            _agentId = agentId;
        }

        /// <summary>
        /// Universal entry point for Agent intent. 
        /// Routes thoughts directly into the Forge's arbitration nervous system.
        /// </summary>
        public void SubmitInput(string value)
        {
            // If the Bus is online, push the data to the central nervous system
            if (SingularityDataBus.Instance != null)
            {
                // We target "Hermit_Output" so the Manifestation Chamber UI picks it up
                SingularityDataBus.Instance.SendLocal("Hermit_Output", value);
            }
            else
            {
                ForgeLogger.LogWarning($"[ForgeInputContext] Agent {_agentId} attempted to submit input, but the Data Bus is offline.");
            }
        }
    }
}