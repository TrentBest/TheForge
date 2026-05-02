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
    /// Evaluates the generative universe function by containing it within a bounding "Crystal Sphere".
    /// Utilizes Inverse-Square volumetric projection with Alpha Blending.
    /// </summary>
    public class MastersOfOrionII_Gui_UniverseCrystalForge : IGuiProvider
    {
        public string Title => "FORGE: CRYSTAL UNIVERSE EVALUATOR";

        private GuiContext _guiContext;
        private VisualElement _rootContainer;
        private VisualElement _controlsContainer;
        private VisualElement _previewContainer;

        // --- 3D ENVIRONMENT ---
        private GameObject _universeRoot;
        private GameObject _crystalSphere;
        private Material _crystalMaterial;
        private Texture2D _crystalTexture;

        // --- UNIVERSE PARAMS ---
        private int _starsPerCell = 2500;
        private int _textureResolution = 1024;
        private uint _masterSeed = 999;
        private float _sphereRadius = 100f;
        private float _exposure = 150f; // Controls light accumulation blowout

        // --- TELEMETRY UI ---
        private Label _lblTimeTotal;
        private VisualElement _blinkIndicator;

        public VisualElement CreateGui(GuiContext ctx)
        {
            _guiContext = ctx;

            _rootContainer = new GraphicalUserInterfaceBuilder("CrystalForgeRoot")
                .WithPercentSize(100, 100)
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.08f))
                .Build();

            Initialize3DEnvironment();

            var splitPanel = new ForgeSplitPanelBuilder(400, Side.Left)
                .WithSidebar(new DynamicGuiProvider(c => BuildControlPanel()))
                .WithMain(new DynamicGuiProvider(c => BuildPreviewPanel()));

            _rootContainer.Add(splitPanel.CreateGui(ctx));

            _rootContainer.RegisterCallback<DetachFromPanelEvent>(e => CleanUpMemory());

            return _rootContainer;
        }

        private void Initialize3DEnvironment()
        {
            _universeRoot = new GameObject("[CRYSTAL_UNIVERSE_ROOT]");
            _universeRoot.transform.position = new Vector3(0, -10000, 0);

            // Standard Sphere (Look from outside)
            _crystalSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _crystalSphere.transform.SetParent(_universeRoot.transform, false);
            _crystalSphere.transform.localScale = Vector3.one * 10f; // Size in the LMP

            UnityEngine.Object.DestroyImmediate(_crystalSphere.GetComponent<Collider>());

            // Use Sprites/Default. It natively supports Unlit Alpha Blending perfectly across all pipelines!
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null) shader = Shader.Find("Unlit/Transparent"); // Fallback
            if (shader == null) shader = Shader.Find("Standard");

            _crystalMaterial = new Material(shader);
            _crystalSphere.GetComponent<Renderer>().sharedMaterial = _crystalMaterial;
        }

        private VisualElement BuildControlPanel()
        {
            _controlsContainer = new ScrollView(ScrollViewMode.Vertical) { style = { flexGrow = 1, paddingTop = 20, backgroundColor = new Color(0.08f, 0.08f, 0.1f) } };

            _controlsContainer.Add(new Label("VOLUMETRIC EVALUATOR") { style = { fontSize = 20, unityFontStyleAndWeight = FontStyle.Bold, color = new Color(0.4f, 0.8f, 1f), marginBottom = 20 } });

            // --- PARAMS ---
            _controlsContainer.Add(new Label("VOLUME PARAMETERS") { style = { color = Color.gray, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });

            var starSlider = new SliderInt("Contained Star Density", 500, 15000) { value = _starsPerCell };
            starSlider.RegisterValueChangedCallback(e => _starsPerCell = e.newValue);
            _controlsContainer.Add(starSlider);

            var resDropdown = new DropdownField("Crystal Resolution (Px)", new System.Collections.Generic.List<string> { "512", "1024", "2048" }, 1);
            resDropdown.RegisterValueChangedCallback(e => _textureResolution = int.Parse(e.newValue));
            _controlsContainer.Add(resDropdown);

            var expSlider = new Slider("Volumetric Exposure (Light Multiplier)", 10f, 2000f) { value = _exposure };
            expSlider.RegisterValueChangedCallback(e => _exposure = e.newValue);
            _controlsContainer.Add(expSlider);

            var seedField = new TextField("Quantum Seed") { value = _masterSeed.ToString() };
            seedField.RegisterValueChangedCallback(e => { if (uint.TryParse(e.newValue, out uint s)) _masterSeed = s; });
            _controlsContainer.Add(seedField);

            _controlsContainer.Add(new VisualElement { style = { height = 1, backgroundColor = Color.black, marginTop = 15, marginBottom = 15 } });

            // --- ACTIONS ---
            var execBtn = new ForgeButtonBuilder("MANIFEST CRYSTAL SPHERE", ExecuteBurstGeneration)
                .WithHeight(60).WithBackgroundColor(new Color(0.6f, 0.1f, 0.8f)).WithTextColor(Color.white).WithFontStyle(FontStyle.Bold)
                .CreateGui(_guiContext);
            execBtn.style.fontSize = 18;
            _controlsContainer.Add(execBtn);

            _controlsContainer.Add(new VisualElement { style = { height = 1, backgroundColor = Color.black, marginTop = 20, marginBottom = 20 } });

            // --- TELEMETRY ---
            _controlsContainer.Add(new Label("EVALUATION TELEMETRY") { style = { color = Color.gray, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });

            _lblTimeTotal = new Label("Generation Chronos: 0.00 ms") { style = { fontSize = 16, color = Color.white, unityFontStyleAndWeight = FontStyle.Bold } };
            _controlsContainer.Add(_lblTimeTotal);

            _blinkIndicator = new VisualElement { style = { width = 100, height = 10, marginTop = 10, backgroundColor = Color.gray } };
            _controlsContainer.Add(_blinkIndicator);

            return _controlsContainer;
        }

        private VisualElement BuildPreviewPanel()
        {
            _previewContainer = new VisualElement { style = { flexGrow = 1, backgroundColor = Color.black } };

            var lmp = new LiveModelPreviewBuilder(_universeRoot)
                .WithZoom(20f)
                .WithPitch(15f)
                .WithMouseControl(true)
                .WithGizmos(false)
                .WithAutoOrbit(Vector3.up, 10f)
                .WithBackgroundColor(new Color(0.01f, 0.01f, 0.03f)) // Deep space background shows through the glass!
                .Build();

            lmp.style.flexGrow = 1;
            _previewContainer.Add(lmp);

            _previewContainer.Add(new Label("OUTSIDE LOOKING IN - DRAG TO ORBIT CRYSTAL") { style = { position = Position.Absolute, top = 20, left = 20, color = new Color(1, 1, 1, 0.5f), unityFontStyleAndWeight = FontStyle.Bold } });

            return _previewContainer;
        }

        // --- HIGH PERFORMANCE COMPUTING LOGIC ---

        private void ExecuteBurstGeneration()
        {
            Stopwatch swTotal = Stopwatch.StartNew();

            int texWidth = _textureResolution;
            int texHeight = _textureResolution / 2;
            int pixelCount = texWidth * texHeight;

            NativeArray<float3> starPositions = new NativeArray<float3>(_starsPerCell, Allocator.TempJob);
            NativeArray<float3> starColors = new NativeArray<float3>(_starsPerCell, Allocator.TempJob);
            NativeArray<Color32> pixelData = new NativeArray<Color32>(pixelCount, Allocator.TempJob);

            // 1. GENERATE CONTAINED STARS
            var starJob = new GenerateContainedStarsJob
            {
                Seed = _masterSeed,
                Radius = _sphereRadius * 0.95f,
                Positions = starPositions,
                Colors = starColors
            };
            JobHandle starHandle = starJob.Schedule(_starsPerCell, 64);

            // 2. VOLUMETRIC ALPHA PROJECTION
            var renderJob = new VolumetricAlphaProjectionJob
            {
                Width = texWidth,
                Height = texHeight,
                SphereRadius = _sphereRadius,
                Exposure = _exposure,
                StarPositions = starPositions,
                StarColors = starColors,
                Pixels = pixelData
            };

            renderJob.Schedule(pixelCount, 64, starHandle).Complete();

            // 3. UPLOAD TO GPU
            if (_crystalTexture == null || _crystalTexture.width != texWidth)
            {
                if (_crystalTexture != null) UnityEngine.Object.DestroyImmediate(_crystalTexture);
                _crystalTexture = new Texture2D(texWidth, texHeight, TextureFormat.RGBA32, false);
                _crystalTexture.filterMode = FilterMode.Bilinear;
                _crystalTexture.wrapMode = TextureWrapMode.Repeat;

                _crystalMaterial.mainTexture = _crystalTexture;
            }

            _crystalTexture.SetPixelData(pixelData, 0);
            _crystalTexture.Apply(false);

            swTotal.Stop();
            double totalMs = swTotal.Elapsed.TotalMilliseconds;
            _lblTimeTotal.text = $"Generation Chronos: {totalMs:F2} ms";
            _blinkIndicator.style.backgroundColor = (totalMs < 150) ? Color.green : ((totalMs < 400) ? Color.yellow : Color.red);

            starPositions.Dispose();
            starColors.Dispose();
            pixelData.Dispose();
        }

        // --- BURST COMPILED JOBS ---

        [BurstCompile(CompileSynchronously = true)]
        public struct GenerateContainedStarsJob : IJobParallelFor
        {
            [ReadOnly] public uint Seed;
            [ReadOnly] public float Radius;
            [WriteOnly] public NativeArray<float3> Positions;
            [WriteOnly] public NativeArray<float3> Colors;

            public void Execute(int index)
            {
                uint noiseX = Squirrel3((uint)index, Seed);
                uint noiseY = Squirrel3((uint)index, Seed + 1);
                uint noiseZ = Squirrel3((uint)index, Seed + 2);
                uint noiseC = Squirrel3((uint)index, Seed + 3);

                float u = (float)noiseX / (float)uint.MaxValue;
                float v = (float)noiseY / (float)uint.MaxValue;

                float PI_f = 3.14159265f;
                float theta = u * 2.0f * PI_f;
                float phi = math.acos(2.0f * v - 1.0f);

                // Distribute towards the core
                float r = Radius * math.pow(((float)noiseZ / (float)uint.MaxValue), 0.6f);

                Positions[index] = new float3(r * math.sin(phi) * math.cos(theta), r * math.sin(phi) * math.sin(theta), r * math.cos(phi));

                float temp = (float)noiseC / (float)uint.MaxValue;
                Colors[index] = new float3(math.lerp(0.4f, 1.0f, temp), math.lerp(0.6f, 0.9f, temp), math.lerp(0.8f, 1.0f, 1f - temp));
            }

            private uint Squirrel3(uint position, uint seed)
            {
                const uint BIT_NOISE1 = 0xb5297a4d; const uint BIT_NOISE2 = 0x68e31da4; const uint BIT_NOISE3 = 0x1b56c4e9;
                uint noise = position; noise *= BIT_NOISE1; noise += seed; noise ^= (noise >> 8); noise += BIT_NOISE2; noise ^= (noise << 8); noise *= BIT_NOISE3; noise ^= (noise >> 8); return noise;
            }
        }

        [BurstCompile(CompileSynchronously = true)]
        public struct VolumetricAlphaProjectionJob : IJobParallelFor
        {
            [ReadOnly] public int Width;
            [ReadOnly] public int Height;
            [ReadOnly] public float SphereRadius;
            [ReadOnly] public float Exposure;
            [ReadOnly] public NativeArray<float3> StarPositions;
            [ReadOnly] public NativeArray<float3> StarColors;

            [WriteOnly] public NativeArray<Color32> Pixels;

            public void Execute(int index)
            {
                int x = index % Width;
                int y = index / Width;

                float PI_f = 3.14159265f;
                float u = (float)x / (float)(Width - 1);
                float v = (float)y / (float)(Height - 1);

                float theta = u * 2.0f * PI_f - PI_f;
                float phi = v * PI_f;

                float3 surfacePoint = new float3(
                    math.sin(phi) * math.sin(theta),
                    math.cos(phi),
                    math.sin(phi) * math.cos(theta)
                ) * SphereRadius;

                float3 accumulatedLight = new float3(0, 0, 0);

                for (int s = 0; s < StarPositions.Length; s++)
                {
                    float distSqr = math.distancesq(surfacePoint, StarPositions[s]);

                    // The +5f prevents extreme blowout for stars resting right against the surface
                    float intensity = Exposure / (distSqr + 5f);

                    if (intensity > 0.01f)
                    {
                        accumulatedLight += StarColors[s] * intensity;
                    }
                }

                // Total Energy dictates the Transparency (Alpha). 
                float energy = math.max(accumulatedLight.x, math.max(accumulatedLight.y, accumulatedLight.z));

                // Add 0.05f so the glass of the sphere is barely visible even when dark
                float alpha = math.clamp(energy + 0.05f, 0f, 1f);

                // Add a very subtle purple crystalline tint to the base color
                accumulatedLight += new float3(0.05f, 0.02f, 0.1f) * alpha;
                accumulatedLight = math.clamp(accumulatedLight, 0f, 1f);

                Pixels[index] = new Color32(
                    (byte)(accumulatedLight.x * 255f),
                    (byte)(accumulatedLight.y * 255f),
                    (byte)(accumulatedLight.z * 255f),
                    (byte)(alpha * 255f) // TRUE TRANSPARENCY
                );
            }
        }

        private void CleanUpMemory()
        {
            if (_universeRoot != null) UnityEngine.Object.DestroyImmediate(_universeRoot);
            if (_crystalTexture != null) UnityEngine.Object.DestroyImmediate(_crystalTexture);
            if (_crystalMaterial != null) UnityEngine.Object.DestroyImmediate(_crystalMaterial);
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
#if UNITY_EDITOR
        public void ToUIDocument(string path) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), path);
#endif
        public void FromUIDocument(string path) => _guiContext?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(path));
    }
}