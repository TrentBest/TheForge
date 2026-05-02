using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.GuiTesting
{
    public class Workshop_Gui_ImageBuilderTest : IGuiProvider
    {
        public string Title => "TEST: ImageGuiBuilder Configuration";

        public VisualElement CreateGui(GuiContext ctx)
        {
            return new GraphicalUserInterfaceBuilder("ImageTestRoot")
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceEvenly, Align.Center)
                .WithPercentSize(100, 100)
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.1f))

                // Test 1: String Constructor (Resources Load) + Fixed Size + Border
                .AddChild(new ImageGuiBuilder("Images/Warlords/SiriansFactionArt")
                    .WithFixedSize(200, 200)
                    .WithBorder(Color.yellow, 3f)
                    .WithCaption("String Ctor + Fixed Size"))

                // Test 2: Null Texture + Flex Grow (Should show empty box without crashing)
                .AddChild(new GraphicalUserInterfaceBuilder("FlexContainer")
                    .WithBackgroundColor(Color.black)
                    .OnBuild(ve => { ve.style.width = 300; ve.style.height = 300; })
                    .AddChild(new ImageGuiBuilder()
                        .WithBackgroundColor(new Color(0.2f, 0, 0))
                        .WithCaption("Null Texture + Flex Fill"))
                    .Build())
                .Build();
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) { }
        public void FromUIDocument(string path) { }
    }
}