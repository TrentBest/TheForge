using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.UI_And_Tools.Forge
{
    public partial class ForgeDisplay
    {
        /// <summary>
        /// A lightweight provider used to display a fallback title when the main display is idle.
        /// Reforged to eliminate stubs and utilize the Forge Component pattern.
        /// </summary>
        private class StandbyProvider : IGuiProvider
        {
            private string _title;
            public string Title => _title;

            public StandbyProvider(string title)
            {
                _title = string.IsNullOrEmpty(title) ? "SYSTEM STANDBY" : title;
            }

            /// <summary>
            /// Implements the standardized Forge UI builder pattern.
            /// </summary>
            public Action<VisualElement> GetGuiBuilder()
            {
                return root => root.Add(CreateGui(new GuiContext()));
            }

            /// <summary>
            /// Creates the visual representation using Forge Component Wrappers.
            /// </summary>
            public VisualElement CreateGui(GuiContext ctx)
            {
                // We use the ForgeLabelBuilder to ensure consistent styling and skinning
                return new ForgeLabelBuilder(_title)
                    .WithFontSize(24)
                    .WithFontStyle(FontStyle.Bold)
                    .WithColor(new Color(0.3f, 0.3f, 0.35f)) // Dimmed Standby Grey
                    .WithFlexGrow(1)
                    //.WithAlignItems(Align.Center)
                   // .WithJustifyContent(Justify.Center)
                    .Build();
            }

            /// <summary>
            /// Serializes the standby state to a static UXML document.
            /// </summary>
            public void ToUIDocument(string assetPath)
            {
                var snapshotRoot = CreateGui(new GuiContext());
                string fileName = string.IsNullOrEmpty(assetPath) ? "ForgeDisplay_Standby" : System.IO.Path.GetFileNameWithoutExtension(assetPath);

                // Leverage the Baker for editor-side archival
                WorkshopUxmlBaker.Bake(snapshotRoot, fileName);
            }

            /// <summary>
            /// Fulfills the IGuiProvider contract without introducing unhandled exceptions.
            /// </summary>
            public void FromUIDocument(string assetPath)
            {
                Debug.LogWarning("[StandbyProvider] FromUIDocument is bypassed. Content is dynamically defined by the Display Orchestrator.");
            }
        }
    }
}