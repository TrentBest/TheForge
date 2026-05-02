using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Systems.MicroPackages.Literature;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.Systems.MicroPackages.Packages.Literature
{
    public class Workshop_Gui_SemanticHologram : IGuiProvider
    {
        public string Title => "SEMANTIC AURA";

        private string _characterId;
        private List<SemanticBeat> _characterBeats;

        public Workshop_Gui_SemanticHologram(string characterId, List<SemanticBeat> allStoryBeats)
        {
            _characterId = characterId;
            // Filter only the beats where this character is the Subject or Object
            _characterBeats = allStoryBeats.Where(b => b.SubjectId == _characterId || b.ObjectId == _characterId).ToList();
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new ForgeContainerBuilder("SemanticHologramRoot")
                .WithFlexGrow(1).WithPadding(15)
                .WithBackgroundColor(new Color(0.0f, 0.05f, 0.1f, 0.8f)) // Holographic Blue/Black
                .WithBorderRadius(10).WithBorderColor(Color.cyan).WithBorderWidth(2)
                .AddChild(new ForgeLabelBuilder($"SUBJECT: {_characterId.ToUpper()}")
                    .WithColor(Color.cyan).WithBold().WithFontSize(24).WithMarginBottom(15));

            // 1. The Noun-Verb-Noun Analytics
            var analyticsRow = new ForgeContainerBuilder("Analytics")
                .WithDirection(FlexDirection.Row).WithMarginBottom(20)
                .AddChild(BuildFrequencyCloud("DOMINANT VERBS", _characterBeats.Select(b => b.ActionVerb), Color.yellow))
                .AddChild(BuildFrequencyCloud("APPLIED ADJECTIVES", _characterBeats.SelectMany(b => b.Adjectives), Color.magenta));

            root.AddChild(analyticsRow);

            // 2. The Floating Pages (Usages)
            root.AddChild(new ForgeLabelBuilder("NARRATIVE MANIFESTATIONS (USAGES)")
                .WithColor(Color.green).WithBold().WithMarginBottom(10));

            var pagesScroll = new ScrollView(ScrollViewMode.Horizontal) { style = { flexGrow = 1, height = 150 } };
            var pagesContainer = new ForgeContainerBuilder("PagesContainer").WithDirection(FlexDirection.Row).WithPadding(5);

            foreach (var beat in _characterBeats)
            {
                var page = new ForgeContainerBuilder($"Page_{beat.BeatId}")
                    .WithWidth(200).WithMarginRight(10).WithPadding(10)
                    .WithBackgroundColor(new Color(0.1f, 0.1f, 0.15f))
                    .WithBorderRadius(5).WithBorderColor(Color.gray).WithBorderWidth(1)

                    // The constructed Mad Lib
                    .AddChild(new ForgeLabelBuilder($"\"{beat.GetConstructedSentence()}\"")
                        .WithColor(Color.white).WithWhiteSpace(WhiteSpace.Normal).WithMarginBottom(10))

                    // The Interactive Override
                    .OnBuild(ve => {
                        var verbOverride = new TextField("Override Verb:") { value = beat.ActionVerb };
                        verbOverride.RegisterValueChangedCallback(evt => {
                            beat.ActionVerb = evt.newValue;
                            // In a real system, this triggers an event on the SingularityDataBus to recompile the manuscript
                        });
                        ve.Add(verbOverride);
                    });

                pagesContainer.AddChild(page);
            }

            pagesScroll.Add(pagesContainer.Build());
            root.OnBuild(ve => ve.Add(pagesScroll));

            return root.Build();
        }

        private ForgeContainerBuilder BuildFrequencyCloud(string title, IEnumerable<string> words, Color color)
        {
            var container = new ForgeContainerBuilder(title)
                .WithFlexGrow(1).WithPadding(5)
                .WithBorderRightColor(Color.gray).WithBorderRightWidth(1)
                .AddChild(new ForgeLabelBuilder(title).WithColor(Color.white).WithFontSize(12).WithMarginBottom(5));

            var frequencies = words.GroupBy(w => w).ToDictionary(g => g.Key, g => g.Count());

            var cloud = new VisualElement { style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap } };
            foreach (var kvp in frequencies.OrderByDescending(k => k.Value).Take(10))
            {
                cloud.Add(new Label($"{kvp.Key} ({kvp.Value})") { style = { color = color, marginRight = 8, fontSize = 14 } });
            }

            container.OnBuild(ve => ve.Add(cloud));
            return container;
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) { }
        public void FromUIDocument(string path) { }
    }
}