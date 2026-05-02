using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Workshop.Core.Memory;
using Workshop.Systems.MetaDev;
using static Workshop.Systems.MicroPackages.IPackageArbitrator;

namespace Workshop.Systems.MicroPackages
{
    /// <summary>
    /// The central conductor of the Workshop ecosystem.
    /// Manages package lifecycle, exclusion walls, and the convergence of complex logic.
    /// </summary>
    public class ForgeArbitrator : IPackageArbitrator
    {
        // --- Core Repositories ---
        public DataWarehouse Warehouse { get; } = new DataWarehouse();
        public List<string> InstalledPackages { get; } = new List<string>();
        public Dictionary<string, List<string>> DefinedProcessGroupsPerUnityMessage { get; } = new();

        // --- Internal State ---
        private Dictionary<string, IMicroPackage> _activePackages = new();
        private Queue<Arbitration> _pendingArbitrations = new();
        private bool _isConverging = false;

        public void LoadPackage(IMicroPackage package)
        {
            if (package == null || InstalledPackages.Contains(package.PackageId)) return;

            // === THE EXCLUSION WALL ===
            // Prevents hostile logic boards from co-existing in the same memory space.
            var incomingExclusions = new List<string>(package.GetExclusions());

            foreach (var installedId in InstalledPackages)
            {
                // 1. Does the incoming package exclude an installed one?
                if (incomingExclusions.Contains(installedId))
                {
                    Debug.LogError($"<b>[Arbitrator]</b> 🛑 HOSTILITY BLOCKED: '{package.PackageId}' excludes '{installedId}'.");
                    return;
                }

                // 2. Does an installed package exclude the incoming one?
                var installedPkg = _activePackages[installedId];
                var installedExclusions = new List<string>(installedPkg.GetExclusions());
                if (installedExclusions.Contains(package.PackageId))
                {
                    Debug.LogError($"<b>[Arbitrator]</b> 🛑 HOSTILITY BLOCKED: '{installedId}' excludes '{package.PackageId}'.");
                    return;
                }
            }

            Debug.Log($"<b>[Arbitrator]</b> 📦 Loading Package: {package.PackageId}");

            _activePackages.Add(package.PackageId, package);
            InstalledPackages.Add(package.PackageId);

            // Phase 1: Initialization
            package.LoadPackage(this);

            // Phase 2: Converge (Negotiation Phase)
            if (!_isConverging)
            {
                RunConvergenceLoop();
            }
        }

        public void SubmitArbitration(Arbitration arbitration)
        {
            _pendingArbitrations.Enqueue(arbitration);
        }

        private void RunConvergenceLoop()
        {
            _isConverging = true;
            int iterations = 0;
            bool stateChanged = true;

            // Loop until homeostasis is reached or we hit the safety ceiling
            while (stateChanged && iterations < 10)
            {
                stateChanged = false;
                iterations++;
                Debug.Log($"\n<b>[Arbitrator]</b> --- Convergence Pass {iterations} ---");

                // 1. Allow all packages to survey the ecosystem and submit changes
                foreach (var pkg in _activePackages.Values)
                {
                    pkg.Arbitrate(this);
                }

                // 2. Execute all submitted arbitration commands
                while (_pendingArbitrations.Count > 0)
                {
                    var command = _pendingArbitrations.Dequeue();
                    ApplyArbitrationCommand(command);
                    stateChanged = true;
                }
            }

            if (iterations >= 10)
            {
                Debug.LogWarning("<b>[Arbitrator]</b> 🛑 Convergence limit reached. Manual stabilization required.");
            }
            else
            {
                Debug.Log($"<b>[Arbitrator]</b> ✅ Homeostasis reached in {iterations} passes.\n");
            }

            _isConverging = false;
        }

        public void UninstallPackage(string targetPackageId)
        {
            if (!InstalledPackages.Contains(targetPackageId)) return;

            Debug.Log($"<b>[Arbitrator]</b> Commencing purge of {targetPackageId}");
            _activePackages.Remove(targetPackageId);
            InstalledPackages.Remove(targetPackageId);

            // Re-simulating from the new baseline
            if (!_isConverging)
            {
                RunConvergenceLoop();
            }
        }

        private void ApplyArbitrationCommand(Arbitration arb)
        {
            Debug.Log($"<b>[Arbitrator]</b> Applying {arb.arbitrationType} from {arb.requestingPackage} -> {arb.targetedPackage}.");

            // Telemetry Handshake: Record the delta for the Experience Manifest
            string payloadString = arb.payload?.ToString() ?? "NULL_PAYLOAD";
            MetaDevObserver.RecordArbitration(arb.targetedPackage, payloadString);

            // Functional Handshake: Inject data into the DataWarehouse
            Warehouse.InjectData(arb.targetedPackage, arb.payload);
        }

        /// <summary>
        /// Fast-boots a published experience using a baked MetaDev manifest.
        /// </summary>
        public void BootFromManifest(ExperienceManifest manifest)
        {
            Debug.Log($"<b>[Arbitrator]</b> Fast-Booting: {manifest.ExperienceId} (v{manifest.Version})");

            foreach (string pkgId in manifest.PackageBootOrder)
            {
                var package = FetchPackageFromYard(pkgId);

                if (package != null)
                {
                    _activePackages.Add(package.PackageId, package);
                    InstalledPackages.Add(package.PackageId);

                    // Hydrate pre-calculated configurations
                    if (manifest.PackageConfigurationDeltas.TryGetValue(pkgId, out var deltas))
                    {
                        foreach (var delta in deltas)
                        {
                            ApplyArbitrationCommand(new Arbitration
                            {
                                targetedPackage = pkgId,
                                payload = delta,
                                arbitrationType = ArbitrationType.Modification
                            });
                        }
                    }
                    package.LoadPackage(this);
                }
            }
        }

        private IMicroPackage FetchPackageFromYard(string id)
        {
            var localInventory = SingularityPackageManager.GetLocalPackages();
            return localInventory.FirstOrDefault(p => p.PackageId == id);
        }

        public void None() { }

        // Interface compliance for provider registration
        public void AddProvider(IProvider provider)
        {
            // Log or store the provider in the specific registry
        }
    }
}