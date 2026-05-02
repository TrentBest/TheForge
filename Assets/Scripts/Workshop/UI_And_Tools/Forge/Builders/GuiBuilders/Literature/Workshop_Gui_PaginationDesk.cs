using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Literature
{
    public class Workshop_Gui_PaginationDesk : IGuiProvider
    {
        public string Title => "THE PAGINATION DESK";

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new ForgeContainerBuilder("PaginationRoot")
                .WithFlexGrow(1).WithPadding(20)
                .WithBackgroundColor(new Color(0.1f, 0.08f, 0.05f)) // Wood desk aesthetic
                .AddChild(new ForgeLabelBuilder("BOOK CONTENT PROVIDER: COMPILER")
                    .WithColor(new Color(0.8f, 0.6f, 0.2f)).WithBold().WithFontSize(22).WithMarginBottom(20));

            // LEFT: Raw Text Ingestion
            var textIngestor = new ForgeContainerBuilder("TextIngestor")
                .WithFlexGrow(1).WithPadding(10)
                .WithBorderRightColor(new Color(0.3f, 0.2f, 0.1f)).WithBorderRightWidth(2)
                .AddChild(new ForgeLabelBuilder("RAW MANUSCRIPT STREAM").WithColor(Color.white).WithBold().WithMarginBottom(10))
                .AddChild(new Label("Select a chapter from the MIT Whiteboards to flow text into the compiler.") { style = { color = Color.gray, whiteSpace = WhiteSpace.Normal } });

            // RIGHT: Physical Page Preview
            var pagePreview = new ForgeContainerBuilder("PagePreview")
                .WithFlexGrow(1.5f).WithPadding(20)
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)
                .AddChild(new ForgeLabelBuilder("PHYSICAL MANIFESTATION PREVIEW").WithColor(Color.white).WithBold().WithMarginBottom(20));

            // The "Physical" Book Mockup
            var bookSpread = new ForgeContainerBuilder("BookSpread")
                .WithDirection(FlexDirection.Row)
                .WithWidth(600).WithHeight(400)
                .WithBackgroundColor(Color.black)
                .WithPadding(5)
                .AddChild(CreatePageMockup("Page 1"))
                .AddChild(CreatePageMockup("Page 2"));

            pagePreview.AddChild(bookSpread);

            // Output Compiler Button
            pagePreview.AddChild(new ForgeButtonBuilder("COMPILE TO BOOK_CONTENT_PROVIDER")
                .WithBackgroundColor(new Color(0.1f, 0.4f, 0.1f)).WithTextColor(Color.white)
                .WithMarginTop(30).WithPadding(15).WithBold()
                .WithOnClick(() => Debug.Log("Compiling manuscript into a deployable MicroPackage...")));

            var splitPanel = new ForgeSplitPanelBuilder(400, Side.Left)
                .WithSidebar(textIngestor)
                .WithMain(pagePreview);

            return root.AddChild(splitPanel).Build();
        }

        private ForgeContainerBuilder CreatePageMockup(string pageLabel)
        {
            return new ForgeContainerBuilder(pageLabel)
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.95f, 0.95f, 0.9f)) // Paper color
                .WithMargin(2)
                .WithPadding(20)
                .AddChild(new ForgeLabelBuilder(pageLabel).WithColor(Color.gray).WithFontSize(10))
                .AddChild(new ForgeLabelBuilder("[ Formatted Text Rendered Here ]")
                    .WithColor(Color.black).WithMarginTop(20).WithWhiteSpace(WhiteSpace.Normal));
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) { }
        public void FromUIDocument(string path) { }
    }
}