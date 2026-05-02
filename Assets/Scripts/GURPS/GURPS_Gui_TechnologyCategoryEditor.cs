using Assets.Scripts.GURPS;
using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.GURPS
{
    public class GURPS_Gui_TechnologyCategoryEditor : IGuiProvider
    {
        public string Title => "TL CATEGORY EDITOR";

        private GuiContext _lastCtx;
        private GURPS_CRUD_Builder<TechnologyCategory> _crudInterface;

        public GURPS_Gui_TechnologyCategoryEditor()
        {
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            DigitalGenericUniversalRolePlayingSystem.InitializeCoreSystem();

            _crudInterface = new GURPS_CRUD_Builder<TechnologyCategory>(
                title: "TL CATEGORIES",

                getGlobalData: () => DigitalGenericUniversalRolePlayingSystem.TechCategories.GetAll(),
                getDisplayName: (cat) => cat.Name,
                getSourceBook: (cat) => cat.SourceBookName,
                setSourceBook: (cat, book) => cat.SourceBookName = book,

                buildEditorForm: (cat) =>
                {
                    var form = new VisualElement();

                    var bookTag = new Label($"Source Book: {cat.SourceBookName}") { style = { color = Color.yellow, fontSize = 14, marginBottom = 15, unityFontStyleAndWeight = FontStyle.Bold } };
                    form.Add(bookTag);

                    var nameField = new TextField("Category Name") { value = cat.Name, style = { marginBottom = 10 } };
                    nameField.Q<Label>().style.minWidth = 150;
                    nameField.RegisterValueChangedCallback(e => cat.Name = e.newValue);
                    form.Add(nameField);

                    var descField = new TextField("Description") { value = cat.Description, multiline = true, style = { marginBottom = 10, height = 100 } };
                    descField.Q<Label>().style.minWidth = 150; descField.Q<VisualElement>("unity-text-input").style.whiteSpace = WhiteSpace.Normal;
                    descField.RegisterValueChangedCallback(e => cat.Description = e.newValue);
                    form.Add(descField);

                    return form;
                },

                onSaveGlobal: (cat) => DigitalGenericUniversalRolePlayingSystem.TechCategories.Register(cat),
                onDeleteGlobal: (cat) => DigitalGenericUniversalRolePlayingSystem.TechCategories.UnRegister(cat),

                getSubtitle: (cat) => cat.SourceBookName,
                getSortKey: (cat) => 0,
                showAllBooksFilter: true
            );

            return _crudInterface.CreateGui(ctx);
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
#if UNITY_EDITOR
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
#endif
    }
}