using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Showcase;
using TheSingularityWorkshop.FSM_API;

namespace Assets.Scripts.Fractals
{
    public class Fractals_ShowcaseProvider : IShowcasePortal
    {
        public string Title => "THE ARCHIVE OF INFINITY";
        public bool IsDiscoverable => true;
        public string Category => "PART II: FOUNDATIONAL SYSTEMS";
        public string Overview => "A comprehensive, GPU-accelerated encyclopedia of complex dynamics and escape-time fractals. Explore the mathematical boundaries of infinity through real-time Compute Shaders.";
        public Color AccentColor => Color.cyan;
        public List<string> CoreTechnologies => new List<string> { "Compute Shaders", "HLSL", "Atomic FSM", "Data-Driven UI", "Dynamic Telemetry" };
        public string TelemetryState => "AWAITING MATHEMATICAL DOMAIN...";
        public bool IsUnderConstruction => false;

        private IGuiRouter _router;
        private FractalForgeContext _sharedContext;
        private FSMHandle _diveFsmHandle;

        private ScrollView _loreScrollView;
        private Label _telemetryReadout;

        private static Vector2 _diveTargetPan;
        private static string _diveTargetName = "IDLE";
        private static bool _isDiving = false;

        private readonly string DIVE_MACHINE_NAME = "FractalDiveFSM";
        private readonly string POOL_NAME = "FractalShowcasePool";

        // --- THE ENCYCLOPEDIA REPOSITORY DELEGATE ---
        private class FractalArchiveEntry
        {
            public string Title;
            public Func<FractalForgeContext, IGuiProvider> ProviderFactory; // THE DELEGATION LINK
            public string DiveName;
            public Vector2 DiveTarget;
        }

        private List<FractalArchiveEntry> _archiveDatabase;

        public Fractals_ShowcaseProvider()
        {
            InitializeDatabase();
        }

        public void InjectRouter(IGuiRouter router)
        {
            _router = router;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _sharedContext = new FractalForgeContext();

            var navContainer = new ForgeContainerBuilder("ArchiveNav")
                .WithDirection(FlexDirection.Row).WithFlexWrap(Wrap.Wrap).WithPadding(10)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f))
                .WithBorderBottomWidth(2).WithBorderBottomColor(Color.cyan);

            foreach (var entry in _archiveDatabase)
            {
                navContainer.AddChild(new ForgeButtonBuilder(entry.Title.ToUpper())
                    .WithBackgroundColor(new Color(0.15f, 0.15f, 0.2f)).WithTextColor(Color.white).WithMargin(2)
                    .WithOnClick(() => LoadArchiveEntry(entry)));
            }

            _loreScrollView = new ScrollView { style = { flexGrow = 1, paddingRight = 10 } };

            var lorePanel = new ForgeContainerBuilder("LorePanel")
                .WithDirection(FlexDirection.Column).WithPadding(20).WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.06f))
                .OnBuild(ve => ve.Add(_loreScrollView))
                .Build();

            var telemetryPanel = new ForgeContainerBuilder("TelemetryHUD")
                .WithBackgroundColor(new Color(0.0f, 0.1f, 0.0f, 0.15f))
                .WithBorderTopColor(Color.green).WithBorderTopWidth(1).WithPadding(15)
                .OnBuild(ve =>
                {
                    ve.Add(new Label("LIVE GPU TELEMETRY") { style = { color = Color.green, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 5 } });
                    _telemetryReadout = new Label("BOOTING SENSORS...") { style = { color = new Color(0.7f, 1.0f, 0.7f), whiteSpace = WhiteSpace.Normal } };
                    ve.Add(_telemetryReadout);
                })
                .Build();

            var leftColumn = new ForgeContainerBuilder("LeftColumn")
                .WithDirection(FlexDirection.Column).WithFlexGrow(1)
                .WithBorderRightWidth(2).WithBorderRightColor(Color.cyan)
                .AddChild(navContainer)
                .OnBuild(ve => ve.Add(lorePanel))
                .OnBuild(ve => ve.Add(telemetryPanel));

            var splitPanel = new ForgeSplitPanelBuilder(500, Side.Left)
                .WithSidebar(leftColumn)
                .WithMain(new FractalEditorGuiBuilder(_sharedContext));

            var root = new GraphicalUserInterfaceBuilder("FractalsShowcaseRoot")
                .WithFlexGrow(1).WithBackgroundColor(new Color(0.02f, 0.02f, 0.02f))
                .AddChild(splitPanel)
                .CreateGui(ctx);

            root.RegisterCallback<AttachToPanelEvent>(evt =>
            {
                LoadArchiveEntry(_archiveDatabase[0]);

                root.schedule.Execute(() =>
                {
                    try { FSM_API.Interaction.Update(POOL_NAME); }
                    catch (Exception ex) { Debug.LogError($"[FractalDiveTickError] {ex.Message}"); }

                    if (_telemetryReadout != null && _sharedContext != null)
                    {
                        _telemetryReadout.text = $"TARGET: {_diveTargetName}\n" +
                                                 $"ZOOM: {_sharedContext.ViewContext.ZoomLevel:N2}x\n" +
                                                 $"COORDS: X:{_sharedContext.ViewContext.PanOffset.x:F6} Y:{_sharedContext.ViewContext.PanOffset.y:F6}\n" +
                                                 $"ITERATIONS: {_sharedContext.MaxIterations}\n" +
                                                 $"STATUS: {(_isDiving ? "AUTOPILOT ENGAGED" : "MANUAL INTERACTIVE")}";
                    }
                }).Every(16);
            });

            root.RegisterCallback<DetachFromPanelEvent>(evt =>
            {
                StopDive();
                if (_diveFsmHandle != null) FSM_API.Interaction.DestroyInstance(_diveFsmHandle);
            });

            return root;
        }

        private void LoadArchiveEntry(FractalArchiveEntry entry)
        {
            StopDive();

            // Standard reset for view coordinates
            _sharedContext.ViewContext.ZoomLevel = 1.0f;
            _sharedContext.ViewContext.PanOffset = Vector2.zero;
            _sharedContext.MaxIterations = 200;

            _loreScrollView.Clear();

            // INSTANTIATE THE DEDICATED GUI PROVIDER
            var specializedProvider = entry.ProviderFactory(_sharedContext);
            _loreScrollView.Add(specializedProvider.CreateGui(new GuiContext()));

            // Append the Contextual Dive Controls below the custom provider
            _loreScrollView.Add(new ForgeLabelBuilder("AUTOPILOT SEQUENCE")
                .WithColor(Color.cyan).WithBold().WithMarginTop(20).WithMarginBottom(5).CreateGui(new GuiContext()));

            var diveBtn = new ForgeButtonBuilder($"EXECUTE TELEMETRY: {entry.DiveName.ToUpper()}")
                .WithBackgroundColor(new Color(0.1f, 0.4f, 0.1f)).WithTextColor(Color.white).WithMarginBottom(5).WithBold()
                .WithOnClick(() => InitiateDive(entry.DiveTarget, entry.DiveName))
                .CreateGui(new GuiContext());

            var abortBtn = new ForgeButtonBuilder("ABORT AUTOPILOT")
                .WithBackgroundColor(new Color(0.4f, 0.1f, 0.1f)).WithTextColor(Color.white).WithBold()
                .WithOnClick(StopDive)
                .CreateGui(new GuiContext());

            _loreScrollView.Add(diveBtn);
            _loreScrollView.Add(abortBtn);
        }

        private void InitiateDive(Vector2 target, string targetName)
        {
            _diveTargetPan = target;
            _diveTargetName = targetName;
            _isDiving = true;

            if (!FSM_API.Interaction.Exists(DIVE_MACHINE_NAME))
            {
                FSM_API.Create.CreateFiniteStateMachine(DIVE_MACHINE_NAME, -1, POOL_NAME)
                    .State("Idle", null, null, null)
                    .State("Diving", null, OnDiveTick, null)
                    .WithInitialState("Idle")
                    .BuildDefinition();
            }

            if (_diveFsmHandle == null) _diveFsmHandle = FSM_API.Create.CreateInstance(DIVE_MACHINE_NAME, _sharedContext, POOL_NAME);

            _diveFsmHandle.TransitionTo("Diving");
        }

        private void StopDive()
        {
            _isDiving = false;
            _diveTargetName = "IDLE";
            if (_diveFsmHandle != null) _diveFsmHandle.TransitionTo("Idle");
        }

        public static void OnDiveTick(object context)
        {
            if (!(context is FractalForgeContext ctx)) return;

            ctx.ViewContext.PanOffset = Vector2.Lerp(ctx.ViewContext.PanOffset, _diveTargetPan, 0.02f);
            ctx.ViewContext.ZoomLevel *= 1.03f;

            if (ctx.ViewContext.ZoomLevel > 300f && ctx.MaxIterations < 1000) ctx.MaxIterations += 3;

            ctx.ViewContext.IsDirty = true;

            if (ctx.ViewContext.ZoomLevel > 150000f)
            {
                _isDiving = false;
                _diveTargetName = "TERMINAL FLOATING-POINT DEPTH REACHED";
                ctx.Status.TransitionTo("Idle");
            }
        }

        private void InitializeDatabase()
        {
            _archiveDatabase = new List<FractalArchiveEntry>
            {
                new FractalArchiveEntry
{
    Title = "Iterated Systems",
    DiveName = "Sierpinski Void", DiveTarget = new Vector2(0, 0),
    ProviderFactory = ctx => new IFSLoreProvider(ctx)
},
new FractalArchiveEntry
{
    Title = "L-Systems",
    DiveName = "Morphogenesis", DiveTarget = new Vector2(0, 0),
    ProviderFactory = ctx => new LSystemLoreProvider(ctx)
},
new FractalArchiveEntry
{
    Title = "Random Terrain",
    DiveName = "Brownian Descent", DiveTarget = new Vector2(0, 0),
    ProviderFactory = ctx => new RandomFractalLoreProvider(ctx)
},
                new FractalArchiveEntry
                {
                    Title = "Mandelbrot",
                    DiveName = "Seahorse Valley", DiveTarget = new Vector2(-0.74364388f, 0.1318259f),
                    ProviderFactory = ctx => new MandelbrotLoreProvider(ctx)
                },
                new FractalArchiveEntry
                {
                    Title = "Julia Sets",
                    DiveName = "The Singularity Eye", DiveTarget = new Vector2(-0.1f, 0.1f),
                    ProviderFactory = ctx => new JuliaLoreProvider(ctx)
                },
                new FractalArchiveEntry
                {
                    Title = "Burning Ship",
                    DiveName = "The Armada", DiveTarget = new Vector2(-1.76f, -0.04f),
                    ProviderFactory = ctx => new BurningShipLoreProvider(ctx)
                },
                new FractalArchiveEntry
                {
                    Title = "Tricorn",
                    DiveName = "The Swallowtail Split", DiveTarget = new Vector2(-0.1f, 0.85f),
                    ProviderFactory = ctx => new TricornLoreProvider(ctx)
                },
                new FractalArchiveEntry
                {
                    Title = "Multibrot",
                    DiveName = "The Dual Node", DiveTarget = new Vector2(0.0f, 0.65f),
                    ProviderFactory = ctx => new MultibrotLoreProvider(ctx)
                }
            };
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}