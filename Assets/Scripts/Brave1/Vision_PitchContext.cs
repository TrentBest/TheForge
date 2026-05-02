using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using TheSingularityWorkshop.FSM_API;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Vision
{
    public class Vision_PitchContext : IStateContext
    {
        public bool IsValid { get; set; } = true;
        public string ActiveSector { get; set; } = "Core";
        public string Name { get; set ; }
    }

    public class SingularityVision_GuiProvider : IGuiProvider
    {
        public string Title => "THE SINGULARITY WORKSHOP: THE VISION";

        private VisualElement _mainContent;
        private GuiContext _guiContext;

        public VisualElement CreateGui(GuiContext ctx)
        {
            _guiContext = ctx;

            // Using the GraphicalUserInterfaceBuilder for consistent layout
            var root = new GraphicalUserInterfaceBuilder("VisionRoot")
                .WithPercentSize(100, 100)
                .WithBackgroundColor(new Color(0.01f, 0.01f, 0.02f))
                .OnBuild(ve => {
                    BuildHeader(ve);
                    BuildMainStage(ve);
                }).Build();

            // CRITICAL UI TICK PATTERN: Ticking the FSM locally within the UI schedule
            root.schedule.Execute(() => {
                FSM_API.Interaction.Update("VisionLogic");
            }).Every(16);

            return root;
        }

        private void BuildHeader(VisualElement root)
        {
            var header = new VisualElement { style = { height = 80, flexDirection = FlexDirection.Row, alignItems = Align.Center, paddingLeft = 40, borderBottomWidth = 1, borderBottomColor = new Color(0.2f, 0.2f, 0.3f) } };
            header.Add(new Label("THE SINGULARITY WORKSHOP //") { style = { color = Color.gray, fontSize = 12, letterSpacing = 3 } });
            header.Add(new Label(" FSM_API") { style = { color = Color.white, fontSize = 18, unityFontStyleAndWeight = FontStyle.Bold } });
            root.Add(header);
        }

        private void BuildMainStage(VisualElement root)
        {
            _mainContent = new VisualElement { style = { flexGrow = 1, paddingLeft = 60, paddingRight = 60, justifyContent = Justify.Center } };

            var visionTitle = new Label("ONE LOGIC. ANY PLATFORM.")
            {
                style = { color = Color.white, fontSize = 48, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 }
            };

            var subTitle = new Label("You are currently viewing a web interface powered by the FSM_API, managing a Unity WebGL engine powered by the FSM_API. This is the end of dependency-heavy architecture.")
            {
                style = { color = new Color(0.6f, 0.6f, 0.8f), fontSize = 18, whiteSpace = WhiteSpace.Normal, marginBottom = 40, maxWidth = 800 }
            };

            var ctaRow = new VisualElement { style = { flexDirection = FlexDirection.Row } };

            // Interaction: Pushing the user into the "Fold"
            var enterWorkshop = new Button(() => Debug.Log("Entering Workshop Flow..."))
            {
                text = "WITNESS THE COMPUTE POWER",
                style = { height = 50, width = 300, backgroundColor = new Color(0.1f, 0.5f, 0.8f), color = Color.white, unityFontStyleAndWeight = FontStyle.Bold }
            };

            ctaRow.Add(enterWorkshop);
            _mainContent.Add(visionTitle);
            _mainContent.Add(subTitle);
            _mainContent.Add(ctaRow);

            root.Add(_mainContent);
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string id) { }
        public void FromUIDocument(string doc) { }
    }
}