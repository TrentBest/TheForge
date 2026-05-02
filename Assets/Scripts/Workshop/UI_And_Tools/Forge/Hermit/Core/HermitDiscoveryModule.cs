using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Workshop.UI_And_Tools.Forge.Core;

namespace Workshop.UI_And_Tools.Forge.Hermit.Core
{
    /// <summary>
    /// The logic that inverts affordance to prioritize the "un-pushed" buttons.
    /// </summary>
    public class HermitDiscoveryModule
    {
        public PerceptionData GetHighestPriorityTarget(List<PerceptionData> sensedObjects)
        {
            // Inverse logic: Order by NeglectScore (distance * time since last touch)
            return sensedObjects
                .OrderByDescending(o => o.NeglectScore)
                .FirstOrDefault();
        }
    }
}