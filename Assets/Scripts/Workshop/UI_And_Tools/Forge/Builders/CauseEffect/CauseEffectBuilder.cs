using Assets.Scripts.Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.UI_And_Tools.Forge.Builders.CauseEffect
{
    /// <summary>
    /// The Logic Manifest attached to deployed GameObjects.
    /// </summary>
    public class CauseEffectManifest : MonoBehaviour
    {
        public List<CauseEffectRule> Rules = new List<CauseEffectRule>();
    }

    [Serializable]
    public class CauseEffectRule
    {
        public string RuleName = "New Rule";
        public string CauseType = "OnInteract";
        public string CauseParameter = "";
        public string EffectType = "Play VFX";
        public string EffectParameter = "";
    }

    /// <summary>
    /// The Cause & Effect Builder: An "Atom Builder" for systemic logic.
    /// Inherits from IGuiProvider to follow the Singularity Workshop Experience Model.
    /// </summary>
    public class CauseEffectBuilder : MonoBehaviour, IForgeBuilder, IGuiProvider
    {
        // --- IFORGEBUILDER IMPLEMENTATION ---
        public string ToolName => "Cause & Effect Architect";
        public Type GetProductType() => typeof(CauseEffectManifest);
        public IGuiProvider GetGuiProvider() => this;

        // --- INTERNAL STATE ---
        [SerializeField] private List<CauseEffectRule> _activeRules = new List<CauseEffectRule>();
        private CauseEffectRule _selectedRule;

        // UI State References
        private ScrollView _leftListPanel;
        private VisualElement _rightEditorPanel;

        private readonly List<string> _causeTypes = new List<string> { "OnInteract", "OnCollisionEnter", "OnHealthZero", "OnTimerElapsed", "OnZoneEnter" };
        private readonly List<string> _effectTypes = new List<string> { "Play VFX", "Play SFX", "Modify Stat", "Spawn Entity", "Destroy Self", "Transition State" };

        private void OnEnable()
        {
            if (_activeRules.Count == 0)
                _activeRules.Add(new CauseEffectRule { RuleName = "Default Reaction" });
        }

        // --- CORE FABRICATION LOGIC ---
        public object Build()
        {
            var go = new GameObject($"LogicNode_{Guid.NewGuid().ToString().Substring(0, 4)}");
            go.transform.SetParent(this.transform);

            var manifest = go.AddComponent<CauseEffectManifest>();
            foreach (var rule in _activeRules)
            {
                manifest.Rules.Add(new CauseEffectRule
                {
                    RuleName = rule.RuleName,
                    CauseType = rule.CauseType,
                    CauseParameter = rule.CauseParameter,
                    EffectType = rule.EffectType,
                    EffectParameter = rule.EffectParameter
                });
            }

            // Primitive Proxy for Editor visualization
            var proxy = GameObject.CreatePrimitive(PrimitiveType.Cube);
            proxy.transform.SetParent(go.transform, false);
            proxy.transform.localScale = Vector3.one * 0.5f;
            var renderer = proxy.GetComponent<Renderer>();
            if (renderer != null) renderer.material.color = new Color(0.64f, 0.17f, 0.77f); // Logic Purple
            Destroy(proxy.GetComponent<Collider>());

            return manifest;
        }

        // --- IGUIPROVIDER IMPLEMENTATION ---
        public string Title => ToolName;

        public VisualElement CreateGui(GuiContext ctx)
        {
            var rootBuilder = new ForgeContainerBuilder("CauseEffect_Root")
                .WithFlexGrow(1f)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.09f));

            // Header Section
            rootBuilder.AddChild(new ForgeContainerBuilder("Header")
                .WithBackgroundColor(new Color(0.15f, 0.15f, 0.18f))
                .WithPadding(15f)
                .WithBorderColor(new Color(0.64f, 0.17f, 0.77f))
                .WithBorderWidth(0, 0, 2f, 0)
                .AddChild(new ForgeLabelBuilder(Title).WithFontSize(18).WithBold().WithColor(new Color(0.8f, 0.4f, 1.0f)))
                .AddChild(new ForgeLabelBuilder("Systemic logic linking for swarm and environment.").WithColor(Color.gray).WithFontSize(11))
            );

            // Split Dashboard
            var splitBody = new ForgeContainerBuilder("SplitBody")
                .WithDirection(FlexDirection.Row)
                .WithFlexGrow(1f);

            // LEFT: Rule Registry
            var listPane = new ForgeContainerBuilder("ListPane")
                .WithWidth(new StyleLength(Length.Percent(35f)))
                .WithBorderColor(new Color(0.2f, 0.2f, 0.25f))
                .WithBorderWidth(0, 1f, 0, 0)
                .WithPadding(10f)
                // FIX: Explicitly using DynamicGuiProvider to resolve Lambda ambiguity
                .AddChild(new DynamicGuiProvider(c => {
                    var sv = new ScrollView(ScrollViewMode.Vertical);
                    sv.style.flexGrow = 1;
                    _leftListPanel = sv;
                    return sv;
                }))
                .AddChild(new ForgeButtonBuilder("➕ New Logic Rule")
                    .WithBackgroundColor(new Color(0.1f, 0.5f, 0.2f))
                    .WithMarginTop(10f)
                    .OnClick(() => {
                        var newRule = new CauseEffectRule { RuleName = $"Rule {_activeRules.Count + 1}" };
                        _activeRules.Add(newRule);
                        SelectRule(newRule);
                    }))
                .AddChild(new ForgeButtonBuilder("🔨 DEPLOY LOGIC")
                    .WithBackgroundColor(new Color(0.64f, 0.17f, 0.77f))
                    .WithBold()
                    .WithMarginTop(10f)
                    .OnClick(() => Build()));

            // RIGHT: Rule Architect
            var editorPane = new ForgeContainerBuilder("EditorPane")
                .WithFlexGrow(1f)
                .WithPadding(20f)
                .OnBuild(ve => _rightEditorPanel = ve);

            splitBody.AddChild(listPane).AddChild(editorPane);
            rootBuilder.AddChild(splitBody);

            var root = rootBuilder.Build();
            RefreshRuleList();
            if (_activeRules.Count > 0) SelectRule(_activeRules[0]);

            return root;
        }

        private void RefreshRuleList()
        {
            if (_leftListPanel == null) return;
            _leftListPanel.Clear();

            foreach (var rule in _activeRules)
            {
                var r = rule;
                var isSelected = r == _selectedRule;
                var btnColor = isSelected ? new Color(0.3f, 0.3f, 0.4f) : new Color(0.15f, 0.15f, 0.18f);

                _leftListPanel.Add(new ForgeButtonBuilder($"{r.RuleName}\n({r.CauseType} ➔ {r.EffectType})")
                    .OnClick(() => SelectRule(r))
                    .WithBackgroundColor(btnColor)
                    .WithMarginBottom(4f)
                    .WithBorderRadius(3f)
                    .Build());
            }
        }

        private void SelectRule(CauseEffectRule rule)
        {
            _selectedRule = rule;
            RefreshRuleList();

            if (_rightEditorPanel == null || rule == null) return;
            _rightEditorPanel.Clear();

            var editor = new ForgeContainerBuilder("Architect")
                .AddChild(new ForgeLabelBuilder("LOGIC DEFINITION").WithBold().WithMarginBottom(15f))

                .AddChild(new ForgeTextFieldBuilder("Designation", rule.RuleName)
                    .OnValueChanged(v => { rule.RuleName = v.newValue; RefreshRuleList(); }))

                .AddSeparator(new Color(0.2f, 0.2f, 0.2f), 1f)

                // CAUSE BLOCK
                .AddChild(new ForgeContainerBuilder("Cause")
                    .WithPadding(10f).WithMarginBottom(10f).WithBackgroundColor(new Color(0.12f, 0.1f, 0.1f))
                    .AddChild(new ForgeLabelBuilder("IF (CAUSE)").WithColor(new Color(0.8f, 0.4f, 0.4f)).WithBold())
                    .AddChild(new ForgeDropdownBuilder("Trigger", _causeTypes, rule.CauseType, v => { rule.CauseType = v; RefreshRuleList(); }))
                    .AddChild(new ForgeTextFieldBuilder("Trigger Param", rule.CauseParameter).OnValueChanged(v => rule.CauseParameter = v.newValue)))

                // EFFECT BLOCK
                .AddChild(new ForgeContainerBuilder("Effect")
                    .WithPadding(10f).WithBackgroundColor(new Color(0.1f, 0.12f, 0.14f))
                    .AddChild(new ForgeLabelBuilder("THEN (EFFECT)").WithColor(new Color(0.4f, 0.8f, 1.0f)).WithBold())
                    .AddChild(new ForgeDropdownBuilder("Action", _effectTypes, rule.EffectType, v => { rule.EffectType = v; RefreshRuleList(); }))
                    .AddChild(new ForgeTextFieldBuilder("Action Param", rule.EffectParameter).OnValueChanged(v => rule.EffectParameter = v.newValue)))

                .AddChild(new ForgeButtonBuilder("🗑 Remove Logic")
                    .WithBackgroundColor(new Color(0.5f, 0.1f, 0.1f))
                    .WithMarginTop(20f)
                    .OnClick(() => {
                        _activeRules.Remove(rule);
                        SelectRule(_activeRules.Count > 0 ? _activeRules[0] : null);
                    }));

            _rightEditorPanel.Add(editor.Build());
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) => WorkshopUxmlBaker.Bake(CreateGui(new GuiContext()), "LogicSnapshot");
        public void FromUIDocument(string path) { }
    }
}