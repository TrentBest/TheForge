using TheSingularityWorkshop.Builders.GuiBuilders;

using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using UnityEngine;
using UnityEngine.UIElements;

public class FlightControlsTab : IShipTabBuilder
{
    public string TabName => "Flight Controls";
    public string TabIcon => "🚀";

    public string Title => TabName;

    public VisualElement CreateGui(GuiContext ctx)
    {
        var root = new GraphicalUserInterfaceBuilder(TabName)
            .WithEditorMode(true)
            .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch);

        // LEFT: Engine Magnitude Sliders (0-10)
        var thrustControls = new GraphicalUserInterfaceBuilder("ThrustControls")
            .WithSize(220, 0)
            .WithPadding(10)
            .WithTitle("ENGINE MAGNITUDE");

        for (int i = 1; i <= 4; i++)
        {
            thrustControls.AddIntSliderData($"ENG {i}", 0, 10, 0, val => {
                // Magnitude 8+ triggers exponential G-force lethality
            });
        }

        thrustControls.AddChild(new Label("G-Force Safety: NOMINAL")
        {
            style = { color = Color.green, marginTop = 10 }
        });

        // CENTER: 3D Tactical Radar Viewport
        var radarViewport = new GraphicalUserInterfaceBuilder("RadarView")
            .WithAutoGrow()
            .WithBackgroundColor(Color.black)
            .WithBorderWidth(1)
            .WithBorderColor(Color.green)
            .WithTitle("TACTICAL RADAR (3D)");

        // RIGHT: 26-Vector Matrix & Perspective Snaps
        var perspectivePanel = new GraphicalUserInterfaceBuilder("Perspectives")
            .WithSize(180, 0)
            .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
            .WithTitle("NAV-CUBE");

        perspectivePanel.WithPanel("QuickViews").WithFlexWrap(Wrap.Wrap);
        foreach (var view in new[] { "FRNT", "REAR", "LEFT", "RGHT", "TOP", "BTM" })
        {
            perspectivePanel.AddChild(new Button(() => { }) { text = view, style = { width = 50, height = 50 } });
        }

        return root.AddChild(thrustControls)
                   .AddChild(radarViewport)
                   .AddChild(perspectivePanel)
                   .Build();
    }

    public void FromUIDocument(string assetPath)
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
}