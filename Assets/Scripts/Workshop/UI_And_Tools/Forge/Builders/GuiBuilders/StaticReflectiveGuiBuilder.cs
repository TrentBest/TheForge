using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.UIElements.Cursor;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    public class StaticReflectiveGuiBuilder : IGuiProvider
    {
        public string Title { get; private set; }

        private Type _targetType;
        private int _refreshRateMs;
        private GuiContext _lastCtx;
        private VisualElement _root;

        public StaticReflectiveGuiBuilder(Type targetType, string title = "Static Telemetry", int refreshRateMs = 100)
        {
            _targetType = targetType;
            Title = title;
            _refreshRateMs = refreshRateMs;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            var rootBuilder = new GraphicalUserInterfaceBuilder($"{_targetType.Name}_TelemetryRoot")
                .WithPadding(15)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f)) // Deepest space background
                .AddChild(new Label(Title) { style = { fontSize = 18, unityFontStyleAndWeight = FontStyle.Bold, color = Color.white, marginBottom = 20, letterSpacing = 2 } });

            _root = rootBuilder.Build();

            var primitivesContainer = new VisualElement { style = { marginBottom = 20 } };
            var dictionariesContainer = new VisualElement { style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap, justifyContent = Justify.FlexStart } };

            _root.Add(primitivesContainer);
            _root.Add(dictionariesContainer);

            var updateActions = new List<Action>();

            var fields = _targetType.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            var properties = _targetType.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var field in fields)
            {
                if (field.Name.Contains("k__BackingField")) continue;
                CategorizeAndBuildUI(field.FieldType, field.Name, () => field.GetValue(null), primitivesContainer, dictionariesContainer, updateActions);
            }

            foreach (var prop in properties)
            {
                if (!prop.CanRead) continue;
                CategorizeAndBuildUI(prop.PropertyType, prop.Name, () => prop.GetValue(null), primitivesContainer, dictionariesContainer, updateActions);
            }

            _root.schedule.Execute(() => {
                foreach (var action in updateActions) action.Invoke();
            }).Every(_refreshRateMs);

            return _root;
        }

        private void CategorizeAndBuildUI(Type type, string name, Func<object> getValue, VisualElement primitivesBlock, VisualElement dictionariesBlock, List<Action> updateActions)
        {
            if (typeof(IDictionary).IsAssignableFrom(type))
            {
                dictionariesBlock.Add(BuildDictionaryVisualizer(name, getValue, updateActions));
            }
            else if (typeof(ICollection).IsAssignableFrom(type) && type != typeof(string))
            {
                dictionariesBlock.Add(BuildCollectionVisualizer(name, getValue, updateActions));
            }
            else
            {
                primitivesBlock.Add(BuildPrimitiveRow(name, getValue, updateActions));
            }
        }

        private VisualElement BuildDictionaryVisualizer(string name, Func<object> getValue, List<Action> updateActions)
        {
            Color accentColor = GetAccentColor(name);
            var node = new VisualElement
            {
                style = {
                    backgroundColor = new Color(0.12f, 0.12f, 0.15f),
                    borderLeftWidth = 4, borderLeftColor = accentColor,
                    paddingLeft = 15, paddingRight = 15, paddingTop = 15, paddingBottom = 15,
                    marginBottom = 15, marginRight = 15, flexGrow = 1, minWidth = 280,
                    borderTopLeftRadius = 4, borderTopRightRadius = 4, borderBottomLeftRadius = 4, borderBottomRightRadius = 4
                }
            };

            node.Add(new Label(name.ToUpper().Replace("_", " ")) { style = { color = accentColor, unityFontStyleAndWeight = FontStyle.Bold, fontSize = 14, marginBottom = 15, letterSpacing = 1 } });

            var bucketsContainer = new VisualElement { style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap } };
            var summaryLabel = new Label() { style = { color = Color.gray, display = DisplayStyle.None, unityFontStyleAndWeight = FontStyle.Italic } };

            node.Add(bucketsContainer);
            node.Add(summaryLabel);

            var cardMap = new Dictionary<string, Label>();

            updateActions.Add(() => {
                IDictionary dict = null;
                try { dict = getValue() as IDictionary; } catch { return; }
                if (dict == null) return;

                if (dict.Count > 100)
                {
                    bucketsContainer.style.display = DisplayStyle.None;
                    summaryLabel.style.display = DisplayStyle.Flex;
                    summaryLabel.text = $"[ Engine Optimizing: {dict.Count:N0} Keys Active ]";
                    return;
                }

                bucketsContainer.style.display = DisplayStyle.Flex;
                summaryLabel.style.display = DisplayStyle.None;

                foreach (var key in dict.Keys)
                {
                    string kStr = key?.ToString() ?? "null";
                    object val = dict[key];

                    string vStr = (val is ICollection col) ? $"{col.Count:N0} Instances" : val?.ToString() ?? "0";

                    if (cardMap.TryGetValue(kStr, out Label valLabel))
                    {
                        valLabel.text = vStr;
                    }
                    else
                    {
                        var card = new Button(() => InspectData(val, kStr))
                        {
                            style = {
                                backgroundColor = new Color(0.18f, 0.18f, 0.22f),
                                paddingLeft = 10, paddingRight = 10, paddingTop = 8, paddingBottom = 8,
                                marginRight = 8, marginBottom = 8,
                                borderTopWidth = 2, borderTopColor = accentColor,
                                borderTopLeftRadius = 4, borderTopRightRadius = 4, borderBottomLeftRadius = 4, borderBottomRightRadius = 4,
                                minWidth = 100,
                                borderBottomWidth = 0, borderLeftWidth = 0, borderRightWidth = 0,
                                unityTextAlign = TextAnchor.MiddleLeft, cursor = new Cursor()
                            }
                        };
                        var kLabel = new Label(kStr) { style = { color = Color.white, unityFontStyleAndWeight = FontStyle.Bold, fontSize = 12 } };
                        var vLabel = new Label(vStr) { style = { color = new Color(0.7f, 0.75f, 0.8f), fontSize = 11, marginTop = 4 } };

                        card.Add(kLabel);
                        card.Add(vLabel);
                        bucketsContainer.Add(card);

                        cardMap[kStr] = vLabel;
                    }
                }
            });

            return node;
        }

        private VisualElement BuildCollectionVisualizer(string name, Func<object> getValue, List<Action> updateActions)
        {
            Color accentColor = GetAccentColor(name);
            var card = new Button(() => InspectData(getValue(), name))
            {
                style = {
                    backgroundColor = new Color(0.12f, 0.12f, 0.15f),
                    borderLeftWidth = 4, borderLeftColor = accentColor,
                    borderTopWidth = 0, borderBottomWidth = 0, borderRightWidth = 0,
                    paddingLeft = 15, paddingRight = 15, paddingTop = 15, paddingBottom = 15,
                    marginBottom = 15, marginRight = 15, flexGrow = 1, minWidth = 200,
                    borderTopLeftRadius = 4, borderTopRightRadius = 4, borderBottomLeftRadius = 4, borderBottomRightRadius = 4,
                    unityTextAlign = TextAnchor.MiddleLeft
                }
            };

            var nameLabel = new Label(name.ToUpper().Replace("_", " ")) { style = { color = Color.gray, fontSize = 11, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 8 } };
            var valueLabel = new Label("0 ITEMS") { style = { color = Color.white, fontSize = 24, unityFontStyleAndWeight = FontStyle.Bold } };

            card.Add(nameLabel);
            card.Add(valueLabel);

            updateActions.Add(() => {
                try
                {
                    if (getValue() is ICollection col) valueLabel.text = $"{col.Count:N0} ITEMS";
                    else valueLabel.text = "NULL";
                }
                catch { valueLabel.text = "ERR"; }
            });

            return card;
        }

        private VisualElement BuildPrimitiveRow(string name, Func<object> getValue, List<Action> updateActions)
        {
            var row = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween, paddingBottom = 6, paddingTop = 6, borderBottomWidth = 1, borderBottomColor = new Color(0.2f, 0.2f, 0.2f) } };
            var nameLabel = new Label(name) { style = { color = new Color(0.7f, 0.7f, 0.7f), fontSize = 13 } };
            var valueLabel = new Label("-") { style = { color = Color.white, unityFontStyleAndWeight = FontStyle.Bold, fontSize = 13 } };

            row.Add(nameLabel);
            row.Add(valueLabel);

            updateActions.Add(() => {
                try { valueLabel.text = getValue()?.ToString() ?? "null"; }
                catch { valueLabel.text = "Err"; }
            });

            return row;
        }

        // --- THE INSPECTOR OVERLAY ---
        private void InspectData(object dataToInspect, string title)
        {
            if (dataToInspect == null) return;

            Type t = dataToInspect.GetType();
            if (t.IsPrimitive || t == typeof(string)) return;

            var autoProvider = new AutoGuiProvider(dataToInspect, $"INSPECTING: {title}");
            var inspectorUi = autoProvider.CreateGui(_lastCtx ?? new GuiContext());

            var overlay = new GraphicalUserInterfaceBuilder("InspectorOverlay")
                .WithPosition(Position.Absolute)
                .WithPercentSize(100, 100)
                .WithBackgroundColor(new Color(0f, 0f, 0f, 0.8f))
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)
                .Build();

            var window = new GraphicalUserInterfaceBuilder("InspectorWindow")
                .WithWidth(600).WithHeight(500)
                .WithBackgroundColor(new Color(0.1f, 0.11f, 0.13f))
                .WithBorderTopLeftRadius(8).WithBorderTopRightRadius(8).WithBorderBottomLeftRadius(8).WithBorderBottomRightRadius(8)
                .WithBorderWidth(1).WithBorderColor(new Color(0.3f, 0.3f, 0.4f))
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                .Build();

            var header = new VisualElement
            {
                style = {
                    flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween,
                    backgroundColor = new Color(0.15f, 0.16f, 0.18f),
                    paddingLeft = 15, paddingRight = 15, paddingTop = 10, paddingBottom = 10,
                    borderTopLeftRadius = 8, borderTopRightRadius = 8
                }
            };
            header.Add(new Label(autoProvider.Title) { style = { color = Color.cyan, unityFontStyleAndWeight = FontStyle.Bold } });
            header.Add(new Button(() => _root.Remove(overlay)) { text = "✕ CLOSE", style = { backgroundColor = Color.clear, color = Color.gray, borderTopWidth = 0, borderBottomWidth = 0, borderLeftWidth = 0, borderRightWidth = 0 } });

            var content = new ScrollView(ScrollViewMode.Vertical) { style = { flexGrow = 1, paddingLeft = 15, paddingRight = 15, paddingTop = 15, paddingBottom = 15 } };
            content.Add(inspectorUi);

            window.Add(header);
            window.Add(content);
            overlay.Add(window);

            _root.Add(overlay);
        }

        private Color GetAccentColor(string name)
        {
            int hash = Mathf.Abs(name.GetHashCode());
            switch (hash % 5)
            {
                case 0: return new Color(0.1f, 0.8f, 0.9f);
                case 1: return new Color(0.38f, 0.81f, 0.38f);
                case 2: return new Color(0.9f, 0.6f, 0.2f);
                case 3: return new Color(0.8f, 0.4f, 0.8f);
                default: return new Color(0.9f, 0.4f, 0.4f);
            }
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string id) { }
        public void FromUIDocument(string doc) { }
    }
}