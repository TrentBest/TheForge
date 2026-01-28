using Assets.Scripts;
using Assets.Scripts.Builders.GuiBuilders;
using UnityEngine;
using UnityEngine.UIElements;

public class DockingManeuversTab : IShipTabBuilder
{
    public string TabName => "Docking";
    public string TabIcon => "⚓";

    public VisualElement CreateGui(GuiContext ctx)
    {
        var builder = new GraphicalUserInterfaceBuilder(TabName)
            .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch);

        // TOP: Docking Interlock
        builder.WithPanel("ClampControl")
               .WithPadding(15)
               .WithBackgroundColor(new Color(0.15f, 0.15f, 0.15f))
               .AddChild(new Button(() => { })
               {
                   text = "ENGAGE DOCKING CLAMPS",
                   style = { height = 40, backgroundColor = Color.blue }
               })
               .AddChild(new Label("Station Interlock: READY") { style = { color = Color.cyan, fontSize = 10 } })
               .EndPanel();

        // CENTER: Visual Guidance
        var cameraRow = new GraphicalUserInterfaceBuilder("CameraRow")
            .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center);

        cameraRow.WithPanel("CamLeft").WithSize(300, 200).WithBackgroundColor(Color.black).WithTitle("45° PORT").EndPanel();
        cameraRow.WithPanel("CenterPath").WithAutoGrow().WithBackgroundColor(Color.black).WithTitle("PATH PROJECTION").EndPanel();
        cameraRow.WithPanel("CamRight").WithSize(300, 200).WithBackgroundColor(Color.black).WithTitle("45° STBD").EndPanel();

        return builder.AddChild(cameraRow).Build();
    }
}