#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace TheSingularityWorkshop.FSM_API.Editor
{
    public class FSMBuilderEditorWindow : EditorWindow
    {
        [MenuItem("TheSingularityWorkshop/FSMs/FSM Builder")]
        public static void ShowWindow()
        {
            var wnd = GetWindow<FSMBuilderEditorWindow>();
            wnd.titleContent = new GUIContent("FSM Builder", "Define FSM Blueprints");
            wnd.minSize = new Vector2(600, 500);
        }

        private TextField _fsmNameField;
        private IntegerField _rateField;
        private TextField _groupField;
        private TextField _initialStateField;

        private VisualElement _statesContainer;
        private VisualElement _transitionsContainer;

        private List<StateEntry> _stateEntries = new();
        private List<TransitionEntry> _transitionEntries = new();

        private void OnEnable()
        {
            rootVisualElement.Clear();

            // Build the UI using a similar structure to ExperienceBuilderEditorWindow
            var root = new GraphicalUserInterfaceBuilder("FSMBuilderRoot")
                .WithTitle("FSM Blueprint Designer")
                .WithPadding(10)
                .WithBackgroundColor(new Color(0.2f, 0.2f, 0.2f))
                .CreateGui(new GuiContext());

            // --- Header Info ---
            _fsmNameField = new TextField("FSM Name") { value = "NewStateMachine" };
            _rateField = new IntegerField("Process Rate (N)") { value = 60, tooltip = "Update every Nth tick" };
            _groupField = new TextField("Processing Group") { value = "Update" };
            _initialStateField = new TextField("Initial State");

            root.Add(_fsmNameField);
            root.Add(_rateField);
            root.Add(_groupField);
            root.Add(_initialStateField);

            // --- States Section ---
            var statesHeader = CreateSectionHeader("States", () => AddStateEntry("", "", ""));
            root.Add(statesHeader);
            _statesContainer = new VisualElement { name = "StatesContainer" };
            root.Add(_statesContainer);

            // --- Transitions Section ---
            var transHeader = CreateSectionHeader("Transitions", () => AddTransitionEntry("", "", ""));
            root.Add(transHeader);
            _transitionsContainer = new VisualElement { name = "TransitionsContainer" };
            root.Add(_transitionsContainer);

            // --- Build Button ---
            var buildBtn = new Button(ExecuteBuild)
            {
                text = "Register FSM Definition",
                style = { marginTop = 20, height = 30, backgroundColor = new Color(0.1f, 0.4f, 0.1f) }
            };
            root.Add(buildBtn);

            rootVisualElement.Add(root);

            // Add initial default state
            AddStateEntry("Idle", "// onUpdate logic", "");
        }

        private VisualElement CreateSectionHeader(string label, Action onAdd)
        {
            var row = new VisualElement { style = { flexDirection = FlexDirection.Row, marginTop = 10, marginBottom = 5 } };
            row.Add(new Label(label) { style = { unityFontStyleAndWeight = FontStyle.Bold, flexGrow = 1 } });
            row.Add(new Button(onAdd) { text = "+" });
            return row;
        }

        private void AddStateEntry(string name, string updateSnippet, string enterSnippet)
        {
            var row = new VisualElement { style = { flexDirection = FlexDirection.Row, marginBottom = 2 } };
            var nameField = new TextField { value = name, style = { flexGrow = 1 } };
            var removeBtn = new Button(() => { _statesContainer.Remove(row); _stateEntries.RemoveAll(x => x.Root == row); }) { text = "X" };

            row.Add(new Label("Name:"));
            row.Add(nameField);
            row.Add(removeBtn);
            _statesContainer.Add(row);

            _stateEntries.Add(new StateEntry { Root = row, NameField = nameField });
        }

        private void AddTransitionEntry(string from, string to, string condition)
        {
            var row = new VisualElement { style = { flexDirection = FlexDirection.Row, marginBottom = 2 } };
            var fromField = new TextField { value = from, style = { width = 100 } };
            var toField = new TextField { value = to, style = { width = 100 } };
            var condField = new TextField { value = condition, style = { flexGrow = 1 }, tooltip = "Lambda condition (context) => .." };
            var removeBtn = new Button(() => { _transitionsContainer.Remove(row); _transitionEntries.RemoveAll(x => x.Root == row); }) { text = "X" };

            row.Add(new Label("From:")); row.Add(fromField);
            row.Add(new Label(" To:")); row.Add(toField);
            row.Add(condField);
            row.Add(removeBtn);
            _transitionsContainer.Add(row);

            _transitionEntries.Add(new TransitionEntry { Root = row, FromField = fromField, ToField = toField, ConditionField = condField });
        }

        private void ExecuteBuild()
        {
            // This mirrors the internal logic of FSMBuilder.BuildDefinition
            Debug.Log($"Building FSM: {_fsmNameField.value}..");

            // In a real implementation, you would use Reflection or a Dynamic Script Engine 
            // to compile the string snippets into Action<IStateContext> and Func<IStateContext, bool>.
            // For now, we simulate the Fluent API calls.

            var builder = FSM_API.Create.CreateFiniteStateMachine(
                _fsmNameField.value,
                _rateField.value,
                _groupField.value
            );

            foreach (var s in _stateEntries)
            {
                // UI Toolkit handles state definition mapping
                builder.State(s.NameField.value, null, null, null);
            }

            if (!string.IsNullOrEmpty(_initialStateField.value))
            {
                builder.WithInitialState(_initialStateField.value); //
            }

            builder.BuildDefinition(); // Finalizes registration

            EditorUtility.DisplayDialog("FSM Builder", $"FSM '{_fsmNameField.value}' registered successfully.", "OK");
        }

        private async void RefreshGlobalRegistry()
        {
            using (UnityWebRequest www = UnityWebRequest.Get("http://localhost:5000/api/behaviors/states"))
            {
                var operation = www.SendWebRequest();
                while (!operation.isDone) await Task.Yield();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    var states = JsonUtility.FromJson<List<string>>(www.downloadHandler.text);
                    // Update your Searchable Combos here!
                }
            }
        }

        private class StateEntry { public VisualElement Root; public TextField NameField; }
        private class TransitionEntry { public VisualElement Root; public TextField FromField; public TextField ToField; public TextField ConditionField; }
    }
}
#endif