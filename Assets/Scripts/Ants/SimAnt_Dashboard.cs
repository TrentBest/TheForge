using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using TheSingularityWorkshop.FSM_API;

namespace Assets.Scripts.Ants
{

    // --- 3. REFORGED DASHBOARD GUI ---

    public class Ants_SimAnt_Dashboard : IGuiProvider
    {
        public string Title => "SIM ANT REFORGED";
        private VisualElement _rootContainer;
        private Image _renderTarget;
        private SimAntWorld _world;

        private Label _lblFps;
        private Label _lblSwarm;
        private Label _lblFood;
        private Label _lblWater;

        private byte _activeTool = 0;
        private Button _btnToolFood;
        private Button _btnToolWater;
        private Button _btnToolWall;

        public Ants_SimAnt_Dashboard()
        {
            _world = new SimAntWorld();
            MacroAntLogic.InitializeFSM();
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _rootContainer = new VisualElement { style = { flexGrow = 1, flexDirection = FlexDirection.Row, backgroundColor = new Color(0.1f, 0.1f, 0.1f) } };

            var sidebar = new ScrollView { style = { width = 280, paddingBottom = 15, paddingTop = 15, paddingLeft = 15, paddingRight = 15, borderRightWidth = 2, borderRightColor = new Color(0.4f, 0.2f, 0.6f) } };

            // --- TOOLS ---
            sidebar.Add(new Label("MACRO CONTROLS") { style = { color = new Color(0.8f, 0.6f, 1.0f), unityFontStyleAndWeight = FontStyle.Bold, fontSize = 16, marginBottom = 15 } });
            _btnToolFood = new Button(() => SetTool(1)) { text = "PAINT FOOD", style = { height = 30, marginBottom = 5 } };
            _btnToolWater = new Button(() => SetTool(2)) { text = "PAINT WATER", style = { height = 30, marginBottom = 5 } };
            _btnToolWall = new Button(() => SetTool(3)) { text = "PAINT OBSTRUCTION", style = { height = 30, marginBottom = 20 } };
            sidebar.Add(_btnToolFood);
            sidebar.Add(_btnToolWater);
            sidebar.Add(_btnToolWall);
            SetTool(0);

            // --- SETTINGS TUNER ---
            var settingsBox = new VisualElement { style = { backgroundColor = new Color(0.15f, 0.15f, 0.15f), paddingBottom = 10, paddingTop = 10, paddingLeft = 10, paddingRight = 10, marginBottom = 20 } };
            settingsBox.Add(new Label("SIMULATION TUNING") { style = { color = new Color(1f, 0.6f, 0.2f), marginBottom = 10, unityFontStyleAndWeight = FontStyle.Bold } });

            settingsBox.Add(CreateSliderInt("Max Population", 0, 1000, _world.Settings.MaxPopulation, v => _world.Settings.MaxPopulation = v));
            settingsBox.Add(CreateSliderFloat("Ant Base Speed", 0.1f, 3.0f, _world.Settings.AntBaseSpeed, v => _world.Settings.AntBaseSpeed = v));
            settingsBox.Add(CreateSliderFloat("Wander Jitter", 0f, 45f, _world.Settings.WanderJitter, v => _world.Settings.WanderJitter = v));

            // Pheromone Settings
            settingsBox.Add(CreateSliderInt("Drop Amount (+Scent)", 1, 100, _world.Settings.PheromoneDropAmount, v => _world.Settings.PheromoneDropAmount = v));
            settingsBox.Add(CreateSliderInt("Decay Rate (-Scent)", 0, 10, _world.Settings.PheromoneDecayRate, v => _world.Settings.PheromoneDecayRate = v));
            settingsBox.Add(CreateSliderFloat("Scent Range (Depletion)", 0.05f, 1.0f, _world.Settings.ChargeDecayRate, v => _world.Settings.ChargeDecayRate = v));

            sidebar.Add(settingsBox);

            // --- TELEMETRY ---
            var statsBox = new VisualElement { style = { backgroundColor = new Color(0.15f, 0.15f, 0.15f), paddingBottom = 10, paddingTop = 10, paddingLeft = 10 } };
            statsBox.Add(new Label("TELEMETRY") { style = { color = Color.gray, marginBottom = 5, unityFontStyleAndWeight = FontStyle.Bold } });

            _lblFps = new Label("FPS: --");
            _lblSwarm = new Label($"Population: 0 / {_world.Settings.MaxPopulation}");
            _lblFood = new Label("Food Stored: 0");
            _lblWater = new Label("Water Stored: 0");

            statsBox.Add(_lblFps);
            statsBox.Add(_lblSwarm);
            statsBox.Add(_lblFood);
            statsBox.Add(_lblWater);
            sidebar.Add(statsBox);

            // --- RENDER VIEW ---
            var renderContainer = new VisualElement { style = { flexGrow = 1, alignItems = Align.Center, justifyContent = Justify.Center, paddingBottom = 20, paddingTop = 20 } };
            _renderTarget = new Image { image = _world.DisplayTexture, scaleMode = ScaleMode.ScaleToFit, style = { width = Length.Percent(100), height = Length.Percent(100) } };

            _renderTarget.RegisterCallback<PointerDownEvent>(OnRenderClicked);
            _renderTarget.RegisterCallback<PointerMoveEvent>(evt => { if (evt.actionKey) OnRenderClicked(evt); });

            renderContainer.Add(_renderTarget);
            _rootContainer.Add(sidebar);
            _rootContainer.Add(renderContainer);

            // --- TICK LOOP ---
            _rootContainer.schedule.Execute(() =>
            {
                if (_world.Swarm.Count < _world.Settings.MaxPopulation)
                {
                    for (int i = 0; i < 3 && _world.Swarm.Count < _world.Settings.MaxPopulation; i++)
                    {
                        var ant = new MacroAntContext(_world, _world.Width / 2, _world.Height / 2);
                        ant.Name = $"Ant_{_world.Swarm.Count}";
                        _world.Swarm.Add(ant);
                        FSM_API.Create.CreateInstance("MacroAntAgent", ant, "MacroSwarm");
                    }
                }

                FSM_API.Interaction.Update("MacroSwarm");

                UpdateEnvironmentAndTelemetry();
                RenderContext(_world);
                _renderTarget.MarkDirtyRepaint();

            }).Every(16);

            return _rootContainer;
        }

        // --- UI HELPERS ---
        private VisualElement CreateSliderInt(string label, int min, int max, int current, Action<int> onValueChanged)
        {
            var container = new VisualElement { style = { marginBottom = 10 } };
            var lbl = new Label($"{label}: {current}") { style = { color = Color.white, fontSize = 11 } };
            var slider = new SliderInt(min, max) { value = current };
            slider.RegisterValueChangedCallback(evt => {
                lbl.text = $"{label}: {evt.newValue}";
                onValueChanged(evt.newValue);
            });
            container.Add(lbl);
            container.Add(slider);
            return container;
        }

        private VisualElement CreateSliderFloat(string label, float min, float max, float current, Action<float> onValueChanged)
        {
            var container = new VisualElement { style = { marginBottom = 10 } };
            var lbl = new Label($"{label}: {current:F2}") { style = { color = Color.white, fontSize = 11 } };
            var slider = new Slider(min, max) { value = current };
            slider.RegisterValueChangedCallback(evt => {
                lbl.text = $"{label}: {evt.newValue:F2}";
                onValueChanged(evt.newValue);
            });
            container.Add(lbl);
            container.Add(slider);
            return container;
        }

        private void SetTool(byte toolId)
        {
            _activeTool = toolId;
            Color inactive = new Color(0.2f, 0.2f, 0.2f);
            Color active = new Color(0.3f, 0.1f, 0.5f);
            _btnToolFood.style.backgroundColor = _activeTool == 1 ? active : inactive;
            _btnToolWater.style.backgroundColor = _activeTool == 2 ? active : inactive;
            _btnToolWall.style.backgroundColor = _activeTool == 3 ? new Color(0.5f, 0.5f, 0.5f) : inactive;
        }

        private void OnRenderClicked(PointerEventBase<PointerMoveEvent> evt) { HandlePaint(evt.localPosition); }
        private void OnRenderClicked(PointerEventBase<PointerDownEvent> evt) { HandlePaint(evt.localPosition); }

        private void HandlePaint(Vector2 localPosition)
        {
            if (_activeTool == 0) return;

            float xRatio = localPosition.x / _renderTarget.layout.width;
            float yRatio = 1f - (localPosition.y / _renderTarget.layout.height);

            int cx = Mathf.Clamp((int)(xRatio * _world.Width), 0, _world.Width - 1);
            int cy = Mathf.Clamp((int)(yRatio * _world.Height), 0, _world.Height - 1);

            int radius = _activeTool == 3 ? 8 : 15;

            for (int i = -radius; i <= radius; i++)
            {
                for (int j = -radius; j <= radius; j++)
                {
                    if (i * i + j * j <= radius * radius)
                    {
                        int px = Mathf.Clamp(cx + i, 0, _world.Width - 1);
                        int py = Mathf.Clamp(cy + j, 0, _world.Height - 1);

                        if (_activeTool == 3 && Vector2.Distance(new Vector2(px, py), new Vector2(_world.Width / 2, _world.Height / 2)) < 20f)
                            continue;

                        _world.ResourceGrid[px, py] = _activeTool;
                    }
                }
            }
        }

        private void UpdateEnvironmentAndTelemetry()
        {
            int offset = Time.frameCount % 2;
            for (int x = offset; x < _world.Width; x += 2)
            {
                for (int y = 0; y < _world.Height; y++)
                {
                    if (_world.PheromoneG[x, y] > 0)
                        _world.PheromoneG[x, y] = (byte)Mathf.Max(0, _world.PheromoneG[x, y] - _world.Settings.PheromoneDecayRate);
                    if (_world.PheromoneB[x, y] > 0)
                        _world.PheromoneB[x, y] = (byte)Mathf.Max(0, _world.PheromoneB[x, y] - _world.Settings.PheromoneDecayRate);
                }
            }

            _lblFps.text = $"FPS: {Mathf.RoundToInt(1.0f / Time.unscaledDeltaTime)}";
            _lblSwarm.text = $"Population: {_world.Swarm.Count} / {_world.Settings.MaxPopulation}";
            _lblFood.text = $"Food Stored: {_world.TotalFoodStored}";
            _lblWater.text = $"Water Stored: {_world.TotalWaterStored}";
        }

        private void SafeSetPixel(SimAntWorld ctx, int x, int y, Color32 color)
        {
            if (x >= 0 && x < ctx.Width && y >= 0 && y < ctx.Height)
            {
                ctx.PixelColors[y * ctx.Width + x] = color;
            }
        }

        private void RenderContext(SimAntWorld ctx)
        {
            int cx = ctx.Width / 2;
            int cy = ctx.Height / 2;

            for (int x = 0; x < ctx.Width; x++)
            {
                for (int y = 0; y < ctx.Height; y++)
                {
                    if (Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy)) < 12f)
                    {
                        ctx.PixelColors[y * ctx.Width + x] = new Color32(20, 15, 10, 255);
                        continue;
                    }

                    byte res = ctx.ResourceGrid[x, y];
                    if (res == 1) ctx.PixelColors[y * ctx.Width + x] = new Color32(50, 255, 50, 255);
                    else if (res == 2) ctx.PixelColors[y * ctx.Width + x] = new Color32(50, 200, 255, 255);
                    else if (res == 3) ctx.PixelColors[y * ctx.Width + x] = new Color32(100, 100, 100, 255);
                    else
                    {
                        ctx.PixelColors[y * ctx.Width + x] = new Color32(
                            ctx.PheromoneR[x, y], ctx.PheromoneG[x, y], ctx.PheromoneB[x, y], 255);
                    }
                }
            }

            foreach (var ant in ctx.Swarm)
            {
                Color32 antColor = new Color32(255, 255, 255, 255);
                if (ant.HasFood) antColor = new Color32(50, 255, 50, 255);
                else if (ant.HasWater) antColor = new Color32(50, 200, 255, 255);

                int ax = (int)ant.Position.x;
                int ay = (int)ant.Position.y;

                SafeSetPixel(ctx, ax, ay, antColor);
                SafeSetPixel(ctx, ax + 1, ay, antColor);
                SafeSetPixel(ctx, ax, ay + 1, antColor);
                SafeSetPixel(ctx, ax + 1, ay + 1, antColor);
            }

            ctx.DisplayTexture.SetPixels32(ctx.PixelColors);
            ctx.DisplayTexture.Apply();
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}