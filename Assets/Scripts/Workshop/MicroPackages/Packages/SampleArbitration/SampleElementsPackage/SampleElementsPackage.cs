using TheSingularityWorkshop.MicroPackages;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace TheSingularityWorkshop.MicroPackages.Packages.SampleArbitration.SampleElementsPackage
{
    public class SampleElementsPackage : MonoBehaviour, IMicroPackage
    {
        
        public Dictionary<string, List<string>> ProcessGroupsPerUnityMessage  { get; } = new Dictionary<string, List<string>>();

        public void Arbitrate(IPackageArbitrator arbitrator)
        {
            
        }

        public void LoadPackage(IPackageArbitrator arbitrator)
        {
            //arbitrator.LoadPackage();

        }
    }
}
