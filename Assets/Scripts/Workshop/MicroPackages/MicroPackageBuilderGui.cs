using System;
using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;

namespace TheSingularityWorkshop.MicroPackages
{
    public class MicroPackageBuilderGui : IGuiProvider
    {
        public string Title => "MicroPackage Forge";

        private MicroPackageBuilder _builder = new MicroPackageBuilder();

        // Local state for the GUI selections
        private string _packageName = "com.singularity.newpackage";
        private ProviderType _selectedProviderType = ProviderType.None;
        private IPackageArbitrator.ArbitrationType _selectedArbitrationType = IPackageArbitrator.ArbitrationType.Additive;

        public Action<VisualElement> GetGuiBuilder()
        {
            return (root) =>
            {
                root.Add(CreateGui(new GuiContext()));
            };
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            // Set up the initial builder state
            _builder.WithPackageId(_packageName);

            var gui = new GraphicalUserInterfaceBuilder("MicroPackageBuilderPanel")
                .WithTitle("Forge: MicroPackage")
                .WithHeaderFontSize(20)
                .WithPadding(15)
                .WithBackgroundColor(new Color(0.12f, 0.12f, 0.12f, 1f))
                .WithAutoGrow()
                .WithScrollable(true, ScrollViewMode.Vertical)

                // --- Identity Section ---
                .AddStringData("Package ID", _packageName, val =>
                {
                    _packageName = val;
                    _builder.WithPackageId(_packageName);
                })
                .AddSeparator()

                // --- Providers Section ---
                .AddChild(new GraphicalUserInterfaceBuilder("ProvidersSection")
                    .WithTitle("Providers")
                    .WithHeaderFontSize(14)
                    .WithMarginBottom(10)
                    .AddEnumData("Provider Type", _selectedProviderType, val => _selectedProviderType = val)
                    .AddButton("Add Empty Provider Template", () =>
                    {
                        // In a full implementation, this would open a sub-builder for the specific ProviderType
                        Debug.Log($"[MicroPackageBuilder] Adding template for {_selectedProviderType}");
                        // _builder.AddProvider(..);
                    })
                )
                .AddSeparator()

                // --- Arbitration Section ---
                .AddChild(new GraphicalUserInterfaceBuilder("ArbitrationSection")
                    .WithTitle("Arbitrations")
                    .WithHeaderFontSize(14)
                    .WithMarginBottom(10)
                    .AddEnumData("Arbitration Type", _selectedArbitrationType, val => _selectedArbitrationType = val)
                    .AddButton("Add Arbitration Directive", () =>
                    {
                        var newArbitration = new IPackageArbitrator.Arbitration
                        {
                            requestingPackage = _packageName,
                            targetedPackage = "Target.Package.Id", // Would come from a text field in a more complex UI
                            arbitrationType = _selectedArbitrationType
                        };
                        _builder.AddArbitration(newArbitration);
                        Debug.Log($"[MicroPackageBuilder] Added {_selectedArbitrationType} arbitration.");
                    })
                )
                .AddSeparator()

                // --- Build / Export Section ---
                .AddButton("Compile Package", () =>
                {
                    IMicroPackage finishedPackage = _builder.Build();
                    Debug.Log($"[MicroPackageBuilder] Compiled Package: {((DynamicMicroPackage)finishedPackage).PackageId}");

                    // Here you would register it with the Arbitrator, save it to disk, or serialize it.
                });

            return gui.CreateGui(ctx);
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