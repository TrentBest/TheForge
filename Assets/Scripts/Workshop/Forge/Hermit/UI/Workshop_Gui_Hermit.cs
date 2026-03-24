// File: Assets/Scripts/Workshop/Forge/Builders/GuiBuilders/Agents/Workshop_Gui_Hermit.cs
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.Forge;
using TheSingularityWorkshop.Forge.Builders;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.Workshop.Forge.Hermit.UI
{
    /// <summary>
    /// The Hermit Shell: A diegetic terminal for interacting with a local AI host process.
    /// Allows users to inject Forge assets into the AI's "Immediate Environment" (Context Window).
    /// </summary>
    public class Workshop_Gui_Hermit : MonoBehaviour, IForgeBuilder
    {
        public string ToolName => "Hermit Shell (AI Bridge)";
        public string Title => ToolName;
        public Type GetProductType() => typeof(object);
        public IGuiProvider GetGuiProvider() => new HermitGuiProvider();

        private void OnEnable() => BuilderRegistry.Register(this);

        public object Build()
        {
            throw new NotImplementedException();
        }
    }

    public class HermitGuiProvider : IGuiProvider
    {
        // --- State: The Context Window ---
        private List<ChatMessage> _messageHistory = new List<ChatMessage>();
        private List<EnvironmentItem> _activeEnvironment = new List<EnvironmentItem>();
        private string _currentInput = "";

        // --- UI References ---
        private ScrollView _chatDisplay;
        private ScrollView _environmentDisplay;
        private TextField _inputField;

        public string Title => throw new NotImplementedException();

        public VisualElement CreateGui(GuiContext ctx)
        {
            if (_messageHistory.Count == 0)
            {
                _messageHistory.Add(new ChatMessage("System", "Hermit Shell v1.0 Initialized. LocalHost bridge ready. Waiting for environmental context..."));
            }

            var root = new GraphicalUserInterfaceBuilder("HermitShell")
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                .WithPercentSize(100, 100)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f)); // Deep terminal blue/black

            // ==========================================
            // LEFT PANE: THE NEURAL LINK (Chat & Output)
            // ==========================================
            root.AddChild(new GraphicalUserInterfaceBuilder("NeuralLink")
                .WithFlexGrow(2) // Takes up ~66% of the screen
                .WithBorderRightWidth(2)
                .WithBorderRightColor(new Color(0.3f, 0.3f, 0.4f))
                .WithPadding(15)
                .AddHeader("THE NEURAL LINK", Color.cyan)

                // Chat Display Log
                .AddChild(c => {
                    _chatDisplay = new ScrollView { style = { flexGrow = 1, marginBottom = 15, paddingTop = 10, paddingBottom = 10, paddingLeft = 10, paddingRight = 10, backgroundColor = new Color(0, 0, 0, 0.3f) } };
                    RefreshChatDisplay();
                    return _chatDisplay;
                })

                // Input Row
                .AddChild(new GraphicalUserInterfaceBuilder("InputArea")
                    .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                    .WithHeight(40)
                    .AddChild(c => {
                        _inputField = new TextField { value = _currentInput, style = { flexGrow = 1, marginRight = 10 } };
                        _inputField.RegisterValueChangedCallback(e => _currentInput = e.newValue);
                        // Submit on Enter key
                        _inputField.RegisterCallback<KeyDownEvent>(e => { if (e.keyCode == KeyCode.Return) SubmitPrompt(); });
                        return _inputField;
                    })
                    .AddButton("TRANSMIT", SubmitPrompt)
                )
            );

            // ==========================================
            // RIGHT PANE: THE WORKBENCH (Environment)
            // ==========================================
            root.AddChild(new GraphicalUserInterfaceBuilder("Workbench")
                .WithFlexGrow(1) // Takes up ~33% of the screen
                .WithBackgroundColor(new Color(0.12f, 0.12f, 0.15f))
                .WithPadding(15)
                .AddHeader("IMMEDIATE ENVIRONMENT", Color.yellow)
                .AddChild(new Label("Objects here are serialized and sent to Hermit's context window.") { style = { color = Color.gray, fontSize = 10, marginBottom = 10, whiteSpace = WhiteSpace.Normal } })

                // Active Context Viewer
                .AddChild(c => {
                    _environmentDisplay = new ScrollView { style = { flexGrow = 1, marginBottom = 10 } };
                    RefreshEnvironmentDisplay();
                    return _environmentDisplay;
                })

                // Tools to inject items into the environment
                .AddSeparator(Color.gray)
                .AddChild(new Label("Inject Context") { style = { unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10, marginBottom = 5 } })
                .AddButton("+ Add Global Stage Data", () => InjectIntoEnvironment("StageData", "Current Scene Hierarchy & Props"))
                .AddButton("+ Add Active Actors", () => InjectIntoEnvironment("ActorRoster", "JSON dump of all active Actor definitions"))
                .AddButton("+ Add Forge Settings", () => InjectIntoEnvironment("ForgeConfig", "Global lighting, gravity, and rule settings"))
            );

            return root.Build();
        }

        // --- Interaction Logic ---

        private void InjectIntoEnvironment(string id, string description)
        {
            if (!_activeEnvironment.Any(e => e.Id == id))
            {
                _activeEnvironment.Add(new EnvironmentItem { Id = id, Description = description });
                _messageHistory.Add(new ChatMessage("System", $"[+] Injected {id} into environment."));
                RefreshEnvironmentDisplay();
                RefreshChatDisplay();
            }
        }

        private void RemoveFromEnvironment(EnvironmentItem item)
        {
            _activeEnvironment.Remove(item);
            _messageHistory.Add(new ChatMessage("System", $"[-] Removed {item.Id} from environment."));
            RefreshEnvironmentDisplay();
            RefreshChatDisplay();
        }

        private void SubmitPrompt()
        {
            if (string.IsNullOrWhiteSpace(_currentInput)) return;

            // 1. Log User Input
            string userText = _currentInput;
            _messageHistory.Add(new ChatMessage("User", userText));
            _currentInput = "";
            _inputField.SetValueWithoutNotify("");
            RefreshChatDisplay();

            // 2. Bundle the Request (Sending the Prompt + The Environment Data)
            BundleAndSendToLocalHost(userText, _activeEnvironment);
        }

        private void BundleAndSendToLocalHost(string prompt, List<EnvironmentItem> contextItems)
        {
            // [!] THIS IS WHERE YOU BIND TO YOUR PYTHON / LOCALHOST SERVER [!]
            Debug.Log($"[Hermit] Transmitting to localhost:5000...");
            Debug.Log($"[Hermit] Prompt: {prompt}");
            Debug.Log($"[Hermit] Environmental Payloads: {contextItems.Count}");

            // MOCKING THE AI RESPONSE:
            // In a real scenario, this would be an async HTTP/WebSocket request.
            string mockResponse = $"I hear you. I am currently analyzing {contextItems.Count} items in my environment. ";
            if (contextItems.Count > 0)
            {
                mockResponse += $"I see {string.Join(", ", contextItems.Select(c => c.Id))}. ";
            }
            mockResponse += "How would you like me to manipulate them?";

            // Simulate Network Delay via simple deferment (or just immediate for now)
            _messageHistory.Add(new ChatMessage("Hermit", mockResponse));
            RefreshChatDisplay();
        }

        // --- UI Refreshers ---

        private void RefreshChatDisplay()
        {
            if (_chatDisplay == null) return;
            _chatDisplay.Clear();

            foreach (var msg in _messageHistory)
            {
                Color nameColor = msg.Sender == "User" ? Color.green : (msg.Sender == "System" ? Color.gray : Color.cyan);

                var msgRow = new VisualElement { style = { flexDirection = FlexDirection.Row, marginBottom = 8 } };
                msgRow.Add(new Label($"[{msg.Sender}] ") { style = { color = nameColor, unityFontStyleAndWeight = FontStyle.Bold, flexShrink = 0 } });
                msgRow.Add(new Label(msg.Content) { style = { color = Color.white, whiteSpace = WhiteSpace.Normal, flexGrow = 1 } });

                _chatDisplay.Add(msgRow);
            }

            // Auto-scroll to bottom (Requires a tiny delay in UI Toolkit sometimes, but this works generally)
            _chatDisplay.schedule.Execute(() => _chatDisplay.ScrollTo(_chatDisplay.Children().LastOrDefault())).StartingIn(10);
        }

        private void RefreshEnvironmentDisplay()
        {
            if (_environmentDisplay == null) return;
            _environmentDisplay.Clear();

            if (_activeEnvironment.Count == 0)
            {
                _environmentDisplay.Add(new Label("Environment is empty.") { style = { color = new Color(1, 1, 1, 0.3f), unityTextAlign = TextAnchor.MiddleCenter, marginTop = 50 } });
                return;
            }

            foreach (var item in _activeEnvironment)
            {
                var capturedItem = item; // Capture for closure
                var card = new GraphicalUserInterfaceBuilder($"EnvCard_{item.Id}")
                    .WithBackgroundColor(new Color(0.2f, 0.2f, 0.25f))
                    .WithPadding(8)
                    .WithMarginBottom(8)
                    .WithBorderRadius(4)
                    .WithBorderLeftWidth(3)
                    .WithBorderLeftColor(Color.yellow)
                    .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                    .AddChild(new GraphicalUserInterfaceBuilder("TextData")
                        .WithFlexLayout(FlexDirection.Column)
                        .AddChild(new Label(item.Id) { style = { unityFontStyleAndWeight = FontStyle.Bold, color = Color.white } })
                        .AddChild(new Label(item.Description) { style = { fontSize = 10, color = Color.gray, whiteSpace = WhiteSpace.Normal } })
                    )
                    .AddButton("X", () => RemoveFromEnvironment(capturedItem)); // Eject button

                _environmentDisplay.Add(card.Build());
            }
        }

        // --- Data Structures ---

        private struct ChatMessage
        {
            public string Sender;
            public string Content;
            public ChatMessage(string sender, string content) { Sender = sender; Content = content; }
        }

        private class EnvironmentItem
        {
            public string Id;
            public string Description;
            // In the future, this holds a direct reference to the Unity Object or JSON String
            // public object DataPayload; 
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void FromUIDocument(string assetPath) { }

        public void ToUIDocument(string assetPath)
        {
            throw new NotImplementedException();
        }
    }
}