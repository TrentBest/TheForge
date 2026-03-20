using System.Collections.Generic;

namespace TheSingularityWorkshop.MicroPackages
{
    /// <summary>
    /// A runtime-configurable implementation of a MicroPackage.
    /// </summary>
    public class DynamicMicroPackage : IMicroPackage
    {
        public string PackageId { get; set; }

        public Dictionary<string, List<string>> ProcessGroupsPerUnityMessage { get; set; }
            = new Dictionary<string, List<string>>();

        // Storing the arbitrations and providers locally until loaded
        public List<IPackageArbitrator.Arbitration> Arbitrations { get; set; } = new List<IPackageArbitrator.Arbitration>();
        public List<IProvider> Providers { get; set; } = new List<IProvider>();

        public void LoadPackage(IPackageArbitrator arbitrator)
        {
            foreach (var provider in Providers)
            {
                arbitrator.AddProvider(provider);
            }
        }

        public void Arbitrate(IPackageArbitrator arbitrator)
        {
            foreach (var arbitration in Arbitrations)
            {
                arbitrator.SubmitArbitration(arbitration);
            }
        }
    }
}