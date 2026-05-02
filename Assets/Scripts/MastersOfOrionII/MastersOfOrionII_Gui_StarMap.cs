using Assets.Scripts.MastersOfOrionII.Math;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.MastersOfOrionII
{
    public class MastersOfOrionII_Gui_StarSystem : IGuiProvider
    {
        public string Title => $"SYSTEM SCAN: {_currentStar.Class}";

        private int _systemSeed;
        private Star _currentStar;
        private List<Planet> _planets = new List<Planet>();
        private int _selectedPlanetIndex = -1;

        private VisualElement _orbitContainer;
        private VisualElement _detailsPanel;
        private MastersOfOrionII_Game _game;
        private GuiContext _lastCtx;

        private const float AU_SCALE = 80f; // Zoomed in slightly for better terminal fit

        public MastersOfOrionII_Gui_StarSystem()
        {
        }

        public void Initialize(int seed)
        {
            _systemSeed = seed;
            // Deterministic generation from DataModels.cs
            _currentStar = SystemFactory.GenerateStar(seed);
            _planets = SystemFactory.GeneratePlanets(_currentStar);
            _selectedPlanetIndex = -1;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;
            _game = UnityEngine.Object.FindAnyObjectByType<MastersOfOrionII_Game>();

            var builder = new GraphicalUserInterfaceBuilder("StarSystem_Root")
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                .WithPercentSize(100, 100)
                .WithBackgroundColor(new Color(0, 0.01f, 0.02f));

            var root = builder.Build();

            // --- LEFT PANEL: THE TACTICAL ORBITAL MAP ---
            var visPanel = new VisualElement
            {
                name = "OrbitalViewport",
                style = {
                flexGrow = 1,
                overflow = Overflow.Hidden,
                backgroundColor = Color.black,
                borderRightWidth = 2,
                borderRightColor = Color.cyan
            }
            };

            // Procedural Grid Overlay for that "Vector Display" feel
            visPanel.generateVisualContent += DrawSystemGrid;

            // Add the Goldilocks Zone Underlay
            visPanel.Add(CreateGoldilocksZone(_currentStar));

            // The Central Star
            float sunSize = 50 + (_currentStar.Mass * 15);
            var sun = new VisualElement
            {
                style = {
                width = sunSize, height = sunSize,
                backgroundColor = GetStarColor(_currentStar.Class),
                position = Position.Absolute, left = Length.Percent(50), top = Length.Percent(50),
                marginLeft = -sunSize / 2, marginTop = -sunSize / 2,
                borderTopLeftRadius = sunSize, borderTopRightRadius = sunSize,
                borderBottomLeftRadius = sunSize, borderBottomRightRadius = sunSize,
                borderLeftWidth = 2, borderRightWidth = 2, borderTopWidth = 2, borderBottomWidth = 2,
                borderLeftColor = new Color(1, 1, 1, 0.2f)
            }
            };
            sun.RegisterCallback<ClickEvent>(evt => SelectObject(-1));
            visPanel.Add(sun);

            // Orbits Layer
            _orbitContainer = new VisualElement { style = { position = Position.Absolute, width = Length.Percent(100), height = Length.Percent(100) } };
            visPanel.Add(_orbitContainer);

            visPanel.RegisterCallback<GeometryChangedEvent>(evt => RenderOrbits());
            root.Add(visPanel);

            // --- RIGHT PANEL: THE SCANNER READOUT ---
            _detailsPanel = new VisualElement
            {
                name = "ScannerReadout",
                style = {
                width = 350,
                backgroundColor = new Color(0.05f, 0.07f, 0.1f),
                paddingTop = 20, paddingBottom = 20, paddingLeft = 20, paddingRight = 20,
            }
            };
            root.Add(_detailsPanel);

            SelectObject(-1);
            return root;
        }

        private void RenderOrbits()
        {
            _orbitContainer.Clear();
            float centerX = _orbitContainer.resolvedStyle.width / 2f;
            float centerY = _orbitContainer.resolvedStyle.height / 2f;
            if (centerX < 1) return;

            for (int i = 0; i < _planets.Count; i++)
            {
                Planet p = _planets[i];
                float distPx = p.SemiMajorAxisAU * AU_SCALE;
                bool isSelected = _selectedPlanetIndex == i;

                // Draw Orbit Ring
                var ring = new VisualElement
                {
                    style = {
                    position = Position.Absolute,
                    width = distPx * 2, height = distPx * 2,
                    left = centerX - distPx, top = centerY - distPx,
                    borderLeftWidth = 1, borderRightWidth = 1, borderTopWidth = 1, borderBottomWidth = 1,
                    borderLeftColor = new Color(0, 1, 1, 0.1f), borderRightColor = new Color(0, 1, 1, 0.1f),
                    borderTopColor = new Color(0, 1, 1, 0.1f), borderBottomColor = new Color(0, 1, 1, 0.1f),
                    borderTopLeftRadius = distPx, borderTopRightRadius = distPx,
                    borderBottomLeftRadius = distPx, borderBottomRightRadius = distPx
                }
                };
                _orbitContainer.Add(ring);

                // Calculate Planet Position (Deterministic from Seed)
                float angle = GalaxyMath.Range(p.Seed, 99, 0f, 360f) * Mathf.Deg2Rad;
                float px = centerX + Mathf.Cos(angle) * distPx;
                float py = centerY + Mathf.Sin(angle) * distPx;

                float size = isSelected ? 24 : 16;
                var planetBtn = new Button(() => SelectObject(p.SystemIndex))
                {
                    style = {
                    position = Position.Absolute,
                    left = px - (size / 2), top = py - (size / 2),
                    width = size, height = size,
                    backgroundColor = GetPlanetColor(p.Type),
                    borderTopLeftRadius = size, borderTopRightRadius = size,
                    borderBottomLeftRadius = size, borderBottomRightRadius = size,
                    borderLeftWidth = isSelected ? 2 : 0, borderLeftColor = Color.white
                }
                };
                _orbitContainer.Add(planetBtn);
            }
        }

        private void SelectObject(int index)
        {
            _selectedPlanetIndex = index;
            RenderOrbits();
            _detailsPanel.Clear();

            if (index == -1)
            {
                // STAR SCANNER VIEW
                _detailsPanel.Add(new Label("PRIMARY STAR SCAN") { style = { color = Color.cyan, fontSize = 20, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 15 } });
                _detailsPanel.Add(CreateStatRow("Spectral Class:", _currentStar.Class.ToString()));
                _detailsPanel.Add(CreateStatRow("Luminosity:", $"{_currentStar.Luminosity:F2} Sols"));
                _detailsPanel.Add(CreateStatRow("System Mass:", $"{_currentStar.Mass:F2} M⊙"));
                _detailsPanel.Add(CreateStatRow("Planetary Bodies:", _planets.Count.ToString()));
            }
            else
            {
                // PLANET SCANNER VIEW (Injecting the specialized PlanetView)
                Planet p = _planets[index];
                ColonyData colony = _game.Data.GetColony(p.Id);
                IntelLevel intel = _game.Data.GetIntel(p.Id);

                var planetView = new MastersOfOrionII_Gui_PlanetView();
              //  planetView.Initialize(_systemSeed, p, colony, intel);
                _detailsPanel.Add(planetView.CreateGui(null));
            }
        }

        private void DrawSystemGrid(MeshGenerationContext mgc)
        {
            var painter = mgc.painter2D;
            painter.strokeColor = new Color(0, 1, 1, 0.02f);
            for (float i = 0; i < 2000; i += 50)
            {
                painter.BeginPath(); painter.MoveTo(new Vector2(i, 0)); painter.LineTo(new Vector2(i, 2000)); painter.Stroke();
                painter.BeginPath(); painter.MoveTo(new Vector2(0, i)); painter.LineTo(new Vector2(2000, i)); painter.Stroke();
            }
        }

        private VisualElement CreateGoldilocksZone(Star star)
        {
            float centerDistAU = Mathf.Sqrt(star.Luminosity);
            float widthAU = 0.4f * centerDistAU;
            float pxRadius = (centerDistAU - widthAU / 2) * AU_SCALE;
            float pxThickness = widthAU * AU_SCALE;

            return new VisualElement
            {
                style = {
                position = Position.Absolute,
                width = (pxRadius + pxThickness) * 2, height = (pxRadius + pxThickness) * 2,
                left = Length.Percent(50), top = Length.Percent(50),
                marginLeft = -(pxRadius + pxThickness), marginTop = -(pxRadius + pxThickness),
                borderLeftWidth = pxThickness, borderRightWidth = pxThickness,
                borderTopWidth = pxThickness, borderBottomWidth = pxThickness,
                borderLeftColor = new Color(0, 1, 0.4f, 0.05f),
                borderTopLeftRadius = pxRadius + pxThickness, borderBottomRightRadius = pxRadius + pxThickness
            }
            };
        }

        private VisualElement CreateStatRow(string k, string v)
        {
            var row = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween, marginBottom = 5 } };
            row.Add(new Label(k) { style = { color = Color.gray } });
            row.Add(new Label(v) { style = { color = Color.white, unityFontStyleAndWeight = FontStyle.Bold } });
            return row;
        }

        private Color GetStarColor(StarSpectralClass sc)
        {
            switch (sc)
            {
                case StarSpectralClass.O: return new Color(0.6f, 0.8f, 1f);
                case StarSpectralClass.M: return new Color(1f, 0.4f, 0.4f);
                case StarSpectralClass.G: return new Color(1f, 0.9f, 0.4f);
                default: return Color.white;
            }
        }

        private Color GetPlanetColor(PlanetType type)
        {
            switch (type)
            {
                case PlanetType.GasGiant: return new Color(0.8f, 0.6f, 0.4f);
                case PlanetType.Terrestrial: return new Color(0.2f, 0.7f, 0.3f);
                case PlanetType.Oceanic: return Color.blue;
                case PlanetType.Molten: return Color.red;
                default: return Color.gray;
            }
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));

#if UNITY_EDITOR
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
#endif
        public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
    }
}