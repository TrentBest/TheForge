using System;
using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.MastersOfOrionII.Gui
{
    /// <summary>
    /// HPC Dashboard demonstrating a Two-Tiered LOD Universe Generation pipeline.
    /// Achieves "Blink of an Eye" local rendering, followed by asynchronous Deep Space cluster integration.
    /// </summary>
    public class MastersOfOrionII_Gui_UniversePerformanceBench : IGuiProvider
    {
        public string Title => "HPC PROGRESSIVE UNIVERSE ENGINE";

        private GuiContext _guiContext;
        private VisualElement _rootContainer;
        private VisualElement _controlsContainer;
        private VisualElement _previewContainer;

        // --- 3D ENVIRONMENT ---
        private GameObject _universeRoot;
        private GameObject _celestialDome;
        private Material _domeMaterial;
        private Texture2D _domeTexture;

        // --- UNIVERSE PARAMS ---
        private const int UNIVERSE_SIZE = 100; // The massive 100x100x100 grid
        private int3 _targetCell = new int3(50, 50, 50);
        private int _starsPerCell = 1500;
        private int _deepSpaceRadius = 15; // How many cells out to process (e.g. 15 = 31x31x31 grid = ~30,000 clusters)
        private float _clusterLightThreshold = 0.05f; // Culling threshold for distant light

        private int _textureResolution = 1024;
        private uint _masterSeed = 42;

        // --- ASYNC PIPELINE STATE ---
        private bool _isProcessingDeepSpace = false;
        private Stopwatch _swPipeline;
        private NativeArray<Color32> _persistentPixels;
        private NativeArray<float3> _localStarPositions;
        private NativeArray<float3> _localStarColors;

        // --- TELEMETRY UI ---
        private Label _lblCellData;
        private Label _lblTier1Time;
        private Label _lblTier2Time;
        private Label _lblTotalTime;
        private VisualElement _tier1Indicator;
        private VisualElement _tier2Indicator;

        public VisualElement CreateGui(GuiContext ctx)
        {
            _guiContext = ctx;

            _rootContainer = new GraphicalUserInterfaceBuilder("HpcBenchRoot")
                .WithPercentSize(100, 100).WithFlexGrow(1).WithBackgroundColor(new Color(0.05f, 0.05f, 0.08f)).Build();

            Initialize3DEnvironment();

            var splitPanel = new ForgeSplitPanelBuilder(450, Side.Left)
                .WithSidebar(new DynamicGuiProvider(c => BuildControlPanel()))
                .WithMain(new DynamicGuiProvider(c => BuildPreviewPanel()));

            _rootContainer.Add(splitPanel.CreateGui(ctx));

            // Async Hook for Tier 2 Processing
            _rootContainer.RegisterCallback<AttachToPanelEvent>(e => {
                _rootContainer.schedule.Execute(ProcessDeepSpaceIntegration).Every(16);
            });

            _rootContainer.RegisterCallback<DetachFromPanelEvent>(e => CleanUpMemory());

            return _rootContainer;
        }

        private void Initialize3DEnvironment()
        {
            _universeRoot = new GameObject("[HPC_UNIVERSE_ROOT]");
            _universeRoot.transform.position = new Vector3(0, -10000, 0);

            _celestialDome = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _celestialDome.transform.SetParent(_universeRoot.transform, false);
            _celestialDome.transform.localScale = Vector3.one * 50f;

            UnityEngine.Object.DestroyImmediate(_celestialDome.GetComponent<Collider>());

            // Invert the dome to render on the inside
            MeshFilter filter = _celestialDome.GetComponent<MeshFilter>();
            if (filter != null && filter.sharedMesh != null)
            {
                Mesh invertedMesh = UnityEngine.Object.Instantiate(filter.sharedMesh);
                invertedMesh.name = "Inverted_Sky_Sphere";
                Vector3[] normals = invertedMesh.normals;
                for (int i = 0; i < normals.Length; i++) normals[i] = -normals[i];
                invertedMesh.normals = normals;
                int[] triangles = invertedMesh.triangles;
                Array.Reverse(triangles);
                invertedMesh.triangles = triangles;
                filter.sharedMesh = invertedMesh;
            }

            bool isURP = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null;
            Shader shader = isURP ? Shader.Find("Universal Render Pipeline/Unlit") : Shader.Find("Unlit/Texture");
            _domeMaterial = new Material(shader != null ? shader : Shader.Find("Standard"));
            _celestialDome.GetComponent<Renderer>().sharedMaterial = _domeMaterial;
        }

        private VisualElement BuildControlPanel()
        {
            _controlsContainer = new ScrollView(ScrollViewMode.Vertical) { style = { flexGrow = 1,  backgroundColor = new Color(0.08f, 0.08f, 0.1f) } };

            _controlsContainer.Add(new Label("PROGRESSIVE UNIVERSE ENGINE") { style = { fontSize = 20, unityFontStyleAndWeight = FontStyle.Bold, color = new Color(0.4f, 0.8f, 1f), marginBottom = 20 } });

            // --- NAVIGATION ---
            _controlsContainer.Add(new Label("1. TARGET COORDINATES") { style = { color = Color.gray, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 5 } });

            var jumpBtn = new ForgeButtonBuilder("JUMP TO RANDOM CELL", () => {
                System.Random rnd = new System.Random();
                _targetCell = new int3(rnd.Next(0, UNIVERSE_SIZE), rnd.Next(0, UNIVERSE_SIZE), rnd.Next(0, UNIVERSE_SIZE));
                _lblCellData.text = $"CURRENT CELL: [{_targetCell.x}, {_targetCell.y}, {_targetCell.z}]";
            }).WithHeight(40).WithBackgroundColor(new Color(0.2f, 0.2f, 0.3f)).WithTextColor(Color.white).WithFontStyle(FontStyle.Bold).CreateGui(_guiContext);
            _controlsContainer.Add(jumpBtn);

            _lblCellData = new Label($"CURRENT CELL: [{_targetCell.x}, {_targetCell.y}, {_targetCell.z}]") { style = { color = new Color(0.9f, 0.9f, 0.1f), marginTop = 10, unityFontStyleAndWeight = FontStyle.Bold } };
            _controlsContainer.Add(_lblCellData);

            _controlsContainer.Add(new VisualElement { style = { height = 1, backgroundColor = Color.black, marginTop = 15, marginBottom = 15 } });

            // --- TIER 1 PARAMS ---
            _controlsContainer.Add(new Label("2. LOCAL CELL PARAMETERS (TIER 1)") { style = { color = Color.gray, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });

            var starSlider = new SliderInt("Local Star Density", 500, 5000) { value = _starsPerCell };
            starSlider.RegisterValueChangedCallback(e => _starsPerCell = e.newValue);
            _controlsContainer.Add(starSlider);

            var resDropdown = new DropdownField("Skybox Resolution (Px)", new System.Collections.Generic.List<string> { "512", "1024", "2048" }, 1);
            resDropdown.RegisterValueChangedCallback(e => _textureResolution = int.Parse(e.newValue));
            _controlsContainer.Add(resDropdown);

            _controlsContainer.Add(new VisualElement { style = { height = 1, backgroundColor = Color.black, marginTop = 15, marginBottom = 15 } });

            // --- TIER 2 PARAMS ---
            _controlsContainer.Add(new Label("3. DEEP SPACE INTEGRATION (TIER 2)") { style = { color = Color.gray, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });

            var radSlider = new SliderInt("Cosmic Search Radius (Cells)", 5, 40) { value = _deepSpaceRadius };
            radSlider.RegisterValueChangedCallback(e => _deepSpaceRadius = e.newValue);
            _controlsContainer.Add(radSlider);

            var threshSlider = new Slider("Light Culling Threshold", 0.01f, 0.2f) { value = _clusterLightThreshold };
            threshSlider.RegisterValueChangedCallback(e => _clusterLightThreshold = e.newValue);
            _controlsContainer.Add(threshSlider);

            _controlsContainer.Add(new VisualElement { style = { height = 1, backgroundColor = Color.black, marginTop = 20, marginBottom = 20 } });

            // --- ACTIONS ---
            var execBtn = new ForgeButtonBuilder("IGNITE UNIVERSE PIPELINE", ExecuteTier1Ignition)
                .WithHeight(60).WithBackgroundColor(new Color(0.8f, 0.3f, 0.1f)).WithTextColor(Color.white).WithFontStyle(FontStyle.Bold)
                .CreateGui(_guiContext);
            execBtn.style.fontSize = 18;
            _controlsContainer.Add(execBtn);

            _controlsContainer.Add(new VisualElement { style = { height = 1, backgroundColor = Color.black, marginTop = 20, marginBottom = 20 } });

            // --- TELEMETRY ---
            _controlsContainer.Add(new Label("PIPELINE TELEMETRY") { style = { color = Color.gray, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });

            _lblTier1Time = CreateTelemetryRow("TIER 1 (LOCAL IGNITION):", out _tier1Indicator);
            _lblTier2Time = CreateTelemetryRow("TIER 2 (DEEP SPACE):", out _tier2Indicator);

            _lblTotalTime = new Label("TOTAL PIPELINE: 0.00 ms") { style = { fontSize = 16, color = Color.white, marginTop = 10, unityFontStyleAndWeight = FontStyle.Bold } };

            _controlsContainer.Add(_lblTotalTime);

            return _controlsContainer;
        }

        private Label CreateTelemetryRow(string title, out VisualElement indicator)
        {
            var row = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 5 } };
            indicator = new VisualElement { style = { width = 12, height = 12,  backgroundColor = new Color(0.2f, 0.2f, 0.2f), marginRight = 10 } };
            var lbl = new Label($"{title} AWAITING") { style = { color = Color.gray, fontSize = 14 } };
            row.Add(indicator);
            row.Add(lbl);
            _controlsContainer.Add(row);
            return lbl;
        }

        private VisualElement BuildPreviewPanel()
        {
            _previewContainer = new VisualElement { style = { flexGrow = 1, backgroundColor = Color.black } };

            var lmp = new LiveModelPreviewBuilder(_universeRoot)
                .WithZoom(0.001f) // Push the camera dead center inside the inverted dome
                .WithPitch(0f)
                .WithMouseControl(true)
                .WithGizmos(false)
                .WithBackgroundColor(Color.black)
                .Build();

            lmp.style.flexGrow = 1;
            _previewContainer.Add(lmp);
            _previewContainer.Add(new Label("CELESTIAL OBSERVATORY - DRAG TO LOOK AROUND") { style = { position = Position.Absolute, top = 20, left = 20, color = new Color(1, 1, 1, 0.5f), unityFontStyleAndWeight = FontStyle.Bold } });

            return _previewContainer;
        }

        // --- HIGH PERFORMANCE COMPUTING LOGIC ---

        private void ExecuteTier1Ignition()
        {
            if (_isProcessingDeepSpace) return; // Prevent spam clicking

            _swPipeline = Stopwatch.StartNew();

            int texWidth = _textureResolution;
            int texHeight = _textureResolution / 2;
            int pixelCount = texWidth * texHeight;

            // Initialize or Clear Persistent Memory
            if (!_persistentPixels.IsCreated || _persistentPixels.Length != pixelCount)
            {
                if (_persistentPixels.IsCreated) _persistentPixels.Dispose();
                _persistentPixels = new NativeArray<Color32>(pixelCount, Allocator.Persistent);

                if (_localStarPositions.IsCreated) _localStarPositions.Dispose();
                if (_localStarColors.IsCreated) _localStarColors.Dispose();
                _localStarPositions = new NativeArray<float3>(_starsPerCell, Allocator.Persistent);
                _localStarColors = new NativeArray<float3>(_starsPerCell, Allocator.Persistent);
            }

            // 1. GENERATE LOCAL STARS (Job)
            var starJob = new GenerateCellStarsJob
            {
                CellCoords = _targetCell,
                MasterSeed = _masterSeed,
                Radius = 100f,
                Positions = _localStarPositions,
                Colors = _localStarColors
            };
            JobHandle starHandle = starJob.Schedule(_starsPerCell, 64);

            // 2. RENDER PIXELS TO BUFFER (Job)
            var renderJob = new RenderLocalStarsJob
            {
                Width = texWidth,
                Height = texHeight,
                StarPositions = _localStarPositions,
                StarColors = _localStarColors,
                Pixels = _persistentPixels
            };
            renderJob.Schedule(pixelCount, 64, starHandle).Complete(); // BLOCK MAIN THREAD for Tier 1

            // 3. UPLOAD TO GPU
            UploadTexture(texWidth, texHeight);

            // TIER 1 TELEMETRY
            double ms = _swPipeline.Elapsed.TotalMilliseconds;
            _lblTier1Time.text = $"TIER 1 (LOCAL IGNITION): {ms:F2} ms";
            _tier1Indicator.style.backgroundColor = (ms < 150) ? Color.green : ((ms < 400) ? Color.yellow : Color.red);

            _tier2Indicator.style.backgroundColor = new Color(0.9f, 0.6f, 0.1f);
            _lblTier2Time.text = "TIER 2 (DEEP SPACE): CALCULATING...";

            // Trigger Async Deep Space Processing
            _isProcessingDeepSpace = true;
        }

        private void ProcessDeepSpaceIntegration()
        {
            if (!_isProcessingDeepSpace) return;

            Stopwatch swDeepSpace = Stopwatch.StartNew();

            int texWidth = _textureResolution;
            int texHeight = _textureResolution / 2;
            int pixelCount = texWidth * texHeight;

            // 1. ALLOCATE TEMPORARY CLUSTER ARRAYS
            // Calculate max possible clusters in radius: (2R+1)^3
            int dim = (_deepSpaceRadius * 2) + 1;
            int maxClusters = dim * dim * dim;

            NativeArray<float3> clusterPositions = new NativeArray<float3>(maxClusters, Allocator.TempJob);
            NativeArray<float3> clusterColors = new NativeArray<float3>(maxClusters, Allocator.TempJob);

            // 2. GENERATE DISTANT CLUSTERS (Aggregated Cell Light)
            var clusterJob = new GenerateDistantClustersJob
            {
                OriginCell = _targetCell,
                UniverseMax = UNIVERSE_SIZE,
                Radius = _deepSpaceRadius,
                MasterSeed = _masterSeed,
                Threshold = _clusterLightThreshold,
                CellSpacing = 50f, // Distance between cells
                ClusterPositions = clusterPositions,
                ClusterColors = clusterColors
            };
            JobHandle clusterHandle = clusterJob.Schedule(maxClusters, 64);

            // 3. ADD CLUSTER LIGHT TO EXISTING PIXELS
            var renderClustersJob = new AddDistantClustersJob
            {
                Width = texWidth,
                Height = texHeight,
                ClusterPositions = clusterPositions,
                ClusterColors = clusterColors,
                Pixels = _persistentPixels // We are reading and modifying the Persistent Buffer!
            };
            renderClustersJob.Schedule(pixelCount, 64, clusterHandle).Complete(); // Block for integration

            // 4. SECONDARY GPU UPLOAD
            UploadTexture(texWidth, texHeight);

            swDeepSpace.Stop();
            _swPipeline.Stop();

            // TIER 2 TELEMETRY
            double ms = swDeepSpace.Elapsed.TotalMilliseconds;
            _lblTier2Time.text = $"TIER 2 (DEEP SPACE): {ms:F2} ms";
            _tier2Indicator.style.backgroundColor = Color.green;
            _lblTotalTime.text = $"TOTAL PIPELINE: {_swPipeline.Elapsed.TotalMilliseconds:F2} ms";

            // Cleanup Temp Arrays
            clusterPositions.Dispose();
            clusterColors.Dispose();

            _isProcessingDeepSpace = false;
        }

        private void UploadTexture(int width, int height)
        {
            if (_domeTexture == null || _domeTexture.width != width)
            {
                if (_domeTexture != null) UnityEngine.Object.DestroyImmediate(_domeTexture);
                _domeTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
                _domeTexture.filterMode = FilterMode.Bilinear;
                _domeTexture.wrapMode = TextureWrapMode.Repeat;
                _domeMaterial.mainTexture = _domeTexture;
                if (_domeMaterial.HasProperty("_BaseMap")) _domeMaterial.SetTexture("_BaseMap", _domeTexture);
            }
            _domeTexture.SetPixelData(_persistentPixels, 0);
            _domeTexture.Apply(false);
        }

        // --- BURST COMPILED JOBS ---

        [BurstCompile(CompileSynchronously = true)]
        public struct GenerateCellStarsJob : IJobParallelFor
        {
            [ReadOnly] public int3 CellCoords;
            [ReadOnly] public uint MasterSeed;
            [ReadOnly] public float Radius;
            [WriteOnly] public NativeArray<float3> Positions;
            [WriteOnly] public NativeArray<float3> Colors;

            public void Execute(int index)
            {
                uint cellHash = Squirrel3((uint)(CellCoords.x * 73856093 ^ CellCoords.y * 19349663 ^ CellCoords.z * 83492791), MasterSeed);
                uint noiseX = Squirrel3((uint)index, cellHash);
                uint noiseY = Squirrel3((uint)index, cellHash + 1);
                uint noiseZ = Squirrel3((uint)index, cellHash + 2);
                uint noiseC = Squirrel3((uint)index, cellHash + 3);

                float u = (float)noiseX / (float)uint.MaxValue;
                float v = (float)noiseY / (float)uint.MaxValue;

                float PI_f = 3.14159265f;
                float theta = u * 2.0f * PI_f;
                float phi = math.acos(2.0f * v - 1.0f);
                float r = Radius + (((float)noiseZ / (float)uint.MaxValue) * Radius * 0.2f);

                Positions[index] = new float3(r * math.sin(phi) * math.cos(theta), r * math.sin(phi) * math.sin(theta), r * math.cos(phi));

                float temp = (float)noiseC / (float)uint.MaxValue;
                Colors[index] = new float3(math.lerp(0.4f, 1.0f, temp), math.lerp(0.6f, 0.9f, temp), math.lerp(0.8f, 1.0f, 1f - temp));
            }

            private uint Squirrel3(uint position, uint seed)
            { /* Implementation */
                const uint BIT_NOISE1 = 0xb5297a4d; const uint BIT_NOISE2 = 0x68e31da4; const uint BIT_NOISE3 = 0x1b56c4e9;
                uint noise = position; noise *= BIT_NOISE1; noise += seed; noise ^= (noise >> 8); noise += BIT_NOISE2; noise ^= (noise << 8); noise *= BIT_NOISE3; noise ^= (noise >> 8); return noise;
            }
        }

        [BurstCompile(CompileSynchronously = true)]
        public struct RenderLocalStarsJob : IJobParallelFor
        {
            [ReadOnly] public int Width;
            [ReadOnly] public int Height;
            [ReadOnly] public NativeArray<float3> StarPositions;
            [ReadOnly] public NativeArray<float3> StarColors;
            [WriteOnly] public NativeArray<Color32> Pixels;

            public void Execute(int index)
            {
                int x = index % Width; int y = index / Width;
                float PI_f = 3.14159265f;
                float u = (float)x / (float)(Width - 1); float v = (float)y / (float)(Height - 1);
                float theta = u * 2.0f * PI_f - PI_f; float phi = v * PI_f;

                float3 rayDir = math.normalize(new float3(math.sin(phi) * math.sin(theta), math.cos(phi), math.sin(phi) * math.cos(theta)));
                float3 accumulatedLight = new float3(0.01f, 0.01f, 0.02f); // Deep space base

                for (int s = 0; s < StarPositions.Length; s++)
                {
                    float dot = math.dot(rayDir, math.normalize(StarPositions[s]));
                    if (dot > 0) accumulatedLight += StarColors[s] * math.pow(dot, 15000f);
                }

                accumulatedLight = math.clamp(accumulatedLight, 0f, 1f);
                Pixels[index] = new Color32((byte)(accumulatedLight.x * 255f), (byte)(accumulatedLight.y * 255f), (byte)(accumulatedLight.z * 255f), 255);
            }
        }

        [BurstCompile(CompileSynchronously = true)]
        public struct GenerateDistantClustersJob : IJobParallelFor
        {
            [ReadOnly] public int3 OriginCell;
            [ReadOnly] public int UniverseMax;
            [ReadOnly] public int Radius;
            [ReadOnly] public uint MasterSeed;
            [ReadOnly] public float Threshold;
            [ReadOnly] public float CellSpacing; // e.g. 50 units per cell distance

            [WriteOnly] public NativeArray<float3> ClusterPositions;
            [WriteOnly] public NativeArray<float3> ClusterColors;

            public void Execute(int index)
            {
                // Map linear index to 3D local cluster grid (-Radius to +Radius)
                int dim = (Radius * 2) + 1;
                int z = index / (dim * dim);
                int y = (index / dim) % dim;
                int x = index % dim;

                int3 localOffset = new int3(x - Radius, y - Radius, z - Radius);
                int3 target = OriginCell + localOffset;

                // Default to 0,0,0 (ignored later)
                ClusterPositions[index] = float3.zero;
                ClusterColors[index] = float3.zero;

                // Boundary checks and self-exclusion
                if (target.x < 0 || target.x >= UniverseMax || target.y < 0 || target.y >= UniverseMax || target.z < 0 || target.z >= UniverseMax) return;
                if (localOffset.x == 0 && localOffset.y == 0 && localOffset.z == 0) return; // Skip our own cell

                // Distance culling math
                float3 worldDist = new float3(localOffset.x * CellSpacing, localOffset.y * CellSpacing, localOffset.z * CellSpacing);
                float distSqr = math.lengthsq(worldDist);

                // Inverse Square Law for aggregated cell brightness
                float intensity = 1000f / distSqr;

                if (intensity > Threshold)
                {
                    uint cellHash = Squirrel3((uint)(target.x * 73856093 ^ target.y * 19349663 ^ target.z * 83492791), MasterSeed);
                    uint noiseC = Squirrel3((uint)index, cellHash);
                    float temp = (float)noiseC / (float)uint.MaxValue;

                    ClusterPositions[index] = worldDist;

                    // Clusters are often purplish/blue or bright white due to atmospheric scattering at distance
                    ClusterColors[index] = new float3(math.lerp(0.5f, 1.0f, temp), math.lerp(0.5f, 0.8f, temp), math.lerp(0.8f, 1.0f, 1f - temp)) * intensity;
                }
            }

            private uint Squirrel3(uint position, uint seed)
            { /* Implementation */
                const uint BIT_NOISE1 = 0xb5297a4d; const uint BIT_NOISE2 = 0x68e31da4; const uint BIT_NOISE3 = 0x1b56c4e9;
                uint noise = position; noise *= BIT_NOISE1; noise += seed; noise ^= (noise >> 8); noise += BIT_NOISE2; noise ^= (noise << 8); noise *= BIT_NOISE3; noise ^= (noise >> 8); return noise;
            }
        }

        [BurstCompile(CompileSynchronously = true)]
        public struct AddDistantClustersJob : IJobParallelFor
        {
            [ReadOnly] public int Width;
            [ReadOnly] public int Height;
            [ReadOnly] public NativeArray<float3> ClusterPositions;
            [ReadOnly] public NativeArray<float3> ClusterColors;

            public NativeArray<Color32> Pixels; // Read & Write to existing buffer

            public void Execute(int index)
            {
                int x = index % Width; int y = index / Width;
                float PI_f = 3.14159265f;
                float u = (float)x / (float)(Width - 1); float v = (float)y / (float)(Height - 1);
                float theta = u * 2.0f * PI_f - PI_f; float phi = v * PI_f;

                float3 rayDir = math.normalize(new float3(math.sin(phi) * math.sin(theta), math.cos(phi), math.sin(phi) * math.cos(theta)));

                // Read EXISTING pixel color
                Color32 existingC = Pixels[index];
                float3 accumulatedLight = new float3(existingC.r / 255f, existingC.g / 255f, existingC.b / 255f);

                for (int c = 0; c < ClusterPositions.Length; c++)
                {
                    if (ClusterPositions[c].x == 0 && ClusterPositions[c].y == 0 && ClusterPositions[c].z == 0) continue; // Skip culled clusters

                    float dot = math.dot(rayDir, math.normalize(ClusterPositions[c]));
                    if (dot > 0)
                    {
                        // Broader exponent for clusters (they look like glowing dust/galaxies, not sharp stars)
                        accumulatedLight += ClusterColors[c] * math.pow(dot, 500f);
                    }
                }

                accumulatedLight = math.clamp(accumulatedLight, 0f, 1f);
                Pixels[index] = new Color32((byte)(accumulatedLight.x * 255f), (byte)(accumulatedLight.y * 255f), (byte)(accumulatedLight.z * 255f), 255);
            }
        }

        private void CleanUpMemory()
        {
            if (_persistentPixels.IsCreated) _persistentPixels.Dispose();
            if (_localStarPositions.IsCreated) _localStarPositions.Dispose();
            if (_localStarColors.IsCreated) _localStarColors.Dispose();

            if (_universeRoot != null) UnityEngine.Object.DestroyImmediate(_universeRoot);
            if (_domeTexture != null) UnityEngine.Object.DestroyImmediate(_domeTexture);
            if (_domeMaterial != null) UnityEngine.Object.DestroyImmediate(_domeMaterial);
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
#if UNITY_EDITOR
        public void ToUIDocument(string path) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), path);
#endif
        public void FromUIDocument(string path) => _guiContext?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(path));
    }
}