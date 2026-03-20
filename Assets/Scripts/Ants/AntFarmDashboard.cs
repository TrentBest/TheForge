using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using System.Collections.Generic;
using TheSingularityWorkshop.FSM_API;
using TheSingularityWorkshop.Sandbox.Ants;
using UnityEngine;
using UnityEngine.UIElements;

public class AntFarmDashboard : IGuiProvider
{
    public string Title => "MYRMECOLOGY ENGINE";

    private List<AntFarmContext> _farms;
    private AntFarmContext _activeFarm;
    private VisualElement _rootContainer;
    private Image _renderTarget;
    private VisualElement _tabsContainer;

    public AntFarmDashboard()
    {
        _farms = new List<AntFarmContext>
        {
            new AntFarmContext("Main Colony", 256, 256),
            new AntFarmContext("Expansion Tube A", 256, 256)
        };
        _activeFarm = _farms[0];

        // In a real setup, we map the FSM_API processing group here.
        // FSM_API.FSM_API.Create.CreateFiniteStateMachine("AntLogic"..
    }

    public VisualElement CreateGui(GuiContext ctx)
    {
        _rootContainer = new VisualElement { style = { flexGrow = 1, flexDirection = FlexDirection.Row, backgroundColor = new Color(0.1f, 0.1f, 0.1f) } };

        // --- 1. SIDEBAR: THE ACTIONS ---
        var sidebar = new VisualElement { style = { width = 220, paddingTop = 15, paddingRight = 15, paddingLeft = 15, paddingBottom = 15, borderRightWidth = 2, borderRightColor = new Color(0.8f, 0.5f, 0.2f) } };
        sidebar.Add(new Label("COLONY CONTROLS") { style = { color = new Color(0.8f, 0.5f, 0.2f), unityFontStyleAndWeight = FontStyle.Bold, fontSize = 16, marginBottom = 20 } });

        sidebar.Add(new Button(() => DropMaterial(2)) { text = "DROP FOOD", style = { height = 40, backgroundColor = new Color(0.2f, 0.6f, 0.2f), color = Color.white, marginBottom = 10 } });
        sidebar.Add(new Button(() => DropMaterial(3)) { text = "POUR WATER", style = { height = 40, backgroundColor = new Color(0.2f, 0.4f, 0.8f), color = Color.white, marginBottom = 10 } });

        var shakeBtn = new Button(() => {
            _activeFarm.IsShaking = true;
            _activeFarm.ShakeTimer = 2.0f; // Shake for 2 seconds
        })
        {
            text = "SHAKE FARM",
            style = { height = 40, backgroundColor = new Color(0.8f, 0.2f, 0.2f), color = Color.white, marginBottom = 20 }
        };
        sidebar.Add(shakeBtn);

        sidebar.Add(new Button(() => AddNewFarm()) { text = "+ CONNECT NEW FARM", style = { height = 30, marginTop = 20 } });

        // --- 2. MAIN VIEW: TABS & RENDERER ---
        var mainContent = new VisualElement { style = { flexGrow = 1, flexDirection = FlexDirection.Column } };

        // Tab Bar
        _tabsContainer = new VisualElement { style = { flexDirection = FlexDirection.Row, height = 40, backgroundColor = new Color(0.15f, 0.15f, 0.15f), borderBottomWidth = 2, borderBottomColor = new Color(0.8f, 0.5f, 0.2f) } };
        mainContent.Add(_tabsContainer);
        RefreshTabs();

        // Render View
        var renderContainer = new VisualElement { style = { flexGrow = 1, alignItems = Align.Center, justifyContent = Justify.Center, paddingTop = 20, paddingBottom = 20, paddingLeft = 20, paddingRight = 20 } };
        _renderTarget = new Image
        {
            image = _activeFarm.DisplayTexture,
            scaleMode = ScaleMode.ScaleToFit,
            style = { width = Length.Percent(100), height = Length.Percent(100) }
        };
        renderContainer.Add(_renderTarget);
        mainContent.Add(renderContainer);

        _rootContainer.Add(sidebar);
        _rootContainer.Add(mainContent);

        // --- 3. THE "GHOST" SIMULATION TICK ---
        _rootContainer.schedule.Execute(() =>
        {
            // 1. Tick the FSM Processing Group for ALL farms
            // FSM_API.FSM_API.Interaction.Update("AntColonySim");

            // (Mocking the compute step for the UI script context)
            SimulateActiveFarm(_activeFarm);

            // 2. Redraw the active texture
            _renderTarget.MarkDirtyRepaint();

        }).Every(16); // ~60fps target

        return _rootContainer;
    }

    private void RefreshTabs()
    {
        _tabsContainer.Clear();
        foreach (var farm in _farms)
        {
            bool isActive = farm == _activeFarm;
            var tabBtn = new Button(() => {
                _activeFarm = farm;
                _renderTarget.image = _activeFarm.DisplayTexture;
                RefreshTabs();
            })
            {
                text = farm.FarmName,
                style = {
                    flexGrow = 1,
                    backgroundColor = isActive ? new Color(0.8f, 0.5f, 0.2f) : new Color(0.2f, 0.2f, 0.2f),
                    color = isActive ? Color.black : Color.white,
                    unityFontStyleAndWeight = isActive ? FontStyle.Bold : FontStyle.Normal,
                    borderBottomWidth = 0
                }
            };
            _tabsContainer.Add(tabBtn);
        }
    }

    private void AddNewFarm()
    {
        var newFarm = new AntFarmContext($"Sector {ToRoman(_farms.Count + 1)}", 256, 256);
        _farms.Add(newFarm);
        RefreshTabs();
    }

    private void DropMaterial(byte stateValue)
    {
        // Drop a cluster of pixels at the top center
        int cx = _activeFarm.Width / 2;
        int cy = _activeFarm.Height - 10;
        for (int i = -5; i <= 5; i++)
        {
            for (int j = -5; j <= 5; j++)
            {
                _activeFarm.Grid[cx + i, cy + j] = stateValue;
            }
        }
    }

    // --- MOCK COMPUTE SHADER STEP ---
    private void SimulateActiveFarm(AntFarmContext farm)
    {
        // In reality, this is where the FSM_API calls the Compute Shader.
        // The shader reads the byte map, applies falling gravity to water/food, 
        // makes the ants dig sand, and updates the PixelColors buffer.

        Color32 colorAir = new Color32(30, 30, 30, 255);
        Color32 colorSand = new Color32(194, 178, 128, 255);
        Color32 colorWater = new Color32(50, 150, 255, 255);
        Color32 colorFood = new Color32(100, 255, 100, 255);
        Color32 colorAnt = new Color32(255, 50, 50, 255); // Red ants!
        Color32 colorPanic = new Color32(255, 255, 0, 255); // Yellow when shaken

        if (farm.IsShaking)
        {
            farm.ShakeTimer -= Time.unscaledDeltaTime;
            if (farm.ShakeTimer <= 0) farm.IsShaking = false;
        }

        // Map bytes to colors for the Texture
        for (int x = 0; x < farm.Width; x++)
        {
            for (int y = 0; y < farm.Height; y++)
            {
                byte state = farm.Grid[x, y];
                Color32 pColor = colorAir;

                if (state == 1) pColor = colorSand;
                else if (state == 2) pColor = colorFood;
                else if (state == 3) pColor = colorWater;
                else if (state == 4 || state == 5) pColor = farm.IsShaking ? colorPanic : colorAnt;

                // Set pixel
                farm.PixelColors[y * farm.Width + x] = pColor;
            }
        }

        farm.DisplayTexture.SetPixels32(farm.PixelColors);
        farm.DisplayTexture.Apply();
    }

    private string ToRoman(int number) { return number == 1 ? "I" : number == 2 ? "II" : number == 3 ? "III" : number.ToString(); }
    public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
    public void FromUIDocument(string assetPath) { }
    public void ToUIDocument(string assetPath) { }
}