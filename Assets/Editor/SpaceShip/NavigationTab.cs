using TheSingularityWorkshop.Builders.GuiBuilders;

using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using UnityEngine;
using UnityEngine.UIElements;

public class NavigationTab : IShipTabBuilder
{
    public string TabName => "Navigation";
    public string TabIcon => "🗺️";

    public string Title => TabName;

    public VisualElement CreateGui(GuiContext ctx)
    {
        var sidebar = new GraphicalUserInterfaceBuilder("Destinations")
            .WithTitle("STEL-MAP REGISTRY")
            .AddChild(new Label("Plot Destination.."))
            .AddChild(new Button(() => { }) { text = "Sol System" })
            .AddChild(new Button(() => { }) { text = "Alpha Centauri" })
            .AddChild(new Button(() => { }) { text = "Canopus III" });

        var mainView = new GraphicalUserInterfaceBuilder("AstralChart")
            .WithAutoGrow()
            .WithBackgroundColor(Color.black)
            .WithTitle("LONG-RANGE STELLAR PROJECTION")
            .AddChild(new Label("Course Intercept: 0.00 LY") { style = { color = Color.cyan } })
            .AddChild(new Label("[PLACEHOLDER: STAR CHART VIEWPORT]")); // Will use AtomOrbitView logic

        return new SplitPanelBuilder(250)
            .WithSidebar(sidebar)
            .WithMain(mainView)
            .CreateGui(ctx);
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