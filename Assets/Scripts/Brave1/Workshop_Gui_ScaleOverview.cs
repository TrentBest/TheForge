using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.UI_And_Tools.Forge.Builders.Staging
{
    public class Workshop_Gui_ScaleOverview : IGuiProvider
    {
        public string Title => "THE CONTINUOUS COMPUTE SPECTRUM";

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new ScrollView { style = { flexGrow = 1, paddingLeft = 40, paddingRight = 40, paddingTop = 40, paddingBottom = 40 } };

            var title = new Label("FSM_API: DETERMINISTIC MULTIVERSE EXECUTION")
            { style = { color = Color.white, fontSize = 28, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 20 } };
            root.Add(title);

            var thesis = new Label("Standard Object-Oriented software architectures shatter under the weight of extreme scale. Whether tracking the covalent bonds of 10,000 subatomic particles, or simulating the economic supply chains of 4.2 billion stellar entities across a galaxy, traditional thread-pools bottleneck.\n\nThe Singularity Workshop utilizes an agnostic Finite State Machine (FSM) Application Programming Interface. By abstracting logic into byte-state parameters bound directly to the GPU, we provide Compute as a Service (CaaS) that scales flawlessly from the Microscopic to the Multiverse. Either, and all between, we can do it.")
            { style = { color = Color.silver, fontSize = 16, whiteSpace = WhiteSpace.Normal, marginBottom = 40, height = 24 } };
            root.Add(thesis);

            // THE TBD EXPLORATION CARDS
            var cardsContainer = new VisualElement { style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap, justifyContent = Justify.SpaceBetween } };

            cardsContainer.Add(CreateExplainerCard(
                "MICRO-COMPUTE MECHANICS",
                "Explore how the FSM assigns deterministic states to molecular swarms without utilizing heavy GameObject MonoBehaviours.",
                Color.cyan,
                () => Debug.Log("Micro Explainer TBD")));

            cardsContainer.Add(CreateExplainerCard(
                "MACRO-COMPUTE MECHANICS",
                "Explore the Spherical Parallax projection engine and how the FSM maintains state coherence across millions of lightyears.",
                Color.yellow,
                () => Debug.Log("Macro Explainer TBD")));

            cardsContainer.Add(CreateExplainerCard(
                "THE GPU BYTE-STATE PIPELINE",
                "Deep dive into the HLSL structured buffers and how we condense 255 states into a single visual pixel channel.",
                new Color(0.8f, 0.1f, 0.8f),
                () => Debug.Log("GPU Pipeline Explainer TBD")));

            root.Add(cardsContainer);
            return root;
        }

        private VisualElement CreateExplainerCard(string title, string desc, Color accent, Action onClick)
        {
            var card = new VisualElement
            {
                style =
                {
                    backgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.5f),
                    width = new StyleLength(new Length(30, LengthUnit.Percent)),
                    minWidth = 250,
                    marginBottom = 20, paddingBottom = 20, paddingTop = 20, paddingLeft = 20, paddingRight = 20,
                    borderTopWidth = 3, borderTopColor = accent
                }
            };

            card.Add(new Label(title) { style = { color = Color.white, fontSize = 16, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });
            card.Add(new Label(desc) { style = { color = Color.gray, fontSize = 14, whiteSpace = WhiteSpace.Normal, flexGrow = 1, marginBottom = 15 } });

            var btn = new Button(onClick) { text = "DELVE DEEPER (TBD)", style = { backgroundColor = new Color(0.2f, 0.2f, 0.2f), color = accent, unityFontStyleAndWeight = FontStyle.Bold } };
            card.Add(btn);

            return card;
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}