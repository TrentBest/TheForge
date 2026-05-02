using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.Systems.MicroPackages.Packages.DesignPatterns.Factory
{
    [ForgeInspector(typeof(FactoryArchitectContext))]
    public class Workshop_Gui_FactoryArchitect : IGuiProvider
    {
        public string Title => "Pattern Architect: Factory";

        private FactoryArchitectContext _context;
        private VisualElement _blueprintListContainer;

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _context = new FactoryArchitectContext();

            var root = new ForgeContainerBuilder("FactoryArchitectRoot")
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f, 1f))
                .WithPadding(15);

            // --- HEADER ---
            root.AddChild(new ForgeLabelBuilder("FACTORY PATTERN CONFIGURATOR")
                .WithColor(new Color(0.0f, 0.9f, 1.0f)) // Cyan
                .WithFontSize(20)
                .WithFontStyle(FontStyle.Bold)
                .WithMarginBottom(5));

            root.AddChild(new ForgeLabelBuilder("Define a manufacturing pipeline and map FSM integration.")
                .WithColor(Color.gray)
                .WithMarginBottom(20));

            // --- FACTORY SETTINGS ---
            var settingsBox = new ForgeContainerBuilder("SettingsBox")
                .WithBackgroundColor(new Color(0.12f, 0.12f, 0.15f))
                .WithBorderRadius(5)
                .WithPadding(10)
                .WithMarginBottom(20);

            settingsBox.AddChild(new ForgeTextFieldBuilder("Factory Designation:")
                .WithValue(_context.ActiveFactoryName)
                .OnChanged(val => _context.ActiveFactoryName = val));

            root.AddChild(settingsBox);

            // --- BLUEPRINT LIST ---
            root.AddChild(new ForgeLabelBuilder("REGISTERED BLUEPRINTS")
                .WithColor(new Color(0.64f, 0.17f, 0.77f)) // Forge Purple
                .WithFontStyle(FontStyle.Bold)
                .WithMarginBottom(10));

            _blueprintListContainer = new ForgeScrollViewBuilder("BlueprintList").WithFlexGrow(1).Build();
            root.AddChild(_blueprintListContainer);

            RefreshBlueprintList();

            // --- CONTROLS ---
            var controlRow = new ForgeContainerBuilder("ControlRow")
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                .WithMarginTop(15);

            controlRow.AddChild(new ForgeButtonBuilder("+ Add Blueprint", AddNewBlueprint)
                .WithBackgroundColor(new Color(0.2f, 0.5f, 0.2f))
                .WithTextColor(Color.white)
                .WithPadding(8, 8, 15, 15));

            controlRow.AddChild(new ForgeButtonBuilder("Generate Factory Code", GenerateFactoryOutput)
                .WithBackgroundColor(new Color(0.64f, 0.17f, 0.77f))
                .WithTextColor(Color.white)
                .WithPadding(8, 8, 15, 15));

            root.AddChild(controlRow);

            return root.Build();
        }

        private void AddNewBlueprint()
        {
            _context.DraftBlueprints.Add(new FactoryArchitectContext.BlueprintDef());
            RefreshBlueprintList();
        }

        private void RefreshBlueprintList()
        {
            _blueprintListContainer.Clear();

            if (_context.DraftBlueprints.Count == 0)
            {
                _blueprintListContainer.Add(new ForgeLabelBuilder("No blueprints defined. Factory will be empty.")
                    .WithColor(Color.gray).WithFontStyle(FontStyle.Italic).Build());
                return;
            }

            for (int i = 0; i < _context.DraftBlueprints.Count; i++)
            {
                var bp = _context.DraftBlueprints[i];
                int index = i; // Local copy for lambda closure

                var card = new ForgeContainerBuilder($"BP_{i}")
                    .WithBackgroundColor(new Color(0.15f, 0.15f, 0.18f))
                    .WithBorderColor(new Color(0.3f, 0.3f, 0.3f))
                    .WithBorderWidth(1).WithBorderRadius(4)
                    .WithPadding(10).WithMarginBottom(10);

                var headerRow = new ForgeContainerBuilder("Header")
                    .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                    .WithMarginBottom(10);

                headerRow.AddChild(new ForgeLabelBuilder($"Blueprint [{index}]").WithColor(Color.white).WithFontStyle(FontStyle.Bold));
                headerRow.AddChild(new ForgeButtonBuilder("X", () => { _context.DraftBlueprints.RemoveAt(index); RefreshBlueprintList(); })
                    .WithBackgroundColor(Color.clear).WithTextColor(new Color(0.8f, 0.2f, 0.2f)));

                card.AddChild(headerRow);

                card.AddChild(new ForgeTextFieldBuilder("Blueprint ID:")
                    .WithValue(bp.BlueprintId)
                    .OnChanged(val => bp.BlueprintId = val));

                card.AddChild(new ForgeTextFieldBuilder("Resources Prefab Path:")
                    .WithValue(bp.PrefabPath)
                    .OnChanged(val => bp.PrefabPath = val));

                card.AddChild(new ForgeTextFieldBuilder("Initial FSM State:")
                    .WithValue(bp.InitialFsmState)
                    .OnChanged(val => bp.InitialFsmState = val));

                _blueprintListContainer.Add(card.Build());
            }
        }

        private void GenerateFactoryOutput()
        {
            // In a full implementation, this could dynamically emit C# code via CodeDOM, 
            // write a JSON manifest to your CacheBuilder, or directly instantiate the ForgeEntityFactory in memory.

            Debug.Log($"[FactoryArchitect] Compiling Factory: {_context.ActiveFactoryName}");
            foreach (var bp in _context.DraftBlueprints)
            {
                Debug.Log($"   -> Registered: {bp.BlueprintId} | Prefab: {bp.PrefabPath} | State: {bp.InitialFsmState}");
            }

            Debug.Log("[FactoryArchitect] Factory generation payload dispatched to DataWarehouse.");
        }
    }
}