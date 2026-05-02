using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.Systems.MicroPackages
{
    public class MicroPackages_DashboardCard : IGuiProvider
    {
        public string Title => "Package Arbitrator Pulse";

        private IPackageArbitrator _arbitrator;

        public Action<VisualElement> GetGuiBuilder()
        {
            return (root) => root.Add(CreateGui(new GuiContext()));
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            int loadedPackages = _arbitrator?.InstalledPackages?.Count ?? 0;

            var gui = new GraphicalUserInterfaceBuilder("MicroPackageCard")
                .WithTitle("Micro Packages")
                .WithHeaderFontSize(16)
                .WithPadding(15)
                .WithBackgroundColor(new Color(0.15f, 0.12f, 0.20f, 1f))
                .WithBorderRadius(8)
                .WithBorderWidth(1)
                .WithBorderColor(new Color(0.3f, 0.2f, 0.4f, 1f))

                .AddChild(new ForgeLabelBuilder($"Loaded Packages: {loadedPackages}")
                    .WithColor(Color.white)
                    .WithFontSize(14)
                    .WithFontStyle(FontStyle.Bold))
                .AddChild(new ForgeLabelBuilder("Arbitration Status: Stable")
                    .WithColor(Color.green)
                    .WithFontSize(12)
                    .WithFontStyle(FontStyle.Normal))
                .AddSeparator()

                // Pure ForgeButtonBuilder implementation
                .AddChild(new ForgeButtonBuilder("Open Nexus Forge", () =>
                {
                    Debug.Log("[MicroPackageDashboard] Launching Dominant MicroPackage Nexus...");
                    // GuiRouter.NavigateTo(new MicroPackageNexusGui());
                })
                .WithBackgroundColor(new Color(0.25f, 0.2f, 0.3f))
                .WithTextColor(Color.white)
                .WithMarginTop(10));

            var rootContainer = gui.CreateGui(ctx);

            rootContainer.schedule.Execute(() => {
                // FSM_API.Interaction.Update("MicroPackageDashboardGroup");
            }).Every(1000);

            return rootContainer;
        }

#if UNITY_EDITOR
        public void ToUIDocument(string assetPath) { }
#endif
        public void FromUIDocument(string assetPath) { }
    }
}