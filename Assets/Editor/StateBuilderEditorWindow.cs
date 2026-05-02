#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TheSingularityWorkshop;
using TheSingularityWorkshop.Builders.GuiBuilders;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.Core.Diagnostics; // Accessing GraphicalUserInterfaceBuilder

public class StateBuilderEditorWindow : EditorWindow
{
    [MenuItem("TheSingularityWorkshop/FSMs/State Builder")]
    public static void ShowWindow()
    {
        var wnd = GetWindow<StateBuilderEditorWindow>();
        wnd.titleContent = new GUIContent("State Builder");
        wnd.minSize = new Vector2(500, 450);
    }

    private TextField _stateNameField;
    private string _cachePath;

    // Separate registries for distinct behavior categories
    private List<string> _enterBehaviors = new();
    private List<string> _updateBehaviors = new();
    private List<string> _exitBehaviors = new();

    private void OnEnable()
    {
        _cachePath = Path.Combine(Application.persistentDataPath, "TSW_Registry_Cache");
        if (!Directory.Exists(_cachePath)) Directory.CreateDirectory(_cachePath);

        InitializeRegistries();
        RefreshUI();
    }

    private void InitializeRegistries()
    {
        _enterBehaviors = LoadRegistry("EnterRegistry.txt");
        _updateBehaviors = LoadRegistry("UpdateRegistry.txt");
        _exitBehaviors = LoadRegistry("ExitRegistry.txt");
    }

    private List<string> LoadRegistry(string fileName)
    {
        string path = Path.Combine(_cachePath, fileName);
        if (!File.Exists(path))
        {
            File.WriteAllText(path, ""); // Create empty if missing
            return new List<string> { "None" };
        }

        var lines = File.ReadAllLines(path).ToList();
        if (lines.Count == 0) lines.Add("None");
        return lines;
    }

    private void RefreshUI()
    {
        rootVisualElement.Clear();

        var builder = new GraphicalUserInterfaceBuilder("StateEditorRoot")
            .WithTitle("State Behavior Gatekeeper")
            .WithPadding(12)
            .WithBackgroundColor(new Color(0.15f, 0.15f, 0.15f));

        // State Name
        builder.AddChild(ctx => {
            _stateNameField = new TextField("State Identifier") { value = "NewState" };
            return _stateNameField;
        });

        // Searchable Behavior Selectors
        builder.AddChild(ctx => new PopupField<string>("On Enter", _enterBehaviors, 0));
        builder.AddChild(ctx => new PopupField<string>("On Update", _updateBehaviors, 0));
        builder.AddChild(ctx => new PopupField<string>("On Exit", _exitBehaviors, 0));

        // Registry Management Section
        builder.AddChild(ctx => {
            var manageRow = new VisualElement { style = { flexDirection = FlexDirection.Row, marginTop = 15 } };
            manageRow.Add(new Button(SyncWithFleet) { text = "Sync with Fleet", style = { flexGrow = 1, backgroundColor = new Color(0.1f, 0.4f, 0.6f) } });
            manageRow.Add(new Button(OpenCacheFolder) { text = "📂", tooltip = "Open Local Cache" });
            return manageRow;
        });

        // CRUD / Save Actions
        builder.AddChild(ctx => {
            var saveBtn = new Button(PushStateDefinition)
            {
                text = "Register State Definition",
                style = { height = 35, marginTop = 10, backgroundColor = new Color(0.2f, 0.5f, 0.2f) }
            };
            return saveBtn;
        });

        rootVisualElement.Add(builder.Build());
    }

    private  void SyncWithFleet()
    {
        ForgeLogger.Log("Pulling latest registries from Fleet..");
        // This is where your REST abstraction consumes existing content
        // await FetchRegistryFromServer("api/registry/enter", "EnterRegistry.txt");
        // await FetchRegistryFromServer("api/registry/update", "UpdateRegistry.txt");
        // await FetchRegistryFromServer("api/registry/exit", "ExitRegistry.txt");

        InitializeRegistries();
        RefreshUI();
    }

    private void PushStateDefinition()
    {
        // This acts as the Gatekeeper, pushing new content to your server
        ForgeLogger.Log($"Pushing State '{_stateNameField.value}' to Fleet Registry..");
    }

    private void OpenCacheFolder() => EditorUtility.RevealInFinder(_cachePath);
}
#endif