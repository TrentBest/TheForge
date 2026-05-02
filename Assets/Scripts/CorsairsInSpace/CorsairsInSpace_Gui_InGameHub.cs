using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Corsair
{
    public class CorsairsInSpace_Gui_InGameHub : IGuiProvider
    {
        public string Title => "CORSAIR SYNDICATE: BRIDGE TERMINAL";

        private GuiContext _lastCtx;
        private IGuiRouter _router;

        public CorsairsInSpace_Gui_InGameHub() { }
        public CorsairsInSpace_Gui_InGameHub(IGuiRouter router) { _router = router; }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            var builder = new GraphicalUserInterfaceBuilder("Corsairs_InGameHub_Root")
                .WithPercentSize(100, 100)
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.08f)); // Deep space background

            // --- HEADER ---
            builder.AddChild(new GraphicalUserInterfaceBuilder("HeaderContainer")
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)
                .WithPadding(20)
                .WithBackgroundColor(new Color(0, 0, 0, 0.8f))
                .WithBorderBottomColor(Color.cyan)
                .WithBorderBottomWidth(2)
                .AddChild(new ForgeLabelBuilder("SYNDICATE COMMAND BRIDGE")
                    .WithFontSize(32).WithColor(Color.cyan).WithFontStyle(FontStyle.Bold))
                .AddChild(new ForgeLabelBuilder("SELECT OPERATION PROTOCOL")
                    .WithFontSize(14).WithColor(Color.gray).WithMargin(5, 0)));

            // --- STAGGERED VERTICAL LIST ---
            var listContainer = new GraphicalUserInterfaceBuilder("StaggeredList")
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Stretch)
                .WithAutoGrow(true)
                .WithPaddingTop(40);

            // Item 1: Button on Left
            listContainer.AddChild(CreateStaggeredRow("LAIR ARCHITECT", "Expand your subterranean base and excavate voxels.",
                new Color(0.15f, 0.1f, 0.05f), "LairArchitect", true, ctx));

            // Item 2: Button on Right
            listContainer.AddChild(CreateStaggeredRow("OPERATIONS", "Manage your roster of Workers, Scientists, and Engineers.",
                new Color(0.05f, 0.15f, 0.05f), "OpsManager", false, ctx));

            // Item 3: Button on Left
            listContainer.AddChild(CreateStaggeredRow("SHIPYARD", "Design and construct your armada chassis.",
                new Color(0.15f, 0.05f, 0.05f), "Shipyard", true, ctx));

            // Item 4: Button on Right
            listContainer.AddChild(CreateStaggeredRow("NAVIGATION", "Plot hyperspace routes and launch raid operations.",
                new Color(0.05f, 0.05f, 0.15f), "Navigation", false, ctx));

            builder.AddChild(listContainer);

            return builder.Build();
        }

        private IGuiProvider CreateStaggeredRow(string title, string subtitle, Color btnColor, string routeId, bool buttonOnLeft, GuiContext ctx)
        {
            var row = new GraphicalUserInterfaceBuilder($"Row_{title}")
                .WithFlexLayout(FlexDirection.Row, Justify.Center, Align.Center)
                .WithMarginTop(10)
                .WithMarginBottom(10);

            var clickAction = new Action(() => {
                Debug.Log($"Routing to {routeId}...");
                if (_router != null) _router.NavigateTo(routeId);
            });

            var btn = new ForgeButtonBuilder(title, clickAction)
                .WithBackgroundColor(btnColor)
                .WithTextColor(Color.white)
                .WithFontSize(22)
                .WithFontStyle(FontStyle.Bold)
                .WithHeight(80)
                .WithWidth(350);

            var descBuilder = new GraphicalUserInterfaceBuilder("Desc")
                .WithFlexLayout(FlexDirection.Column, Justify.Center, buttonOnLeft ? Align.FlexStart : Align.FlexEnd)
                .WithPadding(20)
                .WithWidth(300)
                .AddChild(new ForgeLabelBuilder(subtitle)
                    .WithFontSize(14)
                    .WithColor(Color.gray));

            // Stagger Logic
            if (buttonOnLeft)
            {
                row.AddChild(btn);
                row.AddChild(descBuilder);
            }
            else
            {
                row.AddChild(descBuilder);
                row.AddChild(btn);
            }

            return row;
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
#if UNITY_EDITOR
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
#endif
        public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
    }
}