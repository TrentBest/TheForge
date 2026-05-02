using System;

namespace Workshop.GURPS
{
    /// <summary>
    /// The Atomic Unit of Technological Advancement.
    /// Defines the capabilities and knowledge constraints for a specific era.
    /// </summary>
    [Serializable]
    public class GURPSTechDefinition
    {
        public int TL { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}