using System;
using UnityEngine;
using TheSingularityWorkshop.FSM_API;

namespace TheSingularityWorkshop.Armada2525.Economy
{

    public class Economy : IStateContext
    {
        public bool IsValid { get ; set ; }
        public string Name { get ; set ; }
    }
}