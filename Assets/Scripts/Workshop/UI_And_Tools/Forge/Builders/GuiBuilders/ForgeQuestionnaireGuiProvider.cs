using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.IO;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    public class ForgeQuestionnaireGuiProvider : IGuiProvider
    {
        public string Title => "Singularity Forge Manifestation";

        private VisualElement _rootContainer;
        private VisualElement _mediumsContainer; // Top: Checkboxes/Toggles
        private VisualElement _configContainer;  // Bottom: Contextual Phases

        // Normalized data structure. In production, this data should be fetched 
        // from your SST Warehouse or IPackageArbitrator, not hardcoded here.
        private List<MediumManifest> _availableMediums;

        public struct MediumManifest
        {
            public string Id;
            public string DisplayName;
            public string Description;
            public bool IsAvailable; // Your "good to go" switch
            public Action OnManifest; // The trigger to build the MicroPackage
            public List<PhaseData> ContextualPhases; // Follow-up configurations if needed
        }

        public struct PhaseData
        {
            public string PhaseTitle;
            public string Description;
            public Dictionary<string, Action> Options;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            // Sync with your available packages
            SyncWithSingleSourceOfTruth();

            var rootBuilder = new ForgeContainerBuilder("ManifestationRoot")
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.08f, 0.95f))
                .WithPadding(10, 10, 10, 10);

            // --- TOP PANEL (HEADER) ---
            var headerContainer = new ForgeContainerBuilder("Header_ScopeAndMediums")
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.12f, 0.9f))
                .WithBorderRadius(8)
                .WithPadding(15, 15, 15, 15)
                .WithMarginBottom(10); // Spacing between header and questions

            // 1. The Experience Name Input
            headerContainer.AddChild(new ForgeTextFieldBuilder("Experience Name:")
     .WithPlaceholder("e.g., Project Asteroids...")
     .OnChanged(val => UnityEngine.Debug.Log($"Name set to: {val}")));

            // 2. The Dynamic Medium Toggles
            var mediumsRow = new ForgeContainerBuilder("MediumsRow")
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Center)
                .WithMarginTop(10);

            mediumsRow.AddChild(new ForgeLabelBuilder("Target Mediums: ").WithColor(Color.gray).WithMarginRight(15));

            // Dynamically generate toggles based on registered Mediums
            foreach (var medium in _availableMediums.Where(m => m.IsAvailable))
            {
                var toggle = new Toggle(medium.DisplayName);
                toggle.style.marginRight = 15;
                toggle.style.color = Color.white;
                toggle.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue)
                    {
                        // Inject the MicroPackage into the active scene!
                        medium.OnManifest?.Invoke();
                    }
                });
                mediumsRow.AddChild(toggle);
            }

            headerContainer.AddChild(mediumsRow);

            // --- BOTTOM PANEL (20 QUESTIONS / TOOLING) ---
            var bodyContainer = new ForgeScrollViewBuilder("Questionnaire_Body")
                .WithFlexGrow(1) // Takes up the rest of the vertical space
                .WithPadding(10, 10, 10, 10);

            // Populate your 20 questions here...
            bodyContainer.AddChild(new ForgeLabelBuilder("Step 1: Define Workspace Tooling")
                .WithFontSize(18).WithColor(new Color(0.0f, 0.9f, 1.0f)));

            // (Add your dynamic question builders here)

            // Assemble the root
            rootBuilder.AddChild(headerContainer);
            rootBuilder.AddChild(bodyContainer);

            _rootContainer = rootBuilder.Build();
            return _rootContainer;
        }


        private VisualElement BuildMediumSelectors(GuiContext ctx)
        {
            var topSection = new ForgeContainerBuilder("NexusHeader")
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.12f, 0.9f))
                .WithPadding(15, 15, 15, 15)
                .WithBorderRadius(5)
                .AddChild(new ForgeLabelBuilder("THE NEXUS: I/O CONTRACT")
                    .WithColor(new Color(0.64f, 0.17f, 0.77f)) // Deep purple
                    .WithFontSize(18)
                    .WithFontStyle(FontStyle.Bold)
                    .WithMarginBottom(15));

            // 1. The Name
            topSection.AddChild(new ForgeTextFieldBuilder("Experience Name:")
                .WithPlaceholder("e.g., Echoes in the Dark...")
                .WithMarginBottom(15));

            // 2. The Mediums
            var mediumsRow = new ForgeContainerBuilder("MediumsRow")
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Center)
                .WithMarginBottom(10)
                .AddChild(new ForgeLabelBuilder("Target Mediums: ").WithColor(Color.gray).WithWidth(120));

            foreach (var medium in _availableMediums.Where(m => m.IsAvailable))
            {
                var toggle = new Toggle(medium.DisplayName) { style = { marginRight = 15, color = Color.white } };
                toggle.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue) medium.OnManifest?.Invoke();
                });
                mediumsRow.AddChild(toggle);
            }
            topSection.AddChild(mediumsRow);

            // 3. The Target Senses (The new addition)
            var sensesRow = new ForgeContainerBuilder("SensesRow")
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Center)
                .AddChild(new ForgeLabelBuilder("Target Senses: ").WithColor(Color.gray).WithWidth(120));

            // In your actual architecture, this list would be pulled from a SenseRegistry
            string[] availableSenses = { "Visual", "Audio", "Haptic", "Psionic / Neural" };

            foreach (var sense in availableSenses)
            {
                var toggle = new Toggle(sense) { style = { marginRight = 15, color = Color.white } };

                // Default visual/audio to on, just for UX convenience
                if (sense == "Visual" || sense == "Audio") toggle.value = true;

                toggle.RegisterValueChangedCallback(evt =>
                {
                    UnityEngine.Debug.Log($"[Forge Nexus] Sense Pipeline '{sense}' is now {(evt.newValue ? "ACTIVE" : "DISABLED")}.");
                    // Here you would broadcast to the DataWarehouse that a sense pipeline has been toggled.
                    // If they disable "Visual", the Forge might automatically collapse the Material/Mesh toolbars.
                });
                sensesRow.AddChild(toggle);
            }
            topSection.AddChild(sensesRow);

            return topSection.Build();
        }

        private void RenderContextualPhases(MediumManifest medium)
        {
            _configContainer.Clear();

            if (medium.ContextualPhases == null || medium.ContextualPhases.Count == 0)
            {
                _configContainer.Add(new ForgeLabelBuilder($"{medium.DisplayName} MicroPackage deployed. No further parameters required.")
                    .WithColor(new Color(0.64f, 0.17f, 0.77f))
                    .WithFontSize(16)
                    .WithFontStyle(FontStyle.Italic)
                    .Build());
                return;
            }

            // If the medium requires deep configuration, render the phases recursively
            // This replaces your old _currentPhaseIndex loop by scoping it to the active medium
            foreach (var phase in medium.ContextualPhases)
            {
                var phaseBlock = new ForgeContainerBuilder($"Phase_{phase.PhaseTitle}")
                    .WithMarginBottom(20)
                    .AddChild(new ForgeLabelBuilder(phase.PhaseTitle)
                        .WithColor(new Color(0.0f, 0.9f, 1.0f))
                        .WithFontSize(16)
                        .WithFontStyle(FontStyle.Bold))
                    .AddChild(new ForgeLabelBuilder(phase.Description)
                        .WithColor(Color.white)
                        .WithFontSize(12)
                        .WithMarginBottom(10));

                foreach (var option in phase.Options)
                {
                    phaseBlock.AddChild(new ForgeButtonBuilder(option.Key)
                        .OnClick(() => option.Value?.Invoke())
                        .WithBackgroundColor(new Color(0.15f, 0.15f, 0.18f))
                        .WithColor(Color.white)
                        .WithMarginBottom(5));
                }

                _configContainer.Add(phaseBlock.Build());
            }
        }

        private void SyncWithSingleSourceOfTruth()
        {
            // Mocking the Data Fetch. 
            // Replace this with a call to your IPackageArbitrator or DataWarehouse.
            _availableMediums = new List<MediumManifest>
            {
                new MediumManifest
                {
                    Id = "Medium_Stage",
                    DisplayName = "A Single Stage (Arena/City)",
                    Description = "Localized Grid Proximities",
                    IsAvailable = true, // Switch to true when ready
                    OnManifest = () => UnityEngine.Debug.Log("MicroPackage [Stage] Generated."),
                    ContextualPhases = new List<PhaseData>
                    {
                        new PhaseData
                        {
                            PhaseTitle = "Stage Physics",
                            Description = "Select local laws of motion.",
                            Options = new Dictionary<string, Action>
                            {
                                { "RigidBody Standard", () => UnityEngine.Debug.Log("RigidBody Package Loaded") },
                                { "FSM Driven Agents", () => UnityEngine.Debug.Log("FSM Silos Online") }
                            }
                        }
                    }
                },
                new MediumManifest
                {
                    Id = "Medium_Planetary",
                    DisplayName = "Planetary Icosphere",
                    Description = "Atmospheric Shaders & Hex Grids",
                    IsAvailable = false, // Hidden until you authorize it
                    OnManifest = () => UnityEngine.Debug.Log("MicroPackage [Planetary] Generated.")
                }
            };
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) => WorkshopUxmlBaker.Bake(CreateGui(new GuiContext()), string.IsNullOrEmpty(assetPath) ? "Questionnaire_Snapshot" : System.IO.Path.GetFileNameWithoutExtension(assetPath));
        public void FromUIDocument(string assetPath) => Debug.LogWarning("Dynamic generation only.");
    }
}