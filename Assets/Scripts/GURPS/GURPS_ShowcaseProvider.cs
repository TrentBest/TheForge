using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Showcase;

namespace Workshop.GURPS
{
    public class GURPS_ShowcaseProvider : IShowcasePortal
    {
        public string Title => "GURPS Digital Engine";
        public bool IsDiscoverable => true;
        public string Category => "PART VI: SYSTEM TOOLS";
        public string Overview => "The mathematical 'Physics' engine for the Singularity. Arbitrates success, combat, and tech progression.";
        public Color AccentColor => Color.magenta;
        public List<string> CoreTechnologies => new List<string> { "Rule Engine", "Probability Graphs", "API-First Design" };
        public string TelemetryState => "CALCULATING SUCCESS PROBABILITIES";

        private IGuiRouter _router;
        private GuiContext _context;
        public bool IsUnderConstruction => true;

        public void InjectRouter(IGuiRouter router) => _router = router;

        public VisualElement CreateGui(GuiContext ctx)
        {
            _context = ctx;

            // ROOT: Deep 'Void' background with strict padding
            var root = new ForgeContainerBuilder("GURPS_ShowcaseRoot")
                .WithPadding(30)
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.02f, 0.01f, 0.03f, 1.0f)); // The Void

            // HEADER: High-contrast typography with Cyan/Magenta telemetry data
            var headerContainer = new ForgeContainerBuilder("Header_Telemetry")
                .WithDirection(FlexDirection.Column)
                .WithMarginBottom(25)
                .WithBorderBottomWidth(2)
                .WithBorderColor(AccentColor)
                .WithPaddingBottom(15)
                .AddChild(new ForgeLabelBuilder("SYS.OP // DURPS")
                    .WithFontSize(10)
                    .WithColor(Color.cyan)
                    .WithBold()
                    .WithMarginBottom(5))
                .AddChild(new ForgeLabelBuilder("SYSTEMIC RULESET ENGINE")
                    .WithFontSize(32)
                    .WithColor(Color.white)
                    .WithBold()
                    .OnBuild(ve => ve.style.letterSpacing = 3))
                .AddChild(new ForgeLabelBuilder($"STATUS: {TelemetryState}...")
                    .WithFontSize(11)
                    .WithColor(new Color(0.7f, 0.7f, 0.7f))
                    .WithMarginTop(5));

            root.AddChild(headerContainer);

            // GRID: The Module Matrix
            var grid = new ForgeContainerBuilder("Module_Matrix")
                .WithDirection(FlexDirection.Row)
                .WithAlignItems(Align.FlexStart)
                .WithJustifyContent(Justify.FlexStart)
                .OnBuild(ve => ve.style.flexWrap = Wrap.Wrap);

            // Sub-Editors
            grid.AddChild(CreateModuleCard("Character Builder", "Architect heroes, NPCs, and biological templates with precise trait/point distribution.", () => Navigate(new GURPS_Gui_CharacterSheet())));
            grid.AddChild(CreateModuleCard("Universe & Tech", "Manage TL (Technology Levels), mana densities, and core physics parameters.", () => Navigate(new GURPS_UniverseEditor())));
            grid.AddChild(CreateModuleCard("Armory Database", "Configure ballistics, melee, and ultra-tech armaments with strict stat validation.", () => Navigate(new GURPS_WeaponEditor())));
            grid.AddChild(CreateModuleCard("Success Grapher", "Visualize probability distributions and standard deviation curves for 3d6 checks.", () => Navigate(new ProbabilityGraphBuilder())));

            root.AddChild(grid);

            // FOOTER: Playable Alpha Launch Sequence
            var footer = new ForgeContainerBuilder("Launch_Sequence")
                .WithMarginTop(30)
                .WithFlexGrow(1)
                .WithJustifyContent(Justify.FlexEnd)
                .AddChild(new ForgeButtonBuilder(">> INITIALIZE COMBAT SANDBOX (ALPHA) <<", () => Navigate(new GURPS_CombatAlpha_Provider()))
                    .WithHeight(50)
                    .WithBold()
                    .WithFontSize(14)
                    .WithBackgroundColor(new Color(0.15f, 0.0f, 0.15f, 0.8f))
                    .WithBorderColor(AccentColor)
                    .WithBorderWidth(1)
                    .WithTextColor(Color.white)
                    .OnBuild(ve => {
                        ve.style.letterSpacing = 2;
                        // Button Hover Pulse
                        ve.RegisterCallback<MouseEnterEvent>(e => {
                            ve.style.backgroundColor = AccentColor;
                            ve.style.color = Color.black;
                            ve.style.borderTopColor = Color.cyan;
                        });
                        ve.RegisterCallback<MouseLeaveEvent>(e => {
                            ve.style.backgroundColor = new Color(0.15f, 0.0f, 0.15f, 0.8f);
                            ve.style.color = Color.white;
                            ve.style.borderTopColor = AccentColor;
                        });
                    }));

            root.AddChild(footer);

            return root.CreateGui(ctx);
        }

        private void Navigate(IGuiProvider provider) => _router?.NavigateTo(provider.Title);

        /// <summary>
        /// Generates an interactive, tactical UI card with dynamic hover states.
        /// </summary>
        private IGuiProvider CreateModuleCard(string title, string desc, Action onClick)
        {
            return new DynamicGuiProvider(ctx => {
                var card = new ForgeContainerBuilder($"Card_{title}")
                    .WithWidth(240)
                    .WithHeight(110)
                    .WithMargin(10)
                    .WithPadding(15)
                    .WithBackgroundColor(new Color(0.08f, 0.05f, 0.1f)) // Deep void purple
                    .WithBorderWidth(1)
                    .WithBorderColor(new Color(0.3f, 0.1f, 0.3f))
                    .OnBuild(ve => {
                        // Tactical aesthetic cut-corners
                        ve.style.borderTopLeftRadius = 8;
                        ve.style.borderBottomRightRadius = 8;

                        // Hover Interaction Events
                        ve.RegisterCallback<MouseEnterEvent>(e => {
                            ve.style.backgroundColor = new Color(0.2f, 0.05f, 0.25f);
                            ve.style.borderTopColor = Color.cyan;
                            ve.style.borderBottomColor = Color.cyan;
                            ve.style.borderLeftColor = AccentColor;
                            ve.style.borderRightColor = AccentColor;
                        });
                        ve.RegisterCallback<MouseLeaveEvent>(e => {
                            ve.style.backgroundColor = new Color(0.08f, 0.05f, 0.1f);
                            ve.style.borderTopColor = new Color(0.3f, 0.1f, 0.3f);
                            ve.style.borderBottomColor = new Color(0.3f, 0.1f, 0.3f);
                            ve.style.borderLeftColor = new Color(0.3f, 0.1f, 0.3f);
                            ve.style.borderRightColor = new Color(0.3f, 0.1f, 0.3f);
                        });
                    })
                    .AddChild(new ForgeLabelBuilder(title)
                        .WithBold()
                        .WithColor(AccentColor)
                        .WithFontSize(14)
                        .WithMarginBottom(8))
                    .AddChild(new ForgeLabelBuilder(desc)
                        .WithFontSize(10)
                        .WithColor(new Color(0.7f, 0.7f, 0.75f))
                        .WithWordWrap())
                    .CreateGui(ctx);

                card.AddManipulator(new Clickable(onClick));
                return card;
            });
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));

#if UNITY_EDITOR
        public void ToUIDocument(string assetPath)
        {
            var root = CreateGui(_context ?? new GuiContext());
            GraphicalUserInterfaceBuilder.ConvertToUIDocument(root, assetPath);
        }
#else
        public void ToUIDocument(string assetPath) { }
#endif

        public void FromUIDocument(string assetPath) { }
    }
}