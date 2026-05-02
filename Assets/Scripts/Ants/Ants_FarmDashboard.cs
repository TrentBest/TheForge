using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using TheSingularityWorkshop.FSM_API;

namespace Assets.Scripts.Ants
{
    public class Ants_FarmDashboard : IGuiProvider
    {
        public string Title => "MYRMECOLOGY ENGINE";

        // --- DECOUPLED RENDERING STATE ---
        // We wrap the pure data context with the visual resources needed for this specific UI
        private class FarmDisplayState
        {
            public AntFarmContext Context;
            public Texture2D Texture;
            public Color32[] Pixels;

            public FarmDisplayState(AntFarmContext context)
            {
                Context = context;
                Texture = new Texture2D(context.Width, context.Height, TextureFormat.RGBA32, false)
                {
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp
                };
                Pixels = new Color32[context.Width * context.Height];
            }
        }

        private List<FarmDisplayState> _farms;
        private FarmDisplayState _activeDisplay;
        private VisualElement _rootContainer;
        private Image _renderTarget;
        private VisualElement _tabsContainer;

        public Ants_FarmDashboard()
        {
            _farms = new List<FarmDisplayState>();

            // 1. Initialize the FSM Definition in memory ONCE
            AntColonyLogic.InitializeFSM();

            // 2. Create our initial farms and bind them to the API
            AddFarm(new AntFarmContext("Main Colony", 256, 256));
            AddFarm(new AntFarmContext("Expansion Tube A", 256, 256));

            _activeDisplay = _farms[0];
        }

        private void AddFarm(AntFarmContext newFarm)
        {
            _farms.Add(new FarmDisplayState(newFarm));

            // 3. Create the FSM instance for this specific farm
            FSM_API.Create.CreateInstance("AntColonySim", newFarm, "AntFarmLogic");
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            // Pattern: Utilize the ForgeSplitPanelBuilder for macro-layout
            _rootContainer = new ForgeSplitPanelBuilder(sidebarWidth: 260, Side.Left)
                .WithSidebar(BuildControlsSidebar(ctx))
                .WithMain(BuildMainPreview(ctx))
                .CreateGui(ctx);

            // --- THE LIVE SIMULATION TICK ---
            // Schedule the ticking natively within the UI panel's lifecycle
            _rootContainer.schedule.Execute(() =>
            {
                // 1. Tick the FSM Processing Group for ALL farms
                FSM_API.Interaction.Update("AntFarmLogic");

                // 2. Redraw the active texture using the fast path
                RenderActiveFarm(_activeDisplay);
                _renderTarget.MarkDirtyRepaint();

            }).Every(16); // ~60fps target

            return _rootContainer;
        }

        private IGuiProvider BuildControlsSidebar(GuiContext ctx)
        {
            return new GraphicalUserInterfaceBuilder("AntColonyControls")
                .WithPadding(15)
                .WithBackgroundColor(new Color(0.10f, 0.10f, 0.14f))

                .AddHeader("COLONY CONTROLS")
                .AddSeparator(new Color(0.8f, 0.5f, 0.2f), 2)

                // Interactables
                .AddChild(new Button(() => DropMaterial(1)) { text = "DROP SAND", style = { height = 40, backgroundColor = new Color(0.7f, 0.6f, 0.4f), color = Color.white, marginTop = 10, unityFontStyleAndWeight = FontStyle.Bold } })
                .AddChild(new Button(() => DropMaterial(3)) { text = "POUR WATER", style = { height = 40, backgroundColor = new Color(0.2f, 0.4f, 0.8f), color = Color.white, marginTop = 10, unityFontStyleAndWeight = FontStyle.Bold } })

                .AddChild(new Button(() => TriggerShake())
                {
                    text = "SHAKE FARM",
                    style = { height = 40, backgroundColor = new Color(0.8f, 0.2f, 0.2f), color = Color.white, marginTop = 20, unityFontStyleAndWeight = FontStyle.Bold }
                })

                .AddSeparator(new Color(0.3f, 0.3f, 0.3f), 1)

                .AddChild(new Button(() => AddNewFarm())
                {
                    text = "+ CONNECT NEW FARM",
                    style = { height = 35, marginTop = 20, backgroundColor = new Color(0.2f, 0.2f, 0.2f), color = Color.white }
                });
        }

        private IGuiProvider BuildMainPreview(GuiContext ctx)
        {
            return new GraphicalUserInterfaceBuilder("AntMainView")
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.05f))
                .OnBuild(ve =>
                {
                    // Tab Bar
                    _tabsContainer = new VisualElement { style = { flexDirection = FlexDirection.Row, height = 45, backgroundColor = new Color(0.15f, 0.15f, 0.15f), borderBottomWidth = 2, borderBottomColor = new Color(0.8f, 0.5f, 0.2f) } };
                    ve.Add(_tabsContainer);
                    RefreshTabs();

                    // Render View
                    var renderContainer = new VisualElement { style = { flexGrow = 1, alignItems = Align.Center, justifyContent = Justify.Center, paddingBottom = 20, paddingTop = 20 } };
                    _renderTarget = new Image
                    {
                        image = _activeDisplay.Texture,
                        scaleMode = ScaleMode.ScaleToFit,
                        style = { width = Length.Percent(95), height = Length.Percent(95) }
                    };
                    renderContainer.Add(_renderTarget);
                    ve.Add(renderContainer);
                });
        }

        private void TriggerShake()
        {
            if (_activeDisplay != null)
            {
                _activeDisplay.Context.IsShaking = true;
                _activeDisplay.Context.ShakeTimer = 2.0f; // Shake for 2 seconds
            }
        }

        private void RefreshTabs()
        {
            _tabsContainer.Clear();
            foreach (var display in _farms)
            {
                bool isActive = display == _activeDisplay;
                var tabBtn = new Button(() =>
                {
                    _activeDisplay = display;
                    _renderTarget.image = _activeDisplay.Texture;
                    RefreshTabs();
                })
                {
                    text = display.Context.FarmName,
                    style = {
                        flexGrow = 1,
                        backgroundColor = isActive ? new Color(0.8f, 0.5f, 0.2f) : new Color(0.2f, 0.2f, 0.2f),
                        color = isActive ? Color.black : Color.white,
                        unityFontStyleAndWeight = isActive ? FontStyle.Bold : FontStyle.Normal,
                        borderBottomWidth = 0,
                        borderTopWidth = 0,
                        borderLeftWidth = 0,
                        borderRightWidth = 1,
                        borderRightColor = new Color(0.1f, 0.1f, 0.1f)
                    }
                };
                _tabsContainer.Add(tabBtn);
            }
        }

        private void AddNewFarm()
        {
            var newFarm = new AntFarmContext($"Sector {ToRoman(_farms.Count + 1)}", 256, 256);
            AddFarm(newFarm);
            RefreshTabs();
        }

        private void DropMaterial(byte stateValue)
        {
            if (_activeDisplay == null) return;

            var ctx = _activeDisplay.Context;

            // Drop a cluster of pixels at the top center
            int cx = ctx.Width / 2;
            int cy = ctx.Height - 10;
            int w = ctx.Width;

            for (int i = -5; i <= 5; i++)
            {
                for (int j = -5; j <= 5; j++)
                {
                    int x = cx + i;
                    int y = cy + j;

                    // Boundary check to prevent array index out of bounds exceptions
                    if (x >= 0 && x < w && y >= 0 && y < ctx.Height)
                    {
                        // The 1D Flattening Math!
                        int idx = x + (y * w);
                        ctx.Grid[idx] = stateValue;
                    }
                }
            }
        }

        // --- RENDER ONLY ---
        private void RenderActiveFarm(FarmDisplayState display)
        {
            var ctx = display.Context;

            // Updated color palette to match the new physics definitions
            Color32 colorAir = new Color32(25, 25, 30, 255);
            Color32 colorSand = new Color32(194, 178, 128, 255);
            Color32 colorStone = new Color32(100, 100, 100, 255);     // Our new un-diggable stone!
            Color32 colorWater = new Color32(50, 150, 255, 255);

            Color32 colorAntEmpty = new Color32(255, 50, 50, 255);    // Red workers
            Color32 colorAntHauler = new Color32(255, 140, 0, 255);   // Orange haulers (Carrying sand)

            Color32 colorPanic = new Color32(255, 255, 0, 255);       // Yellow when shaken

            if (ctx.IsShaking)
            {
                ctx.ShakeTimer -= Time.unscaledDeltaTime;
                if (ctx.ShakeTimer <= 0) ctx.IsShaking = false;
            }

            // O(N) Iteration through the flattened memory space
            for (int i = 0; i < ctx.Grid.Length; i++)
            {
                byte state = ctx.Grid[i];
                Color32 pColor = colorAir;

                if (state == 1) pColor = colorSand;
                else if (state == 2) pColor = colorStone;
                else if (state == 3) pColor = colorWater;
                else if (state == 4) pColor = ctx.IsShaking ? colorPanic : colorAntEmpty;
                else if (state == 6) pColor = ctx.IsShaking ? colorPanic : colorAntHauler;

                display.Pixels[i] = pColor;
            }

            // O(1) Memory blast to the graphics card
            display.Texture.SetPixelData(display.Pixels, 0);
            display.Texture.Apply(false, false);
        }

        private string ToRoman(int number) { return number == 1 ? "I" : number == 2 ? "II" : number == 3 ? "III" : number.ToString(); }
        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void FromUIDocument(string assetPath) { }
        public void ToUIDocument(string assetPath) { }
    }
}