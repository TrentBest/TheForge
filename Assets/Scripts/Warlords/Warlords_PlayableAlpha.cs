using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.Warlords
{
    public class Warlords_Playable_Alpha : IGuiProvider
    {
        public string Title => "WARLORDS: ALPHA CLIENT";
        private GuiContext _lastCtx;

        // The parameterless constructor ensures this shows up in your Editor!
        public Warlords_Playable_Alpha() { }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            var router = new GuiFlowRouterBuilder("Warlords_Router", "MainMenu")
                .AddRoute("MainMenu", r => new Warlords_Gui_MainMenu(r))
                .AddRoute("GameSetup", r => new Warlords_Gui_GameSetup(r))
                .AddRoute("InGame", r => new Warlords_Gui_InGame(r)); // Racing to the actual game

            return router.CreateGui(ctx);
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
#if UNITY_EDITOR
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
#endif
        public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
    }
}