using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.PillVirus
{
    public class PillVirus_DifficultySelect : IGuiProvider
    {
        public string Title => "Select Difficulty";
        private IGuiRouter _router;
        public PillVirus_DifficultySelect() { }

        public PillVirus_DifficultySelect(IGuiRouter router) => _router = router;

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new VisualElement { style = { flexGrow = 1, alignItems = Align.Center, justifyContent = Justify.Center } };

            var label = new Label("SELECT CLEARANCE LEVEL") { style = { fontSize = 30, color = UnityEngine.Color.white, marginBottom = 20 } };
            root.Add(label);


            void StartGame(float speed, int virusCount)
            {
                PillVirusConfig.FallSpeed = speed;
                PillVirusConfig.VirusCount = virusCount;
                if (_router != null) _router.NavigateTo("Gameplay");
            }


            root.Add(new Button(() => StartGame(0.8f, 10)) { text = "INTERN (EASY)", style = { width = 200, height = 50, marginBottom = 10 } });
            root.Add(new Button(() => StartGame(0.4f, 20)) { text = "RESIDENT (MED)", style = { width = 200, height = 50, marginBottom = 10 } });
            root.Add(new Button(() => StartGame(0.15f, 35)) { text = "ATTENDING (HARD)", style = { width = 200, height = 50 } });

            return root;
        }

        // ... standard interface implementations ...
        public System.Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void FromUIDocument(string assetPath) { }
        public void ToUIDocument(string assetPath) { }
    }
}