using Assets.Scripts.GURPS;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Memory;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.GURPS
{
    public class GURPS_TechnologyLevelEditor : IGuiProvider
    {
        public string Title => "TL DEFINITIONS EDITOR";

        private DataWarehouse _warehouse;
        private GuiContext _lastCtx;
        private GURPS_CRUD_Builder<TechnologyLevel> _crudInterface;

        public GURPS_TechnologyLevelEditor()
        {
            _warehouse = new DataWarehouse();
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            if (_warehouse == null) _warehouse = new DataWarehouse();
            DigitalGenericUniversalRolePlayingSystem.InitializeCoreSystem(_warehouse);

            _crudInterface = new GURPS_CRUD_Builder<TechnologyLevel>(
                title: "TL DEFINITIONS",
                getGlobalData: () => DigitalGenericUniversalRolePlayingSystem.TechLevels.GetAll(),
                getDisplayName: (tl) => tl.GetFormattedTL(),
                getSourceBook: (tl) => tl.SourceBookName,
                setSourceBook: (tl, book) => tl.SourceBookName = book,
                buildEditorForm: (tl) => BuildTLEditor(tl),
                onSaveGlobal: (tl) => DigitalGenericUniversalRolePlayingSystem.TechLevels.Register(tl),
                onDeleteGlobal: (tl) => DigitalGenericUniversalRolePlayingSystem.TechLevels.UnRegister(tl),
                getSubtitle: (tl) => tl.Name,
                getSortKey: (tl) => (float)tl.Level,
                showAllBooksFilter: true
            );

            return _crudInterface.CreateGui(ctx);
        }

        private VisualElement BuildTLEditor(TechnologyLevel tl)
        {
            tl.EnsureStandardCategories();
            var form = new VisualElement();
            form.Add(new Label($"Source Book: {tl.SourceBookName}") { style = { color = Color.yellow, unityFontStyleAndWeight = FontStyle.Bold } });

            var nameField = new TextField("Epoch Name") { value = tl.Name };
            nameField.RegisterValueChangedCallback(e => tl.Name = e.newValue);
            form.Add(nameField);

            return form;
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
        public void ToUIDocument(string assetPath) { }
    }
}