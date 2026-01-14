#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ExperienceBuilderEditorWindow : EditorWindow
{
    [MenuItem("Tools/Builders/Experience Manifest Editor")]
    public static void ShowWindow()
    {
        var wnd = GetWindow<ExperienceBuilderEditorWindow>();
        wnd.titleContent = new GUIContent("Experience Manifest");
        wnd.minSize = new Vector2(520, 480);
    }

    private TextField idField;
    private TextField nameField;
    private TextField descField;
    private VisualElement sensesContainer;
    private List<SenseEntry> entries = new List<SenseEntry>();

    private void OnEnable()
    {
        rootVisualElement.Clear();

        var root = new VisualElement { name = "ExperienceBuilderRoot" };

        // Title
        var title = new Label("Experience Manifest Editor")
        {
            style =
            {
                unityFontStyleAndWeight = FontStyle.Bold,
                fontSize = 14,
                marginBottom = 6
            }
        };
        root.Add(title);

        // ID/Name/Description
        idField = new TextField("ID (file name)") { isDelayed = true, value = "vision_audio_touch" };
        nameField = new TextField("Display Name") { isDelayed = true, value = "Vision + Audio + Touch Demo" };
        descField = new TextField("Description") { isDelayed = true, multiline = true, value = "A simple demo containing Vision, Audio and Touch senses." };

        root.Add(idField);
        root.Add(nameField);
        root.Add(descField);

        // Senses header and add button
        var sensesHeader = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center } };
        sensesHeader.Add(new Label("Senses") { style = { unityFontStyleAndWeight = FontStyle.Bold, flexGrow = 1 } });
        var addSenseBtn = new Button(() => AddSenseEntry(Sense.Vision, "DefaultVision", ""))
        { text = "Add Sense", tooltip = "Add a new sense entry" };
        sensesHeader.Add(addSenseBtn);
        root.Add(sensesHeader);

        // Container
        sensesContainer = new VisualElement { name = "sensesContainer", style = { flexDirection = FlexDirection.Column, marginTop = 6 } };
        root.Add(sensesContainer);

        // Pre-populate defaults
        AddSenseEntry(Sense.Vision, "DefaultVision", "resolution=1920x1080;ambientColor=#FFFFFF");
        AddSenseEntry(Sense.Audio, "SpatialAudio", "ambientClip=ambient_loop.wav;volume=0.8");
        AddSenseEntry(Sense.Touch, "HapticsV1", "pattern=gentle_pulse;intensity=0.7");

        // Save / Export buttons row
        var buttonsRow = new VisualElement { style = { flexDirection = FlexDirection.Row, marginTop = 10 } };

        var saveBtn = new Button(() =>
        {
            if (string.IsNullOrWhiteSpace(idField.value))
            {
                Debug.LogError("Please provide an ID (used as filename) before saving.");
                return;
            }

            try
            {
                var builder = new ExperienceGuiBuilder()
                    .WithId(idField.value.Trim())
                    .WithName(nameField.value?.Trim())
                    .WithDescription(descField.value?.Trim());

                foreach (var e in entries)
                {
                    var config = ParseConfigString(e.ConfigField.value);
                    // Fix: pass the Sense enum value (cast) rather than the EnumField itself
                    builder.AddSense((Sense)e.SenseField.value, e.ProviderField.value, config);
                }

                // Save under StreamingAssets/Experiences/<id>.json
                builder.SaveToStreamingAssets(idField.value.Trim());
                EditorUtility.DisplayDialog("Saved", $"Manifest saved to StreamingAssets/Experiences/{idField.value.Trim()}.json", "OK");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save manifest: {ex}");
            }
        })
        { text = "Save to StreamingAssets" };

        var exportJsonBtn = new Button(() =>
        {
            try
            {
                var manifest = new ExperienceGuiBuilder()
                    .WithId(idField.value.Trim())
                    .WithName(nameField.value?.Trim())
                    .WithDescription(descField.value?.Trim());

                foreach (var e in entries)
                {
                    var config = ParseConfigString(e.ConfigField.value);
                    // Fix: cast EnumField value to Sense
                    manifest.AddSense((Sense)e.SenseField.value, e.ProviderField.value, config);
                }

                var json = manifest.BuildJson();
                var path = EditorUtility.SaveFilePanel("Export Manifest JSON", Application.dataPath, idField.value.Trim() + ".json", "json");
                if (!string.IsNullOrWhiteSpace(path))
                {
                    File.WriteAllText(path, json);
                    EditorUtility.DisplayDialog("Exported", $"Manifest exported to {path}", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Export failed: {ex}");
            }
        })
        { text = "Export JSON..." };

        buttonsRow.Add(saveBtn);
        buttonsRow.Add(exportJsonBtn);
        root.Add(buttonsRow);

        rootVisualElement.Add(root);
    }

    // Adds one UI row representing a Sense entry
    private void AddSenseEntry(Sense initialSense, string provider, string config)
    {
        var container = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 4 } };

        var senseField = new EnumField(initialSense) { style = { width = 140 } };
        var providerField = new TextField("Provider") { isDelayed = true, value = provider, style = { flexGrow = 1, marginLeft = 6 } };
        var configField = new TextField("Config (k=v;k2=v2)") { isDelayed = true, value = config, style = { width = 260, marginLeft = 6 } };
        var removeBtn = new Button(() =>
        {
            sensesContainer.Remove(container);
            entries.RemoveAll(x => x.Root == container);
        })
        { text = "Remove", style = { marginLeft = 6 } };

        // Hide labels for compactness; keep providers/config labels in tooltip
        senseField.label = "";
        providerField.label = "";
        configField.label = "";
        providerField.tooltip = "Provider name (e.g., DefaultVision, SpatialAudio, HapticsV1)";
        configField.tooltip = "Configuration as k=v pairs separated by ';'";

        container.Add(senseField);
        container.Add(providerField);
        container.Add(configField);
        container.Add(removeBtn);

        sensesContainer.Add(container);

        entries.Add(new SenseEntry
        {
            Root = container,
            SenseField = senseField,
            ProviderField = providerField,
            ConfigField = configField
        });
    }

    private static Dictionary<string, string> ParseConfigString(string raw)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(raw)) return dict;

        var parts = raw.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var p in parts)
        {
            var idx = p.IndexOf('=');
            if (idx > 0)
            {
                var k = p.Substring(0, idx).Trim();
                var v = p.Substring(idx + 1).Trim();
                if (!string.IsNullOrWhiteSpace(k))
                    dict[k] = v;
            }
        }
        return dict;
    }

    private class SenseEntry
    {
        public VisualElement Root;
        public EnumField SenseField;
        public TextField ProviderField;
        public TextField ConfigField;
    }
}
#endif
