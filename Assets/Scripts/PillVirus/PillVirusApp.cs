using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.PillVirus
{
    public class PillVirus_App : IGuiProvider
    {
        public string Title => "CLINICAL TRIALS";
        private GuiContext _lastCtx;

        // The parameterless constructor ensures this shows up in your Editor safely!
        public PillVirus_App() { }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            // Instantiate the router and pass 'r' (the router itself) down to each view
            var router = new GuiFlowRouterBuilder("PillVirus_Router", "MainMenu")
                .AddRoute("MainMenu", r => new PillVirus_MainMenu(r))
                .AddRoute("Difficulty", r => new PillVirus_DifficultySelect(r))
                .AddRoute("Gameplay", r => new PillVirus_Gui_Arcade(r)); // Racing to the actual game

            return router.CreateGui(ctx);
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void FromUIDocument(string assetPath) { }
        public void ToUIDocument(string assetPath) { }
    }
}