#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.Unity
{
    public class Forge_Gui_LobbyRelayModule : IGuiProvider
    {
        public string Title => "RELAY & LOBBY";
        public VisualElement CreateGui(GuiContext context)
        {
            return new GraphicalUserInterfaceBuilder("Lobby_Stub")
                .WithPadding(20)
                .AddChild(new Label("MULTIPLAYER ROUTING") { style = { color = Color.cyan, fontSize = 20, unityFontStyleAndWeight = FontStyle.Bold } })
                .AddChild(new Label("Listening for region-specific relay endpoints...") { style = { color = Color.gray, marginTop = 10 } })
                .Build();
        }
        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void FromUIDocument(string assetPath) { }
        public void ToUIDocument(string assetPath) { }
    }
}
#endif