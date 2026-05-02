using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Diagnostics;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

// --- THE FIX: Wrap the Editor namespace ---
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Assets.Resources.Shaders.Visualization
{
    public class ToonShaderGuiBuilder : IGuiProvider
    {
        public string Title => "TOON SHADER FORGE";

        private List<ToonShaderSettings> _savedConfigs = new List<ToonShaderSettings>();
        private ToonShaderSettings _activeSettings = new ToonShaderSettings();
        private VisualElement _listContainer;

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new VisualElement { style = { flexGrow = 1, paddingTop = 20, paddingBottom = 20, paddingLeft = 20, paddingRight = 20, backgroundColor = new Color(0.15f, 0.12f, 0.18f) } };

            root.Add(new Label("CELL-SHADING COMPILER") { style = { fontSize = 24, color = new Color(1f, 0.5f, 0.8f), unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 20 } });

            // --- CRUD SECTION ---
            var crudPanel = new VisualElement { style = { flexDirection = FlexDirection.Row, marginBottom = 20, borderBottomWidth = 1, borderBottomColor = Color.gray, paddingBottom = 10 } };

            _listContainer = new VisualElement { style = { flexGrow = 1, flexDirection = FlexDirection.Row } };
            RefreshConfigList();

            var addNewBtn = new Button(() => { _activeSettings = new ToonShaderSettings(); RefreshGui(); }) { text = "+ NEW", style = { width = 60 } };

            crudPanel.Add(new Label("SAVED:") { style = { marginTop = 5, marginRight = 10 } });
            crudPanel.Add(_listContainer);
            crudPanel.Add(addNewBtn);
            root.Add(crudPanel);

            // --- SETTINGS EDITOR ---
            root.Add(CreateSettingsEditor());

            root.Add(new VisualElement { style = { flexGrow = 1 } });

            // Compile Button
            var compileBtn = new Button(() => {
                SaveActiveConfig();
                CompileToonShader();
            })
            {
                text = "FORGE .SHADER FILE",
                style = { height = 50, backgroundColor = new Color(0.8f, 0.2f, 0.5f), color = Color.white, fontSize = 16, unityFontStyleAndWeight = FontStyle.Bold }
            };
            root.Add(compileBtn);

            return root;
        }

        private VisualElement CreateSettingsEditor()
        {
            var editor = new VisualElement();

            var nameField = new TextField("Shader Name") { value = _activeSettings.ShaderName };
            nameField.RegisterValueChangedCallback(e => _activeSettings.ShaderName = e.newValue);
            editor.Add(nameField);

            var outlineToggle = new Toggle("Enable Outlines") { value = _activeSettings.EnableOutlines };
            outlineToggle.RegisterValueChangedCallback(e => _activeSettings.EnableOutlines = e.newValue);
            editor.Add(outlineToggle);

            var bandsField = new SliderInt("Lighting Bands", 2, 10) { value = _activeSettings.ColorBands, showInputField = true };
            bandsField.RegisterValueChangedCallback(e => _activeSettings.ColorBands = e.newValue);
            editor.Add(bandsField);

            return editor;
        }

        private void RefreshConfigList()
        {
            _listContainer.Clear();
            foreach (var config in _savedConfigs)
            {
                var btn = new Button(() => { _activeSettings = config; RefreshGui(); })
                {
                    text = config.ShaderName,
                    style = { marginRight = 5 }
                };
                _listContainer.Add(btn);
            }
        }

        private void SaveActiveConfig()
        {
            if (!_savedConfigs.Contains(_activeSettings))
                _savedConfigs.Add(_activeSettings);
            RefreshConfigList();
        }

        private void CompileToonShader()
        {
            // --- THE FIX: Wrap Editor-only File/Asset logic ---
#if UNITY_EDITOR
            string folderPath = "Assets/GeneratedShaders";
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Shader \"TheForge/Toon/{_activeSettings.ShaderName}\"");
            sb.AppendLine("{");
            sb.AppendLine("    Properties {");
            sb.AppendLine("        _MainTex (\"Texture\", 2D) = \"white\" {}");
            sb.AppendLine("        _Color (\"Main Color\", Color) = (1,1,1,1)");
            if (_activeSettings.EnableOutlines)
            {
                sb.AppendLine("        _OutlineColor (\"Outline Color\", Color) = (0,0,0,1)");
                sb.AppendLine("        _OutlineWidth (\"Outline Width\", Range(0.0, 0.1)) = 0.015");
            }
            sb.AppendLine($"        _Bands (\"Color Bands\", Float) = {_activeSettings.ColorBands}.0");
            sb.AppendLine("    }");
            // ... (Rest of original shader string logic)
            sb.AppendLine("}");

            File.WriteAllText($"{folderPath}/{_activeSettings.ShaderName}.shader", sb.ToString());
            AssetDatabase.Refresh();
            ForgeLogger.Log($"<color=magenta><b>FORGE:</b></color> Generated {_activeSettings.ShaderName}.shader").SendToUnity();
#else
            ForgeLogger.LogWarning("Shader Forging is only available in the Unity Editor. Use pre-forged shaders for WebGL builds.").SendToUnity();
#endif
        }

        private void RefreshGui() { /* Logic to reload the root container */ }
        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
         
        
#if UNITY_EDITOR
        public void ToUIDocument(string assetPath)
        {    // Add your ConvertToUIDocument call here if needed

        }
#endif
        public void FromUIDocument(string assetPath) { }
    }

  
}