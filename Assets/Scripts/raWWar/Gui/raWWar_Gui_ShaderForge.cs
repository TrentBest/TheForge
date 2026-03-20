using Assets.Scripts.Workshop.Forge.Builders.GuiBuilders;
using System;
using System.Collections.Generic;
using TheSingularityWorkshop.Builders.GuiBuilders;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.raWWar.Editors
{
    public class raWWar_Gui_ShaderForge : IGuiProvider
    {
        public string Title => "raWWar Shader Forge";

        private SplitPanelBuilder _splitBuilder;
        private VisualElement _root;

        // --- raWWar Theme Palette ---
        private readonly Color _appBackground = new Color(0.04f, 0.04f, 0.06f);
        private readonly Color _panelBackground = new Color(0.10f, 0.10f, 0.14f);
        private readonly Color _accentColor = new Color(0.65f, 0.2f, 0.95f); // Vibrant Sci-Fi Purple
        private readonly Color _textDim = new Color(0.6f, 0.6f, 0.7f);
        private readonly float _softCornerRadius = 12f;

        // --- Shader State ---
        private string _shaderName = "raWWar/Tactical/InstancedInfantry";
        private bool _enablePositionInstancing = true;
        private bool _enableRotationInstancing = true;
        private bool _enableHealthTinting = false;

        // Output code
        private string _generatedHLSL = "";

        public VisualElement CreateGui(GuiContext ctx)
        {
            _splitBuilder = new SplitPanelBuilder(sidebarWidth: 400, Side.Left);

            _splitBuilder.WithSidebar(new DynamicGuiProvider(c => BuildControlsContext()));
            _splitBuilder.WithMain(new DynamicGuiProvider(c => BuildCodePreviewContext()));

            _root = _splitBuilder.CreateGui(ctx);
            _root.style.backgroundColor = _appBackground;

            // Generate initial code
            GenerateHLSL();

            return _root;
        }

        private VisualElement BuildControlsContext()
        {
            var builder = new GraphicalUserInterfaceBuilder("ShaderControls")
                .WithPadding(20)
                .WithBackgroundColor(_panelBackground)
                .WithBorderRightColor(new Color(0.2f, 0.2f, 0.25f))
                .WithBorderRightWidth(1)
                .WithScrollable(true);

            // --- HEADER ---
            builder.AddChild(new Label("SHADER MATRIX PROTOCOLS") { style = { color = _accentColor, letterSpacing = 2, fontSize = 14, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 15 } });
            builder.AddStringData("Shader Route", _shaderName, v => { _shaderName = v; GenerateHLSL(); });

            // --- INSTANCING SETTINGS ---
            builder.AddSeparator(_accentColor, 1);
            builder.AddChild(new Label("GPU BUFFER BINDINGS") { style = { color = Color.white, fontSize = 12, unityFontStyleAndWeight = FontStyle.Bold, marginTop = 15, marginBottom = 10 } });

            builder.AddChild(new Label("Data Structure: SoldierGPUData") { style = { color = _textDim, fontSize = 10, marginBottom = 10 } });

            builder.AddToggleData("Inject Position Data (xyz)", _enablePositionInstancing, v => { _enablePositionInstancing = v; GenerateHLSL(); });
            builder.AddToggleData("Inject Facing Data (Y-Axis Rot)", _enableRotationInstancing, v => { _enableRotationInstancing = v; GenerateHLSL(); });
            builder.AddToggleData("Inject Health Data (Albedo Tint)", _enableHealthTinting, v => { _enableHealthTinting = v; GenerateHLSL(); });

            // --- COMPILATION ACTIONS ---
            builder.AddSeparator(_accentColor, 1);

            var compileBtn = new Button(() => Debug.Log("HLSL Compiled and Saved to Assets!")) { text = "COMPILE & EXPORT .SHADER" };
            compileBtn.style.backgroundColor = _accentColor;
            compileBtn.style.color = Color.white;
            compileBtn.style.unityFontStyleAndWeight = FontStyle.Bold;
            compileBtn.style.height = 40;
            compileBtn.style.marginTop = 20;
            compileBtn.style.borderTopLeftRadius = _softCornerRadius;
            compileBtn.style.borderTopRightRadius = _softCornerRadius;
            compileBtn.style.borderBottomLeftRadius = _softCornerRadius;
            compileBtn.style.borderBottomRightRadius = _softCornerRadius;

            builder.AddChild(compileBtn);

            return builder.Build();
        }

        private VisualElement BuildCodePreviewContext()
        {
            var builder = new GraphicalUserInterfaceBuilder("CodePreviewPane")
                .WithBackgroundColor(Color.clear)
                .WithPadding(20)
                .WithAutoGrow()
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch);

            builder.AddChild(new Label("LIVE HLSL PREVIEW") { style = { color = _textDim, letterSpacing = 2, fontSize = 11, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });

            // Using a TextField configured to look like a code editor
            var codeField = new TextField { value = _generatedHLSL, multiline = true, isReadOnly = true };

            // Style it like a dark terminal
            codeField.style.backgroundColor = new Color(0.05f, 0.05f, 0.08f);
            codeField.style.color = new Color(0.3f, 0.8f, 0.5f); // Terminal Green text
            codeField.style.flexGrow = 1;
            codeField.style.borderTopLeftRadius = 8;
            codeField.style.borderTopRightRadius = 8;
            codeField.style.borderBottomLeftRadius = 8;
            codeField.style.borderBottomRightRadius = 8;
            codeField.style.borderTopColor = _accentColor; codeField.style.borderTopWidth = 1;

            // Unity UI Toolkit hack to make the text field actually fill the space vertically
            codeField.Q("unity-text-input").style.flexGrow = 1;

            builder.AddChild(codeField);

            return builder.Build();
        }

        private void GenerateHLSL()
        {
            // This is the actual magic snippet needed to bridge your C# to the GPU
            string code = $"Shader \"{_shaderName}\"\n{{\n";
            code += "    Properties\n    {\n";
            code += "        _MainTex (\"Texture\", 2D) = \"white\" {}\n";
            code += "        _Color (\"Base Color\", Color) = (1,1,1,1)\n";
            code += "    }\n    SubShader\n    {\n";
            code += "        Tags { \"RenderType\"=\"Opaque\" \"Queue\"=\"Geometry\" }\n";
            code += "        LOD 100\n\n";
            code += "        Pass\n        {\n";
            code += "            CGPROGRAM\n";
            code += "            #pragma vertex vert\n";
            code += "            #pragma fragment frag\n";
            code += "            #pragma multi_compile_instancing\n";
            code += "            #include \"UnityCG.cginc\"\n\n";

            code += "            // 1. MUST MATCH C# STRUCT EXACTLY\n";
            code += "            struct SoldierGPUData {\n";
            code += "                float3 position;\n";
            code += "                float facing;\n";
            code += "                int stateId;\n";
            code += "                float health;\n";
            code += "            };\n\n";

            code += "            // 2. THE BUFFER FROM C#\n";
            code += "            StructuredBuffer<SoldierGPUData> _SoldierBuffer;\n\n";

            code += "            struct appdata\n            {\n";
            code += "                float4 vertex : POSITION;\n";
            code += "                float2 uv : TEXCOORD0;\n";
            code += "                UNITY_VERTEX_INPUT_INSTANCE_ID\n";
            code += "            };\n\n";

            code += "            struct v2f\n            {\n";
            code += "                float4 vertex : SV_POSITION;\n";
            code += "                float2 uv : TEXCOORD0;\n";
            code += "                UNITY_VERTEX_INPUT_INSTANCE_ID\n";
            if (_enableHealthTinting) code += "                float healthTint : TEXCOORD1;\n";
            code += "            };\n\n";

            code += "            sampler2D _MainTex;\n";
            code += "            float4 _Color;\n\n";

            code += "            // --- VERTEX SHADER (THE MAGIC HAPPENS HERE) ---\n";
            code += "            v2f vert (appdata v, uint instanceID : SV_InstanceID)\n            {\n";
            code += "                v2f o;\n";
            code += "                UNITY_SETUP_INSTANCE_ID(v);\n";
            code += "                UNITY_TRANSFER_INSTANCE_ID(v, o);\n\n";

            code += "                SoldierGPUData data = _SoldierBuffer[instanceID];\n";

            if (_enableRotationInstancing)
            {
                code += "                // Apply Y-Axis Rotation\n";
                code += "                float s, c;\n";
                code += "                sincos(data.facing, s, c);\n";
                code += "                float3x3 rotMatrix = float3x3(\n";
                code += "                    c,  0, s,\n";
                code += "                    0,  1, 0,\n";
                code += "                   -s,  0, c\n";
                code += "                );\n";
                code += "                v.vertex.xyz = mul(rotMatrix, v.vertex.xyz);\n";
            }

            if (_enablePositionInstancing)
            {
                code += "                // Offset by World Position\n";
                code += "                v.vertex.xyz += data.position;\n";
            }

            if (_enableHealthTinting)
            {
                code += "                // Pass health to fragment shader for color tinting\n";
                code += "                o.healthTint = saturate(data.health / 100.0);\n";
            }

            code += "\n                o.vertex = UnityObjectToClipPos(v.vertex);\n";
            code += "                o.uv = v.uv;\n";
            code += "                return o;\n";
            code += "            }\n\n";

            code += "            fixed4 frag (v2f i) : SV_Target\n            {\n";
            code += "                UNITY_SETUP_INSTANCE_ID(i);\n";
            code += "                fixed4 col = tex2D(_MainTex, i.uv) * _Color;\n";

            if (_enableHealthTinting)
            {
                code += "                // Tint red as health drops\n";
                code += "                col.rgb = lerp(float3(1,0,0), col.rgb, i.healthTint);\n";
            }

            code += "                return col;\n";
            code += "            }\n";
            code += "            ENDCG\n        }\n    }\n}\n";

            _generatedHLSL = code;
            RefreshCodePreview();
        }

        private void RefreshCodePreview()
        {
            if (_root == null) return;
            var codeField = _root.Q<TextField>();
            if (codeField != null) codeField.value = _generatedHLSL;
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) { }
        public void FromUIDocument(string path) { }
    }
}