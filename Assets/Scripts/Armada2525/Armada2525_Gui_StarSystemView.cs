using TheSingularityWorkshop.Armada2525;
using TheSingularityWorkshop.Armada2525.Math;
using TheSingularityWorkshop.Builders.GuiBuilders;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Armada2525_Gui_StarSystemView : IGuiProvider
{
    public string Title => "System Command";

    // --- State Data ---
    private int _systemSeed;
    private Star _currentStar;
    private List<Planet> _planets = new List<Planet>();
    private int _selectedPlanetIndex = -1; // -1 = Star selected

    // --- UI References ---
    private VisualElement _orbitContainer;
    private VisualElement _detailsPanel;
    private Armada2525 _game;

    // --- Configuration ---
    private const float AU_SCALE = 60f; // Pixels per AU (Adjusted for better screen fit)

    /// <summary>
    /// Initialize the view with a specific system seed.
    /// This regenerates the immutable physics data for the system.
    /// </summary>
    public void Initialize(int seed)
    {
        _systemSeed = seed;

        // 1. Generate the immutable data on the fly
        _currentStar = SystemFactory.GenerateStar(seed);
        _planets = SystemFactory.GeneratePlanets(_currentStar);

        // Reset selection
        _selectedPlanetIndex = -1;
    }

    public VisualElement CreateGui(GuiContext ctx)
    {
        _game = UnityEngine.Object.FindAnyObjectByType<Armada2525>();

        // 1. Setup Root Container
        var builder = new GraphicalUserInterfaceBuilder("StarSystem_Root")
            .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
            .WithPercentSize(100, 100)
            .WithBackgroundColor(Color.black);

        var root = builder.Build();

        // 2. LEFT PANEL: VISUALIZATION (Orbits & Sun)
        // This takes up 65% of the screen
        var visPanel = new VisualElement
        {
            style = {
                flexGrow = 0.65f,
                overflow = Overflow.Hidden,
                backgroundColor = new Color(0,0,0,0.95f),
                borderRightWidth = 1,
                borderRightColor = new Color(0.3f, 0.3f, 0.4f)
            }
        };

        // A. The "Goldilocks Zone" Underlay
        // (Visualized as a faint green ring/band behind planets)
        var goldilocks = CreateGoldilocksZone(_currentStar);
        visPanel.Add(goldilocks);

        // B. The Central Star
        var sunSize = 60 + (_currentStar.Mass * 10); // Scale sun slightly by mass
        var sun = new VisualElement
        {
            style = {
                width = sunSize, height = sunSize,
                backgroundColor = GetStarColor(_currentStar.Class),
                position = Position.Absolute, left = Length.Percent(50), top = Length.Percent(50),
                marginLeft = -sunSize/2, marginTop = -sunSize/2, // Center pivot
                borderTopLeftRadius = sunSize, borderTopRightRadius = sunSize,
                borderBottomLeftRadius = sunSize, borderBottomRightRadius = sunSize,
                //borderWidth = 4, borderColor = new Color(1,1,1,0.1f) // Glow effect
            }
        };

        // Clicking the sun deselects planets
        sun.RegisterCallback<ClickEvent>(evt => SelectObject(-1));
        visPanel.Add(sun);

        // C. The Orbits Container
        _orbitContainer = new VisualElement { style = { width = new StyleLength(StyleKeyword.Auto), height = new StyleLength(StyleKeyword.Auto), position = Position.Absolute, /*pickingMode = PickingMode.Ignore*/ } };
        visPanel.Add(_orbitContainer);

        // Wait for layout to calculate center, then render
        visPanel.RegisterCallback<GeometryChangedEvent>(evt => RenderOrbits());

        root.Add(visPanel);

        // 3. RIGHT PANEL: DETAILS (The "Inspector")
        // This takes up 35% of the screen and hosts the PlanetView
        _detailsPanel = new VisualElement
        {
            style = {
                width = new Length(35, LengthUnit.Percent),
                backgroundColor = new Color(0.1f, 0.12f, 0.15f),
                paddingTop = 0, paddingLeft = 0, paddingRight = 0
            }
        };

        root.Add(_detailsPanel);

        // Initial Selection (Select the Star)
        SelectObject(-1);

        return root;
    }

    private void RenderOrbits()
    {
        _orbitContainer.Clear();

        float centerX = _orbitContainer.resolvedStyle.width / 2f;
        float centerY = _orbitContainer.resolvedStyle.height / 2f;

        // Fallback if layout hasn't run yet
        if (centerX < 1) return;

        for (int i = 0; i < _planets.Count; i++)
        {
            Planet p = _planets[i];

            // 1. Calculate Orbit Distance
            // We clamp it slightly so distant planets don't disappear off-screen entirely
            float distPx = Mathf.Clamp(p.SemiMajorAxisAU * AU_SCALE, 50, centerX - 40);

            // 2. Draw Orbit Ring (The thin gray line)
            var ring = new VisualElement
            {
                style = {
                    position = Position.Absolute,
                    width = distPx * 2, height = distPx * 2,
                    left = centerX - distPx, top = centerY - distPx,
                    //borderWidth = 1, borderColor = new Color(1,1,1,0.08f), // Faint
                    borderTopLeftRadius = distPx, borderTopRightRadius = distPx,
                    borderBottomLeftRadius = distPx, borderBottomRightRadius = distPx,
                    //pickingMode = PickingMode.Ignore
                }
            };
            _orbitContainer.Add(ring);

            // 3. Draw Planet Icon
            IntelLevel intel = _game.Data.GetIntel(p.Id);

            float size = p.Type == PlanetType.GasGiant ? 20 : 12;
            if (p.Type == PlanetType.IceGiant) size = 16;

            Color planetColor = (intel == IntelLevel.Unknown) ? Color.gray : GetPlanetColor(p.Type);

            // Calculate Position on Orbit (Random angle based on seed so they aren't all in a line)
            float angle = GalaxyMath.Range(p.Seed, 99, 0, 360) * Mathf.Deg2Rad;
            float px = centerX + Mathf.Cos(angle) * distPx;
            float py = centerY + Mathf.Sin(angle) * distPx;

            // The clickable button representing the planet
            var planetBtn = new Button(() => SelectObject(p.SystemIndex))
            {
                tooltip = (intel == IntelLevel.Unknown) ? "Unexplored Signal" : p.Type.ToString(),
                style = {
                    position = Position.Absolute,
                    left = px - (size/2), top = py - (size/2),
                    width = size, height = size,
                    backgroundColor = planetColor,
                    borderTopLeftRadius = size, borderTopRightRadius = size,
                    borderBottomLeftRadius = size, borderBottomRightRadius = size,
                    //borderWidth = 1, borderColor = new Color(0,0,0,0.8f)
                }
            };

            // Selection Highlight Ring
            //if (_selectedPlanetIndex == i)
            //{
            //    planetBtn.style.borderWidth = 2;
            //    planetBtn.style.borderColor = Color.white;
            //}

            _orbitContainer.Add(planetBtn);
        }
    }

    private void SelectObject(int index)
    {
        _selectedPlanetIndex = index;

        // Re-render orbits to update the white selection ring
        RenderOrbits();

        // Clear the right-hand inspector panel
        _detailsPanel.Clear();

        if (index == -1)
        {
            // --- VIEW A: STAR DETAILS ---
            // If we selected the sun, show basic system stats
            var starHeader = new Label($"STAR: {_currentStar.Class}")
            {
                style = {
                    fontSize = 24, unityFontStyleAndWeight = FontStyle.Bold,
                    color = GetStarColor(_currentStar.Class), //marginAll = 20
                }
            };
            _detailsPanel.Add(starHeader);

            var stats = new VisualElement { style = { paddingLeft = 20 } };
            stats.Add(new Label($"Luminosity: {_currentStar.Luminosity:F2} Sols"));
            stats.Add(new Label($"Mass: {_currentStar.Mass:F2} Sols"));
            stats.Add(new Label($"Planet Count: {_planets.Count}"));
            _detailsPanel.Add(stats);
            return;
        }

        // --- VIEW B: PLANET DETAILS (Injection) ---
        Planet p = _planets[index];

        // Fetch Mutable Data from GameState
        // (Null if not colonized, Unknown if not surveyed)
        ColonyData colony = _game.Data.GetColony(p.Id);
        IntelLevel intel = _game.Data.GetIntel(p.Id);

        // Instantiate the Planet View
        var planetView = new Armada2525_Gui_PlanetView();

        // Inject Context
        planetView.Initialize(_systemSeed, p, colony, intel);

        // Render it into our panel
        _detailsPanel.Add(planetView.CreateGui(null));
    }

    // --- VISUAL HELPERS ---

    private VisualElement CreateGoldilocksZone(Star star)
    {
        // Simple approximation: Luminosity determines zone distance
        // L = 1 (Sun) -> Zone at 1 AU
        // L = 4 -> Zone at 2 AU
        float centerDistAU = Mathf.Sqrt(star.Luminosity);

        // Zone Width (wider for hotter stars)
        float widthAU = 0.4f * centerDistAU;

        // Convert to Pixels
        float pxRadius = centerDistAU * AU_SCALE;
        float pxWidth = widthAU * AU_SCALE;

        return new VisualElement
        {
            style = {
                position = Position.Absolute,
                width = (pxRadius + pxWidth) * 2,
                height = (pxRadius + pxWidth) * 2,
                left = Length.Percent(50), top = Length.Percent(50),
                marginLeft = -(pxRadius + pxWidth), marginTop = -(pxRadius + pxWidth),
                
                // Draw it as a thick border
                //borderWidth = pxWidth,
                //borderColor = new Color(0.2f, 1f, 0.4f, 0.15f), // Transparent Sci-Fi Green
                
                borderTopLeftRadius = pxRadius + pxWidth, borderTopRightRadius = pxRadius + pxWidth,
                borderBottomLeftRadius = pxRadius + pxWidth, borderBottomRightRadius = pxRadius + pxWidth,
                //pickingMode = PickingMode.Ignore
            }
        };
    }

    private Color GetStarColor(StarSpectralClass spectralClass)
    {
        switch (spectralClass)
        {
            case StarSpectralClass.O: return new Color(0.6f, 0.8f, 1f); // Blue
            case StarSpectralClass.B: return new Color(0.7f, 0.85f, 1f); // Blue-White
            case StarSpectralClass.A: return Color.white;
            case StarSpectralClass.F: return new Color(1f, 1f, 0.9f); // Yellow-White
            case StarSpectralClass.G: return new Color(1f, 0.9f, 0.4f); // Yellow (Sun)
            case StarSpectralClass.K: return new Color(1f, 0.7f, 0.2f); // Orange
            case StarSpectralClass.M: return new Color(1f, 0.4f, 0.4f); // Red
            default: return Color.white;
        }
    }

    private Color GetPlanetColor(PlanetType type)
    {
        switch (type)
        {
            case PlanetType.GasGiant: return new Color(0.85f, 0.7f, 0.5f); // Beige/Brown
            case PlanetType.IceGiant: return new Color(0.4f, 0.9f, 1f); // Cyan
            case PlanetType.Terrestrial: return new Color(0.3f, 0.7f, 0.3f); // Green/Blue mix
            case PlanetType.Oceanic: return new Color(0.2f, 0.4f, 0.9f); // Deep Blue
            case PlanetType.Molten: return new Color(1f, 0.3f, 0.1f); // Magma
            case PlanetType.Barren: return new Color(0.6f, 0.6f, 0.6f); // Grey
            case PlanetType.Desert: return new Color(0.9f, 0.8f, 0.4f); // Sand
            case PlanetType.Tundra: return new Color(0.8f, 0.9f, 0.95f); // White/Blue
            default: return Color.magenta; // Error pink
        }
    }

    // Required by Interface
    public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));

    public void ToUIDocument(string assetPath)
    {
        throw new NotImplementedException();
    }

    public void FromUIDocument(string assetPath)
    {
        throw new NotImplementedException();
    }
}