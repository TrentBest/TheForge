using TheSingularityWorkshop.Builders.GuiBuilders;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using UnityEngine;
using UnityEngine.UIElements;

public class Armada2525_Gui_Settings : IGuiProvider
{
    public string Title => "System Configuration";

    public VisualElement CreateGui(GuiContext ctx)
    {
        var game = Object.FindAnyObjectByType<Armada2525>();

        var builder = new GraphicalUserInterfaceBuilder("Settings_Root")
            .WithTitle("PROTOCOLS // CONFIGURATION")
            .WithBackgroundColor(new Color(0.1f, 0.1f, 0.1f))
            .WithPercentSize(100, 100)
            .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center);

        builder.AddChild(context =>
        {
            var panel = new GraphicalUserInterfaceBuilder("WarningPanel")
                .WithPadding(40)
                //.WithBorder(2, 2, 2, 2, Color.yellow)
                .WithBackgroundColor(new Color(0.2f, 0.2f, 0, 0.5f));

            panel.AddChild(c => new Label("SYSTEM ALERT")
            {
                style = {
                    fontSize = 30,
                    color = Color.yellow,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    alignSelf = Align.Center,
                    marginBottom = 20
                }
            });

            panel.AddChild(c => new Label("Configuration subsystems are currently hard-coded.\nAudio and Video protocols operating at default parameters.")
            {
                style = {
                    fontSize = 14,
                    color = Color.white,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    marginBottom = 40
                }
            });

            // The Return Button
            panel.AddChild(c =>
            {
                return new Button(() => game.SwitchGui("MainMenu"))
                {
                    text = "< RETURN TO COMMAND",
                    style = {
                        height = 50,
                        backgroundColor = new Color(0.3f, 0.3f, 0.3f),
                        color = Color.white,
                        fontSize = 14,
                        unityFontStyleAndWeight = FontStyle.Bold
                    }
                };
            });

            return panel.Build();
        });

        return builder.Build();
    }

    public void FromUIDocument(string assetPath)
    {
        throw new System.NotImplementedException();
    }

    public System.Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));

    public void ToUIDocument(string assetPath)
    {
        throw new System.NotImplementedException();
    }
}