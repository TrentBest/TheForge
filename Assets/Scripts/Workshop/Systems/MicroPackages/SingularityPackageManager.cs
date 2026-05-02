using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Workshop.Systems.MicroPackages
{
    /// <summary>
    /// Handles the physical I/O, fetching, and caching of DynamicMicroPackages.
    /// Acts as the bridge between the hard drive (Gondola Yard) and the UI/Arbitrator.
    /// </summary>
    public static class SingularityPackageManager
    {
        // Define the local storage path for downloaded/created packages
        private static string StorageDirectory => Path.Combine(Application.persistentDataPath, "GondolaYard_Packages");

        /// <summary>
        /// Ensures the physical directory exists on the hard drive.
        /// </summary>
        private static void InitializeYard()
        {
            if (!Directory.Exists(StorageDirectory))
            {
                Directory.CreateDirectory(StorageDirectory);
                Debug.Log($"<b>[Gondola Yard]</b> Initialized local package storage at: {StorageDirectory}");
            }
        }

        /// <summary>
        /// Scans the local hard drive and parses all saved .pkg files.
        /// This unblocks the onFetchAll call in your MicroPackages_Gui_PackageDashboard.
        /// </summary>
        public static List<DynamicMicroPackage> GetLocalPackages()
        {
            InitializeYard();
            List<DynamicMicroPackage> localPackages = new List<DynamicMicroPackage>();

            string[] packageFiles = Directory.GetFiles(StorageDirectory, "*.singularitypkg");

            foreach (var filePath in packageFiles)
            {
                try
                {
                    string json = File.ReadAllText(filePath);

                    // NOTE: Standard JsonUtility struggles with interfaces like IProvider.
                    // For a robust system, you will eventually want to use Newtonsoft.Json here 
                    // with a TypeNameHandling setting to maintain polymorphism.
                    DynamicMicroPackage pkg = JsonUtility.FromJson<DynamicMicroPackage>(json);

                    if (pkg != null)
                    {
                        localPackages.Add(pkg);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"<b>[Gondola Yard]</b> Failed to unpack crate at {filePath}: {ex.Message}");
                }
            }

            Debug.Log($"<b>[Gondola Yard]</b> Inventory complete. Found {localPackages.Count} crates on the dock.");
            return localPackages;
        }

        /// <summary>
        /// Saves a constructed DynamicMicroPackage to the hard drive.
        /// </summary>
        public static void SaveLocalPackage(DynamicMicroPackage package)
        {
            InitializeYard();

            if (package == null || string.IsNullOrWhiteSpace(package.PackageId))
            {
                Debug.LogError("<b>[Gondola Yard]</b> Cannot save a null or unnamed package.");
                return;
            }

            string safeFileName = package.PackageId.Replace(".", "_") + ".singularitypkg";
            string fullPath = Path.Combine(StorageDirectory, safeFileName);

            try
            {
                string json = JsonUtility.ToJson(package, true);
                File.WriteAllText(fullPath, json);
                Debug.Log($"<b>[Gondola Yard]</b> Crate packed and secured: {package.PackageId}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"<b>[Gondola Yard]</b> Failed to secure crate {package.PackageId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes a physical package file from the local drive.
        /// </summary>
        public static void DeleteLocalPackage(string packageId)
        {
            InitializeYard();
            string safeFileName = packageId.Replace(".", "_") + ".singularitypkg";
            string fullPath = Path.Combine(StorageDirectory, safeFileName);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                Debug.Log($"<b>[Gondola Yard]</b> Crate incinerated: {packageId}");
            }
        }
    }
}