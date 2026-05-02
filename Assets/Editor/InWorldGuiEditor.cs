using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

[CustomEditor(typeof(InWorldGuiBuilder))]
public class InWorldGuiEditor : Editor
{
    private List<Type> _providerTypes;
    private List<string> _providerNames;

    private void OnEnable()
    {
        // 1. Reflection: Find all classes that implement IGuiProvider
        var typeCollection = TypeCache.GetTypesDerivedFrom<IGuiProvider>();

        _providerTypes = typeCollection
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .ToList();

        _providerNames = _providerTypes.Select(t => t.Name).ToList();

        // Add a "None" option at the top
        _providerNames.Insert(0, "None");
        _providerTypes.Insert(0, null);
    }

    public override VisualElement CreateInspectorGUI()
    {
        var targetScript = (InWorldGuiBuilder)target;

        // Use your builder to construct the Editor UI
        var editorUi = new GraphicalUserInterfaceBuilder("InWorldEditor")
            .WithEditorMode(true)
            .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
            .WithPadding(12)
            .WithBackgroundColor(new Color(0.18f, 0.18f, 0.18f));

        // ---------------------------------------------------------
        // SECTION 1: THE SOFTWARE (GUI Provider)
        // ---------------------------------------------------------
        editorUi.AddChild(ctx =>
        {
            var container = new VisualElement { style = { marginBottom = 15 } };

            // Label
            var label = new Label("SOFTWARE CARTRIDGE")
            {
                style = {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    color = new Color(0.7f, 0.9f, 1f), // Cyan-ish
                    marginBottom = 4
                }
            };
            container.Add(label);

            // Calculate current index
            int currentIndex = 0;
            if (!string.IsNullOrEmpty(targetScript.selectedProviderTypeName))
            {
                currentIndex = _providerNames.IndexOf(targetScript.selectedProviderTypeName);
                if (currentIndex == -1) currentIndex = 0;
            }

            // Dropdown
            var dropdown = new PopupField<string>("Active GUI", _providerNames, currentIndex);
            dropdown.RegisterValueChangedCallback(evt =>
            {
                serializedObject.Update();
                var selectedName = evt.newValue;
                targetScript.selectedProviderTypeName = selectedName == "None" ? "" : selectedName;

                // Preview Logic
                if (selectedName != "None")
                {
                    var typeIndex = _providerNames.IndexOf(selectedName);
                    var type = _providerTypes[typeIndex];
                    try
                    {
                        var instance = (IGuiProvider)Activator.CreateInstance(type);
                        targetScript.Initialize(instance);
                        targetScript.Build();
                    }
                    catch (Exception ex) { Debug.LogWarning($"Could not preview {selectedName}: {ex.Message}"); }
                }

                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(targetScript);
            });

            container.Add(dropdown);
            return container;
        });

        // ---------------------------------------------------------
        // SECTION 2: THE CONFIGURATION (Design -> Reality)
        // ---------------------------------------------------------
        editorUi.AddChild(ctx =>
        {
            var container = new VisualElement
            {
                style = {
                    backgroundColor = new Color(0.15f, 0.15f, 0.15f),
                    borderTopWidth = 1, borderTopColor = Color.black,
                    borderBottomWidth = 1, borderBottomColor = Color.black,
                    paddingTop = 10, paddingBottom = 10
                }
            };

            var settingsProp = serializedObject.FindProperty("_settings");

            // --- 1. THE DESIGN ---
            var headerDesign = new Label("1. THE DESIGN (Editor Size)")
            { style = { unityFontStyleAndWeight = FontStyle.Bold, color = Color.gray, marginBottom = 2 } };
            container.Add(headerDesign);

            var designProp = settingsProp.FindPropertyRelative("designLayoutSize");
            var designField = new Vector2IntField("Layout Resolution") { bindingPath = designProp.propertyPath };
            designField.Bind(serializedObject);
            designField.tooltip = "The resolution you designed this GUI for (e.g. 1024x768).";
            container.Add(designField);

            // Spacer
            container.Add(new VisualElement { style = { height = 10 } });

            // --- 2. THE REALITY ---
            var headerReality = new Label("2. THE REALITY (In-World Size)")
            { style = { unityFontStyleAndWeight = FontStyle.Bold, color = Color.gray, marginBottom = 2 } };
            container.Add(headerReality);

            var physProp = settingsProp.FindPropertyRelative("physicalSize");
            var physField = new Vector2Field("Physical Size (m)") { bindingPath = physProp.propertyPath };
            physField.Bind(serializedObject);
            physField.tooltip = "Physical width/height in meters. Leave at 0,0 to auto-detect from Mesh.";
            container.Add(physField);

            // Spacer
            container.Add(new VisualElement { style = { height = 10 } });

            // --- 3. THE BRIDGE ---
            var headerBridge = new Label("3. THE BRIDGE (Quality)")
            { style = { unityFontStyleAndWeight = FontStyle.Bold, color = Color.gray, marginBottom = 2 } };
            container.Add(headerBridge);

            var texProp = settingsProp.FindPropertyRelative("targetTextureWidth");
            // Using SliderInt to prevent insane values, but allowing high max
            var texField = new IntegerField("Texture Width (px)") { bindingPath = texProp.propertyPath };
            texField.Bind(serializedObject);
            texField.tooltip = "The actual sharpness (width) of the generated texture.";
            container.Add(texField);

            return container;
        });

        // ---------------------------------------------------------
        // SECTION 3: HARDWARE SPECS
        // ---------------------------------------------------------
        editorUi.AddChild(ctx =>
        {
            var container = new VisualElement { style = { marginTop = 15 } };

            var header = new Label("HARDWARE SPECS") { style = { unityFontStyleAndWeight = FontStyle.Bold, color = Color.gray } };
            container.Add(header);

            var settingsProp = serializedObject.FindProperty("_settings");

            // Display Mode
            var modeProp = settingsProp.FindPropertyRelative("mode");
            var modeField = new PropertyField(modeProp, "Mode");
            modeField.Bind(serializedObject);
            container.Add(modeField);

            // Curvature
            var curveProp = settingsProp.FindPropertyRelative("curvature");
            var curveSlider = new Slider("Curvature", 0f, 1f) { bindingPath = curveProp.propertyPath };
            curveSlider.Bind(serializedObject);
            container.Add(curveSlider);

            // Interaction Layer
            var layerProp = settingsProp.FindPropertyRelative("interactionLayer");
            var layerField = new LayerMaskField("Interaction Layer", layerProp.intValue);
            layerField.RegisterValueChangedCallback(evt =>
            {
                serializedObject.Update();
                layerProp.intValue = evt.newValue;
                serializedObject.ApplyModifiedProperties();
            });
            container.Add(layerField);

            // Manual Rebuild
            var rebuildBtn = new Button(() => targetScript.Build())
            {
                text = "Reboot System",
                style = { marginTop = 15, height = 30 }
            };
            container.Add(rebuildBtn);

            return container;
        });

        return editorUi.Build();
    }
}