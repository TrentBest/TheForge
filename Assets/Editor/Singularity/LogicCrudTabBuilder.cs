#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.UI_And_Tools.Forge.Builders.FSM
{
    /// <summary>
    /// The Logic CRUD Hub: The "God Mode" editor for FSM behavior.
    /// Manages the Create, Read, Update, and Delete operations for state transitions and logic actions.
    /// </summary>
    public class LogicCrudTabBuilder : IGuiProvider
    {
        public string TabName => "Logic & Transitions";
        public string TabIcon => "⚡";
        public string Title => TabName;

        // --- INTERNAL DATA ---
        private List<LogicRule> _rules = new List<LogicRule>();
        private LogicRule _selectedRule;

        // UI Capture Refs
        private ScrollView _listPanel;
        private VisualElement _editorPanel;

        public LogicCrudTabBuilder()
        {
            // Seed initial data for "God Mode" exploration
            _rules.Add(new LogicRule { Name = "Low Fuel -> RTB", Condition = "Fuel < 10%", TargetState = "ReturnToBase" });
            _rules.Add(new LogicRule { Name = "Enemy Detected -> Combat", Condition = "ScanCount > 0", TargetState = "Aggressive" });
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var rootBuilder = new ForgeContainerBuilder("LogicCrud_Root")
                .WithFlexGrow(1f)
                .WithBackgroundColor(new Color(0.06f, 0.05f, 0.08f)); // Deep Logic Purple

            // --- HEADER ---
            rootBuilder.AddChild(new ForgeContainerBuilder("Header")
                .WithPadding(15f).WithBackgroundColor(new Color(0.12f, 0.1f, 0.15f))
                .WithBorderWidth(0, 0, 2f, 0).WithBorderColor(new Color(0.6f, 0.2f, 0.8f))
                .AddChild(new ForgeLabelBuilder($"{TabIcon} {Title}").WithFontSize(18).WithBold().WithColor(Color.cyan))
                .AddChild(new ForgeLabelBuilder("Modify state transitions and conditional logic manifests.").WithColor(Color.gray).WithFontSize(10)));

            // --- BODY SPLIT ---
            var body = new ForgeContainerBuilder("Body").WithDirection(FlexDirection.Row).WithFlexGrow(1f);

            // LEFT: Rules Registry
            var listPane = new ForgeContainerBuilder("RegistryPane")
                .WithWidth(new StyleLength(Length.Percent(35f)))
                .WithPadding(10f).WithBorderWidth(0, 1f, 0, 0).WithBorderColor(new Color(0.2f, 0.2f, 0.25f))
                // FIX: Lambda conversion safety
                .AddChild(new DynamicGuiProvider(c => {
                    _listPanel = new ScrollView(ScrollViewMode.Vertical) { style = { flexGrow = 1 } };
                    return _listPanel;
                }))
                .AddChild(new ForgeButtonBuilder("➕ Create New Logic Rule")
                    .WithBackgroundColor(new Color(0.1f, 0.5f, 0.2f)).WithMarginTop(10f)
                    .OnClick(() => {
                        var newRule = new LogicRule { Name = "New Transition" };
                        _rules.Add(newRule);
                        SelectRule(newRule);
                    }));

            // RIGHT: Rule Editor
            var editorPane = new ForgeContainerBuilder("EditorPane")
                .WithFlexGrow(1f).WithPadding(20f)
                .OnBuild(ve => _editorPanel = ve);

            rootBuilder.AddChild(body.AddChild(listPane).AddChild(editorPane));

            RefreshList();
            if (_rules.Count > 0) SelectRule(_rules[0]);

            return rootBuilder.Build();
        }

        private void RefreshList()
        {
            if (_listPanel == null) return;
            _listPanel.Clear();

            foreach (var rule in _rules)
            {
                var captured = rule;
                var isSelected = captured == _selectedRule;

                _listPanel.Add(new ForgeButtonBuilder($"{captured.Name}\n<size=9>IF: {captured.Condition}</size>")
                    .WithBackgroundColor(isSelected ? new Color(0.3f, 0.2f, 0.5f) : new Color(0.15f, 0.15f, 0.18f))
                    .WithMarginBottom(4f)
                    .OnClick(() => SelectRule(captured)).Build());
            }
        }

        private void SelectRule(LogicRule rule)
        {
            _selectedRule = rule;
            RefreshList();

            if (_editorPanel == null || rule == null) return;
            _editorPanel.Clear();

            var editor = new ForgeContainerBuilder("Architect").WithFlexGrow(1f);

            editor.AddChild(new ForgeLabelBuilder("LOGIC PROPERTIES").WithBold().WithMarginBottom(15f))
                  .AddChild(new ForgeTextFieldBuilder("Rule Designation", rule.Name)
                        .OnValueChanged(v => { rule.Name = v.newValue; RefreshList(); }))

                  .AddSeparator(new Color(0.3f, 0.3f, 0.3f), 1f)

                  .AddChild(new ForgeLabelBuilder("TRIGGER CONDITION").WithColor(Color.yellow).WithMarginTop(10f))
                  .AddChild(new ForgeTextFieldBuilder("Evaluation String", rule.Condition)
                        .OnValueChanged(v => rule.Condition = v.newValue))

                  .AddChild(new ForgeLabelBuilder("TRANSITION TARGET").WithColor(Color.cyan).WithMarginTop(10f))
                  .AddChild(new ForgeTextFieldBuilder("Destination State", rule.TargetState)
                        .OnValueChanged(v => rule.TargetState = v.newValue))

                  .AddChild(new ForgeButtonBuilder("🗑 Delete Logic Rule")
                        .WithBackgroundColor(new Color(0.5f, 0.1f, 0.1f)).WithMarginTop(30f)
                        .OnClick(() => {
                            _rules.Remove(rule);
                            SelectRule(_rules.Count > 0 ? _rules[0] : null);
                        }));

            _editorPanel.Add(editor.Build());
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));

        public void ToUIDocument(string path) =>
            WorkshopUxmlBaker.Bake(CreateGui(new GuiContext()), "LogicCrud_Snapshot");

        public void FromUIDocument(string path) { }

        [Serializable]
        public class LogicRule
        {
            public string Name;
            public string Condition = "Always";
            public string TargetState = "None";
        }
    }
}
#endif