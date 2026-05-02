using System.Collections.Generic;
using TheSingularityWorkshop.FSM_API;
using UnityEngine;

namespace Assets.Scripts.MastersOfOrionII
{
    public interface IFreighterDispatch : IStateContext
    {
        void NotifyArrivalAtColony(int freighterId, int colonyId);
        void NotifyDeparture(int freighterId, int departureColonyId, int destinationColonyId);

        void NotifyCargoLoaded(int freighterId);
        void NotifyCargoUnloaded(int freighterId);

        void RequestAssignment(int freighterId);
        List<Vector3> RequestFlightPlan(int freighterId, int colonyId);
    }
}
