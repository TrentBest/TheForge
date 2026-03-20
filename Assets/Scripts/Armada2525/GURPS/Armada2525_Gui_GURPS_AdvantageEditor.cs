using TheSingularityWorkshop.Construction;
using TheSingularityWorkshop.Builders.GuiBuilders;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.Armada2525.GURPS;

public class Armada2525_Gui_GURPS_AdvantageEditor : IGuiProvider
{
    public string Title => "ADVANTAGES EDITOR";

    private GuiContext _lastCtx;
    private GURPS_CRUD_Builder<GURPSAdvantage> _crudInterface;

    public VisualElement CreateGui(GuiContext ctx)
    {
        _lastCtx = ctx;

        // Ensure the core system is booted
        DigitalGenericUniversalRolePlayingSystem.InitializeCoreSystem();

        _crudInterface = new GURPS_CRUD_Builder<GURPSAdvantage>(
            title: "ADVANTAGES",

            getGlobalData: () => DigitalGenericUniversalRolePlayingSystem.Advantages.GetAll(),

            getDisplayName: (a) => a.Name,
            getSourceBook: (a) => a.SourceBookName,
            setSourceBook: (a, book) => a.SourceBookName = book,

            buildEditorForm: (a) => BuildAdvantageForm(a),

            onSaveGlobal: (a) => DigitalGenericUniversalRolePlayingSystem.Advantages.Register(a),
            onDeleteGlobal: (a) => DigitalGenericUniversalRolePlayingSystem.Advantages.GetAll().Remove(a),

            getSubtitle: (a) => $"Base Cost: {a.BaseCost} pts",
            getSortKey: (a) => a.BaseCost,
            showAllBooksFilter: true
        );

        return _crudInterface.CreateGui(ctx);
    }

    private VisualElement BuildAdvantageForm(GURPSAdvantage trait)
    {
        var form = new VisualElement();

        // Source Book Tag
        var bookTag = new Label($"Source Book: {trait.SourceBookName}") { style = { color = Color.yellow, fontSize = 14, marginBottom = 15, unityFontStyleAndWeight = FontStyle.Bold } };
        form.Add(bookTag);

        var nameField = new TextField("Trait Name") { value = trait.Name, style = { marginBottom = 10 } };
        nameField.Q<Label>().style.color = Color.gray;
        nameField.Q<Label>().style.minWidth = 150;
        nameField.RegisterValueChangedCallback(e => trait.Name = e.newValue);
        form.Add(nameField);

        var costField = new IntegerField("Base Cost (Points)") { value = trait.BaseCost, style = { marginBottom = 10 } };
        costField.Q<Label>().style.color = Color.gray;
        costField.Q<Label>().style.minWidth = 150;
        costField.RegisterValueChangedCallback(e => trait.BaseCost = e.newValue);
        form.Add(costField);

        var descField = new TextField("Description") { value = trait.Description, multiline = true, style = { marginBottom = 20, height = 80 } };
        descField.Q<Label>().style.color = Color.gray;
        descField.Q<Label>().style.minWidth = 150;
        descField.Q<VisualElement>("unity-text-input").style.whiteSpace = WhiteSpace.Normal;
        descField.RegisterValueChangedCallback(e => trait.Description = e.newValue);
        form.Add(descField);

        // Gameplay Effects Section
        form.Add(new Label("GAMEPLAY EFFECTS") { style = { color = Color.cyan, fontSize = 16, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10, marginTop = 10, borderBottomWidth = 1, borderBottomColor = Color.gray } });

        var effectsContainer = new VisualElement();
        RenderEffects(effectsContainer, trait);
        form.Add(effectsContainer);

        var addEffectBtn = new Button(() => {
            trait.Effects.Add(new GameEffect { TargetTag = "new.tag", MathType = GameEffectMath.FlatBonus, ValuePerLevel = 1f });
            RenderEffects(effectsContainer, trait);
        })
        { text = "+ ADD EFFECT", style = { backgroundColor = new Color(0.2f, 0.2f, 0.4f), color = Color.white, marginTop = 10 } };
        form.Add(addEffectBtn);

        return form;
    }

    private void RenderEffects(VisualElement container, GURPSAdvantage trait)
    {
        container.Clear();
        foreach (var effect in trait.Effects.ToList())
        {
            var row = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 5, backgroundColor = new Color(0.15f, 0.15f, 0.15f), paddingBottom = 5, paddingTop = 5, paddingLeft = 5, paddingRight = 5 } };

            var targetField = new TextField { value = effect.TargetTag, style = { width = 180, marginRight = 10 } };
            targetField.RegisterValueChangedCallback(e => effect.TargetTag = e.newValue);

            var valField = new FloatField { value = effect.ValuePerLevel, style = { width = 60, marginRight = 10 } };
            valField.RegisterValueChangedCallback(e => effect.ValuePerLevel = e.newValue);

            var delBtn = new Button(() => { trait.Effects.Remove(effect); RenderEffects(container, trait); })
            { text = "X", style = { color = Color.white, backgroundColor = new Color(0.5f, 0, 0), unityFontStyleAndWeight = FontStyle.Bold } };

            row.Add(new Label("Tag:") { style = { width = 30, color = Color.gray } });
            row.Add(targetField);
            row.Add(new Label("Val:") { style = { width = 30, color = Color.gray } });
            row.Add(valField);
            row.Add(new VisualElement { style = { flexGrow = 1 } }); // Spacer
            row.Add(delBtn);

            container.Add(row);
        }
    }

    public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
    public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
#if UNITY_EDITOR
    public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
#endif
}