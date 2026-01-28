using System.Collections.Generic;

namespace Assets.Scripts.MicroPackages
{
    public struct FSM
    {
        public string Name { get; }
        public string Description { get; }
        public List<State> States { get; }
        public List<Transition> Transitions { get; }
    }
}
