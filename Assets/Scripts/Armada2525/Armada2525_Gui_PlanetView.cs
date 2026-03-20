using TheSingularityWorkshop.Armada2525;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Armada2525_Gui_PlanetView : IGuiProvider
{
    public string Title => "Orbital Scan: " + _planet.Name;
    private Planet _planet;
    private List<Moon> _moons; // New addition for astronomy view

    // --- Orbital Dynamics ---
    // These should be calculated based on the planet's Seed and SemiMajorAxis
    public float AxialTilt => _planet.Seed % 45f;
    public float RotationPeriodHours => 12f + (_planet.Seed % 60f);

    public VisualElement CreateGui(GuiContext ctx)
    {
        var builder = new GraphicalUserInterfaceBuilder("OrbitalConsole")
            .WithBackgroundColor(new Color(0.01f, 0.02f, 0.05f))
            .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch);

        // LEFT SIDE: The 3D Render Placeholder (Generated Surface Mesh)
        builder.AddChild(c => new VisualElement
        {
            name = "OrbitalViewport",
            style = { flexGrow = 1, backgroundColor = Color.black, borderRightWidth = 2, borderRightColor = Color.cyan }
        });

        // RIGHT SIDE: Astronomical Data Readout
        builder.AddChild(c => {
            var readout = new VisualElement { style = { width = 300, paddingTop = 20, paddingBottom = 20, paddingLeft = 20, paddingRight = 20 } };
            readout.Add(new Label("ASTROMETRIC READOUT") { style = { unityFontStyleAndWeight = FontStyle.Bold, color = Color.cyan } });

            // Physical Specs from DataModels.cs
            readout.Add(CreateStatRow("Mass:", $"{_planet.RadiusEarth:F2} M⊕"));
            readout.Add(CreateStatRow("Gravity:", $"{_planet.GravityG:F2} G"));
            readout.Add(CreateStatRow("Year:", $"{_planet.OrbitalPeriodDays:F0} Days"));
            readout.Add(CreateStatRow("Day Cycle:", $"{RotationPeriodHours:F1} Hours"));
            readout.Add(CreateStatRow("Axial Tilt:", $"{AxialTilt:F1}°"));

            // Moons Section
            readout.Add(new Label("SATELLITES") { style = { marginTop = 20, color = Color.gray } });
            foreach (var moon in _moons) readout.Add(new Label($"- Moon {moon.Id}: {moon.Type}"));

            return readout;
        });

        return builder.Build();
    }

    private VisualElement CreateStatRow(string v1, string v2)
    {
        throw new NotImplementedException();
    }

    public Action<VisualElement> GetGuiBuilder()
    {
        throw new NotImplementedException();
    }

    public void ToUIDocument(string assetPath)
    {
        throw new NotImplementedException();
    }

    public void FromUIDocument(string assetPath)
    {
        throw new NotImplementedException();
    }

    internal void Initialize(int systemSeed, Planet p, ColonyData colony, IntelLevel intel)
    {
        throw new NotImplementedException();
    }
}