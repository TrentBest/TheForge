using System;
using TheSingularityWorkshop.FSM_API;

namespace Workshop.Systems.MicroPackages
{
    public class StateOnEnterProvider : IStateOnEnterProvider
    {
        public Action<IStateContext> Provided { get; }

        public int Id { get; }

        public ProviderType ProviderType => ProviderType.StateOnEnter;

        public StateOnEnterProvider(int id, Action<IStateContext> provided)
        {
            Id = id;
            Provided = provided;
        }
    }
}
