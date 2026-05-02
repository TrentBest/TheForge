using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.ReachForTheStars
{
    // ================================================================
    // THE CUSTOM GUI PROVIDER
    // This is the beautiful, hand-crafted UI that will intercept Reflection
    // ================================================================
    public class StarSystemCustomGuiProvider : IGuiProvider
    {
        public string Title => _star.Name;
        private StarSystem _star;

        public StarSystemCustomGuiProvider(StarSystem star)
        {
            _star = star;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            Color ownerCol = _star.OwnerId == 1 ? Color.cyan : (_star.OwnerId == 2 ? Color.red : Color.gray);

            var root = new GraphicalUserInterfaceBuilder("CustomStarCard")
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.15f))
                .WithBorderColor(ownerCol)
                .WithBorderWidth(2)
                .WithPadding(10)
                .WithMarginBottom(10)
                .WithFlexLayout(FlexDirection.Column)
                .Build();

            // Header
            root.Add(new ForgeLabelBuilder($"★ {_star.Name.ToUpper()}")
                .WithFontSize(16)
                .WithColor(ownerCol)
                .WithFontStyle(FontStyle.Bold)
                .Build());

            // Key Stats Row
            var statRow = new GraphicalUserInterfaceBuilder("StatRow")
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                .WithMarginTop(10)
                .Build();

            statRow.Add(new ForgeLabelBuilder($"POP: {_star.Population}/{_star.MaxPopulation}")
                .WithColor(Color.green).WithFontSize(12).Build());

            statRow.Add(new ForgeLabelBuilder($"TECH: LVL {_star.TechLevel}")
                .WithColor(Color.yellow).WithFontSize(12).Build());

            root.Add(statRow);

            // Fleet Sub-box
            var fleetBox = new GraphicalUserInterfaceBuilder("FleetBox")
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.08f))
                .WithPadding(5).WithMarginTop(10)
                .WithBorderColor(new Color(0.2f, 0.2f, 0.3f)).WithBorderWidth(1)
                .Build();

            fleetBox.Add(new ForgeLabelBuilder($"Warships: {_star.Warships}  |  Transports: {_star.Transports}  |  Scouts: {_star.Scouts}")
                .WithColor(new Color(0.8f, 0.8f, 1f)).WithFontSize(11).Build());

            root.Add(fleetBox);
            return root;
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }


    // ================================================================
    // THE SHOWCASE DEEP DIVE
    // ================================================================
    public class RftS_CustomRegistryDeepDive : IGuiProvider
    {
        public string Title => "RftS: Custom UI Registry";

        private VisualElement _parentContainer;
        private GuiContext _ctx;
        private RftS_Playable_Alpha _simulatedGame;

        public RftS_CustomRegistryDeepDive(VisualElement parentContainer, GuiContext ctx)
        {
            _parentContainer = parentContainer;
            _ctx = ctx;

            // 1. STACK THE DECK!
            // Register our custom provider before the reflection tool fires up.
            TypeGuiProviderFactory.Register<StarSystem>(star => new StarSystemCustomGuiProvider(star));

            // 2. Initialize a headless instance of the game
            _simulatedGame = new RftS_Playable_Alpha();
            _simulatedGame.CreateGui(new GuiContext());
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new GraphicalUserInterfaceBuilder("DeepDiveRoot")
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.02f, 0.02f, 0.04f))
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                .Build();

            var header = new GraphicalUserInterfaceBuilder("Header")
                .WithPadding(20).WithBackgroundColor(new Color(0.05f, 0.05f, 0.06f))
                .WithBorderBottomColor(new Color(0.6f, 0.2f, 0.6f)) // Purple for Custom UI
                .WithBorderBottomWidth(2)
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                .Build();

            header.Add(new ForgeLabelBuilder("FACTORY INTERCEPTION (CUSTOM UI REGISTRY)")
                .WithFontSize(24).WithColor(Color.white).WithFontStyle(FontStyle.Bold).Build());

            header.Add(new ForgeButtonBuilder("RETURN TO MENU")
                .WithBackgroundColor(new Color(0.6f, 0.2f, 0.2f))
                .WithTextColor(Color.white).WithHeight(40).WithWidth(180).WithFontStyle(FontStyle.Bold)
                .OnClick(() => {
                    _parentContainer.Clear();
                    _parentContainer.Add(new RftS_ShowcaseProvider().CreateGui(ctx));
                }).CreateGui(ctx));

            root.Add(header);

            var contentSplit = new GraphicalUserInterfaceBuilder("ContentSplit")
                .WithFlexGrow(1).WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch).Build();

            // LEFT PANEL: Explanation
            var explainerPanel = new GraphicalUserInterfaceBuilder("ExplainerPanel")
                .WithFlexGrow(1).WithPadding(20).WithBackgroundColor(new Color(0.06f, 0.06f, 0.09f))
                .WithBorderRightColor(Color.black).WithBorderRightWidth(2)
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                .Build();

            explainerPanel.Add(new ForgeLabelBuilder("THE 'STACKED DECK' PATTERN")
                .WithFontSize(18).WithColor(new Color(0.8f, 0.4f, 1f)).WithFontStyle(FontStyle.Bold).WithMarginBottom(15).Build());

            explainerPanel.Add(new ForgeLabelBuilder("In the previous Deep Dive, the Memory Inspector used raw Reflection to break down the StarSystem objects into lists of primitive integers and strings.\n\n" +
                                                     "Here, we executed this line of code before rendering:\n\n" +
                                                     "TypeGuiProviderFactory.Register<StarSystem>(star => new StarSystemCustomGuiProvider(star));\n\n" +
                                                     "Now, when the ReflectiveGuiBuilder hits the _activeStars list, it bypasses reflection and instantly serves up the custom UI card. Expand the _activeStars collection on the right to see the interception in action!")
                .WithFontSize(14).WithColor(new Color(0.8f, 0.8f, 0.8f)).WithWhiteSpace(WhiteSpace.Normal).Build());

            contentSplit.Add(explainerPanel);

            // RIGHT PANEL: LIVE REFLECTION
            var reflectionPanel = new GraphicalUserInterfaceBuilder("ReflectionPanel")
                .WithFlexGrow(2).WithPadding(20).WithBackgroundColor(new Color(0.03f, 0.03f, 0.05f))
                .Build();

            reflectionPanel.Add(new ForgeLabelBuilder("INTERCEPTED MEMORY INSPECTOR")
                .WithFontSize(18).WithColor(Color.white).WithFontStyle(FontStyle.Bold).WithMarginBottom(15).Build());

            var rightScrollView = new ForgeScrollViewBuilder("InspectorScroll")
                .OnBuild(v => {
                    v.style.flexGrow = 1;
                    if (v is ScrollView sv) sv.mode = ScrollViewMode.Vertical;
                }).Build();

            try
            {
                var inspectorUi = new ReflectiveGuiBuilder<RftS_Playable_Alpha>(_simulatedGame).CreateGui(ctx);
                rightScrollView.Add(inspectorUi);
            }
            catch (Exception ex)
            {
                rightScrollView.Add(new ForgeLabelBuilder($"Error Reflecting Target: {ex.Message}").WithColor(Color.red).Build());
            }

            reflectionPanel.Add(rightScrollView);
            contentSplit.Add(reflectionPanel);

            root.Add(contentSplit);
            return root;
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}