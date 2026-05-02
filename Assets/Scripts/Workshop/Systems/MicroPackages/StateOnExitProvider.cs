using System.Collections.Generic;

namespace Workshop.Systems.MicroPackages
{
    public class StateOnExitProvider : IStateOnExitProvider
    {
        public List<StateMethod> Provided { get; } = new List<StateMethod>();

        public int Id { get; }

        public ProviderType ProviderType => ProviderType.StateOnExit;

        public StateOnExitProvider(int id, List<StateMethod> provided)
        {
            Id = id;
            Provided = provided;
        }

        public void Add(StateMethod method)
        {
            Provided.Add(method);
        }

        public void Remove(StateMethod method)
        {
            Provided.Remove(method);
        }
    }
}
