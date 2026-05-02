using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Memory;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.Core;
using Workshop.UI_And_Tools.Forge.Hermit.UI;

namespace Workshop.GURPS
{
    public class GURPS_WeaponEditor : IGuiProvider
    {
        public string Title => "WEAPON ARSENAL EDITOR";

        private DataWarehouse _warehouse;
        private GuiContext _lastCtx;
        private GURPS_CRUD_Builder<GURPSWeapon> _crudInterface;

        public GURPS_WeaponEditor()
        {
            _warehouse = new DataWarehouse();
            DigitalGenericUniversalRolePlayingSystem.InitializeCoreSystem(_warehouse);
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            _crudInterface = new GURPS_CRUD_Builder<GURPSWeapon>(
                title: "ARMORY DATABASE",
                getGlobalData: () => DigitalGenericUniversalRolePlayingSystem.Weapons.GetAll(),
                getDisplayName: (w) => w.Name,
                getSourceBook: (w) => w.SourceBookName,
                setSourceBook: (w, b) => w.SourceBookName = b,
                buildEditorForm: (w) => BuildWeaponForm(w),
                onSaveGlobal: (w) => DigitalGenericUniversalRolePlayingSystem.Weapons.Register(w),
                onDeleteGlobal: (w) => DigitalGenericUniversalRolePlayingSystem.Weapons.UnRegister(w),
                getSubtitle: (w) => $"TL {w.TechLevel} | {w.DamageString}",
                getSortKey: (w) => (float)w.TechLevel,
                showAllBooksFilter: true
            );

            var root = _crudInterface.CreateGui(ctx);

            // FIXED: Properly inject Hermit CORTEX Link onto the root container
            new GraphicalUserInterfaceBuilder("WeaponHermitBridge")
                .WithHermit(HermitSettings.LoadOrCreate())
                .OnBuild(ve => root.Add(ve))
                .Build();

            return root;
        }

        private VisualElement BuildWeaponForm(GURPSWeapon weapon)
        {
            var formRoot = new ForgeContainerBuilder("WeaponForm_Root")
                .WithDirection(FlexDirection.Column)
                .WithPadding(15)
                .WithBackgroundColor(new Color(0.05f, 0.02f, 0.06f)) // Void theme
                .WithBorderColor(Color.magenta)
                .WithBorderWidth(1);

            formRoot.AddChild(new ForgeLabelBuilder($"TACTICAL PROFILE: {weapon.SourceBookName.ToUpper()}")
                .WithColor(Color.cyan)
                .WithFontSize(14)
                .WithBold()
                .WithMarginBottom(15)
                .OnBuild(ve => ve.style.letterSpacing = 2));

            var idRow = new ForgeContainerBuilder("IdentityRow")
                .WithDirection(FlexDirection.Row)
                .WithAlignItems(Align.Center)
                .WithMarginBottom(15);

            idRow.AddChild(new ForgeTextFieldBuilder("Designation", weapon.Name)
                .WithFlexGrow(1)
                .OnChanged(val => weapon.Name = val));

            // LINT FIX: Safely parse TL through the Forge Builder
            idRow.AddChild(new ForgeTextFieldBuilder("Tech Level", weapon.TechLevel.ToString())
                .WithWidth(80)
                .WithMarginLeft(10)
                .OnChanged(val => { if (int.TryParse(val, out int tl)) weapon.TechLevel = tl; }));

            formRoot.AddChild(idRow);

            // Probability Graph
            formRoot.AddChild(new ForgeLabelBuilder("DAMAGE YIELD PROBABILITY")
                .WithColor(Color.magenta)
                .WithFontSize(10)
                .WithBold()
                .WithMarginTop(10)
                .WithMarginBottom(5));

            var probGraph = new ProbabilityGraphBuilder(title: "DAMAGE SPREAD", maxX: 100f, maxYProb: 0.20f);
            formRoot.OnBuild(ve => ve.Add(probGraph.CreateGui(_lastCtx)));

            // Deep Crunch Reflection
            formRoot.AddChild(new ForgeLabelBuilder("BALLISTIC & KINETIC PARAMETERS")
                .WithColor(Color.cyan)
                .WithFontSize(10)
                .WithBold()
                .WithMarginTop(20)
                .WithMarginBottom(5));

            formRoot.AddSeparator(Color.gray, 1);

            // Automatically expose all remaining GURPSWeapon variables for editing
            formRoot.AddChild(new ReflectiveGuiBuilder<GURPSWeapon>(weapon).WithRecursion(1));

            return formRoot.CreateGui(_lastCtx);
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