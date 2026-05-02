using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Systems.MicroPackages;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.UI_And_Tools.Showcase.MicroPackagesGui
{
    public class MicroPackages_Gui_MicroPackageInspector : IGuiProvider
    {
        public string Title => "Package Inspector";

        private DynamicMicroPackage _package;

        // 1. Parameterless Default Constructor for Editor/Factory Instantiation
        public MicroPackages_Gui_MicroPackageInspector() { }

        // Optional convenience constructor
        public MicroPackages_Gui_MicroPackageInspector(DynamicMicroPackage packageTarget)
        {
            _package = packageTarget;
        }

        // 2. Injection Method for the Dashboard/Router to set the target after instantiation
        public void SetTargetPackage(DynamicMicroPackage packageTarget)
        {
            _package = packageTarget;
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));

        public VisualElement CreateGui(GuiContext ctx)
        {
            if (_package == null)
            {
                return new GraphicalUserInterfaceBuilder("EmptyInspector")
                    .AddChild(new ForgeLabelBuilder("No Package Selected. Please select a package from the Ecosystem tab.")
                        .WithColor(Color.gray))
                    .CreateGui(ctx);
            }

            var builder = new GraphicalUserInterfaceBuilder("InspectorRoot")
                .WithFlexLayout(FlexDirection.Column)
                .WithScrollable(true, ScrollViewMode.Vertical)
                .WithBackgroundColor(new Color(0.15f, 0.15f, 0.15f, 1f))
                .WithPadding(15)

                // Header / Metadata
                .AddChild(new ForgeLabelBuilder($"Inspecting: {_package.PackageId}")
                    .WithFontSize(18).WithFontStyle(FontStyle.Bold).WithColor(Color.cyan))
                .AddChild(new ForgeLabelBuilder($"Version: {_package.Version} | Status: {(_package.IsLocalShunt ? "Local Shunt" : "Published")}")
                    .WithColor(Color.white).WithMarginBottom(15))
                .AddSeparator()

                // Payload Statistics
                .AddChild(new ForgeLabelBuilder("Payload Breakdown")
                    .WithFontSize(14).WithFontStyle(FontStyle.Bold).WithColor(Color.white).WithMarginBottom(5))
                .AddChild(new ForgeLabelBuilder($"Providers Packaged: {_package.Providers?.Count ?? 0}")
                    .WithColor(Color.yellow))
                .AddChild(new ForgeLabelBuilder($"Arbitrations Packaged: {_package.Arbitrations?.Count ?? 0}")
                    .WithColor(new Color(1f, 0.5f, 0f))) // Orange
                .AddSeparator()

                // Detailed Provider Listing
                .AddChild(new ForgeLabelBuilder("Provider Index")
                    .WithFontSize(14).WithFontStyle(FontStyle.Bold).WithColor(Color.white).WithMarginBottom(5));

            if (_package.Providers != null)
            {
                for (int i = 0; i < _package.Providers.Count; i++)
                {
                    var provider = _package.Providers[i];
                    builder.AddChild(new ForgeLabelBuilder($"[{i}] Type: {provider.ProviderType} | ID: {provider.Id}")
                        .WithColor(Color.white).WithMarginLeft(10));
                }
            }

            return builder.CreateGui(ctx);
        }

        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}