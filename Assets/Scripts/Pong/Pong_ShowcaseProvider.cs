using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Showcase;

namespace Workshop.Pong
{
    public class Pong_ShowcaseProvider : IShowcasePortal
    {
        public string Title => "Singularity Pong";
        public bool IsDiscoverable => true;
        public string Category => "PART I: KINETIC FOUNDATIONS";
        public string Overview => "A demonstration of basic physics, UI Toolkit rendering, and autonomous AI integration.";
        public Color AccentColor => new Color(0.8f, 0.1f, 0.8f); // Singularity Purple
        public List<string> CoreTechnologies => new List<string> { "UI Toolkit Loop", "Heuristic AI", "Input Polling", "Network Stubs" };
        public string TelemetryState => "AWAITING SIGN-IN";

        private IGuiRouter _router;
        private GuiContext _ctx;

        // LMPB State
        private Label _opponentLmpbVisual;
        private Label _opponentLmpbTitle;
        public bool IsUnderConstruction => false;
        public Pong_ShowcaseProvider() { }

        public void InjectRouter(IGuiRouter router)
        {
            _router = router;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _ctx = ctx;

            var root = new GraphicalUserInterfaceBuilder("PongShowcaseRoot")
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.05f))
                .Build();

            var contentContainer = new VisualElement { style = { flexGrow = 1 } };
            root.Add(contentContainer);

            ShowMainMenu(contentContainer);

            return root;
        }

        private void ShowMainMenu(VisualElement container)
        {
            container.Clear();

            var menuBuilder = new GraphicalUserInterfaceBuilder("MainMenu")
                .WithFlexGrow(1)
                .WithPadding(20)
                .WithFlexLayout(FlexDirection.Column, Justify.SpaceBetween, Align.Stretch);

            // ==========================================
            // HEADER: HISTORY & INFO
            // ==========================================
            var header = new GraphicalUserInterfaceBuilder("Header")
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)
                .WithMarginBottom(20)
                .Build();

            header.Add(new ForgeLabelBuilder("SINGULARITY PONG")
                .WithFontSize(40)
                .WithColor(new Color(0.8f, 0.1f, 0.8f))
                .WithFontStyle(FontStyle.Bold)
                .Build());

            string historyText = "The kinetic grandfather of interactive media. This showcase rebuilds the 1972 classic using pure UI Toolkit elements, " +
                                 "driven by modern Forge Builders and real-time state machines. Choose your opponent below to witness deterministic heuristics, " +
                                 "neural network simulations, or connect to the grid.";

            header.Add(new ForgeLabelBuilder(historyText)
                .WithFontSize(14)
                .WithColor(Color.gray)
                .WithAlignment(TextAnchor.UpperCenter)
                .WithWhiteSpace(WhiteSpace.Normal)
                .OnBuild(l => l.style.width = 600)
                .Build());

            menuBuilder.AddChild(header);

            // ==========================================
            // CENTER STAGE: LMPB & 4-QUADRANT SELECTION
            // ==========================================
            var centerStage = new GraphicalUserInterfaceBuilder("CenterStage")
                .WithFlexGrow(1)
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Stretch)
                .Build();

            // LEFT: Player Avatar LMPB (Placeholder)
            var playerLmpb = CreateLmpb("PLAYER ONE", "P1", Color.cyan, "Awaiting Telemetry...");
            centerStage.Add(playerLmpb);

            // MIDDLE: The 4 Quadrants & Sign In
            var quadrantArea = new GraphicalUserInterfaceBuilder("QuadrantArea")
                .WithFlexGrow(2)
                .WithMarginLeft(20).WithMarginRight(20)
                .OnBuild(ve => ve.style.position = Position.Relative) // Required for absolute center button
                .Build();

            // Top Row
            var topRow = new GraphicalUserInterfaceBuilder("QuadTopRow")
                .WithFlexGrow(1).WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Stretch).Build();
            topRow.Add(CreateQuadButton("LOCAL HOTSEAT", new Color(0.2f, 0.6f, 0.8f), container, PongGameMode.Hotseat, "H", "Human Input Detected"));
            topRow.Add(CreateQuadButton("VS AI (HERMIT)", new Color(0.6f, 0.6f, 0.2f), container, PongGameMode.AiHermit, "Ω", "Heuristic Lerp Active"));

            // Bottom Row
            var bottomRow = new GraphicalUserInterfaceBuilder("QuadBottomRow")
                .WithFlexGrow(1).WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Stretch).Build();
            bottomRow.Add(CreateQuadButton("VS AI (NEURAL NET)", new Color(0.8f, 0.2f, 0.2f), container, PongGameMode.AiNeuralNet, "⎈", "NEURONS FIRING [1024 Nodes]"));
            bottomRow.Add(CreateQuadButton("MULTIPLAYER", new Color(0.4f, 0.2f, 0.8f), container, PongGameMode.Multiplayer, "∞", "Connecting to Grid..."));

            quadrantArea.Add(topRow);
            quadrantArea.Add(bottomRow);

            // Absolute Center: SIGN IN BUTTON
            var signInBtn = new ForgeButtonBuilder("AUTHENTICATE")
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.15f))
                .WithTextColor(Color.white)
                .WithHeight(80).WithWidth(160)
                .WithFontStyle(FontStyle.Bold)
                .OnBuild(ve => {
                    ve.style.position = Position.Absolute;
                    ve.style.top = Length.Percent(50);
                    ve.style.left = Length.Percent(50);
                    ve.style.translate = new Translate(Length.Percent(-50), Length.Percent(-50));
                    ve.style.borderTopColor = Color.cyan; ve.style.borderBottomColor = Color.cyan;
                    ve.style.borderLeftColor = Color.cyan; ve.style.borderRightColor = Color.cyan;
                    ve.style.borderTopWidth = 2; ve.style.borderBottomWidth = 2;
                    ve.style.borderLeftWidth = 2; ve.style.borderRightWidth = 2;
                    ve.style.borderTopLeftRadius = 40; ve.style.borderBottomRightRadius = 40; // Stylized shape
                })
                .OnClick(() => Debug.Log("Player Signed In!"))
                .CreateGui(_ctx);

            quadrantArea.Add(signInBtn);
            centerStage.Add(quadrantArea);

            // RIGHT: Opponent LMPB (Dynamic)
            var opponentLmpbContainer = CreateLmpb("OPPONENT", "?", Color.gray, "Select a mode...");
            _opponentLmpbVisual = opponentLmpbContainer.Q<Label>("LmpbIcon");
            _opponentLmpbTitle = opponentLmpbContainer.Q<Label>("LmpbStatus");
            centerStage.Add(opponentLmpbContainer);

            menuBuilder.AddChild(centerStage);

            // ==========================================
            // FOOTER: DEEP DIVE
            // ==========================================
            var footer = new GraphicalUserInterfaceBuilder("Footer")
                .WithFlexLayout(FlexDirection.Row, Justify.Center, Align.Center)
                .WithMarginTop(20)
                .Build();

            footer.Add(new ForgeButtonBuilder("TECHNICAL DEEP DIVE")
                .WithBackgroundColor(new Color(0.2f, 0.2f, 0.2f))
                .WithTextColor(Color.white)
                .WithHeight(40).WithWidth(250)
                .OnClick(() => ShowDeepDive(container))
                .CreateGui(_ctx));

            menuBuilder.AddChild(footer);
            container.Add(menuBuilder.Build());
        }

        // --- Helper Builders ---

        private VisualElement CreateLmpb(string header, string defaultIcon, Color color, string defaultStatus)
        {
            var lmpb = new GraphicalUserInterfaceBuilder($"LMPB_{header}")
                .WithWidth(220)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f))
                .WithBorderColor(color).WithBorderWidth(1)
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Center)
                .WithPadding(10)
                .Build();

            lmpb.Add(new ForgeLabelBuilder(header).WithColor(color).WithFontStyle(FontStyle.Bold).WithMarginBottom(10).Build());

            var visualBox = new GraphicalUserInterfaceBuilder("VisualBox")
                .WithWidth(180).WithHeight(250)
                .WithBackgroundColor(Color.black)
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)
                .Build();

            var iconLabel = new ForgeLabelBuilder(defaultIcon)
                .WithFontSize(80).WithColor(color)
                .OnBuild(ve => ve.name = "LmpbIcon")
                .Build();

            visualBox.Add(iconLabel);
            lmpb.Add(visualBox);

            var statusLabel = new ForgeLabelBuilder(defaultStatus)
                .WithColor(Color.gray).WithFontSize(10)
                .WithMarginTop(10).WithWhiteSpace(WhiteSpace.Normal)
                .WithAlignment(TextAnchor.UpperCenter)
                .OnBuild(ve => ve.name = "LmpbStatus")
                .Build();

            lmpb.Add(statusLabel);

            return lmpb;
        }

        private VisualElement CreateQuadButton(string text, Color color, VisualElement container, PongGameMode mode, string hoverIcon, string hoverStatus)
        {
            var btn = new GraphicalUserInterfaceBuilder($"QuadBtn_{mode}")
                .WithFlexGrow(1).WithMargin(5)
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.1f))
                .WithBorderColor(color).WithBorderWidth(2)
                .OnBuild(ve => ve.style.position = Position.Relative) // Ensure children stack correctly
                .Build();

            // 1. THE LIVE BACKGROUND
            // We instantiate the autonomous game and set it to fill the absolute space of the button
            var backgroundGame = new Pong_Autonomous_Background(color).CreateGui(_ctx);
            btn.Add(backgroundGame);

            // 2. THE FOREGROUND CONTENT
            // We put the text in an overlay container so it floats above the live game
            var contentOverlay = new GraphicalUserInterfaceBuilder("ContentOverlay")
                .WithFlexGrow(1)
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)
                .OnBuild(ve => {
                    ve.pickingMode = PickingMode.Ignore; // Clicks go to the parent button
                    ve.style.position = Position.Absolute;
                    ve.style.top = 0; ve.style.bottom = 0;
                    ve.style.left = 0; ve.style.right = 0;
                })
                .Build();

            // Text needs a slight shadow or background plate to ensure readability over the moving ball
            var textPlate = new GraphicalUserInterfaceBuilder("TextPlate")
                .WithBackgroundColor(new Color(0, 0, 0, 0.6f))
                .WithPadding(10).WithBorderRadius(5)
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)
                .OnBuild(ve => ve.pickingMode = PickingMode.Ignore)
                .Build();

            textPlate.Add(new ForgeLabelBuilder(text).WithColor(Color.white).WithFontStyle(FontStyle.Bold).WithFontSize(18).Build());
            textPlate.Add(new ForgeLabelBuilder("LAUNCH ALPHA").WithColor(color).WithFontSize(12).WithMarginTop(5).Build());

            contentOverlay.Add(textPlate);
            btn.Add(contentOverlay);

            // 3. INTERACTIONS
            btn.RegisterCallback<PointerEnterEvent>(e => {
                btn.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f);
                _opponentLmpbVisual.text = hoverIcon;
                _opponentLmpbVisual.style.color = color;
                _opponentLmpbTitle.text = hoverStatus;
                _opponentLmpbTitle.style.color = color;
            });

            btn.RegisterCallback<PointerLeaveEvent>(e => {
                btn.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f);
            });

            btn.RegisterCallback<PointerDownEvent>(e => {
                container.Clear();
                container.Add(new Pong_Playable_Alpha(mode).CreateGui(_ctx));
            });

            return btn;
        }

        // ==============================================================================
        // TECHNICAL DEEP DIVE: ARCHITECTURE & X-RAY LAUNCHER
        // ==============================================================================
        private void ShowDeepDive(VisualElement container)
        {
            container.Clear();

            var deepDiveBuilder = new GraphicalUserInterfaceBuilder("DeepDiveRoot")
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f))
                .WithPadding(30)
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch);

            // Header & Back Button
            var header = new GraphicalUserInterfaceBuilder("HeaderRow")
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                .WithBorderBottomColor(Color.cyan).WithBorderBottomWidth(2).WithMarginBottom(20)
                .Build();

            header.Add(new ForgeLabelBuilder("SYSTEM ARCHITECTURE: PONG").WithFontSize(24).WithColor(Color.cyan).WithFontStyle(FontStyle.Bold).Build());
            header.Add(new ForgeButtonBuilder("BACK TO MENU").WithBackgroundColor(new Color(0.4f, 0.1f, 0.1f)).OnClick(() => ShowMainMenu(container)).CreateGui(_ctx));
            deepDiveBuilder.AddChild(header);

            // Documentation Content
            var contentRow = new GraphicalUserInterfaceBuilder("ContentRow")
                .WithFlexGrow(1)
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Stretch)
                .Build();

            // Tech Explanation Column
            var explanationCol = new GraphicalUserInterfaceBuilder("Docs")
                .WithFlexGrow(2).WithMarginRight(30)
                .Build();

            explanationCol.Add(new ForgeLabelBuilder("HOW IT WAS BUILT").WithFontSize(18).WithColor(Color.white).WithMarginBottom(10).Build());
            explanationCol.Add(new ForgeLabelBuilder(
                "• GUI Builders: The entire interface, including the game arena, paddles, and ball, are built using our pure C# GraphicalUserInterfaceBuilder. No prefabs or GameObjects are used.\n\n" +
                "• Context & State: The game relies on the injected GuiContext to maintain its boundaries, while internal game loops are bound directly to the UI Toolkit's schedule.Execute() method, bypassing standard Unity Update cycles.\n\n" +
                "• Finite State Machines: Mode selection (Hotseat vs AI) fundamentally alters the state machine driving the right paddle. The Neural Net mode processes coordinates through a local lightweight matrix, whereas the Hermit AI relies on standard linear interpolation.\n\n" +
                "• LMPB Integration: The Live Model Preview Boxes act as localized visual states, awaiting future implementation of live RenderTextures for 3D avatar representation."
            ).WithColor(Color.gray).WithWhiteSpace(WhiteSpace.Normal).Build());

            contentRow.Add(explanationCol);

            // X-Ray Launch Column
            var xrayCol = new GraphicalUserInterfaceBuilder("XRayLaunch")
                .WithFlexGrow(1).WithBackgroundColor(new Color(0.05f, 0.05f, 0.05f))
                .WithBorderColor(Color.cyan).WithBorderWidth(1).WithPadding(20)
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)
                .Build();

            xrayCol.Add(new ForgeLabelBuilder("X-RAY PROFILER").WithColor(Color.cyan).WithFontSize(18).WithFontStyle(FontStyle.Bold).WithMarginBottom(10).Build());
            xrayCol.Add(new ForgeLabelBuilder("Launch the game with the Memory Inspector physically updated in real-time. The ReflectiveGuiBuilder will traverse the private memory footprint of the live game instance.")
                .WithColor(Color.gray).WithWhiteSpace(WhiteSpace.Normal).WithAlignment(TextAnchor.UpperCenter).WithMarginBottom(20).Build());

            xrayCol.Add(new ForgeButtonBuilder("PLAY WITH X-RAY ENABLED")
                .WithBackgroundColor(new Color(0.2f, 0.6f, 0.2f))
                .WithTextColor(Color.white).WithHeight(50).WithWidth(200).WithFontStyle(FontStyle.Bold)
                .OnClick(() => ShowXRayView(container))
                .CreateGui(_ctx));

            contentRow.Add(xrayCol);
            deepDiveBuilder.AddChild(contentRow);
            container.Add(deepDiveBuilder.Build());
        }

        // ==============================================================================
        // X-RAY VIEW: SPLIT PANEL REFLECTION
        // ==============================================================================
        private void ShowXRayView(VisualElement container)
        {
            container.Clear();

            var root = new GraphicalUserInterfaceBuilder("XRayRoot")
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f))
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                .Build();

            // Header
            var header = new GraphicalUserInterfaceBuilder("Header")
                .WithPadding(10).WithBackgroundColor(new Color(0.05f, 0.05f, 0.06f))
                .WithBorderBottomColor(Color.cyan).WithBorderBottomWidth(2)
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                .Build();

            header.Add(new ForgeLabelBuilder("LIVE MEMORY INSPECTION").WithFontSize(20).WithColor(Color.cyan).WithFontStyle(FontStyle.Bold).Build());
            header.Add(new ForgeButtonBuilder("RETURN TO DEEP DIVE").WithBackgroundColor(new Color(0.6f, 0.2f, 0.2f)).OnClick(() => ShowDeepDive(container)).CreateGui(_ctx));
            root.Add(header);

            // Split View
            var contentSplit = new GraphicalUserInterfaceBuilder("Split")
                .WithFlexGrow(1).WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                .Build();

            // Left: Live Game
            var simulatedGame = new Pong_Playable_Alpha(PongGameMode.AiHermit); // Defaulting X-Ray to AI to show it off
            var gamePanel = new GraphicalUserInterfaceBuilder("GamePanel")
                .WithFlexGrow(1).WithBorderRightColor(Color.black).WithBorderRightWidth(2)
                .Build();
            gamePanel.Add(simulatedGame.CreateGui(_ctx));
            contentSplit.Add(gamePanel);

            // Right: Reflection Inspector
            var reflectionPanel = new GraphicalUserInterfaceBuilder("ReflectionPanel")
                .WithFlexGrow(1).WithPadding(20).WithBackgroundColor(new Color(0.06f, 0.06f, 0.08f))
                .Build();

            reflectionPanel.Add(new ForgeLabelBuilder("LIVE OBJECT FOOTPRINT").WithFontSize(18).WithColor(Color.white).WithMarginBottom(10).Build());

            var scrollView = new ScrollView(ScrollViewMode.Vertical);
            scrollView.style.flexGrow = 1;

            try
            {
                // Hooking into your ReflectiveGuiBuilder from the Empire example
                var inspectorUi = new ReflectiveGuiBuilder<Pong_Playable_Alpha>(simulatedGame).CreateGui(_ctx);
                scrollView.Add(inspectorUi);
            }
            catch (Exception ex)
            {
                scrollView.Add(new Label($"Error Reflecting Target: {ex.Message}") { style = { color = Color.red } });
            }

            reflectionPanel.Add(scrollView);
            contentSplit.Add(reflectionPanel);

            root.Add(contentSplit);
            container.Add(root);
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}