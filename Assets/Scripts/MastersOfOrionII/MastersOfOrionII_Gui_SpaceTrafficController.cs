using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.IO;

namespace Assets.Scripts.MastersOfOrionII
{
    /// <summary>
    /// Orbital Traffic Control: Managing the flow of commerce and defense.
    /// Reforged to follow the Forge Protocol and the Singularity Experience Model.
    /// </summary>
    public class MastersOfOrionII_Gui_SpaceTrafficController : IGuiProvider
    {
        public string Title => "ORBITAL TRAFFIC CONTROL";

        private MastersOfOrionII_Game _game;
        private int _starportLevel = 1;
        private int _safeLandings = 0;
        private int _maxCapacity = 5;

        // Captured for dynamic updates
        private VisualElement _statsContainer;

        public VisualElement CreateGui(GuiContext ctx)
        {
            _game = Object.FindAnyObjectByType<MastersOfOrionII_Game>();

            var rootBuilder = new ForgeContainerBuilder("TrafficController_Root")
                .WithFlexGrow(1f)
                .WithDirection(FlexDirection.Row)
                .WithBackgroundColor(new Color(0.01f, 0.04f, 0.01f)); // Deep Radar Green

            // --- LEFT SIDEBAR: HUD & STATS (20%) ---
            var sidebar = new ForgeContainerBuilder("Sidebar")
                .WithWidth(new StyleLength(Length.Percent(20f)))
                .WithBackgroundColor(new Color(0.05f, 0.1f, 0.05f))
                .WithBorderWidth(0, 2f, 0, 0)
                .WithBorderColor(Color.green)
                .WithPadding(15f);

            sidebar.AddChild(new ForgeButtonBuilder("<< ABORT SIMULATION")
                .WithBackgroundColor(Color.clear)
                .WithColor(Color.gray)
                .WithMarginBottom(20f)
                .OnClick(() => _game?.SwitchGui("ColonyView")));

            sidebar.AddChild(new ForgeLabelBuilder("STARPORT RADAR")
                .WithFontSize(20)
                .WithBold()
                .WithColor(Color.green)
                .WithMarginBottom(20f));

            // STATS BLOCK - Wrapped in DynamicGuiProvider for clean re-renders
            sidebar.AddChild(new DynamicGuiProvider(c => {
                _statsContainer = new ForgeContainerBuilder("StatsBox")
                    .WithBackgroundColor(new Color(0, 0, 0, 0.5f))
                    .WithPadding(10f)
                    .WithMarginBottom(20f)
                    .WithBorderWidth(0, 0, 0, 2f)
                    .WithBorderColor(Color.cyan)
                    .AddChild(new ForgeLabelBuilder($"PORT LEVEL: {_starportLevel}").WithBold())
                    .AddChild(new ForgeLabelBuilder($"CAPACITY: {_maxCapacity} Ships").WithFontSize(10).WithColor(Color.gray))
                    .AddChild(new ForgeLabelBuilder($"SAFE DOCKS: {_safeLandings}").WithFontSize(16).WithColor(Color.cyan).WithMarginTop(10f))
                    .Build();
                return _statsContainer;
            }));

            sidebar.AddChild(new ForgeButtonBuilder("UPGRADE PORT (10,000cr)")
                .WithBackgroundColor(new Color(0.1f, 0.4f, 0.1f))
                .WithHeight(40f)
                .WithBold()
                .OnClick(() => {
                    _starportLevel++;
                    _maxCapacity += 5;
                    RefreshStats(); // Local re-render
                }));

            // Incoming Queue
            sidebar.AddChild(new ForgeLabelBuilder("INCOMING VECTOR QUEUE")
                .WithFontSize(10).WithColor(Color.gray).WithMarginTop(20f).WithMarginBottom(10f));

            sidebar.AddChild(new ForgeLabelBuilder("• Heavy Freighter (ETA 12s)").WithColor(Color.yellow));
            sidebar.AddChild(new ForgeLabelBuilder("• Civilian Shuttle (ETA 18s)").WithColor(Color.white));
            sidebar.AddChild(new ForgeLabelBuilder("• Military Cruiser (ETA 25s)").WithColor(Color.red));

            // --- RIGHT AREA: THE RADAR SCREEN (80%) ---
            var radarArea = new ForgeContainerBuilder("RadarArea")
                .WithFlexGrow(1f)
                .WithAlignItems(Align.Center)
                .WithJustifyContent(Justify.Center)
                .AddChild(new ForgeLabelBuilder("[ REAL-TIME 3D RADAR VIEW ]")
                    .WithFontSize(24).WithBold().WithColor(new Color(0, 1, 0, 0.3f)))
                .AddChild(new ForgeLabelBuilder("Pathfind vector logic active. Drag from ship icons to docking bays.")
                    .WithColor(Color.gray).WithMarginTop(10f));

            rootBuilder.AddChild(sidebar);
            rootBuilder.AddChild(radarArea);

            return rootBuilder.Build();
        }

        private void RefreshStats()
        {
            if (_statsContainer == null) return;
            var parent = _statsContainer.parent;
            int index = parent.IndexOf(_statsContainer);
            parent.Remove(_statsContainer);

            // Re-build just the stats block
            _statsContainer = new ForgeContainerBuilder("StatsBox")
                .WithBackgroundColor(new Color(0, 0, 0, 0.5f))
                .WithPadding(10f)
                .WithMarginBottom(20f)
                .WithBorderWidth(0, 0, 0, 2f)
                .WithBorderColor(Color.cyan)
                .AddChild(new ForgeLabelBuilder($"PORT LEVEL: {_starportLevel}").WithBold())
                .AddChild(new ForgeLabelBuilder($"CAPACITY: {_maxCapacity} Ships").WithFontSize(10).WithColor(Color.gray))
                .AddChild(new ForgeLabelBuilder($"SAFE DOCKS: {_safeLandings}").WithFontSize(16).WithColor(Color.cyan).WithMarginTop(10f))
                .Build();

            parent.Insert(index, _statsContainer);
        }

        public System.Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));

        public void ToUIDocument(string assetPath) =>
            WorkshopUxmlBaker.Bake(CreateGui(new GuiContext()), "SpaceTrafficControl_Snapshot");

        public void FromUIDocument(string assetPath) { }
    }
}