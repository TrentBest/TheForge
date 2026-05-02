using System;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.Warlords
{
    public class Warlords_ShowcaseProvider : IGuiProvider
    {
        public string Title => "WARLORDS SHOWCASE";
        private GuiContext _lastCtx;

        public Warlords_ShowcaseProvider() { }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            var router = new GuiFlowRouterBuilder("Warlords_Router", "Intro")
                .AddRoute("Intro", r => new Warlords_Gui_Intro(r))
                // Pointing directly to your Playable Alpha router!
                .AddRoute("Interactive", r => new Warlords_Playable_Alpha());

            return router.CreateGui(ctx);
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}