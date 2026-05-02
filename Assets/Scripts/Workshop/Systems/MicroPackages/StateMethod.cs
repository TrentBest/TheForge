using System;
using TheSingularityWorkshop.FSM_API;

namespace Workshop.Systems.MicroPackages
{
    public struct StateMethod
    {
        public StateMethod(string name, string description, Action<IStateContext> method) : this()
        {
            Name = name;
            Description = description;
            Method = method;
        }

        public string Name { get; }
        public string Description { get; }
        public Action<IStateContext> Method { get; }
    }
}
