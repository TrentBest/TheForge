#if UNITY_EDITOR
using System;
using System.Reflection; // Added for Reflection
using UnityEditor;
using UnityEditor.UIElements; // Added for specific UI fields
using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.Ontology;

public class ExperienceBuilderEditorWindow : EditorWindow
{
    private ExperienceReflection.OntologyNode _ontologyRoot;
    private ExperienceReflection.OntologyNode _selectedNode;

    private VisualElement _treeContainer;
    private VisualElement _detailsRoot;
    private string manifest = "";

    // We store the configuration object here so we don't lose data while switching UI focus
    private object _pendingConfiguration;

    [MenuItem("TheSingularityWorkshop/NonSupported/Genesis Hub")]
    public static void ShowWindow() => GetWindow<ExperienceBuilderEditorWindow>("Genesis Hub").Show();

    public static void ShowWindow(string manifest)
    {
        var window = GetWindow<ExperienceBuilderEditorWindow>("Genesis Hub");
        window.manifest = manifest;
        window.Show();
    }

    private void OnEnable()
    {
        // Deterministic Logic Init
        _ontologyRoot = ExperienceReflection.BuildOntologyTree();
    }

    private void CreateGUI()
    {
        var root = rootVisualElement;
        root.style.flexDirection = FlexDirection.Row;

        // LEFT PANE: Ontology Browser
        var leftPane = new VisualElement { style = { width = 250, borderRightWidth = 1, borderRightColor = Color.black, backgroundColor = new Color(0.18f, 0.18f, 0.18f) } };
        leftPane.Add(new Label("ONTOLOGY REGISTRY") { style = { unityFontStyleAndWeight = FontStyle.Bold, color = new Color(0.7f, 0.7f, 0.7f), paddingLeft = 10, paddingTop = 10 } });

        var treeScroll = new ScrollView();
        if (_ontologyRoot != null) RenderTreeNode(_ontologyRoot, treeScroll, 0);
        leftPane.Add(treeScroll);
        root.Add(leftPane);

        // RIGHT PANE: The "Easy Button" Context
        _detailsRoot = new VisualElement { style = { flexGrow = 1,  } };
        RenderEmptyState();
        root.Add(_detailsRoot);
    }

    // Recursive UI Builder
    private void RenderTreeNode(ExperienceReflection.OntologyNode node, VisualElement parent, int depth)
    {
        var row = new Button(() => SelectNode(node))
        {
            style = {
                backgroundColor = Color.clear,
                paddingLeft = 10 + (depth * 15), paddingTop = 4, paddingBottom = 4,
                unityTextAlign = TextAnchor.MiddleLeft
            }
        };

        // Visual distinction: Interfaces vs Concrete Classes
        bool isConcrete = !node.Type.IsInterface && !node.Type.IsAbstract;
        string prefix = isConcrete ? "📦 " : (depth == 0 ? "⦿ " : "↳ ");
        var label = new Label(prefix + node.Meta.DisplayName)
        {
            style = { color = isConcrete ? new Color(0.6f, 1f, 0.6f) : Color.white }
        };

        row.Add(label);
        parent.Add(row);

        foreach (var child in node.Children)
        {
            RenderTreeNode(child, parent, depth + 1);
        }
    }

    private void SelectNode(ExperienceReflection.OntologyNode node)
    {
        _selectedNode = node;
        _detailsRoot.Clear();

        // 1. Title & Header
        _detailsRoot.Add(new Label(node.Meta.DisplayName.ToUpper()) { style = { fontSize = 24, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 5 } });
        _detailsRoot.Add(new Label($"Type Identity: {node.Type.FullName}") { style = { fontSize = 10, opacity = 0.5f, marginBottom = 15 } });

        // 2. Context Description (From Attribute)
        var descBox = new Box { style = { backgroundColor = new Color(0.25f, 0.25f, 0.25f), marginBottom = 20,  } };
        descBox.Add(new Label(node.Meta.Description) { style = { fontSize = 12, whiteSpace = WhiteSpace.Normal, color = new Color(0.8f, 0.8f, 0.8f) } });
        _detailsRoot.Add(descBox);

        // 3. User Input & Configuration (Reflection Based)

        if (!node.Type.IsInterface && !node.Type.IsAbstract)
        {
            _detailsRoot.Add(new Label("Experience Configuration") { style = { unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10, color = new Color(0.6f, 1f, 0.6f) } });

            // Create a temporary instance to hold values if one doesn't exist for this type
            if (_pendingConfiguration == null || _pendingConfiguration.GetType() != node.Type)
            {
                try
                {
                    _pendingConfiguration = Activator.CreateInstance(node.Type);
                }
                catch (Exception)
                {
                    // Fallback for types without parameterless constructors
                    _detailsRoot.Add(new Label("⚠ Type requires constructor parameters."));
                }
            }

            // --- AUTO-GENERATE UI FROM FIELDS ---
            var fields = node.Type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            var scrollContainer = new ScrollView() { style = { maxHeight = 300, marginBottom = 20 } };

            foreach (var field in fields)
            {
                if (field.FieldType == typeof(int))
                {
                    var input = new IntegerField(field.Name) { value = (int)field.GetValue(_pendingConfiguration) };
                    input.RegisterValueChangedCallback(e => field.SetValue(_pendingConfiguration, e.newValue));
                    scrollContainer.Add(input);
                }
                else if (field.FieldType == typeof(float))
                {
                    var input = new FloatField(field.Name) { value = (float)field.GetValue(_pendingConfiguration) };
                    input.RegisterValueChangedCallback(e => field.SetValue(_pendingConfiguration, e.newValue));
                    scrollContainer.Add(input);
                }
                else if (field.FieldType == typeof(bool))
                {
                    var input = new Toggle(field.Name) { value = (bool)field.GetValue(_pendingConfiguration) };
                    input.RegisterValueChangedCallback(e => field.SetValue(_pendingConfiguration, e.newValue));
                    scrollContainer.Add(input);
                }
                else if (field.FieldType == typeof(string))
                {
                    var input = new TextField(field.Name) { value = (string)field.GetValue(_pendingConfiguration) };
                    input.RegisterValueChangedCallback(e => field.SetValue(_pendingConfiguration, e.newValue));
                    scrollContainer.Add(input);
                }
            }
            _detailsRoot.Add(scrollContainer);

            // 4. Boot Button
            var bootBtn = new Button(() => BootExperience(node))
            {
                text = "INITIALIZE REALITY",
                style = { height = 40, fontSize = 14, backgroundColor = new Color(0.2f, 0.5f, 0.2f), color = Color.white }
            };
            _detailsRoot.Add(bootBtn);
        }
        else
        {
            var warning = new Label("⚠ Abstract Archetype Selected") { style = { color = Color.yellow, unityFontStyleAndWeight = FontStyle.Bold } };
            var subWarn = new Label("You cannot instantiate an idea. Please select a concrete implementation (📦) below this node, or create a new C# class that inherits from this interface.");
            subWarn.style.whiteSpace = WhiteSpace.Normal;
            _detailsRoot.Add(warning);
            _detailsRoot.Add(subWarn);
        }
    }

    private void RenderEmptyState()
    {
        _detailsRoot.Clear();
        _detailsRoot.Add(new Label("Select an Origin Point from the Ontology.") { style = { alignSelf = Align.Center, opacity = 0.5f, marginTop = 100 } });
    }

    private void BootExperience(ExperienceReflection.OntologyNode node)
    {
        // Here we serialize the configured object into JSON or pass it to a Manager
        string configJson = JsonUtility.ToJson(_pendingConfiguration, true);

        Debug.Log($"<color=cyan>BOOTING {node.Meta.DisplayName.ToUpper()}..</color>");
        Debug.Log($"Manifest DNA: {node.Type.FullName}");
        Debug.Log($"Configuration: \n{configJson}");

        // TODO: Call your scene loader or ExperienceManager here
        // ExperienceManager.Initialize(node.Type, _pendingConfiguration);
    }
}
#endif