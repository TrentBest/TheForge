// File: Assets/Scripts/Workshop/Core/SingularityBootloader.cs
using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using Workshop.Core.Diagnostics;
using Workshop.Core.Memory;
using Workshop.Core.Rendering;
using Workshop.Core.Rendering.Splash;
using Workshop.UI_And_Tools.Forge;
using Workshop.UI_And_Tools.Forge.IO;
using TheSingularityWorkshop.FSM_API.Scripts;
using TheSingularityWorkshop.FSM_API;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Workshop.Core
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public static class SingularityBootloader
    {
        public static DataWarehouse MainWarehouse { get; private set; }
        public static TheForge ForgeOS { get; private set; }
        public static AnyAppContext AppContext { get; private set; }
        private static IDataTransport _hermitUdpLink; // Changed to Interface for WebGL support
        private static bool _isBooting = false;
        private static int _bootSafetyCounter = 0;

        static SingularityBootloader() { }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void IgniteSingularity()
        {
            if (_isBooting) return;
            _isBooting = true;

            ForgeLogger.Log(">> ENTER: IgniteSingularity").WithHeader("Temporal").WithColor("#AAAAAA").SendToUnity();
            var sw = Stopwatch.StartNew();

            try
            {
                ForgeLogger.Log("Sovereign Kernel Boot Sequence Initiated.").WithHeader("Boot").WithColor("#00FFCC").SendToUnity();

                // --- THE REHYDRATION FIX ---
                // 1. Dispose the old warehouse
                if (MainWarehouse != null) MainWarehouse.Dispose();

                // 2. Free the locked UDP port (Application.quitting misses Domain Reloads)
                (_hermitUdpLink as IDisposable)?.Dispose();
                _hermitUdpLink = null;

                // 3. Kill the stale context to force a fresh pipeline binding
                AppContext = null;
                // ---------------------------

                MainWarehouse = new DataWarehouse();

                var obsShelf = MainWarehouse.GetOrCreateShelf<WorldObserver>(1);
                obsShelf.Store(new WorldObserver
                {
                    Position = new Vector3(0, 0, -20),
                    Rotation = Quaternion.identity,
                    IsActive = true,
                    ViewRadius = 100f
                });

                MainWarehouse.GetOrCreateShelf<SovereignGuiSurface>(10);
                MainWarehouse.GetOrCreateShelf<HermitEntity>(1);
                MainWarehouse.GetOrCreateShelf<MinionEntity>(100);

                ForgeLogger.Log("Entity Shelves allocated.").WithHeader("Memory").SendToUnity();

#if UNITY_EDITOR
                EditorApplication.update -= EditorTick;
                EditorApplication.update += EditorTick;
#endif

                ExecuteSequentialBoot();

                Application.quitting -= OnShutdown;
                Application.quitting += OnShutdown;
            }
            finally
            {
                _isBooting = false;
                sw.Stop();
                ForgeLogger.Log($"<< EXIT: IgniteSingularity (Elapsed: {sw.ElapsedMilliseconds}ms)").WithHeader("Temporal").WithColor("#AAAAAA").SendToUnity();
            }
        }

        private static void ExecuteSequentialBoot()
        {
            ForgeLogger.Log(">> ENTER: ExecuteSequentialBoot").WithHeader("Temporal").WithColor("#AAAAAA").SendToUnity();
            var sw = Stopwatch.StartNew();

            try
            {
                if (AppContext == null)
                {
                    BootSovereignEye();

                    // Force the pipeline into GraphicsSettings if Unity stripped it
                    if (UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline == null)
                    {
                        var emergencyAsset = Resources.Load<UnityEngine.Rendering.RenderPipelineAsset>("Rendering/New Singularity Pipeline Asset");
                        if (emergencyAsset != null)
                        {
                            UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline = emergencyAsset;
                            QualitySettings.renderPipeline = emergencyAsset;
                        }
                    }

                    // Create the context. The Pipeline slot will be temporarily NULL.
                    AppContext = new AnyAppContext(MainWarehouse, null);
                    IgniteHeartbeat();

                    ForgeLogger.Log("Bootloader standing by. Awaiting Pipeline Ignition...").WithColor(Color.yellow).SendToUnity();
                }
            }
            finally
            {
                sw.Stop();
                ForgeLogger.Log($"<< EXIT: ExecuteSequentialBoot (Elapsed: {sw.ElapsedMilliseconds}ms)").WithHeader("Temporal").WithColor("#AAAAAA").SendToUnity();
            }
        }

        // We move the final steps into a public method the Pipeline can trigger
        public static void FinalizeBootSequence(Workshop.Core.Rendering.SingularityRenderPipeline pipelineInstance)
        {
            if (AppContext.Pipeline != null) return; // Only run once

            ForgeLogger.Log("Pipeline Acquired via Inversion of Control! Proceeding to Core Packages.").WithColor(Color.green).SendToUnity();
            AppContext.SetPipeline(pipelineInstance);

            LoadCorePackages(AppContext);
            BootForgeConstruct(AppContext);

            ForgeLogger.Log("Sequential Boot Complete. Logic Flow Restored.").WithColor(Color.green).SendToUnity();
        }

        private static System.Collections.IEnumerator RetryBootRoutine()
        {
            // Yielding null tells Unity to wait exactly 1 frame before continuing
            yield return null;
            ExecuteSequentialBoot();
        }

        private static void OnPipelineAwakened(ScriptableRenderContext context, System.Collections.Generic.List<Camera> cameras)
        {
            var pipe = RenderPipelineManager.currentPipeline as SingularityRenderPipeline;
            if (pipe == null) return;

            // Immediately unhook so we only ever fire this once
            RenderPipelineManager.beginContextRendering -= OnPipelineAwakened;

            ForgeLogger.Log("Pipeline Acquired via Native Render Hook! Proceeding to Core Packages.").WithColor(Color.green).SendToUnity();
            AppContext.SetPipeline(pipe);

            ProceedWithCoreBoot();
        }

        private static void ProceedWithCoreBoot()
        {
            LoadCorePackages(AppContext);
            BootForgeConstruct(AppContext);
            ForgeLogger.Log("Sequential Boot Complete. Logic Flow Restored.").WithColor(Color.green).SendToUnity();
        }

        private static void LoadCorePackages(AnyAppContext ctx)
        {
            ForgeLogger.Log(">> ENTER: LoadCorePackages").WithHeader("Temporal").WithColor("#AAAAAA").SendToUnity();
            var sw = Stopwatch.StartNew();

            try
            {
                Diagnostics.ForgeLogger.Log("Injecting Splash Pass...").WithHeader("Boot").SendToUnity();

                Mesh[] loadedMeshes = Resources.LoadAll<Mesh>("Meshes/Splash");
                Material[] loadedMats = Resources.LoadAll<Material>("Materials/Splash");

                if (loadedMeshes.Length == 0 || loadedMats.Length == 0) return;

                var splashPass = new SplashMonikerRenderPass(loadedMeshes, loadedMats);
                ctx.Pipeline.RegisterDrawPass(splashPass);

                var letterShelf = ctx.Warehouse.GetOrCreateShelf<SplashLetterEntity>(22);
                string[] wordLines = { "THE", "SINGULARITY", "WORKSHOP" };

                Color[] palette = {
                    new Color(1.0f, 0.1f, 0.2f),
                    new Color(1.0f, 0.5f, 0.0f),
                    new Color(1.0f, 0.9f, 0.1f),
                    new Color(0.1f, 1.0f, 0.2f),
                    new Color(0.1f, 0.4f, 1.0f),
                    new Color(1.0f, 0.0f, 1.0f)
                };

                int globalIndex = 0;
                float goldenRatio = 1.618f;
                float viewportScale = 1.85f;
                float baseSpacing = 1.2f * goldenRatio * viewportScale;
                float ySpacing = 1.5f * goldenRatio * viewportScale;
                Vector3 goldenScale = Vector3.one * goldenRatio * viewportScale;

                for (int row = 0; row < wordLines.Length; row++)
                {
                    string word = wordLines[row];
                    float[] xPositions = new float[word.Length];
                    float cursor = 0f;

                    for (int c = 0; c < word.Length; c++)
                    {
                        xPositions[c] = cursor;
                        if (c < word.Length - 1)
                        {
                            float advance = (GetLetterKerningWeight(word[c]) + GetLetterKerningWeight(word[c + 1])) / 2f * baseSpacing;
                            cursor += advance;
                        }
                    }

                    float totalWidth = cursor;
                    float startX = -totalWidth / 2f;
                    float startY = ((wordLines.Length - 1) * ySpacing) / 2f - (row * ySpacing);

                    for (int c = 0; c < word.Length; c++)
                    {
                        if (globalIndex >= letterShelf.PageSize) break;

                        char character = word[c];
                        int mappedMeshId = 0;
                        for (int m = 0; m < loadedMeshes.Length; m++)
                        {
                            if (loadedMeshes[m].name.EndsWith(character.ToString(), StringComparison.OrdinalIgnoreCase))
                            {
                                mappedMeshId = m; break;
                            }
                        }

                        Vector3 finalPos = new Vector3(startX + xPositions[c], startY, 0);

                        letterShelf.Update(globalIndex, new SplashLetterEntity
                        {
                            LetterIndex = globalIndex,
                            OriginalPosition = finalPos,
                            OriginalRotation = Quaternion.Euler(0, 180, 0),
                            OriginalScale = goldenScale,
                            Position = finalPos,
                            Rotation = Quaternion.Euler(0, 180, 0),
                            Scale = goldenScale,
                            FactionColor = palette[globalIndex % 6],
                            MeshId = mappedMeshId,
                            MaterialId = 0
                        });

                        globalIndex++;
                    }
                }

                new SplashAnimatorContext();
                ctx.AreCorePackagesLoaded = true;
            }
            finally
            {
                sw.Stop();
                ForgeLogger.Log($"<< EXIT: LoadCorePackages (Elapsed: {sw.ElapsedMilliseconds}ms)").WithHeader("Temporal").WithColor("#AAAAAA").SendToUnity();
            }
        }

#if UNITY_EDITOR
        private static void EditorTick()
        {
            if (!Application.isPlaying)
            {
                FSM_API.Interaction.Update("Update");
                FSM_API.Interaction.Update("Workshop");
                SceneView.RepaintAll();
            }
        }
#endif

        private static float GetLetterKerningWeight(char c)
        {
            return c switch { 'I' => 0.45f, 'W' => 1.50f, 'M' => 1.50f, 'T' => 1.15f, 'Y' => 1.15f, 'L' => 0.90f, _ => 1.0f };
        }
       public static void DoNothing()
        {

        }
        private static FSM_UnityIntegrationAdvanced IgniteHeartbeat()
        {
            ForgeLogger.Log(">> ENTER: IgniteHeartbeat").WithHeader("Temporal").WithColor("#AAAAAA").SendToUnity();
            var sw = Stopwatch.StartNew();

            try
            {

                
                var integration = FSM_UnityIntegrationAdvanced.Instance;

                integration.AddProcessingGroup("Update", "Core_Logic");
                integration.AddProcessingGroup("Update", "Workshop");
                integration.AddProcessingGroup("LateUpdate", "Network_Outbound_Tick");
                integration.AddProcessingGroup("LateUpdate", "Render_Sync");

                return integration;
            }
            finally
            {
                sw.Stop();
                ForgeLogger.Log($"<< EXIT: IgniteHeartbeat (Elapsed: {sw.ElapsedMilliseconds}ms)").WithHeader("Temporal").WithColor("#AAAAAA").SendToUnity();
            }
        }

        private static void BootSovereignEye()
        {
            ForgeLogger.Log(">> ENTER: BootSovereignEye").WithHeader("Temporal").WithColor("#AAAAAA").SendToUnity();
            var sw = Stopwatch.StartNew();

            try
            {
                var ghostEyes = Resources.FindObjectsOfTypeAll<Camera>();
                foreach (var cam in ghostEyes)
                {
                    if (cam.name == "Sovereign_Eye")
                    {
                        if (Application.isPlaying) UnityEngine.Object.Destroy(cam.gameObject);
                        else UnityEngine.Object.DestroyImmediate(cam.gameObject);
                    }
                }

                ForgeLogger.Log("No visual cortex detected. Spawning Sovereign Eye.").WithHeader("Boot").WithColor("#00FFCC").SendToUnity();

                var eyeGo = new GameObject("Sovereign_Eye");
                eyeGo.tag = "MainCamera";

                var newCam = eyeGo.AddComponent<Camera>();
                newCam.clearFlags = CameraClearFlags.SolidColor;
                newCam.backgroundColor = new Color(0.05f, 0.02f, 0.06f);
                newCam.transform.position = new Vector3(0, 0, -20);
                newCam.transform.rotation = Quaternion.identity;

                if (Application.isPlaying)
                    UnityEngine.Object.DontDestroyOnLoad(eyeGo);
                else
                    eyeGo.hideFlags = HideFlags.HideAndDontSave;
            }
            finally
            {
                sw.Stop();
                ForgeLogger.Log($"<< EXIT: BootSovereignEye (Elapsed: {sw.ElapsedMilliseconds}ms)").WithHeader("Temporal").WithColor("#AAAAAA").SendToUnity();
            }
        }

        private static void BootForgeConstruct(AnyAppContext ctx)
        {
            ForgeLogger.Log(">> ENTER: BootForgeConstruct").WithHeader("Temporal").WithColor("#AAAAAA").SendToUnity();
            var sw = Stopwatch.StartNew();

            try
            {
                ForgeLogger.Log("Entering Forge Construct Mode.").WithHeader("Forge OS").WithColor("#FF00FF").SendToUnity();

                // RE-INJECTED: Shield the bootloader from socket crashes
                try
                {
                    BootNetworkLayer();
                }
                catch (Exception ex)
                {
                    ForgeLogger.LogWarning($"UDP Port is occupied. Bypassing Network Boot. Message: {ex.Message}").SendToUnity();
                }

                BootForgeUI();

                // RE-INJECTED: Seed the experience context so TheForge doesn't redline!
                if (ExperienceContext.Active == null)
                {
                    new ExperienceContext("Genesis_Workshop", HostEnvironment.AnyApp_Forge);
                }

                var fabricator = FabricatorFactory.GetSelectionFabricator();
                ForgeOS = new TheForge(fabricator);
            }
            finally
            {
                sw.Stop();
                ForgeLogger.Log($"<< EXIT: BootForgeConstruct (Elapsed: {sw.ElapsedMilliseconds}ms)").WithHeader("Temporal").WithColor("#AAAAAA").SendToUnity();
            }
        }

        private static void BootNetworkLayer()
        {
            ForgeLogger.Log(">> ENTER: BootNetworkLayer").WithHeader("Temporal").WithColor("#AAAAAA").SendToUnity();
            var sw = Stopwatch.StartNew();

            try
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                ForgeLogger.Log("Manifesting WebGL Interop Transport...").SendToUnity();
                // Assumes BrowserInteropDataTransport exists in your project.
                // _hermitUdpLink = new BrowserInteropDataTransport("Hermit_Web_Link"); 
#else
                ForgeLogger.Log("Manifesting UDP Socket Transport...").SendToUnity();
                _hermitUdpLink = new UdpDataTransport("Hermit_UDP_Link", "127.0.0.1", 8080, 8081);
#endif

                if (_hermitUdpLink != null)
                {
                    SingularityDataBus.Instance.RegisterRoute("Hermit", _hermitUdpLink);
                }
            }
            finally
            {
                sw.Stop();
                ForgeLogger.Log($"<< EXIT: BootNetworkLayer (Elapsed: {sw.ElapsedMilliseconds}ms)").WithHeader("Temporal").WithColor("#AAAAAA").SendToUnity();
            }
        }

        private static void BootForgeUI()
        {
            ForgeLogger.Log(">> ENTER: BootForgeUI").WithHeader("Temporal").WithColor("#AAAAAA").SendToUnity();
            var sw = Stopwatch.StartNew();

            try
            {
                var uiDoc = UnityEngine.Object.FindAnyObjectByType<UIDocument>();
                if (uiDoc == null)
                {
                    var uiGo = new GameObject("Singularity_UI_Shell");
                    uiDoc = uiGo.AddComponent<UIDocument>();
                    if (Application.isPlaying)
                    {
                        UnityEngine.Object.DontDestroyOnLoad(uiGo);
                    }
                }
            }
            finally
            {
                sw.Stop();
                ForgeLogger.Log($"<< EXIT: BootForgeUI (Elapsed: {sw.ElapsedMilliseconds}ms)").WithHeader("Temporal").WithColor("#AAAAAA").SendToUnity();
            }
        }

        private static void OnShutdown()
        {
            ForgeLogger.Log(">> ENTER: OnShutdown").WithHeader("Temporal").WithColor("#AAAAAA").SendToUnity();
            var sw = Stopwatch.StartNew();

            try
            {
                var tempWarehouse = MainWarehouse;
                MainWarehouse = null;
                tempWarehouse?.Dispose();

                // Interface cast for safe disposal
                (_hermitUdpLink as IDisposable)?.Dispose();

                var ghostEye = GameObject.Find("Sovereign_Eye");
                if (ghostEye != null)
                {
                    if (Application.isPlaying) UnityEngine.Object.Destroy(ghostEye);
                    else UnityEngine.Object.DestroyImmediate(ghostEye);
                }
            }
            finally
            {
                sw.Stop();
                ForgeLogger.Log($"<< EXIT: OnShutdown (Elapsed: {sw.ElapsedMilliseconds}ms)").WithHeader("Temporal").WithColor("#AAAAAA").SendToUnity();
            }
        }
    }
}