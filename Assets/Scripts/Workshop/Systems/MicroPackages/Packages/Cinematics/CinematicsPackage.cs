using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Systems.MicroPackages;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.Systems.MicroPackages.Packages.Cinematics
{
    /// <summary>
    /// The Cinematics Micro-Package: The Director of the Singularity.
    /// Orchestrates camera behavior, actor staging, and high-impact arrival sequences.
    /// </summary>
    public class CinematicsPackage : IMicroPackage, IArchivalMetadata, IGuiProvider
    {
        // --- Core Identity ---
        public string PackageId { get; set; } = "Workshop.Cinematics.Core";
        public Dictionary<string, IProvider> Providers { get; } = new Dictionary<string, IProvider>();

        /// <summary>
        /// Cinematic updates are slaved to LateUpdate to ensure the camera 
        /// tracks the final resolved position of all physics-driven actors.
        /// </summary>
        public Dictionary<string, List<string>> ProcessGroupsPerUnityMessage { get; } = new Dictionary<string, List<string>>
        {
            { "LateUpdate", new List<string> { "Camera_Orchestration", "Timeline_PostProcess_Sync" } }
        };

        // --- Architectural Intent ---
        public PackageExecutionScope ExecutionScope => PackageExecutionScope.Universal;

        // Cinematic assets require high-bandwidth delivery for textures and post-process profiles
        public string DeliveryProfileId => "Delivery_ProductionSled";

        // Cinematics rely heavily on the FSM engine to trigger sequence transitions
        public string[] GetDependencies() => new[] { "Workshop.Core.FSM" };

        public string[] GetExclusions() => new string[0];

        #region --- IArchivalMetadata (The Data Card) ---

        public string Version => "1.0.0";
        public string Author => "The Singularity Workshop";
        public ForgeSpatialZone SpatialZone => ForgeSpatialZone.PhysicalAssets; // The Foundry

        public string ShortDescription => "Directorial tools for cosmic cinematic sequences.";
        public string LongDescription => "Provides foundational logic for camera direction, actor staging, and timeline-based orchestration. Includes the Cosmic Descent Director for high-impact arrival sequences and automated cinematic framing.";
        public string ThumbnailUrl => "Textures/Thumbnails/Archive_Cinematics_Director";

        public int ReleaseYear => DateTime.Now.Year;
        public string OriginalAuthor => "Trent Best";
        public string OriginalPublisher => "The Singularity Workshop";
        public string OriginalPlatform => "The Forge";
        public string Era => "Emergence";
        public string HistoricalSignificance => "The lens through which the Singularity is observed; bridges the gap between raw simulation and narrative drama.";
        public Color AccentColor => new Color(0.9f, 0.7f, 0.2f); // Gold / Hollywood Spotlight

        #endregion

        public CinematicsPackage()
        {
            // Placeholder for the Director Provider
            // Providers.Add("CosmicDescentDirector", new CosmicDescentDirectorProvider());
        }

        /// <summary>
        /// Phase 1: Injection.
        /// Register the cinematic directors with the Arbitrator.
        /// </summary>
        public void LoadPackage(IPackageArbitrator arbitrator)
        {
            Debug.Log($"<b>[The Foundry]</b> {PackageId} online. Ready for principal photography.");

            foreach (var provider in Providers.Values)
            {
                arbitrator.AddProvider(provider);
            }
        }

        /// <summary>
        /// Phase 2: Convergence.
        /// Cinematics can auto-wire to 'Weapons' packages to trigger slow-motion 
        /// or 'impact' camera shakes automatically.
        /// </summary>
        public void Arbitrate(IPackageArbitrator arbitrator)
        {
            if (arbitrator.InstalledPackages.Contains("Workshop.Sample.Weapons"))
            {
                Debug.Log($"<b>[Cinematics]</b> Weapons detected. Initializing Kinetic Impact Camera Shake hooks.");
            }
        }

        // --- IGuiProvider (Director's Dashboard) ---

        public string Title => "CINEMATIC CONTROL UNIT";

        public VisualElement CreateGui(GuiContext ctx)
        {
            var rootBuilder = new GraphicalUserInterfaceBuilder("Cinematic_Dashboard")
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Center)
                .WithPadding(15)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f))
                .AddChild(new ForgeLabelBuilder("DIRECTOR'S VIEWFINDER").WithFontSize(20).WithBold())
                .AddChild(new ForgeSeparatorBuilder(AccentColor))
                .AddChild(new ForgeLabelBuilder("Camera Status: STANDBY").WithColor(Color.gray).WithMarginBottom(10));

            // Controls for FOV, Depth of Field, etc.
            rootBuilder.AddChild(new ForgeButtonBuilder("🎬 INITIATE COSMIC DESCENT", () => Debug.Log("Sequence Started!"))
                .WithBackgroundColor(AccentColor)
                .WithTextColor(Color.black));

            return rootBuilder.Build();
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) { }
        public void FromUIDocument(string path) { }
    }
}