using System;
using System.Collections.Generic;
using UnityEngine;
using Workshop.Systems.MicroPackages;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Tools
{
    public class SFXStudioMicroPackage : IMicroPackage, IArchivalMetadata
    {
        // --- Core Identity ---
        public string PackageId { get; set; } = "Workshop.Tools.SFXStudio";

        // Mapped to standard Unity messages so the Arbitrator knows WHEN to run these compute groups
        public Dictionary<string, List<string>> ProcessGroupsPerUnityMessage { get; } = new Dictionary<string, List<string>>
        {
            {
                "FixedUpdate",
                new List<string> { "ComputeWaveSimulator", "AcousticBoundaryMapper" }
            }
        };

        // --- Architectural Intent ---
        // Universal because the acoustic math runs in the final game, even if the studio UI is Forge-only
        public PackageExecutionScope ExecutionScope => PackageExecutionScope.Universal;

        // Arrives via heavy architectural fabrication rather than a small data stream
        public string DeliveryProfileId => "Delivery_ArchitecturalFabrication";

        // Plays nicely with the ecosystem
        public string[] GetDependencies() => new string[0];
        public string[] GetExclusions() => new string[0];

        // ====================================================================
        // 🏛️ IArchivalMetadata Implementation (The Data Card)
        // ====================================================================
        public string Version { get; set; } = "1.0.0";
        public string Author { get; set; } = "The Singularity Workshop";
        public ForgeSpatialZone SpatialZone { get; set; } = ForgeSpatialZone.PhysicalAssets; // The Foundry

        public string ShortDescription { get; set; } = "A diegetic BIM-quality recording studio and acoustic simulator.";
        public string LongDescription { get; set; } = "Deploys a fully interactive, physical sound studio into the Forge. Allows creators to record, synthesize, and mix audio, or conduct virtual orchestras. Includes real-time compute shader wave simulation for acoustic propagation.";
        public string ThumbnailUrl { get; set; } = "Textures/Thumbnails/Archive_SFXStudio";

        public int ReleaseYear { get; set; } = DateTime.Now.Year;
        public string OriginalAuthor { get; set; } = "";
        public string OriginalPublisher { get; set; } = "";
        public string OriginalPlatform { get; set; } = "The Singularity";
        public string Era { get; set; } = "Modern Era";
        public string HistoricalSignificance { get; set; } = "Transforms abstract digital audio configuration into a tactile, spatial acoustic laboratory.";
        public Color AccentColor { get; set; } = new Color(0.8f, 0.5f, 0.2f); // Brass/Copper acoustic vibe

        // --- Lifecycle ---
        public void LoadPackage(IPackageArbitrator arbitrator)
        {
            Debug.Log($"<b>[Forge]</b> Loading {PackageId} into the Ecosystem...");

            // If the weapons package is installed, we could automatically hook into their 
            // collision events to generate dynamic impact acoustics here!
        }

        public void Arbitrate(IPackageArbitrator arbitrator)
        {
            Debug.Log($"<b>[Forge]</b> Arbitrating dependencies for {PackageId}. Synchronizing Acoustic Boundary Mapper.");
        }

        // --- The Diegetic Manifestation ---
        public void ManifestIntoWorld(Vector3 playerPosition, Vector3 playerForward)
        {
            Debug.Log("<b>[Forge]</b> Manifesting SFX Studio Diegetic Entrance...");

            // 1. Calculate spawn position
            Vector3 spawnPos = playerPosition + (Quaternion.Euler(0, 90, 0) * playerForward * 5f);

            // Temporary stub: Spawning a primitive wall until you build the Shader/Prefab
            GameObject impossibleWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            impossibleWall.name = "Diegetic_SFX_Wall";
            impossibleWall.transform.position = spawnPos;
            impossibleWall.transform.localScale = new Vector3(4f, 3f, 0.5f);

            // Hook up the Diegetic Interaction
            var trigger = impossibleWall.AddComponent<DiegeticPortalTrigger>();
            trigger.OnPlayerEnter += () => {
                Debug.Log("🔴 <b>[SFX Studio]</b> Recording Light ON! Transitioning to Audio Workspace...");
                // Execute actual teleport/workspace shift to the BIM interior here
            };
        }
    }

    // Stub to ensure compilation if not defined elsewhere
    public class DiegeticPortalTrigger : MonoBehaviour
    {
        public event Action OnPlayerEnter;
        private void OnTriggerEnter(Collider other) { OnPlayerEnter?.Invoke(); }
    }
}