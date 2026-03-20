using TheSingularityWorkshop.MicroPackages.Providers;
using TheSingularityWorkshop.MicroPackages;
using System.Collections.Generic;

namespace TheSingularityWorkshop.MicroPackages.Packages.DigitalLogic
{
    public class Visualization : IMicroPackage
    {
        public Dictionary<string, List<string>> ProcessGroupsPerUnityMessage { get; } = new();

        public void Arbitrate(IPackageArbitrator arbitrator) => arbitrator.None();

        public void LoadPackage(IPackageArbitrator arbitrator)
        {
            // Decoding bits for visual displays
            //arbitrator.AddProvider(new FsmStateOnUpdateActionProvider(
            //    new StateMethod("DECODE_7SEGMENT", "Maps 4-bit BCD to segments a-g",
            //    ctx => { /* Mapping logic */ })));
        }
    }
}