using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

public class DockingManeuversTab : IShipTabBuilder
{
    public string TabName => "Docking";
    public string TabIcon => "⚓";
    public string Title => TabName;

    public VisualElement CreateGui(GuiContext ctx)
    {
        // The Root Container
        var rootBuilder = new ForgeContainerBuilder(TabName)
            .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
            .WithBackgroundColor(new Color(0.05f, 0.05f, 0.07f));

        // --- TOP: Docking Interlock (Container + Forge Components) ---
        rootBuilder.AddChild(new ForgeContainerBuilder("ClampControl")
            .WithPadding(15)
            .WithBackgroundColor(new Color(0.12f, 0.12f, 0.15f))
            .WithBorderColor(Color.blue)
            .WithBorderWidth(0, 0, 2, 0)
            .AddChild(new ForgeButtonBuilder("ENGAGE DOCKING CLAMPS")
                .WithBackgroundColor(new Color(0.1f, 0.2f, 0.6f))
                .WithHeight(40)
                .WithFontStyle(FontStyle.Bold)
                .OnClick(() => { /* Interlock Logic */ }))
            .AddChild(new ForgeLabelBuilder("Station Interlock: READY")
                .WithColor(Color.cyan)
                .WithFontSize(11)
                .WithMarginTop(8))
        );

        // --- CENTER: Visual Guidance (Horizontal Container) ---
        var cameraRow = new ForgeContainerBuilder("CameraRow")
            .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Stretch)
            .WithMarginTop(10)
            .WithFlexGrow(1);

        // Port Cam
        cameraRow.AddChild(new ForgeContainerBuilder("CamLeft")
            .WithWidth(new StyleLength(Length.Percent(30)))
            .WithBackgroundColor(Color.black)
            .WithBorderColor(new Color(0.3f, 0.3f, 0.3f))
            .WithBorderWidth(1)
            .AddChild(new ForgeLabelBuilder("45° PORT").WithColor(Color.gray).WithFontSize(9).WithMargin(5, 5)));

        // Center Feed / Path Projection
        cameraRow.AddChild(new ForgeContainerBuilder("CenterPath")
            .WithFlexGrow(1)
            .WithMargin(10)
            .WithBackgroundColor(Color.black)
            .WithBorderColor(Color.cyan)
            .WithBorderWidth(1)
            .AddChild(new ForgeLabelBuilder("PATH PROJECTION").WithColor(Color.cyan).WithBold().WithFontSize(10).WithMargin(5, 5)));

        // Starboard Cam
        cameraRow.AddChild(new ForgeContainerBuilder("CamRight")
            .WithWidth(new StyleLength(Length.Percent(30)))
            .WithBackgroundColor(Color.black)
            .WithBorderColor(new Color(0.3f, 0.3f, 0.3f))
            .WithBorderWidth(1)
            .AddChild(new ForgeLabelBuilder("45° STBD").WithColor(Color.gray).WithFontSize(9).WithMargin(5, 5)));

        rootBuilder.AddChild(cameraRow);

        return rootBuilder.Build();
    }

    // --- INTERFACE IMPLEMENTATION ---

    public Action<VisualElement> GetGuiBuilder()
    {
        return root => root.Add(CreateGui(new GuiContext()));
    }

    public void ToUIDocument(string assetPath)
    {
        // Logic for baking to UXML if needed
    }

    public void FromUIDocument(string assetPath)
    {
        // Logic for loading from baked UXML
    }
}