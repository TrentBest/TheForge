using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

// Assume FSM_API namespaces are available based on your architecture
// using TheSingularityWorkshop.FSM_API; 

namespace Workshop.Systems.MicroPackages
{
    public class MicroPackages_GuiBuilder : IGuiProvider
    {
        public string Title => "MicroPackage Forge & Arbitrator";

        private MicroPackageBuilder _builder;

        // Local state for the GUI selections
        private string _packageName = "com.singularity.newpackage";
        private ProviderType _selectedProviderType = ProviderType.None;
        private IPackageArbitrator.ArbitrationType _selectedArbitrationType = IPackageArbitrator.ArbitrationType.Additive;
        private string _targetPackageId = "com.singularity.target";

        // Simulated entanglement state for the preview
        private List<string> _simulatedArbitrationLog = new List<string>();

        public MicroPackages_GuiBuilder()
        {
            _builder = new MicroPackageBuilder();
            _builder.WithPackageId(_packageName);
        }

        public Action<VisualElement> GetGuiBuilder()
        {
            return (root) =>
            {
                root.Add(CreateGui(new GuiContext()));
            };
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            // FIX: Pass the IGuiProvider blueprints directly. Do not pass the 'ctx' yet.
            var splitPanel = new ForgeSplitPanelBuilder("MicroPackageWorkspace")
                .WithFlexDirection(FlexDirection.Row)
                .WithLeftPane(BuildDefinitionPane())
                .WithRightPane(BuildArbitrationSimulatorPane());

            // The split panel handles cascading the ctx down to the panes here:
            var rootContainer = splitPanel.CreateGui(ctx);

            // --- CRITICAL UI TICK PATTERN ---
            rootContainer.schedule.Execute(() => {
                // FSM_API.Interaction.Update("MicroPackageForgeGroup");
            }).Every(16); // ~60fps target

            return rootContainer;
        }

        // FIX: Return IGuiProvider, not VisualElement
        private IGuiProvider BuildDefinitionPane()
        {
            return new GraphicalUserInterfaceBuilder("PackageDefinition")
                .WithTitle("Package Blueprint")
                .WithHeaderFontSize(18)
                .WithPadding(10)
                .WithBackgroundColor(new Color(0.12f, 0.12f, 0.12f, 1f))
                .WithScrollable(true, ScrollViewMode.Vertical)

                .AddStringData("Package ID", _packageName, val =>
                {
                    _packageName = val;
                    _builder.WithPackageId(_packageName);
                })
                .AddSeparator()

                .AddChild(new GraphicalUserInterfaceBuilder("ProvidersSection")
                    .WithTitle("Content Providers")
                    .WithHeaderFontSize(14)
                    .WithMarginBottom(10)
                    .AddEnumData("Provider Type", _selectedProviderType, val => _selectedProviderType = val)

                    // Converted to pure ForgeButtonBuilder
                    .AddChild(new ForgeButtonBuilder("Add Provider Template", () =>
                    {
                        Debug.Log($"[MicroPackageBuilder] Adding template for {_selectedProviderType}");
                    }))
                );
        }

        // FIX: Return IGuiProvider, not VisualElement
        private IGuiProvider BuildArbitrationSimulatorPane()
        {
            return new GraphicalUserInterfaceBuilder("ArbitrationSimulator")
                .WithTitle("Arbitration & Convergence")
                .WithHeaderFontSize(18)
                .WithPadding(10)
                .WithBackgroundColor(new Color(0.15f, 0.15f, 0.15f, 1f))
                .WithScrollable(true, ScrollViewMode.Vertical)

                .AddChild(new GraphicalUserInterfaceBuilder("RuleDefinition")
                    .WithTitle("Define Negotiation Rules")
                    .WithHeaderFontSize(14)
                    .WithMarginBottom(10)
                    .AddStringData("Target Package", _targetPackageId, val => _targetPackageId = val)
                    .AddEnumData("Arbitration Type", _selectedArbitrationType, val => _selectedArbitrationType = val)

                    // Converted to pure ForgeButtonBuilder
                    .AddChild(new ForgeButtonBuilder("Add Arbitration Directive", () =>
                    {
                        var newArbitration = new IPackageArbitrator.Arbitration
                        {
                            requestingPackage = _packageName,
                            targetedPackage = _targetPackageId,
                            arbitrationType = _selectedArbitrationType
                        };
                        _builder.AddArbitration(newArbitration);
                        _simulatedArbitrationLog.Add($"Added Directive: {_selectedArbitrationType} targeting {_targetPackageId}");
                        Debug.Log($"[MicroPackageBuilder] Added {_selectedArbitrationType} arbitration targeting {_targetPackageId}.");
                    }))
                )
                .AddSeparator()

                .AddChild(new GraphicalUserInterfaceBuilder("SimulationLog")
                    .WithTitle("Convergence Simulator")
                    .WithHeaderFontSize(14)
                    .WithMarginBottom(10)

                    // Converted to pure ForgeButtonBuilders
                    .AddChild(new ForgeButtonBuilder("Simulate Arbitration Rounds", () => RunSimulation()))
                    .AddChild(new ForgeButtonBuilder("Compile Package", () => CompilePackage()))
                );
        }

        private void RunSimulation()
        {
            Debug.Log("[MicroPackageBuilder] Running Simulation Rounds...");
            _simulatedArbitrationLog.Add("--- Simulation Started ---");
            _simulatedArbitrationLog.Add($"Round 1: {_packageName} requesting access to {_targetPackageId}.");
            _simulatedArbitrationLog.Add("Round 2: Convergence achieved.");
        }

        private void CompilePackage()
        {
            IMicroPackage finishedPackage = _builder.Build();

            // Integrated the Gondola Shunt metadata mapping we built earlier
            var dynamicPackage = (DynamicMicroPackage)finishedPackage;
            dynamicPackage.IsLocalShunt = true;

            Debug.Log($"[MicroPackageBuilder] Compiled Package: {dynamicPackage.PackageId}");

            // Here you would hook into your global DataWarehouse to save to the local cache:
            // e.g., globalWarehouse.StoreAsset(dynamicPackage.PackageId, dynamicPackage);
        }

#if UNITY_EDITOR
        public void ToUIDocument(string assetPath)
        {
            var gui = new GraphicalUserInterfaceBuilder("Temp").WithTitle("MicroPackageBuilder");
            gui.ToUIDocument(assetPath);
        }
#endif

        public void FromUIDocument(string assetPath)
        {
            // Standard hydration logic
        }
    }
}