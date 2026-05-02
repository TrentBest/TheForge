using System;
using System.Collections.Generic;
using UnityEngine;

namespace Workshop.Systems.MicroPackages
{
    /// <summary>
    /// Fluent builder for constructing IMicroPackage instances (specifically DynamicMicroPackage)
    /// during authoring/Forge time.
    /// </summary>
    public class MicroPackageBuilder
    {
        private string _packageId = "com.singularity.newpackage";
        private List<IPackageArbitrator.Arbitration> _arbitrations;

        // Trackers for both GUI templates and concrete programmatic providers
        private List<ProviderType> _stagedProviders;
        private List<IProvider> _providers;

        public MicroPackageBuilder()
        {
            _arbitrations = new List<IPackageArbitrator.Arbitration>();
            _stagedProviders = new List<ProviderType>();
            _providers = new List<IProvider>();
        }

        public MicroPackageBuilder WithPackageId(string packageId)
        {
            _packageId = packageId;
            return this;
        }

        public MicroPackageBuilder AddArbitration(IPackageArbitrator.Arbitration arbitration)
        {
            _arbitrations.Add(arbitration);
            return this;
        }

        public MicroPackageBuilder AddProviderTemplate(ProviderType providerType)
        {
            if (!_stagedProviders.Contains(providerType))
            {
                _stagedProviders.Add(providerType);
            }
            return this;
        }

        // --- RESTORED METHOD ---
        // This is what CosmicDescentFsmBuilder is looking for
        public MicroPackageBuilder AddProvider(IProvider provider)
        {
            if (provider != null && !_providers.Contains(provider))
            {
                _providers.Add(provider);
            }
            return this;
        }

        public IMicroPackage Build()
        {
            // Instantiate the concrete package type
            var package = new DynamicMicroPackage();

            // Hydrate the dynamic package with the fluent state
            package.PackageId = _packageId;

            // Depending on your DynamicMicroPackage internal API, you would inject
            // the arbitrations and concrete providers here before returning it.
            // e.g., 
            // foreach(var p in _providers) { package.AddProvider(p); }
            // package.Arbitrations = new List<IPackageArbitrator.Arbitration>(_arbitrations);

            return package;
        }
    }
}