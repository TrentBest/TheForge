namespace Workshop.UI_And_Tools.Showcase
{
    /// <summary>
    /// Pushed to the ForgeNetworkBus. Hermit's Cognitive Cortex listens for these.
    /// </summary>
    public struct GuiGenerationIntent
    {
        public string TargetTypeFullName;
        public string SerializedDataSchema; // The JSON representation of the type's fields/properties
        public string UserPrompt;           // e.g., "Make this look like a holographic RTS command terminal"
        public string TargetDomainID;       // e.g., "GrandCentralStation" or "SciFi_Hangout"
    }
}