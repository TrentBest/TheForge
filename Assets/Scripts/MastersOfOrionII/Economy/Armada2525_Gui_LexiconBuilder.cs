#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.MastersOfOrionII.Economy
{
    public class MastersOfOrionII_Gui_LexiconBuilder : IGuiProvider
    {
        public string Title => "UNIVERSAL LINGUISTIC ARCHIVE";

        private GuiContext _lastCtx;
        private CRUD_Builder<LexiconCategory> _crudInterface;

        // The Master Database of all strings in the game
        public static List<LexiconCategory> LexiconDatabase = new List<LexiconCategory>
        {
            new LexiconCategory { Name = "Human Mega-Corp Prefixes", RawData = "Aegis\nNova\nStell\nOmni\nVanguard" },
            new LexiconCategory { Name = "Industrial Suffixes", RawData = "Heavy Industries\nFoundries\nDynamics" }
        };

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            _crudInterface = new CRUD_Builder<LexiconCategory>(
                title: "GENERATIVE LEXICONS",
                dataSource: () => LexiconDatabase,
                getDisplayName: (lex) => string.IsNullOrEmpty(lex.Name) ? "Unnamed Set" : lex.Name,
                getGroupCategory: (lex) => "Linguistics",

                buildEditorForm: (lex) =>
                {
                    var form = new VisualElement { style = { flexDirection = FlexDirection.Column, paddingLeft = 10, paddingRight = 10 } };

                    form.Add(new Label("LEXICON DEFINITION") { style = { color = Color.cyan, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });

                    var nameField = new TextField("Category Name") { value = lex.Name };
                    nameField.RegisterValueChangedCallback(e => lex.Name = e.newValue);
                    form.Add(nameField);

                    var descField = new TextField("Context / Usage") { value = lex.Description };
                    descField.RegisterValueChangedCallback(e => lex.Description = e.newValue);
                    form.Add(descField);

                    form.Add(new Label("DATA ENTRIES (One per line)") { style = { color = Color.yellow, marginTop = 15, marginBottom = 5, unityFontStyleAndWeight = FontStyle.Bold } });

                    var dataField = new TextField { value = lex.RawData, multiline = true, style = { height = 300, backgroundColor = new Color(0.05f, 0.05f, 0.05f) } };
                    dataField.RegisterValueChangedCallback(e => lex.RawData = e.newValue);
                    form.Add(dataField);

                    return form;
                },
                onSave: (lex) => { if (!LexiconDatabase.Contains(lex)) LexiconDatabase.Add(lex); },
                onDelete: (lex) => { LexiconDatabase.Remove(lex); },
                getSubtitle: (lex) => $"Entries: {lex.GetEntries().Length}"
            );

            return _crudInterface.CreateGui(ctx);
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
    }
}
#endif