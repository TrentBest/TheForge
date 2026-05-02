using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.ReachForTheStars
{
    public class RftS_TechnicalDeepDive : IGuiProvider
    {
        public string Title => "RftS: Technical Deep Dive";

        private VisualElement _parentContainer;
        private GuiContext _ctx;

        // The live target we will reflect over
        private RftS_Playable_Alpha _simulatedGame;

        public RftS_TechnicalDeepDive(VisualElement parentContainer, GuiContext ctx)
        {
            _parentContainer = parentContainer;
            _ctx = ctx;

            // Initialize a headless instance of the game
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

            // --- HEADER ---
            var header = new GraphicalUserInterfaceBuilder("Header")
                .WithPadding(20)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.06f))
                .WithBorderBottomColor(new Color(0.8f, 0.4f, 0.0f)) // Space Orange
                .WithBorderBottomWidth(2)
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                .Build();

            header.Add(new ForgeLabelBuilder("ARCHITECTURAL EXPLODED VIEW")
                .WithFontSize(24).WithColor(Color.white).WithFontStyle(FontStyle.Bold).Build());

            header.Add(new ForgeButtonBuilder("RETURN TO MENU")
                .WithBackgroundColor(new Color(0.6f, 0.2f, 0.2f))
                .WithTextColor(Color.white).WithHeight(40).WithWidth(180).WithFontStyle(FontStyle.Bold)
                .OnClick(() => {
                    _parentContainer.Clear();
                    _parentContainer.Add(new RftS_ShowcaseProvider().CreateGui(ctx));
                })
                .CreateGui(ctx));

            root.Add(header);

            // --- CONTENT SPLIT ---
            var contentSplit = new GraphicalUserInterfaceBuilder("ContentSplit")
                .WithFlexGrow(1).WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch).Build();

            // LEFT PANEL: LIVE GAMEPLAY
            var gamePanel = new GraphicalUserInterfaceBuilder("GamePanel")
                .WithFlexGrow(1).WithPadding(20).WithBackgroundColor(new Color(0.06f, 0.06f, 0.09f))
                .WithBorderRightColor(Color.black).WithBorderRightWidth(2)
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                .Build();

            gamePanel.Add(new ForgeLabelBuilder("LIVE SIMULATION")
                .WithFontSize(18).WithColor(new Color(0.8f, 0.4f, 0.0f)).WithFontStyle(FontStyle.Bold).WithMarginBottom(5).Build());

            gamePanel.Add(new ForgeLabelBuilder("Play the RftS simulation. Watch the Treasury drop and the FSM arrays update in the Memory Inspector.")
                .WithFontSize(13).WithColor(Color.gray).WithWhiteSpace(WhiteSpace.Normal).WithMarginBottom(15).Build());

            // Swapped out the raw VisualElement for a strict builder
            var gameContainer = new GraphicalUserInterfaceBuilder("GameContainer")
                .WithBackgroundColor(Color.black)
                .WithBorderColor(new Color(0.8f, 0.4f, 0.0f))
                .WithBorderWidth(2)
                .OnBuild(v => {
                    v.style.flexGrow = 1;
                    v.style.overflow = Overflow.Hidden;
                    v.style.borderTopLeftRadius = 15;
                    v.style.borderTopRightRadius = 15;
                    v.style.borderBottomLeftRadius = 15;
                    v.style.borderBottomRightRadius = 15;
                })
                .Build();

            // Replaced the raw ScrollView with the ForgeScrollViewBuilder
            var gameScrollView = new ForgeScrollViewBuilder("GameScrollView")
                .OnBuild(v => {
                    v.style.flexGrow = 1;
                    if (v is ScrollView sv) sv.mode = ScrollViewMode.VerticalAndHorizontal;
                })
                .AddChild(_simulatedGame.CreateGui(ctx))
                .Build();

            gameContainer.Add(gameScrollView);
            gamePanel.Add(gameContainer);
            contentSplit.Add(gamePanel);

            // RIGHT PANEL: LIVE REFLECTION
            var reflectionPanel = new GraphicalUserInterfaceBuilder("ReflectionPanel")
                .WithFlexGrow(1).WithPadding(20).WithBackgroundColor(new Color(0.03f, 0.03f, 0.05f))
                .Build();

            reflectionPanel.Add(new ForgeLabelBuilder("LIVE MEMORY INSPECTOR")
                .WithFontSize(18).WithColor(Color.white).WithFontStyle(FontStyle.Bold).WithMarginBottom(5).Build());

            reflectionPanel.Add(new ForgeLabelBuilder("The ReflectiveGuiBuilder is actively traversing the private state. Note how the economic data is decoupled from the Vector rendering.")
                .WithFontSize(13).WithColor(Color.gray).WithWhiteSpace(WhiteSpace.Normal).WithMarginBottom(20).Build());

            // Replaced the raw ScrollView with the ForgeScrollViewBuilder
            var rightScrollView = new ForgeScrollViewBuilder("InspectorScroll")
                .OnBuild(v => {
                    v.style.flexGrow = 1;
                    if (v is ScrollView sv) sv.mode = ScrollViewMode.Vertical;
                })
                .Build();

            try
            {
                var inspectorUi = new ReflectiveGuiBuilder<RftS_Playable_Alpha>(_simulatedGame).CreateGui(ctx);
                rightScrollView.Add(inspectorUi);
            }
            catch (Exception ex)
            {
                // Replaced the raw Label with ForgeLabelBuilder
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