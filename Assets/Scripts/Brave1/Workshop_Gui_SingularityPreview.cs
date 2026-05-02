using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Showcase;

namespace Workshop.UI_And_Tools.Forge.Builders.Staging
{
    public class Workshop_Gui_SingularityPreview : IGuiProvider
    {
        public string Title => "THE SINGULARITY WORKSHOP PREVIEW";

        private VisualElement _rootContainer;
        private GuiContext _context;

        // --- ADDED: The Router Dependency ---
        private readonly IGuiRouter _router;

        // Constructor injection (defaulting to null so Unity's Reflection/Activator doesn't crash)
        public Workshop_Gui_SingularityPreview(IGuiRouter router = null)
        {
            _router = router;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _context = ctx;
            _rootContainer = new VisualElement { style = { flexGrow = 1, backgroundColor = Color.black } };

            // Start by loading the Interactive Boot Sequence
            LoadBootSequence();

            return _rootContainer;
        }

        private void LoadBootSequence()
        {
            _rootContainer.Clear();
            var bootSequence = new InteractiveBootSequence(OnSequenceComplete);
            _rootContainer.Add(bootSequence.CreateGui(_context));
        }

        private void OnSequenceComplete()
        {
            _rootContainer.Clear();

            // --- THE FIX: Pass the router down to the Lobby! ---
            var lobby = new Showcase_SingularityLobby(_router);

            _rootContainer.Add(lobby.CreateGui(_context));
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}