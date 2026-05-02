using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.raWWar.Gui
{
    public class raWWar_Gui_ModularShaderBuilder : IGuiProvider
    {
        public string Title => "raWWar Modular Shader Forge";

        // Our list of active FSM Actions (The nodes the user has added)
        private List<Action<ShaderForgeContext>> _activeBehaviors = new List<Action<ShaderForgeContext>>();
        private ShaderForgeContext _currentContext = new ShaderForgeContext();

        // UI State
        private ForgeSplitPanelBuilder _splitBuilder;
        private VisualElement _root;
        private string _selectedBehaviorToAdd = "Position Instancing";

        // Mapping strings for the Dropdown to actual FSM Actions
        private Dictionary<string, Action<ShaderForgeContext>> _behaviorLibrary = new Dictionary<string, Action<ShaderForgeContext>>
        {
            { "Position Instancing", ShaderBehaviors.InjectPositionInstancing },
            { "Rotation Instancing", ShaderBehaviors.InjectRotationInstancing },
            { "Health Damage Tinting", ShaderBehaviors.InjectHealthTinting }
        };

        public VisualElement CreateGui(GuiContext ctx)
        {
            _splitBuilder = new ForgeSplitPanelBuilder(sidebarWidth: 400, Side.Left)
                .WithSidebar(new DynamicGuiProvider(c => BuildNodeStacker()))
                .WithMain(new DynamicGuiProvider(c => BuildCompiledPreview()));

            _root = _splitBuilder.CreateGui(ctx);
            _root.style.backgroundColor = new Color(0.04f, 0.04f, 0.06f); // Deep Space

            ExecuteShaderCompilationFSM();
            return _root;
        }

        private VisualElement BuildNodeStacker()
        {
            var builder = new GraphicalUserInterfaceBuilder("NodeEditor")
                .WithPadding(20)
                .WithBackgroundColor(new Color(0.10f, 0.10f, 0.14f))
                .WithScrollable(true);

            builder.AddChild(new Label("FSM SHADER COMPOSER") { style = { color = new Color(0.65f, 0.2f, 0.95f), unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 15 } });

            // --- 1. ADD NEW BEHAVIOR ---
            builder.AddDropdownData("Available Behaviors", new List<string>(_behaviorLibrary.Keys), 0, v => _selectedBehaviorToAdd = v);

            builder.AddButton("+ INJECT BEHAVIOR", () => {
                if (_behaviorLibrary.TryGetValue(_selectedBehaviorToAdd, out var action))
                {
                    _activeBehaviors.Add(action);
                    ExecuteShaderCompilationFSM();
                    RefreshUI(); // Force layout rebuild to show new node
                }
            });

            builder.AddSeparator(Color.gray, 1);

            // --- 2. ACTIVE BEHAVIOR STACK (The GUI representation of the FSM Actions) ---
            builder.AddChild(new Label("ACTIVE COMPUTE STACK") { style = { color = Color.white, unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10, marginBottom = 10 } });

            for (int i = 0; i < _activeBehaviors.Count; i++)
            {
                int index = i; // capture for closure
                // A visual representation of the Action block
                var nodeBlock = new GraphicalUserInterfaceBuilder($"Node_{i}")
                    .WithBackgroundColor(new Color(0.18f, 0.18f, 0.22f))
                    .WithBorderColor(new Color(0.65f, 0.2f, 0.95f))
                    .WithBorderWidth(1).WithBorderRadius(6)
                    .WithPadding(10).WithMarginBottom(8)
                    .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center);

                nodeBlock.AddChild(new Label($"Action Context [{i}]") { style = { color = Color.white } });

                // Remove button for this specific behavior
                nodeBlock.AddButton("X", () => {
                    _activeBehaviors.RemoveAt(index);
                    ExecuteShaderCompilationFSM();
                    RefreshUI();
                });

                builder.AddChild(nodeBlock);
            }

            return builder.Build();
        }

        private VisualElement BuildCompiledPreview()
        {
            var builder = new GraphicalUserInterfaceBuilder("Preview")
                .WithPadding(20).WithAutoGrow();

            var codeField = new TextField { value = _currentContext.CompiledHLSL, multiline = true, isReadOnly = true };
            codeField.style.backgroundColor = new Color(0.05f, 0.05f, 0.08f);
            codeField.style.color = new Color(0.3f, 0.8f, 0.5f);
            codeField.style.flexGrow = 1;
            codeField.Q("unity-text-input").style.flexGrow = 1;

            builder.AddChild(codeField);
            return builder.Build();
        }

        // --- THE MAGIC FSM TICK ---
        private void ExecuteShaderCompilationFSM()
        {
            // 1. Reset Context
            _currentContext = new ShaderForgeContext();

            // 2. Iterate through the FSM State Actions
            foreach (var behavior in _activeBehaviors)
            {
                // The Action signature (Action<IStateContext>) is invoked here. 
                // Inside the behavior, it will check if IsValid is true before mutating data!
                behavior.Invoke(_currentContext);
            }

            // 3. Post-Processing (Compiling the Context lists into the final string)
            if (_currentContext.IsValid)
            {
                AssembleFinalHLSLString();
            }
        }

        private void AssembleFinalHLSLString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Shader \"{_currentContext.ShaderName}\" {{");
            sb.AppendLine("  Properties {");
            foreach (var p in _currentContext.Properties) sb.AppendLine($"    {p}");
            sb.AppendLine("  }");
            sb.AppendLine("  SubShader {");

            sb.AppendLine("    struct SoldierGPUData {");
            foreach (var s in _currentContext.StructData) sb.AppendLine($"      {s}");
            sb.AppendLine("    };");

            sb.AppendLine("    // --- VERTEX PASS ---");
            foreach (var v in _currentContext.VertexLogic) sb.AppendLine($"    {v}");

            sb.AppendLine("    // --- FRAGMENT PASS ---");
            foreach (var f in _currentContext.FragmentLogic) sb.AppendLine($"    {f}");

            sb.AppendLine("  }");
            sb.AppendLine("}");

            _currentContext.CompiledHLSL = sb.ToString();
        }

        private void RefreshUI()
        {
            if (_root?.parent != null)
            {
                var parent = _root.parent;
                int i = parent.IndexOf(_root);
                parent.RemoveAt(i);
                parent.Insert(i, CreateGui(new GuiContext()));
            }
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string p) { }
        public void FromUIDocument(string p) { }
    }
}