using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.UI_And_Tools.Forge.GuiBuilders
{
    /// <summary>
    /// Forge Builder for the Drone Hangar.
    /// Registers the tool into the Singularity Hub ecosystem.
    /// </summary>
    public class Workshop_Gui_DroneHangar : MonoBehaviour, IForgeBuilder
    {
        public string ToolName => "Drone Swarm Commander";
        public string Title => ToolName;
        public Type GetProductType() => typeof(DroneHangarGuiProvider);

        public IGuiProvider GetGuiProvider() => new DroneHangarGuiProvider();

        private void OnEnable() => BuilderRegistry.Register(this);

        /// <summary>
        /// Returns the logic controller for the Swarm Matrix.
        /// </summary>
        public object Build() => GetGuiProvider();
    }

    /// <summary>
    /// The concrete GUI Provider refactored for the Forge Builder Ecosystem.
    /// Manages the Swarm Matrix and Environment generation via REST.
    /// </summary>
    public class DroneHangarGuiProvider : IGuiProvider
    {
        public string Title => "Drone Swarm Commander";

        // --- State: The Swarm & Environment Context ---
        private List<DroneDef> _availableDrones = new List<DroneDef>();
        private Dictionary<DroneDef, int> _activeSwarm = new Dictionary<DroneDef, int>();
        private string _targetRegion = "47.1352° N, 37.5536° E (Sample Frontline)";
        private bool _terrainDataFetched = false;

        // --- Forge Builder References (Dynamic Rebuild Targets) ---
        private VisualElement _statusLabel;
        private VisualElement _catalogContainer;
        private VisualElement _weaveContainer;
        private VisualElement _statsLabel;
        private VisualElement _deploymentButton;
        private GuiContext _activeCtx;

        public DroneHangarGuiProvider()
        {
            PopulateScrapedData();
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _activeCtx = ctx;

            var root = new ForgeContainerBuilder("DroneHangar_Root")
                .WithPadding(15)
                .WithBackgroundColor(new Color(0.12f, 0.12f, 0.15f, 1.0f))
                .WithFlexGrow(1);

            // 1. Header Section
            root.AddChild(new ForgeLabelBuilder("🛸 Swarm Control Weave Matrix")
                .WithFontSize(22).WithBold().WithColor(Color.white).WithMarginBottom(5));

            _statusLabel = new ForgeLabelBuilder("System Status: Idle | Awaiting Swarm Configuration.")
                .WithColor(new Color(0.6f, 0.9f, 0.6f)).WithMarginBottom(15).Build();
            root.AddChild(_statusLabel);

            // 2. Environment Section (The REST Integration)
            var envPanel = new ForgeContainerBuilder("Env_Integration")
                .WithPadding(10).WithMarginBottom(15).WithBorderRadius(5)
                .WithBackgroundColor(new Color(0.18f, 0.22f, 0.25f))
                .AddChild(new ForgeLabelBuilder("🌍 Topography & Target Integration").WithBold().WithMarginBottom(5))
                .AddChild(new ForgeTextFieldBuilder("Target Region:", _targetRegion)
                    .OnChanged(val => _targetRegion = val))
                .AddChild(new ForgeButtonBuilder("📡 Connect via REST & Generate Topography", FetchEnvironmentDataViaRest)
                    .WithBackgroundColor(new Color(0.2f, 0.4f, 0.6f)).WithHeight(25).WithMarginTop(5));
            root.AddChild(envPanel);

            // 3. The Matrix Split Panel
            var matrixSplit = new ForgeContainerBuilder("MatrixSplit").WithDirection(FlexDirection.Row).WithFlexGrow(1);

            // Left: Catalog
            var catalogPanel = new ForgeContainerBuilder("CatalogPanel").WithWidth(new StyleLength(Length.Percent(45))).WithMarginRight(10)
                .AddChild(new ForgeLabelBuilder("Platform Catalog").WithBold().WithMarginBottom(5));

            // LINT FIX: Replaced raw ScrollView
            _catalogContainer = new ForgeScrollViewBuilder("CatalogContainer")
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0, 0, 0, 0.3f))
                .CreateGui(ctx);
            catalogPanel.AddChild(_catalogContainer);

            // Right: Active Swarm
            var weavePanel = new ForgeContainerBuilder("WeavePanel").WithWidth(new StyleLength(Length.Percent(55))).WithMarginLeft(10)
                .AddChild(new ForgeLabelBuilder("Active Control Weave").WithBold().WithMarginBottom(5));

            // LINT FIX: Replaced raw ScrollView
            _weaveContainer = new ForgeScrollViewBuilder("WeaveContainer")
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0, 0, 0, 0.3f))
                .CreateGui(ctx);
            weavePanel.AddChild(_weaveContainer);

            matrixSplit.AddChild(catalogPanel).AddChild(weavePanel);
            root.AddChild(matrixSplit);

            // 4. Deployment Footer
            var footer = new ForgeContainerBuilder("HangarFooter").WithMarginTop(15).WithPaddingTop(10).WithBorderWidth(1).WithBorderColor(Color.gray);

            _statsLabel = new ForgeLabelBuilder("Total Units: 0 | Composition: Balanced").WithColor(Color.white).WithMarginBottom(10).Build();
            footer.AddChild(_statsLabel); // Fixed an issue here where _statusLabel was added instead of _statsLabel

            _deploymentButton = new ForgeButtonBuilder("🔥 INITIALIZE GAMIFIED ENVIRONMENT 🔥", ExecuteSwarmTest)
                .WithBackgroundColor(new Color(0.6f, 0.2f, 0.2f)).WithBold().WithHeight(40).WithOpacity(0.5f).Build();
            footer.AddChild(_deploymentButton);

            root.AddChild(footer);

            RefreshUI();
            return root.CreateGui(ctx);
        }

        private void RefreshUI()
        {
            if (_catalogContainer == null || _weaveContainer == null) return;

            _catalogContainer.Clear();
            _weaveContainer.Clear();

            // Render Platform Catalog via Forge Rows
            foreach (var drone in _availableDrones)
            {
                var row = new ForgeContainerBuilder($"Row_{drone.Name}")
                    .WithPadding(5).WithMarginBottom(8).WithBorderRadius(4)
                    .WithBackgroundColor(new Color(0.2f, 0.2f, 0.25f))
                    .AddChild(new ForgeLabelBuilder($"<b>{drone.Name}</b>\n<size=10>{drone.ClassType}</size>").WithWordWrap());

                var controls = new ForgeContainerBuilder("AddActions").WithDirection(FlexDirection.Row).WithMarginTop(5)
                    .AddChild(new ForgeButtonBuilder("+1", () => AdjustCount(drone, 1)).WithFlexGrow(1))
                    .AddChild(new ForgeButtonBuilder("+10", () => AdjustCount(drone, 10)).WithFlexGrow(1));

                row.AddChild(controls);
                _catalogContainer.Add(row.CreateGui(_activeCtx));
            }

            // Render Active Weave with Stats Tracking
            int totalISR = 0, totalKinetic = 0, totalStrike = 0;
            foreach (var kvp in _activeSwarm.ToList())
            {
                var drone = kvp.Key;
                var count = kvp.Value;

                if (drone.ClassType.Contains("ISR")) totalISR += count;
                else if (drone.ClassType.Contains("Kinetic")) totalKinetic += count;
                else totalStrike += count;

                var row = new ForgeContainerBuilder($"Active_{drone.Name}")
                    .WithDirection(FlexDirection.Row).WithAlignItems(Align.Center)
                    .WithPadding(5).WithMarginBottom(5).WithBorderRadius(4)
                    .WithBackgroundColor(new Color(0.25f, 0.2f, 0.3f))
                    .AddChild(new ForgeLabelBuilder(drone.Name).WithWidth(new StyleLength(Length.Percent(40))))
                    .AddChild(new ForgeButtonBuilder("-10", () => AdjustCount(drone, -10)))
                    .AddChild(new ForgeButtonBuilder("-", () => AdjustCount(drone, -1)))
                    .AddChild(new ForgeLabelBuilder(count.ToString()).WithBold().WithTextAlign(TextAnchor.MiddleCenter).WithWidth(35))
                    .AddChild(new ForgeButtonBuilder("+", () => AdjustCount(drone, 1)))
                    .AddChild(new ForgeButtonBuilder("+10", () => AdjustCount(drone, 10)));

                _weaveContainer.Add(row.CreateGui(_activeCtx));
            }

            // Update Dynamic Telemetry
            if (_statsLabel is Label stats) stats.text = $"Total Units: {_activeSwarm.Values.Sum()} | Composition: {totalISR} ISR, {totalKinetic} Kinetic, {totalStrike} Strike";

            if (_deploymentButton is Button deploy)
            {
                bool ready = _terrainDataFetched && _activeSwarm.Count > 0;
                deploy.style.opacity = ready ? 1.0f : 0.5f;
                deploy.style.backgroundColor = ready ? new Color(0.8f, 0.3f, 0.3f) : new Color(0.6f, 0.2f, 0.2f);
            }
        }

        private void AdjustCount(DroneDef drone, int amount)
        {
            if (!_activeSwarm.ContainsKey(drone)) { if (amount > 0) _activeSwarm[drone] = amount; }
            else { _activeSwarm[drone] += amount; if (_activeSwarm[drone] <= 0) _activeSwarm.Remove(drone); }
            RefreshUI();
        }

        private void FetchEnvironmentDataViaRest()
        {
            if (_statusLabel is Label l) { l.text = "System Status: Contacting REST API for topography..."; l.style.color = Color.yellow; }
            _activeCtx?.OnBuilt?.Invoke(null); // Simulate a system-wide refresh signal

            // Fake latency for the topography generator
            _weaveContainer.schedule.Execute(() => {
                _terrainDataFetched = true;
                if (_statusLabel is Label l2) { l2.text = "System Status: Terrain Data Acquired."; l2.style.color = new Color(0.4f, 0.8f, 1.0f); }
                RefreshUI();
            }).StartingIn(1000);
        }

        private void ExecuteSwarmTest()
        {
            if (!_terrainDataFetched) return;
            Debug.Log($"[Swarm Commander] Handoff to Stage Builder for region: {_targetRegion}");
        }

        private void PopulateScrapedData()
        {
            _availableDrones.Add(new DroneDef("Vesper Quad", "ISR / Spotter", "48x Thermal Optics"));
            _availableDrones.Add(new DroneDef("The Thwomp", "Kinetic Striker", "Tungsten Pogo-Ball"));
            _availableDrones.Add(new DroneDef("Switchblade 300", "Loitering Munition", "Frag Warhead"));
        }

        public Action<VisualElement> GetGuiBuilder() => container => container.Add(CreateGui(new GuiContext()));

#if UNITY_EDITOR
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
#endif

        public void FromUIDocument(string assetPath) { }
    }

    public struct DroneDef
    {
        public string Name;
        public string ClassType;
        public string Payload;
        public DroneDef(string name, string classType, string payload) { Name = name; ClassType = classType; Payload = payload; }
    }
}