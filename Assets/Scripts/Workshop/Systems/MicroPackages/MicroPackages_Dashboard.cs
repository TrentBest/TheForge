using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.Systems.MicroPackages
{
    public class MicroPackages_Dashboard : IGuiProvider
    {
        public string Title => "MicroPackages Dashboard";

        private IGuiRouter _router;

        public void InjectRouter(IGuiRouter router)
        {
            _router = router;
        }

        public Action<VisualElement> GetGuiBuilder()
        {
            return (root) => root.Add(CreateGui(new GuiContext { Name = Title }));
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var builder = new GraphicalUserInterfaceBuilder("MicroPackageDashboard")
                .WithTitle("Local Package Repository")
                .WithHeaderFontSize(22)
                .WithPadding(15)
                .AddChild(new ForgeButtonBuilder("+ Create New MicroPackage", () => LaunchBuilder(null)))
                .AddSeparator();

            // Mock Data Fetch: Replace with actual File I/O scanning your Experiences folder
            var existingPackages = new List<string> { "pkg_core_physics", "pkg_render_asteroids", "pkg_fsm_swarm_logic" };

            foreach (var pkg in existingPackages)
            {
                // A mini-card for each package
                builder.AddChild(new GraphicalUserInterfaceBuilder($"card_{pkg}")
                    .WithFlexDirection(FlexDirection.Row)
                    .WithPadding(5)
                    .WithBackgroundColor(new Color(0.12f, 0.12f, 0.12f, 1f))
                    .AddChild(new ForgeLabelBuilder(pkg))

                    // Route to Update/Assemble
                    .AddChild(new ForgeButtonBuilder("Edit / Assemble", () => LaunchBuilder(pkg)))

                    // Delete Logic
                    .AddChild(new ForgeButtonBuilder("Delete", () => DeletePackage(pkg)))
                );
            }

            // Back Button to return to Portal
            builder.AddSeparator();
            builder.AddChild(new ForgeButtonBuilder("Back to Portal", () => _router?.NavigateBack()));

            return builder.CreateGui(ctx);
        }

        private void LaunchBuilder(string packageId)
        {
            if (_router != null)
            {
                var compilerGui = new MicroPackages_GuiBuilder();
               // compilerGui.InjectRouter(_router);

                // If packageId is not null, the builder should deserialize it for editing
                //if (!string.IsNullOrEmpty(packageId)) compilerGui.LoadPackageForEditing(packageId);

                //_router.NavigateTo(compilerGui);
            }
        }

        private void DeletePackage(string packageId)
        {
            Debug.Log($"[Dashboard] Shredding package: {packageId}");
            // TODO: Add actual File.Delete logic here, then refresh the GUI
        }

        public void ToUIDocument(string assetPath)
        {
            throw new NotImplementedException();
        }

        public void FromUIDocument(string assetPath)
        {
            throw new NotImplementedException();
        }
    }
}