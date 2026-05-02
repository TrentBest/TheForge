using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Systems.MicroPackages;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.Armada2525
{
    // ====================================================================
    // 🏛️ THE DIGITAL ARCHIVE (Dynamic Encyclopedia Entry)
    // ====================================================================
    public class Armada_Gui_ArchiveIntro : IGuiProvider
    {
        public string Title => "Encyclopedia Galactica";
        private IGuiRouter _router;
        private IArchivalMetadata _metaData;

        // The UI now dynamically reflects the injected ontology
        public Armada_Gui_ArchiveIntro(IGuiRouter router, IArchivalMetadata metaData)
        {
            _router = router;
            _metaData = metaData;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            return new ForgeSplitPanelBuilder(sidebarWidth: 350, Side.Left)
                .WithSidebar(BuildMetaDataPanel(ctx))
                .WithMain(BuildHistoricalLorePanel(ctx))
                .CreateGui(ctx);
        }

        private IGuiProvider BuildMetaDataPanel(GuiContext ctx)
        {
            return new GraphicalUserInterfaceBuilder("ArchiveMetaData")
                .WithPadding(30)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.08f))

                .AddChild(new GraphicalUserInterfaceBuilder("LogoMount")
                    .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)
                    .WithBackgroundColor(new Color(0.1f, 0.1f, 0.15f))
                    .OnBuild(ve => { ve.style.height = 200; ve.style.marginBottom = 30; ve.style.borderTopWidth = 2; ve.style.borderTopColor = new Color(0.2f, 0.6f, 0.9f); })
                    .AddChild(new Label("A R M A D A") { style = { color = Color.white, fontSize = 36, letterSpacing = 5, unityFontStyleAndWeight = FontStyle.Bold } })
                    .AddChild(new Label("2 5 2 5") { style = { color = new Color(0.2f, 0.6f, 0.9f), fontSize = 24, letterSpacing = 10, unityFontStyleAndWeight = FontStyle.Bold } })
                    .Build())

                .AddHeader("ARCHIVE METADATA")
                .AddSeparator(Color.gray, 1)

                // Read directly from the Interface!
                .AddChild(CreateMetaRow("RELEASE YEAR:", _metaData.ReleaseYear.ToString()))
                .AddChild(CreateMetaRow("DEVELOPER:", _metaData.OriginalAuthor))
                .AddChild(CreateMetaRow("PUBLISHER:", _metaData.OriginalPublisher))
                .AddChild(CreateMetaRow("PLATFORM:", _metaData.OriginalPlatform))
                .AddChild(CreateMetaRow("ERA:", _metaData.Era))

                .AddChild(new ForgeButtonBuilder("<- RETURN TO FORGE", () => _router.NavigateTo("ExitToForge"))
                    .WithBackgroundColor(Color.clear)
                    .WithTextColor(Color.gray)
                    .WithMargin(40, 0, 0, 0)
                    .Build());
        }

        private IGuiProvider BuildHistoricalLorePanel(GuiContext ctx)
        {
            return new GraphicalUserInterfaceBuilder("HistoricalLore")
                .WithPadding(50)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f))

                .AddChild(new Label("ALGORITHMIC HERITAGE") { style = { color = new Color(0.8f, 0.6f, 0.2f), fontSize = 14, letterSpacing = 2, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } })
                .AddChild(new Label("THE PIONEER OF THE STARS") { style = { color = Color.white, fontSize = 32, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 30 } })

                // Read the Significance directly from the Interface!
                .AddChild(new Label(_metaData.HistoricalSignificance) { style = { color = new Color(0.8f, 0.8f, 0.85f), fontSize = 16, whiteSpace = WhiteSpace.Normal, marginBottom = 50,  } })

                .AddChild(new Label("TECHNICAL RECONSTRUCTION") { style = { color = Color.gray, fontSize = 12, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } })
                .AddChild(new Label("• Seed-based Deterministic Galaxy Generation\n• Concurrent Entity FSM Ticking\n• Sovereign Data Encapsulation") { style = { color = new Color(0.2f, 0.6f, 0.9f), fontSize = 14, marginBottom = 50 } })

                .AddChild(new ForgeButtonBuilder("INITIALIZE RECONSTRUCTION", () => _router.NavigateTo("Bootloader"))
                    .WithBackgroundColor(new Color(0.2f, 0.6f, 0.9f))
                    .WithTextColor(Color.black)
                    .WithHeight(50)
                    .WithWidth(300)
                    .WithFontStyle(FontStyle.Bold)
                    .Build());
        }

        private VisualElement CreateMetaRow(string label, string value)
        {
            var row = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween, marginTop = 10 } };
            row.Add(new Label(label) { style = { color = Color.gray, fontSize = 12, unityFontStyleAndWeight = FontStyle.Bold } });
            row.Add(new Label(value) { style = { color = Color.white, fontSize = 12 } });
            return row;
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}