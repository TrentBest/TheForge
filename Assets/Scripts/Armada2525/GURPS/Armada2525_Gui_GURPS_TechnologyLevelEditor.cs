using TheSingularityWorkshop.Construction;
using TheSingularityWorkshop.Builders.GuiBuilders;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.Armada2525.GURPS;

public class Armada2525_Gui_GURPS_TechnologyLevelEditor : IGuiProvider
{
    public string Title => "TL DEFINITIONS EDITOR";

    private GuiContext _lastCtx;
    private GURPS_CRUD_Builder<TechnologyLevel> _crudInterface;

    public VisualElement CreateGui(GuiContext ctx)
    {
        _lastCtx = ctx;

        DigitalGenericUniversalRolePlayingSystem.InitializeCoreSystem();

        _crudInterface = new GURPS_CRUD_Builder<TechnologyLevel>(
            title: "TL DEFINITIONS",

            getGlobalData: () => DigitalGenericUniversalRolePlayingSystem.TechLevels.GetAll(),

            getDisplayName: (tl) => tl.GetFormattedTL(),
            getSourceBook: (tl) => tl.SourceBookName,

            setSourceBook: (tl, book) => tl.SourceBookName = book,

            buildEditorForm: (tl) =>
            {
                // Failsafe: Ensure legacy saved items get the new categories injected
                tl.EnsureStandardCategories();

                var form = new VisualElement();

                var bookTag = new Label($"Source Book: {tl.SourceBookName}") { style = { color = Color.yellow, fontSize = 14, marginBottom = 15, unityFontStyleAndWeight = FontStyle.Bold } };
                form.Add(bookTag);

                var nameField = new TextField("Epoch Name") { value = tl.Name, style = { marginBottom = 10 } };
                nameField.Q<Label>().style.minWidth = 150; nameField.RegisterValueChangedCallback(e => tl.Name = e.newValue);
                form.Add(nameField);

                var levelField = new IntegerField("Base TL") { value = tl.Level, style = { marginBottom = 10 } };
                levelField.Q<Label>().style.minWidth = 150;
                form.Add(levelField);

                var divField = new IntegerField("Divergent Offset (+X)") { value = tl.DivergentOffset, style = { marginBottom = 10 } };
                divField.Q<Label>().style.minWidth = 150;
                divField.RegisterValueChangedCallback(e => tl.DivergentOffset = e.newValue);
                form.Add(divField);

                var wealthField = new FloatField("Wealth Multiplier") { value = tl.StartingWealthMultiplier, style = { marginBottom = 20 } };
                wealthField.Q<Label>().style.minWidth = 150; wealthField.RegisterValueChangedCallback(e => tl.StartingWealthMultiplier = e.newValue);
                form.Add(wealthField);

                var descField = new TextField("Description") { value = tl.Description, multiline = true, style = { marginBottom = 10, height = 60 } };
                descField.Q<Label>().style.minWidth = 150; descField.Q<VisualElement>("unity-text-input").style.whiteSpace = WhiteSpace.Normal;
                descField.RegisterValueChangedCallback(e => tl.Description = e.newValue);
                form.Add(descField);

                // --- CATEGORY SLIDERS SECTION ---
                var categoryBox = new VisualElement { style = { marginTop = 20, paddingTop = 10, borderTopWidth = 1, borderTopColor = Color.gray } };
                categoryBox.Add(new Label("CATEGORY OFFSETS (SPLIT TLs)") { style = { color = Color.cyan, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });

                // We keep a list of update actions so when the Base TL changes, we can refresh the math on all slider labels
                List<Action> updateLabels = new List<Action>();

                foreach (var cat in tl.CategoryOffsets)
                {
                    var row = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 5 } };

                    // Allow offsets from -10 to +10
                    var slider = new SliderInt(cat.CategoryName, -10, 10) { value = cat.Offset, style = { flexGrow = 1 } };
                    slider.Q<Label>().style.minWidth = 150;
                    slider.Q<Label>().style.color = new Color(0.8f, 0.8f, 0.8f);

                    var effectiveTLLabel = new Label() { style = { width = 140, unityTextAlign = TextAnchor.MiddleRight, color = Color.yellow, unityFontStyleAndWeight = FontStyle.Bold } };

                    // Closure to refresh just this specific label
                    Action refreshLabel = () => {
                        int effectiveTL = tl.Level + cat.Offset;
                        string prefix = cat.Offset > 0 ? "+" : "";
                        effectiveTLLabel.text = $"[{prefix}{cat.Offset}] => Eff. TL: {Mathf.Max(0, effectiveTL)}"; // Prevents negative TL display
                    };

                    refreshLabel(); // Call once to initialize text
                    updateLabels.Add(refreshLabel);

                    slider.RegisterValueChangedCallback(e => {
                        cat.Offset = e.newValue;
                        refreshLabel();
                    });

                    row.Add(slider);
                    row.Add(effectiveTLLabel);
                    categoryBox.Add(row);
                }

                // If the user changes the BASE level, execute all the label refresh actions instantly
                levelField.RegisterValueChangedCallback(e => {
                    tl.Level = e.newValue;
                    foreach (var update in updateLabels) update();
                });

                form.Add(categoryBox);

                return form;
            },

            onSaveGlobal: (tl) => DigitalGenericUniversalRolePlayingSystem.TechLevels.Register(tl),
            onDeleteGlobal: (tl) => DigitalGenericUniversalRolePlayingSystem.TechLevels.UnRegister(tl),

            getSubtitle: (tl) => tl.Name,
            getSortKey: (tl) => tl.Level,
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