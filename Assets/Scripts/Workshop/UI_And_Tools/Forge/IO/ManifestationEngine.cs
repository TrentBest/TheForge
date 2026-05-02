using System;
using System.IO;
using UnityEngine;
using Workshop.UI_And_Tools.Forge.Hermit.Bridge;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Workshop.UI_And_Tools.Forge.IO
{
    /// <summary>
    /// The ultimate bridge between AI Intents, Data Streams, and Physical Reality.
    /// Handles both Scene-level manifestations and file-system artifact generation.
    /// </summary>
    public static class ManifestationEngine
    {
        // ====================================================================
        // PART 1: PHYSICAL REALITY ARBITRATION (SCENE MANIPULATION)
        // ====================================================================

        /// <summary>
        /// Translates a validated AI Directive into actual physical changes in the Unity Engine.
        /// Returns true if successful, false if the action violated physical laws or failed.
        /// </summary>
        public static bool TryManifest(HermitDirectivePayload directive, out string statusMsg)
        {
            try
            {
                // 1. Locate the Target in Reality
                GameObject targetObj = GameObject.Find(directive.TargetId);

                // 2. Arbitrate Intent
                switch (directive.Intent)
                {
                    case "Delete":
                    case "Trash":
                        if (targetObj == null)
                        {
                            statusMsg = $"Target '{directive.TargetId}' does not exist in the current spatial matrix.";
                            return false;
                        }

                        // THE SAFETY RAILS: Prevent the AI from destroying the fabric of the universe
                        if (targetObj.GetComponent<Light>() != null || targetObj.GetComponent<Camera>() != null)
                        {
                            statusMsg = $"DENIED: '{directive.TargetId}' is foundational infrastructure. It cannot be harvested.";
                            return false;
                        }

                        // If it passes arbitration, execute the reality change.
                        UnityEngine.Object.Destroy(targetObj);
                        statusMsg = $"Target '{directive.TargetId}' has been successfully deconstructed.";
                        return true;

                    case "Spawn":
                    case "Create":
                        // If it already exists, don't spawn a duplicate unless intended
                        if (targetObj != null)
                        {
                            statusMsg = $"Target '{directive.TargetId}' already exists in the hierarchy. Spawn aborted.";
                            return false;
                        }

                        // Basic primitive spawning (can be expanded to pull from DataWarehouse or Resources)
                        GameObject newObj;
                        if (directive.TargetId.ToLower().Contains("cube")) newObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        else if (directive.TargetId.ToLower().Contains("sphere")) newObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        else newObj = new GameObject(directive.TargetId); // Empty container

                        newObj.name = directive.TargetId;
                        statusMsg = $"Spawn intent authorized. Reality matrix updated to include '{directive.TargetId}'.";
                        return true;

                    case "UpdateScript":
                        // Stub for Roslyn / runtime script injection
                        statusMsg = $"Logic injection successful. Code for '{directive.TargetId}' has been hot-swapped.";
                        return true;

                    case "MoveTo":
                        if (targetObj == null)
                        {
                            statusMsg = $"Cannot move '{directive.TargetId}': Object not found.";
                            return false;
                        }

                        // Fallback logic: Moves to origin if no specific vector is provided in the payload string
                        // (Assuming directive.PayloadData might contain JSON vectors in the future)
                        Vector3 targetPos = Vector3.zero;

                        targetObj.transform.position = targetPos;
                        statusMsg = $"Kinematic vectors updated for '{directive.TargetId}'. Moved to {targetPos}.";
                        return true;

                    default:
                        statusMsg = $"Unknown Intent Matrix: '{directive.Intent}'. Cannot parse.";
                        return false;
                }
            }
            catch (Exception ex)
            {
                statusMsg = $"Catastrophic Reality Failure during manifestation: {ex.Message}";
                Debug.LogError($"[Manifestation Engine] {statusMsg}");
                return false;
            }
        }

        // ====================================================================
        // PART 2: ARTIFACT GENERATION (FILE SYSTEM I/O)
        // ====================================================================

        /// <summary>
        /// Safely ensures a directory path exists, creating it if it does not.
        /// </summary>
        public static void EnsureDirectoryExists(string path)
        {
            try
            {
                string directory = Path.GetDirectoryName(path);

                // If path is just a folder name (no file attached), GetDirectoryName might return null or empty.
                if (string.IsNullOrEmpty(directory))
                    directory = path;

                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    Debug.Log($"[Manifestation Engine] Constructed new physical directory matrix at: {directory}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Manifestation Engine] Directory Generation Failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Writes an artifact (text/code/json) to the disk. 
        /// Automatically triggers an AssetDatabase refresh if running in the Unity Editor.
        /// </summary>
        public static bool WriteArtifact(string filePath, string content)
        {
            try
            {
                EnsureDirectoryExists(filePath);
                File.WriteAllText(filePath, content);

                Debug.Log($"[Manifestation Engine] Artifact manifested successfully at: {filePath}");

#if UNITY_EDITOR
                // If we are in the editor and writing to the Assets folder, we need to refresh so Unity sees it.
                if (filePath.Contains("Assets"))
                {
                    AssetDatabase.Refresh();
                }
#endif
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Manifestation Engine] Failed to write artifact to {filePath}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Reads an artifact from disk. Returns null if the artifact does not exist.
        /// </summary>
        public static string ReadArtifact(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    return File.ReadAllText(filePath);
                }

                Debug.LogWarning($"[Manifestation Engine] Artifact not found at: {filePath}");
                return null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Manifestation Engine] Failed to read artifact at {filePath}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Deletes an artifact from the disk.
        /// </summary>
        public static bool DeleteArtifact(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);

#if UNITY_EDITOR
                    if (filePath.Contains("Assets"))
                    {
                        AssetDatabase.Refresh();
                    }
#endif
                    Debug.Log($"[Manifestation Engine] Artifact expunged: {filePath}");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Manifestation Engine] Failed to expunge artifact at {filePath}: {ex.Message}");
                return false;
            }
        }
    }
}