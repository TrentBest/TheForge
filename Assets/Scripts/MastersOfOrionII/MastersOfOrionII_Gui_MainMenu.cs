using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.MastersOfOrionII
{
    public class MastersOfOrionII_Gui_MainMenu : IGuiProvider
    {
        public string Title => "ARMADA MAIN MENU";

        private VisualElement _centerAnchor;
        private GuiContext _lastCtx;

        // The "Dumb View" Action delegates
        private Action _onGenesisClicked;

        // Default constructor for Forge previews
        public MastersOfOrionII_Gui_MainMenu() { }

        // Injected constructor for Mediator routing
        public MastersOfOrionII_Gui_MainMenu(Action onGenesisClicked = null)
        {
            _onGenesisClicked = onGenesisClicked;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            var builder = new GraphicalUserInterfaceBuilder("Armada_MainMenu_Root")
                .WithPercentSize(100, 100)
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                .WithBackgroundColor(new Color(0.02f, 0.02f, 0.03f)); // Void Black

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

                // 1. GENESIS (Top Left - Deep Orange/Brown hue)
                // Ready for an LMPB background provider as the last argument!
                topRow.Add(CreateQuadrant("GENESIS", "INITIALIZE NEW CAMPAIGN", new Color(0.15f, 0.05f, 0.0f),
                    () => _onGenesisClicked?.Invoke(), ctx));

                // 2. PERSISTENCE (Top Right - Deep Blue hue)
                topRow.Add(CreateQuadrant("PERSISTENCE", "LOAD PERSISTENT UNIVERSE", new Color(0.05f, 0.05f, 0.15f),
                    () => Debug.Log("Load not hooked up yet."), ctx));

                grid.Add(topRow);

                var bottomRow = new GraphicalUserInterfaceBuilder("BottomRow")
                    .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                    .OnBuild(ve => ve.style.flexGrow = 1)
                    .Build();

                // 3. COSMOLOGY (Bottom Left - Deep Teal/Cyan hue)
                bottomRow.Add(CreateQuadrant("COSMOLOGY", "GURPS RULESET ARCHIVE", new Color(0.02f, 0.1f, 0.1f),
                    () => Debug.Log("GURPS Archive not hooked up yet."), ctx));

                // 4. TERMINATE (Bottom Right - Dark Gray)
                //bottomRow.Add(CreateQuadrant("TERMINATE", "EXIT TO DESKTOP", new Color(0.1f, 0.1f, 0.1f),
                //    () => Application.Quit(), ctx));

                grid.Add(bottomRow);
                return grid;
            });

            // --- CENTER EMPIRE ANCHOR ---
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

                var centerSignInBtn = new ForgeButtonBuilder("", () => Debug.Log("Empire Authentication Requested"))
                    .WithBackgroundColor(new Color(0.05f, 0.05f, 0.06f, 1f))
                    .CreateGui(ctx);

                Color armadaOrange = new Color(1f, 0.5f, 0f);

                centerSignInBtn.style.width = 140; centerSignInBtn.style.height = 140;
                centerSignInBtn.style.borderBottomLeftRadius = 70; centerSignInBtn.style.borderTopLeftRadius = 70;
                centerSignInBtn.style.borderBottomRightRadius = 70; centerSignInBtn.style.borderTopRightRadius = 70;
                centerSignInBtn.style.borderTopWidth = 3;
                centerSignInBtn.style.borderBottomWidth = 3;
                centerSignInBtn.style.borderLeftWidth = 3;
                centerSignInBtn.style.borderRightWidth = 3;
                centerSignInBtn.style.borderTopColor = armadaOrange;
                centerSignInBtn.style.borderBottomColor = armadaOrange;
                centerSignInBtn.style.borderLeftColor = armadaOrange;
                centerSignInBtn.style.borderRightColor = armadaOrange;
                centerSignInBtn.style.justifyContent = Justify.Center; centerSignInBtn.style.alignItems = Align.Center;

                var logoLbl = new ForgeLabelBuilder("A").WithColor(armadaOrange).WithFontSize(56).WithFontStyle(FontStyle.Bold).CreateGui(ctx);
                var signLbl = new ForgeLabelBuilder("DOMINION").WithColor(Color.gray).WithFontSize(10).WithFontStyle(FontStyle.Bold).CreateGui(ctx);
                signLbl.style.letterSpacing = 2;

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

            // Render the Live Background if provided (Ready for LiveModelPreviewBuilder!)
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

            var subLbl = new ForgeLabelBuilder(subtitle).WithFontSize(12).WithColor(new Color(1f, 0.5f, 0f)).CreateGui(ctx); // Armada Orange
            subLbl.style.opacity = 0;
            subLbl.style.marginTop = 10;
            subLbl.style.translate = new Translate(0, 20, 0);

            btn.Add(titleLbl);
            btn.Add(subLbl);

            btn.RegisterCallback<MouseEnterEvent>(evt => {
                container.style.backgroundColor = Lighten(baseColor, 0.1f);
                titleLbl.style.color = Color.white;
                subLbl.style.opacity = 1; subLbl.style.translate = new Translate(0, 0, 0);

                // Illuminate the live background simulation
                if (bgLayer != null) bgLayer.style.opacity = 0.85f;
            });
            btn.RegisterCallback<MouseLeaveEvent>(evt => {
                container.style.backgroundColor = baseColor;
                titleLbl.style.color = new Color(1, 1, 1, 0.3f);
                subLbl.style.opacity = 0; subLbl.style.translate = new Translate(0, 20, 0);

                // Dim the live background simulation back down
                if (bgLayer != null) bgLayer.style.opacity = 0.35f;
            });

            container.Add(btn);
            return container;
        }

        private Color Lighten(Color c, float amount) => new Color(Mathf.Clamp01(c.r + amount), Mathf.Clamp01(c.g + amount), Mathf.Clamp01(c.b + amount));

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}