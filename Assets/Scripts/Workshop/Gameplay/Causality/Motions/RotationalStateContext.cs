using System;
using System.Collections.Generic;
using System.Text;
using TheSingularityWorkshop.FSM_API;
using UnityEngine;

namespace Workshop.Gameplay.Causality.Motions
{
    class RotationalStateContext : IStateContext
    {
        public bool IsValid { get ; set ; } = false;
        public string Name { get ; set; }

        public RotationalStateContext(Transform transform)
        {

        }
    }
}
