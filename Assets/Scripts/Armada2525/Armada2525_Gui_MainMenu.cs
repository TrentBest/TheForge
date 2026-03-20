using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheSingularityWorkshop.Armada2525
{
    public class Armada2525_Gui_MainMenu : IGuiProvider
    {
        public string Title => "MAIN MENU";
        private Action _onGenesisClicked;

        // REQUIRED BY ROUTER
        public Armada2525_Gui_MainMenu(Action onGenesisClicked = null)
        {
            _onGenesisClicked = onGenesisClicked;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            return new GraphicalUserInterfaceBuilder("MainMenuPanel")
                .WithPadding(40).WithWidth(400)
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.15f, 0.9f))
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)

                .AddChild(new Label("ARMADA 2525") { style = { color = Color.cyan, fontSize = 32, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 30 } })

                // Routes back up to the Mediator!
                .AddButton("GENESIS (NEW GAME)", () => _onGenesisClicked?.Invoke())
                .AddButton("LOAD GAME", () => Debug.Log("Load Game clicked"))
                .AddButton("MULTIPLAYER SIGN IN", () => Debug.Log("Sign In clicked"))

                .AddSeparator()
                .AddButton("EXIT TO DESKTOP", () => Application.Quit())
                .Build();
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}