using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Showcase;

namespace Workshop.ReachForTheStars
{
    public class RftS_ShowcaseProvider : IShowcasePortal
    {
        public string Title => "Reach for the Stars";

        public bool IsDiscoverable => true;
        public string Category => "PART IV: STRATEGIC ABSTRACTIONS";
        public string Overview => "A tribute to SSG's 1983 pioneer of the Sci-Fi 4X genre. Demonstrates node-based economic simulation and deep-space fleet logistics over a pure UI grid.";
        public Color AccentColor => new Color(0.8f, 0.4f, 0.0f); // Deep Space Orange/Gold
        public List<string> CoreTechnologies => new List<string> { "Node-Based Maps", "Planetary Economies", "Blind AI", "Vector UI Rendering" };
        public string TelemetryState => "AWAITING LAUNCH SEQUENCE";

        private IGuiRouter _router;
        private GuiContext _lastCtx;
        public bool IsUnderConstruction => true;
        public RftS_ShowcaseProvider() { }

        public void InjectRouter(IGuiRouter router)
        {
            _router = router;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            var root = new GraphicalUserInterfaceBuilder("RftS_ShowcaseRoot")
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.02f, 0.02f, 0.05f)) // Very dark space blue
                .Build();

            var contentContainer = new VisualElement { style = { flexGrow = 1 } };
            root.Add(contentContainer);

            ShowWelcomeScreen(contentContainer, ctx);

            return root;
        }

        private void ShowWelcomeScreen(VisualElement container, GuiContext ctx)
        {
            container.Clear();

            var welcomeBuilder = new GraphicalUserInterfaceBuilder("WelcomeView")
                .WithFlexGrow(1)
                .WithPadding(40)
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center);

            welcomeBuilder.AddChild(new ForgeLabelBuilder("REACH FOR THE STARS")
                .WithFontSize(45)
                .WithColor(new Color(1f, 0.6f, 0.1f))
                .WithFontStyle(FontStyle.Bold)
                .WithMarginBottom(20)
                .Build());

            var descriptionText = "Released in 1983 by Strategic Studies Group, 'Reach for the Stars' is widely recognized as the very first Sci-Fi 4X game. " +
                                  "It shifted the focus from tactical hex-crawling to macro-level planetary economics: balancing Population growth, Industrial output, and Environmental degradation.\n\n" +
                                  "This tribute utilizes our custom Forge UI architecture to render the cosmos and its data. Instead of raw memory manipulation on an Apple II, " +
                                  "we use strongly-typed C# arrays and FSMs to power the sprawling logistics of an interstellar empire.";

            welcomeBuilder.AddChild(new ForgeLabelBuilder(descriptionText)
                .WithFontSize(16)
                .WithColor(new Color(0.85f, 0.85f, 0.85f))
                .WithAlignment(TextAnchor.UpperCenter)
                .WithWhiteSpace(WhiteSpace.Normal)
                .OnBuild(l => l.style.width = 750)
                .WithMarginBottom(50)
                .Build());

            var buttonRow = new GraphicalUserInterfaceBuilder("ButtonRow")
                .WithFlexLayout(FlexDirection.Row, Justify.Center, Align.Center)
                .Build();

            var playBtn = new ForgeButtonBuilder("LAUNCH STAR MAP")
                .WithBackgroundColor(new Color(0.6f, 0.3f, 0.0f))
                .WithTextColor(Color.white)
                .WithHeight(50)
                .WithWidth(260)
                .WithFontSize(14)
                .WithFontStyle(FontStyle.Bold)
                .WithMarginRight(20)
                .OnClick(() => {
                    container.Clear();
                    container.Add(new RftS_Playable_Alpha().CreateGui(ctx));
                })
                .CreateGui(ctx);

            var deepDiveBtn = new ForgeButtonBuilder("ECONOMIC ALGORITHMS")
     .WithBackgroundColor(new Color(0.2f, 0.3f, 0.6f))
     .WithTextColor(Color.white)
     .WithHeight(50)
     .WithWidth(260)
     .WithFontSize(14)
     .WithFontStyle(FontStyle.Bold)
     .OnClick(() => {
         container.Clear();
         container.Add(new RftS_TechnicalDeepDive(container, ctx).CreateGui(ctx));
     })
     .CreateGui(ctx);

            var interceptBtn = new ForgeButtonBuilder("CUSTOM UI REGISTRY")
                .WithBackgroundColor(new Color(0.6f, 0.2f, 0.6f)) // Purple Intercept color
                .WithTextColor(Color.white)
                .WithHeight(50)
                .WithWidth(260)
                .WithFontSize(14)
                .WithFontStyle(FontStyle.Bold)
                .WithMarginLeft(20)
                .OnClick(() => {
                    container.Clear();
                    container.Add(new RftS_CustomRegistryDeepDive(container, ctx).CreateGui(ctx));
                })
                .CreateGui(ctx);

            buttonRow.Add(playBtn);
            buttonRow.Add(deepDiveBtn);
            buttonRow.Add(interceptBtn);

            welcomeBuilder.AddChild(buttonRow);
            container.Add(welcomeBuilder.Build());
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}