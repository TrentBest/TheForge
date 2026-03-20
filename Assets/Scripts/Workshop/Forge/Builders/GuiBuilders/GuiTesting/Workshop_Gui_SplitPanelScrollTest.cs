#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders
{
    public class Workshop_Gui_SplitPanelScrollTest : IGuiProvider
    {
        public string Title => "SPLIT PANEL SCROLL TEST";

        public VisualElement CreateGui(GuiContext ctx)
        {
            // Create a split panel with a 350px sidebar, just like the Ingestor
            var splitPanel = new SplitPanelBuilder(350)
                .WithSidebar(CreateSidebar())
                .WithMain(CreateMain());

            return splitPanel.CreateGui(ctx);
        }

        private IGuiProvider CreateSidebar()
        {
            // 1. THE SCROLL CONTAINER (Bottom Bread)
            // It MUST be allowed to shrink.
            var sidebar = new GraphicalUserInterfaceBuilder("TestSidebar")
                .WithBackgroundColor(new Color(0.15f, 0.15f, 0.2f))
                .WithPadding(10)
                .WithFlexGrow(1)
                .WithFlexShrink(1) // CRITICAL
                .WithScrollable(true, ScrollViewMode.Vertical);

            sidebar.AddChild(new Label("SIDEBAR SCROLL")
            {
                style = { color = Color.yellow, fontSize = 18, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 }
            });

            // 2. THE CONTENT (The Meat)
            // 50 items that refuse to shrink.
            for (int i = 1; i <= 50; i++)
            {
                int index = i;
                sidebar.AddChild(new GraphicalUserInterfaceBuilder($"SideItem_{i}")
                    .WithBackgroundColor(new Color(0.2f, 0.2f, 0.25f))
                    .WithHeight(40)
                    .WithMarginBottom(5)
                    .WithPadding(10)
                    .WithFlexShrink(0) // CRITICAL: Force overflow
                    .AddChild(new Label($"Sidebar Item {index}") { style = { color = Color.white } })
                );
            }

            return sidebar;
        }

        private IGuiProvider CreateMain()
        {
            // A static main area to prove the split panel is dividing space correctly
            var mainArea = new GraphicalUserInterfaceBuilder("TestMain")
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.15f))
                .WithPadding(20)
                .WithFlexGrow(1)
                .WithFlexShrink(1);

            mainArea.AddChild(new Label("MAIN CANVAS AREA")
            {
                style = { color = Color.cyan, fontSize = 24, unityFontStyleAndWeight = FontStyle.Bold }
            });

            mainArea.AddChild(new Label("If the sidebar scrolls properly, the SplitPanelBuilder is structurally sound. " +
                                        "That means the bug in AssetIngestor is related to how the Foldouts/Cards are nested.")
            {
                style = { color = Color.white, marginTop = 20, whiteSpace = WhiteSpace.Normal }
            });

            return mainArea;
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void FromUIDocument(string path) { }
        public void ToUIDocument(string path) { }
    }
}
#endif