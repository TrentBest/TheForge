#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders
{
    public class Workshop_Gui_ScrollTest : IGuiProvider
    {
        public string Title => "SCROLL STRESS TEST";

        public VisualElement CreateGui(GuiContext ctx)
        {
            // 1. THE OUTER ROOT (The Top Bread)
            // Must be bounded to the window size so it doesn't infinitely expand.
            var root = new GraphicalUserInterfaceBuilder("ScrollTestRoot")
                .WithPercentSize(100, 100)
                .WithFlexGrow(1)
                .WithFlexShrink(1) // CRITICAL: Allows root to clamp to window
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.15f))
                .WithPadding(20);

            root.AddChild(new Label("SCROLL BAR STRESS TEST")
            {
                style = { color = Color.yellow, fontSize = 24, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 }
            });

            // 2. THE SCROLL CONTAINER (The Bottom Bread)
            // Must be allowed to shrink so the flex engine knows where the bottom edge is.
            var scrollContainer = new GraphicalUserInterfaceBuilder("ScrollArea")
                .WithScrollable(true, ScrollViewMode.Vertical)
                .WithFlexGrow(1)
                .WithFlexShrink(1) // CRITICAL: Scroll container must shrink so it doesn't push past the root
                .WithBackgroundColor(new Color(0.15f, 0.15f, 0.2f))
                .WithBorderColor(Color.cyan)
                .WithBorderWidth(2)
                .WithPadding(10);

            // 3. THE CONTENT (The Meat)
            // Add enough items to force an overflow. 
            // They MUST have flexShrink(0) so they don't crush themselves to fit on screen.
            for (int i = 1; i <= 50; i++)
            {
                int index = i; // Capture for lambda

                scrollContainer.AddChild(new GraphicalUserInterfaceBuilder($"Item_{i}")
                    .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                    .WithBackgroundColor(new Color(0.2f, 0.2f, 0.25f))
                    .WithHeight(40)
                    .WithMarginBottom(5)
                    .WithPadding(10)
                    .WithBorderRadius(5)
                    .WithFlexShrink(0) // CRITICAL: Items refuse to shrink, forcing the scrollbar to appear!
                    .AddChild(new Label($"Test List Item #{index}") { style = { color = Color.white, fontSize = 14 } })
                    .AddButton("TEST CLICK", () => Debug.Log($"Clicked Item {index}"))
                );
            }

            root.AddChild(scrollContainer);

            return root.Build();
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void FromUIDocument(string path) { }
        public void ToUIDocument(string path) { }
    }
}
#endif