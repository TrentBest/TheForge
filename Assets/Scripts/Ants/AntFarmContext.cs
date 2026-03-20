using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using System.Collections.Generic;
using TheSingularityWorkshop.FSM_API;



using UnityEngine;
using UnityEngine.UIElements;

namespace TheSingularityWorkshop.Sandbox.Ants
{
    // --- 1. THE DATA CONTEXT ---
    public class AntFarmContext : IStateContext
    {
        public string FarmName;
        public int Width = 128;
        public int Height = 128;
        public byte[,] Grid;
        public Texture2D DisplayTexture;
        public Color32[] PixelColors;
        public bool IsPaused = false;

        // Re-added to fix compiler errors from your other tabs/scripts
        public bool IsShaking = false;
        public float ShakeTimer = 0f;

        // Constructor handles optional parameters to fix "does not contain a constructor that takes 3 arguments"
        public AntFarmContext(string name = "Myrmecology Lab", int width = 128, int height = 128)
        {
            FarmName = name;
            Width = width;
            Height = height;
            Grid = new byte[Width, Height];
            DisplayTexture = new Texture2D(Width, Height) { filterMode = FilterMode.Point };
            PixelColors = new Color32[Width * Height];

            // Fill bottom half with Sand (1), Air (0) above.
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Grid[x, y] = (y < Height * 0.6f) ? (byte)1 : (byte)0;
                }
            }

            // Spawn 200 Ants (4) on the surface
            for (int i = 0; i < 200; i++)
            {
                Grid[UnityEngine.Random.Range(5, Width - 5), (int)(Height * 0.61f)] = 4;
            }
            IsValid = true;
        }

        public bool IsValid { get   ; set ; } = false;
        public string Name { get; set; }
    }

    // --- 2. THE FSM LOGIC (Defined Once) ---
    public static class AntColonyLogic
    {
        public static void InitializeFSM()
        {
            // Define the ruleset. 
            FSM_API.FSM_API.Create.CreateFiniteStateMachine("AntColonySim", processRate: 1, processingGroup: "AntFarmLogic")
                .State("Initializing", OnEnterInitializing, null, null)
                .State("Simulating", OnEnterSimulating, OnUpdateSimulating, OnExitSimulating)
                .State("Paused", OnEnterPaused, OnUpdatePaused, OnExitPaused)
                .Transition("Initializing", "Simulating", (ctx) => true)
                .Transition("Simulating", "Paused", ShouldPause)
                .Transition("Paused", "Simulating", ShouldUnpause)
                .BuildDefinition();
        }

        private static void OnEnterInitializing(IStateContext context) { }
        private static void OnEnterSimulating(IStateContext context) { }

        // WIRED: This now actually runs the simulation when in the "Simulating" state
        private static void OnUpdateSimulating(IStateContext context)
        {
            if (context is AntFarmContext farm)
            {
                ProcessAutomata(farm);
            }
        }

        private static void OnExitSimulating(IStateContext context) { }
        private static void OnEnterPaused(IStateContext context) { }
        private static void OnUpdatePaused(IStateContext context) { }
        private static void OnExitPaused(IStateContext context) { }

        // WIRED: Transition checks now read the IsPaused bool from your UI
        private static bool ShouldPause(IStateContext context)
        {
            return context is AntFarmContext farm && farm.IsPaused;
        }

        private static bool ShouldUnpause(IStateContext context)
        {
            return context is AntFarmContext farm && !farm.IsPaused;
        }

        private static void ProcessAutomata(AntFarmContext farm)
        {
            int w = farm.Width;
            int h = farm.Height;
            bool[,] processed = new bool[w, h];

            // Process bottom to top
            for (int y = 1; y < h; y++)
            {
                // Randomize horizontal processing direction to prevent bias
                bool leftToRight = UnityEngine.Random.value > 0.5f;

                for (int i = 0; i < w; i++)
                {
                    int x = leftToRight ? i : (w - 1 - i);

                    if (processed[x, y]) continue;

                    byte state = farm.Grid[x, y];
                    if (state == 0) continue; // Air

                    // Sand (1) or Water (3) Falls
                    if (state == 1 || state == 3)
                    {
                        if (farm.Grid[x, y - 1] == 0)
                        {
                            Swap(farm, processed, x, y, x, y - 1);
                        }
                        else if (state == 3) // Water flows sideways
                        {
                            int dir = UnityEngine.Random.value > 0.5f ? -1 : 1;
                            if (x + dir > 0 && x + dir < w && farm.Grid[x + dir, y] == 0)
                            {
                                Swap(farm, processed, x, y, x + dir, y);
                            }
                        }
                    }
                    // Ant (4) Wanders & Digs
                    else if (state == 4)
                    {
                        // Gravity applies to ants
                        if (farm.Grid[x, y - 1] == 0)
                        {
                            Swap(farm, processed, x, y, x, y - 1);
                            continue;
                        }

                        // Wander randomly
                        int nx = Mathf.Clamp(x + UnityEngine.Random.Range(-1, 2), 0, w - 1);
                        int ny = Mathf.Clamp(y + UnityEngine.Random.Range(-1, 2), 0, h - 1);

                        if (farm.Grid[nx, ny] == 0) // Move into air
                        {
                            Swap(farm, processed, x, y, nx, ny);
                        }
                        else if (farm.Grid[nx, ny] == 1 && UnityEngine.Random.value > 0.85f) // 15% chance to dig sand
                        {
                            farm.Grid[nx, ny] = 0; // Delete sand
                            Swap(farm, processed, x, y, nx, ny); // Move into the new tunnel
                        }
                    }
                }
            }
        }

        private static void Swap(AntFarmContext farm, bool[,] processed, int x1, int y1, int x2, int y2)
        {
            byte temp = farm.Grid[x1, y1];
            farm.Grid[x1, y1] = farm.Grid[x2, y2];
            farm.Grid[x2, y2] = temp;
            processed[x1, y1] = true;
            processed[x2, y2] = true;
        }
    }

    // --- 3. THE UI PROVIDER ---
    public class AntFarmDashboard : IGuiProvider
    {
        public string Title => "MYRMECOLOGY LAB";

        private AntFarmContext _farmContext;
        private VisualElement _rootContainer;
        private Image _renderTarget;

        public AntFarmDashboard()
        {
            // 1. Set up Data
            _farmContext = new AntFarmContext();

            // 2. Build the FSM Definition in memory
            AntColonyLogic.InitializeFSM();

#if UNITY_EDITOR
            // 3. Hook our logic group into the Editor's native Update loop!
            // FIX: FSM_EditorIntegrationAdvanced is STATIC. Do not use GameObject.FindAnyObjectByType
            //FSM_EditorIntegrationAdvanced.AddProcessingGroup("EditorUpdate", "AntFarmLogic");
#endif

            // 4. Create the FSM instance to start the simulation
            FSM_API.FSM_API.Create.CreateInstance("AntColonySim", _farmContext, "AntFarmLogic");
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _rootContainer = new VisualElement { style = { flexGrow = 1, flexDirection = FlexDirection.Row, backgroundColor = new Color(0.1f, 0.1f, 0.1f) } };

            // FIX: Replaced paddingAll with individual padding settings for UI Toolkit compatibility
            var sidebar = new VisualElement { style = { width = 200, paddingTop = 15, paddingBottom = 15, paddingLeft = 15, paddingRight = 15, borderRightWidth = 2, borderRightColor = Color.grey } };

            sidebar.Add(new Button(() => _farmContext.IsPaused = !_farmContext.IsPaused) { text = "PLAY / PAUSE", style = { height = 40, marginBottom = 10 } });
            sidebar.Add(new Button(() => DropMaterial(3)) { text = "POUR WATER", style = { height = 40, backgroundColor = new Color(0.2f, 0.4f, 0.8f), color = Color.white } });

            // Main Display
            var mainContent = new VisualElement { style = { flexGrow = 1, alignItems = Align.Center, justifyContent = Justify.Center } };
            _renderTarget = new Image
            {
                image = _farmContext.DisplayTexture,
                scaleMode = ScaleMode.ScaleToFit,
                style = { width = Length.Percent(95), height = Length.Percent(95) }
            };
            mainContent.Add(_renderTarget);

            _rootContainer.Add(sidebar);
            _rootContainer.Add(mainContent);

            // --- THE RENDER TICK ---
            _rootContainer.schedule.Execute(() =>
            {
                RenderToTexture(_farmContext);
                _renderTarget.MarkDirtyRepaint();
            }).Every(16); // ~60fps UI refresh

            return _rootContainer;
        }

        private void DropMaterial(byte stateValue)
        {
            for (int i = -3; i <= 3; i++)
            {
                for (int j = -3; j <= 3; j++)
                {
                    int x = Mathf.Clamp((_farmContext.Width / 2) + i, 0, _farmContext.Width - 1);
                    int y = Mathf.Clamp(_farmContext.Height - 5 + j, 0, _farmContext.Height - 1);
                    _farmContext.Grid[x, y] = stateValue;
                }
            }
        }

        private void RenderToTexture(AntFarmContext farm)
        {
            Color32 colAir = new Color32(30, 30, 30, 255);
            Color32 colSand = new Color32(194, 178, 128, 255);
            Color32 colWater = new Color32(50, 150, 255, 255);
            Color32 colAnt = new Color32(255, 50, 50, 255);

            for (int x = 0; x < farm.Width; x++)
            {
                for (int y = 0; y < farm.Height; y++)
                {
                    byte state = farm.Grid[x, y];
                    Color32 pColor = colAir;

                    if (state == 1) pColor = colSand;
                    else if (state == 3) pColor = colWater;
                    else if (state == 4) pColor = colAnt;

                    farm.PixelColors[y * farm.Width + x] = pColor;
                }
            }
            farm.DisplayTexture.SetPixels32(farm.PixelColors);
            farm.DisplayTexture.Apply();
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void FromUIDocument(string assetPath) { }
        public void ToUIDocument(string assetPath) { }
    }
}