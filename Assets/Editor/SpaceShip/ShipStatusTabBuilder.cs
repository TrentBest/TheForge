using TheSingularityWorkshop.Builders.GuiBuilders;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using UnityEngine.UIElements;

public class ShipStatusTabBuilder : IShipTabBuilder
{
    public string TabName => "Status";
    public string TabIcon => "📊";

    public string Title => TabName;

    public VisualElement CreateGui(GuiContext ctx)
    {
        var builder = new GraphicalUserInterfaceBuilder(TabName)
            .WithTitle("SYSTEM DIAGNOSTICS")
            .WithScrollable(true, ScrollViewMode.VerticalAndHorizontal); // Enable multi-directional scrolling

        // Mocking up large diagnostic panels
        builder.WithPanel("ReactorCore").WithSize(400, 300).WithBorderWidth(1)
               .AddChild(new Label("REACTOR CORE: STABLE (98%)"))
               .AddChild(new Label("Fuel Pressure: 105 PSI [GREEN]"))
               .EndPanel();

        builder.WithPanel("LifeSupport").WithSize(400, 300).WithBorderWidth(1).WithMarginLeft(10)
               .AddChild(new Label("O2 LEVELS: 21%"))
               .AddChild(new Label("CO2 SCRUBBERS: ACTIVE"))
               .EndPanel();

        builder.WithPanel("GravityGen").WithSize(400, 300).WithBorderWidth(1).WithMarginTop(10)
               .AddChild(new Label("GRAVITY FIELD: 1.0G"))
               .EndPanel();

        return builder.Build();
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