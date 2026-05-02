using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.FSM_API;
using Workshop.UI_And_Tools.Forge.IO; // <-- Injected native Forge IO

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    public enum GpuDataType { Float, Int, Float2, Float3, Float4, Bool }

    public class ShaderFieldDef
    {
        public string Name = "NewVariable";
        public GpuDataType DataType = GpuDataType.Float;
    }

    public class RuntimeShaderControl { public string Name; public float Min, Max; }

    public class ComputeShaderGuiBuilder : IGuiProvider
    {
        public string Title => "COMPUTE SHADER FORGE: SWARMY EDITION";

        private List<ShaderFieldDef> _fields = new List<ShaderFieldDef>();
        private List<string> _fsmStates = new List<string> { "Patrol_Eyes", "Decoy_Fake", "Weapon_Strike", "RTB_Recharge" };
        private string _agentName = "SwarmDrone";
        private ComputeShader _simShader;
        private Dictionary<string, string> _uniformBindings = new Dictionary<string, string>();

        // Runtime Storage
        private List<RuntimeShaderControl> _sliders = new List<RuntimeShaderControl>();
        private Dictionary<string, ComputeBuffer> _buffers = new Dictionary<string, ComputeBuffer>();

        private VisualElement _fieldsContainer;
        private VisualElement _statesContainer;
        private GuiContext _lastCtx;

        public ComputeShaderGuiBuilder() { DefaultFields(); }
        public ComputeShaderGuiBuilder(ComputeShader shader)
        {
            _simShader = shader;
            DefaultFields();
        }

        private void DefaultFields()
        {
            _fields.Add(new ShaderFieldDef { Name = "Position", DataType = GpuDataType.Float3 });
            _fields.Add(new ShaderFieldDef { Name = "Velocity", DataType = GpuDataType.Float3 });
            _fields.Add(new ShaderFieldDef { Name = "StateId", DataType = GpuDataType.Int });
            _fields.Add(new ShaderFieldDef { Name = "Battery", DataType = GpuDataType.Float });
        }

        public ComputeShaderGuiBuilder WithBuffer(string name, ComputeBuffer buffer)
        {
            _buffers[name] = buffer;
            return this;
        }

        public ComputeShaderGuiBuilder WithSlider(string name, float min, float max)
        {
            _sliders.Add(new RuntimeShaderControl { Name = name, Min = min, Max = max });
            return this;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx ?? new GuiContext();

            var rootBuilder = new ForgeContainerBuilder("GpuArchitectRoot")
                .WithFlexGrow(1)
                .WithPadding(25)
                .WithBackgroundColor(new Color(0.05f, 0.02f, 0.06f)); // Deep Void

            // ==========================================
            // COMPILER SECTION
            // ==========================================
            rootBuilder.AddChild(new ForgeLabelBuilder("GPU DOCTRINE COMPILER")
                .WithFontSize(24).WithColor(Color.cyan).WithBold().WithMarginBottom(20)
                .OnBuild(ve => ve.style.letterSpacing = 2));

            var nameRow = new ForgeContainerBuilder("NameRow")
                .WithDirection(FlexDirection.Row).WithAlignItems(Align.Center).WithMarginBottom(15)
                .AddChild(new ForgeLabelBuilder("Agent Struct Name: ").WithWidth(150).WithColor(Color.white).WithBold())
                .AddChild(new ForgeTextFieldBuilder("", _agentName)
                    .WithFlexGrow(1).OnChanged(val => _agentName = val));

            rootBuilder.AddChild(nameRow);

            var statesPanel = new ForgeContainerBuilder("StatesPanel")
                .WithPadding(15).WithMarginBottom(15).WithBorderColor(Color.magenta).WithBorderWidth(1)
                .WithBackgroundColor(new Color(0.1f, 0.05f, 0.1f))
                .OnBuild(ve => _statesContainer = ve);
            rootBuilder.AddChild(statesPanel);

            var fieldsPanel = new ForgeContainerBuilder("FieldsPanel")
                .WithPadding(15).WithMarginBottom(20).WithBorderColor(Color.cyan).WithBorderWidth(1)
                .WithBackgroundColor(new Color(0.05f, 0.1f, 0.1f))
                .OnBuild(ve => _fieldsContainer = ve);
            rootBuilder.AddChild(fieldsPanel);

            rootBuilder.AddChild(new ForgeButtonBuilder(">> COMPILE MACHINE CODE <<", CompileToGPU)
                .WithHeight(45).WithBold().WithBackgroundColor(new Color(0.6f, 0.1f, 0.6f)).WithTextColor(Color.white)
                .OnBuild(ve => ve.style.letterSpacing = 2));

            // ==========================================
            // RUNTIME CONTROLS SECTION
            // ==========================================
            if (_sliders.Count > 0)
            {
                var runtimeBox = new ForgeContainerBuilder("RuntimeControls")
                    .WithPadding(15).WithMarginTop(30).WithBackgroundColor(new Color(0.15f, 0.15f, 0.15f))
                    .WithBorderColor(Color.yellow).WithBorderTopWidth(2)
                    .AddChild(new ForgeLabelBuilder("LIVE SHADER PARAMETERS").WithColor(Color.yellow).WithBold().WithMarginBottom(15));

                foreach (var s in _sliders)
                {
                    var sliderRow = new ForgeContainerBuilder($"Slider_{s.Name}").WithDirection(FlexDirection.Row).WithAlignItems(Align.Center).WithMarginBottom(5);
                    sliderRow.AddChild(new ForgeLabelBuilder(s.Name).WithWidth(120).WithColor(Color.white));
                    sliderRow.OnBuild(ve => {
                        var slider = new Slider(s.Min, s.Max) { style = { flexGrow = 1 } };
                        slider.RegisterValueChangedCallback(evt => { if (_simShader != null) _simShader.SetFloat(s.Name, evt.newValue); });
                        ve.Add(slider);
                    });
                    runtimeBox.AddChild(sliderRow);
                }
                rootBuilder.AddChild(runtimeBox);
            }

            var root = rootBuilder.CreateGui(_lastCtx);
            RefreshStatesUI(_lastCtx);
            RefreshFieldsUI(_lastCtx);
            return root;
        }

        private void RefreshStatesUI(GuiContext ctx)
        {
            if (_statesContainer == null) return;
            _statesContainer.Clear();
            _statesContainer.Add(new ForgeLabelBuilder("FSM STATE ENUMS").WithColor(Color.magenta).WithBold().WithMarginBottom(10).CreateGui(ctx));

            for (int i = 0; i < _fsmStates.Count; i++)
            {
                int index = i;
                var row = new ForgeContainerBuilder($"State_{index}").WithDirection(FlexDirection.Row).WithAlignItems(Align.Center).WithMarginBottom(4)
                    .AddChild(new ForgeTextFieldBuilder("", _fsmStates[index]).WithFlexGrow(1).OnChanged(val => _fsmStates[index] = val))
                    .AddChild(new ForgeButtonBuilder("×", () => { _fsmStates.RemoveAt(index); RefreshStatesUI(ctx); })
                        .WithBackgroundColor(Color.clear).WithTextColor(Color.red).WithWidth(30));
                _statesContainer.Add(row.CreateGui(ctx));
            }

            _statesContainer.Add(new ForgeButtonBuilder("+ ADD STATE", () => { _fsmStates.Add("NewState"); RefreshStatesUI(ctx); })
                .WithMarginTop(10).WithBackgroundColor(new Color(0.2f, 0.05f, 0.2f)).WithTextColor(Color.magenta).CreateGui(ctx));
        }

        private void RefreshFieldsUI(GuiContext ctx)
        {
            if (_fieldsContainer == null) return;
            _fieldsContainer.Clear();
            _fieldsContainer.Add(new ForgeLabelBuilder("BLITTABLE STRUCT LAYOUT").WithColor(Color.cyan).WithBold().WithMarginBottom(10).CreateGui(ctx));

            for (int i = 0; i < _fields.Count; i++)
            {
                int index = i;
                var row = new ForgeContainerBuilder($"Field_{index}").WithDirection(FlexDirection.Row).WithAlignItems(Align.Center).WithMarginBottom(4)
                    .AddChild(new ForgeTextFieldBuilder("", _fields[index].Name).WithFlexGrow(1).WithMarginRight(10).OnChanged(val => _fields[index].Name = val))
                    .AddChild(new ForgeLabelBuilder(_fields[index].DataType.ToString()).WithWidth(80).WithColor(Color.cyan))
                    .AddChild(new ForgeButtonBuilder("×", () => { _fields.RemoveAt(index); RefreshFieldsUI(ctx); })
                        .WithBackgroundColor(Color.clear).WithTextColor(Color.red).WithWidth(30));
                _fieldsContainer.Add(row.CreateGui(ctx));
            }

            _fieldsContainer.Add(new ForgeButtonBuilder("+ ADD FIELD", () => { _fields.Add(new ShaderFieldDef()); RefreshFieldsUI(ctx); })
                .WithMarginTop(10).WithBackgroundColor(new Color(0.05f, 0.2f, 0.2f)).WithTextColor(Color.cyan).CreateGui(ctx));
        }

        [Serializable]
        public struct GPUCompilerPayload
        {
            public string TargetAgent;
            public string CSharpCode;
            public string ComputeCode;
        }

        // ==========================================
        // FORGE I/O COMPILATION LOGIC
        // ==========================================
        private void CompileToGPU()
        {
            string csharpCode = GenerateCSharpStruct();
            string computeCode = GenerateComputeShader();

            // 1. Pack the payload
            var payload = new GPUCompilerPayload
            {
                TargetAgent = _agentName,
                CSharpCode = csharpCode,
                ComputeCode = computeCode
            };

            // 2. Dispatch through the authentic, zero-boxing SingularityDataBus
            SingularityDataBus.Instance.SendLocal("CompileGpuDoctrine", payload);

#if UNITY_EDITOR
            // 3. If in Editor, let the ManifestationEngine physically write the artifacts
            string folderPath = "Assets/GeneratedGPU/Swarmy";
            ManifestationEngine.EnsureDirectoryExists(folderPath);

            ManifestationEngine.WriteArtifact($"{folderPath}/{_agentName}.cs", csharpCode);
            ManifestationEngine.WriteArtifact($"{folderPath}/{_agentName}Sim.compute", computeCode);

            Debug.Log($"<color=cyan><b>MIC DROP:</b></color> GPU Doctrine manifested to disk for {_agentName}!");
#else
            Debug.Log($"<color=cyan><b>MIC DROP:</b></color> GPU Doctrine compiled to DataBus for {_agentName}! (Runtime execution only)");
#endif
        }

        private string GenerateCSharpStruct()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using System.Runtime.InteropServices;");
            sb.AppendLine();
            sb.AppendLine($"public enum {_agentName}State {{ {string.Join(", ", _fsmStates)} }}");
            sb.AppendLine();
            sb.AppendLine("[StructLayout(LayoutKind.Sequential)]");
            sb.AppendLine($"public struct {_agentName}");
            sb.AppendLine("{");
            foreach (var f in _fields) sb.AppendLine($"    public {GetCSharpType(f.DataType)} {f.Name};");
            sb.AppendLine("}");
            return sb.ToString();
        }

        private string GenerateComputeShader()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("#pragma kernel CSMain");
            sb.AppendLine();
            sb.AppendLine($"struct {_agentName}");
            sb.AppendLine("{");
            foreach (var f in _fields) sb.AppendLine($"    {GetHLSLType(f.DataType)} {f.Name};");
            sb.AppendLine("};");
            sb.AppendLine();
            sb.AppendLine($"RWStructuredBuffer<{_agentName}> _AgentBuffer;");
            sb.AppendLine("StructuredBuffer<float4> _AttractorBuffer;");
            sb.AppendLine("uint _AttractorCount;");
            sb.AppendLine("float _DeltaTime;");
            sb.AppendLine();
            sb.AppendLine("[numthreads(64, 1, 1)]");
            sb.AppendLine("void CSMain (uint3 id : SV_DispatchThreadID)");
            sb.AppendLine("{");
            sb.AppendLine($"    {_agentName} agent = _AgentBuffer[id.x];");
            sb.AppendLine("    // [FSM LOGIC INJECTED]");
            sb.AppendLine("    _AgentBuffer[id.x] = agent;");
            sb.AppendLine("}");
            return sb.ToString();
        }

        public ComputeShaderGuiBuilder BindUniform(string shaderVariable, string uiLabelValue)
        {
            _uniformBindings[shaderVariable] = uiLabelValue;
            return this;
        }

        private string GetCSharpType(GpuDataType t) => t switch { GpuDataType.Float3 => "Vector3", GpuDataType.Int => "int", _ => "float" };
        private string GetHLSLType(GpuDataType t) => t switch { GpuDataType.Float3 => "float3", GpuDataType.Int => "int", _ => "float" };

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));

        // Let the DataBus/ManifestationEngine handle dynamic serialization instead of hardcoded UIDocuments
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}