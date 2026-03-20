using TheSingularityWorkshop.Construction;
using TheSingularityWorkshop.Builders.GuiBuilders;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.Armada2525.GURPS;

public class Armada2525_Gui_GURPS_SkillEditor : IGuiProvider
{
    public string Title => "GURPS SKILL DICTIONARY (CRUD)";

    private GuiContext _lastCtx;
    private GURPS_CRUD_Builder<GURPSSkillDefinition> _crudInterface;

    public VisualElement CreateGui(GuiContext ctx)
    {
        _lastCtx = ctx;

        // Ensure the core system is booted
        DigitalGenericUniversalRolePlayingSystem.InitializeCoreSystem();

        _crudInterface = new GURPS_CRUD_Builder<GURPSSkillDefinition>(
            title: "SKILLS",

            // Replace the mock list with the actual API
            getGlobalData: () => DigitalGenericUniversalRolePlayingSystem.Skills.GetAll(),

            getDisplayName: (s) => s.Name,
            getSourceBook: (s) => s.SourceBookName,
            setSourceBook: (s, book) => s.SourceBookName = book,

            buildEditorForm: (s) => BuildSkillForm(s),

            // Map standard save/delete to the Skills API
            onSaveGlobal: (s) => DigitalGenericUniversalRolePlayingSystem.Skills.Register(s),
            onDeleteGlobal: (s) => DigitalGenericUniversalRolePlayingSystem.Skills.UnRegister(s),

            getSubtitle: (s) => $"{s.BaseAttribute}/{s.Difficulty}",
            getSortKey: (s) => 0f, // Sorting is handled alphabetically by default in CRUD builder
            showAllBooksFilter: true
        );

        return _crudInterface.CreateGui(ctx);
    }

    private VisualElement BuildSkillForm(GURPSSkillDefinition skill)
    {
        var form = new VisualElement();

        // Source Book Tag
        var bookTag = new Label($"Source Book: {skill.SourceBookName}") { style = { color = Color.yellow, fontSize = 14, marginBottom = 15, unityFontStyleAndWeight = FontStyle.Bold } };
        form.Add(bookTag);

        // Name
        var nameField = new TextField("Skill Name") { value = skill.Name, style = { marginBottom = 10 } };
        nameField.Q<Label>().style.color = Color.gray;
        nameField.Q<Label>().style.minWidth = 120;
        nameField.RegisterValueChangedCallback(e => skill.Name = e.newValue);
        form.Add(nameField);

        // Attribute Dropdown
        var attrOptions = Enum.GetNames(typeof(GURPSAttributeType)).ToList();
        var attrField = new DropdownField("Base Attribute", attrOptions, skill.BaseAttribute.ToString()) { style = { marginBottom = 10 } };
        attrField.Q<Label>().style.color = Color.gray;
        attrField.Q<Label>().style.minWidth = 120;
        attrField.RegisterValueChangedCallback(e => {
            if (Enum.TryParse<GURPSAttributeType>(e.newValue, out var parsed))
                skill.BaseAttribute = parsed;
        });
        form.Add(attrField);

        // Difficulty Dropdown
        var diffOptions = Enum.GetNames(typeof(GURPSDifficulty)).ToList();
        var diffField = new DropdownField("Difficulty", diffOptions, skill.Difficulty.ToString()) { style = { marginBottom = 10 } };
        diffField.Q<Label>().style.color = Color.gray;
        diffField.Q<Label>().style.minWidth = 120;
        diffField.RegisterValueChangedCallback(e => {
            if (Enum.TryParse<GURPSDifficulty>(e.newValue, out var parsed))
                skill.Difficulty = parsed;
        });
        form.Add(diffField);

        // Description
        var descField = new TextField("Description") { value = skill.Description, multiline = true, style = { marginBottom = 20, height = 100 } };
        descField.Q<Label>().style.color = Color.gray;
        descField.Q<Label>().style.minWidth = 120;
        descField.Q<VisualElement>("unity-text-input").style.whiteSpace = WhiteSpace.Normal;
        descField.RegisterValueChangedCallback(e => skill.Description = e.newValue);
        form.Add(descField);

        return form;
    }

    public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
    public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
#if UNITY_EDITOR
    public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
#endif
}