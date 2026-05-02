using Assets.Scripts.GURPS;
using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Memory;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.GURPS
{
    public class GURPS_UniverseEditor : IGuiProvider
    {
        public string Title => "MULTIVERSE COSMOLOGY EDITOR";
        private GuiContext _lastCtx;
        private CRUD_Builder<GURPSUniverse> _crudInterface;
        private DataWarehouse _warehouse;

        public GURPS_UniverseEditor()
        {
            _warehouse = new DataWarehouse();
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            if (_warehouse == null) _warehouse = new DataWarehouse();
            DigitalGenericUniversalRolePlayingSystem.InitializeCoreSystem(_warehouse);

            _crudInterface = new CRUD_Builder<GURPSUniverse>(
                title: "MULTIVERSE REGISTRY",
                dataSource: () => DigitalGenericUniversalRolePlayingSystem.Universes.GetAll(),
                getDisplayName: (u) => $"[{u.UniverseSeed}] {u.Name}",
                getGroupCategory: (u) => u.IsRealWorld ? "REALITY" : "SIMULATED",
                buildEditorForm: (u) => BuildUniverseForm(u),
                onSave: (u) => DigitalGenericUniversalRolePlayingSystem.Universes.Register(u),
                getSubtitle: (u) => $"TL: {u.TechLevel} | {u.Physics}",
                onDelete: (u) => DigitalGenericUniversalRolePlayingSystem.Universes.GetAll().Remove(u)
            );

            _crudInterface.Style.AccentColor = new Color(0.4f, 0.4f, 0.9f);
            _crudInterface.Style.ListWidth = 220f;

            return _crudInterface.CreateGui(ctx);
        }

        private VisualElement BuildUniverseForm(GURPSUniverse uni)
        {
            var formRoot = new ForgeContainerBuilder().WithDirection(FlexDirection.Column).WithFlexGrow(1);
            var contentArea = new VisualElement { name = "ContentArea", style = { flexGrow = 1 } };

            var tabHeader = new ForgeContainerBuilder().WithDirection(FlexDirection.Row).WithMarginBottom(10)
                .AddChild(new ForgeButtonBuilder("COSMOLOGY", () => { contentArea.Clear(); contentArea.Add(new GURPS_Gui_CosmologyBuilder().CreateGui(uni)); }).WithFlexGrow(1))
                .AddChild(new ForgeButtonBuilder("LIBRARY", () => { contentArea.Clear(); contentArea.Add(new GURPS_Gui_BookSelector().CreateGui(uni)); }).WithFlexGrow(1));

            var root = formRoot.CreateGui(_lastCtx);
            root.Add(tabHeader.CreateGui(_lastCtx));
            root.Add(contentArea);

            contentArea.Add(new GURPS_Gui_CosmologyBuilder().CreateGui(uni));
            return root;
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
        public void ToUIDocument(string assetPath) { }
    }
}