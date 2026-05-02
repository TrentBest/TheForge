using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.Asteroids
{
    public class Asteroids_Gui_MainMenu : IGuiProvider
    {
        public string Title => "ASTEROIDS: MAIN COMMAND";

        private VisualElement _centerAnchor;
        private GuiContext _lastCtx;
        private IGuiRouter _router;

        public Asteroids_Gui_MainMenu() { }

        public Asteroids_Gui_MainMenu(IGuiRouter router)
        {
            _router = router;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            var builder = new GraphicalUserInterfaceBuilder("Asteroids_MainMenu_Root")
                .WithPercentSize(100, 100)
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                .WithBackgroundColor(new Color(0.02f, 0.02f, 0.05f)); // Deep Space Black/Blue

            builder.AddChild(context =>
            {
                var grid = new GraphicalUserInterfaceBuilder("GridContainer")
                    .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                    .OnBuild(ve => ve.style.flexGrow = 1)
                    .Build();

                var topRow = new GraphicalUserInterfaceBuilder("TopRow")
                    .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                    .OnBuild(ve => ve.style.flexGrow = 1)
                    .Build();

                // 1. SOLO MISSION - Injecting the Asteroid Field Simulator as the Background
                topRow.Add(CreateQuadrant("SOLO", "COMMENCE DEPLOYMENT", new Color(0.1f, 0.05f, 0.2f),
                    () => _router?.NavigateTo("HangarBay"), ctx, new Asteroids_Gui_AsteroidFieldSim()));

                // 2. MULTIPLAYER
                topRow.Add(CreateQuadrant("WINGS", "CO-OP & VERSUS ENCOUNTERS", new Color(0.05f, 0.15f, 0.15f),
                    () => Debug.Log("Multiplayer Not Available in Alpha"), ctx));

                grid.Add(topRow);

                var bottomRow = new GraphicalUserInterfaceBuilder("BottomRow")
                    .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                    .OnBuild(ve => ve.style.flexGrow = 1)
                    .Build();

                // 3. SETTINGS
                bottomRow.Add(CreateQuadrant("SYSTEMS", "HARDWARE CONFIGURATION", new Color(0.1f, 0.1f, 0.1f),
                    () => Debug.Log("Opening Settings..."), ctx));

                // 4. QUIT
                bottomRow.Add(CreateQuadrant("ABORT", "TERMINATE CONNECTION", new Color(0.2f, 0.05f, 0.05f),
                    () => Application.Quit(), ctx));

                grid.Add(bottomRow);
                return grid;
            });

            // --- CENTER PILOT ID ANCHOR ---
            builder.AddChild(context =>
            {
                _centerAnchor = new GraphicalUserInterfaceBuilder("CenterAnchor")
                    .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)
                    .OnBuild(ve => {
                        ve.style.position = Position.Absolute;
                        ve.style.left = Length.Percent(50);
                        ve.style.top = Length.Percent(50);
                        ve.style.width = 0; ve.style.height = 0;
                        ve.pickingMode = PickingMode.Ignore;
                    }).Build();

                var centerSignInBtn = new ForgeButtonBuilder("", () => Debug.Log("Sign In Requested"))
                    .WithBackgroundColor(new Color(0.05f, 0.05f, 0.1f, 1f))
                    .CreateGui(ctx);

                centerSignInBtn.style.width = 140; centerSignInBtn.style.height = 140;
                centerSignInBtn.style.borderBottomLeftRadius = 70; centerSignInBtn.style.borderTopLeftRadius = 70;
                centerSignInBtn.style.borderBottomRightRadius = 70; centerSignInBtn.style.borderTopRightRadius = 70;
                centerSignInBtn.style.borderTopWidth = 3;
                centerSignInBtn.style.borderBottomWidth = 3;
                centerSignInBtn.style.borderLeftWidth = 3;
                centerSignInBtn.style.borderRightWidth = 3;
                centerSignInBtn.style.borderTopColor = new Color(0.4f, 0.8f, 1f); // Neon Blue Border
                centerSignInBtn.style.borderBottomColor = new Color(0.4f, 0.8f, 1f); // Neon Blue Border
                centerSignInBtn.style.borderLeftColor = new Color(0.4f, 0.8f, 1f); // Neon Blue Border
                centerSignInBtn.style.borderRightColor = new Color(0.4f, 0.8f, 1f); // Neon Blue Border
                centerSignInBtn.style.justifyContent = Justify.Center; centerSignInBtn.style.alignItems = Align.Center;

                var logoLbl = new ForgeLabelBuilder("A").WithColor(new Color(0.4f, 0.8f, 1f)).WithFontSize(56).WithFontStyle(FontStyle.Bold).CreateGui(ctx);
                var signLbl = new ForgeLabelBuilder("IDENTIFY PILOT").WithColor(Color.gray).WithFontSize(10).WithFontStyle(FontStyle.Bold).CreateGui(ctx);

                centerSignInBtn.Add(logoLbl);
                centerSignInBtn.Add(signLbl);

                centerSignInBtn.RegisterCallback<MouseEnterEvent>(e => centerSignInBtn.style.scale = new Scale(Vector3.one * 1.1f));
                centerSignInBtn.RegisterCallback<MouseLeaveEvent>(e => centerSignInBtn.style.scale = new Scale(Vector3.one));

                _centerAnchor.Add(centerSignInBtn);
                return _centerAnchor;
            });

            return builder.Build();
        }

        private VisualElement CreateQuadrant(string title, string subtitle, Color baseColor, System.Action onClick, GuiContext ctx, IGuiProvider bgProvider = null)
        {
            var container = new GraphicalUserInterfaceBuilder($"Quadrant_{title}")
                .WithBackgroundColor(baseColor)
                .OnBuild(ve => {
                    ve.style.flexGrow = 1;
                    ve.style.width = Length.Percent(50);
                    ve.style.overflow = Overflow.Hidden;
                }).Build();

            // Render the Live Background if provided
            VisualElement bgLayer = null;
            if (bgProvider != null)
            {
                bgLayer = bgProvider.CreateGui(ctx);
                bgLayer.style.position = Position.Absolute;
                bgLayer.style.left = 0; bgLayer.style.top = 0; bgLayer.style.right = 0; bgLayer.style.bottom = 0;
                bgLayer.style.opacity = 0.35f; // Dimmed until hovered
                container.Add(bgLayer);
            }

            var btn = new ForgeButtonBuilder("", onClick)
                .WithBackgroundColor(Color.clear)
                .CreateGui(ctx);

            btn.style.flexGrow = 1;
            btn.style.borderTopWidth = 0;
            btn.style.borderBottomWidth = 0;
            btn.style.borderLeftWidth = 0;
            btn.style.borderRightWidth = 0;
            btn.style.justifyContent = Justify.Center; btn.style.alignItems = Align.Center;

            var titleLbl = new ForgeLabelBuilder(title).WithFontSize(42).WithColor(new Color(1, 1, 1, 0.3f)).WithFontStyle(FontStyle.Bold).CreateGui(ctx);
            titleLbl.style.letterSpacing = 12;

            var subLbl = new ForgeLabelBuilder(subtitle).WithFontSize(12).WithColor(new Color(0.4f, 0.8f, 1f)).CreateGui(ctx);
            subLbl.style.opacity = 0;
            subLbl.style.marginTop = 10;
            subLbl.style.translate = new Translate(0, 20, 0);

            btn.Add(titleLbl);
            btn.Add(subLbl);

            btn.RegisterCallback<MouseEnterEvent>(evt => {
                container.style.backgroundColor = Lighten(baseColor, 0.1f);
                titleLbl.style.color = Color.white;
                subLbl.style.opacity = 1; subLbl.style.translate = new Translate(0, 0, 0);

                // Illuminate the live asteroid field simulation
                if (bgLayer != null) bgLayer.style.opacity = 0.85f;
            });
            btn.RegisterCallback<MouseLeaveEvent>(evt => {
                container.style.backgroundColor = baseColor;
                titleLbl.style.color = new Color(1, 1, 1, 0.3f);
                subLbl.style.opacity = 0; subLbl.style.translate = new Translate(0, 20, 0);

                // Dim the asteroid field simulation back down
                if (bgLayer != null) bgLayer.style.opacity = 0.35f;
            });

            container.Add(btn);
            return container;
        }

        private Color Lighten(Color c, float amount) => new Color(Mathf.Clamp01(c.r + amount), Mathf.Clamp01(c.g + amount), Mathf.Clamp01(c.b + amount));

        public System.Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
#if UNITY_EDITOR
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
#endif
        public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
    }
}