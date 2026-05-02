using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Showcase;

namespace Workshop.Empire
{
    public class Empire_ShowcaseProvider : IShowcasePortal
    {
        // --- IGuiProvider Implementation ---
        public string Title => "Empire: A Tribute";

        // --- IShowcasePortal Hooks (Auto-Discovery) ---
        public bool IsDiscoverable => true;
        public string Category => "PART IV: STRATEGIC ABSTRACTIONS";
        public string Overview => "A tribute to Walter Bright's 1983 classic. Demonstrates rapid 4X strategy development using modern FSMs, pure UI grids, and custom Forge tooling.";
        public Color AccentColor => new Color(0.2f, 0.4f, 0.8f); // Empire Blue
        public List<string> CoreTechnologies => new List<string> { "Fog of War", "Turn-Based AI", "FSM Architecture", "Pure UI Rendering" };
        public string TelemetryState => "AWAITING COMMAND";

        public bool IsUnderConstruction => false;

        private IGuiRouter _router;
        private GuiContext _lastCtx;

        // 1. Required Parameterless Constructor for Reflection
        public Empire_ShowcaseProvider() { }

        // 2. Injected by the ExperienceRegistry before boot
        public void InjectRouter(IGuiRouter router)
        {
            _router = router;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            var root = new GraphicalUserInterfaceBuilder("EmpireShowcaseRoot")
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.05f))
                .Build();

            // The container that will swap between the Menu, Alpha, and Deep Dive
            var contentContainer = new VisualElement { style = { flexGrow = 1 } };
            root.Add(contentContainer);

            // Mount the Initial Welcome Screen
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

            // Title
            welcomeBuilder.AddChild(new ForgeLabelBuilder("EMPIRE: A TRIBUTE")
                .WithFontSize(45)
                .WithColor(Color.cyan)
                .WithFontStyle(FontStyle.Bold)
                .WithMarginBottom(20)
                .Build());

            // Credits & Context
            var descriptionText = "Originally written by Walter Bright in 1983/1984, Empire is widely considered the grandfather of the 4X strategy genre.\n\n" +
                                  "This project is not a replacement, but a heartfelt tribute. It serves to illustrate the incredible evolution of game development. " +
                                  "What once required painstaking terminal layouts, hardware-specific protocols, and manual memory management can now be rapidly assembled. " +
                                  "By leveraging modern C#, Finite State Machines, and custom Forge UI tooling, bringing these classic, deep mechanics to life is faster and cleaner than ever before.";

            welcomeBuilder.AddChild(new ForgeLabelBuilder(descriptionText)
                .WithFontSize(16)
                .WithColor(new Color(0.85f, 0.85f, 0.85f))
                .WithAlignment(TextAnchor.UpperCenter)
                .WithWhiteSpace(WhiteSpace.Normal)
                .OnBuild(l => l.style.width = 700)
                .WithMarginBottom(50)
                .Build());

            // Navigation Buttons Row
            var buttonRow = new GraphicalUserInterfaceBuilder("ButtonRow")
                .WithFlexLayout(FlexDirection.Row, Justify.Center, Align.Center)
                .Build();

            var playBtn = new ForgeButtonBuilder("LAUNCH PLAYABLE ALPHA")
                .WithBackgroundColor(new Color(0.1f, 0.5f, 0.2f))
                .WithTextColor(Color.white)
                .WithHeight(50)
                .WithWidth(260)
                .WithFontSize(14)
                .WithFontStyle(FontStyle.Bold)
                .WithMarginRight(20)
                .OnClick(() => {
                    container.Clear();
                    container.Add(new Empire_Playable_Alpha().CreateGui(ctx));
                })
                .CreateGui(ctx);

            var deepDiveBtn = new ForgeButtonBuilder("TECHNICAL DEEP DIVE")
                .WithBackgroundColor(new Color(0.2f, 0.4f, 0.8f))
                .WithTextColor(Color.white)
                .WithHeight(50)
                .WithWidth(260)
                .WithFontSize(14)
                .WithFontStyle(FontStyle.Bold)
                .OnClick(() => {
                    container.Clear();
                    container.Add(new Empire_TechnicalDeepDive(container, ctx).CreateGui(ctx));
                })
                .CreateGui(ctx);

            buttonRow.Add(playBtn);
            buttonRow.Add(deepDiveBtn);

            welcomeBuilder.AddChild(buttonRow);
            container.Add(welcomeBuilder.Build());
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }

    // ==============================================================================
    // TECHNICAL DEEP DIVE: ARCHITECTURAL EXPLODED VIEW
    // ==============================================================================
    public class Empire_TechnicalDeepDive : IGuiProvider
    {
        public string Title => "Empire: Technical Deep Dive";

        private VisualElement _parentContainer;
        private GuiContext _ctx;
        private Empire_Playable_Alpha _simulatedGame;

        public Empire_TechnicalDeepDive(VisualElement parentContainer, GuiContext ctx)
        {
            _parentContainer = parentContainer;
            _ctx = ctx;

            // 1. Initialize a "headless" instance of the game to inspect.
            _simulatedGame = new Empire_Playable_Alpha();
            _simulatedGame.CreateGui(new GuiContext());
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new GraphicalUserInterfaceBuilder("DeepDiveRoot")
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f))
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                .Build();

            // ==========================================
            // HEADER
            // ==========================================
            var header = new GraphicalUserInterfaceBuilder("Header")
                .WithPadding(20)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.06f))
                .WithBorderBottomColor(new Color(0.2f, 0.4f, 0.8f))
                .WithBorderBottomWidth(2)
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                .Build();

            header.Add(new ForgeLabelBuilder("ARCHITECTURAL EXPLODED VIEW")
                .WithFontSize(24)
                .WithColor(Color.cyan)
                .WithFontStyle(FontStyle.Bold)
                .Build());

            header.Add(new ForgeButtonBuilder("RETURN TO MENU")
                .WithBackgroundColor(new Color(0.6f, 0.2f, 0.2f))
                .WithTextColor(Color.white)
                .WithHeight(40)
                .WithWidth(180)
                .WithFontStyle(FontStyle.Bold)
                .OnClick(() => {
                    _parentContainer.Clear();
                    _parentContainer.Add(new Empire_ShowcaseProvider().CreateGui(ctx));
                })
                .CreateGui(ctx));

            root.Add(header);

            // ==========================================
            // CONTENT - SPLIT PANEL
            // ==========================================
            var contentSplit = new GraphicalUserInterfaceBuilder("ContentSplit")
                .WithFlexGrow(1)
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                .Build();

            // --- LEFT PANEL: LIVE GAMEPLAY (The Subject) ---
            var gamePanel = new GraphicalUserInterfaceBuilder("GamePanel")
                .WithFlexGrow(1)
                .WithPadding(20)
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.12f))
                .WithBorderRightColor(Color.black)
                .WithBorderRightWidth(2)
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                .Build();

            gamePanel.Add(new ForgeLabelBuilder("LIVE SIMULATION")
                .WithFontSize(18)
                .WithColor(Color.white)
                .WithFontStyle(FontStyle.Bold)
                .WithMarginBottom(5)
                .Build());

            gamePanel.Add(new ForgeLabelBuilder("Play the simulation below. Watch the Memory Inspector physically update in real-time on the right.")
                .WithFontSize(13)
                .WithColor(new Color(0.8f, 0.8f, 0.8f))
                .WithWhiteSpace(WhiteSpace.Normal)
                .WithMarginBottom(15)
                .Build());

            // Create a styled, rounded "monitor" container for the game
            var gameContainer = new VisualElement
            {
                style = {
                    flexGrow = 1,
                    overflow = Overflow.Hidden,
                    borderTopLeftRadius = 15, borderTopRightRadius = 15,
                    borderBottomLeftRadius = 15, borderBottomRightRadius = 15,
                    borderTopWidth = 2, borderBottomWidth = 2, borderLeftWidth = 2, borderRightWidth = 2,
                    borderTopColor = new Color(0.2f, 0.4f, 0.8f), borderBottomColor = new Color(0.2f, 0.4f, 0.8f),
                    borderLeftColor = new Color(0.2f, 0.4f, 0.8f), borderRightColor = new Color(0.2f, 0.4f, 0.8f),
                    backgroundColor = Color.black
                }
            };

            var gameScrollView = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
            gameScrollView.style.flexGrow = 1;

            // Mount the game directly into the sub-view
            var liveGameUI = _simulatedGame.CreateGui(ctx);
            gameScrollView.Add(liveGameUI);
            gameContainer.Add(gameScrollView);
            gamePanel.Add(gameContainer);

            contentSplit.Add(gamePanel);

            // --- RIGHT PANEL: LIVE REFLECTION (The X-Ray) ---
            var reflectionPanel = new GraphicalUserInterfaceBuilder("ReflectionPanel")
                .WithFlexGrow(1)
                .WithPadding(20)
                .WithBackgroundColor(new Color(0.06f, 0.06f, 0.08f))
                .Build();

            reflectionPanel.Add(new ForgeLabelBuilder("LIVE MEMORY INSPECTOR")
                .WithFontSize(18)
                .WithColor(Color.white)
                .WithFontStyle(FontStyle.Bold)
                .WithMarginBottom(5)
                .Build());

            reflectionPanel.Add(new ForgeLabelBuilder("The ReflectiveGuiBuilder is actively traversing the private memory footprint of the live game instance.")
                .WithFontSize(13)
                .WithColor(Color.gray)
                .WithWhiteSpace(WhiteSpace.Normal)
                .WithMarginBottom(20)
                .Build());

            var scrollView = new ScrollView(ScrollViewMode.Vertical);
            scrollView.style.flexGrow = 1;

            try
            {
                var inspectorUi = new ReflectiveGuiBuilder<Empire_Playable_Alpha>(_simulatedGame).CreateGui(ctx);
                scrollView.Add(inspectorUi);
            }
            catch (Exception ex)
            {
                scrollView.Add(new Label($"Error Reflecting Target: {ex.Message}") { style = { color = Color.red } });
            }

            reflectionPanel.Add(scrollView);
            contentSplit.Add(reflectionPanel);

            root.Add(contentSplit);

            return root;
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}