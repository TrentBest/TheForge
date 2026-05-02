using System;
using UnityEngine;
using TheSingularityWorkshop.FSM_API;

namespace Assets.Scripts.MastersOfOrionII.Economy
{

    public class Economy : IStateContext
    {
        public bool IsValid { get ; set ; }
        public string Name { get ; set ; }
    }
}