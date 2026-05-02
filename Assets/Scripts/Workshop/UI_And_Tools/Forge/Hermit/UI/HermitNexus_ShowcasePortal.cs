using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Showcase;

namespace Workshop.UI_And_Tools.Forge.Hermit.UI
{
    /// <summary>
    /// The unified Showcase Portal for all Hermit Tooling.
    /// Auto-discovered by the ExperienceRegistry via reflection.
    /// </summary>
    public class HermitNexus_ShowcasePortal : IShowcasePortal
    {
        private IGuiRouter _router;

        // --- IShowcasePortal Implementation ---
        public string Title => "Hermit Neural Nexus";
        public string Category => "THE SINGULARITY WORKSHOP";
        public string Overview => "Unified command center for the Hermit Autopoietic Entity. Manages Cortex uplinks and Diagnostic Merges.";
        public List<string> CoreTechnologies => new List<string> { "LocalHost LLM", "Sovereign Memory", "Data-Oriented DOD", "FSM Merge" };

        public bool IsDiscoverable => true;
        public bool IsUnderConstruction => false;
        public string TelemetryState => "NEXUS ONLINE";
        public Color AccentColor => Color.cyan;

        public void InjectRouter(IGuiRouter router) => _router = router;

        // --- The Sub-Tools comprising the Suite ---
        private readonly AI_Gui_Hermit _cortexGui = new AI_Gui_Hermit();
        private readonly AI_ReviewChamberGuiProvider _reviewGui = new AI_ReviewChamberGuiProvider();

        private VisualElement _contentContainer;
        private GuiContext _currentCtx;

        public VisualElement CreateGui(GuiContext ctx)
        {
            _currentCtx = ctx;

            var root = new GraphicalUserInterfaceBuilder("HermitNexusSuite")
                .WithFlexGrow(1)
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.06f))
                .Build();

            // --- Suite Navigation Sidebar ---
            var sidebar = new GraphicalUserInterfaceBuilder("NexusSidebar")
                .WithWidth(280)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f))
                .WithBorderRightWidth(2)
                .WithBorderRightColor(AccentColor)
                .WithPadding(15)
                .Build();

            // Foundational Identity Header
            sidebar.Add(new ForgeLabelBuilder("THE SINGULARITY WORKSHOP")
                .WithColor(Color.gray)
                .WithFontSize(10)
                .WithFontStyle(FontStyle.Bold)
                .WithMarginBottom(5)
                .CreateGui(ctx));

            sidebar.Add(new ForgeLabelBuilder("HERMIT NEXUS")
                .WithColor(AccentColor)
                .WithFontSize(20)
                .WithFontStyle(FontStyle.Bold)
                .WithMarginBottom(30)
                .CreateGui(ctx));

            // Tool Routing Buttons
            sidebar.Add(CreateNavButton("🧠 CORTEX UPLINK", () => RouteToTool(_cortexGui)));
            sidebar.Add(CreateNavButton("🔬 REVIEW CHAMBER", () => RouteToTool(_reviewGui)));

            // Spacer to push the exit button to the bottom
            var spacer = new VisualElement { style = { flexGrow = 1 } };
            sidebar.Add(spacer);

            sidebar.Add(new ForgeButtonBuilder("RETURN TO LOBBY")
                .WithHeight(40)
                .WithBackgroundColor(new Color(0.2f, 0.1f, 0.1f))
                .WithColor(Color.white)
                .WithFontStyle(FontStyle.Bold)
                .OnClick(() =>
                {
                    if (_router != null)
                    {
                        // Routes back to the main curated lobby in the ExperienceRegistry
                        _router.NavigateTo("Showcase_SingularityLobby");
                    }
                })
                .CreateGui(ctx));

            root.Add(sidebar);

            // --- Dynamic Tool Container ---
            _contentContainer = new GraphicalUserInterfaceBuilder("NexusContent")
                .WithFlexGrow(1)
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                .Build();

            root.Add(_contentContainer);

            // Mount the default route
            RouteToTool(_cortexGui);

            return root;
        }

        private VisualElement CreateNavButton(string label, Action onClick)
        {
            return new ForgeButtonBuilder(label)
                .WithHeight(45)
                .WithMarginBottom(12)
                .WithBackgroundColor(new Color(0.15f, 0.15f, 0.18f))
                .WithColor(Color.white)
                .WithFontStyle(FontStyle.Bold)
                .OnClick(onClick)
                .OnBuild(ve =>
                {
                    ve.RegisterCallback<MouseEnterEvent>(e => ve.style.backgroundColor = new Color(0.25f, 0.25f, 0.3f));
                    ve.RegisterCallback<MouseLeaveEvent>(e => ve.style.backgroundColor = new Color(0.15f, 0.15f, 0.18f));
                })
                .CreateGui(_currentCtx);
        }

        private void RouteToTool(IGuiProvider toolProvider)
        {
            // O(1) Hot-swapping of the active UI tool
            _contentContainer.Clear();
            var toolGui = toolProvider.CreateGui(_currentCtx);
            toolGui.style.flexGrow = 1;
            _contentContainer.Add(toolGui);
        }

        public Action<VisualElement> GetGuiBuilder() => r => r.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string p) { }
        public void FromUIDocument(string p) { }
    }
}