using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.UI_And_Tools.Showcase
{
    public class Showcase_SingularityLobby : IGuiProvider
    {
        public string Title => "SINGULARITY LOBBY DASHBOARD";

        private readonly IGuiRouter _router;
        private GuiContext _context;

        public Showcase_SingularityLobby(IGuiRouter router)
        {
            _router = router;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _context = ctx;

            var rootBuilder = new GraphicalUserInterfaceBuilder("LobbyRoot")
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.06f))
                .WithScrollable(true, ScrollViewMode.Vertical);

            // ==========================================
            // THE MANIFESTO (Header)
            // ==========================================
            var headerPanel = new GraphicalUserInterfaceBuilder("ManifestoHeader")
                .WithPadding(40)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f))
                .WithBorderBottomWidth(2).WithBorderBottomColor(new Color(0.8f, 0.1f, 0.8f));

            headerPanel.AddChild(new ForgeLabelBuilder("THE SINGULARITY WORKSHOP")
                .WithColor(Color.white).WithFontSize(32).WithFontStyle(FontStyle.Bold));

            string manifestoText = "Historically, software engineering has been a bottleneck to human imagination. " +
                                   "The Singularity is not a robotic revolution; it is the total eradication of technical friction, " +
                                   "allowing pure creative intent to be instantly materialized. " +
                                   "Through Sovereign Data, FSM-governed logic, and zero-GC performance, these tools flatten the field. " +
                                   "Welcome to the new performant generation of software.";

            headerPanel.AddChild(new ForgeLabelBuilder(manifestoText)
                .WithColor(Color.gray).WithFontSize(14).WithMarginTop(10).WithWhiteSpace(WhiteSpace.Normal));

            rootBuilder.AddChild(headerPanel);

            // ==========================================
            // THE GRID (Data Cards)
            // ==========================================
            var gridWrapper = new GraphicalUserInterfaceBuilder("GridWrapper")
                .WithPadding(30)
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.FlexStart)
                .WithFlexWrap(Wrap.Wrap);

            var registry = ExperienceRegistry.GetDiscoveredPortals(_router);

            foreach (var category in registry)
            {
                // Category Header spans full width
                gridWrapper.AddChild(new GraphicalUserInterfaceBuilder($"Category_{category.Key}")
                    .WithWidth(new Length(100, LengthUnit.Percent))
                    .WithMarginBottom(10).WithMarginTop(20)
                    .WithBorderBottomWidth(1).WithBorderBottomColor(Color.gray)
                    .AddChild(new ForgeLabelBuilder(category.Key).WithColor(Color.cyan).WithFontSize(18).WithFontStyle(FontStyle.Bold)));

                foreach (var portal in category.Value)
                {
                    gridWrapper.AddChild(CreateDataCard(portal));
                }
            }

            rootBuilder.AddChild(gridWrapper);
            return rootBuilder.Build();
        }

        private IGuiProvider CreateDataCard(IShowcasePortal portal)
        {
            var cardBuilder = new GraphicalUserInterfaceBuilder($"Card_{portal.Title}")
                .WithWidth(300).WithHeight(200)
                .WithMargin(10)
                .WithPadding(15)
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.12f))
                .WithBorderTopWidth(3).WithBorderTopColor(portal.AccentColor)
                .WithFlexLayout(FlexDirection.Column, Justify.SpaceBetween, Align.Stretch);

            // Title & Overview
            var topSection = new GraphicalUserInterfaceBuilder("CardTop")
                .AddChild(new ForgeLabelBuilder(portal.Title).WithColor(Color.white).WithFontSize(16).WithFontStyle(FontStyle.Bold))
                .AddChild(new ForgeLabelBuilder(portal.Overview).WithColor(Color.gray).WithFontSize(12).WithWhiteSpace(WhiteSpace.Normal).WithMarginTop(5));

            cardBuilder.AddChild(topSection);

            // Action Button
            cardBuilder.AddChild(new ForgeButtonBuilder("ENTER DOMAIN")
                .WithBackgroundColor(new Color(0.2f, 0.2f, 0.2f))
                .OnClick(() =>
                {
                    // This creates the Dominant View takeover!
                    var dominantView = portal.CreateGui(_context);

                    // We cheat the router slightly here by pushing a visual element directly 
                    // over the screen, or you can route to it if you have a dynamic route wrapper.
                    // Assuming your portal is registered as a route:
                    _router.NavigateTo(portal.GetType().Name);
                }));

            return cardBuilder;
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}