using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Memory;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.Core;
using Workshop.UI_And_Tools.Forge.Hermit.UI;

namespace Workshop.GURPS
{
    public class GURPS_SkillEditor : IGuiProvider
    {
        public string Title => "GURPS SKILL DICTIONARY";

        private DataWarehouse _warehouse;
        private GuiContext _lastCtx;
        private GURPS_CRUD_Builder<GURPSSkillDefinition> _crudInterface;

        public GURPS_SkillEditor()
        {
            _warehouse = new DataWarehouse();
            DigitalGenericUniversalRolePlayingSystem.InitializeCoreSystem(_warehouse);
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            _crudInterface = new GURPS_CRUD_Builder<GURPSSkillDefinition>(
                title: "SKILLS",
                getGlobalData: () => DigitalGenericUniversalRolePlayingSystem.Skills.GetAll(),
                getDisplayName: (s) => s.Name,
                getSourceBook: (s) => s.SourceBookName,
                setSourceBook: (s, book) => s.SourceBookName = book,
                buildEditorForm: (s) => BuildSkillForm(s),
                onSaveGlobal: (s) => DigitalGenericUniversalRolePlayingSystem.Skills.Register(s),
                onDeleteGlobal: (s) => DigitalGenericUniversalRolePlayingSystem.Skills.UnRegister(s),
                getSubtitle: (s) => $"{s.BaseAttribute}/{s.Difficulty}",
                showAllBooksFilter: true
            );

            var root = _crudInterface.CreateGui(ctx);

            // Inject Hermit presence (CORTEX Link)
            new GraphicalUserInterfaceBuilder("SkillBridge")
                .WithHermit(HermitSettings.LoadOrCreate())
                .OnBuild(ve => root.Add(ve))
                .Build();

            return root;
        }

        private VisualElement BuildSkillForm(GURPSSkillDefinition skill)
        {
            var form = new ForgeContainerBuilder("SkillForm_Root")
                .WithPadding(15)
                .WithBackgroundColor(new Color(0.05f, 0.02f, 0.06f)) // Deep Void theme
                .WithBorderColor(Color.cyan)
                .WithBorderWidth(1)

                // Thematic Header
                .AddChild(new ForgeLabelBuilder("SYNAPTIC IMPRINT: SKILL METADATA")
                    .WithBold()
                    .WithColor(Color.magenta)
                    .WithFontSize(14)
                    .WithMarginBottom(15)
                    .OnBuild(ve => ve.style.letterSpacing = 2))

                // Core Human-Readable Fields
                .AddChild(new ForgeTextFieldBuilder("Skill Name", skill.Name)
                    .WithMarginBottom(10)
                    .OnChanged(v => skill.Name = v))
                .AddChild(new ForgeTextFieldBuilder("Description", skill.Description)
                    .AsMultiline()
                    .WithHeight(80)
                    .WithMarginBottom(15)
                    .OnChanged(v => skill.Description = v))

                // Mechanical Crunch Header
                .AddChild(new ForgeLabelBuilder("CORE MECHANICS & DEFAULTS")
                    .WithColor(Color.cyan)
                    .WithFontSize(10)
                    .WithMarginBottom(5))
                .AddSeparator(Color.gray, 1)

                // Reflection handles BaseAttribute, Difficulty, and any future complex fields automatically
                .AddChild(new ReflectiveGuiBuilder<GURPSSkillDefinition>(skill).WithRecursion(2));

            return form.CreateGui(_lastCtx);
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));

        public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));

#if UNITY_EDITOR
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
#else
        public void ToUIDocument(string assetPath) { }
#endif
    }
}