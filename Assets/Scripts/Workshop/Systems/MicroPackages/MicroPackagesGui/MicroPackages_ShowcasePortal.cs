using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Showcase;

namespace Workshop.Systems.MicroPackages
{
    public class MicroPackages_ShowcasePortal : IShowcasePortal
    {
        public string Title => "MicroPackage Nexus";
        public bool IsDiscoverable => true;
        public bool IsUnderConstruction => false;
        public string Category => "Systems Architecture";
        public string Overview => "The fundamental unit of logical distribution. Micro in focus, not necessarily in size.";
        public Color AccentColor => new Color(0.2f, 0.6f, 0.9f);
        public List<string> CoreTechnologies => new List<string> { "FSM Reflection", "Data-Oriented Contiguous Arrays", "Zero-Allocation Operations" };
        public string TelemetryState => "Online";

        private IGuiRouter _router;

        // Track if we are running the baked version or the dynamic version
        private string _hydratedUxmlPath = string.Empty;

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
            VisualElement root;

            // 1. VISUAL CONSTRUCTION PHASE
            if (!string.IsNullOrEmpty(_hydratedUxmlPath))
            {
                root = GraphicalUserInterfaceBuilder.ConvertFromUIDocument(_hydratedUxmlPath);
            }
            else
            {
                root = BuildDynamicVisuals(ctx);
            }

            // 2. LOGIC BINDING PHASE
            BindLogic(root);

            return root;
        }

        private VisualElement BuildDynamicVisuals(GuiContext ctx)
        {
            return GetDynamicBuilder().CreateGui(ctx);
        }

        private GraphicalUserInterfaceBuilder GetDynamicBuilder()
        {
            string deepDiveText =
                "MicroPackages bypass the bloat of modern software design by utilizing strict, data-oriented memory management and Hash-backed Finite State Machines. " +
                "They are designed to handle staggering scale—capable of managing contiguous arrays for billions of orbiting asteroids, " +
                "pushing the hardware until the agents literally outnumber the discreet pixels available on the screen.";

            string deepDivePromo =
                "Launch the Deep Dive to experience live Telemetric Validation. Watch a high-density simulation execute side-by-side with its Reflective GUI, " +
                "proving O(1) operational speeds and true logical integrity in real-time.";

            return new GraphicalUserInterfaceBuilder("MicroPackagePortal")
                .WithTitle(Title)
                .WithPadding(20)
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.12f))

                // --- Tagline ---
                .AddChild(new ForgeLabelBuilder($"\"{Overview}\"")
                    .WithFontSize(16)
                    .WithFontStyle(FontStyle.Italic)
                    .WithColor(AccentColor)
                    .WithMarginBottom(15))

                // --- Core Explanation ---
                .AddChild(new ForgeLabelBuilder(deepDiveText)
                    .WithFontSize(13)
                    .WithWhiteSpace(WhiteSpace.Normal)
                    .WithMarginBottom(20))

                .AddSeparator(new Color(0.3f, 0.3f, 0.3f), 1)

                // --- Deep Dive Promo ---
                .AddChild(new ForgeLabelBuilder("Technical Proof of Concept")
                    .WithFontSize(14)
                    .WithFontStyle(FontStyle.Bold)
                    .WithMarginTop(10)
                    .WithMarginBottom(5))
                .AddChild(new ForgeLabelBuilder(deepDivePromo)
                    .WithFontSize(12)
                    .WithColor(new Color(0.7f, 0.7f, 0.7f))
                    .WithWhiteSpace(WhiteSpace.Normal)
                    .WithMarginBottom(20))

                // --- Action Routing Area ---
                .AddChild(new GraphicalUserInterfaceBuilder("ActionRow")
                    .WithFlexDirection(FlexDirection.Row)
                    .WithJustifyContent(Justify.SpaceBetween)
                    .WithMarginTop(10)

                    // The Standard Operational Entry
                    .AddChild(new ForgeButtonBuilder("Launch Nexus Dashboard")
                        .WithName("btn_launch_nexus")
                        .WithBackgroundColor(new Color(0.2f, 0.2f, 0.2f))
                        .WithPadding(10)
                        .WithFlexGrow(1)
                        .WithMarginRight(10))

                    // The Pitch / TRL 8 Evaluator Entry
                    .AddChild(new ForgeButtonBuilder("Deep Dive: Telemetric Validation")
                        .WithName("btn_launch_deepdive")
                        .WithBackgroundColor(new Color(0.6f, 0.2f, 0.2f)) // A distinct, aggressive color for the deep dive
                        .WithPadding(10)
                        .WithFlexGrow(1))
                );
        }

        private void BindLogic(VisualElement root)
        {
            // Bind Dashboard Route
            var launchBtn = root.Q<Button>("btn_launch_nexus");
            if (launchBtn != null)
            {
                launchBtn.clicked -= OnLaunchDashboardClicked;
                launchBtn.clicked += OnLaunchDashboardClicked;
            }

            // Bind Deep Dive Route
            var deepDiveBtn = root.Q<Button>("btn_launch_deepdive");
            if (deepDiveBtn != null)
            {
                deepDiveBtn.clicked -= OnLaunchDeepDiveClicked;
                deepDiveBtn.clicked += OnLaunchDeepDiveClicked;
            }
        }

        private void OnLaunchDashboardClicked()
        {
            if (_router != null) _router.NavigateTo("MicroPackages_Dashboard");
            else Debug.LogError("[ShowcasePortal] Critical Failure: No router injected by the Lobby!");
        }

        private void OnLaunchDeepDiveClicked()
        {
            if (_router != null) _router.NavigateTo("MicroPackages_DeepDive_Telemetric");
            else Debug.LogError("[ShowcasePortal] Critical Failure: No router injected by the Lobby!");
        }

        // ==============================================================================
        // THE BAKING ENGINE INTEGRATION
        // ==============================================================================

        public void ToUIDocument(string assetPath)
        {
#if UNITY_EDITOR
            var dynamicBuilder = GetDynamicBuilder();
            dynamicBuilder.ToUIDocument(assetPath);
            Debug.Log($"[Baking Engine] Successfully baked {Title} to {assetPath}");
#else
            Debug.LogWarning("[Baking Engine] Baking UXML is an Editor-only capability.");
#endif
        }

        public void FromUIDocument(string assetPath)
        {
            _hydratedUxmlPath = assetPath;
        }
    }
}