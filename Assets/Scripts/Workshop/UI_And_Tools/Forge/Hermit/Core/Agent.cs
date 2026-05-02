using System;
using Workshop;
using Workshop.Core.Diagnostics; // Injected for ForgeLogger

namespace Workshop.UI_And_Tools.Forge.Core
{
    public class Agent
    {
        public string AgentId { get; private set; }
        public ForgeOutputContext OutputContext { get; set; }
        public ForgeInputContext InputContext { get; set; }

        public Agent(string agentId = null)
        {
            // Ensure every agent has a unique identifier in the Forge
            AgentId = string.IsNullOrEmpty(agentId) ? Guid.NewGuid().ToString() : agentId;

            OutputContext = new ForgeOutputContext();
            InputContext = new ForgeInputContext(AgentId);

            ForgeLogger.Log($"New Agent Manifested: {AgentId}")
                .WithHeader("Identity")
                .WithColor("#FF00FF") // Magenta for Identity/Agent creation
                .SendToUnity();
        }
    }
}