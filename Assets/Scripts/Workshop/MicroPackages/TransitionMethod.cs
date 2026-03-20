using System;
using TheSingularityWorkshop.FSM_API;

namespace TheSingularityWorkshop.MicroPackages
{
    public struct TransitionMethod
    {
        public TransitionMethod(string fromState, string toState, Func<IStateContext, bool> value) 
        {
            Method = value;
            Name = $"TransitionFrom:{fromState}To:{toState}";
            Description = Name;
        }


        public TransitionMethod(string fromState, string toState, string description, Func<IStateContext, bool> value)
        {
            Method = value;
            Name = $"TransitionFrom:{fromState}To:{toState}";
            Description = description;
        }

        public string Name { get; }
        public string Description { get; }
        public Func<IStateContext, bool> Method { get; }
    }
}
