using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.IO;

namespace Assets.Scripts.MastersOfOrionII
{
    /// <summary>
    /// The Command Terminal for the Orion Alpha.
    /// Acts as the primary UI entry point for the experience lifecycle.
    /// Reforged to kill stubs and utilize the Forge Builder suite.
    /// </summary>
    public class MastersOfOrionII_Playable_Alpha : IGuiProvider
    {
        public string Title => "ORION PROJECT: ALPHA TERMINAL";

        private MastersOfOrionII_Game _gameInstance;

        public VisualElement CreateGui(GuiContext ctx)
        {
            // Resolve the master orchestrator
            _gameInstance = UnityEngine.Object.FindAnyObjectByType<MastersOfOrionII_Game>();

            var rootBuilder = new ForgeContainerBuilder("OrionAlpha_Root")
                .WithFlexGrow(1f)
                .WithBackgroundColor(new Color(0.02f, 0.02f, 0.05f)) // Deep Space Blue
                .WithAlignItems(Align.Center)
                .WithJustifyContent(Justify.Center);

            // --- CENTRAL COMMAND PANEL ---
            rootBuilder.AddChild(new DynamicGuiProvider(c =>
            {
                var panel = new ForgeContainerBuilder("CommandPanel")
                    .WithWidth(500f)
                    .WithPadding(40f)
                    .WithBackgroundColor(new Color(0.05f, 0.08f, 0.15f, 0.8f))
                    .WithBorderWidth(2f)
                    .WithBorderColor(Color.cyan)
                    .WithBorderRadius(10f);

                // Header
                panel.AddChild(new ForgeLabelBuilder("MASTERS OF ORION II")
                    .WithFontSize(32)
                    .WithBold()
                    .WithColor(Color.cyan)
                    .WithTextAlign(TextAnchor.MiddleCenter)
                    .WithMarginBottom(10f));

                panel.AddChild(new ForgeLabelBuilder("QUANTUM SIMULATION // ALPHA BUILD")
                    .WithFontSize(12)
                    .WithColor(new Color(0.5f, 0.8f, 1f))
                    .WithTextAlign(TextAnchor.MiddleCenter)
                    .WithMarginBottom(30f));

                // Navigation Options
                panel.AddChild(CreateMenuButton("🚀 START NEW EXPEDITION", () => {
                    _gameInstance?.StartNewGameFlow(); // Transitions FSM to GameSetup
                }));

                panel.AddChild(CreateMenuButton("📑 LOGISTICS REGISTRY", () => {
                    _gameInstance?.SwitchGui("Logistics");
                }));

                panel.AddChild(CreateMenuButton("🧪 TECHNOLOGY TREE", () => {
                    _gameInstance?.SwitchGui("Science");
                }));

                panel.AddChild(CreateMenuButton("⚙️ SYSTEM CONFIG", () => {
                    _gameInstance?.SwitchGui("Settings");
                }));

                // Footer Telemetry
                panel.AddChild(new ForgeLabelBuilder("SINGULARITY DATA BUS: ONLINE")
                    .WithFontSize(10)
                    .WithColor(Color.gray)
                    .WithMarginTop(30f)
                    .WithTextAlign(TextAnchor.MiddleCenter));

                return panel.Build();
            }));

            return rootBuilder.Build();
        }

        private IGuiProvider CreateMenuButton(string text, Action onClick)
        {
            return new ForgeButtonBuilder(text)
                .WithHeight(50f)
                .WithMarginBottom(10f)
                .WithBackgroundColor(new Color(0.1f, 0.15f, 0.25f))
                .WithBold()
                .OnClick(onClick);
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));

        public void ToUIDocument(string assetPath)
        {
            var snapshot = CreateGui(new GuiContext());
            WorkshopUxmlBaker.Bake(snapshot, "OrionAlpha_MainMenu");
        }

        public void FromUIDocument(string assetPath)
        {
            Debug.LogWarning("[OrionAlpha] Manual UXML import not supported for dynamic terminal.");
        }
    }
}