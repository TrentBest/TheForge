using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Corsair
{
    public class CorsairsInSpace_Playable_Alpha : IGuiProvider
    {
        public string Title => "CORSAIRS IN SPACE: ALPHA CLIENT";
        private GuiContext _lastCtx;

        public CorsairsInSpace_Playable_Alpha() { }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            // Establish the Router and map our modules
            var router = new GuiFlowRouterBuilder("Corsairs_Router", "MainMenu")
                .AddRoute("MainMenu", r => new CorsairsInSpace_Gui_MainMenu(r))
                .AddRoute("InGameHub", r => new CorsairsInSpace_Gui_InGameHub(r)) // <-- The new Hub
                .AddRoute("LairArchitect", r => new CorsairsInSpace_Gui_LairArchitect(r))
                .AddRoute("Shipyard", r => new CorsairsInSpace_Gui_Shipyard(r))
                .AddRoute("OpsManager", r => new CorsairsInSpace_Gui_OpsManager(r))
                .AddRoute("Navigation", r => new CorsairsInSpace_Gui_Navigation(r));

            return router.CreateGui(ctx);
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
#if UNITY_EDITOR
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
#endif
        public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
    }
}