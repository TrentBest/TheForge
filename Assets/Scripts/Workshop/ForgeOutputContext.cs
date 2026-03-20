using System;

namespace TheSingularityWorkshop.Forge.AI
{
    internal class ForgeOutputContext
    {
        public Action<string> OnMessageReceived { get; internal set; }
    }
}