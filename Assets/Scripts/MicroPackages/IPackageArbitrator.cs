using System.Collections.Generic;

namespace Assets.Scripts.MicroPackages
{
    public interface IPackageArbitrator
    {
        List<string> InstalledPackages { get; }
        Dictionary<string, List<string>> DefinedProcessGroupsPerUnityMessage { get; }

        public struct Arbitration
        {
            public string requestingPackage;
            public string targetedPackage;
            public ArbitrationType arbitrationType;
            public object payload;
        }

        public enum ArbitrationType
        {
            Invalid = 0,
            Additive = 1,//Adding something
            Subtractive = 2,//Removing something
            Modification = 3,//Modifying something
        }

        void SubmitArbitration(Arbitration arbitration);
        //MetaDevData GetAggregatedMetaDev();
    }
}
