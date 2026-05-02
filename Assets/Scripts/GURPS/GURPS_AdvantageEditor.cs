using System;
using System.Linq;
using UnityEngine.UIElements;
using Workshop.Core.Memory;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.GURPS
{
    public class GURPS_AdvantageEditor : IGuiProvider
    {
        public string Title => "ADVANTAGES EDITOR";
        private DataWarehouse _warehouse;
        private GuiContext _lastCtx;
        private GURPS_CRUD_Builder<GURPSAdvantage> _crudInterface;

        public GURPS_AdvantageEditor() { _warehouse = new DataWarehouse(); }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;
            if (_warehouse == null) _warehouse = new DataWarehouse();
            DigitalGenericUniversalRolePlayingSystem.InitializeCoreSystem(_warehouse);

            _crudInterface = new GURPS_CRUD_Builder<GURPSAdvantage>(
                title: "ADVANTAGES",
                getGlobalData: () => DigitalGenericUniversalRolePlayingSystem.Advantages.GetAll().OfType<GURPSAdvantage>(),
                getDisplayName: (a) => a.Name,
                getSourceBook: (a) => a.SourceBookName,
                setSourceBook: (a, book) => a.SourceBookName = book,
                buildEditorForm: (a) => new VisualElement(), // Implement as per Disadvantage logic
                onSaveGlobal: (a) => DigitalGenericUniversalRolePlayingSystem.Advantages.Register(a),
                onDeleteGlobal: (a) => DigitalGenericUniversalRolePlayingSystem.Advantages.GetAll().Remove(a),
                getSubtitle: (a) => $"Base Cost: {a.BaseCost} pts",
                getSortKey: (a) => (float)a.BaseCost,
                showAllBooksFilter: true
            );

            return _crudInterface.CreateGui(ctx);
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void FromUIDocument(string assetPath) { }
        public void ToUIDocument(string assetPath) { }
    }
}