using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheSingularityWorkshop.Armada2525.GURPS
{
    public class Armada2525_Gui_GURPS_UniverseEditor : IGuiProvider
    {
        public string Title => "MULTIVERSE COSMOLOGY EDITOR";
        private GuiContext _lastCtx;
        private CRUD_Builder<GURPSUniverse> _crudInterface;

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;
            DigitalGenericUniversalRolePlayingSystem.InitializeCoreSystem(); //

            _crudInterface = new CRUD_Builder<GURPSUniverse>(
                title: "MULTIVERSE REGISTRY",
                dataSource: () => DigitalGenericUniversalRolePlayingSystem.Universes.GetAll(), //
                getDisplayName: (u) => $"[{u.UniverseSeed}] {u.Name}",
                getGroupCategory: (u) => u.IsRealWorld ? "REALITY" : "SIMULATED", //
                buildEditorForm: (u) => BuildUniverseForm(u),
                onSave: (u) => DigitalGenericUniversalRolePlayingSystem.Universes.Register(u), //
                getSubtitle: (u) => $"TL: {u.DefaultTechLevel} | {u.PhysicsProfile}",
                onDelete: (u) => DeleteUniverse(u)
            );

            return _crudInterface.CreateGui(ctx);
        }

        private void DeleteUniverse(GURPSUniverse u)
        {
            DigitalGenericUniversalRolePlayingSystem.Universes.GetAll().Remove(u);
        }

        private VisualElement BuildUniverseForm(GURPSUniverse uni)
        {
            var root = new VisualElement();

            // Create the Tabbed Panel (or a set of toggle buttons to swap views)
            var tabHeader = new VisualElement { style = { flexDirection = FlexDirection.Row, marginBottom = 10 } };
            var contentArea = new VisualElement { style = { flexGrow = 1 } };

            // --- Tab 1: Cosmology & Physics ---
            var cosmologyTab = BuildCosmologyTab(uni);

            // --- Tab 2: The Library (Books) ---
            var libraryTab = BuildLibraryTab(uni);

            // Switch logic: Show/Hide based on selection
            tabHeader.Add(new Button(() => { contentArea.Clear(); contentArea.Add(cosmologyTab); }) { text = "COSMOLOGY" });
            tabHeader.Add(new Button(() => { contentArea.Clear(); contentArea.Add(libraryTab); }) { text = "LIBRARY" });

            root.Add(tabHeader);
            root.Add(contentArea);

            // Default view
            contentArea.Add(cosmologyTab);

            return root;
        }

        private VisualElement BuildLibraryTab(GURPSUniverse uni)
        {
            return new Armada2525_Gui_GURPS_BookSelector().CreateGui(uni);
        }

        private VisualElement BuildCosmologyTab(GURPSUniverse uni)
        {
            return new Armada2525_Gui_GURPS_CosmologyBuilder().CreateGui(uni);
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
#if UNITY_EDITOR
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
#endif
    }
}