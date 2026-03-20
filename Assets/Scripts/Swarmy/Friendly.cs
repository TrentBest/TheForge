using System;
using System.Collections.Generic;
using System.Text;
using TheSingularityWorkshop.FSM_API;

namespace TheSingularityWorkshop.Swarmy
{
    public class Friendly : IStateContext
    {
        public bool IsValid { get  ; set  ; }
        public string Name { get  ; set  ; }
    }
}
