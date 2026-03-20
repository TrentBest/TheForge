using TheSingularityWorkshop.MicroPackages;
using System.Collections.Generic;
using UnityEngine;

public class ScimitirSword : IMicroPackage
{
    public Dictionary<string, List<string>> ProcessGroupsPerUnityMessage { get; } = new Dictionary<string, List<string>>();

    public ScimitirSword()
    {
        ProcessGroupsPerUnityMessage.Add("Update", new List<string> { "Update" });
    }

    public void Arbitrate(IPackageArbitrator arbitrator)
    {
        throw new System.NotImplementedException();
    }

    public void LoadPackage(IPackageArbitrator arbitrator)
    {
        throw new System.NotImplementedException();
    }
}
