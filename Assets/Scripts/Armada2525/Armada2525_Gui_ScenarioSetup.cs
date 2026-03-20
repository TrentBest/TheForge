using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheSingularityWorkshop.Armada2525
{
    public class Armada2525_Gui_ScenarioSetup : IGuiProvider
    {
        public string Title => "SCENARIO SETUP";
        private Action _onLocalPlayerClicked;
        private Action _onBackClicked;

        // REQUIRED BY ROUTER
        public Armada2525_Gui_ScenarioSetup(Action onLocalPlayerClicked = null, Action onBackClicked = null)
        {
            _onLocalPlayerClicked = onLocalPlayerClicked;
            _onBackClicked = onBackClicked;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            return new GraphicalUserInterfaceBuilder("ScenarioPanel")
                .WithPadding(40).WithWidth(500)
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.15f, 0.9f))
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)

                .AddChild(new Label("SELECT GAME MODE") { style = { color = Color.yellow, fontSize = 24, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 20 } })

                .AddChild(new GraphicalUserInterfaceBuilder("ModeButtons")
                    .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                    .WithWidth(400)
                    .AddButton("LOCAL GALAXY", () => _onLocalPlayerClicked?.Invoke())
                    .AddButton("MULTIPLAYER (HOST)", () => Debug.Log("Host Multi clicked"))
                    .Build())

                .AddSeparator()
                .AddButton("<< BACK TO MENU", () => _onBackClicked?.Invoke())
                .Build();
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}