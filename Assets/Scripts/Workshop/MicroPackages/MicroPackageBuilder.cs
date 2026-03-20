using System.Collections.Generic;

namespace TheSingularityWorkshop.MicroPackages
{
    public class MicroPackageBuilder
    {
        private DynamicMicroPackage _package;

        public MicroPackageBuilder()
        {
            Reset();
        }

        public void Reset()
        {
            _package = new DynamicMicroPackage();
        }

        public MicroPackageBuilder WithPackageId(string id)
        {
            _package.PackageId = id;
            return this;
        }

        public MicroPackageBuilder AddProvider(IProvider provider)
        {
            if (provider != null && !_package.Providers.Contains(provider))
            {
                _package.Providers.Add(provider);
            }
            return this;
        }

        public MicroPackageBuilder AddArbitration(IPackageArbitrator.Arbitration arbitration)
        {
            _package.Arbitrations.Add(arbitration);
            return this;
        }

        public MicroPackageBuilder MapProcessGroup(string unityMessage, string processGroup)
        {
            if (!_package.ProcessGroupsPerUnityMessage.ContainsKey(unityMessage))
            {
                _package.ProcessGroupsPerUnityMessage[unityMessage] = new List<string>();
            }

            if (!_package.ProcessGroupsPerUnityMessage[unityMessage].Contains(processGroup))
            {
                _package.ProcessGroupsPerUnityMessage[unityMessage].Add(processGroup);
            }
            return this;
        }

        public IMicroPackage Build()
        {
            var builtPackage = _package;
            Reset(); // Prepare for the next build
            return builtPackage;
        }

        // Expose current state for the GUI to read
        public string CurrentPackageId => _package?.PackageId ?? string.Empty;
        public int ProviderCount => _package?.Providers.Count ?? 0;
        public int ArbitrationCount => _package?.Arbitrations.Count ?? 0;
    }
}