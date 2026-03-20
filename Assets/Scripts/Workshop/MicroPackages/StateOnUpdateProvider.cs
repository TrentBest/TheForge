using System.Collections.Generic;

namespace TheSingularityWorkshop.MicroPackages
{
    public class StateOnUpdateProvider : IStateOnUpdateProvider
    {
        public List<StateMethod> Provided { get; } = new List<StateMethod>();

        public int Id { get; }

        public ProviderType ProviderType => ProviderType.StateOnUpdate;

        public StateOnUpdateProvider(int id, List<StateMethod> provided)
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
