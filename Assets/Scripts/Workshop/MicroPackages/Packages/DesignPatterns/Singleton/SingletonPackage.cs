using TheSingularityWorkshop.MicroPackages;
using System;
using System.Collections.Generic;
using System.Text;
using TheSingularityWorkshop.FSM_API;

namespace TheSingularityWorkshop.MicroPackages.Packages.DesignPatterns.Singleton
{
    public class SingletonPackage : IMicroPackage
    {
        public Dictionary<string, List<string>> ProcessGroupsPerUnityMessage { get; } = new Dictionary<string, List<string>>();

        public void Arbitrate(IPackageArbitrator arbitrator)
        {
           
        }

        public void LoadPackage(IPackageArbitrator arbitrator)
        {
            
        }
    }

    public class SingletonContext : IStateContext
    {
        public bool IsValid { get  ; set  ; }
        public string Name { get  ; set  ; }

        public SingletonContext(IStateContext singleton)
        {

        }
    }
}
