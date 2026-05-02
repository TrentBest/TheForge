using System;
using System.Collections.Generic;
using System.IO;
using Workshop.Core;
using Workshop.Core.Memory;
using Workshop.Core.Diagnostics; 
using Workshop.UI_And_Tools.Forge.IO;

// 1. Wrap the namespace leak
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Workshop.UI_And_Tools.Forge.Core
{
    // 2. Wrap the attribute
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public static class ForgeDomainConductor
    {
        public const string DUMP_FILE_PATH = "Temp/ForgeMemoryDump.bin";
        private static readonly List<IDomainPassenger> _passengers = new List<IDomainPassenger>();
        public static event Action OnBeforeDomainReload;
        public static event Action OnAfterDomainReload;

        // Keep this exposed so runtime scripts can still safely call it without breaking the build
        public static void IssueTicket(IDomainPassenger passenger)
        {
            if (!_passengers.Contains(passenger)) _passengers.Add(passenger);
        }

// 3. Lobotomize all Domain Reload and File I/O logic for WebGL
#if UNITY_EDITOR
        static ForgeDomainConductor()
        {
            AssemblyReloadEvents.beforeAssemblyReload -= HandleBeforeReload;
            AssemblyReloadEvents.beforeAssemblyReload += HandleBeforeReload;

            AssemblyReloadEvents.afterAssemblyReload -= HandleAfterReload;
            AssemblyReloadEvents.afterAssemblyReload += HandleAfterReload;
        }

        private static void HandleBeforeReload()
        {
            ForgeLogger.Log("All Aboard! Assembly reload impending.")
                .WithHeader("Domain Conductor").WithColor("#FF8C00").SendToUnity();

            if (EditorApplication.isCompiling)
            {
                ForgeLogger.LogWarning("Code Recompile Detected! Burning unmanaged memory cache to prevent struct mutation corruption. The Sovereign Kernel will reseed on wake.")
                    .WithHeader("Domain Conductor").SendToUnity();

                if (File.Exists(DUMP_FILE_PATH)) File.Delete(DUMP_FILE_PATH);
                return; // Abort the suspension. Do not save the zombie state!
            }

            ForgeLogger.Log("All Aboard! Assembly reload impending.")
                .WithHeader("Domain Conductor").WithColor("#FF8C00").SendToUnity();

            OnBeforeDomainReload?.Invoke();
            foreach (var passenger in _passengers) passenger.OnSuspend();

            ExecuteMemoryDumpTransaction();

            ForgeLogger.Log("Suspend sequence complete.")
                .WithHeader("Domain Conductor").WithColor("#FF8C00").SendToUnity();
        }

        private static void HandleAfterReload()
        {
            ForgeLogger.Log("Disembarking! Rehydrating sovereign state.")
                .WithHeader("Domain Conductor").WithColor("#32CD32").SendToUnity();

            ExecuteMemoryRehydrateTransaction();

            foreach (var passenger in _passengers) passenger.OnResume();
            OnAfterDomainReload?.Invoke();

            ForgeLogger.Log("Resume sequence complete.")
                .WithHeader("Domain Conductor").WithColor("#32CD32").SendToUnity();
        }

        private static void ExecuteMemoryDumpTransaction()
        {
            if (SingularityBootloader.MainWarehouse == null) return;

            using (var stream = File.Open(DUMP_FILE_PATH, FileMode.Create))
            using (var writer = new BinaryWriter(stream))
            {
                SingularityBootloader.MainWarehouse.ExportEcosystem(writer);
            }

            // --- THE LEAK FIX ---
            // Manually purge the unmanaged memory before the AppDomain drops.
            SingularityBootloader.MainWarehouse.Dispose();
        }

        private static void ExecuteMemoryRehydrateTransaction()
        {
            if (SingularityBootloader.MainWarehouse == null)
            {
                SingularityBootloader.IgniteSingularity();
            }

            if (!File.Exists(DUMP_FILE_PATH)) return;

            using (var stream = File.OpenRead(DUMP_FILE_PATH))
            using (var reader = new BinaryReader(stream))
            {
                SingularityBootloader.MainWarehouse.ImportEcosystem(reader);
            }

            File.Delete(DUMP_FILE_PATH);
        }
#endif
    }
}