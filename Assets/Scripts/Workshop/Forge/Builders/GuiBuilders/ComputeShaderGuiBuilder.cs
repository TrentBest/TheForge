using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.FSM_API;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;

namespace TheSingularityWorkshop.Sandbox.Builders
{
    // The data model for a single field in our GPU struct
    public enum GpuDataType { Float, Int, Float2, Float3, Float4, Bool }

    public class ShaderFieldDef
    {
        public string Name = "NewVariable";
        public GpuDataType DataType = GpuDataType.Float;
    }

    public class ComputeShaderGuiBuilder : IGuiProvider
    {
        public string Title => "COMPUTE SHADER FORGE";

        private List<ShaderFieldDef> _fields = new List<ShaderFieldDef>();
        private string _agentName = "AntData";
        private VisualElement _fieldsContainer;

        public ComputeShaderGuiBuilder()
        {
            // Add some default fields for our Ant scenario
            _fields.Add(new ShaderFieldDef { Name = "Position", DataType = GpuDataType.Float2 });
            _fields.Add(new ShaderFieldDef { Name = "StateId", DataType = GpuDataType.Int });
            _fields.Add(new ShaderFieldDef { Name = "Energy", DataType = GpuDataType.Float });
        }

        // WIRED: Now correctly accepts the GuiContext to satisfy IGuiProvider
        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new VisualElement { style = { flexGrow = 1, paddingTop = 20, paddingBottom = 20, paddingLeft = 20, paddingRight = 20, backgroundColor = new Color(0.12f, 0.12f, 0.15f) } };

            // Header
            root.Add(new Label("GPU AGENT COMPILER") { style = { fontSize = 24, color = Color.cyan, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 20 } });

            // Agent Name
            var nameRow = new VisualElement { style = { flexDirection = FlexDirection.Row, marginBottom = 10 } };
            nameRow.Add(new Label("Agent Struct Name: ") { style = { width = 150, color = Color.white, marginTop = 3 } });
            var nameInput = new TextField { value = _agentName, style = { flexGrow = 1 } };
            nameInput.RegisterValueChangedCallback(evt => _agentName = evt.newValue);
            nameRow.Add(nameInput);
            root.Add(nameRow);

            // Fields List
            root.Add(new Label("MEMORY LAYOUT (FIELDS):") { style = { color = Color.yellow, marginTop = 20, marginBottom = 10, unityFontStyleAndWeight = FontStyle.Bold } });

            _fieldsContainer = new VisualElement { style = { backgroundColor = new Color(0.1f, 0.1f, 0.1f), paddingTop = 10, paddingRight = 10, paddingLeft = 10, paddingBottom = 10, borderBottomWidth = 2, borderBottomColor = Color.cyan } };
            root.Add(_fieldsContainer);
            RefreshFieldsUI();

            // Add Field Button
            var addBtn = new Button(() => { _fields.Add(new ShaderFieldDef()); RefreshFieldsUI(); })
            {
                text = "+ ADD FIELD",
                style = { width = 120, marginTop = 10, backgroundColor = new Color(0.2f, 0.4f, 0.2f) }
            };
            root.Add(addBtn);

            // Spacer
            root.Add(new VisualElement { style = { flexGrow = 1 } });

            // THE MIC DROP BUTTON
            var compileBtn = new Button(() => CompileToGPU())
            {
                text = "COMPILE TO RAW GPU MACHINE CODE",
                style = { height = 50, backgroundColor = new Color(0.8f, 0.2f, 0.1f), color = Color.white, fontSize = 16, unityFontStyleAndWeight = FontStyle.Bold }
            };
            root.Add(compileBtn);

            return root;
        }

        private void RefreshFieldsUI()
        {
            _fieldsContainer.Clear();
            for (int i = 0; i < _fields.Count; i++)
            {
                int index = i;
                var row = new VisualElement { style = { flexDirection = FlexDirection.Row, marginBottom = 5 } };

                var nameField = new TextField { value = _fields[index].Name, style = { width = 200 } };
                nameField.RegisterValueChangedCallback(e => _fields[index].Name = e.newValue);

                // Using EnumField for UI Toolkit
                var typeField = new EnumField(_fields[index].DataType) { style = { width = 150 } };
                typeField.RegisterValueChangedCallback(e => _fields[index].DataType = (GpuDataType)e.newValue);

                var delBtn = new Button(() => { _fields.RemoveAt(index); RefreshFieldsUI(); }) { text = "X", style = { width = 30, backgroundColor = Color.red } };

                row.Add(nameField);
                row.Add(typeField);
                row.Add(delBtn);
                _fieldsContainer.Add(row);
            }
        }

        // --- THE COMPILER ENGINE ---
        private void CompileToGPU()
        {
            string folderPath = "Assets/GeneratedGPU";
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            GenerateCSharpStruct(folderPath);
            GenerateComputeShader(folderPath);

            AssetDatabase.Refresh();
            Debug.Log($"<color=cyan><b>MIC DROP:</b></color> Generated GPU Memory Layout for {_agentName}!");
        }

        private void GenerateCSharpStruct(string folder)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("// AUTO-GENERATED BY COMPUTE SHADER FORGE");
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine();
            sb.AppendLine($"public struct {_agentName}");
            sb.AppendLine("{");

            foreach (var f in _fields)
            {
                string csType = GetCSharpType(f.DataType);
                sb.AppendLine($"    public {csType} {f.Name};");
            }

            sb.AppendLine("}");

            File.WriteAllText($"{folder}/{_agentName}.cs", sb.ToString());
        }

        private void GenerateComputeShader(string folder)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("// AUTO-GENERATED BY COMPUTE SHADER FORGE");
            sb.AppendLine("#pragma kernel CSMain");
            sb.AppendLine();

            // Build the HLSL Struct matching C# exactly
            sb.AppendLine($"struct {_agentName} {{");
            foreach (var f in _fields)
            {
                string hlslType = GetHLSLType(f.DataType);
                sb.AppendLine($"    {hlslType} {f.Name};");
            }
            sb.AppendLine("};");
            sb.AppendLine();

            // Setup the massive data buffer
            sb.AppendLine($"RWStructuredBuffer<{_agentName}> _AgentBuffer;");
            sb.AppendLine();

            // The Dispatch Kernel
            sb.AppendLine("[numthreads(64, 1, 1)]");
            sb.AppendLine("void CSMain (uint3 id : SV_DispatchThreadID)");
            sb.AppendLine("{");
            sb.AppendLine($"    {_agentName} agent = _AgentBuffer[id.x];");
            sb.AppendLine();
            sb.AppendLine("    // --- FSM LOGIC INJECTED HERE LATER ---");
            sb.AppendLine();
            sb.AppendLine("    // Write back to memory");
            sb.AppendLine("    _AgentBuffer[id.x] = agent;");
            sb.AppendLine("}");

            File.WriteAllText($"{folder}/{_agentName}Sim.compute", sb.ToString());
        }

        // Type Translators
        private string GetCSharpType(GpuDataType t) => t switch
        {
            GpuDataType.Float => "float",
            GpuDataType.Int => "int",
            GpuDataType.Float2 => "Vector2",
            GpuDataType.Float3 => "Vector3",
            GpuDataType.Float4 => "Vector4",
            GpuDataType.Bool => "int", // Bools are tricky in compute buffers, use int
            _ => "float"
        };

        private string GetHLSLType(GpuDataType t) => t switch
        {
            GpuDataType.Float => "float",
            GpuDataType.Int => "int",
            GpuDataType.Float2 => "float2",
            GpuDataType.Float3 => "float3",
            GpuDataType.Float4 => "float4",
            GpuDataType.Bool => "int",
            _ => "float"
        };

        // --- IGuiProvider Fulfillment ---
        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}