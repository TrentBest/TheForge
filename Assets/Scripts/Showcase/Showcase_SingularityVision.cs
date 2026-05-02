using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.UI_And_Tools.Showcase
{
    public class Showcase_SingularityVision : IGuiProvider
    {
        public string Title => "THE SINGULARITY VISION";

        public VisualElement CreateGui(GuiContext ctx)
        {
            var rootBuilder = new GraphicalUserInterfaceBuilder("VisionRoot")
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                .WithPercentSize(100, 100)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f))
                // Wrapping everything in a ScrollView ensures no UI overlaps
                .WithScrollable(true, ScrollViewMode.Vertical)
                .WithPadding(40);

            // ==========================================
            // HEADER SECTION
            // ==========================================
            rootBuilder.AddChild(new GraphicalUserInterfaceBuilder("HeaderContainer")
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)
                .WithMarginBottom(40)
                .AddChild(new ForgeLabelBuilder("THE SINGULARITY").WithColor(Color.white).WithFontSize(36).WithFontStyle(FontStyle.Bold))
                .AddChild(new ForgeLabelBuilder("A Creative Renaissance, Not a Technological Crisis.").WithColor(new Color(0.8f, 0.1f, 0.8f)).WithFontSize(18).WithFontStyle(FontStyle.Italic))
            );

            // ==========================================
            // NARRATIVE TEXT: THE PHILOSOPHY
            // ==========================================
            string p1 = "When we hear 'The Singularity,' we are conditioned to imagine a struggle: Man vs. Machine. We picture a world where we are made obsolete by cold, calculating intellects. But that is a profound misunderstanding of human nature.";
            string p2 = "When machines can do everything we physically can, but faster and better... When they possess the sum of all recorded data because they are instantly connected to vast databases... What is left for us? The answer is the one thing they cannot compute: True Creativity.";
            string p3 = "Generative AI is powerful, but it is derivative. It synthesizes stock images and rephrases existing knowledge. It lacks the 'spark in the dark'—the spontaneous, illogical, beautiful leap of human imagination. The true Singularity is the moment humanity is released from rote technical labor, unlocking our ultimate potential as creators.";

            rootBuilder.AddChild(new GraphicalUserInterfaceBuilder("NarrativeContainer")
                .WithMarginBottom(40)
                .AddChild(new ForgeLabelBuilder(p1).WithColor(new Color(0.8f, 0.8f, 0.8f)).WithFontSize(16).WithWhiteSpace(WhiteSpace.Normal).WithMargin(0, 15, 0, 0))
                .AddChild(new ForgeLabelBuilder(p2).WithColor(new Color(0.8f, 0.8f, 0.8f)).WithFontSize(16).WithWhiteSpace(WhiteSpace.Normal).WithMargin(0, 15, 0, 0))
                .AddChild(new ForgeLabelBuilder(p3).WithColor(new Color(0.8f, 0.8f, 0.8f)).WithFontSize(16).WithWhiteSpace(WhiteSpace.Normal))
            );

            // ==========================================
            // THE MISSION (CARDS)
            // ==========================================
            rootBuilder.AddChild(new GraphicalUserInterfaceBuilder("MissionHeader")
                .WithMarginBottom(20)
                .AddChild(new ForgeLabelBuilder("THE MISSION: EMPOWERING THE INDIVIDUAL").WithColor(Color.white).WithFontSize(20).WithFontStyle(FontStyle.Bold))
                .AddChild(new GraphicalUserInterfaceBuilder("Divider").WithBackgroundColor(new Color(0.8f, 0.1f, 0.8f)).WithHeight(2).WithWidth(100).WithMarginTop(5))
            );

            string missionText = "Why build The Forge? Why build the FSM_API? Because we are building the foundational brick and mortar of this new era. These tools are designed to strip away the friction of software development, allowing the individual to seamlessly manifest and monetize their imagination.";
            rootBuilder.AddChild(new ForgeLabelBuilder(missionText).WithColor(Color.silver).WithFontSize(14).WithWhiteSpace(WhiteSpace.Normal).WithMargin(0, 30, 0, 0));

            // CARDS CONTAINER (Using FlexWrap so they stack cleanly on small screens, preventing overlap)
            var cardsContainer = new GraphicalUserInterfaceBuilder("CardsContainer")
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.FlexStart)
                .WithFlexWrap(Wrap.Wrap);

            cardsContainer.AddChild(CreateVisionCard("1. ERADICATE FRICTION", "Automate the boiler-plate. Abstract the complex syntax. The mind should focus on the 'What', while the engine handles the 'How'.", new Color(0.2f, 0.6f, 1.0f)));
            cardsContainer.AddChild(CreateVisionCard("2. CONTINUOUS COMPUTE", "A living, breathing architecture. Logic is decoupled from the frame-tick, allowing ideas to scale infinitely without architectural collapse.", new Color(1.0f, 0.8f, 0.2f)));
            cardsContainer.AddChild(CreateVisionCard("3. MONETIZE IMAGINATION", "By lowering the barrier to entry, individuals can forge entire universes, systems, and games, turning pure thought into tangible value.", new Color(0.4f, 0.9f, 0.4f)));

            rootBuilder.AddChild(cardsContainer);

            return rootBuilder.Build();
        }

        private GraphicalUserInterfaceBuilder CreateVisionCard(string title, string body, Color accentColor)
        {
            return new GraphicalUserInterfaceBuilder($"Card_{title}")
                .WithBackgroundColor(new Color(0.12f, 0.12f, 0.14f))
                .WithBorderTopWidth(3)
                .WithBorderTopColor(accentColor)
                .WithPadding(20)
                .WithMarginBottom(20)
                // FlexBasis of ~30% minus margins ensures 3 cards fit on a wide screen, but they will wrap gracefully if squeezed
                .WithFlexGrow(1).WithFlexShrink(1).WithMinWidth(250).WithMarginRight(10)
                .AddChild(new ForgeLabelBuilder(title).WithColor(Color.white).WithFontSize(16).WithFontStyle(FontStyle.Bold).WithMargin(0, 10, 0, 0))
                .AddChild(new ForgeLabelBuilder(body).WithColor(Color.gray).WithFontSize(13).WithWhiteSpace(WhiteSpace.Normal));
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}