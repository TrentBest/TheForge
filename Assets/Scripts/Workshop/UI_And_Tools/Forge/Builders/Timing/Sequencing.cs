using System;
using Workshop.Systems.MicroPackages;

namespace Workshop.UI_And_Tools.Forge.Builders.Staging
{
    public interface ISequenceNode
    {
        string NodeName { get; }
        Type TargetDataContext { get; } // e.g., typeof(CosmicCellContext)
        float Duration { get; }
    }

    public class SequenceNode : ISequenceNode
    {
        public string NodeName { get; private set; }
        public Type TargetDataContext { get; private set; }
        public float Duration { get; private set; }

        public SequenceNode(string name, Type contextType, float duration)
        {
            NodeName = name;
            TargetDataContext = contextType;
            Duration = duration;
        }
    }
}