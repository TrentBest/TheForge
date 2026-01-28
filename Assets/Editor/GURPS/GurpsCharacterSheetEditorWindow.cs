#if UNITY_EDITOR
using Assets.Scripts;
using Assets.Scripts.Builders.GuiBuilders;
using Assets.Scripts.Builders.GuiBuilders.PanelBuilders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class GurpsCharacterSheetEditorWindow : EditorWindow
{
    [MenuItem("Singularity/GURPS Character Forge")]
    public static void ShowWindow()
    {
        var window = GetWindow<GurpsCharacterSheetEditorWindow>("Character Forge");
        //var display = Display.displays[0];

        //var windowWidth = display.renderingWidth;
        //var winddowHeight = display.renderingHeight;

        //window.position = new Rect(Vector2.zero, new Vector2(windowWidth, winddowHeight));
        window.maximized = true;
    }

    private List<GurpsCharacterDefinition> _characterRegistry = new List<GurpsCharacterDefinition>();
    private GurpsCharacterDefinition _selectedCharacter;
    private ListView _listView;

    private void OnEnable() => RefreshRegistry();

    private void CreateGUI()
    {
        var root = new GraphicalUserInterfaceBuilder("ForgeRoot")
            .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
            .WithBackgroundColor(new Color(0.12f, 0.12f, 0.12f));

        root.AddChild(CreateHeader());

        // Step 2: Main Workspace using your SplitPanelBuilder
        root.AddChild(new SplitPanelBuilder(250)
            .WithSidebar(CreateRegistrySidebar())
            .WithMain(CreateMainCharacterSheet()));

        rootVisualElement.Add(root.Build());
    }

    private IGuiProvider CreateHeader()
    {
        return new GraphicalUserInterfaceBuilder("Header")
            .WithBackgroundColor(new Color(0.08f, 0.08f, 0.08f))
            .WithPadding(10)
            .WithBorderBottomWidth(2)
            .WithBorderBottomColor(Color.cyan)
            .AddChild(new Label("GURPS CHARACTER FORGE")
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 18, color = Color.white }
            });
    }

    private IGuiProvider CreateRegistrySidebar()
    {
        var sidebar = new GraphicalUserInterfaceBuilder("Registry")
            .WithPadding(5);

        // CRUD Toolbar using your + / - preference
        sidebar.WithPanel("Toolbar").WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Auto)
            .AddChild(new Button(CreateNewCharacter) { text = "+" })
            .AddChild(new Button(DeleteCharacter) { text = "-" })
            .ContinueWithParentPanel();

        sidebar.AddChild(ctx =>
        {
            _listView = new ListView(_characterRegistry.Select(c => c.Name).ToList(), 20,
                () => new Label(),
                (e, i) => (e as Label).text = _characterRegistry[i].Name);

            _listView.selectionChanged += items =>
            {
                int index = _listView.selectedIndex;
                if (index >= 0)
                {
                    _selectedCharacter = _characterRegistry[index];
                    Repaint(); // Refresh main sheet area
                }
            };
            return _listView;
        });

        return sidebar;
    }

    private IGuiProvider CreateMainCharacterSheet()
    {
        if (_selectedCharacter == null)
            return new GraphicalUserInterfaceBuilder("Empty").AddChild(new Button(OnClickNewCharacter) { text = "Select a character..." });

        var sheet = new GraphicalUserInterfaceBuilder("CharacterSheet")
            .WithPadding(20)
            .WithScrollable();

        // Attribute CRUD Section using our generalized theory
        sheet.WithPanel("PrimaryAttributes").WithTitle("Attributes")
            .AddIntegerData("ST", _selectedCharacter.Strength, val => { _selectedCharacter.Strength = val; SaveCharacter(_selectedCharacter); })
            .AddIntegerData("DX", _selectedCharacter.Dexterity, val => { _selectedCharacter.Dexterity = val; SaveCharacter(_selectedCharacter); })
            .AddIntegerData("IQ", _selectedCharacter.Intelligence, val => { _selectedCharacter.Intelligence = val; SaveCharacter(_selectedCharacter); })
            .AddIntegerData("HT", _selectedCharacter.Health, val => { _selectedCharacter.Health = val; SaveCharacter(_selectedCharacter); })
            .ContinueWithParentPanel();

        return sheet;
    }

    private void OnClickNewCharacter()
    {
        CreateNewCharacter();
    }

    // --- IO & Data Logic ---

    private void RefreshRegistry()
    {
        _characterRegistry.Clear();
        string path = Path.Combine(Application.streamingAssetsPath, "GURPS/Characters");
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);

        var files = Directory.GetFiles(path, "*.json");
        foreach (var file in files)
        {
            var data = File.ReadAllText(file);
            _characterRegistry.Add(JsonUtility.FromJson<GurpsCharacterDefinition>(data));
        }
    }

    private void CreateNewCharacter()
    {
        var newChar = new GurpsCharacterDefinition { Name = "New Hero " + (_characterRegistry.Count + 1) };
        SaveCharacter(newChar);
        RefreshRegistry();
    }

    private void DeleteCharacter()
    {
        if (_selectedCharacter == null) return;
        if (EditorUtility.DisplayDialog("Confirm Action", $"Delete {_selectedCharacter.Name} forever?", "Proceed", "Cancel"))
        {
            string path = Path.Combine(Application.streamingAssetsPath, "GURPS/Characters", $"{_selectedCharacter.Guid}.json");
            if (File.Exists(path)) File.Delete(path);
            _selectedCharacter = null;
            RefreshRegistry();
        }
    }

    private void SaveCharacter(GurpsCharacterDefinition character)
    {
        string path = Path.Combine(Application.streamingAssetsPath, "GURPS/Characters", $"{character.Guid}.json");
        File.WriteAllText(path, JsonUtility.ToJson(character));
    }
}
#endif