using System;
using System.Collections.Generic;
using UnityEngine;
using Workshop.UI_And_Tools.Forge.IO;
using Workshop.Systems.MicroPackages;

namespace Workshop.Systems.MicroPackages.Packages.Literature
{
    /// <summary>
    /// The Literature Authoring Suite: The Scribe of the Singularity.
    /// Provides diegetic tools for semantic narrative construction, scrying, and physical pagination.
    /// Slotted into The Observatory for high-level narrative orchestration.
    /// </summary>
    public class LiteratureAuthoringPackage : IMicroPackage, IArchivalMetadata
    {
        // --- Core Identity ---
        public string PackageId { get; set; } = "Core.Literature.AuthoringSuite";

        /// <summary>
        /// The Heartbeat: Maps literary engines to the Unity Lifecycle.
        /// Narrative pacing and scrying simulations run during Update, 
        /// while physical page sorting happens in FixedUpdate.
        /// </summary>
        public Dictionary<string, List<string>> ProcessGroupsPerUnityMessage => new Dictionary<string, List<string>>
        {
            { "Update", new List<string> {
                "Narrative_Pacing_Engine", // Evaluates story beats over time
                "Scrying_Simulation"       // Ticks the holographic projections
            }},
            { "FixedUpdate", new List<string> {
                "Pagination_Physics"       // Physical sorting of manuscript pages on the desk
            }},
            { "LateUpdate", new List<string> {
                "Semantic_Layout_Render"   // UI repositioning for Storyboards and Whiteboards
            }}
        };

        // --- Architectural Intent ---

        /// <summary>
        /// ForgeOnly: These tools are for the creator to author the narrative.
        /// The resulting manuscripts are saved to the DataWarehouse for the player to find.
        /// </summary>
        public PackageExecutionScope ExecutionScope => PackageExecutionScope.ForgeOnly;

        /// <summary>
        /// Scholarly delivery: Manifests as a specialized desk or archive terminal.
        /// </summary>
        public string DeliveryProfileId => "Delivery_ScholarlyDropSled";

        /// <summary>
        /// Relies on the UI layer for the diegetic interfaces and the DataWarehouse for storage.
        /// </summary>
        public string[] GetDependencies() => new[] { "Core.Forge.DiegeticUI", "Core.Memory.DataWarehouse" };

        public string[] GetExclusions() => new string[0];

        #region --- IArchivalMetadata (The Data Card) ---

        public string Version => "1.0.0";
        public string Author => "The Singularity Workshop";
        public ForgeSpatialZone SpatialZone => ForgeSpatialZone.MetaTools; // Quadrant 4: The Observatory

        public string ShortDescription => "Diegetic tools for semantic narrative construction.";
        public string LongDescription => "Enables the 'Scribe' workflow. Features holographic scrying pools for character arc tracking, physical pagination physics for manuscript sorting, and semantic pacing engines.";
        public string ThumbnailUrl => "Textures/Thumbnails/Archive_Literature_Scribe";

        public int ReleaseYear => DateTime.Now.Year;
        public string OriginalAuthor => "Trent Best";
        public string OriginalPublisher => "The Singularity Workshop";
        public string OriginalPlatform => "The Forge";
        public string Era => "Emergence";
        public string HistoricalSignificance => "The foundational bridge between game state and literary narrative; where data becomes story.";
        public Color AccentColor => new Color(0.9f, 0.8f, 0.6f); // Vellum / Aged Parchment

        #endregion

        // Internal State Tracking
        private bool _isDataShelfSecured = false;
        private bool _isBusSubscribed = false;

        public void Initialize()
        {
            Debug.Log($"<b>[{PackageId}]</b> Initializing Narrative Chronology Matrices and Semantic MadLibs...");

            if (!_isBusSubscribed)
            {
                // Hook into the Universal Bus for cross-package semantic events
                SingularityDataBus.Instance.Subscribe<string>("Narrative_Beat_Triggered", OnNarrativeBeat);
                SingularityDataBus.Instance.Subscribe<string>("Character_Arc_Shift", OnCharacterArcShift);
                _isBusSubscribed = true;
            }
        }

        public void Terminate()
        {
            Debug.Log($"<b>[{PackageId}]</b> Shutting down literary tools and packing away the manuscripts.");

            if (_isBusSubscribed)
            {
                SingularityDataBus.Instance.Unsubscribe<string>("Narrative_Beat_Triggered", OnNarrativeBeat);
                SingularityDataBus.Instance.Unsubscribe<string>("Character_Arc_Shift", OnCharacterArcShift);
                _isBusSubscribed = false;
            }
        }

        /// <summary>
        /// Phase 1: Injection.
        /// Register the literary providers and scrying tools.
        /// </summary>
        public void LoadPackage(IPackageArbitrator arbitrator)
        {
            Debug.Log($"<b>[{PackageId}]</b> Loading semantic structures. Preparing initial arbitration requests...");
            _isDataShelfSecured = false;

            Initialize();
        }

        /// <summary>
        /// Phase 2: Convergence.
        /// Secures a 'Manuscript Shelf' within the DataWarehouse to store authored content.
        /// </summary>
        public void Arbitrate(IPackageArbitrator arbitrator)
        {
            if (!_isDataShelfSecured)
            {
                arbitrator.SubmitArbitration(new IPackageArbitrator.Arbitration
                {
                    requestingPackage = PackageId,
                    targetedPackage = "Core.Memory.DataWarehouse",
                    arbitrationType = IPackageArbitrator.ArbitrationType.Additive,
                    payload = "EnsureDataShelf_Global_Narrative_Manuscripts"
                });

                if (arbitrator.Warehouse != null && arbitrator.Warehouse.TryGetAsset<object>("Global_Narrative_Manuscripts", out _))
                {
                    _isDataShelfSecured = true;
                }
            }
        }

        private void OnNarrativeBeat(string payload)
        {
            Debug.Log($"<b>[{PackageId}]</b> Narrative Beat Received: {payload}. Updating Scrying Pools...");
        }

        private void OnCharacterArcShift(string payload)
        {
            Debug.Log($"<b>[{PackageId}]</b> Character Arc Shift Detected: {payload}. Re-evaluating Pagination...");
        }

        // --- ORACLE / CONSTRAINTS ---

        public List<OracleInquiry> GetPackageConstraints()
        {
            return new List<OracleInquiry>
            {
                new OracleInquiry
                {
                    InquiryId = "output_medium",
                    Type = InquiryType.Binary,
                    SemanticPrompt = "Will this reality be compiled as a Linear Narrative (Book)?",
                    RequiredBinary = true
                },
                new OracleInquiry
                {
                    InquiryId = "narrative_complexity",
                    Type = InquiryType.Magnitude,
                    SemanticPrompt = "Expected branching complexity of the plot (1-10)?",
                    RequiredMagnitude = 5
                }
            };
        }
    }

    // --- ORACLE STUBS ---
    public enum InquiryType { Binary, Magnitude }

    [Serializable]
    public class OracleInquiry
    {
        public string InquiryId;
        public InquiryType Type;
        public string SemanticPrompt;
        public bool RequiredBinary;
        public int RequiredMagnitude;
    }
}