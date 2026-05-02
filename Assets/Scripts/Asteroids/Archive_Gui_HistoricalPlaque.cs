using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Systems.MicroPackages;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.Asteroids
{
    public class Archive_Gui_HistoricalPlaque : IGuiProvider
    {
        public string Title => "Archive Entry: 1979";
        private IGuiRouter _router;
        private IArchivalMetadata _meta;

        public Archive_Gui_HistoricalPlaque(IGuiRouter router, IArchivalMetadata meta)
        {
            _router = router;
            _meta = meta;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            return new ForgeSplitPanelBuilder(sidebarWidth: 380, Side.Left)
                .WithSidebar(BuildMetadataColumn(ctx))
                .WithMain(BuildReconstructionBody(ctx))
                .CreateGui(ctx);
        }

        private IGuiProvider BuildMetadataColumn(GuiContext ctx)
        {
            return new GraphicalUserInterfaceBuilder("ArchiveMeta")
                .WithPadding(25)
                .WithBackgroundColor(new Color(0.02f, 0.02f, 0.02f))

                .AddHeader("CHRONOLOGICAL RECORD")
                .AddSeparator(Color.white, 2)

                .AddChild(new Label($"YEAR: {_meta.ReleaseYear}") { style = { color = Color.white, fontSize = 20, unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10 } })
                .AddChild(new Label(_meta.Era) { style = { color = Color.gray, fontSize = 12, letterSpacing = 2, marginBottom = 20 } })

                .AddChild(CreateDataField("AUTHOR", _meta.OriginalAuthor))
                .AddChild(CreateDataField("PUBLISHER", _meta.OriginalPublisher))
                .AddChild(CreateDataField("HARDWARE", _meta.OriginalPlatform))

                .AddChild(new ForgeButtonBuilder("INITIALIZE HANGAR", () => _router.NavigateTo("Hangar"))
                    .WithHeight(50).WithMargin(40, 0, 0, 0)
                    .WithBackgroundColor(Color.white).WithTextColor(Color.black)
                    .WithFontStyle(FontStyle.Bold)
                    .Build())

                .AddChild(new ForgeButtonBuilder("EXIT ARCHIVE", () => _router.NavigateTo("ExitToForge"))
                    .WithBackgroundColor(Color.clear).WithTextColor(Color.gray).WithMargin(20, 0, 0, 0)
                    .Build());
        }

        private IGuiProvider BuildReconstructionBody(GuiContext ctx)
        {
            return new GraphicalUserInterfaceBuilder("LoreBody")
                .WithPadding(60)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.05f))
                .AddChild(new Label("ALGORITHMIC SIGNIFICANCE") { style = { color = Color.gray, fontSize = 12, letterSpacing = 4, marginBottom = 10 } })
                .AddChild(new Label("THE VECTOR FOUNDATION") { style = { color = Color.white, fontSize = 42, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 40 } })

                .AddChild(new Label(_meta.HistoricalSignificance) { style = { color = Color.silver, fontSize = 18, whiteSpace = WhiteSpace.Normal, marginBottom = 50 } })

                .AddChild(new Label("RECONSTRUCTION PARAMETERS") { style = { color = Color.white, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 15 } })
                .AddChild(new Label("• 1:1 Vector Projection Math\n• O(1) Wrap-around Logic\n• Zero-Garbage Entity Disposal") { style = { color = Color.gray, fontSize = 14, marginBottom = 40 } });
        }

        private VisualElement CreateDataField(string label, string val)
        {
            var ve = new VisualElement { style = { marginBottom = 10 } };
            ve.Add(new Label(label) { style = { color = Color.gray, fontSize = 10, unityFontStyleAndWeight = FontStyle.Bold } });
            ve.Add(new Label(val) { style = { color = Color.white, fontSize = 14 } });
            return ve;
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}