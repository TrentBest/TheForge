using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.UI_And_Tools.Showcase
{
    public struct RetroTributeDef
    {
        public string Title;
        public string OriginalCreator;
        public string ReleaseYear;
        public string HistoricalSignificance; // The "Claim to Fame"
        public string TechnicalOverview;      // How the Forge modernizes it
        public List<string> CoreTechnologies;
        public Color AccentColor;

        public Func<GuiContext, IGuiProvider> PlayableAlphaFactory;
    }

    public class RetroTribute_ExperiencePortal : IGuiProvider
    {
        private readonly RetroTributeDef _def;
        public string Title => _def.Title;

        public RetroTribute_ExperiencePortal(RetroTributeDef def)
        {
            _def = def;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new GraphicalUserInterfaceBuilder($"Tribute_{_def.Title}")
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.05f))
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                .Build();

            var contentContainer = new VisualElement { style = { flexGrow = 1 } };
            root.Add(contentContainer);

            ShowWelcomeScreen(contentContainer, ctx);
            return root;
        }

        private void ShowWelcomeScreen(VisualElement container, GuiContext ctx)
        {
            container.Clear();

            var welcomeBuilder = new GraphicalUserInterfaceBuilder("WelcomeView")
                .WithFlexGrow(1)
                .WithPadding(40)
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center);

            // Title & Historical Data
            welcomeBuilder.AddChild(new ForgeLabelBuilder(_def.Title.ToUpper())
                .WithFontSize(45).WithColor(_def.AccentColor).WithFontStyle(FontStyle.Bold).WithMarginBottom(10));

            welcomeBuilder.AddChild(new ForgeLabelBuilder($"ORIGINAL DESIGN: {_def.OriginalCreator} ({_def.ReleaseYear})")
                .WithFontSize(14).WithColor(Color.gray).WithFontStyle(FontStyle.Italic).WithMarginBottom(20));

            var descriptionText = $"HISTORICAL SIGNIFICANCE:\n{_def.HistoricalSignificance}\n\n" +
                                  $"FORGE RECONSTRUCTION:\n{_def.TechnicalOverview}";

            welcomeBuilder.AddChild(new ForgeLabelBuilder(descriptionText)
                .WithFontSize(16).WithColor(new Color(0.85f, 0.85f, 0.85f))
                .WithAlignment(TextAnchor.UpperCenter).WithWhiteSpace(WhiteSpace.Normal)
                .OnBuild(l => l.style.width = 700).WithMarginBottom(50));

            // Universal Routing Buttons
            var buttonRow = new GraphicalUserInterfaceBuilder("ButtonRow")
                .WithFlexLayout(FlexDirection.Row, Justify.Center, Align.Center)
                .AddChild(new ForgeButtonBuilder("LAUNCH PLAYABLE ALPHA")
                    .WithBackgroundColor(new Color(0.1f, 0.5f, 0.2f)).WithTextColor(Color.white)
                    .WithHeight(50).WithWidth(260).WithFontStyle(FontStyle.Bold).WithMarginRight(20)
                    .OnClick(() => {
                        container.Clear();
                        container.Add(_def.PlayableAlphaFactory(ctx).CreateGui(ctx));
                    }))
                .AddChild(new ForgeButtonBuilder("TECHNICAL DEEP DIVE")
                    .WithBackgroundColor(_def.AccentColor).WithTextColor(Color.white)
                    .WithHeight(50).WithWidth(260).WithFontStyle(FontStyle.Bold)
                    .OnClick(() => {
                        container.Clear();
                        // We wrap their factory output in our generic Deep Dive container
                        var liveInstance = _def.PlayableAlphaFactory(ctx);
                        container.Add(new Omniscience_DeepDive<IGuiProvider>(liveInstance, this, container).CreateGui(ctx));
                    }));

            welcomeBuilder.AddChild(buttonRow);
            container.Add(welcomeBuilder.Build());
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}