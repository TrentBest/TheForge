using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    public static class WorkshopUxmlBaker
    {
        /// <summary>
        /// Opens a Save File dialog and bakes the target VisualElement tree into a UXML file.
        /// </summary>
        public static void Bake(VisualElement root, string defaultFileName)
        {
#if UNITY_EDITOR
            string path = EditorUtility.SaveFilePanel("Save UXML Snapshot", "Assets", defaultFileName, "uxml");
            if (string.IsNullOrEmpty(path)) return; // User canceled

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                sb.AppendLine("<ui:UXML xmlns:ui=\"UnityEngine.UIElements\" xmlns:uie=\"UnityEditor.UIElements\">");

                SerializeNode(root, sb, 1);

                sb.AppendLine("</ui:UXML>");

                File.WriteAllText(path, sb.ToString());
                AssetDatabase.Refresh();
                Debug.Log($"[UXML Baker] ✅ Successfully baked layout to: {path}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UXML Baker] ❌ Failed to bake UXML: {ex.Message}");
            }
#else
            Debug.LogWarning("UXML Baking is only available in the Unity Editor.");
#endif
        }

        private static void SerializeNode(VisualElement ve, StringBuilder sb, int indent)
        {
            string tabs = new string(' ', indent * 4);
            string typeName = ve.GetType().Name;

            // Map Unity types to UXML tags
            string tag = $"ui:{typeName}";

            sb.Append($"{tabs}<{tag}");

            if (!string.IsNullOrEmpty(ve.name))
                sb.Append($" name=\"{ve.name}\"");

            // Extract Text (For Labels, Buttons, TextFields)
            if (ve is TextElement textElement && !string.IsNullOrEmpty(textElement.text))
            {
                string safeText = textElement.text.Replace("\"", "&quot;").Replace("<", "&lt;").Replace(">", "&gt;");
                sb.Append($" text=\"{safeText}\"");
            }

            // Extract Inline CSS Styles
            List<string> styles = new List<string>();
            var s = ve.style;

            // Flex & Layout
            if (s.flexDirection.keyword != StyleKeyword.Null) styles.Add($"flex-direction: {s.flexDirection.value.ToString().ToLower()};");
            if (s.justifyContent.keyword != StyleKeyword.Null) styles.Add($"justify-content: {s.justifyContent.value.ToString().ToLower()};");
            if (s.alignItems.keyword != StyleKeyword.Null) styles.Add($"align-items: {s.alignItems.value.ToString().ToLower()};");
            if (s.flexWrap.keyword != StyleKeyword.Null) styles.Add($"flex-wrap: {s.flexWrap.value.ToString().ToLower()};");
            if (s.flexGrow.keyword != StyleKeyword.Null) styles.Add($"flex-grow: {s.flexGrow.value};");
            if (s.flexShrink.keyword != StyleKeyword.Null) styles.Add($"flex-shrink: {s.flexShrink.value};");

            // Size
            if (s.width.keyword != StyleKeyword.Null) styles.Add($"width: {FormatLength(s.width.value)};");
            if (s.height.keyword != StyleKeyword.Null) styles.Add($"height: {FormatLength(s.height.value)};");

            // Colors & Borders
            if (s.backgroundColor.keyword != StyleKeyword.Null) styles.Add($"background-color: {FormatColor(s.backgroundColor.value)};");
            if (s.color.keyword != StyleKeyword.Null) styles.Add($"color: {FormatColor(s.color.value)};");
            if (s.borderTopColor.keyword != StyleKeyword.Null) styles.Add($"border-color: {FormatColor(s.borderTopColor.value)};");
            if (s.borderTopWidth.keyword != StyleKeyword.Null) styles.Add($"border-width: {s.borderTopWidth.value}px;");
            if (s.borderTopLeftRadius.keyword != StyleKeyword.Null) styles.Add($"border-radius: {s.borderTopLeftRadius.value}px;");

            // Margins & Padding
            if (s.marginTop.keyword != StyleKeyword.Null) styles.Add($"margin-top: {s.marginTop.value.value}px;");
            if (s.marginBottom.keyword != StyleKeyword.Null) styles.Add($"margin-bottom: {s.marginBottom.value.value}px;");
            if (s.marginRight.keyword != StyleKeyword.Null) styles.Add($"margin-right: {s.marginRight.value.value}px;");
            if (s.marginLeft.keyword != StyleKeyword.Null) styles.Add($"margin-left: {s.marginLeft.value.value}px;");
            if (s.paddingTop.keyword != StyleKeyword.Null) styles.Add($"padding: {s.paddingTop.value.value}px;");

            // Compile style attribute
            if (styles.Count > 0) sb.Append($" style=\"{string.Join(" ", styles)}\"");

            // Handle Children Recursively
            var children = ve.Children().ToList();
            if (children.Count == 0)
            {
                sb.AppendLine(" />"); // Self-closing tag
            }
            else
            {
                sb.AppendLine(">");
                foreach (var child in children) SerializeNode(child, sb, indent + 1);
                sb.AppendLine($"{tabs}</{tag}>");
            }
        }

        private static string FormatLength(Length l) => l.unit == LengthUnit.Percent ? $"{l.value}%" : $"{l.value}px";
        private static string FormatColor(Color c) => $"rgba({Mathf.RoundToInt(c.r * 255)}, {Mathf.RoundToInt(c.g * 255)}, {Mathf.RoundToInt(c.b * 255)}, {c.a:F2})";
    }
}