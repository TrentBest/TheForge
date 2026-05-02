using System;
using System.Collections.Generic;
using System.Linq;
using TheSingularityWorkshop.FSM_API;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.UI_And_Tools.Showcase
{
    public class TypeMappingCacheRecord
    {
        public Type TargetType { get; set; }
        public string TypeName => TargetType.Name;
        public bool HasCachedMapping { get; set; }
        public string ActiveSkinName { get; set; }
    }

    public class Showcase_DomainMappingDashboard : IGuiProvider
    {
        public string Title => "Domain Registry & Mapping";

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new GraphicalUserInterfaceBuilder("RegistryRoot")
                .WithFlexGrow(1).WithBackgroundColor(new Color(0.1f, 0.1f, 0.12f))
                .WithPadding(15);

            // --- HEADER ---
            root.AddChild(new ForgeLabelBuilder("DOMAIN GUI REGISTRY")
                .WithFontSize(24).WithColor(Color.cyan).WithFontStyle(FontStyle.Bold).WithMarginBottom(5))
                .AddSeparator(Color.cyan, 2);

            root.AddChild(new ForgeLabelBuilder("Scanning Assembly for State Contexts and Data Models...")
                .WithColor(Color.gray).WithFontStyle(FontStyle.Italic).WithMarginBottom(15));

            // --- ASSEMBLE THE DATA ---
            var records = ScanAndEvaluateAssembly();

            // --- BUILD THE CRUD LIST ---
            // We use the Forge CRUD Builder to manage the list of TypeMappingCacheRecords
            var crudType = typeof(CRUD_Builder<>).MakeGenericType(typeof(TypeMappingCacheRecord));
            var crudInstance = Activator.CreateInstance(crudType, new object[]
            {
                "Known Data Types",
                (Func<IEnumerable<TypeMappingCacheRecord>>)(() => records),
                (Func<TypeMappingCacheRecord, string>)(record => record.TypeName),
                (Func<TypeMappingCacheRecord, VisualElement>)(record => BuildRecordRow(record, ctx)), // Custom Renderer
                (Action<TypeMappingCacheRecord>)(record => { }), // OnSelect
                (Action<TypeMappingCacheRecord>)(record => { }), // OnEdit
                null, null
            });

            var crudElement = (VisualElement)crudType.GetMethod("CreateGui").Invoke(crudInstance, new object[] { ctx });

            root.AddChild(crudElement);

            return root.Build();
        }

        private List<TypeMappingCacheRecord> ScanAndEvaluateAssembly()
        {
            var records = new List<TypeMappingCacheRecord>();

            // STRICT DOMAIN ISOLATION: Only retrieve our architectural pillars
            var relevantTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract &&
                            (typeof(IStateContext).IsAssignableFrom(t) ||
                             typeof(IGuiProvider).IsAssignableFrom(t)))
                .OrderBy(t => t.Name);

            foreach (var type in relevantTypes)
            {
                bool hasMapping = TypeGuiProviderFactory.HasProviderFor(type);

                records.Add(new TypeMappingCacheRecord
                {
                    TargetType = type,
                    HasCachedMapping = hasMapping,
                    ActiveSkinName = hasMapping ? "Custom Domain Skin" : "Native Reflection"
                });
            }

            return records;
        }

        private VisualElement BuildRecordRow(TypeMappingCacheRecord record, GuiContext ctx)
        {
            var row = new GraphicalUserInterfaceBuilder($"Row_{record.TypeName}")
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                .WithBackgroundColor(new Color(0.15f, 0.15f, 0.15f))
                .WithPadding(8).WithMarginBottom(4)
                .WithBorderRadius(4);

            // Left side: Type Name and Status
            var infoContainer = new GraphicalUserInterfaceBuilder("Info")
                .WithFlexLayout(FlexDirection.Column)
                .AddChild(new ForgeLabelBuilder(record.TypeName).WithColor(Color.white).WithFontStyle(FontStyle.Bold))
                .AddChild(new ForgeLabelBuilder(record.ActiveSkinName)
                    .WithColor(record.HasCachedMapping ? Color.green : Color.yellow).WithFontSize(10));
            row.AddChild(infoContainer);

            // Right side: The Command Dropdown / Action Buttons
            var actionRow = new GraphicalUserInterfaceBuilder("Actions")
                .WithFlexLayout(FlexDirection.Row);

            if (record.HasCachedMapping)
            {
                actionRow.AddChild(new ForgeButtonBuilder("EDIT SKIN")
                    .WithBackgroundColor(new Color(0.2f, 0.4f, 0.6f)).WithWidth(100)
                    .OnClick(() => Debug.Log($"Opening Visual Editor for {record.TypeName}")));
            }
            else
            {
                // This is the trigger that invokes Hermit
                actionRow.AddChild(new ForgeButtonBuilder("HERMIT: DESIGN SKIN")
                    .WithBackgroundColor(new Color(0.6f, 0.2f, 0.6f)).WithWidth(150) // Purple for AI ops
                    .OnClick(() => DispatchToHermit(record.TargetType)));
            }

            // Always allow them to view the raw reflection
            actionRow.AddChild(new ForgeButtonBuilder("INSPECT RAW")
                .WithBackgroundColor(new Color(0.3f, 0.3f, 0.3f)).WithWidth(100).WithMarginLeft(5)
                .OnClick(() => Debug.Log("Load standard ReflectiveGuiBuilder in the split pane")));

            row.AddChild(actionRow);
            return row.Build();
        }

        private void DispatchToHermit(Type targetType)
        {
            Debug.Log($"[Hermit Dispatch] Packaging schema for {targetType.Name}...");

            // 1. Serialize the core data definition so Hermit knows what fields exist
            string schema = SerializeTypeSchema(targetType);

            // 2. Build the intent
            var intent = new GuiGenerationIntent
            {
                TargetTypeFullName = targetType.FullName,
                SerializedDataSchema = schema,
                UserPrompt = "Analyze this data structure and generate a fluent C# Forge UI layout for it.", // This could be pulled from a text field the user types into
                TargetDomainID = "GrandCentralStation"
            };

            // 3. Fire it onto the bus. Hermit's background thread is listening.
            // ForgeNetworkBus.Instance.QueueRequest(intent);
            Debug.Log($"[Hermit Dispatch] Intent broadcasted. Awaiting mini-hermit actuation.");
        }

        private string SerializeTypeSchema(Type type)
        {
            // A simple mockup of reflecting the properties to send to the LLM
            var props = type.GetProperties().Select(p => $"{p.PropertyType.Name} {p.Name}");
            return $"class {type.Name} {{\n  " + string.Join(";\n  ", props) + ";\n}";
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}