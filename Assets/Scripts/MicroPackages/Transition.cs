using TheSingularityWorkshop.FSM_API;

namespace Assets.Scripts.MicroPackages
{
    public struct Transition
    {
        public string From { get; }
        public string To { get; }
        public string Description { get; }

        public FSMTransition transition { get; }
    }
}
