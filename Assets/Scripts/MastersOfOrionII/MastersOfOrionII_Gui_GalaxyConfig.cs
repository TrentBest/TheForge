using Assets.Scripts.MastersOfOrionII.Math;
using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.MastersOfOrionII
{
    public class MastersOfOrionII_Gui_GalaxyConfig : IGuiProvider
    {
        public string Title => "UNIVERSE ARCHITECT";

        public string ClickedScaleButton { get; private set; } = "Medium";
        public int MAXIMUM_OPPONENTS { get; private set; } = 100;
        private readonly Color _highlightColor = new Color(0, 0.6f, 0.8f);

        private MastersOfOrionII_Gui_GameBuilder _orchestrator;
        private Label _starCountLabel;
        private VisualElement _morphologyButtonContainer;
        private VisualElement _scaleButtonContainer;

        // --- PREVIEW RENDERER ---
        private VisualElement _galaxyPreviewContainer;
        private float _previewZoom = 1.0f;
        private int _visualStarCount = 500;
        private GuiContext _lastCtx;

        // Parameterless Constructor required for Reflection in Editor UI Tools
        public MastersOfOrionII_Gui_GalaxyConfig() { _orchestrator = new MastersOfOrionII_Gui_GameBuilder(); }
        public MastersOfOrionII_Gui_GalaxyConfig(MastersOfOrionII_Gui_GameBuilder owner) { _orchestrator = owner; }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;
            var root = new VisualElement { style = { flexDirection = FlexDirection.Row, width = Length.Percent(100), height = Length.Percent(100), backgroundColor = Color.black } };

            // --- LEFT COLUMN (1/3): THE FUN BOX & ENVIRONMENTAL SLIDERS ---
            var sideBar = new VisualElement
            {
                style = {
                width = Length.Percent(33.3f), height = Length.Percent(100),
                borderRightWidth = 2, borderRightColor = Color.cyan,
                paddingTop = 20, paddingBottom = 20, paddingLeft = 15, paddingRight = 15
            }
            };

            var funBox = new VisualElement
            {
                style = {
                height = Length.Percent(55f), backgroundColor = new Color(0.05f, 0.05f, 0.15f, 0.9f),
                paddingTop = 15, paddingBottom = 15, paddingLeft = 15, paddingRight = 15,
                borderTopWidth = 2, borderBottomWidth = 2, borderLeftWidth = 2, borderRightWidth = 2,
                borderTopColor = Color.cyan, borderBottomColor = Color.cyan, borderLeftColor = Color.cyan, borderRightColor = Color.cyan
            }
            };
            funBox.Add(new Label("THE FUN BOX // MULTIVERSAL STABILITY") { style = { color = Color.cyan, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });

            funBox.Add(new Toggle("Allow Sci-Fi Tropes") { value = true, style = { color = Color.white } });
            funBox.Add(new Toggle("Mirror AI Reflections") { value = true });

            funBox.Add(new Label("GALAXY MASTER SEED") { style = { color = Color.yellow, marginTop = 15, fontSize = 10 } });

            // Ensure initial seed is set if 0
            if (_orchestrator.GalaxyConfig.MasterSeed == 0) _orchestrator.GalaxyConfig.MasterSeed = (uint)UnityEngine.Random.Range(1000, 999999);

            var seedInput = new TextField { value = _orchestrator.GalaxyConfig.MasterSeed.ToString() };
            seedInput.RegisterValueChangedCallback(evt =>
            {
                if (uint.TryParse(evt.newValue, out uint res))
                {
                    _orchestrator.GalaxyConfig.MasterSeed = res;
                    UpdateVisuals(); // Update preview on text change!
                }
            });
            funBox.Add(seedInput);
            sideBar.Add(funBox);

            // BOTTOM: HORIZONTAL SLIDERS
            var conditions = new VisualElement { style = { marginTop = 20, flexGrow = 1 } };
            conditions.Add(new Label("ENVIRONMENTAL CONDITIONS") { style = { color = Color.yellow, marginBottom = 10, unityFontStyleAndWeight = FontStyle.Bold, fontSize = 10 } });

            conditions.Add(CreateHorizontalSlider("Universe Age", 0.5f, 15.0f, _orchestrator.GalaxyConfig.UniverseAge, v => { _orchestrator.GalaxyConfig.UniverseAge = v; UpdateVisuals(); }));
            conditions.Add(CreateHorizontalSlider("Resource Abundance", 0.1f, 3.0f, _orchestrator.GalaxyConfig.Abundance, v => _orchestrator.GalaxyConfig.Abundance = v));
            conditions.Add(CreateHorizontalSlider("Habitability", 0.1f, 2.0f, _orchestrator.GalaxyConfig.LifeSupportingPlanetaryAbundance, v => _orchestrator.GalaxyConfig.LifeSupportingPlanetaryAbundance = v));
            conditions.Add(CreateHorizontalSlider("Opponents", 0, MAXIMUM_OPPONENTS, _orchestrator.GalaxyConfig.OpponentCount, v => _orchestrator.GalaxyConfig.OpponentCount = (int)v));

            sideBar.Add(conditions);
            root.Add(sideBar);

            // --- MAIN CONTENT (2/3): SCALE, MORPHOLOGY & VIEWPORT ---
            var mainContent = new VisualElement { style = { flexGrow = 1, paddingLeft = 20, paddingRight = 20, paddingTop = 20 } };

            mainContent.Add(CreateHeader("GALAXY SCALE"));
            _scaleButtonContainer = new VisualElement { style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap } };
            RenderScaleButtons();
            mainContent.Add(_scaleButtonContainer);

            mainContent.Add(CreateHeader("MORPHOLOGY"));
            _morphologyButtonContainer = new VisualElement { style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap, marginBottom = 10 } };
            RenderMorphologyButtons();
            mainContent.Add(_morphologyButtonContainer);

            // Interactive Viewport
            var viewportContainer = new VisualElement { style = { flexGrow = 1, alignItems = Align.Center, justifyContent = Justify.Center } };
            var squareViewport = new VisualElement
            {
                style = {
                width = Length.Percent(90), height = Length.Percent(90), backgroundColor = new Color(0.01f, 0.01f, 0.03f),
                borderTopWidth = 2, borderBottomWidth = 2, borderLeftWidth = 2, borderRightWidth = 2,
                borderTopColor = Color.cyan, borderBottomColor = Color.cyan, borderLeftColor = Color.cyan, borderRightColor = Color.cyan,
                overflow = Overflow.Hidden // Crucial to keep stars inside the box
            }
            };

            _galaxyPreviewContainer = new VisualElement { style = { position = Position.Absolute, width = 1000, height = 1000 } };
            squareViewport.Add(_galaxyPreviewContainer);

            var hint = new Label("STOCHASTIC VISUALIZATION // PAN & ZOOM ACTIVE")
            {
                style = { position = Position.Absolute, bottom = 10, left = 10, fontSize = 9, color = new Color(0, 1, 1, 0.4f) }
            };
            squareViewport.Add(hint);

            _starCountLabel = new Label("Est. Star Count: 500") { style = { color = Color.gray, position = Position.Absolute, top = 10, right = 10 } };
            squareViewport.Add(_starCountLabel);

            squareViewport.RegisterCallback<PointerMoveEvent>(OnPreviewDrag);
            squareViewport.RegisterCallback<WheelEvent>(OnPreviewZoom);

            viewportContainer.Add(squareViewport);
            mainContent.Add(viewportContainer);

            mainContent.Add(new Button(() => _orchestrator.AdvanceToRace()) // We advance to Race phase next
            {
                text = "SEQUENCING GENOME >>",
                style = { height = 50, marginTop = 10, marginBottom = 20, backgroundColor = new Color(0, 0.4f, 0.2f), color = Color.white, unityFontStyleAndWeight = FontStyle.Bold }
            });

            root.Add(mainContent);

            UpdateVisuals(); // Initial Render
            return root;
        }

        private VisualElement CreateHorizontalSlider(string v1, float v2, float v3, object lifeSupportingPlanetaryAbundance, Action<float> value)
        {
            throw new NotImplementedException();
        }

        private void UpdateVisuals()
        {
            if (_galaxyPreviewContainer == null) return;
            _galaxyPreviewContainer.Clear();

            uint seed = _orchestrator.GalaxyConfig.MasterSeed;
            float ageSpread = Mathf.Lerp(0.6f, 2.2f, _orchestrator.GalaxyConfig.UniverseAge / 15.0f);

            // Cap visual nodes so UI Builder doesn't lag out on "Epic" sizes
            int points = Mathf.Min(_visualStarCount, 800);

            for (int i = 0; i < points; i++)
            {
                uint starSeed = GalaxyMath.GetStarSeed((int)seed, i);
                Vector2 pos = GenerateStarPosition(i, _orchestrator.GalaxyConfig.Morphology.ToString(), starSeed) * ageSpread;

                var dot = new VisualElement
                {
                    style = {
                    position = Position.Absolute,
                    left = (pos.x * 300) + 400, // Offset to center of container
                    top = (pos.y * 300) + 400,
                    width = 2, height = 2,
                    backgroundColor = GalaxyMath.GetStarColor(starSeed)
                }
                };
                _galaxyPreviewContainer.Add(dot);
            }
        }

        private Vector2 GenerateStarPosition(int index, string shape, uint seed)
        {
            float r1 = GalaxyMath.Range(seed, 1, 0f, 1f);
            float theta = GalaxyMath.Range(seed, 2, 0f, Mathf.PI * 2);

            switch (shape)
            {
                case "Spiral":
                    float spiralTheta = (r1 * 10) + ((index % 2 == 0) ? 0 : Mathf.PI);
                    return new Vector2(r1 * Mathf.Cos(spiralTheta), r1 * Mathf.Sin(spiralTheta)) + new Vector2(UnityEngine.Random.value * 0.1f, UnityEngine.Random.value * 0.1f);

                case "Elliptical":
                    // Gaussian-like concentration in center, stretched horizontally
                    float eR = r1 * r1;
                    return new Vector2(eR * Mathf.Cos(theta) * 1.5f, eR * Mathf.Sin(theta) * 0.8f);

                case "Ring":
                    float ringR = 0.7f + (r1 * 0.3f);
                    return new Vector2(ringR * Mathf.Cos(theta), ringR * Mathf.Sin(theta));

                case "Cluster":
                    // Extremely tight dense ball
                    float cR = r1 * r1 * r1 * 0.5f;
                    return new Vector2(cR * Mathf.Cos(theta), cR * Mathf.Sin(theta));

                case "Irregular":
                default:
                    // Randomly scattered clusters
                    float clumpX = GalaxyMath.Range(seed % 5, 1, -0.6f, 0.6f);
                    float clumpY = GalaxyMath.Range(seed % 5, 2, -0.6f, 0.6f);
                    return new Vector2(r1 * Mathf.Cos(theta) + clumpX, r1 * Mathf.Sin(theta) + clumpY) * 0.7f;
            }
        }

        private void OnPreviewDrag(PointerMoveEvent e)
        {
            if ((e.pressedButtons & 1) != 0)
            {
                _galaxyPreviewContainer.style.left = _galaxyPreviewContainer.resolvedStyle.left + e.deltaPosition.x;
                _galaxyPreviewContainer.style.top = _galaxyPreviewContainer.resolvedStyle.top + e.deltaPosition.y;
            }
        }

        private void OnPreviewZoom(WheelEvent e)
        {
            _previewZoom = Mathf.Clamp(_previewZoom - e.delta.y * 0.05f, 0.2f, 5f);
            _galaxyPreviewContainer.style.scale = new Scale(new Vector3(_previewZoom, _previewZoom, 1));
        }

        private VisualElement CreateHorizontalSlider(string label, float min, float max, float val, Action<float> onValChange)
        {
            var group = new VisualElement { style = { marginBottom = 10 } };
            var lbl = new Label($"{label}: {val:F1}") { style = { fontSize = 10, color = Color.white } };
            var slider = new Slider(min, max) { value = val, direction = SliderDirection.Horizontal };
            slider.RegisterValueChangedCallback(evt =>
            {
                lbl.text = $"{label}: {evt.newValue:F1}";
                onValChange(evt.newValue);
            });
            group.Add(lbl);
            group.Add(slider);
            return group;
        }

        private void RenderScaleButtons()
        {
            _scaleButtonContainer.Clear();
            string[] scales = { "Tiny", "Small", "Medium", "Large", "Huge", "Epic", "Infinite" };
            int[] starEstimates = { 10, 100, 500, 2000, 10000, 50000, 1000000 };

            for (int i = 0; i < scales.Length; i++)
            {
                int idx = i;
                string scaleName = scales[i];
                var btn = new Button(() =>
                {
                    _orchestrator.GalaxyConfig.ScaleIndex = idx;
                    ClickedScaleButton = scaleName;
                    _visualStarCount = starEstimates[idx];
                    _starCountLabel.text = $"Est. Star Count: {_visualStarCount}";
                    RenderScaleButtons();
                    UpdateVisuals(); // Update density
                })
                { text = scaleName, style = { flexGrow = 1, height = 30, marginTop = 2, marginRight = 2, marginLeft = 2, marginBottom = 2 } };

                if (scaleName == "Infinite") { btn.SetEnabled(false); btn.text = "🔒 INFINITE"; }

                if (ClickedScaleButton == scaleName)
                {
                    btn.style.backgroundColor = _highlightColor;
                    btn.style.color = Color.white;
                }
                _scaleButtonContainer.Add(btn);
            }
        }

        private void RenderMorphologyButtons()
        {
            _morphologyButtonContainer.Clear();
            foreach (GalaxyMorphology morph in Enum.GetValues(typeof(GalaxyMorphology)))
            {
                var btn = new Button(() =>
                {
                    _orchestrator.GalaxyConfig.Morphology = morph;
                    RenderMorphologyButtons();
                    UpdateVisuals(); // Instantly update preview shape!
                })
                { text = morph.ToString().ToUpper(), style = { flexGrow = 1, marginTop = 2, marginRight = 2, marginLeft = 2, marginBottom = 2, height = 30 } };

                if (_orchestrator.GalaxyConfig.Morphology == morph)
                {
                    btn.style.backgroundColor = _highlightColor;
                    btn.style.color = Color.white;
                }
                _morphologyButtonContainer.Add(btn);
            }
        }

        private VisualElement CreateHeader(string text) => new Label(text) { style = { marginTop = 10, marginBottom = 5, color = Color.gray, fontSize = 10, unityFontStyleAndWeight = FontStyle.Bold } };
        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));

#if UNITY_EDITOR
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
#endif

        public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
    }
}