using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.Armada2525
{
    /// <summary>
    /// The Bootloader serves as the synthesis phase for the Armada 2525 Digital Archive.
    /// It bridges the gap between historical metadata and the functional simulation.
    /// </summary>
    public class Armada_Gui_Bootloader : IGuiProvider
    {
        public string Title => "RECONSTRUCTION: SYNTHESIS PHASE";

        private readonly IGuiRouter _router;
        private readonly ArmadaGalaxyContext _context;
        private Label _telemetryLabel;

        public Armada_Gui_Bootloader(IGuiRouter router, ArmadaGalaxyContext context)
        {
            _router = router ?? throw new ArgumentNullException(nameof(router));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            // 1. Root Synthesis Container
            var root = new GraphicalUserInterfaceBuilder("ArmadaSynthesisRoot")
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.02f, 0.02f, 0.03f)) // Deep Void Black
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)

                // 🏛️ Encyclopedia Identity
                .AddChild(new Label("ENCYCLOPEDIA GALACTICA | ARCHIVE RECONSTRUCTION")
                {
                    style = { color = Color.gray, fontSize = 10, letterSpacing = 4, marginBottom = 20 }
                })

                // 🌀 Synthesis Title
                .AddChild(new ForgeLabelBuilder("SYNTHESIZING SECTOR ALPHA")
                    .WithColor(new Color(0.2f, 0.6f, 0.9f))
                    .WithFontSize(32)
                    .WithFontStyle(FontStyle.Bold)
                    .Build())

                // 🛰️ Deterministic Telemetry
                .OnBuild(ve =>
                {
                    _telemetryLabel = new Label($"SEED: {_context.UniversalSeed} | ALGORITHM: SQUIRREL-3")
                    {
                        style = { color = new Color(0.4f, 0.4f, 0.5f), fontSize = 12, marginTop = 10 }
                    };
                    ve.Add(_telemetryLabel);

                    // Add a thematic "scanning" line
                    var scanner = new VisualElement { style = { height = 1, width = 400, backgroundColor = new Color(0.2f, 0.6f, 0.9f, 0.5f), marginTop = 40 } };
                    ve.Add(scanner);
                })
                .Build();

            // 2. Trigger Sovereign Logic
            // We manually invoke the Playable Alpha to start the background thread.
            var alphaLogic = new Armada2525_Playable_Alpha();
            alphaLogic.OnEnter(_context);

            // 3. Monitor Generation Ticks
            // The UI strictly observes the Context until the synthesis is verified.
            root.schedule.Execute(() =>
            {
                if (_context.IsGalaxyGenerated)
                {
                    Debug.Log("[Armada Bootloader] Synthesis complete. Projecting tactical grid.");
                    _router.NavigateTo("TacticalMap");
                }
                else
                {
                    // Animate telemetry to show the user the "Digital Knowledge Base" is working
                    _telemetryLabel.text = $"SYNTHESIZING... {UnityEngine.Random.Range(1000, 9999).ToString("X")}::DATA_STREAM";
                }
            }).Every(100);

            return root;
        }

        // --- IGuiProvider Requirements ---

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));

        public void ToUIDocument(string assetPath)
        {
            Debug.Log($"[Archive] Baking Synthesis Terminal to: {assetPath}");
        }

        public void FromUIDocument(string assetPath)
        {
            Debug.Log($"[Archive] Restoring Terminal State from: {assetPath}");
        }
    }
}