using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.Asteroids
{
    public class Asteroids_Playable_Alpha : IGuiProvider
    {
        public string Title => "ASTEROIDS: ALPHA CLIENT";
        private GuiContext _lastCtx;

        // The parameterless constructor ensures this shows up in your Editor!
        public Asteroids_Playable_Alpha() { }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            // Initialize the Flow Router for the client state
            var router = new GuiFlowRouterBuilder("Asteroids_Router", "MainMenu")
                .AddRoute("MainMenu", r => new Asteroids_Gui_MainMenu(r))
                .AddRoute("HangarBay", r => new Asteroids_Gui_Hangar(r))
                .AddRoute("InGame", r => new Asteroids_Gui_InGame(r))
                .AddRoute("Game-Over", r => new Asteroids_Gui_GameOver(r));

            // Wrap the router in a root container to provide a persistent "Deep Space/Void" backdrop
            var root = new ForgeContainerBuilder("Asteroids_Client_Root")
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.02f, 0.01f, 0.03f, 1.0f)) // Deep Void background
                .AddChild(router);

            return root.CreateGui(ctx);
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));

        // --- PREPROCESSOR SHIELDING FIXED ---
#if UNITY_EDITOR
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
#else
        public void ToUIDocument(string assetPath) { }
#endif

        public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
    }
}