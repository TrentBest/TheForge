using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.Physics;

namespace Workshop.UI_And_Tools.Physics.Editors
{
    public class PhysicsDashboardProvider : IGuiProvider
    {
        public string Title => "Universal Constants & Physics Engine";

        private readonly Color _panelBg = new Color(0.10f, 0.10f, 0.14f);
        private readonly Color _accentColor = new Color(0.2f, 0.8f, 0.95f); // Quantum Cyan
        private readonly Color _textDim = new Color(0.6f, 0.6f, 0.7f);

        // The active mathematical state of this specific universe
        private UniversalConstants _currentUniverse = new UniversalConstants();

        // Used to trigger the Atom Preview in the Periodic Table
        public Action<float> OnAtomicScaleChanged;

        public VisualElement CreateGui(GuiContext ctx)
        {
            return new ForgeSplitPanelBuilder(sidebarWidth: 450, Side.Left)
                .WithSidebar(BuildConstantsEditor(ctx))
                .WithMain(BuildEducationalDataView(ctx))
                .CreateGui(ctx);
        }

        private IGuiProvider BuildConstantsEditor(GuiContext ctx)
        {
            return new GraphicalUserInterfaceBuilder("ConstantsEditor")
                .WithPadding(20)
                .WithBackgroundColor(_panelBg)
                .WithScrollable(true)

                .AddHeader("THE FABRIC OF REALITY")
                .AddChild(new Label("Alter the mathematical foundation of this experience.") { style = { color = _textDim, marginBottom = 20 } })

                // --- PLANCK CONSTANT ---
                .AddSeparator(_accentColor, 1)
                .AddChild(new Label("Planck Constant (h)") { style = { color = Color.white, unityFontStyleAndWeight = FontStyle.Bold } })
                .AddSliderData("Quantum Resolution (x 10^-34)", 0.1f, 20.0f, _currentUniverse.PlanckConstant, v => {
                    _currentUniverse.PlanckConstant = v;
                    BroadcastAtomicUpdate();
                })

                // --- ELEMENTARY CHARGE ---
                .AddSeparator(_accentColor, 1)
                .AddChild(new Label("Elementary Charge (e)") { style = { color = Color.white, unityFontStyleAndWeight = FontStyle.Bold } })
                .AddSliderData("Electromagnetic Binding (x 10^-19)", 0.1f, 10.0f, _currentUniverse.ElementaryCharge, v => {
                    _currentUniverse.ElementaryCharge = v;
                    BroadcastAtomicUpdate();
                })

                // --- GRAVITATIONAL CONSTANT ---
                .AddSeparator(_accentColor, 1)
                .AddChild(new Label("Gravitational Constant (G)") { style = { color = Color.white, unityFontStyleAndWeight = FontStyle.Bold } })
                .AddSliderData("Spacetime Curvature (x 10^-11)", 0.1f, 50.0f, _currentUniverse.GravitationalConstant, v => {
                    _currentUniverse.GravitationalConstant = v;
                })

                // --- SPEED OF LIGHT ---
                .AddSeparator(_accentColor, 1)
                .AddChild(new Label("Speed of Light (c)") { style = { color = Color.white, unityFontStyleAndWeight = FontStyle.Bold } })
                .AddSliderData("Causality Limit (m/s)", 1000f, 1000000000f, _currentUniverse.SpeedOfLight, v => {
                    _currentUniverse.SpeedOfLight = v;
                })

                .AddChild(new Button(() => ResetToEarth())
                {
                    text = "REVERT TO EARTH STANDARD",
                    style = { marginTop = 30, backgroundColor = new Color(0.2f, 0.2f, 0.2f), color = Color.white }
                });
        }

        private IGuiProvider BuildEducationalDataView(GuiContext ctx)
        {
            return new GraphicalUserInterfaceBuilder("DataReadout")
                .WithPadding(30)

                .AddHeader("EDUCATIONAL TELEMETRY")
                .AddSeparator(Color.gray, 1)

                // Educational Tiering: Simple to Complex
                .AddChild(new Label("Understanding: Planck Constant (h)") { style = { color = _accentColor, fontSize = 16, marginTop = 10, unityFontStyleAndWeight = FontStyle.Bold } })

                .AddChild(new Label("[ SIMPLE ]") { style = { color = Color.white, marginTop = 10 } })
                .AddChild(new Label("This number defines the 'pixel size' of the universe. It is the height of the staircase that energy has to climb. If you make it bigger, atoms become physically larger and quantum teleportation could happen at human scales.") { style = { color = _textDim, whiteSpace = WhiteSpace.Normal } })

                .AddChild(new Label("[ MEDIUM ]") { style = { color = Color.white, marginTop = 10 } })
                .AddChild(new Label("Planck's constant relates the energy of a photon to its frequency (E = hf). Increasing this value means low-frequency waves (like radio) would carry immense, destructive energy.") { style = { color = _textDim, whiteSpace = WhiteSpace.Normal } })

                .AddChild(new Label("[ COMPLEX ]") { style = { color = Color.white, marginTop = 10 } })
                .AddChild(new Label("In the Bohr model, radius r = (n^2 * h^2 * e_0) / (pi * m_e * e^2). Notice that radius scales with the SQUARE of the Planck constant. Doubling 'h' makes every atom in the simulation four times larger.") { style = { color = _textDim, whiteSpace = WhiteSpace.Normal } });
        }

        private void BroadcastAtomicUpdate()
        {
            // Calculate the new math and fire the event for the Atom Preview
            float newScale = _currentUniverse.GetAtomicScaleMultiplier();
            OnAtomicScaleChanged?.Invoke(newScale);
        }

        private void ResetToEarth()
        {
            _currentUniverse = new UniversalConstants();
            BroadcastAtomicUpdate();
            // A trigger to refresh the UI sliders would go here
        }

        public void ToUIDocument(string id) { }
        public void FromUIDocument(string document) { }
        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
    }
}