using Assets.Scripts.GURPS;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.GURPS
{
    public class GURPS_Gui_DisadvantageEditor : IGuiProvider
    {
        public string Title => "DISADVANTAGES EDITOR";

        private GuiContext _lastCtx;
        private GURPS_CRUD_Builder<GURPSDisadvantage> _crudInterface;

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            // Ensure the core system is booted (The "Back Office")
            DigitalGenericUniversalRolePlayingSystem.InitializeCoreSystem();

            _crudInterface = new GURPS_CRUD_Builder<GURPSDisadvantage>(
                title: "DISADVANTAGES",

                // Logic Fix: Filter the unified Traits list for Disadvantages only
                getGlobalData: () => DigitalGenericUniversalRolePlayingSystem.Disadvantages.GetAll().OfType<GURPSDisadvantage>(),

                getDisplayName: (a) => a.Name,
                getSourceBook: (a) => a.SourceBookName,
                setSourceBook: (a, book) => a.SourceBookName = book,

                // REFORGED: You can now use the Workhorse Reflective Builder here to handle everything,
                // or use the manual Forge layout below for specific GURPS styling.
                buildEditorForm: (a) => BuildDisadvantageForm(a),

                onSaveGlobal: (a) => DigitalGenericUniversalRolePlayingSystem.Disadvantages.Register(a),

                // Logic Fix: Use the specialized Remove(string) method in TraitsApi
                onDeleteGlobal: (a) => DigitalGenericUniversalRolePlayingSystem.Disadvantages.Remove(a.Id),

                getSubtitle: (a) => $"Base Cost: {a.BaseCost} pts",
                getSortKey: (a) => (float)a.BaseCost,
                showAllBooksFilter: true
            );

            return _crudInterface.CreateGui(ctx);
        }

        private VisualElement BuildDisadvantageForm(GURPSDisadvantage trait)
        {
            // REFORGED: Using ForgeContainerBuilder as the root
            var form = new ForgeContainerBuilder()
                .WithDirection(FlexDirection.Column)
                .WithPadding(10);

            // 1. Source Book Header
            form.AddChild(new ForgeLabelBuilder($"Source Book: {trait.SourceBookName}")
                .WithColor(Color.yellow)
                .WithFontSize(14)
                .WithBold()
                .WithMarginBottom(15));

            // 2. Trait Identification
            form.AddChild(new ForgeTextFieldBuilder("Trait Name")
                .WithValue(trait.Name)
                .WithMarginBottom(10)
                .OnChanged(val => trait.Name = val));

            // 3. Point Cost (Utilizing OnBuild for native numeric fields)
            form.OnBuild(ve => {
                var costField = new IntegerField("Base Cost (Points)") { value = trait.BaseCost };
                costField.style.marginBottom = 10;
                costField.RegisterValueChangedCallback(e => trait.BaseCost = e.newValue);
                ve.Add(costField);
            });

            // 4. Description
            form.AddChild(new ForgeTextFieldBuilder("Description")
                .WithValue(trait.Description)
                .AsMultiline()
                .WithHeight(80)
                .WithMarginBottom(20)
                .OnChanged(val => trait.Description = val));

            // 5. Gameplay Effects (Delegating to the Reflective "Powerhouse")
            form.AddChild(new ForgeLabelBuilder("GAMEPLAY EFFECTS")
                .WithColor(Color.cyan)
                .WithFontSize(16)
                .WithBold()
                .WithMarginTop(10)
                .WithMarginBottom(10))
                .AddSeparator(Color.gray, 1);

            // Here we use the ReflectiveGuiBuilder to handle the Effects collection automatically.
            // It will see List<GameEffect> and embed a nested CRUD_Builder.
            var effectsEditor = new ReflectiveGuiBuilder<GURPSDisadvantage>(trait)
                .WithFilter("Effects");

            form.OnBuild(ve => ve.Add(effectsEditor.CreateGui(_lastCtx)));

            return form.CreateGui(_lastCtx);
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
#if UNITY_EDITOR
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
#endif
    }
}