#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Scripts;
using Assets.Scripts.Builders.GuiBuilders;

public class TransitionBuilderEditorWindow : EditorWindow
{
    [MenuItem("Tools/The Singularity Workshop/Transition Builder")]
    public static void ShowWindow() => GetWindow<TransitionBuilderEditorWindow>("Transition Builder");

    private List<string> _availableStates = new();
    private List<string> _availableConditions = new();
    private string _cachePath;

    private void OnEnable()
    {
        _cachePath = Path.Combine(Application.persistentDataPath, "TSW_Registry_Cache");
        LoadRegistries();
        RefreshUI();
    }

    private void LoadRegistries()
    {
        // Transitions pull 'States' as targets and 'Conditions' as rules
        _availableStates = LoadFile("StateRegistry.txt");
        _availableConditions = LoadFile("ConditionRegistry.txt");
    }

    private List<string> LoadFile(string fileName)
    {
        string path = Path.Combine(_cachePath, fileName);
        return File.Exists(path) ? File.ReadAllLines(path).ToList() : new List<string> { "None" };
    }

    private void RefreshUI()
    {
        var builder = new GraphicalUserInterfaceBuilder("TransitionEditor")
            .WithTitle("Transition Logic Designer")
            .WithPadding(12);

        builder.AddChild(ctx => new PopupField<string>("From State", _availableStates, 0));
        builder.AddChild(ctx => new PopupField<string>("To State", _availableStates, 0));
        builder.AddChild(ctx => new PopupField<string>("Condition Rule", _availableConditions, 0));

        // metaDev Metric Display
        builder.AddChild(ctx => {
            var stats = new VisualElement { style = { marginTop = 20, paddingBottom = 5, paddingLeft = 5, paddingRight = 5, paddingTop = 5,backgroundColor = new Color(0.1f, 0.1f, 0.1f) } };
            stats.Add(new Label("metaDev Telemetry") { style = { unityFontStyleAndWeight = FontStyle.Bold } });
            stats.Add(new Label("• Avg Evaluation Time: 0.02ms"));
            stats.Add(new Label("• Global Transition Count: " + _availableConditions.Count));
            return stats;
        });

        rootVisualElement.Clear();
        rootVisualElement.Add(builder.Build());
    }
}
#endif