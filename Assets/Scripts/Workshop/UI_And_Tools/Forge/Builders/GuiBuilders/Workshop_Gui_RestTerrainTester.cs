using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.UI_And_Tools.Forge.GuiBuilders
{
    /// <summary>
    /// Forge Builder for the REST Terrain Tester.
    /// Registers the standalone tool into the Singularity Hub ecosystem.
    /// </summary>
    public class Workshop_Gui_RestTerrainTester : MonoBehaviour, IForgeBuilder
    {
        public string ToolName => "REST Terrain Ingestor";
        public string Title => ToolName;
        public Type GetProductType() => typeof(RestTerrainTesterProvider);

        public IGuiProvider GetGuiProvider() => new RestTerrainTesterProvider();

        private void OnEnable() => BuilderRegistry.Register(this);

        /// <summary>
        /// Returns the logic provider for the Terrain Ingestor.
        /// </summary>
        public object Build() => GetGuiProvider();
    }

    /// <summary>
    /// The standalone GUI Provider refactored for the Forge Builder Ecosystem.
    /// Manages REST endpoint consumption and Unity Terrain grid generation.
    /// </summary>
    public class RestTerrainTesterProvider : IGuiProvider
    {
        public string Title => "REST Terrain Ingestor";

        // --- UI References & State ---
        private VisualElement _rootContainer;
        private VisualElement _statusLabel;
        private VisualElement _fetchButton;
        private ScrollView _logView;
        private GuiContext _activeCtx;

        // Configuration
        private string _endpointUrl = "https://api.mapbox.com/v4/mapbox.terrain-rgb/<bbox>";
        private float _unityTileSize = 1000f;
        private int _heightmapResolution = 513;

        public VisualElement CreateGui(GuiContext ctx)
        {
            _activeCtx = ctx;

            // ROOT: Forge Container utilizing the Granular Padding Protocol
            var root = new ForgeContainerBuilder("RestTopography_Root")
                .WithPadding(15)
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.12f, 0.15f, 0.12f, 1.0f));

            // 1. Header Section
            root.AddChild(new ForgeLabelBuilder("📡 REST Topography Testbed")
                .WithFontSize(24).WithBold().WithColor(Color.white).WithMarginBottom(10));

            _statusLabel = new ForgeLabelBuilder("Status: Idle | Awaiting API execution.")
                .WithColor(new Color(0.6f, 0.9f, 0.6f)).WithMarginBottom(15).Build();
            root.AddChild(_statusLabel);

            // 2. Configuration Panel (Endpoint & Spatial Calibration)
            var config = new ForgeContainerBuilder("SpatialCalibration")
                .WithPadding(10).WithMarginBottom(15).WithBorderRadius(5)
                .WithBackgroundColor(new Color(0.18f, 0.22f, 0.25f))
                .AddChild(new ForgeLabelBuilder("Spatial Configuration").WithBold().WithMarginBottom(8))
                .AddChild(new ForgeTextFieldBuilder("REST Endpoint:", _endpointUrl)
                    .OnChanged(val => _endpointUrl = val))
                .AddChild(new ForgeTextFieldBuilder("Unity Base Tile Size (m):", _unityTileSize.ToString())
                    .OnChanged(val => { if (float.TryParse(val, out float res)) _unityTileSize = res; }));

            _fetchButton = new ForgeButtonBuilder("GET /terrain/elevation", ExecuteRestIngestion)
                .WithMarginTop(10).WithHeight(30).WithBold()
                .WithBackgroundColor(new Color(0.2f, 0.5f, 0.3f)).WithTextColor(Color.white).Build();

            config.AddChild(_fetchButton);
            root.AddChild(config);

            // 3. Execution Log Section
            root.AddChild(new ForgeLabelBuilder("Ingestion Pipeline Log")
                .WithBold().WithColor(Color.white).WithMarginBottom(5));

            _logView = new ScrollView { style = { flexGrow = 1, backgroundColor = new Color(0, 0, 0, 0.4f) } };
            root.AddChild(_logView);

            _rootContainer = root.CreateGui(ctx);
            return _rootContainer;
        }

        private void ExecuteRestIngestion()
        {
            if (_rootContainer == null) return;

            _fetchButton.SetEnabled(false);
            if (_statusLabel is Label l) { l.text = "Status: Executing HTTP GET..."; l.style.color = Color.yellow; }
            LogMessage($"[REST] Dispatching request to {_endpointUrl}...");

            // Simulated Latency via Editor Schedule
            _rootContainer.schedule.Execute(ProcessRestResponse).StartingIn(1500);
        }

        private void ProcessRestResponse()
        {
            if (_statusLabel is Label l) { l.text = "Status: Data Acquired. Building Grid..."; l.style.color = new Color(0.4f, 0.8f, 1.0f); }

            float apiReturnedWidth = 2700f;
            float apiReturnedLength = 2300f;

            LogMessage($"[REST] 200 OK. Footprint: {apiReturnedWidth}m x {apiReturnedLength}m.");

            int gridColsX = Mathf.CeilToInt(apiReturnedWidth / _unityTileSize);
            int gridRowsZ = Mathf.CeilToInt(apiReturnedLength / _unityTileSize);

            LogMessage($"[Grid Calc] Tile Size: {_unityTileSize}m. Generating {gridColsX}x{gridRowsZ} grid.");

            GenerateTerrainGrid(gridColsX, gridRowsZ);

            if (_statusLabel is Label l2) { l2.text = "Status: Terrain Grid Generated Successfully."; l2.style.color = Color.green; }
            _fetchButton.SetEnabled(true);
        }

        private void GenerateTerrainGrid(int colsX, int rowsZ)
        {
            GameObject gridParent = new GameObject("REST_Terrain_Grid");

            for (int x = 0; x < colsX; x++)
            {
                for (int z = 0; z < rowsZ; z++)
                {
                    TerrainData tData = new TerrainData
                    {
                        heightmapResolution = _heightmapResolution,
                        size = new Vector3(_unityTileSize, 600f, _unityTileSize)
                    };

                    GameObject terrainGO = Terrain.CreateTerrainGameObject(tData);
                    terrainGO.name = $"Tile_{x}_{z}";
                    terrainGO.transform.SetParent(gridParent.transform);
                    terrainGO.transform.position = new Vector3(x * _unityTileSize, 0, z * _unityTileSize);

                    LogMessage($"[Builder] Calibrated {terrainGO.name} at {terrainGO.transform.position}");
                }
            }
        }

        private void LogMessage(string msg)
        {
            var entry = new ForgeLabelBuilder($"> {msg}")
                .WithColor(new Color(0.8f, 0.8f, 0.8f))
                .WithFontSize(11).WithMarginBottom(2).Build();

            _logView.Add(entry);
            _rootContainer.schedule.Execute(() => _logView.scrollOffset = new Vector2(0, _logView.contentContainer.layout.height));
        }

        public Action<VisualElement> GetGuiBuilder() => container => container.Add(CreateGui(new GuiContext()));

        public void ToUIDocument(string assetPath)
        {
            var root = CreateGui(_activeCtx ?? new GuiContext());
            string fileName = string.IsNullOrEmpty(assetPath) ? "REST_Terrain_Ingestor_Bake" : System.IO.Path.GetFileNameWithoutExtension(assetPath);
            WorkshopUxmlBaker.Bake(root, fileName);
        }

        public void FromUIDocument(string assetPath) => Debug.Log($"[REST] Layout sync from {assetPath} requested.");
    }
}