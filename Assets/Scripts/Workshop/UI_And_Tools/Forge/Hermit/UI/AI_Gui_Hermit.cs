using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.Hermit.Bridge;
using Workshop.UI_And_Tools.Forge.Hermit.Core;

namespace Workshop.UI_And_Tools.Forge.Hermit.UI
{
    public class AI_Gui_Hermit : IGuiProvider
    {
        public string Title => "HERMIT NEURAL INTERFACE";

        private HermitUIState _uiState;
        private Label _apiStatusLabel, _tokenUsageLabel, _rateLimitLabel;
        private VisualElement _chatHistoryRoot;
        private TextField _promptFieldRef;

        public AI_Gui_Hermit() { }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _uiState = HermitSessionManager.LoadState();

            var root = new GraphicalUserInterfaceBuilder("HermitRoot")
                .WithFlexGrow(1)
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f))
                .OnBuild(ve =>
                {
                    ve.RegisterCallback<AttachToPanelEvent>(e => {
                        LocalHostClient.OnDirectiveReceived -= HandleIncomingDirective;
                        LocalHostClient.OnDirectiveReceived += HandleIncomingDirective;
                    });
                    ve.RegisterCallback<DetachFromPanelEvent>(e => {
                        LocalHostClient.OnDirectiveReceived -= HandleIncomingDirective;
                        if (_promptFieldRef != null) _uiState.CurrentInput = _promptFieldRef.value;
                        HermitSessionManager.SaveState(_uiState);
                    });
                })
                .Build();

            // ==========================================
            // LEFT COLUMN
            // ==========================================
            var leftColumn = new GraphicalUserInterfaceBuilder("LeftColumn")
                .WithFlexGrow(2.5f)
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                .WithBorderRightColor(new Color(0.3f, 0.3f, 0.3f))
                .WithBorderRightWidth(2)
                .Build();

            var previewContainer = new GraphicalUserInterfaceBuilder("HermitVisionContainer")
                .WithFlexGrow(1)
                .WithBackgroundColor(Color.black)
                .OnBuild(ve => {
                    ve.style.overflow = Overflow.Hidden;
                    ve.style.flexShrink = 1;
                }).Build();

            // 1. SMART TARGETING: Try to find the broader environment root first!
            var environmentTarget = GameObject.Find("ForgeEnvironment")
                                 ?? GameObject.Find("Forge")
                                 ?? GameObject.Find("Environment")
                                 ?? GameObject.Find("Hermit")
                                 ?? GameObject.CreatePrimitive(PrimitiveType.Cube);

            // 2. WIDE ANGLE LENS: Pull the zoom out from 3f to 15f to see the whole room
            var lmpb = new LiveModelPreviewBuilder(environmentTarget)
                .WithZoom(15f)
                .WithOriginalModel(true);

            previewContainer.Add(lmpb.CreateGui(new GuiContext()));
            leftColumn.Add(previewContainer);

            //var hermitTarget = GameObject.Find("Hermit") ?? GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //var lmpb = new LiveModelPreviewBuilder(hermitTarget).WithZoom(3f).WithOriginalModel(true);
            //previewContainer.Add(lmpb.CreateGui(new GuiContext()));
            //leftColumn.Add(previewContainer);

            // --- Chat ---
            var chatContainer = new GraphicalUserInterfaceBuilder("ChatInputContainer")
                .WithHeight(250)
                .WithBackgroundColor(new Color(0.12f, 0.12f, 0.15f))
                .WithPadding(15)
                .WithFlexLayout(FlexDirection.Column, Justify.SpaceBetween, Align.Stretch)
                .Build();

            _chatHistoryRoot = new ForgeScrollViewBuilder("History").WithFlexGrow(1).CreateGui(new GuiContext());

            if (_uiState.ChatSenders.Count == 0)
                AddChatMessage("System", "Neural Link Initialized. Awaiting Uplink...");
            else
            {
                for (int i = 0; i < _uiState.ChatSenders.Count; i++)
                    RenderChatMessage(_uiState.ChatSenders[i], _uiState.ChatContents[i]);
            }
            chatContainer.Add(_chatHistoryRoot);

            var inputRow = new GraphicalUserInterfaceBuilder("InputRow")
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.FlexEnd)
                .WithMinHeight(40)
                .Build();

            var promptField = new ForgeTextFieldBuilder()
                .WithColor(Color.white)
                .WithValue(_uiState.CurrentInput)
                .OnBuild(ve => {
                    ve.style.flexGrow = 1;
                    ve.style.flexShrink = 1;

                    if (ve is TextField tf)
                    {
                        tf.multiline = true;
                        tf.style.maxHeight = 80;
                        tf.style.whiteSpace = WhiteSpace.Normal;
                        _promptFieldRef = tf;
                    }
                })
                .CreateGui(new GuiContext());

            inputRow.Add(promptField);

            inputRow.Add(new ForgeButtonBuilder("TRANSMIT")
                .WithFontStyle(FontStyle.Bold)
                .OnClick(PerformTransmit)
                .OnBuild(ve => {
                    ve.style.backgroundColor = new Color(0.8f, 0.1f, 0.8f);
                    ve.style.color = Color.white;
                })
                .CreateGui(new GuiContext()));

            chatContainer.Add(inputRow);
            leftColumn.Add(chatContainer);
            root.Add(leftColumn);

            // ==========================================
            // RIGHT COLUMN
            // ==========================================
            var rightColumn = new GraphicalUserInterfaceBuilder("RightColumn")
                .WithFlexGrow(1)
                .WithMinWidth(300)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.06f))
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                .Build();

            var toolingScroll = new ForgeScrollViewBuilder("Telemetry").WithPadding(15, 15, 15, 15).CreateGui(new GuiContext());

            toolingScroll.Add(new ForgeButtonBuilder("LAUNCH CORTEX")
                .WithFontStyle(FontStyle.Bold)
                .WithHeight(45)
                .WithMarginBottom(15)
                .OnClick(LaunchCortex)
                .OnBuild(ve => {
                    ve.style.backgroundColor = new Color(0.15f, 0.6f, 0.25f);
                    ve.style.color = Color.white;
                })
                .CreateGui(new GuiContext()));

            _apiStatusLabel = AddDiagnosticCard(toolingScroll, "GEMINI UPLINK", _uiState.IsGhostLinked ? "ONLINE" : "OFFLINE");
            _tokenUsageLabel = AddDiagnosticCard(toolingScroll, "SESSION TOKENS", "Awaiting Data...");
            _rateLimitLabel = AddDiagnosticCard(toolingScroll, "ALLOTMENT", "RPM: Active");

            if (_uiState.IsGhostLinked) _apiStatusLabel.style.color = Color.green;

            rightColumn.Add(toolingScroll);
            root.Add(rightColumn);

            return root;
        }

        private void PerformTransmit()
        {
            if (_promptFieldRef == null || string.IsNullOrEmpty(_promptFieldRef.value)) return;
            string msg = _promptFieldRef.value;
            _promptFieldRef.value = "";
            _uiState.CurrentInput = "";

            AddChatMessage("User", msg);
            LocalHostClient.SendPrompt(JsonUtility.ToJson(new HermitDirectivePayload
            {
                AgentId = "Hermit",
                Intent = "Prompt",
                Content = msg
            }));
        }

        private void LaunchCortex()
        {
            // 1. SMART CONNECTION: Check if Cortex is already running!
            var existingProcesses = System.Diagnostics.Process.GetProcessesByName("AgentLLMConsole");
            if (existingProcesses.Length > 0)
            {
                _apiStatusLabel.text = "Reconnecting...";
                _apiStatusLabel.style.color = Color.yellow;

                // Fire a lightweight ping to the existing process
                LocalHostClient.SendPrompt(JsonUtility.ToJson(new HermitDirectivePayload { Intent = "Handshake", Content = "PING" }));
                return;
            }

            // 2. Hard Boot if it's completely dead
            string projectRoot = System.IO.Directory.GetParent(Application.dataPath).FullName;
            string path = System.IO.Path.Combine(projectRoot, "Tools", "AgentLLMConsole", "AgentLLMConsole.exe");

            if (!System.IO.File.Exists(path)) path = @"C:\Users\TBest\TheForge\Tools\AgentLLMConsole\AgentLLMConsole.exe";

            if (System.IO.File.Exists(path))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true,
                    WorkingDirectory = System.IO.Path.GetDirectoryName(path)
                });
                _apiStatusLabel.text = "Booting...";
                _apiStatusLabel.style.color = Color.yellow;
            }
        }

        private void HandleIncomingDirective(HermitDirectivePayload payload)
        {
            if (payload == null || string.IsNullOrEmpty(payload.Content)) return;

            bool wasLinked = _uiState.IsGhostLinked;

            // --- SMART HANDSHAKE PARSING ---
            if (payload.Intent == "Handshake_ACK" || payload.Intent == "System_Handshake")
            {
                _uiState.IsGhostLinked = true;
                _apiStatusLabel.text = "ONLINE";
                _apiStatusLabel.style.color = Color.green;

                if (!wasLinked) AddChatMessage("System", "Cortex Synchronized. Uplink Secure.");
                else AddChatMessage("System", "Cortex Reconnected. Uplink Restored.");

                return; // Absorb the packet, do not render it as chat text!
            }

            // Failsafe link logic
            if (!_uiState.IsGhostLinked)
            {
                _uiState.IsGhostLinked = true;
                _apiStatusLabel.text = "ONLINE";
                _apiStatusLabel.style.color = Color.green;
            }

            // --- NATIVE TELEMETRY BINDING ---
            if (payload.SessionTokens > 0 && _tokenUsageLabel != null)
                _tokenUsageLabel.text = $"P: {payload.PromptTokens} | C: {payload.GenTokens}\nTotal Session: {payload.SessionTokens}";

            if (payload.MaxRPM > 0 && _rateLimitLabel != null)
            {
                _rateLimitLabel.text = $"RPM: {payload.CurrentRPM} / {payload.MaxRPM}";
                _rateLimitLabel.style.color = payload.CurrentRPM >= payload.MaxRPM ? Color.red : Color.white;
            }

            AddChatMessage(payload.AgentId.Contains("System") ? "System" : "Hermit", payload.Content);
        }

        private void AddChatMessage(string sender, string message)
        {
            _uiState.ChatSenders.Add(sender);
            _uiState.ChatContents.Add(message);
            HermitSessionManager.SaveState(_uiState);
            RenderChatMessage(sender, message);
        }

        private void RenderChatMessage(string sender, string message)
        {
            Color col = sender == "User" ? Color.white : sender == "Hermit" ? Color.cyan : Color.green;
            string prefix = sender == "User" ? "> " : $"{sender}: ";

            var msgLabel = new ForgeLabelBuilder(prefix + message).WithColor(col).CreateGui(new GuiContext());
            msgLabel.style.whiteSpace = WhiteSpace.Normal;
            msgLabel.style.marginBottom = 10;

            _chatHistoryRoot.Add(msgLabel);
            _chatHistoryRoot.MarkDirtyRepaint();

            _chatHistoryRoot.schedule.Execute(() =>
            {
                if (_chatHistoryRoot is ScrollView sv)
                {
                    sv.scrollOffset = new Vector2(0, sv.contentContainer.layout.height);
                }
            }).StartingIn(50);
        }

        private Label AddDiagnosticCard(VisualElement container, string title, string status)
        {
            var card = new GraphicalUserInterfaceBuilder(title)
                .WithBackgroundColor(new Color(0.12f, 0.12f, 0.15f))
                .WithBorderColor(new Color(0.3f, 0.3f, 0.4f))
                .WithBorderWidth(1)
                .WithBorderRadius(4)
                .WithPadding(10)
                .WithMarginBottom(10)
                .Build();

            card.Add(new ForgeLabelBuilder(title).WithColor(Color.cyan).WithFontSize(10).WithFontStyle(FontStyle.Bold).WithMarginBottom(5).CreateGui(new GuiContext()));

            var l = new ForgeLabelBuilder(status).WithColor(Color.white).CreateGui(new GuiContext()) as Label;
            card.Add(l);
            container.Add(card);
            return l;
        }

        public Action<VisualElement> GetGuiBuilder() => r => r.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string p) { }
        public void FromUIDocument(string p) { }
    }
}