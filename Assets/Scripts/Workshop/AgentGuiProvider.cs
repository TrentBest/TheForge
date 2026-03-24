using UnityEngine.UIElements;
//using TheSingularityWorkshop.LLM;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using Assets.Scripts.Workshop.Forge.Hermit.Core;

namespace TheSingularityWorkshop.Forge.AI
{
    public class AgentGuiProvider : IGuiProvider
    {
        private Agent _agent;
        private ScrollView _chatLog;
        private TextField _inputField;

        public string Title => "Digital Assistant";

        public AgentGuiProvider(Agent agent)
        {
            _agent = agent;
        }

        public System.Action<VisualElement> GetGuiBuilder() => null;

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new VisualElement();
            root.style.flexDirection = FlexDirection.Column;
            root.style.flexGrow = 1;

            // The Chat Log (My Output)
            _chatLog = new ScrollView();
            _chatLog.style.flexGrow = 1;
            root.Add(_chatLog);

            // The Input Area (Your Input)
            var inputRow = new VisualElement();
            inputRow.style.flexDirection = FlexDirection.Row;

            _inputField = new TextField();
            _inputField.style.flexGrow = 1;

            var sendBtn = new Button(Submit) { text = "Send" };

            inputRow.Add(_inputField);
            inputRow.Add(sendBtn);
            root.Add(inputRow);

            // Subscribe the UI to the Agent's Output Context!
            // (Assuming ForgeOutputContext fires an event when I speak)
            if (_agent.OutputContext is ForgeOutputContext forgeOutput)
            {
                forgeOutput.OnMessageReceived += AppendMessage;
            }

            return root;
        }

        private void Submit()
        {
            if (string.IsNullOrWhiteSpace(_inputField.value)) return;

            AppendMessage($"Trent: {_inputField.value}");

            // Pass the string to the Agent's input context
            if (_agent.InputContext is ForgeInputContext forgeInput)
            {
                forgeInput.SubmitInput(_inputField.value);
            }

            _inputField.value = "";
        }

        private void AppendMessage(string msg)
        {
            // Unity UI Elements must be updated on the main thread
            _chatLog.Add(new Label(msg) { style = { whiteSpace = WhiteSpace.Normal } });
            _chatLog.ScrollTo(_chatLog.contentContainer);
        }

        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}