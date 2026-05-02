using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.Systems.MicroPackages;

namespace Workshop.UI_And_Tools.Showcase.MicroPackagesGui
{
    public class Workshop_Gui_InteractivePackageKiosk : IGuiProvider
    {
        public string Title => "The Foundry: Intake Kiosk";
        public IGuiProvider CurrentGuiProvider { get; internal set; }

        private ForgeArbitrator _arbitrator;
        private VisualElement _kioskRoot;

        // Mocking the cloud database for the showcase
        private List<DynamicMicroPackage> _cloudPackages = new List<DynamicMicroPackage>();

        public Workshop_Gui_InteractivePackageKiosk(ForgeArbitrator arbitrator)
        {
            _arbitrator = arbitrator;

            // Hydrate mock cloud data based on our sample packages
            _cloudPackages.Add(new DynamicMicroPackage { PackageId = "Workshop.Sample.Elements", SpatialZone = ForgeSpatialZone.LogicAndCompute });
            _cloudPackages.Add(new DynamicMicroPackage { PackageId = "Workshop.Sample.Magic", SpatialZone = ForgeSpatialZone.GameplaySystems });
            _cloudPackages.Add(new DynamicMicroPackage { PackageId = "Workshop.Sample.Weapons", SpatialZone = ForgeSpatialZone.PhysicalAssets });
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var rootBuilder = new GraphicalUserInterfaceBuilder("KioskRoot")
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Center)
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.12f))
                .WithPadding(20)
                .WithPercentSize(100, 100)
                .OnBuild(ve => _kioskRoot = ve);

            // --- HEADER & CACHE CLEAR ---
            var header = new GraphicalUserInterfaceBuilder("HeaderRow")
     .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
     .WithWidth(Length.Percent(100)).WithMarginBottom(20)
                 .AddChild(new ForgeLabelBuilder("NEXUS INTAKE KIOSK").WithFontSize(24).WithColor(Color.white).WithFontStyle(FontStyle.Bold))
                .AddChild(new ForgeButtonBuilder("Wipe Local Gondola Yard (Reset Cache)", ResetEnvironment)
                    .WithBackgroundColor(new Color(0.6f, 0.2f, 0.2f)).WithTextColor(Color.white));

            rootBuilder.AddChild(header);
            rootBuilder.AddChild(new ForgeLabelBuilder("Click a Data Card to observe the kinetic download sequence.").WithColor(Color.gray).WithMarginBottom(20));

            // --- PACKAGE CARDS ---
            foreach (var pkg in _cloudPackages)
            {
                rootBuilder.AddChild(BuildInteractiveCard(pkg));
            }

            return rootBuilder.Build();
        }

        private GraphicalUserInterfaceBuilder BuildInteractiveCard(DynamicMicroPackage pkg)
        {
            VisualElement containerElement = null;
            Button primaryBtn = null;
            VisualElement loadBtnContainer = null;

            // The main container that will stretch
            var cardContainer = new GraphicalUserInterfaceBuilder($"Card_{pkg.PackageId}")
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Center)
                .WithHeight(60)
                .WithMarginBottom(10)
                .OnBuild(ve =>
                {
                    containerElement = ve;
                    // Inject native UI Toolkit smooth transitions
                    ve.style.width = 300; // Starting shrunk width
                    ve.style.transitionProperty = new StyleList<StylePropertyName>(new List<StylePropertyName> { new StylePropertyName("width") });
                    ve.style.transitionDuration = new StyleList<TimeValue>(new List<TimeValue> { new TimeValue(0.4f, TimeUnit.Second) });
                    ve.style.transitionTimingFunction = new StyleList<EasingFunction>(new List<EasingFunction> { new EasingFunction(EasingMode.EaseInOutSine) });
                });

            // The "Load" button (Hidden initially)
            var loadButton = new ForgeButtonBuilder("Load to Arbitrator", () =>
            {
                Debug.Log($"[Kiosk] Sending {pkg.PackageId} to Arbitrator.");
                primaryBtn.text = "Uninstall Package";
                primaryBtn.style.backgroundColor = new Color(0.6f, 0.2f, 0.2f); // Turn Red
                _arbitrator.LoadPackage(pkg);
            })
                .WithBackgroundColor(new Color(0.2f, 0.6f, 0.2f)) // Green
                .WithTextColor(Color.white)
                .WithHeight(60).WithWidth(150);

            var rightSlot = new GraphicalUserInterfaceBuilder("RightSlot")
                .WithWidth(150).WithHeight(60).WithMarginLeft(10)
                .AddChild(loadButton)
               .OnBuild(ve =>
               {
                   loadBtnContainer = ve;
                   ve.style.opacity = 0; // Invisible to start
                   ve.style.display = DisplayStyle.None;
                   ve.style.transitionProperty = new StyleList<StylePropertyName>(new List<StylePropertyName> { new StylePropertyName("opacity") });
                   ve.style.transitionDuration = new StyleList<TimeValue>(new List<TimeValue> { new TimeValue(0.3f, TimeUnit.Second) });
               });

            // The Primary Action Button (Download / Uninstall)
            var primaryButton = new ForgeButtonBuilder($"Download: {pkg.PackageId}", () =>
            {
                if (primaryBtn.text.StartsWith("Download"))
                {
                    // 1. Start Stretching
                    primaryBtn.text = "Downloading...";
                    containerElement.style.width = 460; // 300 + 150 + 10 margin

                    // Simulate Async Download
                    containerElement.schedule.Execute(() =>
                    {
                        primaryBtn.text = "Downloaded";
                        loadBtnContainer.style.display = DisplayStyle.Flex;

                        // Fade in the load button slightly after display is set
                        containerElement.schedule.Execute(() => loadBtnContainer.style.opacity = 1).StartingIn(50);

                    }).StartingIn(600); // Wait for stretch animation + network delay
                }
                else if (primaryBtn.text.StartsWith("Uninstall"))
                {
                    // 1. Uninstall from Ecosystem
                    _arbitrator.UninstallPackage(pkg.PackageId);

                    // 2. Hide Load Button
                    loadBtnContainer.style.opacity = 0;

                    containerElement.schedule.Execute(() =>
                    {
                        loadBtnContainer.style.display = DisplayStyle.None;
                        // 3. Shrink Container Back
                        primaryBtn.text = $"Download: {pkg.PackageId}";
                        primaryBtn.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f); // Reset color
                        containerElement.style.width = 300;

                    }).StartingIn(300); // Wait for fade out
                }
            })
                .WithBackgroundColor(new Color(0.2f, 0.2f, 0.2f))
                .WithTextColor(Color.white)
                .WithHeight(60).WithWidth(300) // Fixed size, the container stretches around it
                .OnBuild(ve => primaryBtn = ve as Button);

            cardContainer.AddChild(primaryButton);
            cardContainer.AddChild(rightSlot);

            return cardContainer;
        }

        private void ResetEnvironment()
        {
            Debug.Log("<b>[Kiosk]</b> Wiping Local Gondola Yard and resetting Ecosystem to Homeostasis.");

            // 1. Delete all physical files
            var localPackages = SingularityPackageManager.GetLocalPackages();
            foreach (var pkg in localPackages)
            {
                SingularityPackageManager.DeleteLocalPackage(pkg.PackageId);
            }

            // 2. Force Arbitrator wipe
            var installedList = new List<string>(_arbitrator.InstalledPackages);
            foreach (var id in installedList)
            {
                _arbitrator.UninstallPackage(id);
            }

            // 3. Refresh UI (Hack: re-triggering the transition shrinks them visually)
            // In a full production setup, you'd bind the UI state to an EventBus.
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) { }
        public void FromUIDocument(string path) { }
    }
}