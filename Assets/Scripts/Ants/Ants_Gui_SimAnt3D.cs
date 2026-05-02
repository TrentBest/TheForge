using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.Ants
{
    public class Ants_Gui_SimAnt3D : IGuiProvider
    {
        public string Title => "SimAnt 3D: Elastic Compute";
        private readonly IGuiRouter _router;

        // Voxel Grid State
        private int _gridSizeX = 256;
        private int _gridSizeY = 64;
        private int _gridSizeZ = 256;
        private int _activeAnts = 0;
        private bool _isHeatmapActive = false;

        public Ants_Gui_SimAnt3D(IGuiRouter router)
        {
            _router = router;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            return new ForgeSplitPanelBuilder(sidebarWidth: 320, Side.Left)
                .WithSidebar(BuildCommandCenter(ctx))
                .WithMain(BuildViewport(ctx))
                .CreateGui(ctx);
        }

        private IGuiProvider BuildCommandCenter(GuiContext ctx)
        {
            return new GraphicalUserInterfaceBuilder("SimAntCommands")
                .WithPadding(20)
                .WithBackgroundColor(new Color(0.10f, 0.10f, 0.12f))

                .AddHeader("MACRO-ENVIRONMENT")
                .AddSeparator(new Color(1f, 0.4f, 0f), 2)

                // --- TELEMETRY ---
                .AddChild(new Label("ELASTIC COMPUTE TELEMETRY") { style = { color = new Color(0.6f, 0.6f, 0.7f), fontSize = 11, letterSpacing = 1, marginTop = 10, marginBottom = 10, unityFontStyleAndWeight = FontStyle.Bold } })

                .AddChild(new Label($"Voxel Grid: {_gridSizeX}x{_gridSizeY}x{_gridSizeZ}") { style = { color = Color.white, marginBottom = 5 } })
                .AddChild(new Label($"Total Volume: {(_gridSizeX * _gridSizeY * _gridSizeZ):N0} Voxels") { style = { color = Color.gray, marginBottom = 15 } })

                .AddChild(new Label("Active Agents:") { style = { color = Color.white } })
                .AddChild(new Label($"{_activeAnts:N0} / 1,000,000") { style = { color = new Color(1f, 0.4f, 0f), fontSize = 18, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 20 } })

                .AddSeparator(new Color(0.3f, 0.3f, 0.3f), 1)

                // --- ARBITRATION CONTROLS ---
                .AddChild(new Label("PACKAGE INJECTION") { style = { color = new Color(0.6f, 0.6f, 0.7f), fontSize = 11, letterSpacing = 1, marginTop = 10, marginBottom = 10, unityFontStyleAndWeight = FontStyle.Bold } })

                .AddChild(new ForgeButtonBuilder("INITIALIZE HIVE MIND", () => InjectHiveMind())
                    .WithBackgroundColor(new Color(0.2f, 0.6f, 0.2f))
                    .WithTextColor(Color.white)
                    .WithHeight(40)
                    .WithFontStyle(FontStyle.Bold)
                    .Build())

                .AddChild(new ForgeButtonBuilder("PURGE UNMANAGED MEMORY", () => PurgeMemory())
                    .WithBackgroundColor(new Color(0.8f, 0.2f, 0.2f))
                    .WithTextColor(Color.white)
                    .WithHeight(30)
                    .WithMargin(10, 0, 0, 0)
                    .Build())

                .AddSeparator(new Color(0.3f, 0.3f, 0.3f), 1)

                // --- SENSORY OVERLAYS ---
                .AddChild(new Label("SENSORY OVERLAYS") { style = { color = new Color(0.6f, 0.6f, 0.7f), fontSize = 11, letterSpacing = 1, marginTop = 10, marginBottom = 10, unityFontStyleAndWeight = FontStyle.Bold } })

                .AddChild(new ForgeButtonBuilder("TOGGLE PHEROMONE HEATMAP", () => ToggleHeatmap(ctx))
                    .WithBackgroundColor(new Color(0.2f, 0.2f, 0.3f))
                    .WithTextColor(new Color(0.8f, 0.4f, 1f))
                    .WithHeight(40)
                    .Build())

                // --- ESCAPE HATCH ---
                .AddChild(new ForgeButtonBuilder("<- RETURN TO HUB", () => _router?.NavigateTo("Interactive"))
                    .WithBackgroundColor(Color.clear)
                    .WithTextColor(Color.gray)
                    .WithMargin(40, 0, 0, 0)
                    .Build());
        }

        private IGuiProvider BuildViewport(GuiContext ctx)
        {
            // For now, this is where the SingularityRenderPipeline will draw the 3D unmanaged scene.
            // We use a dark, empty container that the pipeline can composite over or render into.
            return new GraphicalUserInterfaceBuilder("Viewport")
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.02f, 0.02f, 0.03f))
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)

                // We'll replace this label with a LiveModelPreviewBuilder or a direct Texture injection later!
                .AddChild(new ForgeLabelBuilder("[ 3D ELASTIC PIPELINE VIEWPORT ]")
                    .WithColor(new Color(0.3f, 0.3f, 0.3f))
                    .WithFontSize(20)
                    .WithFontStyle(FontStyle.Bold)
                );
        }

        // --- ARBITRATION ACTIONS ---

        private void InjectHiveMind()
        {
            Debug.Log("[Myrmecology] Allocating 3D Voxel Grid and 1,000,000 FSMAgentData structs...");
            _activeAnts = 1000000;
            // TODO: Hook into DataWarehouse and boot ColonyComputePackage
        }

        private void PurgeMemory()
        {
            Debug.Log("[Myrmecology] Purging Voxel Grid from VRAM.");
            _activeAnts = 0;
            // TODO: Release buffers
        }

        private void ToggleHeatmap(GuiContext ctx)
        {
            _isHeatmapActive = !_isHeatmapActive;
            Debug.Log($"[Myrmecology] Pheromone Heatmap: {(_isHeatmapActive ? "ONLINE" : "OFFLINE")}");
            // TODO: Dispatch global shader variable (_RenderMode = 1)
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}