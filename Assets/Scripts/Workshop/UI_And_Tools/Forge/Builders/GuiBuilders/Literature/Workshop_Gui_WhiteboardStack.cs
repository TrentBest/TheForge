using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Literature
{
    public class Workshop_Gui_WhiteboardStack : IGuiProvider
    {
        public string Title => "THE MIT WHITEBOARDS";

        private VisualElement _boardContainer;

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new ForgeContainerBuilder("WhiteboardRoot")
                .WithFlexGrow(1).WithPadding(20)
                .WithBackgroundColor(new Color(0.9f, 0.9f, 0.9f)) // Whiteboard aesthetic
                .AddChild(new ForgeLabelBuilder("NARRATIVE STRUCTURE ARCHITECTURE")
                    .WithColor(Color.black).WithBold().WithFontSize(24).WithMarginBottom(20));

            // The Sliding Board Navigation
            var navContainer = new ForgeContainerBuilder("BoardNav")
                .WithDirection(FlexDirection.Row).WithMarginBottom(20)
                .AddChild(new ForgeButtonBuilder("BOARD 1: THE SERIES")
                    .WithBackgroundColor(new Color(0.2f, 0.2f, 0.2f)).WithTextColor(Color.white).WithMarginRight(5)
                    .WithOnClick(() => LoadBoard("Series Overview", "Define the overarching macro-narrative, themes, and global rules.")))
                .AddChild(new ForgeButtonBuilder("BOARD 2: THE BOOK")
                    .WithBackgroundColor(new Color(0.2f, 0.2f, 0.2f)).WithTextColor(Color.white).WithMarginRight(5)
                    .WithOnClick(() => LoadBoard("Book Architecture", "Define the specific narrative arc, acts, and pacing for this installment.")))
                .AddChild(new ForgeButtonBuilder("BOARD 3: TABLE OF CONTENTS")
                    .WithBackgroundColor(new Color(0.2f, 0.2f, 0.2f)).WithTextColor(Color.white)
                    .WithOnClick(() => LoadBoard("Chapter Outline", "Granular scene-by-scene beats and chapter flow.")));

            _boardContainer = new ForgeContainerBuilder("ActiveBoard")
                .WithFlexGrow(1)
                .WithBorderColor(Color.black).WithBorderWidth(2)
                .WithPadding(20)
                .Build();

            root.AddChild(navContainer);
            root.OnBuild(ve => ve.Add(_boardContainer));

            // Load default board
            LoadBoard("Series Overview", "Define the overarching macro-narrative, themes, and global rules.");

            return root.Build();
        }

        private void LoadBoard(string title, string description)
        {
            _boardContainer.Clear();
            _boardContainer.Add(new Label(title) { style = { color = Color.black, fontSize = 20, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });
            _boardContainer.Add(new Label(description) { style = { color = new Color(0.3f, 0.3f, 0.3f), marginBottom = 20 } });

            // In production, this loads specific CRUD builders for Series, Books, or Chapters
            _boardContainer.Add(new Label("[ Interactive Outline CRUD Interface Goes Here ]") { style = { color = Color.red, unityFontStyleAndWeight = FontStyle.Italic } });
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) { }
        public void FromUIDocument(string path) { }
    }
}