using System;
using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.FSM_API;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Assets.Scripts.CorsairsInSpace;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Corsair
{
    public class CorsairsInSpace_Gui_LairArchitect : IGuiProvider, IStateContext
    {
        public string Title => "Lair Architect & Prospector";

        public string Name { get; set; } = "LairArchitect_UI_Context";
        public bool IsValid { get; set; } = true;

        private CorsairLairContext _lairContext;
        private Label _statsLabel;
        private VisualElement _mainViewContainer;

        private string _appState = "Prospecting";

        private float _slicePitch = 0f;
        private float _sliceYaw = 0f;
        private int _asteroidScaleMeters = 128;

        private Image _renderTarget;
        private byte _currentTool = 0;
        private int _currentLevel = 0;

        public CorsairsInSpace_Gui_LairArchitect(IGuiRouter r)
        {
        }

        public VisualElement CreateGui(GuiContext context)
        {
            _lairContext = GameObject.FindAnyObjectByType<CorsairLairContext>();
            if (_lairContext == null)
            {
                var go = new GameObject("CorsairLairData");
                _lairContext = go.AddComponent<CorsairLairContext>();
            }

            var builder = new GraphicalUserInterfaceBuilder("LairArchitectRoot")
                .WithPercentSize(100, 100)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.08f, 1.0f))
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                .OnBuild(ve => {
                    ve.style.paddingTop = 10; ve.style.paddingBottom = 10;
                    ve.style.paddingLeft = 10; ve.style.paddingRight = 10;
                });

            builder.AddChild(ctx => {
                var headerLbl = new ForgeLabelBuilder("CORSAIR SYNDICATE: LAIR FOUNDATION")
                    .WithFontSize(24)
                    .WithFontStyle(FontStyle.Bold)
                    .WithColor(new Color(0f, 0.8f, 1f))
                    .CreateGui(ctx);
                headerLbl.style.marginBottom = 10;
                return headerLbl;
            });

            builder.AddChild(ctx => {
                _statsLabel = new ForgeLabelBuilder("Awaiting Telemetry...")
                    .WithColor(Color.white)
                    .WithFontSize(14)
                    .CreateGui(ctx) as Label;
                _statsLabel.style.backgroundColor = new Color(0, 0, 0, 0.5f);
                _statsLabel.style.paddingTop = 5; _statsLabel.style.paddingBottom = 5;
                _statsLabel.style.marginBottom = 10;
                return _statsLabel;
            });

            builder.AddChild(ctx => {
                _mainViewContainer = new VisualElement { style = { flexGrow = 1 } };
                return _mainViewContainer;
            });

            var root = builder.Build();

            InitializeFSM();
            RebuildMainView(context);

            root.schedule.Execute(() => {
                FSM_API.Interaction.Update("LairArchitect_OS");
                UpdateUIFields();
                if (_appState == "Architect") RenderBlueprint();
            }).Every(16);

            return root;
        }

        private void RebuildMainView(GuiContext ctx)
        {
            _mainViewContainer.Clear();

            if (_appState == "Prospecting")
            {
                _mainViewContainer.Add(BuildProspectorView(ctx));
            }
            else if (_appState == "Architect")
            {
                _mainViewContainer.Add(BuildArchitectView(ctx));
            }
        }

        private VisualElement BuildProspectorView(GuiContext ctx)
        {
            var container = new VisualElement { style = { flexGrow = 1, flexDirection = FlexDirection.Row } };

            var previewBox = new VisualElement { style = { flexGrow = 1, borderRightWidth = 2, borderRightColor = Color.gray } };

            var asteroidMock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            asteroidMock.hideFlags = HideFlags.HideAndDontSave;
            asteroidMock.GetComponent<MeshRenderer>().sharedMaterial.color = new Color(0.4f, 0.4f, 0.4f);

            var lmpb = new LiveModelPreviewBuilder(asteroidMock)
                .WithBackgroundColor(new Color(0.02f, 0.02f, 0.05f))
                .WithAutoRotate(true, 5f)
                .WithGizmos(true)
                .WithZoom(2.5f)
                .CreateGui(ctx);

            previewBox.Add(lmpb);
            container.Add(previewBox);

            var controlsBox = new ScrollView { style = { width = 300, paddingLeft = 15, paddingRight = 15 } };

            controlsBox.Add(new Label("ESTABLISH LAIR PARAMETERS") { style = { color = Color.cyan, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 20, marginTop = 20 } });

            // FIXED: Using standard UI Elements styling for word wrap instead of Label.layout
            controlsBox.Add(new Label("These parameters define the physical boundaries of your base within raWWar's galaxy map. Resources laundered from the frontline will be deposited here.") { style = { whiteSpace = WhiteSpace.Normal, marginBottom = 15 } });

            controlsBox.Add(CreateSliderInt("Asteroid Scale (Meters)", 64, 512, _asteroidScaleMeters, v => _asteroidScaleMeters = v));
            controlsBox.Add(CreateSliderFloat("Equatorial Pitch", -90f, 90f, _slicePitch, v => _slicePitch = v));
            controlsBox.Add(CreateSliderFloat("Equatorial Yaw", -180f, 180f, _sliceYaw, v => _sliceYaw = v));

            var btnEstablish = new Button(() =>
            {
                _appState = "Architect";

                _lairContext.Levels.Clear();
                _lairContext.Levels.Add(0, new CorsairGridLevel(0, _asteroidScaleMeters, _asteroidScaleMeters));

                RebuildMainView(ctx);
                GameObject.DestroyImmediate(asteroidMock);

            })
            { text = "INJECT BOARDING SHUTTLE", style = { height = 50, marginTop = 40, backgroundColor = new Color(0.6f, 0.1f, 0.1f), color = Color.white, unityFontStyleAndWeight = FontStyle.Bold } };

            controlsBox.Add(btnEstablish);
            container.Add(controlsBox);

            return container;
        }

        private VisualElement BuildArchitectView(GuiContext ctx)
        {
            var container = new VisualElement { style = { flexGrow = 1, flexDirection = FlexDirection.Column } };

            var toolbar = new GraphicalUserInterfaceBuilder("Toolbar")
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Center)
                .OnBuild(ve => ve.style.marginBottom = 10)
                .Build();

            toolbar.Add(CreateToolButton("SURVEY", 0, ctx));
            toolbar.Add(CreateToolButton("MINE ROCK (₡10)", 1, ctx));
            toolbar.Add(CreateToolButton("BUILD GENERATOR (₡100)", 2, ctx));
            toolbar.Add(CreateToolButton("BUILD BARRACKS (₡100)", 3, ctx));
            container.Add(toolbar);

            var gridContainer = new GraphicalUserInterfaceBuilder("GridContainer")
                .WithBackgroundColor(Color.black)
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)
                .OnBuild(ve => {
                    ve.style.flexGrow = 1;
                    ve.style.borderTopWidth = 2; ve.style.borderBottomWidth = 2;
                    ve.style.borderLeftWidth = 2; ve.style.borderRightWidth = 2;
                    ve.style.borderTopColor = new Color(0f, 0.5f, 0.8f);
                    ve.style.borderBottomColor = new Color(0f, 0.5f, 0.8f);
                    ve.style.borderLeftColor = new Color(0f, 0.5f, 0.8f);
                    ve.style.borderRightColor = new Color(0f, 0.5f, 0.8f);
                }).Build();

            _renderTarget = new Image
            {
                image = _lairContext.Levels[_currentLevel].BlueprintTexture,
                scaleMode = ScaleMode.ScaleToFit,
                style = { width = Length.Percent(100), height = Length.Percent(100) }
            };

            _renderTarget.RegisterCallback<PointerDownEvent>(OnRenderClicked);
            // FIXED: PointerMoveEvent uses pressedButtons != 0 instead of .pressed
            _renderTarget.RegisterCallback<PointerMoveEvent>(evt => { if (evt.pressedButtons != 0) OnRenderClicked(evt); });

            gridContainer.Add(_renderTarget);
            container.Add(gridContainer);

            return container;
        }

        private VisualElement CreateToolButton(string label, byte toolId, GuiContext ctx)
        {
            var btn = new ForgeButtonBuilder(label, () => _currentTool = toolId)
                .WithBackgroundColor(new Color(0.15f, 0.15f, 0.2f))
                .WithHeight(35)
                .CreateGui(ctx);
            btn.style.flexGrow = 1;
            btn.style.marginRight = 5;
            return btn;
        }

        private VisualElement CreateSliderInt(string label, int min, int max, int current, Action<int> onValueChanged)
        {
            var container = new VisualElement { style = { marginBottom = 15 } };
            var lbl = new Label($"{label}: {current}") { style = { color = Color.white } };
            var slider = new SliderInt(min, max) { value = current };
            slider.RegisterValueChangedCallback(evt => { lbl.text = $"{label}: {evt.newValue}"; onValueChanged(evt.newValue); });
            container.Add(lbl); container.Add(slider); return container;
        }

        private VisualElement CreateSliderFloat(string label, float min, float max, float current, Action<float> onValueChanged)
        {
            var container = new VisualElement { style = { marginBottom = 15 } };
            var lbl = new Label($"{label}: {current:F1}") { style = { color = Color.white } };
            var slider = new Slider(min, max) { value = current };
            slider.RegisterValueChangedCallback(evt => { lbl.text = $"{label}: {evt.newValue:F1}"; onValueChanged(evt.newValue); });
            container.Add(lbl); container.Add(slider); return container;
        }

        private void OnRenderClicked(PointerEventBase<PointerMoveEvent> evt) { HandlePaint(evt.localPosition); }
        private void OnRenderClicked(PointerEventBase<PointerDownEvent> evt) { HandlePaint(evt.localPosition); }

        private void HandlePaint(Vector2 localPosition)
        {
            if (_currentTool == 0 || _appState != "Architect") return;

            float xRatio = localPosition.x / _renderTarget.layout.width;
            float yRatio = 1f - (localPosition.y / _renderTarget.layout.height);

            var lvl = _lairContext.Levels[_currentLevel];
            int cx = Mathf.Clamp((int)(xRatio * lvl.Width), 0, lvl.Width - 1);
            int cy = Mathf.Clamp((int)(yRatio * lvl.Height), 0, lvl.Height - 1);

            int brushSize = 2;
            for (int i = -brushSize; i <= brushSize; i++)
            {
                for (int j = -brushSize; j <= brushSize; j++)
                {
                    ApplyArchitecture(cx + i, cy + j, lvl);
                }
            }
        }

        private void ApplyArchitecture(int x, int y, CorsairGridLevel lvl)
        {
            if (x < 0 || x >= lvl.Width || y < 0 || y >= lvl.Height) return;

            if (x == 0 || x == lvl.Width - 1 || y == 0 || y == lvl.Height - 1) return;

            byte currentState = lvl.VoxelGrid[x, y];

            if (_currentTool == 1 && currentState == 0)
            {
                if (_lairContext.TotalCredits >= 10)
                {
                    _lairContext.TotalCredits -= 10;
                    _lairContext.GlobalHeat += 0.1f;
                    lvl.VoxelGrid[x, y] = 1;
                }
            }
            else if (_currentTool == 2 && currentState == 1)
            {
                if (_lairContext.TotalCredits >= 100)
                {
                    _lairContext.TotalCredits -= 100;
                    _lairContext.TotalPowerGenerated += 50;
                    lvl.VoxelGrid[x, y] = 2;
                }
            }
            else if (_currentTool == 3 && currentState == 1)
            {
                if (_lairContext.TotalCredits >= 100)
                {
                    _lairContext.TotalCredits -= 100;
                    _lairContext.MaxMinionCapacity += 10;
                    lvl.VoxelGrid[x, y] = 3;
                }
            }
        }

        private void RenderBlueprint()
        {
            var lvl = _lairContext.Levels[_currentLevel];

            Color32 colRock = new Color32(20, 20, 25, 255);
            Color32 colCrust = new Color32(40, 40, 50, 255);
            Color32 colCorridor = new Color32(100, 100, 110, 255);
            Color32 colGen = new Color32(200, 180, 50, 255);
            Color32 colBarracks = new Color32(50, 100, 200, 255);

            for (int x = 0; x < lvl.Width; x++)
            {
                for (int y = 0; y < lvl.Height; y++)
                {
                    byte state = lvl.VoxelGrid[x, y];
                    Color32 pCol = colRock;

                    if (x == 0 || x == lvl.Width - 1 || y == 0 || y == lvl.Height - 1) pCol = colCrust;
                    else if (state == 1) pCol = colCorridor;
                    else if (state == 2) pCol = colGen;
                    else if (state == 3) pCol = colBarracks;

                    lvl.PixelColors[y * lvl.Width + x] = pCol;
                }
            }

            lvl.BlueprintTexture.SetPixels32(lvl.PixelColors);
            lvl.BlueprintTexture.Apply();
        }

        private void InitializeFSM() { /* FSM Logic preserved for future use */ }

        private void UpdateUIFields()
        {
            if (_lairContext == null || _statsLabel == null) return;

            if (_appState == "Prospecting")
            {
                _statsLabel.text = $" ASTEROID TELEMETRY | SCALE: {_asteroidScaleMeters}m | MASS: ~{_asteroidScaleMeters * _asteroidScaleMeters * 10}kT | CRUST INTEGRITY: OPTIMAL";
                _statsLabel.style.color = Color.cyan;
            }
            else
            {
                _statsLabel.text = $" CREDITS: ₡{_lairContext.TotalCredits} | POWER: {_lairContext.TotalPowerConsumed}/{_lairContext.TotalPowerGenerated} | MINIONS: {_lairContext.TotalMinions}/{_lairContext.MaxMinionCapacity} | HEAT: {_lairContext.GlobalHeat:F1}%";
                _statsLabel.style.color = _lairContext.GlobalHeat > 90f ? Color.red : Color.white;
            }
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));

        // FIXED: Added missing IGuiProvider methods
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}