using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Systems.MicroPackages;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.BardsTale
{
    /// <summary>
    /// Functional Archive Terminal for the Bard's Tale reconstruction.
    /// Provides historical metadata and live session telemetry.
    /// </summary>
    public class BardsTale_Gui_ArchivePlaque : IGuiProvider
    {
        public string Title => "Encyclopedia Galactica: Chronological Record 1985";

        private IGuiRouter _router;
        private IArchivalMetadata _meta;
        private BardsTaleExperienceContext _gameCtx;

        // --- 🏛️ DEFAULT CONSTRUCTOR ---
        // Vital for Forge discovery, reflection, and editor-time initialization.
        public BardsTale_Gui_ArchivePlaque() { }

        public BardsTale_Gui_ArchivePlaque(IGuiRouter router, IArchivalMetadata meta)
        {
            _router = router;
            _meta = meta;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            // Locate current session context for live data binding
            _gameCtx = UnityEngine.Object.FindAnyObjectByType<BardsTaleExperienceContext>();

            // The Split Panel establishes the standard 'Archive Entry' layout
            return new ForgeSplitPanelBuilder(sidebarWidth: 380, Side.Left)
                .WithSidebar(BuildMetadataColumn(ctx))
                .WithMain(BuildReconstructionBody(ctx))
                .CreateGui(ctx);
        }

        private IGuiProvider BuildMetadataColumn(GuiContext ctx)
        {
            var sidebar = new GraphicalUserInterfaceBuilder("ArchiveMeta")
                .WithPadding(25)
                .WithBackgroundColor(new Color(0.08f, 0.05f, 0.05f))
                .AddHeader("CRITICAL CHRONICLE")
                .AddSeparator(_meta?.AccentColor ?? Color.white, 2);

            // --- metadata Injection ---
            if (_meta != null)
            {
                sidebar.AddChild(new ForgeLabelBuilder($"YEAR: {_meta.ReleaseYear}")
                    .WithColor(Color.white).WithFontSize(20).WithFontStyle(FontStyle.Bold))

                    .AddChild(new ForgeLabelBuilder(_meta.Era)
                    .WithColor(Color.gray).WithFontSize(12))

                    .AddChild(CreateDataField("ARCHITECT", _meta.OriginalAuthor))
                    .AddChild(CreateDataField("COLLECTIVE", _meta.OriginalPublisher))
                    .AddChild(CreateDataField("HARDWARE", _meta.OriginalPlatform));
            }

            // --- 🗺️ FIELD ARCHIVE LOGS (Functionality) ---
            // If a session is active, we display live mapping telemetry.
            if (_gameCtx != null)
            {
                sidebar.AddSeparator(new Color(0.3f, 0.3f, 0.3f), 1)
                    .AddChild(new ForgeLabelBuilder("SESSION TELEMETRY")
                        .WithColor(Color.gray).WithFontSize(10).WithFontStyle(FontStyle.Bold))
                    .AddChild(new ForgeLabelBuilder($"COORDS: [{_gameCtx.PlayerPosition.x}, {_gameCtx.PlayerPosition.y}]")
                        .WithColor(new Color(0.8f, 0.6f, 0.2f)).WithFontSize(12))
                    .AddChild(new ForgeLabelBuilder($"EXPLORATION: {CalculateMapPercent()}%")
                        .WithColor(new Color(0.8f, 0.6f, 0.2f)).WithFontSize(12));
            }

            // Control Group
            sidebar.AddChild(new ForgeButtonBuilder("ENTER THE GUILD", () => _router?.NavigateTo("Guild"))
                    .WithHeight(50).WithMargin(40, 0, 0, 0)
                    .WithBackgroundColor(_meta?.AccentColor ?? Color.red).WithTextColor(Color.white)
                    .WithFontStyle(FontStyle.Bold)
                    .Build())

                .AddChild(new ForgeButtonBuilder("EXIT ARCHIVE", () => _router?.NavigateTo("ExitToForge"))
                    .WithBackgroundColor(Color.clear).WithTextColor(Color.gray).WithMargin(20, 0, 0, 0)
                    .Build());

            return sidebar;
        }

        private IGuiProvider BuildReconstructionBody(GuiContext ctx)
        {
            var body = new GraphicalUserInterfaceBuilder("LoreBody")
                .WithPadding(60)
                .WithBackgroundColor(new Color(0.04f, 0.02f, 0.02f));

            if (_meta != null)
            {
                body.AddChild(new ForgeLabelBuilder("THE FOUNDATION OF SKARA BRAE")
                    .WithColor(_meta.AccentColor).WithFontSize(32).WithFontStyle(FontStyle.Bold))

                    .AddChild(new ForgeLabelBuilder(_meta.HistoricalSignificance)
                    .WithColor(Color.silver).WithFontSize(18).WithWhiteSpace(WhiteSpace.Normal));
            }

            body.AddChild(new ForgeLabelBuilder("ALGORITHMIC RECONSTRUCTION")
                    .WithColor(Color.white).WithFontStyle(FontStyle.Bold))
                .AddChild(new ForgeLabelBuilder("• 1st-Person Grid Raycasting\n• Discrete Encounter Triggers\n• Party State Concurrency")
                    .WithColor(Color.gray).WithFontSize(14));

            return body;
        }

        /// <summary>
        /// Pure Forge Data Field Helper.
        /// Replaces the old 'VisualElement' implementation with a functional composition.
        /// </summary>
        private IGuiProvider CreateDataField(string label, string val)
        {
            return new GraphicalUserInterfaceBuilder($"Field_{label}")
                .OnBuild(ve => ve.style.marginBottom = 12)
                .AddChild(new ForgeLabelBuilder(label)
                    .WithColor(Color.gray).WithFontSize(10).WithFontStyle(FontStyle.Bold))
                .AddChild(new ForgeLabelBuilder(val)
                    .WithColor(Color.white).WithFontSize(14));
        }

        private int CalculateMapPercent()
        {
            if (_gameCtx == null || _gameCtx.ExploredMap == null) return 0;
            int total = 30 * 30;
            int explored = 0;
            for (int x = 0; x < 30; x++)
                for (int y = 0; y < 30; y++)
                    if (_gameCtx.ExploredMap[x, y]) explored++;
            return (int)((float)explored / total * 100);
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}