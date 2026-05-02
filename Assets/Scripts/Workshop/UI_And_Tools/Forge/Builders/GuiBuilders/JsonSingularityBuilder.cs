using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Newtonsoft.Json.Linq;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.Core.Diagnostics;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    public class JsonSingularityBuilder : IGuiProvider
    {
        public string Title { get; set; } = "FORGE: DATA INTERPRETER";

        private JToken _rootToken;
        private JObject _interceptorRules;
        private bool _showTruthOverlay = false;

        // FIX: Default constructor for the Forge Editor reflection
        public JsonSingularityBuilder()
        {
            // Initialize with a simple sample so the editor doesn't crash on load
            _rootToken = JToken.Parse("{ \"Status\": \"Awaiting Data...\", \"Singularity\": \"Active\" }");
        }

        public JsonSingularityBuilder(string jsonPayload, string interceptorJson = null)
        {
            _rootToken = JToken.Parse(jsonPayload);
            if (!string.IsNullOrEmpty(interceptorJson))
                _interceptorRules = JObject.Parse(interceptorJson);
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new GraphicalUserInterfaceBuilder("JsonSingularity_Root")
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                .WithPadding(10)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.08f));

            // Header with "Truth Toggle"
            var header = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween, marginBottom = 10 } };
            header.Add(new Label(Title) { style = { color = Color.cyan, unityFontStyleAndWeight = FontStyle.Bold } });

            var truthBtn = new Button(() => {
                _showTruthOverlay = !_showTruthOverlay;
                // Use the Extension method to refresh the UI in the Forge
                var newContent = CreateGui(ctx);
                root.Build().parent.Add(newContent);
                root.Build().RemoveFromHierarchy();
            })
            { text = _showTruthOverlay ? "MODE: SOURCE TRUTH" : "MODE: EXPERIENCE" };

            truthBtn.style.backgroundColor = _showTruthOverlay ? Color.red : new Color(0.2f, 0.2f, 0.25f);
            header.Add(truthBtn);
            root.AddChild(header);

            if (_rootToken != null)
                root.AddChild(BuildToken(_rootToken, "Root", 0));

            return root.Build();
        }

        private VisualElement BuildToken(JToken token, string key, int depth)
        {
            var container = new VisualElement { style = { marginLeft = depth * 15, marginBottom = 5 } };

            string displayValue = null;
            bool isForged = false;
            if (_interceptorRules != null && _interceptorRules[key] != null && token.Type != JTokenType.Object)
            {
                displayValue = _interceptorRules[key].ToString();
                isForged = true;
            }

            switch (token.Type)
            {
                case JTokenType.Object:
                    var foldout = new Foldout { text = key.ToUpper(), value = true };
                    foreach (var prop in ((JObject)token).Properties())
                        foldout.Add(BuildToken(prop.Value, prop.Name, depth + 1));
                    container.Add(foldout);
                    break;

                case JTokenType.Array:
                    var arrayLabel = new Label($"{key} [{((JArray)token).Count}]") { style = { color = Color.gray, fontSize = 10 } };
                    container.Add(arrayLabel);
                    int i = 0;
                    foreach (var item in (JArray)token)
                        container.Add(BuildToken(item, $"{key}[{i++}]", depth + 1));
                    break;

                default:
                    var row = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center } };
                    row.Add(new Label($"{key}: ") { style = { color = new Color(0.7f, 0.7f, 0.7f), width = 120 } });

                    string finalValue = isForged && !_showTruthOverlay ? displayValue : token.ToString();
                    var valLabel = new Label(finalValue);

                    if (isForged && !_showTruthOverlay)
                        valLabel.style.color = new Color(0.6f, 0.2f, 1f);
                    else if (isForged && _showTruthOverlay)
                        valLabel.style.backgroundColor = new Color(0.4f, 0.1f, 0.1f);

                    row.Add(valLabel);
                    row.tooltip = isForged ? $"[FORGERY DETECTED] Original: {token}" : "Verified Source Data";
                    container.Add(row);
                    break;
            }
            return container;
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));

        // --- UIDOCUMENT IMPLEMENTATION ---

        public void ToUIDocument(string assetPath)
        {
#if UNITY_EDITOR
            // Serialize the current state (the build) to a UXML file
            var root = CreateGui(new GuiContext());
            GraphicalUserInterfaceBuilder.ConvertToUIDocument(root, assetPath);
            ForgeLogger.Log($"[The Forge] Exported JSON Snapshot to {assetPath}");
#endif
        }

        public void FromUIDocument(string assetPath)
        {
#if UNITY_EDITOR
            // Load a UXML snapshot
            var ve = GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath);
            // In a browser context, we usually don't "load" a JSON builder from UXML,
            // but this satisfies the interface for the Forge Editor.
            ForgeLogger.Log($"[The Forge] Loaded Static Snapshot from {assetPath}");
#endif
        }
    }
}